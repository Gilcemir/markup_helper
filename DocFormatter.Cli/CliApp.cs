using System.Reflection;
using DocFormatter.Core.Jats;
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
    internal const int ExitVerifyMismatch = 3;

    internal const string LogFileName = "_app.log";
    internal const string BatchSummaryFileName = "_batch_summary.txt";

    internal const string Phase1OutputSubdir = "formatted";
    internal const string Phase2OutputSubdir = "formatted-phase2";
    internal const string Phase3OutputSubdir = "formatted-phase3";

    internal const string Phase2Subcommand = "phase2";
    internal const string Phase2VerifySubcommand = "phase2-verify";
    internal const string Phase3Subcommand = "phase3";

    internal const string NonInteractiveFlag = "--non-interactive";
    internal const string OtherTableFileName = "other.txt";
    internal const string MarkupSourceDirName = "scielo_markup";

    // How far up from the package directory to search for other.txt (inclusive of
    // the package directory itself). Covers the corpus layout
    // examples/phase-3/{scielo_package,scielo_markup,other.txt} and flat layouts.
    private const int Phase3LayoutSearchDepth = 5;

    public static int Run(string[] args, TextWriter stdout, TextWriter stderr)
        => Run(args, Console.In, stdout, stderr);

    public static int Run(string[] args, TextReader stdin, TextWriter stdout, TextWriter stderr)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(stdin);
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
                case Phase3Subcommand:
                    return RunPhase3(args.AsSpan(1).ToArray(), stdin, stdout, stderr);
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

    internal static int RunPhase3(string[] args, TextReader stdin, TextWriter stdout, TextWriter stderr)
    {
        string? input = null;
        string? nonInteractive = null;

        foreach (var arg in args)
        {
            if (arg.StartsWith(NonInteractiveFlag, StringComparison.Ordinal))
            {
                var eq = arg.IndexOf('=');
                if (eq < 0 || eq == arg.Length - 1)
                {
                    stderr.WriteLine($"error: {NonInteractiveFlag} requires a value: =accept or =fail");
                    return ExitUsageError;
                }

                nonInteractive = arg[(eq + 1)..];
            }
            else if (input is null)
            {
                input = arg;
            }
            else
            {
                stderr.WriteLine("error: phase3 takes a single <input> argument");
                stderr.WriteLine();
                stderr.WriteLine(GetUsage());
                return ExitUsageError;
            }
        }

        if (input is null)
        {
            stderr.WriteLine(GetUsage());
            return ExitUsageError;
        }

        if (!TrySelectConfirmer(nonInteractive, stdin, stdout, out var confirmer, out var selectError))
        {
            stderr.WriteLine($"error: {selectError}");
            return ExitUsageError;
        }

        if (File.Exists(input))
        {
            if (!string.Equals(Path.GetExtension(input), ".xml", StringComparison.OrdinalIgnoreCase))
            {
                stderr.WriteLine($"error: only .xml files are supported, got '{Path.GetExtension(input)}'");
                return ExitUsageError;
            }

            return RunPhase3Single(input, confirmer, stdout, stderr);
        }

        if (Directory.Exists(input))
        {
            return RunPhase3Batch(input, confirmer, stdout, stderr);
        }

        stderr.WriteLine($"path not found: {input}");
        return ExitUsageError;
    }

    // Selects the confirmer policy (ADR-006): absent flag → interactive
    // ConsoleConfirmer; =accept → AutoAcceptConfirmer; =fail → FailOnPromptConfirmer.
    internal static bool TrySelectConfirmer(
        string? nonInteractive,
        TextReader stdin,
        TextWriter stdout,
        out IConfirmer confirmer,
        out string? error)
    {
        switch (nonInteractive)
        {
            case null:
                confirmer = new ConsoleConfirmer(stdin, stdout);
                error = null;
                return true;
            case "accept":
                confirmer = new AutoAcceptConfirmer();
                error = null;
                return true;
            case "fail":
                confirmer = new FailOnPromptConfirmer();
                error = null;
                return true;
            default:
                confirmer = new AutoAcceptConfirmer();
                error = $"{NonInteractiveFlag} must be 'accept' or 'fail', got '{nonInteractive}'";
                return false;
        }
    }

    private static int RunPhase3Single(
        string xmlPath,
        IConfirmer confirmer,
        TextWriter stdout,
        TextWriter stderr)
    {
        var packageDir = Path.GetDirectoryName(Path.GetFullPath(xmlPath))
            ?? Directory.GetCurrentDirectory();

        if (!TryResolvePhase3Layout(packageDir, out var markupDir, out var otherTable, out var layoutError))
        {
            stderr.WriteLine($"error: {layoutError}");
            return ExitUsageError;
        }

        var outDir = Path.Combine(packageDir, Phase3OutputSubdir);
        Directory.CreateDirectory(outDir);

        using var logger = BuildLogger(Path.Combine(outDir, LogFileName));
        using var services = BuildPhase3ServiceProvider();
        var processor = new Phase3Processor(
            services, logger, markupDir, otherTable, confirmer, Phase3OutputSubdir);

        Phase3Outcome outcome;
        try
        {
            outcome = processor.Process(xmlPath);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "unexpected error processing {File}", xmlPath);
            stderr.WriteLine($"error: {ex.Message}");
            return ExitCriticalAbort;
        }

        switch (outcome.Kind)
        {
            case Phase3OutcomeKind.Processed:
                stdout.WriteLine(
                    outcome.Prompted
                        ? $"✓ injected {outcome.FileName} (prompted)"
                        : $"✓ injected {outcome.FileName}");
                return ExitSuccess;
            case Phase3OutcomeKind.Skipped:
                stderr.WriteLine($"⤼ {outcome.FileName} skipped: {outcome.Reason}");
                return ExitCriticalAbort;
            default:
                stderr.WriteLine($"✗ {outcome.FileName}: {outcome.Reason}");
                return ExitCriticalAbort;
        }
    }

    private static int RunPhase3Batch(
        string folderPath,
        IConfirmer confirmer,
        TextWriter stdout,
        TextWriter stderr)
    {
        var packageDir = Path.GetFullPath(folderPath);

        if (!TryResolvePhase3Layout(packageDir, out var markupDir, out var otherTable, out var layoutError))
        {
            stderr.WriteLine($"error: {layoutError}");
            return ExitUsageError;
        }

        var outDir = Path.Combine(packageDir, Phase3OutputSubdir);
        Directory.CreateDirectory(outDir);

        using var logger = BuildLogger(Path.Combine(outDir, LogFileName));
        using var services = BuildPhase3ServiceProvider();
        var processor = new Phase3Processor(
            services, logger, markupDir, otherTable, confirmer, Phase3OutputSubdir);

        var inputs = Directory.EnumerateFiles(packageDir, "*.xml", SearchOption.TopDirectoryOnly)
            .Where(p => !IsTransientArtifact(Path.GetFileName(p)))
            .OrderBy(p => p, StringComparer.Ordinal)
            .ToList();

        if (inputs.Count == 0)
        {
            stdout.WriteLine($"no .xml files found in {folderPath}");
            File.WriteAllText(Path.Combine(outDir, BatchSummaryFileName), string.Empty);
            return ExitSuccess;
        }

        var outcomes = new List<Phase3Outcome>(inputs.Count);
        foreach (var inputFile in inputs)
        {
            try
            {
                outcomes.Add(processor.Process(inputFile));
            }
            catch (Exception ex)
            {
                logger.Error(ex, "unexpected error processing {File}", inputFile);
                outcomes.Add(new Phase3Outcome(
                    Path.GetFileNameWithoutExtension(inputFile),
                    Phase3OutcomeKind.Failed,
                    Prompted: false,
                    ex.Message));
            }
        }

        WritePhase3BatchSummary(Path.Combine(outDir, BatchSummaryFileName), outcomes);

        var processed = outcomes.Count(o => o.Kind == Phase3OutcomeKind.Processed);
        var prompted = outcomes.Count(o => o.Prompted);
        var skipped = outcomes.Count(o => o.Kind == Phase3OutcomeKind.Skipped);
        var failed = outcomes.Count(o => o.Kind == Phase3OutcomeKind.Failed);
        stdout.WriteLine(
            $"phase3 batch complete: processed={processed} prompted={prompted} skipped={skipped} failed={failed} ({outcomes.Count} file(s))");

        // A Critical abort or a fail-on-prompt makes the whole run non-zero; a
        // pairing skip is a per-document outcome and does not by itself fail the batch.
        return failed > 0 ? ExitCriticalAbort : ExitSuccess;
    }

    // Resolves the auxiliary inputs from the package directory: walk up (inclusive)
    // to the first directory holding other.txt — the phase-3 root — and take its
    // scielo_markup subdirectory as the docx source (falling back to the root
    // itself when that subdirectory is absent). No extra CLI flags are introduced.
    internal static bool TryResolvePhase3Layout(
        string packageDir,
        out string markupSourceDir,
        out OtherTable otherTable,
        out string? error)
    {
        var dir = new DirectoryInfo(Path.GetFullPath(packageDir));
        for (var i = 0; i < Phase3LayoutSearchDepth && dir is not null; i++, dir = dir.Parent)
        {
            var otherPath = Path.Combine(dir.FullName, OtherTableFileName);
            if (!File.Exists(otherPath))
            {
                continue;
            }

            try
            {
                otherTable = OtherTable.Load(otherPath);
            }
            catch (InvalidDataException ex)
            {
                markupSourceDir = string.Empty;
                otherTable = null!;
                error = $"failed to load {otherPath}: {ex.Message}";
                return false;
            }

            var markup = Path.Combine(dir.FullName, MarkupSourceDirName);
            markupSourceDir = Directory.Exists(markup) ? markup : dir.FullName;
            error = null;
            return true;
        }

        markupSourceDir = string.Empty;
        otherTable = null!;
        error = $"could not locate '{OtherTableFileName}' searching up from '{packageDir}'";
        return false;
    }

    private static void WritePhase3BatchSummary(string path, IReadOnlyList<Phase3Outcome> outcomes)
    {
        var processed = outcomes.Count(o => o.Kind == Phase3OutcomeKind.Processed);
        var prompted = outcomes.Count(o => o.Prompted);
        var skipped = outcomes.Count(o => o.Kind == Phase3OutcomeKind.Skipped);
        var failed = outcomes.Count(o => o.Kind == Phase3OutcomeKind.Failed);

        var lines = new List<string>(outcomes.Count + 1)
        {
            $"processed={processed} prompted={prompted} skipped={skipped} failed={failed}",
        };

        foreach (var o in outcomes)
        {
            var marker = o.Kind switch
            {
                Phase3OutcomeKind.Processed => o.Prompted ? "✓ prompted" : "✓",
                Phase3OutcomeKind.Skipped => $"⤼ skipped {o.Reason}",
                _ => $"✗ {o.Reason}",
            };
            lines.Add($"{o.FileName}.xml {marker}");
        }

        File.WriteAllLines(path, lines);
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

    internal static ServiceProvider BuildPhase3ServiceProvider()
    {
        var services = new ServiceCollection();

        services.AddTransient<IReport, Report>();

        services.AddPhase3Injectors();

        services.AddTransient<Phase3Pipeline>();

        return services.BuildServiceProvider();
    }

    internal static string GetUsage() =>
        """
        Usage: docformatter <path-to-file.docx>
               docformatter <path-to-folder>
               docformatter phase2 <path-to-file.docx | path-to-folder>
               docformatter phase2-verify <before-dir> <after-dir>
               docformatter phase3 <path-to-file.xml | path-to-folder> [--non-interactive=accept|fail]
               docformatter --help
               docformatter --version

        Single file: writes outputs to <dir>/formatted/<name>.docx and <name>.report.txt.
        Folder:      processes every *.docx (non-recursive), writes outputs to <folder>/formatted/,
                     plus _batch_summary.txt.

        phase2:        runs the Phase 2 pipeline; outputs go to <dir>/formatted-phase2/.
        phase2-verify: runs Phase 2 over each <before-dir>/*.docx and diffs each result against
                       <after-dir>/<same-name>.docx, scoped to Phase2Scope.Current. Prints
                       [PASS] <id> or [FAIL] <id> with first-divergence context.

        phase3:        injects the four JATS tags into each XML; outputs go to <dir>/formatted-phase3/
                       (modified .xml, .report.txt, .diagnostic.json) plus _batch_summary.txt for a folder.
                       Pairs each XML with its docx and other.txt by walking up to the directory holding
                       other.txt (its scielo_markup/ holds the docx). --non-interactive=accept writes
                       best-guess proposals; =fail aborts on any prompt; absent prompts interactively.

        Exit codes:
          0  success (file or batch ran, regardless of warnings; phase2-verify all pass)
          1  usage error or path not found
          2  critical pipeline abort (single-file mode, or phase3 fail-on-prompt / any failed batch doc)
          3  phase2-verify mismatch on any pair
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
