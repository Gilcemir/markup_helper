using System.Text;
using System.Xml.Linq;
using DocFormatter.Core.Jats;
using Xunit;

namespace DocFormatter.Tests.Jats;

public sealed class JatsXmlWriterTests
{
    private static readonly XNamespace Xlink = "http://www.w3.org/1999/xlink";

    // A miniature JATS document mirroring the corpus shape: CRLF endings,
    // tab indentation, an XML declaration, a DOCTYPE, the xlink/mml namespaces
    // on the root, an empty element written as "<x/>", and the sps version.
    private const string SampleXml =
        "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
        "<!DOCTYPE article\r\n  PUBLIC \"-//NLM//DTD JATS\" \"JATS.dtd\">\r\n" +
        "<article specific-use=\"sps-1.9\" xmlns:mml=\"http://www.w3.org/1998/Math/MathML\" xmlns:xlink=\"http://www.w3.org/1999/xlink\">\r\n" +
        "\t<front>\r\n" +
        "\t\t<article-id pub-id-type=\"doi\">10.1590/x</article-id>\r\n" +
        "\t\t<graphic xlink:href=\"gf1.jpg\"/>\r\n" +
        "\t</front>\r\n" +
        "</article>\r\n";

    private static string WriteTemp(string content, Encoding encoding)
    {
        var path = Path.Combine(Path.GetTempPath(), $"jats-{Guid.NewGuid():N}.xml");
        File.WriteAllText(path, content, encoding);
        return path;
    }

    [Fact]
    public void LoadThenSave_NoMutation_IsByteIdentical()
    {
        var source = WriteTemp(SampleXml, new UTF8Encoding(false));
        var output = Path.Combine(Path.GetTempPath(), $"jats-out-{Guid.NewGuid():N}.xml");
        try
        {
            var doc = JatsXmlWriter.Load(source);
            doc.Save(output);

            Assert.Equal(File.ReadAllBytes(source), File.ReadAllBytes(output));
        }
        finally
        {
            File.Delete(source);
            File.Delete(output);
        }
    }

    [Fact]
    public void RoundTrip_PreservesDeclarationAndDoctype()
    {
        var source = WriteTemp(SampleXml, new UTF8Encoding(false));
        try
        {
            var serialized = JatsXmlWriter.Load(source).Serialize();

            Assert.StartsWith("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n", serialized, StringComparison.Ordinal);
            Assert.Contains("<!DOCTYPE article\r\n  PUBLIC \"-//NLM//DTD JATS\" \"JATS.dtd\">", serialized, StringComparison.Ordinal);
        }
        finally
        {
            File.Delete(source);
        }
    }

