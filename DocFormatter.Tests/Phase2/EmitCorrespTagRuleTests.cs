using DocFormatter.Core.Pipeline;
using DocFormatter.Core.Rules.Phase2;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Xunit;

namespace DocFormatter.Tests.Phase2;

public sealed class EmitCorrespTagRuleTests : IDisposable
{
    private readonly string _tempDir;

    public EmitCorrespTagRuleTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"emit-corresp-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, recursive: true); } catch { /* best effort */ }
    }

    [Fact]
    public void Apply_GoldenPath_AsteriskEmail_WrapsParagraphInCorrespTag()
    {
        var path = WriteFixture(
            new[]
            {
                BuildPlainParagraph("Body paragraph"),
                BuildPlainParagraph("* E-mail: x@y.com"),
                BuildPlainParagraph("After"),
            });

        var (ctx, report) = Apply(path);

        var bodyText = ReadBodyText(path);
        Assert.Contains("[corresp id=\"c1\"]* E-mail: x@y.com[/corresp]", bodyText);
        Assert.NotNull(ctx.CorrespAuthor);
        Assert.Equal("x@y.com", ctx.CorrespAuthor!.Email);
        Assert.DoesNotContain(
            report.Entries,
            e => e.Rule == nameof(EmitCorrespTagRule) && e.Level == ReportLevel.Warn);
    }

    [Fact]
    public void Apply_GoldenPath_CorrespondingAuthorMarker_WrapsParagraphInCorrespTag()
    {
        var path = WriteFixture(
            new[]
            {
                BuildPlainParagraph("Corresponding author: foo@bar.com"),
            });

        var (ctx, _) = Apply(path);

        var bodyText = ReadBodyText(path);
        Assert.Contains("[corresp id=\"c1\"]Corresponding author: foo@bar.com[/corresp]", bodyText);
        Assert.NotNull(ctx.CorrespAuthor);
        Assert.Equal("foo@bar.com", ctx.CorrespAuthor!.Email);
    }

    [Fact]
    public void Apply_NoCorrespParagraph_SkipsAndWarnsWithReasonCode()
    {
        var path = WriteFixture(
            new[]
            {
                BuildPlainParagraph("Some body."),
                BuildPlainParagraph("Another paragraph."),
            });

        var (ctx, report) = Apply(path);

        Assert.Null(ctx.CorrespAuthor);
        Assert.Contains(
            report.Entries,
            e => e.Rule == nameof(EmitCorrespTagRule)
                && e.Level == ReportLevel.Warn
                && e.Message == EmitCorrespTagRule.CorrespBlockNotFoundMessage);

        var bodyText = ReadBodyText(path);
        Assert.DoesNotContain("[corresp", bodyText);
    }

    [Fact]
    public void Apply_PreExistingCorrespLiteralInDocument_IsIdempotent_NoDoubleWrap()
    {
        var path = WriteFixture(
            new[]
            {
                BuildPlainParagraph("[corresp id=\"c1\"]* E-mail: x@y.com[/corresp]"),
            });

        var (_, report) = Apply(path);

        var bodyText = ReadBodyText(path);
        // Exactly one [corresp opening: re-running the rule must not nest.
        Assert.Equal(1, CountOccurrences(bodyText, "[corresp id=\"c1\"]"));
        Assert.Contains(
            report.Entries,
            e => e.Rule == nameof(EmitCorrespTagRule)
                && e.Level == ReportLevel.Warn
                && e.Message == EmitCorrespTagRule.CorrespBlockNotFoundMessage);
    }

    [Fact]
    public void Apply_DoesNotEmitAntiDuplicationListLiterals()
    {
        var path = WriteFixture(
            new[]
            {
                BuildPlainParagraph("* E-mail: x@y.com"),
            });

        Apply(path);

        var bodyText = ReadBodyText(path);
        EmitElocationTagRuleTests.AssertNoAntiDuplicationLiterals(bodyText);
    }

    [Fact]
    public void Apply_PopulatesCorrespAuthorFromCtx_WhenPhase1AlreadyExtractedIndex()
    {
        var path = WriteFixture(
            new[]
            {
                BuildPlainParagraph("* E-mail: x@y.com"),
            });

        var ctx = new FormattingContext
        {
            CorrespondingAuthorIndex = 2,
            CorrespondingOrcid = "0000-0000-0000-0001",
        };
        var report = new Report();
        using var doc = WordprocessingDocument.Open(path, isEditable: true);
        new EmitCorrespTagRule().Apply(doc, ctx, report);

        Assert.NotNull(ctx.CorrespAuthor);
        Assert.Equal(2, ctx.CorrespAuthor!.AuthorIndex);
        Assert.Equal("0000-0000-0000-0001", ctx.CorrespAuthor.Orcid);
        Assert.Equal("x@y.com", ctx.CorrespAuthor.Email);
    }

    private (FormattingContext Ctx, IReport Report) Apply(string path)
    {
        var ctx = new FormattingContext();
        var report = new Report();
        using var doc = WordprocessingDocument.Open(path, isEditable: true);
        var rule = new EmitCorrespTagRule();
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
