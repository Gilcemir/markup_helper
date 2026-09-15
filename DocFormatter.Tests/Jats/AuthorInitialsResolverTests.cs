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

    private static string SurnameOf(AuthorResolution result)
        => result.Contrib!.Descendants().First(e => e.Name.LocalName == "surname").Value;

    // v26n3 5524: "SA" is Saleem Abid (full, Tier 1) and also the given-names
    // initials of Sajjad Ahmad Khan (Tier 2). Tier 1 must win.
    [Fact]
    public void Resolve_BareInitials_FullNameTierWinsOverGivenOnly()
    {
        var contribs = new[]
        {
            Contrib("Abid", "Saleem"),
            Contrib("Khan", "Sajjad Ahmad"),
            Contrib("Sohail", "Muhammad"),
            Contrib("Zahid", "Saleem"),
        };
        var result = AuthorInitialsResolver.Resolve("SA", contribs);
        Assert.Equal(ResolveStatus.Resolved, result.Status);
        Assert.Equal("Abid", SurnameOf(result));
    }

    // v26n3 5528: hyphenated surname yields both one- and two-initial variants.
    [Theory]
    [InlineData("LBB")]
    [InlineData("LB")]
    public void Resolve_BareInitials_HyphenatedSurnameVariants(string key)
    {
        var contribs = new[]
        {
            Contrib("Barboza-Barquero", "Luis"),
            Contrib("Rojas", "Carlos"),
        };
        var result = AuthorInitialsResolver.Resolve(key, contribs);
        Assert.Equal(ResolveStatus.Resolved, result.Status);
        Assert.Equal("Barboza-Barquero", SurnameOf(result));
    }

    [Theory]
    [InlineData("Barboza‐Barquero", "LBB")]
    [InlineData("Barboza–Barquero", "LBB")]
    public void Resolve_BareInitials_UnicodeHyphensSplitLikeAsciiHyphen(string surname, string key)
    {
        var contribs = new[] { Contrib(surname, "Luis") };
        Assert.Equal(ResolveStatus.Resolved, AuthorInitialsResolver.Resolve(key, contribs).Status);
    }

    // v26n3 5613: a capitalized particle ("Van") yields kept and dropped variants.
    [Theory]
    [InlineData("TVB")]
    [InlineData("TB")]
    public void Resolve_BareInitials_CapitalizedParticleVariants(string key)
    {
        var contribs = new[]
        {
            Contrib("Bui", "Truong Van"),
            Contrib("Nguyen", "Hoai"),
        };
        var result = AuthorInitialsResolver.Resolve(key, contribs);
        Assert.Equal(ResolveStatus.Resolved, result.Status);
        Assert.Equal("Bui", SurnameOf(result));
    }

    [Theory]
    [InlineData("MS")]
    [InlineData("MDS")]
    public void Resolve_BareInitials_CapitalizedParticleInSurname(string key)
    {
        var contribs = new[] { Contrib("Da Silva", "Maria") };
        Assert.Equal(ResolveStatus.Resolved, AuthorInitialsResolver.Resolve(key, contribs).Status);
    }

    [Fact]
    public void Resolve_BareInitials_LowercaseParticleIsAlwaysDropped()
    {
        var contribs = new[] { Contrib("Silva", "Maria da") };
        Assert.Equal(ResolveStatus.Resolved, AuthorInitialsResolver.Resolve("MS", contribs).Status);
        Assert.Equal(ResolveStatus.NotFound, AuthorInitialsResolver.Resolve("MDS", contribs).Status);
    }

    // v26n3 5529 / 5617 / 5719: initials that skip a name token have no variant.
    [Theory]
    [InlineData("TTR", "Rocha", "Taine Teotônio Teixeira da")]
    [InlineData("JGS", "Simão", "Janine Magalhães Guedes")]
    [InlineData("MRC", "Costa", "Marcia")]
    public void Resolve_BareInitials_SkippedNameToken_IsNotFound(string key, string surname, string givenNames)
    {
        var contribs = new[] { Contrib(surname, givenNames) };
        Assert.Equal(ResolveStatus.NotFound, AuthorInitialsResolver.Resolve(key, contribs).Status);
    }

    [Fact]
    public void Resolve_BareInitials_TwoFullMatchesInTierOne_IsAmbiguous()
    {
        var contribs = new[]
        {
            Contrib("Abid", "Saleem"),
            Contrib("Ahmad", "Sara"),
        };
        Assert.Equal(ResolveStatus.Ambiguous, AuthorInitialsResolver.Resolve("SA", contribs).Status);
    }

    // 5316-style: the full name survives only in <given-names>; the given-only
    // tier still resolves when no full candidate matches anywhere.
    [Fact]
    public void Resolve_BareInitials_GivenOnlyFallbackWhenNoFullMatch()
    {
        var contribs = new[]
        {
            Contrib("0000-0001-2345-6789", "Maria Aparecida Fernandes"),
            Contrib("Khan", "Sajjad Ahmad"),
        };
        var maf = AuthorInitialsResolver.Resolve("MAF", contribs);
        Assert.Equal(ResolveStatus.Resolved, maf.Status);
        Assert.Equal("Maria Aparecida Fernandes", maf.Contrib!.Descendants().First(e => e.Name.LocalName == "given-names").Value);

        var sa = AuthorInitialsResolver.Resolve("SA", contribs);
        Assert.Equal(ResolveStatus.Resolved, sa.Status);
        Assert.Equal("Khan", SurnameOf(sa));
    }

    [Fact]
    public void Resolve_BareInitials_TwoGivenOnlyMatchesInTierTwo_IsAmbiguous()
    {
        var contribs = new[]
        {
            Contrib("Khan", "Sajjad Ahmad"),
            Contrib("Malik", "Sara Aisha"),
        };
        Assert.Equal(ResolveStatus.Ambiguous, AuthorInitialsResolver.Resolve("SA", contribs).Status);
    }

    [Fact]
    public void Resolve_DuplicateSurnameNarrowedByHyphenVariant_Resolves()
    {
        var contribs = new[]
        {
            Contrib("Silva", "Ana-Beatriz"),
            Contrib("Silva", "Carlos Daniel"),
        };
        var result = AuthorInitialsResolver.Resolve("Silva AB", contribs);
        Assert.Equal(ResolveStatus.Resolved, result.Status);
        Assert.Equal("Ana-Beatriz", result.Contrib!.Descendants().First(e => e.Name.LocalName == "given-names").Value);
    }
}
