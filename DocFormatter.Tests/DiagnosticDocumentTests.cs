using System.Text.Json;
using DocFormatter.Core.Reporting;
using Xunit;

namespace DocFormatter.Tests;

public sealed class DiagnosticDocumentTests
{
    [Fact]
    public void DiagnosticHistoryMove_Construction_ExposesProvidedValues()
    {
        var move = new DiagnosticHistoryMove(
            Applied: true,
            SkippedReason: null,
            AnchorFound: true,
            FromIndex: 9,
            ToIndexBeforeIntro: 13,
            ParagraphsMoved: 3);

        Assert.True(move.Applied);
        Assert.Null(move.SkippedReason);
        Assert.True(move.AnchorFound);
        Assert.Equal(9, move.FromIndex);
        Assert.Equal(13, move.ToIndexBeforeIntro);
        Assert.Equal(3, move.ParagraphsMoved);
    }

    [Fact]
    public void DiagnosticHistoryMove_ValueEquality_HoldsForIdenticalFields()
    {
        var a = new DiagnosticHistoryMove(true, null, true, 9, 13, 3);
        var b = new DiagnosticHistoryMove(true, null, true, 9, 13, 3);

        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void DiagnosticHistoryMove_ValueEquality_DiffersWhenSingleFieldChanges()
    {
        var a = new DiagnosticHistoryMove(true, null, true, 9, 13, 3);
        var b = new DiagnosticHistoryMove(true, null, true, 9, 13, 2);

        Assert.NotEqual(a, b);
    }

    [Fact]
    public void DiagnosticSectionPromotion_Construction_ExposesProvidedValues()
    {
        var promotion = new DiagnosticSectionPromotion(
            Applied: true,
            SkippedReason: null,
            AnchorFound: true,
            AnchorParagraphIndex: 14,
            SectionsPromoted: 7,
            SubsectionsPromoted: 3,
            SkippedParagraphsInsideTables: 18,
            SkippedParagraphsBeforeAnchor: 2);

        Assert.True(promotion.Applied);
        Assert.Null(promotion.SkippedReason);
        Assert.True(promotion.AnchorFound);
        Assert.Equal(14, promotion.AnchorParagraphIndex);
        Assert.Equal(7, promotion.SectionsPromoted);
        Assert.Equal(3, promotion.SubsectionsPromoted);
        Assert.Equal(18, promotion.SkippedParagraphsInsideTables);
        Assert.Equal(2, promotion.SkippedParagraphsBeforeAnchor);
    }

    [Fact]
    public void DiagnosticSectionPromotion_ValueEquality_HoldsForIdenticalFields()
    {
        var a = new DiagnosticSectionPromotion(true, null, true, 14, 7, 3, 18, 2);
        var b = new DiagnosticSectionPromotion(true, null, true, 14, 7, 3, 18, 2);

        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void DiagnosticFormatting_Construction_PlacesNewPropertiesAtEnd()
    {
        var formatting = new DiagnosticFormatting(
            AlignmentApplied: new DiagnosticAlignment(true, true, true),
            AbstractFormatted: null,
            AuthorBlockSpacingApplied: true,
            CorrespondingEmail: null,
            HistoryMove: null,
            SectionPromotion: null);

        Assert.NotNull(formatting.AlignmentApplied);
        Assert.True(formatting.AlignmentApplied!.Doi);
        Assert.Null(formatting.AbstractFormatted);
        Assert.True(formatting.AuthorBlockSpacingApplied);
        Assert.Null(formatting.CorrespondingEmail);
        Assert.Null(formatting.HistoryMove);
        Assert.Null(formatting.SectionPromotion);
    }

    [Fact]
    public void DiagnosticFormatting_NullPhase3Fields_RoundTripPreservesNulls()
    {
        var formatting = new DiagnosticFormatting(
            AlignmentApplied: new DiagnosticAlignment(true, true, true),
            AbstractFormatted: new DiagnosticAbstract(true, true, false),
            AuthorBlockSpacingApplied: true,
            CorrespondingEmail: new DiagnosticCorrespondingEmail("editor@example.org", null),
            HistoryMove: null,
            SectionPromotion: null);

        var json = JsonSerializer.Serialize(formatting, DiagnosticWriter.JsonOptions);
        var roundtripped = JsonSerializer.Deserialize<DiagnosticFormatting>(json, DiagnosticWriter.JsonOptions);

        Assert.NotNull(roundtripped);
        Assert.Equal(formatting, roundtripped);
        Assert.Null(roundtripped!.HistoryMove);
        Assert.Null(roundtripped.SectionPromotion);
    }

    [Fact]
    public void DiagnosticFormatting_PopulatedPhase3Fields_SerializeUnderCamelCaseKeys()
    {
        var formatting = new DiagnosticFormatting(
            AlignmentApplied: null,
            AbstractFormatted: null,
            AuthorBlockSpacingApplied: null,
            CorrespondingEmail: null,
            HistoryMove: new DiagnosticHistoryMove(true, null, true, 9, 13, 3),
            SectionPromotion: new DiagnosticSectionPromotion(true, null, true, 14, 7, 3, 18, 2));

        var json = JsonSerializer.Serialize(formatting, DiagnosticWriter.JsonOptions);

        Assert.Contains("\"historyMove\":", json);
        Assert.Contains("\"sectionPromotion\":", json);
        Assert.Contains("\"applied\": true", json);
        Assert.Contains("\"anchorFound\": true", json);
        Assert.Contains("\"fromIndex\": 9", json);
        Assert.Contains("\"toIndexBeforeIntro\": 13", json);
        Assert.Contains("\"paragraphsMoved\": 3", json);
        Assert.Contains("\"anchorParagraphIndex\": 14", json);
        Assert.Contains("\"sectionsPromoted\": 7", json);
        Assert.Contains("\"subsectionsPromoted\": 3", json);
        Assert.Contains("\"skippedParagraphsInsideTables\": 18", json);
        Assert.Contains("\"skippedParagraphsBeforeAnchor\": 2", json);
    }

    [Theory]
    [InlineData("anchor_missing")]
    [InlineData("partial_block")]
    [InlineData("out_of_order")]
    [InlineData("not_adjacent")]
    [InlineData("not_found")]
    public void DiagnosticHistoryMove_SkippedReason_RoundTripsThroughJson(string reason)
    {
        var move = new DiagnosticHistoryMove(
            Applied: false,
            SkippedReason: reason,
            AnchorFound: false,
            FromIndex: null,
            ToIndexBeforeIntro: null,
            ParagraphsMoved: 0);

        var json = JsonSerializer.Serialize(move, DiagnosticWriter.JsonOptions);
        var roundtripped = JsonSerializer.Deserialize<DiagnosticHistoryMove>(json, DiagnosticWriter.JsonOptions);

        Assert.NotNull(roundtripped);
        Assert.Equal(reason, roundtripped!.SkippedReason);
        Assert.Equal(move, roundtripped);
    }

    [Theory]
    [InlineData("anchor_missing")]
    [InlineData("partial_block")]
    [InlineData("out_of_order")]
    [InlineData("not_adjacent")]
    [InlineData("not_found")]
    public void DiagnosticSectionPromotion_SkippedReason_RoundTripsThroughJson(string reason)
    {
        var promotion = new DiagnosticSectionPromotion(
            Applied: false,
            SkippedReason: reason,
            AnchorFound: false,
            AnchorParagraphIndex: null,
            SectionsPromoted: 0,
            SubsectionsPromoted: 0,
            SkippedParagraphsInsideTables: 0,
            SkippedParagraphsBeforeAnchor: 0);

        var json = JsonSerializer.Serialize(promotion, DiagnosticWriter.JsonOptions);
        var roundtripped = JsonSerializer.Deserialize<DiagnosticSectionPromotion>(json, DiagnosticWriter.JsonOptions);

        Assert.NotNull(roundtripped);
        Assert.Equal(reason, roundtripped!.SkippedReason);
        Assert.Equal(promotion, roundtripped);
    }

    [Fact]
    public void DiagnosticDocument_FullyPopulatedFormatting_RoundTripsPhase3Fields()
    {
        var document = new DiagnosticDocument(
            File: "phase3.docx",
            Status: "warning",
            ExtractedAt: new DateTime(2026, 5, 8, 12, 30, 45, DateTimeKind.Utc),
            Fields: new DiagnosticFields(
                Doi: new DiagnosticField("10.1/x", FieldConfidence.High),
                Elocation: new DiagnosticField("e1", FieldConfidence.High),
                Title: new DiagnosticField("T", FieldConfidence.High),
                Authors: Array.Empty<DiagnosticAuthor>()),
            Formatting: new DiagnosticFormatting(
                AlignmentApplied: new DiagnosticAlignment(true, true, true),
                AbstractFormatted: new DiagnosticAbstract(true, true, false),
                AuthorBlockSpacingApplied: true,
                CorrespondingEmail: new DiagnosticCorrespondingEmail("editor@example.org", null),
                HistoryMove: new DiagnosticHistoryMove(true, null, true, 9, 13, 3),
                SectionPromotion: new DiagnosticSectionPromotion(true, null, true, 14, 7, 3, 18, 2)),
            Issues: Array.Empty<DiagnosticIssue>());

        var json = JsonSerializer.Serialize(document, DiagnosticWriter.JsonOptions);
        var roundtripped = JsonSerializer.Deserialize<DiagnosticDocument>(json, DiagnosticWriter.JsonOptions);

        Assert.NotNull(roundtripped);
        Assert.Equal(document, roundtripped);
        Assert.NotNull(roundtripped!.Formatting);
        Assert.NotNull(roundtripped.Formatting!.HistoryMove);
        Assert.NotNull(roundtripped.Formatting.SectionPromotion);
        Assert.Equal(3, roundtripped.Formatting.HistoryMove!.ParagraphsMoved);
        Assert.Equal(7, roundtripped.Formatting.SectionPromotion!.SectionsPromoted);
    }
}
