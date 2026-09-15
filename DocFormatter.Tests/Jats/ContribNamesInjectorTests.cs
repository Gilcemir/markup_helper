using System.Xml.Linq;
using DocFormatter.Core.Jats;
using DocFormatter.Core.Pipeline;
using Xunit;

namespace DocFormatter.Tests.Jats;

public sealed class ContribNamesInjectorTests
{
    private const string RuleName = "contrib-names";

    private sealed class ThrowingConfirmer : IConfirmer
    {
        // A verification-only rule must never reach the confirmation gate.
        public ConfirmResult Confirm(Proposal proposal)
            => throw new InvalidOperationException("ContribNamesInjector must not prompt.");
    }

    private static Phase3Context CreateContext(XDocument xml)
        => new()
        {
            Source = new DocxSource { ElocationId = "e53162630", Doi = "10.1590/x" },
            Xml = xml,
            OtherNumber = "00301",
            Confirm = new ThrowingConfirmer(),
        };

    private static string Contrib(string surname, string givenNames)
        => "\t\t<contrib contrib-type=\"author\">\n" +
           "\t\t\t<name>\n" +
           $"\t\t\t\t<surname>{surname}</surname>\n" +
           $"\t\t\t\t<given-names>{givenNames}</given-names>\n" +
           "\t\t\t</name>\n" +
           "\t\t\t<xref ref-type=\"aff\" rid=\"aff1\">1</xref>\n" +
           "\t\t</contrib>\n";

    private static XDocument ArticleWithContribs(params string[] contribs)
        => XDocument.Parse(
            "<article>\n" +
            "\t<contrib-group>\n" +
            string.Concat(contribs) +
            "\t</contrib-group>\n" +
            "</article>\n",
            LoadOptions.PreserveWhitespace);

    private static Report Apply(Phase3Context ctx)
    {
        var report = new Report();
        new ContribNamesInjector().Apply(ctx, report);
        return report;
    }

    [Fact]
    public void Contract_NameAndSeverity()
    {
        var injector = new ContribNamesInjector();

        Assert.Equal(RuleName, injector.Name);
        Assert.Equal(RuleSeverity.Optional, injector.Severity);
    }

