using DocFormatter.Core.Models;
using DocFormatter.Core.Options;
using DocFormatter.Core.Pipeline;
using DocFormatter.Core.Rules;
using DocFormatter.Tests.Fixtures.Authors;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Xunit;

namespace DocFormatter.Tests;

public sealed class RewriteHeaderMvpRuleTests
{
    private static RewriteHeaderMvpRule CreateRule() => new(new FormattingOptions());

    private static string ParagraphText(Paragraph p)
        => string.Concat(p.Descendants<Text>().Select(t => t.Text));

    private static bool IsSuperscript(Run run)
    {
        var vert = run.RunProperties?.VerticalTextAlignment;
        return vert?.Val is { } val && val.Value == VerticalPositionValues.Superscript;
    }

    [Fact]
    public void Apply_WithDoiAndSingleAuthorNoOrcid_RendersFourFieldHeader()
    {
        using var doc = AuthorsParagraphFactory.CreateDocumentWithAuthorsParagraph(
            AuthorsParagraphFactory.TextRun("Maria Silva"),
            AuthorsParagraphFactory.SuperscriptRun("1"));

        var ctx = new FormattingContext
        {
            Doi = "10.1234/abc",
            ArticleTitle = AuthorsParagraphFactory.TitleText,
        };
        ctx.Authors.Add(new Author("Maria Silva", new[] { "1" }, OrcidId: null));

        var report = new Report();
        CreateRule().Apply(doc, ctx, report);

        var paragraphs = AuthorsParagraphFactory.GetBody(doc).Elements<Paragraph>().ToList();
        Assert.Equal("10.1234/abc", ParagraphText(paragraphs[0]));
        Assert.Equal(AuthorsParagraphFactory.SectionText, ParagraphText(paragraphs[1]));
        Assert.Equal(AuthorsParagraphFactory.TitleText, ParagraphText(paragraphs[2]));
        Assert.Equal(string.Empty, ParagraphText(paragraphs[3]));
        Assert.Equal("Maria Silva1", ParagraphText(paragraphs[4]));

        var authorRuns = paragraphs[4].Elements<Run>().ToList();
        Assert.Equal(2, authorRuns.Count);
        Assert.False(IsSuperscript(authorRuns[0]));
        Assert.True(IsSuperscript(authorRuns[1]));
        Assert.Equal("1", authorRuns[1].InnerText);

        Assert.DoesNotContain(report.Entries, e => e.Level == ReportLevel.Warn || e.Level == ReportLevel.Error);
    }

    [Fact]
    public void Apply_WithNullDoi_SkipsDoiLineAndLogsWarn()
    {
        using var doc = AuthorsParagraphFactory.CreateDocumentWithAuthorsParagraph(
            AuthorsParagraphFactory.TextRun("Maria Silva"),
            AuthorsParagraphFactory.SuperscriptRun("1"));

        var ctx = new FormattingContext
        {
            Doi = null,
            ArticleTitle = AuthorsParagraphFactory.TitleText,
        };
        ctx.Authors.Add(new Author("Maria Silva", new[] { "1" }, OrcidId: null));

        var report = new Report();
        CreateRule().Apply(doc, ctx, report);

        var paragraphs = AuthorsParagraphFactory.GetBody(doc).Elements<Paragraph>().ToList();
        Assert.Equal(AuthorsParagraphFactory.SectionText, ParagraphText(paragraphs[0]));
        Assert.Equal(AuthorsParagraphFactory.TitleText, ParagraphText(paragraphs[1]));
        Assert.Equal(string.Empty, ParagraphText(paragraphs[2]));
        Assert.Equal("Maria Silva1", ParagraphText(paragraphs[3]));

        var warn = Assert.Single(report.Entries, e => e.Level == ReportLevel.Warn);
        Assert.Equal(RewriteHeaderMvpRule.MissingDoiMessage, warn.Message);
    }

