using DocFormatter.Core.Pipeline;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace DocFormatter.Core.Rules;

public sealed class PromoteSectionsRule : IFormattingRule
{
    public const string AnchorMissingMessage =
        "INTRODUCTION anchor not found — section formatting skipped";

    public const string AnchorPositionMessagePrefix = "INTRODUCTION anchor at body position ";

    public const string SummaryPromotedPrefix = "promoted ";

    public const string SummarySectionsInfix = " sections (16pt center) and ";

    public const string SummarySubsectionsSuffix = " sub-sections (14pt center)";

    public const string SkipCountsMessagePrefix = "skipped ";

    public const string SkipCountsInTablesInfix = " paragraphs inside tables and ";

    public const string SkipCountsBeforeAnchorSuffix = " paragraphs before anchor";

    private const string SectionFontSizeHalfPoints = "32";

    private const string SubsectionFontSizeHalfPoints = "28";

    public string Name => nameof(PromoteSectionsRule);

    public RuleSeverity Severity => RuleSeverity.Optional;

    public void Apply(WordprocessingDocument doc, FormattingContext ctx, IReport report)
    {
        ArgumentNullException.ThrowIfNull(doc);
        ArgumentNullException.ThrowIfNull(ctx);
        ArgumentNullException.ThrowIfNull(report);

        var mainPart = doc.MainDocumentPart;
        var body = mainPart?.Document?.Body;
        if (body is null)
        {
            return;
        }

        var anchor = BodySectionDetector.FindIntroductionAnchor(body, mainPart);
        if (anchor is null)
        {
            report.Warn(Name, AnchorMissingMessage);
            return;
        }

        var allParagraphs = body.Elements<Paragraph>().ToList();
        var anchorIndex = allParagraphs.IndexOf(anchor);
        report.Info(Name, $"{AnchorPositionMessagePrefix}{anchorIndex}");

        var skippedInTables = CountParagraphsInsideTables(body);
        var skippedBeforeAnchor = anchorIndex;

        var sectionsPromoted = 0;
        var subsectionsPromoted = 0;
        for (var i = anchorIndex; i < allParagraphs.Count; i++)
        {
            var paragraph = allParagraphs[i];

            if (BodySectionDetector.IsInsideTable(paragraph))
            {
                continue;
            }

            if (IsContextSkipParagraph(paragraph, ctx))
            {
                continue;
            }

            if (BodySectionDetector.IsSection(paragraph, mainPart))
            {
                ApplyPromotion(paragraph, SectionFontSizeHalfPoints);
                sectionsPromoted++;
            }
            else if (BodySectionDetector.IsSubsection(paragraph, mainPart))
            {
                ApplyPromotion(paragraph, SubsectionFontSizeHalfPoints);
                subsectionsPromoted++;
            }
        }

        report.Info(
            Name,
            $"{SummaryPromotedPrefix}{sectionsPromoted}{SummarySectionsInfix}{subsectionsPromoted}{SummarySubsectionsSuffix}");
        report.Info(
            Name,
            $"{SkipCountsMessagePrefix}{skippedInTables}{SkipCountsInTablesInfix}{skippedBeforeAnchor}{SkipCountsBeforeAnchorSuffix}");
    }

    private static int CountParagraphsInsideTables(Body body)
    {
        var count = 0;
        foreach (var paragraph in body.Descendants<Paragraph>())
        {
            if (BodySectionDetector.IsInsideTable(paragraph))
            {
                count++;
            }
        }

        return count;
    }

    private static bool IsContextSkipParagraph(Paragraph paragraph, FormattingContext ctx)
        => ReferenceEquals(paragraph, ctx.SectionParagraph)
           || ReferenceEquals(paragraph, ctx.TitleParagraph)
           || ReferenceEquals(paragraph, ctx.DoiParagraph);

    private static void ApplyPromotion(Paragraph paragraph, string fontSizeHalfPoints)
    {
        SetCenterAlignment(paragraph);

        foreach (var run in paragraph.Descendants<Run>())
        {
            if (!HasText(run))
            {
                continue;
            }

            SetRunFontSize(run, fontSizeHalfPoints);
        }
    }

    private static void SetCenterAlignment(Paragraph paragraph)
    {
        var properties = paragraph.ParagraphProperties ??= new ParagraphProperties();
        properties.Justification = new Justification { Val = JustificationValues.Center };
    }

    private static void SetRunFontSize(Run run, string halfPoints)
    {
        var properties = run.RunProperties ??= new RunProperties();
        properties.FontSize = new FontSize { Val = halfPoints };
        properties.FontSizeComplexScript = new FontSizeComplexScript { Val = halfPoints };
    }

    private static bool HasText(Run run)
    {
        foreach (var _ in run.Descendants<Text>())
        {
            return true;
        }

        return false;
    }
}
