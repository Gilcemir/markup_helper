using DocFormatter.Core.Models;
using DocFormatter.Core.Options;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DocFormatter.Tests;

public sealed class FormattingOptionsTests
{
    private readonly FormattingOptions _options = new();

    [Theory]
    [InlineData("10.1234/abc.def")]
    [InlineData("10.12345/path-with_special:chars/123")]
    public void DoiRegex_Matches_ValidDoiStrings(string doi)
    {
        Assert.Matches(_options.DoiRegex, doi);
    }

    [Theory]
    [InlineData("abc/10.1234")]
    [InlineData("10.1/short")]
    public void DoiRegex_DoesNotMatch_InvalidDoiStrings(string candidate)
    {
        Assert.DoesNotMatch(_options.DoiRegex, candidate);
    }

    [Theory]
    [InlineData("0000-0002-1825-0097")]
    [InlineData("0000-0001-2345-678X")]
    [InlineData("0000-0001-2345-678x")]
    public void OrcidIdRegex_Matches_ValidOrcidIdentifiers(string orcid)
    {
        Assert.Matches(_options.OrcidIdRegex, orcid);
    }

    [Theory]
    [InlineData("0000-0002-1825-009")]
    [InlineData("0000-0002-18250097")]
    public void OrcidIdRegex_DoesNotMatch_InvalidOrcidIdentifiers(string candidate)
    {
        Assert.DoesNotMatch(_options.OrcidIdRegex, candidate);
    }

    [Fact]
    public void OrcidUrlMarker_IsOrcidOrgSubstring()
    {
        Assert.Equal("orcid.org", _options.OrcidUrlMarker);
    }

    [Fact]
    public void AuthorSeparators_AreCommaThenAndInOrder()
    {
        Assert.Equal(new[] { ", ", " and " }, _options.AuthorSeparators);
    }

    [Fact]
    public void AbstractMarkers_AreAbstractAndResumo()
    {
        Assert.Equal(new[] { "abstract", "resumo" }, _options.AbstractMarkers);
    }

    [Fact]
    public void Author_Equality_IsValueBased()
    {
        var labels = new[] { "1", "2" };
        var first = new Author("Jane Doe", labels, "0000-0002-1825-0097");
        var second = new Author("Jane Doe", new[] { "1", "2" }, "0000-0002-1825-0097");

        Assert.Equal(first, second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
    }

    [Fact]
    public void Author_AllowsNullOrcid_AndOmitsItFromToString()
    {
        var author = new Author("José Silva", new[] { "1" }, null);

        Assert.Null(author.OrcidId);
        Assert.DoesNotContain("orcid", author.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("foo@y.edu")]
    [InlineData("first.last+tag@u-aberta.pt")]
    [InlineData("Maria.Silva@usp.br")]
    public void EmailRegex_Matches_RepresentativeAddresses(string email)
    {
        Assert.Matches(_options.EmailRegex, email);
    }

    [Theory]
    [InlineData("foo@bar")]
    [InlineData("foo@.edu")]
    [InlineData("@y.edu")]
    public void EmailRegex_DoesNotMatch_MalformedAddresses(string candidate)
    {
        Assert.DoesNotMatch(_options.EmailRegex, candidate);
    }

    [Theory]
    [InlineData("* E-mail:")]
    [InlineData("*  E-mail :")]
    [InlineData("*Email:")]
    [InlineData("* email :")]
    public void CorrespondingMarkerRegex_Matches_StarEmailVariants(string marker)
    {
        Assert.Matches(_options.CorrespondingMarkerRegex, marker);
    }

    [Theory]
    [InlineData("*")]
    [InlineData("E-mail:")]
    public void CorrespondingMarkerRegex_DoesNotMatch_BareTokens(string candidate)
    {
        Assert.DoesNotMatch(_options.CorrespondingMarkerRegex, candidate);
    }

    [Theory]
    [InlineData("Corresponding Author:")]
    [InlineData("coresponding author -")]
    [InlineData("Correspondign Author")]
    [InlineData("Correspondent Autor")]
    [InlineData("corresponding author —")]
    public void CorrespondingAuthorLabelRegex_Matches_TypoTolerantVariants(string label)
    {
        Assert.Matches(_options.CorrespondingAuthorLabelRegex, label);
    }

    [Fact]
    public void CorrespondingAuthorLabelRegex_DoesNotMatch_CorrespondenceLabel()
    {
        Assert.DoesNotMatch(_options.CorrespondingAuthorLabelRegex, "Correspondence:");
    }

    [Fact]
    public void EmailRegex_ReturnsSameInstanceOnRepeatedReads()
    {
        Assert.Same(_options.EmailRegex, _options.EmailRegex);
    }

    [Fact]
    public void CorrespondingMarkerRegex_ReturnsSameInstanceOnRepeatedReads()
    {
        Assert.Same(_options.CorrespondingMarkerRegex, _options.CorrespondingMarkerRegex);
    }

    [Fact]
    public void CorrespondingAuthorLabelRegex_ReturnsSameInstanceOnRepeatedReads()
    {
        Assert.Same(_options.CorrespondingAuthorLabelRegex, _options.CorrespondingAuthorLabelRegex);
    }

    [Fact]
    public void FormattingOptions_RegisteredAsSingleton_ResolvesSameInstanceAcrossScopes()
    {
        var services = new ServiceCollection();
        services.AddSingleton<FormattingOptions>();
        using var provider = services.BuildServiceProvider();

        FormattingOptions firstScopeInstance;
        FormattingOptions secondScopeInstance;

        using (var firstScope = provider.CreateScope())
        {
            firstScopeInstance = firstScope.ServiceProvider.GetRequiredService<FormattingOptions>();
        }

        using (var secondScope = provider.CreateScope())
        {
            secondScopeInstance = secondScope.ServiceProvider.GetRequiredService<FormattingOptions>();
        }

        Assert.Same(firstScopeInstance, secondScopeInstance);
    }
}
