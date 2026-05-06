using DocumentFormat.OpenXml.Packaging;

namespace DocFormatter.Core.Pipeline;

public interface IFormattingRule
{
    string Name { get; }

    RuleSeverity Severity { get; }

    void Apply(WordprocessingDocument doc, FormattingContext ctx, IReport report);
}
