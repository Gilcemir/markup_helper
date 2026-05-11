using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using DocFormatter.Core.Models;
using DocFormatter.Core.Pipeline;
using DocFormatter.Core.Rules;
using DocFormatter.Core.Rules.Phase2;

namespace DocFormatter.Core.Reporting;

public static class DiagnosticWriter
{
    private static readonly JsonSerializerOptions SerializerOptions = BuildOptions();

    public static bool Write(string filePath, string sourceFileName, FormattingContext ctx, IReport report)
        => Write(filePath, sourceFileName, ctx, report, DateTime.UtcNow);

    public static bool Write(
        string filePath,
        string sourceFileName,
        FormattingContext ctx,
        IReport report,
        DateTime extractedAt)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath);
        ArgumentException.ThrowIfNullOrEmpty(sourceFileName);
        ArgumentNullException.ThrowIfNull(ctx);
        ArgumentNullException.ThrowIfNull(report);

        if (report.HighestLevel < ReportLevel.Warn)
        {
            return false;
        }

        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var document = Build(sourceFileName, ctx, report, extractedAt);
        var json = JsonSerializer.Serialize(document, SerializerOptions);
        File.WriteAllText(filePath, json);
        return true;
    }

    public static DiagnosticDocument Build(
        string sourceFileName,
        FormattingContext ctx,
        IReport report,
        DateTime extractedAt)
    {
        ArgumentException.ThrowIfNullOrEmpty(sourceFileName);
        ArgumentNullException.ThrowIfNull(ctx);
        ArgumentNullException.ThrowIfNull(report);

        return new DiagnosticDocument(
            File: sourceFileName,
            Status: MapStatus(report.HighestLevel),
            ExtractedAt: TruncateToSeconds(extractedAt),
            Fields: BuildFields(ctx),
            Formatting: BuildFormatting(report),
            Issues: BuildIssues(report),
            Phase2: BuildPhase2(ctx, report));
    }

    private static DiagnosticPhase2? BuildPhase2(FormattingContext ctx, IReport report)
    {
        var elocationEntries = FilterByRule(report, nameof(EmitElocationTagRule));
        var abstractEntries = FilterByRule(report, nameof(EmitAbstractTagRule));
        var keywordsEntries = FilterByRule(report, nameof(EmitKwdgrpTagRule));
        var correspEntries = FilterByRule(report, nameof(EmitCorrespTagRule));
        var authorXrefEntries = FilterByRule(report, nameof(EmitAuthorXrefsRule));
        var histEntries = FilterByRule(report, nameof(EmitHistTagRule));

        if (elocationEntries.Count == 0
            && abstractEntries.Count == 0
            && keywordsEntries.Count == 0
            && correspEntries.Count == 0
            && authorXrefEntries.Count == 0
            && histEntries.Count == 0)
        {
            return null;
        }

        return new DiagnosticPhase2(
            Elocation: BuildElocationDiagnostic(ctx, elocationEntries),
            Abstract: BuildAbstractDiagnostic(ctx, abstractEntries),
            Keywords: BuildKeywordsDiagnostic(ctx, keywordsEntries),
            Corresp: BuildCorrespDiagnostic(ctx, correspEntries),
            Xref: BuildAuthorXrefDiagnostic(ctx, authorXrefEntries),
            Hist: BuildHistDiagnostic(ctx, histEntries));
    }

    private static DiagnosticField BuildHistDiagnostic(
        FormattingContext ctx,
        IReadOnlyList<ReportEntry> entries)
    {
        // Skip-and-warn on Received (the only required child) blocks any
        // emission — see EmitHistTagRule.{HistReceivedMissingMessage,
        // HistReceivedUnparseableMessage}. Optional warns about Accepted or
        // Published unparseability do not zero out the diagnostic because the
        // rule still emitted a [hist] block with Received.
        var receivedFailed = HasWarnMessage(entries, EmitHistTagRule.HistReceivedMissingMessage)
            || HasWarnMessage(entries, EmitHistTagRule.HistReceivedUnparseableMessage);
        if (receivedFailed || ctx.History is null)
        {
            return new DiagnosticField(null, FieldConfidence.Missing);
        }

        var parts = new List<string>(3) { $"received={ctx.History.Received.ToDateIso()}" };
        if (ctx.History.Accepted is not null)
        {
            parts.Add($"accepted={ctx.History.Accepted.ToDateIso()}");
        }
        if (ctx.History.Published is not null)
        {
            parts.Add($"published={ctx.History.Published.ToDateIso()}");
        }
        return new DiagnosticField(string.Join(",", parts), FieldConfidence.High);
    }

    private static DiagnosticField BuildCorrespDiagnostic(
        FormattingContext ctx,
        IReadOnlyList<ReportEntry> entries)
    {
        if (HasWarnOrError(entries) || ctx.CorrespAuthor is null)
        {
            return new DiagnosticField(null, FieldConfidence.Missing);
        }

        var summary = ctx.CorrespAuthor.Email ?? ctx.CorrespAuthor.Orcid ?? "c1";
        return new DiagnosticField(summary, FieldConfidence.High);
    }

    private static IReadOnlyList<DiagnosticAuthorXref> BuildAuthorXrefDiagnostic(
        FormattingContext ctx,
        IReadOnlyList<ReportEntry> entries)
    {
        if (HasWarnOrError(entries) || ctx.Authors.Count == 0)
        {
            return Array.Empty<DiagnosticAuthorXref>();
        }

        var result = new List<DiagnosticAuthorXref>(ctx.Authors.Count);
        for (var i = 0; i < ctx.Authors.Count; i++)
        {
            var author = ctx.Authors[i];
            result.Add(new DiagnosticAuthorXref(
                AuthorIndex: i,
                Affiliations: author.AffiliationLabels.ToArray(),
                Corresp: ctx.CorrespondingAuthorIndex == i,
                HasAuthorid: !string.IsNullOrEmpty(author.OrcidId)));
        }
        return result;
    }

    private static DiagnosticField BuildElocationDiagnostic(
        FormattingContext ctx,
        IReadOnlyList<ReportEntry> entries)
    {
        if (HasWarnOrError(entries))
        {
            return new DiagnosticField(null, FieldConfidence.Missing);
        }

        return string.IsNullOrEmpty(ctx.ElocationId)
            ? new DiagnosticField(null, FieldConfidence.Missing)
            : new DiagnosticField(ctx.ElocationId, FieldConfidence.High);
    }

    private static DiagnosticField BuildAbstractDiagnostic(
        FormattingContext ctx,
        IReadOnlyList<ReportEntry> entries)
    {
        if (HasWarnOrError(entries) || ctx.Abstract is null)
        {
            return new DiagnosticField(null, FieldConfidence.Missing);
        }

        return new DiagnosticField(ctx.Abstract.Language, FieldConfidence.High);
    }

    private static DiagnosticField BuildKeywordsDiagnostic(
        FormattingContext ctx,
        IReadOnlyList<ReportEntry> entries)
    {
        if (HasWarnOrError(entries) || ctx.Keywords is null)
        {
            return new DiagnosticField(null, FieldConfidence.Missing);
        }

        var summary = string.Join(", ", ctx.Keywords.Keywords);
        return new DiagnosticField(
            string.IsNullOrEmpty(summary) ? ctx.Keywords.Language : summary,
            FieldConfidence.High);
    }

    public static JsonSerializerOptions JsonOptions => SerializerOptions;

    private static DiagnosticFields BuildFields(FormattingContext ctx)
    {
        return new DiagnosticFields(
            Doi: BuildExtractedField(ctx.Doi),
            Elocation: BuildExtractedField(ctx.ElocationId),
            Title: BuildExtractedField(ctx.ArticleTitle),
            Authors: ctx.Authors.Select(BuildAuthor).ToArray());
    }

    private static DiagnosticField BuildExtractedField(string? value)
    {
        return string.IsNullOrEmpty(value)
            ? new DiagnosticField(null, FieldConfidence.Missing)
            : new DiagnosticField(value, FieldConfidence.High);
    }

    private static DiagnosticAuthor BuildAuthor(Author author)
    {
        return new DiagnosticAuthor(
            Name: author.Name,
            AffiliationLabels: author.AffiliationLabels.ToArray(),
            Orcid: author.OrcidId,
            Confidence: MapConfidence(author.Confidence));
    }

    private static DiagnosticFormatting? BuildFormatting(IReport report)
    {
        var alignment = FilterByRule(report, nameof(ApplyHeaderAlignmentRule));
        var spacing = FilterByRule(report, nameof(EnsureAuthorBlockSpacingRule));
        var email = FilterByRule(report, nameof(ExtractCorrespondingAuthorRule));
        var abs = FilterByRule(report, nameof(RewriteAbstractRule));
        var historyMoveEntries = FilterByRule(report, nameof(MoveHistoryRule));
        var sectionPromotionEntries = FilterByRule(report, nameof(PromoteSectionsRule));

        var hasPhase12Signal = HasWarnOrError(alignment)
            || HasWarnOrError(spacing)
            || HasWarnOrError(email)
            || HasWarnOrError(abs);
        var hasPhase3Signal = historyMoveEntries.Count > 0 || sectionPromotionEntries.Count > 0;
        if (!hasPhase12Signal && !hasPhase3Signal)
        {
            return null;
        }

        return new DiagnosticFormatting(
            AlignmentApplied: BuildAlignment(alignment),
            AbstractFormatted: BuildAbstract(abs),
            AuthorBlockSpacingApplied: BuildSpacingApplied(spacing),
            CorrespondingEmail: BuildCorrespondingEmail(email),
            HistoryMove: BuildHistoryMove(historyMoveEntries),
            SectionPromotion: BuildSectionPromotion(sectionPromotionEntries));
    }

    private static List<ReportEntry> FilterByRule(IReport report, string rule)
    {
        var filtered = new List<ReportEntry>();
        foreach (var entry in report.Entries)
        {
            if (string.Equals(entry.Rule, rule, StringComparison.Ordinal))
            {
                filtered.Add(entry);
            }
        }

        return filtered;
    }

    private static bool HasWarnOrError(IReadOnlyList<ReportEntry> entries)
    {
        foreach (var entry in entries)
        {
            if (entry.Level >= ReportLevel.Warn)
            {
                return true;
            }
        }

        return false;
    }

    private static DiagnosticAlignment? BuildAlignment(IReadOnlyList<ReportEntry> entries)
    {
        if (!HasWarnOrError(entries))
        {
            return null;
        }

        var doiOk = !HasWarnMessage(entries, ApplyHeaderAlignmentRule.MissingDoiParagraphMessage);
        var sectionOk = !HasWarnMessage(entries, ApplyHeaderAlignmentRule.MissingSectionParagraphMessage);
        var titleOk = !HasWarnMessage(entries, ApplyHeaderAlignmentRule.MissingTitleParagraphMessage);
        return new DiagnosticAlignment(doiOk, sectionOk, titleOk);
    }

    private static DiagnosticAbstract? BuildAbstract(IReadOnlyList<ReportEntry> entries)
    {
        if (!HasWarnOrError(entries))
        {
            return null;
        }

        if (HasWarnMessage(entries, RewriteAbstractRule.AbstractNotFoundMessage))
        {
            return new DiagnosticAbstract(
                HeadingRewritten: false,
                BodyDeitalicized: false,
                InternalItalicPreserved: false);
        }

        var stripped = HasInfoMessage(entries, RewriteAbstractRule.StructuralItalicRemovedMessage);
        return new DiagnosticAbstract(
            HeadingRewritten: true,
            BodyDeitalicized: stripped,
            InternalItalicPreserved: !stripped);
    }

    private static bool? BuildSpacingApplied(IReadOnlyList<ReportEntry> entries)
    {
        if (entries.Count == 0)
        {
            return null;
        }

        return !HasWarnOrError(entries);
    }

    private static DiagnosticCorrespondingEmail? BuildCorrespondingEmail(IReadOnlyList<ReportEntry> entries)
    {
        ReportEntry? failure = null;
        foreach (var entry in entries)
        {
            if (entry.Level == ReportLevel.Warn
                && string.Equals(
                    entry.Message,
                    ExtractCorrespondingAuthorRule.EmailExtractionFailedMessage,
                    StringComparison.Ordinal))
            {
                failure = entry;
                break;
            }
        }

        if (failure is null)
        {
            return null;
        }

        return new DiagnosticCorrespondingEmail(Value: null, Reason: failure.Message);
    }

    // Reconstructs the Phase 3 history-move diagnostic from MoveHistoryRule's report entries.
    // ToIndexBeforeIntro and FromIndex are parsed from MovedMessagePrefix when the move applied;
    // ParagraphsMoved follows ADR-002 (always 3 on success or already-adjacent, 0 otherwise).
    private static DiagnosticHistoryMove? BuildHistoryMove(IReadOnlyList<ReportEntry> entries)
    {
        if (entries.Count == 0)
        {
            return null;
        }

        foreach (var entry in entries)
        {
            if (entry.Level == ReportLevel.Warn)
            {
                if (string.Equals(entry.Message, MoveHistoryRule.AnchorMissingMessage, StringComparison.Ordinal))
                {
                    return new DiagnosticHistoryMove(
                        Applied: false,
                        SkippedReason: "anchor_missing",
                        AnchorFound: false,
                        FromIndex: null,
                        ToIndexBeforeIntro: null,
                        ParagraphsMoved: 0);
                }

                if (entry.Message.StartsWith(MoveHistoryRule.PartialBlockMessagePrefix, StringComparison.Ordinal))
                {
                    return new DiagnosticHistoryMove(
                        Applied: false,
                        SkippedReason: "partial_block",
                        AnchorFound: true,
                        FromIndex: null,
                        ToIndexBeforeIntro: null,
                        ParagraphsMoved: 0);
                }

                if (entry.Message.StartsWith(MoveHistoryRule.OutOfOrderMessagePrefix, StringComparison.Ordinal))
                {
                    return new DiagnosticHistoryMove(
                        Applied: false,
                        SkippedReason: "out_of_order",
                        AnchorFound: true,
                        FromIndex: null,
                        ToIndexBeforeIntro: null,
                        ParagraphsMoved: 0);
                }

                if (entry.Message.StartsWith(MoveHistoryRule.NotAdjacentMessagePrefix, StringComparison.Ordinal))
                {
                    return new DiagnosticHistoryMove(
                        Applied: false,
                        SkippedReason: "not_adjacent",
                        AnchorFound: true,
                        FromIndex: null,
                        ToIndexBeforeIntro: null,
                        ParagraphsMoved: 0);
                }
            }
            else if (entry.Level == ReportLevel.Info)
            {
                if (entry.Message.StartsWith(MoveHistoryRule.MovedMessagePrefix, StringComparison.Ordinal)
                    && TryParseMovedIndices(entry.Message, out var toIndex, out var fromIndex))
                {
                    return new DiagnosticHistoryMove(
                        Applied: true,
                        SkippedReason: null,
                        AnchorFound: true,
                        FromIndex: fromIndex,
                        ToIndexBeforeIntro: toIndex,
                        ParagraphsMoved: 3);
                }

                if (string.Equals(entry.Message, MoveHistoryRule.AlreadyAdjacentMessage, StringComparison.Ordinal))
                {
                    return new DiagnosticHistoryMove(
                        Applied: true,
                        SkippedReason: null,
                        AnchorFound: true,
                        FromIndex: null,
                        ToIndexBeforeIntro: null,
                        ParagraphsMoved: 3);
                }

                if (string.Equals(entry.Message, MoveHistoryRule.NotFoundMessage, StringComparison.Ordinal))
                {
                    return new DiagnosticHistoryMove(
                        Applied: false,
                        SkippedReason: "not_found",
                        AnchorFound: true,
                        FromIndex: null,
                        ToIndexBeforeIntro: null,
                        ParagraphsMoved: 0);
                }
            }
        }

        return new DiagnosticHistoryMove(
            Applied: false,
            SkippedReason: "unknown",
            AnchorFound: true,
            FromIndex: null,
            ToIndexBeforeIntro: null,
            ParagraphsMoved: 0);
    }

    private static bool TryParseMovedIndices(string message, out int? toIndex, out int? fromIndex)
    {
        toIndex = null;
        fromIndex = null;

        var prefix = MoveHistoryRule.MovedMessagePrefix;
        var infix = MoveHistoryRule.MovedMessageOriginInfix;
        if (!message.StartsWith(prefix, StringComparison.Ordinal)
            || !message.EndsWith(")", StringComparison.Ordinal))
        {
            return false;
        }

        var infixStart = message.IndexOf(infix, prefix.Length, StringComparison.Ordinal);
        if (infixStart < 0)
        {
            return false;
        }

        var toToken = message.AsSpan(prefix.Length, infixStart - prefix.Length);
        var fromTokenStart = infixStart + infix.Length;
        var fromTokenLength = message.Length - 1 - fromTokenStart;
        if (fromTokenLength <= 0)
        {
            return false;
        }

        var fromToken = message.AsSpan(fromTokenStart, fromTokenLength);
        if (int.TryParse(toToken, NumberStyles.Integer, CultureInfo.InvariantCulture, out var toValue))
        {
            toIndex = toValue;
        }

        if (int.TryParse(fromToken, NumberStyles.Integer, CultureInfo.InvariantCulture, out var fromValue))
        {
            fromIndex = fromValue;
        }

        return toIndex.HasValue;
    }

    // Reconstructs the Phase 3 section-promotion diagnostic from PromoteSectionsRule's entries.
    // The rule emits three INFO messages on success (anchor position, promotion summary, skip
    // counts) and a single WARN on anchor_missing.
    private static DiagnosticSectionPromotion? BuildSectionPromotion(IReadOnlyList<ReportEntry> entries)
    {
        if (entries.Count == 0)
        {
            return null;
        }

        int? anchorParagraphIndex = null;
        var sectionsPromoted = 0;
        var subsectionsPromoted = 0;
        var skippedInsideTables = 0;
        var skippedBeforeAnchor = 0;
        var hasSummary = false;

        foreach (var entry in entries)
        {
            if (entry.Level == ReportLevel.Warn
                && string.Equals(entry.Message, PromoteSectionsRule.AnchorMissingMessage, StringComparison.Ordinal))
            {
                return new DiagnosticSectionPromotion(
                    Applied: false,
                    SkippedReason: "anchor_missing",
                    AnchorFound: false,
                    AnchorParagraphIndex: null,
                    SectionsPromoted: 0,
                    SubsectionsPromoted: 0,
                    SkippedParagraphsInsideTables: 0,
                    SkippedParagraphsBeforeAnchor: 0);
            }

            if (entry.Level != ReportLevel.Info)
            {
                continue;
            }

            if (entry.Message.StartsWith(PromoteSectionsRule.AnchorPositionMessagePrefix, StringComparison.Ordinal))
            {
                anchorParagraphIndex = ParseTrailingInteger(
                    entry.Message,
                    PromoteSectionsRule.AnchorPositionMessagePrefix.Length);
            }
            else if (entry.Message.StartsWith(PromoteSectionsRule.SummaryPromotedPrefix, StringComparison.Ordinal)
                && TryParsePromotionSummary(entry.Message, out var sections, out var subsections))
            {
                sectionsPromoted = sections;
                subsectionsPromoted = subsections;
                hasSummary = true;
            }
            else if (entry.Message.StartsWith(PromoteSectionsRule.SkipCountsMessagePrefix, StringComparison.Ordinal)
                && TryParseSkipCounts(entry.Message, out var inTables, out var beforeAnchor))
            {
                skippedInsideTables = inTables;
                skippedBeforeAnchor = beforeAnchor;
            }
        }

        if (!hasSummary && anchorParagraphIndex is null)
        {
            return new DiagnosticSectionPromotion(
                Applied: false,
                SkippedReason: "unknown",
                AnchorFound: true,
                AnchorParagraphIndex: null,
                SectionsPromoted: 0,
                SubsectionsPromoted: 0,
                SkippedParagraphsInsideTables: 0,
                SkippedParagraphsBeforeAnchor: 0);
        }

        return new DiagnosticSectionPromotion(
            Applied: true,
            SkippedReason: null,
            AnchorFound: true,
            AnchorParagraphIndex: anchorParagraphIndex,
            SectionsPromoted: sectionsPromoted,
            SubsectionsPromoted: subsectionsPromoted,
            SkippedParagraphsInsideTables: skippedInsideTables,
            SkippedParagraphsBeforeAnchor: skippedBeforeAnchor);
    }

    private static bool TryParseSkipCounts(string message, out int inTables, out int beforeAnchor)
    {
        inTables = 0;
        beforeAnchor = 0;

        var prefix = PromoteSectionsRule.SkipCountsMessagePrefix;
        var infix = PromoteSectionsRule.SkipCountsInTablesInfix;
        var suffix = PromoteSectionsRule.SkipCountsBeforeAnchorSuffix;

        if (!message.StartsWith(prefix, StringComparison.Ordinal)
            || !message.EndsWith(suffix, StringComparison.Ordinal))
        {
            return false;
        }

        var infixStart = message.IndexOf(infix, prefix.Length, StringComparison.Ordinal);
        if (infixStart < 0)
        {
            return false;
        }

        var suffixStart = message.Length - suffix.Length;
        if (infixStart + infix.Length > suffixStart)
        {
            return false;
        }

        var inTablesToken = message.AsSpan(prefix.Length, infixStart - prefix.Length);
        var beforeAnchorToken = message.AsSpan(infixStart + infix.Length, suffixStart - (infixStart + infix.Length));

        if (!int.TryParse(inTablesToken, NumberStyles.Integer, CultureInfo.InvariantCulture, out inTables))
        {
            return false;
        }

        if (!int.TryParse(beforeAnchorToken, NumberStyles.Integer, CultureInfo.InvariantCulture, out beforeAnchor))
        {
            return false;
        }

        return true;
    }

    private static int? ParseTrailingInteger(string message, int startIndex, char? trailingChar = null)
    {
        if (startIndex < 0 || startIndex > message.Length)
        {
            return null;
        }

        var slice = message.AsSpan(startIndex);
        if (trailingChar.HasValue && slice.Length > 0 && slice[^1] == trailingChar.Value)
        {
            slice = slice[..^1];
        }

        return int.TryParse(slice, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
            ? value
            : null;
    }

    private static bool TryParsePromotionSummary(string message, out int sections, out int subsections)
    {
        sections = 0;
        subsections = 0;

        var prefix = PromoteSectionsRule.SummaryPromotedPrefix;
        var infix = PromoteSectionsRule.SummarySectionsInfix;
        var suffix = PromoteSectionsRule.SummarySubsectionsSuffix;

        if (!message.StartsWith(prefix, StringComparison.Ordinal))
        {
            return false;
        }

        if (!message.EndsWith(suffix, StringComparison.Ordinal))
        {
            return false;
        }

        var infixStart = message.IndexOf(infix, prefix.Length, StringComparison.Ordinal);
        if (infixStart < 0)
        {
            return false;
        }

        var suffixStart = message.Length - suffix.Length;
        if (infixStart + infix.Length > suffixStart)
        {
            return false;
        }

        var sectionsToken = message.AsSpan(prefix.Length, infixStart - prefix.Length);
        var subsectionsToken = message.AsSpan(infixStart + infix.Length, suffixStart - (infixStart + infix.Length));

        if (!int.TryParse(sectionsToken, NumberStyles.Integer, CultureInfo.InvariantCulture, out sections))
        {
            return false;
        }

        if (!int.TryParse(subsectionsToken, NumberStyles.Integer, CultureInfo.InvariantCulture, out subsections))
        {
            return false;
        }

        return true;
    }

    private static bool HasWarnMessage(IReadOnlyList<ReportEntry> entries, string message)
    {
        foreach (var entry in entries)
        {
            if (entry.Level == ReportLevel.Warn
                && string.Equals(entry.Message, message, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasInfoMessage(IReadOnlyList<ReportEntry> entries, string message)
    {
        foreach (var entry in entries)
        {
            if (entry.Level == ReportLevel.Info
                && string.Equals(entry.Message, message, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static IReadOnlyList<DiagnosticIssue> BuildIssues(IReport report)
    {
        var issues = new List<DiagnosticIssue>(report.Entries.Count);
        foreach (var entry in report.Entries)
        {
            if (entry.Level == ReportLevel.Info)
            {
                continue;
            }

            issues.Add(new DiagnosticIssue(
                Rule: entry.Rule,
                Level: MapLevel(entry.Level),
                Message: entry.Message));
        }

        return issues;
    }

    private static string MapStatus(ReportLevel level) => level switch
    {
        ReportLevel.Info => "ok",
        ReportLevel.Warn => "warning",
        ReportLevel.Error => "error",
        _ => throw new ArgumentOutOfRangeException(nameof(level), level, "unknown report level"),
    };

    private static string MapLevel(ReportLevel level) => level switch
    {
        ReportLevel.Warn => "warn",
        ReportLevel.Error => "error",
        _ => throw new ArgumentOutOfRangeException(nameof(level), level, "info entries are not emitted as issues"),
    };

    private static FieldConfidence MapConfidence(AuthorConfidence confidence) => confidence switch
    {
        AuthorConfidence.High => FieldConfidence.High,
        AuthorConfidence.Medium => FieldConfidence.Medium,
        AuthorConfidence.Low => FieldConfidence.Low,
        AuthorConfidence.Missing => FieldConfidence.Missing,
        _ => throw new ArgumentOutOfRangeException(nameof(confidence), confidence, "unknown author confidence"),
    };

    private static DateTime TruncateToSeconds(DateTime value)
    {
        var utc = value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime();
        return new DateTime(
            utc.Year, utc.Month, utc.Day,
            utc.Hour, utc.Minute, utc.Second,
            DateTimeKind.Utc);
    }

    private static JsonSerializerOptions BuildOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        };
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        options.Converters.Add(new Iso8601SecondsDateTimeConverter());
        return options;
    }

    private sealed class Iso8601SecondsDateTimeConverter : JsonConverter<DateTime>
    {
        private const string Format = "yyyy-MM-ddTHH:mm:ssZ";

        public override DateTime Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            var text = reader.GetString()
                ?? throw new JsonException("expected ISO-8601 timestamp string");
            return DateTime.Parse(
                text,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
        }

        public override void Write(
            Utf8JsonWriter writer,
            DateTime value,
            JsonSerializerOptions options)
        {
            var utc = value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime();
            writer.WriteStringValue(utc.ToString(Format, CultureInfo.InvariantCulture));
        }
    }
}
