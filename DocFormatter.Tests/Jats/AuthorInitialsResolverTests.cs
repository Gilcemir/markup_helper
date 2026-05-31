using System.Xml.Linq;
using DocFormatter.Core.Jats;
using Xunit;

namespace DocFormatter.Tests.Jats;

public sealed class AuthorInitialsResolverTests
{
    private static XElement Contrib(string surname, string givenNames, string? suffix = null)
    {
        var name = new XElement("name",
            new XElement("surname", surname),
            new XElement("given-names", givenNames));
        if (suffix is not null)
        {
            name.Add(new XElement("suffix", suffix));
        }

        return new XElement("contrib", new XAttribute("contrib-type", "author"), name);
    }

    private static readonly IReadOnlyList<XElement> Sample = new[]
    {
        Contrib("Lopes", "Danilo Alves Porto da Silva"),
        Contrib("Nascimento", "Ildon Rodrigues do"),
        Contrib("Ferreira", "Osvaldo José", "Júnior"),
        Contrib("Amaral", "Antônio Teixeira do", "Júnior"),
    };

    [Theory]
    [InlineData("Lopes DAPS", "Lopes")]
    [InlineData("Nascimento IRN", "Nascimento")]
    [InlineData("Ferreira Júnior OJ", "Ferreira")]
    public void Resolve_SurnameInitialsForm_ResolvesUniqueContrib(string key, string expectedSurname)
    {
        var result = AuthorInitialsResolver.Resolve(key, Sample);
        Assert.Equal(ResolveStatus.Resolved, result.Status);
        Assert.Equal(expectedSurname, result.Contrib!.Descendants().First(e => e.Name.LocalName == "surname").Value);
    }

    [Fact]
    public void Resolve_BareInitialsForm_MatchesCandidateInitials()
    {
        var result = AuthorInitialsResolver.Resolve("ATAJ", Sample);
        Assert.Equal(ResolveStatus.Resolved, result.Status);
        Assert.Equal("Amaral", result.Contrib!.Descendants().First(e => e.Name.LocalName == "surname").Value);
    }

    [Fact]
    public void Resolve_CommaInitialsForm_ResolvesBySurname()
    {
        var result = AuthorInitialsResolver.Resolve("Lopes, D. A. P. S.", Sample);
        Assert.Equal(ResolveStatus.Resolved, result.Status);
    }

    [Fact]
    public void Resolve_NoMatch_IsNotFound()
    {
        Assert.Equal(ResolveStatus.NotFound, AuthorInitialsResolver.Resolve("Zzz QQ", Sample).Status);
    }

    [Fact]
    public void Resolve_DuplicateSurnameWithoutDistinguishingInitials_IsAmbiguous()
    {
        var contribs = new[]
        {
            Contrib("Silva", "Ana Beatriz"),
            Contrib("Silva", "Carlos Daniel"),
        };
        Assert.Equal(ResolveStatus.Ambiguous, AuthorInitialsResolver.Resolve("Silva ZZ", contribs).Status);
    }

    [Fact]
    public void Resolve_DuplicateSurnameNarrowedByInitials_Resolves()
    {
        var contribs = new[]
        {
            Contrib("Silva", "Ana Beatriz"),
            Contrib("Silva", "Carlos Daniel"),
        };
        // "AB" = given(Ana Beatriz) → matches the first Silva only.
        var result = AuthorInitialsResolver.Resolve("Silva AB", contribs);
        Assert.Equal(ResolveStatus.Resolved, result.Status);
        Assert.Equal("Ana Beatriz", result.Contrib!.Descendants().First(e => e.Name.LocalName == "given-names").Value);
    }

    [Fact]
    public void Resolve_Empty_IsNotFound()
    {
        Assert.Equal(ResolveStatus.NotFound, AuthorInitialsResolver.Resolve("  ", Sample).Status);
    }
}
