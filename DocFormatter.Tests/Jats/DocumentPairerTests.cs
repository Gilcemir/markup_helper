using System.Xml.Linq;
using DocFormatter.Core.Jats;
using Xunit;

namespace DocFormatter.Tests.Jats;

public sealed class DocumentPairerTests
{
    private const string Elocation = "e54492621";
    private const string Doi = "10.1590/1984-70332026v26n2a16";
    private const string XmlPath = "1984-7033-cbab-26-02-e54492621.xml";

    private static XDocument ArticleXml(string? elocationId, string? doi)
    {
        var ids = doi is null
            ? string.Empty
            : $"\t\t\t<article-id pub-id-type=\"doi\">{doi}</article-id>\n";
        var eloc = elocationId is null
            ? string.Empty
            : $"\t\t\t<elocation-id>{elocationId}</elocation-id>\n";
        var xml =
            "<article>\n" +
            "\t<front>\n" +
            "\t\t<article-meta>\n" +
            ids +
            "\t\t\t<volume>26</volume>\n" +
            eloc +
            "\t\t</article-meta>\n" +
            "\t</front>\n" +
            "\t<back>\n" +
            // Reference citations also carry <elocation-id>; these must be ignored.
            "\t\t<ref-list><ref><element-citation><elocation-id>e18</elocation-id></element-citation></ref></ref-list>\n" +
            "\t</back>\n" +
            "</article>\n";
        return XDocument.Parse(xml, LoadOptions.PreserveWhitespace);
    }

    private static DocxCandidate Candidate(string elocationId, string doi, string path = "5449.docx")
        => new(path, new DocxSource { ElocationId = elocationId, Doi = doi });

    private static OtherTable OtherWith(string basenameWithExt, string number)
        => OtherTable.Parse(new[] { $"{basenameWithExt}\t{number}" });

    [Fact]
    public void Pair_MatchingElocationEqualDoiPresentOther_ReturnsSuccessfulPair()
    {
        var xml = ArticleXml(Elocation, Doi);
        var candidates = new[] { Candidate(Elocation, Doi) };
        var other = OtherWith("1984-7033-cbab-26-02-e54492621.pdf", "00201");

        var result = DocumentPairer.Pair(xml, XmlPath, candidates, other);

        Assert.True(result.IsPaired);
        Assert.Null(result.Error);
        var pair = result.Pair!;
        Assert.Equal(Elocation, pair.ElocationId);
        Assert.Equal(Doi, pair.Doi);
        Assert.Equal("00201", pair.OtherNumber);
        Assert.Equal(XmlPath, pair.XmlPath);
        Assert.Equal("5449.docx", pair.DocxPath);
        Assert.Same(candidates[0].Source, pair.Source);
    }

    [Fact]
    public void Pair_EqualDoiDifferentCase_StillPairs()
    {
        // DOIs are case-insensitive; a case-only difference is not a mismatch.
        var xml = ArticleXml(Elocation, Doi.ToUpperInvariant());
        var candidates = new[] { Candidate(Elocation, Doi.ToLowerInvariant()) };
        var other = OtherWith("1984-7033-cbab-26-02-e54492621.pdf", "00201");

        var result = DocumentPairer.Pair(xml, XmlPath, candidates, other);

        Assert.True(result.IsPaired);
    }

    [Fact]
    public void Pair_DoiMismatch_FailsLoudNamingTheConflict()
    {
        var xml = ArticleXml(Elocation, Doi);
        var candidates = new[] { Candidate(Elocation, "10.1590/1984-70332026v26n2a99") };
        var other = OtherWith("1984-7033-cbab-26-02-e54492621.pdf", "00201");

        var result = DocumentPairer.Pair(xml, XmlPath, candidates, other);

        Assert.False(result.IsPaired);
        Assert.Null(result.Pair);
        Assert.NotNull(result.Error);
        Assert.Contains("DOI mismatch", result.Error);
        Assert.Contains(Doi, result.Error);
        Assert.Contains("10.1590/1984-70332026v26n2a99", result.Error);
    }

    [Fact]
    public void Pair_XmlMissingElocationId_FailsLoud()
    {
        var xml = ArticleXml(elocationId: null, doi: Doi);
        var candidates = new[] { Candidate(Elocation, Doi) };
        var other = OtherWith("1984-7033-cbab-26-02-e54492621.pdf", "00201");

        var result = DocumentPairer.Pair(xml, XmlPath, candidates, other);

        Assert.False(result.IsPaired);
        Assert.Contains("elocation-id", result.Error);
    }

