using System.Xml.Linq;
using DocFormatter.Core.Jats;
using DocFormatter.Core.Pipeline;
using Xunit;

namespace DocFormatter.Tests.Jats;

public sealed class DataAvailabilityInjectorTests
{
    private sealed class ThrowingConfirmer : IConfirmer
    {
        // A confident classification must never reach the gate.
        public ConfirmResult Confirm(Proposal proposal)
            => throw new InvalidOperationException("DataAvailabilityInjector must not prompt for confident text.");
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

    private static Phase3Context CreateContext(XDocument xml, string? dataAvailability, IConfirmer confirmer)
        => new()
        {
            Source = new DocxSource
            {
                ElocationId = "e54492621",
                Doi = "10.1590/x",
                DataAvailabilityText = dataAvailability,
            },
            Xml = xml,
            OtherNumber = "00201",
            Confirm = confirmer,
        };

    private static XDocument ArticleWithBack(string backInner)
    {
        var xml =
            "<article>\n" +
            "\t<body>\n" +
            "\t\t<sec>\n" +
            "\t\t\t<title>Intro</title>\n" +
            "\t\t</sec>\n" +
            "\t</body>\n" +
            "\t<back>\n" +
            backInner +
            "\t</back>\n" +
            "</article>\n";
        return XDocument.Parse(xml, LoadOptions.PreserveWhitespace);
    }

    private static readonly string AckThenRefs =
        "\t\t<ack>\n" +
        "\t\t\t<title>ACKNOWLEDGMENTS</title>\n" +
        "\t\t\t<p>We thank the lab.</p>\n" +
        "\t\t</ack>\n" +
        "\t\t<ref-list>\n" +
        "\t\t\t<title>REFERENCES</title>\n" +
        "\t\t</ref-list>\n";

    private static IReadOnlyList<XElement> DataAvailabilitySecs(XDocument xml)
        => xml.Descendants()
            .Where(e => e.Name.LocalName == "sec" && (string?)e.Attribute("sec-type") == "data-availability")
            .ToList();

    private static string SpecificUseOf(XElement sec) => (string)sec.Attribute("specific-use")!;

    [Fact]
    public void Apply_UponRequestText_AutoClassifies_DataAvailableUponRequest()
    {
        var xml = ArticleWithBack(AckThenRefs);

        new DataAvailabilityInjector().Apply(
            CreateContext(
                xml,
                "The datasets are available from the corresponding author upon reasonable request.",
                new ThrowingConfirmer()),
            new Report());

        var sec = Assert.Single(DataAvailabilitySecs(xml));
        Assert.Equal("data-available-upon-request", SpecificUseOf(sec));
    }

    [Fact]
    public void Apply_RepositoryLinkText_AutoClassifies_DataAvailable()
    {
        var xml = ArticleWithBack(AckThenRefs);

        new DataAvailabilityInjector().Apply(
            CreateContext(
                xml,
                "The data are openly available in the Zenodo repository at https://doi.org/10.5281/zenodo.123.",
                new ThrowingConfirmer()),
            new Report());

        var sec = Assert.Single(DataAvailabilitySecs(xml));
        Assert.Equal("data-available", SpecificUseOf(sec));
    }

    [Fact]
    public void Apply_NoNewDataText_AutoClassifies_Uninformed()
    {
        var xml = ArticleWithBack(AckThenRefs);

        new DataAvailabilityInjector().Apply(
            CreateContext(xml, "No new data were created or analyzed in this study.", new ThrowingConfirmer()),
            new Report());

        var sec = Assert.Single(DataAvailabilitySecs(xml));
        Assert.Equal("uninformed", SpecificUseOf(sec));
    }

    [Fact]
    public void Apply_AmbiguousText_ProposesAndUsesConfirmerResult()
    {
        var xml = ArticleWithBack(AckThenRefs);
        var confirmer = new StubConfirmer(new ConfirmResult("data-in-article", ConfirmDisposition.Overridden));

        new DataAvailabilityInjector().Apply(
            CreateContext(xml, "Os procedimentos seguiram as diretrizes institucionais.", confirmer),
            new Report());

        // The gate was consulted with a data-availability proposal...
        Assert.NotNull(confirmer.Received);
        Assert.Equal("data-availability", confirmer.Received!.Tag);
        // ...and the operator's override is what gets written.
        var sec = Assert.Single(DataAvailabilitySecs(xml));
        Assert.Equal("data-in-article", SpecificUseOf(sec));
    }

