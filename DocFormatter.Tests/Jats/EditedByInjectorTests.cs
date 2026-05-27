using System.Xml.Linq;
using DocFormatter.Core.Jats;
using DocFormatter.Core.Pipeline;
using Xunit;

namespace DocFormatter.Tests.Jats;

public sealed class EditedByInjectorTests
{
    private sealed class ThrowingConfirmer : IConfirmer
    {
        // edited-by is deterministic name+role: it must never call the gate.
        public ConfirmResult Confirm(Proposal proposal)
            => throw new InvalidOperationException("EditedByInjector must not prompt.");
    }

    private static Phase3Context CreateContext(XDocument xml, string? scientific = null, string? associate = null)
        => new()
        {
            Source = new DocxSource
            {
                ElocationId = "e54492621",
                Doi = "10.1590/x",
                ScientificEditor = scientific,
                AssociateEditor = associate,
            },
            Xml = xml,
            OtherNumber = "00201",
            Confirm = new ThrowingConfirmer(),
        };

    private static XDocument ArticleWith(string articleMetaTail)
    {
        var xml =
            "<article>\n" +
            "\t<front>\n" +
            "\t\t<article-meta>\n" +
            "\t\t\t<contrib-group>\n" +
            "\t\t\t\t<contrib contrib-type=\"author\"/>\n" +
            "\t\t\t</contrib-group>\n" +
            "\t\t\t<aff id=\"aff1\">\n" +
            "\t\t\t\t<institution content-type=\"orgname\">Uni</institution>\n" +
            "\t\t\t</aff>\n" +
            articleMetaTail +
            "\t\t\t<pub-date date-type=\"pub\"/>\n" +
            "\t\t</article-meta>\n" +
            "\t</front>\n" +
            "</article>\n";
        return XDocument.Parse(xml, LoadOptions.PreserveWhitespace);
    }

    private static IReadOnlyList<XElement> EditedByFns(XDocument xml)
        => xml.Descendants()
            .Where(e => e.Name.LocalName == "fn" && (string?)e.Attribute("fn-type") == "edited-by")
            .ToList();

    [Fact]
    public void Apply_ScientificEditorNoOrcid_EmitsLabelAndParagraph_WithoutExtLink()
    {
        var xml = ArticleWith(
            "\t\t\t<author-notes>\n" +
            "\t\t\t\t<corresp id=\"c1\">* E-mail: <email>a@b.c</email>\n" +
            "\t\t\t\t</corresp>\n" +
            "\t\t\t</author-notes>\n");

        new EditedByInjector().Apply(CreateContext(xml, scientific: "Luiz Antônio dos Santos Dias"), new Report());

        var fn = Assert.Single(EditedByFns(xml));
        Assert.Equal("SCIENTIFIC EDITOR:", fn.Elements().First(e => e.Name.LocalName == "label").Value);
        Assert.Equal("Luiz Antônio dos Santos Dias", fn.Elements().First(e => e.Name.LocalName == "p").Value);
        Assert.DoesNotContain(fn.Descendants(), e => e.Name.LocalName == "ext-link");
        // The PRD forbids title/bold/italic for the role label.
        Assert.DoesNotContain(fn.Descendants(), e => e.Name.LocalName is "title" or "bold" or "italic");
    }

    [Fact]
    public void Apply_ExistingAuthorNotes_AppendsFn_AndPreservesCorresp()
    {
        var xml = ArticleWith(
            "\t\t\t<author-notes>\n" +
            "\t\t\t\t<corresp id=\"c1\">* E-mail: <email>a@b.c</email>\n" +
            "\t\t\t\t</corresp>\n" +
            "\t\t\t</author-notes>\n");

        new EditedByInjector().Apply(CreateContext(xml, scientific: "Jane Doe"), new Report());

        var authorNotes = xml.Descendants().Single(e => e.Name.LocalName == "author-notes");
        var children = authorNotes.Elements().ToList();
        Assert.Equal("corresp", children[0].Name.LocalName);
        Assert.Equal("c1", (string?)children[0].Attribute("id"));
        Assert.Equal("fn", children[1].Name.LocalName);
        Assert.Single(EditedByFns(xml));
    }

    [Fact]
    public void Apply_EmptyExistingAuthorNotes_SeedsFnAsChild()
    {
        var xml = ArticleWith(
            "\t\t\t<author-notes>\n" +
            "\t\t\t</author-notes>\n");

        new EditedByInjector().Apply(CreateContext(xml, scientific: "Jane Doe"), new Report());

        var authorNotes = xml.Descendants().Single(e => e.Name.LocalName == "author-notes");
        var fn = Assert.Single(authorNotes.Elements(), e => e.Name.LocalName == "fn");
        Assert.Equal("SCIENTIFIC EDITOR:", fn.Elements().First(e => e.Name.LocalName == "label").Value);
    }

    [Fact]
    public void Apply_NoAuthorNotes_CreatesOneAfterAff_WithFn()
    {
        var xml = ArticleWith(string.Empty);

        new EditedByInjector().Apply(CreateContext(xml, scientific: "Jane Doe"), new Report());

        var authorNotes = Assert.Single(xml.Descendants(), e => e.Name.LocalName == "author-notes");
        Assert.Single(authorNotes.Elements(), e => e.Name.LocalName == "fn");
        // Placed after the last <aff> and before <pub-date> (JATS-valid slot).
        var aff = xml.Descendants().Single(e => e.Name.LocalName == "aff");
        var nextElement = (XElement)aff.NodesAfterSelf().First(n => n is XElement);
        Assert.Equal("author-notes", nextElement.Name.LocalName);
    }

