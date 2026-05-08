using System.Text.Json;
using DocFormatter.Core.Models;
using DocFormatter.Core.Pipeline;
using DocFormatter.Core.Reporting;
using DocFormatter.Core.Rules;
using Xunit;

namespace DocFormatter.Tests;

public sealed class DiagnosticWriterTests : IDisposable
{
    private readonly string _tempDir;

    public DiagnosticWriterTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"docfmt-diag-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, recursive: true); } catch { /* best effort */ }
    }

    [Fact]
    public void Write_OnlyInfoEntries_DoesNotProduceFile_ReturnsFalse()
    {
        var path = Path.Combine(_tempDir, "info-only.diagnostic.json");
        var report = new Report();
        report.Info("R1", "informational");
        var ctx = new FormattingContext { Doi = "10.1234/abc", ElocationId = "e2024001", ArticleTitle = "T" };

        var written = DiagnosticWriter.Write(path, "info-only.docx", ctx, report);

        Assert.False(written);
        Assert.False(File.Exists(path));
    }

    [Fact]
    public void Write_OneWarnEntry_ProducesWarningStatus_WithSingleIssue()
    {
        var path = Path.Combine(_tempDir, "warn.diagnostic.json");
        var report = new Report();
        report.Info("R0", "ignored info");
        report.Warn("ParseAuthors", "suspicious comma in author 2");
        var ctx = new FormattingContext { Doi = "10.1234/abc", ElocationId = "e2024001", ArticleTitle = "Title" };

        var written = DiagnosticWriter.Write(path, "warn.docx", ctx, report);

        Assert.True(written);
        Assert.True(File.Exists(path));
        var doc = ReadDocument(path);
        Assert.Equal("warning", doc.Status);
        Assert.Single(doc.Issues);
        Assert.Equal("ParseAuthors", doc.Issues[0].Rule);
        Assert.Equal("warn", doc.Issues[0].Level);
        Assert.Equal("suspicious comma in author 2", doc.Issues[0].Message);
    }

    [Fact]
    public void Write_WarnAndErrorEntries_StatusError_IssuesInEntryOrder()
    {
        var path = Path.Combine(_tempDir, "error.diagnostic.json");
        var report = new Report();
        report.Warn("RuleA", "first warn");
        report.Error("RuleB", "then critical");
        var ctx = new FormattingContext();

        DiagnosticWriter.Write(path, "error.docx", ctx, report);

        var doc = ReadDocument(path);
        Assert.Equal("error", doc.Status);
        Assert.Equal(2, doc.Issues.Count);
        Assert.Equal(("RuleA", "warn", "first warn"),
            (doc.Issues[0].Rule, doc.Issues[0].Level, doc.Issues[0].Message));
        Assert.Equal(("RuleB", "error", "then critical"),
            (doc.Issues[1].Rule, doc.Issues[1].Level, doc.Issues[1].Message));
    }

    [Fact]
    public void Write_DoiNull_SerializesValueNull_ConfidenceMissing()
    {
        var path = Path.Combine(_tempDir, "no-doi.diagnostic.json");
        var report = new Report();
        report.Warn("Some", "trigger writer");
        var ctx = new FormattingContext { Doi = null, ElocationId = "e2024", ArticleTitle = "T" };

        DiagnosticWriter.Write(path, "no-doi.docx", ctx, report);

        var raw = File.ReadAllText(path);
        var doc = ReadDocument(path);
        Assert.Null(doc.Fields.Doi.Value);
        Assert.Equal(FieldConfidence.Missing, doc.Fields.Doi.Confidence);
        Assert.Contains("\"doi\":", raw);
        Assert.Contains("\"confidence\": \"missing\"", raw);
    }

    [Fact]
    public void Write_AllFieldsNull_AllFieldsAreMissing_AuthorsListEmpty()
    {
        var path = Path.Combine(_tempDir, "all-missing.diagnostic.json");
        var report = new Report();
        report.Error("Bootstrap", "could not extract anything");
        var ctx = new FormattingContext();

        DiagnosticWriter.Write(path, "all-missing.docx", ctx, report);

        var doc = ReadDocument(path);
        Assert.Equal(FieldConfidence.Missing, doc.Fields.Doi.Confidence);
        Assert.Null(doc.Fields.Doi.Value);
        Assert.Equal(FieldConfidence.Missing, doc.Fields.Elocation.Confidence);
        Assert.Null(doc.Fields.Elocation.Value);
        Assert.Equal(FieldConfidence.Missing, doc.Fields.Title.Confidence);
        Assert.Null(doc.Fields.Title.Value);
        Assert.Empty(doc.Fields.Authors);
    }

    [Fact]
    public void Write_TwoAuthors_OneLowOneHigh_PreservesPerAuthorConfidence()
    {
        var path = Path.Combine(_tempDir, "authors.diagnostic.json");
        var report = new Report();
        report.Warn("ParseAuthors", "second author flagged");
        var ctx = new FormattingContext
        {
            Doi = "10.1/x",
            ElocationId = "e1",
            ArticleTitle = "T",
        };
        ctx.Authors.Add(new Author(
            "José Silva",
            new[] { "1" },
            "0000-0002-1825-0097",
            AuthorConfidence.High));
        ctx.Authors.Add(new Author(
            "Jane Doe Jr",
            new[] { "2" },
            null,
            AuthorConfidence.Low));

        DiagnosticWriter.Write(path, "authors.docx", ctx, report);

        var doc = ReadDocument(path);
        Assert.Equal(2, doc.Fields.Authors.Count);
        Assert.Equal(FieldConfidence.High, doc.Fields.Authors[0].Confidence);
        Assert.Equal("José Silva", doc.Fields.Authors[0].Name);
        Assert.Equal(new[] { "1" }, doc.Fields.Authors[0].AffiliationLabels);
        Assert.Equal("0000-0002-1825-0097", doc.Fields.Authors[0].Orcid);
        Assert.Equal(FieldConfidence.Low, doc.Fields.Authors[1].Confidence);
        Assert.Equal("Jane Doe Jr", doc.Fields.Authors[1].Name);
        Assert.Null(doc.Fields.Authors[1].Orcid);
    }

    [Fact]
    public void Write_ExtractedAt_IsIso8601UtcWithSecondsPrecision_WithinOneSecondOfNow()
    {
        var path = Path.Combine(_tempDir, "time.diagnostic.json");
        var report = new Report();
        report.Warn("R", "trigger");
        var ctx = new FormattingContext();

        var before = DateTime.UtcNow;
        DiagnosticWriter.Write(path, "time.docx", ctx, report);
        var after = DateTime.UtcNow;

        var raw = File.ReadAllText(path);
        var startIdx = raw.IndexOf("\"extractedAt\":", StringComparison.Ordinal);
        Assert.True(startIdx >= 0, "extractedAt key missing from JSON");
        var quoteOpen = raw.IndexOf('"', startIdx + "\"extractedAt\":".Length);
        var quoteClose = raw.IndexOf('"', quoteOpen + 1);
        var stamp = raw.Substring(quoteOpen + 1, quoteClose - quoteOpen - 1);
        Assert.Matches(@"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}Z$", stamp);

        var doc = ReadDocument(path);
        Assert.Equal(DateTimeKind.Utc, doc.ExtractedAt.Kind);
        var beforeFloor = TruncateToSeconds(before).AddSeconds(-1);
        var afterCeiling = TruncateToSeconds(after).AddSeconds(1);
        Assert.InRange(doc.ExtractedAt, beforeFloor, afterCeiling);
    }

    [Fact]
    public void Write_PropertyNamesAreCamelCase()
    {
        var path = Path.Combine(_tempDir, "case.diagnostic.json");
        var report = new Report();
        report.Warn("R", "trigger");
        var ctx = new FormattingContext { Doi = "10.1/x", ElocationId = "e1", ArticleTitle = "T" };
        ctx.Authors.Add(new Author("A. B.", new[] { "1" }, null, AuthorConfidence.High));

        DiagnosticWriter.Write(path, "case.docx", ctx, report);

        var raw = File.ReadAllText(path);
        Assert.Contains("\"file\":", raw);
        Assert.Contains("\"status\":", raw);
        Assert.Contains("\"extractedAt\":", raw);
        Assert.Contains("\"fields\":", raw);
        Assert.Contains("\"elocation\":", raw);
        Assert.Contains("\"affiliationLabels\":", raw);
        Assert.Contains("\"issues\":", raw);
        Assert.DoesNotContain("\"AffiliationLabels\":", raw);
        Assert.DoesNotContain("\"ExtractedAt\":", raw);
        Assert.DoesNotContain("\"Status\":", raw);
    }

    [Fact]
    public void Write_OutputIsIndented_AndContainsExpectedShape()
    {
        var path = Path.Combine(_tempDir, "shape.diagnostic.json");
        var report = new Report();
        report.Warn("R", "trigger");
        var ctx = new FormattingContext { Doi = "10.1/x", ElocationId = "e1", ArticleTitle = "T" };

        DiagnosticWriter.Write(path, "shape.docx", ctx, report);

        var raw = File.ReadAllText(path);
        Assert.Contains("\n  ", raw);
        Assert.StartsWith("{", raw);
        Assert.EndsWith("}", raw.TrimEnd());
    }

    [Fact]
    public void Write_RoundTrip_ProducesEqualDocument()
    {
        var path = Path.Combine(_tempDir, "round-trip.diagnostic.json");
        var report = new Report();
        report.Warn("ParseAuthors", "look at author 2");
        var ctx = new FormattingContext { Doi = "10.1/x", ElocationId = "e1", ArticleTitle = "T" };
        ctx.Authors.Add(new Author("A One", new[] { "1" }, "0000-0001-2345-6789", AuthorConfidence.High));
        ctx.Authors.Add(new Author("B Two", new[] { "2", "3" }, null, AuthorConfidence.Low));

        var fixedTime = new DateTime(2026, 5, 6, 12, 30, 45, DateTimeKind.Utc);
        var built = DiagnosticWriter.Build("round-trip.docx", ctx, report, fixedTime);
        DiagnosticWriter.Write(path, "round-trip.docx", ctx, report, fixedTime);

        var roundtripped = ReadDocument(path);
        Assert.Equal(built, roundtripped);
    }

    [Fact]
    public void Write_DoesNotCreateFile_WhenHighestLevelIsInfo_EvenWithExtractedFields()
    {
        var path = Path.Combine(_tempDir, "skip.diagnostic.json");
        var report = new Report();
        var ctx = new FormattingContext { Doi = "10.1/x", ElocationId = "e1", ArticleTitle = "T" };

        var written = DiagnosticWriter.Write(path, "skip.docx", ctx, report);

        Assert.False(written);
        Assert.False(File.Exists(path));
    }

    [Fact]
    public void Build_ConfidenceEnumSerializesAsLowercaseString()
    {
        var report = new Report();
        report.Warn("R", "trigger");
        var ctx = new FormattingContext { Doi = "10.1/x" };

        var doc = DiagnosticWriter.Build("any.docx", ctx, report, DateTime.UtcNow);
        var json = JsonSerializer.Serialize(doc, DiagnosticWriter.JsonOptions);

        Assert.Contains("\"confidence\": \"high\"", json);
        Assert.Contains("\"confidence\": \"missing\"", json);
        Assert.DoesNotContain("\"High\"", json);
        Assert.DoesNotContain("\"Missing\"", json);
    }

    [Fact]
    public void Build_NoPhase2RuleWarnsOrErrors_FormattingIsNull_LegacyKeysUnchanged()
    {
        var path = Path.Combine(_tempDir, "phase2-silent.diagnostic.json");
        var report = new Report();
        report.Warn("ParseAuthors", "legacy warn — none of the four phase 2 rules");
        report.Info(nameof(ApplyHeaderAlignmentRule), "alignment applied (doi=true, section=true, title=true)");
        report.Info(nameof(ExtractCorrespondingAuthorRule), ExtractCorrespondingAuthorRule.NoMarkerMessage);
        var ctx = new FormattingContext { Doi = "10.1/x", ElocationId = "e1", ArticleTitle = "T" };

        DiagnosticWriter.Write(path, "phase2-silent.docx", ctx, report);

        var doc = ReadDocument(path);
        Assert.Null(doc.Formatting);

        var raw = File.ReadAllText(path);
        Assert.Contains("\"formatting\": null", raw);
        Assert.Contains("\"file\":", raw);
        Assert.Contains("\"status\":", raw);
        Assert.Contains("\"extractedAt\":", raw);
        Assert.Contains("\"fields\":", raw);
        Assert.Contains("\"issues\":", raw);
    }

    [Fact]
    public void Build_AlignmentWarnsOnTitleOnly_AlignmentPopulated_OtherSubObjectsNull()
    {
        var report = new Report();
        report.Warn(nameof(ApplyHeaderAlignmentRule), ApplyHeaderAlignmentRule.MissingTitleParagraphMessage);
        report.Info(nameof(ApplyHeaderAlignmentRule), "alignment applied (doi=true, section=true, title=false)");
        var ctx = new FormattingContext();

        var doc = DiagnosticWriter.Build("title-warn.docx", ctx, report, DateTime.UtcNow);

        Assert.NotNull(doc.Formatting);
        Assert.NotNull(doc.Formatting!.AlignmentApplied);
        Assert.True(doc.Formatting.AlignmentApplied!.Doi);
        Assert.True(doc.Formatting.AlignmentApplied.Section);
        Assert.False(doc.Formatting.AlignmentApplied.Title);
        Assert.Null(doc.Formatting.AbstractFormatted);
        Assert.Null(doc.Formatting.AuthorBlockSpacingApplied);
        Assert.Null(doc.Formatting.CorrespondingEmail);
    }

    [Fact]
    public void Build_AlignmentWarnsOnAllThree_AllAlignmentBoolsFalse()
    {
        var report = new Report();
        report.Warn(nameof(ApplyHeaderAlignmentRule), ApplyHeaderAlignmentRule.MissingDoiParagraphMessage);
        report.Warn(nameof(ApplyHeaderAlignmentRule), ApplyHeaderAlignmentRule.MissingSectionParagraphMessage);
        report.Warn(nameof(ApplyHeaderAlignmentRule), ApplyHeaderAlignmentRule.MissingTitleParagraphMessage);
        var ctx = new FormattingContext();

        var doc = DiagnosticWriter.Build("triple-warn.docx", ctx, report, DateTime.UtcNow);

        Assert.NotNull(doc.Formatting);
        var alignment = doc.Formatting!.AlignmentApplied;
        Assert.NotNull(alignment);
        Assert.False(alignment!.Doi);
        Assert.False(alignment.Section);
        Assert.False(alignment.Title);
    }

    [Fact]
    public void Build_CorrespondingEmailExtractionFailed_ValueNull_ReasonPopulated()
    {
        var report = new Report();
        report.Warn(
            nameof(ExtractCorrespondingAuthorRule),
            ExtractCorrespondingAuthorRule.EmailExtractionFailedMessage);
        var ctx = new FormattingContext();

        var doc = DiagnosticWriter.Build("email-failed.docx", ctx, report, DateTime.UtcNow);

        Assert.NotNull(doc.Formatting);
        Assert.NotNull(doc.Formatting!.CorrespondingEmail);
        Assert.Null(doc.Formatting.CorrespondingEmail!.Value);
        Assert.Equal(
            ExtractCorrespondingAuthorRule.EmailExtractionFailedMessage,
            doc.Formatting.CorrespondingEmail.Reason);
        Assert.Null(doc.Formatting.AlignmentApplied);
        Assert.Null(doc.Formatting.AbstractFormatted);
        Assert.Null(doc.Formatting.AuthorBlockSpacingApplied);
    }

    [Fact]
    public void Build_AbstractStructuralItalicBranch_HeadingRewrittenAndDeitalicized()
    {
        var report = new Report();
        report.Warn(nameof(RewriteAbstractRule), RewriteAbstractRule.MissingSeparatorMessage);
        report.Info(nameof(RewriteAbstractRule), RewriteAbstractRule.StructuralItalicRemovedMessage);
        var ctx = new FormattingContext();

        var doc = DiagnosticWriter.Build("structural.docx", ctx, report, DateTime.UtcNow);

        Assert.NotNull(doc.Formatting);
        var abs = doc.Formatting!.AbstractFormatted;
        Assert.NotNull(abs);
        Assert.True(abs!.HeadingRewritten);
        Assert.True(abs.BodyDeitalicized);
        Assert.False(abs.InternalItalicPreserved);
    }

    [Fact]
    public void Build_AbstractMixedItalicBranch_HeadingRewrittenButItalicPreserved()
    {
        var report = new Report();
        report.Warn(nameof(RewriteAbstractRule), RewriteAbstractRule.MissingSeparatorMessage);
        report.Info(nameof(RewriteAbstractRule), RewriteAbstractRule.CanonicalLineInsertedMessage);
        var ctx = new FormattingContext();

        var doc = DiagnosticWriter.Build("mixed.docx", ctx, report, DateTime.UtcNow);

        Assert.NotNull(doc.Formatting);
        var abs = doc.Formatting!.AbstractFormatted;
        Assert.NotNull(abs);
        Assert.True(abs!.HeadingRewritten);
        Assert.False(abs.BodyDeitalicized);
        Assert.True(abs.InternalItalicPreserved);
    }

    [Fact]
    public void Build_AbstractNotFound_AllAbstractFlagsFalse()
    {
        var report = new Report();
        report.Warn(nameof(RewriteAbstractRule), RewriteAbstractRule.AbstractNotFoundMessage);
        var ctx = new FormattingContext();

        var doc = DiagnosticWriter.Build("no-abstract.docx", ctx, report, DateTime.UtcNow);

        Assert.NotNull(doc.Formatting);
        var abs = doc.Formatting!.AbstractFormatted;
        Assert.NotNull(abs);
        Assert.False(abs!.HeadingRewritten);
        Assert.False(abs.BodyDeitalicized);
        Assert.False(abs.InternalItalicPreserved);
    }

    [Fact]
    public void Build_SpacingMissingAnchorWarn_AuthorBlockSpacingFalse()
    {
        var report = new Report();
        report.Warn(
            nameof(EnsureAuthorBlockSpacingRule),
            EnsureAuthorBlockSpacingRule.MissingAuthorBlockEndMessage);
        var ctx = new FormattingContext();

        var doc = DiagnosticWriter.Build("spacing-missing.docx", ctx, report, DateTime.UtcNow);

        Assert.NotNull(doc.Formatting);
        Assert.False(doc.Formatting!.AuthorBlockSpacingApplied);
    }

    [Fact]
    public void Build_SpacingBlankAlreadyPresentInfo_AndOtherRuleWarns_AuthorBlockSpacingTrue()
    {
        var report = new Report();
        report.Warn(nameof(ApplyHeaderAlignmentRule), ApplyHeaderAlignmentRule.MissingTitleParagraphMessage);
        report.Info(
            nameof(EnsureAuthorBlockSpacingRule),
            EnsureAuthorBlockSpacingRule.BlankLineAlreadyPresentMessage);
        var ctx = new FormattingContext();

        var doc = DiagnosticWriter.Build("spacing-info.docx", ctx, report, DateTime.UtcNow);

        Assert.NotNull(doc.Formatting);
        Assert.True(doc.Formatting!.AuthorBlockSpacingApplied);
    }

    [Fact]
    public void Build_MultipleRulesContribute_AllSubObjectsPopulatedOnlyForRulesThatWarned()
    {
        var report = new Report();
        report.Warn(nameof(ApplyHeaderAlignmentRule), ApplyHeaderAlignmentRule.MissingDoiParagraphMessage);
        report.Warn(
            nameof(EnsureAuthorBlockSpacingRule),
            EnsureAuthorBlockSpacingRule.MissingAffiliationMessage);
        report.Warn(
            nameof(ExtractCorrespondingAuthorRule),
            ExtractCorrespondingAuthorRule.EmailExtractionFailedMessage);
        report.Info(nameof(RewriteAbstractRule), RewriteAbstractRule.CanonicalLineInsertedMessage);
        var ctx = new FormattingContext();

        var doc = DiagnosticWriter.Build("combo.docx", ctx, report, DateTime.UtcNow);

        Assert.NotNull(doc.Formatting);
        Assert.NotNull(doc.Formatting!.AlignmentApplied);
        Assert.False(doc.Formatting.AlignmentApplied!.Doi);
        Assert.True(doc.Formatting.AlignmentApplied.Section);
        Assert.True(doc.Formatting.AlignmentApplied.Title);
        Assert.False(doc.Formatting.AuthorBlockSpacingApplied);
        Assert.NotNull(doc.Formatting.CorrespondingEmail);
        Assert.Null(doc.Formatting.CorrespondingEmail!.Value);
        Assert.Null(doc.Formatting.AbstractFormatted);
    }

    [Fact]
    public void DiagnosticDocument_Equals_TreatsFormattingNullAndNullAsEqual()
    {
        var report = new Report();
        report.Warn("Other", "trigger");
        var ctx = new FormattingContext();

        var a = DiagnosticWriter.Build("eq.docx", ctx, report, new DateTime(2026, 5, 7, 10, 0, 0, DateTimeKind.Utc));
        var b = DiagnosticWriter.Build("eq.docx", ctx, report, new DateTime(2026, 5, 7, 10, 0, 0, DateTimeKind.Utc));

        Assert.Null(a.Formatting);
        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void DiagnosticDocument_Equals_ReturnsFalseWhenOnlyFormattingDiffers()
    {
        var fixedTime = new DateTime(2026, 5, 7, 10, 0, 0, DateTimeKind.Utc);

        var greenReport = new Report();
        greenReport.Warn("Other", "trigger");
        var ctx = new FormattingContext();
        var withoutFormatting = DiagnosticWriter.Build("eq.docx", ctx, greenReport, fixedTime);

        var warnReport = new Report();
        warnReport.Warn(nameof(ApplyHeaderAlignmentRule), ApplyHeaderAlignmentRule.MissingTitleParagraphMessage);
        warnReport.Warn("Other", "trigger");
        var withFormatting = DiagnosticWriter.Build("eq.docx", ctx, warnReport, fixedTime);

        Assert.Null(withoutFormatting.Formatting);
        Assert.NotNull(withFormatting.Formatting);
        Assert.NotEqual(withoutFormatting, withFormatting);
    }

    [Fact]
    public void Build_FormattingSerializesAsCamelCaseSubObjects()
    {
        var path = Path.Combine(_tempDir, "camel.diagnostic.json");
        var report = new Report();
        report.Warn(nameof(ApplyHeaderAlignmentRule), ApplyHeaderAlignmentRule.MissingTitleParagraphMessage);
        report.Warn(
            nameof(EnsureAuthorBlockSpacingRule),
            EnsureAuthorBlockSpacingRule.MissingAuthorBlockEndMessage);
        var ctx = new FormattingContext();

        DiagnosticWriter.Write(path, "camel.docx", ctx, report);

        var raw = File.ReadAllText(path);
        Assert.Contains("\"formatting\":", raw);
        Assert.Contains("\"alignmentApplied\":", raw);
        Assert.Contains("\"authorBlockSpacingApplied\":", raw);
        Assert.Contains("\"abstractFormatted\":", raw);
        Assert.Contains("\"correspondingEmail\":", raw);
        Assert.DoesNotContain("\"AlignmentApplied\":", raw);
    }

    [Fact]
    public void Write_RoundTrip_WithFormatting_ProducesEqualDocument()
    {
        var path = Path.Combine(_tempDir, "round-trip-formatting.diagnostic.json");
        var report = new Report();
        report.Warn(nameof(ApplyHeaderAlignmentRule), ApplyHeaderAlignmentRule.MissingTitleParagraphMessage);
        report.Warn(
            nameof(ExtractCorrespondingAuthorRule),
            ExtractCorrespondingAuthorRule.EmailExtractionFailedMessage);
        report.Info(
            nameof(EnsureAuthorBlockSpacingRule),
            EnsureAuthorBlockSpacingRule.BlankLineInsertedMessage);
        var ctx = new FormattingContext { Doi = "10.1/x", ElocationId = "e1", ArticleTitle = "T" };

        var fixedTime = new DateTime(2026, 5, 7, 12, 30, 45, DateTimeKind.Utc);
        var built = DiagnosticWriter.Build("round-trip-formatting.docx", ctx, report, fixedTime);
        DiagnosticWriter.Write(path, "round-trip-formatting.docx", ctx, report, fixedTime);

        var roundtripped = ReadDocument(path);
        Assert.Equal(built, roundtripped);
        Assert.NotNull(roundtripped.Formatting);
    }

    [Fact]
    public void Build_HistoryMove_NoEntries_ReturnsNullProperty()
    {
        var report = new Report();
        report.Warn(nameof(ApplyHeaderAlignmentRule), ApplyHeaderAlignmentRule.MissingTitleParagraphMessage);
        var ctx = new FormattingContext();

        var doc = DiagnosticWriter.Build("phase3-no-history.docx", ctx, report, DateTime.UtcNow);

        Assert.NotNull(doc.Formatting);
        Assert.Null(doc.Formatting!.HistoryMove);
        Assert.Null(doc.Formatting.SectionPromotion);
    }

    [Fact]
    public void Build_HistoryMove_MovedInfo_AppliedTrueAnchorTrueParagraphsThree()
    {
        var report = new Report();
        report.Info(
            nameof(MoveHistoryRule),
            $"{MoveHistoryRule.MovedMessagePrefix}13{MoveHistoryRule.MovedMessageOriginInfix}5)");
        var ctx = new FormattingContext();

        var doc = DiagnosticWriter.Build("phase3-moved.docx", ctx, report, DateTime.UtcNow);

        Assert.NotNull(doc.Formatting);
        var move = doc.Formatting!.HistoryMove;
        Assert.NotNull(move);
        Assert.True(move!.Applied);
        Assert.Null(move.SkippedReason);
        Assert.True(move.AnchorFound);
        Assert.Equal(3, move.ParagraphsMoved);
        Assert.Equal(13, move.ToIndexBeforeIntro);
        Assert.Equal(5, move.FromIndex);
    }

    [Fact]
    public void Build_HistoryMove_AlreadyAdjacentInfo_AppliedTrueParagraphsThree()
    {
        var report = new Report();
        report.Info(nameof(MoveHistoryRule), MoveHistoryRule.AlreadyAdjacentMessage);
        var ctx = new FormattingContext();

        var doc = DiagnosticWriter.Build("phase3-already-adjacent.docx", ctx, report, DateTime.UtcNow);

        Assert.NotNull(doc.Formatting);
        var move = doc.Formatting!.HistoryMove;
        Assert.NotNull(move);
        Assert.True(move!.Applied);
        Assert.Null(move.SkippedReason);
        Assert.True(move.AnchorFound);
        Assert.Equal(3, move.ParagraphsMoved);
        Assert.Null(move.ToIndexBeforeIntro);
    }

    [Fact]
    public void Build_HistoryMove_AnchorMissingWarn_SkippedAnchorMissingAnchorFalse()
    {
        var report = new Report();
        report.Warn(nameof(MoveHistoryRule), MoveHistoryRule.AnchorMissingMessage);
        var ctx = new FormattingContext();

        var doc = DiagnosticWriter.Build("phase3-anchor-missing.docx", ctx, report, DateTime.UtcNow);

        Assert.NotNull(doc.Formatting);
        var move = doc.Formatting!.HistoryMove;
        Assert.NotNull(move);
        Assert.False(move!.Applied);
        Assert.Equal("anchor_missing", move.SkippedReason);
        Assert.False(move.AnchorFound);
        Assert.Equal(0, move.ParagraphsMoved);
    }

    [Fact]
    public void Build_HistoryMove_PartialBlockWarn_SkippedPartialBlock()
    {
        var report = new Report();
        report.Warn(
            nameof(MoveHistoryRule),
            $"{MoveHistoryRule.PartialBlockMessagePrefix}Received=1 Accepted=0 Published=1 — not moved");
        var ctx = new FormattingContext();

        var doc = DiagnosticWriter.Build("phase3-partial.docx", ctx, report, DateTime.UtcNow);

        var move = doc.Formatting!.HistoryMove;
        Assert.NotNull(move);
        Assert.False(move!.Applied);
        Assert.Equal("partial_block", move.SkippedReason);
        Assert.True(move.AnchorFound);
    }

    [Fact]
    public void Build_HistoryMove_OutOfOrderWarn_SkippedOutOfOrder()
    {
        var report = new Report();
        report.Warn(
            nameof(MoveHistoryRule),
            $"{MoveHistoryRule.OutOfOrderMessagePrefix}(Published→Received→Accepted) — not moved");
        var ctx = new FormattingContext();

        var doc = DiagnosticWriter.Build("phase3-out-of-order.docx", ctx, report, DateTime.UtcNow);

        var move = doc.Formatting!.HistoryMove;
        Assert.NotNull(move);
        Assert.False(move!.Applied);
        Assert.Equal("out_of_order", move.SkippedReason);
        Assert.True(move.AnchorFound);
    }

    [Fact]
    public void Build_HistoryMove_NotAdjacentWarn_SkippedNotAdjacent()
    {
        var report = new Report();
        report.Warn(
            nameof(MoveHistoryRule),
            $"{MoveHistoryRule.NotAdjacentMessagePrefix}(gap of 1 non-empty paragraphs between markers) — not moved");
        var ctx = new FormattingContext();

        var doc = DiagnosticWriter.Build("phase3-not-adjacent.docx", ctx, report, DateTime.UtcNow);

        var move = doc.Formatting!.HistoryMove;
        Assert.NotNull(move);
        Assert.False(move!.Applied);
        Assert.Equal("not_adjacent", move.SkippedReason);
        Assert.True(move.AnchorFound);
    }

    [Fact]
    public void Build_HistoryMove_NotFoundInfo_SkippedNotFound()
    {
        var report = new Report();
        report.Info(nameof(MoveHistoryRule), MoveHistoryRule.NotFoundMessage);
        // Force the JSON write trigger so DiagnosticFormatting populates beyond Phase 3.
        report.Warn(nameof(ApplyHeaderAlignmentRule), ApplyHeaderAlignmentRule.MissingTitleParagraphMessage);
        var ctx = new FormattingContext();

        var doc = DiagnosticWriter.Build("phase3-not-found.docx", ctx, report, DateTime.UtcNow);

        var move = doc.Formatting!.HistoryMove;
        Assert.NotNull(move);
        Assert.False(move!.Applied);
        Assert.Equal("not_found", move.SkippedReason);
        Assert.True(move.AnchorFound);
    }

    [Fact]
    public void Build_HistoryMove_OnlyPhase3InfoEntry_ForcesFormattingPopulationWithoutPhase12Warn()
    {
        // Phase 3 INFO alone makes BuildFormatting populate the HistoryMove sub-object even when
        // no Phase 1+2 rule warned. The JSON file write trigger (HighestLevel >= Warn) is unchanged
        // and unrelated.
        var report = new Report();
        report.Info(nameof(MoveHistoryRule), MoveHistoryRule.NotFoundMessage);
        var ctx = new FormattingContext();

        var doc = DiagnosticWriter.Build("phase3-info-only.docx", ctx, report, DateTime.UtcNow);

        Assert.NotNull(doc.Formatting);
        Assert.NotNull(doc.Formatting!.HistoryMove);
        Assert.Null(doc.Formatting.AlignmentApplied);
        Assert.Null(doc.Formatting.AbstractFormatted);
        Assert.Null(doc.Formatting.AuthorBlockSpacingApplied);
        Assert.Null(doc.Formatting.CorrespondingEmail);
        Assert.Null(doc.Formatting.SectionPromotion);
    }

    [Fact]
    public void Build_SectionPromotion_NoEntries_ReturnsNullProperty()
    {
        var report = new Report();
        report.Warn(nameof(ApplyHeaderAlignmentRule), ApplyHeaderAlignmentRule.MissingTitleParagraphMessage);
        var ctx = new FormattingContext();

        var doc = DiagnosticWriter.Build("phase3-no-promotion.docx", ctx, report, DateTime.UtcNow);

        Assert.NotNull(doc.Formatting);
        Assert.Null(doc.Formatting!.SectionPromotion);
    }

    [Fact]
    public void Build_SectionPromotion_Summary_AppliedTrueWithParsedCounts()
    {
        var report = new Report();
        report.Info(
            nameof(PromoteSectionsRule),
            $"{PromoteSectionsRule.AnchorPositionMessagePrefix}14");
        report.Info(
            nameof(PromoteSectionsRule),
            $"{PromoteSectionsRule.SummaryPromotedPrefix}7{PromoteSectionsRule.SummarySectionsInfix}3{PromoteSectionsRule.SummarySubsectionsSuffix}");
        var ctx = new FormattingContext();

        var doc = DiagnosticWriter.Build("phase3-promoted.docx", ctx, report, DateTime.UtcNow);

        var promo = doc.Formatting!.SectionPromotion;
        Assert.NotNull(promo);
        Assert.True(promo!.Applied);
        Assert.Null(promo.SkippedReason);
        Assert.True(promo.AnchorFound);
        Assert.Equal(14, promo.AnchorParagraphIndex);
        Assert.Equal(7, promo.SectionsPromoted);
        Assert.Equal(3, promo.SubsectionsPromoted);
        Assert.Equal(0, promo.SkippedParagraphsInsideTables);
        Assert.Equal(0, promo.SkippedParagraphsBeforeAnchor);
    }

    [Fact]
    public void Build_SectionPromotion_AnchorMissingWarn_SkippedAnchorMissing()
    {
        var report = new Report();
        report.Warn(nameof(PromoteSectionsRule), PromoteSectionsRule.AnchorMissingMessage);
        var ctx = new FormattingContext();

        var doc = DiagnosticWriter.Build("phase3-promote-missing.docx", ctx, report, DateTime.UtcNow);

        var promo = doc.Formatting!.SectionPromotion;
        Assert.NotNull(promo);
        Assert.False(promo!.Applied);
        Assert.Equal("anchor_missing", promo.SkippedReason);
        Assert.False(promo.AnchorFound);
        Assert.Null(promo.AnchorParagraphIndex);
        Assert.Equal(0, promo.SectionsPromoted);
        Assert.Equal(0, promo.SubsectionsPromoted);
    }

    [Fact]
    public void Build_BothPhase3RulesRan_FormattingPopulatesBothSubObjects()
    {
        var report = new Report();
        report.Info(
            nameof(MoveHistoryRule),
            $"{MoveHistoryRule.MovedMessagePrefix}13{MoveHistoryRule.MovedMessageOriginInfix}5)");
        report.Info(
            nameof(PromoteSectionsRule),
            $"{PromoteSectionsRule.AnchorPositionMessagePrefix}13");
        report.Info(
            nameof(PromoteSectionsRule),
            $"{PromoteSectionsRule.SummaryPromotedPrefix}5{PromoteSectionsRule.SummarySectionsInfix}2{PromoteSectionsRule.SummarySubsectionsSuffix}");
        report.Info(
            nameof(PromoteSectionsRule),
            $"{PromoteSectionsRule.SkipCountsMessagePrefix}3{PromoteSectionsRule.SkipCountsInTablesInfix}13{PromoteSectionsRule.SkipCountsBeforeAnchorSuffix}");
        report.Warn(nameof(ApplyHeaderAlignmentRule), ApplyHeaderAlignmentRule.MissingTitleParagraphMessage);
        var ctx = new FormattingContext();

        var doc = DiagnosticWriter.Build("phase3-both.docx", ctx, report, DateTime.UtcNow);

        Assert.NotNull(doc.Formatting);
        Assert.NotNull(doc.Formatting!.HistoryMove);
        Assert.True(doc.Formatting.HistoryMove!.Applied);
        Assert.Equal(13, doc.Formatting.HistoryMove.ToIndexBeforeIntro);
        Assert.Equal(5, doc.Formatting.HistoryMove.FromIndex);
        Assert.NotNull(doc.Formatting.SectionPromotion);
        Assert.True(doc.Formatting.SectionPromotion!.Applied);
        Assert.Equal(5, doc.Formatting.SectionPromotion.SectionsPromoted);
        Assert.Equal(2, doc.Formatting.SectionPromotion.SubsectionsPromoted);
        Assert.Equal(3, doc.Formatting.SectionPromotion.SkippedParagraphsInsideTables);
        Assert.Equal(13, doc.Formatting.SectionPromotion.SkippedParagraphsBeforeAnchor);
    }

    [Fact]
    public void Build_NeitherPhase3RuleRan_BothPropertiesNull()
    {
        var report = new Report();
        report.Warn(nameof(ApplyHeaderAlignmentRule), ApplyHeaderAlignmentRule.MissingTitleParagraphMessage);
        var ctx = new FormattingContext();

        var doc = DiagnosticWriter.Build("phase3-absent.docx", ctx, report, DateTime.UtcNow);

        Assert.NotNull(doc.Formatting);
        Assert.Null(doc.Formatting!.HistoryMove);
        Assert.Null(doc.Formatting.SectionPromotion);
    }

    [Fact]
    public void Build_AllSixFormattingSubObjects_PopulatedAndJsonRoundTrips()
    {
        // Integration-style: mix Phase 1+2 entries (one INFO, one WARN per rule kind) with Phase 3
        // INFO and WARN; expect every sub-object on DiagnosticFormatting to be present and the
        // JSON file to round-trip equal.
        var path = Path.Combine(_tempDir, "phase3-integration.diagnostic.json");
        var report = new Report();
        // Phase 1+2
        report.Warn(nameof(ApplyHeaderAlignmentRule), ApplyHeaderAlignmentRule.MissingTitleParagraphMessage);
        report.Warn(nameof(EnsureAuthorBlockSpacingRule), EnsureAuthorBlockSpacingRule.MissingAuthorBlockEndMessage);
        report.Warn(nameof(RewriteAbstractRule), RewriteAbstractRule.MissingSeparatorMessage);
        report.Info(nameof(RewriteAbstractRule), RewriteAbstractRule.StructuralItalicRemovedMessage);
        report.Warn(nameof(ExtractCorrespondingAuthorRule), ExtractCorrespondingAuthorRule.EmailExtractionFailedMessage);
        // Phase 3 (one INFO, one WARN equivalent path)
        report.Info(
            nameof(MoveHistoryRule),
            $"{MoveHistoryRule.MovedMessagePrefix}11{MoveHistoryRule.MovedMessageOriginInfix}4)");
        report.Warn(nameof(PromoteSectionsRule), PromoteSectionsRule.AnchorMissingMessage);
        var ctx = new FormattingContext { Doi = "10.1/x", ElocationId = "e1", ArticleTitle = "T" };

        var fixedTime = new DateTime(2026, 5, 8, 10, 0, 0, DateTimeKind.Utc);
        var built = DiagnosticWriter.Build("phase3-integration.docx", ctx, report, fixedTime);

        Assert.NotNull(built.Formatting);
        var f = built.Formatting!;
        Assert.NotNull(f.AlignmentApplied);
        Assert.NotNull(f.AbstractFormatted);
        Assert.NotNull(f.AuthorBlockSpacingApplied);
        Assert.NotNull(f.CorrespondingEmail);
        Assert.NotNull(f.HistoryMove);
        Assert.NotNull(f.SectionPromotion);

        Assert.True(f.HistoryMove!.Applied);
        Assert.Equal(11, f.HistoryMove.ToIndexBeforeIntro);
        Assert.False(f.SectionPromotion!.Applied);
        Assert.Equal("anchor_missing", f.SectionPromotion.SkippedReason);

        DiagnosticWriter.Write(path, "phase3-integration.docx", ctx, report, fixedTime);
        var roundtripped = ReadDocument(path);
        Assert.Equal(built, roundtripped);
    }

    [Fact]
    public void Write_Phase3JsonRoundTrip_ProducesEqualDocument()
    {
        var path = Path.Combine(_tempDir, "phase3-round-trip.diagnostic.json");
        var report = new Report();
        report.Info(
            nameof(MoveHistoryRule),
            $"{MoveHistoryRule.MovedMessagePrefix}9{MoveHistoryRule.MovedMessageOriginInfix}3)");
        report.Info(
            nameof(PromoteSectionsRule),
            $"{PromoteSectionsRule.AnchorPositionMessagePrefix}9");
        report.Info(
            nameof(PromoteSectionsRule),
            $"{PromoteSectionsRule.SummaryPromotedPrefix}4{PromoteSectionsRule.SummarySectionsInfix}1{PromoteSectionsRule.SummarySubsectionsSuffix}");
        report.Info(
            nameof(PromoteSectionsRule),
            $"{PromoteSectionsRule.SkipCountsMessagePrefix}0{PromoteSectionsRule.SkipCountsInTablesInfix}9{PromoteSectionsRule.SkipCountsBeforeAnchorSuffix}");
        report.Warn(nameof(ApplyHeaderAlignmentRule), ApplyHeaderAlignmentRule.MissingTitleParagraphMessage);
        var ctx = new FormattingContext { Doi = "10.1/x", ElocationId = "e1", ArticleTitle = "T" };

        var fixedTime = new DateTime(2026, 5, 8, 12, 30, 45, DateTimeKind.Utc);
        var built = DiagnosticWriter.Build("phase3-round-trip.docx", ctx, report, fixedTime);
        DiagnosticWriter.Write(path, "phase3-round-trip.docx", ctx, report, fixedTime);

        var roundtripped = ReadDocument(path);
        Assert.Equal(built, roundtripped);
        Assert.NotNull(roundtripped.Formatting);
        Assert.NotNull(roundtripped.Formatting!.HistoryMove);
        Assert.NotNull(roundtripped.Formatting.SectionPromotion);

        var raw = File.ReadAllText(path);
        Assert.Contains("\"historyMove\":", raw);
        Assert.Contains("\"sectionPromotion\":", raw);
        Assert.Contains("\"applied\": true", raw);
        Assert.Contains("\"sectionsPromoted\": 4", raw);
        Assert.Contains("\"subsectionsPromoted\": 1", raw);
    }

    private static DiagnosticDocument ReadDocument(string path)
    {
        var raw = File.ReadAllText(path);
        return JsonSerializer.Deserialize<DiagnosticDocument>(raw, DiagnosticWriter.JsonOptions)
            ?? throw new InvalidOperationException("deserialization returned null");
    }

    private static DateTime TruncateToSeconds(DateTime value)
    {
        var utc = value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime();
        return new DateTime(
            utc.Year, utc.Month, utc.Day,
            utc.Hour, utc.Minute, utc.Second,
            DateTimeKind.Utc);
    }
}
