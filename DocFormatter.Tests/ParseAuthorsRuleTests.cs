using DocFormatter.Core.Models;
using DocFormatter.Core.Options;
using DocFormatter.Core.Pipeline;
using DocFormatter.Core.Rules;
using DocFormatter.Tests.Fixtures.Authors;
using DocumentFormat.OpenXml.Wordprocessing;
using Xunit;

namespace DocFormatter.Tests;

public sealed class ParseAuthorsRuleTests
{
    private static ParseAuthorsRule CreateRule() => new(new FormattingOptions());

    [Fact]
    public void Apply_WithSingleAuthorOneLabel_EmitsOneHighConfidenceAuthor()
    {
        using var doc = AuthorsParagraphFactory.CreateDocumentWithAuthorsParagraph(
            AuthorsParagraphFactory.TextRun("Maria Silva"),
            AuthorsParagraphFactory.SuperscriptRun("1"));

        var ctx = new FormattingContext();
        var report = new Report();
        CreateRule().Apply(doc, ctx, report);

        var author = Assert.Single(ctx.Authors);
        Assert.Equal("Maria Silva", author.Name);
        Assert.Equal(new[] { "1" }, author.AffiliationLabels);
        Assert.Null(author.OrcidId);
        Assert.Equal(AuthorConfidence.High, author.Confidence);
        Assert.DoesNotContain(report.Entries, e => e.Level == ReportLevel.Warn || e.Level == ReportLevel.Error);
    }

    [Fact]
    public void Apply_WithCommaSeparatedAuthors_EmitsThreeHighConfidenceAuthors()
    {
        using var doc = AuthorsParagraphFactory.CreateDocumentWithAuthorsParagraph(
            AuthorsParagraphFactory.TextRun("A"),
            AuthorsParagraphFactory.SuperscriptRun("1"),
            AuthorsParagraphFactory.TextRun(", B"),
            AuthorsParagraphFactory.SuperscriptRun("2"),
            AuthorsParagraphFactory.TextRun(", C"),
            AuthorsParagraphFactory.SuperscriptRun("1"));

        var ctx = new FormattingContext();
        var report = new Report();
        CreateRule().Apply(doc, ctx, report);

        Assert.Equal(3, ctx.Authors.Count);
        Assert.Equal("A", ctx.Authors[0].Name);
        Assert.Equal(new[] { "1" }, ctx.Authors[0].AffiliationLabels);
        Assert.Equal("B", ctx.Authors[1].Name);
        Assert.Equal(new[] { "2" }, ctx.Authors[1].AffiliationLabels);
        Assert.Equal("C", ctx.Authors[2].Name);
        Assert.Equal(new[] { "1" }, ctx.Authors[2].AffiliationLabels);
        Assert.All(ctx.Authors, a => Assert.Equal(AuthorConfidence.High, a.Confidence));
        Assert.DoesNotContain(report.Entries, e => e.Level == ReportLevel.Warn || e.Level == ReportLevel.Error);
    }

    [Fact]
    public void Apply_WithTrailingAndSeparator_SplitsThreeAuthorsWithoutLeakingAndIntoNames()
    {
        using var doc = AuthorsParagraphFactory.CreateDocumentWithAuthorsParagraph(
            AuthorsParagraphFactory.TextRun("A"),
            AuthorsParagraphFactory.SuperscriptRun("1"),
            AuthorsParagraphFactory.TextRun(", B"),
            AuthorsParagraphFactory.SuperscriptRun("2"),
            AuthorsParagraphFactory.TextRun(" and C"),
            AuthorsParagraphFactory.SuperscriptRun("3"));

        var ctx = new FormattingContext();
        var report = new Report();
        CreateRule().Apply(doc, ctx, report);

        Assert.Equal(3, ctx.Authors.Count);
        Assert.Equal(new[] { "A", "B", "C" }, ctx.Authors.Select(a => a.Name));
        Assert.Equal(new[] { "1", "2", "3" }, ctx.Authors.SelectMany(a => a.AffiliationLabels));
        Assert.DoesNotContain(ctx.Authors, a => a.Name.Contains(" and ", StringComparison.Ordinal));
        Assert.All(ctx.Authors, a => Assert.Equal(AuthorConfidence.High, a.Confidence));
    }

