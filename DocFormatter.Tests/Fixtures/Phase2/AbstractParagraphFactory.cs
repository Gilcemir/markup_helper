using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;

namespace DocFormatter.Tests.Fixtures.Phase2;

internal static class AbstractParagraphFactory
{
    public const string DefaultBodyText = "lorem ipsum dolor sit amet";
    public const string DefaultPortugueseBodyText = "este resumo é em português";

    public static Paragraph CreateItalicWrapped(string heading, string body)
    {
        return new Paragraph(BuildItalicRun($"{heading} - {body}"));
    }

    public static Paragraph CreateMixedItalic(string heading, string preItalic, string italicSpan, string postItalic)
    {
        return new Paragraph(
            BuildItalicRun($"{heading} - {preItalic}"),
            BuildItalicRun(italicSpan),
            BuildPlainRun(postItalic));
    }

    private static Run BuildItalicRun(string text)
        => new(
            new RunProperties(new Italic()),
            new Text(text) { Space = SpaceProcessingModeValues.Preserve });

    private static Run BuildPlainRun(string text)
        => new(new Text(text) { Space = SpaceProcessingModeValues.Preserve });
}
