using DocFormatter.Core.Options;
using DocFormatter.Core.Pipeline;
using DocFormatter.Core.Rules;
using DocFormatter.Tests.Fixtures.Authors;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Xunit;

namespace DocFormatter.Tests;

public sealed class LocateAbstractAndInsertElocationRuleTests
{
    private static LocateAbstractAndInsertElocationRule CreateRule()
        => new(new FormattingOptions());

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

    private static Paragraph BoldAbstractParagraph(string prefix, string remainder)
    {
        var boldRun = new Run(
            new RunProperties(new Bold()),
            new Text(prefix) { Space = SpaceProcessingModeValues.Preserve });
        var tailRun = new Run(new Text(remainder) { Space = SpaceProcessingModeValues.Preserve });
        return new Paragraph(boldRun, tailRun);
    }

    private static Paragraph PlainParagraph(string text)
        => new(new Run(new Text(text) { Space = SpaceProcessingModeValues.Preserve }));

    private static string ParagraphText(Paragraph p)
        => string.Concat(p.Descendants<Text>().Select(t => t.Text));

    [Fact]
    public void Apply_WithEnglishAbstract_InsertsElocationAboveMatch()
    {
        var section = PlainParagraph("Original Article");
        var title = PlainParagraph("On the Behavior of Title");
        var abstractPara = BoldAbstractParagraph("Abstract", " — On the behavior...");

        using var doc = CreateDocumentWith(section, title, abstractPara);

        var ctx = new FormattingContext { ElocationId = "e2024001" };
        var report = new Report();

        CreateRule().Apply(doc, ctx, report);

        var paragraphs = GetBody(doc).Elements<Paragraph>().ToList();
        Assert.Equal(4, paragraphs.Count);
        Assert.Equal("Original Article", ParagraphText(paragraphs[0]));
        Assert.Equal("On the Behavior of Title", ParagraphText(paragraphs[1]));
        Assert.Equal("e2024001", ParagraphText(paragraphs[2]));
        Assert.Same(abstractPara, paragraphs[3]);
        Assert.DoesNotContain(report.Entries, e => e.Level >= ReportLevel.Warn);
    }

    [Fact]
    public void Apply_WithPortugueseResumo_InsertsElocationAboveMatch()
    {
        var section = PlainParagraph("Artigo Original");
        var title = PlainParagraph("Sobre o comportamento de título");
        var resumoPara = BoldAbstractParagraph("Resumo", " — Sobre o comportamento...");

        using var doc = CreateDocumentWith(section, title, resumoPara);

        var ctx = new FormattingContext { ElocationId = "e2024002" };
        var report = new Report();

        CreateRule().Apply(doc, ctx, report);

        var paragraphs = GetBody(doc).Elements<Paragraph>().ToList();
        Assert.Equal(4, paragraphs.Count);
        Assert.Equal("e2024002", ParagraphText(paragraphs[2]));
        Assert.Same(resumoPara, paragraphs[3]);
        Assert.DoesNotContain(report.Entries, e => e.Level >= ReportLevel.Warn);
    }

    [Fact]
    public void Apply_WithUppercaseAbstract_MatchesCaseInsensitively()
    {
        var section = PlainParagraph("Original Article");
        var title = PlainParagraph("On the Behavior of Title");
        var abstractPara = BoldAbstractParagraph("ABSTRACT", " - body");

        using var doc = CreateDocumentWith(section, title, abstractPara);

        var ctx = new FormattingContext { ElocationId = "e2024003" };
        var report = new Report();

        CreateRule().Apply(doc, ctx, report);

        var paragraphs = GetBody(doc).Elements<Paragraph>().ToList();
        Assert.Equal("e2024003", ParagraphText(paragraphs[2]));
        Assert.Same(abstractPara, paragraphs[3]);
    }

