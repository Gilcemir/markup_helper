using DocFormatter.Core.Pipeline;
using DocFormatter.Core.Rules.Phase2;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Xunit;

namespace DocFormatter.Tests.Phase2;

public sealed class EmitHistTagRuleTests : IDisposable
{
    private readonly string _tempDir;

    public EmitHistTagRuleTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"emit-hist-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, recursive: true); } catch { /* best effort */ }
    }

    [Fact]
    public void Apply_GoldenPath_ReceivedAcceptedPublished_EmitsHistBlockInStrictOrder()
    {
        var path = WriteFixture(
            new[]
            {
                BuildPlainParagraph("Received: 12 March 2024"),
                BuildPlainParagraph("Accepted: 15 April 2024"),
                BuildPlainParagraph("Published: 01 May 2024"),
            });

        var (ctx, report) = Apply(path);

        var bodyText = ReadBodyText(path);
        Assert.Contains(
            "[hist]Received: [received dateiso=\"20240312\"]12 March 2024[/received]",
            bodyText);
        Assert.Contains(
            "Accepted: [accepted dateiso=\"20240415\"]15 April 2024[/accepted]",
            bodyText);
        Assert.Contains(
            "Published: [histdate dateiso=\"20240501\" datetype=\"pub\"]01 May 2024[/histdate][/hist]",
            bodyText);

        Assert.NotNull(ctx.History);
        Assert.Equal("20240312", ctx.History!.Received.ToDateIso());
        Assert.Equal("20240415", ctx.History.Accepted!.ToDateIso());
        Assert.Equal("20240501", ctx.History.Published!.ToDateIso());
        Assert.Empty(ctx.History.Revised);

        Assert.DoesNotContain(
            report.Entries,
            e => e.Rule == nameof(EmitHistTagRule) && e.Level == ReportLevel.Warn);
    }

    [Fact]
    public void Apply_GoldenPath_ReceivedOnly_EmitsHistOpenAndCloseInSameParagraph()
    {
        var path = WriteFixture(
            new[]
            {
                BuildPlainParagraph("Received: 12 March 2024"),
            });

        var (ctx, _) = Apply(path);

        var bodyText = ReadBodyText(path);
        Assert.Contains(
            "[hist]Received: [received dateiso=\"20240312\"]12 March 2024[/received][/hist]",
            bodyText);
        Assert.NotNull(ctx.History);
        Assert.Null(ctx.History!.Accepted);
        Assert.Null(ctx.History.Published);
    }

    [Fact]
    public void Apply_StrictOrdering_AcceptedEmittedBeforeHistClosingTag_OnReceivedAcceptedOnly()
    {
        var path = WriteFixture(
            new[]
            {
                BuildPlainParagraph("Received: 12 March 2024"),
                BuildPlainParagraph("Accepted: 15 April 2024"),
            });

        Apply(path);

        var bodyText = ReadBodyText(path);
        var acceptedOffset = bodyText.IndexOf("[/accepted]", StringComparison.Ordinal);
        var histCloseOffset = bodyText.IndexOf("[/hist]", StringComparison.Ordinal);
        Assert.InRange(acceptedOffset, 0, histCloseOffset);
    }

    [Fact]
    public void Apply_DateisoZeroPadding_MonthDay_PadsToTwoDigits()
    {
        var path = WriteFixture(
            new[]
            {
                BuildPlainParagraph("Received: 5 March 2024"),
                BuildPlainParagraph("Accepted: 8 April 2024"),
                BuildPlainParagraph("Published: 1 May 2024"),
            });

        Apply(path);

        var bodyText = ReadBodyText(path);
        Assert.Contains("dateiso=\"20240305\"", bodyText);
        Assert.Contains("dateiso=\"20240408\"", bodyText);
        Assert.Contains("dateiso=\"20240501\"", bodyText);
    }

    [Fact]
    public void Apply_DateisoZeroPadding_YearOnlyPublished_PadsToZeroes()
    {
        var path = WriteFixture(
            new[]
            {
                BuildPlainParagraph("Received: 12 March 2024"),
                BuildPlainParagraph("Published: 2024"),
            });

        var (ctx, _) = Apply(path);

        var bodyText = ReadBodyText(path);
        Assert.Contains("[histdate dateiso=\"20240000\" datetype=\"pub\"]2024[/histdate]", bodyText);
        Assert.Equal(2024, ctx.History!.Published!.Year);
        Assert.Null(ctx.History.Published.Month);
        Assert.Null(ctx.History.Published.Day);
    }

    [Fact]
    public void Apply_NoReceivedParagraph_SkipsBlockAndWarns_HistReceivedMissing()
    {
        var path = WriteFixture(
            new[]
            {
                BuildPlainParagraph("Body paragraph."),
                BuildPlainParagraph("Accepted: 15 April 2024"),
                BuildPlainParagraph("Published: 01 May 2024"),
            });

        var (ctx, report) = Apply(path);

        var bodyText = ReadBodyText(path);
        Assert.DoesNotContain("[hist", bodyText);
        Assert.DoesNotContain("[accepted", bodyText);
        Assert.DoesNotContain("[histdate", bodyText);
        Assert.Null(ctx.History);
        Assert.Contains(
            report.Entries,
            e => e.Rule == nameof(EmitHistTagRule)
                && e.Level == ReportLevel.Warn
                && e.Message == EmitHistTagRule.HistReceivedMissingMessage);
    }

    [Fact]
    public void Apply_ReceivedParagraphUnparseable_SkipsBlockAndWarns_HistReceivedUnparseable()
    {
        var path = WriteFixture(
            new[]
            {
                BuildPlainParagraph("Received: tomorrow morning"),
                BuildPlainParagraph("Accepted: 15 April 2024"),
                BuildPlainParagraph("Published: 01 May 2024"),
            });

        var (ctx, report) = Apply(path);

        var bodyText = ReadBodyText(path);
        Assert.DoesNotContain("[hist", bodyText);
        Assert.DoesNotContain("[received", bodyText);
        Assert.DoesNotContain("[accepted", bodyText);
        Assert.DoesNotContain("[histdate", bodyText);
        Assert.Null(ctx.History);
        Assert.Contains(
            report.Entries,
            e => e.Rule == nameof(EmitHistTagRule)
                && e.Level == ReportLevel.Warn
                && e.Message == EmitHistTagRule.HistReceivedUnparseableMessage);
    }

    [Fact]
    public void Apply_AcceptedUnparseable_EmitsHistWithReceivedOnlyAndWarns()
    {
        var path = WriteFixture(
            new[]
            {
                BuildPlainParagraph("Received: 12 March 2024"),
                BuildPlainParagraph("Accepted: maybe later"),
                BuildPlainParagraph("Published: 01 May 2024"),
            });

        var (ctx, report) = Apply(path);

        var bodyText = ReadBodyText(path);
        Assert.Contains("[hist]Received: [received dateiso=\"20240312\"]12 March 2024[/received]", bodyText);
        Assert.DoesNotContain("[accepted", bodyText);
        Assert.Contains("[histdate dateiso=\"20240501\" datetype=\"pub\"]01 May 2024[/histdate][/hist]", bodyText);

        Assert.NotNull(ctx.History);
        Assert.Null(ctx.History!.Accepted);
        Assert.NotNull(ctx.History.Published);
        Assert.Contains(
            report.Entries,
            e => e.Rule == nameof(EmitHistTagRule)
                && e.Level == ReportLevel.Warn
                && e.Message == EmitHistTagRule.HistAcceptedUnparseableMessage);
    }

    [Fact]
    public void Apply_TwoRevisions_DocumentOrderRevisedBetweenReceivedAndAccepted_NotEmittedYet()
    {
        // Phase 4 corpus has no `Revised` paragraphs. The rule still
        // documents the ordering invariant by ensuring that when only the
        // three canonical markers are present, [/accepted] appears before
        // [/hist] (i.e. no rewiring of children moves accepted past the
        // hist closing).
        var path = WriteFixture(
            new[]
            {
                BuildPlainParagraph("Received: 12 March 2024"),
                BuildPlainParagraph("Accepted: 15 April 2024"),
            });

        Apply(path);

        var bodyText = ReadBodyText(path);
        var receivedClose = bodyText.IndexOf("[/received]", StringComparison.Ordinal);
        var acceptedOpen = bodyText.IndexOf("[accepted", StringComparison.Ordinal);
        var acceptedClose = bodyText.IndexOf("[/accepted]", StringComparison.Ordinal);
        var histClose = bodyText.IndexOf("[/hist]", StringComparison.Ordinal);

        Assert.InRange(receivedClose, 0, acceptedOpen);
        Assert.InRange(acceptedOpen, 0, acceptedClose);
        Assert.InRange(acceptedClose, 0, histClose);
    }

    [Fact]
    public void Apply_DoesNotEmitAntiDuplicationLiterals()
    {
        var path = WriteFixture(
            new[]
            {
                BuildPlainParagraph("Received: 12 March 2024"),
                BuildPlainParagraph("Accepted: 15 April 2024"),
                BuildPlainParagraph("Published: 01 May 2024"),
            });

        Apply(path);

        var bodyText = ReadBodyText(path);
        EmitElocationTagRuleTests.AssertNoAntiDuplicationLiterals(bodyText);
    }

    [Fact]
    public void Apply_PreExistingHistLiteral_IsIdempotent_NoDoubleWrap()
    {
        var path = WriteFixture(
            new[]
            {
                BuildPlainParagraph("[hist]Received: [received dateiso=\"20240312\"]12 March 2024[/received][/hist]"),
            });

        Apply(path);

        var bodyText = ReadBodyText(path);
        Assert.Equal(1, CountOccurrences(bodyText, "[hist]"));
    }

    [Fact]
    public void Apply_BodyIsNull_SkipsAndWarns()
    {
        // Build a doc whose main part has no Document/Body and confirm the
        // rule still skip-and-warns instead of throwing.
        var path = Path.Combine(_tempDir, $"{Guid.NewGuid():N}.docx");
        using (var doc = WordprocessingDocument.Create(path, WordprocessingDocumentType.Document))
        {
            var mainPart = doc.AddMainDocumentPart();
            mainPart.Document = new Document();
        }

        var ctx = new FormattingContext();
        var report = new Report();
        using (var doc = WordprocessingDocument.Open(path, isEditable: true))
        {
            new EmitHistTagRule().Apply(doc, ctx, report);
        }

        Assert.Null(ctx.History);
        Assert.Contains(
            report.Entries,
            e => e.Rule == nameof(EmitHistTagRule)
                && e.Level == ReportLevel.Warn
                && e.Message == EmitHistTagRule.DocumentBodyMissingMessage);
    }

    [Fact]
    public void Apply_NoCandidatesAtAll_SkipsAndWarnsHistReceivedMissing()
    {
        // The corpus / Phase 1 MoveHistoryRule uses English markers
        // (`Received`/`Accepted`/`Published`); the rule's locator follows the
        // same convention. A document whose history phrasing is not in that
        // shape (e.g. Portuguese or stripped to bare dates) skips the entire
        // block.
        var path = WriteFixture(
            new[]
            {
                BuildPlainParagraph("Recebido em 12 de março de 2024"),
                BuildPlainParagraph("Aceito em 15 de abril de 2024"),
            });

        var (ctx, report) = Apply(path);

        Assert.Null(ctx.History);
        var bodyText = ReadBodyText(path);
        Assert.DoesNotContain("[hist", bodyText);
        Assert.Contains(
            report.Entries,
            e => e.Rule == nameof(EmitHistTagRule)
                && e.Level == ReportLevel.Warn
                && e.Message == EmitHistTagRule.HistReceivedMissingMessage);
    }

    private (FormattingContext Ctx, IReport Report) Apply(string path)
    {
        var ctx = new FormattingContext();
        var report = new Report();
        using var doc = WordprocessingDocument.Open(path, isEditable: true);
        var rule = new EmitHistTagRule();
        rule.Apply(doc, ctx, report);
        return (ctx, report);
    }

    private static int CountOccurrences(string haystack, string needle)
    {
        var count = 0;
        var idx = 0;
        while ((idx = haystack.IndexOf(needle, idx, StringComparison.Ordinal)) >= 0)
        {
            count++;
            idx += needle.Length;
        }
        return count;
    }

    private static Paragraph BuildPlainParagraph(string text)
        => new(new Run(new Text(text) { Space = SpaceProcessingModeValues.Preserve }));

    private string WriteFixture(IEnumerable<Paragraph> paragraphs)
    {
        var path = Path.Combine(_tempDir, $"{Guid.NewGuid():N}.docx");
        using var doc = WordprocessingDocument.Create(path, WordprocessingDocumentType.Document);
        var mainPart = doc.AddMainDocumentPart();
        var body = new Body();
        foreach (var p in paragraphs)
        {
            body.AppendChild(p);
        }
        mainPart.Document = new Document(body);
        return path;
    }

    private static string ReadBodyText(string path)
    {
        using var doc = WordprocessingDocument.Open(path, isEditable: false);
        var body = doc.MainDocumentPart!.Document!.Body!;
        var lines = new List<string>();
        foreach (var p in body.Elements<Paragraph>())
        {
            var text = string.Concat(p.Descendants<Text>().Select(t => t.Text));
            lines.Add(text);
        }
        return string.Join("\n", lines);
    }
}
