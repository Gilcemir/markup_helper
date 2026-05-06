using DocFormatter.Core.Pipeline;
using Xunit;

namespace DocFormatter.Tests;

public sealed class ReportTests
{
    [Fact]
    public void Info_RecordsSingleEntry_WithMatchingFields()
    {
        var report = new Report();

        report.Info("R", "msg");

        var entry = Assert.Single(report.Entries);
        Assert.Equal(ReportLevel.Info, entry.Level);
        Assert.Equal("R", entry.Rule);
        Assert.Equal("msg", entry.Message);
    }

    [Fact]
    public void Entries_PreserveInsertionOrder_AcrossLevels()
    {
        var report = new Report();

        report.Info("A", "a");
        report.Warn("B", "b");
        report.Error("C", "c");
        report.Info("D", "d");

        Assert.Collection(
            report.Entries,
            e => Assert.Equal(("A", ReportLevel.Info, "a"), (e.Rule, e.Level, e.Message)),
            e => Assert.Equal(("B", ReportLevel.Warn, "b"), (e.Rule, e.Level, e.Message)),
            e => Assert.Equal(("C", ReportLevel.Error, "c"), (e.Rule, e.Level, e.Message)),
            e => Assert.Equal(("D", ReportLevel.Info, "d"), (e.Rule, e.Level, e.Message)));
    }

    [Fact]
    public void HighestLevel_IsInfo_WhenEmpty()
    {
        var report = new Report();

        Assert.Empty(report.Entries);
        Assert.Equal(ReportLevel.Info, report.HighestLevel);
    }

    [Fact]
    public void HighestLevel_IsWarn_AfterInfoThenWarn()
    {
        var report = new Report();

        report.Info("R", "i");
        report.Warn("R", "w");

        Assert.Equal(ReportLevel.Warn, report.HighestLevel);
    }

    [Fact]
    public void HighestLevel_IsError_AfterErrorAndOtherEntries()
    {
        var report = new Report();

        report.Info("R", "i");
        report.Error("R", "e");
        report.Warn("R", "w");
        report.Info("R", "i2");

        Assert.Equal(ReportLevel.Error, report.HighestLevel);
    }

    [Fact]
    public void Timestamp_IsUtc_AndWithinOneSecondOfCall()
    {
        var report = new Report();

        var before = DateTime.UtcNow;
        report.Info("R", "msg");
        var after = DateTime.UtcNow;

        var entry = Assert.Single(report.Entries);
        Assert.Equal(DateTimeKind.Utc, entry.Timestamp.Kind);
        Assert.InRange(entry.Timestamp, before.AddSeconds(-1), after.AddSeconds(1));
    }
}
