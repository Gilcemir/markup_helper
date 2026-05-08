using DocFormatter.Core.Pipeline;
using DocFormatter.Core.Rules;
using DocFormatter.Tests.Fixtures.Phase3;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Xunit;

namespace DocFormatter.Tests;

public sealed class PromoteSectionsRuleTests
{
    private static PromoteSectionsRule CreateRule() => new();

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

    private static void AssertContentPreserved(List<string> before, WordprocessingDocument doc)
    {
        Assert.Equal(before, CollectBodyTexts(doc));
    }

    private static bool IsCenterJustified(Paragraph p)
    {
        var value = p.ParagraphProperties?.Justification?.Val;
        return value is not null && value.HasValue && value.Value == JustificationValues.Center;
    }

    private static JustificationValues? GetJustification(Paragraph p)
    {
        var value = p.ParagraphProperties?.Justification?.Val;
        if (value is null || !value.HasValue)
        {
            return null;
        }

        return value.Value;
    }

    private static (string? sz, string? szCs) GetRunFontSizes(Run run)
        => (run.RunProperties?.FontSize?.Val?.Value,
            run.RunProperties?.FontSizeComplexScript?.Val?.Value);

    [Fact]
    public void Apply_SectionParagraphAfterAnchor_GetsCenterAlignmentAndSize32()
    {
        var intro = Phase3DocxFixtureBuilder.BuildIntroductionAnchorParagraph(
            alignment: JustificationValues.Left);
        var bodyText = Phase3DocxFixtureBuilder.BuildParagraph("Body intro text.");
        var methods = Phase3DocxFixtureBuilder.BuildSectionParagraph(
            "MATERIAL AND METHODS",
            alignment: JustificationValues.Left);

        using var doc = Phase3DocxFixtureBuilder.CreateDocument(paragraphs: new[]
        {
            intro, bodyText, methods,
        });

        var before = CollectBodyTexts(doc);
        var report = new Report();

        CreateRule().Apply(doc, new FormattingContext(), report);

        AssertContentPreserved(before, doc);
        Assert.True(IsCenterJustified(methods));
        var run = methods.Elements<Run>().Single();
        var (sz, szCs) = GetRunFontSizes(run);
        Assert.Equal("32", sz);
        Assert.Equal("32", szCs);
    }

    [Fact]
    public void Apply_SubsectionParagraphAfterAnchor_GetsCenterAlignmentAndSize28()
    {
        var intro = Phase3DocxFixtureBuilder.BuildIntroductionAnchorParagraph(
            alignment: JustificationValues.Left);
        var sub = Phase3DocxFixtureBuilder.BuildSubsectionParagraph(
            "Plant sampling and DNA extraction",
            alignment: JustificationValues.Left);

        using var doc = Phase3DocxFixtureBuilder.CreateDocument(paragraphs: new[]
        {
            intro, sub,
        });

        var before = CollectBodyTexts(doc);
        var report = new Report();

        CreateRule().Apply(doc, new FormattingContext(), report);

        AssertContentPreserved(before, doc);
        Assert.True(IsCenterJustified(sub));
        var run = sub.Elements<Run>().Single();
        var (sz, szCs) = GetRunFontSizes(run);
        Assert.Equal("28", sz);
        Assert.Equal("28", szCs);
    }

    [Fact]
    public void Apply_AnchorParagraphItself_IsReformattedAsSection()
    {
        var intro = Phase3DocxFixtureBuilder.BuildIntroductionAnchorParagraph(
            alignment: JustificationValues.Left);

        using var doc = Phase3DocxFixtureBuilder.CreateDocument(paragraphs: new[] { intro });

        var before = CollectBodyTexts(doc);
        var report = new Report();

        CreateRule().Apply(doc, new FormattingContext(), report);

        AssertContentPreserved(before, doc);
        Assert.True(IsCenterJustified(intro));
        var run = intro.Elements<Run>().Single();
        var (sz, szCs) = GetRunFontSizes(run);
        Assert.Equal("32", sz);
        Assert.Equal("32", szCs);
    }