    [Fact]
    public void Apply_ConfirmerSkips_DoesNotInject_AndReportsWarn()
    {
        var xml = ArticleWithBack(AckThenRefs);
        var before = xml.ToString(SaveOptions.DisableFormatting);
        var confirmer = new StubConfirmer(new ConfirmResult(string.Empty, ConfirmDisposition.Skipped));
        var report = new Report();

        new DataAvailabilityInjector().Apply(
            CreateContext(xml, "Os procedimentos seguiram as diretrizes institucionais.", confirmer),
            report);

        Assert.Equal(before, xml.ToString(SaveOptions.DisableFormatting));
        Assert.Empty(DataAvailabilitySecs(xml));
        var entry = Assert.Single(report.Entries);
        Assert.Equal(ReportLevel.Warn, entry.Level);
    }

    [Fact]
    public void Apply_PlacesSectionImmediatelyAfterAck_WithTitleAndStatementParagraph()
    {
        var xml = ArticleWithBack(AckThenRefs);
        const string statement = "The datasets are available from the corresponding author upon request.";

        new DataAvailabilityInjector().Apply(
            CreateContext(xml, statement, new ThrowingConfirmer()),
            new Report());

        var ack = xml.Descendants().Single(e => e.Name.LocalName == "ack");
        var nextElement = (XElement)ack.NodesAfterSelf().First(n => n is XElement);
        Assert.Equal("sec", nextElement.Name.LocalName);
        Assert.Equal("data-availability", (string?)nextElement.Attribute("sec-type"));

        var children = nextElement.Elements().ToList();
        Assert.Equal("title", children[0].Name.LocalName);
        Assert.Equal("Data Availability Statement", children[0].Value);
        Assert.Equal("p", children[1].Name.LocalName);
        Assert.Equal(statement, children[1].Value);
    }

    [Fact]
    public void Apply_NoAck_PlacesSectionBeforeFirstChildOfBack()
    {
        var xml = ArticleWithBack(
            "\t\t<ref-list>\n" +
            "\t\t\t<title>REFERENCES</title>\n" +
            "\t\t</ref-list>\n");

        new DataAvailabilityInjector().Apply(
            CreateContext(xml, "Data available upon request from the corresponding author.", new ThrowingConfirmer()),
            new Report());

        var back = xml.Descendants().Single(e => e.Name.LocalName == "back");
        var firstChild = back.Elements().First();
        Assert.Equal("sec", firstChild.Name.LocalName);
        Assert.Equal("data-availability", (string?)firstChild.Attribute("sec-type"));
    }

    [Fact]
    public void Apply_EmptyBack_PlacesSectionAsOnlyChild()
    {
        var xml = ArticleWithBack(string.Empty);

        new DataAvailabilityInjector().Apply(
            CreateContext(xml, "Data available upon request from the corresponding author.", new ThrowingConfirmer()),
            new Report());

        var back = xml.Descendants().Single(e => e.Name.LocalName == "back");
        var sec = Assert.Single(back.Elements());
        Assert.Equal("data-availability", (string?)sec.Attribute("sec-type"));
    }

