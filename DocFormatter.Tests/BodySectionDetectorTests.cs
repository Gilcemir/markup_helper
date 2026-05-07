using DocFormatter.Core.Rules;
using DocFormatter.Tests.Fixtures.Phase3;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Xunit;

namespace DocFormatter.Tests;

public sealed class BodySectionDetectorTests
{
    [Fact]
    public void IsBoldEffective_RunDirectBold_ReturnsTrue()
    {
        var paragraph = Phase3DocxFixtureBuilder.BuildParagraph("hello", runDirectBold: true);
        using var doc = Phase3DocxFixtureBuilder.CreateDocument(paragraphs: new[] { paragraph });

        var run = Phase3DocxFixtureBuilder.GetFirstRun(paragraph);

        Assert.True(BodySectionDetector.IsBoldEffective(run, paragraph, doc.MainDocumentPart));
    }

    [Fact]
    public void IsBoldEffective_RunDirectBoldValTrue_ReturnsTrue()
    {
        var paragraph = Phase3DocxFixtureBuilder.BuildParagraph(
            "hello",
            runDirectBold: true,
            runDirectBoldVal: true);
        using var doc = Phase3DocxFixtureBuilder.CreateDocument(paragraphs: new[] { paragraph });

        var run = Phase3DocxFixtureBuilder.GetFirstRun(paragraph);

        Assert.True(BodySectionDetector.IsBoldEffective(run, paragraph, doc.MainDocumentPart));
    }

    [Fact]
    public void IsBoldEffective_RunDirectBoldValFalse_ReturnsFalse()
    {
        var paragraph = Phase3DocxFixtureBuilder.BuildParagraph(
            "hello",
            runDirectBold: true,
            runDirectBoldVal: false);
        using var doc = Phase3DocxFixtureBuilder.CreateDocument(paragraphs: new[] { paragraph });

        var run = Phase3DocxFixtureBuilder.GetFirstRun(paragraph);

        Assert.False(BodySectionDetector.IsBoldEffective(run, paragraph, doc.MainDocumentPart));
    }

    [Fact]
    public void IsBoldEffective_RunDirectBoldOverridesParagraphBold_ReturnsFalse()
    {
        var paragraph = Phase3DocxFixtureBuilder.BuildParagraph(
            "hello",
            runDirectBold: true,
            runDirectBoldVal: false,
            paragraphMarkBold: true);
        using var doc = Phase3DocxFixtureBuilder.CreateDocument(paragraphs: new[] { paragraph });

        var run = Phase3DocxFixtureBuilder.GetFirstRun(paragraph);

        Assert.False(BodySectionDetector.IsBoldEffective(run, paragraph, doc.MainDocumentPart));
    }

    [Fact]
    public void IsBoldEffective_ParagraphMarkBold_ReturnsTrue()
    {
        var paragraph = Phase3DocxFixtureBuilder.BuildParagraph(
            "hello",
            paragraphMarkBold: true);
        using var doc = Phase3DocxFixtureBuilder.CreateDocument(paragraphs: new[] { paragraph });

        var run = Phase3DocxFixtureBuilder.GetFirstRun(paragraph);

        Assert.True(BodySectionDetector.IsBoldEffective(run, paragraph, doc.MainDocumentPart));
    }

    [Fact]
    public void IsBoldEffective_ParagraphMarkBoldValFalse_ReturnsFalse()
    {
        var paragraph = Phase3DocxFixtureBuilder.BuildParagraph(
            "hello",
            paragraphMarkBold: true,
            paragraphMarkBoldVal: false);
        using var doc = Phase3DocxFixtureBuilder.CreateDocument(paragraphs: new[] { paragraph });

        var run = Phase3DocxFixtureBuilder.GetFirstRun(paragraph);

        Assert.False(BodySectionDetector.IsBoldEffective(run, paragraph, doc.MainDocumentPart));
    }

    [Fact]
    public void IsBoldEffective_SingleLevelStyleRunPropertiesBold_ReturnsTrue()
    {
        var paragraph = Phase3DocxFixtureBuilder.BuildParagraph("hello", styleId: "Heading1");
        var styles = new[]
        {
            new Phase3DocxFixtureBuilder.StyleDefinition("Heading1", RunPropertiesBold: true),
        };
        using var doc = Phase3DocxFixtureBuilder.CreateDocument(
            paragraphs: new[] { paragraph },
            styles: styles);

        var run = Phase3DocxFixtureBuilder.GetFirstRun(paragraph);

        Assert.True(BodySectionDetector.IsBoldEffective(run, paragraph, doc.MainDocumentPart));
    }

    [Fact]
    public void IsBoldEffective_SingleLevelStyleParagraphMarkBold_ReturnsTrue()
    {
        var paragraph = Phase3DocxFixtureBuilder.BuildParagraph("hello", styleId: "Heading1");
        var styles = new[]
        {
            new Phase3DocxFixtureBuilder.StyleDefinition("Heading1", ParagraphMarkBold: true),
        };
        using var doc = Phase3DocxFixtureBuilder.CreateDocument(
            paragraphs: new[] { paragraph },
            styles: styles);

        var run = Phase3DocxFixtureBuilder.GetFirstRun(paragraph);

        Assert.True(BodySectionDetector.IsBoldEffective(run, paragraph, doc.MainDocumentPart));
    }

