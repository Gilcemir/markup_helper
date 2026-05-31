using System.Security.Cryptography;
using DocFormatter.Core.Jats;
using Xunit;

namespace DocFormatter.Tests.Jats;

public sealed class DocxSourceReaderTests
{
    private const string Header =
        "[doc sps=\"1.9\" acron=\"cbab\" volid=\"26\" issueno=\"2\" order=\"01\" " +
        "elocatid=\"e54492621\" doctopic=\"oa\" language=\"en\"]" +
        "[doi]10.1590/1984-70332026v26n2a16[/doi]";

    // ----- Header keys -----

    [Fact]
    public void Parse_DocHeader_ExtractsElocationIdAndDoi()
    {
        var source = DocxSourceReader.Parse(new[] { Header });

        Assert.Equal("e54492621", source.ElocationId);
        Assert.Equal("10.1590/1984-70332026v26n2a16", source.Doi);
    }

    [Fact]
    public void Parse_DoiWithLeadingSpace_IsTrimmed()
    {
        var header =
            "[doc elocatid=\"e53132629\"][doi] 10.1590/1984-70332026v26n2c24[/doi]";

        var source = DocxSourceReader.Parse(new[] { header });

        Assert.Equal("e53132629", source.ElocationId);
        Assert.Equal("10.1590/1984-70332026v26n2c24", source.Doi);
    }

    [Fact]
    public void Parse_MissingElocationId_Throws()
    {
        var header = "[doc acron=\"cbab\"][doi]10.1590/x[/doi]";

        var ex = Assert.Throws<InvalidDataException>(() => DocxSourceReader.Parse(new[] { header }));
        Assert.Contains("elocatid", ex.Message);
    }

    [Fact]
    public void Parse_MissingDoi_Throws()
    {
        var header = "[doc elocatid=\"e54492621\"]";

        var ex = Assert.Throws<InvalidDataException>(() => DocxSourceReader.Parse(new[] { header }));
        Assert.Contains("doi", ex.Message);
    }

    // ----- Scientific / associate editor -----

    [Fact]
    public void Parse_ScientificEditor_RegularSpace_ParsesName()
    {
        var source = DocxSourceReader.Parse(new[]
        {
            Header,
            "Scientific Editor: Ann Smith",
        });

        Assert.Equal("Ann Smith", source.ScientificEditor);
        Assert.Null(source.AssociateEditor);
    }

    [Fact]
    public void Parse_ScientificEditor_NonBreakingSpaceVariants_NormalizeToRegularSpaces()
    {
        // U+202F between "Scientific" and "Editor" (5313/5517), U+00A0 after the
        // colon (5523), U+202F inside the name (5517), and a trailing U+00A0
        // (5458) all normalize to single regular spaces and are trimmed.
        var editorLine = "Scientific Editor: Luiz Antonio dos Santos Dias  ";

        var source = DocxSourceReader.Parse(new[] { Header, editorLine });

        Assert.Equal("Luiz Antonio dos Santos Dias", source.ScientificEditor);
    }

    [Fact]
    public void Parse_AssociateEditor_WhenPresent_ParsesName()
    {
        var source = DocxSourceReader.Parse(new[]
        {
            Header,
            "Associate Editor: Jane Doe",
            "Scientific Editor: Ann Smith",
        });

        Assert.Equal("Jane Doe", source.AssociateEditor);
        Assert.Equal("Ann Smith", source.ScientificEditor);
    }

    [Fact]
    public void Parse_NoEditorLine_YieldsNull()
    {
        var source = DocxSourceReader.Parse(new[] { Header, "[corresp id=\"c1\"]x[/corresp]" });

        Assert.Null(source.ScientificEditor);
        Assert.Null(source.AssociateEditor);
    }

    // ----- DATA AVAILABILITY -----

    [Fact]
    public void Parse_DataAvailability_ExtractsBody_StopsBeforeRefs()
    {
        var source = DocxSourceReader.Parse(new[]
        {
            Header,
            "DATA AVAILABILITY",
            "The datasets are available from the corresponding author upon request.",
            "[refs][sectitle]REFERENCES[/sectitle]",
            "[ref id=\"r1\"]x[/ref]",
        });

        Assert.Equal(
            "The datasets are available from the corresponding author upon request.",
            source.DataAvailabilityText);
    }

    [Fact]
    public void Parse_MissingDataAvailability_YieldsNull()
    {
        var source = DocxSourceReader.Parse(new[]
        {
            Header,
            "CREDIT STATEMENT",
            "All authors contributed equally.",
            "[refs][sectitle]REFERENCES[/sectitle]",
        });

        Assert.Null(source.DataAvailabilityText);
    }

    // ----- CREDIT STATEMENT (raw, all three shapes) -----

    [Fact]
    public void Parse_CreditStatement_RoleKeyed_ReturnedVerbatim()
    {
        const string credit =
            "Conceptualization: Lopes DAPS, Faria MV; Methodology: Lopes DAPS, Costa NMEPL.";
        var source = DocxSourceReader.Parse(new[]
        {
            Header,
            "CREDIT STATEMENT",
            credit,
            "DATA AVAILABILITY",
            "Data available on request.",
            "[refs]",
        });

        Assert.Equal(credit, source.CreditStatementRaw);
    }

    [Fact]
    public void Parse_CreditStatement_AuthorKeyed_ReturnedVerbatim()
    {
        const string credit =
            "ATAJ: Conceptualization, Methodology, Software, Validation, Investigation.";
        var source = DocxSourceReader.Parse(new[] { Header, "CREDIT STATEMENT", credit, "[refs]" });

        Assert.Equal(credit, source.CreditStatementRaw);
    }