    [Fact]
    public void Apply_ParagraphsBeforeAnchor_AreUntouched()
    {
        var titleParagraph = Phase3DocxFixtureBuilder.BuildSectionParagraph(
            "ARTICLE",
            alignment: JustificationValues.Left);
        var preAnchorBody = Phase3DocxFixtureBuilder.BuildParagraph(
            "Some front-matter text",
            alignment: JustificationValues.Left);
        var intro = Phase3DocxFixtureBuilder.BuildIntroductionAnchorParagraph(
            alignment: JustificationValues.Left);

        using var doc = Phase3DocxFixtureBuilder.CreateDocument(paragraphs: new[]
        {
            titleParagraph, preAnchorBody, intro,
        });

        var titleJcBefore = GetJustification(titleParagraph);
        var titleSzBefore = titleParagraph.Elements<Run>().Single().RunProperties?.FontSize?.Val?.Value;
        var preBodyJcBefore = GetJustification(preAnchorBody);
        var preBodySzBefore = preAnchorBody.Elements<Run>().Single().RunProperties?.FontSize?.Val?.Value;

        var before = CollectBodyTexts(doc);
        var report = new Report();

        CreateRule().Apply(doc, new FormattingContext(), report);

        AssertContentPreserved(before, doc);

        Assert.Equal(titleJcBefore, GetJustification(titleParagraph));
        Assert.Equal(titleSzBefore, titleParagraph.Elements<Run>().Single().RunProperties?.FontSize?.Val?.Value);
        Assert.Equal(preBodyJcBefore, GetJustification(preAnchorBody));
        Assert.Equal(preBodySzBefore, preAnchorBody.Elements<Run>().Single().RunProperties?.FontSize?.Val?.Value);
    }

    [Fact]
    public void Apply_ParagraphsInsideTable_AreUntouchedEvenWhenMatchingPredicate()
    {
        var intro = Phase3DocxFixtureBuilder.BuildIntroductionAnchorParagraph(
            alignment: JustificationValues.Left);
        var insideTable = Phase3DocxFixtureBuilder.BuildSectionParagraph(
            "RESULTS AND DISCUSSION",
            alignment: JustificationValues.Left);
        var table = Phase3DocxFixtureBuilder.WrapInTable(insideTable);
        var afterTable = Phase3DocxFixtureBuilder.BuildParagraph("Body content");

        using var doc = Phase3DocxFixtureBuilder.CreateDocument(bodyElements: new OpenXmlElement[]
        {
            intro, table, afterTable,
        });

        var insideJcBefore = GetJustification(insideTable);
        var insideSzBefore = insideTable.Elements<Run>().Single().RunProperties?.FontSize?.Val?.Value;

        var before = CollectBodyTexts(doc);
        var report = new Report();

        CreateRule().Apply(doc, new FormattingContext(), report);

        AssertContentPreserved(before, doc);
        Assert.Equal(insideJcBefore, GetJustification(insideTable));
        Assert.Equal(insideSzBefore, insideTable.Elements<Run>().Single().RunProperties?.FontSize?.Val?.Value);
    }

    [Fact]
    public void Apply_ContextSkipListParagraphsAboveAnchor_AreUntouched()
    {
        var doiParagraph = Phase3DocxFixtureBuilder.BuildSectionParagraph(
            "ARTICLE",
            alignment: JustificationValues.Left);
        var titleParagraph = Phase3DocxFixtureBuilder.BuildSectionParagraph(
            "TITLE PARAGRAPH",
            alignment: JustificationValues.Left);
        var sectionParagraph = Phase3DocxFixtureBuilder.BuildSectionParagraph(
            "RESEARCH",
            alignment: JustificationValues.Left);
        var intro = Phase3DocxFixtureBuilder.BuildIntroductionAnchorParagraph(
            alignment: JustificationValues.Left);

        using var doc = Phase3DocxFixtureBuilder.CreateDocument(paragraphs: new[]
        {
            doiParagraph, titleParagraph, sectionParagraph, intro,
        });

        var ctx = new FormattingContext
        {
            DoiParagraph = doiParagraph,
            TitleParagraph = titleParagraph,
            SectionParagraph = sectionParagraph,
        };

        var doiJcBefore = GetJustification(doiParagraph);
        var titleJcBefore = GetJustification(titleParagraph);
        var sectionJcBefore = GetJustification(sectionParagraph);

        var before = CollectBodyTexts(doc);
        var report = new Report();

        CreateRule().Apply(doc, ctx, report);

        AssertContentPreserved(before, doc);
        Assert.Equal(doiJcBefore, GetJustification(doiParagraph));
        Assert.Equal(titleJcBefore, GetJustification(titleParagraph));
        Assert.Equal(sectionJcBefore, GetJustification(sectionParagraph));
        Assert.True(IsCenterJustified(intro));
    }

