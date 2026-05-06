using DocumentFormat.OpenXml.Wordprocessing;

namespace DocFormatter.Core.Rules;

internal static class HeaderParagraphLocator
{
    public static Paragraph? FindAuthorsParagraph(Body body)
    {
        ArgumentNullException.ThrowIfNull(body);

        var nonEmptyCount = 0;
        foreach (var paragraph in body.Elements<Paragraph>())
        {
            var text = string.Concat(paragraph.Descendants<Text>().Select(t => t.Text));
            if (string.IsNullOrWhiteSpace(text))
            {
                continue;
            }

            nonEmptyCount++;
            if (nonEmptyCount == 3)
            {
                return paragraph;
            }
        }

        return null;
    }
}