    [Fact]
    public void Apply_WithTwoAuthorsAndOneOrcid_RendersBothOnSeparateParagraphs()
    {
        using var doc = AuthorsParagraphFactory.CreateDocumentWithAuthorsParagraph(
            AuthorsParagraphFactory.TextRun("Author A"),
            AuthorsParagraphFactory.SuperscriptRun("1"),
            AuthorsParagraphFactory.TextRun(", Author B"),
            AuthorsParagraphFactory.SuperscriptRun("2"));

        var ctx = new FormattingContext
        {
            Doi = "10.1234/abc",
            ArticleTitle = AuthorsParagraphFactory.TitleText,
        };
        ctx.Authors.Add(new Author("Author A", new[] { "1" }, OrcidId: null));
        ctx.Authors.Add(new Author("Author B", new[] { "2" }, OrcidId: "0000-0002-1825-0097"));

        var report = new Report();
        CreateRule().Apply(doc, ctx, report);

        var paragraphs = AuthorsParagraphFactory.GetBody(doc).Elements<Paragraph>().ToList();
        Assert.Equal("Author A1", ParagraphText(paragraphs[4]));
        Assert.Equal("Author B2 0000-0002-1825-0097", ParagraphText(paragraphs[5]));

        var aRuns = paragraphs[4].Elements<Run>().ToList();
        Assert.Equal(2, aRuns.Count);
        Assert.True(IsSuperscript(aRuns[1]));

        var bRuns = paragraphs[5].Elements<Run>().ToList();
        Assert.Equal(3, bRuns.Count);
        Assert.False(IsSuperscript(bRuns[0]));
        Assert.True(IsSuperscript(bRuns[1]));
        Assert.False(IsSuperscript(bRuns[2]));
        Assert.Equal(" 0000-0002-1825-0097", bRuns[2].InnerText);
    }

    [Fact]
    public void Apply_WithEmptyAuthorsList_LogsWarn_StillWritesDoiLine_AndLeavesPlaceholderParagraphIntact()
    {
        using var doc = AuthorsParagraphFactory.CreateDocumentWithAuthorsParagraph(
            AuthorsParagraphFactory.TextRun("placeholder text"));

        var ctx = new FormattingContext
        {
            Doi = "10.1234/abc",
            ArticleTitle = AuthorsParagraphFactory.TitleText,
        };

        var report = new Report();
        CreateRule().Apply(doc, ctx, report);

        var paragraphs = AuthorsParagraphFactory.GetBody(doc).Elements<Paragraph>().ToList();
        Assert.Equal("10.1234/abc", ParagraphText(paragraphs[0]));
        Assert.Equal(AuthorsParagraphFactory.SectionText, ParagraphText(paragraphs[1]));
        Assert.Equal(AuthorsParagraphFactory.TitleText, ParagraphText(paragraphs[2]));
        Assert.Equal("placeholder text", ParagraphText(paragraphs[3]));

        var warn = Assert.Single(report.Entries, e => e.Level == ReportLevel.Warn);
        Assert.Equal(RewriteHeaderMvpRule.EmptyAuthorsMessage, warn.Message);
    }

