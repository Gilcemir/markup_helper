using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using DocFormatter.Core.Models;
using DocFormatter.Core.Pipeline;
using DocFormatter.Core.Rules;

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
            Issues: BuildIssues(report));
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

        if (!HasWarnOrError(alignment)
            && !HasWarnOrError(spacing)
            && !HasWarnOrError(email)
            && !HasWarnOrError(abs))
        {
            return null;
        }

        return new DiagnosticFormatting(
            AlignmentApplied: BuildAlignment(alignment),
            AbstractFormatted: BuildAbstract(abs),
            AuthorBlockSpacingApplied: BuildSpacingApplied(spacing),
            CorrespondingEmail: BuildCorrespondingEmail(email));
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