    [Fact]
    public void Parse_CreditStatement_Prose_ReturnedVerbatim()
    {
        const string credit =
            "All authors contributed to the study conception and design. " +
            "The initial draft was written by TTN Le, and all authors provided feedback.";
        var source = DocxSourceReader.Parse(new[] { Header, "CREDIT STATEMENT", credit, "[refs]" });

        Assert.Equal(credit, source.CreditStatementRaw);
    }

    // ----- Ordering and glued-header edge cases observed in the corpus -----

    [Fact]
    public void Parse_DataAvailabilityBeforeCredit_ExtractsBoth()
    {
        var source = DocxSourceReader.Parse(new[]
        {
            Header,
            "DATA AVAILABILITY",
            "Data on request.",
            "CREDIT STATEMENT",
            "Author A: Conceptualization.",
            "[refs]",
        });

        Assert.Equal("Data on request.", source.DataAvailabilityText);
        Assert.Equal("Author A: Conceptualization.", source.CreditStatementRaw);
    }

    [Fact]
    public void Parse_GluedNextHeader_SplitsBodyFromHeader()
    {
        // 5640 shape: the CREDIT STATEMENT header is glued onto the end of the
        // DATA AVAILABILITY body paragraph with no paragraph break.
        var source = DocxSourceReader.Parse(new[]
        {
            Header,
            "DATA AVAILABILITY",
            "The datasets are available upon request from the corresponding author.CREDIT STATEMENT",
            "The authors contributed collectively to the work.",
            "[refs]",
        });

        Assert.Equal(
            "The datasets are available upon request from the corresponding author.",
            source.DataAvailabilityText);
        Assert.Equal("The authors contributed collectively to the work.", source.CreditStatementRaw);
    }

    [Fact]
    public void Parse_NoCreditStatement_YieldsNull()
    {
        var source = DocxSourceReader.Parse(new[]
        {
            Header,
            "DATA AVAILABILITY",
            "Data available on request.",
            "[refs]",
        });

        Assert.Null(source.CreditStatementRaw);
    }

    // ----- Integration tests over the real corpus docx -----

    [Fact]
    public void Read_5449_PopulatesEditorProseDaAndProseCredit()
    {
        var source = new DocxSourceReader().Read(CorpusDocx("5449.docx"));

        Assert.Equal("e54492621", source.ElocationId);
        Assert.Equal("10.1590/1984-70332026v26n2a16", source.Doi);
        Assert.Equal("Luiz Antônio dos Santos Dias", source.ScientificEditor);
        Assert.NotNull(source.DataAvailabilityText);
        Assert.Contains("corresponding author", source.DataAvailabilityText!);
        Assert.DoesNotContain("[refs]", source.DataAvailabilityText!);
        Assert.NotNull(source.CreditStatementRaw);
        // Prose form: starts with the narrative sentence, no role-key colon list.
        Assert.StartsWith("All authors contributed", source.CreditStatementRaw!);
    }

    [Fact]
    public void Read_5523_PopulatesRoleKeyedCreditAndDa()
    {
        var source = new DocxSourceReader().Read(CorpusDocx("5523.docx"));

        Assert.Equal("e55232626", source.ElocationId);
        Assert.Equal("10.1590/1984-70332026v26n2a21", source.Doi);
        Assert.Equal("Luiz Antônio dos Santos Dias", source.ScientificEditor);
        Assert.NotNull(source.DataAvailabilityText);
        Assert.Contains("corresponding author", source.DataAvailabilityText!);
        Assert.NotNull(source.CreditStatementRaw);
        // Role-keyed form: begins with a CRediT role label followed by a colon.
        Assert.StartsWith("Conceptualization:", source.CreditStatementRaw!);
        Assert.DoesNotContain("[refs]", source.CreditStatementRaw!);
    }

    [Fact]
    public void Read_EveryCorpusDocx_HasHeaderKeys()
    {
        var reader = new DocxSourceReader();
        foreach (var path in CorpusDocxFiles())
        {
            var source = reader.Read(path);
            Assert.False(string.IsNullOrWhiteSpace(source.ElocationId), $"{path}: empty elocatid");
            Assert.False(string.IsNullOrWhiteSpace(source.Doi), $"{path}: empty doi");
        }
    }

    [Fact]
    public void Read_DoesNotModifyTheDocx()
    {
        var path = CorpusDocx("5449.docx");
        var before = Sha256(path);

        _ = new DocxSourceReader().Read(path);

        Assert.Equal(before, Sha256(path));
    }

    private static string Sha256(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream));
    }

    private static IEnumerable<string> CorpusDocxFiles()
        => Directory.EnumerateFiles(CorpusDir(), "*.docx")
            // Word lock files (~$...) are not real documents.
            .Where(p => !Path.GetFileName(p).StartsWith("~$", StringComparison.Ordinal));

    private static string CorpusDocx(string fileName)
    {
        var path = Path.Combine(CorpusDir(), fileName);
        Assert.True(File.Exists(path), $"missing corpus docx {path}");
        return path;
    }

    private static string CorpusDir()
    {
        var dir = AppContext.BaseDirectory;
        while (dir != null)
        {
            var candidate = Path.Combine(dir, "examples", "phase-3", "scielo_markup");
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            dir = Path.GetDirectoryName(dir);
        }

        throw new InvalidOperationException(
            $"Could not locate examples/phase-3/scielo_markup/ from {AppContext.BaseDirectory}.");
    }
}
