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

        var ok = CliApp.TryResolvePhase3Layout(pkg, out _, out _, out _, out var error);

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

        var ok = CliApp.TryResolvePhase3Layout(pkg, out var markupDir, out var table, out var otherTablePath, out var error);

        Assert.True(ok);
        Assert.Null(error);
        Assert.Equal(Path.GetFullPath(markup), Path.GetFullPath(markupDir));
        Assert.Equal(Path.GetFullPath(Path.Combine(root, "other.txt")), Path.GetFullPath(otherTablePath));
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

        // A pairing failure is a recoverable per-document skip (ADR-004), not a
        // critical abort — its own non-zero code, distinct from a crash.
        Assert.Equal(CliApp.ExitPairingSkipped, exit);
        var reportPath = Path.Combine(pkg, "formatted-phase3", "orphan.report.txt");
        Assert.True(File.Exists(reportPath));
    }

    [Fact]
    public void Run_Phase3_SingleFile_UnresolvedAuthor_DiagnosticNamesTheAuthorAsNotFound()
    {
        // credit-corpus-v26n3-fixes ADR-005: the diagnostic carries the statement
        // as read and each author's resolution, so a pending article is diagnosed
        // without reopening the docx. "XYZ" matches no contributor → gated →
        // WARN → diagnostic written with XYZ as notFound and ABC as resolved.
        var root = Path.Combine(_tempDir, $"synthetic-{Guid.NewGuid():N}");
        var markupDir = Path.Combine(root, "scielo_markup");
        var packageDir = Path.Combine(root, "scielo_package");
        Directory.CreateDirectory(markupDir);
        Directory.CreateDirectory(packageDir);

        const string xmlName = "1984-7033-cbab-26-03-e56132631.xml";
        File.WriteAllText(Path.Combine(root, "other.txt"), "1984-7033-cbab-26-03-e56132631.pdf\t00301\n");
        Fixtures.Phase3.Phase3DocxFixtureBuilder.WriteMarkupDocxWithCreditStatement(
            Path.Combine(markupDir, "5613.docx"),
            "ABC: Conceptualization; Methodology. XYZ: Software.");
        File.WriteAllText(
            Path.Combine(packageDir, xmlName),
            "<article>\n" +
            "\t<front>\n" +
            "\t\t<article-meta>\n" +
            $"\t\t\t<article-id pub-id-type=\"doi\">{Fixtures.Phase3.Phase3DocxFixtureBuilder.MarkupDoi}</article-id>\n" +
            "\t\t\t<elocation-id>e56132631</elocation-id>\n" +
            "\t\t\t<contrib-group>\n" +
            "\t\t\t\t<contrib contrib-type=\"author\">\n" +
            "\t\t\t\t\t<name><surname>Costa</surname><given-names>Ana Beatriz</given-names></name>\n" +
            "\t\t\t\t</contrib>\n" +
            "\t\t\t</contrib-group>\n" +
            "\t\t</article-meta>\n" +
            "\t</front>\n" +
            "</article>\n");

        var exit = CliApp.Run(
            new[] { "phase3", Path.Combine(packageDir, xmlName), "--non-interactive=accept" },
            new StringWriter(),
            new StringWriter());

        Assert.Equal(CliApp.ExitSuccess, exit);

        var diagnosticPath = Path.Combine(packageDir, "formatted-phase3", "1984-7033-cbab-26-03-e56132631.diagnostic.json");
        Assert.True(File.Exists(diagnosticPath), ".diagnostic.json");

        using var json = System.Text.Json.JsonDocument.Parse(File.ReadAllText(diagnosticPath));
        var statement = json.RootElement.GetProperty("phase3").GetProperty("creditStatement");
        Assert.Equal("ABC: Conceptualization; Methodology. XYZ: Software.", statement.GetProperty("raw").GetString());
        Assert.Equal("authorKeyed", statement.GetProperty("shape").GetString());

        var entries = statement.GetProperty("entries").EnumerateArray().ToList();
        var abc = Assert.Single(entries, e => e.GetProperty("authorKey").GetString() == "ABC");
        var xyz = Assert.Single(entries, e => e.GetProperty("authorKey").GetString() == "XYZ");
        Assert.Equal("resolved", abc.GetProperty("resolution").GetString());
        Assert.True(abc.GetProperty("applied").GetBoolean());
        Assert.Equal("notFound", xyz.GetProperty("resolution").GetString());
        Assert.False(xyz.GetProperty("applied").GetBoolean());
    }

    // ── batch summary pendency block (credit-corpus-v26n3-fixes) ─────────────

    private static Phase3Outcome Processed(CreditOutcome? credit, int brokenNames = 0, bool prompted = false)
        => new("art", Phase3OutcomeKind.Processed, prompted, null, credit, brokenNames);

    private static CreditEntryOutcome Entry(
        string key,
        string resolution = CreditResolution.Resolved,
        bool applied = true,
        params string[] unknownTerms)
        => new(key, new[] { "Data curation" }, resolution, unknownTerms, applied);

    [Fact]
    public void Phase3PendencyLines_AutoAppliedAndNoBrokenNames_IsEmpty()
    {
        var credit = new CreditOutcome("A: Data curation.", CreditShape.AuthorKeyed,
            new[] { Entry("A") }, CreditDisposition.AutoApplied);

        Assert.Empty(CliApp.Phase3PendencyLines(Processed(credit)));
    }

    [Fact]
    public void Phase3PendencyLines_ConfirmedWithNotFoundAuthor_ListsAppliedAndPending()
    {
        var credit = new CreditOutcome("...", CreditShape.AuthorKeyed,
            new[] { Entry("TVB"), Entry("QHTP"), Entry("NHN", CreditResolution.NotFound, applied: false) },
            CreditDisposition.Confirmed);

        var line = Assert.Single(CliApp.Phase3PendencyLines(Processed(credit, prompted: true)));
        Assert.Equal("  credit: applied TVB, QHTP; pending NHN (notFound)", line);
    }

    [Fact]
    public void Phase3PendencyLines_UnknownTerm_NamesTheTerm()
    {
        var credit = new CreditOutcome("...", CreditShape.AuthorKeyed,
            new[] { Entry("X", applied: false, unknownTerms: "Metodology") },
            CreditDisposition.Confirmed);

        var line = Assert.Single(CliApp.Phase3PendencyLines(Processed(credit)));
        Assert.Equal("  credit: applied none; pending X (unknown term: Metodology)", line);
    }

    [Fact]
    public void Phase3PendencyLines_NotFoundAndUnknownTerm_JoinsBothReasons()
    {
        var credit = new CreditOutcome("...", CreditShape.AuthorKeyed,
            new[] { Entry("A"), Entry("Z", CreditResolution.NotFound, applied: false, "Q") },
            CreditDisposition.Confirmed);

        var line = Assert.Single(CliApp.Phase3PendencyLines(Processed(credit)));
        Assert.Equal("  credit: applied A; pending Z (notFound; unknown term: Q)", line);
    }

    [Fact]
    public void Phase3PendencyLines_SkippedByOperator_ReasonIsTheDisposition()
    {
        var credit = new CreditOutcome("...", CreditShape.AuthorKeyed,
            new[] { Entry("A", applied: false), Entry("B", CreditResolution.Ambiguous, applied: false) },
            CreditDisposition.Skipped);

        var line = Assert.Single(CliApp.Phase3PendencyLines(Processed(credit, prompted: true)));
        Assert.Equal("  credit: applied none; pending A (skipped), B (ambiguous)", line);
    }

    [Fact]
    public void Phase3PendencyLines_Prose_HeaderEmpty_Absent_HaveFixedWording()
    {
        static CreditOutcome Outcome(string disposition) =>
            new("raw", CreditShape.Prose, Array.Empty<CreditEntryOutcome>(), disposition);

        Assert.Equal("  credit: free prose (not auto-applied)",
            Assert.Single(CliApp.Phase3PendencyLines(Processed(Outcome(CreditDisposition.Prose)))));
        Assert.Equal("  credit: header found, body empty",
            Assert.Single(CliApp.Phase3PendencyLines(Processed(Outcome(CreditDisposition.HeaderEmpty)))));
        Assert.Equal("  credit: no CREDIT STATEMENT on the docx",
            Assert.Single(CliApp.Phase3PendencyLines(Processed(Outcome(CreditDisposition.Absent)))));
    }

    [Fact]
    public void Phase3PendencyLines_AlreadyPresentAuthors_AreListedApartAndNotPending()
    {
        // Re-run over an already-injected XML: six contributors skipped by the
        // idempotency check, one still unresolved (5719 shape on task_09's copy).
        var credit = new CreditOutcome("...", CreditShape.AuthorKeyed,
            new[]
            {
                new CreditEntryOutcome("LRS", new[] { "Data curation" }, CreditResolution.Resolved, Array.Empty<string>(), Applied: false, AlreadyPresent: true),
                new CreditEntryOutcome("GFPA", new[] { "Methodology" }, CreditResolution.Resolved, Array.Empty<string>(), Applied: false, AlreadyPresent: true),
                Entry("MRC", CreditResolution.NotFound, applied: false),
            },
            CreditDisposition.Confirmed);

        var line = Assert.Single(CliApp.Phase3PendencyLines(Processed(credit, prompted: true)));
        Assert.Equal("  credit: already present LRS, GFPA; pending MRC (notFound)", line);
    }

    [Fact]
    public void Phase3PendencyLines_AppliedAndAlreadyPresent_BothListedBeforePending()
    {
        var credit = new CreditOutcome("...", CreditShape.AuthorKeyed,
            new[]
            {
                Entry("A"),
                new CreditEntryOutcome("B", new[] { "Software" }, CreditResolution.Resolved, Array.Empty<string>(), Applied: false, AlreadyPresent: true),
                Entry("C", CreditResolution.Ambiguous, applied: false),
            },
            CreditDisposition.Confirmed);

        var line = Assert.Single(CliApp.Phase3PendencyLines(Processed(credit)));
        Assert.Equal("  credit: applied A; already present B; pending C (ambiguous)", line);
    }

    [Fact]
    public void Phase3PendencyLines_BrokenNames_AppendsCountAfterCreditLine()
    {
        var credit = new CreditOutcome("...", CreditShape.AuthorKeyed,
            new[] { Entry("PSA"), Entry("MAF", CreditResolution.NotFound, applied: false) },
            CreditDisposition.Confirmed);

        var lines = CliApp.Phase3PendencyLines(Processed(credit, brokenNames: 7, prompted: true));

        Assert.Equal(
            new[] { "  credit: applied PSA; pending MAF (notFound)", "  contrib-names: 7 broken surname(s)" },
            lines);
    }

    [Fact]
    public void Phase3PendencyLines_AutoAppliedWithBrokenNames_OnlyContribNamesLine()
    {
        var credit = new CreditOutcome("...", CreditShape.AuthorKeyed,
            new[] { Entry("A") }, CreditDisposition.AutoApplied);

        var line = Assert.Single(CliApp.Phase3PendencyLines(Processed(credit, brokenNames: 1)));
        Assert.Equal("  contrib-names: 1 broken surname(s)", line);
    }

    [Fact]
    public void Phase3PendencyLines_SkippedOrFailedOutcome_IsEmpty()
    {
        var credit = new CreditOutcome("...", CreditShape.Prose, Array.Empty<CreditEntryOutcome>(), CreditDisposition.Prose);

        Assert.Empty(CliApp.Phase3PendencyLines(new Phase3Outcome("a", Phase3OutcomeKind.Skipped, false, "no docx", credit, 3)));
        Assert.Empty(CliApp.Phase3PendencyLines(new Phase3Outcome("b", Phase3OutcomeKind.Failed, true, "boom", credit, 3)));
    }

    [Fact]
    public void WritePhase3BatchSummary_KeepsHeaderAndFileLines_AndIndentsPendencyUnderTheirFile()
    {
        var clean = new Phase3Outcome("clean", Phase3OutcomeKind.Processed, false, null,
            new CreditOutcome("A: Data curation.", CreditShape.AuthorKeyed, new[] { Entry("A") }, CreditDisposition.AutoApplied));
        var pending = new Phase3Outcome("pending", Phase3OutcomeKind.Processed, true, null,
            new CreditOutcome("...", CreditShape.AuthorKeyed,
                new[] { Entry("A"), Entry("NHN", CreditResolution.NotFound, applied: false) }, CreditDisposition.Confirmed),
            BrokenNames: 1);
        var skipped = new Phase3Outcome("orphan", Phase3OutcomeKind.Skipped, false, "no docx pairs");

        var path = Path.Combine(_tempDir, "_batch_summary.txt");
        CliApp.WritePhase3BatchSummary(path, new[] { clean, pending, skipped });

        Assert.Equal(
            new[]
            {
                "processed=2 prompted=1 skipped=1 failed=0",
                "clean.xml ✓",
                "pending.xml ✓ prompted",
                "  credit: applied A; pending NHN (notFound)",
                "  contrib-names: 1 broken surname(s)",
                "orphan.xml ⤼ skipped no docx pairs",
            },
            File.ReadAllLines(path));
    }

    [Fact]
    public void Run_Phase3_Batch_CleanAndPendingArticles_SummaryHasBlockOnlyUnderThePendingOne()
    {
        var root = Path.Combine(_tempDir, $"synthetic-batch-{Guid.NewGuid():N}");
        var markupDir = Path.Combine(root, "scielo_markup");
        var packageDir = Path.Combine(root, "scielo_package");
        Directory.CreateDirectory(markupDir);
        Directory.CreateDirectory(packageDir);

        const string cleanBase = "1984-7033-cbab-26-03-e10000001";
        const string pendingBase = "1984-7033-cbab-26-03-e20000002";
        File.WriteAllText(
            Path.Combine(root, "other.txt"),
            $"{cleanBase}.pdf\t00001\n{pendingBase}.pdf\t00002\n");

        Fixtures.Phase3.Phase3DocxFixtureBuilder.WriteMarkupDocxWithCreditStatement(
            Path.Combine(markupDir, "clean.docx"), "ABC: Conceptualization; Methodology.",
            "e10000001", "10.1590/clean");
        Fixtures.Phase3.Phase3DocxFixtureBuilder.WriteMarkupDocxWithCreditStatement(
            Path.Combine(markupDir, "pending.docx"), "ABC: Conceptualization. XYZ: Software.",
            "e20000002", "10.1590/pending");

        File.WriteAllText(Path.Combine(packageDir, $"{cleanBase}.xml"),
            SyntheticArticle("10.1590/clean", "e10000001", surname: "Costa", givenNames: "Ana Beatriz"));
        File.WriteAllText(Path.Combine(packageDir, $"{pendingBase}.xml"),
            // 5316 mechanism: the ORCID landed in <surname> and the full name in
            // <given-names>, so "ABC" still resolves through the given-only tier.
            SyntheticArticle("10.1590/pending", "e20000002", surname: "0000-0001-2345-6789", givenNames: "Ana Beatriz Costa"));

        var exit = CliApp.Run(
            new[] { "phase3", packageDir, "--non-interactive=accept" }, new StringWriter(), new StringWriter());

        Assert.Equal(CliApp.ExitSuccess, exit);
        var summary = File.ReadAllLines(Path.Combine(packageDir, "formatted-phase3", "_batch_summary.txt"));
        Assert.Equal(
            new[]
            {
                "processed=2 prompted=1 skipped=0 failed=0",
                $"{cleanBase}.xml ✓",
                $"{pendingBase}.xml ✓ prompted",
                "  credit: applied ABC; pending XYZ (notFound)",
                "  contrib-names: 1 broken surname(s)",
            },
            summary);
    }

    // A minimal JATS article with the DOI the other-id injector anchors on, the
    // elocation-id the pairer matches, and one contributor.
    private static string SyntheticArticle(string doi, string elocationId, string surname, string givenNames)
        => "<article>\n" +
           "\t<front>\n" +
           "\t\t<article-meta>\n" +
           $"\t\t\t<article-id pub-id-type=\"doi\">{doi}</article-id>\n" +
           $"\t\t\t<elocation-id>{elocationId}</elocation-id>\n" +
           "\t\t\t<contrib-group>\n" +
           "\t\t\t\t<contrib contrib-type=\"author\">\n" +
           $"\t\t\t\t\t<name><surname>{surname}</surname><given-names>{givenNames}</given-names></name>\n" +
           "\t\t\t\t</contrib>\n" +
           "\t\t\t</contrib-group>\n" +
           "\t\t</article-meta>\n" +
           "\t</front>\n" +
           "</article>\n";

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
