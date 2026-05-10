using System.Reflection;
using DocFormatter.Core.Options;
using DocFormatter.Core.Pipeline;
using DocFormatter.Core.Reporting;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace DocFormatter.Cli;

internal static class CliApp
{
    internal const int ExitSuccess = 0;
    internal const int ExitUsageError = 1;
    internal const int ExitCriticalAbort = 2;
    internal const int ExitVerifyMismatch = 1;

    internal const string LogFileName = "_app.log";
    internal const string BatchSummaryFileName = "_batch_summary.txt";

    internal const string Phase1OutputSubdir = "formatted";
    internal const string Phase2OutputSubdir = "formatted-phase2";

    internal const string Phase2Subcommand = "phase2";
    internal const string Phase2VerifySubcommand = "phase2-verify";

    public static int Run(string[] args, TextWriter stdout, TextWriter stderr)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(stdout);
        ArgumentNullException.ThrowIfNull(stderr);

        if (args.Length == 0)
        {
            stderr.WriteLine(GetUsage());
            return ExitUsageError;
        }

        var first = args[0];

        if (first is "-h" or "--help")
        {
            stdout.WriteLine(GetUsage());
            return ExitSuccess;
        }

        if (first == "--version")
        {
            stdout.WriteLine(GetVersion());
            return ExitSuccess;
        }

        // Subcommand dispatch (ADR-005). Disambiguation rule: a token that
        // names an existing file or directory is treated as Phase 1 input even
        // when it textually equals a subcommand name. Otherwise, recognized
        // subcommand tokens route to their handlers.
        if (!File.Exists(first) && !Directory.Exists(first))
        {
            switch (first)
            {
                case Phase2Subcommand:
                    return RunPhase2(args.AsSpan(1).ToArray(), stdout, stderr);
                case Phase2VerifySubcommand:
                    return RunPhase2Verify(args.AsSpan(1).ToArray(), stdout, stderr);
            }
        }

        if (args.Length > 1)
        {
            stderr.WriteLine("error: only one positional argument is supported");
            stderr.WriteLine();
            stderr.WriteLine(GetUsage());
            return ExitUsageError;
        }

        var path = first;

        if (File.Exists(path))
        {
            if (!string.Equals(Path.GetExtension(path), ".docx", StringComparison.OrdinalIgnoreCase))
            {
                stderr.WriteLine($"error: only .docx files are supported, got '{Path.GetExtension(path)}'");
                return ExitUsageError;
            }

            return RunSingleFile(path, stdout, stderr);
        }

        if (Directory.Exists(path))
        {
            return RunBatch(path, stdout, stderr);
        }

