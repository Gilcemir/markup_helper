using System.Xml.Linq;
using DocFormatter.Core.Jats;
using DocFormatter.Core.Pipeline;
using Xunit;

namespace DocFormatter.Tests.Jats;

public sealed class CreditRolesInjectorTests
{
    private const string ConceptualizationUrl = "http://credit.niso.org/contributor-roles/conceptualization/";
    private const string MethodologyUrl = "http://credit.niso.org/contributor-roles/methodology/";
    private const string WritingOriginalDraftUrl = "http://credit.niso.org/contributor-roles/writing-original-draft/";

    private sealed class ThrowingConfirmer : IConfirmer
    {
        // A fully-resolved structured statement must auto-apply without prompting.
        public ConfirmResult Confirm(Proposal proposal)
            => throw new InvalidOperationException("CreditRolesInjector must not prompt for a fully-resolved statement.");
    }

    private sealed class StubConfirmer : IConfirmer
    {
        private readonly ConfirmResult _result;

        public StubConfirmer(ConfirmResult result) => _result = result;

        public Proposal? Received { get; private set; }

        public ConfirmResult Confirm(Proposal proposal)
        {
            Received = proposal;
            return _result;
        }
    }

    private static Phase3Context CreateContext(XDocument xml, string? credit, IConfirmer confirmer)
        => new()
        {
            Source = new DocxSource
            {
                ElocationId = "e54492621",
                Doi = "10.1590/x",
                CreditStatementRaw = credit,
            },
            Xml = xml,
            OtherNumber = "00201",
            Confirm = confirmer,
        };

    private static string Contrib(string surname, string givenNames, string? suffix = null)
    {
        var suffixLine = suffix is null ? string.Empty : $"\t\t\t\t<suffix>{suffix}</suffix>\n";
        return
            "\t\t<contrib contrib-type=\"author\">\n" +
            "\t\t\t<name>\n" +
            $"\t\t\t\t<surname>{surname}</surname>\n" +
            $"\t\t\t\t<given-names>{givenNames}</given-names>\n" +
            suffixLine +
            "\t\t\t</name>\n" +
            "\t\t\t<xref ref-type=\"aff\" rid=\"aff1\">1</xref>\n" +
            "\t\t</contrib>\n";
    }

    private static XDocument ArticleWithContribs(params string[] contribs)
    {
        var xml =
            "<article>\n" +
            "\t<contrib-group>\n" +
            string.Concat(contribs) +
            "\t</contrib-group>\n" +
            "</article>\n";
        return XDocument.Parse(xml, LoadOptions.PreserveWhitespace);
    }

    private static IReadOnlyList<XElement> RolesOf(XDocument xml, string surname)
        => xml.Descendants()
            .Single(e => e.Name.LocalName == "contrib"
                && e.Descendants().Any(n => n.Name.LocalName == "surname" && n.Value == surname))
            .Elements()
            .Where(e => e.Name.LocalName == "role")
            .ToList();

    [Fact]
    public void Apply_RoleKeyed_MapsBothAuthorsConceptualization_Auto()
    {
        var xml = ArticleWithContribs(
            Contrib("Lopes", "Danilo Alves Porto da Silva"),
            Contrib("Nascimento", "Ildon Rodrigues do"));

        new CreditRolesInjector().Apply(
            CreateContext(xml, "Conceptualization: Lopes DAPS, Nascimento IRN", new ThrowingConfirmer()),
            new Report());

        var lopes = Assert.Single(RolesOf(xml, "Lopes"));
        var nascimento = Assert.Single(RolesOf(xml, "Nascimento"));
        Assert.Equal(ConceptualizationUrl, (string?)lopes.Attribute("content-type"));
        Assert.Equal(ConceptualizationUrl, (string?)nascimento.Attribute("content-type"));
        Assert.Equal("Conceptualization", lopes.Value);
    }

