using System.Reflection;
using DocFormatter.Core.Options;
using DocFormatter.Core.Pipeline;
using DocFormatter.Core.Rules;
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

    internal const string LogFileName = "_app.log";
    internal const string BatchSummaryFileName = "_batch_summary.txt";

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
    {
        var sourceDir = Path.GetDirectoryName(Path.GetFullPath(filePath))
            ?? Directory.GetCurrentDirectory();
        var formattedDir = Path.Combine(sourceDir, "formatted");
        Directory.CreateDirectory(formattedDir);

        using var logger = BuildLogger(Path.Combine(formattedDir, LogFileName));
        using var services = BuildServiceProvider();
        var processor = new FileProcessor(services, logger);

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
    {
        var formattedDir = Path.Combine(Path.GetFullPath(folderPath), "formatted");
        Directory.CreateDirectory(formattedDir);

        using var logger = BuildLogger(Path.Combine(formattedDir, LogFileName));
        using var services = BuildServiceProvider();
        var processor = new FileProcessor(services, logger);

        var inputs = Directory.EnumerateFiles(folderPath, "*.docx", SearchOption.TopDirectoryOnly)
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

    private static ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddSingleton<FormattingOptions>();

        services.AddTransient<IReport, Report>();

        services.AddTransient<IFormattingRule, ExtractTopTableRule>();
        services.AddTransient<IFormattingRule, ParseHeaderLinesRule>();
        services.AddTransient<IFormattingRule, ExtractOrcidLinksRule>();
        services.AddTransient<IFormattingRule, ParseAuthorsRule>();
        services.AddTransient<IFormattingRule, RewriteHeaderMvpRule>();
        services.AddTransient<IFormattingRule, LocateAbstractAndInsertElocationRule>();

        services.AddTransient<FormattingPipeline>();

        return services.BuildServiceProvider();
    }

    internal static string GetUsage() =>
        """
        Usage: docformatter <path-to-file.docx>
               docformatter <path-to-folder>
               docformatter --help
               docformatter --version

        Single file: writes outputs to <dir>/formatted/<name>.docx and <name>.report.txt.
        Folder:      processes every *.docx (non-recursive), writes outputs to <folder>/formatted/,
                     plus _batch_summary.txt.

        Exit codes:
          0  success (file or batch ran, regardless of warnings)
          1  usage error (missing argument, unknown flag, or path not found)
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
