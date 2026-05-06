using DocFormatter.Core.Pipeline;
using DocFormatter.Core.Reporting;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace DocFormatter.Cli;

internal enum ProcessOutcomeKind
{
    Success,
    Warning,
    CriticalAbort,
}

internal sealed record ProcessOutcome(
    string FileName,
    ProcessOutcomeKind Kind,
    int WarnCount,
    string? CriticalReason);

internal sealed class FileProcessor
{
    private readonly IServiceProvider _services;
    private readonly ILogger _logger;

    public FileProcessor(IServiceProvider services, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(logger);
        _services = services;
        _logger = logger;
    }

    public ProcessOutcome Process(string sourceFile)
    {
        ArgumentException.ThrowIfNullOrEmpty(sourceFile);

        var sourceDir = Path.GetDirectoryName(sourceFile);
        if (string.IsNullOrEmpty(sourceDir))
        {
            sourceDir = Directory.GetCurrentDirectory();
        }

        var name = Path.GetFileNameWithoutExtension(sourceFile);
        var formattedDir = Path.Combine(sourceDir, "formatted");
        Directory.CreateDirectory(formattedDir);

        var copyPath = Path.Combine(formattedDir, $"{name}.docx");
        var reportPath = Path.Combine(formattedDir, $"{name}.report.txt");
        var diagnosticPath = Path.Combine(formattedDir, $"{name}.diagnostic.json");
        var sourceFileName = Path.GetFileName(sourceFile);

        File.Copy(sourceFile, copyPath, overwrite: true);

        using var scope = _services.CreateScope();
        var pipeline = scope.ServiceProvider.GetRequiredService<FormattingPipeline>();
        var report = scope.ServiceProvider.GetRequiredService<IReport>();
        var ctx = new FormattingContext();

        var aborted = false;
        try
        {
            using var doc = WordprocessingDocument.Open(copyPath, isEditable: true);
            pipeline.Run(doc, ctx, report);
        }
        catch (Exception ex)
        {
            aborted = true;
            _logger.Error(ex, "pipeline aborted for {File}", name);
        }

        ReportWriter.Write(reportPath, report);
        DiagnosticWriter.Write(diagnosticPath, sourceFileName, ctx, report);

        if (aborted)
        {
            TryDelete(copyPath);
            var reason = report.Entries
                .LastOrDefault(e => e.Level == ReportLevel.Error)?.Message
                ?? "critical pipeline abort";
            _logger.Error("✗ {File}: {Reason}", name, reason);
            return new ProcessOutcome(name, ProcessOutcomeKind.CriticalAbort, 0, reason);
        }

        var warnCount = report.Entries.Count(e => e.Level == ReportLevel.Warn);
        if (warnCount > 0)
        {
            _logger.Warning("⚠ {File}: {Count} warning(s)", name, warnCount);
            return new ProcessOutcome(name, ProcessOutcomeKind.Warning, warnCount, null);
        }

        _logger.Information("✓ {File}", name);
        return new ProcessOutcome(name, ProcessOutcomeKind.Success, 0, null);
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}
