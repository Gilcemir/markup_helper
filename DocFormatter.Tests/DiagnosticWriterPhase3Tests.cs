using System.Text.Json;
using System.Xml.Linq;
using DocFormatter.Core.Jats;
using DocFormatter.Core.Pipeline;
using DocFormatter.Core.Reporting;
using Xunit;

namespace DocFormatter.Tests;

/// <summary>
/// Unit tests for the Phase 3 section of the diagnostic document
/// (<see cref="DiagnosticWriter.BuildPhase3Document"/> /
/// <see cref="DiagnosticWriter.WritePhase3"/>): per-tag values read from the
/// injected XML and dispositions derived from the report plus the confirmer's
/// recorded decisions.
/// </summary>
public sealed class DiagnosticWriterPhase3Tests : IDisposable
{
    private static readonly DateTime FixedTime = new(2026, 5, 27, 12, 0, 0, DateTimeKind.Utc);
    private readonly string _tempDir;

    public DiagnosticWriterPhase3Tests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"docfmt-diag-phase3-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, recursive: true); } catch { /* best effort */ }
    }

    private static XDocument FullyInjectedXml() => new(
        new XElement("article",
            new XElement("front",
                new XElement("article-meta",
                    new XElement("elocation-id", "e54492621"),
                    new XElement("article-id", new XAttribute("pub-id-type", "doi"), "10.1590/x"),
                    new XElement("article-id", new XAttribute("pub-id-type", "other"), "00201"),
                    new XElement("title-group", new XElement("article-title", "A Title")),
                    new XElement("author-notes",
                        new XElement("fn", new XAttribute("fn-type", "edited-by"),
                            new XElement("label", "SCIENTIFIC EDITOR:"),
                            new XElement("p", "Jane Roe"))),
                    new XElement("contrib-group",
                        new XElement("contrib",
                            new XElement("role", new XAttribute(
                                "content-type",
                                "http://credit.niso.org/contributor-roles/writing-original-draft/"),
                                "Writing – original draft"))))),
            new XElement("back",
                new XElement("sec",
                    new XAttribute("sec-type", "data-availability"),
                    new XAttribute("specific-use", "data-available"),
                    new XElement("title", "Data Availability Statement"),
                    new XElement("p", "Data are available.")))));

    [Fact]
    public void BuildPhase3Document_AllTagsApplied_ReportsValuesFromXmlAndAutoAppliedDispositions()
    {
        var xml = FullyInjectedXml();
        var report = new Report();
        report.Info("other-id", "Inserted <article-id pub-id-type=\"other\">00201</article-id> after the DOI.");
        report.Info("edited-by", "Injected 1 edited-by fn(s) into <author-notes>: SCIENTIFIC EDITOR: Jane Roe.");
        report.Info("data-availability", "Injected <sec sec-type=\"data-availability\"> after <ack> (AutoApplied).");
        report.Info("credit-roles", "Injected 1 <role> for 'JR' (AutoApplied): Writing – original draft.");
        // Force the >= Warn write gate without changing tag dispositions.
        report.Warn("phase3", "synthetic gate trigger");

        var doc = DiagnosticWriter.BuildPhase3Document(
            "x.xml", xml, report, EmptyDispositions(), FixedTime);

        Assert.NotNull(doc.Phase3);
        Assert.Equal(("other-id", "00201", "autoApplied"), Tuple(doc.Phase3!.OtherId));
        Assert.Equal(("edited-by", "Jane Roe", "autoApplied"), Tuple(doc.Phase3.EditedBy));
        Assert.Equal(("data-availability", "data-available", "autoApplied"), Tuple(doc.Phase3.DataAvailability));
        Assert.Equal(("credit-roles", "1", "autoApplied"), Tuple(doc.Phase3.CreditRoles));
    }

    [Fact]
    public void BuildPhase3Document_ReadsDoiAndElocationFromXmlIntoFields()
    {
        var xml = FullyInjectedXml();
        var report = new Report();
        report.Warn("phase3", "gate");

        var doc = DiagnosticWriter.BuildPhase3Document("x.xml", xml, report, EmptyDispositions(), FixedTime);

        Assert.Equal("10.1590/x", doc.Fields.Doi.Value);
        Assert.Equal("e54492621", doc.Fields.Elocation.Value);
        Assert.Equal("A Title", doc.Fields.Title.Value);
        Assert.Empty(doc.Fields.Authors);
        Assert.Null(doc.Phase2);
        Assert.Null(doc.Formatting);
    }

    [Fact]
    public void BuildPhase3Document_PromptedTag_SurfacesRecordedConfirmDisposition()
    {
        var xml = FullyInjectedXml();
        var report = new Report();
        report.Info("data-availability", "Injected <sec sec-type=\"data-availability\"> (Overridden).");
        report.Warn("phase3", "gate");

        var recorded = new Dictionary<string, ConfirmDisposition>(StringComparer.Ordinal)
        {
            ["data-availability"] = ConfirmDisposition.Overridden,
        };

        var doc = DiagnosticWriter.BuildPhase3Document("x.xml", xml, report, recorded, FixedTime);

        Assert.Equal("overridden", doc.Phase3!.DataAvailability.Disposition);
    }

    [Fact]
    public void BuildPhase3Document_FreeProseCredit_NotWritten_IsSkippedDespiteAutoApplyVote()
    {
        // The CREDIT statement was free prose: the confirmer voted AutoApplied but
        // the injector declined to write, so no <role> reached the XML.
        var xml = new XDocument(new XElement("article", new XElement("front")));
        var report = new Report();
        report.Warn("credit-roles", "CREDIT statement is free prose; roles not auto-applied (AutoApplied).");

        var recorded = new Dictionary<string, ConfirmDisposition>(StringComparer.Ordinal)
        {
            ["credit-roles"] = ConfirmDisposition.AutoApplied,
        };

        var doc = DiagnosticWriter.BuildPhase3Document("x.xml", xml, report, recorded, FixedTime);

        Assert.Equal(("credit-roles", null, "skipped"), Tuple(doc.Phase3!.CreditRoles));
    }

    [Fact]
    public void BuildPhase3Document_FreeTextCredit_MapsTheNewDispositionWithoutThrowing()
    {
        // ADR-007: a document switched to free text emits roles without
        // @content-type, so ReadCreditRolesValue (CRediT-typed only) sees none, but
        // the recorded FreeText disposition must surface as "freeText".
        var xml = new XDocument(
            new XElement("article",
                new XElement("front",
                    new XElement("contrib-group",
                        new XElement("contrib",
                            new XElement("role", "Conceptualization"))))));
        var report = new Report();
        report.Info("credit-roles", "Injected 1 free-text <role> for 'Costa AES' (FreeText): Conceptualization.");
        report.Warn("credit-roles", "Free-text roles emitted; 1 author(s) unresolved and not placed: Neto VBP (NotFound).");

        var recorded = new Dictionary<string, ConfirmDisposition>(StringComparer.Ordinal)
        {
            ["credit-roles"] = ConfirmDisposition.FreeText,
        };

        var doc = DiagnosticWriter.BuildPhase3Document("x.xml", xml, report, recorded, FixedTime);

        Assert.Equal("freeText", doc.Phase3!.CreditRoles.Disposition);
    }

    [Fact]
    public void WritePhase3_FreeTextCredit_WritesFreeTextDispositionToDisk()
    {
        var xml = new XDocument(
            new XElement("article",
                new XElement("front",
                    new XElement("contrib-group",
                        new XElement("contrib", new XElement("role", "Conceptualization"))))));
        var report = new Report();
        report.Info("credit-roles", "Injected 1 free-text <role> for 'Costa AES' (FreeText): Conceptualization.");
        report.Warn("credit-roles", "Free-text roles emitted; 1 author(s) unresolved and not placed.");

        var recorded = new Dictionary<string, ConfirmDisposition>(StringComparer.Ordinal)
        {
            ["credit-roles"] = ConfirmDisposition.FreeText,
        };

        var path = Path.Combine(_tempDir, "freetext.diagnostic.json");
        var written = DiagnosticWriter.WritePhase3(path, "x.xml", xml, report, recorded);

        Assert.True(written);
        using var json = JsonDocument.Parse(File.ReadAllText(path));
        var creditRoles = json.RootElement.GetProperty("phase3").GetProperty("creditRoles");
        Assert.Equal("freeText", creditRoles.GetProperty("disposition").GetString());
    }

    [Fact]
    public void BuildPhase3Document_IdempotentSkipAndAbsentSource_AreSkippedAndAbsent()
    {
        var xml = new XDocument(new XElement("article", new XElement("front")));
        var report = new Report();
        report.Info("other-id", "<article-id pub-id-type=\"other\"> already present (\"00201\"); skipped.");
        report.Info("edited-by", "No responsible editor on the docx source; skipped.");
        report.Warn("phase3", "gate");

        var doc = DiagnosticWriter.BuildPhase3Document("x.xml", xml, report, EmptyDispositions(), FixedTime);

        Assert.Equal("skipped", doc.Phase3!.OtherId.Disposition);
        Assert.Equal("absent", doc.Phase3.EditedBy.Disposition);
    }

    [Fact]
    public void BuildPhase3Document_PipelineError_IsFailed()
    {
        var xml = new XDocument(new XElement("article", new XElement("front")));
        var report = new Report();
        report.Error("other-id", "No <article-id pub-id-type=\"doi\"> to anchor the other id.");

        var doc = DiagnosticWriter.BuildPhase3Document("x.xml", xml, report, EmptyDispositions(), FixedTime);

        Assert.Equal("failed", doc.Phase3!.OtherId.Disposition);
        Assert.Equal("error", doc.Status);
    }

    [Fact]
    public void WritePhase3_BelowWarn_DoesNotWriteFile()
    {
        var xml = FullyInjectedXml();
        var report = new Report();
        report.Info("other-id", "Inserted ... after the DOI.");

        var path = Path.Combine(_tempDir, "info-only.diagnostic.json");
        var written = DiagnosticWriter.WritePhase3(path, "x.xml", xml, report, EmptyDispositions());

        Assert.False(written);
        Assert.False(File.Exists(path));
    }

    [Fact]
    public void WritePhase3_AtWarn_WritesCamelCasePhase3SectionToDisk()
    {
        var xml = FullyInjectedXml();
        var report = new Report();
        report.Info("other-id", "Inserted <article-id pub-id-type=\"other\">00201</article-id> after the DOI.");
        report.Warn("credit-roles", "CREDIT statement is free prose; roles not auto-applied (AutoApplied).");

        var path = Path.Combine(_tempDir, "warn.diagnostic.json");
        var written = DiagnosticWriter.WritePhase3(path, "x.xml", xml, report, EmptyDispositions());

        Assert.True(written);
        using var json = JsonDocument.Parse(File.ReadAllText(path));
        var phase3 = json.RootElement.GetProperty("phase3");
        Assert.Equal("other-id", phase3.GetProperty("otherId").GetProperty("tag").GetString());
        Assert.Equal("00201", phase3.GetProperty("otherId").GetProperty("value").GetString());
        Assert.Equal("autoApplied", phase3.GetProperty("otherId").GetProperty("disposition").GetString());
    }

    private static IReadOnlyDictionary<string, ConfirmDisposition> EmptyDispositions()
        => new Dictionary<string, ConfirmDisposition>(StringComparer.Ordinal);

    private static (string, string?, string) Tuple(DiagnosticPhase3Tag t)
        => (t.Tag, t.Value, t.Disposition);
}
