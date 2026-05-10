using DocFormatter.Cli;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Xunit;

namespace DocFormatter.Tests;

public sealed class CliPhase2Tests : IDisposable
{
    private readonly string _tempDir;

    public CliPhase2Tests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"docfmt-cli-phase2-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, recursive: true); } catch { /* best effort */ }
    }

    [Fact]
    public void Help_DocumentsPhase2AndPhase2VerifySubcommands()
    {
        var stdout = new StringWriter();
        var exit = CliApp.Run(new[] { "--help" }, stdout, new StringWriter());

        Assert.Equal(0, exit);
        var help = stdout.ToString();
        Assert.Contains("phase2", help);
        Assert.Contains("phase2-verify", help);
    }

    [Fact]
    public void Run_Phase2Subcommand_NoArgs_PrintsUsageError()
    {
        var stderr = new StringWriter();

        var exit = CliApp.Run(new[] { "phase2" }, new StringWriter(), stderr);

        Assert.Equal(CliApp.ExitUsageError, exit);
        Assert.Contains("Usage:", stderr.ToString());
    }

    [Fact]
    public void Run_Phase2VerifySubcommand_MissingArgs_PrintsUsageError()
    {
        var stderr = new StringWriter();

        var exit = CliApp.Run(new[] { "phase2-verify" }, new StringWriter(), stderr);

        Assert.Equal(CliApp.ExitUsageError, exit);
        Assert.Contains("phase2-verify", stderr.ToString());
    }

    [Fact]
    public void Run_Phase2_SingleFile_WritesOutputsUnderFormattedPhase2_SourceUntouched()
    {
        var sourcePath = Path.Combine(_tempDir, "alpha.docx");
        WritePlainDocx(sourcePath, "hello", "world");
        var sourceBytesBefore = File.ReadAllBytes(sourcePath);

        var exit = CliApp.Run(new[] { "phase2", sourcePath }, new StringWriter(), new StringWriter());

        Assert.Equal(CliApp.ExitSuccess, exit);

        var phase2Dir = Path.Combine(_tempDir, "formatted-phase2");
        Assert.True(Directory.Exists(phase2Dir));
        Assert.True(File.Exists(Path.Combine(phase2Dir, "alpha.docx")));
        Assert.True(File.Exists(Path.Combine(phase2Dir, "alpha.report.txt")));
        Assert.True(File.Exists(Path.Combine(phase2Dir, "_app.log")));

        // At task 06 the Phase 2 rule set has three emitters; a plain docx
        // missing the elocation paragraph / abstract heading / keywords block
        // triggers per-rule skip-and-warn (ADR-002), so diagnostic.json IS
        // written.
        Assert.True(File.Exists(Path.Combine(phase2Dir, "alpha.diagnostic.json")));

        // Phase 1's "formatted/" directory must NOT be created by phase2.
        Assert.False(Directory.Exists(Path.Combine(_tempDir, "formatted")));

        Assert.Equal(sourceBytesBefore, File.ReadAllBytes(sourcePath));
    }

    [Fact]
    public void Run_Phase2_Directory_WritesPerFileOutputsAndBatchSummaryUnderFormattedPhase2()
    {
        var folder = Path.Combine(_tempDir, "phase2-batch");
        Directory.CreateDirectory(folder);
        var a = Path.Combine(folder, "a.docx");
        var b = Path.Combine(folder, "b.docx");
        WritePlainDocx(a, "first");
        WritePlainDocx(b, "second");

        var exit = CliApp.Run(new[] { "phase2", folder }, new StringWriter(), new StringWriter());

        Assert.Equal(CliApp.ExitSuccess, exit);

        var phase2Dir = Path.Combine(folder, "formatted-phase2");
        Assert.True(File.Exists(Path.Combine(phase2Dir, "a.docx")));
        Assert.True(File.Exists(Path.Combine(phase2Dir, "b.docx")));
        Assert.True(File.Exists(Path.Combine(phase2Dir, "_batch_summary.txt")));
        Assert.True(File.Exists(Path.Combine(phase2Dir, "_app.log")));

        Assert.False(Directory.Exists(Path.Combine(folder, "formatted")));

        var summary = File.ReadAllLines(Path.Combine(phase2Dir, "_batch_summary.txt"));
        Assert.Equal(2, summary.Length);
        Assert.Contains(summary, l => l.StartsWith("a.docx", StringComparison.Ordinal));
        Assert.Contains(summary, l => l.StartsWith("b.docx", StringComparison.Ordinal));
    }

    [Fact]
    public void Run_Phase2_NonDocxFile_ReturnsUsageError()
    {
        var txt = Path.Combine(_tempDir, "not-a-doc.txt");
        File.WriteAllText(txt, "x");

        var exit = CliApp.Run(new[] { "phase2", txt }, new StringWriter(), new StringWriter());

        Assert.Equal(CliApp.ExitUsageError, exit);
    }

    [Fact]
    public void Run_Phase2_PathDoesNotExist_ReturnsUsageError()
    {
        var bogus = Path.Combine(_tempDir, "nope.docx");

        var stderr = new StringWriter();
        var exit = CliApp.Run(new[] { "phase2", bogus }, new StringWriter(), stderr);

        Assert.Equal(CliApp.ExitUsageError, exit);
        Assert.Contains("path not found", stderr.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Run_Phase2Verify_ByteIdenticalPairs_PrintsPassPerFile_ReturnsExitZero()
    {
        var beforeDir = Path.Combine(_tempDir, "before");
        var afterDir = Path.Combine(_tempDir, "after");
        Directory.CreateDirectory(beforeDir);
        Directory.CreateDirectory(afterDir);

        // Use plain docx files with no bracket tags so that with the empty
        // Phase 2 rule set (task 05) the produced output equals before, and
        // before equals after under the diff (no out-of-scope strip applies
        // because there are no bracket tags in the body).
        foreach (var id in new[] { "5136", "5293", "5419" })
        {
            WritePlainDocx(Path.Combine(beforeDir, $"{id}.docx"), $"body of {id}");
            WritePlainDocx(Path.Combine(afterDir, $"{id}.docx"), $"body of {id}");
        }

        var stdout = new StringWriter();
        var exit = CliApp.Run(
            new[] { "phase2-verify", beforeDir, afterDir },
            stdout,
            new StringWriter());

        Assert.Equal(CliApp.ExitSuccess, exit);
        var output = stdout.ToString();
        Assert.Contains("[PASS] 5136", output);
        Assert.Contains("[PASS] 5293", output);
        Assert.Contains("[PASS] 5419", output);
        Assert.DoesNotContain("[FAIL]", output);
    }

    [Fact]
    public void Run_Phase2Verify_MutatedAfter_PrintsFailWithDivergenceContext_ReturnsExitOne()
    {
        var beforeDir = Path.Combine(_tempDir, "before");
        var afterDir = Path.Combine(_tempDir, "after-mutated");
        Directory.CreateDirectory(beforeDir);
        Directory.CreateDirectory(afterDir);

        WritePlainDocx(Path.Combine(beforeDir, "5136.docx"), "intact body");
        WritePlainDocx(Path.Combine(afterDir, "5136.docx"), "MUTATED body");

        var stdout = new StringWriter();
        var exit = CliApp.Run(
            new[] { "phase2-verify", beforeDir, afterDir },
            stdout,
            new StringWriter());

        Assert.Equal(CliApp.ExitVerifyMismatch, exit);
        var output = stdout.ToString();
        Assert.Contains("[FAIL] 5136", output);
        Assert.Contains("diverge at offset", output);
        Assert.Contains("produced:", output);
        Assert.Contains("after:", output);
    }

    [Fact]
    public void Run_Phase2Verify_MissingCounterpartInAfter_FailsThatPair_ReturnsExitOne()
    {
        var beforeDir = Path.Combine(_tempDir, "before");
        var afterDir = Path.Combine(_tempDir, "after");
        Directory.CreateDirectory(beforeDir);
        Directory.CreateDirectory(afterDir);

        WritePlainDocx(Path.Combine(beforeDir, "5136.docx"), "x");

        // afterDir has no counterpart for 5136.docx.
        var stdout = new StringWriter();
        var exit = CliApp.Run(
            new[] { "phase2-verify", beforeDir, afterDir },
            stdout,
            new StringWriter());

        Assert.Equal(CliApp.ExitVerifyMismatch, exit);
        Assert.Contains("[FAIL] 5136", stdout.ToString());
        Assert.Contains("missing counterpart", stdout.ToString());
    }

    [Fact]
    public void Run_Phase2Verify_OutOfScopeTagInAfterHasBracketsStrippedSymmetrically_PassesAtTaskScope()
    {
        // The strip is symmetric and content-preserving: an out-of-scope pair
        // has its brackets removed but its content stays. Build before/after
        // such that produced (= before, since none of the Phase 2 rules'
        // heuristics trigger here) equals after AFTER stripping the
        // out-of-scope `[fname]` brackets from BOTH sides. `fname` is not in
        // the cumulative scope (anti-duplication, ADR-001).
        var beforeDir = Path.Combine(_tempDir, "before");
        var afterDir = Path.Combine(_tempDir, "after");
        Directory.CreateDirectory(beforeDir);
        Directory.CreateDirectory(afterDir);

        WritePlainDocx(Path.Combine(beforeDir, "id.docx"), "alpha beta gamma");
        WritePlainDocx(
            Path.Combine(afterDir, "id.docx"),
            "alpha [fname]beta[/fname] gamma");

        var stdout = new StringWriter();
        var exit = CliApp.Run(
            new[] { "phase2-verify", beforeDir, afterDir },
            stdout,
            new StringWriter());

        Assert.Equal(CliApp.ExitSuccess, exit);
        Assert.Contains("[PASS] id", stdout.ToString());
    }

    [Fact]
    public void Run_Phase2Verify_BeforeDirNotFound_ReturnsUsageError()
    {
        var afterDir = Path.Combine(_tempDir, "after");
        Directory.CreateDirectory(afterDir);

        var stderr = new StringWriter();
        var exit = CliApp.Run(
            new[] { "phase2-verify", Path.Combine(_tempDir, "missing-before"), afterDir },
            new StringWriter(),
            stderr);

        Assert.Equal(CliApp.ExitUsageError, exit);
        Assert.Contains("before directory not found", stderr.ToString());
    }

    [Fact]
    public void Run_Phase2Verify_AfterDirNotFound_ReturnsUsageError()
    {
        var beforeDir = Path.Combine(_tempDir, "before");
        Directory.CreateDirectory(beforeDir);

        var stderr = new StringWriter();
        var exit = CliApp.Run(
            new[] { "phase2-verify", beforeDir, Path.Combine(_tempDir, "missing-after") },
            new StringWriter(),
            stderr);

        Assert.Equal(CliApp.ExitUsageError, exit);
        Assert.Contains("after directory not found", stderr.ToString());
    }

    [Fact]
    public void Run_Phase2Verify_EmptyBeforeDirectory_ReturnsExitZeroWithMessage()
    {
        var beforeDir = Path.Combine(_tempDir, "before");
        var afterDir = Path.Combine(_tempDir, "after");
        Directory.CreateDirectory(beforeDir);
        Directory.CreateDirectory(afterDir);

        var stdout = new StringWriter();
        var exit = CliApp.Run(
            new[] { "phase2-verify", beforeDir, afterDir },
            stdout,
            new StringWriter());

        Assert.Equal(CliApp.ExitSuccess, exit);
        Assert.Contains("no .docx files", stdout.ToString());
    }

    [Fact]
    public void Run_Phase1Default_Regression_StillProducesFormattedDirOutputs()
    {
        // Anchor: the dispatcher refactor must not change Phase 1 behavior for
        // the default invocation. Source untouched, output under formatted/.
        var sourcePath = Path.Combine(_tempDir, "regression.docx");
        DocxFixtureBuilder.WriteValidDocx(sourcePath);

        var exit = CliApp.Run(new[] { sourcePath }, new StringWriter(), new StringWriter());

        Assert.Equal(CliApp.ExitSuccess, exit);
        var formattedDir = Path.Combine(_tempDir, "formatted");
        Assert.True(File.Exists(Path.Combine(formattedDir, "regression.docx")));
        Assert.True(File.Exists(Path.Combine(formattedDir, "regression.report.txt")));
        Assert.False(Directory.Exists(Path.Combine(_tempDir, "formatted-phase2")));
    }

    private static void WritePlainDocx(string path, params string[] paragraphTexts)
    {
        using var doc = WordprocessingDocument.Create(path, WordprocessingDocumentType.Document);
        var mainPart = doc.AddMainDocumentPart();
        var body = new Body();
        foreach (var text in paragraphTexts)
        {
            body.AppendChild(new Paragraph(
                new Run(new Text(text) { Space = SpaceProcessingModeValues.Preserve })));
        }
        mainPart.Document = new Document(body);
    }
}
