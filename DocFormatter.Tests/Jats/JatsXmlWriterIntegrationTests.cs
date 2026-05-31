using System.Xml.Linq;
using DocFormatter.Core.Jats;
using Xunit;

namespace DocFormatter.Tests.Jats;

/// <summary>
/// Exercises <see cref="JatsXmlWriter"/> against a real SciELO package: a no-op
/// load/save is byte-identical, and injecting one element changes only the
/// injected line (TechSpec "Whitespace-preserving XML write").
/// </summary>
public sealed class JatsXmlWriterIntegrationTests
{
    private const string CorpusFile = "1984-7033-cbab-26-02-e54342625.xml";

    [Fact]
    public void NoOpRoundTrip_OnCorpusFile_IsByteIdentical()
    {
        var source = CorpusPath(CorpusFile);
        var output = Path.Combine(Path.GetTempPath(), $"jats-corpus-{Guid.NewGuid():N}.xml");
        try
        {
            JatsXmlWriter.Load(source).Save(output);
            Assert.Equal(File.ReadAllBytes(source), File.ReadAllBytes(output));
        }
        finally
        {
            File.Delete(output);
        }
    }

    [Fact]
    public void InjectingOneElement_ChangesOnlyTheInjectedLine()
    {
        var source = CorpusPath(CorpusFile);
        var original = File.ReadAllText(source);

        var doc = JatsXmlWriter.Load(source);
        var doi = doc.Document.Descendants()
            .First(e => e.Name.LocalName == "article-id" && (string?)e.Attribute("pub-id-type") == "doi");
        var depth = Depth(doi);

        var injected = JatsXmlWriter.BuildLeaf(
            doi.Name,
            "00123",
            new[] { new XAttribute("pub-id-type", "other") });
        JatsXmlWriter.InsertAfter(doi, injected, depth);

        var produced = doc.Serialize();

        var diff = AddedLines(original, produced);
        var addedLine = Assert.Single(diff);
        Assert.Equal("\t\t\t<article-id pub-id-type=\"other\">00123</article-id>", addedLine);
    }

    // Indentation depth = number of leading tabs on the line carrying the element.
    private static int Depth(XElement element)
    {
        if (element.PreviousNode is XText { Value: var ws })
        {
            var tabs = ws.Length - ws.TrimEnd('\t').Length;
            return tabs;
        }

        return 0;
    }

    // Returns the lines present in `produced` but not in `original`, assuming an
    // insertion-only edit (no removals or reorderings).
    private static List<string> AddedLines(string original, string produced)
    {
        var originalLines = original.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
        var producedLines = produced.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');

        var added = new List<string>();
        int o = 0;
        for (int p = 0; p < producedLines.Length; p++)
        {
            if (o < originalLines.Length && originalLines[o] == producedLines[p])
            {
                o++;
            }
            else
            {
                added.Add(producedLines[p]);
            }
        }

        Assert.Equal(originalLines.Length, o); // every original line was matched in order
        return added;
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
