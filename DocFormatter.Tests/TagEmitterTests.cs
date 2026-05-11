using DocFormatter.Core.Rules;
using DocFormatter.Core.TagEmission;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Xunit;

namespace DocFormatter.Tests;

public sealed class TagEmitterTests
{
    private static readonly IReadOnlyList<(string Key, string Value)> NoAttrs =
        Array.Empty<(string, string)>();

    private static string RunText(Run run)
        => string.Concat(run.Descendants<Text>().Select(t => t.Text));

    private static SpaceProcessingModeValues? RunTextSpace(Run run)
        => run.Descendants<Text>().Single().Space?.Value;

    private static Paragraph PlainParagraph(params (string Text, bool Superscript)[] runs)
    {
        var paragraph = new Paragraph();
        foreach (var (text, superscript) in runs)
        {
            var props = new RunProperties();
            if (superscript)
            {
                props.AppendChild(new VerticalTextAlignment { Val = VerticalPositionValues.Superscript });
            }

            paragraph.AppendChild(new Run(
                props,
                new Text(text) { Space = SpaceProcessingModeValues.Preserve }));
        }

        return paragraph;
    }

    [Fact]
    public void OpeningTag_WithSingleAttribute_EmitsLiteralWithSpacePreserve()
    {
        var run = TagEmitter.OpeningTag("abstract", new[] { ("language", "en") });

        Assert.Equal("[abstract language=\"en\"]", RunText(run));
        Assert.Equal(SpaceProcessingModeValues.Preserve, RunTextSpace(run));
    }

    [Fact]
    public void ClosingTag_EmitsBracketSlashLiteralWithSpacePreserve()
    {
        var run = TagEmitter.ClosingTag("abstract");

        Assert.Equal("[/abstract]", RunText(run));
        Assert.Equal(SpaceProcessingModeValues.Preserve, RunTextSpace(run));
    }

    [Fact]
    public void OpeningTag_WithMultipleAttributes_PreservesOrderAndSeparatesWithSingleSpaces()
    {
        var run = TagEmitter.OpeningTag(
            "xref",
            new[] { ("ref-type", "aff"), ("rid", "aff1"), ("id", "x1") });

        Assert.Equal("[xref ref-type=\"aff\" rid=\"aff1\" id=\"x1\"]", RunText(run));
    }

    [Fact]
    public void OpeningTag_WithEmptyAttributeList_OmitsTrailingSpace()
    {
        var run = TagEmitter.OpeningTag("hist", NoAttrs);

        Assert.Equal("[hist]", RunText(run));
    }

    [Fact]
    public void OpeningTag_WithRawValueContainingClosingBracket_EmitsAsIs()
    {
        // DTD 4.0 invariant: raw attribute values, no escaping.
        var run = TagEmitter.OpeningTag("aff", new[] { ("orgname", "X[Y]Z") });

        Assert.Equal("[aff orgname=\"X[Y]Z\"]", RunText(run));
    }

    [Fact]
    public void OpeningTag_RunPropertiesExtendBaseWithTagColor()
    {
        var emitted = TagEmitter.OpeningTag("hist", NoAttrs);

        // RunProperties carries the base font/size and a per-tag w:color.
        var rPr = emitted.RunProperties!;
        Assert.NotNull(rPr.GetFirstChild<RunFonts>());
        Assert.NotNull(rPr.GetFirstChild<FontSize>());
        var color = rPr.GetFirstChild<Color>();
        Assert.NotNull(color);
        Assert.Equal("FF6600", color!.Val);
    }

    [Fact]
    public void ClosingTag_RunPropertiesCarryTagColor()
    {
        var emitted = TagEmitter.ClosingTag("hist");

        var color = emitted.RunProperties!.GetFirstChild<Color>();
        Assert.NotNull(color);
        Assert.Equal("FF6600", color!.Val);
    }