    [Fact]
    public void Apply_ContextSectionParagraphAfterAnchor_StillSkippedByReferenceEquality()
    {
        var intro = Phase3DocxFixtureBuilder.BuildIntroductionAnchorParagraph(
            alignment: JustificationValues.Left);
        var contextSection = Phase3DocxFixtureBuilder.BuildSectionParagraph(
            "MATERIAL AND METHODS",
            alignment: JustificationValues.Left);

        using var doc = Phase3DocxFixtureBuilder.CreateDocument(paragraphs: new[]
        {
            intro, contextSection,
        });

        var ctx = new FormattingContext
        {
            SectionParagraph = contextSection,
        };

        var jcBefore = GetJustification(contextSection);
        var szBefore = contextSection.Elements<Run>().Single().RunProperties?.FontSize?.Val?.Value;

        var before = CollectBodyTexts(doc);
        var report = new Report();

        CreateRule().Apply(doc, ctx, report);

        AssertContentPreserved(before, doc);
        Assert.Equal(jcBefore, GetJustification(contextSection));
        Assert.Equal(szBefore, contextSection.Elements<Run>().Single().RunProperties?.FontSize?.Val?.Value);
    }

    [Fact]
    public void Apply_AnchorMissing_EmitsWarnAndNoMutation()
    {
        var p1 = Phase3DocxFixtureBuilder.BuildSectionParagraph(
            "MATERIAL AND METHODS",
            alignment: JustificationValues.Left);
        var p2 = Phase3DocxFixtureBuilder.BuildParagraph("body text");

        using var doc = Phase3DocxFixtureBuilder.CreateDocument(paragraphs: new[] { p1, p2 });

        var p1JcBefore = GetJustification(p1);
        var p1SzBefore = p1.Elements<Run>().Single().RunProperties?.FontSize?.Val?.Value;

        var before = CollectBodyTexts(doc);
        var report = new Report();

        CreateRule().Apply(doc, new FormattingContext(), report);

        AssertContentPreserved(before, doc);
        Assert.Equal(p1JcBefore, GetJustification(p1));
        Assert.Equal(p1SzBefore, p1.Elements<Run>().Single().RunProperties?.FontSize?.Val?.Value);

        var warn = Assert.Single(report.Entries, e => e.Level == ReportLevel.Warn);
        Assert.Equal(nameof(PromoteSectionsRule), warn.Rule);
        Assert.Equal(PromoteSectionsRule.AnchorMissingMessage, warn.Message);
    }

    [Fact]
    public void Apply_RunTwice_ProducesByteIdenticalOoxmlOnAffectedParagraphs()
    {
        var intro = Phase3DocxFixtureBuilder.BuildIntroductionAnchorParagraph(
            alignment: JustificationValues.Left);
        var section = Phase3DocxFixtureBuilder.BuildSectionParagraph(
            "MATERIAL AND METHODS",
            alignment: JustificationValues.Left);
        var sub = Phase3DocxFixtureBuilder.BuildSubsectionParagraph(
            "Plant sampling and analysis",
            alignment: JustificationValues.Left);

        using var doc = Phase3DocxFixtureBuilder.CreateDocument(paragraphs: new[]
        {
            intro, section, sub,
        });

        var before = CollectBodyTexts(doc);
        var rule = CreateRule();

        rule.Apply(doc, new FormattingContext(), new Report());

        var introXmlAfterFirst = intro.OuterXml;
        var sectionXmlAfterFirst = section.OuterXml;
        var subXmlAfterFirst = sub.OuterXml;

        rule.Apply(doc, new FormattingContext(), new Report());

        AssertContentPreserved(before, doc);
        Assert.Equal(introXmlAfterFirst, intro.OuterXml);
        Assert.Equal(sectionXmlAfterFirst, section.OuterXml);
        Assert.Equal(subXmlAfterFirst, sub.OuterXml);
    }

    [Fact]
    public void Apply_RunWithoutRunProperties_ReceivesNewRunPropertiesWithSize()
    {
        var intro = Phase3DocxFixtureBuilder.BuildIntroductionAnchorParagraph(
            alignment: JustificationValues.Left);

        using var doc = Phase3DocxFixtureBuilder.CreateDocument(paragraphs: new[] { intro });

        // Verify the run starts without a FontSize element so the test exercises the create path.
        var run = intro.Elements<Run>().Single();
        Assert.Null(run.RunProperties?.FontSize);

        var before = CollectBodyTexts(doc);

        CreateRule().Apply(doc, new FormattingContext(), new Report());

        AssertContentPreserved(before, doc);

        var (sz, szCs) = GetRunFontSizes(run);
        Assert.Equal("32", sz);
        Assert.Equal("32", szCs);
    }