    [Fact]
    public void Apply_WithNullArticleTitle_ThrowsCriticalReferencingArticleTitle()
    {
        using var doc = AuthorsParagraphFactory.CreateDocumentWithAuthorsParagraph(
            AuthorsParagraphFactory.TextRun("ignored"));

        var ctx = new FormattingContext
        {
            Doi = "10.1234/abc",
            ArticleTitle = null,
        };
        ctx.Authors.Add(new Author("Maria Silva", new[] { "1" }, OrcidId: null));

        var report = new Report();
        var ex = Assert.Throws<InvalidOperationException>(() => CreateRule().Apply(doc, ctx, report));
        Assert.Equal(RewriteHeaderMvpRule.MissingArticleTitleMessage, ex.Message);
        Assert.Contains("ArticleTitle", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Apply_WithBodyContentBelowAuthors_LeavesEverythingBelowUnchanged()
    {
        var section = new Paragraph(new Run(new Text("Original Article")));
        var title = new Paragraph(new Run(new Text("On X")));
        var authors = new Paragraph(
            new Run(new Text("Maria Silva")),
            new Run(
                new RunProperties(new VerticalTextAlignment { Val = VerticalPositionValues.Superscript }),
                new Text("1")));

        var affiliation = new Paragraph(
            new Run(
                new RunProperties(new VerticalTextAlignment { Val = VerticalPositionValues.Superscript }),
                new Text("1")),
            new Run(new Text("Universidade Federal X")));
        var history = new Paragraph(new Run(new Text("Received: 2024-01-01")));
        var abstractPara = new Paragraph(
            new Run(new RunProperties(new Bold()), new Text("Abstract")),
            new Run(new Text(" - body text")));
        var bodyPara = new Paragraph(new Run(new Text("Some body content with citations [1].")));
        var references = new Paragraph(new Run(new Text("References: ...")));

        var stream = new MemoryStream();
        using var doc = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document);
        var mainPart = doc.AddMainDocumentPart();
        mainPart.Document = new Document(
            new Body(section, title, authors, affiliation, history, abstractPara, bodyPara, references));

        var capturedAfterAuthors = new[] { affiliation, history, abstractPara, bodyPara, references }
            .Select(p => p.OuterXml)
            .ToList();

        var ctx = new FormattingContext
        {
            Doi = "10.1234/abc",
            ArticleTitle = "On X",
        };
        ctx.Authors.Add(new Author("Maria Silva", new[] { "1" }, OrcidId: null));

        var report = new Report();
        CreateRule().Apply(doc, ctx, report);

        var body = mainPart.Document.Body!;
        var paragraphs = body.Elements<Paragraph>().ToList();
        Assert.Equal("10.1234/abc", ParagraphText(paragraphs[0]));
        Assert.Equal("Original Article", ParagraphText(paragraphs[1]));
        Assert.Equal("On X", ParagraphText(paragraphs[2]));
        Assert.Equal(string.Empty, ParagraphText(paragraphs[3]));
        Assert.Equal("Maria Silva1", ParagraphText(paragraphs[4]));

        var afterAuthorsXml = paragraphs.Skip(5).Select(p => p.OuterXml).ToList();
        Assert.Equal(capturedAfterAuthors, afterAuthorsXml);
    }

    [Fact]
    public void Apply_WithAuthorWithoutLabels_RendersNameOnly()
    {
        using var doc = AuthorsParagraphFactory.CreateDocumentWithAuthorsParagraph(
            AuthorsParagraphFactory.TextRun("Anonymous Author"));

        var ctx = new FormattingContext
        {
            Doi = null,
            ArticleTitle = AuthorsParagraphFactory.TitleText,
        };
        ctx.Authors.Add(new Author("Anonymous Author", Array.Empty<string>(), OrcidId: null));

        var report = new Report();
        CreateRule().Apply(doc, ctx, report);

        var paragraphs = AuthorsParagraphFactory.GetBody(doc).Elements<Paragraph>().ToList();
        var authorParagraph = paragraphs.Last();
        Assert.Equal("Anonymous Author", ParagraphText(authorParagraph));
        var runs = authorParagraph.Elements<Run>().ToList();
        Assert.Single(runs);
        Assert.False(IsSuperscript(runs[0]));
    }

    [Fact]
    public void Apply_WithEmptyNameAuthorRecord_SkipsRendering()
    {
        using var doc = AuthorsParagraphFactory.CreateDocumentWithAuthorsParagraph(
            AuthorsParagraphFactory.TextRun("ignored"));

        var ctx = new FormattingContext
        {
            Doi = "10.1234/abc",
            ArticleTitle = AuthorsParagraphFactory.TitleText,
        };
        ctx.Authors.Add(new Author("Maria Silva", new[] { "1" }, OrcidId: null));
        ctx.Authors.Add(new Author(string.Empty, Array.Empty<string>(), OrcidId: null, AuthorConfidence.Low));

        var report = new Report();
        CreateRule().Apply(doc, ctx, report);

        var paragraphs = AuthorsParagraphFactory.GetBody(doc).Elements<Paragraph>().ToList();
        Assert.Equal("Maria Silva1", ParagraphText(paragraphs[4]));
        Assert.Equal(5, paragraphs.Count);
    }

    [Fact]
    public void Pipeline_FullFiveRules_WhenAuthorsParagraphMissing_StillWritesDoiLineAndWarns()
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
        var section = new Paragraph(new Run(new Text(AuthorsParagraphFactory.SectionText)));
        var title = new Paragraph(new Run(new Text(AuthorsParagraphFactory.TitleText)));

        var stream = new MemoryStream();
        using var doc = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document);
        var mainPart = doc.AddMainDocumentPart();
        mainPart.Document = new Document(new Body(table, section, title));