    [Fact]
    public void TagLiteralRun_AssignsColorBasedOnExtractedTagName()
    {
        var run = TagEmitter.TagLiteralRun("[xref ref-type=\"aff\" rid=\"aff1\"]");

        Assert.Equal("[xref ref-type=\"aff\" rid=\"aff1\"]", RunText(run));
        var color = run.RunProperties!.GetFirstChild<Color>();
        Assert.NotNull(color);
        Assert.Equal("0000FF", color!.Val);
    }

    [Fact]
    public void TagLiteralRun_ClosingTagAlsoGetsColor()
    {
        var run = TagEmitter.TagLiteralRun("[/authorid]");

        var color = run.RunProperties!.GetFirstChild<Color>();
        Assert.NotNull(color);
        Assert.Equal("FF99CC", color!.Val);
    }

    [Fact]
    public void InsertOpeningBefore_PlacesRunAsPreviousSiblingOfFirstInline()
    {
        var paragraph = PlainParagraph(("hello", false), (" world", false));
        var originalFirst = paragraph.Elements<Run>().First();

        TagEmitter.InsertOpeningBefore(paragraph, "abstract", new[] { ("language", "en") });

        var children = paragraph.Elements<Run>().ToList();
        Assert.Equal("[abstract language=\"en\"]", RunText(children[0]));
        Assert.Same(originalFirst, children[1]);
        Assert.Equal("hello", RunText(children[1]));
        Assert.Equal(" world", RunText(children[2]));
    }

    [Fact]
    public void InsertOpeningBefore_RespectsParagraphPropertiesAsFirstChild()
    {
        // Word requires <w:pPr> to be the first child of <w:p>; the helper
        // must NOT push the opening run before it.
        var paragraph = new Paragraph(
            new ParagraphProperties(new ParagraphStyleId { Val = "Heading1" }),
            new Run(new Text("body") { Space = SpaceProcessingModeValues.Preserve }));

        TagEmitter.InsertOpeningBefore(paragraph, "kwdgrp", new[] { ("language", "en") });

        Assert.IsType<ParagraphProperties>(paragraph.ChildElements[0]);
        var openingRun = Assert.IsType<Run>(paragraph.ChildElements[1]);
        Assert.Equal("[kwdgrp language=\"en\"]", RunText(openingRun));
    }

    [Fact]
    public void InsertClosingAfter_PlacesRunAsNextSiblingOfLastInline()
    {
        var paragraph = PlainParagraph(("hello", false), (" world", false));
        var originalLast = paragraph.Elements<Run>().Last();

        TagEmitter.InsertClosingAfter(paragraph, "abstract");

        var children = paragraph.Elements<Run>().ToList();
        Assert.Same(originalLast, children[^2]);
        Assert.Equal("[/abstract]", RunText(children[^1]));
    }

    [Fact]
    public void WrapParagraphContent_ProducesOpeningOriginalsClosingInOrder()
    {
        var paragraph = PlainParagraph(("first", false), ("second", false), ("third", false));

        TagEmitter.WrapParagraphContent(paragraph, "abstract", new[] { ("language", "en") });

        var texts = paragraph.Elements<Run>().Select(RunText).ToList();
        Assert.Equal(
            new[] { "[abstract language=\"en\"]", "first", "second", "third", "[/abstract]" },
            texts);
    }

    [Fact]
    public void WrapParagraphContent_ZeroesSuperscriptOnSuperscriptedRunOnly()
    {
        var paragraph = PlainParagraph(("name", false), ("1", true));

        TagEmitter.WrapParagraphContent(paragraph, "label", NoAttrs);

        var originalRuns = paragraph.Elements<Run>()
            .Where(r => !RunText(r).StartsWith('[') || RunText(r) == "[")
            .ToList();

        // The two original runs are at index 1 and 2 in the wrapped paragraph
        // (index 0 is the opening literal, last is the closing literal).
        var inner = paragraph.Elements<Run>().ToList();
        var nameRun = inner[1];
        var labelRun = inner[2];

        Assert.Equal("name", RunText(nameRun));
        Assert.Equal("1", RunText(labelRun));

        // Non-superscript sibling: untouched (RunProperties present, no VertAlign).
        Assert.Null(nameRun.RunProperties?.VerticalTextAlignment);

        // Superscript run: VertAlign element removed.
        Assert.Null(labelRun.RunProperties?.VerticalTextAlignment);
    }