    [Fact]
    public void Apply_RoleKeyed_DuplicateRoleSpellingForOneAuthor_AutoAppliesOnce()
    {
        // The same CRediT role written twice with different dash spellings
        // (hyphen vs en-dash) for each author. Both spellings normalize to one URL,
        // so the role is emitted once — but the author must still auto-apply without
        // prompting. Regression: isClean used roles.Count == Terms.Count, so the
        // collapsed duplicate (1 role vs 2 terms) made the author "unclean" and the
        // auto-apply branch silently dropped it (no role, no report).
        var xml = ArticleWithContribs(
            Contrib("Lopes", "Danilo Alves Porto da Silva"),
            Contrib("Nascimento", "Ildon Rodrigues do"));

        new CreditRolesInjector().Apply(
            CreateContext(
                xml,
                "Writing - original draft: Lopes DAPS, Nascimento IRN; "
                    + "Writing – original draft: Lopes DAPS, Nascimento IRN",
                new ThrowingConfirmer()),
            new Report());

        var lopes = Assert.Single(RolesOf(xml, "Lopes"));
        var nascimento = Assert.Single(RolesOf(xml, "Nascimento"));
        Assert.Equal(WritingOriginalDraftUrl, (string?)lopes.Attribute("content-type"));
        Assert.Equal(WritingOriginalDraftUrl, (string?)nascimento.Attribute("content-type"));
    }

    [Fact]
    public void Apply_AuthorKeyed_EmitsTwoRolesForResolvedAuthor_Auto()
    {
        // "ATAJ" = given(Antônio Teixeira)→AT + surname(Amaral)→A + suffix(Júnior)→J.
        var xml = ArticleWithContribs(
            Contrib("Amaral", "Antônio Teixeira do", "Júnior"),
            Contrib("Santos", "Talles de Oliveira"));

        new CreditRolesInjector().Apply(
            CreateContext(xml, "ATAJ: Conceptualization, Methodology", new ThrowingConfirmer()),
            new Report());

        var roles = RolesOf(xml, "Amaral");
        Assert.Equal(2, roles.Count);
        Assert.Equal(ConceptualizationUrl, (string?)roles[0].Attribute("content-type"));
        Assert.Equal(MethodologyUrl, (string?)roles[1].Attribute("content-type"));
        Assert.Empty(RolesOf(xml, "Santos"));
    }

    [Fact]
    public void Apply_UnrecognizedTerm_ProposesAndDoesNotSilentlyAutoApply()
    {
        var xml = ArticleWithContribs(Contrib("Amaral", "Antônio Teixeira do", "Júnior"));
        var confirmer = new StubConfirmer(new ConfirmResult(string.Empty, ConfirmDisposition.Skipped));

        new CreditRolesInjector().Apply(
            CreateContext(xml, "ATAJ: Conceptualization, Choreography", confirmer),
            new Report());

        Assert.NotNull(confirmer.Received);
        Assert.Equal("credit-roles", confirmer.Received!.Tag);
        Assert.Contains("Choreography", confirmer.Received.Reason, StringComparison.Ordinal);
        Assert.Empty(RolesOf(xml, "Amaral"));
    }

    [Fact]
    public void Apply_ProseStatement_SurfacesProposal_NotAutoApplied()
    {
        var xml = ArticleWithContribs(
            Contrib("Le", "TTN"),
            Contrib("Do", "HDK"));
        var confirmer = new StubConfirmer(new ConfirmResult(string.Empty, ConfirmDisposition.Skipped));

        new CreditRolesInjector().Apply(
            CreateContext(
                xml,
                "All authors contributed to the study's conception and design. "
                    + "The initial draft was written by TTN Le, and all authors provided feedback.",
                confirmer),
            new Report());

        Assert.NotNull(confirmer.Received);
        Assert.DoesNotContain(xml.Descendants(), e => e.Name.LocalName == "role");
    }

    [Fact]
    public void Apply_InitialsMatchingNoContrib_Unresolved_Prompts()
    {
        var xml = ArticleWithContribs(Contrib("Lopes", "Danilo Alves Porto da Silva"));
        var confirmer = new StubConfirmer(new ConfirmResult(string.Empty, ConfirmDisposition.Skipped));

        new CreditRolesInjector().Apply(
            CreateContext(xml, "Conceptualization: Ferreira XYZ", confirmer),
            new Report());

        Assert.NotNull(confirmer.Received);
        Assert.Contains("unresolved author", confirmer.Received!.Reason, StringComparison.Ordinal);
        Assert.Empty(RolesOf(xml, "Lopes"));
    }

    [Fact]
    public void Apply_InitialsMatchingTwoContribs_Ambiguous_Prompts()
    {
        var xml = ArticleWithContribs(
            Contrib("Silva", "Ana Beatriz"),
            Contrib("Silva", "Carlos Daniel"));
        var confirmer = new StubConfirmer(new ConfirmResult(string.Empty, ConfirmDisposition.Skipped));

        new CreditRolesInjector().Apply(
            CreateContext(xml, "Conceptualization: Silva ZZ", confirmer),
            new Report());

        Assert.NotNull(confirmer.Received);
        Assert.Contains("Ambiguous", confirmer.Received!.Reason, StringComparison.Ordinal);
        Assert.DoesNotContain(xml.Descendants(), e => e.Name.LocalName == "role");
    }

