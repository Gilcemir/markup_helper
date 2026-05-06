using System.Text.Json;
using DocFormatter.Core.Models;
using DocFormatter.Core.Pipeline;
using DocFormatter.Core.Reporting;
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
