using DocFormatter.Core.Models;
using DocFormatter.Core.Options;
using DocFormatter.Core.Pipeline;
using DocFormatter.Core.Rules;
using DocFormatter.Tests.Fixtures.Authors;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Xunit;
using Blip = DocumentFormat.OpenXml.Drawing.Blip;

namespace DocFormatter.Tests;

public sealed class ExtractAuthorsRuleTests
{
    private const string ImageRelationshipType =
        "http://schemas.openxmlformats.org/officeDocument/2006/relationships/image";

    private static ExtractAuthorsRule CreateRule() => new(new FormattingOptions());

    private static MainDocumentPart GetMainPart(WordprocessingDocument doc)
        => doc.MainDocumentPart!;

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
        Assert.Equal(ExtractAuthorsRule.MissingAuthorsParagraphMessage, warn.Message);
    }

    [Fact]
    public void Apply_WithOrcidHyperlinkWrappingId_AttachesIdAndDropsHyperlink()
    {
        using var doc = AuthorsParagraphFactory.CreateDocumentWithAuthorsParagraph();
        var mainPart = GetMainPart(doc);
        var rel = mainPart.AddHyperlinkRelationship(new Uri("https://orcid.org/0000-0002-1825-0097"), true);
        var authors = AuthorsParagraphFactory.GetAuthorsParagraph(doc);
        authors.AppendChild(AuthorsParagraphFactory.TextRun("José Silva"));
        authors.AppendChild(AuthorsParagraphFactory.SuperscriptRun("1"));
        authors.AppendChild(AuthorsParagraphFactory.Hyperlink(
            rel.Id,
            AuthorsParagraphFactory.TextRun("0000-0002-1825-0097")));

        var ctx = new FormattingContext();
        var report = new Report();
        CreateRule().Apply(doc, ctx, report);

        var author = Assert.Single(ctx.Authors);
        Assert.Equal("José Silva", author.Name);
        Assert.Equal(new[] { "1" }, author.AffiliationLabels);
        Assert.Equal("0000-0002-1825-0097", author.OrcidId);
        Assert.Equal(AuthorConfidence.High, author.Confidence);

        Assert.Empty(authors.Elements<Hyperlink>());
        Assert.Empty(mainPart.HyperlinkRelationships);
    }

    [Fact]
    public void Apply_WithOrcidHyperlinkWrappingAuthorName_PreservesNameAndAttachesId()
    {
        // Reproducer for reviews-002/issue_001: production article 1_AR_5449_2.docx wraps
        // the author NAME inside the ORCID-targeted hyperlink. Old design dropped the name.
        using var doc = AuthorsParagraphFactory.CreateDocumentWithAuthorsParagraph();
        var mainPart = GetMainPart(doc);
        var rel1 = mainPart.AddHyperlinkRelationship(new Uri("https://orcid.org/0009-0007-2181-5830"), true);
        var rel2 = mainPart.AddHyperlinkRelationship(new Uri("https://orcid.org/0000-0002-7970-9359"), true);
        var authors = AuthorsParagraphFactory.GetAuthorsParagraph(doc);
        authors.AppendChild(AuthorsParagraphFactory.Hyperlink(
            rel1.Id, AuthorsParagraphFactory.TextRun("Thi Thanh Nga Le")));
        authors.AppendChild(AuthorsParagraphFactory.SuperscriptRun("1"));
        authors.AppendChild(AuthorsParagraphFactory.TextRun(" and "));
        authors.AppendChild(AuthorsParagraphFactory.Hyperlink(
            rel2.Id, AuthorsParagraphFactory.TextRun("Hoang Dang Khoa Do")));
        authors.AppendChild(AuthorsParagraphFactory.SuperscriptRun("1,2"));
        authors.AppendChild(AuthorsParagraphFactory.SuperscriptRun("*"));

        var ctx = new FormattingContext();
        var report = new Report();
        CreateRule().Apply(doc, ctx, report);

        Assert.Equal(2, ctx.Authors.Count);
        Assert.Equal("Thi Thanh Nga Le", ctx.Authors[0].Name);
        Assert.Equal(new[] { "1" }, ctx.Authors[0].AffiliationLabels);
        Assert.Equal("0009-0007-2181-5830", ctx.Authors[0].OrcidId);
        Assert.Equal(AuthorConfidence.High, ctx.Authors[0].Confidence);

        Assert.Equal("Hoang Dang Khoa Do", ctx.Authors[1].Name);
        Assert.Equal(new[] { "1", "2", "*" }, ctx.Authors[1].AffiliationLabels);
        Assert.Equal("0000-0002-7970-9359", ctx.Authors[1].OrcidId);
        Assert.Equal(AuthorConfidence.High, ctx.Authors[1].Confidence);

        Assert.Empty(authors.Elements<Hyperlink>());
        Assert.Empty(mainPart.HyperlinkRelationships);
        Assert.DoesNotContain(report.Entries, e => e.Level == ReportLevel.Warn || e.Level == ReportLevel.Error);
    }

    [Fact]
    public void Apply_WithOrcidHyperlinkWrappingOrcidCaptionText_TreatsAsBadgeAndDropsText()
    {
        using var doc = AuthorsParagraphFactory.CreateDocumentWithAuthorsParagraph();
        var mainPart = GetMainPart(doc);
        var rel = mainPart.AddHyperlinkRelationship(
            new Uri("file:///C:/article.docx#orcid.org/0000-0002-1825-0097"),
            true);
        var authors = AuthorsParagraphFactory.GetAuthorsParagraph(doc);
        authors.AppendChild(AuthorsParagraphFactory.TextRun("José Silva"));
        authors.AppendChild(AuthorsParagraphFactory.SuperscriptRun("1"));
        authors.AppendChild(AuthorsParagraphFactory.Hyperlink(
            rel.Id, AuthorsParagraphFactory.TextRun("ORCID")));

        var ctx = new FormattingContext();
        var report = new Report();
        CreateRule().Apply(doc, ctx, report);

        var author = Assert.Single(ctx.Authors);
        Assert.Equal("José Silva", author.Name);
        Assert.Equal("0000-0002-1825-0097", author.OrcidId);
        Assert.Empty(authors.Elements<Hyperlink>());
        Assert.Empty(mainPart.HyperlinkRelationships);
    }

    [Fact]
    public void Apply_WithOrcidHyperlinkContainingDrawing_TreatsAsBadgeAndDropsHyperlink()
    {
        using var doc = AuthorsParagraphFactory.CreateDocumentWithAuthorsParagraph();
        var mainPart = GetMainPart(doc);
        var rel = mainPart.AddHyperlinkRelationship(new Uri("https://orcid.org/0000-0002-1825-0097"), true);
        var authors = AuthorsParagraphFactory.GetAuthorsParagraph(doc);
        authors.AppendChild(AuthorsParagraphFactory.TextRun("José Silva"));
        authors.AppendChild(AuthorsParagraphFactory.SuperscriptRun("1"));
        authors.AppendChild(AuthorsParagraphFactory.Hyperlink(
            rel.Id,
            new Run(new Drawing(new Blip { Embed = rel.Id }))));

        var ctx = new FormattingContext();
        var report = new Report();
        CreateRule().Apply(doc, ctx, report);

        var author = Assert.Single(ctx.Authors);
        Assert.Equal("José Silva", author.Name);
        Assert.Equal("0000-0002-1825-0097", author.OrcidId);
        Assert.Empty(authors.Elements<Hyperlink>());
        Assert.Empty(authors.Descendants<Drawing>());
        Assert.Empty(mainPart.HyperlinkRelationships);
    }

    [Fact]
    public void Apply_WithNonOrcidHyperlinkWrappingNameFragment_TokenizesAsAuthorAndKeepsHyperlink()
    {
        using var doc = AuthorsParagraphFactory.CreateDocumentWithAuthorsParagraph();
        var mainPart = GetMainPart(doc);
        var rel = mainPart.AddHyperlinkRelationship(new Uri("https://example.com/profile"), true);
        var authors = AuthorsParagraphFactory.GetAuthorsParagraph(doc);
        authors.AppendChild(AuthorsParagraphFactory.Hyperlink(
            rel.Id, AuthorsParagraphFactory.TextRun("Maria Silva")));
        authors.AppendChild(AuthorsParagraphFactory.SuperscriptRun("1"));

        var ctx = new FormattingContext();
        var report = new Report();
        CreateRule().Apply(doc, ctx, report);

        var author = Assert.Single(ctx.Authors);
        Assert.Equal("Maria Silva", author.Name);
        Assert.Equal(new[] { "1" }, author.AffiliationLabels);
        Assert.Null(author.OrcidId);

        Assert.Single(authors.Elements<Hyperlink>());
        Assert.Single(mainPart.HyperlinkRelationships);
    }

    [Fact]
    public void Apply_WithOrcidUrlButGarbledId_LogsWarnAndStillTokenizesInnerText()
    {
        using var doc = AuthorsParagraphFactory.CreateDocumentWithAuthorsParagraph();
        var mainPart = GetMainPart(doc);
        var rel = mainPart.AddHyperlinkRelationship(new Uri("https://orcid.org/garbled-id"), true);
        var authors = AuthorsParagraphFactory.GetAuthorsParagraph(doc);
        authors.AppendChild(AuthorsParagraphFactory.Hyperlink(
            rel.Id, AuthorsParagraphFactory.TextRun("Maria Silva")));
        authors.AppendChild(AuthorsParagraphFactory.SuperscriptRun("1"));

        var ctx = new FormattingContext();
        var report = new Report();
        CreateRule().Apply(doc, ctx, report);

        var author = Assert.Single(ctx.Authors);
        Assert.Equal("Maria Silva", author.Name);
        Assert.Equal(new[] { "1" }, author.AffiliationLabels);
        Assert.Null(author.OrcidId);

        Assert.Single(authors.Elements<Hyperlink>());
        Assert.Single(mainPart.HyperlinkRelationships);
        Assert.Contains(
            report.Entries,
            e => e.Level == ReportLevel.Warn && e.Message.Contains("orcid.org", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Apply_WithFreeStandingOrcidBadgeBeforeHyperlink_LeavesDrawingAndLogsWarn()
    {
        using var doc = AuthorsParagraphFactory.CreateDocumentWithAuthorsParagraph();
        var mainPart = GetMainPart(doc);
        var imageRel = mainPart.AddExternalRelationship(
            ImageRelationshipType,
            new Uri("https://orcid.org/badge.png"));
        var hyperlinkRel = mainPart.AddHyperlinkRelationship(
            new Uri("https://orcid.org/0000-0002-1825-0097"),
            true);
        var authors = AuthorsParagraphFactory.GetAuthorsParagraph(doc);
        authors.AppendChild(AuthorsParagraphFactory.TextRun("Maria Silva"));
        authors.AppendChild(AuthorsParagraphFactory.SuperscriptRun("1"));
        authors.AppendChild(new Run(new Drawing(new Blip { Embed = imageRel.Id })));
        authors.AppendChild(AuthorsParagraphFactory.Hyperlink(
            hyperlinkRel.Id, AuthorsParagraphFactory.TextRun("0000-0002-1825-0097")));

        var ctx = new FormattingContext();
        var report = new Report();
        CreateRule().Apply(doc, ctx, report);

        var author = Assert.Single(ctx.Authors);
        Assert.Equal("Maria Silva", author.Name);
        Assert.Equal("0000-0002-1825-0097", author.OrcidId);

        Assert.Single(authors.Descendants<Drawing>());
        Assert.Single(authors.Descendants<Blip>(), b => b.Embed?.Value == imageRel.Id);
        Assert.Contains(
            report.Entries,
            e => e.Level == ReportLevel.Warn && e.Message.Contains("free-standing", StringComparison.OrdinalIgnoreCase));
        Assert.Single(mainPart.ExternalRelationships);
        Assert.Empty(mainPart.HyperlinkRelationships);
    }

    [Fact]
    public void Pipeline_FullPipeline_WithIssue001Reproducer_ProducesBothNamedAuthors()
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

        using var doc = AuthorsParagraphFactory.CreateDocumentWithTopTableAndAuthors(table);
        var mainPart = doc.MainDocumentPart!;
        var rel1 = mainPart.AddHyperlinkRelationship(new Uri("https://orcid.org/0009-0007-2181-5830"), true);
        var rel2 = mainPart.AddHyperlinkRelationship(new Uri("https://orcid.org/0000-0002-7970-9359"), true);
        var authors = AuthorsParagraphFactory.GetAuthorsParagraph(doc);
        authors.AppendChild(AuthorsParagraphFactory.Hyperlink(
            rel1.Id, AuthorsParagraphFactory.TextRun("Thi Thanh Nga Le")));
        authors.AppendChild(AuthorsParagraphFactory.SuperscriptRun("1"));
        authors.AppendChild(AuthorsParagraphFactory.TextRun(" and "));
        authors.AppendChild(AuthorsParagraphFactory.Hyperlink(
            rel2.Id, AuthorsParagraphFactory.TextRun("Hoang Dang Khoa Do")));
        authors.AppendChild(AuthorsParagraphFactory.SuperscriptRun("1,2"));
        authors.AppendChild(AuthorsParagraphFactory.SuperscriptRun("*"));

        var pipeline = new FormattingPipeline(new IFormattingRule[]
        {
            new ExtractTopTableRule(new FormattingOptions()),
            new ParseHeaderLinesRule(),
            new ExtractAuthorsRule(new FormattingOptions()),
        });
        var ctx = new FormattingContext();
        var report = new Report();
        pipeline.Run(doc, ctx, report);

        Assert.Equal(2, ctx.Authors.Count);
        Assert.Equal("Thi Thanh Nga Le", ctx.Authors[0].Name);
        Assert.Equal("0009-0007-2181-5830", ctx.Authors[0].OrcidId);
        Assert.Equal("Hoang Dang Khoa Do", ctx.Authors[1].Name);
        Assert.Equal("0000-0002-7970-9359", ctx.Authors[1].OrcidId);
        Assert.Empty(AuthorsParagraphFactory.GetBody(doc).Elements<Table>());
        Assert.DoesNotContain(report.Entries, e => e.Level == ReportLevel.Error);
    }

    [Fact]
    public void Apply_WithAndSeparatorFragmentedAcrossThreeRuns_SplitsCorrectly()
    {
        // Mirrors artigos 2, 5, 9 da pasta examples/: the " and " separator is
        // split into 3 runs (" ", "and", " ") because Word inserted proofErr
        // markers (or a superscript whitespace) between them. Each fragment
        // arrives in its own ProcessTextRun call in the old design, so the
        // separator never matched. Tokenize+consume merges adjacent text into
        // a single buffer before searching for separators.
        using var doc = AuthorsParagraphFactory.CreateDocumentWithAuthorsParagraph(
            AuthorsParagraphFactory.TextRun("Author A"),
            AuthorsParagraphFactory.SuperscriptRun("1"),
            AuthorsParagraphFactory.TextRun(" "),
            AuthorsParagraphFactory.TextRun("and"),
            AuthorsParagraphFactory.TextRun(" "),
            AuthorsParagraphFactory.TextRun("Author B"),
            AuthorsParagraphFactory.SuperscriptRun("2"));

        var ctx = new FormattingContext();
        var report = new Report();
        CreateRule().Apply(doc, ctx, report);

        Assert.Equal(2, ctx.Authors.Count);
        Assert.Equal("Author A", ctx.Authors[0].Name);
        Assert.Equal("Author B", ctx.Authors[1].Name);
        Assert.DoesNotContain(ctx.Authors, a => a.Name.Contains("and", StringComparison.Ordinal));
    }

    [Fact]
    public void Apply_WithCommaSeparatorFragmentedAcrossTwoRuns_SplitsCorrectly()
    {
        // Mirrors artigo 7 da pasta examples/: ", " separator is split into
        // two adjacent runs ("," and " ") which the old per-run matcher missed.
        using var doc = AuthorsParagraphFactory.CreateDocumentWithAuthorsParagraph(
            AuthorsParagraphFactory.TextRun("Author A"),
            AuthorsParagraphFactory.SuperscriptRun("1"),
            AuthorsParagraphFactory.TextRun(","),
            AuthorsParagraphFactory.TextRun(" "),
            AuthorsParagraphFactory.TextRun("Author B"),
            AuthorsParagraphFactory.SuperscriptRun("2"));

        var ctx = new FormattingContext();
        var report = new Report();
        CreateRule().Apply(doc, ctx, report);

        Assert.Equal(2, ctx.Authors.Count);
        Assert.Equal("Author A", ctx.Authors[0].Name);
        Assert.Equal("Author B", ctx.Authors[1].Name);
    }

    [Fact]
    public void Apply_WithSuperscriptWhitespaceBeforeAndSeparator_SplitsCorrectly()
    {
        // Exact shape of artigo 5 (and 10): the space before "and" is
        // accidentally formatted as superscript. Whitespace-only superscript
        // runs must not be silently dropped — they need to be preserved as
        // text so the " and " separator can match.
        using var doc = AuthorsParagraphFactory.CreateDocumentWithAuthorsParagraph(
            AuthorsParagraphFactory.TextRun("Gabriel"),
            AuthorsParagraphFactory.SuperscriptRun("1"),
            AuthorsParagraphFactory.SuperscriptRun(" "),
            AuthorsParagraphFactory.TextRun("and"),
            AuthorsParagraphFactory.TextRun(" "),
            AuthorsParagraphFactory.TextRun("Peggy"),
            AuthorsParagraphFactory.SuperscriptRun("2"));

        var ctx = new FormattingContext();
        var report = new Report();
        CreateRule().Apply(doc, ctx, report);

        Assert.Equal(2, ctx.Authors.Count);
        Assert.Equal("Gabriel", ctx.Authors[0].Name);
        Assert.Equal(new[] { "1" }, ctx.Authors[0].AffiliationLabels);
        Assert.Equal("Peggy", ctx.Authors[1].Name);
        Assert.Equal(new[] { "2" }, ctx.Authors[1].AffiliationLabels);
    }

    [Fact]
    public void Apply_WithAuthorsAcrossTwoParagraphs_StoppedByAffiliationLine_EmitsBothAuthors()
    {
        // Mirrors artigo 1 da pasta examples/: 2 authors split across separate
        // paragraphs, then an affiliation paragraph that begins with a
        // superscript number which signals the end of the author block.
        using var doc = AuthorsParagraphFactory.CreateDocumentWithAuthorsParagraph(
            AuthorsParagraphFactory.TextRun("Thi Thanh Nga Le"),
            AuthorsParagraphFactory.SuperscriptRun("1"));
        var body = AuthorsParagraphFactory.GetBody(doc);
        var secondAuthors = new Paragraph(
            new Run(new Text("Hoang Dang Khoa Do") { Space = SpaceProcessingModeValues.Preserve }),
            AuthorsParagraphFactory.SuperscriptRun("1,2"));
        var affiliation = new Paragraph(
            AuthorsParagraphFactory.SuperscriptRun("1"),
            new Run(new Text(" Faculty of Biology") { Space = SpaceProcessingModeValues.Preserve }));
        body.AppendChild(secondAuthors);
        body.AppendChild(affiliation);

        var ctx = new FormattingContext();
        var report = new Report();
        CreateRule().Apply(doc, ctx, report);

        Assert.Equal(2, ctx.Authors.Count);
        Assert.Equal("Thi Thanh Nga Le", ctx.Authors[0].Name);
        Assert.Equal(new[] { "1" }, ctx.Authors[0].AffiliationLabels);
        Assert.Equal("Hoang Dang Khoa Do", ctx.Authors[1].Name);
        Assert.Equal(new[] { "1", "2" }, ctx.Authors[1].AffiliationLabels);
        Assert.DoesNotContain(report.Entries, e => e.Level == ReportLevel.Error);
    }

    [Fact]
    public void Apply_WithLocalPathHyperlinkContainingOrcidId_ExtractsIdAndDropsHyperlink()
    {
        using var doc = AuthorsParagraphFactory.CreateDocumentWithAuthorsParagraph();
        var mainPart = GetMainPart(doc);
        var rel = mainPart.AddHyperlinkRelationship(
            new Uri("file:///Users/educbank/Documents/personal_workspace/markup_helper/examples/0000-0002-8233-7883"),
            true);
        var authors = AuthorsParagraphFactory.GetAuthorsParagraph(doc);
        authors.AppendChild(AuthorsParagraphFactory.TextRun("Maria Silva"));
        authors.AppendChild(AuthorsParagraphFactory.SuperscriptRun("1"));
        authors.AppendChild(AuthorsParagraphFactory.Hyperlink(
            rel.Id, AuthorsParagraphFactory.TextRun("ORCID")));

        var ctx = new FormattingContext();
        var report = new Report();
        CreateRule().Apply(doc, ctx, report);

        var author = Assert.Single(ctx.Authors);
        Assert.Equal("Maria Silva", author.Name);
        Assert.Equal(new[] { "1" }, author.AffiliationLabels);
        Assert.Equal("0000-0002-8233-7883", author.OrcidId);
        Assert.Empty(authors.Elements<Hyperlink>());
        Assert.Empty(mainPart.HyperlinkRelationships);
    }
}
