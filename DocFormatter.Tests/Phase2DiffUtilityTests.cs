using System.Text.RegularExpressions;
using DocFormatter.Core.Reporting;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Xunit;

namespace DocFormatter.Tests;

public sealed class Phase2DiffUtilityTests : IDisposable
{
    private readonly string _tempDir;

    public Phase2DiffUtilityTests()
    {
        _tempDir = Path.Combine(
            Path.GetTempPath(),
            "phase2-diff-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_tempDir))
            {
                Directory.Delete(_tempDir, recursive: true);
            }
        }
        catch
        {
            // Best-effort cleanup; do not fail the test on temp-dir teardown.
        }
    }

    private string WriteDocx(string fileName, params string[] paragraphTexts)
    {
        var path = Path.Combine(_tempDir, fileName);
        using var doc = WordprocessingDocument.Create(path, WordprocessingDocumentType.Document);
        var mainPart = doc.AddMainDocumentPart();
        var body = new Body();
        foreach (var t in paragraphTexts)
        {
            body.AppendChild(BuildParagraph(t));
        }
        mainPart.Document = new Document(body);
        return path;
    }

    private static Paragraph BuildParagraph(string text)
        => new(new Run(new Text(text) { Space = SpaceProcessingModeValues.Preserve }));

    [Fact]
    public void ExtractBodyText_ThreeParagraphs_JoinsWithNewlineSeparators()
    {
        var path = WriteDocx("alpha-beta-gamma.docx", "alpha", "beta", "gamma");

        Assert.Equal("alpha\nbeta\ngamma", Phase2DiffUtility.ExtractBodyText(path));
    }

    [Fact]
    public void NormalizeParagraphWhitespace_CollapsesRunsAndTrims()
    {
        Assert.Equal(
            "alpha beta",
            Phase2DiffUtility.NormalizeParagraphWhitespace("  alpha   beta  "));
    }

    [Fact]
    public void NormalizeParagraphWhitespace_ConvertsMixedWhitespaceToSingleSpace()
    {
        Assert.Equal(
            "alpha beta gamma",
            Phase2DiffUtility.NormalizeParagraphWhitespace("alpha\tbeta\n  gamma"));
    }

    [Fact]
    public void ExtractBodyText_PreservesScieloTagLiteralsInParagraphText()
    {
        var path = WriteDocx("tag.docx", "[abstract language=\"en\"]body[/abstract]");

        Assert.Equal(
            "[abstract language=\"en\"]body[/abstract]",
            Phase2DiffUtility.ExtractBodyText(path));
    }

    [Fact]
    public void StripOutOfScope_RemovesUnscopedPairAndKeepsScopedPair()
    {
        var input = "[kwdgrp language=\"en\"]K1, K2[/kwdgrp][abstract]X[/abstract]";

        var result = Phase2DiffUtility.StripOutOfScope(input, new[] { "abstract" });

        Assert.Equal("[abstract]X[/abstract]", result);
    }

    [Fact]
    public void StripOutOfScope_PreservesLeadingAndTrailingContextAroundStrippedPair()
    {
        var input = "before [kwd]Y[/kwd] after";

        var result = Phase2DiffUtility.StripOutOfScope(input, Array.Empty<string>());

        // The pair is replaced by the empty string; the surrounding spaces remain so
        // operators can see exactly what bracket pair was stripped.
        Assert.Equal("before  after", result);
    }

    [Fact]
    public void StripOutOfScope_EmptyInScopeRemovesEveryRecognizedTagPair()
    {
        var input = "before [hist][received]2024[/received][/hist] middle [abstract]X[/abstract] end";

        var result = Phase2DiffUtility.StripOutOfScope(input, Array.Empty<string>());

        Assert.Equal("before  middle  end", result);
    }

