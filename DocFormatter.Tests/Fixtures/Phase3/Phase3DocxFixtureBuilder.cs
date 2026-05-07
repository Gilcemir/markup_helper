using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace DocFormatter.Tests.Fixtures.Phase3;

internal static class Phase3DocxFixtureBuilder
{
    public sealed record StyleDefinition(
        string StyleId,
        bool RunPropertiesBold = false,
        bool? RunPropertiesBoldVal = null,
        bool ParagraphMarkBold = false,
        bool? ParagraphMarkBoldVal = null,
        string? BasedOn = null);

    public static WordprocessingDocument CreateDocument(
        IEnumerable<Paragraph>? paragraphs = null,
        IEnumerable<StyleDefinition>? styles = null,
        bool includeStylesPart = true)
    {
        var stream = new MemoryStream();
        var doc = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document);
        var mainPart = doc.AddMainDocumentPart();
        var bodyChildren = paragraphs?.Cast<OpenXmlElement>().ToArray() ?? [];
        mainPart.Document = new Document(new Body(bodyChildren));

        if (includeStylesPart)
        {
            var stylesPart = mainPart.AddNewPart<StyleDefinitionsPart>();
            var stylesElement = new Styles();
            if (styles is not null)
            {
                foreach (var styleDefinition in styles)
                {
                    stylesElement.Append(BuildStyle(styleDefinition));
                }
            }

            stylesPart.Styles = stylesElement;
        }

        return doc;
    }

    public static Paragraph BuildParagraph(
        string text,
        string? styleId = null,
        bool runDirectBold = false,
        bool? runDirectBoldVal = null,
        bool paragraphMarkBold = false,
        bool? paragraphMarkBoldVal = null)
    {
        var paragraph = new Paragraph();

        if (styleId is not null || paragraphMarkBold)
        {
            var paragraphProperties = new ParagraphProperties();

            if (styleId is not null)
            {
                paragraphProperties.ParagraphStyleId = new ParagraphStyleId { Val = styleId };
            }

            if (paragraphMarkBold)
            {
                paragraphProperties.AppendChild(
                    new ParagraphMarkRunProperties(BuildBold(paragraphMarkBoldVal)));
            }

            paragraph.ParagraphProperties = paragraphProperties;
        }

        Run run;
        if (runDirectBold)
        {
            var runProperties = new RunProperties(BuildBold(runDirectBoldVal));
            run = new Run(
                runProperties,
                new Text(text) { Space = SpaceProcessingModeValues.Preserve });
        }
        else
        {
            run = new Run(new Text(text) { Space = SpaceProcessingModeValues.Preserve });
        }

        paragraph.AppendChild(run);
        return paragraph;
    }

    public static Run GetFirstRun(Paragraph paragraph)
    {
        ArgumentNullException.ThrowIfNull(paragraph);
        return paragraph.Elements<Run>().First();
    }

    public static Body GetBody(WordprocessingDocument doc)
    {
        ArgumentNullException.ThrowIfNull(doc);
        return doc.MainDocumentPart!.Document!.Body!;
    }

    private static Style BuildStyle(StyleDefinition definition)
    {
        var style = new Style
        {
            Type = StyleValues.Paragraph,
            StyleId = definition.StyleId,
        };

        if (definition.BasedOn is not null)
        {
            style.AppendChild(new BasedOn { Val = definition.BasedOn });
        }

        if (definition.RunPropertiesBold)
        {
            style.AppendChild(
                new StyleRunProperties(BuildBold(definition.RunPropertiesBoldVal)));
        }

        if (definition.ParagraphMarkBold)
        {
            var styleParagraphProperties = new StyleParagraphProperties();
            styleParagraphProperties.AppendChild(
                new ParagraphMarkRunProperties(BuildBold(definition.ParagraphMarkBoldVal)));
            style.AppendChild(styleParagraphProperties);
        }

        return style;
    }

    private static Bold BuildBold(bool? val)
    {
        var bold = new Bold();
        if (val.HasValue)
        {
            bold.Val = OnOffValue.FromBoolean(val.Value);
        }

        return bold;
    }
}
