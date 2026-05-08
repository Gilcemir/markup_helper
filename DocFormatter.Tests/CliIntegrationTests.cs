using System.Security.Cryptography;
using System.Text.Json;
using DocFormatter.Cli;
using DocFormatter.Core.Pipeline;
using DocFormatter.Core.Reporting;
using DocFormatter.Core.Rules;
using DocFormatter.Tests.Fixtures.Phase2;
using DocFormatter.Tests.Fixtures.Phase3;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DocFormatter.Tests;

public sealed class CliIntegrationTests : IDisposable
{
    private readonly string _tempDir;

    public CliIntegrationTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"docfmt-cli-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, recursive: true); } catch { /* best effort */ }
    }

    [Fact]
    public void Run_NoArguments_PrintsUsageToStderr_ReturnsUsageError()
    {
        var stdout = new StringWriter();
        var stderr = new StringWriter();

        var exit = CliApp.Run(Array.Empty<string>(), stdout, stderr);

        Assert.Equal(1, exit);
        Assert.Empty(stdout.ToString());
        Assert.Contains("Usage:", stderr.ToString());
    }

    [Fact]
    public void Run_HelpFlag_PrintsUsageToStdout_ReturnsSuccess()
    {
        var stdout = new StringWriter();
        var stderr = new StringWriter();

        var exit = CliApp.Run(new[] { "--help" }, stdout, stderr);

        Assert.Equal(0, exit);
        Assert.Contains("Usage:", stdout.ToString());
        Assert.Empty(stderr.ToString());
    }

    [Fact]
    public void Run_VersionFlag_PrintsAssemblyInformationalVersion_ReturnsSuccess()
    {
        var stdout = new StringWriter();
        var stderr = new StringWriter();

        var exit = CliApp.Run(new[] { "--version" }, stdout, stderr);

        Assert.Equal(0, exit);
        var printed = stdout.ToString().Trim();
        Assert.Equal(CliApp.GetVersion(), printed);
        Assert.False(string.IsNullOrEmpty(printed));
        Assert.Empty(stderr.ToString());
    }

    [Fact]
    public void Run_NonDocxFileExtension_PrintsErrorToStderr_ReturnsUsageError()
    {
        var stdout = new StringWriter();
        var stderr = new StringWriter();
        var txtPath = Path.Combine(_tempDir, "wrong-ext.txt");
        File.WriteAllText(txtPath, "not a docx");

        var exit = CliApp.Run(new[] { txtPath }, stdout, stderr);

        Assert.Equal(1, exit);
        Assert.Contains(".docx", stderr.ToString(), StringComparison.OrdinalIgnoreCase);
        Assert.Empty(stdout.ToString());
        Assert.False(Directory.Exists(Path.Combine(_tempDir, "formatted")));
    }

    [Fact]
    public void Run_PathDoesNotExist_PrintsErrorToStderr_ReturnsUsageError()
    {
        var stdout = new StringWriter();
        var stderr = new StringWriter();
        var bogusPath = Path.Combine(_tempDir, "nope.docx");

        var exit = CliApp.Run(new[] { bogusPath }, stdout, stderr);

        Assert.Equal(1, exit);
        Assert.Contains("path not found", stderr.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Run_SingleFile_HappyPath_ProducesOutputAndReport_LeavesSourceUntouched()
    {
        var sourcePath = Path.Combine(_tempDir, "happy.docx");
        DocxFixtureBuilder.WriteValidDocx(sourcePath);
        var sourceHash = HashFile(sourcePath);

        var exit = CliApp.Run(new[] { sourcePath }, new StringWriter(), new StringWriter());

        Assert.Equal(0, exit);
        var formattedDir = Path.Combine(_tempDir, "formatted");
        Assert.True(File.Exists(Path.Combine(formattedDir, "happy.docx")));
        Assert.True(File.Exists(Path.Combine(formattedDir, "happy.report.txt")));
        Assert.False(File.Exists(Path.Combine(formattedDir, "happy.diagnostic.json")));
        Assert.Equal(sourceHash, HashFile(sourcePath));
    }

    [Fact]
    public void Run_SingleFile_RuleEmitsWarning_WritesDiagnosticJson_RoundTrips()
    {
        var sourcePath = Path.Combine(_tempDir, "warn.docx");
        DocxFixtureBuilder.WriteDocxWithoutAbstract(sourcePath);

        var exit = CliApp.Run(new[] { sourcePath }, new StringWriter(), new StringWriter());

        Assert.Equal(0, exit);
        var formattedDir = Path.Combine(_tempDir, "formatted");
        var diagnosticPath = Path.Combine(formattedDir, "warn.diagnostic.json");
        Assert.True(File.Exists(diagnosticPath));

        var raw = File.ReadAllText(diagnosticPath);
        var doc = JsonSerializer.Deserialize<DiagnosticDocument>(raw, DiagnosticWriter.JsonOptions);
        Assert.NotNull(doc);
        Assert.Equal("warn.docx", doc!.File);
        Assert.Equal("warning", doc.Status);
        Assert.Contains(doc.Issues, i => i.Level == "warn");

        var rewritten = JsonSerializer.Serialize(doc, DiagnosticWriter.JsonOptions);
        var roundtripped = JsonSerializer.Deserialize<DiagnosticDocument>(rewritten, DiagnosticWriter.JsonOptions);
        Assert.Equal(doc, roundtripped);
    }

    [Fact]
    public void Run_SingleFile_CriticalAbort_WritesDiagnosticJson_StatusError()
    {
        var sourcePath = Path.Combine(_tempDir, "broken-abort.docx");
        DocxFixtureBuilder.WriteDocxWithoutTopTable(sourcePath);

        var exit = CliApp.Run(new[] { sourcePath }, new StringWriter(), new StringWriter());

        Assert.Equal(2, exit);
        var formattedDir = Path.Combine(_tempDir, "formatted");
        var diagnosticPath = Path.Combine(formattedDir, "broken-abort.diagnostic.json");
        Assert.True(File.Exists(diagnosticPath));

        var doc = JsonSerializer.Deserialize<DiagnosticDocument>(
            File.ReadAllText(diagnosticPath),
            DiagnosticWriter.JsonOptions);
        Assert.NotNull(doc);
        Assert.Equal("error", doc!.Status);
        Assert.Contains(doc.Issues, i => i.Level == "error");
    }

    [Fact]
    public void Run_SingleFile_MissingTopTable_ReturnsCriticalAbort_NoOutputDocx_ReportRecordsReason()
    {
        var sourcePath = Path.Combine(_tempDir, "broken.docx");
        DocxFixtureBuilder.WriteDocxWithoutTopTable(sourcePath);

        var exit = CliApp.Run(new[] { sourcePath }, new StringWriter(), new StringWriter());

        Assert.Equal(2, exit);
        var formattedDir = Path.Combine(_tempDir, "formatted");
        Assert.False(File.Exists(Path.Combine(formattedDir, "broken.docx")));
        var reportPath = Path.Combine(formattedDir, "broken.report.txt");
        Assert.True(File.Exists(reportPath));
        var reportContent = File.ReadAllText(reportPath);
        Assert.Contains("[ERROR]", reportContent);
        Assert.Contains(ExtractTopTableRule.CriticalAbortMessage, reportContent);
    }

    [Fact]
    public void Run_Batch_TwoValid_OneMissingTopTable_WritesSummary_OnlyValidFilesProduceOutput()
    {
        var folder = Path.Combine(_tempDir, "inbox");
        Directory.CreateDirectory(folder);

        var ok1 = Path.Combine(folder, "a-ok.docx");
        var ok2 = Path.Combine(folder, "b-ok.docx");
        var bad = Path.Combine(folder, "c-bad.docx");
        DocxFixtureBuilder.WriteValidDocx(ok1);
        DocxFixtureBuilder.WriteValidDocx(ok2);
        DocxFixtureBuilder.WriteDocxWithoutTopTable(bad);

        var exit = CliApp.Run(new[] { folder }, new StringWriter(), new StringWriter());

        Assert.Equal(0, exit);
        var formattedDir = Path.Combine(folder, "formatted");
        Assert.True(File.Exists(Path.Combine(formattedDir, "a-ok.docx")));
        Assert.True(File.Exists(Path.Combine(formattedDir, "b-ok.docx")));
        Assert.False(File.Exists(Path.Combine(formattedDir, "c-bad.docx")));

        var summaryPath = Path.Combine(formattedDir, "_batch_summary.txt");
        Assert.True(File.Exists(summaryPath));
        var summary = File.ReadAllLines(summaryPath);
        Assert.Equal(3, summary.Length);
        Assert.Equal(2, summary.Count(l => l.Contains(" ✓")));
        Assert.Equal(1, summary.Count(l => l.Contains(" ✗")));
        Assert.Contains(summary, l => l.StartsWith("a-ok.docx ✓", StringComparison.Ordinal));
        Assert.Contains(summary, l => l.StartsWith("b-ok.docx ✓", StringComparison.Ordinal));
        Assert.Contains(summary, l => l.StartsWith("c-bad.docx ✗", StringComparison.Ordinal));
    }

    [Fact]
    public void Run_Batch_SkipsWordLockAndAppleResourceForkArtifacts()
    {
        var folder = Path.Combine(_tempDir, "inbox-with-junk");
        Directory.CreateDirectory(folder);

        var ok = Path.Combine(folder, "real.docx");
        DocxFixtureBuilder.WriteValidDocx(ok);

        // Word writes ~$<name>.docx lock files when a doc is open; macOS
        // sometimes drops ._<name>.docx resource forks. Both are not valid
        // OpenXML packages and must be ignored, not aborted on.
        File.WriteAllBytes(Path.Combine(folder, "~$real.docx"), new byte[] { 0x00 });
        File.WriteAllBytes(Path.Combine(folder, "._real.docx"), new byte[] { 0x00 });

        var exit = CliApp.Run(new[] { folder }, new StringWriter(), new StringWriter());

        Assert.Equal(0, exit);
        var formattedDir = Path.Combine(folder, "formatted");
        Assert.True(File.Exists(Path.Combine(formattedDir, "real.docx")));
        Assert.False(File.Exists(Path.Combine(formattedDir, "~$real.docx")));
        Assert.False(File.Exists(Path.Combine(formattedDir, "._real.docx")));

        var summary = File.ReadAllLines(Path.Combine(formattedDir, "_batch_summary.txt"));
        Assert.Single(summary);
        Assert.StartsWith("real.docx ✓", summary[0]);
    }

    [Fact]
    public void Run_SingleFile_WritesAppLogUnderFormattedDirectory()
    {
        var sourcePath = Path.Combine(_tempDir, "with-log.docx");
        DocxFixtureBuilder.WriteValidDocx(sourcePath);

        CliApp.Run(new[] { sourcePath }, new StringWriter(), new StringWriter());

        var logPath = Path.Combine(_tempDir, "formatted", "_app.log");
        Assert.True(File.Exists(logPath));
    }

    [Fact]
    public void BuildServiceProvider_RegistersFormattingRulesInTechSpecOrder()
    {
        using var services = CliApp.BuildServiceProvider();

        var rules = services.GetServices<IFormattingRule>().Select(r => r.GetType()).ToArray();

        Assert.Equal(
            new[]
            {
                typeof(ExtractTopTableRule),
                typeof(ParseHeaderLinesRule),
                typeof(ExtractAuthorsRule),
                typeof(ExtractCorrespondingAuthorRule),
                typeof(RewriteHeaderMvpRule),
                typeof(ApplyHeaderAlignmentRule),
                typeof(EnsureAuthorBlockSpacingRule),
                typeof(RewriteAbstractRule),
                typeof(LocateAbstractAndInsertElocationRule),
                typeof(MoveHistoryRule),
                typeof(PromoteSectionsRule),
            },
            rules);
    }

    [Fact]
    public void Run_Phase2_WithCorrespondingMarker_AppliesAllFourBehaviorsEndToEnd()
    {
        var sourcePath = Path.Combine(_tempDir, "phase2-marker.docx");
        Phase2DocxFixtureBuilder.WriteWithCorrespondingMarker(sourcePath);

        var exit = CliApp.Run(new[] { sourcePath }, new StringWriter(), new StringWriter());
        Assert.Equal(0, exit);

        var formattedPath = Path.Combine(_tempDir, "formatted", "phase2-marker.docx");
        Assert.True(File.Exists(formattedPath));

        using var doc = WordprocessingDocument.Open(formattedPath, isEditable: false);
        var paragraphs = doc.MainDocumentPart!.Document!.Body!.Elements<Paragraph>().ToList();

        var doiIdx = FindParagraphIndex(paragraphs, p => p.InnerText.Contains(Phase2DocxFixtureBuilder.Doi));
        var sectionIdx = FindParagraphIndex(paragraphs, p => p.InnerText == Phase2DocxFixtureBuilder.SectionText);
        var titleIdx = FindParagraphIndex(paragraphs, p => p.InnerText == Phase2DocxFixtureBuilder.TitleText);
        var authorIdx = FindParagraphIndex(paragraphs, p => p.InnerText.StartsWith(Phase2DocxFixtureBuilder.AuthorName, StringComparison.Ordinal));
        var aff1Idx = FindParagraphIndex(paragraphs, p => p.InnerText.Contains(Phase2DocxFixtureBuilder.Affiliation1Text));
        var aff2Idx = FindParagraphIndex(paragraphs, p => p.InnerText.Contains(Phase2DocxFixtureBuilder.Affiliation2Text));
        var emailIdx = FindParagraphIndex(paragraphs, p => p.InnerText.StartsWith("Corresponding author:", StringComparison.Ordinal));
        var headingIdx = FindParagraphIndex(paragraphs, IsBoldOnlyAbstractHeading);

        Assert.Equal(JustificationValues.Right, JustificationOf(paragraphs[doiIdx]));
        Assert.Equal(JustificationValues.Right, JustificationOf(paragraphs[sectionIdx]));
        Assert.Equal(JustificationValues.Center, JustificationOf(paragraphs[titleIdx]));

        Assert.Equal(authorIdx + 2, aff1Idx);
        Assert.True(IsBlankParagraph(paragraphs[authorIdx + 1]));

        Assert.Equal(
            "Corresponding author: " + Phase2DocxFixtureBuilder.CorrespondingEmail,
            paragraphs[emailIdx].InnerText);
        Assert.True(emailIdx > aff2Idx);
        Assert.True(emailIdx < headingIdx);

        var bodyParagraph = paragraphs[headingIdx + 1];
        Assert.Equal(AbstractParagraphFactory.DefaultBodyText, bodyParagraph.InnerText);
        Assert.All(bodyParagraph.Descendants<Run>(), run => Assert.Null(run.RunProperties?.Italic));

        var trailerStripped = !paragraphs[aff2Idx].InnerText.Contains("E-mail", StringComparison.OrdinalIgnoreCase);
        Assert.True(trailerStripped, $"affiliation 2 still carries the email trailer: '{paragraphs[aff2Idx].InnerText}'");
    }

    [Fact]
    public void Run_Phase2_WithoutCorrespondingMarker_AlignsSpacesAndRewritesAbstract_NoCorrespondingLine()
    {
        var sourcePath = Path.Combine(_tempDir, "phase2-nomarker.docx");
        Phase2DocxFixtureBuilder.WriteWithoutCorrespondingMarker(sourcePath);

        var exit = CliApp.Run(new[] { sourcePath }, new StringWriter(), new StringWriter());
        Assert.Equal(0, exit);

        var formattedPath = Path.Combine(_tempDir, "formatted", "phase2-nomarker.docx");
        using var doc = WordprocessingDocument.Open(formattedPath, isEditable: false);
        var paragraphs = doc.MainDocumentPart!.Document!.Body!.Elements<Paragraph>().ToList();

        var sectionIdx = FindParagraphIndex(paragraphs, p => p.InnerText == Phase2DocxFixtureBuilder.SectionText);
        var titleIdx = FindParagraphIndex(paragraphs, p => p.InnerText == Phase2DocxFixtureBuilder.TitleText);
        var authorIdx = FindParagraphIndex(paragraphs, p => p.InnerText.StartsWith(Phase2DocxFixtureBuilder.AuthorName, StringComparison.Ordinal));
        var aff1Idx = FindParagraphIndex(paragraphs, p => p.InnerText.Contains(Phase2DocxFixtureBuilder.Affiliation1Text));
        var headingIdx = FindParagraphIndex(paragraphs, IsBoldOnlyAbstractHeading);

        Assert.Equal(JustificationValues.Right, JustificationOf(paragraphs[sectionIdx]));
        Assert.Equal(JustificationValues.Center, JustificationOf(paragraphs[titleIdx]));
        Assert.Equal(authorIdx + 2, aff1Idx);
        Assert.True(IsBlankParagraph(paragraphs[authorIdx + 1]));

        Assert.DoesNotContain(paragraphs, p => p.InnerText.StartsWith("Corresponding author:", StringComparison.Ordinal));

        var bodyParagraph = paragraphs[headingIdx + 1];
        Assert.Equal(AbstractParagraphFactory.DefaultBodyText, bodyParagraph.InnerText);
        Assert.All(bodyParagraph.Descendants<Run>(), run => Assert.Null(run.RunProperties?.Italic));
    }

    [Fact]
    public void Run_Phase2_WithResumoSource_NormalizesHeadingToAbstract_KeepsBodyLanguage()
    {
        var sourcePath = Path.Combine(_tempDir, "phase2-resumo.docx");
        Phase2DocxFixtureBuilder.WriteWithResumoSource(sourcePath);

        var exit = CliApp.Run(new[] { sourcePath }, new StringWriter(), new StringWriter());
        Assert.Equal(0, exit);

        var formattedPath = Path.Combine(_tempDir, "formatted", "phase2-resumo.docx");
        using var doc = WordprocessingDocument.Open(formattedPath, isEditable: false);
        var paragraphs = doc.MainDocumentPart!.Document!.Body!.Elements<Paragraph>().ToList();

        var headingIdx = FindParagraphIndex(paragraphs, IsBoldOnlyAbstractHeading);
        Assert.Equal("Abstract", paragraphs[headingIdx].InnerText);

        var bodyParagraph = paragraphs[headingIdx + 1];
        Assert.Equal(AbstractParagraphFactory.DefaultPortugueseBodyText, bodyParagraph.InnerText);
    }

    [Fact]
    public void Run_Phase2_WithMalformedEmail_PopulatesFormattingDiagnosticSection()
    {
        var sourcePath = Path.Combine(_tempDir, "phase2-bad-email.docx");
        Phase2DocxFixtureBuilder.WriteWithMalformedEmail(sourcePath);

        var exit = CliApp.Run(new[] { sourcePath }, new StringWriter(), new StringWriter());
        Assert.Equal(0, exit);

        var diagnosticPath = Path.Combine(_tempDir, "formatted", "phase2-bad-email.diagnostic.json");
        Assert.True(File.Exists(diagnosticPath));

        var doc = JsonSerializer.Deserialize<DiagnosticDocument>(
            File.ReadAllText(diagnosticPath),
            DiagnosticWriter.JsonOptions);
        Assert.NotNull(doc);
        Assert.NotNull(doc!.Formatting);
        Assert.NotNull(doc.Formatting!.CorrespondingEmail);
        Assert.Null(doc.Formatting.CorrespondingEmail!.Value);
        Assert.Equal(
            ExtractCorrespondingAuthorRule.EmailExtractionFailedMessage,
            doc.Formatting.CorrespondingEmail.Reason);
    }

    [Fact]
    public void Run_Phase3_HappyPath_MovesHistoryAndPromotesSectionsEndToEnd()
    {
        var sourcePath = Path.Combine(_tempDir, "phase3-happy.docx");
        Phase3DocxFixtureBuilder.WritePhase123HappyPathDocx(sourcePath);
        var inputTexts = CollectNonEmptyBodyTexts(sourcePath);

        var exit = CliApp.Run(new[] { sourcePath }, new StringWriter(), new StringWriter());
        Assert.Equal(0, exit);

        var formattedDir = Path.Combine(_tempDir, "formatted");
        var formattedPath = Path.Combine(formattedDir, "phase3-happy.docx");
        Assert.True(File.Exists(formattedPath));

        using var doc = WordprocessingDocument.Open(formattedPath, isEditable: false);
        var body = doc.MainDocumentPart!.Document!.Body!;
        var paragraphs = body.Elements<Paragraph>().ToList();

        var introIndex = FindParagraphIndex(
            paragraphs,
            p => p.InnerText.Trim() == Phase3DocxFixtureBuilder.IntroductionText);
        var receivedIndex = FindParagraphIndex(
            paragraphs,
            p => p.InnerText.StartsWith("Received:", StringComparison.Ordinal));
        var acceptedIndex = FindParagraphIndex(
            paragraphs,
            p => p.InnerText.StartsWith("Accepted:", StringComparison.Ordinal));
        var publishedIndex = FindParagraphIndex(
            paragraphs,
            p => p.InnerText.StartsWith("Published:", StringComparison.Ordinal));

        Assert.Equal(introIndex - 3, receivedIndex);
        Assert.Equal(introIndex - 2, acceptedIndex);
        Assert.Equal(introIndex - 1, publishedIndex);

        AssertCenteredSection(paragraphs[introIndex], halfPoints: "32");
        var materialIndex = FindParagraphIndex(
            paragraphs,
            p => p.InnerText.Trim() == Phase3DocxFixtureBuilder.SectionMaterialText);
        AssertCenteredSection(paragraphs[materialIndex], halfPoints: "32");
        var resultsIndex = FindParagraphIndex(
            paragraphs,
            p => p.InnerText.Trim() == Phase3DocxFixtureBuilder.SectionResultsText);
        AssertCenteredSection(paragraphs[resultsIndex], halfPoints: "32");

        var subsectionIndex = FindParagraphIndex(
            paragraphs,
            p => p.InnerText.Trim() == Phase3DocxFixtureBuilder.SubsectionDnaText);
        AssertCenteredSection(paragraphs[subsectionIndex], halfPoints: "28");

        var titleIdx = FindParagraphIndex(
            paragraphs,
            p => p.InnerText == Phase2DocxFixtureBuilder.TitleText);
        Assert.Equal(JustificationValues.Center, JustificationOf(paragraphs[titleIdx]));

        var tableNestedParagraph = body
            .Descendants<Paragraph>()
            .First(p => p.InnerText.Trim() == Phase3DocxFixtureBuilder.TableNestedText);
        Assert.Null(JustificationOf(tableNestedParagraph));
        AssertNoFontSizeOnRuns(tableNestedParagraph);

        var diagnosticPath = Path.Combine(formattedDir, "phase3-happy.diagnostic.json");
        Assert.True(File.Exists(diagnosticPath));
        var diagnostic = JsonSerializer.Deserialize<DiagnosticDocument>(
            File.ReadAllText(diagnosticPath),
            DiagnosticWriter.JsonOptions);
        Assert.NotNull(diagnostic);
        Assert.NotNull(diagnostic!.Formatting);

        var historyMove = diagnostic.Formatting!.HistoryMove;
        Assert.NotNull(historyMove);
        Assert.True(historyMove!.Applied);
        Assert.Null(historyMove.SkippedReason);
        Assert.True(historyMove.AnchorFound);
        Assert.Equal(3, historyMove.ParagraphsMoved);

        var sectionPromotion = diagnostic.Formatting.SectionPromotion;
        Assert.NotNull(sectionPromotion);
        Assert.True(sectionPromotion!.Applied);
        Assert.Null(sectionPromotion.SkippedReason);
        Assert.True(sectionPromotion.AnchorFound);
        Assert.NotNull(sectionPromotion.AnchorParagraphIndex);
        Assert.True(sectionPromotion.SectionsPromoted >= 3);
        Assert.Equal(1, sectionPromotion.SubsectionsPromoted);

        AssertPhase3TextsPreserved(inputTexts, formattedPath);
    }

    [Fact]
    public void Run_Phase3_AnchorMissing_BothRulesEmitWarn_DiagnosticReportsSkippedReason()
    {
        var sourcePath = Path.Combine(_tempDir, "phase3-anchor-missing.docx");
        Phase3DocxFixtureBuilder.WritePhase123AnchorMissingDocx(sourcePath);
        var inputTexts = CollectNonEmptyBodyTexts(sourcePath);

        var exit = CliApp.Run(new[] { sourcePath }, new StringWriter(), new StringWriter());
        Assert.Equal(0, exit);

        var formattedDir = Path.Combine(_tempDir, "formatted");
        var formattedPath = Path.Combine(formattedDir, "phase3-anchor-missing.docx");
        Assert.True(File.Exists(formattedPath));

        var reportPath = Path.Combine(formattedDir, "phase3-anchor-missing.report.txt");
        Assert.True(File.Exists(reportPath));
        var reportContent = File.ReadAllText(reportPath);
        Assert.Contains(MoveHistoryRule.AnchorMissingMessage, reportContent);
        Assert.Contains(PromoteSectionsRule.AnchorMissingMessage, reportContent);

        var diagnosticPath = Path.Combine(formattedDir, "phase3-anchor-missing.diagnostic.json");
        Assert.True(File.Exists(diagnosticPath));
        var diagnostic = JsonSerializer.Deserialize<DiagnosticDocument>(
            File.ReadAllText(diagnosticPath),
            DiagnosticWriter.JsonOptions);
        Assert.NotNull(diagnostic);
        Assert.NotNull(diagnostic!.Formatting);

        var historyMove = diagnostic.Formatting!.HistoryMove;
        Assert.NotNull(historyMove);
        Assert.False(historyMove!.Applied);
        Assert.Equal("anchor_missing", historyMove.SkippedReason);
        Assert.False(historyMove.AnchorFound);
        Assert.Equal(0, historyMove.ParagraphsMoved);

        var sectionPromotion = diagnostic.Formatting.SectionPromotion;
        Assert.NotNull(sectionPromotion);
        Assert.False(sectionPromotion!.Applied);
        Assert.Equal("anchor_missing", sectionPromotion.SkippedReason);
        Assert.False(sectionPromotion.AnchorFound);

        AssertPhase3TextsPreserved(inputTexts, formattedPath);
    }

    private static IReadOnlyList<string> CollectNonEmptyBodyTexts(string docxPath)
    {
        using var doc = WordprocessingDocument.Open(docxPath, isEditable: false);
        return doc.MainDocumentPart!.Document!.Body!
            .Descendants<Text>()
            .Select(t => (t.Text ?? string.Empty).Trim())
            .Where(s => s.Length > 0)
            .ToList();
    }

    private static void AssertPhase3TextsPreserved(IReadOnlyList<string> inputTexts, string formattedPath)
    {
        var phase3Texts = new[]
        {
            Phase3DocxFixtureBuilder.KeywordsText,
            Phase3DocxFixtureBuilder.BetweenHistoryAndIntroText,
            Phase3DocxFixtureBuilder.IntroductionBodyText,
            Phase3DocxFixtureBuilder.SectionMaterialText,
            Phase3DocxFixtureBuilder.MaterialBodyText,
            Phase3DocxFixtureBuilder.SubsectionDnaText,
            Phase3DocxFixtureBuilder.DnaBodyText,
            Phase3DocxFixtureBuilder.SectionResultsText,
            Phase3DocxFixtureBuilder.ResultsBodyText,
            Phase3DocxFixtureBuilder.TableNestedText,
        };

        var outputTexts = CollectNonEmptyBodyTexts(formattedPath);
        var inputMultiset = inputTexts.GroupBy(s => s).ToDictionary(g => g.Key, g => g.Count());
        var outputMultiset = outputTexts.GroupBy(s => s).ToDictionary(g => g.Key, g => g.Count());

        foreach (var text in phase3Texts)
        {
            Assert.True(
                inputMultiset.TryGetValue(text, out var inputCount) && inputCount > 0,
                $"fixture sanity: '{text}' missing from input");
            Assert.True(
                outputMultiset.TryGetValue(text, out var outputCount) && outputCount >= inputCount,
                $"INV-01 violated: '{text}' present {inputCount}x in input but {outputCount}x in output");
        }
    }

    private static void AssertCenteredSection(Paragraph paragraph, string halfPoints)
    {
        Assert.Equal(JustificationValues.Center, JustificationOf(paragraph));
        var runs = paragraph.Descendants<Run>()
            .Where(r => r.Descendants<Text>().Any())
            .ToList();
        Assert.NotEmpty(runs);
        Assert.All(runs, run =>
        {
            Assert.Equal(halfPoints, run.RunProperties?.FontSize?.Val?.Value);
            Assert.Equal(halfPoints, run.RunProperties?.FontSizeComplexScript?.Val?.Value);
        });
    }

    private static void AssertNoFontSizeOnRuns(Paragraph paragraph)
    {
        foreach (var run in paragraph.Descendants<Run>())
        {
            Assert.Null(run.RunProperties?.FontSize);
            Assert.Null(run.RunProperties?.FontSizeComplexScript);
        }
    }

    private static int FindParagraphIndex(IReadOnlyList<Paragraph> paragraphs, Func<Paragraph, bool> predicate)
    {
        for (var i = 0; i < paragraphs.Count; i++)
        {
            if (predicate(paragraphs[i]))
            {
                return i;
            }
        }

        throw new InvalidOperationException("paragraph not found");
    }

    private static JustificationValues? JustificationOf(Paragraph paragraph)
        => paragraph.ParagraphProperties?.GetFirstChild<Justification>()?.Val?.Value;

    private static bool IsBlankParagraph(Paragraph paragraph)
        => string.IsNullOrWhiteSpace(paragraph.InnerText);

    private static bool IsBoldOnlyAbstractHeading(Paragraph paragraph)
    {
        var runs = paragraph.Elements<Run>().ToList();
        if (runs.Count != 1)
        {
            return false;
        }

        var run = runs[0];
        var bold = run.RunProperties?.GetFirstChild<Bold>();
        if (bold is null)
        {
            return false;
        }

        var text = string.Concat(run.Descendants<Text>().Select(t => t.Text));
        return text == "Abstract";
    }

    private static string HashFile(string path)
    {
        using var stream = File.OpenRead(path);
        var hash = SHA256.HashData(stream);
        return Convert.ToHexString(hash);
    }
}

internal static class DocxFixtureBuilder
{
    public static void WriteValidDocx(string path)
    {
        using var doc = WordprocessingDocument.Create(path, WordprocessingDocumentType.Document);
        var mainPart = doc.AddMainDocumentPart();
        mainPart.Document = new Document(BuildBody(includeTopTable: true, includeAbstract: true));
    }

    public static void WriteDocxWithoutTopTable(string path)
    {
        using var doc = WordprocessingDocument.Create(path, WordprocessingDocumentType.Document);
        var mainPart = doc.AddMainDocumentPart();
        mainPart.Document = new Document(BuildBody(includeTopTable: false, includeAbstract: true));
    }

    public static void WriteDocxWithoutAbstract(string path)
    {
        using var doc = WordprocessingDocument.Create(path, WordprocessingDocumentType.Document);
        var mainPart = doc.AddMainDocumentPart();
        mainPart.Document = new Document(BuildBody(includeTopTable: true, includeAbstract: false));
    }

    private static Body BuildBody(bool includeTopTable, bool includeAbstract)
    {
        var children = new List<OpenXmlElement>();

        if (includeTopTable)
        {
            children.Add(BuildTopTable());
        }

        children.Add(PlainParagraph("Original Article"));
        children.Add(PlainParagraph("On the Behavior of Title"));
        children.Add(BuildAuthorsParagraph());

        if (includeAbstract)
        {
            children.Add(BuildAbstractParagraph());
        }

        children.Add(BuildIntroductionAnchorParagraph());

        return new Body(children);
    }

    private static Paragraph BuildIntroductionAnchorParagraph()
    {
        var run = new Run(
            new RunProperties(new Bold()),
            new Text("INTRODUCTION") { Space = SpaceProcessingModeValues.Preserve });
        return new Paragraph(run);
    }

    private static Table BuildTopTable()
    {
        var grid = new TableGrid(
            new GridColumn { Width = "2000" },
            new GridColumn { Width = "2000" },
            new GridColumn { Width = "2000" });
        return new Table(
            grid,
            new TableRow(
                BuildCell("id", "ART01"),
                BuildCell("elocation", "e2024001"),
                BuildCell("doi", "10.1234/abc")));
    }

    private static TableCell BuildCell(params string[] paragraphTexts)
    {
        var cell = new TableCell();
        foreach (var text in paragraphTexts)
        {
            cell.AppendChild(PlainParagraph(text));
        }
        return cell;
    }

    private static Paragraph PlainParagraph(string text)
        => new(new Run(new Text(text) { Space = SpaceProcessingModeValues.Preserve }));

    private static Paragraph BuildAuthorsParagraph()
    {
        var nameRun = new Run(new Text("Maria Silva") { Space = SpaceProcessingModeValues.Preserve });
        var labelProperties = new RunProperties(
            new VerticalTextAlignment { Val = VerticalPositionValues.Superscript });
        var labelRun = new Run(
            labelProperties,
            new Text("1") { Space = SpaceProcessingModeValues.Preserve });
        return new Paragraph(nameRun, labelRun);
    }

    private static Paragraph BuildAbstractParagraph()
    {
        var boldRun = new Run(
            new RunProperties(new Bold()),
            new Text("Abstract") { Space = SpaceProcessingModeValues.Preserve });
        var bodyRun = new Run(
            new Text(" — body goes here") { Space = SpaceProcessingModeValues.Preserve });
        return new Paragraph(boldRun, bodyRun);
    }
}
