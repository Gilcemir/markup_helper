using DocFormatter.Core.Options;
using DocFormatter.Core.Pipeline;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace DocFormatter.Core.Rules;

public sealed class LocateAbstractAndInsertElocationRule : IFormattingRule
{
    public const string AbstractNotFoundMessage =
        "Abstract paragraph not found, ELOCATION not inserted";

    public const string MissingElocationIdMessage =
        "ElocationId is null, skipping insertion";

    private readonly FormattingOptions _options;

    public LocateAbstractAndInsertElocationRule(FormattingOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options;
    }

    public string Name => nameof(LocateAbstractAndInsertElocationRule);

    public RuleSeverity Severity => RuleSeverity.Optional;

    public void Apply(WordprocessingDocument doc, FormattingContext ctx, IReport report)
    {
        ArgumentNullException.ThrowIfNull(doc);
        ArgumentNullException.ThrowIfNull(ctx);
        ArgumentNullException.ThrowIfNull(report);

        if (string.IsNullOrEmpty(ctx.ElocationId))
        {
            report.Warn(Name, MissingElocationIdMessage);
            return;
        }

        var body = doc.MainDocumentPart?.Document?.Body;
        if (body is null)
        {
            report.Warn(Name, AbstractNotFoundMessage);
            return;
        }

        var match = FindAbstractParagraph(body);
        if (match is null)
        {
            report.Warn(Name, AbstractNotFoundMessage);
            return;
        }

        var elocationParagraph = new Paragraph(
            new Run(new Text(ctx.ElocationId) { Space = SpaceProcessingModeValues.Preserve }));
        body.InsertBefore(elocationParagraph, match);

        report.Info(Name, $"ELOCATION '{ctx.ElocationId}' inserted above Abstract paragraph");
    }

    private Paragraph? FindAbstractParagraph(Body body)
    {
        foreach (var paragraph in body.Elements<Paragraph>())
        {
            var firstRun = paragraph.Descendants<Run>().FirstOrDefault();
            if (firstRun is null || !IsBold(firstRun))
            {
                continue;
            }

            var text = firstRun.InnerText.TrimStart();
            if (text.Length == 0)
            {
                continue;
            }

            foreach (var marker in _options.AbstractMarkers)
            {
                if (text.StartsWith(marker, StringComparison.OrdinalIgnoreCase))
                {
                    return paragraph;
                }
            }
        }

        return null;
    }

    private static bool IsBold(Run run)
    {
        var bold = run.RunProperties?.Bold;
        if (bold is null)
        {
            return false;
        }

        return bold.Val is null || bold.Val.Value;
    }
}