    [Fact]
    public void StripOutOfScope_TagWithEqualsInsideAttributeValueIsStripped()
    {
        var input = "[histdate dateiso=\"20240101\"]Jan[/histdate]";

        var result = Phase2DiffUtility.StripOutOfScope(input, Array.Empty<string>());

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void StripOutOfScope_NestedScopedTagAroundUnscopedInner_StripsOnlyInner()
    {
        // [abstract] is in scope; the inner [xref] is not. The outer pair is preserved
        // and the recursive strip removes [xref]Z[/xref] from the body.
        var input = "[abstract]A [xref]Z[/xref] B[/abstract]";

        var result = Phase2DiffUtility.StripOutOfScope(input, new[] { "abstract" });

        Assert.Equal("[abstract]A  B[/abstract]", result);
    }

    [Fact]
    public void StripOutOfScope_NoOpWhenAllPairsInScope()
    {
        var input = "[abstract][p]X[/p][/abstract]";

        var result = Phase2DiffUtility.StripOutOfScope(input, new[] { "abstract", "p" });

        Assert.Equal("[abstract][p]X[/p][/abstract]", result);
    }

    [Fact]
    public void FindFirstDivergenceOffset_EqualStrings_ReturnsNull()
    {
        Assert.Null(Phase2DiffUtility.FindFirstDivergenceOffset("abc", "abc"));
    }

    [Fact]
    public void FindFirstDivergenceOffset_DifferAtMiddle_ReturnsCharIndex()
    {
        Assert.Equal(7, Phase2DiffUtility.FindFirstDivergenceOffset("0123456X", "0123456Y"));
    }

    [Fact]
    public void FindFirstDivergenceOffset_PrefixOfLonger_ReturnsShorterLength()
    {
        Assert.Equal(3, Phase2DiffUtility.FindFirstDivergenceOffset("abc", "abcdef"));
        Assert.Equal(3, Phase2DiffUtility.FindFirstDivergenceOffset("abcdef", "abc"));
    }

    [Fact]
    public void SliceContext_ClampsToStringBoundsAndReturnsUpToEightyCharsEachSide()
    {
        var s = new string('x', 50) + "Y" + new string('z', 200);

        var slice = Phase2DiffUtility.SliceContext(s, offset: 50);

        // Up to 80 chars on each side, clamped at the start (only 50 chars before offset).
        Assert.Equal(50 + 80, slice.Length);
        Assert.Contains("Y", slice);
    }

    [Fact]
    public void Compare_EqualSyntheticDocxFiles_ReturnsIsMatchTrue()
    {
        var produced = WriteDocx("p.docx", "alpha", "beta");
        var expected = WriteDocx("e.docx", "alpha", "beta");

        var result = Phase2DiffUtility.Compare(produced, expected, Array.Empty<string>());

        Assert.True(result.IsMatch);
        Assert.Null(result.FirstDivergenceOffset);
        Assert.Null(result.ProducedContext);
        Assert.Null(result.ExpectedContext);
    }

    [Fact]
    public void Compare_DifferAtOffsetSeven_ReportsOffsetAndPopulatesBothContexts()
    {
        var produced = WriteDocx("p.docx", "0123456X");
        var expected = WriteDocx("e.docx", "0123456Y");

        var result = Phase2DiffUtility.Compare(produced, expected, Array.Empty<string>());

        Assert.False(result.IsMatch);
        Assert.Equal(7, result.FirstDivergenceOffset);
        Assert.Equal("0123456X", result.ProducedContext);
        Assert.Equal("0123456Y", result.ExpectedContext);
    }

    [Fact]
    public void Compare_OutOfScopeTagInExpectedIsStrippedBeforeMatching()
    {
        // Produced has only the in-scope abstract pair. Expected has an out-of-scope
        // [kwdgrp] pair sandwiching [abstract]X[/abstract]. With scope={abstract}, the
        // strip removes [kwdgrp ...] from the expected side and the match holds.
        var produced = WriteDocx("p.docx", "[abstract]X[/abstract]");
        var expected = WriteDocx(
            "e.docx",
            "[kwdgrp language=\"en\"]K1, K2[/kwdgrp][abstract]X[/abstract]");

        var result = Phase2DiffUtility.Compare(produced, expected, new[] { "abstract" });

        Assert.True(
            result.IsMatch,
            $"Expected match. Offset={result.FirstDivergenceOffset}; " +
            $"produced={result.ProducedContext}; expected={result.ExpectedContext}");
    }

    [Fact]
    public void Compare_ByteIdenticalDocxCopy_ReturnsIsMatchTrue()
    {
        var src = WriteDocx("src.docx", "hello", "world");
        var copy = Path.Combine(_tempDir, "copy.docx");
        File.Copy(src, copy);

        var result = Phase2DiffUtility.Compare(src, copy, Array.Empty<string>());

        Assert.True(result.IsMatch);
    }

    [Fact]
    public void Compare_OneParagraphMutated_DivergesInsideMutatedParagraph()
    {
        // Two leading paragraphs identical (18 chars each + \n separators).
        // Mutation in the third paragraph.
        var produced = WriteDocx(
            "p.docx",
            "intact paragraph 1",
            "intact paragraph 2",
            "produced final paragraph");
        var expected = WriteDocx(
            "e.docx",
            "intact paragraph 1",
            "intact paragraph 2",
            "EXPECTED final paragraph");

        var result = Phase2DiffUtility.Compare(produced, expected, Array.Empty<string>());

        Assert.False(result.IsMatch);
        Assert.NotNull(result.FirstDivergenceOffset);
        // 18 + 1 + 18 + 1 = 38 chars match before the third paragraph begins.
        Assert.Equal(38, result.FirstDivergenceOffset);
        Assert.NotNull(result.ProducedContext);
        Assert.Contains("produced final paragraph", result.ProducedContext);
        Assert.NotNull(result.ExpectedContext);
        Assert.Contains("EXPECTED final paragraph", result.ExpectedContext);
    }

    [Fact]
    public void Compare_RealCorpusFileAgainstItselfWithFullScope_ReturnsIsMatchTrue()
    {
        // Self-compare on a real corpus file. The Phase 2 release scope
        // ({abstract, kwdgrp, elocation}) does NOT cover all bracket pairs already
        // present in `before/<id>.docx` (e.g. [author], [doctitle], [normaff], [xref]
        // appear from upstream Stage-1 markup). Per ADR-006, strip applies to the
        // expected side only — so for self-compare to hold the in-scope set must
        // include every tag-pair name the file actually contains. Discover the set
        // dynamically so this test stays correct as the corpus evolves.
        var corpusPath = ResolveCorpusPath("5136.docx");
        var bodyText = Phase2DiffUtility.ExtractBodyText(corpusPath);
        var tagsInFile = ExtractTagPairNames(bodyText);

        var result = Phase2DiffUtility.Compare(corpusPath, corpusPath, tagsInFile);

        Assert.True(
            result.IsMatch,
            $"Self-compare divergence at offset {result.FirstDivergenceOffset}.\n" +
            $"Produced ctx: {result.ProducedContext}\n" +
            $"Expected ctx: {result.ExpectedContext}");
    }

    [Fact]
    public void Compare_TwoDifferentRealCorpusFiles_ReportsDivergenceWithContext()
    {
        var first = ResolveCorpusPath("5136.docx");
        var second = ResolveCorpusPath("5293.docx");

        var result = Phase2DiffUtility.Compare(first, second, Array.Empty<string>());

        Assert.False(result.IsMatch);
        Assert.NotNull(result.FirstDivergenceOffset);
        Assert.NotNull(result.ProducedContext);
        Assert.NotNull(result.ExpectedContext);
    }

    [Fact]
    public void Compare_NullOrEmptyArguments_ThrowsArgumentException()
    {
        var path = WriteDocx("p.docx", "alpha");

        Assert.Throws<ArgumentException>(
            () => Phase2DiffUtility.Compare("", path, Array.Empty<string>()));
        Assert.Throws<ArgumentException>(
            () => Phase2DiffUtility.Compare(path, "", Array.Empty<string>()));
        Assert.Throws<ArgumentNullException>(
            () => Phase2DiffUtility.Compare(path, path, null!));
    }

    private static IReadOnlyCollection<string> ExtractTagPairNames(string text)
    {
        // Capture every opening or closing bracket-tag name regardless of nesting depth.
        // The strip in Phase2DiffUtility recurses into the content of each in-scope match,
        // so the discovery here must do the same — using the linear pair-pattern would
        // miss tags nested inside an outer matched pair.
        var pattern = new Regex(@"\[/?(\w+)", RegexOptions.Singleline);
        var names = new HashSet<string>(StringComparer.Ordinal);
        foreach (Match match in pattern.Matches(text))
        {
            names.Add(match.Groups[1].Value);
        }
        return names;
    }

    private static string ResolveCorpusPath(string fileName)
    {
        var dir = AppContext.BaseDirectory;
        while (dir != null)
        {
            var candidate = Path.Combine(dir, "examples", "phase-2", "before", fileName);
            if (File.Exists(candidate))
            {
                return candidate;
            }
            dir = Path.GetDirectoryName(dir);
        }
        throw new InvalidOperationException(
            $"Could not locate examples/phase-2/before/{fileName} from {AppContext.BaseDirectory}.");
    }
}
