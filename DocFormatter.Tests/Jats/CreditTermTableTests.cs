using DocFormatter.Core.Jats;
using Xunit;

namespace DocFormatter.Tests.Jats;

public sealed class CreditTermTableTests
{
    [Theory]
    [InlineData("Conceptualization", "http://credit.niso.org/contributor-roles/conceptualization/", "Conceptualization")]
    [InlineData("conceptualization", "http://credit.niso.org/contributor-roles/conceptualization/", "Conceptualization")]
    [InlineData("Data curation", "http://credit.niso.org/contributor-roles/data-curation/", "Data curation")]
    [InlineData("Project administration", "http://credit.niso.org/contributor-roles/project-administration/", "Project administration")]
    public void TryMap_KnownTerm_ResolvesUrlAndDisplay(string term, string url, string display)
    {
        Assert.True(CreditTermTable.TryMap(term, out var role));
        Assert.Equal(url, role.ContentTypeUrl);
        Assert.Equal(display, role.Display);
    }

    [Theory]
    [InlineData("Writing - Original Draft")]
    [InlineData("Writing – original draft")]
    [InlineData("writing - original  draft")]
    [InlineData("Writing − original draft")] // U+2212 math minus
    [InlineData("Writing － original draft")] // U+FF0D fullwidth hyphen-minus
    [InlineData("Writing ﹣ original draft")] // U+FE63 small hyphen-minus
    public void TryMap_WritingOriginalDraft_NormalizesAndExactMatches(string term)
    {
        Assert.True(CreditTermTable.TryMap(term, out var role));
        Assert.Equal("http://credit.niso.org/contributor-roles/writing-original-draft/", role.ContentTypeUrl);
        Assert.Equal("Writing – original draft", role.Display);
    }

    [Theory]
    [InlineData("Writing - review and editing")]
    [InlineData("Writing – review & editing")]
    [InlineData("Writing - Review &amp; Editing")]
    public void TryMap_WritingReviewEditing_FoldsAmpersandAndDash(string term)
    {
        Assert.True(CreditTermTable.TryMap(term, out var role));
        Assert.Equal("http://credit.niso.org/contributor-roles/writing-review-editing/", role.ContentTypeUrl);
        Assert.Equal("Writing – review & editing", role.Display);
    }

    [Theory]
    [InlineData("Choreography")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void TryMap_UnknownOrBlank_ReturnsFalse(string? term)
    {
        Assert.False(CreditTermTable.TryMap(term, out _));
    }
}