        stderr.WriteLine($"path not found: {path}");
        return ExitUsageError;
    }

    private static int RunSingleFile(string filePath, TextWriter stdout, TextWriter stderr)
        => RunSingleFile(filePath, Phase1OutputSubdir, BuildPhase1ServiceProvider, stdout, stderr);

    private static int RunSingleFile(
        string filePath,
        string outputSubdir,
        Func<ServiceProvider> buildServices,
        TextWriter stdout,
        TextWriter stderr)
    {
        var sourceDir = Path.GetDirectoryName(Path.GetFullPath(filePath))
            ?? Directory.GetCurrentDirectory();
        var formattedDir = Path.Combine(sourceDir, outputSubdir);
        Directory.CreateDirectory(formattedDir);

        using var logger = BuildLogger(Path.Combine(formattedDir, LogFileName));
        using var services = buildServices();
        var processor = new FileProcessor(services, logger, outputSubdir);

        ProcessOutcome outcome;
        try
        {
            outcome = processor.Process(filePath);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "unexpected error processing {File}", filePath);
            stderr.WriteLine($"error: {ex.Message}");
            return ExitCriticalAbort;
        }

        switch (outcome.Kind)
        {
            case ProcessOutcomeKind.Success:
                stdout.WriteLine($"✓ formatted {outcome.FileName}");
                return ExitSuccess;
            case ProcessOutcomeKind.Warning:
                stdout.WriteLine($"⚠ formatted {outcome.FileName} ({outcome.WarnCount} warning(s))");
                return ExitSuccess;
            case ProcessOutcomeKind.CriticalAbort:
                stderr.WriteLine($"✗ {outcome.FileName}: {outcome.CriticalReason}");
                return ExitCriticalAbort;
            default:
                return ExitCriticalAbort;
        }
    }

    private static int RunBatch(string folderPath, TextWriter stdout, TextWriter stderr)
        => RunBatch(folderPath, Phase1OutputSubdir, BuildPhase1ServiceProvider, stdout, stderr);

    private static int RunBatch(
        string folderPath,
        string outputSubdir,
        Func<ServiceProvider> buildServices,
        TextWriter stdout,
        TextWriter stderr)
    {
        var formattedDir = Path.Combine(Path.GetFullPath(folderPath), outputSubdir);
        Directory.CreateDirectory(formattedDir);

        using var logger = BuildLogger(Path.Combine(formattedDir, LogFileName));
        using var services = buildServices();
        var processor = new FileProcessor(services, logger, outputSubdir);

        var inputs = Directory.EnumerateFiles(folderPath, "*.docx", SearchOption.TopDirectoryOnly)
            .Where(p => !IsTransientArtifact(Path.GetFileName(p)))
            .Where(p => !string.Equals(
                Path.GetFullPath(Path.GetDirectoryName(p) ?? string.Empty),
                formattedDir,
                StringComparison.OrdinalIgnoreCase))
            .OrderBy(p => p, StringComparer.Ordinal)
            .ToList();

        if (inputs.Count == 0)
        {
            stdout.WriteLine($"no .docx files found in {folderPath}");
            File.WriteAllText(Path.Combine(formattedDir, BatchSummaryFileName), string.Empty);
            return ExitSuccess;
        }

        var outcomes = new List<ProcessOutcome>(inputs.Count);
        foreach (var input in inputs)
        {
            try
            {
                outcomes.Add(processor.Process(input));
            }
            catch (Exception ex)
            {
                logger.Error(ex, "unexpected error processing {File}", input);
                outcomes.Add(new ProcessOutcome(
                    Path.GetFileNameWithoutExtension(input),
                    ProcessOutcomeKind.CriticalAbort,
                    0,
                    ex.Message));
            }
        }

        WriteBatchSummary(Path.Combine(formattedDir, BatchSummaryFileName), outcomes);

        var success = outcomes.Count(o => o.Kind == ProcessOutcomeKind.Success);
        var warned = outcomes.Count(o => o.Kind == ProcessOutcomeKind.Warning);
        var failed = outcomes.Count(o => o.Kind == ProcessOutcomeKind.CriticalAbort);
        stdout.WriteLine(
            $"batch complete: {success} ✓ / {warned} ⚠ / {failed} ✗ ({outcomes.Count} file(s))");

        return ExitSuccess;
    }

    internal static int RunPhase2(string[] args, TextWriter stdout, TextWriter stderr)
    {
        if (args.Length == 0)
        {
            stderr.WriteLine(GetUsage());
            return ExitUsageError;
        }

        if (args.Length > 1)
        {
            stderr.WriteLine("error: phase2 takes a single <input> argument");
            stderr.WriteLine();
            stderr.WriteLine(GetUsage());
            return ExitUsageError;
        }

        var input = args[0];

        if (File.Exists(input))
        {
            if (!string.Equals(Path.GetExtension(input), ".docx", StringComparison.OrdinalIgnoreCase))
            {
                stderr.WriteLine($"error: only .docx files are supported, got '{Path.GetExtension(input)}'");
                return ExitUsageError;
            }

            return RunSingleFile(input, Phase2OutputSubdir, BuildPhase2ServiceProvider, stdout, stderr);
        }

        if (Directory.Exists(input))
        {
            return RunBatch(input, Phase2OutputSubdir, BuildPhase2ServiceProvider, stdout, stderr);
        }

        stderr.WriteLine($"path not found: {input}");
        return ExitUsageError;
    }

    internal static int RunPhase2Verify(string[] args, TextWriter stdout, TextWriter stderr)
    {
        if (args.Length != 2)
        {
            stderr.WriteLine("error: phase2-verify takes <before-dir> and <after-dir>");
            stderr.WriteLine();
            stderr.WriteLine(GetUsage());
            return ExitUsageError;
        }

        var beforeDir = args[0];
        var afterDir = args[1];

        if (!Directory.Exists(beforeDir))
        {
            stderr.WriteLine($"error: before directory not found: {beforeDir}");
            return ExitUsageError;
        }

        if (!Directory.Exists(afterDir))
        {
            stderr.WriteLine($"error: after directory not found: {afterDir}");
            return ExitUsageError;
        }

        var inputs = Directory.EnumerateFiles(beforeDir, "*.docx", SearchOption.TopDirectoryOnly)
            .Where(p => !IsTransientArtifact(Path.GetFileName(p)))
            .OrderBy(p => p, StringComparer.Ordinal)
            .ToList();

        if (inputs.Count == 0)
        {
            stdout.WriteLine($"no .docx files found in {beforeDir}");
            return ExitSuccess;
        }

        var tempRoot = Path.Combine(
            Path.GetTempPath(),
            $"docfmt-phase2-verify-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempRoot);

        using var nullLogger = new LoggerConfiguration()
            .MinimumLevel.Fatal()
            .CreateLogger();
        using var services = BuildPhase2ServiceProvider();
        var processor = new FileProcessor(services, nullLogger, "tmp-phase2-verify");

        var anyFail = false;
        try
        {
            foreach (var beforeFile in inputs)
            {
                var name = Path.GetFileName(beforeFile);
                var id = Path.GetFileNameWithoutExtension(beforeFile);
                var afterFile = Path.Combine(afterDir, name);

                if (!File.Exists(afterFile))
                {
                    stdout.WriteLine($"[FAIL] {id}");
                    stdout.WriteLine($"   missing counterpart in after dir: {afterFile}");
                    anyFail = true;
                    continue;
                }

                // Run the Phase 2 pipeline over a copy of the before file and
                // diff the produced .docx against the after side.
                var perFileTempDir = Path.Combine(tempRoot, id);
                Directory.CreateDirectory(perFileTempDir);
                var stagedInput = Path.Combine(perFileTempDir, name);
                File.Copy(beforeFile, stagedInput, overwrite: true);

                ProcessOutcome outcome;
                try
                {
                    outcome = processor.Process(stagedInput);
                }
                catch (Exception ex)
                {
                    stdout.WriteLine($"[FAIL] {id}");
                    stdout.WriteLine($"   pipeline error: {ex.Message}");
                    anyFail = true;
                    continue;
                }

                if (outcome.Kind == ProcessOutcomeKind.CriticalAbort)
                {
                    stdout.WriteLine($"[FAIL] {id}");
                    stdout.WriteLine($"   pipeline aborted: {outcome.CriticalReason}");
                    anyFail = true;
                    continue;
                }

                var producedFile = Path.Combine(
                    perFileTempDir,
                    "tmp-phase2-verify",
                    name);

                var diff = Phase2DiffUtility.Compare(producedFile, afterFile, Phase2Scope.Current);
                if (diff.IsMatch)
                {
                    stdout.WriteLine($"[PASS] {id}");
                }
                else
                {
                    stdout.WriteLine($"[FAIL] {id}");
                    stdout.WriteLine($"   diverge at offset {diff.FirstDivergenceOffset}");
                    stdout.WriteLine($"   produced: {diff.ProducedContext}");
                    stdout.WriteLine($"      after: {diff.ExpectedContext}");
                    anyFail = true;
                }
            }
        }
        finally
        {
            try { Directory.Delete(tempRoot, recursive: true); } catch { /* best effort */ }
        }

        return anyFail ? ExitVerifyMismatch : ExitSuccess;
    }

    internal static bool IsTransientArtifact(string fileName)
        => fileName.StartsWith("~$", StringComparison.Ordinal)
        || fileName.StartsWith("._", StringComparison.Ordinal);

    private static void WriteBatchSummary(string path, IReadOnlyList<ProcessOutcome> outcomes)
    {
        var lines = outcomes.Select(o => o.Kind switch
        {
            ProcessOutcomeKind.Success => $"{o.FileName}.docx ✓",
            ProcessOutcomeKind.Warning => $"{o.FileName}.docx ⚠ {o.WarnCount}",
            ProcessOutcomeKind.CriticalAbort => $"{o.FileName}.docx ✗ {o.CriticalReason}",
            _ => $"{o.FileName}.docx ?",
        });
        File.WriteAllLines(path, lines);
    }

    private static Logger BuildLogger(string logFilePath)
    {
        return new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console(
                outputTemplate: "[{Level:u3}] {Message:lj}{NewLine}{Exception}",
                restrictedToMinimumLevel: LogEventLevel.Information)
            .WriteTo.File(
                logFilePath,
                outputTemplate: "{Timestamp:yyyy-MM-ddTHH:mm:ss.fffZ} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
    }

    internal static ServiceProvider BuildServiceProvider() => BuildPhase1ServiceProvider();

    internal static ServiceProvider BuildPhase1ServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddSingleton<FormattingOptions>();

        services.AddTransient<IReport, Report>();

        services.AddPhase1Rules();

        services.AddTransient<FormattingPipeline>();

        return services.BuildServiceProvider();
    }

    internal static ServiceProvider BuildPhase2ServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddSingleton<FormattingOptions>();

        services.AddTransient<IReport, Report>();

        services.AddPhase2Rules();

        services.AddTransient<FormattingPipeline>();

        return services.BuildServiceProvider();
    }

    internal static string GetUsage() =>
        """
        Usage: docformatter <path-to-file.docx>
               docformatter <path-to-folder>
               docformatter phase2 <path-to-file.docx | path-to-folder>
               docformatter phase2-verify <before-dir> <after-dir>
               docformatter --help
               docformatter --version

        Single file: writes outputs to <dir>/formatted/<name>.docx and <name>.report.txt.
        Folder:      processes every *.docx (non-recursive), writes outputs to <folder>/formatted/,
                     plus _batch_summary.txt.

        phase2:        runs the Phase 2 pipeline; outputs go to <dir>/formatted-phase2/.
        phase2-verify: runs Phase 2 over each <before-dir>/*.docx and diffs each result against
                       <after-dir>/<same-name>.docx, scoped to Phase2Scope.Current. Prints
                       [PASS] <id> or [FAIL] <id> with first-divergence context.

        Exit codes:
          0  success (file or batch ran, regardless of warnings; phase2-verify all pass)
          1  usage error, path not found, or phase2-verify mismatch on any pair
          2  critical pipeline abort (single-file mode only)
        """;

    internal static string GetVersion()
    {
        var assembly = typeof(CliApp).Assembly;
        var info = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        return info?.InformationalVersion
            ?? assembly.GetName().Version?.ToString()
            ?? "0.0.0";
    }
}
