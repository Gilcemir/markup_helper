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
    public void IsBoldEffective_ParagraphMarkBoldButRunNotBold_ReturnsFalse()
    {
        // pPr/rPr/b formats only the paragraph mark (the pilcrow); Word does not
        // cascade it to runs without their own <w:b/>. Detection must mirror that.
        var paragraph = Phase3DocxFixtureBuilder.BuildParagraph(
            "hello",
            paragraphMarkBold: true);
        using var doc = Phase3DocxFixtureBuilder.CreateDocument(paragraphs: new[] { paragraph });

        var run = Phase3DocxFixtureBuilder.GetFirstRun(paragraph);

        Assert.False(BodySectionDetector.IsBoldEffective(run, paragraph, doc.MainDocumentPart));
    }

    [Fact]
    public void IsBoldEffective_ParagraphMarkBoldValFalseAndRunNotBold_ReturnsFalse()
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
    public void IsSubsection_ParagraphMarkBoldButRunsNotBold_ReturnsFalse()
    {
        // Regression: prior versions treated pPr/rPr/b as if it cascaded to runs,
        // wrongly classifying body paragraphs/references as sub-sections.
        var paragraph = Phase3DocxFixtureBuilder.BuildParagraph(
            "Although participatory plant breeding has been successfully applied",
            paragraphMarkBold: true,
            alignment: JustificationValues.Both);
        using var doc = Phase3DocxFixtureBuilder.CreateDocument(paragraphs: new[] { paragraph });

        Assert.False(BodySectionDetector.IsSubsection(paragraph, doc.MainDocumentPart));
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
    public void IsInsideTable_ParagraphInSingleTable_ReturnsTrue()
    {
        var paragraph = Phase3DocxFixtureBuilder.BuildParagraph("INTRODUCTION");
        var table = Phase3DocxFixtureBuilder.WrapInTable(paragraph);
        using var doc = Phase3DocxFixtureBuilder.CreateDocument(bodyElements: new OpenXmlElement[] { table });

        Assert.True(BodySectionDetector.IsInsideTable(paragraph));
    }

    [Fact]
    public void IsInsideTable_ParagraphInDeeplyNestedTable_ReturnsTrue()
    {
        var paragraph = Phase3DocxFixtureBuilder.BuildParagraph("INTRODUCTION");
        var nestedTable = Phase3DocxFixtureBuilder.WrapInNestedTable(depth: 3, paragraph);
        using var doc = Phase3DocxFixtureBuilder.CreateDocument(bodyElements: new OpenXmlElement[] { nestedTable });

        Assert.True(BodySectionDetector.IsInsideTable(paragraph));
    }

    [Fact]
    public void IsInsideTable_TopLevelBodyParagraph_ReturnsFalse()
    {
        var paragraph = Phase3DocxFixtureBuilder.BuildParagraph("INTRODUCTION");
        using var doc = Phase3DocxFixtureBuilder.CreateDocument(paragraphs: new[] { paragraph });

        Assert.False(BodySectionDetector.IsInsideTable(paragraph));
    }

    [Fact]
    public void IsSection_BoldAllCapsLeftAligned_ReturnsTrue()
    {
        var paragraph = Phase3DocxFixtureBuilder.BuildParagraph(
            "INTRODUCTION",
            runDirectBold: true,
            alignment: JustificationValues.Left);
        using var doc = Phase3DocxFixtureBuilder.CreateDocument(paragraphs: new[] { paragraph });

        Assert.True(BodySectionDetector.IsSection(paragraph, doc.MainDocumentPart));
    }

    [Fact]
    public void IsSection_BoldAllCapsBothAlignment_ReturnsTrue()
    {
        var paragraph = Phase3DocxFixtureBuilder.BuildParagraph(
            "MATERIAL AND METHODS",
            runDirectBold: true,
            alignment: JustificationValues.Both);
        using var doc = Phase3DocxFixtureBuilder.CreateDocument(paragraphs: new[] { paragraph });

        Assert.True(BodySectionDetector.IsSection(paragraph, doc.MainDocumentPart));
    }

    [Fact]
    public void IsSection_BoldAllCapsNoAlignment_ReturnsTrue()
    {
        var paragraph = Phase3DocxFixtureBuilder.BuildParagraph(
            "RESULTS",
            runDirectBold: true);
        using var doc = Phase3DocxFixtureBuilder.CreateDocument(paragraphs: new[] { paragraph });

        Assert.True(BodySectionDetector.IsSection(paragraph, doc.MainDocumentPart));
    }

    [Fact]
    public void IsSection_CenterAlignment_ReturnsFalse()
    {
        var paragraph = Phase3DocxFixtureBuilder.BuildParagraph(
            "RESULTS",
            runDirectBold: true,
            alignment: JustificationValues.Center);
        using var doc = Phase3DocxFixtureBuilder.CreateDocument(paragraphs: new[] { paragraph });

        Assert.False(BodySectionDetector.IsSection(paragraph, doc.MainDocumentPart));
    }

    [Fact]
    public void IsSection_RightAlignment_ReturnsFalse()
    {
        var paragraph = Phase3DocxFixtureBuilder.BuildParagraph(
            "ARTICLE",
            runDirectBold: true,
            alignment: JustificationValues.Right);
        using var doc = Phase3DocxFixtureBuilder.CreateDocument(paragraphs: new[] { paragraph });

        Assert.False(BodySectionDetector.IsSection(paragraph, doc.MainDocumentPart));
    }

    [Fact]
    public void IsSection_TwoCharacterText_ReturnsFalse()
    {
        var paragraph = Phase3DocxFixtureBuilder.BuildParagraph(
            "AB",
            runDirectBold: true,
            alignment: JustificationValues.Left);
        using var doc = Phase3DocxFixtureBuilder.CreateDocument(paragraphs: new[] { paragraph });

        Assert.False(BodySectionDetector.IsSection(paragraph, doc.MainDocumentPart));
    }

    [Fact]
    public void IsSection_DigitsOnly_ReturnsFalse()
    {
        var paragraph = Phase3DocxFixtureBuilder.BuildParagraph(
            "12345",
            runDirectBold: true,
            alignment: JustificationValues.Left);
        using var doc = Phase3DocxFixtureBuilder.CreateDocument(paragraphs: new[] { paragraph });

        Assert.False(BodySectionDetector.IsSection(paragraph, doc.MainDocumentPart));
    }

    [Fact]
    public void IsSection_BoldRatioBelowNinetyPercent_ReturnsFalse()
    {
        var runs = new[]
        {
            new Phase3DocxFixtureBuilder.RunSpec("HEADER", Bold: true),
            new Phase3DocxFixtureBuilder.RunSpec("HEADER", Bold: false),
        };
        var paragraph = Phase3DocxFixtureBuilder.BuildParagraphWithRuns(
            runs,
            alignment: JustificationValues.Left);
        using var doc = Phase3DocxFixtureBuilder.CreateDocument(paragraphs: new[] { paragraph });

        Assert.False(BodySectionDetector.IsSection(paragraph, doc.MainDocumentPart));
    }

    [Fact]
    public void IsSection_BoldRatioExactlyNinetyPercent_ReturnsTrue()
    {
        var runs = new[]
        {
            new Phase3DocxFixtureBuilder.RunSpec("HEADERVAL", Bold: true),
            new Phase3DocxFixtureBuilder.RunSpec("X", Bold: false),
        };
        var paragraph = Phase3DocxFixtureBuilder.BuildParagraphWithRuns(
            runs,
            alignment: JustificationValues.Left);
        using var doc = Phase3DocxFixtureBuilder.CreateDocument(paragraphs: new[] { paragraph });

        Assert.True(BodySectionDetector.IsSection(paragraph, doc.MainDocumentPart));
    }

    [Fact]
    public void IsSection_MixedCaseText_ReturnsFalse()
    {
        var paragraph = Phase3DocxFixtureBuilder.BuildParagraph(
            "Header Mixed Case",
            runDirectBold: true,
            alignment: JustificationValues.Left);
        using var doc = Phase3DocxFixtureBuilder.CreateDocument(paragraphs: new[] { paragraph });

        Assert.False(BodySectionDetector.IsSection(paragraph, doc.MainDocumentPart));
    }

    [Fact]
    public void IsSection_NonBoldWhitespaceOnlyRun_DoesNotAffectRatio()
    {
        var runs = new[]
        {
            new Phase3DocxFixtureBuilder.RunSpec("HEADER", Bold: true),
            new Phase3DocxFixtureBuilder.RunSpec("   ", Bold: false),
        };
        var paragraph = Phase3DocxFixtureBuilder.BuildParagraphWithRuns(
            runs,
            alignment: JustificationValues.Left);
        using var doc = Phase3DocxFixtureBuilder.CreateDocument(paragraphs: new[] { paragraph });

        Assert.True(BodySectionDetector.IsSection(paragraph, doc.MainDocumentPart));
    }

    [Fact]
    public void IsSubsection_BoldMixedCaseLeftAligned_ReturnsTrue()
    {
        var paragraph = Phase3DocxFixtureBuilder.BuildParagraph(
            "Sample preparation",
            runDirectBold: true,
            alignment: JustificationValues.Left);
        using var doc = Phase3DocxFixtureBuilder.CreateDocument(paragraphs: new[] { paragraph });

        Assert.True(BodySectionDetector.IsSubsection(paragraph, doc.MainDocumentPart));
    }

    [Fact]
    public void IsSubsection_AllCapsParagraphAlsoIsSection_ReturnsFalse()
    {
        var paragraph = Phase3DocxFixtureBuilder.BuildParagraph(
            "RESULTS",
            runDirectBold: true,
            alignment: JustificationValues.Left);
        using var doc = Phase3DocxFixtureBuilder.CreateDocument(paragraphs: new[] { paragraph });

        Assert.True(BodySectionDetector.IsSection(paragraph, doc.MainDocumentPart));
        Assert.False(BodySectionDetector.IsSubsection(paragraph, doc.MainDocumentPart));
    }

    [Fact]
    public void IsSubsection_NoLowerCase_ReturnsFalse()
    {
        var paragraph = Phase3DocxFixtureBuilder.BuildParagraph(
            "INTRODUCTION",
            runDirectBold: true,
            alignment: JustificationValues.Left);
        using var doc = Phase3DocxFixtureBuilder.CreateDocument(paragraphs: new[] { paragraph });

        Assert.False(BodySectionDetector.IsSubsection(paragraph, doc.MainDocumentPart));
    }

    [Fact]
    public void FindIntroductionAnchor_ExactMatch_ReturnsParagraph()
    {
        var anchor = Phase3DocxFixtureBuilder.BuildIntroductionAnchorParagraph();
        using var doc = Phase3DocxFixtureBuilder.CreateDocument(paragraphs: new[] { anchor });
        var body = Phase3DocxFixtureBuilder.GetBody(doc);

        Assert.Same(anchor, BodySectionDetector.FindIntroductionAnchor(body, doc.MainDocumentPart));
    }

    [Fact]
    public void FindIntroductionAnchor_AcceptsTrailingColon_ReturnsParagraph()
    {
        var anchor = Phase3DocxFixtureBuilder.BuildIntroductionAnchorParagraph("INTRODUCTION:");
        using var doc = Phase3DocxFixtureBuilder.CreateDocument(paragraphs: new[] { anchor });
        var body = Phase3DocxFixtureBuilder.GetBody(doc);

        Assert.Same(anchor, BodySectionDetector.FindIntroductionAnchor(body, doc.MainDocumentPart));
    }

    [Fact]
    public void FindIntroductionAnchor_AcceptsTrailingPeriod_ReturnsParagraph()
    {
        var anchor = Phase3DocxFixtureBuilder.BuildIntroductionAnchorParagraph("INTRODUCTION.");
        using var doc = Phase3DocxFixtureBuilder.CreateDocument(paragraphs: new[] { anchor });
        var body = Phase3DocxFixtureBuilder.GetBody(doc);

        Assert.Same(anchor, BodySectionDetector.FindIntroductionAnchor(body, doc.MainDocumentPart));
    }

    [Fact]
    public void FindIntroductionAnchor_AcceptsTrailingWhitespace_ReturnsParagraph()
    {
        var anchor = Phase3DocxFixtureBuilder.BuildIntroductionAnchorParagraph("INTRODUCTION ");
        using var doc = Phase3DocxFixtureBuilder.CreateDocument(paragraphs: new[] { anchor });
        var body = Phase3DocxFixtureBuilder.GetBody(doc);

        Assert.Same(anchor, BodySectionDetector.FindIntroductionAnchor(body, doc.MainDocumentPart));
    }

    [Fact]
    public void FindIntroductionAnchor_RejectsExtraText_ReturnsNull()
    {
        var paragraph = Phase3DocxFixtureBuilder.BuildIntroductionAnchorParagraph("INTRODUCTION bla bla");
        using var doc = Phase3DocxFixtureBuilder.CreateDocument(paragraphs: new[] { paragraph });
        var body = Phase3DocxFixtureBuilder.GetBody(doc);

        Assert.Null(BodySectionDetector.FindIntroductionAnchor(body, doc.MainDocumentPart));
    }

    [Fact]
    public void FindIntroductionAnchor_RejectsPortuguese_ReturnsNull()
    {
        var paragraph = Phase3DocxFixtureBuilder.BuildIntroductionAnchorParagraph("INTRODUÇÃO");
        using var doc = Phase3DocxFixtureBuilder.CreateDocument(paragraphs: new[] { paragraph });
        var body = Phase3DocxFixtureBuilder.GetBody(doc);

        Assert.Null(BodySectionDetector.FindIntroductionAnchor(body, doc.MainDocumentPart));
    }

    [Fact]
    public void FindIntroductionAnchor_RejectsNumberedPrefix_ReturnsNull()
    {
        var paragraph = Phase3DocxFixtureBuilder.BuildIntroductionAnchorParagraph("1. INTRODUCTION");
        using var doc = Phase3DocxFixtureBuilder.CreateDocument(paragraphs: new[] { paragraph });
        var body = Phase3DocxFixtureBuilder.GetBody(doc);

        Assert.Null(BodySectionDetector.FindIntroductionAnchor(body, doc.MainDocumentPart));
    }

    [Fact]
    public void FindIntroductionAnchor_RejectsLowercase_ReturnsNull()
    {
        var paragraph = Phase3DocxFixtureBuilder.BuildIntroductionAnchorParagraph("Introduction");
        using var doc = Phase3DocxFixtureBuilder.CreateDocument(paragraphs: new[] { paragraph });
        var body = Phase3DocxFixtureBuilder.GetBody(doc);

        Assert.Null(BodySectionDetector.FindIntroductionAnchor(body, doc.MainDocumentPart));
    }

    [Fact]
    public void FindIntroductionAnchor_MatchesRegexButNotBold_ReturnsNull()
    {
        var paragraph = Phase3DocxFixtureBuilder.BuildParagraph(
            "INTRODUCTION",
            alignment: JustificationValues.Left);
        using var doc = Phase3DocxFixtureBuilder.CreateDocument(paragraphs: new[] { paragraph });
        var body = Phase3DocxFixtureBuilder.GetBody(doc);

        Assert.Null(BodySectionDetector.FindIntroductionAnchor(body, doc.MainDocumentPart));
    }

    [Fact]
    public void FindIntroductionAnchor_MultipleQualifying_ReturnsFirst()
    {
        var first = Phase3DocxFixtureBuilder.BuildIntroductionAnchorParagraph();
        var second = Phase3DocxFixtureBuilder.BuildIntroductionAnchorParagraph("INTRODUCTION:");
        using var doc = Phase3DocxFixtureBuilder.CreateDocument(paragraphs: new[] { first, second });
        var body = Phase3DocxFixtureBuilder.GetBody(doc);

        Assert.Same(first, BodySectionDetector.FindIntroductionAnchor(body, doc.MainDocumentPart));
    }

    [Fact]
    public void FindIntroductionAnchor_NoMatch_ReturnsNull()
    {
        var paragraph = Phase3DocxFixtureBuilder.BuildParagraph(
            "RESULTS",
            runDirectBold: true,
            alignment: JustificationValues.Left);
        using var doc = Phase3DocxFixtureBuilder.CreateDocument(paragraphs: new[] { paragraph });
        var body = Phase3DocxFixtureBuilder.GetBody(doc);

        Assert.Null(BodySectionDetector.FindIntroductionAnchor(body, doc.MainDocumentPart));
    }

    [Fact]
    public void FindIntroductionAnchor_OnlyInsideTable_ReturnsNull()
    {
        var insideTable = Phase3DocxFixtureBuilder.BuildIntroductionAnchorParagraph();
        var table = Phase3DocxFixtureBuilder.WrapInTable(insideTable);
        using var doc = Phase3DocxFixtureBuilder.CreateDocument(bodyElements: new OpenXmlElement[] { table });
        var body = Phase3DocxFixtureBuilder.GetBody(doc);

        Assert.Null(BodySectionDetector.FindIntroductionAnchor(body, doc.MainDocumentPart));
    }

    [Fact]
    public void FindIntroductionAnchor_PrefersBodyParagraphOverTableParagraph()
    {
        var insideTable = Phase3DocxFixtureBuilder.BuildIntroductionAnchorParagraph();
        var table = Phase3DocxFixtureBuilder.WrapInTable(insideTable);
        var bodyAnchor = Phase3DocxFixtureBuilder.BuildIntroductionAnchorParagraph();
        using var doc = Phase3DocxFixtureBuilder.CreateDocument(
            bodyElements: new OpenXmlElement[] { table, bodyAnchor });
        var body = Phase3DocxFixtureBuilder.GetBody(doc);

        Assert.Same(bodyAnchor, BodySectionDetector.FindIntroductionAnchor(body, doc.MainDocumentPart));
    }

    [Fact]
    public void FindIntroductionAnchor_EmptyBody_ReturnsNull()
    {
        using var doc = Phase3DocxFixtureBuilder.CreateDocument();
        var body = Phase3DocxFixtureBuilder.GetBody(doc);

        Assert.Null(BodySectionDetector.FindIntroductionAnchor(body, doc.MainDocumentPart));
    }

    [Fact]
    public void Predicates_ComposeOnFullSyntheticDocument_ClassifyEachParagraphCorrectly()
    {
        var articleLabel = Phase3DocxFixtureBuilder.BuildParagraph(
            "ARTICLE",
            runDirectBold: true,
            alignment: JustificationValues.Right);
        var title = Phase3DocxFixtureBuilder.BuildParagraph(
            "Effects of Drought on Sample",
            runDirectBold: true,
            alignment: JustificationValues.Center);
        var abstractHeading = Phase3DocxFixtureBuilder.BuildParagraph(
            "Abstract",
            runDirectBold: true,
            alignment: JustificationValues.Left);
        var introduction = Phase3DocxFixtureBuilder.BuildIntroductionAnchorParagraph(
            alignment: JustificationValues.Left);
        var materialAndMethods = Phase3DocxFixtureBuilder.BuildParagraph(
            "MATERIAL AND METHODS",
            runDirectBold: true,
            alignment: JustificationValues.Left);
        var samplePrep = Phase3DocxFixtureBuilder.BuildParagraph(
            "Sample preparation",
            runDirectBold: true,
            alignment: JustificationValues.Left);
        var results = Phase3DocxFixtureBuilder.BuildParagraph(
            "RESULTS",
            runDirectBold: true,
            alignment: JustificationValues.Both);
        var tableParagraph = Phase3DocxFixtureBuilder.BuildParagraph(
            "RESULTS",
            runDirectBold: true,
            alignment: JustificationValues.Left);
        var table = Phase3DocxFixtureBuilder.WrapInTable(tableParagraph);

        using var doc = Phase3DocxFixtureBuilder.CreateDocument(
            bodyElements: new OpenXmlElement[]
            {
                articleLabel,
                title,
                abstractHeading,
                introduction,
                materialAndMethods,
                samplePrep,
                results,
                table,
            });
        var body = Phase3DocxFixtureBuilder.GetBody(doc);
        var mainPart = doc.MainDocumentPart;

        Assert.Same(introduction, BodySectionDetector.FindIntroductionAnchor(body, mainPart));

        Assert.False(BodySectionDetector.IsSection(articleLabel, mainPart));
        Assert.False(BodySectionDetector.IsSubsection(articleLabel, mainPart));

        Assert.False(BodySectionDetector.IsSection(title, mainPart));
        Assert.False(BodySectionDetector.IsSubsection(title, mainPart));

        Assert.False(BodySectionDetector.IsSection(abstractHeading, mainPart));
        Assert.True(BodySectionDetector.IsSubsection(abstractHeading, mainPart));

        Assert.True(BodySectionDetector.IsSection(introduction, mainPart));
        Assert.False(BodySectionDetector.IsSubsection(introduction, mainPart));

        Assert.True(BodySectionDetector.IsSection(materialAndMethods, mainPart));
        Assert.False(BodySectionDetector.IsSubsection(materialAndMethods, mainPart));

        Assert.False(BodySectionDetector.IsSection(samplePrep, mainPart));
        Assert.True(BodySectionDetector.IsSubsection(samplePrep, mainPart));

        Assert.True(BodySectionDetector.IsSection(results, mainPart));
        Assert.False(BodySectionDetector.IsSubsection(results, mainPart));

        Assert.True(BodySectionDetector.IsInsideTable(tableParagraph));
        Assert.False(BodySectionDetector.IsInsideTable(introduction));
    }
}