    [Fact]
    public void Apply_NoDataAvailabilityText_LeavesXmlUnchanged_AndReportsSkipped()
    {
        var xml = ArticleWithBack(AckThenRefs);
        var before = xml.ToString(SaveOptions.DisableFormatting);
        var report = new Report();

        new DataAvailabilityInjector().Apply(CreateContext(xml, null, new ThrowingConfirmer()), report);

        Assert.Equal(before, xml.ToString(SaveOptions.DisableFormatting));
        Assert.Empty(DataAvailabilitySecs(xml));
        var entry = Assert.Single(report.Entries);
        Assert.Equal(ReportLevel.Info, entry.Level);
        Assert.Contains("No data-availability statement", entry.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Apply_NoBack_LeavesXmlUnchanged_AndReportsSkipped()
    {
        var xml = XDocument.Parse(
            "<article>\n\t<body>\n\t\t<p>x</p>\n\t</body>\n</article>\n",
            LoadOptions.PreserveWhitespace);
        var before = xml.ToString(SaveOptions.DisableFormatting);
        var report = new Report();

        new DataAvailabilityInjector().Apply(
            CreateContext(xml, "Data available upon request.", new ThrowingConfirmer()),
            report);

        Assert.Equal(before, xml.ToString(SaveOptions.DisableFormatting));
        Assert.Empty(DataAvailabilitySecs(xml));
        var entry = Assert.Single(report.Entries);
        Assert.Contains("No <back>", entry.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Apply_ExistingDataAvailabilitySec_LeavesXmlUnchanged_AndReportsSkipped()
    {
        var xml = ArticleWithBack(
            "\t\t<ack>\n" +
            "\t\t\t<p>thanks</p>\n" +
            "\t\t</ack>\n" +
            "\t\t<sec sec-type=\"data-availability\" specific-use=\"data-available\">\n" +
            "\t\t\t<title>Data Availability Statement</title>\n" +
            "\t\t\t<p>Existing.</p>\n" +
            "\t\t</sec>\n");
        var before = xml.ToString(SaveOptions.DisableFormatting);
        var report = new Report();

        new DataAvailabilityInjector().Apply(
            CreateContext(xml, "No new data were created.", new ThrowingConfirmer()),
            report);

        Assert.Equal(before, xml.ToString(SaveOptions.DisableFormatting));
        Assert.Single(DataAvailabilitySecs(xml));
        var entry = Assert.Single(report.Entries);
        Assert.Equal(ReportLevel.Info, entry.Level);
        Assert.Contains("already present", entry.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Apply_ExistingDataAvailabilityFn_LeavesXmlUnchanged_AndReportsSkipped()
    {
        var xml = ArticleWithBack(
            "\t\t<fn-group>\n" +
            "\t\t\t<fn fn-type=\"data-availability\" specific-use=\"uninformed\">\n" +
            "\t\t\t\t<label>Data Availability Statement</label>\n" +
            "\t\t\t\t<p>Existing.</p>\n" +
            "\t\t\t</fn>\n" +
            "\t\t</fn-group>\n");
        var before = xml.ToString(SaveOptions.DisableFormatting);
        var report = new Report();

        new DataAvailabilityInjector().Apply(
            CreateContext(xml, "No new data were created.", new ThrowingConfirmer()),
            report);

        Assert.Equal(before, xml.ToString(SaveOptions.DisableFormatting));
        Assert.Empty(DataAvailabilitySecs(xml));
        var entry = Assert.Single(report.Entries);
        Assert.Contains("already present", entry.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Apply_InheritsRootNamespace_NoRedundantXmlns()
    {
        XNamespace ns = "http://jats.nlm.nih.gov";
        var xml = XDocument.Parse(
            $"<article xmlns=\"{ns}\">\n" +
            "\t<back>\n" +
            "\t\t<ack>\n" +
            "\t\t\t<p>thanks</p>\n" +
            "\t\t</ack>\n" +
            "\t</back>\n" +
            "</article>\n",
            LoadOptions.PreserveWhitespace);

        new DataAvailabilityInjector().Apply(
            CreateContext(xml, "Data available upon request from the corresponding author.", new ThrowingConfirmer()),
            new Report());

        var sec = Assert.Single(DataAvailabilitySecs(xml));
        Assert.Equal(ns, sec.Name.Namespace);
        Assert.DoesNotContain(sec.DescendantsAndSelf().Attributes(), a => a.IsNamespaceDeclaration);
    }

    [Fact]
    public void Apply_OverCorpusXml_InjectsSectionAfterAck_WithClassifiedSpecificUse()
    {
        var corpus = CorpusPackagePath("1984-7033-cbab-26-02-e54492621.xml");
        var doc = JatsXmlWriter.Load(corpus);
        // Pair to its docx source (5449) so the real data-availability text drives
        // the classifier, matching the end-to-end CLI path (ADR-002/ADR-004).
        var source = new DocxSourceReader().Read(CorpusMarkupPath("5449.docx"));
        var ctx = new Phase3Context
        {
            Source = source,
            Xml = doc.Document,
            OtherNumber = "00201",
            Confirm = new AutoAcceptConfirmer(),
        };

        new DataAvailabilityInjector().Apply(ctx, new Report());

        var ack = doc.Document.Descendants().Single(e => e.Name.LocalName == "ack");
        var nextElement = (XElement)ack.NodesAfterSelf().First(n => n is XElement);
        Assert.Equal("sec", nextElement.Name.LocalName);
        Assert.Equal("data-availability", (string?)nextElement.Attribute("sec-type"));
        // 5449's statement ("…corresponding author upon request") is confidently
        // classified, so the section carries the upon-request category.
        Assert.Equal("data-available-upon-request", SpecificUseOf(nextElement));

        // The injected nodes serialize with the surrounding indentation preserved.
        var newLine = doc.NewLine;
        var serialized = doc.Serialize();
        Assert.Contains($"\t\t</ack>{newLine}\t\t<sec sec-type=\"data-availability\"", serialized, StringComparison.Ordinal);
        Assert.Contains($"<title>Data Availability Statement</title>", serialized, StringComparison.Ordinal);
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