    [Fact]
    public void Pair_XmlMissingArticleMeta_FailsLoud()
    {
        var xml = XDocument.Parse("<article>\n\t<body/>\n</article>\n", LoadOptions.PreserveWhitespace);
        var candidates = new[] { Candidate(Elocation, Doi) };
        var other = OtherWith("1984-7033-cbab-26-02-e54492621.pdf", "00201");

        var result = DocumentPairer.Pair(xml, XmlPath, candidates, other);

        Assert.False(result.IsPaired);
        Assert.Contains("article-meta", result.Error);
    }

    [Fact]
    public void Pair_XmlMissingDoi_FailsLoud()
    {
        var xml = ArticleXml(Elocation, doi: null);
        var candidates = new[] { Candidate(Elocation, Doi) };
        var other = OtherWith("1984-7033-cbab-26-02-e54492621.pdf", "00201");

        var result = DocumentPairer.Pair(xml, XmlPath, candidates, other);

        Assert.False(result.IsPaired);
        Assert.Contains("doi", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Pair_NoDocxWithMatchingElocatid_FailsLoud()
    {
        var xml = ArticleXml(Elocation, Doi);
        var candidates = new[] { Candidate("e99999999", Doi) };
        var other = OtherWith("1984-7033-cbab-26-02-e54492621.pdf", "00201");

        var result = DocumentPairer.Pair(xml, XmlPath, candidates, other);

        Assert.False(result.IsPaired);
        Assert.Contains(Elocation, result.Error);
        Assert.Contains("No docx", result.Error);
    }

    [Fact]
    public void Pair_MultipleDocxShareElocatid_FailsLoudWithoutGuessing()
    {
        var xml = ArticleXml(Elocation, Doi);
        var candidates = new[]
        {
            Candidate(Elocation, Doi, "5449.docx"),
            Candidate(Elocation, Doi, "5450.docx"),
        };
        var other = OtherWith("1984-7033-cbab-26-02-e54492621.pdf", "00201");

        var result = DocumentPairer.Pair(xml, XmlPath, candidates, other);

        Assert.False(result.IsPaired);
        Assert.Contains("Ambiguous", result.Error);
        Assert.Contains("5449.docx", result.Error);
        Assert.Contains("5450.docx", result.Error);
    }

    [Fact]
    public void Pair_BasenameAbsentFromOtherTable_FailsLoud()
    {
        var xml = ArticleXml(Elocation, Doi);
        var candidates = new[] { Candidate(Elocation, Doi) };
        var other = OtherWith("1984-7033-cbab-26-02-e99999999.pdf", "00999");

        var result = DocumentPairer.Pair(xml, XmlPath, candidates, other);

        Assert.False(result.IsPaired);
        Assert.Contains("other.txt", result.Error);
        Assert.Contains("1984-7033-cbab-26-02-e54492621", result.Error);
    }

    [Fact]
    public void PairFailure_BlankReason_Throws()
        => Assert.Throws<ArgumentException>(() => PairingResult.Failure("  "));

    // ----- Integration: pair every XML in the real corpus -----

    [Fact]
    public void Pair_RealCorpus_EveryXmlPairsWithAgreeingDoi()
    {
        var corpusRoot = CorpusRoot();
        var packageDir = Path.Combine(corpusRoot, "scielo_package");
        var markupDir = Path.Combine(corpusRoot, "scielo_markup");
        var otherTable = OtherTable.Load(Path.Combine(corpusRoot, "other.txt"));

        var xmlFiles = Directory.EnumerateFiles(packageDir, "*.xml").ToList();
        Assert.NotEmpty(xmlFiles);

        foreach (var xmlPath in xmlFiles)
        {
            var result = DocumentPairer.Pair(xmlPath, markupDir, otherTable);

            Assert.True(result.IsPaired, $"{Path.GetFileName(xmlPath)} failed to pair: {result.Error}");
            var pair = result.Pair!;
            // The pairing already asserts DOI equality; confirm the contract holds.
            Assert.Equal(
                pair.Source.Doi.Trim(),
                pair.Doi,
                ignoreCase: true);
            Assert.False(string.IsNullOrEmpty(pair.OtherNumber));
            Assert.True(File.Exists(pair.DocxPath));
        }
    }

    private static string CorpusRoot()
    {
        var dir = AppContext.BaseDirectory;
        while (dir != null)
        {
            var candidate = Path.Combine(dir, "examples", "phase-3");
            if (Directory.Exists(Path.Combine(candidate, "scielo_package")))
            {
                return candidate;
            }

            dir = Path.GetDirectoryName(dir);
        }

        throw new InvalidOperationException(
            $"Could not locate examples/phase-3/ from {AppContext.BaseDirectory}.");
    }
}
