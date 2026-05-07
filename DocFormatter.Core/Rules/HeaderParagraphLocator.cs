using System.Text;
using DocumentFormat.OpenXml.Wordprocessing;

namespace DocFormatter.Core.Rules;

internal static class HeaderParagraphLocator
{
    public static IReadOnlyList<Paragraph> FindAuthorsParagraphs(
        Body body,
        IReadOnlyList<string> abstractMarkers)
    {
        ArgumentNullException.ThrowIfNull(body);
        ArgumentNullException.ThrowIfNull(abstractMarkers);

        var collected = new List<Paragraph>();
        var nonEmptyLineCount = 0;

        foreach (var paragraph in body.Elements<Paragraph>())
        {
            var nonEmptyLines = CountNonEmptyLogicalLines(paragraph);
            if (nonEmptyLines == 0)
            {
                continue;
            }

            if (collected.Count == 0)
            {
                nonEmptyLineCount += nonEmptyLines;
                if (nonEmptyLineCount >= 3)
                {
                    collected.Add(paragraph);
                }

                continue;
            }

            if (LooksLikeAffiliationOrAbstract(paragraph, abstractMarkers))
            {
                break;
            }

            collected.Add(paragraph);
        }

        return collected;
    }

    private static int CountNonEmptyLogicalLines(Paragraph paragraph)
    {
        var lines = 0;
        var current = new StringBuilder();

        foreach (var node in paragraph.Descendants())
        {
            switch (node)
            {
                case Text t:
                    current.Append(t.Text);
                    break;
                case Break:
                    if (!IsBlank(current))
                    {
                        lines++;
                    }

                    current.Clear();
                    break;
            }
        }

        if (!IsBlank(current))
        {
            lines++;
        }

        return lines;

        static bool IsBlank(StringBuilder sb)
        {
            for (var i = 0; i < sb.Length; i++)
            {
                if (!char.IsWhiteSpace(sb[i]))
                {
                    return false;
                }
            }

            return true;
        }
    }

    private static bool LooksLikeAffiliationOrAbstract(
        Paragraph paragraph,
        IReadOnlyList<string> abstractMarkers)
    {
        foreach (var run in paragraph.Descendants<Run>())
        {
            var text = string.Concat(run.Descendants<Text>().Select(t => t.Text));
            if (string.IsNullOrWhiteSpace(text))
            {
                continue;
            }

            if (IsSuperscript(run))
            {
                return true;
            }

            var trimmed = text.TrimStart();
            foreach (var marker in abstractMarkers)
            {
                if (trimmed.StartsWith(marker, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        return false;
    }

    private static bool IsSuperscript(Run run)
    {
        var vert = run.RunProperties?.VerticalTextAlignment;
        return vert?.Val is { } val && val.Value == VerticalPositionValues.Superscript;
    }
}
