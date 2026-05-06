using DocFormatter.Core.Options;
using DocFormatter.Core.Pipeline;
using DocFormatter.Core.Rules;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Xunit;
using Blip = DocumentFormat.OpenXml.Drawing.Blip;

namespace DocFormatter.Tests;

public sealed class ExtractOrcidLinksRuleTests
{
    private const string ImageRelationshipType =
        "http://schemas.openxmlformats.org/officeDocument/2006/relationships/image";

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

    private static MainDocumentPart GetMainPart(WordprocessingDocument doc)
        => doc.MainDocumentPart!;

    private static Paragraph BuildParagraph(string text)
        => new(new Run(new Text(text) { Space = SpaceProcessingModeValues.Preserve }));

    private static Run BuildRun(string text, RunProperties? properties = null)
    {
        var run = new Run();
        if (properties is not null)
        {
            run.AppendChild(properties);
        }

        run.AppendChild(new Text(text) { Space = SpaceProcessingModeValues.Preserve });
        return run;
    }

    private static Hyperlink BuildHyperlink(string rId, params OpenXmlElement[] children)
        => new(children) { Id = rId };

    private static Paragraph BuildAuthorsParagraph(params OpenXmlElement[] children)
        => new(children);

    private static (Paragraph Section, Paragraph Title) HeaderParagraphs()
        => (BuildParagraph("Original Article"), BuildParagraph("On the Behavior of..."));

    [Fact]
    public void Apply_WithHttpsOrcidHyperlink_ReplacesWithPlainRunAndStagesId()
    {
        var (section, title) = HeaderParagraphs();
        using var doc = CreateDocumentWith(section, title, BuildAuthorsParagraph());
        var mainPart = GetMainPart(doc);
        var rel = mainPart.AddHyperlinkRelationship(new Uri("https://orcid.org/0000-0002-1825-0097"), true);
        var authors = GetBody(doc).Elements<Paragraph>().Last();
        authors.AppendChild(BuildRun("José Silva"));
        authors.AppendChild(BuildHyperlink(rel.Id, BuildRun("0000-0002-1825-0097")));

        var rule = new ExtractOrcidLinksRule(new FormattingOptions());
        var ctx = new FormattingContext();
        var report = new Report();

        rule.Apply(doc, ctx, report);

        Assert.Empty(authors.Elements<Hyperlink>());
        var runs = authors.Elements<Run>().ToList();
        Assert.Equal(2, runs.Count);
        Assert.Equal("José Silva", string.Concat(runs[0].Descendants<Text>().Select(t => t.Text)));
        Assert.Equal("0000-0002-1825-0097", string.Concat(runs[1].Descendants<Text>().Select(t => t.Text)));
        Assert.Empty(mainPart.HyperlinkRelationships);
        Assert.Equal("0000-0002-1825-0097", ctx.OrcidStaging[1]);
        Assert.DoesNotContain(report.Entries, e => e.Level == ReportLevel.Warn || e.Level == ReportLevel.Error);
    }

    [Fact]
    public void Apply_WithFileUrlContainingOrcidMarker_ExtractsIdFromFragment()
    {
        var (section, title) = HeaderParagraphs();
        using var doc = CreateDocumentWith(section, title, BuildAuthorsParagraph());
        var mainPart = GetMainPart(doc);
        var rel = mainPart.AddHyperlinkRelationship(
            new Uri("file:///C:/article.docx#orcid.org/0000-0002-1825-0097"),
            true);
        var authors = GetBody(doc).Elements<Paragraph>().Last();
        authors.AppendChild(BuildHyperlink(rel.Id, BuildRun("ORCID")));

        var rule = new ExtractOrcidLinksRule(new FormattingOptions());
        var ctx = new FormattingContext();
        var report = new Report();

        rule.Apply(doc, ctx, report);

        Assert.Empty(authors.Elements<Hyperlink>());
        var run = Assert.Single(authors.Elements<Run>());
        Assert.Equal("0000-0002-1825-0097", string.Concat(run.Descendants<Text>().Select(t => t.Text)));
        Assert.Empty(mainPart.HyperlinkRelationships);
        Assert.Equal("0000-0002-1825-0097", ctx.OrcidStaging[0]);
    }

