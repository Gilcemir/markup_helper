using System.Security.Cryptography;
using System.Text.Json;
using DocFormatter.Cli;
using DocFormatter.Core.Reporting;
using DocFormatter.Core.Rules;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Xunit;

namespace DocFormatter.Tests;

public sealed class CliIntegrationTests : IDisposable
{
    private readonly string _tempDir;

    public CliIntegrationTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"docfmt-cli-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, recursive: true); } catch { /* best effort */ }
    }

    [Fact]
    public void Run_NoArguments_PrintsUsageToStderr_ReturnsUsageError()
    {
        var stdout = new StringWriter();
        var stderr = new StringWriter();

        var exit = CliApp.Run(Array.Empty<string>(), stdout, stderr);

        Assert.Equal(1, exit);
        Assert.Empty(stdout.ToString());
        Assert.Contains("Usage:", stderr.ToString());
    }

    [Fact]
    public void Run_HelpFlag_PrintsUsageToStdout_ReturnsSuccess()
    {
        var stdout = new StringWriter();
        var stderr = new StringWriter();

        var exit = CliApp.Run(new[] { "--help" }, stdout, stderr);

        Assert.Equal(0, exit);
        Assert.Contains("Usage:", stdout.ToString());
        Assert.Empty(stderr.ToString());
    }

    [Fact]
    public void Run_VersionFlag_PrintsAssemblyInformationalVersion_ReturnsSuccess()
    {
        var stdout = new StringWriter();
        var stderr = new StringWriter();

        var exit = CliApp.Run(new[] { "--version" }, stdout, stderr);

        Assert.Equal(0, exit);
        var printed = stdout.ToString().Trim();
        Assert.Equal(CliApp.GetVersion(), printed);
        Assert.False(string.IsNullOrEmpty(printed));
        Assert.Empty(stderr.ToString());
    }

    [Fact]
    public void Run_PathDoesNotExist_PrintsErrorToStderr_ReturnsUsageError()
    {
        var stdout = new StringWriter();
        var stderr = new StringWriter();
        var bogusPath = Path.Combine(_tempDir, "nope.docx");

        var exit = CliApp.Run(new[] { bogusPath }, stdout, stderr);

        Assert.Equal(1, exit);
        Assert.Contains("path not found", stderr.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Run_SingleFile_HappyPath_ProducesOutputAndReport_LeavesSourceUntouched()
    {
        var sourcePath = Path.Combine(_tempDir, "happy.docx");
        DocxFixtureBuilder.WriteValidDocx(sourcePath);
        var sourceHash = HashFile(sourcePath);

        var exit = CliApp.Run(new[] { sourcePath }, new StringWriter(), new StringWriter());

        Assert.Equal(0, exit);
        var formattedDir = Path.Combine(_tempDir, "formatted");
        Assert.True(File.Exists(Path.Combine(formattedDir, "happy.docx")));
        Assert.True(File.Exists(Path.Combine(formattedDir, "happy.report.txt")));
        Assert.False(File.Exists(Path.Combine(formattedDir, "happy.diagnostic.json")));
        Assert.Equal(sourceHash, HashFile(sourcePath));
    }

    [Fact]
    public void Run_SingleFile_RuleEmitsWarning_WritesDiagnosticJson_RoundTrips()
    {
        var sourcePath = Path.Combine(_tempDir, "warn.docx");
        DocxFixtureBuilder.WriteDocxWithoutAbstract(sourcePath);

        var exit = CliApp.Run(new[] { sourcePath }, new StringWriter(), new StringWriter());

        Assert.Equal(0, exit);
        var formattedDir = Path.Combine(_tempDir, "formatted");
        var diagnosticPath = Path.Combine(formattedDir, "warn.diagnostic.json");
        Assert.True(File.Exists(diagnosticPath));

        var raw = File.ReadAllText(diagnosticPath);
        var doc = JsonSerializer.Deserialize<DiagnosticDocument>(raw, DiagnosticWriter.JsonOptions);
        Assert.NotNull(doc);
        Assert.Equal("warn.docx", doc!.File);
        Assert.Equal("warning", doc.Status);
        Assert.Contains(doc.Issues, i => i.Level == "warn");

        var rewritten = JsonSerializer.Serialize(doc, DiagnosticWriter.JsonOptions);
        var roundtripped = JsonSerializer.Deserialize<DiagnosticDocument>(rewritten, DiagnosticWriter.JsonOptions);
        Assert.Equal(doc, roundtripped);
    }

    [Fact]
    public void Run_SingleFile_CriticalAbort_WritesDiagnosticJson_StatusError()
    {
        var sourcePath = Path.Combine(_tempDir, "broken-abort.docx");
        DocxFixtureBuilder.WriteDocxWithoutTopTable(sourcePath);

        var exit = CliApp.Run(new[] { sourcePath }, new StringWriter(), new StringWriter());

        Assert.Equal(2, exit);
        var formattedDir = Path.Combine(_tempDir, "formatted");
        var diagnosticPath = Path.Combine(formattedDir, "broken-abort.diagnostic.json");
        Assert.True(File.Exists(diagnosticPath));

        var doc = JsonSerializer.Deserialize<DiagnosticDocument>(
            File.ReadAllText(diagnosticPath),
            DiagnosticWriter.JsonOptions);
        Assert.NotNull(doc);
        Assert.Equal("error", doc!.Status);
        Assert.Contains(doc.Issues, i => i.Level == "error");
    }

    [Fact]
    public void Run_SingleFile_MissingTopTable_ReturnsCriticalAbort_NoOutputDocx_ReportRecordsReason()
    {
        var sourcePath = Path.Combine(_tempDir, "broken.docx");
        DocxFixtureBuilder.WriteDocxWithoutTopTable(sourcePath);

        var exit = CliApp.Run(new[] { sourcePath }, new StringWriter(), new StringWriter());

        Assert.Equal(2, exit);
        var formattedDir = Path.Combine(_tempDir, "formatted");
        Assert.False(File.Exists(Path.Combine(formattedDir, "broken.docx")));
        var reportPath = Path.Combine(formattedDir, "broken.report.txt");
        Assert.True(File.Exists(reportPath));
        var reportContent = File.ReadAllText(reportPath);
        Assert.Contains("[ERROR]", reportContent);
        Assert.Contains(ExtractTopTableRule.CriticalAbortMessage, reportContent);
    }

    [Fact]
    public void Run_Batch_TwoValid_OneMissingTopTable_WritesSummary_OnlyValidFilesProduceOutput()
    {
        var folder = Path.Combine(_tempDir, "inbox");
        Directory.CreateDirectory(folder);

        var ok1 = Path.Combine(folder, "a-ok.docx");
        var ok2 = Path.Combine(folder, "b-ok.docx");
        var bad = Path.Combine(folder, "c-bad.docx");
        DocxFixtureBuilder.WriteValidDocx(ok1);
        DocxFixtureBuilder.WriteValidDocx(ok2);
        DocxFixtureBuilder.WriteDocxWithoutTopTable(bad);

        var exit = CliApp.Run(new[] { folder }, new StringWriter(), new StringWriter());

        Assert.Equal(0, exit);
        var formattedDir = Path.Combine(folder, "formatted");
        Assert.True(File.Exists(Path.Combine(formattedDir, "a-ok.docx")));
        Assert.True(File.Exists(Path.Combine(formattedDir, "b-ok.docx")));
        Assert.False(File.Exists(Path.Combine(formattedDir, "c-bad.docx")));

        var summaryPath = Path.Combine(formattedDir, "_batch_summary.txt");
        Assert.True(File.Exists(summaryPath));
        var summary = File.ReadAllLines(summaryPath);
        Assert.Equal(3, summary.Length);
        Assert.Equal(2, summary.Count(l => l.Contains(" ✓")));
        Assert.Equal(1, summary.Count(l => l.Contains(" ✗")));
        Assert.Contains(summary, l => l.StartsWith("a-ok.docx ✓", StringComparison.Ordinal));
        Assert.Contains(summary, l => l.StartsWith("b-ok.docx ✓", StringComparison.Ordinal));
        Assert.Contains(summary, l => l.StartsWith("c-bad.docx ✗", StringComparison.Ordinal));
    }

    [Fact]
    public void Run_SingleFile_WritesAppLogUnderFormattedDirectory()
    {
        var sourcePath = Path.Combine(_tempDir, "with-log.docx");
        DocxFixtureBuilder.WriteValidDocx(sourcePath);

        CliApp.Run(new[] { sourcePath }, new StringWriter(), new StringWriter());

        var logPath = Path.Combine(_tempDir, "formatted", "_app.log");
        Assert.True(File.Exists(logPath));
    }

    private static string HashFile(string path)
    {
        using var stream = File.OpenRead(path);
        var hash = SHA256.HashData(stream);
        return Convert.ToHexString(hash);
    }
}

internal static class DocxFixtureBuilder
{
    public static void WriteValidDocx(string path)
    {
        using var doc = WordprocessingDocument.Create(path, WordprocessingDocumentType.Document);
        var mainPart = doc.AddMainDocumentPart();
        mainPart.Document = new Document(BuildBody(includeTopTable: true, includeAbstract: true));
    }

    public static void WriteDocxWithoutTopTable(string path)
    {
        using var doc = WordprocessingDocument.Create(path, WordprocessingDocumentType.Document);
        var mainPart = doc.AddMainDocumentPart();
        mainPart.Document = new Document(BuildBody(includeTopTable: false, includeAbstract: true));
    }

    public static void WriteDocxWithoutAbstract(string path)
    {
        using var doc = WordprocessingDocument.Create(path, WordprocessingDocumentType.Document);
        var mainPart = doc.AddMainDocumentPart();
        mainPart.Document = new Document(BuildBody(includeTopTable: true, includeAbstract: false));
    }

    private static Body BuildBody(bool includeTopTable, bool includeAbstract)
    {
        var children = new List<OpenXmlElement>();

        if (includeTopTable)
        {
            children.Add(BuildTopTable());
        }

        children.Add(PlainParagraph("Original Article"));
        children.Add(PlainParagraph("On the Behavior of Title"));
        children.Add(BuildAuthorsParagraph());

        if (includeAbstract)
        {
            children.Add(BuildAbstractParagraph());
        }

        return new Body(children);
    }

    private static Table BuildTopTable()
    {
        var grid = new TableGrid(
            new GridColumn { Width = "2000" },
            new GridColumn { Width = "2000" },
            new GridColumn { Width = "2000" });
        return new Table(
            grid,
            new TableRow(
                BuildCell("id", "ART01"),
                BuildCell("elocation", "e2024001"),
                BuildCell("doi", "10.1234/abc")));
    }

    private static TableCell BuildCell(params string[] paragraphTexts)
    {
        var cell = new TableCell();
        foreach (var text in paragraphTexts)
        {
            cell.AppendChild(PlainParagraph(text));
        }
        return cell;
    }

    private static Paragraph PlainParagraph(string text)
        => new(new Run(new Text(text) { Space = SpaceProcessingModeValues.Preserve }));

    private static Paragraph BuildAuthorsParagraph()
    {
        var nameRun = new Run(new Text("Maria Silva") { Space = SpaceProcessingModeValues.Preserve });
        var labelProperties = new RunProperties(
            new VerticalTextAlignment { Val = VerticalPositionValues.Superscript });
        var labelRun = new Run(
            labelProperties,
            new Text("1") { Space = SpaceProcessingModeValues.Preserve });
        return new Paragraph(nameRun, labelRun);
    }

    private static Paragraph BuildAbstractParagraph()
    {
        var boldRun = new Run(
            new RunProperties(new Bold()),
            new Text("Abstract") { Space = SpaceProcessingModeValues.Preserve });
        var bodyRun = new Run(
            new Text(" — body goes here") { Space = SpaceProcessingModeValues.Preserve });
        return new Paragraph(boldRun, bodyRun);
    }
}
