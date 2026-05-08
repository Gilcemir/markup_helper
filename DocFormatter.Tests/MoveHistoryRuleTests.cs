using DocFormatter.Core.Pipeline;
using DocFormatter.Core.Rules;
using DocFormatter.Tests.Fixtures.Phase3;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Xunit;

namespace DocFormatter.Tests;

public sealed class MoveHistoryRuleTests
{
    private static MoveHistoryRule CreateRule() => new();

    private static List<string> CollectBodyTexts(WordprocessingDocument doc)
    {
        return doc.MainDocumentPart!.Document!.Body!
            .Descendants<Text>()
            .Select(t => t.Text ?? string.Empty)
            .Select(s => s.Trim())
            .Where(s => s.Length > 0)
            .OrderBy(s => s, StringComparer.Ordinal)
            .ToList();
    }

    private static List<Paragraph> Paragraphs(WordprocessingDocument doc)
        => doc.MainDocumentPart!.Document!.Body!.Elements<Paragraph>().ToList();

    [Fact]
    public void Apply_WellFormedHistoryBlockBeforeIntro_MovesAndEmitsInfo()
    {
        var keywords = Phase3DocxFixtureBuilder.BuildParagraph("Keywords: maize, breeding");
        var received = Phase3DocxFixtureBuilder.BuildHistoryParagraph("Received", "2024-01-15");
        var accepted = Phase3DocxFixtureBuilder.BuildHistoryParagraph("Accepted", "2024-03-10");
        var published = Phase3DocxFixtureBuilder.BuildHistoryParagraph("Published", "2024-04-01");
        var betweenA = Phase3DocxFixtureBuilder.BuildParagraph("Between A");
        var betweenB = Phase3DocxFixtureBuilder.BuildParagraph("Between B");
        var intro = Phase3DocxFixtureBuilder.BuildIntroductionAnchorParagraph();
        var bodyP = Phase3DocxFixtureBuilder.BuildParagraph("Body content");

        using var doc = Phase3DocxFixtureBuilder.CreateDocument(paragraphs: new[]
        {
            keywords, received, accepted, published, betweenA, betweenB, intro, bodyP,
        });

        var before = CollectBodyTexts(doc);
        var report = new Report();

        CreateRule().Apply(doc, new FormattingContext(), report);

        var after = CollectBodyTexts(doc);
        Assert.Equal(before, after);

        var paragraphs = Paragraphs(doc);
        var introIndex = paragraphs.IndexOf(intro);
        Assert.True(introIndex >= 3);
        Assert.Same(received, paragraphs[introIndex - 3]);
        Assert.Same(accepted, paragraphs[introIndex - 2]);
        Assert.Same(published, paragraphs[introIndex - 1]);
        Assert.Equal(8, paragraphs.Count);

        var info = Assert.Single(report.Entries, e => e.Level == ReportLevel.Info);
        Assert.Equal(nameof(MoveHistoryRule), info.Rule);
        Assert.StartsWith(MoveHistoryRule.MovedMessagePrefix, info.Message);
        Assert.Contains(
            $"{MoveHistoryRule.MovedMessagePrefix}{introIndex}{MoveHistoryRule.MovedMessageOriginInfix}",
            info.Message);
        Assert.EndsWith(")", info.Message);
        Assert.DoesNotContain(report.Entries, e => e.Level >= ReportLevel.Warn);
    }

