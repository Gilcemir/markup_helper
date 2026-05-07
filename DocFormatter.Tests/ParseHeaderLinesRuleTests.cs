using DocFormatter.Core.Options;
using DocFormatter.Core.Pipeline;
using DocFormatter.Core.Rules;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Xunit;

namespace DocFormatter.Tests;

public sealed class ParseHeaderLinesRuleTests
{
    private static WordprocessingDocument CreateDocumentWith(params OpenXmlElement[] bodyChildren)
    {
        var stream = new MemoryStream();
        var doc = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document);
        var mainPart = doc.AddMainDocumentPart();
        mainPart.Document = new Document(new Body(bodyChildren));
        return doc;
    }

    private static Body GetBody(WordprocessingDocument doc)
        => doc.MainDocumentPart!.Document!.Body!;

    private static Paragraph BuildParagraph(string text)
        => new(new Run(new Text(text) { Space = SpaceProcessingModeValues.Preserve }));

    private static Paragraph BuildParagraphWithRuns(params string[] runTexts)
    {
        var runs = runTexts
            .Select(t => new Run(new Text(t) { Space = SpaceProcessingModeValues.Preserve }))
            .Cast<OpenXmlElement>()
            .ToArray();
        return new Paragraph(runs);
    }

    private static TableCell BuildCell(params string[] paragraphTexts)
    {
        var paragraphs = paragraphTexts
            .Select(t => new Paragraph(new Run(new Text(t) { Space = SpaceProcessingModeValues.Preserve })))
            .Cast<OpenXmlElement>()
            .ToArray();
        return new TableCell(paragraphs);
    }

    private static Table BuildThreeByOneTable(TableCell c1, TableCell c2, TableCell c3)
    {
        var grid = new TableGrid(
            new GridColumn { Width = "1000" },
            new GridColumn { Width = "1000" },
            new GridColumn { Width = "1000" });
        return new Table(grid, new TableRow(c1, c2, c3));
    }

    [Fact]
    public void Apply_WithSectionAndTitle_AssignsArticleTitleAndDoesNotMutateDocument()
    {
        var section = BuildParagraph("Original Article");
        var title = BuildParagraph("On the Behavior of...");
        var authors = BuildParagraph("John Doe, Jane Roe");
        using var doc = CreateDocumentWith(section, title, authors);
        var rule = new ParseHeaderLinesRule();
        var ctx = new FormattingContext();
        var report = new Report();

        rule.Apply(doc, ctx, report);

        Assert.Equal("On the Behavior of...", ctx.ArticleTitle);
        Assert.Same(section, ctx.SectionParagraph);
        Assert.Same(title, ctx.TitleParagraph);
        var paragraphs = GetBody(doc).Elements<Paragraph>().ToList();
        Assert.Equal(3, paragraphs.Count);
        Assert.Equal("Original Article", string.Concat(paragraphs[0].Descendants<Text>().Select(t => t.Text)));
        Assert.Equal("On the Behavior of...", string.Concat(paragraphs[1].Descendants<Text>().Select(t => t.Text)));
        Assert.Equal("John Doe, Jane Roe", string.Concat(paragraphs[2].Descendants<Text>().Select(t => t.Text)));
        Assert.DoesNotContain(report.Entries, e => e.Level == ReportLevel.Warn || e.Level == ReportLevel.Error);
    }

    [Fact]
    public void Apply_SkipsEmptyAndWhitespaceOnlyParagraphs_BeforeAndBetweenSectionAndTitle()
    {
        using var doc = CreateDocumentWith(
            BuildParagraph(string.Empty),
            BuildParagraph("Original Article"),
            BuildParagraph("  "),
            BuildParagraph("On the Behavior of..."));
        var rule = new ParseHeaderLinesRule();
        var ctx = new FormattingContext();
        var report = new Report();

        rule.Apply(doc, ctx, report);

        Assert.Equal("On the Behavior of...", ctx.ArticleTitle);
    }

    [Fact]
    public void Apply_WhenTitleParagraphHasMultipleRuns_ConcatenatesRunInnerText()
    {
        var section = BuildParagraph("Original Article");
        var title = BuildParagraphWithRuns("On the ", "Behavior");
        using var doc = CreateDocumentWith(section, title);
        var rule = new ParseHeaderLinesRule();
        var ctx = new FormattingContext();
        var report = new Report();

        rule.Apply(doc, ctx, report);

        Assert.Equal("On the Behavior", ctx.ArticleTitle);
    }

    [Fact]
    public void Apply_WhenOnlySectionParagraphPresent_ThrowsCriticalAbortReferencingTitle()
    {
        var section = BuildParagraph("Original Article");
        using var doc = CreateDocumentWith(section);
        var rule = new ParseHeaderLinesRule();
        var ctx = new FormattingContext();
        var report = new Report();

        var ex = Assert.Throws<InvalidOperationException>(() => rule.Apply(doc, ctx, report));

        Assert.Equal(ParseHeaderLinesRule.MissingTitleMessage, ex.Message);
        Assert.Contains("title", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Null(ctx.ArticleTitle);
    }

    [Fact]
    public void Apply_WhenNoNonEmptyParagraphsPresent_ThrowsCriticalAbortReferencingSection()
    {
        using var doc = CreateDocumentWith(
            BuildParagraph(string.Empty),
            BuildParagraph("   "));
        var rule = new ParseHeaderLinesRule();
        var ctx = new FormattingContext();
        var report = new Report();

        var ex = Assert.Throws<InvalidOperationException>(() => rule.Apply(doc, ctx, report));

        Assert.Equal(ParseHeaderLinesRule.MissingSectionMessage, ex.Message);
        Assert.Contains("section", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Null(ctx.ArticleTitle);
    }

    [Fact]
    public void Apply_WhenBodyHasNoParagraphsAtAll_ThrowsCriticalAbortReferencingSection()
    {
        using var doc = CreateDocumentWith();
        var rule = new ParseHeaderLinesRule();
        var ctx = new FormattingContext();
        var report = new Report();

        var ex = Assert.Throws<InvalidOperationException>(() => rule.Apply(doc, ctx, report));

        Assert.Equal(ParseHeaderLinesRule.MissingSectionMessage, ex.Message);
        Assert.Null(ctx.ArticleTitle);
    }

    [Fact]
    public void Apply_OnlyConsidersFirstNonEmptyTitleParagraph_AndIgnoresSubsequentParagraphs()
    {
        using var doc = CreateDocumentWith(
            BuildParagraph("Original Article"),
            BuildParagraph("On the Behavior of..."),
            BuildParagraph("Translated subtitle"),
            BuildParagraph("John Doe"));
        var rule = new ParseHeaderLinesRule();
        var ctx = new FormattingContext();
        var report = new Report();

        rule.Apply(doc, ctx, report);

        Assert.Equal("On the Behavior of...", ctx.ArticleTitle);
    }

    [Fact]
    public void Apply_WhenSectionAndTitleShareParagraphSeparatedByBreak_AssignsBoth()
    {
        // Mirrors artigo 4 da pasta examples/: P[0] is "<rB|'ARTICLE'> <r[BR]|''> <rB|'Protein selection gain...'>".
        // Without splitting on <w:br/>, GetParagraphPlainText concatenated both into "ARTICLEProtein selection gain..."
        // and then took P[1] (authors) as the title, desalinhando todo o resto.
        var firstParagraph = new Paragraph(
            new Run(new Text("ARTICLE") { Space = SpaceProcessingModeValues.Preserve }),
            new Run(new Break()),
            new Run(new Text("Protein selection gain") { Space = SpaceProcessingModeValues.Preserve }));
        var authors = BuildParagraph("Maria Silva, Joana Souza");
        using var doc = CreateDocumentWith(firstParagraph, authors);
        var rule = new ParseHeaderLinesRule();
        var ctx = new FormattingContext();
        var report = new Report();

        rule.Apply(doc, ctx, report);

        Assert.Equal("Protein selection gain", ctx.ArticleTitle);
        Assert.Same(firstParagraph, ctx.SectionParagraph);
        Assert.Same(firstParagraph, ctx.TitleParagraph);
        Assert.Contains(
            report.Entries,
            e => e.Level == ReportLevel.Info
                && e.Message.Contains("section='ARTICLE'", StringComparison.Ordinal)
                && e.Message.Contains("articleTitle='Protein selection gain'", StringComparison.Ordinal));
    }

    [Fact]
    public void Apply_WhenOnlySectionParagraphPresent_DoesNotPublishParagraphReferences()
    {
        var section = BuildParagraph("Original Article");
        using var doc = CreateDocumentWith(section);
        var rule = new ParseHeaderLinesRule();
        var ctx = new FormattingContext();
        var report = new Report();

        Assert.Throws<InvalidOperationException>(() => rule.Apply(doc, ctx, report));

        Assert.Null(ctx.SectionParagraph);
        Assert.Null(ctx.TitleParagraph);
    }

    [Fact]
    public void Apply_WhenSectionAndTitleAreInSeparatePostBlankParagraphs_AssignsExactParagraphReferences()
    {
        var leadingBlank = BuildParagraph(string.Empty);
        var section = BuildParagraph("Original Article");
        var midBlank = BuildParagraph("   ");
        var title = BuildParagraph("On the Behavior of...");
        using var doc = CreateDocumentWith(leadingBlank, section, midBlank, title);
        var rule = new ParseHeaderLinesRule();
        var ctx = new FormattingContext();
        var report = new Report();

        rule.Apply(doc, ctx, report);

        Assert.Same(section, ctx.SectionParagraph);
        Assert.Same(title, ctx.TitleParagraph);
        Assert.NotSame(ctx.SectionParagraph, ctx.TitleParagraph);
    }

    [Fact]
    public void Pipeline_WithExtractTopTableThenParseHeaderLines_PopulatesDoiElocationAndArticleTitle()
    {
        var table = BuildThreeByOneTable(
            BuildCell("ART01"),
            BuildCell("e2024001"),
            BuildCell("10.1234/abc"));
        var section = BuildParagraph("Original Article");
        var title = BuildParagraph("On the Behavior of...");
        var authors = BuildParagraph("John Doe");
        using var doc = CreateDocumentWith(table, section, title, authors);
        var pipeline = new FormattingPipeline(new IFormattingRule[]
        {
            new ExtractTopTableRule(new FormattingOptions()),
            new ParseHeaderLinesRule(),
        });
        var ctx = new FormattingContext();
        var report = new Report();

        pipeline.Run(doc, ctx, report);

        Assert.Equal("e2024001", ctx.ElocationId);
        Assert.Equal("10.1234/abc", ctx.Doi);
        Assert.Equal("On the Behavior of...", ctx.ArticleTitle);
        Assert.Same(section, ctx.SectionParagraph);
        Assert.Same(title, ctx.TitleParagraph);
        Assert.Empty(GetBody(doc).Elements<Table>());
        Assert.Equal(3, GetBody(doc).Elements<Paragraph>().Count());
        Assert.DoesNotContain(report.Entries, e => e.Level == ReportLevel.Error);
    }
}
