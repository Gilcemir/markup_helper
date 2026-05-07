using DocFormatter.Core.Options;
using DocFormatter.Core.Pipeline;
using DocFormatter.Core.Rules;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Xunit;

namespace DocFormatter.Tests;

public sealed class RewriteAbstractRuleTests
{
    private static RewriteAbstractRule CreateRule()
        => new(new FormattingOptions());

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

    private static Paragraph PlainParagraph(string text)
        => new(new Run(new Text(text) { Space = SpaceProcessingModeValues.Preserve }));

    private static Run TextRun(string text)
        => new(new Text(text) { Space = SpaceProcessingModeValues.Preserve });

    private static Run ItalicTextRun(string text)
    {
        return new Run(
            new RunProperties(new Italic()),
            new Text(text) { Space = SpaceProcessingModeValues.Preserve });
    }

    private static string ParagraphText(Paragraph p)
        => string.Concat(p.Descendants<Text>().Select(t => t.Text));

    private static bool IsHeadingParagraph(Paragraph p)
    {
        var run = p.Elements<Run>().FirstOrDefault();
        if (run is null)
        {
            return false;
        }

        var bold = run.RunProperties?.GetFirstChild<Bold>();
        if (bold is null)
        {
            return false;
        }

        var text = string.Concat(run.Descendants<Text>().Select(t => t.Text));
        return text == "Abstract";
    }

    [Fact]
    public void Apply_WithUniformItalic_StripsItalicAndSplitsIntoTwoParagraphs()
    {
        var abstractPara = new Paragraph(
            ItalicTextRun("Abstract - lorem ipsum"));

        using var doc = CreateDocumentWith(abstractPara);

        var ctx = new FormattingContext();
        var report = new Report();

        CreateRule().Apply(doc, ctx, report);

        var paragraphs = GetBody(doc).Elements<Paragraph>().ToList();
        Assert.Equal(2, paragraphs.Count);
        Assert.True(IsHeadingParagraph(paragraphs[0]));
        Assert.Same(abstractPara, paragraphs[1]);
        Assert.Equal("lorem ipsum", ParagraphText(abstractPara));

        foreach (var run in abstractPara.Descendants<Run>())
        {
            Assert.Null(run.RunProperties?.Italic);
        }

        Assert.Contains(
            report.Entries,
            e => e.Level == ReportLevel.Info
                 && e.Message == RewriteAbstractRule.StructuralItalicRemovedMessage);
    }

    [Fact]
    public void Apply_WithMixedItalic_PreservesRunLevelItalic()
    {
        var abstractPara = new Paragraph(
            ItalicTextRun("Abstract - lorem "),
            ItalicTextRun("Aedes aegypti"),
            TextRun(" more text"));

        using var doc = CreateDocumentWith(abstractPara);

        var ctx = new FormattingContext();
        var report = new Report();

        CreateRule().Apply(doc, ctx, report);

        var paragraphs = GetBody(doc).Elements<Paragraph>().ToList();
        Assert.Equal(2, paragraphs.Count);
        Assert.True(IsHeadingParagraph(paragraphs[0]));
        Assert.Same(abstractPara, paragraphs[1]);
        Assert.Equal("lorem Aedes aegypti more text", ParagraphText(abstractPara));

        var bodyRuns = abstractPara.Elements<Run>().ToList();
        Assert.Equal(3, bodyRuns.Count);
        Assert.NotNull(bodyRuns[0].RunProperties?.Italic);
        Assert.NotNull(bodyRuns[1].RunProperties?.Italic);
        Assert.Null(bodyRuns[2].RunProperties?.Italic);

        Assert.DoesNotContain(
            report.Entries,
            e => e.Message == RewriteAbstractRule.StructuralItalicRemovedMessage);
    }

