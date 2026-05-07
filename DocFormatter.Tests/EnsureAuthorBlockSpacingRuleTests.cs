using DocFormatter.Core.Pipeline;
using DocFormatter.Core.Rules;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Xunit;

namespace DocFormatter.Tests;

public sealed class EnsureAuthorBlockSpacingRuleTests
{
    private static EnsureAuthorBlockSpacingRule CreateRule() => new();

    private static WordprocessingDocument CreateDocumentWith(params OpenXmlElement[] bodyChildren)
    {
        var stream = new MemoryStream();
        var doc = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document);
        var mainPart = doc.AddMainDocumentPart();
        mainPart.Document = new Document(new Body(bodyChildren));
        return doc;
    }

    private static Paragraph TextParagraph(string text)
        => new(new Run(new Text(text) { Space = SpaceProcessingModeValues.Preserve }));

    private static Paragraph BlankParagraph() => new();

    private static Paragraph WhitespaceParagraph()
        => new(new Run(new Text("   ") { Space = SpaceProcessingModeValues.Preserve }));

    private static IReadOnlyList<Paragraph> Paragraphs(WordprocessingDocument doc)
        => doc.MainDocumentPart!.Document!.Body!.Elements<Paragraph>().ToList();

    [Fact]
    public void Apply_WhenAffiliationDirectlyFollowsAuthors_InsertsBlankParagraph()
    {
        var author = TextParagraph("Maria Silva1");
        var affiliation = TextParagraph("1 University of Example");

        using var doc = CreateDocumentWith(author, affiliation);

        var ctx = new FormattingContext { AuthorBlockEndParagraph = author };
        var report = new Report();

        CreateRule().Apply(doc, ctx, report);

        var paragraphs = Paragraphs(doc);
        Assert.Equal(3, paragraphs.Count);
        Assert.Same(author, paragraphs[0]);
        Assert.Empty(paragraphs[1].Descendants<Text>());
        Assert.Same(affiliation, paragraphs[2]);

        var info = Assert.Single(report.Entries, e => e.Level == ReportLevel.Info);
        Assert.Equal(nameof(EnsureAuthorBlockSpacingRule), info.Rule);
        Assert.Equal(EnsureAuthorBlockSpacingRule.BlankLineInsertedMessage, info.Message);
        Assert.DoesNotContain(report.Entries, e => e.Level >= ReportLevel.Warn);
    }

    [Fact]
    public void Apply_WhenBlankAlreadyPresent_DoesNotInsertAndLogsAlreadyPresent()
    {
        var author = TextParagraph("Maria Silva1");
        var blank = BlankParagraph();
        var affiliation = TextParagraph("1 University of Example");

        using var doc = CreateDocumentWith(author, blank, affiliation);

        var ctx = new FormattingContext { AuthorBlockEndParagraph = author };
        var report = new Report();

        CreateRule().Apply(doc, ctx, report);

        var paragraphs = Paragraphs(doc);
        Assert.Equal(3, paragraphs.Count);
        Assert.Same(author, paragraphs[0]);
        Assert.Same(blank, paragraphs[1]);
        Assert.Same(affiliation, paragraphs[2]);

        var info = Assert.Single(report.Entries, e => e.Level == ReportLevel.Info);
        Assert.Equal(EnsureAuthorBlockSpacingRule.BlankLineAlreadyPresentMessage, info.Message);
        Assert.DoesNotContain(report.Entries, e => e.Level >= ReportLevel.Warn);
    }

    [Fact]
    public void Apply_WhenAuthorBlockEndIsNull_LogsWarnAndDoesNotMutate()
    {
        var author = TextParagraph("Maria Silva1");
        var affiliation = TextParagraph("1 University of Example");

        using var doc = CreateDocumentWith(author, affiliation);
        var bodyXmlBefore = doc.MainDocumentPart!.Document!.Body!.OuterXml;

        var ctx = new FormattingContext { AuthorBlockEndParagraph = null };
        var report = new Report();

        CreateRule().Apply(doc, ctx, report);

        Assert.Equal(bodyXmlBefore, doc.MainDocumentPart!.Document!.Body!.OuterXml);

        var warn = Assert.Single(report.Entries, e => e.Level == ReportLevel.Warn);
        Assert.Equal(nameof(EnsureAuthorBlockSpacingRule), warn.Rule);
        Assert.Equal(EnsureAuthorBlockSpacingRule.MissingAuthorBlockEndMessage, warn.Message);
        Assert.DoesNotContain(report.Entries, e => e.Level == ReportLevel.Info);
    }

    [Fact]
    public void Apply_WhenOnlyBlankParagraphsFollowAuthorBlock_LogsWarnAndDoesNotInsert()
    {
        var author = TextParagraph("Maria Silva1");
        var blank1 = BlankParagraph();
        var blank2 = WhitespaceParagraph();

        using var doc = CreateDocumentWith(author, blank1, blank2);
        var bodyXmlBefore = doc.MainDocumentPart!.Document!.Body!.OuterXml;

        var ctx = new FormattingContext { AuthorBlockEndParagraph = author };
        var report = new Report();

        CreateRule().Apply(doc, ctx, report);

        Assert.Equal(bodyXmlBefore, doc.MainDocumentPart!.Document!.Body!.OuterXml);

        var warn = Assert.Single(report.Entries, e => e.Level == ReportLevel.Warn);
        Assert.Equal(EnsureAuthorBlockSpacingRule.MissingAffiliationMessage, warn.Message);
        Assert.DoesNotContain(report.Entries, e => e.Level == ReportLevel.Info);
    }

    [Fact]
    public void Apply_WhenWhitespaceParagraphSeparatesAuthorsAndAffiliation_TreatsItAsBlank()
    {
        var author = TextParagraph("Maria Silva1");
        var whitespace = WhitespaceParagraph();
        var affiliation = TextParagraph("1 University of Example");

        using var doc = CreateDocumentWith(author, whitespace, affiliation);

        var ctx = new FormattingContext { AuthorBlockEndParagraph = author };
        var report = new Report();

        CreateRule().Apply(doc, ctx, report);

        var paragraphs = Paragraphs(doc);
        Assert.Equal(3, paragraphs.Count);
        Assert.Same(author, paragraphs[0]);
        Assert.Same(whitespace, paragraphs[1]);
        Assert.Same(affiliation, paragraphs[2]);

        var info = Assert.Single(report.Entries, e => e.Level == ReportLevel.Info);
        Assert.Equal(EnsureAuthorBlockSpacingRule.BlankLineAlreadyPresentMessage, info.Message);
    }

    [Fact]
    public void Apply_RunTwice_IsIdempotent()
    {
        var author = TextParagraph("Maria Silva1");
        var affiliation = TextParagraph("1 University of Example");

        using var doc = CreateDocumentWith(author, affiliation);

        var ctx = new FormattingContext { AuthorBlockEndParagraph = author };

        CreateRule().Apply(doc, ctx, new Report());

        var paragraphsAfterFirst = Paragraphs(doc);
        Assert.Equal(3, paragraphsAfterFirst.Count);

        var secondReport = new Report();
        CreateRule().Apply(doc, ctx, secondReport);

        var paragraphsAfterSecond = Paragraphs(doc);
        Assert.Equal(3, paragraphsAfterSecond.Count);
        Assert.Same(author, paragraphsAfterSecond[0]);
        Assert.Empty(paragraphsAfterSecond[1].Descendants<Text>());
        Assert.Same(affiliation, paragraphsAfterSecond[2]);

        var info = Assert.Single(secondReport.Entries, e => e.Level == ReportLevel.Info);
        Assert.Equal(EnsureAuthorBlockSpacingRule.BlankLineAlreadyPresentMessage, info.Message);
        Assert.DoesNotContain(secondReport.Entries, e => e.Level >= ReportLevel.Warn);
    }

    [Fact]
    public void Apply_WhenAffiliationIsImmediateNextSibling_DoesNotMutateUnrelatedParagraphs()
    {
        var doiParagraph = TextParagraph("DOI 10.1234/abc");
        var author = TextParagraph("Maria Silva1");
        var affiliation = TextParagraph("1 University of Example");
        var laterContent = TextParagraph("Abstract - lorem ipsum");

        using var doc = CreateDocumentWith(doiParagraph, author, affiliation, laterContent);

        var ctx = new FormattingContext { AuthorBlockEndParagraph = author };
        var report = new Report();

        CreateRule().Apply(doc, ctx, report);

        var paragraphs = Paragraphs(doc);
        Assert.Equal(5, paragraphs.Count);
        Assert.Same(doiParagraph, paragraphs[0]);
        Assert.Same(author, paragraphs[1]);
        Assert.Empty(paragraphs[2].Descendants<Text>());
        Assert.Same(affiliation, paragraphs[3]);
        Assert.Same(laterContent, paragraphs[4]);
    }
}
