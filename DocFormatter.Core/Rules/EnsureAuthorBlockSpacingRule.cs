using DocFormatter.Core.Pipeline;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace DocFormatter.Core.Rules;

public sealed class EnsureAuthorBlockSpacingRule : IFormattingRule
{
    public const string MissingAuthorBlockEndMessage =
        "ctx.AuthorBlockEndParagraph is null; cannot ensure author/affiliation spacing";

    public const string MissingAffiliationMessage =
        "no affiliation paragraph found after author block; spacing not applied";

    public const string BlankLineInsertedMessage =
        "blank line inserted between authors and affiliations";

    public const string BlankLineAlreadyPresentMessage =
        "blank line already present between authors and affiliations";

    public string Name => nameof(EnsureAuthorBlockSpacingRule);

    public RuleSeverity Severity => RuleSeverity.Optional;

    public void Apply(WordprocessingDocument doc, FormattingContext ctx, IReport report)
    {
        ArgumentNullException.ThrowIfNull(doc);
        ArgumentNullException.ThrowIfNull(ctx);
        ArgumentNullException.ThrowIfNull(report);

        var anchor = ctx.AuthorBlockEndParagraph;
        if (anchor is null)
        {
            report.Warn(Name, MissingAuthorBlockEndMessage);
            return;
        }

        var body = doc.MainDocumentPart?.Document?.Body
            ?? throw new InvalidOperationException("document is missing its body");

        Paragraph? affiliation = null;
        Paragraph previousParagraph = anchor;
        var sibling = anchor.NextSibling<Paragraph>();
        while (sibling is not null)
        {
            if (!IsBlank(sibling))
            {
                affiliation = sibling;
                break;
            }

            previousParagraph = sibling;
            sibling = sibling.NextSibling<Paragraph>();
        }

        if (affiliation is null)
        {
            report.Warn(Name, MissingAffiliationMessage);
            return;
        }

        if (IsBlank(previousParagraph))
        {
            report.Info(Name, BlankLineAlreadyPresentMessage);
            return;
        }

        body.InsertBefore(new Paragraph(), affiliation);
        report.Info(Name, BlankLineInsertedMessage);
    }

    private static bool IsBlank(Paragraph paragraph)
    {
        foreach (var text in paragraph.Descendants<Text>())
        {
            if (!string.IsNullOrWhiteSpace(text.Text))
            {
                return false;
            }
        }

        return true;
    }
}
