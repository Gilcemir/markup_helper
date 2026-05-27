using DocFormatter.Cli;
using DocFormatter.Core.Reporting;
using Xunit;
using Xunit.Abstractions;

namespace DocFormatter.Tests.Phase3;

/// <summary>
/// The Phase 3 release gate (ADR-005 / ADR-006), mirroring
/// <see cref="DocFormatter.Tests.Phase2.Phase2CorpusTests"/>. Stages the
/// <c>examples/phase-3/</c> corpus into a temp layout, runs the <c>phase3</c>
/// subcommand over it with <c>--non-interactive=accept</c>, and asserts that each
/// produced XML is byte-equal to its curated golden under
/// <c>examples/phase-3/expected/&lt;basename&gt;.xml</c> and that the only
/// difference from the source XML is the four injected tags (no incidental
/// reformatting — proving the whitespace-preserving writer). A second strict run
/// with <c>--non-interactive=fail</c> documents the set of ambiguous documents
/// that prompt. The accept goldens encode the tool's auto + best-guess proposals
/// (ADR-006), so the same gate exercises the proposal path.
/// </summary>
public sealed class Phase3CorpusTests
{
    private const string NamePrefix = "1984-7033-cbab-26-02-";

    // The single fully-deterministic document: author-keyed CRediT with all terms
    // exact-mapping and all initials resolving, plus an auto-classified
    // data-availability category. It is the only document that does NOT prompt.
    private const string AutoBasename = NamePrefix + "e51362627"; // docx 5136

    // Representative CRediT shapes (ADR-005), by paired docx basename:
    private const string RoleKeyedBasename = NamePrefix + "e55232626"; // docx 5523 (role-keyed)
    private const string AuthorKeyedBasename = AutoBasename;           // docx 5136 (author-keyed)
    private const string ProseBasename = NamePrefix + "e54492621";     // docx 5449 (free prose)

    // Documents that prompt under --non-interactive=fail (the current ambiguity
    // set: free-prose CRediT, unrecognized terms, unresolved initials, or an
    // ambiguous data-availability category). Updated intentionally when the
    // corpus or the confidence heuristics change.
    private static readonly HashSet<string> AmbiguousBasenames = new(StringComparer.Ordinal)
    {
        NamePrefix + "e52932623",
        NamePrefix + "e53132629",
        NamePrefix + "e54192624",
        NamePrefix + "e54242622",
        NamePrefix + "e54342625",
        NamePrefix + "e54492621",
        NamePrefix + "e54582628",
        NamePrefix + "e548726214",
        NamePrefix + "e551726212",
        NamePrefix + "e55232626",
        NamePrefix + "e554826211",
        NamePrefix + "e554926210",
        NamePrefix + "e557026213",
        NamePrefix + "e564026215",
    };

    private const string CreditRoleSignature = "content-type=\"http://credit.niso.org/contributor-roles/";

    private readonly ITestOutputHelper _output;

    public Phase3CorpusTests(ITestOutputHelper output)
    {
        _output = output;
    }

    // ── 12.1 corpus discovery ────────────────────────────────────────────────

    [Fact]
    public void Discovery_WalksUpToCorpus_FindsPackagesAndExpectedGoldens()
    {
        var corpusRoot = ResolveCorpusRoot();
        var packageDir = Path.Combine(corpusRoot, "scielo_package");
        var expectedDir = ResolveExpectedDir();

        var packages = DiscoverPackageBasenames(packageDir);
        Assert.NotEmpty(packages);

        foreach (var basename in packages)
        {
            Assert.True(
                File.Exists(Path.Combine(expectedDir, $"{basename}.xml")),
                $"missing golden expected/{basename}.xml for corpus package {basename}");
        }
    }