    [Fact]
    public void Apply_RunWithExistingBoldRunProperties_PreservesBoldAndAddsSize()
    {
        var intro = Phase3DocxFixtureBuilder.BuildIntroductionAnchorParagraph(
            alignment: JustificationValues.Left);
        var section = Phase3DocxFixtureBuilder.BuildSectionParagraph(
            "MATERIAL AND METHODS",
            alignment: JustificationValues.Left);

        using var doc = Phase3DocxFixtureBuilder.CreateDocument(paragraphs: new[] { intro, section });

        var run = section.Elements<Run>().Single();
        Assert.NotNull(run.RunProperties);
        Assert.NotNull(run.RunProperties!.GetFirstChild<Bold>());

        var before = CollectBodyTexts(doc);

        CreateRule().Apply(doc, new FormattingContext(), new Report());

        AssertContentPreserved(before, doc);

        Assert.NotNull(run.RunProperties!.GetFirstChild<Bold>());
        var (sz, szCs) = GetRunFontSizes(run);
        Assert.Equal("32", sz);
        Assert.Equal("32", szCs);
    }

    [Fact]
    public void Apply_ParagraphPropertiesWithOtherChildren_PreservesOthersAndSetsCenterJustification()
    {
        var intro = Phase3DocxFixtureBuilder.BuildIntroductionAnchorParagraph(
            alignment: JustificationValues.Left);
        var section = Phase3DocxFixtureBuilder.BuildSectionParagraph(
            "MATERIAL AND METHODS",
            alignment: JustificationValues.Left);
        section.ParagraphProperties!.AppendChild(new PageBreakBefore());

        using var doc = Phase3DocxFixtureBuilder.CreateDocument(paragraphs: new[] { intro, section });

        var before = CollectBodyTexts(doc);

        CreateRule().Apply(doc, new FormattingContext(), new Report());

        AssertContentPreserved(before, doc);

        Assert.NotNull(section.ParagraphProperties);
        Assert.True(IsCenterJustified(section));
        Assert.NotNull(section.ParagraphProperties!.GetFirstChild<PageBreakBefore>());
    }

    [Fact]
    public void Apply_ParagraphWithoutParagraphProperties_ReceivesNewParagraphPropertiesWithCenter()
    {
        var intro = Phase3DocxFixtureBuilder.BuildIntroductionAnchorParagraph(
            alignment: JustificationValues.Left);
        // Build a section paragraph with no ParagraphProperties at all (no alignment, no styleId).
        var section = Phase3DocxFixtureBuilder.BuildSectionParagraph("MATERIAL AND METHODS");
        Assert.Null(section.ParagraphProperties);

        using var doc = Phase3DocxFixtureBuilder.CreateDocument(paragraphs: new[] { intro, section });

        var before = CollectBodyTexts(doc);

        CreateRule().Apply(doc, new FormattingContext(), new Report());

        AssertContentPreserved(before, doc);
        Assert.NotNull(section.ParagraphProperties);
        Assert.True(IsCenterJustified(section));
    }

