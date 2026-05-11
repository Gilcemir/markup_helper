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
        // ADR-008: the trailing "*" superscript run is the corresp marker; it merges into
        // the preceding affiliation label so the produced superscript becomes "1,2*" (which
        // Markup's mark_authors handles) instead of "1,2,*" (which it mis-splits at the
        // inner comma).
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
        Assert.Equal(new[] { "1", "2*" }, ctx.Authors[1].AffiliationLabels);
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
    public void Apply_WhenSectionAndTitleShareFirstParagraph_LocatesAuthorsAtSecondParagraph()
    {
        // Bug B+C interaction: artigo 4 da pasta examples/ has section+title
        // packed into P[0] with a <w:br/> between them. Counting paragraphs
        // would put authors at the third non-empty paragraph (which is the
        // affiliation line). Counting logical lines correctly puts authors at
        // the second paragraph (line 3 overall: section, title, authors).
        var stream = new MemoryStream();
        using var doc = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document);
        var mainPart = doc.AddMainDocumentPart();
        var firstParagraph = new Paragraph(
            new Run(new Text("ARTICLE") { Space = SpaceProcessingModeValues.Preserve }),
            new Run(new Break()),
            new Run(new Text("Long Title Goes Here") { Space = SpaceProcessingModeValues.Preserve }));
        var authorsParagraph = new Paragraph(
            new Run(new Text("Author A") { Space = SpaceProcessingModeValues.Preserve }),
            AuthorsParagraphFactory.SuperscriptRun("1"),
            new Run(new Text(", Author B") { Space = SpaceProcessingModeValues.Preserve }),
            AuthorsParagraphFactory.SuperscriptRun("2"));
        var affiliation = new Paragraph(
            AuthorsParagraphFactory.SuperscriptRun("1"),
            new Run(new Text(" Universidade X") { Space = SpaceProcessingModeValues.Preserve }));
        mainPart.Document = new Document(new Body(firstParagraph, authorsParagraph, affiliation));

        var ctx = new FormattingContext();
        var report = new Report();
        CreateRule().Apply(doc, ctx, report);

        Assert.Equal(2, ctx.Authors.Count);
        Assert.Equal("Author A", ctx.Authors[0].Name);
        Assert.Equal("Author B", ctx.Authors[1].Name);
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
    public void Apply_WithSeparateAsteriskSuperscriptAfterDigit_MergesAsteriskOntoLastLabel_5313Shape()
    {
        // ADR-008: 9_CR_5313_2.docx Stage-1 input — the affected author's superscript is
        // emitted as two adjacent superscript runs: "1" then "*". Pre-fix the rule produced
        // ["1", "*"], which RewriteHeaderMvpRule comma-joined as "1,*" — Markup's
        // mark_authors splits on that inner comma and fails to auto-mark the author.
        // Post-fix the asterisk is folded onto the trailing label, producing ["1*"] →
        // emitted superscript "1*" (matching the canonical 5136 shape).
        using var doc = AuthorsParagraphFactory.CreateDocumentWithAuthorsParagraph(
            AuthorsParagraphFactory.Build5313FailureShape("Flavia Alves Silva"));

        var ctx = new FormattingContext();
        var report = new Report();
        CreateRule().Apply(doc, ctx, report);

        var author = Assert.Single(ctx.Authors);
        Assert.Equal("Flavia Alves Silva", author.Name);
        Assert.Equal(new[] { "1*" }, author.AffiliationLabels);
        Assert.Equal(AuthorConfidence.High, author.Confidence);
        Assert.DoesNotContain(report.Entries, e => e.Level == ReportLevel.Warn || e.Level == ReportLevel.Error);
    }

    [Fact]
    public void Apply_WithSeparateAsteriskSuperscriptAfterMultiDigit_MergesOntoLastLabelOnly_5449Shape()
    {
        // ADR-008: 1_AR_5449_2.docx Stage-1 input — the corresponding author's superscript
        // is emitted as "1,2" (single run, comma-internal) followed by a separate "*" run.
        // The asterisk is the corresp marker for that single author and must merge onto the
        // trailing aff label only ("2" → "2*"), not onto the entire list. The earlier "1"
        // entry keeps its own slot. Result: ["1", "2*"] → emitted superscript "1,2*".
        using var doc = AuthorsParagraphFactory.CreateDocumentWithAuthorsParagraph(
            AuthorsParagraphFactory.Build5449FailureShape("Hoang Dang Khoa Do"));

        var ctx = new FormattingContext();
        var report = new Report();
        CreateRule().Apply(doc, ctx, report);

        var author = Assert.Single(ctx.Authors);
        Assert.Equal("Hoang Dang Khoa Do", author.Name);
        Assert.Equal(new[] { "1", "2*" }, author.AffiliationLabels);
        Assert.Equal(AuthorConfidence.High, author.Confidence);
        Assert.DoesNotContain(report.Entries, e => e.Level == ReportLevel.Warn || e.Level == ReportLevel.Error);
    }

    [Fact]
    public void Apply_WithCommaSeparatedAsteriskInsideSingleSuperscriptRun_MergesAsteriskAfterSplit()
    {
        // Variant of the 5313 shape: instead of two adjacent superscript runs, a single run
        // contains "1,*". SplitLabels produces ["1", "*"] tokens and the merge folds them
        // into ["1*"]. Same observable post-fix shape as the two-run variant.
        using var doc = AuthorsParagraphFactory.CreateDocumentWithAuthorsParagraph(
            AuthorsParagraphFactory.TextRun("Solo Author"),
            AuthorsParagraphFactory.SuperscriptRun("1,*"));

        var ctx = new FormattingContext();
        var report = new Report();
        CreateRule().Apply(doc, ctx, report);

        var author = Assert.Single(ctx.Authors);
        Assert.Equal("Solo Author", author.Name);
        Assert.Equal(new[] { "1*" }, author.AffiliationLabels);
        Assert.Equal(AuthorConfidence.High, author.Confidence);
    }

    [Fact]
    public void Apply_WithLeadingAsteriskOnlyLabel_KeepsAsteriskAsFirstLabel()
    {
        // Boundary: an author whose only label is the corresp asterisk (no aff index). The
        // merge predicate requires a previous label; with none, the asterisk is kept as
        // ["*"]. Information is preserved for downstream Phase 3 work.
        using var doc = AuthorsParagraphFactory.CreateDocumentWithAuthorsParagraph(
            AuthorsParagraphFactory.TextRun("Corresp Only"),
            AuthorsParagraphFactory.SuperscriptRun("*"));

        var ctx = new FormattingContext();
        var report = new Report();
        CreateRule().Apply(doc, ctx, report);

        var author = Assert.Single(ctx.Authors);
        Assert.Equal("Corresp Only", author.Name);
        Assert.Equal(new[] { "*" }, author.AffiliationLabels);
    }

    [Fact]
    public void Apply_WithMultipleAdjacentAsteriskSuperscripts_AllMergeOntoLastLabel()
    {
        // Defensive: if for some reason a paragraph contains "1" then "*" then "*", both
        // asterisks attach to the trailing aff label. Same downstream shape, no inner
        // commas introduced.
        using var doc = AuthorsParagraphFactory.CreateDocumentWithAuthorsParagraph(
            AuthorsParagraphFactory.TextRun("Edge Case"),
            AuthorsParagraphFactory.SuperscriptRun("1"),
            AuthorsParagraphFactory.SuperscriptRun("*"),
            AuthorsParagraphFactory.SuperscriptRun("*"));

        var ctx = new FormattingContext();
        var report = new Report();
        CreateRule().Apply(doc, ctx, report);

        var author = Assert.Single(ctx.Authors);
        Assert.Equal("Edge Case", author.Name);
        Assert.Equal(new[] { "1**" }, author.AffiliationLabels);
    }

    [Fact]
    public void Apply_WithSingleAuthorAndMergedDigitAsteriskRun_PreservesShapeUntouched_5136Baseline()
    {
        // Non-regression: when the original author paragraph stores the digit and the
        // asterisk in a single superscript run (the canonical 5136 / 5548 shape), nothing
        // changes — SplitLabels yields ["1*"], no merge required, label set unaffected.
        using var doc = AuthorsParagraphFactory.CreateDocumentWithAuthorsParagraph(
            AuthorsParagraphFactory.TextRun("Talles de Oliveira Santos"),
            AuthorsParagraphFactory.SuperscriptRun("1*"));

        var ctx = new FormattingContext();
        var report = new Report();
        CreateRule().Apply(doc, ctx, report);

        var author = Assert.Single(ctx.Authors);
        Assert.Equal("Talles de Oliveira Santos", author.Name);
        Assert.Equal(new[] { "1*" }, author.AffiliationLabels);
        Assert.Equal(AuthorConfidence.High, author.Confidence);
    }

    [Fact]
    public void Apply_NoneOfProducedRunsContainBracketLiterals_AntiDuplicationInvariant()
    {
        // ADR-008 / REENTRANCE.md invariant: the fix must not pre-mark [author], [fname],
        // [surname] (Markup auto-marks those and would duplicate). Verify that none of the
        // body's run text in the post-fix author block contains those literals.
        using var doc = AuthorsParagraphFactory.CreateDocumentWithAuthorsParagraph(
            AuthorsParagraphFactory.TextRun("Author A"),
            AuthorsParagraphFactory.SuperscriptRun("1"),
            AuthorsParagraphFactory.SuperscriptRun("*"),
            AuthorsParagraphFactory.TextRun(", Author B"),
            AuthorsParagraphFactory.SuperscriptRun("2"));

        var ctx = new FormattingContext();
        var report = new Report();
        CreateRule().Apply(doc, ctx, report);

        var bodyText = string.Concat(
            AuthorsParagraphFactory
                .GetBody(doc)
                .Descendants<Text>()
                .Select(t => t.Text));
        Assert.DoesNotContain("[author]", bodyText, StringComparison.Ordinal);
        Assert.DoesNotContain("[fname]", bodyText, StringComparison.Ordinal);
        Assert.DoesNotContain("[surname]", bodyText, StringComparison.Ordinal);
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
