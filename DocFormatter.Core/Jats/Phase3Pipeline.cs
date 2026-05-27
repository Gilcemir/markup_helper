using DocFormatter.Core.Pipeline;

namespace DocFormatter.Core.Jats;

/// <summary>
/// Runs the registered <see cref="IJatsInjector"/>s in order over a
/// <see cref="Phase3Context"/>, mirroring <see cref="FormattingPipeline"/>'s
/// error model (ADR-003): an <see cref="RuleSeverity.Optional"/> injector that
/// throws is logged and the run continues; a <see cref="RuleSeverity.Critical"/>
/// one logs and rethrows to abort the document; an
/// <see cref="OperationCanceledException"/> always rethrows immediately.
/// </summary>
public sealed class Phase3Pipeline
{
    private readonly IJatsInjector[] _injectors;

    public Phase3Pipeline(IEnumerable<IJatsInjector> injectors)
    {
        _injectors = injectors.ToArray();
    }

    public void Run(Phase3Context ctx, IReport report)
    {
        foreach (var injector in _injectors)
        {
            try
            {
                injector.Apply(ctx, report);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                report.Error(injector.Name, ex.Message);
                if (injector.Severity == RuleSeverity.Critical)
                {
                    throw;
                }
            }
        }
    }
}
