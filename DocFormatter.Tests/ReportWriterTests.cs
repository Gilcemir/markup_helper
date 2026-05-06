using DocFormatter.Core.Pipeline;
using DocFormatter.Core.Reporting;
using Xunit;

namespace DocFormatter.Tests;

public sealed class ReportWriterTests
{
    [Fact]
    public void FormatLines_OneInfoOneWarnOneError_ProducesThreeFormattedLinesInOrder()
    {
        var report = new Report();
        report.Info("RuleA", "info message");
        report.Warn("RuleB", "warn message");
        report.Error("RuleC", "error message");

        var lines = ReportWriter.FormatLines(report).ToList();

        Assert.Equal(
            new[]
            {
                "[INFO] RuleA — info message",
                "[WARN] RuleB — warn message",
                "[ERROR] RuleC — error message",
            },
            lines);
    }

    [Fact]
    public void Write_PersistsAllEntriesInOrder_OneEntryPerLine()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"docfmt-rw-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        try
        {
            var path = Path.Combine(tempDir, "sample.report.txt");
            var report = new Report();
            report.Info("R1", "first");
            report.Warn("R2", "second");

            ReportWriter.Write(path, report);

            Assert.True(File.Exists(path));
            var lines = File.ReadAllLines(path);
            Assert.Equal(
                new[]
                {
                    "[INFO] R1 — first",
                    "[WARN] R2 — second",
                },
                lines);
        }
        finally
        {
            try { Directory.Delete(tempDir, recursive: true); } catch { /* best effort */ }
        }
    }

    [Fact]
    public void Write_CreatesParentDirectoryIfMissing()
    {
        var root = Path.Combine(Path.GetTempPath(), $"docfmt-rw-{Guid.NewGuid():N}");
        try
        {
            var path = Path.Combine(root, "nested", "out.report.txt");
            var report = new Report();
            report.Info("R1", "msg");

            ReportWriter.Write(path, report);

            Assert.True(File.Exists(path));
        }
        finally
        {
            try { Directory.Delete(root, recursive: true); } catch { /* best effort */ }
        }
    }
}