    [Fact]
    public void Apply_RunTwice_IsIdempotentAndEmitsAlreadyAdjacentOnSecondRun()
    {
        var keywords = Phase3DocxFixtureBuilder.BuildParagraph("Keywords: maize, breeding");
        var received = Phase3DocxFixtureBuilder.BuildHistoryParagraph("Received", "2024-01-15");
        var accepted = Phase3DocxFixtureBuilder.BuildHistoryParagraph("Accepted", "2024-03-10");
        var published = Phase3DocxFixtureBuilder.BuildHistoryParagraph("Published", "2024-04-01");
        var blank = Phase3DocxFixtureBuilder.BuildBlankParagraph();
        var intro = Phase3DocxFixtureBuilder.BuildIntroductionAnchorParagraph();

        using var doc = Phase3DocxFixtureBuilder.CreateDocument(paragraphs: new[]
        {
            keywords, received, accepted, published, blank, intro,
        });

        var beforeFirst = CollectBodyTexts(doc);
        var rule = CreateRule();

        rule.Apply(doc, new FormattingContext(), new Report());

        var afterFirstRun = CollectBodyTexts(doc);
        Assert.Equal(beforeFirst, afterFirstRun);

        var paragraphsAfterFirst = Paragraphs(doc).ToList();
        var introIndexAfterFirst = paragraphsAfterFirst.IndexOf(intro);
        Assert.Same(received, paragraphsAfterFirst[introIndexAfterFirst - 3]);
        Assert.Same(accepted, paragraphsAfterFirst[introIndexAfterFirst - 2]);
        Assert.Same(published, paragraphsAfterFirst[introIndexAfterFirst - 1]);

        var secondReport = new Report();
        rule.Apply(doc, new FormattingContext(), secondReport);

        var afterSecondRun = CollectBodyTexts(doc);
        Assert.Equal(beforeFirst, afterSecondRun);

        var paragraphsAfterSecond = Paragraphs(doc).ToList();
        Assert.Equal(paragraphsAfterFirst.Count, paragraphsAfterSecond.Count);
        for (var i = 0; i < paragraphsAfterFirst.Count; i++)
        {
            Assert.Same(paragraphsAfterFirst[i], paragraphsAfterSecond[i]);
        }

        var info = Assert.Single(secondReport.Entries, e => e.Level == ReportLevel.Info);
        Assert.Equal(MoveHistoryRule.AlreadyAdjacentMessage, info.Message);
        Assert.DoesNotContain(secondReport.Entries, e => e.Level >= ReportLevel.Warn);
    }

    [Fact]
    public void Apply_AlreadyAdjacent_NoMutationAndEmitsInfo()
    {
        var keywords = Phase3DocxFixtureBuilder.BuildParagraph("Keywords: maize, breeding");
        var received = Phase3DocxFixtureBuilder.BuildHistoryParagraph("Received", "2024-01-15");
        var accepted = Phase3DocxFixtureBuilder.BuildHistoryParagraph("Accepted", "2024-03-10");
        var published = Phase3DocxFixtureBuilder.BuildHistoryParagraph("Published", "2024-04-01");
        var intro = Phase3DocxFixtureBuilder.BuildIntroductionAnchorParagraph();

        using var doc = Phase3DocxFixtureBuilder.CreateDocument(paragraphs: new[]
        {
            keywords, received, accepted, published, intro,
        });

        var before = CollectBodyTexts(doc);
        var paragraphsBefore = Paragraphs(doc).ToList();

        var report = new Report();
        CreateRule().Apply(doc, new FormattingContext(), report);

        Assert.Equal(before, CollectBodyTexts(doc));

        var paragraphsAfter = Paragraphs(doc).ToList();
        Assert.Equal(paragraphsBefore.Count, paragraphsAfter.Count);
        for (var i = 0; i < paragraphsBefore.Count; i++)
        {
            Assert.Same(paragraphsBefore[i], paragraphsAfter[i]);
        }

        var info = Assert.Single(report.Entries, e => e.Level == ReportLevel.Info);
        Assert.Equal(MoveHistoryRule.AlreadyAdjacentMessage, info.Message);
        Assert.DoesNotContain(report.Entries, e => e.Level >= ReportLevel.Warn);
    }

    [Fact]
    public void Apply_AnchorMissing_EmitsWarnAndPreservesDocument()
    {
        var keywords = Phase3DocxFixtureBuilder.BuildParagraph("Keywords: maize, breeding");
        var received = Phase3DocxFixtureBuilder.BuildHistoryParagraph("Received", "2024-01-15");
        var accepted = Phase3DocxFixtureBuilder.BuildHistoryParagraph("Accepted", "2024-03-10");
        var published = Phase3DocxFixtureBuilder.BuildHistoryParagraph("Published", "2024-04-01");
        var bodyP = Phase3DocxFixtureBuilder.BuildParagraph("First section text");

        using var doc = Phase3DocxFixtureBuilder.CreateDocument(paragraphs: new[]
        {
            keywords, received, accepted, published, bodyP,
        });

        var before = CollectBodyTexts(doc);
        var paragraphsBefore = Paragraphs(doc).ToList();

        var report = new Report();
        CreateRule().Apply(doc, new FormattingContext(), report);

        Assert.Equal(before, CollectBodyTexts(doc));

        var paragraphsAfter = Paragraphs(doc).ToList();
        Assert.Equal(paragraphsBefore.Count, paragraphsAfter.Count);
        for (var i = 0; i < paragraphsBefore.Count; i++)
        {
            Assert.Same(paragraphsBefore[i], paragraphsAfter[i]);
        }

        var warn = Assert.Single(report.Entries, e => e.Level == ReportLevel.Warn);
        Assert.Equal(nameof(MoveHistoryRule), warn.Rule);
        Assert.Equal(MoveHistoryRule.AnchorMissingMessage, warn.Message);
    }

