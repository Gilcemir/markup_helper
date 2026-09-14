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

    [Fact]
    public void Parse_AuthorKeyed_WithMalformedSegment_FallsBackToProse()
    {
        // A non-empty segment that is not "keys: terms" makes the statement
        // ambiguous: fall back to Prose (prompt) instead of silently auto-applying
        // only the well-formed segments (P3 / ADR-005).
        var raw = "ATAJ: Conceptualization, Methodology. see acknowledgements. DRSJ: Software";
        Assert.Equal(CreditShape.Prose, CreditStatementParser.Parse(raw).Shape);
    }

    // ── CBAB v26n3 corpus (ADR-003) ──────────────────────────────────────────
    // Real statement texts as read by DocxSourceReader from the edition's docx.

    private const string Corpus5642 =
        "EVT: Conceptualization; Data curation; Formal analysis; Investigation; Methodology; Software; "
        + "Supervision; Validation; Visualization; Writing - original draft; Writing - review & editing.";

    private const string Corpus5412 =
        "AN; SR: Conceptualization; Funding acquisition; Supervision; Writing - original draft; "
        + "Writing - review & editing; Validation; Data curation; Formal analysis; Investigation; "
        + "Methodology; Resources; Visualization.";

    private const string Corpus5501 =
        "CFA; MN; VCF: Conceptualization; CFA; MN; MDVR; CDC: Methodology; CFA; MN; VCF: Software; "
        + "CFA; MN: Formal analysis; CFA; MN; VCF: Visualization; CFA; MN: Investigation; "
        + "CFA: Project administration; MDVR; CDC: Supervision; CFA; MN; VCF; MDVR; CDC: Validation; "
        + "CFA: Funding acquisition; CFA; MN; VCF; MDVR; CDC: Resources; CFA; MN: Writing - original draft; "
        + "CFA; MN; VCF; MDVR; CDC: Writing - review & editing.";

    // The double space in "Writing  original draft" is in the docx (the dash was
    // deleted by the author); CreditTermTable.Normalize folds it, so it maps.
    private const string Corpus5547 =
        "EAA: Conceptualization, Data curation, Formal analysis, Investigation, Methodology, Software, "
        + "Supervision, Validation, Visualization, Writing  original draft, Writing  review & editing; "
        + "ASGC: Conceptualization, Investigation, Methodology, Software, Supervision, Validation, "
        + "Visualization, Writing  review & editing. "
        + "HSP: Conceptualization, Data curation, Formal analysis, Funding acquisition, Investigation, "
        + "Methodology, Project administration, Resources, Supervision, Validation, Visualization, "
        + "Writing  review & editing; "
        + "LCM: Conceptualization, Data curation, Formal analysis, Funding acquisition, Investigation, "
        + "Methodology, Project administration, Resources, Supervision, Validation, Visualization, "
        + "Writing  review & editing; "
        + "PGSM: Conceptualization, Formal analysis, Investigation, Methodology, Supervision, Validation, "
        + "Visualization, Writing review & editing.";

    private const string Corpus5613 =
        "NHN: Conceptualization, Funding acquisition, Investigation, Methodology, Project administration, "
        + "Resources, Software, Supervision, Visualization, Writing - original draft, Writing - review & editing; "
        + "TVB: Data curation, Formal analysis, Funding acquisition, Methodology, Resources, Software, "
        + "Validation, Visualization, Writing - original draft, Writing - review & editing; "
        + "QHTP: Data curation, Formal analysis.";

    private const string Corpus5528 =
        "JILR; AAMV; ORJC; AAD; LBB: Conceptualization, Writing - Original Draft; Writing - review & editing. "
        + "JILR; ORJC; LBB: Data curation. JILR; ORJC; AAD; LBB: Methodology. JILR; ORJC: Formal analysis. "
        + "JILR; ORJC: Visualization. AAMV; LBB: Funding acquisition. AAMV; LBB: Project administration. "
        + "AAMV; AAD; LBB: Resources. ORJC; AAD; LBB: Supervision. ORJC: Validation.";

    private const string Corpus5441 =
        "CFA; JAC: Conceptualization. CFA; JAC; MN; ACCN; CDC: Methodology. CFA; JAC: Data curation. "
        + "CFA; JAC: Formal analysis; Investigation; Software; Visualization; Writing - original draft. "
        + "CFA; JAC; MN; ACCN; CDC: Writing - review & editing.";

    [Fact]
    public void Parse_Corpus5642_SemicolonBetweenTerms_IsAuthorKeyed()
    {
        var statement = CreditStatementParser.Parse(Corpus5642);

        Assert.Equal(CreditShape.AuthorKeyed, statement.Shape);
        var evt = Assert.Single(statement.Entries);
        Assert.Equal("EVT", evt.AuthorKey);
        Assert.Equal(11, evt.Terms.Count);
        Assert.All(evt.Terms, t => Assert.True(CreditTermTable.TryMap(t, out _), $"unmapped term '{t}'"));
    }

    [Fact]
    public void Parse_Corpus5412_SharedKeysWithSemicolonTerms_IsAuthorKeyed()
    {
        var statement = CreditStatementParser.Parse(Corpus5412);

        Assert.Equal(CreditShape.AuthorKeyed, statement.Shape);
        Assert.Equal(new[] { "AN", "SR" }, statement.Entries.Select(e => e.AuthorKey));
        var an = statement.Entries[0].Terms;
        Assert.Equal(12, an.Count);
        Assert.Equal(an, statement.Entries[1].Terms);
        Assert.All(an, t => Assert.True(CreditTermTable.TryMap(t, out _), $"unmapped term '{t}'"));
    }

    [Fact]
    public void Parse_Corpus5501_SemicolonBetweenEntriesAndTerms_UsesInitialsLookback()
    {
        var statement = CreditStatementParser.Parse(Corpus5501);

        Assert.Equal(CreditShape.AuthorKeyed, statement.Shape);
        Assert.Equal(new[] { "CFA", "MN", "VCF", "MDVR", "CDC" }, statement.Entries.Select(e => e.AuthorKey));

        var cdc = Assert.Single(statement.Entries, e => e.AuthorKey == "CDC");
        Assert.Equal(
            new[] { "Methodology", "Supervision", "Validation", "Resources", "Writing - review & editing" },
            cdc.Terms);

        var cfa = Assert.Single(statement.Entries, e => e.AuthorKey == "CFA");
        Assert.Equal(12, cfa.Terms.Count);
        Assert.Contains("Project administration", cfa.Terms);
        Assert.Contains("Funding acquisition", cfa.Terms);

        var mn = Assert.Single(statement.Entries, e => e.AuthorKey == "MN");
        Assert.Equal(
            new[]
            {
                "Conceptualization", "Methodology", "Software", "Formal analysis", "Visualization",
                "Investigation", "Validation", "Resources", "Writing - original draft", "Writing - review & editing",
            },
            mn.Terms);

        AssertNoJunkTerms(statement);
    }

    [Fact]
    public void Parse_Corpus5547_SemicolonAndDotBetweenEntries_IsAuthorKeyed()
    {
        var statement = CreditStatementParser.Parse(Corpus5547);

        Assert.Equal(CreditShape.AuthorKeyed, statement.Shape);
        Assert.Equal(new[] { "EAA", "ASGC", "HSP", "LCM", "PGSM" }, statement.Entries.Select(e => e.AuthorKey));

        var asgc = Assert.Single(statement.Entries, e => e.AuthorKey == "ASGC");
        Assert.Equal(
            new[]
            {
                "Conceptualization", "Investigation", "Methodology", "Software", "Supervision", "Validation",
                "Visualization", "Writing  review & editing",
            },
            asgc.Terms);

        Assert.Equal(11, Assert.Single(statement.Entries, e => e.AuthorKey == "EAA").Terms.Count);
        Assert.Equal(12, Assert.Single(statement.Entries, e => e.AuthorKey == "HSP").Terms.Count);
        Assert.Equal(12, Assert.Single(statement.Entries, e => e.AuthorKey == "LCM").Terms.Count);
        Assert.Equal(8, Assert.Single(statement.Entries, e => e.AuthorKey == "PGSM").Terms.Count);
        AssertNoJunkTerms(statement);
    }

    [Fact]
    public void Parse_Corpus5613_SemicolonBetweenEntries_IsAuthorKeyed()
    {
        var statement = CreditStatementParser.Parse(Corpus5613);

        Assert.Equal(CreditShape.AuthorKeyed, statement.Shape);
        Assert.Equal(new[] { "NHN", "TVB", "QHTP" }, statement.Entries.Select(e => e.AuthorKey));
        Assert.Equal(
            new[] { "Data curation", "Formal analysis" },
            Assert.Single(statement.Entries, e => e.AuthorKey == "QHTP").Terms);
        Assert.Equal(11, Assert.Single(statement.Entries, e => e.AuthorKey == "NHN").Terms.Count);
        Assert.Equal(10, Assert.Single(statement.Entries, e => e.AuthorKey == "TVB").Terms.Count);
        AssertNoJunkTerms(statement);
    }

    [Fact]
    public void Parse_Corpus5528_IsolatedSemicolonInsideCommaList_SplitsTerms()
    {
        var statement = CreditStatementParser.Parse(Corpus5528);

        Assert.Equal(CreditShape.AuthorKeyed, statement.Shape);
        Assert.Equal(new[] { "JILR", "AAMV", "ORJC", "AAD", "LBB" }, statement.Entries.Select(e => e.AuthorKey));

        var jilr = Assert.Single(statement.Entries, e => e.AuthorKey == "JILR");
        Assert.Equal(
            new[]
            {
                "Conceptualization", "Writing - Original Draft", "Writing - review & editing", "Data curation",
                "Methodology", "Formal analysis", "Visualization",
            },
            jilr.Terms);

        var orjc = Assert.Single(statement.Entries, e => e.AuthorKey == "ORJC");
        Assert.Contains("Validation", orjc.Terms);
        AssertNoJunkTerms(statement);
    }

    [Fact]
    public void Parse_Corpus5441_IsolatedSemicolonList_DeduplicatesPerAuthor()
    {
        var statement = CreditStatementParser.Parse(Corpus5441);

        Assert.Equal(CreditShape.AuthorKeyed, statement.Shape);
        Assert.Equal(new[] { "CFA", "JAC", "MN", "ACCN", "CDC" }, statement.Entries.Select(e => e.AuthorKey));

        var expected = new[]
        {
            "Conceptualization", "Methodology", "Data curation", "Formal analysis", "Investigation", "Software",
            "Visualization", "Writing - original draft", "Writing - review & editing",
        };
        Assert.Equal(expected, Assert.Single(statement.Entries, e => e.AuthorKey == "CFA").Terms);
        Assert.Equal(expected, Assert.Single(statement.Entries, e => e.AuthorKey == "JAC").Terms);
        Assert.Equal(
            new[] { "Methodology", "Writing - review & editing" },
            Assert.Single(statement.Entries, e => e.AuthorKey == "CDC").Terms);
        AssertNoJunkTerms(statement);
    }

    [Theory]
    [InlineData("ABC: Methodology; DEF")]                         // pending key never closed by a ':' piece
    [InlineData("ABC: Methodology; DEF; Software")]               // initials block followed by a term, not ':'
    [InlineData("ABC: ; DEF: Software")]                          // entry without terms
    [InlineData("ABC: Methodology; DEF: Software. GHI")]          // '.'-segment without ':'
    [InlineData("ABC: Foo; DEF: Bar")]                            // no term maps to a CRediT term
    public void Parse_AuthorKeyedLookbackViolations_FallBackToProse(string raw)
    {
        Assert.Equal(CreditShape.Prose, CreditStatementParser.Parse(raw).Shape);
    }

    [Fact]
    public void Parse_AuthorKeyed_KeysBeforeFirstColonSplitOnComma()
    {
        var statement = CreditStatementParser.Parse("ABC, DEF: Methodology; GHI: Software.");

        Assert.Equal(CreditShape.AuthorKeyed, statement.Shape);
        Assert.Equal(new[] { "ABC", "DEF", "GHI" }, statement.Entries.Select(e => e.AuthorKey));
        Assert.Equal(new[] { "Methodology" }, statement.Entries[1].Terms);
        Assert.Equal(new[] { "Software" }, statement.Entries[2].Terms);
    }

    private static void AssertNoJunkTerms(CreditStatement statement)
    {
        foreach (var entry in statement.Entries)
        {
            Assert.All(entry.Terms, t =>
            {
                Assert.DoesNotContain(";", t);
                Assert.DoesNotContain(":", t);
            });
        }
    }
}