    [Fact]
    public void Apply_WithResumoMarker_NormalizesHeadingToAbstractAndKeepsBodyLanguage()
    {
        var abstractPara = new Paragraph(TextRun("Resumo - corpo em português"));

        using var doc = CreateDocumentWith(abstractPara);

        var ctx = new FormattingContext();
        var report = new Report();

        CreateRule().Apply(doc, ctx, report);

        var paragraphs = GetBody(doc).Elements<Paragraph>().ToList();
        Assert.Equal(2, paragraphs.Count);
        Assert.True(IsHeadingParagraph(paragraphs[0]));
        Assert.Same(abstractPara, paragraphs[1]);
        Assert.Equal("corpo em português", ParagraphText(abstractPara));

        Assert.Contains(
            report.Entries,
            e => e.Level == ReportLevel.Info
                 && e.Message == RewriteAbstractRule.ResumoNormalizedMessage);
    }

    [Fact]
    public void Apply_WithEmailAndNoTypedLine_InsertsCanonicalParagraphAboveHeading()
    {
        var author = PlainParagraph("Maria Silva");
        var affiliation = PlainParagraph("1 University");
        var abstractPara = new Paragraph(TextRun("Abstract - body text"));

        using var doc = CreateDocumentWith(author, affiliation, abstractPara);

        var ctx = new FormattingContext { CorrespondingEmail = "foo@x.com" };
        ctx.AuthorParagraphs.Add(author);
        var report = new Report();

        CreateRule().Apply(doc, ctx, report);

        var paragraphs = GetBody(doc).Elements<Paragraph>().ToList();
        Assert.Equal(5, paragraphs.Count);
        Assert.Same(author, paragraphs[0]);
        Assert.Same(affiliation, paragraphs[1]);
        Assert.Equal("Corresponding author: foo@x.com", ParagraphText(paragraphs[2]));
        Assert.True(IsHeadingParagraph(paragraphs[3]));
        Assert.Same(abstractPara, paragraphs[4]);

        Assert.Contains(
            report.Entries,
            e => e.Level == ReportLevel.Info
                 && e.Message == RewriteAbstractRule.CanonicalLineInsertedMessage);
    }

    [Fact]
    public void Apply_WithEmailAndCanonicalTypedLine_ReplacesTypedLineWithCanonical()
    {
        var author = PlainParagraph("Maria Silva");
        var affiliation = PlainParagraph("1 University");
        var typedLine = PlainParagraph("Corresponding Author: foo@x.com");
        var abstractPara = new Paragraph(TextRun("Abstract - body text"));

        using var doc = CreateDocumentWith(author, affiliation, typedLine, abstractPara);

        var ctx = new FormattingContext { CorrespondingEmail = "foo@x.com" };
        ctx.AuthorParagraphs.Add(author);
        var report = new Report();

        CreateRule().Apply(doc, ctx, report);

        var paragraphs = GetBody(doc).Elements<Paragraph>().ToList();
        Assert.Equal(5, paragraphs.Count);
        Assert.Same(author, paragraphs[0]);
        Assert.Same(affiliation, paragraphs[1]);
        Assert.Equal("Corresponding author: foo@x.com", ParagraphText(paragraphs[2]));
        Assert.True(IsHeadingParagraph(paragraphs[3]));
        Assert.Same(abstractPara, paragraphs[4]);

        Assert.DoesNotContain(GetBody(doc).Elements<Paragraph>(), p => ReferenceEquals(p, typedLine));

        Assert.Contains(
            report.Entries,
            e => e.Level == ReportLevel.Info
                 && e.Message.StartsWith(RewriteAbstractRule.ReplacedTypedLineMessagePrefix, StringComparison.Ordinal)
                 && e.Message.Contains("Corresponding Author: foo@x.com"));
    }

    [Fact]
    public void Apply_WithEmailAndMisspelledTypedLine_MatchesAndReplaces()
    {
        var author = PlainParagraph("Maria Silva");
        var affiliation = PlainParagraph("1 University");
        var typedLine = PlainParagraph("coresponding author - foo@x.com");
        var abstractPara = new Paragraph(TextRun("Abstract - body text"));

        using var doc = CreateDocumentWith(author, affiliation, typedLine, abstractPara);

        var ctx = new FormattingContext { CorrespondingEmail = "foo@x.com" };
        ctx.AuthorParagraphs.Add(author);
        var report = new Report();

        CreateRule().Apply(doc, ctx, report);

        var paragraphs = GetBody(doc).Elements<Paragraph>().ToList();
        Assert.Equal(5, paragraphs.Count);
        Assert.Equal("Corresponding author: foo@x.com", ParagraphText(paragraphs[2]));
        Assert.DoesNotContain(GetBody(doc).Elements<Paragraph>(), p => ReferenceEquals(p, typedLine));
    }

