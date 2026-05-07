using DocFormatter.Core.Models;
using DocFormatter.Core.Pipeline;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace DocFormatter.Core.Rules;

public sealed class RewriteHeaderMvpRule : IFormattingRule
{
    public const string MissingArticleTitleMessage =
        "ctx.ArticleTitle is null; cannot rewrite header (missing field: ArticleTitle)";

    public const string EmptyAuthorsMessage =
        "ctx.Authors is empty; cannot rewrite header (missing field: Authors)";

    public const string MissingDoiMessage =
        "ctx.Doi is null; skipping DOI line";

    public const string MissingAuthorsParagraphMessage =
        "authors paragraph not found; cannot rewrite header";

    public string Name => nameof(RewriteHeaderMvpRule);

    public RuleSeverity Severity => RuleSeverity.Critical;

    public void Apply(WordprocessingDocument doc, FormattingContext ctx, IReport report)
    {
        ArgumentNullException.ThrowIfNull(doc);
        ArgumentNullException.ThrowIfNull(ctx);
        ArgumentNullException.ThrowIfNull(report);

        if (ctx.ArticleTitle is null)
        {
            throw new InvalidOperationException(MissingArticleTitleMessage);
        }

        var body = doc.MainDocumentPart?.Document?.Body
            ?? throw new InvalidOperationException("document is missing its body");

        var renderableAuthors = ctx.Authors
            .Where(a => !string.IsNullOrWhiteSpace(a.Name))
            .ToList();

        if (ctx.Authors.Count == 0)
        {
            report.Warn(Name, EmptyAuthorsMessage);
        }
        else
        {
            var authorsParagraph = HeaderParagraphLocator.FindAuthorsParagraph(body)
                ?? throw new InvalidOperationException(MissingAuthorsParagraphMessage);

            var newElements = new List<OpenXmlElement> { new Paragraph() };
            foreach (var author in renderableAuthors)
            {
                newElements.Add(BuildAuthorParagraph(author));
            }

            OpenXmlElement insertAfter = authorsParagraph;
            foreach (var element in newElements)
            {
                body.InsertAfter(element, insertAfter);
                insertAfter = element;
            }

            authorsParagraph.Remove();
        }

        if (ctx.Doi is not null)
        {
            var doiParagraph = BuildPlainParagraph(ctx.Doi);
            var firstContent = body.ChildElements.FirstOrDefault(e => e is not SectionProperties);
            if (firstContent is null)
            {
                body.AppendChild(doiParagraph);
            }
            else
            {
                body.InsertBefore(doiParagraph, firstContent);
            }

            report.Info(Name, $"DOI line inserted at top: '{ctx.Doi}'");
        }
        else
        {
            report.Warn(Name, MissingDoiMessage);
        }

        report.Info(
            Name,
            $"rewrote header with {renderableAuthors.Count} author paragraph(s) "
            + $"(skipped {ctx.Authors.Count - renderableAuthors.Count} empty-name record(s))");
    }

    private const string DefaultFont = "Times New Roman";
    private const string DefaultFontSizeHalfPoints = "24";

    internal static RunProperties CreateBaseRunProperties()
        => new(
            new RunFonts
            {
                Ascii = DefaultFont,
                HighAnsi = DefaultFont,
                ComplexScript = DefaultFont,
                EastAsia = DefaultFont,
            },
            new FontSize { Val = DefaultFontSizeHalfPoints },
            new FontSizeComplexScript { Val = DefaultFontSizeHalfPoints });

    private static Paragraph BuildPlainParagraph(string text)
    {
        return new Paragraph(
            new Run(
                CreateBaseRunProperties(),
                new Text(text) { Space = SpaceProcessingModeValues.Preserve }));
    }

    private static Paragraph BuildAuthorParagraph(Author author)
    {
        var paragraph = new Paragraph();

        paragraph.AppendChild(
            new Run(
                CreateBaseRunProperties(),
                new Text(author.Name) { Space = SpaceProcessingModeValues.Preserve }));

        if (author.AffiliationLabels.Count > 0)
        {
            var labelText = string.Join(",", author.AffiliationLabels);
            paragraph.AppendChild(BuildSuperscriptRun(labelText));
        }

        if (!string.IsNullOrEmpty(author.OrcidId))
        {
            paragraph.AppendChild(
                new Run(
                    CreateBaseRunProperties(),
                    new Text(" " + author.OrcidId) { Space = SpaceProcessingModeValues.Preserve }));
        }

        return paragraph;
    }

    private static Run BuildSuperscriptRun(string text)
    {
        var properties = CreateBaseRunProperties();
        properties.AppendChild(new VerticalTextAlignment { Val = VerticalPositionValues.Superscript });
        return new Run(properties, new Text(text) { Space = SpaceProcessingModeValues.Preserve });
    }
}
