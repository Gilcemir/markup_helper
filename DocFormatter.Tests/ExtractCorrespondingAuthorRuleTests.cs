using DocFormatter.Core.Models;
using DocFormatter.Core.Options;
using DocFormatter.Core.Pipeline;
using DocFormatter.Core.Rules;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Xunit;

namespace DocFormatter.Tests;

public sealed class ExtractCorrespondingAuthorRuleTests
{
    private static ExtractCorrespondingAuthorRule CreateRule()
        => new(new FormattingOptions());

    private static WordprocessingDocument CreateDocumentWith(params OpenXmlElement[] bodyChildren)
    {
        var stream = new MemoryStream();
        var doc = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document);
        var mainPart = doc.AddMainDocumentPart();
        mainPart.Document = new Document(new Body(bodyChildren));
        return doc;
    }

    private static Run TextRun(string text)
        => new(new Text(text) { Space = SpaceProcessingModeValues.Preserve });

    private static Run SuperscriptRun(string text)
    {
        var properties = new RunProperties(
            new VerticalTextAlignment { Val = VerticalPositionValues.Superscript });
        return new Run(properties, new Text(text) { Space = SpaceProcessingModeValues.Preserve });
    }

    private static Paragraph Paragraph(params OpenXmlElement[] runs) => new(runs);

    private static string PlainText(Paragraph paragraph)
        => string.Concat(paragraph.Descendants<Text>().Select(t => t.Text));

    [Fact]
    public void Apply_WithEmailOnlyTrailer_StripsTrailerAndExtractsEmail()
    {
        var authorsParagraph = Paragraph(TextRun("Maria Silva"), SuperscriptRun("1*"));
        var affiliation = Paragraph(TextRun("2 Universidade Y * E-mail: foo@y.edu"));

        using var doc = CreateDocumentWith(authorsParagraph, affiliation);

        var ctx = new FormattingContext();
        ctx.AuthorParagraphs.Add(authorsParagraph);
        ctx.Authors.Add(new Author("Maria Silva", new[] { "1" }, OrcidId: null));

        var report = new Report();

        CreateRule().Apply(doc, ctx, report);

        Assert.Equal("foo@y.edu", ctx.CorrespondingEmail);
        Assert.Null(ctx.CorrespondingOrcid);
        Assert.Same(affiliation, ctx.CorrespondingAffiliationParagraph);
        Assert.Equal("2 Universidade Y", PlainText(affiliation));
        Assert.Equal(0, ctx.CorrespondingAuthorIndex);
        Assert.DoesNotContain(report.Entries, e => e.Level >= ReportLevel.Warn);
    }

    [Fact]
    public void Apply_WithEmailAndOrcidTrailer_PromotesOrcidWhenAuthorHasNone()
    {
        var authorsParagraph = Paragraph(TextRun("Maria Silva"), SuperscriptRun("1*"));
        var affiliation = Paragraph(TextRun(
            "2 Universidade Y * E-mail: foo@y.edu ORCID: https://orcid.org/0000-0002-1825-0097"));

        using var doc = CreateDocumentWith(authorsParagraph, affiliation);

        var ctx = new FormattingContext();
        ctx.AuthorParagraphs.Add(authorsParagraph);
        ctx.Authors.Add(new Author("Maria Silva", new[] { "1" }, OrcidId: null));

        var report = new Report();

        CreateRule().Apply(doc, ctx, report);

        Assert.Equal("foo@y.edu", ctx.CorrespondingEmail);
        Assert.Equal("0000-0002-1825-0097", ctx.CorrespondingOrcid);
        Assert.Equal(0, ctx.CorrespondingAuthorIndex);
        Assert.Equal("0000-0002-1825-0097", ctx.Authors[0].OrcidId);

        var info = Assert.Single(
            report.Entries,
            e => e.Level == ReportLevel.Info
                 && e.Message == ExtractCorrespondingAuthorRule.OrcidPromotedMessage);
        Assert.Equal(nameof(ExtractCorrespondingAuthorRule), info.Rule);
        Assert.DoesNotContain(report.Entries, e => e.Level >= ReportLevel.Warn);
    }

    [Fact]
    public void Apply_WithExistingAuthorOrcid_DropsAffiliationOrcidSilently()
    {
        var authorsParagraph = Paragraph(TextRun("Maria Silva"), SuperscriptRun("1*"));
        var affiliation = Paragraph(TextRun(
            "2 Universidade Y * E-mail: foo@y.edu ORCID: https://orcid.org/0000-0002-1825-0097"));

        using var doc = CreateDocumentWith(authorsParagraph, affiliation);

        var ctx = new FormattingContext();
        ctx.AuthorParagraphs.Add(authorsParagraph);
        ctx.Authors.Add(new Author("Maria Silva", new[] { "1" }, OrcidId: "9999-9999-9999-9999"));

        var report = new Report();

        CreateRule().Apply(doc, ctx, report);

        Assert.Equal("foo@y.edu", ctx.CorrespondingEmail);
        Assert.Equal("0000-0002-1825-0097", ctx.CorrespondingOrcid);
        Assert.Equal("9999-9999-9999-9999", ctx.Authors[0].OrcidId);
        Assert.DoesNotContain(report.Entries, e => e.Level >= ReportLevel.Warn);
        Assert.DoesNotContain(
            report.Entries,
            e => e.Message == ExtractCorrespondingAuthorRule.OrcidPromotedMessage);
    }

    [Fact]
    public void Apply_WithSuperscriptStarOnAuthor_IdentifiesCorrespondingAuthor()
    {
        var authorsParagraph = Paragraph(
            TextRun("João Pereira"),
            SuperscriptRun("1"),
            TextRun(", Maria Silva"),
            SuperscriptRun("1,2*"));
        var affiliation = Paragraph(TextRun("2 Universidade Y * E-mail: maria@y.edu"));

        using var doc = CreateDocumentWith(authorsParagraph, affiliation);

        var ctx = new FormattingContext();
        ctx.AuthorParagraphs.Add(authorsParagraph);
        ctx.Authors.Add(new Author("João Pereira", new[] { "1" }, OrcidId: null));
        ctx.Authors.Add(new Author("Maria Silva", new[] { "1", "2" }, OrcidId: null));

        var report = new Report();

        CreateRule().Apply(doc, ctx, report);

        Assert.Equal(1, ctx.CorrespondingAuthorIndex);
        Assert.Equal("Maria Silva", ctx.Authors[ctx.CorrespondingAuthorIndex!.Value].Name);
        Assert.Equal("maria@y.edu", ctx.CorrespondingEmail);
        Assert.DoesNotContain(report.Entries, e => e.Level >= ReportLevel.Warn);
    }

    [Fact]
    public void Apply_WithPlainTextStarOnAuthor_IdentifiesCorrespondingAuthor()
    {
        var authorsParagraph = Paragraph(
            TextRun("João Pereira, Maria Silva*"));
        var affiliation = Paragraph(TextRun("1 Universidade Y * E-mail: maria@y.edu"));

        using var doc = CreateDocumentWith(authorsParagraph, affiliation);

        var ctx = new FormattingContext();
        ctx.AuthorParagraphs.Add(authorsParagraph);
        ctx.Authors.Add(new Author("João Pereira", Array.Empty<string>(), OrcidId: null));
        ctx.Authors.Add(new Author("Maria Silva", Array.Empty<string>(), OrcidId: null));

        var report = new Report();

        CreateRule().Apply(doc, ctx, report);

        Assert.Equal(1, ctx.CorrespondingAuthorIndex);
        Assert.Equal("maria@y.edu", ctx.CorrespondingEmail);
        Assert.DoesNotContain(report.Entries, e => e.Level >= ReportLevel.Warn);
    }

    [Fact]
    public void Apply_WithTwoStarMarkers_FirstWinsAndSecondWarns()
    {
        var authorsParagraph = Paragraph(
            TextRun("Alice*, Bob*"));
        var affiliation = Paragraph(TextRun("1 Place * E-mail: alice@x.edu"));

        using var doc = CreateDocumentWith(authorsParagraph, affiliation);

        var ctx = new FormattingContext();
        ctx.AuthorParagraphs.Add(authorsParagraph);
        ctx.Authors.Add(new Author("Alice", Array.Empty<string>(), OrcidId: null));
        ctx.Authors.Add(new Author("Bob", Array.Empty<string>(), OrcidId: null));

        var report = new Report();

        CreateRule().Apply(doc, ctx, report);

        Assert.Equal(0, ctx.CorrespondingAuthorIndex);
        Assert.Equal("alice@x.edu", ctx.CorrespondingEmail);
        Assert.Equal("1 Place", PlainText(affiliation));

        var warn = Assert.Single(report.Entries, e => e.Level == ReportLevel.Warn);
        Assert.Equal(nameof(ExtractCorrespondingAuthorRule), warn.Rule);
        Assert.Equal(ExtractCorrespondingAuthorRule.SecondMarkerMessage, warn.Message);
    }

    [Fact]
    public void Apply_WithNoStarAnywhere_LogsInfoAndIsNoOp()
    {
        var authorsParagraph = Paragraph(TextRun("Maria Silva"), SuperscriptRun("1"));
        var affiliation = Paragraph(TextRun("1 Universidade Y"));

        using var doc = CreateDocumentWith(authorsParagraph, affiliation);
        var bodyXmlBefore = doc.MainDocumentPart!.Document!.Body!.OuterXml;

        var ctx = new FormattingContext();
        ctx.AuthorParagraphs.Add(authorsParagraph);
        ctx.Authors.Add(new Author("Maria Silva", new[] { "1" }, OrcidId: null));

        var report = new Report();

        CreateRule().Apply(doc, ctx, report);

        Assert.Equal(bodyXmlBefore, doc.MainDocumentPart!.Document!.Body!.OuterXml);
        Assert.Null(ctx.CorrespondingEmail);
        Assert.Null(ctx.CorrespondingOrcid);
        Assert.Null(ctx.CorrespondingAuthorIndex);
        Assert.Null(ctx.CorrespondingAffiliationParagraph);

        var info = Assert.Single(report.Entries, e => e.Level == ReportLevel.Info);
        Assert.Equal(nameof(ExtractCorrespondingAuthorRule), info.Rule);
        Assert.Equal(ExtractCorrespondingAuthorRule.NoMarkerMessage, info.Message);
        Assert.DoesNotContain(report.Entries, e => e.Level >= ReportLevel.Warn);
    }

    [Fact]
    public void Apply_WithMarkerButGarbledEmail_StripsTrailerAndWarns()
    {
        var authorsParagraph = Paragraph(TextRun("Maria Silva"), SuperscriptRun("1*"));
        var affiliation = Paragraph(TextRun("2 Universidade Y * E-mail: not-a-valid-email"));

        using var doc = CreateDocumentWith(authorsParagraph, affiliation);

        var ctx = new FormattingContext();
        ctx.AuthorParagraphs.Add(authorsParagraph);
        ctx.Authors.Add(new Author("Maria Silva", new[] { "1" }, OrcidId: null));

        var report = new Report();

        CreateRule().Apply(doc, ctx, report);

        Assert.Null(ctx.CorrespondingEmail);
        Assert.Equal("2 Universidade Y", PlainText(affiliation));
        Assert.Equal(0, ctx.CorrespondingAuthorIndex);

        var warn = Assert.Single(report.Entries, e => e.Level == ReportLevel.Warn);
        Assert.Equal(nameof(ExtractCorrespondingAuthorRule), warn.Rule);
        Assert.Equal(ExtractCorrespondingAuthorRule.EmailExtractionFailedMessage, warn.Message);
    }

    [Fact]
    public void Apply_WithTrailerSplitAcrossRuns_StripsAllTrailerRuns()
    {
        var authorsParagraph = Paragraph(TextRun("Maria Silva"), SuperscriptRun("1*"));
        var affiliation = Paragraph(
            TextRun("2 Universidade Y "),
            TextRun("*"),
            TextRun(" E-mail: "),
            TextRun("foo@y.edu"));

        using var doc = CreateDocumentWith(authorsParagraph, affiliation);

        var ctx = new FormattingContext();
        ctx.AuthorParagraphs.Add(authorsParagraph);
        ctx.Authors.Add(new Author("Maria Silva", new[] { "1" }, OrcidId: null));

        var report = new Report();

        CreateRule().Apply(doc, ctx, report);

        Assert.Equal("foo@y.edu", ctx.CorrespondingEmail);
        Assert.Equal("2 Universidade Y", PlainText(affiliation));
        Assert.DoesNotContain(report.Entries, e => e.Level >= ReportLevel.Warn);
    }

    [Fact]
    public void Apply_WithTrailerSplittingARun_PreservesPreStarTextInThatRun()
    {
        var authorsParagraph = Paragraph(TextRun("Maria Silva"), SuperscriptRun("1*"));
        var affiliation = Paragraph(
            TextRun("2 Universidade Y *E-mail: foo@y.edu"));

        using var doc = CreateDocumentWith(authorsParagraph, affiliation);

        var ctx = new FormattingContext();
        ctx.AuthorParagraphs.Add(authorsParagraph);
        ctx.Authors.Add(new Author("Maria Silva", new[] { "1" }, OrcidId: null));

        var report = new Report();

        CreateRule().Apply(doc, ctx, report);

        Assert.Equal("foo@y.edu", ctx.CorrespondingEmail);
        Assert.Equal("2 Universidade Y", PlainText(affiliation));
        Assert.DoesNotContain(report.Entries, e => e.Level >= ReportLevel.Warn);
    }

    [Fact]
    public void Apply_WithOnlyStarContentInAffiliation_RemovesParagraphFromBody()
    {
        var authorsParagraph = Paragraph(TextRun("Maria Silva"), SuperscriptRun("1*"));
        var affiliation = Paragraph(TextRun("* E-mail: foo@y.edu"));

        using var doc = CreateDocumentWith(authorsParagraph, affiliation);

        var ctx = new FormattingContext();
        ctx.AuthorParagraphs.Add(authorsParagraph);
        ctx.Authors.Add(new Author("Maria Silva", new[] { "1" }, OrcidId: null));

        var report = new Report();

        CreateRule().Apply(doc, ctx, report);

        Assert.Equal("foo@y.edu", ctx.CorrespondingEmail);
        Assert.Null(ctx.CorrespondingAffiliationParagraph);

        var paragraphs = doc.MainDocumentPart!.Document!.Body!.Elements<Paragraph>().ToList();
        Assert.Single(paragraphs);
        Assert.Same(authorsParagraph, paragraphs[0]);
    }

    [Fact]
    public void Apply_StopsAtAbstractMarker_DoesNotScanAbstractParagraph()
    {
        var authorsParagraph = Paragraph(TextRun("Maria Silva"), SuperscriptRun("1"));
        var affiliation = Paragraph(TextRun("1 Universidade Y"));
        var abstractParagraph = Paragraph(TextRun("Abstract - this references * E-mail: noise@x.edu"));

        using var doc = CreateDocumentWith(authorsParagraph, affiliation, abstractParagraph);

        var ctx = new FormattingContext();
        ctx.AuthorParagraphs.Add(authorsParagraph);
        ctx.Authors.Add(new Author("Maria Silva", new[] { "1" }, OrcidId: null));

        var report = new Report();

        CreateRule().Apply(doc, ctx, report);

        Assert.Null(ctx.CorrespondingEmail);
        Assert.Equal(
            "Abstract - this references * E-mail: noise@x.edu",
            PlainText(abstractParagraph));

        var info = Assert.Single(report.Entries, e => e.Level == ReportLevel.Info);
        Assert.Equal(ExtractCorrespondingAuthorRule.NoMarkerMessage, info.Message);
    }

    [Fact]
    public void Apply_WithSingleAuthorAndNoStar_DoesNotWarn()
    {
        var authorsParagraph = Paragraph(TextRun("Maria Silva"), SuperscriptRun("1"));
        var affiliation = Paragraph(TextRun("1 Universidade Y"));

        using var doc = CreateDocumentWith(authorsParagraph, affiliation);

        var ctx = new FormattingContext();
        ctx.AuthorParagraphs.Add(authorsParagraph);
        ctx.Authors.Add(new Author("Maria Silva", new[] { "1" }, OrcidId: null));

        var report = new Report();

        CreateRule().Apply(doc, ctx, report);

        Assert.DoesNotContain(report.Entries, e => e.Level >= ReportLevel.Warn);
    }

    [Fact]
    public void Apply_WithEmptyAuthorParagraphs_LogsInfoAndIsNoOp()
    {
        var orphan = Paragraph(TextRun("Some content"));
        using var doc = CreateDocumentWith(orphan);
        var bodyXmlBefore = doc.MainDocumentPart!.Document!.Body!.OuterXml;

        var ctx = new FormattingContext();
        var report = new Report();

        CreateRule().Apply(doc, ctx, report);

        Assert.Equal(bodyXmlBefore, doc.MainDocumentPart!.Document!.Body!.OuterXml);

        var info = Assert.Single(report.Entries, e => e.Level == ReportLevel.Info);
        Assert.Equal(nameof(ExtractCorrespondingAuthorRule), info.Rule);
        Assert.Equal(ExtractCorrespondingAuthorRule.NoMarkerMessage, info.Message);
    }

    [Fact]
    public void Apply_TruncatedRunDoesNotLoseLeadingTextWhenStarIsMidRun()
    {
        var authorsParagraph = Paragraph(TextRun("Maria Silva"), SuperscriptRun("1*"));
        var affiliation = Paragraph(TextRun("2 Universidade Y *E-mail: foo@y.edu"));

        using var doc = CreateDocumentWith(authorsParagraph, affiliation);

        var ctx = new FormattingContext();
        ctx.AuthorParagraphs.Add(authorsParagraph);
        ctx.Authors.Add(new Author("Maria Silva", new[] { "1" }, OrcidId: null));

        var report = new Report();
        CreateRule().Apply(doc, ctx, report);

        // Run snapshot: only one Run survives, holding "2 Universidade Y" (whitespace trimmed).
        var runs = affiliation.Elements<Run>().ToList();
        Assert.Single(runs);
        var surviving = runs[0].Descendants<Text>().Single().Text;
        Assert.Equal("2 Universidade Y", surviving);
    }

    [Fact]
    public void Apply_WithEmailLowercaseHyphenated_RecognizesEmailToken()
    {
        var authorsParagraph = Paragraph(TextRun("Maria Silva"), SuperscriptRun("1*"));
        var affiliation = Paragraph(TextRun("2 Universidade Y *E-mail: foo@y.edu"));

        using var doc = CreateDocumentWith(authorsParagraph, affiliation);

        var ctx = new FormattingContext();
        ctx.AuthorParagraphs.Add(authorsParagraph);
        ctx.Authors.Add(new Author("Maria Silva", new[] { "1" }, OrcidId: null));

        var report = new Report();
        CreateRule().Apply(doc, ctx, report);

        Assert.Equal("foo@y.edu", ctx.CorrespondingEmail);
    }
}