    [Fact]
    public void Apply_EmptySurname_WarnsOnceMentioningEmpty()
    {
        // 5316 shape: Markup promoted the ORCID elsewhere and left the surname blank.
        var xml = ArticleWithContribs(Contrib(string.Empty, "Paulo Sérgio Alves"));

        var report = Apply(CreateContext(xml));

        var entry = Assert.Single(report.Entries);
        Assert.Equal(RuleName, entry.Rule);
        Assert.Equal(ReportLevel.Warn, entry.Level);
        Assert.Contains("#1", entry.Message, StringComparison.Ordinal);
        Assert.Contains("empty", entry.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Apply_SelfClosingEmptySurname_WarnsOnce()
    {
        var xml = XDocument.Parse(
            "<article><contrib-group><contrib><name><surname/><given-names>Ana</given-names></name></contrib></contrib-group></article>");

        var report = Apply(CreateContext(xml));

        var entry = Assert.Single(report.Entries);
        Assert.Equal(ReportLevel.Warn, entry.Level);
        Assert.Contains("empty", entry.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Apply_OrcidSurname_WarnsOnceMentioningOrcidAndValue()
    {
        // 5613 shape: the plain-text ORCID became the surname.
        var xml = ArticleWithContribs(Contrib("0009-0008-3948-7334", "Nguyen Hoai Nguyen"));

        var report = Apply(CreateContext(xml));

        var entry = Assert.Single(report.Entries);
        Assert.Equal(ReportLevel.Warn, entry.Level);
        Assert.Contains("ORCID", entry.Message, StringComparison.Ordinal);
        Assert.Contains("0009-0008-3948-7334", entry.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Apply_AffiliationDigitGluedToOrcid_WarnsOnce()
    {
        // 5501 shape: an affiliation label and an ORCID where the surname should be.
        var xml = ArticleWithContribs(Contrib("3 0000-0003-3513-3391", "Carlos Daniel"));

        var report = Apply(CreateContext(xml));

        var entry = Assert.Single(report.Entries);
        Assert.Equal(ReportLevel.Warn, entry.Level);
        Assert.Contains("ORCID", entry.Message, StringComparison.Ordinal);
        Assert.Contains("3 0000-0003-3513-3391", entry.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Apply_DigitsWithoutOrcid_WarnsMentioningDigitsNotOrcid()
    {
        var xml = ArticleWithContribs(Contrib("Silva2", "João"));

        var report = Apply(CreateContext(xml));

        var entry = Assert.Single(report.Entries);
        Assert.Equal(ReportLevel.Warn, entry.Level);
        Assert.Contains("digits", entry.Message, StringComparison.Ordinal);
        Assert.Contains("Silva2", entry.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("ORCID", entry.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Apply_WellFormedSurnames_EmitsNothing()
    {
        // Hyphenated and plain surnames are both clean; no INFO noise either.
        var xml = ArticleWithContribs(
            Contrib("Bruzi", "Adriano Teodoro"),
            Contrib("Barboza-Barquero", "Luis"));

        var report = Apply(CreateContext(xml));

        Assert.Empty(report.Entries);
        Assert.Equal(ReportLevel.Info, report.HighestLevel);
    }

    [Fact]
    public void Apply_ContribWithoutSurnameElement_IsSkipped()
    {
        // A <collab> contributor has no <name>/<surname>; that is not a broken byline.
        var xml = XDocument.Parse(
            "<article><contrib-group><contrib contrib-type=\"author\"><collab>Consortium</collab></contrib></contrib-group></article>");

        var report = Apply(CreateContext(xml));

        Assert.Empty(report.Entries);
    }

    [Fact]
    public void Apply_SevenBrokenAndOneClean_EmitsSevenWarnsWithOneBasedIndexes()
    {
        // 5316: seven of eight contributors broken. The clean one sits at position 4
        // so the 1-based numbering is visibly not a running count of defects.
        var xml = ArticleWithContribs(
            Contrib(string.Empty, "Paulo Sérgio Alves"),
            Contrib("0000-0001-0000-0001", "Marcos Antônio Ferreira"),
            Contrib("0000-0001-0000-0002", "José Airton Nunes"),
            Contrib("Bruzi", "Adriano Teodoro"),
            Contrib(string.Empty, "Ana Tereza Bastos"),
            Contrib("2 0000-0001-0000-0003", "Renato Rodrigues Pereira"),
            Contrib("Souza3", "Antônio Gomes"),
            Contrib(string.Empty, "João Silva Paiva"));

        var report = Apply(CreateContext(xml));

        Assert.Equal(7, report.Entries.Count);
        Assert.All(report.Entries, e =>
        {
            Assert.Equal(RuleName, e.Rule);
            Assert.Equal(ReportLevel.Warn, e.Level);
        });
        Assert.Equal(
            new[] { "#1", "#2", "#3", "#5", "#6", "#7", "#8" },
            report.Entries.Select(e => e.Message.Split(' ')[1]).ToArray());
    }

    [Fact]
    public void Apply_NeverMutatesXmlOrCreditOutcome()
    {
        var xml = ArticleWithContribs(
            Contrib(string.Empty, "Paulo Sérgio Alves"),
            Contrib("0009-0008-3948-7334", "Nguyen Hoai Nguyen"),
            Contrib("Bruzi", "Adriano Teodoro"));
        var before = xml.ToString(SaveOptions.DisableFormatting);
        var ctx = CreateContext(xml);

        var report = Apply(ctx);

        Assert.Equal(2, report.Entries.Count);
        Assert.Equal(before, xml.ToString(SaveOptions.DisableFormatting));
        Assert.Null(ctx.Credit);
    }

    [Fact]
    public void Apply_NullArguments_Throw()
    {
        var injector = new ContribNamesInjector();
        var ctx = CreateContext(ArticleWithContribs(Contrib("Bruzi", "Adriano")));

        Assert.Throws<ArgumentNullException>(() => injector.Apply(null!, new Report()));
        Assert.Throws<ArgumentNullException>(() => injector.Apply(ctx, null!));
    }
}