    [Fact]
    public void Apply_PartialBlock_EmitsWarnAndPreservesDocument()
    {
        var keywords = Phase3DocxFixtureBuilder.BuildParagraph("Keywords: maize, breeding");
        var received = Phase3DocxFixtureBuilder.BuildHistoryParagraph("Received", "2024-01-15");
        var published = Phase3DocxFixtureBuilder.BuildHistoryParagraph("Published", "2024-04-01");
        var intro = Phase3DocxFixtureBuilder.BuildIntroductionAnchorParagraph();

        using var doc = Phase3DocxFixtureBuilder.CreateDocument(paragraphs: new[]
        {
            keywords, received, published, intro,
        });

        var before = CollectBodyTexts(doc);
        var paragraphsBefore = Paragraphs(doc).ToList();

        var report = new Report();
        CreateRule().Apply(doc, new FormattingContext(), report);

        Assert.Equal(before, CollectBodyTexts(doc));

        var paragraphsAfter = Paragraphs(doc).ToList();
        Assert.Equal(paragraphsBefore.Count, paragraphsAfter.Count);
        for (var i = 0; i < paragraphsBefore.Count; i++)
        {
            Assert.Same(paragraphsBefore[i], paragraphsAfter[i]);
        }

        var warn = Assert.Single(report.Entries, e => e.Level == ReportLevel.Warn);
        Assert.StartsWith(MoveHistoryRule.PartialBlockMessagePrefix, warn.Message);
    }

    [Fact]
    public void Apply_OutOfOrder_EmitsWarnAndPreservesDocument()
    {
        var keywords = Phase3DocxFixtureBuilder.BuildParagraph("Keywords: maize, breeding");
        var published = Phase3DocxFixtureBuilder.BuildHistoryParagraph("Published", "2024-04-01");
        var received = Phase3DocxFixtureBuilder.BuildHistoryParagraph("Received", "2024-01-15");
        var accepted = Phase3DocxFixtureBuilder.BuildHistoryParagraph("Accepted", "2024-03-10");
        var intro = Phase3DocxFixtureBuilder.BuildIntroductionAnchorParagraph();

        using var doc = Phase3DocxFixtureBuilder.CreateDocument(paragraphs: new[]
        {
            keywords, published, received, accepted, intro,
        });

        var before = CollectBodyTexts(doc);
        var paragraphsBefore = Paragraphs(doc).ToList();

        var report = new Report();
        CreateRule().Apply(doc, new FormattingContext(), report);

        Assert.Equal(before, CollectBodyTexts(doc));

        var paragraphsAfter = Paragraphs(doc).ToList();
        Assert.Equal(paragraphsBefore.Count, paragraphsAfter.Count);
        for (var i = 0; i < paragraphsBefore.Count; i++)
        {
            Assert.Same(paragraphsBefore[i], paragraphsAfter[i]);
        }

        var warn = Assert.Single(report.Entries, e => e.Level == ReportLevel.Warn);
        Assert.StartsWith(MoveHistoryRule.OutOfOrderMessagePrefix, warn.Message);
    }

    [Fact]
    public void Apply_NotAdjacent_EmitsWarnAndPreservesDocument()
    {
        var keywords = Phase3DocxFixtureBuilder.BuildParagraph("Keywords: maize, breeding");
        var received = Phase3DocxFixtureBuilder.BuildHistoryParagraph("Received", "2024-01-15");
        var stray = Phase3DocxFixtureBuilder.BuildParagraph("Some unrelated affiliation line");
        var accepted = Phase3DocxFixtureBuilder.BuildHistoryParagraph("Accepted", "2024-03-10");
        var published = Phase3DocxFixtureBuilder.BuildHistoryParagraph("Published", "2024-04-01");
        var intro = Phase3DocxFixtureBuilder.BuildIntroductionAnchorParagraph();

        using var doc = Phase3DocxFixtureBuilder.CreateDocument(paragraphs: new[]
        {
            keywords, received, stray, accepted, published, intro,
        });

        var before = CollectBodyTexts(doc);
        var paragraphsBefore = Paragraphs(doc).ToList();

        var report = new Report();
        CreateRule().Apply(doc, new FormattingContext(), report);

        Assert.Equal(before, CollectBodyTexts(doc));

        var paragraphsAfter = Paragraphs(doc).ToList();
        Assert.Equal(paragraphsBefore.Count, paragraphsAfter.Count);
        for (var i = 0; i < paragraphsBefore.Count; i++)
        {
            Assert.Same(paragraphsBefore[i], paragraphsAfter[i]);
        }

        var warn = Assert.Single(report.Entries, e => e.Level == ReportLevel.Warn);
        Assert.StartsWith(MoveHistoryRule.NotAdjacentMessagePrefix, warn.Message);
    }

