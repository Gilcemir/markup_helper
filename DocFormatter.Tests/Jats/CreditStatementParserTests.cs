using DocFormatter.Core.Jats;
using Xunit;

namespace DocFormatter.Tests.Jats;

public sealed class CreditStatementParserTests
{
    [Fact]
    public void Parse_Null_IsProse()
    {
        Assert.Equal(CreditShape.Prose, CreditStatementParser.Parse(null).Shape);
    }

    [Fact]
    public void Parse_RoleKeyed_MergesTermsPerAuthorInOrder()
    {
        var statement = CreditStatementParser.Parse(
            "Conceptualization: Lopes DAPS, Nascimento IRN; Methodology: Lopes DAPS.");

        Assert.Equal(CreditShape.RoleKeyed, statement.Shape);
        var lopes = Assert.Single(statement.Entries, e => e.AuthorKey == "Lopes DAPS");
        Assert.Equal(new[] { "Conceptualization", "Methodology" }, lopes.Terms);
        var nascimento = Assert.Single(statement.Entries, e => e.AuthorKey == "Nascimento IRN");
        Assert.Equal(new[] { "Conceptualization" }, nascimento.Terms);
    }

    [Fact]
    public void Parse_AuthorKeyed_SharesTermsAcrossInitialBlock()
    {
        var statement = CreditStatementParser.Parse(
            "ATAJ: Conceptualization, Methodology. DRSJ; TOS: Investigation, Data curation.");

        Assert.Equal(CreditShape.AuthorKeyed, statement.Shape);
        Assert.Equal(new[] { "Conceptualization", "Methodology" },
            Assert.Single(statement.Entries, e => e.AuthorKey == "ATAJ").Terms);
        Assert.Equal(new[] { "Investigation", "Data curation" },
            Assert.Single(statement.Entries, e => e.AuthorKey == "DRSJ").Terms);
        Assert.Equal(new[] { "Investigation", "Data curation" },
            Assert.Single(statement.Entries, e => e.AuthorKey == "TOS").Terms);
    }

    [Theory]
    [InlineData("All authors contributed to the study's conception and design.")]
    [InlineData("Both authors participated in the development and implementation of the algorithms.")]
    [InlineData("KB Viandro, AT Bruzi and MF Santos conceived and designed the study.")]
    public void Parse_Prose_IsProse(string raw)
    {
        Assert.Equal(CreditShape.Prose, CreditStatementParser.Parse(raw).Shape);
    }

    [Fact]
    public void Parse_CompoundLabelRoleKeyed_FallsBackToProse()
    {
        // 5313-style: compound labels + comma-initials defeat clean detection, so
        // the statement is surfaced for confirmation rather than mis-parsed.
        var raw = "Conceptualization and Supervision: Viana, A. P.; Gonzaga, M. P. "
            + "Writing - original draft: Silva, F. A.";
        Assert.Equal(CreditShape.Prose, CreditStatementParser.Parse(raw).Shape);
    }
}
