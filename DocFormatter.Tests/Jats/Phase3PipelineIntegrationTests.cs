using System.Xml.Linq;
using DocFormatter.Core.Jats;
using DocFormatter.Core.Pipeline;
using DocFormatter.Tests.Fixtures.Phase3;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DocFormatter.Tests.Jats;

/// <summary>
/// Exercises <see cref="Phase3Pipeline"/> end-to-end over a real
/// <see cref="Phase3Context"/>: with stub injectors that both mutate the target
/// <see cref="XDocument"/> and route a value through the <see cref="IConfirmer"/>
/// gate, asserting ordered report entries and that only the XML is changed; and
/// with the registered injectors over a docx read by <see cref="DocxSourceReader"/>.
/// </summary>
public sealed class Phase3PipelineIntegrationTests : IDisposable
{
    private readonly string _tempDir;

    public Phase3PipelineIntegrationTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"docfmt-phase3-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, recursive: true); } catch { /* best effort */ }
    }

    private sealed class ThrowingConfirmer : IConfirmer
    {
        public ConfirmResult Confirm(Proposal proposal)
            => throw new InvalidOperationException($"unexpected prompt from '{proposal.Tag}': {proposal.Reason}");
    }

    private sealed class AppendChildInjector : IJatsInjector
    {
        private readonly string _childName;
        private readonly Func<Phase3Context, string> _valueFactory;

        public AppendChildInjector(string name, string childName, Func<Phase3Context, string> valueFactory)
        {
            Name = name;
            _childName = childName;
            _valueFactory = valueFactory;
        }

        public string Name { get; }

        public RuleSeverity Severity => RuleSeverity.Optional;

        public void Apply(Phase3Context ctx, IReport report)
        {
            var value = _valueFactory(ctx);
            ctx.Xml.Root!.Add(new XElement(_childName, value));
            report.Info(Name, $"injected {_childName}={value}");
        }
    }

    private sealed class RecordingConfirmer : IConfirmer
    {
        public List<Proposal> Seen { get; } = new();

        public ConfirmResult Confirm(Proposal proposal)
        {
            Seen.Add(proposal);
            return new ConfirmResult(proposal.ProposedValue, ConfirmDisposition.AutoApplied);
        }
    }

    [Fact]
    public void Run_TwoStubPipeline_ProducesOrderedReportEntries_AndMutatesOnlyXml()
    {
        var source = new DocxSource
        {
            ElocationId = "e54492621",
            Doi = "10.1590/1984-70332026v26n2a16",
            ScientificEditor = "Jane Roe",
        };
        var xml = new XDocument(new XElement("article", new XElement("front")));
        var confirmer = new RecordingConfirmer();
        var ctx = new Phase3Context
        {
            Source = source,
            Xml = xml,
            OtherNumber = "00123",
            Confirm = confirmer,
        };

        // First injector derives deterministically from OtherNumber; second routes
        // the editor name through the confirmer gate.
        var otherId = new AppendChildInjector("other-id", "article-id", c => c.OtherNumber!);
        var editedBy = new AppendChildInjector(
            "edited-by",
            "fn",
            c => c.Confirm.Confirm(new Proposal("edited-by", c.Source.ScientificEditor!, "editor line")).Value);
        var pipeline = new Phase3Pipeline(new IJatsInjector[] { otherId, editedBy });
        var report = new Report();

        pipeline.Run(ctx, report);

        // Ordered report entries.
        Assert.Collection(
            report.Entries,
            e => Assert.Equal(("other-id", "injected article-id=00123"), (e.Rule, e.Message)),
            e => Assert.Equal(("edited-by", "injected fn=Jane Roe"), (e.Rule, e.Message)));

        // XML mutated in registration order; source untouched.
        var children = xml.Root!.Elements().Select(e => e.Name.LocalName).ToArray();
        Assert.Equal(new[] { "front", "article-id", "fn" }, children);
        Assert.Equal("00123", xml.Root!.Element("article-id")!.Value);
        Assert.Equal("Jane Roe", xml.Root!.Element("fn")!.Value);
        Assert.Equal("Jane Roe", source.ScientificEditor);

        // Confirmer gate was exercised exactly once.
        var seen = Assert.Single(confirmer.Seen);
        Assert.Equal("edited-by", seen.Tag);
        Assert.Equal("Jane Roe", seen.ProposedValue);
    }

    [Fact]
    public void Run_RegisteredInjectors_OverParagraphTaggedSemicolonCreditDocx_InjectsAllRoles_NoWarn()
    {
        // CBAB v26n3 layout (ADR-003): the CREDIT body is [p]-tagged and uses ';'
        // between terms and between author entries. The reader must strip the
        // tags, the parser must accept the separators, and the resolver must map
        // the bare initials, so the whole document auto-applies without a prompt.
        var docxPath = Path.Combine(_tempDir, "5613.docx");
        Phase3DocxFixtureBuilder.WriteMarkupDocxWithCreditStatement(
            docxPath,
            Phase3DocxFixtureBuilder.ParagraphTaggedSemicolonCreditText);
        var source = new DocxSourceReader().Read(docxPath);

        var xml = ArticleWithDoiAndContribs(
            Contrib("Costa", "Ana Beatriz"),
            Contrib("Ferreira", "Daniel Eduardo"),
            Contrib("Iglesias", "Gabriel Henrique"));
        var ctx = new Phase3Context
        {
            Source = source,
            Xml = xml,
            OtherNumber = "00301",
            Confirm = new ThrowingConfirmer(),
        };
        var report = new Report();

        using var provider = new ServiceCollection().AddPhase3Injectors().BuildServiceProvider();
        new Phase3Pipeline(provider.GetServices<IJatsInjector>()).Run(ctx, report);

        // Reader: [p]/[/p] stripped, header seen.
        Assert.Equal("ABC; DEF: Conceptualization; Methodology. GHI: Software.", source.CreditStatementRaw);
        Assert.True(source.CreditHeaderFound);

        // Injector: every contributor received its CRediT roles, in statement order.
        Assert.Equal(
            new[] { "Conceptualization", "Methodology" },
            RolesOf(xml, "Costa").Select(r => r.Value));
        Assert.Equal(
            new[] { "Conceptualization", "Methodology" },
            RolesOf(xml, "Ferreira").Select(r => r.Value));
        Assert.Equal(new[] { "Software" }, RolesOf(xml, "Iglesias").Select(r => r.Value));
        Assert.All(
            xml.Descendants().Where(e => e.Name.LocalName == "role"),
            r => Assert.StartsWith("http://credit.niso.org/contributor-roles/", (string?)r.Attribute("content-type"), StringComparison.Ordinal));

        // Report: nothing at WARN or above across the whole pipeline.
        Assert.Equal(ReportLevel.Info, report.HighestLevel);
        Assert.DoesNotContain(report.Entries, e => e.Level >= ReportLevel.Warn);

        // Outcome (ADR-005): auto-applied, every entry resolved and applied.
        Assert.NotNull(ctx.Credit);
        Assert.Equal(CreditDisposition.AutoApplied, ctx.Credit!.Disposition);
        Assert.Equal(CreditShape.AuthorKeyed, ctx.Credit.Shape);
        Assert.Equal(new[] { "ABC", "DEF", "GHI" }, ctx.Credit.Entries.Select(e => e.AuthorKey));
        Assert.All(ctx.Credit.Entries, e =>
        {
            Assert.Equal(CreditResolution.Resolved, e.Resolution);
            Assert.True(e.Applied);
            Assert.Empty(e.UnknownTerms);
        });
    }

    private static string Contrib(string surname, string givenNames)
        => "\t\t\t\t<contrib contrib-type=\"author\">\n" +
           "\t\t\t\t\t<name>\n" +
           $"\t\t\t\t\t\t<surname>{surname}</surname>\n" +
           $"\t\t\t\t\t\t<given-names>{givenNames}</given-names>\n" +
           "\t\t\t\t\t</name>\n" +
           "\t\t\t\t\t<xref ref-type=\"aff\" rid=\"aff1\">1</xref>\n" +
           "\t\t\t\t</contrib>\n";

    /// <summary>
    /// A minimal JATS front with the DOI <c>article-id</c> the Critical
    /// <c>other-id</c> injector anchors on, plus the given contributors.
    /// </summary>
    private static XDocument ArticleWithDoiAndContribs(params string[] contribs)
        => XDocument.Parse(
            "<article>\n" +
            "\t<front>\n" +
            "\t\t<article-meta>\n" +
            $"\t\t\t<article-id pub-id-type=\"doi\">{Phase3DocxFixtureBuilder.MarkupDoi}</article-id>\n" +
            "\t\t\t<contrib-group>\n" +
            string.Concat(contribs) +
            "\t\t\t</contrib-group>\n" +
            "\t\t</article-meta>\n" +
            "\t</front>\n" +
            "</article>\n",
            LoadOptions.PreserveWhitespace);

    private static IReadOnlyList<XElement> RolesOf(XDocument xml, string surname)
        => xml.Descendants()
            .Single(e => e.Name.LocalName == "contrib"
                && e.Descendants().Any(n => n.Name.LocalName == "surname" && n.Value == surname))
            .Elements()
            .Where(e => e.Name.LocalName == "role")
            .ToList();
}
