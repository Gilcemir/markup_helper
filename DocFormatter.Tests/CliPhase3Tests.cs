using DocFormatter.Cli;
using DocFormatter.Core.Jats;
using Xunit;

namespace DocFormatter.Tests;

/// <summary>
/// CLI-level tests for the <c>phase3</c> subcommand: dispatch, flag parsing,
/// confirmer-policy selection, layout resolution, exit codes, and end-to-end
/// orchestration over the <c>examples/phase-3/</c> corpus.
/// </summary>
public sealed class CliPhase3Tests : IDisposable
{
    private readonly string _tempDir;

    public CliPhase3Tests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"docfmt-cli-phase3-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, recursive: true); } catch { /* best effort */ }
    }

    // ── flag / dispatch / confirmer selection ────────────────────────────────

    [Fact]
    public void Help_DocumentsPhase3Subcommand()
    {
        var stdout = new StringWriter();
        var exit = CliApp.Run(new[] { "--help" }, stdout, new StringWriter());

        Assert.Equal(0, exit);
        var help = stdout.ToString();
        Assert.Contains("phase3", help);
        Assert.Contains("--non-interactive", help);
    }

    [Fact]
    public void Run_Phase3_NoArgs_PrintsUsageError()
    {
        var stderr = new StringWriter();

        var exit = CliApp.Run(new[] { "phase3" }, new StringWriter(), stderr);

        Assert.Equal(CliApp.ExitUsageError, exit);
        Assert.Contains("Usage:", stderr.ToString());
    }

    [Fact]
    public void Run_Phase3_InvalidNonInteractiveValue_PrintsUsageError()
    {
        var stderr = new StringWriter();
        var xml = Path.Combine(_tempDir, "x.xml");
        File.WriteAllText(xml, "<article/>");

        var exit = CliApp.Run(new[] { "phase3", xml, "--non-interactive=maybe" }, new StringWriter(), stderr);

        Assert.Equal(CliApp.ExitUsageError, exit);
        Assert.Contains("--non-interactive", stderr.ToString());
    }

    [Fact]
    public void Run_Phase3_NonXmlFile_ReturnsUsageError()
    {
        var txt = Path.Combine(_tempDir, "not-xml.txt");
        File.WriteAllText(txt, "x");

        var exit = CliApp.Run(new[] { "phase3", txt }, new StringWriter(), new StringWriter());

        Assert.Equal(CliApp.ExitUsageError, exit);
    }

    [Fact]
    public void Run_Phase3_PathDoesNotExist_ReturnsUsageError()
    {
        var bogus = Path.Combine(_tempDir, "nope.xml");
        var stderr = new StringWriter();

        var exit = CliApp.Run(new[] { "phase3", bogus }, new StringWriter(), stderr);

        Assert.Equal(CliApp.ExitUsageError, exit);
        Assert.Contains("path not found", stderr.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("accept", typeof(AutoAcceptConfirmer))]
    [InlineData("fail", typeof(FailOnPromptConfirmer))]
    public void TrySelectConfirmer_NonInteractiveValue_SelectsMatchingPolicy(string value, Type expected)
    {
        var ok = CliApp.TrySelectConfirmer(value, TextReader.Null, TextWriter.Null, out var confirmer, out var error);

        Assert.True(ok);
        Assert.Null(error);
        Assert.IsType(expected, confirmer);
    }

    [Fact]
    public void TrySelectConfirmer_Absent_SelectsConsoleConfirmer()
    {
        var ok = CliApp.TrySelectConfirmer(null, TextReader.Null, TextWriter.Null, out var confirmer, out var error);

        Assert.True(ok);
        Assert.Null(error);
        Assert.IsType<ConsoleConfirmer>(confirmer);
    }

    [Fact]
    public void TrySelectConfirmer_UnknownValue_FailsWithMessage()
    {
        var ok = CliApp.TrySelectConfirmer("nope", TextReader.Null, TextWriter.Null, out _, out var error);

        Assert.False(ok);
        Assert.NotNull(error);
        Assert.Contains("accept", error!);
    }

    [Fact]
    public void TryResolvePhase3Layout_NoOtherTxt_FailsWithMessage()
    {
        var pkg = Path.Combine(_tempDir, "scielo_package");
        Directory.CreateDirectory(pkg);

        var ok = CliApp.TryResolvePhase3Layout(pkg, out _, out _, out var error);

        Assert.False(ok);
        Assert.NotNull(error);
        Assert.Contains("other.txt", error!);
    }

    [Fact]
    public void TryResolvePhase3Layout_WalksUpToRoot_FindsOtherTxtAndMarkupDir()
    {
        var root = Path.Combine(_tempDir, "phase-3");
        var pkg = Path.Combine(root, "scielo_package");
        var markup = Path.Combine(root, "scielo_markup");
        Directory.CreateDirectory(pkg);
        Directory.CreateDirectory(markup);
        File.WriteAllText(Path.Combine(root, "other.txt"), "x.pdf\t00001");

        var ok = CliApp.TryResolvePhase3Layout(pkg, out var markupDir, out var table, out var error);

        Assert.True(ok);
        Assert.Null(error);
        Assert.Equal(Path.GetFullPath(markup), Path.GetFullPath(markupDir));
        Assert.True(table.TryGetOther("x", out var other));
        Assert.Equal("00001", other);
    }

    // ── end-to-end over the corpus ───────────────────────────────────────────

    private const string PromptingXml = "1984-7033-cbab-26-02-e54492621.xml"; // free-prose CREDIT → prompts

    [Fact]
    public void Run_Phase3_SingleFile_Accept_ProducesXmlReportAndDiagnostic()
    {
        var corpus = StageCorpus(PromptingXml);
        var xmlPath = Path.Combine(corpus, "scielo_package", PromptingXml);

        var stdout = new StringWriter();
        var exit = CliApp.Run(new[] { "phase3", xmlPath, "--non-interactive=accept" }, stdout, new StringWriter());

        Assert.Equal(CliApp.ExitSuccess, exit);

        var outDir = Path.Combine(corpus, "scielo_package", "formatted-phase3");
        var baseName = Path.GetFileNameWithoutExtension(PromptingXml);
        Assert.True(File.Exists(Path.Combine(outDir, $"{baseName}.xml")), "modified XML");
        Assert.True(File.Exists(Path.Combine(outDir, $"{baseName}.report.txt")), ".report.txt");
        Assert.True(File.Exists(Path.Combine(outDir, $"{baseName}.diagnostic.json")), ".diagnostic.json");

        // The other-id is the deterministic injection; it must be present.
        var producedXml = File.ReadAllText(Path.Combine(outDir, $"{baseName}.xml"));
        Assert.Contains("pub-id-type=\"other\"", producedXml);

        // Source XML untouched.
        Assert.DoesNotContain("pub-id-type=\"other\"", File.ReadAllText(xmlPath));
    }

    [Fact]
    public void Run_Phase3_Batch_Accept_EmitsBatchSummaryWithCounts()
    {
        var corpus = StageCorpus(PromptingXml, "1984-7033-cbab-26-02-e54242622.xml");
        var packageDir = Path.Combine(corpus, "scielo_package");

        var stdout = new StringWriter();
        var exit = CliApp.Run(new[] { "phase3", packageDir, "--non-interactive=accept" }, stdout, new StringWriter());

        Assert.Equal(CliApp.ExitSuccess, exit);
        Assert.Contains("phase3 batch complete:", stdout.ToString());
        Assert.Contains("processed=", stdout.ToString());

        var summaryPath = Path.Combine(packageDir, "formatted-phase3", "_batch_summary.txt");
        Assert.True(File.Exists(summaryPath));
        var summary = File.ReadAllLines(summaryPath);
        Assert.Contains("processed=", summary[0]);
        Assert.Contains("prompted=", summary[0]);
        Assert.Contains("skipped=", summary[0]);
        Assert.Contains("failed=", summary[0]);
    }

    [Fact]
    public void Run_Phase3_SingleFile_FailOnPrompt_ReturnsNonZeroExit()
    {
        var corpus = StageCorpus(PromptingXml);
        var xmlPath = Path.Combine(corpus, "scielo_package", PromptingXml);

        var stderr = new StringWriter();
        var exit = CliApp.Run(new[] { "phase3", xmlPath, "--non-interactive=fail" }, new StringWriter(), stderr);

        Assert.NotEqual(CliApp.ExitSuccess, exit);
        Assert.Equal(CliApp.ExitCriticalAbort, exit);
    }

    [Fact]
    public void Run_Phase3_UnpairableSingleFile_IsSkippedWithNonZeroExit()
    {
        // An XML with no other.txt entry / no matching docx fails pairing (ADR-004).
        var root = Path.Combine(_tempDir, "phase-3");
        var pkg = Path.Combine(root, "scielo_package");
        Directory.CreateDirectory(pkg);
        Directory.CreateDirectory(Path.Combine(root, "scielo_markup"));
        File.WriteAllText(Path.Combine(root, "other.txt"), "unrelated.pdf\t00001");
        var xmlPath = Path.Combine(pkg, "orphan.xml");
        File.WriteAllText(
            xmlPath,
            "<article><front><article-meta><elocation-id>e999</elocation-id>"
            + "<article-id pub-id-type=\"doi\">10.1/x</article-id></article-meta></front></article>");

        var stderr = new StringWriter();
        var exit = CliApp.Run(new[] { "phase3", xmlPath, "--non-interactive=accept" }, new StringWriter(), stderr);

        Assert.Equal(CliApp.ExitCriticalAbort, exit);
        var reportPath = Path.Combine(pkg, "formatted-phase3", "orphan.report.txt");
        Assert.True(File.Exists(reportPath));
    }

    // Copies the corpus other.txt + all docx + the named XMLs into a temp layout
    // (root/{other.txt, scielo_markup/, scielo_package/}) so the CLI's walk-up
    // layout resolution finds them, leaving the repo corpus untouched.
    private string StageCorpus(params string[] xmlNames)
    {
        var corpusRoot = ResolveCorpusRoot();
        var stageRoot = Path.Combine(_tempDir, $"phase-3-{Guid.NewGuid():N}");
        var stageMarkup = Path.Combine(stageRoot, "scielo_markup");
        var stagePackage = Path.Combine(stageRoot, "scielo_package");
        Directory.CreateDirectory(stageMarkup);
        Directory.CreateDirectory(stagePackage);

        File.Copy(Path.Combine(corpusRoot, "other.txt"), Path.Combine(stageRoot, "other.txt"));

        foreach (var docx in Directory.EnumerateFiles(
            Path.Combine(corpusRoot, "scielo_markup"), "*.docx"))
        {
            var name = Path.GetFileName(docx);
            if (name.StartsWith("~$", StringComparison.Ordinal))
            {
                continue;
            }
            File.Copy(docx, Path.Combine(stageMarkup, name));
        }

        foreach (var xmlName in xmlNames)
        {
            File.Copy(
                Path.Combine(corpusRoot, "scielo_package", xmlName),
                Path.Combine(stagePackage, xmlName));
        }

        return stageRoot;
    }

    private static string ResolveCorpusRoot()
    {
        var dir = AppContext.BaseDirectory;
        while (dir != null)
        {
            var candidate = Path.Combine(dir, "examples", "phase-3");
            if (Directory.Exists(candidate) && File.Exists(Path.Combine(candidate, "other.txt")))
            {
                return candidate;
            }
            dir = Path.GetDirectoryName(dir);
        }
        throw new InvalidOperationException(
            $"Could not locate examples/phase-3/ from {AppContext.BaseDirectory}.");
    }
}