    [Fact]
    public void Apply_NotFound_EmitsInfoAndPreservesDocument()
    {
        var keywords = Phase3DocxFixtureBuilder.BuildParagraph("Keywords: maize, breeding");
        var intro = Phase3DocxFixtureBuilder.BuildIntroductionAnchorParagraph();
        var bodyP = Phase3DocxFixtureBuilder.BuildParagraph("Body content");

        using var doc = Phase3DocxFixtureBuilder.CreateDocument(paragraphs: new[]
        {
            keywords, intro, bodyP,
        });

        var before = CollectBodyTexts(doc);
        var paragraphsBefore = Paragraphs(doc).ToList();

        var report = new Report();
        CreateRule().Apply(doc, new FormattingContext(), report);

        Assert.Equal(before, CollectBodyTexts(doc));

        var paragraphsAfter = Paragraphs(doc).ToList();
        Assert.Equal(paragraphsBefore.Count, paragraphsAfter.Count);
        for (var i = 0; i < paragraphsBefore.Count; i++)
        {
            Assert.Same(paragraphsBefore[i], paragraphsAfter[i]);
        }

        var info = Assert.Single(report.Entries, e => e.Level == ReportLevel.Info);
        Assert.Equal(MoveHistoryRule.NotFoundMessage, info.Message);
        Assert.DoesNotContain(report.Entries, e => e.Level >= ReportLevel.Warn);
    }

    [Fact]
    public void Apply_HistoryMarkersAfterAnchor_AreIgnored()
    {
        var keywords = Phase3DocxFixtureBuilder.BuildParagraph("Keywords: maize, breeding");
        var intro = Phase3DocxFixtureBuilder.BuildIntroductionAnchorParagraph();
        var receivedAfter = Phase3DocxFixtureBuilder.BuildHistoryParagraph("Received", "2024-01-15");
        var acceptedAfter = Phase3DocxFixtureBuilder.BuildHistoryParagraph("Accepted", "2024-03-10");
        var publishedAfter = Phase3DocxFixtureBuilder.BuildHistoryParagraph("Published", "2024-04-01");

        using var doc = Phase3DocxFixtureBuilder.CreateDocument(paragraphs: new[]
        {
            keywords, intro, receivedAfter, acceptedAfter, publishedAfter,
        });

        var before = CollectBodyTexts(doc);
        var paragraphsBefore = Paragraphs(doc).ToList();

        var report = new Report();
        CreateRule().Apply(doc, new FormattingContext(), report);

        Assert.Equal(before, CollectBodyTexts(doc));

        var paragraphsAfter = Paragraphs(doc).ToList();
        Assert.Equal(paragraphsBefore.Count, paragraphsAfter.Count);
        for (var i = 0; i < paragraphsBefore.Count; i++)
        {
            Assert.Same(paragraphsBefore[i], paragraphsAfter[i]);
        }

        var info = Assert.Single(report.Entries, e => e.Level == ReportLevel.Info);
        Assert.Equal(MoveHistoryRule.NotFoundMessage, info.Message);
        Assert.DoesNotContain(report.Entries, e => e.Level >= ReportLevel.Warn);
    }

