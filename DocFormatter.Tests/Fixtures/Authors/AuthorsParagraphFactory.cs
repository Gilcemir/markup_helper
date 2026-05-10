using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace DocFormatter.Tests.Fixtures.Authors;

internal static class AuthorsParagraphFactory
{
    public const string SectionText = "Original Article";
    public const string TitleText = "On the Behavior of Title";

    public static WordprocessingDocument CreateDocumentWithAuthorsParagraph(params OpenXmlElement[] authorsParagraphChildren)
    {
        var authors = new Paragraph(authorsParagraphChildren);
        return CreateDocumentInternal(BuildSection(), BuildTitle(), authors);
    }

    public static WordprocessingDocument CreateDocumentWithoutAuthorsParagraph()
    {
        return CreateDocumentInternal(BuildSection(), BuildTitle());
    }

    public static WordprocessingDocument CreateDocumentWithTopTableAndAuthors(
        Table topTable,
        params OpenXmlElement[] authorsParagraphChildren)
    {
        var authors = new Paragraph(authorsParagraphChildren);
        return CreateDocumentInternal(topTable, BuildSection(), BuildTitle(), authors);
    }

    public static Body GetBody(WordprocessingDocument doc)
        => doc.MainDocumentPart!.Document!.Body!;

    public static Paragraph GetAuthorsParagraph(WordprocessingDocument doc)
        => GetBody(doc).Elements<Paragraph>().Last();

    public static Run TextRun(string text)
        => new(new Text(text) { Space = SpaceProcessingModeValues.Preserve });

    public static Run SuperscriptRun(string text)
    {
        var properties = new RunProperties(
            new VerticalTextAlignment { Val = VerticalPositionValues.Superscript });
        return new Run(properties, new Text(text) { Space = SpaceProcessingModeValues.Preserve });
    }

    public static Hyperlink Hyperlink(string relationshipId, params OpenXmlElement[] children)
        => new(children) { Id = relationshipId };

    // ADR-008 regression fixtures: reproduce the divergent shapes that pre-fix produced
    // "1,*" / "1,2,*" superscripts in the formatted .docx and broke Markup's mark_authors.
    // The factories take the affected author's name and emit the exact run sequence from
    // the original 5313 / 5449 input so the rule is exercised end-to-end with the failure
    // shape baked in.
    public static OpenXmlElement[] Build5313FailureShape(string authorName)
        => new OpenXmlElement[]
        {
            TextRun(authorName),
            SuperscriptRun("1"),
            SuperscriptRun("*"),
        };

    public static OpenXmlElement[] Build5449FailureShape(string authorName)
        => new OpenXmlElement[]
        {
            TextRun(authorName),
            SuperscriptRun("1,2"),
            SuperscriptRun("*"),
        };

    private static Paragraph BuildSection()
        => new(new Run(new Text(SectionText) { Space = SpaceProcessingModeValues.Preserve }));

    private static Paragraph BuildTitle()
        => new(new Run(new Text(TitleText) { Space = SpaceProcessingModeValues.Preserve }));

    private static WordprocessingDocument CreateDocumentInternal(params OpenXmlElement[] bodyChildren)
    {
        var stream = new MemoryStream();
        var doc = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document);
        var mainPart = doc.AddMainDocumentPart();
        mainPart.Document = new Document(new Body(bodyChildren));
        return doc;
    }
}
