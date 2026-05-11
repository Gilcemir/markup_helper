using DocFormatter.Core.Pipeline;
using DocFormatter.Core.Rules.Phase2;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Xunit;

namespace DocFormatter.Tests.Phase2;

public sealed class EmitAuthorXrefsRuleTests : IDisposable
{
    private readonly string _tempDir;

    public EmitAuthorXrefsRuleTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"emit-author-xrefs-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, recursive: true); } catch { /* best effort */ }
    }

    [Fact]
    public void Apply_AuthorWithSingleAff_AddsRidAndCorrespNoAndDeceasedAttrs()
    {
        var path = WriteFixture(new[]
        {
            BuildPlainParagraph(
                "[author role=\"nd\"][fname]A[/fname] [surname]B[/surname]"
                + "[xref ref-type=\"aff\" rid=\"aff1\"]1 [/xref]0000-0000-0000-0001[/author]"),
        });

        var (ctx, _) = Apply(path);

        var bodyText = ReadBodyText(path);
        Assert.Contains(
            "[author role=\"nd\" rid=\"aff1\" corresp=\"n\" deceased=\"n\" eqcontr=\"nd\"]",
            bodyText);
        Assert.Contains("[xref ref-type=\"aff\" rid=\"aff1\"]1 [/xref]", bodyText);
        Assert.Contains("[authorid authidtp=\"orcid\"]0000-0000-0000-0001[/authorid]", bodyText);
        Assert.Single(ctx.Authors);
        Assert.Equal(new[] { "aff1" }, ctx.Authors[0].AffiliationLabels);
        Assert.Equal("0000-0000-0000-0001", ctx.Authors[0].OrcidId);
    }

    [Fact]
    public void Apply_AuthorWithTwoAffs_AddsSpaceSeparatedRid()
    {
        var path = WriteFixture(new[]
        {
            BuildPlainParagraph(
                "[author role=\"nd\"][fname]A[/fname] [surname]B[/surname]"
                + "[xref ref-type=\"aff\" rid=\"aff1\"]1[/xref]"
                + "[xref ref-type=\"aff\" rid=\"aff2\"]2[/xref] 0000-0000-0000-0002[/author]"),
        });

        Apply(path);

        var bodyText = ReadBodyText(path);
        Assert.Contains(
            "[author role=\"nd\" rid=\"aff1 aff2\" corresp=\"n\" deceased=\"n\" eqcontr=\"nd\"]",
            bodyText);
    }

    [Fact]
    public void Apply_CorrespAuthor_PlainAsteriskTrailer_ConvertedToTwoXrefsAndCorrespY()
    {
        var path = WriteFixture(new[]
        {
            BuildPlainParagraph(
                "[author role=\"nd\"][fname]A[/fname] [surname]B[/surname]1*0000-0000-0000-0003[/author]"),
        });

        var (ctx, _) = Apply(path);

        var bodyText = ReadBodyText(path);
        Assert.Contains("[xref ref-type=\"aff\" rid=\"aff1\"]1[/xref]", bodyText);
        Assert.Contains("[xref ref-type=\"corresp\" rid=\"c1\"]*[/xref]", bodyText);
        Assert.Contains(" corresp=\"y\" ", bodyText);
        Assert.Equal(0, ctx.CorrespondingAuthorIndex);
    }

    [Fact]
    public void Apply_CorrespAuthor_TwoAffsCommaAsteriskTrailer_ConvertedToThreeXrefsAndPreservesComma()
    {
        var path = WriteFixture(new[]
        {
            BuildPlainParagraph(
                "[author role=\"nd\"][fname]A[/fname] [surname]B[/surname]1,2* 0000-0000-0000-0004[/author]"),
        });

        Apply(path);

        var bodyText = ReadBodyText(path);
        Assert.Contains("[xref ref-type=\"aff\" rid=\"aff1\"]1[/xref],[xref ref-type=\"aff\" rid=\"aff2\"]2[/xref][xref ref-type=\"corresp\" rid=\"c1\"]*[/xref]", bodyText);
    }

    [Fact]
    public void Apply_OrcidWrap_PreservesAlreadyWrappedAuthorid_DoesNotDoubleWrap()
    {
        var path = WriteFixture(new[]
        {
            BuildPlainParagraph(
                "[author role=\"nd\"][fname]A[/fname] [surname]B[/surname]"
                + "[xref ref-type=\"aff\" rid=\"aff1\"]1[/xref]"
                + "[authorid authidtp=\"orcid\"]0000-0000-0000-0005[/authorid][/author]"),
        });

        Apply(path);

        var bodyText = ReadBodyText(path);
        var openings = 0;
        var idx = 0;
        while ((idx = bodyText.IndexOf("[authorid", idx, StringComparison.Ordinal)) >= 0)
        {
            openings++;
            idx += "[authorid".Length;
        }
        Assert.Equal(1, openings);
    }

    [Fact]
    public void Apply_AuthorWithoutOrcid_DoesNotEmitAuthorid()
    {
        var path = WriteFixture(new[]
        {
            BuildPlainParagraph(
                "[author role=\"nd\"][fname]A[/fname] [surname]B[/surname]"
                + "[xref ref-type=\"aff\" rid=\"aff1\"]1[/xref][/author]"),
        });

        var (ctx, _) = Apply(path);

        var bodyText = ReadBodyText(path);
        Assert.DoesNotContain("[authorid", bodyText);
        Assert.Null(ctx.Authors[0].OrcidId);
    }

    [Fact]
    public void Apply_NoAuthorParagraphs_SkipsAndWarnsWithReasonCode()
    {
        var path = WriteFixture(new[]
        {
            BuildPlainParagraph("Just body text."),
            BuildPlainParagraph("[xmlabstr language=\"en\"]Abstract content[/xmlabstr]"),
        });

        var (ctx, report) = Apply(path);

        Assert.Empty(ctx.Authors);
        Assert.Contains(
            report.Entries,
            e => e.Rule == nameof(EmitAuthorXrefsRule)
                && e.Level == ReportLevel.Warn
                && e.Message == EmitAuthorXrefsRule.AuthorsMissingMessage);
    }

    [Fact]
    public void Apply_PlainTextAuthorParagraph_WrapsOrcidAndExpandsCorrespMarker()
    {
        var path = WriteFixture(new[]
        {
            BuildPlainParagraph("Author Name1*0000-0000-0000-0006"),
        });

        Apply(path);

        var bodyText = ReadBodyText(path);
        Assert.Contains("[xref ref-type=\"aff\" rid=\"aff1\"]1[/xref]", bodyText);
        Assert.Contains("[xref ref-type=\"corresp\" rid=\"c1\"]*[/xref]", bodyText);
        Assert.Contains("[authorid authidtp=\"orcid\"]0000-0000-0000-0006[/authorid]", bodyText);
    }

    [Fact]
    public void Apply_DoesNotEmitAntiDuplicationListLiterals()
    {
        var path = WriteFixture(new[]
        {
            BuildPlainParagraph(
                "[author role=\"nd\"][fname]A[/fname] [surname]B[/surname]"
                + "[xref ref-type=\"aff\" rid=\"aff1\"]1[/xref] 0000-0000-0000-0007[/author]"),
        });

        Apply(path);

        var bodyText = ReadBodyText(path);
        // The fixture deliberately seeds [author], [fname], [surname] (those
        // existed in BEFORE) — the assertion focuses on the rule NOT
        // introducing additional copies.
        Assert.Equal(1, CountOccurrences(bodyText, "[author role=\"nd\""));
        Assert.Equal(1, CountOccurrences(bodyText, "[/author]"));
        Assert.Equal(1, CountOccurrences(bodyText, "[fname]"));
        Assert.Equal(1, CountOccurrences(bodyText, "[surname]"));
        Assert.DoesNotContain("[normaff", bodyText);
        Assert.DoesNotContain("[kwd]", bodyText);
    }

    [Fact]
    public void Apply_UnicodeSuperscriptAffLabel_WrapsAsXrefAff()
    {
        var path = WriteFixture(new[]
        {
            BuildPlainParagraph(
                "[author role=\"nd\"]Author Name¹ 0000-0000-0000-0008[/author]"),
        });

        Apply(path);

        var bodyText = ReadBodyText(path);
        Assert.Contains("[xref ref-type=\"aff\" rid=\"aff1\"]¹[/xref]", bodyText);
        Assert.Contains("[authorid authidtp=\"orcid\"]0000-0000-0000-0008[/authorid]", bodyText);
    }

    [Fact]
    public void Apply_RewrittenAuthorParagraph_PutsEachTagLiteralInItsOwnRun()
    {
        var path = WriteFixture(new[]
        {
            BuildPlainParagraph(
                "[author role=\"nd\"][fname]A[/fname] [surname]B[/surname]"
                + "[xref ref-type=\"aff\" rid=\"aff1\"]1[/xref]* 0000-0000-0000-0001[/author]"),
        });

        Apply(path);

        var runTexts = ReadRunTexts(path);

        // Each emitted tag literal must be a standalone Run (the VBA macro
        // `color(tag)` assigns a per-tag color; collapsing tags into shared
        // Runs would paint the whole line one color).
        Assert.Contains(runTexts, r => r.StartsWith("[author", StringComparison.Ordinal) && !r.Contains("[fname", StringComparison.Ordinal));
        Assert.Contains(runTexts, r => r.StartsWith("[xref", StringComparison.Ordinal));
        Assert.Contains(runTexts, r => r == "[/xref]");
        Assert.Contains(runTexts, r => r == "[authorid authidtp=\"orcid\"]");
        Assert.Contains(runTexts, r => r == "[/authorid]");
        Assert.Contains(runTexts, r => r == "[/author]");

        // No Run should mix a tag literal with the next plain-text segment.
        foreach (var r in runTexts)
        {
            if (r.StartsWith('[') && r.Contains(']') && r.IndexOf(']') < r.Length - 1)
            {
                Assert.Fail($"run mixes a tag literal with adjacent text: '{r}'");
            }
        }
    }

    private static List<string> ReadRunTexts(string path)
    {
        using var doc = WordprocessingDocument.Open(path, isEditable: false);
        var body = doc.MainDocumentPart!.Document!.Body!;
        var runs = new List<string>();
        foreach (var paragraph in body.Elements<Paragraph>())
        {
            foreach (var run in paragraph.Elements<Run>())
            {
                var text = string.Concat(run.Descendants<Text>().Select(t => t.Text));
                if (text.Length > 0)
                {
                    runs.Add(text);
                }
            }
        }
        return runs;
    }

    private (FormattingContext Ctx, IReport Report) Apply(string path)
    {
        var ctx = new FormattingContext();
        var report = new Report();
        using var doc = WordprocessingDocument.Open(path, isEditable: true);
        new EmitAuthorXrefsRule().Apply(doc, ctx, report);
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
