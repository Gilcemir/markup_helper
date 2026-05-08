using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace DocFormatter.Tests.Fixtures.Phase2;

internal static class Phase2DocxFixtureBuilder
{
    public const string SectionText = "Original Article";
    public const string TitleText = "On the Behavior of Mosquitoes";
    public const string AuthorName = "Maria Silva";
    public const string Affiliation1Text = "Universidade Y";
    public const string Affiliation2Text = "Instituto Z";
    public const string CorrespondingEmail = "maria@y.edu";
    public const string MalformedEmailTrailer = "(see attached)";
    public const string Doi = "10.1234/abc";
    public const string Elocation = "e2024001";

    internal sealed record Phase2Options(
        bool IncludeCorrespondingMarker,
        string AbstractHeading = "Abstract",
        string AbstractBody = AbstractParagraphFactory.DefaultBodyText,
        bool MalformedEmail = false);

    public static void Write(string path, Phase2Options options)
    {
        ArgumentNullException.ThrowIfNull(options);

        using var doc = WordprocessingDocument.Create(path, WordprocessingDocumentType.Document);
        var mainPart = doc.AddMainDocumentPart();
        mainPart.Document = new Document(BuildBody(options));
    }

    public static void WriteWithCorrespondingMarker(string path)
        => Write(path, new Phase2Options(IncludeCorrespondingMarker: true));

    public static void WriteWithoutCorrespondingMarker(string path)
        => Write(path, new Phase2Options(IncludeCorrespondingMarker: false));

    public static void WriteWithResumoSource(string path)
        => Write(
            path,
            new Phase2Options(
                IncludeCorrespondingMarker: false,
                AbstractHeading: "Resumo",
                AbstractBody: AbstractParagraphFactory.DefaultPortugueseBodyText));

    public static void WriteWithMalformedEmail(string path)
        => Write(
            path,
            new Phase2Options(IncludeCorrespondingMarker: true, MalformedEmail: true));

    internal static List<OpenXmlElement> BuildPrologueElements(Phase2Options options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return new List<OpenXmlElement>
        {
            BuildTopTable(),
            PlainParagraph(SectionText),
            PlainParagraph(TitleText),
            BuildAuthorsParagraph(options.IncludeCorrespondingMarker),
            BuildAffiliation1Paragraph(),
            BuildAffiliation2Paragraph(options),
            AbstractParagraphFactory.CreateItalicWrapped(options.AbstractHeading, options.AbstractBody),
        };
    }

    private static Body BuildBody(Phase2Options options)
    {
        return new Body(BuildPrologueElements(options));
    }

    private static Table BuildTopTable()
    {
        var grid = new TableGrid(
            new GridColumn { Width = "2000" },
            new GridColumn { Width = "2000" },
            new GridColumn { Width = "2000" });
        return new Table(
            grid,
            new TableRow(
                BuildCell("id", "ART01"),
                BuildCell("elocation", Elocation),
                BuildCell("doi", Doi)));
    }

    private static TableCell BuildCell(params string[] paragraphTexts)
    {
        var cell = new TableCell();
        foreach (var text in paragraphTexts)
        {
            cell.AppendChild(PlainParagraph(text));
        }
        return cell;
    }

    private static Paragraph PlainParagraph(string text)
        => new(new Run(new Text(text) { Space = SpaceProcessingModeValues.Preserve }));

    private static Paragraph BuildAuthorsParagraph(bool includeCorrespondingMarker)
    {
        var nameRun = new Run(new Text(AuthorName) { Space = SpaceProcessingModeValues.Preserve });
        var labelText = includeCorrespondingMarker ? "1,2*" : "1,2";
        var labelProperties = new RunProperties(
            new VerticalTextAlignment { Val = VerticalPositionValues.Superscript });
        var labelRun = new Run(
            labelProperties,
            new Text(labelText) { Space = SpaceProcessingModeValues.Preserve });
        return new Paragraph(nameRun, labelRun);
    }

    private static Paragraph BuildAffiliation1Paragraph()
    {
        var labelProperties = new RunProperties(
            new VerticalTextAlignment { Val = VerticalPositionValues.Superscript });
        var labelRun = new Run(
            labelProperties,
            new Text("1") { Space = SpaceProcessingModeValues.Preserve });
        var textRun = new Run(
            new Text(" " + Affiliation1Text) { Space = SpaceProcessingModeValues.Preserve });
        return new Paragraph(labelRun, textRun);
    }

    private static Paragraph BuildAffiliation2Paragraph(Phase2Options options)
    {
        var labelProperties = new RunProperties(
            new VerticalTextAlignment { Val = VerticalPositionValues.Superscript });
        var labelRun = new Run(
            labelProperties,
            new Text("2") { Space = SpaceProcessingModeValues.Preserve });

        var bodyText = " " + Affiliation2Text;
        if (options.IncludeCorrespondingMarker)
        {
            var trailer = options.MalformedEmail ? MalformedEmailTrailer : CorrespondingEmail;
            bodyText += " * E-mail: " + trailer;
        }

        var textRun = new Run(
            new Text(bodyText) { Space = SpaceProcessingModeValues.Preserve });
        return new Paragraph(labelRun, textRun);
    }
}