    [Fact]
    public void RoundTrip_PreservesByteOrderMark()
    {
        var source = WriteTemp(SampleXml, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
        var output = Path.Combine(Path.GetTempPath(), $"jats-bom-{Guid.NewGuid():N}.xml");
        try
        {
            JatsXmlWriter.Load(source).Save(output);
            Assert.Equal(File.ReadAllBytes(source), File.ReadAllBytes(output));
        }
        finally
        {
            File.Delete(source);
            File.Delete(output);
        }
    }

    [Fact]
    public void RoundTrip_DoesNotAlterSpecificUseVersion()
    {
        var source = WriteTemp(SampleXml, new UTF8Encoding(false));
        try
        {
            Assert.Contains("specific-use=\"sps-1.9\"", JatsXmlWriter.Load(source).Serialize(), StringComparison.Ordinal);
        }
        finally
        {
            File.Delete(source);
        }
    }

    [Fact]
    public void Load_LineFeedOnlyDocument_SavesWithLineFeeds()
    {
        var lfXml = SampleXml.Replace("\r\n", "\n", StringComparison.Ordinal);
        var source = WriteTemp(lfXml, new UTF8Encoding(false));
        var output = Path.Combine(Path.GetTempPath(), $"jats-lf-{Guid.NewGuid():N}.xml");
        try
        {
            var doc = JatsXmlWriter.Load(source);
            Assert.Equal("\n", doc.NewLine);
            doc.Save(output);
            Assert.Equal(File.ReadAllBytes(source), File.ReadAllBytes(output));
        }
        finally
        {
            File.Delete(source);
            File.Delete(output);
        }
    }

    [Fact]
    public void Load_SelfClosingRoot_RoundTrips()
    {
        const string selfClosing = "<?xml version=\"1.0\"?>\n<article specific-use=\"sps-1.9\"/>\n";
        var source = WriteTemp(selfClosing, new UTF8Encoding(false));
        var output = Path.Combine(Path.GetTempPath(), $"jats-sc-{Guid.NewGuid():N}.xml");
        try
        {
            JatsXmlWriter.Load(source).Save(output);
            Assert.Equal(File.ReadAllBytes(source), File.ReadAllBytes(output));
        }
        finally
        {
            File.Delete(source);
            File.Delete(output);
        }
    }

    [Fact]
    public void Indent_BuildsNewlinePlusTabs()
    {
        Assert.Equal("\n\t\t\t", JatsXmlWriter.Indent(3).Value);
        Assert.Equal("\n", JatsXmlWriter.Indent(0).Value);
        Assert.Equal("\n    ", JatsXmlWriter.Indent(2, "  ").Value);
    }

    [Fact]
    public void Indent_NegativeDepth_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => JatsXmlWriter.Indent(-1));
    }

    [Fact]
    public void BuildLeaf_WritesValueAndAttributes()
    {
        var element = JatsXmlWriter.BuildLeaf(
            "article-id",
            "00123",
            new[] { new XAttribute("pub-id-type", "other") });

        Assert.Equal("<article-id pub-id-type=\"other\">00123</article-id>", element.ToString(SaveOptions.DisableFormatting));
    }

    [Fact]
    public void BuildLeaf_NullValue_ProducesEmptyElement()
    {
        var element = JatsXmlWriter.BuildLeaf("col", null);
        Assert.Empty(element.Nodes());
    }

    [Fact]
    public void BuildElement_IndentsChildrenAndClosingTag()
    {
        var sec = JatsXmlWriter.BuildElement(
            "sec",
            depth: 2,
            attributes: new[] { new XAttribute("sec-type", "data-availability") },
            children: new[]
            {
                JatsXmlWriter.BuildLeaf("title", "Data availability"),
                JatsXmlWriter.BuildLeaf("p", "Available on request."),
            });

        var expected =
            "<sec sec-type=\"data-availability\">" +
            "\n\t\t\t<title>Data availability</title>" +
            "\n\t\t\t<p>Available on request.</p>" +
            "\n\t\t</sec>";
        Assert.Equal(expected, sec.ToString(SaveOptions.DisableFormatting));
    }

    [Fact]
    public void BuildElement_NoChildren_HasNoInternalWhitespace()
    {
        var element = JatsXmlWriter.BuildElement("empty", depth: 1, attributes: null, children: Array.Empty<XElement>());
        Assert.Empty(element.Nodes());
    }

    [Fact]
    public void InsertAfter_AddsIndentedSiblingPreservingFollowingNode()
    {
        var parent = new XElement(
            "front",
            new XText("\n\t\t"),
            new XElement("article-id", new XAttribute("pub-id-type", "doi"), "10.1590/x"),
            new XText("\n\t\t"),
            new XElement("title-group"),
            new XText("\n\t"));
        var doi = parent.Elements().First();

        var injected = JatsXmlWriter.BuildLeaf("article-id", "00123", new[] { new XAttribute("pub-id-type", "other") });
        JatsXmlWriter.InsertAfter(doi, injected, depth: 2);

        var expected =
            "<front>" +
            "\n\t\t<article-id pub-id-type=\"doi\">10.1590/x</article-id>" +
            "\n\t\t<article-id pub-id-type=\"other\">00123</article-id>" +
            // XElement.ToString renders the empty element with a space before
            // "/>"; the corpus' "<x/>" form is restored only by JatsDocument.Save.
            "\n\t\t<title-group />" +
            "\n\t</front>";
        Assert.Equal(expected, parent.ToString(SaveOptions.DisableFormatting));
    }

    [Fact]
    public void InjectedXlinkElement_ReusesRootNamespace_NoRedundantXmlns()
    {
        var source = WriteTemp(SampleXml, new UTF8Encoding(false));
        try
        {
            var doc = JatsXmlWriter.Load(source);
            var front = doc.Document.Root!.Element("front")!;
            var anchor = front.Elements().Last();

            var injected = JatsXmlWriter.BuildLeaf("graphic", null, new[] { new XAttribute(Xlink + "href", "INJECTED.jpg") });
            JatsXmlWriter.InsertAfter(anchor, injected, depth: 2);

            var serialized = doc.Serialize();

            Assert.Contains("<graphic xlink:href=\"INJECTED.jpg\"/>", serialized, StringComparison.Ordinal);
            // Only the root's single declaration must remain; the injected node
            // inherits the prefix rather than redeclaring it.
            Assert.Equal(1, CountOccurrences(serialized, "xmlns:xlink"));
        }
        finally
        {
            File.Delete(source);
        }
    }

    [Fact]
    public void Save_NullPath_Throws()
    {
        var source = WriteTemp(SampleXml, new UTF8Encoding(false));
        try
        {
            Assert.Throws<ArgumentNullException>(() => JatsXmlWriter.Load(source).Save(null!));
        }
        finally
        {
            File.Delete(source);
        }
    }

    [Fact]
    public void Load_NullPath_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => JatsXmlWriter.Load(null!));
    }

    private static int CountOccurrences(string haystack, string needle)
    {
        var count = 0;
        var index = 0;
        while ((index = haystack.IndexOf(needle, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += needle.Length;
        }

        return count;
    }
}
