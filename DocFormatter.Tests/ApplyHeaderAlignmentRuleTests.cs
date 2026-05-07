using DocFormatter.Core.Pipeline;
using DocFormatter.Core.Rules;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Xunit;

namespace DocFormatter.Tests;

public sealed class ApplyHeaderAlignmentRuleTests
{
    private static ApplyHeaderAlignmentRule CreateRule() => new();

    private static WordprocessingDocument CreateDocumentWith(params OpenXmlElement[] bodyChildren)
    {
        var stream = new MemoryStream();
        var doc = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document);
        var mainPart = doc.AddMainDocumentPart();
        mainPart.Document = new Document(new Body(bodyChildren));
        return doc;
    }

    private static Paragraph PlainParagraph(string text)
        => new(new Run(new Text(text) { Space = SpaceProcessingModeValues.Preserve }));

    private static JustificationValues? JustificationOf(Paragraph paragraph)
        => paragraph.ParagraphProperties?.GetFirstChild<Justification>()?.Val?.Value;

    private static int JustificationCount(Paragraph paragraph)
        => paragraph.ParagraphProperties?.Elements<Justification>().Count() ?? 0;

    [Fact]
    public void Apply_WithAllParagraphsPresent_AlignsAndLogsSummary()
    {
        var doiPara = PlainParagraph("DOI 10.1234/abc");
        var sectionPara = PlainParagraph("Original Article");
        var titlePara = PlainParagraph("Title");

        using var doc = CreateDocumentWith(doiPara, sectionPara, titlePara);

        var ctx = new FormattingContext
        {
            DoiParagraph = doiPara,
            SectionParagraph = sectionPara,
            TitleParagraph = titlePara,
        };
        var report = new Report();

        CreateRule().Apply(doc, ctx, report);

        Assert.Equal(JustificationValues.Right, JustificationOf(doiPara));
        Assert.Equal(JustificationValues.Right, JustificationOf(sectionPara));
        Assert.Equal(JustificationValues.Center, JustificationOf(titlePara));

        Assert.DoesNotContain(report.Entries, e => e.Level >= ReportLevel.Warn);

        var info = Assert.Single(report.Entries, e => e.Level == ReportLevel.Info);
        Assert.Equal(nameof(ApplyHeaderAlignmentRule), info.Rule);
        Assert.Equal("alignment applied (doi=true, section=true, title=true)", info.Message);
    }

    [Fact]
    public void Apply_WithDoiNullOnly_AlignsOthersAndWarnsOnDoi()
    {
        var sectionPara = PlainParagraph("Original Article");
        var titlePara = PlainParagraph("Title");

        using var doc = CreateDocumentWith(sectionPara, titlePara);

        var ctx = new FormattingContext
        {
            DoiParagraph = null,
            SectionParagraph = sectionPara,
            TitleParagraph = titlePara,
        };
        var report = new Report();

        CreateRule().Apply(doc, ctx, report);

        Assert.Equal(JustificationValues.Right, JustificationOf(sectionPara));
        Assert.Equal(JustificationValues.Center, JustificationOf(titlePara));

        var warn = Assert.Single(report.Entries, e => e.Level == ReportLevel.Warn);
        Assert.Equal(nameof(ApplyHeaderAlignmentRule), warn.Rule);
        Assert.Equal(ApplyHeaderAlignmentRule.MissingDoiParagraphMessage, warn.Message);

        var info = Assert.Single(report.Entries, e => e.Level == ReportLevel.Info);
        Assert.Equal("alignment applied (doi=false, section=true, title=true)", info.Message);
    }

    [Fact]
    public void Apply_WithSectionNullOnly_AlignsOthersAndWarnsOnSection()
    {
        var doiPara = PlainParagraph("DOI 10.1234/abc");
        var titlePara = PlainParagraph("Title");

        using var doc = CreateDocumentWith(doiPara, titlePara);

        var ctx = new FormattingContext
        {
            DoiParagraph = doiPara,
            SectionParagraph = null,
            TitleParagraph = titlePara,
        };
        var report = new Report();

        CreateRule().Apply(doc, ctx, report);

        Assert.Equal(JustificationValues.Right, JustificationOf(doiPara));
        Assert.Equal(JustificationValues.Center, JustificationOf(titlePara));

        var warn = Assert.Single(report.Entries, e => e.Level == ReportLevel.Warn);
        Assert.Equal(ApplyHeaderAlignmentRule.MissingSectionParagraphMessage, warn.Message);

        var info = Assert.Single(report.Entries, e => e.Level == ReportLevel.Info);
        Assert.Equal("alignment applied (doi=true, section=false, title=true)", info.Message);
    }

    [Fact]
    public void Apply_WithTitleNullOnly_AlignsOthersAndWarnsOnTitle()
    {
        var doiPara = PlainParagraph("DOI 10.1234/abc");
        var sectionPara = PlainParagraph("Original Article");

        using var doc = CreateDocumentWith(doiPara, sectionPara);

        var ctx = new FormattingContext
        {
            DoiParagraph = doiPara,
            SectionParagraph = sectionPara,
            TitleParagraph = null,
        };
        var report = new Report();

        CreateRule().Apply(doc, ctx, report);

        Assert.Equal(JustificationValues.Right, JustificationOf(doiPara));
        Assert.Equal(JustificationValues.Right, JustificationOf(sectionPara));

        var warn = Assert.Single(report.Entries, e => e.Level == ReportLevel.Warn);
        Assert.Equal(ApplyHeaderAlignmentRule.MissingTitleParagraphMessage, warn.Message);

        var info = Assert.Single(report.Entries, e => e.Level == ReportLevel.Info);
        Assert.Equal("alignment applied (doi=true, section=true, title=false)", info.Message);
    }

    [Fact]
    public void Apply_WithAllNull_LogsThreeWarnsAndLeavesBodyUnchanged()
    {
        var unrelated = PlainParagraph("Body content");
        using var doc = CreateDocumentWith(unrelated);
        var bodyXmlBefore = doc.MainDocumentPart!.Document!.Body!.OuterXml;

        var ctx = new FormattingContext
        {
            DoiParagraph = null,
            SectionParagraph = null,
            TitleParagraph = null,
        };
        var report = new Report();

        CreateRule().Apply(doc, ctx, report);

        Assert.Equal(bodyXmlBefore, doc.MainDocumentPart!.Document!.Body!.OuterXml);

        var warns = report.Entries.Where(e => e.Level == ReportLevel.Warn).ToList();
        Assert.Equal(3, warns.Count);
        Assert.Contains(warns, w => w.Message == ApplyHeaderAlignmentRule.MissingDoiParagraphMessage);
        Assert.Contains(warns, w => w.Message == ApplyHeaderAlignmentRule.MissingSectionParagraphMessage);
        Assert.Contains(warns, w => w.Message == ApplyHeaderAlignmentRule.MissingTitleParagraphMessage);

        var info = Assert.Single(report.Entries, e => e.Level == ReportLevel.Info);
        Assert.Equal("alignment applied (doi=false, section=false, title=false)", info.Message);
    }

    [Fact]
    public void Apply_WithDoiAlreadyRightAligned_RewritesWithoutWarning()
    {
        var doiPara = PlainParagraph("DOI 10.1234/abc");
        doiPara.ParagraphProperties = new ParagraphProperties(
            new Justification { Val = JustificationValues.Right });
        var sectionPara = PlainParagraph("Original Article");
        var titlePara = PlainParagraph("Title");

        using var doc = CreateDocumentWith(doiPara, sectionPara, titlePara);

        var ctx = new FormattingContext
        {
            DoiParagraph = doiPara,
            SectionParagraph = sectionPara,
            TitleParagraph = titlePara,
        };
        var report = new Report();

        CreateRule().Apply(doc, ctx, report);

        Assert.Equal(JustificationValues.Right, JustificationOf(doiPara));
        Assert.Equal(1, JustificationCount(doiPara));

        Assert.DoesNotContain(report.Entries, e => e.Level >= ReportLevel.Warn);

        var info = Assert.Single(report.Entries, e => e.Level == ReportLevel.Info);
        Assert.Equal("alignment applied (doi=true, section=true, title=true)", info.Message);
    }

    [Fact]
    public void Apply_WithDoiPreSetToWrongAlignment_OverwritesToRight()
    {
        var doiPara = PlainParagraph("DOI 10.1234/abc");
        doiPara.ParagraphProperties = new ParagraphProperties(
            new Justification { Val = JustificationValues.Left });
        var sectionPara = PlainParagraph("Original Article");
        var titlePara = PlainParagraph("Title");

        using var doc = CreateDocumentWith(doiPara, sectionPara, titlePara);

        var ctx = new FormattingContext
        {
            DoiParagraph = doiPara,
            SectionParagraph = sectionPara,
            TitleParagraph = titlePara,
        };

        CreateRule().Apply(doc, ctx, new Report());

        Assert.Equal(JustificationValues.Right, JustificationOf(doiPara));
        Assert.Equal(1, JustificationCount(doiPara));
    }

    [Fact]
    public void Apply_WithExistingParagraphPropertiesButNoJustification_PreservesOtherProperties()
    {
        var titlePara = PlainParagraph("Title");
        titlePara.ParagraphProperties = new ParagraphProperties(
            new ParagraphStyleId { Val = "Heading1" });

        using var doc = CreateDocumentWith(titlePara);

        var ctx = new FormattingContext { TitleParagraph = titlePara };
        var report = new Report();

        CreateRule().Apply(doc, ctx, report);

        Assert.NotNull(titlePara.ParagraphProperties);
        Assert.Equal("Heading1", titlePara.ParagraphProperties!.ParagraphStyleId?.Val?.Value);
        Assert.Equal(JustificationValues.Center, JustificationOf(titlePara));
    }

    [Fact]
    public void Apply_DoesNotMutateUnrelatedContextState()
    {
        var doiPara = PlainParagraph("DOI 10.1234/abc");
        var sectionPara = PlainParagraph("Original Article");
        var titlePara = PlainParagraph("Title");

        using var doc = CreateDocumentWith(doiPara, sectionPara, titlePara);

        var ctx = new FormattingContext
        {
            Doi = "10.1234/abc",
            ElocationId = "e2024042",
            ArticleTitle = "Title",
            DoiParagraph = doiPara,
            SectionParagraph = sectionPara,
            TitleParagraph = titlePara,
        };
        ctx.Authors.Add(new Core.Models.Author("Maria Silva", new[] { "1" }, OrcidId: null));

        var report = new Report();

        CreateRule().Apply(doc, ctx, report);

        Assert.Equal("10.1234/abc", ctx.Doi);
        Assert.Equal("e2024042", ctx.ElocationId);
        Assert.Equal("Title", ctx.ArticleTitle);
        Assert.Single(ctx.Authors);
        Assert.Equal("Maria Silva", ctx.Authors[0].Name);
    }

    [Fact]
    public void Apply_WhenAlignmentRunsTwice_RemainsIdempotent()
    {
        var doiPara = PlainParagraph("DOI 10.1234/abc");
        var sectionPara = PlainParagraph("Original Article");
        var titlePara = PlainParagraph("Title");

        using var doc = CreateDocumentWith(doiPara, sectionPara, titlePara);

        var ctx = new FormattingContext
        {
            DoiParagraph = doiPara,
            SectionParagraph = sectionPara,
            TitleParagraph = titlePara,
        };

        CreateRule().Apply(doc, ctx, new Report());
        CreateRule().Apply(doc, ctx, new Report());

        Assert.Equal(1, JustificationCount(doiPara));
        Assert.Equal(1, JustificationCount(sectionPara));
        Assert.Equal(1, JustificationCount(titlePara));
        Assert.Equal(JustificationValues.Right, JustificationOf(doiPara));
        Assert.Equal(JustificationValues.Right, JustificationOf(sectionPara));
        Assert.Equal(JustificationValues.Center, JustificationOf(titlePara));
    }
}