    [Fact]
    public void Apply_WithNonOrcidHyperlink_LeavesItIntactAndStagesNothing()
    {
        var (section, title) = HeaderParagraphs();
        using var doc = CreateDocumentWith(section, title, BuildAuthorsParagraph());
        var mainPart = GetMainPart(doc);
        var rel = mainPart.AddHyperlinkRelationship(new Uri("https://example.com"), true);
        var authors = GetBody(doc).Elements<Paragraph>().Last();
        authors.AppendChild(BuildHyperlink(rel.Id, BuildRun("homepage")));

        var rule = new ExtractOrcidLinksRule(new FormattingOptions());
        var ctx = new FormattingContext();
        var report = new Report();

        rule.Apply(doc, ctx, report);

        var hyperlink = Assert.Single(authors.Elements<Hyperlink>());
        Assert.Equal(rel.Id, hyperlink.Id?.Value);
        Assert.Empty(ctx.OrcidStaging);
        Assert.Single(mainPart.HyperlinkRelationships);
        Assert.DoesNotContain(report.Entries, e => e.Level == ReportLevel.Warn || e.Level == ReportLevel.Error);
    }

    [Fact]
    public void Apply_WithOrcidUrlButGarbledId_LogsWarnAndLeavesHyperlinkIntact()
    {
        var (section, title) = HeaderParagraphs();
        using var doc = CreateDocumentWith(section, title, BuildAuthorsParagraph());
        var mainPart = GetMainPart(doc);
        var rel = mainPart.AddHyperlinkRelationship(new Uri("https://orcid.org/garbled-id"), true);
        var authors = GetBody(doc).Elements<Paragraph>().Last();
        authors.AppendChild(BuildHyperlink(rel.Id, BuildRun("ORCID")));

        var rule = new ExtractOrcidLinksRule(new FormattingOptions());
        var ctx = new FormattingContext();
        var report = new Report();

        rule.Apply(doc, ctx, report);

        Assert.Single(authors.Elements<Hyperlink>());
        Assert.Empty(ctx.OrcidStaging);
        Assert.Single(mainPart.HyperlinkRelationships);
        Assert.Contains(
            report.Entries,
            e => e.Level == ReportLevel.Warn && e.Message.Contains("orcid.org", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Apply_WithNestedDrawingInsideOrcidHyperlink_RemovesDrawingAlongsideReplacement()
    {
        var (section, title) = HeaderParagraphs();
        using var doc = CreateDocumentWith(section, title, BuildAuthorsParagraph());
        var mainPart = GetMainPart(doc);
        var rel = mainPart.AddHyperlinkRelationship(new Uri("https://orcid.org/0000-0002-1825-0097"), true);
        var authors = GetBody(doc).Elements<Paragraph>().Last();
        var hyperlink = BuildHyperlink(
            rel.Id,
            BuildRun("0000-0002-1825-0097"),
            new Run(new Drawing(new Blip { Embed = rel.Id })));
        authors.AppendChild(hyperlink);

        var rule = new ExtractOrcidLinksRule(new FormattingOptions());
        var ctx = new FormattingContext();
        var report = new Report();

        rule.Apply(doc, ctx, report);

        Assert.Empty(authors.Elements<Hyperlink>());
        Assert.Empty(authors.Descendants<Drawing>());
        var run = Assert.Single(authors.Elements<Run>());
        Assert.Equal("0000-0002-1825-0097", string.Concat(run.Descendants<Text>().Select(t => t.Text)));
        Assert.Equal("0000-0002-1825-0097", ctx.OrcidStaging[0]);
        Assert.Empty(mainPart.HyperlinkRelationships);
    }

    [Fact]
    public void Apply_WithFreeStandingOrcidBadgeBeforeHyperlink_LeavesDrawingAndLogsWarn()
    {
        var (section, title) = HeaderParagraphs();
        using var doc = CreateDocumentWith(section, title, BuildAuthorsParagraph());
        var mainPart = GetMainPart(doc);
        var imageRel = mainPart.AddExternalRelationship(
            ImageRelationshipType,
            new Uri("https://orcid.org/badge.png"));
        var hyperlinkRel = mainPart.AddHyperlinkRelationship(
            new Uri("https://orcid.org/0000-0002-1825-0097"),
            true);
        var authors = GetBody(doc).Elements<Paragraph>().Last();
        authors.AppendChild(new Run(new Drawing(new Blip { Embed = imageRel.Id })));
        authors.AppendChild(BuildHyperlink(hyperlinkRel.Id, BuildRun("0000-0002-1825-0097")));

        var rule = new ExtractOrcidLinksRule(new FormattingOptions());
        var ctx = new FormattingContext();
        var report = new Report();

        rule.Apply(doc, ctx, report);

        Assert.Single(authors.Descendants<Drawing>());
        Assert.Single(authors.Descendants<Blip>(), b => b.Embed?.Value == imageRel.Id);
        Assert.Equal("0000-0002-1825-0097", ctx.OrcidStaging[1]);
        Assert.Contains(
            report.Entries,
            e => e.Level == ReportLevel.Warn && e.Message.Contains("free-standing", StringComparison.OrdinalIgnoreCase));
        Assert.Single(mainPart.ExternalRelationships);
        Assert.Empty(mainPart.HyperlinkRelationships);
    }

    [Fact]
    public void Apply_WithNoAuthorsParagraph_LogsWarnAndDoesNotMutate()
    {
        var (section, title) = HeaderParagraphs();
        using var doc = CreateDocumentWith(section, title);
        var rule = new ExtractOrcidLinksRule(new FormattingOptions());
        var ctx = new FormattingContext();
        var report = new Report();

        rule.Apply(doc, ctx, report);

        Assert.Empty(ctx.OrcidStaging);
        var warn = Assert.Single(report.Entries, e => e.Level == ReportLevel.Warn);
        Assert.Equal(ExtractOrcidLinksRule.MissingAuthorsParagraphMessage, warn.Message);
        Assert.Equal(2, GetBody(doc).Elements<Paragraph>().Count());
    }

    [Fact]
    public void Apply_PreservesInnerRunPropertiesOnReplacementRun()
    {
        var (section, title) = HeaderParagraphs();
        using var doc = CreateDocumentWith(section, title, BuildAuthorsParagraph());
        var mainPart = GetMainPart(doc);
        var rel = mainPart.AddHyperlinkRelationship(new Uri("https://orcid.org/0000-0002-1825-0097"), true);
        var authors = GetBody(doc).Elements<Paragraph>().Last();
        var properties = new RunProperties(new Bold());
        authors.AppendChild(BuildHyperlink(rel.Id, BuildRun("0000-0002-1825-0097", properties)));

        var rule = new ExtractOrcidLinksRule(new FormattingOptions());
        var ctx = new FormattingContext();
        var report = new Report();

        rule.Apply(doc, ctx, report);

        var run = Assert.Single(authors.Elements<Run>());
        Assert.NotNull(run.RunProperties);
        Assert.Single(run.RunProperties!.Elements<Bold>());
    }

    [Fact]
    public void Pipeline_WithExtractTopTableThenParseHeaderLinesThenExtractOrcid_StagesIdConsumableByNextRule()
    {
        var grid = new TableGrid(
            new GridColumn { Width = "1000" },
            new GridColumn { Width = "1000" },
            new GridColumn { Width = "1000" });
        var table = new Table(
            grid,
            new TableRow(
                new TableCell(new Paragraph(new Run(new Text("ART01")))),
                new TableCell(new Paragraph(new Run(new Text("e2024001")))),
                new TableCell(new Paragraph(new Run(new Text("10.1234/abc"))))));
        var section = BuildParagraph("Original Article");
        var title = BuildParagraph("On the Behavior of...");
        var authors = BuildAuthorsParagraph();

        using var doc = CreateDocumentWith(table, section, title, authors);
        var mainPart = GetMainPart(doc);
        var rel = mainPart.AddHyperlinkRelationship(new Uri("https://orcid.org/0000-0002-1825-0097"), true);
        authors.AppendChild(BuildRun("José Silva, "));
        authors.AppendChild(BuildHyperlink(rel.Id, BuildRun("0000-0002-1825-0097")));

        var pipeline = new FormattingPipeline(new IFormattingRule[]
        {
            new ExtractTopTableRule(new FormattingOptions()),
            new ParseHeaderLinesRule(),
            new ExtractOrcidLinksRule(new FormattingOptions()),
        });
        var ctx = new FormattingContext();
        var report = new Report();

        pipeline.Run(doc, ctx, report);

        Assert.NotEmpty(ctx.OrcidStaging);
        Assert.Equal("0000-0002-1825-0097", ctx.OrcidStaging[1]);
        Assert.Equal("On the Behavior of...", ctx.ArticleTitle);
        Assert.Empty(GetBody(doc).Elements<Table>());
        Assert.DoesNotContain(report.Entries, e => e.Level == ReportLevel.Error);
    }
}
