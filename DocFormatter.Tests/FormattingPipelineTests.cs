using DocFormatter.Core.Pipeline;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DocFormatter.Tests;

public sealed class FormattingPipelineTests
{
    private sealed class StubRule : IFormattingRule
    {
        private readonly Action<FormattingContext, IReport> _apply;

        public StubRule(string name, RuleSeverity severity, Action<FormattingContext, IReport> apply)
        {
            Name = name;
            Severity = severity;
            _apply = apply;
        }

        public string Name { get; }

        public RuleSeverity Severity { get; }

        public void Apply(WordprocessingDocument doc, FormattingContext ctx, IReport report)
            => _apply(ctx, report);
    }

    private static WordprocessingDocument CreateEmptyDocument()
    {
        var stream = new MemoryStream();
        return WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document);
    }

    [Fact]
    public void Run_AllRulesSucceed_RunsInRegistrationOrder_AndAccumulatesInfoEntries()
    {
        var r1 = new StubRule("R1", RuleSeverity.Optional, (_, report) => report.Info("R1", "first"));
        var r2 = new StubRule("R2", RuleSeverity.Critical, (_, report) => report.Info("R2", "second"));
        var pipeline = new FormattingPipeline(new IFormattingRule[] { r1, r2 });
        using var doc = CreateEmptyDocument();
        var ctx = new FormattingContext();
        var report = new Report();

        pipeline.Run(doc, ctx, report);

        Assert.Collection(
            report.Entries,
            e => Assert.Equal(("R1", ReportLevel.Info, "first"), (e.Rule, e.Level, e.Message)),
            e => Assert.Equal(("R2", ReportLevel.Info, "second"), (e.Rule, e.Level, e.Message)));
    }

    [Fact]
    public void Run_OptionalRuleThrows_LogsErrorAndContinues()
    {
        var r2Executed = false;
        var r1 = new StubRule(
            "R1",
            RuleSeverity.Optional,
            (_, _) => throw new InvalidOperationException("boom"));
        var r2 = new StubRule(
            "R2",
            RuleSeverity.Optional,
            (_, report) =>
            {
                r2Executed = true;
                report.Info("R2", "ran");
            });
        var pipeline = new FormattingPipeline(new IFormattingRule[] { r1, r2 });
        using var doc = CreateEmptyDocument();
        var ctx = new FormattingContext();
        var report = new Report();

        pipeline.Run(doc, ctx, report);

        Assert.True(r2Executed);
        Assert.Collection(
            report.Entries,
            e => Assert.Equal(("R1", ReportLevel.Error, "boom"), (e.Rule, e.Level, e.Message)),
            e => Assert.Equal(("R2", ReportLevel.Info, "ran"), (e.Rule, e.Level, e.Message)));
    }

    [Fact]
    public void Run_CriticalRuleThrows_LogsErrorAndRethrows_AndStopsPipeline()
    {
        var r2Executed = false;
        var r1 = new StubRule(
            "R1",
            RuleSeverity.Critical,
            (_, _) => throw new InvalidOperationException("fatal"));
        var r2 = new StubRule(
            "R2",
            RuleSeverity.Optional,
            (_, _) => r2Executed = true);
        var pipeline = new FormattingPipeline(new IFormattingRule[] { r1, r2 });
        using var doc = CreateEmptyDocument();
        var ctx = new FormattingContext();
        var report = new Report();

        var ex = Assert.Throws<InvalidOperationException>(() => pipeline.Run(doc, ctx, report));

        Assert.Equal("fatal", ex.Message);
        Assert.False(r2Executed);
        var entry = Assert.Single(report.Entries);
        Assert.Equal(("R1", ReportLevel.Error, "fatal"), (entry.Rule, entry.Level, entry.Message));
    }

    [Theory]
    [InlineData(RuleSeverity.Critical)]
    [InlineData(RuleSeverity.Optional)]
    public void Run_OperationCanceledException_RethrowsImmediately_RegardlessOfSeverity(RuleSeverity severity)
    {
        var r2Executed = false;
        var r1 = new StubRule(
            "R1",
            severity,
            (_, _) => throw new OperationCanceledException("cancelled"));
        var r2 = new StubRule(
            "R2",
            RuleSeverity.Optional,
            (_, _) => r2Executed = true);
        var pipeline = new FormattingPipeline(new IFormattingRule[] { r1, r2 });
        using var doc = CreateEmptyDocument();
        var ctx = new FormattingContext();
        var report = new Report();

        Assert.Throws<OperationCanceledException>(() => pipeline.Run(doc, ctx, report));

        Assert.False(r2Executed);
    }

    [Fact]
    public void Run_TwoSequentialRuns_WithDifferentContexts_DoNotShareState()
    {
        var callCount = 0;
        var rule = new StubRule(
            "R",
            RuleSeverity.Optional,
            (ctx, report) =>
            {
                callCount++;
                ctx.ArticleTitle = $"title-{callCount}";
                report.Info("R", $"call-{callCount}");
            });
        var pipeline = new FormattingPipeline(new IFormattingRule[] { rule });
        using var doc = CreateEmptyDocument();

        var ctx1 = new FormattingContext();
        var report1 = new Report();
        pipeline.Run(doc, ctx1, report1);

        var ctx2 = new FormattingContext();
        var report2 = new Report();
        pipeline.Run(doc, ctx2, report2);

        Assert.NotSame(ctx1, ctx2);
        Assert.Equal("title-1", ctx1.ArticleTitle);
        Assert.Equal("title-2", ctx2.ArticleTitle);
        Assert.Single(report1.Entries);
        Assert.Single(report2.Entries);
        Assert.Equal("call-1", report1.Entries[0].Message);
        Assert.Equal("call-2", report2.Entries[0].Message);
    }

    [Fact]
    public void Run_ResolvedFromDi_RunsRulesInRegistrationOrder()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IFormattingRule>(
            new StubRule("First", RuleSeverity.Optional, (_, report) => report.Info("First", "1")));
        services.AddSingleton<IFormattingRule>(
            new StubRule("Second", RuleSeverity.Optional, (_, report) => report.Info("Second", "2")));
        services.AddSingleton<IFormattingRule>(
            new StubRule("Third", RuleSeverity.Optional, (_, report) => report.Info("Third", "3")));
        services.AddSingleton<FormattingPipeline>();

        using var provider = services.BuildServiceProvider();
        var pipeline = provider.GetRequiredService<FormattingPipeline>();
        using var doc = CreateEmptyDocument();
        var ctx = new FormattingContext();
        var report = new Report();

        pipeline.Run(doc, ctx, report);

        Assert.Collection(
            report.Entries,
            e => Assert.Equal(("First", "1"), (e.Rule, e.Message)),
            e => Assert.Equal(("Second", "2"), (e.Rule, e.Message)),
            e => Assert.Equal(("Third", "3"), (e.Rule, e.Message)));
    }
}
