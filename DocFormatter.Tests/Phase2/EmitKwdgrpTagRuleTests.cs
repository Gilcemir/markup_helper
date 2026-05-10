using DocFormatter.Core.Pipeline;
using DocFormatter.Core.Rules.Phase2;
using DocFormatter.Tests.Fixtures.Phase2;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Xunit;

namespace DocFormatter.Tests.Phase2;

public sealed class EmitKwdgrpTagRuleTests : IDisposable
{
    private readonly string _tempDir;

    public EmitKwdgrpTagRuleTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"emit-kwdgrp-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, recursive: true); } catch { /* best effort */ }
    }

    [Fact]
    public void Apply_GoldenPath_CommaSeparated_WrapsParagraphInKwdgrpAndDoesNotEmitKwd()
    {
        var path = WriteFixture(
            new[]
            {
                BuildPlainParagraph("Body."),
                KeywordsParagraphFactory.CreateCommaSeparated(
                    KeywordsParagraphFactory.DefaultEnglishMarker,
                    "K1", "K2", "K3"),
            });

        var (ctx, report) = Apply(path);

        var bodyText = ReadBodyText(path);
        Assert.Contains("[kwdgrp language=\"en\"]Keywords: K1, K2, K3[/kwdgrp]", bodyText);
        Assert.DoesNotContain("[kwd]", bodyText); // anti-duplication invariant
        Assert.DoesNotContain("[/kwd]", bodyText);
        Assert.NotNull(ctx.Keywords);
        Assert.Equal(new[] { "K1", "K2", "K3" }, ctx.Keywords!.Keywords);
        Assert.DoesNotContain(
            report.Entries,
            e => e.Rule == nameof(EmitKwdgrpTagRule) && e.Level == ReportLevel.Warn);
    }

    [Fact]
    public void Apply_GoldenPath_SemicolonSeparated_WrapsParagraphAndParsesKeywords()
    {
        var path = WriteFixture(
            new[]
            {
                KeywordsParagraphFactory.CreateSemicolonSeparated(
                    KeywordsParagraphFactory.DefaultEnglishMarker,
                    "K1", "K2", "K3"),
            });

        var (ctx, _) = Apply(path);

        var bodyText = ReadBodyText(path);
        Assert.Contains("[kwdgrp language=\"en\"]Keywords: K1; K2; K3[/kwdgrp]", bodyText);
        Assert.DoesNotContain("[kwd]", bodyText);
        Assert.NotNull(ctx.Keywords);
        Assert.Equal(new[] { "K1", "K2", "K3" }, ctx.Keywords!.Keywords);
    }

    [Fact]
    public void Apply_PalavrasChaveMarker_AlsoRecognized()
    {
        var path = WriteFixture(
            new[]
            {
                KeywordsParagraphFactory.CreateCommaSeparated(
                    KeywordsParagraphFactory.DefaultPortugueseMarker,
                    "K1", "K2"),
            });

        Apply(path);

        var bodyText = ReadBodyText(path);
        Assert.Contains("[kwdgrp language=\"en\"]Palavras-chave: K1, K2[/kwdgrp]", bodyText);
    }

    [Fact]
    public void Apply_NoKeywordsBlock_SkipsAndWarnsWithReasonCode()
    {
        var path = WriteFixture(
            new[]
            {
                BuildPlainParagraph("Some body."),
                BuildPlainParagraph("Another paragraph without keywords."),
            });

        var (ctx, report) = Apply(path);

        Assert.Null(ctx.Keywords);
        Assert.Contains(
            report.Entries,
            e => e.Rule == nameof(EmitKwdgrpTagRule)
                && e.Level == ReportLevel.Warn
                && e.Message == EmitKwdgrpTagRule.KeywordsBlockNotFoundMessage);

        var bodyText = ReadBodyText(path);
        Assert.DoesNotContain("[kwdgrp", bodyText);
    }

    [Fact]
    public void Apply_DoesNotEmitAntiDuplicationListLiterals()
    {
        var path = WriteFixture(
            new[]
            {
                KeywordsParagraphFactory.CreateCommaSeparated(
                    KeywordsParagraphFactory.DefaultEnglishMarker,
                    "K1", "K2", "K3"),
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
        var rule = new EmitKwdgrpTagRule();
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
