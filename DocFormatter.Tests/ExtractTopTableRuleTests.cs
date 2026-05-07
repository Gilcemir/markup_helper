using DocFormatter.Core.Options;
using DocFormatter.Core.Pipeline;
using DocFormatter.Core.Rules;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Xunit;

namespace DocFormatter.Tests;

public sealed class ExtractTopTableRuleTests
{
    private static WordprocessingDocument CreateDocumentWith(params OpenXmlElement[] bodyChildren)
    {
        var stream = new MemoryStream();
        var doc = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document);
        var mainPart = doc.AddMainDocumentPart();
        mainPart.Document = new Document(new Body(bodyChildren));
        return doc;
    }

    private static Body GetBody(WordprocessingDocument doc)
        => doc.MainDocumentPart!.Document!.Body!;

    private static TableCell BuildCell(params string[] paragraphTexts)
    {
        var paragraphs = paragraphTexts
            .Select(t => new Paragraph(new Run(new Text(t) { Space = SpaceProcessingModeValues.Preserve })))
            .Cast<OpenXmlElement>()
            .ToArray();
        return new TableCell(paragraphs);
    }

    private static Table BuildTable(int gridColumns, params TableRow[] rows)
    {
        var grid = new TableGrid();
        for (var i = 0; i < gridColumns; i++)
        {
            grid.Append(new GridColumn { Width = "1000" });
        }

        var table = new Table(grid);
        foreach (var row in rows)
        {
            table.Append(row);
        }

        return table;
    }

    private static Table BuildThreeByOneTable(TableCell c1, TableCell c2, TableCell c3)
        => BuildTable(3, new TableRow(c1, c2, c3));

    [Fact]
    public void Apply_WithHeaderTextInEachCell_ExtractsValuesAndDeletesTable()
    {
        var table = BuildThreeByOneTable(
            BuildCell("id", "ART01"),
            BuildCell("elocation", "e2024001"),
            BuildCell("doi", "10.1234/abc"));
        using var doc = CreateDocumentWith(table);
        var rule = new ExtractTopTableRule(new FormattingOptions());
        var ctx = new FormattingContext();
        var report = new Report();

        rule.Apply(doc, ctx, report);

        Assert.Equal("e2024001", ctx.ElocationId);
        Assert.Equal("10.1234/abc", ctx.Doi);
        Assert.Empty(GetBody(doc).Elements<Table>());
        Assert.DoesNotContain(report.Entries, e => e.Level == ReportLevel.Warn);
    }

    [Fact]
    public void Apply_WithoutHeaders_FallsBackToPositionalMapping_AndLogsWarn()
    {
        var table = BuildThreeByOneTable(
            BuildCell("ART01"),
            BuildCell("e2024001"),
            BuildCell("10.1234/abc"));
        using var doc = CreateDocumentWith(table);
        var rule = new ExtractTopTableRule(new FormattingOptions());
        var ctx = new FormattingContext();
        var report = new Report();

        rule.Apply(doc, ctx, report);

        Assert.Equal("e2024001", ctx.ElocationId);
        Assert.Equal("10.1234/abc", ctx.Doi);
        Assert.Empty(GetBody(doc).Elements<Table>());
        Assert.Contains(
            report.Entries,
            e => e.Level == ReportLevel.Warn && e.Message.Contains("positional", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Apply_WhenDoiCellInvalid_FindsDoiInAnotherCell_AndElocationFallsBack()
    {
        var table = BuildThreeByOneTable(
            BuildCell("ART01"),
            BuildCell("10.5678/xyz"),
            BuildCell("not-a-doi"));
        using var doc = CreateDocumentWith(table);
        var rule = new ExtractTopTableRule(new FormattingOptions());
        var ctx = new FormattingContext();
        var report = new Report();

        rule.Apply(doc, ctx, report);

        Assert.Equal("10.5678/xyz", ctx.Doi);
        Assert.Equal(string.Empty, ctx.ElocationId);
        Assert.Empty(GetBody(doc).Elements<Table>());
        Assert.Contains(
            report.Entries,
            e => e.Level == ReportLevel.Warn
                && e.Message.Contains("DOI cell did not match", StringComparison.Ordinal));
        Assert.Contains(
            report.Entries,
            e => e.Level == ReportLevel.Warn
                && e.Message.Contains("ELOCATION-shaped value", StringComparison.Ordinal));
    }

    [Fact]
    public void Apply_WhenDoiCellWrappedInDoiOrgUrl_StripsPrefix_AndExtractsDoi()
    {
        var table = BuildThreeByOneTable(
            BuildCell("id", "ART01"),
            BuildCell("elocation", "e2024001"),
            BuildCell("doi", "http://dx.doi.org/10.1234/abc"));
        using var doc = CreateDocumentWith(table);
        var rule = new ExtractTopTableRule(new FormattingOptions());
        var ctx = new FormattingContext();
        var report = new Report();

        rule.Apply(doc, ctx, report);

        Assert.Equal("10.1234/abc", ctx.Doi);
        Assert.Equal("e2024001", ctx.ElocationId);
        Assert.DoesNotContain(report.Entries, e => e.Level == ReportLevel.Warn);
    }

    [Fact]
    public void Apply_WhenPositionalCellsReorderedDoiThenElocation_RecoversBothFromOtherCells()
    {
        var table = BuildThreeByOneTable(
            BuildCell("ART01"),
            BuildCell("https://doi.org/10.1234/abc"),
            BuildCell("e2024001"));
        using var doc = CreateDocumentWith(table);
        var rule = new ExtractTopTableRule(new FormattingOptions());
        var ctx = new FormattingContext();
        var report = new Report();

        rule.Apply(doc, ctx, report);

        Assert.Equal("10.1234/abc", ctx.Doi);
        Assert.Equal("e2024001", ctx.ElocationId);
        Assert.Empty(GetBody(doc).Elements<Table>());
        Assert.Contains(
            report.Entries,
            e => e.Level == ReportLevel.Warn
                && e.Message.Contains("positional ELOCATION cell did not match shape", StringComparison.Ordinal));
    }

    [Fact]
    public void Apply_WhenHeaderCellUsesSoftLineBreak_DetectsHeaderInsteadOfPositionalFallback()
    {
        var idCell = new TableCell(
            new Paragraph(
                new Run(
                    new Text("id") { Space = SpaceProcessingModeValues.Preserve },
                    new Break(),
                    new Text("ART01") { Space = SpaceProcessingModeValues.Preserve })));
        var table = BuildThreeByOneTable(
            idCell,
            BuildCell("elocation", "e2024001"),
            BuildCell("doi", "10.1234/abc"));
        using var doc = CreateDocumentWith(table);
        var rule = new ExtractTopTableRule(new FormattingOptions());
        var ctx = new FormattingContext();
        var report = new Report();

        rule.Apply(doc, ctx, report);

        Assert.Equal("10.1234/abc", ctx.Doi);
        Assert.Equal("e2024001", ctx.ElocationId);
        Assert.DoesNotContain(
            report.Entries,
            e => e.Level == ReportLevel.Warn && e.Message.Contains("positional", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Apply_WhenNoDoiShapedValueAnywhere_SetsDoiToNull_AndLogsWarn()
    {
        var table = BuildThreeByOneTable(
            BuildCell("ART01"),
            BuildCell("e2024001"),
            BuildCell("not-a-doi"));
        using var doc = CreateDocumentWith(table);
        var rule = new ExtractTopTableRule(new FormattingOptions());
        var ctx = new FormattingContext();
        var report = new Report();

        rule.Apply(doc, ctx, report);

        Assert.Equal("e2024001", ctx.ElocationId);
        Assert.Null(ctx.Doi);
        Assert.Contains(
            report.Entries,
            e => e.Level == ReportLevel.Warn && e.Message.Contains("Doi=null", StringComparison.Ordinal));
    }

    [Fact]
    public void Apply_WhenElocationCellEmpty_StillSucceeds_AndLogsWarn()
    {
        var table = BuildThreeByOneTable(
            BuildCell("ART01"),
            BuildCell(string.Empty),
            BuildCell("10.1234/abc"));
        using var doc = CreateDocumentWith(table);
        var rule = new ExtractTopTableRule(new FormattingOptions());
        var ctx = new FormattingContext();
        var report = new Report();

        rule.Apply(doc, ctx, report);

        Assert.Equal(string.Empty, ctx.ElocationId);
        Assert.Equal("10.1234/abc", ctx.Doi);
        Assert.Contains(
            report.Entries,
            e => e.Level == ReportLevel.Warn && e.Message.Contains("elocation", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Apply_WhenNoTablePresent_ThrowsCriticalAbort_AndDoesNotMutate()
    {
        var paragraph = new Paragraph(new Run(new Text("not a table")));
        using var doc = CreateDocumentWith(paragraph);
        var rule = new ExtractTopTableRule(new FormattingOptions());
        var ctx = new FormattingContext();
        var report = new Report();

        var ex = Assert.Throws<InvalidOperationException>(() => rule.Apply(doc, ctx, report));

        Assert.Equal(ExtractTopTableRule.CriticalAbortMessage, ex.Message);
        Assert.Null(ctx.Doi);
        Assert.Null(ctx.ElocationId);
        Assert.Single(GetBody(doc).Elements<Paragraph>());
    }

    [Fact]
    public void Apply_WhenTopTableHasTwoColumns_ThrowsCriticalAbort_AndLeavesTableInPlace()
    {
        var table = BuildTable(2, new TableRow(BuildCell("a"), BuildCell("b")));
        using var doc = CreateDocumentWith(table);
        var rule = new ExtractTopTableRule(new FormattingOptions());
        var ctx = new FormattingContext();
        var report = new Report();

        var ex = Assert.Throws<InvalidOperationException>(() => rule.Apply(doc, ctx, report));

        Assert.Equal(ExtractTopTableRule.CriticalAbortMessage, ex.Message);
        Assert.Single(GetBody(doc).Elements<Table>());
    }

    [Fact]
    public void Apply_WhenFirstElementIsParagraph_ThrowsCriticalAbort_EvenIfTableAppearsLater()
    {
        var leadingParagraph = new Paragraph(new Run(new Text("intro paragraph")));
        var table = BuildThreeByOneTable(
            BuildCell("ART01"),
            BuildCell("e2024001"),
            BuildCell("10.1234/abc"));
        using var doc = CreateDocumentWith(leadingParagraph, table);
        var rule = new ExtractTopTableRule(new FormattingOptions());
        var ctx = new FormattingContext();
        var report = new Report();

        var ex = Assert.Throws<InvalidOperationException>(() => rule.Apply(doc, ctx, report));

        Assert.Equal(ExtractTopTableRule.CriticalAbortMessage, ex.Message);
        Assert.Single(GetBody(doc).Elements<Table>());
        Assert.Single(GetBody(doc).Elements<Paragraph>());
    }

    [Fact]
    public void Apply_WhenTableHasTwoRows_ThrowsCriticalAbort()
    {
        var table = BuildTable(
            3,
            new TableRow(BuildCell("id"), BuildCell("elocation"), BuildCell("doi")),
            new TableRow(BuildCell("ART01"), BuildCell("e2024001"), BuildCell("10.1234/abc")));
        using var doc = CreateDocumentWith(table);
        var rule = new ExtractTopTableRule(new FormattingOptions());
        var ctx = new FormattingContext();
        var report = new Report();

        var ex = Assert.Throws<InvalidOperationException>(() => rule.Apply(doc, ctx, report));

        Assert.Equal(ExtractTopTableRule.CriticalAbortMessage, ex.Message);
        Assert.Single(GetBody(doc).Elements<Table>());
    }

    [Fact]
    public void Apply_PreservesElementsFollowingTable_AfterDeletion()
    {
        var table = BuildThreeByOneTable(
            BuildCell("ART01"),
            BuildCell("e2024001"),
            BuildCell("10.1234/abc"));
        var sectionLine = new Paragraph(new Run(new Text("Original Article")));
        var titleLine = new Paragraph(new Run(new Text("Article title")));
        using var doc = CreateDocumentWith(table, sectionLine, titleLine);
        var rule = new ExtractTopTableRule(new FormattingOptions());
        var ctx = new FormattingContext();
        var report = new Report();

        rule.Apply(doc, ctx, report);

        var paragraphs = GetBody(doc).Elements<Paragraph>().ToList();
        Assert.Equal(2, paragraphs.Count);
        Assert.Equal("Original Article", string.Concat(paragraphs[0].Descendants<Text>().Select(t => t.Text)));
        Assert.Equal("Article title", string.Concat(paragraphs[1].Descendants<Text>().Select(t => t.Text)));
        Assert.Empty(GetBody(doc).Elements<Table>());
    }

    [Fact]
    public void Pipeline_WithExtractTopTableRule_OnInvalidInput_AbortsAndLeavesDocumentUntouched()
    {
        var table = BuildTable(2, new TableRow(BuildCell("a"), BuildCell("b")));
        using var doc = CreateDocumentWith(table);
        var pipeline = new FormattingPipeline(new IFormattingRule[] { new ExtractTopTableRule(new FormattingOptions()) });
        var ctx = new FormattingContext();
        var report = new Report();

        var ex = Assert.Throws<InvalidOperationException>(() => pipeline.Run(doc, ctx, report));

        Assert.Equal(ExtractTopTableRule.CriticalAbortMessage, ex.Message);
        Assert.Single(GetBody(doc).Elements<Table>());
        Assert.Null(ctx.Doi);
        Assert.Null(ctx.ElocationId);
        var errorEntry = Assert.Single(report.Entries, e => e.Level == ReportLevel.Error);
        Assert.Equal(nameof(ExtractTopTableRule), errorEntry.Rule);
        Assert.Equal(ExtractTopTableRule.CriticalAbortMessage, errorEntry.Message);
    }

    [Fact]
    public void Apply_WithNonBreakingSpaceSuffixOnElocationCell_TrimsAndStillMatches()
    {
        // Some Word source documents leave a NBSP (U+00A0) glued to the
        // ELOCATION value. Without trimming, the ^[eE]\d+$ regex misses and
        // ELOCATION falls through to empty.
        var table = BuildThreeByOneTable(
            BuildCell("ART01"),
            BuildCell("e51362627 "),
            BuildCell("10.1234/abc"));
        using var doc = CreateDocumentWith(table);
        var rule = new ExtractTopTableRule(new FormattingOptions());
        var ctx = new FormattingContext();
        var report = new Report();

        rule.Apply(doc, ctx, report);

        Assert.Equal("e51362627", ctx.ElocationId);
        Assert.DoesNotContain(
            report.Entries,
            e => e.Level == ReportLevel.Warn
                && e.Message.Contains("ELOCATION-shaped value", StringComparison.Ordinal));
    }

    [Fact]
    public void Pipeline_WithExtractTopTableRule_OnValidInput_ExtractsContextAndDeletesTable()
    {
        var table = BuildThreeByOneTable(
            BuildCell("ART01"),
            BuildCell("e2024001"),
            BuildCell("10.1234/abc"));
        var afterTable = new Paragraph(new Run(new Text("Original Article")));
        using var doc = CreateDocumentWith(table, afterTable);
        var pipeline = new FormattingPipeline(new IFormattingRule[] { new ExtractTopTableRule(new FormattingOptions()) });
        var ctx = new FormattingContext();
        var report = new Report();

        pipeline.Run(doc, ctx, report);

        Assert.Equal("e2024001", ctx.ElocationId);
        Assert.Equal("10.1234/abc", ctx.Doi);
        Assert.Empty(GetBody(doc).Elements<Table>());
        Assert.Single(GetBody(doc).Elements<Paragraph>());
    }
}