    [Theory]
    [InlineData("received: 2024-01-15", "accepted: 2024-03-10", "published: 2024-04-01")]
    [InlineData("RECEIVED: 2024-01-15", "ACCEPTED: 2024-03-10", "PUBLISHED: 2024-04-01")]
    [InlineData("Received - 2024-01-15", "Accepted - 2024-03-10", "Published - 2024-04-01")]
    [InlineData("Received – 2024-01-15", "Accepted – 2024-03-10", "Published – 2024-04-01")]
    [InlineData("Received — 2024-01-15", "Accepted — 2024-03-10", "Published — 2024-04-01")]
    public void Apply_RegexCaseInsensitivityAndSeparators_AllVariantsMatch(
        string receivedText,
        string acceptedText,
        string publishedText)
    {
        var keywords = Phase3DocxFixtureBuilder.BuildParagraph("Keywords: maize, breeding");
        var received = Phase3DocxFixtureBuilder.BuildParagraph(receivedText);
        var accepted = Phase3DocxFixtureBuilder.BuildParagraph(acceptedText);
        var published = Phase3DocxFixtureBuilder.BuildParagraph(publishedText);
        var separator = Phase3DocxFixtureBuilder.BuildParagraph("Some affiliation");
        var intro = Phase3DocxFixtureBuilder.BuildIntroductionAnchorParagraph();

        using var doc = Phase3DocxFixtureBuilder.CreateDocument(paragraphs: new[]
        {
            keywords, received, accepted, published, separator, intro,
        });

        var before = CollectBodyTexts(doc);
        var report = new Report();

        CreateRule().Apply(doc, new FormattingContext(), report);

        Assert.Equal(before, CollectBodyTexts(doc));

        var paragraphs = Paragraphs(doc);
        var introIndex = paragraphs.IndexOf(intro);
        Assert.Same(received, paragraphs[introIndex - 3]);
        Assert.Same(accepted, paragraphs[introIndex - 2]);
        Assert.Same(published, paragraphs[introIndex - 1]);

        var info = Assert.Single(report.Entries, e => e.Level == ReportLevel.Info);
        Assert.StartsWith(MoveHistoryRule.MovedMessagePrefix, info.Message);
        Assert.DoesNotContain(report.Entries, e => e.Level >= ReportLevel.Warn);
    }

    [Fact]
    public void Apply_RegexRejectsParagraphWithoutSeparator()
    {
        var keywords = Phase3DocxFixtureBuilder.BuildParagraph("Keywords: maize, breeding");
        var bareReceived = Phase3DocxFixtureBuilder.BuildParagraph("Received 2024-01-15");
        var intro = Phase3DocxFixtureBuilder.BuildIntroductionAnchorParagraph();

        using var doc = Phase3DocxFixtureBuilder.CreateDocument(paragraphs: new[]
        {
            keywords, bareReceived, intro,
        });

        var before = CollectBodyTexts(doc);
        var paragraphsBefore = Paragraphs(doc).ToList();

        var report = new Report();
        CreateRule().Apply(doc, new FormattingContext(), report);

        Assert.Equal(before, CollectBodyTexts(doc));

        var paragraphsAfter = Paragraphs(doc).ToList();
        Assert.Equal(paragraphsBefore.Count, paragraphsAfter.Count);
        for (var i = 0; i < paragraphsBefore.Count; i++)
        {
            Assert.Same(paragraphsBefore[i], paragraphsAfter[i]);
        }

        var info = Assert.Single(report.Entries, e => e.Level == ReportLevel.Info);
        Assert.Equal(MoveHistoryRule.NotFoundMessage, info.Message);
    }

    [Fact]
    public void Apply_PreservesParagraphProperties()
    {
        var keywords = Phase3DocxFixtureBuilder.BuildParagraph("Keywords: maize, breeding");
        var received = Phase3DocxFixtureBuilder.BuildHistoryParagraph(
            "Received",
            "2024-01-15",
            alignment: JustificationValues.Right);
        var accepted = Phase3DocxFixtureBuilder.BuildHistoryParagraph(
            "Accepted",
            "2024-03-10",
            alignment: JustificationValues.Center);
        var published = Phase3DocxFixtureBuilder.BuildHistoryParagraph(
            "Published",
            "2024-04-01",
            alignment: JustificationValues.Left);
        var intro = Phase3DocxFixtureBuilder.BuildIntroductionAnchorParagraph();

        using var doc = Phase3DocxFixtureBuilder.CreateDocument(paragraphs: new[]
        {
            keywords, received, accepted, published, intro,
        });

        var receivedPropsBefore = received.ParagraphProperties?.OuterXml ?? string.Empty;
        var acceptedPropsBefore = accepted.ParagraphProperties?.OuterXml ?? string.Empty;
        var publishedPropsBefore = published.ParagraphProperties?.OuterXml ?? string.Empty;

        var before = CollectBodyTexts(doc);
        var report = new Report();
        CreateRule().Apply(doc, new FormattingContext(), report);

        Assert.Equal(before, CollectBodyTexts(doc));
        Assert.Equal(receivedPropsBefore, received.ParagraphProperties?.OuterXml ?? string.Empty);
        Assert.Equal(acceptedPropsBefore, accepted.ParagraphProperties?.OuterXml ?? string.Empty);
        Assert.Equal(publishedPropsBefore, published.ParagraphProperties?.OuterXml ?? string.Empty);
    }

