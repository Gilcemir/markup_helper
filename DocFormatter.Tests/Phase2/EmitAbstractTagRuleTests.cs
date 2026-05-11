using DocFormatter.Core.Options;
using DocFormatter.Core.Pipeline;
using DocFormatter.Core.Rules.Phase2;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Xunit;

namespace DocFormatter.Tests.Phase2;

public sealed class EmitAbstractTagRuleTests : IDisposable
{
    private readonly string _tempDir;

    public EmitAbstractTagRuleTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"emit-abstract-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, recursive: true); } catch { /* best effort */ }
    }

    [Fact]
    public void Apply_GoldenPath_WrapsHeadingInSectitleAndBodyInP()
    {
        var path = WriteFixture(
            new[]
            {
                BuildPlainParagraph("Abstract"),
                BuildPlainParagraph("Body of the abstract."),
            });

        var (ctx, report) = Apply(path);

        var bodyText = ReadBodyText(path);
        Assert.Contains("[xmlabstr language=\"en\"][sectitle]Abstract[/sectitle]", bodyText);
        Assert.Contains("[p]Body of the abstract.[/p][/xmlabstr]", bodyText);
        Assert.NotNull(ctx.Abstract);
        Assert.Equal("en", ctx.Abstract!.Language);
        Assert.DoesNotContain(
            report.Entries,
            e => e.Rule == nameof(EmitAbstractTagRule) && e.Level == ReportLevel.Warn);

        AssertEachTagLiteralIsOwnRun(path);
    }

    [Fact]
    public void Apply_ResumoHeading_WrapsThePortugueseAbstractInXmlabstr()
    {
        var path = WriteFixture(
            new[]
            {
                BuildPlainParagraph("Resumo"),
                BuildPlainParagraph("Corpo do resumo."),
            });

        Apply(path);

        var bodyText = ReadBodyText(path);
        Assert.Contains("[xmlabstr language=\"en\"][sectitle]Resumo[/sectitle]", bodyText);
        Assert.Contains("[p]Corpo do resumo.[/p][/xmlabstr]", bodyText);
    }

    [Fact]
    public void Apply_MultiParagraphBody_EmitsOnePPerParagraphAndStopsAtKeywords()
    {
        var path = WriteFixture(
            new[]
            {
                BuildPlainParagraph("Abstract"),
                BuildPlainParagraph("First paragraph of the abstract."),
                BuildPlainParagraph("Second paragraph."),
                BuildPlainParagraph("Keywords: K1, K2"),
                BuildPlainParagraph("After keywords."),
            });

        Apply(path);

        var bodyText = ReadBodyText(path);
        Assert.Contains("[xmlabstr language=\"en\"][sectitle]Abstract[/sectitle]", bodyText);
        Assert.Contains("[p]First paragraph of the abstract.[/p]", bodyText);
        Assert.Contains("[p]Second paragraph.[/p][/xmlabstr]", bodyText);
        // The keywords paragraph and anything past it must NOT be wrapped.
        Assert.DoesNotContain("[p]Keywords:", bodyText);
        Assert.DoesNotContain("[p]After keywords.", bodyText);
    }

    [Fact]
    public void Apply_NoAbstractHeading_SkipsAndWarnsWithReasonCode()
    {
        var path = WriteFixture(
            new[]
            {
                BuildPlainParagraph("Introduction"),
                BuildPlainParagraph("Some body text"),
            });

        var (ctx, report) = Apply(path);

        Assert.Null(ctx.Abstract);
        Assert.Contains(
            report.Entries,
            e => e.Rule == nameof(EmitAbstractTagRule)
                && e.Level == ReportLevel.Warn
                && e.Message == EmitAbstractTagRule.AbstractHeadingNotFoundMessage);

        var bodyText = ReadBodyText(path);
        Assert.DoesNotContain("[xmlabstr", bodyText);
    }

    [Fact]
    public void Apply_AbstractHeadingButNoFollowingBody_SkipsAndWarnsWithReasonCode()
    {
        var path = WriteFixture(
            new[]
            {
                BuildPlainParagraph("Abstract"),
                // no body paragraph
            });

        var (ctx, report) = Apply(path);

        Assert.Null(ctx.Abstract);
        Assert.Contains(
            report.Entries,
            e => e.Rule == nameof(EmitAbstractTagRule)
                && e.Level == ReportLevel.Warn
                && e.Message == EmitAbstractTagRule.AbstractBodyNotFoundMessage);
    }

    [Fact]
    public void Apply_AbstractHeadingFollowedByEmptyParagraphThenBody_WrapsAroundTheRealBody()
    {
        var path = WriteFixture(
            new[]
            {
                BuildPlainParagraph("Abstract"),
                BuildPlainParagraph("   "),         // empty whitespace paragraph
                BuildPlainParagraph("Real body text."),
            });

        Apply(path);

        var bodyText = ReadBodyText(path);
        Assert.Contains("[xmlabstr language=\"en\"][sectitle]Abstract[/sectitle]", bodyText);
        Assert.Contains("[p]Real body text.[/p][/xmlabstr]", bodyText);
    }

    [Fact]
    public void Apply_NonHeadingAbstractPrefixBeforeRealHeading_StillWrapsRealHeading()
    {
        // A paragraph that starts with "Abstract" but continues into a longer
        // sentence ("Abstract submission deadline ...") must not abort the
        // scan — the real heading sits later. Regression for issue 001.
        var path = WriteFixture(
            new[]
            {
                BuildPlainParagraph("Abstract submission deadline: 2026-01-15."),
                BuildPlainParagraph("Abstract"),
                BuildPlainParagraph("Real body of the abstract."),
            });

        var (ctx, report) = Apply(path);

        var bodyText = ReadBodyText(path);
        Assert.Contains("[xmlabstr language=\"en\"][sectitle]Abstract[/sectitle]", bodyText);
        Assert.Contains("[p]Real body of the abstract.[/p][/xmlabstr]", bodyText);
        Assert.NotNull(ctx.Abstract);
        Assert.DoesNotContain(
            report.Entries,
            e => e.Rule == nameof(EmitAbstractTagRule) && e.Level == ReportLevel.Warn);
    }

    [Fact]
    public void Apply_DoesNotEmitAntiDuplicationListLiterals()
    {
        var path = WriteFixture(
            new[]
            {
                BuildPlainParagraph("Abstract"),
                BuildPlainParagraph("Body."),
            });

        Apply(path);

        var bodyText = ReadBodyText(path);
        EmitElocationTagRuleTests.AssertNoAntiDuplicationLiterals(bodyText);
    }

    private (FormattingContext Ctx, IReport Report) Apply(string path)
    {
        var ctx = new FormattingContext();
        var report = new Report();
        using var doc = WordprocessingDocument.Open(path, isEditable: true);
        var rule = new EmitAbstractTagRule(new FormattingOptions());
        rule.Apply(doc, ctx, report);
        return (ctx, report);
    }

    private static Paragraph BuildPlainParagraph(string text)
        => new(new Run(new Text(text) { Space = SpaceProcessingModeValues.Preserve }));

    private static void AssertEachTagLiteralIsOwnRun(string path)
    {
        using var doc = WordprocessingDocument.Open(path, isEditable: false);
        var body = doc.MainDocumentPart!.Document!.Body!;
        var runTexts = new List<string>();
        foreach (var paragraph in body.Elements<Paragraph>())
        {
            foreach (var run in paragraph.Elements<Run>())
            {
                var text = string.Concat(run.Descendants<Text>().Select(t => t.Text));
                if (text.Length > 0)
                {
                    runTexts.Add(text);
                }
            }
        }

        Assert.Contains(runTexts, r => r.StartsWith("[xmlabstr", StringComparison.Ordinal) && !r.Contains("[sectitle", StringComparison.Ordinal));
        Assert.Contains(runTexts, r => r == "[sectitle]");
        Assert.Contains(runTexts, r => r == "[/sectitle]");
        Assert.Contains(runTexts, r => r == "[p]");
        Assert.Contains(runTexts, r => r == "[/p]");
        Assert.Contains(runTexts, r => r == "[/xmlabstr]");
    }

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