    [Fact]
    public void Apply_ContribAlreadyHasRole_IsSkipped_AndReported()
    {
        var xml = XDocument.Parse(
            "<article>\n" +
            "\t<contrib-group>\n" +
            "\t\t<contrib contrib-type=\"author\">\n" +
            "\t\t\t<name>\n" +
            "\t\t\t\t<surname>Lopes</surname>\n" +
            "\t\t\t\t<given-names>Danilo Alves Porto da Silva</given-names>\n" +
            "\t\t\t</name>\n" +
            "\t\t\t<xref ref-type=\"aff\" rid=\"aff1\">1</xref>\n" +
            "\t\t\t<role content-type=\"http://credit.niso.org/contributor-roles/software/\">Software</role>\n" +
            "\t\t</contrib>\n" +
            "\t</contrib-group>\n" +
            "</article>\n",
            LoadOptions.PreserveWhitespace);
        var report = new Report();

        new CreditRolesInjector().Apply(
            CreateContext(xml, "Conceptualization: Lopes DAPS", new ThrowingConfirmer()),
            report);

        var roles = RolesOf(xml, "Lopes");
        var existing = Assert.Single(roles);
        Assert.Equal("Software", existing.Value);
        var entry = Assert.Single(report.Entries);
        Assert.Contains("already has", entry.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Apply_InsertsRoleElementsAfterXref()
    {
        var xml = ArticleWithContribs(Contrib("Lopes", "Danilo Alves Porto da Silva"));

        new CreditRolesInjector().Apply(
            CreateContext(xml, "Conceptualization: Lopes DAPS; Methodology: Lopes DAPS", new ThrowingConfirmer()),
            new Report());

        var contrib = xml.Descendants().Single(e => e.Name.LocalName == "contrib");
        var children = contrib.Elements().ToList();
        var lastXrefIndex = children.FindLastIndex(e => e.Name.LocalName == "xref");
        var firstRoleIndex = children.FindIndex(e => e.Name.LocalName == "role");
        Assert.True(firstRoleIndex > lastXrefIndex, "roles must follow the contrib's <xref> elements");
        Assert.Equal(2, children.Count(e => e.Name.LocalName == "role"));
    }

    [Fact]
    public void Apply_NoCreditStatement_LeavesXmlUnchanged_AndReportsSkipped()
    {
        var xml = ArticleWithContribs(Contrib("Lopes", "Danilo Alves Porto da Silva"));
        var before = xml.ToString(SaveOptions.DisableFormatting);
        var report = new Report();

        new CreditRolesInjector().Apply(CreateContext(xml, null, new ThrowingConfirmer()), report);

        Assert.Equal(before, xml.ToString(SaveOptions.DisableFormatting));
        var entry = Assert.Single(report.Entries);
        Assert.Equal(ReportLevel.Info, entry.Level);
        Assert.Contains("No CREDIT statement", entry.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Apply_InheritsRootNamespace_NoRedundantXmlns()
    {
        XNamespace ns = "http://jats.nlm.nih.gov";
        var xml = XDocument.Parse(
            $"<article xmlns=\"{ns}\">\n" +
            "\t<contrib-group>\n" +
            "\t\t<contrib contrib-type=\"author\">\n" +
            "\t\t\t<name>\n" +
            "\t\t\t\t<surname>Lopes</surname>\n" +
            "\t\t\t\t<given-names>Danilo Alves Porto da Silva</given-names>\n" +
            "\t\t\t</name>\n" +
            "\t\t\t<xref ref-type=\"aff\" rid=\"aff1\">1</xref>\n" +
            "\t\t</contrib>\n" +
            "\t</contrib-group>\n" +
            "</article>\n",
            LoadOptions.PreserveWhitespace);

        new CreditRolesInjector().Apply(
            CreateContext(xml, "Conceptualization: Lopes DAPS", new ThrowingConfirmer()),
            new Report());

        var role = Assert.Single(xml.Descendants(), e => e.Name.LocalName == "role");
        Assert.Equal(ns, role.Name.Namespace);
        Assert.DoesNotContain(role.DescendantsAndSelf().Attributes(), a => a.IsNamespaceDeclaration);
    }

    [Fact]
    public void Apply_OverCorpusXml_5523_RoleKeyed_AutoAppliesAllRoles()
    {
        var doc = JatsXmlWriter.Load(CorpusPackagePath("1984-7033-cbab-26-02-e55232626.xml"));
        var source = new DocxSourceReader().Read(CorpusMarkupPath("5523.docx"));
        var ctx = new Phase3Context
        {
            Source = source,
            Xml = doc.Document,
            OtherNumber = "00201",
            // A clean, fully-resolved role-keyed statement must never prompt.
            Confirm = new ThrowingConfirmer(),
        };

        new CreditRolesInjector().Apply(ctx, new Report());

        // Every named contributor receives at least one role.
        foreach (var surname in new[] { "Lopes", "Nascimento", "Faria", "Costa", "Casais", "Ferreira" })
        {
            Assert.NotEmpty(RolesOf(doc.Document, surname));
        }

        // Lopes' first listed role is Conceptualization with the canonical URL,
        // inserted after the contrib's <xref>.
        var lopesRoles = RolesOf(doc.Document, "Lopes");
        Assert.Equal(ConceptualizationUrl, (string?)lopesRoles[0].Attribute("content-type"));

        var serialized = doc.Serialize();
        Assert.Contains(
            "<role content-type=\"http://credit.niso.org/contributor-roles/conceptualization/\">Conceptualization</role>",
            serialized,
            StringComparison.Ordinal);
        // The en-dash "Writing" terms serialize with their canonical spelling.
        Assert.Contains(
            "<role content-type=\"http://credit.niso.org/contributor-roles/writing-original-draft/\">Writing – original draft</role>",
            serialized,
            StringComparison.Ordinal);
    }

    // ── ADR-007: operator-chosen, document-scoped free-text fallback ──────────

    private static IReadOnlyList<XElement> AllRoles(XDocument xml)
        => xml.Descendants().Where(e => e.Name.LocalName == "role").ToList();

    [Fact]
    public void Apply_FreeText_GateMarksProposalFreeTextEligible()
    {
        // The unrecognized/unresolved branch is the only one that offers free text.
        var xml = ArticleWithContribs(Contrib("Lopes", "Danilo Alves Porto da Silva"));
        var confirmer = new StubConfirmer(new ConfirmResult(string.Empty, ConfirmDisposition.Skipped));

        new CreditRolesInjector().Apply(
            CreateContext(xml, "Conceptualization: Lopes DAPS, Ferreira XYZ", confirmer),
            new Report());

        Assert.NotNull(confirmer.Received);
        Assert.True(confirmer.Received!.AllowsFreeText);
    }

    [Fact]
    public void Apply_FreeText_EmitsEveryAuthorTerm_AsPlainText_NoContentType()
    {
        var xml = ArticleWithContribs(
            Contrib("Lopes", "Danilo Alves Porto da Silva"),
            Contrib("Nascimento", "Ildon Rodrigues do"));
        // "Choreography" is not a CRediT term → gate; both authors resolve.
        var confirmer = new StubConfirmer(new ConfirmResult(string.Empty, ConfirmDisposition.FreeText));

        new CreditRolesInjector().Apply(
            CreateContext(
                xml,
                "Conceptualization: Lopes DAPS, Nascimento IRN; Choreography: Lopes DAPS",
                confirmer),
            new Report());

        var lopes = RolesOf(xml, "Lopes");
        var nascimento = RolesOf(xml, "Nascimento");
        Assert.Equal(new[] { "Conceptualization", "Choreography" }, lopes.Select(r => r.Value));
        Assert.Equal(new[] { "Conceptualization" }, nascimento.Select(r => r.Value));

        // Including the CRediT-matching term, no emitted role carries @content-type.
        Assert.All(AllRoles(xml), r => Assert.Null(r.Attribute("content-type")));
    }

    [Fact]
    public void Apply_FreeText_DocumentHasZeroContentTypeOnAnyRole()
    {
        var xml = ArticleWithContribs(
            Contrib("Lopes", "Danilo Alves Porto da Silva"),
            Contrib("Nascimento", "Ildon Rodrigues do"));
        var confirmer = new StubConfirmer(new ConfirmResult(string.Empty, ConfirmDisposition.FreeText));

        new CreditRolesInjector().Apply(
            CreateContext(
                xml,
                "Conceptualization: Lopes DAPS; Methodology: Nascimento IRN; Choreography: Lopes DAPS",
                confirmer),
            new Report());

        var roles = AllRoles(xml);
        Assert.NotEmpty(roles);
        Assert.DoesNotContain(roles, r => r.Attribute("content-type") != null);
    }

    [Fact]
    public void Apply_FreeText_ContribAlreadyHasRole_IsSkipped_IdempotencyUnchanged()
    {
        var xml = XDocument.Parse(
            "<article>\n" +
            "\t<contrib-group>\n" +
            "\t\t<contrib contrib-type=\"author\">\n" +
            "\t\t\t<name>\n" +
            "\t\t\t\t<surname>Lopes</surname>\n" +
            "\t\t\t\t<given-names>Danilo Alves Porto da Silva</given-names>\n" +
            "\t\t\t</name>\n" +
            "\t\t\t<xref ref-type=\"aff\" rid=\"aff1\">1</xref>\n" +
            "\t\t\t<role>conception</role>\n" +
            "\t\t</contrib>\n" +
            "\t</contrib-group>\n" +
            "</article>\n",
            LoadOptions.PreserveWhitespace);
        var report = new Report();
        var confirmer = new StubConfirmer(new ConfirmResult(string.Empty, ConfirmDisposition.FreeText));

        new CreditRolesInjector().Apply(
            CreateContext(xml, "Conceptualization: Lopes DAPS; Choreography: Lopes DAPS", confirmer),
            report);

        var existing = Assert.Single(RolesOf(xml, "Lopes"));
        Assert.Equal("conception", existing.Value);
        Assert.Contains(report.Entries, e => e.Message.Contains("already has", StringComparison.Ordinal));
    }

    [Fact]
    public void Apply_FreeText_RecordsFreeTextDispositionInReport()
    {
        var xml = ArticleWithContribs(Contrib("Lopes", "Danilo Alves Porto da Silva"));
        var report = new Report();
        var confirmer = new StubConfirmer(new ConfirmResult(string.Empty, ConfirmDisposition.FreeText));

        new CreditRolesInjector().Apply(
            CreateContext(xml, "Conceptualization: Lopes DAPS; Choreography: Lopes DAPS", confirmer),
            report);

        Assert.Contains(
            report.Entries,
            e => e.Message.StartsWith("Injected", StringComparison.Ordinal)
                && e.Message.Contains(ConfirmDisposition.FreeText.ToString(), StringComparison.Ordinal));
    }

    [Fact]
    public void Apply_OverCorpusXml_e54582628_FreeText_EmitsResolvedAuthors_NoContentType()
    {
        var doc = JatsXmlWriter.Load(CorpusPackagePath("1984-7033-cbab-26-02-e54582628.xml"));
        var source = new DocxSourceReader().Read(CorpusMarkupPath("5458.docx"));
        var confirmer = new StubConfirmer(new ConfirmResult(string.Empty, ConfirmDisposition.FreeText));
        var report = new Report();
        var ctx = new Phase3Context
        {
            Source = source,
            Xml = doc.Document,
            OtherNumber = "00201",
            Confirm = confirmer,
        };

        new CreditRolesInjector().Apply(ctx, report);

        // Previously-dropped resolved authors (unrecognized terms) now carry their
        // contributions as free text, alongside the previously-clean subset.
        foreach (var surname in new[] { "Nascimento", "Ishikawa", "Costa", "Borel", "Araújo" })
        {
            Assert.NotEmpty(RolesOf(doc.Document, surname));
        }

        // The whole document is uniform: not a single role carries @content-type.
        Assert.NotEmpty(AllRoles(doc.Document));
        Assert.DoesNotContain(AllRoles(doc.Document), r => r.Attribute("content-type") != null);

        // A term that would have CRediT-matched is written as plain text.
        Assert.Contains(RolesOf(doc.Document, "Costa"), r => r.Value == "Conceptualization");

        // "Neto" (surname Paiva / suffix Neto) stays unresolved — reported, not
        // silently dropped (suffix resolution is out of scope, ADR-007).
        Assert.Empty(RolesOf(doc.Document, "Paiva"));
        Assert.Contains(
            report.Entries,
            e => e.Level == ReportLevel.Warn && e.Message.Contains("unresolved", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Apply_OverCorpusXml_e54582628_Accept_KeepsCleanSubset_NoFreeTextAutoSelected()
    {
        var doc = JatsXmlWriter.Load(CorpusPackagePath("1984-7033-cbab-26-02-e54582628.xml"));
        var source = new DocxSourceReader().Read(CorpusMarkupPath("5458.docx"));
        var ctx = new Phase3Context
        {
            Source = source,
            Xml = doc.Document,
            OtherNumber = "00201",
            Confirm = new AutoAcceptConfirmer(),
        };

        new CreditRolesInjector().Apply(ctx, new Report());

        // accept keeps the clean CRediT subset (Costa/Borel/Araújo) with @content-type;
        // free text is never auto-selected.
        var roles = AllRoles(doc.Document);
        Assert.NotEmpty(roles);
        Assert.All(roles, r => Assert.NotNull(r.Attribute("content-type")));
        Assert.Empty(RolesOf(doc.Document, "Nascimento"));
        Assert.Empty(RolesOf(doc.Document, "Ishikawa"));
        Assert.NotEmpty(RolesOf(doc.Document, "Costa"));
    }

    [Fact]
    public void Apply_AutoApply_WritingReviewEditingTerm_SerializesAmpersandAsEntity()
    {
        // The canonical CRediT Display for the writing-review-editing slug carries
        // a literal '&' (CreditTermTable). The deterministic path stores it as text
        // on the XElement; the serializer (XLinq → XmlWriter) is responsible for
        // escaping it to "&amp;" on the way out, otherwise the emitted XML is
        // malformed.
        var xml = ArticleWithContribs(Contrib("Lopes", "Danilo Alves Porto da Silva"));

        new CreditRolesInjector().Apply(
            CreateContext(xml, "Writing - review & editing: Lopes DAPS", new ThrowingConfirmer()),
            new Report());

        var role = Assert.Single(RolesOf(xml, "Lopes"));
        Assert.Equal("Writing – review & editing", role.Value);

        var serialized = xml.ToString(SaveOptions.DisableFormatting);
        Assert.Contains("Writing – review &amp; editing</role>", serialized, StringComparison.Ordinal);
        Assert.DoesNotContain("Writing – review & editing</role>", serialized, StringComparison.Ordinal);
    }

    [Fact]
    public void Apply_FreeText_VerbatimTermWithAmpersand_SerializesAmpersandAsEntity()
    {
        // The free-text path (ADR-007) emits the verbatim written term from the docx
        // without @content-type. A custom term that contains '&' must serialize as
        // "&amp;" — failing to escape would produce malformed XML in the only branch
        // where unmapped author prose reaches <role> as-is.
        var xml = ArticleWithContribs(Contrib("Lopes", "Danilo Alves Porto da Silva"));
        var confirmer = new StubConfirmer(new ConfirmResult(string.Empty, ConfirmDisposition.FreeText));

        new CreditRolesInjector().Apply(
            CreateContext(
                xml,
                "Conceptualization: Lopes DAPS; Conception & design: Lopes DAPS",
                confirmer),
            new Report());

        var roles = RolesOf(xml, "Lopes");
        Assert.Equal(2, roles.Count);
        Assert.Equal("Conception & design", roles[1].Value);

        var serialized = xml.ToString(SaveOptions.DisableFormatting);
        Assert.Contains("<role>Conception &amp; design</role>", serialized, StringComparison.Ordinal);
        Assert.DoesNotContain("Conception & design</role>", serialized, StringComparison.Ordinal);
    }

    [Fact]
    public void Apply_OverCorpusXml_5449_Prose_SurfacesProposal_NoRolesWritten()
    {
        var doc = JatsXmlWriter.Load(CorpusPackagePath("1984-7033-cbab-26-02-e54492621.xml"));
        var source = new DocxSourceReader().Read(CorpusMarkupPath("5449.docx"));
        var confirmer = new StubConfirmer(new ConfirmResult(string.Empty, ConfirmDisposition.Skipped));
        var ctx = new Phase3Context
        {
            Source = source,
            Xml = doc.Document,
            OtherNumber = "00201",
            Confirm = confirmer,
        };

        new CreditRolesInjector().Apply(ctx, new Report());

        Assert.NotNull(confirmer.Received);
        Assert.DoesNotContain(doc.Document.Descendants(), e => e.Name.LocalName == "role");
    }

    private static string CorpusPackagePath(string file) => CorpusPath("scielo_package", file);

    private static string CorpusMarkupPath(string file) => CorpusPath("scielo_markup", file);

    private static string CorpusPath(string subDir, string file)
    {
        var dir = AppContext.BaseDirectory;
        while (dir != null)
        {
            var candidate = Path.Combine(dir, "examples", "phase-3", subDir, file);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            dir = Path.GetDirectoryName(dir);
        }

        throw new InvalidOperationException(
            $"Could not locate examples/phase-3/{subDir}/{file} from {AppContext.BaseDirectory}.");
    }
}