    [Fact]
    public void WrapParagraphContent_LeavesNonSuperscriptVerticalAlignmentUntouched()
    {
        // Subscript runs are not superscript and must be preserved as-is.
        var paragraph = new Paragraph();
        paragraph.AppendChild(new Run(
            new RunProperties(new VerticalTextAlignment { Val = VerticalPositionValues.Subscript }),
            new Text("H2O") { Space = SpaceProcessingModeValues.Preserve }));

        TagEmitter.WrapParagraphContent(paragraph, "p", NoAttrs);

        var middleRun = paragraph.Elements<Run>().ToList()[1];
        var vert = middleRun.RunProperties!.VerticalTextAlignment;
        Assert.NotNull(vert);
        Assert.Equal(VerticalPositionValues.Subscript, vert!.Val!.Value);
    }

    [Fact]
    public void WrapParagraphContent_RoundTripsThroughWordprocessingDocument()
    {
        // Real OpenXML save+load: assert the literals appear in document text in order.
        using var stream = new MemoryStream();
        using (var doc = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document))
        {
            var mainPart = doc.AddMainDocumentPart();
            var paragraph = new Paragraph(new Run(
                new Text("the body") { Space = SpaceProcessingModeValues.Preserve }));
            mainPart.Document = new Document(new Body(paragraph));

            TagEmitter.WrapParagraphContent(paragraph, "abstract", new[] { ("language", "en") });
            mainPart.Document.Save();
        }

        stream.Position = 0;
        using var reloaded = WordprocessingDocument.Open(stream, isEditable: false);
        var paragraphs = reloaded.MainDocumentPart!.Document!.Body!.Elements<Paragraph>().ToList();
        Assert.Single(paragraphs);

        var concatenated = string.Concat(
            paragraphs[0].Descendants<Text>().Select(t => t.Text));
        Assert.Equal("[abstract language=\"en\"]the body[/abstract]", concatenated);
    }

    [Fact]
    public void OpeningTag_ThrowsOnNullOrWhitespaceTagName()
    {
        Assert.Throws<ArgumentException>(() => TagEmitter.OpeningTag("", NoAttrs));
        Assert.Throws<ArgumentException>(() => TagEmitter.OpeningTag("   ", NoAttrs));
        Assert.Throws<ArgumentNullException>(() => TagEmitter.OpeningTag(null!, NoAttrs));
    }

    [Fact]
    public void OpeningTag_ThrowsOnNullAttrs()
    {
        Assert.Throws<ArgumentNullException>(
            () => TagEmitter.OpeningTag("abstract", null!));
    }

    [Fact]
    public void ClosingTag_ThrowsOnNullOrWhitespaceTagName()
    {
        Assert.Throws<ArgumentException>(() => TagEmitter.ClosingTag(""));
        Assert.Throws<ArgumentNullException>(() => TagEmitter.ClosingTag(null!));
    }

    [Fact]
    public void Insert_ThrowsOnNullAnchor()
    {
        Assert.Throws<ArgumentNullException>(
            () => TagEmitter.InsertOpeningBefore(null!, "abstract", NoAttrs));
        Assert.Throws<ArgumentNullException>(
            () => TagEmitter.InsertClosingAfter(null!, "abstract"));
        Assert.Throws<ArgumentNullException>(
            () => TagEmitter.WrapParagraphContent(null!, "abstract", NoAttrs));
    }
}
