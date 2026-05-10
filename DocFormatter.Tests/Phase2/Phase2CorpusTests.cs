using DocFormatter.Cli;
using DocFormatter.Core.Reporting;
using Xunit;
using Xunit.Abstractions;

namespace DocFormatter.Tests.Phase2;

/// <summary>
/// The Phase 2 release gate (ADR-003 / ADR-006). Runs the Phase 2 pipeline
/// over each <c>examples/phase-2/before/&lt;id&gt;.docx</c>, compares the
/// produced output against <c>examples/phase-2/after/&lt;id&gt;.docx</c>
/// using <see cref="Phase2DiffUtility.Compare"/> with
/// <see cref="Phase2Scope.Current"/>, and asserts every pair matches. A
/// failure here means the cumulative scope and the rule outputs diverge for
/// at least one corpus article.
/// </summary>
public sealed class Phase2CorpusTests
{
    private static readonly string[] CorpusIds =
    {
        "5136", "5293", "5313", "5419", "5424",
        "5434", "5449", "5458", "5523", "5549",
    };

    private readonly ITestOutputHelper _output;

    public Phase2CorpusTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void AllPairsMatch()
    {
        var beforeDir = ResolveDir("before");
        var afterDir = ResolveDir("after");

        var failures = new List<string>();
        foreach (var id in CorpusIds)
        {
            var before = Path.Combine(beforeDir, $"{id}.docx");
            var after = Path.Combine(afterDir, $"{id}.docx");
            Assert.True(File.Exists(before), $"missing before/{id}.docx");
            Assert.True(File.Exists(after), $"missing after/{id}.docx");

            using var tempBaseDir = new TempDir();
            var stagedInput = Path.Combine(tempBaseDir.Path, $"{id}.docx");
            File.Copy(before, stagedInput);

            var exit = CliApp.Run(
                new[] { "phase2", stagedInput },
                new StringWriter(),
                new StringWriter());
            if (exit != CliApp.ExitSuccess)
            {
                failures.Add($"{id}: phase2 pipeline exit code {exit}");
                continue;
            }

            var produced = Path.Combine(tempBaseDir.Path, "formatted-phase2", $"{id}.docx");
            if (!File.Exists(produced))
            {
                failures.Add($"{id}: produced file missing at {produced}");
                continue;
            }

            var result = Phase2DiffUtility.Compare(produced, after, Phase2Scope.Current);
            if (!result.IsMatch)
            {
                failures.Add(
                    $"{id}: diverge at offset {result.FirstDivergenceOffset}. " +
                    $"produced='{result.ProducedContext}'; expected='{result.ExpectedContext}'");
            }
        }

        if (failures.Count > 0)
        {
            foreach (var f in failures)
            {
                _output.WriteLine(f);
            }
            Assert.Fail($"{failures.Count} of {CorpusIds.Length} corpus pair(s) failed:\n" +
                string.Join("\n", failures));
        }
    }

    private static string ResolveDir(string side)
    {
        var dir = AppContext.BaseDirectory;
        while (dir != null)
        {
            var candidate = Path.Combine(dir, "examples", "phase-2", side);
            if (Directory.Exists(candidate))
            {
                return candidate;
            }
            dir = Path.GetDirectoryName(dir);
        }
        throw new InvalidOperationException(
            $"Could not locate examples/phase-2/{side}/ from {AppContext.BaseDirectory}.");
    }

    private sealed class TempDir : IDisposable
    {
        public string Path { get; }

        public TempDir()
        {
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                $"phase2-corpus-{Guid.NewGuid():N}");
            Directory.CreateDirectory(Path);
        }

        public void Dispose()
        {
            try { Directory.Delete(Path, recursive: true); } catch { /* best effort */ }
        }
    }
}
