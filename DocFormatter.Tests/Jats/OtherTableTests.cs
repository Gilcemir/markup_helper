using System;
using DocFormatter.Core.Jats;
using Xunit;

namespace DocFormatter.Tests.Jats;

public sealed class OtherTableTests
{
    [Fact]
    public void Parse_PdfBasename_IsRetrievableByStrippedBasename()
    {
        var table = OtherTable.Parse(new[] { "1984-7033-cbab-26-02-e54492621.pdf\t00201" });

        Assert.True(table.TryGetOther("1984-7033-cbab-26-02-e54492621", out var other));
        Assert.Equal("00201", other);
    }

    [Theory]
    [InlineData("1984-7033-cbab-26-02-e54492621")] // XML basename (no extension)
    [InlineData("1984-7033-cbab-26-02-e54492621.xml")] // XML basename with extension
    [InlineData("1984-7033-cbab-26-02-e54492621.pdf")] // PDF basename
    public void TryGetOther_ResolvesRegardlessOfQueryExtension(string query)
    {
        var table = OtherTable.Parse(new[] { "1984-7033-cbab-26-02-e54492621.pdf\t00201" });

        Assert.True(table.TryGetOther(query, out var other));
        Assert.Equal("00201", other);
    }

    [Fact]
    public void TryGetOther_AbsentBasename_ReturnsNotFoundNotEmptyString()
    {
        var table = OtherTable.Parse(new[] { "1984-7033-cbab-26-02-e54492621.pdf\t00201" });

        Assert.False(table.TryGetOther("does-not-exist", out var other));
        Assert.Null(other); // distinct "not found", never a silent empty string
    }

    [Fact]
    public void Parse_PreservesOtherValueExactly_LeadingZerosIntact()
    {
        var table = OtherTable.Parse(new[] { "foo.pdf\t00201" });

        Assert.True(table.TryGetOther("foo", out var other));
        Assert.Equal("00201", other);
        Assert.Equal(5, other!.Length); // not numerically parsed (would drop zeros)
    }

    [Fact]
    public void Parse_IgnoresBlankLinesAndToleratesTrailingWhitespace()
    {
        var table = OtherTable.Parse(new[]
        {
            string.Empty,
            "   ",
            "a.pdf\t00001  ", // trailing whitespace after the value
            "\t",             // whitespace-only after trim → blank
            "  b.pdf\t00002", // leading whitespace before the key
        });

        Assert.Equal(2, table.Count);
        Assert.True(table.TryGetOther("a", out var a));
        Assert.Equal("00001", a);
        Assert.True(table.TryGetOther("b", out var b));
        Assert.Equal("00002", b);
    }

    [Fact]
    public void Parse_LineWithoutTab_ThrowsInvalidData()
    {
        var ex = Assert.Throws<InvalidDataException>(
            () => OtherTable.Parse(new[] { "no-tab-here-00201" }));
        Assert.Contains("tab-separated", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Parse_DuplicateBasenameConflictingValue_ThrowsInvalidData()
    {
        Assert.Throws<InvalidDataException>(() => OtherTable.Parse(new[]
        {
            "x.pdf\t00201",
            "x.xml\t00999", // same stripped basename, different value
        }));
    }

    [Fact]
    public void Parse_DuplicateBasenameSameValue_IsTolerated()
    {
        var table = OtherTable.Parse(new[]
        {
            "x.pdf\t00201",
            "x.xml\t00201", // same stripped basename, same value
        });

        Assert.Equal(1, table.Count);
        Assert.True(table.TryGetOther("x", out var other));
        Assert.Equal("00201", other);
    }

    [Fact]
    public void TryGetOther_NullOrEmptyBasename_Throws()
    {
        var table = OtherTable.Parse(Array.Empty<string>());

        Assert.Throws<ArgumentNullException>(() => table.TryGetOther(null!, out _));
        Assert.Throws<ArgumentException>(() => table.TryGetOther(string.Empty, out _));
    }

    [Fact]
    public void Load_NullOrEmptyPath_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => OtherTable.Load(null!));
        Assert.Throws<ArgumentException>(() => OtherTable.Load(string.Empty));
    }

    // --- Integration: load the real corpus other.txt -------------------------

    [Fact]
    public void Load_RealCorpusOtherTxt_OneEntryPerNonBlankLineResolvableByXmlBasename()
    {
        var path = CorpusOtherTxt();
        var nonBlankLines = File.ReadAllLines(path)
            .Count(l => !string.IsNullOrWhiteSpace(l));

        var table = OtherTable.Load(path);

        Assert.Equal(nonBlankLines, table.Count);

        // Every line's PDF basename resolves by its XML basename (extension stripped).
        foreach (var line in File.ReadAllLines(path))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var parts = line.Split('\t');
            var xmlBasename = Path.GetFileNameWithoutExtension(parts[0].Trim());
            Assert.True(table.TryGetOther(xmlBasename, out var other));
            Assert.Equal(parts[1].Trim(), other);
        }
    }

    [Fact]
    public void Load_RealCorpusOtherTxt_KnownEntryResolves()
    {
        var table = OtherTable.Load(CorpusOtherTxt());

        Assert.True(table.TryGetOther("1984-7033-cbab-26-02-e54492621", out var other));
        Assert.Equal("00201", other);
    }

    private static string CorpusOtherTxt()
    {
        var dir = AppContext.BaseDirectory;
        while (dir != null)
        {
            var candidate = Path.Combine(dir, "examples", "phase-3", "other.txt");
            if (File.Exists(candidate))
            {
                return candidate;
            }

            dir = Path.GetDirectoryName(dir);
        }

        throw new InvalidOperationException(
            $"Could not locate examples/phase-3/other.txt from {AppContext.BaseDirectory}.");
    }
}
