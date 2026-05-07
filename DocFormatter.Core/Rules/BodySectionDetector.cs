using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace DocFormatter.Core.Rules;

internal static class BodySectionDetector
{
    private const int MaxStyleChainHops = 10;

    public static Paragraph? FindIntroductionAnchor(Body body, MainDocumentPart? mainPart)
    {
        ArgumentNullException.ThrowIfNull(body);
        _ = mainPart;
        return null;
    }

    public static bool IsSection(Paragraph paragraph, MainDocumentPart? mainPart)
    {
        ArgumentNullException.ThrowIfNull(paragraph);
        _ = mainPart;
        return false;
    }

    public static bool IsSubsection(Paragraph paragraph, MainDocumentPart? mainPart)
    {
        ArgumentNullException.ThrowIfNull(paragraph);
        _ = mainPart;
        return false;
    }

    public static bool IsInsideTable(Paragraph paragraph)
    {
        ArgumentNullException.ThrowIfNull(paragraph);
        return false;
    }

    internal static bool IsBoldEffective(Run run, Paragraph paragraph, MainDocumentPart? mainPart)
    {
        ArgumentNullException.ThrowIfNull(run);
        ArgumentNullException.ThrowIfNull(paragraph);

        var runBold = run.RunProperties?.GetFirstChild<Bold>();
        if (runBold is not null)
        {
            return ResolveBold(runBold);
        }

        var paragraphMarkBold = paragraph.ParagraphProperties?
            .GetFirstChild<ParagraphMarkRunProperties>()?
            .GetFirstChild<Bold>();
        if (paragraphMarkBold is not null)
        {
            return ResolveBold(paragraphMarkBold);
        }

        var styleId = paragraph.ParagraphProperties?.ParagraphStyleId?.Val?.Value;
        if (string.IsNullOrEmpty(styleId))
        {
            return false;
        }

        var styles = mainPart?.StyleDefinitionsPart?.Styles;
        if (styles is null)
        {
            return false;
        }

        return ResolveBoldFromStyleChain(styleId, styles);
    }

    private static bool ResolveBoldFromStyleChain(string styleId, Styles styles)
    {
        var visited = new HashSet<string>(StringComparer.Ordinal);
        var current = styleId;

        for (var hop = 0; hop < MaxStyleChainHops; hop++)
        {
            if (string.IsNullOrEmpty(current) || !visited.Add(current))
            {
                return false;
            }

            var style = FindStyle(styles, current);
            if (style is null)
            {
                return false;
            }

            var styleRunBold = style.StyleRunProperties?.GetFirstChild<Bold>();
            if (styleRunBold is not null)
            {
                return ResolveBold(styleRunBold);
            }

            var styleParagraphMarkBold = style.StyleParagraphProperties?
                .GetFirstChild<ParagraphMarkRunProperties>()?
                .GetFirstChild<Bold>();
            if (styleParagraphMarkBold is not null)
            {
                return ResolveBold(styleParagraphMarkBold);
            }

            current = style.BasedOn?.Val?.Value;
        }

        return false;
    }

    private static Style? FindStyle(Styles styles, string styleId)
    {
        foreach (var style in styles.Elements<Style>())
        {
            if (style.StyleId?.Value == styleId)
            {
                return style;
            }
        }

        return null;
    }

    private static bool ResolveBold(Bold bold)
        => bold.Val?.Value ?? true;
}
