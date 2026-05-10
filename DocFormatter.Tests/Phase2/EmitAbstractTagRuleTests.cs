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
    public void Apply_GoldenPath_WrapsHeadingAndBodyParagraphsInXmlabstr()
    {
        var path = WriteFixture(
            new[]
            {
                BuildPlainParagraph("Abstract"),
                BuildPlainParagraph("Body of the abstract."),
            });

        var (ctx, report) = Apply(path);

        var bodyText = ReadBodyText(path);
        Assert.StartsWith("[xmlabstr language=\"en\"]Abstract", bodyText);
        Assert.EndsWith("Body of the abstract.[/xmlabstr]", bodyText);
        Assert.NotNull(ctx.Abstract);
        Assert.Equal("en", ctx.Abstract!.Language);
        Assert.DoesNotContain(
            report.Entries,
            e => e.Rule == nameof(EmitAbstractTagRule) && e.Level == ReportLevel.Warn);
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
        Assert.Contains("[xmlabstr language=\"en\"]Resumo", bodyText);
        Assert.Contains("Corpo do resumo.[/xmlabstr]", bodyText);
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
        Assert.Contains("[xmlabstr language=\"en\"]Abstract", bodyText);
        Assert.Contains("Real body text.[/xmlabstr]", bodyText);
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