    [Fact]
    public void Apply_WithEmailAndPortugueseTypedLineWithoutSeparator_MatchesAndReplaces()
    {
        var author = PlainParagraph("Maria Silva");
        var affiliation = PlainParagraph("1 University");
        var typedLine = PlainParagraph("Correspondent Autor foo@x.com");
        var abstractPara = new Paragraph(TextRun("Abstract - body text"));

        using var doc = CreateDocumentWith(author, affiliation, typedLine, abstractPara);

        var ctx = new FormattingContext { CorrespondingEmail = "foo@x.com" };
        ctx.AuthorParagraphs.Add(author);
        var report = new Report();

        CreateRule().Apply(doc, ctx, report);

        var paragraphs = GetBody(doc).Elements<Paragraph>().ToList();
        Assert.Equal(5, paragraphs.Count);
        Assert.Equal("Corresponding author: foo@x.com", ParagraphText(paragraphs[2]));
        Assert.DoesNotContain(GetBody(doc).Elements<Paragraph>(), p => ReferenceEquals(p, typedLine));
    }

    [Fact]
    public void Apply_WithNoEmailAndTypedLineCarryingEmail_RecoversEmailAndReplaces()
    {
        var author = PlainParagraph("Maria Silva");
        var affiliation = PlainParagraph("1 University");
        var typedLine = PlainParagraph("Corresponding Author: bar@y.edu");
        var abstractPara = new Paragraph(TextRun("Abstract - body text"));

        using var doc = CreateDocumentWith(author, affiliation, typedLine, abstractPara);

        var ctx = new FormattingContext { CorrespondingEmail = null };
        ctx.AuthorParagraphs.Add(author);
        var report = new Report();

        CreateRule().Apply(doc, ctx, report);

        Assert.Equal("bar@y.edu", ctx.CorrespondingEmail);

        var paragraphs = GetBody(doc).Elements<Paragraph>().ToList();
        Assert.Equal(5, paragraphs.Count);
        Assert.Equal("Corresponding author: bar@y.edu", ParagraphText(paragraphs[2]));
        Assert.DoesNotContain(GetBody(doc).Elements<Paragraph>(), p => ReferenceEquals(p, typedLine));

        Assert.Contains(
            report.Entries,
            e => e.Level == ReportLevel.Info
                 && e.Message == RewriteAbstractRule.RecoveredEmailMessage);
        Assert.DoesNotContain(
            report.Entries,
            e => e.Message.StartsWith(RewriteAbstractRule.ReplacedTypedLineMessagePrefix, StringComparison.Ordinal));
    }

    [Fact]
    public void Apply_WithNoEmailAndTypedLineWithoutEmail_LeavesTypedLineUntouched()
    {
        var author = PlainParagraph("Maria Silva");
        var affiliation = PlainParagraph("1 University");
        var typedLine = PlainParagraph("Corresponding author:");
        var abstractPara = new Paragraph(TextRun("Abstract - body text"));

        using var doc = CreateDocumentWith(author, affiliation, typedLine, abstractPara);

        var ctx = new FormattingContext { CorrespondingEmail = null };
        ctx.AuthorParagraphs.Add(author);
        var report = new Report();

        CreateRule().Apply(doc, ctx, report);

        Assert.Null(ctx.CorrespondingEmail);

        var paragraphs = GetBody(doc).Elements<Paragraph>().ToList();
        Assert.Equal(5, paragraphs.Count);
        Assert.Same(author, paragraphs[0]);
        Assert.Same(affiliation, paragraphs[1]);
        Assert.Same(typedLine, paragraphs[2]);
        Assert.True(IsHeadingParagraph(paragraphs[3]));
        Assert.Same(abstractPara, paragraphs[4]);

        Assert.DoesNotContain(report.Entries, e => e.Level >= ReportLevel.Warn);
    }