    [Fact]
    public void Apply_TwoEditorRoleLines_ProduceTwoEditedByFns_AssociateFirst()
    {
        var xml = ArticleWith(string.Empty);

        new EditedByInjector().Apply(
            CreateContext(xml, scientific: "Juraci Almeida Cesar", associate: "Luana Patricia Marmitt"),
            new Report());

        var fns = EditedByFns(xml);
        Assert.Equal(2, fns.Count);
        Assert.Equal("ASSOCIATE EDITOR:", fns[0].Elements().First(e => e.Name.LocalName == "label").Value);
        Assert.Equal("Luana Patricia Marmitt", fns[0].Elements().First(e => e.Name.LocalName == "p").Value);
        Assert.Equal("SCIENTIFIC EDITOR:", fns[1].Elements().First(e => e.Name.LocalName == "label").Value);
        Assert.Equal("Juraci Almeida Cesar", fns[1].Elements().First(e => e.Name.LocalName == "p").Value);
    }

    [Fact]
    public void Apply_EditedByFnAlreadyPresent_LeavesXmlUnchanged_AndReportsSkipped()
    {
        var xml = ArticleWith(
            "\t\t\t<author-notes>\n" +
            "\t\t\t\t<fn fn-type=\"edited-by\">\n" +
            "\t\t\t\t\t<label>SCIENTIFIC EDITOR:</label>\n" +
            "\t\t\t\t\t<p>Existing Name</p>\n" +
            "\t\t\t\t</fn>\n" +
            "\t\t\t</author-notes>\n");
        var before = xml.ToString(SaveOptions.DisableFormatting);
        var report = new Report();

        new EditedByInjector().Apply(CreateContext(xml, scientific: "New Name"), report);

        Assert.Equal(before, xml.ToString(SaveOptions.DisableFormatting));
        Assert.Single(EditedByFns(xml));
        var entry = Assert.Single(report.Entries);
        Assert.Equal(ReportLevel.Info, entry.Level);
        Assert.Contains("already present", entry.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Apply_NoEditorOnSource_LeavesXmlUnchanged_AndReportsSkipped()
    {
        var xml = ArticleWith(string.Empty);
        var before = xml.ToString(SaveOptions.DisableFormatting);
        var report = new Report();

        new EditedByInjector().Apply(CreateContext(xml), report);

        Assert.Equal(before, xml.ToString(SaveOptions.DisableFormatting));
        Assert.Empty(EditedByFns(xml));
        var entry = Assert.Single(report.Entries);
        Assert.Equal(ReportLevel.Info, entry.Level);
        Assert.Contains("No responsible editor", entry.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Apply_InheritsRootNamespace_NoRedundantXmlns()
    {
        XNamespace ns = "http://jats.nlm.nih.gov";
        var xml = XDocument.Parse(
            $"<article xmlns=\"{ns}\">\n" +
            "\t<front>\n" +
            "\t\t<article-meta>\n" +
            "\t\t\t<aff id=\"aff1\"/>\n" +
            "\t\t</article-meta>\n" +
            "\t</front>\n" +
            "</article>\n",
            LoadOptions.PreserveWhitespace);

        new EditedByInjector().Apply(CreateContext(xml, scientific: "Jane Doe"), new Report());

        var fn = Assert.Single(EditedByFns(xml));
        Assert.Equal(ns, fn.Name.Namespace);
        // Namespace is inherited from the root: no injected element re-declares it.
        var authorNotes = xml.Descendants().Single(e => e.Name.LocalName == "author-notes");
        Assert.DoesNotContain(
            authorNotes.DescendantsAndSelf().Attributes(),
            a => a.IsNamespaceDeclaration);
    }

    [Fact]
    public void Apply_OverCorpusXml_AppendsScientificEditorAfterExistingCorresp()
    {
        var corpus = CorpusPath("1984-7033-cbab-26-02-e54492621.xml");
        var doc = JatsXmlWriter.Load(corpus);

        new EditedByInjector().Apply(
            CreateContext(doc.Document, scientific: "Luiz Antônio dos Santos Dias"),
            new Report());

        var authorNotes = Assert.Single(doc.Document.Descendants(), e => e.Name.LocalName == "author-notes");
        var children = authorNotes.Elements().ToList();
        Assert.Equal("corresp", children[0].Name.LocalName);
        Assert.Equal("fn", children[1].Name.LocalName);

        var serialized = doc.Serialize();
        var newLine = doc.NewLine;
        var expected =
            $"\t\t\t\t</corresp>{newLine}" +
            $"\t\t\t\t<fn fn-type=\"edited-by\">{newLine}" +
            $"\t\t\t\t\t<label>SCIENTIFIC EDITOR:</label>{newLine}" +
            $"\t\t\t\t\t<p>Luiz Antônio dos Santos Dias</p>{newLine}" +
            $"\t\t\t\t</fn>{newLine}" +
            "\t\t\t</author-notes>";
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
