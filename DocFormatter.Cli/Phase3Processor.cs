using DocFormatter.Core.Jats;
using DocFormatter.Core.Pipeline;
using DocFormatter.Core.Reporting;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace DocFormatter.Cli;

internal enum Phase3OutcomeKind
{
    /// <summary>Paired, the pipeline ran to completion, and the XML was saved.</summary>
    Processed,

    /// <summary>Pairing failed (ADR-004); the document is skipped, the batch continues.</summary>
    Skipped,

    /// <summary>The pipeline aborted — a Critical injector threw or a prompt hit <c>--non-interactive=fail</c>.</summary>
    Failed,
}

internal sealed record Phase3Outcome(
    string FileName,
    Phase3OutcomeKind Kind,
    bool Prompted,
    string? Reason);

/// <summary>
/// Records every <see cref="ConfirmDisposition"/> taken through an inner
/// <see cref="IConfirmer"/>, keyed by the proposal's tag, and tracks whether the
/// gate was reached at all. A fresh instance is used per document so the captured
/// dispositions feed that document's diagnostic. <see cref="Prompted"/> is set
/// before delegating so a throwing policy (<see cref="FailOnPromptConfirmer"/>)
/// still counts as a prompt for the batch summary.
/// </summary>
internal sealed class RecordingConfirmer : IConfirmer
{
    private readonly IConfirmer _inner;
    private readonly Dictionary<string, ConfirmDisposition> _dispositions = new(StringComparer.Ordinal);

    public RecordingConfirmer(IConfirmer inner)
    {
        ArgumentNullException.ThrowIfNull(inner);
        _inner = inner;
    }

    public bool Prompted { get; private set; }

    public IReadOnlyDictionary<string, ConfirmDisposition> Dispositions => _dispositions;

    public ConfirmResult Confirm(Proposal proposal)
    {
        ArgumentNullException.ThrowIfNull(proposal);
        Prompted = true;
        var result = _inner.Confirm(proposal);
        _dispositions[proposal.Tag] = result.Disposition;
        return result;
    }
}

/// <summary>
/// Orchestrates the per-document Phase 3 flow, mirroring <see cref="FileProcessor"/>:
/// pair the XML with its docx and <c>other.txt</c> entry (ADR-004), load the XML
/// with whitespace preserved, run the <see cref="Phase3Pipeline"/> over a
/// <see cref="Phase3Context"/>, save the mutated XML, and write the
/// <c>.report.txt</c> and <c>.diagnostic.json</c> sidecars. The source docx and
/// XML are never modified — the output XML is written to the phase-3 output
/// directory.
/// </summary>
internal sealed class Phase3Processor
{
    private readonly IServiceProvider _services;
    private readonly ILogger _logger;
    private readonly string _markupSourceDir;
    private readonly OtherTable _otherTable;
    private readonly IConfirmer _confirmer;
    private readonly string _outputSubdirName;

    public Phase3Processor(
        IServiceProvider services,
        ILogger logger,
        string markupSourceDir,
        OtherTable otherTable,
        IConfirmer confirmer,
        string outputSubdirName)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentException.ThrowIfNullOrEmpty(markupSourceDir);
        ArgumentNullException.ThrowIfNull(otherTable);
        ArgumentNullException.ThrowIfNull(confirmer);
        ArgumentException.ThrowIfNullOrEmpty(outputSubdirName);
        _services = services;
        _logger = logger;
        _markupSourceDir = markupSourceDir;
        _otherTable = otherTable;
        _confirmer = confirmer;
        _outputSubdirName = outputSubdirName;
    }

    public Phase3Outcome Process(string xmlPath)
    {
        ArgumentException.ThrowIfNullOrEmpty(xmlPath);

        var sourceDir = Path.GetDirectoryName(xmlPath);
        if (string.IsNullOrEmpty(sourceDir))
        {
            sourceDir = Directory.GetCurrentDirectory();
        }

        var name = Path.GetFileNameWithoutExtension(xmlPath);
        var sourceFileName = Path.GetFileName(xmlPath);
        var outDir = Path.Combine(sourceDir, _outputSubdirName);
        Directory.CreateDirectory(outDir);

        var outXmlPath = Path.Combine(outDir, $"{name}.xml");
        var reportPath = Path.Combine(outDir, $"{name}.report.txt");
        var diagnosticPath = Path.Combine(outDir, $"{name}.diagnostic.json");

        // Pair first (ADR-004). A pairing failure is a reported, non-silent reason
        // to skip this document without aborting the batch.
        var pairing = DocumentPairer.Pair(xmlPath, _markupSourceDir, _otherTable);
        if (!pairing.IsPaired)
        {
            var skipReport = new Report();
            skipReport.Error("pairing", pairing.Error!);
            ReportWriter.Write(reportPath, skipReport);
            _logger.Warning("⤼ {File} skipped: {Reason}", name, pairing.Error);
            return new Phase3Outcome(name, Phase3OutcomeKind.Skipped, Prompted: false, pairing.Error);
        }

        var pair = pairing.Pair!;
        var recording = new RecordingConfirmer(_confirmer);

        using var scope = _services.CreateScope();
        var pipeline = scope.ServiceProvider.GetRequiredService<Phase3Pipeline>();
        var report = scope.ServiceProvider.GetRequiredService<IReport>();

        var jdoc = JatsXmlWriter.Load(pair.XmlPath);
        var ctx = new Phase3Context
        {
            Source = pair.Source,
            Xml = jdoc.Document,
            OtherNumber = pair.OtherNumber,
            Confirm = recording,
        };

        string? failReason = null;
        try
        {
            pipeline.Run(ctx, report);
        }
        catch (OperationCanceledException ex)
        {
            // A prompt under --non-interactive=fail (PromptNotAllowedException) or a
            // cancellation: record it so the report and diagnostic capture the reason.
            report.Error("phase3", ex.Message);
            failReason = ex.Message;
            _logger.Error(ex, "phase3 aborted on prompt for {File}", name);
        }
        catch (Exception ex)
        {
            // A Critical injector rethrew (the pipeline already reported the error).
            failReason = ex.Message;
            _logger.Error(ex, "phase3 critical abort for {File}", name);
        }

        if (failReason is null)
        {
            jdoc.Save(outXmlPath);
        }

        ReportWriter.Write(reportPath, report);
        DiagnosticWriter.WritePhase3(diagnosticPath, sourceFileName, jdoc.Document, report, recording.Dispositions);

        if (failReason is not null)
        {
            return new Phase3Outcome(name, Phase3OutcomeKind.Failed, recording.Prompted, failReason);
        }

        if (recording.Prompted)
        {
            _logger.Information("✓ {File} (prompted)", name);
        }
        else
        {
            _logger.Information("✓ {File}", name);
        }

        return new Phase3Outcome(name, Phase3OutcomeKind.Processed, recording.Prompted, null);
    }
}