    [Fact]
    public void Apply_WithNoEmailAndNoTypedLine_DoesNotInsertEmailButSplitsHeading()
    {
        var author = PlainParagraph("Maria Silva");
        var affiliation = PlainParagraph("1 University");
        var abstractPara = new Paragraph(TextRun("Abstract - body text"));

        using var doc = CreateDocumentWith(author, affiliation, abstractPara);

        var ctx = new FormattingContext { CorrespondingEmail = null };
        ctx.AuthorParagraphs.Add(author);
        var report = new Report();

        CreateRule().Apply(doc, ctx, report);

        Assert.Null(ctx.CorrespondingEmail);

        var paragraphs = GetBody(doc).Elements<Paragraph>().ToList();
        Assert.Equal(4, paragraphs.Count);
        Assert.Same(author, paragraphs[0]);
        Assert.Same(affiliation, paragraphs[1]);
        Assert.True(IsHeadingParagraph(paragraphs[2]));
        Assert.Same(abstractPara, paragraphs[3]);

        Assert.DoesNotContain(report.Entries, e => e.Level >= ReportLevel.Warn);
    }

    [Fact]
    public void Apply_WhenAbstractParagraphMissing_LogsWarnAndDoesNotInsertEmail()
    {
        var author = PlainParagraph("Maria Silva");
        var affiliation = PlainParagraph("1 University");

        using var doc = CreateDocumentWith(author, affiliation);
        var bodyXmlBefore = GetBody(doc).OuterXml;

        var ctx = new FormattingContext { CorrespondingEmail = "foo@x.com" };
        ctx.AuthorParagraphs.Add(author);
        var report = new Report();

        CreateRule().Apply(doc, ctx, report);

        Assert.Equal(bodyXmlBefore, GetBody(doc).OuterXml);

        var warn = Assert.Single(report.Entries, e => e.Level == ReportLevel.Warn);
        Assert.Equal(nameof(RewriteAbstractRule), warn.Rule);
        Assert.Equal(RewriteAbstractRule.AbstractNotFoundMessage, warn.Message);
    }

    [Fact]
    public void Apply_WithCorrespondencePrefix_DoesNotMatchTypedLineRegex()
    {
        var author = PlainParagraph("Maria Silva");
        var typedLine = PlainParagraph("Correspondence: editor@example.com");
        var abstractPara = new Paragraph(TextRun("Abstract - body text"));

        using var doc = CreateDocumentWith(author, typedLine, abstractPara);

        var ctx = new FormattingContext { CorrespondingEmail = "foo@x.com" };
        ctx.AuthorParagraphs.Add(author);
        var report = new Report();

        CreateRule().Apply(doc, ctx, report);

        var paragraphs = GetBody(doc).Elements<Paragraph>().ToList();
        Assert.Equal(5, paragraphs.Count);
        Assert.Same(author, paragraphs[0]);
        Assert.Same(typedLine, paragraphs[1]);
        Assert.Equal("Correspondence: editor@example.com", ParagraphText(typedLine));
        Assert.Equal("Corresponding author: foo@x.com", ParagraphText(paragraphs[2]));
        Assert.True(IsHeadingParagraph(paragraphs[3]));
        Assert.Same(abstractPara, paragraphs[4]);
    }

    [Fact]
    public void Apply_WithMarkerButNoSeparator_LogsWarnAndKeepsPostMarkerBody()
    {
        var abstractPara = new Paragraph(TextRun("Abstract body without separator"));

        using var doc = CreateDocumentWith(abstractPara);

        var ctx = new FormattingContext();
        var report = new Report();

        CreateRule().Apply(doc, ctx, report);

        var paragraphs = GetBody(doc).Elements<Paragraph>().ToList();
        Assert.Equal(2, paragraphs.Count);
        Assert.True(IsHeadingParagraph(paragraphs[0]));
        Assert.Same(abstractPara, paragraphs[1]);
        Assert.Equal(" body without separator", ParagraphText(abstractPara));

        Assert.Contains(
            report.Entries,
            e => e.Level == ReportLevel.Warn
                 && e.Message == RewriteAbstractRule.MissingSeparatorMessage);
    }

