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

        // Abstract wrapped.
        Assert.Contains("[xmlabstr language=\"en\"]Abstract", bodyText);
        Assert.Contains("Body of the abstract.[/xmlabstr]", bodyText);

        // Keywords wrapped, no [kwd] per item.
        Assert.Contains("[kwdgrp language=\"en\"]Keywords: K1, K2, K3[/kwdgrp]", bodyText);
        Assert.DoesNotContain("[kwd]", bodyText);

        // Order: elocation literal precedes abstract precedes kwdgrp.
        var elocationOffset = bodyText.IndexOf("elocatid=\"e51362627\"", StringComparison.Ordinal);
        var abstractOffset = bodyText.IndexOf("[xmlabstr", StringComparison.Ordinal);
        var kwdgrpOffset = bodyText.IndexOf("[kwdgrp", StringComparison.Ordinal);
        Assert.InRange(elocationOffset, 0, abstractOffset);
        Assert.InRange(abstractOffset, 0, kwdgrpOffset);
    }

    [Fact]
    public void Run_Phase2_DiagnosticJsonContainsPhase2BlockWithThreeFields()
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