    [Fact]
    public void Apply_WithStagedOrcid_AttachesIdToAuthor()
    {
        using var doc = AuthorsParagraphFactory.CreateDocumentWithAuthorsParagraph(
            AuthorsParagraphFactory.TextRun("José Silva"),
            AuthorsParagraphFactory.SuperscriptRun("1"),
            AuthorsParagraphFactory.TextRun("0000-0002-1825-0097"));

        var ctx = new FormattingContext();
        ctx.OrcidStaging[2] = "0000-0002-1825-0097";

        var report = new Report();
        CreateRule().Apply(doc, ctx, report);

        var author = Assert.Single(ctx.Authors);
        Assert.Equal("José Silva", author.Name);
        Assert.Equal(new[] { "1" }, author.AffiliationLabels);
        Assert.Equal("0000-0002-1825-0097", author.OrcidId);
        Assert.Equal(AuthorConfidence.High, author.Confidence);
    }

    [Fact]
    public void Apply_WithStagedOrcidFromFileUrlMarker_AttachesIdIdenticallyToHttpsCase()
    {
        using var doc = AuthorsParagraphFactory.CreateDocumentWithAuthorsParagraph(
            AuthorsParagraphFactory.TextRun("José Silva"),
            AuthorsParagraphFactory.SuperscriptRun("1"),
            AuthorsParagraphFactory.TextRun("0000-0002-1825-0097"));

        var ctx = new FormattingContext();
        ctx.OrcidStaging[2] = "0000-0002-1825-0097";

        var report = new Report();
        CreateRule().Apply(doc, ctx, report);

        var author = Assert.Single(ctx.Authors);
        Assert.Equal("0000-0002-1825-0097", author.OrcidId);
        Assert.Equal(AuthorConfidence.High, author.Confidence);
    }

    [Fact]
    public void Apply_WithMultiLabelSuperscript_SplitsLabelsByComma()
    {
        using var doc = AuthorsParagraphFactory.CreateDocumentWithAuthorsParagraph(
            AuthorsParagraphFactory.TextRun("Jane Doe"),
            AuthorsParagraphFactory.SuperscriptRun("1,2"));

        var ctx = new FormattingContext();
        var report = new Report();
        CreateRule().Apply(doc, ctx, report);

        var author = Assert.Single(ctx.Authors);
        Assert.Equal("Jane Doe", author.Name);
        Assert.Equal(new[] { "1", "2" }, author.AffiliationLabels);
        Assert.Equal(AuthorConfidence.High, author.Confidence);
    }

    [Fact]
    public void Apply_WithSuspiciousJrSuffix_MarksFirstAuthorLowAndLogsWarn()
    {
        using var doc = AuthorsParagraphFactory.CreateDocumentWithAuthorsParagraph(
            AuthorsParagraphFactory.TextRun("Smith, Jr."),
            AuthorsParagraphFactory.SuperscriptRun("1"),
            AuthorsParagraphFactory.TextRun(", Jane Doe"),
            AuthorsParagraphFactory.SuperscriptRun("2"));

        var ctx = new FormattingContext();
        var report = new Report();
        CreateRule().Apply(doc, ctx, report);

        Assert.Equal(3, ctx.Authors.Count);
        Assert.Equal("Smith", ctx.Authors[0].Name);
        Assert.Equal(AuthorConfidence.Low, ctx.Authors[0].Confidence);
        Assert.Equal("Jr.", ctx.Authors[1].Name);
        Assert.Equal(AuthorConfidence.Low, ctx.Authors[1].Confidence);
        Assert.Equal("Jane Doe", ctx.Authors[2].Name);
        Assert.Equal(AuthorConfidence.High, ctx.Authors[2].Confidence);

        Assert.Contains(
            report.Entries,
            e => e.Level == ReportLevel.Warn
                && e.Message.Contains("Smith", StringComparison.Ordinal)
                && e.Message.Contains("Jr.", StringComparison.Ordinal));
    }

