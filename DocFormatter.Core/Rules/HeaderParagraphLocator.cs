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
        var nonEmptyCount = 0;

        foreach (var paragraph in body.Elements<Paragraph>())
        {
            var text = string.Concat(paragraph.Descendants<Text>().Select(t => t.Text));
            if (string.IsNullOrWhiteSpace(text))
            {
                continue;
            }

            nonEmptyCount++;

            if (nonEmptyCount < 3)
            {
                continue;
            }

            if (collected.Count == 0)
            {
                collected.Add(paragraph);
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