    [Fact]
    public void IsBoldEffective_TwoLevelBasedOnChain_ReturnsTrue()
    {
        var paragraph = Phase3DocxFixtureBuilder.BuildParagraph("hello", styleId: "Ttulo");
        var styles = new[]
        {
            new Phase3DocxFixtureBuilder.StyleDefinition("Ttulo", BasedOn: "SemEspaamento"),
            new Phase3DocxFixtureBuilder.StyleDefinition("SemEspaamento", RunPropertiesBold: true),
        };
        using var doc = Phase3DocxFixtureBuilder.CreateDocument(
            paragraphs: new[] { paragraph },
            styles: styles);

        var run = Phase3DocxFixtureBuilder.GetFirstRun(paragraph);

        Assert.True(BodySectionDetector.IsBoldEffective(run, paragraph, doc.MainDocumentPart));
    }

    [Fact]
    public void IsBoldEffective_NoBoldAnywhere_ReturnsFalse()
    {
        var paragraph = Phase3DocxFixtureBuilder.BuildParagraph("hello", styleId: "Normal");
        var styles = new[]
        {
            new Phase3DocxFixtureBuilder.StyleDefinition("Normal"),
        };
        using var doc = Phase3DocxFixtureBuilder.CreateDocument(
            paragraphs: new[] { paragraph },
            styles: styles);

        var run = Phase3DocxFixtureBuilder.GetFirstRun(paragraph);

        Assert.False(BodySectionDetector.IsBoldEffective(run, paragraph, doc.MainDocumentPart));
    }

    [Fact]
    public void IsBoldEffective_CyclicBasedOnChain_ReturnsFalseWithoutHanging()
    {
        var paragraph = Phase3DocxFixtureBuilder.BuildParagraph("hello", styleId: "A");
        var styles = new[]
        {
            new Phase3DocxFixtureBuilder.StyleDefinition("A", BasedOn: "B"),
            new Phase3DocxFixtureBuilder.StyleDefinition("B", BasedOn: "A"),
        };
        using var doc = Phase3DocxFixtureBuilder.CreateDocument(
            paragraphs: new[] { paragraph },
            styles: styles);

        var run = Phase3DocxFixtureBuilder.GetFirstRun(paragraph);

        Assert.False(BodySectionDetector.IsBoldEffective(run, paragraph, doc.MainDocumentPart));
    }

    [Fact]
    public void IsBoldEffective_NullMainPart_ReturnsFalse()
    {
        var paragraph = Phase3DocxFixtureBuilder.BuildParagraph("hello", styleId: "Heading1");
        var run = Phase3DocxFixtureBuilder.GetFirstRun(paragraph);

        Assert.False(BodySectionDetector.IsBoldEffective(run, paragraph, mainPart: null));
    }

    [Fact]
    public void IsBoldEffective_StylesPartMissing_ReturnsFalse()
    {
        var paragraph = Phase3DocxFixtureBuilder.BuildParagraph("hello", styleId: "Heading1");
        using var doc = Phase3DocxFixtureBuilder.CreateDocument(
            paragraphs: new[] { paragraph },
            includeStylesPart: false);

        var run = Phase3DocxFixtureBuilder.GetFirstRun(paragraph);

        Assert.False(BodySectionDetector.IsBoldEffective(run, paragraph, doc.MainDocumentPart));
    }

    [Fact]
    public void IsBoldEffective_StyleIdNotInStylesPart_ReturnsFalse()
    {
        var paragraph = Phase3DocxFixtureBuilder.BuildParagraph("hello", styleId: "Missing");
        var styles = new[]
        {
            new Phase3DocxFixtureBuilder.StyleDefinition("Heading1", RunPropertiesBold: true),
        };
        using var doc = Phase3DocxFixtureBuilder.CreateDocument(
            paragraphs: new[] { paragraph },
            styles: styles);

        var run = Phase3DocxFixtureBuilder.GetFirstRun(paragraph);

        Assert.False(BodySectionDetector.IsBoldEffective(run, paragraph, doc.MainDocumentPart));
    }

    [Fact]
    public void IsBoldEffective_RunBoldVal0_ReturnsFalse()
    {
        var paragraph = new Paragraph();
        var runProperties = new RunProperties(new Bold { Val = OnOffValue.FromBoolean(false) });
        var run = new Run(
            runProperties,
            new Text("hello") { Space = SpaceProcessingModeValues.Preserve });
        paragraph.AppendChild(run);

        using var doc = Phase3DocxFixtureBuilder.CreateDocument(paragraphs: new[] { paragraph });

        Assert.False(BodySectionDetector.IsBoldEffective(run, paragraph, doc.MainDocumentPart));
    }

