using System.Xml.Linq;
using DocFormatter.Core.Jats;
using DocFormatter.Core.Pipeline;
using Xunit;

namespace DocFormatter.Tests.Jats;

public sealed class OtherIdInjectorTests
{
    private sealed class ThrowingConfirmer : IConfirmer
    {
        // The 'other' id is fully deterministic: it must never call the gate.
        public ConfirmResult Confirm(Proposal proposal)
            => throw new InvalidOperationException("OtherIdInjector must not prompt.");
    }

    private static Phase3Context CreateContext(XDocument xml, string? otherNumber)
        => new()
        {
            Source = new DocxSource { ElocationId = "e54492621", Doi = "10.1590/x" },
            Xml = xml,
            OtherNumber = otherNumber,
            Confirm = new ThrowingConfirmer(),
        };

    private static XDocument ArticleWith(params string[] articleIdLines)
    {
        var ids = string.Join("\n\t\t\t", articleIdLines);
        var xml =
            "<article>\n" +
            "\t<front>\n" +
            "\t\t<article-meta>\n" +
            $"\t\t\t{ids}\n" +
            "\t\t\t<article-categories/>\n" +
            "\t\t</article-meta>\n" +
            "\t</front>\n" +
            "</article>\n";
        return XDocument.Parse(xml, LoadOptions.PreserveWhitespace);
    }

    private static XElement? OtherElement(XDocument xml)
        => xml.Descendants()
            .FirstOrDefault(e =>
                e.Name.LocalName == "article-id"
                && (string?)e.Attribute("pub-id-type") == "other");

    [Fact]
    public void Apply_DoiPresentWithOtherNumber_InsertsOtherImmediatelyAfterDoi()
    {
        var xml = ArticleWith("<article-id pub-id-type=\"doi\">10.1590/1984-70332026v26n2a16</article-id>");
        var injector = new OtherIdInjector();
        var report = new Report();

        injector.Apply(CreateContext(xml, "00201"), report);

        var doi = xml.Descendants().First(e => (string?)e.Attribute("pub-id-type") == "doi");
        // InsertAfter places an indentation text node, then the injected element,
        // so the 'other' id is the first element following the DOI.
        var injected = (XElement)doi.NodesAfterSelf().First(n => n is XElement);
        Assert.Equal("article-id", injected.Name.LocalName);
        Assert.Equal("other", (string?)injected.Attribute("pub-id-type"));
        Assert.Equal("00201", injected.Value);

        var entry = Assert.Single(report.Entries);
        Assert.Equal(ReportLevel.Info, entry.Level);
        Assert.Contains("00201", entry.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Apply_NoDoiArticleId_Throws_AndInsertsNothing()
    {
        var xml = ArticleWith("<article-id pub-id-type=\"publisher-id\">cbab</article-id>");
        var injector = new OtherIdInjector();
        var report = new Report();

        var ex = Assert.Throws<InvalidOperationException>(() => injector.Apply(CreateContext(xml, "00201"), report));

        Assert.Contains("doi", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Null(OtherElement(xml));
    }

    [Fact]
    public void Apply_OtherAlreadyPresent_LeavesXmlUnchanged_AndReportsSkipped()
    {
        var xml = ArticleWith(
            "<article-id pub-id-type=\"doi\">10.1590/x</article-id>",
            "<article-id pub-id-type=\"other\">99999</article-id>");
        var before = xml.ToString(SaveOptions.DisableFormatting);
        var injector = new OtherIdInjector();
        var report = new Report();

        injector.Apply(CreateContext(xml, "00201"), report);

        Assert.Equal(before, xml.ToString(SaveOptions.DisableFormatting));
        Assert.Single(xml.Descendants(), e => (string?)e.Attribute("pub-id-type") == "other");
        var entry = Assert.Single(report.Entries);
        Assert.Equal(ReportLevel.Info, entry.Level);
        Assert.Contains("already present", entry.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Apply_MissingOtherNumber_Throws_WithReportedReason_AndInsertsNothing(string? otherNumber)
    {
        var xml = ArticleWith("<article-id pub-id-type=\"doi\">10.1590/x</article-id>");
        var injector = new OtherIdInjector();
        var report = new Report();

        var ex = Assert.Throws<InvalidOperationException>(() => injector.Apply(CreateContext(xml, otherNumber), report));

        Assert.Contains("OtherNumber", ex.Message, StringComparison.Ordinal);
        Assert.Null(OtherElement(xml));
    }

    [Fact]
    public void Apply_InsertedElement_InheritsRootNamespace_AndSiblingIndentation()
    {
        // A default namespace on the root: the injected element must adopt it
        // (so no redundant xmlns is emitted) and align to the DOI's tab depth.
        XNamespace ns = "http://jats.nlm.nih.gov";
        var xml = XDocument.Parse(
            $"<article xmlns=\"{ns}\">\n" +
            "\t<front>\n" +
            "\t\t<article-meta>\n" +
            "\t\t\t<article-id pub-id-type=\"doi\">10.1590/x</article-id>\n" +
            "\t\t\t<article-categories/>\n" +
            "\t\t</article-meta>\n" +
            "\t</front>\n" +
            "</article>\n",
            LoadOptions.PreserveWhitespace);
        var injector = new OtherIdInjector();

        injector.Apply(CreateContext(xml, "00201"), new Report());

        var injected = xml.Descendants().First(e => (string?)e.Attribute("pub-id-type") == "other");
        Assert.Equal(ns, injected.Name.Namespace);

        // The whitespace node preceding the injected sibling matches the DOI's
        // depth (3 tabs), so the diff is the injected line only.
        Assert.Equal("\n\t\t\t", ((XText)injected.PreviousNode!).Value);
    }

    [Fact]
    public void Apply_OverCorpusXml_PlacesOtherDirectlyAfterDoiLine()
    {
        var corpus = CorpusPath("1984-7033-cbab-26-02-e54492621.xml");
        var doc = JatsXmlWriter.Load(corpus);
        var injector = new OtherIdInjector();
        var report = new Report();

        injector.Apply(CreateContext(doc.Document, "00201"), report);

        var serialized = doc.Serialize();
        var newLine = doc.NewLine;
        var expected =
            $"\t\t\t<article-id pub-id-type=\"doi\">10.1590/1984-70332026v26n2a16</article-id>{newLine}" +
            "\t\t\t<article-id pub-id-type=\"other\">00201</article-id>";
        Assert.Contains(expected, serialized, StringComparison.Ordinal);
    }

    private static string CorpusPath(string file)
    {
        var dir = AppContext.BaseDirectory;
        while (dir != null)
        {
            var candidate = Path.Combine(dir, "examples", "phase-3", "scielo_package", file);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            dir = Path.GetDirectoryName(dir);
        }

        throw new InvalidOperationException(
            $"Could not locate examples/phase-3/scielo_package/{file} from {AppContext.BaseDirectory}.");
    }
}
