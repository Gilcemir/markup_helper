using DocFormatter.Core.Pipeline;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace DocFormatter.Core.Rules;

public sealed class ApplyHeaderAlignmentRule : IFormattingRule
{
    public const string MissingDoiParagraphMessage =
        "DOI paragraph not found in context";

    public const string MissingSectionParagraphMessage =
        "section paragraph not found in context";

    public const string MissingTitleParagraphMessage =
        "title paragraph not found in context";

    public string Name => nameof(ApplyHeaderAlignmentRule);

    public RuleSeverity Severity => RuleSeverity.Optional;

    public void Apply(WordprocessingDocument doc, FormattingContext ctx, IReport report)
    {
        ArgumentNullException.ThrowIfNull(doc);
        ArgumentNullException.ThrowIfNull(ctx);
        ArgumentNullException.ThrowIfNull(report);

        var doiAligned = TryAlign(ctx.DoiParagraph, JustificationValues.Right, report, MissingDoiParagraphMessage);
        var sectionAligned = TryAlign(ctx.SectionParagraph, JustificationValues.Right, report, MissingSectionParagraphMessage);
        var titleAligned = TryAlign(ctx.TitleParagraph, JustificationValues.Center, report, MissingTitleParagraphMessage);

        report.Info(
            Name,
            $"alignment applied (doi={Format(doiAligned)}, section={Format(sectionAligned)}, title={Format(titleAligned)})");
    }

    private bool TryAlign(Paragraph? paragraph, JustificationValues value, IReport report, string missingMessage)
    {
        if (paragraph is null)
        {
            report.Warn(Name, missingMessage);
            return false;
        }

        var properties = paragraph.ParagraphProperties ??= new ParagraphProperties();
        properties.GetFirstChild<Justification>()?.Remove();
        properties.AppendChild(new Justification { Val = value });
        return true;
    }

    private static string Format(bool value) => value ? "true" : "false";
}