    [Fact]
    public void Discovery_RepresentsEachCreditShape_RoleKeyed_AuthorKeyed_Prose()
    {
        var expectedDir = ResolveExpectedDir();

        // Author-keyed resolves fully → its golden carries injected CRediT roles.
        var authorKeyed = File.ReadAllText(Path.Combine(expectedDir, $"{AuthorKeyedBasename}.xml"));
        Assert.Contains(CreditRoleSignature, authorKeyed);

        // Role-keyed is present in the corpus with a golden (it prompts on a
        // non-CRediT facet, so accept records a best guess).
        Assert.True(File.Exists(Path.Combine(expectedDir, $"{RoleKeyedBasename}.xml")));

        // Free prose is never auto-mapped (ADR-005): its golden carries the other
        // tags but no CRediT roles.
        var prose = File.ReadAllText(Path.Combine(expectedDir, $"{ProseBasename}.xml"));
        Assert.DoesNotContain(CreditRoleSignature, prose);
        Assert.Contains("pub-id-type=\"other\"", prose);
    }

    // ── 12.2 / 12.3 accept-mode golden diff ──────────────────────────────────

    [Fact]
    public void AcceptMode_EachPackage_ByteEqualToGolden_AndInjectedTagsOnly()
    {
        var corpusRoot = ResolveCorpusRoot();
        var expectedDir = ResolveExpectedDir();
        var packages = DiscoverPackageBasenames(Path.Combine(corpusRoot, "scielo_package"));

        using var stage = new TempDir();
        var stagedPackage = StageFullCorpus(corpusRoot, stage.Path, packages);

        var exit = CliApp.Run(
            new[] { "phase3", stagedPackage, "--non-interactive=accept" },
            new StringWriter(),
            new StringWriter());
        Assert.Equal(CliApp.ExitSuccess, exit);

        var outDir = Path.Combine(stagedPackage, "formatted-phase3");
        var failures = new List<string>();

        foreach (var basename in packages)
        {
            var producedPath = Path.Combine(outDir, $"{basename}.xml");
            var goldenPath = Path.Combine(expectedDir, $"{basename}.xml");
            if (!File.Exists(producedPath))
            {
                failures.Add($"{basename}: produced XML missing at {producedPath}");
                continue;
            }

            // (1) byte-equal to the curated golden.
            var producedBytes = File.ReadAllBytes(producedPath);
            var goldenBytes = File.ReadAllBytes(goldenPath);
            if (!producedBytes.AsSpan().SequenceEqual(goldenBytes))
            {
                failures.Add($"{basename}: produced XML is not byte-equal to golden");
            }

            // (2) diff vs the source XML is limited to the injected tags.
            var sourceXml = File.ReadAllText(Path.Combine(stagedPackage, $"{basename}.xml"));
            var producedXml = File.ReadAllText(producedPath);
            var diff = Phase3DiffUtility.CompareInjectedOnly(sourceXml, producedXml);
            if (!diff.InjectedTagsOnly)
            {
                failures.Add(
                    $"{basename}: diff not limited to injected tags. " +
                    $"removed/modified={diff.RemovedOrModifiedLines.Count}, " +
                    $"unexpected-inserted={diff.UnexpectedInsertedLines.Count}. " +
                    $"first removed/modified='{diff.RemovedOrModifiedLines.FirstOrDefault()}'; " +
                    $"first unexpected-inserted='{diff.UnexpectedInsertedLines.FirstOrDefault()}'");
            }
        }

        if (failures.Count > 0)
        {
            foreach (var f in failures)
            {
                _output.WriteLine(f);
            }
            Assert.Fail($"{failures.Count} of {packages.Count} corpus package(s) failed:\n" +
                string.Join("\n", failures));
        }
    }

    // ── 12.4 strict fail-on-prompt ambiguity assertion ───────────────────────

