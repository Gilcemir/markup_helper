using DocumentFormat.OpenXml.Packaging;

namespace DocFormatter.Core.Pipeline;

public sealed class FormattingPipeline
{
    private readonly IFormattingRule[] _rules;

    public FormattingPipeline(IEnumerable<IFormattingRule> rules)
    {
        _rules = rules.ToArray();
    }

    public void Run(WordprocessingDocument doc, FormattingContext ctx, IReport report)
    {
        foreach (var rule in _rules)
        {
            try
            {
                rule.Apply(doc, ctx, report);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                report.Error(rule.Name, ex.Message);
                if (rule.Severity == RuleSeverity.Critical)
                {
                    throw;
                }
            }
        }
    }
}