    [Fact]
    public void Apply_IntegrationWithPhase1And2FrontMatter_PlacesHistoryBeforeIntroduction()
    {
        var doiParagraph = Phase3DocxFixtureBuilder.BuildParagraph("DOI: https://doi.org/10.1234/example");
        var titleParagraph = Phase3DocxFixtureBuilder.BuildParagraph(
            "Sample Article Title for Testing",
            runDirectBold: true,
            alignment: JustificationValues.Center);
        var authorsParagraph = Phase3DocxFixtureBuilder.BuildParagraph("Maria Silva1, João Santos2");
        var affiliationParagraph = Phase3DocxFixtureBuilder.BuildParagraph("1 University of Example, 2 Institute");
        var correspondingParagraph = Phase3DocxFixtureBuilder.BuildParagraph(
            "Corresponding author: maria@example.com");
        var abstractHeading = Phase3DocxFixtureBuilder.BuildParagraph(
            "Abstract",
            runDirectBold: true);
        var abstractBody = Phase3DocxFixtureBuilder.BuildParagraph(
            "This study evaluates a novel approach to crop breeding.");
        var received = Phase3DocxFixtureBuilder.BuildHistoryParagraph("Received", "2024-01-15");
        var accepted = Phase3DocxFixtureBuilder.BuildHistoryParagraph("Accepted", "2024-03-10");
        var published = Phase3DocxFixtureBuilder.BuildHistoryParagraph("Published", "2024-04-01");
        var keywords = Phase3DocxFixtureBuilder.BuildParagraph("Keywords: maize, breeding, genetics");
        var intro = Phase3DocxFixtureBuilder.BuildIntroductionAnchorParagraph();
        var bodyContent = Phase3DocxFixtureBuilder.BuildParagraph(
            "The introduction section describes prior work and motivation.");
        var methodsHeading = Phase3DocxFixtureBuilder.BuildParagraph(
            "MATERIAL AND METHODS",
            runDirectBold: true);
        var methodsBody = Phase3DocxFixtureBuilder.BuildParagraph("Methods text.");

        using var doc = Phase3DocxFixtureBuilder.CreateDocument(paragraphs: new[]
        {
            doiParagraph, titleParagraph, authorsParagraph, affiliationParagraph,
            correspondingParagraph, abstractHeading, abstractBody,
            received, accepted, published,
            keywords, intro, bodyContent, methodsHeading, methodsBody,
        });

        var before = CollectBodyTexts(doc);
        var report = new Report();

        CreateRule().Apply(doc, new FormattingContext(), report);

        var after = CollectBodyTexts(doc);
        Assert.Equal(before, after);

        var paragraphs = Paragraphs(doc);
        var introIndex = paragraphs.IndexOf(intro);
        Assert.Same(received, paragraphs[introIndex - 3]);
        Assert.Same(accepted, paragraphs[introIndex - 2]);
        Assert.Same(published, paragraphs[introIndex - 1]);

        Assert.Same(doiParagraph, paragraphs[0]);
        Assert.Same(titleParagraph, paragraphs[1]);
        Assert.Same(authorsParagraph, paragraphs[2]);
        Assert.Same(affiliationParagraph, paragraphs[3]);
        Assert.Same(correspondingParagraph, paragraphs[4]);
        Assert.Same(abstractHeading, paragraphs[5]);
        Assert.Same(abstractBody, paragraphs[6]);
        Assert.Same(keywords, paragraphs[7]);
        Assert.Same(intro, paragraphs[introIndex]);
        Assert.Same(bodyContent, paragraphs[introIndex + 1]);
        Assert.Same(methodsHeading, paragraphs[introIndex + 2]);
        Assert.Same(methodsBody, paragraphs[introIndex + 3]);

        var info = Assert.Single(report.Entries, e => e.Level == ReportLevel.Info);
        Assert.Equal(nameof(MoveHistoryRule), info.Rule);
        Assert.StartsWith(MoveHistoryRule.MovedMessagePrefix, info.Message);
        Assert.Contains(
            $"{MoveHistoryRule.MovedMessagePrefix}{introIndex}{MoveHistoryRule.MovedMessageOriginInfix}",
            info.Message);
        Assert.EndsWith(")", info.Message);
        Assert.DoesNotContain(report.Entries, e => e.Level >= ReportLevel.Warn);
    }
}
