using DocFormatter.Core.Pipeline;
using DocFormatter.Core.Rules.Phase2;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Xunit;

namespace DocFormatter.Tests.Phase2;

public sealed class EmitElocationTagRuleTests : IDisposable
{
    private const string DocOpeningPlaceholder =
        "[doc sps=\"1.9\" volid=\"26\" issueno=\"1\" elocatid=\"xxx\" doctopic=\"oa\" language=\"en\"]";

    private readonly string _tempDir;

    public EmitElocationTagRuleTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"emit-elocation-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, recursive: true); } catch { /* best effort */ }
    }

    [Fact]
    public void Apply_GoldenPath_RewritesDocOpeningTagAndRemovesStandaloneParagraph()
    {
        var path = WriteFixture(
            new[]
            {
                BuildPlainParagraph(DocOpeningPlaceholder),
                BuildPlainParagraph("e2024001"),
                BuildPlainParagraph("Abstract"),
                BuildPlainParagraph("body"),
            });

        var (ctx, report) = Apply(path);

        Assert.Equal("e2024001", ctx.ElocationId);
        var bodyText = ReadBodyText(path);
        Assert.Contains("elocatid=\"e2024001\"", bodyText);
        Assert.DoesNotContain("elocatid=\"xxx\"", bodyText);
        Assert.DoesNotContain("e2024001\nAbstract", bodyText);
        Assert.DoesNotContain(
            report.Entries,
            e => e.Rule == nameof(EmitElocationTagRule) && e.Level == ReportLevel.Warn);
    }

    [Fact]
    public void Apply_DerivesIssuenoFromElocationId_AndRewritesDocOpeningIssueno()
    {
        // Format: e<article(4)><volid(2)><issueno(1)><order>. issueno is the
        // digit at index 7 of the elocation ID.
        var path = WriteFixture(
            new[]
            {
                BuildPlainParagraph(DocOpeningPlaceholder),
                BuildPlainParagraph("e51362627"),
            });

        Apply(path);

        var bodyText = ReadBodyText(path);
        Assert.Contains("issueno=\"2\"", bodyText);
        Assert.DoesNotContain("issueno=\"1\"", bodyText);
        Assert.Contains("elocatid=\"e51362627\"", bodyText);
    }

    [Fact]
    public void Apply_NoStandaloneElocationParagraph_SkipsAndWarnsWithReasonCode()
    {
        var path = WriteFixture(
            new[]
            {
                BuildPlainParagraph(DocOpeningPlaceholder),
                BuildPlainParagraph("Abstract"),
                BuildPlainParagraph("body"),
            });

        var (_, report) = Apply(path);

        Assert.Contains(
            report.Entries,
            e => e.Rule == nameof(EmitElocationTagRule)
                && e.Level == ReportLevel.Warn
                && e.Message == EmitElocationTagRule.ElocationParagraphMissingMessage);

        var bodyText = ReadBodyText(path);
        Assert.Contains("elocatid=\"xxx\"", bodyText); // unchanged
    }

    [Fact]
    public void Apply_NoDocOpeningTag_SkipsAndWarnsWithReasonCode()
    {
        var path = WriteFixture(
            new[]
            {
                BuildPlainParagraph("e2024001"),
                BuildPlainParagraph("Abstract"),
                BuildPlainParagraph("body"),
            });

        var (_, report) = Apply(path);

        Assert.Contains(
            report.Entries,
            e => e.Rule == nameof(EmitElocationTagRule)
                && e.Level == ReportLevel.Warn
                && e.Message == EmitElocationTagRule.DocOpeningTagMissingMessage);
    }

    [Fact]
    public void Apply_DocTagSplitAcrossMultipleRuns_StillRewritesElocatid()
    {
        var splitDoc = new Paragraph(
            BuildRun("[doc sps=\"1.9\" volid=\"26\" issueno=\""),
            BuildRun("1"),
            BuildRun("\" elocatid=\""),
            BuildRun("xxxx"),
            BuildRun("\" language=\"en\"]"));
        var path = WriteFixture(new[] { splitDoc, BuildPlainParagraph("e54192624") });

        Apply(path);

        var bodyText = ReadBodyText(path);
        Assert.Contains("elocatid=\"e54192624\"", bodyText);
        Assert.DoesNotContain("elocatid=\"xxxx\"", bodyText);
    }

    [Fact]
    public void Apply_DoesNotEmitAntiDuplicationListLiterals()
    {
        var path = WriteFixture(
            new[]
            {
                BuildPlainParagraph(DocOpeningPlaceholder),
                BuildPlainParagraph("e2024001"),
            });

        Apply(path);

        var bodyText = ReadBodyText(path);
        AssertNoAntiDuplicationLiterals(bodyText);
    }

    private (FormattingContext Ctx, IReport Report) Apply(string path)
    {
        var ctx = new FormattingContext();
        var report = new Report();
        using var doc = WordprocessingDocument.Open(path, isEditable: true);
        var rule = new EmitElocationTagRule();
        rule.Apply(doc, ctx, report);
        return (ctx, report);
    }

    private static Paragraph BuildPlainParagraph(string text)
        => new(new Run(new Text(text) { Space = SpaceProcessingModeValues.Preserve }));

    private static Run BuildRun(string text)
        => new(new Text(text) { Space = SpaceProcessingModeValues.Preserve });

    private string WriteFixture(IEnumerable<Paragraph> paragraphs)
    {
        var path = Path.Combine(_tempDir, $"{Guid.NewGuid():N}.docx");
        using var doc = WordprocessingDocument.Create(path, WordprocessingDocumentType.Document);
        var mainPart = doc.AddMainDocumentPart();
        var body = new Body();
        foreach (var p in paragraphs)
        {
            body.AppendChild(p);
        }
        mainPart.Document = new Document(body);
        return path;
    }

    private static string ReadBodyText(string path)
    {
        using var doc = WordprocessingDocument.Open(path, isEditable: false);
        var body = doc.MainDocumentPart!.Document!.Body!;
        var lines = new List<string>();
        foreach (var p in body.Elements<Paragraph>())
        {
            var text = string.Concat(p.Descendants<Text>().Select(t => t.Text));
            lines.Add(text);
        }
        return string.Join("\n", lines);
    }

    internal static void AssertNoAntiDuplicationLiterals(string bodyText)
    {
        // Anti-duplication invariant from docs/scielo_context/REENTRANCE.md.
        // SciELO Markup auto-marks these tags; pre-marking duplicates them.
        // Phase 2 emitters MUST NOT introduce them.
        var forbidden = new[]
        {
            "[author",
            "[/author]",
            "[fname]",
            "[/fname]",
            "[surname]",
            "[/surname]",
            "[kwd]",
            "[/kwd]",
            "[normaff",
            "[/normaff]",
        };
        // [doctitle] / [doi] are present in the corpus pre-Phase-2 but task 06
        // rules MUST NOT introduce additional copies. Test fixtures don't seed
        // them so a literal-presence check is sufficient as a smoke gate.
        foreach (var literal in forbidden)
        {
            Assert.DoesNotContain(literal, bodyText);
        }
    }
}
