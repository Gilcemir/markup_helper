using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;

namespace DocFormatter.Tests.Fixtures.Phase2;

internal static class KeywordsParagraphFactory
{
    public const string DefaultEnglishMarker = "Keywords:";
    public const string DefaultPortugueseMarker = "Palavras-chave:";

    public static Paragraph CreateCommaSeparated(
        string marker = DefaultEnglishMarker,
        params string[] keywords)
    {
        ArgumentNullException.ThrowIfNull(keywords);
        return CreateWithSeparator(marker, ", ", keywords);
    }

    public static Paragraph CreateSemicolonSeparated(
        string marker = DefaultEnglishMarker,
        params string[] keywords)
    {
        ArgumentNullException.ThrowIfNull(keywords);
        return CreateWithSeparator(marker, "; ", keywords);
    }

    private static Paragraph CreateWithSeparator(string marker, string separator, string[] keywords)
    {
        var text = keywords.Length == 0
            ? marker
            : $"{marker} {string.Join(separator, keywords)}";
        return new Paragraph(
            new Run(new Text(text) { Space = SpaceProcessingModeValues.Preserve }));
    }
}
