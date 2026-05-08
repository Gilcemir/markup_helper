using System.Text.RegularExpressions;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace DocFormatter.Core.Rules;

internal static class BodySectionDetector
{
    private const int MaxStyleChainHops = 10;
    private const int MinSectionTextLength = 3;

    private static readonly Regex IntroductionAnchorRegex = new(
        @"^INTRODUCTION[\s.:]*$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static Paragraph? FindIntroductionAnchor(Body body, MainDocumentPart? mainPart)
    {
        ArgumentNullException.ThrowIfNull(body);

        foreach (var paragraph in body.Elements<Paragraph>())
        {
            var text = GetTrimmedText(paragraph);
            if (!IntroductionAnchorRegex.IsMatch(text))
            {
                continue;
            }

            if (!IsSection(paragraph, mainPart))
            {
                continue;
            }

            return paragraph;
        }

        return null;
    }

    public static bool IsSection(Paragraph paragraph, MainDocumentPart? mainPart)
    {
        ArgumentNullException.ThrowIfNull(paragraph);
        return MatchesSectionPredicate(paragraph, mainPart, requireAllUpperLetters: true);
    }

    public static bool IsSubsection(Paragraph paragraph, MainDocumentPart? mainPart)
    {
        ArgumentNullException.ThrowIfNull(paragraph);
        return MatchesSectionPredicate(paragraph, mainPart, requireAllUpperLetters: false);
    }

    public static bool IsInsideTable(Paragraph paragraph)
    {
        ArgumentNullException.ThrowIfNull(paragraph);

        OpenXmlElement? ancestor = paragraph.Parent;
        while (ancestor is not null)
        {
            if (ancestor is Table)
            {
                return true;
            }

            ancestor = ancestor.Parent;
        }

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

        // Note: <w:pPr><w:rPr><w:b/></w:rPr></w:pPr> formats only the paragraph mark
        // (the pilcrow), not the runs. Word does not cascade it to runs without their
        // own <w:b/>, so we don't treat it as a fallback for run-level bold either.

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

    private static bool MatchesSectionPredicate(
        Paragraph paragraph,
        MainDocumentPart? mainPart,
        bool requireAllUpperLetters)
    {
        var trimmed = GetTrimmedText(paragraph);
        if (trimmed.Length < MinSectionTextLength)
        {
            return false;
        }

        var hasLetter = false;
        var hasLowerLetter = false;
        foreach (var ch in trimmed)
        {
            if (!char.IsLetter(ch))
            {
                continue;
            }

            hasLetter = true;
            if (char.IsLower(ch))
            {
                hasLowerLetter = true;
            }
        }

        if (!hasLetter)
        {
            return false;
        }

        if (requireAllUpperLetters)
        {
            if (hasLowerLetter)
            {
                return false;
            }
        }
        else
        {
            if (!hasLowerLetter)
            {
                return false;
            }
        }

        if (!HasAcceptedAlignment(paragraph))
        {
            return false;
        }

        return HasBoldRatioAtLeastNinetyPercent(paragraph, mainPart);
    }

    private static bool HasAcceptedAlignment(Paragraph paragraph)
    {
        var justification = paragraph.ParagraphProperties?.Justification?.Val;
        if (justification is null || !justification.HasValue)
        {
            return true;
        }

        var value = justification.Value;
        return value == JustificationValues.Left || value == JustificationValues.Both;
    }

    private static bool HasBoldRatioAtLeastNinetyPercent(Paragraph paragraph, MainDocumentPart? mainPart)
    {
        var totalNonWhitespace = 0;
        var boldNonWhitespace = 0;

        foreach (var run in paragraph.Descendants<Run>())
        {
            var nonWhitespaceCount = CountNonWhitespace(run.InnerText);
            if (nonWhitespaceCount == 0)
            {
                continue;
            }

            totalNonWhitespace += nonWhitespaceCount;
            if (IsBoldEffective(run, paragraph, mainPart))
            {
                boldNonWhitespace += nonWhitespaceCount;
            }
        }

        if (totalNonWhitespace == 0)
        {
            return false;
        }

        return boldNonWhitespace * 10 >= totalNonWhitespace * 9;
    }

    private static int CountNonWhitespace(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return 0;
        }

        var count = 0;
        foreach (var ch in text)
        {
            if (!char.IsWhiteSpace(ch))
            {
                count++;
            }
        }

        return count;
    }

    private static string GetTrimmedText(Paragraph paragraph)
        => (paragraph.InnerText ?? string.Empty).Trim();

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