    [Fact]
    public void Apply_HeadingParagraphHasBoldRunWithDefaultFontAndSize()
    {
        var abstractPara = new Paragraph(TextRun("Abstract - body"));
        using var doc = CreateDocumentWith(abstractPara);

        var ctx = new FormattingContext();
        var report = new Report();

        CreateRule().Apply(doc, ctx, report);

        var heading = GetBody(doc).Elements<Paragraph>().First();
        var run = heading.Elements<Run>().Single();

        var props = run.RunProperties;
        Assert.NotNull(props);
        Assert.NotNull(props!.GetFirstChild<Bold>());

        var fonts = props.GetFirstChild<RunFonts>();
        Assert.NotNull(fonts);
        Assert.Equal("Times New Roman", fonts!.Ascii?.Value);

        var size = props.GetFirstChild<FontSize>();
        Assert.NotNull(size);
        Assert.Equal("24", size!.Val?.Value);

        Assert.Equal("Abstract", string.Concat(run.Descendants<Text>().Select(t => t.Text)));
    }

    [Fact]
    public void Apply_WithEmailButNoAuthorParagraphsAndNoTypedLine_InsertsCanonicalLine()
    {
        var abstractPara = new Paragraph(TextRun("Abstract - body"));
        using var doc = CreateDocumentWith(abstractPara);

        var ctx = new FormattingContext { CorrespondingEmail = "foo@x.com" };
        var report = new Report();

        CreateRule().Apply(doc, ctx, report);

        var paragraphs = GetBody(doc).Elements<Paragraph>().ToList();
        Assert.Equal(3, paragraphs.Count);
        Assert.Equal("Corresponding author: foo@x.com", ParagraphText(paragraphs[0]));
        Assert.True(IsHeadingParagraph(paragraphs[1]));
        Assert.Same(abstractPara, paragraphs[2]);
    }

    [Fact]
    public void Apply_PreservesNonBodyParagraphs()
    {
        var section = PlainParagraph("Original Article");
        var title = PlainParagraph("Title");
        var author = PlainParagraph("Maria Silva");
        var abstractPara = new Paragraph(TextRun("Abstract - body content"));
        var keywords = PlainParagraph("Keywords: a, b, c");

        using var doc = CreateDocumentWith(section, title, author, abstractPara, keywords);

        var ctx = new FormattingContext();
        ctx.AuthorParagraphs.Add(author);
        var report = new Report();

        CreateRule().Apply(doc, ctx, report);

        var paragraphs = GetBody(doc).Elements<Paragraph>().ToList();
        Assert.Equal(6, paragraphs.Count);
        Assert.Same(section, paragraphs[0]);
        Assert.Same(title, paragraphs[1]);
        Assert.Same(author, paragraphs[2]);
        Assert.True(IsHeadingParagraph(paragraphs[3]));
        Assert.Same(abstractPara, paragraphs[4]);
        Assert.Same(keywords, paragraphs[5]);
        Assert.Equal("Keywords: a, b, c", ParagraphText(keywords));
    }

    [Fact]
    public void Apply_WithUniformItalicAndColonSeparator_StripsItalicAndUsesColon()
    {
        var abstractPara = new Paragraph(
            ItalicTextRun("Abstract: lorem ipsum sit amet"));

        using var doc = CreateDocumentWith(abstractPara);

        var ctx = new FormattingContext();
        var report = new Report();

        CreateRule().Apply(doc, ctx, report);

        var paragraphs = GetBody(doc).Elements<Paragraph>().ToList();
        Assert.Equal(2, paragraphs.Count);
        Assert.True(IsHeadingParagraph(paragraphs[0]));
        Assert.Equal("lorem ipsum sit amet", ParagraphText(paragraphs[1]));
    }
}
