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
    public void StripOutOfScope_OutOfScopePairKeepsContentDropsBrackets()
    {
        // Task 06 strip semantics: an out-of-scope pair has its brackets and
        // attributes removed but its content stays. This keeps cross-release
        // text alignment intact for tags whose attribute changes are owned by
        // a future task (e.g. [author], [authorid] before task 07 ships).
        var input = "[kwdgrp language=\"en\"]K1, K2[/kwdgrp][abstract]X[/abstract]";

        var result = Phase2DiffUtility.StripOutOfScope(input, new[] { "abstract" });

        Assert.Equal("K1, K2[abstract]X[/abstract]", result);
    }

    [Fact]
    public void StripOutOfScope_PreservesContentOfStrippedPair()
    {
        var input = "before [kwd]Y[/kwd] after";

        var result = Phase2DiffUtility.StripOutOfScope(input, Array.Empty<string>());

        Assert.Equal("before Y after", result);
    }

    [Fact]
    public void StripOutOfScope_EmptyInScopePeelsBracketsButKeepsAllContent()
    {
        var input = "before [hist][received]2024[/received][/hist] middle [abstract]X[/abstract] end";

        var result = Phase2DiffUtility.StripOutOfScope(input, Array.Empty<string>());

        Assert.Equal("before 2024 middle X end", result);
    }

    [Fact]
    public void StripOutOfScope_TagWithEqualsInsideAttributeValueHasContentKept()
    {
        var input = "[histdate dateiso=\"20240101\"]Jan[/histdate]";

        var result = Phase2DiffUtility.StripOutOfScope(input, Array.Empty<string>());

        Assert.Equal("Jan", result);
    }

    [Fact]
    public void StripOutOfScope_NestedScopedTagAroundUnscopedInner_PeelsInnerBracketsKeepsContent()
    {
        // [abstract] is in scope; the inner [xref] is not. The outer pair is preserved
        // and the recursive strip removes the [xref] brackets but keeps "Z".
        var input = "[abstract]A [xref]Z[/xref] B[/abstract]";

        var result = Phase2DiffUtility.StripOutOfScope(input, new[] { "abstract" });

        Assert.Equal("[abstract]A Z B[/abstract]", result);
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
    public void Compare_OutOfScopeTagWrappingSameContent_StripsBracketsAndMatches()
    {
        // Symmetric strip (task 06 semantics): produced has the same body
        // text as expected, but expected has an extra [other] wrapper around
        // the trailing word that produced does not have. With scope={abstract}
        // the strip peels [other] brackets from expected (keeping content),
        // and the produced side has no brackets to strip — they match.
        var produced = WriteDocx("p.docx", "[abstract]X[/abstract] body");
        var expected = WriteDocx("e.docx", "[abstract]X[/abstract] [other]body[/other]");

        var result = Phase2DiffUtility.Compare(produced, expected, new[] { "abstract" });

        Assert.True(
            result.IsMatch,
            $"Expected match. Offset={result.FirstDivergenceOffset}; " +
            $"produced={result.ProducedContext}; expected={result.ExpectedContext}");
    }

    [Fact]
    public void Compare_OutOfScopeTagInExpectedOnlyExposesContentMismatch()
    {
        // Out-of-scope content stays after strip (symmetric, content kept).
        // Expected carries "K1, K2" content that produced lacks; the diff
        // surfaces that gap rather than silently masking it.
        var produced = WriteDocx("p.docx", "[abstract]X[/abstract]");
        var expected = WriteDocx(
            "e.docx",
            "[kwdgrp language=\"en\"]K1, K2[/kwdgrp][abstract]X[/abstract]");

        var result = Phase2DiffUtility.Compare(produced, expected, new[] { "abstract" });

        Assert.False(result.IsMatch);
        Assert.Equal(0, result.FirstDivergenceOffset);
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
        // Self-compare on a real corpus file. Even after task 06's symmetric
        // strip + content-keep refinement, comparing a file to itself with
        // any scope (including the empty set) must trivially hold: both sides
        // produce the same stripped string, so the diff must match.
        var corpusPath = ResolveCorpusPath("5136.docx");

        var result = Phase2DiffUtility.Compare(corpusPath, corpusPath, Array.Empty<string>());

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