    [Fact]
    public void Apply_SuccessPath_EmitsAnchorPositionAndSummaryInfoMessages()
    {
        var keywords = Phase3DocxFixtureBuilder.BuildParagraph("Keywords: maize");
        var intro = Phase3DocxFixtureBuilder.BuildIntroductionAnchorParagraph(
            alignment: JustificationValues.Left);
        var sectionA = Phase3DocxFixtureBuilder.BuildSectionParagraph(
            "MATERIAL AND METHODS",
            alignment: JustificationValues.Left);
        var subA = Phase3DocxFixtureBuilder.BuildSubsectionParagraph(
            "Plant sampling and analysis",
            alignment: JustificationValues.Left);
        var sectionB = Phase3DocxFixtureBuilder.BuildSectionParagraph(
            "RESULTS",
            alignment: JustificationValues.Left);
        var subB = Phase3DocxFixtureBuilder.BuildSubsectionParagraph(
            "Sequence assembly results",
            alignment: JustificationValues.Left);
        var sectionC = Phase3DocxFixtureBuilder.BuildSectionParagraph(
            "REFERENCES",
            alignment: JustificationValues.Left);

        using var doc = Phase3DocxFixtureBuilder.CreateDocument(paragraphs: new[]
        {
            keywords, intro, sectionA, subA, sectionB, subB, sectionC,
        });

        var before = CollectBodyTexts(doc);
        var report = new Report();

        CreateRule().Apply(doc, new FormattingContext(), report);

        AssertContentPreserved(before, doc);
        Assert.DoesNotContain(report.Entries, e => e.Level >= ReportLevel.Warn);

        var infos = report.Entries.Where(e => e.Level == ReportLevel.Info).ToList();
        Assert.Equal(3, infos.Count);
        Assert.All(infos, e => Assert.Equal(nameof(PromoteSectionsRule), e.Rule));

        var anchorPositionEntry = Assert.Single(
            infos,
            e => e.Message.StartsWith(PromoteSectionsRule.AnchorPositionMessagePrefix, StringComparison.Ordinal));
        Assert.Equal($"{PromoteSectionsRule.AnchorPositionMessagePrefix}1", anchorPositionEntry.Message);

        var summaryEntry = Assert.Single(
            infos,
            e => e.Message.StartsWith(PromoteSectionsRule.SummaryPromotedPrefix, StringComparison.Ordinal));
        // intro + sectionA + sectionB + sectionC = 4 sections; subA + subB = 2 sub-sections.
        var expectedSummary =
            $"{PromoteSectionsRule.SummaryPromotedPrefix}4{PromoteSectionsRule.SummarySectionsInfix}2{PromoteSectionsRule.SummarySubsectionsSuffix}";
        Assert.Equal(expectedSummary, summaryEntry.Message);

        var skipCountsEntry = Assert.Single(
            infos,
            e => e.Message.StartsWith(PromoteSectionsRule.SkipCountsMessagePrefix, StringComparison.Ordinal));
        // No tables in fixture; anchor at body position 1 means 1 paragraph (keywords) before anchor.
        var expectedSkipCounts =
            $"{PromoteSectionsRule.SkipCountsMessagePrefix}0{PromoteSectionsRule.SkipCountsInTablesInfix}1{PromoteSectionsRule.SkipCountsBeforeAnchorSuffix}";
        Assert.Equal(expectedSkipCounts, skipCountsEntry.Message);
    }

    [Fact]
    public void Apply_RuleNeverThrowsOnEmptyBody()
    {
        using var doc = Phase3DocxFixtureBuilder.CreateDocument(paragraphs: Array.Empty<Paragraph>());

        var report = new Report();

        var ex = Record.Exception(() => CreateRule().Apply(doc, new FormattingContext(), report));

        Assert.Null(ex);
        var warn = Assert.Single(report.Entries, e => e.Level == ReportLevel.Warn);
        Assert.Equal(PromoteSectionsRule.AnchorMissingMessage, warn.Message);
    }