    [Fact]
    public void FailMode_PromptsExactlyOnTheDocumentedAmbiguousSet()
    {
        var corpusRoot = ResolveCorpusRoot();
        var packages = DiscoverPackageBasenames(Path.Combine(corpusRoot, "scielo_package"));

        using var stage = new TempDir();
        var stagedPackage = StageFullCorpus(corpusRoot, stage.Path, packages);

        // The batch continues past each aborted document, so the overall exit is
        // a non-success abort code while non-ambiguous documents still process.
        var exit = CliApp.Run(
            new[] { "phase3", stagedPackage, "--non-interactive=fail" },
            new StringWriter(),
            new StringWriter());
        Assert.Equal(CliApp.ExitCriticalAbort, exit);

        // A prompting document aborts before its XML is saved (no output file),
        // while a fully-deterministic document produces one. The corpus has no
        // pairing failures, so "no produced XML" == "prompted".
        var outDir = Path.Combine(stagedPackage, "formatted-phase3");
        var prompted = packages
            .Where(b => !File.Exists(Path.Combine(outDir, $"{b}.xml")))
            .ToHashSet(StringComparer.Ordinal);

        var expectedAmbiguous = AmbiguousBasenames
            .Where(packages.Contains)
            .ToHashSet(StringComparer.Ordinal);

        var unexpected = prompted.Except(expectedAmbiguous).OrderBy(x => x).ToList();
        var missing = expectedAmbiguous.Except(prompted).OrderBy(x => x).ToList();
        Assert.True(
            unexpected.Count == 0 && missing.Count == 0,
            $"ambiguous set drifted. newly-prompting (unexpected)=[{string.Join(", ", unexpected)}]; " +
            $"no-longer-prompting (missing)=[{string.Join(", ", missing)}]");

        // The deterministic document must still produce output under the strict policy.
        Assert.Contains(AutoBasename, packages);
        Assert.True(
            File.Exists(Path.Combine(outDir, $"{AutoBasename}.xml")),
            $"deterministic document {AutoBasename} should produce XML under --non-interactive=fail");
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private static IReadOnlyList<string> DiscoverPackageBasenames(string packageDir)
    {
        return Directory.EnumerateFiles(packageDir, "*.xml")
            .Select(Path.GetFileNameWithoutExtension)
            .Where(name => !string.IsNullOrEmpty(name))
            .Select(name => name!)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToList();
    }

    // Copies other.txt + every (non-lock) docx + the named package XMLs into a
    // temp layout root/{other.txt, scielo_markup/, scielo_package/} so the CLI's
    // walk-up layout resolution finds them, leaving the repo corpus untouched.
    private static string StageFullCorpus(string corpusRoot, string stageRoot, IReadOnlyList<string> basenames)
    {
        var stageMarkup = Path.Combine(stageRoot, "scielo_markup");
        var stagePackage = Path.Combine(stageRoot, "scielo_package");
        Directory.CreateDirectory(stageMarkup);
        Directory.CreateDirectory(stagePackage);

        File.Copy(Path.Combine(corpusRoot, "other.txt"), Path.Combine(stageRoot, "other.txt"));

        foreach (var docx in Directory.EnumerateFiles(Path.Combine(corpusRoot, "scielo_markup"), "*.docx"))
        {
            var name = Path.GetFileName(docx);
            if (name.StartsWith("~$", StringComparison.Ordinal))
            {
                continue;
            }
            File.Copy(docx, Path.Combine(stageMarkup, name));
        }

        foreach (var basename in basenames)
        {
            File.Copy(
                Path.Combine(corpusRoot, "scielo_package", $"{basename}.xml"),
                Path.Combine(stagePackage, $"{basename}.xml"));
        }

        return stagePackage;
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

    private static string ResolveExpectedDir()
    {
        var expected = Path.Combine(ResolveCorpusRoot(), "expected");
        if (!Directory.Exists(expected))
        {
            throw new InvalidOperationException(
                $"Could not locate examples/phase-3/expected/ at {expected}.");
        }
        return expected;
    }

    private sealed class TempDir : IDisposable
    {
        public string Path { get; }

        public TempDir()
        {
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                $"phase3-corpus-{Guid.NewGuid():N}");
            Directory.CreateDirectory(Path);
        }

        public void Dispose()
        {
            try { Directory.Delete(Path, recursive: true); } catch { /* best effort */ }
        }
    }
}