    [Fact]
    public void Apply_WithEmptyFragmentBetweenCommas_MarksFragmentLowAndLogsWarn()
    {
        using var doc = AuthorsParagraphFactory.CreateDocumentWithAuthorsParagraph(
            AuthorsParagraphFactory.TextRun("A, , B"),
            AuthorsParagraphFactory.SuperscriptRun("1"));

        var ctx = new FormattingContext();
        var report = new Report();
        CreateRule().Apply(doc, ctx, report);

        Assert.Equal(3, ctx.Authors.Count);
        Assert.Equal("A", ctx.Authors[0].Name);
        Assert.Equal(string.Empty, ctx.Authors[1].Name);
        Assert.Equal(AuthorConfidence.Low, ctx.Authors[1].Confidence);
        Assert.Equal("B", ctx.Authors[2].Name);
        Assert.Contains(
            report.Entries,
            e => e.Level == ReportLevel.Warn && e.Message.Contains("empty name", StringComparison.Ordinal));
    }

    [Fact]
    public void Apply_WithNonAlphabeticFragment_MarksAuthorLowAndLogsWarn()
    {
        using var doc = AuthorsParagraphFactory.CreateDocumentWithAuthorsParagraph(
            AuthorsParagraphFactory.TextRun("Alice, 123, Bob"));

        var ctx = new FormattingContext();
        var report = new Report();
        CreateRule().Apply(doc, ctx, report);

        Assert.Equal(3, ctx.Authors.Count);
        Assert.Equal("123", ctx.Authors[1].Name);
        Assert.Equal(AuthorConfidence.Low, ctx.Authors[1].Confidence);
        Assert.Contains(
            report.Entries,
            e => e.Level == ReportLevel.Warn && e.Message.Contains("alphabetic", StringComparison.Ordinal));
    }

    [Fact]
    public void Apply_WithMissingAuthorsParagraph_LogsWarnAndLeavesAuthorsEmpty()
    {
        using var doc = AuthorsParagraphFactory.CreateDocumentWithoutAuthorsParagraph();

        var ctx = new FormattingContext();
        var report = new Report();
        CreateRule().Apply(doc, ctx, report);

        Assert.Empty(ctx.Authors);
        var warn = Assert.Single(report.Entries, e => e.Level == ReportLevel.Warn);
        Assert.Equal(ParseAuthorsRule.MissingAuthorsParagraphMessage, warn.Message);
    }

    [Fact]
    public void Pipeline_WithExtractTopTableThenHeaderLinesThenOrcidThenAuthors_AttachesOrcidAndDropsHyperlink()
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
            AuthorsParagraphFactory.TextRun("José Silva"),
            AuthorsParagraphFactory.SuperscriptRun("1"));

        var mainPart = doc.MainDocumentPart!;
        var rel = mainPart.AddHyperlinkRelationship(new Uri("https://orcid.org/0000-0002-1825-0097"), true);
        var authorsParagraph = AuthorsParagraphFactory.GetAuthorsParagraph(doc);
        authorsParagraph.AppendChild(
            AuthorsParagraphFactory.Hyperlink(rel.Id, AuthorsParagraphFactory.TextRun("0000-0002-1825-0097")));

        var pipeline = new FormattingPipeline(new IFormattingRule[]
        {
            new ExtractTopTableRule(new FormattingOptions()),
            new ParseHeaderLinesRule(),
            new ExtractOrcidLinksRule(new FormattingOptions()),
            new ParseAuthorsRule(new FormattingOptions()),
        });
        var ctx = new FormattingContext();
        var report = new Report();

        pipeline.Run(doc, ctx, report);

        var author = Assert.Single(ctx.Authors);
        Assert.Equal("José Silva", author.Name);
        Assert.Equal(new[] { "1" }, author.AffiliationLabels);
        Assert.Equal("0000-0002-1825-0097", author.OrcidId);
        Assert.Equal(AuthorConfidence.High, author.Confidence);
        Assert.Empty(authorsParagraph.Descendants<Hyperlink>());
        Assert.Empty(mainPart.HyperlinkRelationships);
        Assert.DoesNotContain(report.Entries, e => e.Level == ReportLevel.Error);
    }
}
