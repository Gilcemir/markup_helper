using DocFormatter.Core.Models;
using DocFormatter.Core.Pipeline;
using DocumentFormat.OpenXml.Wordprocessing;
using Xunit;

namespace DocFormatter.Tests;

public sealed class FormattingContextTests
{
    [Fact]
    public void NewContext_AllPhase2Properties_DefaultToNull()
    {
        var ctx = new FormattingContext();

        Assert.Null(ctx.DoiParagraph);
        Assert.Null(ctx.SectionParagraph);
        Assert.Null(ctx.TitleParagraph);
        Assert.Null(ctx.AuthorBlockEndParagraph);
        Assert.Null(ctx.CorrespondingAffiliationParagraph);
        Assert.Null(ctx.CorrespondingEmail);
        Assert.Null(ctx.CorrespondingOrcid);
        Assert.Null(ctx.CorrespondingAuthorIndex);
    }

    [Fact]
    public void NewContext_MvpProperties_RemainAtTheirOriginalDefaults()
    {
        var ctx = new FormattingContext();

        Assert.Null(ctx.Doi);
        Assert.Null(ctx.ElocationId);
        Assert.Null(ctx.ArticleTitle);
        Assert.Empty(ctx.Authors);
        Assert.Empty(ctx.AuthorParagraphs);
    }

    [Fact]
    public void DoiParagraph_RoundTripsTheSameInstance()
    {
        var ctx = new FormattingContext();
        var paragraph = new Paragraph();

        ctx.DoiParagraph = paragraph;

        Assert.Same(paragraph, ctx.DoiParagraph);
    }

    [Fact]
    public void SectionParagraph_RoundTripsTheSameInstance()
    {
        var ctx = new FormattingContext();
        var paragraph = new Paragraph();

        ctx.SectionParagraph = paragraph;

        Assert.Same(paragraph, ctx.SectionParagraph);
    }

    [Fact]
    public void TitleParagraph_RoundTripsTheSameInstance()
    {
        var ctx = new FormattingContext();
        var paragraph = new Paragraph();

        ctx.TitleParagraph = paragraph;

        Assert.Same(paragraph, ctx.TitleParagraph);
    }

    [Fact]
    public void AuthorBlockEndParagraph_RoundTripsTheSameInstance()
    {
        var ctx = new FormattingContext();
        var paragraph = new Paragraph();

        ctx.AuthorBlockEndParagraph = paragraph;

        Assert.Same(paragraph, ctx.AuthorBlockEndParagraph);
    }

    [Fact]
    public void CorrespondingAffiliationParagraph_RoundTripsTheSameInstance()
    {
        var ctx = new FormattingContext();
        var paragraph = new Paragraph();

        ctx.CorrespondingAffiliationParagraph = paragraph;

        Assert.Same(paragraph, ctx.CorrespondingAffiliationParagraph);
    }

    [Fact]
    public void CorrespondingEmail_RoundTripsTheSameValue()
    {
        var ctx = new FormattingContext();

        ctx.CorrespondingEmail = "foo@example.org";

        Assert.Equal("foo@example.org", ctx.CorrespondingEmail);
    }

    [Fact]
    public void CorrespondingOrcid_RoundTripsTheSameValue()
    {
        var ctx = new FormattingContext();

        ctx.CorrespondingOrcid = "0000-0002-1825-0097";

        Assert.Equal("0000-0002-1825-0097", ctx.CorrespondingOrcid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(42)]
    [InlineData(int.MaxValue)]
    public void CorrespondingAuthorIndex_AcceptsNonNegativeIntegers(int value)
    {
        var ctx = new FormattingContext();

        ctx.CorrespondingAuthorIndex = value;

        Assert.Equal(value, ctx.CorrespondingAuthorIndex);
    }

    [Fact]
    public void CorrespondingAuthorIndex_CanBeReassignedToNull()
    {
        var ctx = new FormattingContext { CorrespondingAuthorIndex = 3 };

        ctx.CorrespondingAuthorIndex = null;

        Assert.Null(ctx.CorrespondingAuthorIndex);
    }

    [Fact]
    public void SettingPhase2ParagraphReferences_DoesNotMutateMvpCollections()
    {
        var ctx = new FormattingContext();
        var author = new Author("Maria Silva", new[] { "1" }, OrcidId: null);
        var authorParagraph = new Paragraph();
        ctx.Authors.Add(author);
        ctx.AuthorParagraphs.Add(authorParagraph);
        ctx.Doi = "10.1234/abc";
        ctx.ElocationId = "e54321";
        ctx.ArticleTitle = "Title";

        ctx.DoiParagraph = new Paragraph();
        ctx.SectionParagraph = new Paragraph();
        ctx.TitleParagraph = new Paragraph();
        ctx.AuthorBlockEndParagraph = new Paragraph();
        ctx.CorrespondingAffiliationParagraph = new Paragraph();
        ctx.CorrespondingEmail = "x@y.org";
        ctx.CorrespondingOrcid = "0000-0001-2345-6789";
        ctx.CorrespondingAuthorIndex = 0;

        Assert.Equal("10.1234/abc", ctx.Doi);
        Assert.Equal("e54321", ctx.ElocationId);
        Assert.Equal("Title", ctx.ArticleTitle);
        var keptAuthor = Assert.Single(ctx.Authors);
        Assert.Same(author, keptAuthor);
        var keptParagraph = Assert.Single(ctx.AuthorParagraphs);
        Assert.Same(authorParagraph, keptParagraph);
    }
}
