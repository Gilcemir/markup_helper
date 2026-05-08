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

    public sealed record RunSpec(
        string Text,
        bool Bold = false,
        bool? BoldVal = null);

    public static WordprocessingDocument CreateDocument(
        IEnumerable<OpenXmlElement>? bodyElements = null,
        IEnumerable<Paragraph>? paragraphs = null,
        IEnumerable<StyleDefinition>? styles = null,
        bool includeStylesPart = true)
    {
        var stream = new MemoryStream();
        var doc = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document);
        var mainPart = doc.AddMainDocumentPart();

        var children = new List<OpenXmlElement>();
        if (bodyElements is not null)
        {
            children.AddRange(bodyElements);
        }

        if (paragraphs is not null)
        {
            children.AddRange(paragraphs);
        }

        mainPart.Document = new Document(new Body(children.ToArray()));

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
        bool? paragraphMarkBoldVal = null,
        JustificationValues? alignment = null)
    {
        var paragraph = new Paragraph();

        var needsParagraphProperties = styleId is not null || paragraphMarkBold || alignment is not null;
        if (needsParagraphProperties)
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

            if (alignment is not null)
            {
                paragraphProperties.Justification = new Justification { Val = alignment.Value };
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

    public static Paragraph BuildParagraphWithRuns(
        IEnumerable<RunSpec> runs,
        string? styleId = null,
        bool paragraphMarkBold = false,
        bool? paragraphMarkBoldVal = null,
        JustificationValues? alignment = null)
    {
        ArgumentNullException.ThrowIfNull(runs);

        var paragraph = new Paragraph();

        var needsParagraphProperties = styleId is not null || paragraphMarkBold || alignment is not null;
        if (needsParagraphProperties)
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

            if (alignment is not null)
            {
                paragraphProperties.Justification = new Justification { Val = alignment.Value };
            }

            paragraph.ParagraphProperties = paragraphProperties;
        }

        foreach (var spec in runs)
        {
            Run run;
            if (spec.Bold)
            {
                var runProperties = new RunProperties(BuildBold(spec.BoldVal));
                run = new Run(
                    runProperties,
                    new Text(spec.Text) { Space = SpaceProcessingModeValues.Preserve });
            }
            else
            {
                run = new Run(new Text(spec.Text) { Space = SpaceProcessingModeValues.Preserve });
            }

            paragraph.AppendChild(run);
        }

        return paragraph;
    }

    public static Table WrapInTable(params Paragraph[] paragraphs)
    {
        ArgumentNullException.ThrowIfNull(paragraphs);

        var cell = new TableCell();
        foreach (var paragraph in paragraphs)
        {
            cell.AppendChild(paragraph);
        }

        var row = new TableRow(cell);
        return new Table(row);
    }

    public static Table WrapInNestedTable(int depth, params Paragraph[] paragraphs)
    {
        ArgumentNullException.ThrowIfNull(paragraphs);
        if (depth < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(depth), "Depth must be at least 1.");
        }

        var inner = WrapInTable(paragraphs);
        for (var i = 1; i < depth; i++)
        {
            var cell = new TableCell();
            cell.AppendChild(inner);
            inner = new Table(new TableRow(cell));
        }

        return inner;
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

    public static Paragraph BuildHistoryParagraph(
        string label,
        string detail,
        string separator = ":",
        JustificationValues? alignment = null)
    {
        ArgumentNullException.ThrowIfNull(label);
        ArgumentNullException.ThrowIfNull(detail);
        ArgumentNullException.ThrowIfNull(separator);
        return BuildParagraph(
            $"{label}{separator} {detail}",
            alignment: alignment);
    }

    public static Paragraph BuildIntroductionAnchorParagraph(
        string text = "INTRODUCTION",
        JustificationValues? alignment = null)
    {
        return BuildParagraph(text, runDirectBold: true, alignment: alignment);
    }

    public static Paragraph BuildBlankParagraph() => new();

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