    [Fact]
    public void Apply_WithNoBoldMarkerParagraph_LeavesDocumentUnchangedAndWarns()
    {
        var section = PlainParagraph("Original Article");
        var title = PlainParagraph("On the Behavior of Title");
        var bodyPara = PlainParagraph("Plain text without abstract heading");

        using var doc = CreateDocumentWith(section, title, bodyPara);

        var beforeXml = GetBody(doc).OuterXml;

        var ctx = new FormattingContext { ElocationId = "e2024004" };
        var report = new Report();

        CreateRule().Apply(doc, ctx, report);

        Assert.Equal(beforeXml, GetBody(doc).OuterXml);

        var warn = Assert.Single(report.Entries, e => e.Level == ReportLevel.Warn);
        Assert.Equal(LocateAbstractAndInsertElocationRule.AbstractNotFoundMessage, warn.Message);
    }

    [Fact]
    public void Apply_WithNonBoldAbstractPrefix_DoesNotMatch()
    {
        var section = PlainParagraph("Original Article");
        var title = PlainParagraph("Title");
        var nonBoldAbstract = PlainParagraph("Abstract — should not match without bold");

        using var doc = CreateDocumentWith(section, title, nonBoldAbstract);

        var ctx = new FormattingContext { ElocationId = "e2024005" };
        var report = new Report();

        CreateRule().Apply(doc, ctx, report);

        var paragraphs = GetBody(doc).Elements<Paragraph>().ToList();
        Assert.Equal(3, paragraphs.Count);
        var warn = Assert.Single(report.Entries, e => e.Level == ReportLevel.Warn);
        Assert.Equal(LocateAbstractAndInsertElocationRule.AbstractNotFoundMessage, warn.Message);
    }

    [Fact]
    public void Apply_WithAbstractInsideFootnote_DoesNotMatchAndWarns()
    {
        var section = PlainParagraph("Original Article");
        var title = PlainParagraph("Title");
        var bodyPara = PlainParagraph("Body without abstract heading [1].");

        using var doc = CreateDocumentWith(section, title, bodyPara);

        var mainPart = doc.MainDocumentPart!;
        var footnotesPart = mainPart.AddNewPart<FootnotesPart>();
        footnotesPart.Footnotes = new Footnotes(
            new Footnote(BoldAbstractParagraph("Abstract", " trapped in a footnote"))
            {
                Id = 1,
            });

        var beforeBodyXml = GetBody(doc).OuterXml;

        var ctx = new FormattingContext { ElocationId = "e2024006" };
        var report = new Report();

        CreateRule().Apply(doc, ctx, report);

        Assert.Equal(beforeBodyXml, GetBody(doc).OuterXml);
        var warn = Assert.Single(report.Entries, e => e.Level == ReportLevel.Warn);
        Assert.Equal(LocateAbstractAndInsertElocationRule.AbstractNotFoundMessage, warn.Message);
    }

    [Fact]
    public void Apply_WithNullElocationId_DoesNotInsertAndWarns()
    {
        var section = PlainParagraph("Original Article");
        var title = PlainParagraph("Title");
        var abstractPara = BoldAbstractParagraph("Abstract", " — body");

        using var doc = CreateDocumentWith(section, title, abstractPara);

        var beforeXml = GetBody(doc).OuterXml;

        var ctx = new FormattingContext { ElocationId = null };
        var report = new Report();

        CreateRule().Apply(doc, ctx, report);

        Assert.Equal(beforeXml, GetBody(doc).OuterXml);
        var warn = Assert.Single(report.Entries, e => e.Level == ReportLevel.Warn);
        Assert.Equal(LocateAbstractAndInsertElocationRule.MissingElocationIdMessage, warn.Message);
    }

    [Fact]
    public void Apply_WithEmptyElocationId_DoesNotInsertAndWarns()
    {
        var section = PlainParagraph("Original Article");
        var title = PlainParagraph("Title");
        var abstractPara = BoldAbstractParagraph("Abstract", " — body");

        using var doc = CreateDocumentWith(section, title, abstractPara);

        var beforeXml = GetBody(doc).OuterXml;

        var ctx = new FormattingContext { ElocationId = string.Empty };
        var report = new Report();

        CreateRule().Apply(doc, ctx, report);

        Assert.Equal(beforeXml, GetBody(doc).OuterXml);
        var warn = Assert.Single(report.Entries, e => e.Level == ReportLevel.Warn);
        Assert.Equal(LocateAbstractAndInsertElocationRule.MissingElocationIdMessage, warn.Message);
    }