    [Fact]
    public void Apply_IntegrationSyntheticDocument_PromotesOnlyBodySectionsAndSubsections()
    {
        var doiParagraph = Phase3DocxFixtureBuilder.BuildParagraph(
            "DOI: https://doi.org/10.1234/example",
            alignment: JustificationValues.Right);
        var sectionLabel = Phase3DocxFixtureBuilder.BuildSectionParagraph(
            "ARTICLE",
            alignment: JustificationValues.Right);
        var titleParagraph = Phase3DocxFixtureBuilder.BuildSubsectionParagraph(
            "Sample Article Title",
            alignment: JustificationValues.Center);
        var authorsParagraph = Phase3DocxFixtureBuilder.BuildParagraph(
            "Maria Silva, João Santos");
        var keywords = Phase3DocxFixtureBuilder.BuildParagraph("Keywords: maize, breeding");
        var received = Phase3DocxFixtureBuilder.BuildHistoryParagraph("Received", "2024-01-15");
        var accepted = Phase3DocxFixtureBuilder.BuildHistoryParagraph("Accepted", "2024-03-10");
        var published = Phase3DocxFixtureBuilder.BuildHistoryParagraph("Published", "2024-04-01");
        var intro = Phase3DocxFixtureBuilder.BuildIntroductionAnchorParagraph(
            alignment: JustificationValues.Left);
        var introBody = Phase3DocxFixtureBuilder.BuildParagraph(
            "Introduction body text describing prior work.");
        var methods = Phase3DocxFixtureBuilder.BuildSectionParagraph(
            "MATERIAL AND METHODS",
            alignment: JustificationValues.Left);
        var methodsSub = Phase3DocxFixtureBuilder.BuildSubsectionParagraph(
            "Plant sampling, DNA extraction, and sequencing",
            alignment: JustificationValues.Left);
        var methodsBody = Phase3DocxFixtureBuilder.BuildParagraph(
            "We collected leaf samples from each cultivar.");

        var inTableSection = Phase3DocxFixtureBuilder.BuildSectionParagraph(
            "TABLE INNER HEADER",
            alignment: JustificationValues.Left);
        var inTableBody = Phase3DocxFixtureBuilder.BuildParagraph("Table cell body");
        var dataTable = Phase3DocxFixtureBuilder.WrapInTable(inTableSection, inTableBody);

        var results = Phase3DocxFixtureBuilder.BuildSectionParagraph(
            "RESULTS",
            alignment: JustificationValues.Left);
        var resultsBody = Phase3DocxFixtureBuilder.BuildParagraph(
            "Results body describing the findings.");
        var references = Phase3DocxFixtureBuilder.BuildSectionParagraph(
            "REFERENCES",
            alignment: JustificationValues.Left);
        var bibEntry = Phase3DocxFixtureBuilder.BuildParagraph(
            "Smith J. et al. (2022). Title. Journal.");

        var bodyChildren = new OpenXmlElement[]
        {
            doiParagraph, sectionLabel, titleParagraph, authorsParagraph,
            keywords, received, accepted, published,
            intro, introBody, methods, methodsSub, methodsBody,
            dataTable,
            results, resultsBody, references, bibEntry,
        };

        using var doc = Phase3DocxFixtureBuilder.CreateDocument(bodyElements: bodyChildren);

        var ctx = new FormattingContext
        {
            DoiParagraph = doiParagraph,
            SectionParagraph = sectionLabel,
            TitleParagraph = titleParagraph,
        };

        var inTableSectionJcBefore = GetJustification(inTableSection);
        var inTableSectionSzBefore =
            inTableSection.Elements<Run>().Single().RunProperties?.FontSize?.Val?.Value;
        var doiJcBefore = GetJustification(doiParagraph);
        var sectionLabelJcBefore = GetJustification(sectionLabel);
        var titleJcBefore = GetJustification(titleParagraph);

        var before = CollectBodyTexts(doc);
        var report = new Report();

        CreateRule().Apply(doc, ctx, report);

        AssertContentPreserved(before, doc);

        Assert.True(IsCenterJustified(intro));
        Assert.True(IsCenterJustified(methods));
        Assert.True(IsCenterJustified(methodsSub));
        Assert.True(IsCenterJustified(results));
        Assert.True(IsCenterJustified(references));

        Assert.Equal("32", intro.Elements<Run>().Single().RunProperties?.FontSize?.Val?.Value);
        Assert.Equal("32", methods.Elements<Run>().Single().RunProperties?.FontSize?.Val?.Value);
        Assert.Equal("28", methodsSub.Elements<Run>().Single().RunProperties?.FontSize?.Val?.Value);
        Assert.Equal("32", results.Elements<Run>().Single().RunProperties?.FontSize?.Val?.Value);
        Assert.Equal("32", references.Elements<Run>().Single().RunProperties?.FontSize?.Val?.Value);

        Assert.Equal(doiJcBefore, GetJustification(doiParagraph));
        Assert.Equal(sectionLabelJcBefore, GetJustification(sectionLabel));
        Assert.Equal(titleJcBefore, GetJustification(titleParagraph));

        Assert.Equal(inTableSectionJcBefore, GetJustification(inTableSection));
        Assert.Equal(
            inTableSectionSzBefore,
            inTableSection.Elements<Run>().Single().RunProperties?.FontSize?.Val?.Value);

        Assert.DoesNotContain(report.Entries, e => e.Level >= ReportLevel.Warn);
        var summary = Assert.Single(
            report.Entries,
            e => e.Level == ReportLevel.Info
                 && e.Message.StartsWith(PromoteSectionsRule.SummaryPromotedPrefix, StringComparison.Ordinal));
        // intro + methods + results + references = 4 sections; methodsSub = 1 sub-section.
        var expectedSummary =
            $"{PromoteSectionsRule.SummaryPromotedPrefix}4{PromoteSectionsRule.SummarySectionsInfix}1{PromoteSectionsRule.SummarySubsectionsSuffix}";
        Assert.Equal(expectedSummary, summary.Message);
    }
}
