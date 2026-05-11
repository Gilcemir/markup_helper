using DocFormatter.Cli;
using DocFormatter.Core.Reporting;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Xunit;

namespace DocFormatter.Tests.Phase2;

public sealed class Phase2PipelineIntegrationTests : IDisposable
{
    private const string DocOpening =
        "[doc sps=\"1.9\" volid=\"26\" issueno=\"1\" elocatid=\"xxx\" doctopic=\"oa\" language=\"en\"]";

    private readonly string _tempDir;

    public Phase2PipelineIntegrationTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"phase2-integration-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, recursive: true); } catch { /* best effort */ }
    }

    [Fact]
    public void Run_Phase2_OnSyntheticFixture_EmitsThreeExpectedLiteralsInOrder()
    {
        var sourcePath = Path.Combine(_tempDir, "synthetic.docx");
        WriteSynthetic(sourcePath);

        var exit = CliApp.Run(
            new[] { "phase2", sourcePath },
            new StringWriter(),
            new StringWriter());

        Assert.Equal(CliApp.ExitSuccess, exit);

        var producedPath = Path.Combine(_tempDir, "formatted-phase2", "synthetic.docx");
        Assert.True(File.Exists(producedPath));

        var bodyText = ReadBodyText(producedPath);

        // Elocation: doc opening tag rewritten in place; standalone paragraph removed.
        Assert.Contains("elocatid=\"e51362627\"", bodyText);
        Assert.Contains("issueno=\"2\"", bodyText);
        Assert.DoesNotContain("\ne51362627\n", bodyText);

        // Abstract wrapped with [sectitle] heading and [p] body.
        Assert.Contains("[xmlabstr language=\"en\"][sectitle]Abstract[/sectitle]", bodyText);
        Assert.Contains("[p]Body of the abstract.[/p][/xmlabstr]", bodyText);

        // Keywords wrapped with [sectitle] label and [kwd] per term.
        Assert.Contains(
            "[kwdgrp language=\"en\"][sectitle]Keywords:[/sectitle] "
            + "[kwd]K1[/kwd], [kwd]K2[/kwd], [kwd]K3[/kwd][/kwdgrp]",
            bodyText);

        // Order: elocation literal precedes abstract precedes kwdgrp.
        var elocationOffset = bodyText.IndexOf("elocatid=\"e51362627\"", StringComparison.Ordinal);
        var abstractOffset = bodyText.IndexOf("[xmlabstr", StringComparison.Ordinal);
        var kwdgrpOffset = bodyText.IndexOf("[kwdgrp", StringComparison.Ordinal);
        Assert.InRange(elocationOffset, 0, abstractOffset);
        Assert.InRange(abstractOffset, 0, kwdgrpOffset);
    }

    [Fact]
    public void Run_Phase2_OnSyntheticThreeAuthorFixture_EmitsXrefAuthoridAndCorrespLiteralsInOrder()
    {
        var sourcePath = Path.Combine(_tempDir, "three-authors.docx");
        WriteThreeAuthorFixture(sourcePath);

        var exit = CliApp.Run(
            new[] { "phase2", sourcePath },
            new StringWriter(),
            new StringWriter());

        Assert.Equal(CliApp.ExitSuccess, exit);

        var producedPath = Path.Combine(_tempDir, "formatted-phase2", "three-authors.docx");
        Assert.True(File.Exists(producedPath));

        var bodyText = ReadBodyText(producedPath);

        // Author 1 (Alice, single aff, no corresp, ORCID).
        Assert.Contains(
            "[author role=\"nd\" rid=\"aff1\" corresp=\"n\" deceased=\"n\" eqcontr=\"nd\"]"
                + "[fname]Alice[/fname] [surname]Smith[/surname]"
                + "[xref ref-type=\"aff\" rid=\"aff1\"]1[/xref] "
                + "[authorid authidtp=\"orcid\"]0000-0000-0000-0001[/authorid][/author]",
            bodyText);

        // Author 2 (Bob, corresp author, plain `1*` trailer expanded).
        Assert.Contains(
            "[author role=\"nd\" rid=\"aff1\" corresp=\"y\" deceased=\"n\" eqcontr=\"nd\"]"
                + "[fname]Bob[/fname] [surname]Johnson[/surname]"
                + "[xref ref-type=\"aff\" rid=\"aff1\"]1[/xref]"
                + "[xref ref-type=\"corresp\" rid=\"c1\"]*[/xref]"
                + "[authorid authidtp=\"orcid\"]0000-0000-0000-0002[/authorid][/author]",
            bodyText);

        // Author 3 (Carol, multi-aff, no corresp, ORCID).
        Assert.Contains(
            "[author role=\"nd\" rid=\"aff1 aff2\" corresp=\"n\" deceased=\"n\" eqcontr=\"nd\"]"
                + "[fname]Carol[/fname] [surname]Davis[/surname]"
                + "[xref ref-type=\"aff\" rid=\"aff1\"]1[/xref]"
                + "[xref ref-type=\"aff\" rid=\"aff2\"]2[/xref] "
                + "[authorid authidtp=\"orcid\"]0000-0000-0000-0003[/authorid][/author]",
            bodyText);

        // Corresp paragraph wrapped with inner [email].
        Assert.Contains(
            "[corresp id=\"c1\"]* E-mail: [email]bob@example.com[/email][/corresp]",
            bodyText);

        // Order: authors precede corresp.
        var alice = bodyText.IndexOf("[fname]Alice", StringComparison.Ordinal);
        var bob = bodyText.IndexOf("[fname]Bob", StringComparison.Ordinal);
        var carol = bodyText.IndexOf("[fname]Carol", StringComparison.Ordinal);
        var corresp = bodyText.IndexOf("[corresp", StringComparison.Ordinal);
        Assert.InRange(alice, 0, bob);
        Assert.InRange(bob, 0, carol);
        Assert.InRange(carol, 0, corresp);
    }

    [Fact]
    public void Run_Phase2_DiagnosticJsonContainsPhase2BlockKeys()
    {
        var sourcePath = Path.Combine(_tempDir, "diag.docx");
        WriteSynthetic(sourcePath);

        CliApp.Run(new[] { "phase2", sourcePath }, new StringWriter(), new StringWriter());

        var diagnosticPath = Path.Combine(_tempDir, "formatted-phase2", "diag.diagnostic.json");
        // Phase 2 rules emit Info-level entries on the golden path, so no
        // diagnostic.json is written. To force it, run on a fixture missing
        // the abstract / keywords blocks so the rules warn.
        if (!File.Exists(diagnosticPath))
        {
            // Run again on a partial fixture that triggers warns from the
            // abstract and keywords rules.
            var partial = Path.Combine(_tempDir, "diag-partial.docx");
            WritePartial(partial);
            CliApp.Run(new[] { "phase2", partial }, new StringWriter(), new StringWriter());
            diagnosticPath = Path.Combine(_tempDir, "formatted-phase2", "diag-partial.diagnostic.json");
        }

        Assert.True(File.Exists(diagnosticPath));
        var json = File.ReadAllText(diagnosticPath);
        Assert.Contains("\"phase2\"", json);
        Assert.Contains("\"elocation\"", json);
        Assert.Contains("\"abstract\"", json);
        Assert.Contains("\"keywords\"", json);
        Assert.Contains("\"hist\"", json);
    }

    [Fact]
    public void Run_Phase2_OnSyntheticHistoryFixture_EmitsHistBlockInStrictOrder()
    {
        // Phase 2 + Phase 4 end-to-end: synthetic fixture combining the
        // abstract/keywords/history paragraphs covered by EmitAbstractTagRule,
        // EmitKwdgrpTagRule, and EmitHistTagRule. Asserts the [hist] literal
        // wraps the three Received/Accepted/Published paragraphs in strict
        // DTD ordering and that the surrounding Phase 2 emitters still fire.
        var sourcePath = Path.Combine(_tempDir, "hist-synthetic.docx");
        WriteSyntheticWithHistory(sourcePath);

        var exit = CliApp.Run(
            new[] { "phase2", sourcePath },
            new StringWriter(),
            new StringWriter());

        Assert.Equal(CliApp.ExitSuccess, exit);

        var producedPath = Path.Combine(_tempDir, "formatted-phase2", "hist-synthetic.docx");
        Assert.True(File.Exists(producedPath));

        var bodyText = ReadBodyText(producedPath);

        // Hist block: opening on the Received paragraph, child wraps on each
        // of the three dated paragraphs, closing on the Published paragraph.
        Assert.Contains(
            "[hist]Received: [received dateiso=\"20240312\"]12 March 2024[/received]",
            bodyText);
        Assert.Contains(
            "Accepted: [accepted dateiso=\"20240415\"]15 April 2024[/accepted]",
            bodyText);
        Assert.Contains(
            "Published: [histdate dateiso=\"20240501\" datetype=\"pub\"]01 May 2024[/histdate][/hist]",
            bodyText);

        // Other Phase 2 emitters still fire on the same fixture.
        Assert.Contains("[xmlabstr language=\"en\"][sectitle]Abstract[/sectitle]", bodyText);
        Assert.Contains(
            "[kwdgrp language=\"en\"][sectitle]Keywords:[/sectitle] "
            + "[kwd]K1[/kwd], [kwd]K2[/kwd], [kwd]K3[/kwd][/kwdgrp]",
            bodyText);
    }

    private static void WriteSyntheticWithHistory(string path)
    {
        using var doc = WordprocessingDocument.Create(path, WordprocessingDocumentType.Document);
        var mainPart = doc.AddMainDocumentPart();
        var body = new Body(
            BuildPlainParagraph(DocOpening),
            BuildPlainParagraph("e51362627"),
            BuildPlainParagraph("Abstract"),
            BuildPlainParagraph("Body of the abstract."),
            BuildPlainParagraph("Keywords: K1, K2, K3"),
            BuildPlainParagraph("Received: 12 March 2024"),
            BuildPlainParagraph("Accepted: 15 April 2024"),
            BuildPlainParagraph("Published: 01 May 2024"));
        mainPart.Document = new Document(body);
    }

    private static void WriteThreeAuthorFixture(string path)
    {
        using var doc = WordprocessingDocument.Create(path, WordprocessingDocumentType.Document);
        var mainPart = doc.AddMainDocumentPart();
        var body = new Body(
            BuildPlainParagraph(DocOpening),
            BuildPlainParagraph(
                "[author role=\"nd\"][fname]Alice[/fname] [surname]Smith[/surname]"
                + "[xref ref-type=\"aff\" rid=\"aff1\"]1[/xref] 0000-0000-0000-0001[/author]"),
            BuildPlainParagraph(
                "[author role=\"nd\"][fname]Bob[/fname] [surname]Johnson[/surname]"
                + "1*0000-0000-0000-0002[/author]"),
            BuildPlainParagraph(
                "[author role=\"nd\"][fname]Carol[/fname] [surname]Davis[/surname]"
                + "[xref ref-type=\"aff\" rid=\"aff1\"]1[/xref]"
                + "[xref ref-type=\"aff\" rid=\"aff2\"]2[/xref] 0000-0000-0000-0003[/author]"),
            BuildPlainParagraph("* E-mail: bob@example.com"),
            BuildPlainParagraph("e51362627"),
            BuildPlainParagraph("Abstract"),
            BuildPlainParagraph("Body of the abstract."),
            BuildPlainParagraph("Keywords: K1, K2, K3"));
        mainPart.Document = new Document(body);
    }

    private static void WriteSynthetic(string path)
    {
        using var doc = WordprocessingDocument.Create(path, WordprocessingDocumentType.Document);
        var mainPart = doc.AddMainDocumentPart();
        var body = new Body(
            BuildPlainParagraph(DocOpening),
            BuildPlainParagraph("e51362627"),
            BuildPlainParagraph("Abstract"),
            BuildPlainParagraph("Body of the abstract."),
            BuildPlainParagraph("Keywords: K1, K2, K3"));
        mainPart.Document = new Document(body);
    }

    private static void WritePartial(string path)
    {
        using var doc = WordprocessingDocument.Create(path, WordprocessingDocumentType.Document);
        var mainPart = doc.AddMainDocumentPart();
        var body = new Body(
            BuildPlainParagraph(DocOpening),
            // No standalone elocation paragraph — elocation rule warns.
            BuildPlainParagraph("Some intro text"));
            // No abstract heading and no keywords block — both rules warn.
        mainPart.Document = new Document(body);
    }

    private static Paragraph BuildPlainParagraph(string text)
        => new(new Run(new Text(text) { Space = SpaceProcessingModeValues.Preserve }));

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
}