    [Fact]
    public void Apply_DoesNotMutateMatchedAbstractParagraph()
    {
        var section = PlainParagraph("Original Article");
        var title = PlainParagraph("Title");
        var abstractPara = BoldAbstractParagraph("Abstract", " — body");
        var beforeAbstractXml = abstractPara.OuterXml;

        using var doc = CreateDocumentWith(section, title, abstractPara);

        var ctx = new FormattingContext { ElocationId = "e2024007" };
        var report = new Report();

        CreateRule().Apply(doc, ctx, report);

        Assert.Equal(beforeAbstractXml, abstractPara.OuterXml);
    }

    [Fact]
    public void Apply_WithMultipleAbstractCandidates_InsertsAboveFirstMatch()
    {
        var section = PlainParagraph("Original Article");
        var title = PlainParagraph("Title");
        var firstAbstract = BoldAbstractParagraph("Abstract", " — primary");
        var secondAbstract = BoldAbstractParagraph("Resumo", " — secondary");

        using var doc = CreateDocumentWith(section, title, firstAbstract, secondAbstract);

        var ctx = new FormattingContext { ElocationId = "e2024008" };
        var report = new Report();

        CreateRule().Apply(doc, ctx, report);

        var paragraphs = GetBody(doc).Elements<Paragraph>().ToList();
        Assert.Equal(5, paragraphs.Count);
        Assert.Equal("e2024008", ParagraphText(paragraphs[2]));
        Assert.Same(firstAbstract, paragraphs[3]);
        Assert.Same(secondAbstract, paragraphs[4]);
    }

    [Fact]
    public void Pipeline_FullSixRules_InsertsElocationAboveAbstractAndKeepsRestIntact()
    {
        var grid = new TableGrid(
            new GridColumn { Width = "1000" },
            new GridColumn { Width = "1000" },
            new GridColumn { Width = "1000" });
        var table = new Table(
            grid,
            new TableRow(
                new TableCell(new Paragraph(new Run(new Text("ART01")))),
                new TableCell(new Paragraph(new Run(new Text("e2024009")))),
                new TableCell(new Paragraph(new Run(new Text("10.1234/abc"))))));

        var section = new Paragraph(new Run(new Text(AuthorsParagraphFactory.SectionText)));
        var title = new Paragraph(new Run(new Text(AuthorsParagraphFactory.TitleText)));
        var authors = new Paragraph(
            AuthorsParagraphFactory.TextRun("Author A"),
            AuthorsParagraphFactory.SuperscriptRun("1"));
        var affiliation = PlainParagraph("1 Universidade Federal X");
        var abstractPara = BoldAbstractParagraph("Abstract", " — On the behavior of title");
        var bodyContent = PlainParagraph("Body text follows here.");

        var stream = new MemoryStream();
        using var doc = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document);
        var mainPart = doc.AddMainDocumentPart();
        mainPart.Document = new Document(
            new Body(table, section, title, authors, affiliation, abstractPara, bodyContent));

        var bodyContentXmlBefore = bodyContent.OuterXml;

        var pipeline = new FormattingPipeline(new IFormattingRule[]
        {
            new ExtractTopTableRule(new FormattingOptions()),
            new ParseHeaderLinesRule(),
            new ExtractOrcidLinksRule(new FormattingOptions()),
            new ParseAuthorsRule(new FormattingOptions()),
            new RewriteHeaderMvpRule(),
            new LocateAbstractAndInsertElocationRule(new FormattingOptions()),
        });

        var ctx = new FormattingContext();
        var report = new Report();
        pipeline.Run(doc, ctx, report);

        var paragraphs = mainPart.Document.Body!.Elements<Paragraph>().ToList();

        var abstractIndex = paragraphs.FindIndex(p => ReferenceEquals(p, abstractPara));
        Assert.True(abstractIndex > 0, "abstract paragraph should still be in the body");
        Assert.Equal("e2024009", ParagraphText(paragraphs[abstractIndex - 1]));

        Assert.Equal(bodyContentXmlBefore, bodyContent.OuterXml);
        Assert.DoesNotContain(report.Entries, e => e.Level == ReportLevel.Error);
    }
}