    [Fact]
    public void IsBoldEffective_RunBoldVal1_ReturnsTrue()
    {
        var paragraph = new Paragraph();
        var runProperties = new RunProperties(new Bold { Val = OnOffValue.FromBoolean(true) });
        var run = new Run(
            runProperties,
            new Text("hello") { Space = SpaceProcessingModeValues.Preserve });
        paragraph.AppendChild(run);

        using var doc = Phase3DocxFixtureBuilder.CreateDocument(paragraphs: new[] { paragraph });

        Assert.True(BodySectionDetector.IsBoldEffective(run, paragraph, doc.MainDocumentPart));
    }

    [Fact]
    public void IsBoldEffective_PathologicallyDeepChain_AbortsAfter10HopsAndReturnsFalse()
    {
        const int depth = 12;
        var styles = new List<Phase3DocxFixtureBuilder.StyleDefinition>();
        for (var i = 0; i < depth - 1; i++)
        {
            styles.Add(new Phase3DocxFixtureBuilder.StyleDefinition(
                $"S{i}",
                BasedOn: $"S{i + 1}"));
        }

        styles.Add(new Phase3DocxFixtureBuilder.StyleDefinition(
            $"S{depth - 1}",
            RunPropertiesBold: true));

        var paragraph = Phase3DocxFixtureBuilder.BuildParagraph("hello", styleId: "S0");
        using var doc = Phase3DocxFixtureBuilder.CreateDocument(
            paragraphs: new[] { paragraph },
            styles: styles);

        var run = Phase3DocxFixtureBuilder.GetFirstRun(paragraph);

        Assert.False(BodySectionDetector.IsBoldEffective(run, paragraph, doc.MainDocumentPart));
    }

    [Fact]
    public void IsBoldEffective_ChainOfExactly10HopsWithBoldAtTheEnd_ResolvesBold()
    {
        const int depth = 10;
        var styles = new List<Phase3DocxFixtureBuilder.StyleDefinition>();
        for (var i = 0; i < depth - 1; i++)
        {
            styles.Add(new Phase3DocxFixtureBuilder.StyleDefinition(
                $"S{i}",
                BasedOn: $"S{i + 1}"));
        }

        styles.Add(new Phase3DocxFixtureBuilder.StyleDefinition(
            $"S{depth - 1}",
            RunPropertiesBold: true));

        var paragraph = Phase3DocxFixtureBuilder.BuildParagraph("hello", styleId: "S0");
        using var doc = Phase3DocxFixtureBuilder.CreateDocument(
            paragraphs: new[] { paragraph },
            styles: styles);

        var run = Phase3DocxFixtureBuilder.GetFirstRun(paragraph);

        Assert.True(BodySectionDetector.IsBoldEffective(run, paragraph, doc.MainDocumentPart));
    }

    [Fact]
    public void IsBoldEffective_SingleLevelPStyleAcrossMultipleRuns_ResolvesBoldForEach()
    {
        var paragraph = Phase3DocxFixtureBuilder.BuildParagraph("INTRODUCTION", styleId: "Heading1");
        paragraph.AppendChild(
            new Run(new Text(" extra") { Space = SpaceProcessingModeValues.Preserve }));
        var styles = new[]
        {
            new Phase3DocxFixtureBuilder.StyleDefinition("Heading1", RunPropertiesBold: true),
        };
        using var doc = Phase3DocxFixtureBuilder.CreateDocument(
            paragraphs: new[] { paragraph },
            styles: styles);

        var runs = paragraph.Elements<Run>().ToList();

        Assert.Equal(2, runs.Count);
        foreach (var run in runs)
        {
            Assert.True(BodySectionDetector.IsBoldEffective(run, paragraph, doc.MainDocumentPart));
        }
    }

    [Fact]
    public void Skeleton_FindIntroductionAnchor_ReturnsNull()
    {
        using var doc = Phase3DocxFixtureBuilder.CreateDocument();
        var body = Phase3DocxFixtureBuilder.GetBody(doc);

        Assert.Null(BodySectionDetector.FindIntroductionAnchor(body, doc.MainDocumentPart));
    }

    [Fact]
    public void Skeleton_IsSection_ReturnsFalse()
    {
        var paragraph = Phase3DocxFixtureBuilder.BuildParagraph("HEADER");
        using var doc = Phase3DocxFixtureBuilder.CreateDocument(paragraphs: new[] { paragraph });

        Assert.False(BodySectionDetector.IsSection(paragraph, doc.MainDocumentPart));
    }

    [Fact]
    public void Skeleton_IsSubsection_ReturnsFalse()
    {
        var paragraph = Phase3DocxFixtureBuilder.BuildParagraph("Header");
        using var doc = Phase3DocxFixtureBuilder.CreateDocument(paragraphs: new[] { paragraph });

        Assert.False(BodySectionDetector.IsSubsection(paragraph, doc.MainDocumentPart));
    }

    [Fact]
    public void Skeleton_IsInsideTable_ReturnsFalse()
    {
        var paragraph = Phase3DocxFixtureBuilder.BuildParagraph("INTRODUCTION");

        Assert.False(BodySectionDetector.IsInsideTable(paragraph));
    }
}
