using DocFormatter.Core.Pipeline;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace DocFormatter.Core.Rules;

public sealed class ParseHeaderLinesRule : IFormattingRule
{
    public const string MissingSectionMessage =
        "expected section title paragraph after the top table, but none was found";

    public const string MissingTitleMessage =
        "expected article title paragraph after the section title, but none was found";

    public string Name => nameof(ParseHeaderLinesRule);

    public RuleSeverity Severity => RuleSeverity.Critical;

    public void Apply(WordprocessingDocument doc, FormattingContext ctx, IReport report)
    {
        ArgumentNullException.ThrowIfNull(doc);
        ArgumentNullException.ThrowIfNull(ctx);
        ArgumentNullException.ThrowIfNull(report);

        var body = doc.MainDocumentPart?.Document?.Body
            ?? throw new InvalidOperationException(MissingSectionMessage);

        string? section = null;
        string? title = null;

        foreach (var paragraph in body.Elements<Paragraph>())
        {
            var text = GetParagraphPlainText(paragraph);
            if (string.IsNullOrWhiteSpace(text))
            {
                continue;
            }

            if (section is null)
            {
                section = text;
                continue;
            }

            title = text;
            break;
        }

        if (section is null)
        {
            throw new InvalidOperationException(MissingSectionMessage);
        }

        if (title is null)
        {
            throw new InvalidOperationException(MissingTitleMessage);
        }

        ctx.ArticleTitle = title;
        report.Info(Name, $"section='{section}', articleTitle='{title}'");
    }

    private static string GetParagraphPlainText(Paragraph paragraph)
        => string.Concat(paragraph.Descendants<Text>().Select(t => t.Text));
}