        var pipeline = new FormattingPipeline(new IFormattingRule[]
        {
            new ExtractTopTableRule(new FormattingOptions()),
            new ParseHeaderLinesRule(),
            new ExtractAuthorsRule(new FormattingOptions()),
            new RewriteHeaderMvpRule(new FormattingOptions()),
        });

        var ctx = new FormattingContext();
        var report = new Report();
        pipeline.Run(doc, ctx, report);

        var paragraphs = mainPart.Document.Body!.Elements<Paragraph>().ToList();
        Assert.Equal("10.1234/abc", ParagraphText(paragraphs[0]));
        Assert.Equal(AuthorsParagraphFactory.SectionText, ParagraphText(paragraphs[1]));
        Assert.Equal(AuthorsParagraphFactory.TitleText, ParagraphText(paragraphs[2]));
        Assert.Empty(mainPart.Document.Body!.Elements<Table>());

        Assert.Empty(ctx.Authors);
        Assert.DoesNotContain(report.Entries, e => e.Level == ReportLevel.Error);
        Assert.Contains(
            report.Entries,
            e => e.Level == ReportLevel.Warn && e.Message == RewriteHeaderMvpRule.EmptyAuthorsMessage);
    }

    [Fact]
    public void Pipeline_FullFiveRules_WithTwoAuthorsAndOneOrcid_ProducesExpectedHeader()
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

        using var doc = AuthorsParagraphFactory.CreateDocumentWithTopTableAndAuthors(
            table,
            AuthorsParagraphFactory.TextRun("Author A"),
            AuthorsParagraphFactory.SuperscriptRun("1"),
            AuthorsParagraphFactory.TextRun(", Author B"),
            AuthorsParagraphFactory.SuperscriptRun("2"));

        var mainPart = doc.MainDocumentPart!;
        var rel = mainPart.AddHyperlinkRelationship(new Uri("https://orcid.org/0000-0002-1825-0097"), true);
        var authorsParagraph = AuthorsParagraphFactory.GetAuthorsParagraph(doc);
        authorsParagraph.AppendChild(
            AuthorsParagraphFactory.Hyperlink(rel.Id, AuthorsParagraphFactory.TextRun("0000-0002-1825-0097")));

        var pipeline = new FormattingPipeline(new IFormattingRule[]
        {
            new ExtractTopTableRule(new FormattingOptions()),
            new ParseHeaderLinesRule(),
            new ExtractAuthorsRule(new FormattingOptions()),
            new RewriteHeaderMvpRule(new FormattingOptions()),
        });

        var ctx = new FormattingContext();
        var report = new Report();
        pipeline.Run(doc, ctx, report);

        var paragraphs = AuthorsParagraphFactory.GetBody(doc).Elements<Paragraph>().ToList();
        Assert.Equal("10.1234/abc", ParagraphText(paragraphs[0]));
        Assert.Equal(AuthorsParagraphFactory.SectionText, ParagraphText(paragraphs[1]));
        Assert.Equal(AuthorsParagraphFactory.TitleText, ParagraphText(paragraphs[2]));
        Assert.Equal(string.Empty, ParagraphText(paragraphs[3]));
        Assert.Equal("Author A1", ParagraphText(paragraphs[4]));
        Assert.Equal("Author B2 0000-0002-1825-0097", ParagraphText(paragraphs[5]));

        Assert.Empty(AuthorsParagraphFactory.GetBody(doc).Descendants<Hyperlink>());
        Assert.Empty(mainPart.HyperlinkRelationships);
        Assert.DoesNotContain(report.Entries, e => e.Level == ReportLevel.Error);
    }

    [Fact]
    public void Apply_RuntimeRunsAreFormattedAsTimesNewRoman12pt()
    {
        using var doc = AuthorsParagraphFactory.CreateDocumentWithAuthorsParagraph(
            AuthorsParagraphFactory.TextRun("Maria Silva"),
            AuthorsParagraphFactory.SuperscriptRun("1"));

        var ctx = new FormattingContext
        {
            Doi = "10.1234/abc",
            ArticleTitle = AuthorsParagraphFactory.TitleText,
        };
        ctx.Authors.Add(new Author("Maria Silva", new[] { "1" }, OrcidId: "0000-0002-1825-0097"));

        var report = new Report();
        CreateRule().Apply(doc, ctx, report);

        var paragraphs = AuthorsParagraphFactory.GetBody(doc).Elements<Paragraph>().ToList();

        var doiRun = paragraphs[0].Elements<Run>().Single();
        AssertTimesNewRoman12(doiRun);

        var authorRuns = paragraphs[4].Elements<Run>().ToList();
        Assert.Equal(3, authorRuns.Count);
        AssertTimesNewRoman12(authorRuns[0]); // name
        AssertTimesNewRoman12(authorRuns[1]); // superscript label
        Assert.True(IsSuperscript(authorRuns[1]));
        AssertTimesNewRoman12(authorRuns[2]); // ORCID id

        Assert.DoesNotContain(report.Entries, e => e.Level == ReportLevel.Error);
    }

    [Fact]
    public void Apply_WithAuthorsAcrossTwoParagraphs_RemovesBothAndInsertsRewrittenBlock()
    {
        // Bug B regression: when ExtractAuthorsRule pulls authors from two
        // consecutive paragraphs, the rewriter must remove BOTH original
        // paragraphs (not just the first) before inserting the new block.
        using var doc = AuthorsParagraphFactory.CreateDocumentWithAuthorsParagraph(
            AuthorsParagraphFactory.TextRun("Author A"),
            AuthorsParagraphFactory.SuperscriptRun("1"));
        var body = AuthorsParagraphFactory.GetBody(doc);
        var secondAuthors = new Paragraph(
            new Run(new Text("Author B") { Space = SpaceProcessingModeValues.Preserve }),
            AuthorsParagraphFactory.SuperscriptRun("2"));
        body.AppendChild(secondAuthors);

        var ctx = new FormattingContext
        {
            Doi = "10.1234/abc",
            ArticleTitle = AuthorsParagraphFactory.TitleText,
        };
        ctx.Authors.Add(new Author("Author A", new[] { "1" }, OrcidId: null));
        ctx.Authors.Add(new Author("Author B", new[] { "2" }, OrcidId: null));

        var report = new Report();
        CreateRule().Apply(doc, ctx, report);

        var paragraphs = body.Elements<Paragraph>().ToList();
        Assert.Equal(6, paragraphs.Count);
        Assert.Equal("10.1234/abc", ParagraphText(paragraphs[0]));
        Assert.Equal(AuthorsParagraphFactory.SectionText, ParagraphText(paragraphs[1]));
        Assert.Equal(AuthorsParagraphFactory.TitleText, ParagraphText(paragraphs[2]));
        Assert.Equal(string.Empty, ParagraphText(paragraphs[3]));
        Assert.Equal("Author A1", ParagraphText(paragraphs[4]));
        Assert.Equal("Author B2", ParagraphText(paragraphs[5]));

        // Both original author paragraphs must be gone.
        Assert.DoesNotContain(secondAuthors, body.Elements<Paragraph>());
        Assert.DoesNotContain(report.Entries, e => e.Level == ReportLevel.Error);
    }

    private static void AssertTimesNewRoman12(Run run)
    {
        var props = run.RunProperties;
        Assert.NotNull(props);
        var fonts = props!.GetFirstChild<RunFonts>();
        Assert.NotNull(fonts);
        Assert.Equal("Times New Roman", fonts!.Ascii?.Value);
        Assert.Equal("Times New Roman", fonts.HighAnsi?.Value);
        var size = props.GetFirstChild<FontSize>();
        Assert.NotNull(size);
        Assert.Equal("24", size!.Val?.Value);
    }
}
