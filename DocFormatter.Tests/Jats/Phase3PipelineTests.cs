using System.Xml.Linq;
using DocFormatter.Core.Jats;
using DocFormatter.Core.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DocFormatter.Tests.Jats;

public sealed class Phase3PipelineTests
{
    private sealed class StubInjector : IJatsInjector
    {
        private readonly Action<Phase3Context, IReport> _apply;

        public StubInjector(string name, RuleSeverity severity, Action<Phase3Context, IReport> apply)
        {
            Name = name;
            Severity = severity;
            _apply = apply;
        }

        public string Name { get; }

        public RuleSeverity Severity { get; }

        public void Apply(Phase3Context ctx, IReport report) => _apply(ctx, report);
    }

    private sealed class NoopConfirmer : IConfirmer
    {
        public ConfirmResult Confirm(Proposal proposal)
            => new(proposal.ProposedValue, ConfirmDisposition.AutoApplied);
    }

    private static Phase3Context CreateContext()
        => new()
        {
            Source = new DocxSource { ElocationId = "e54492621", Doi = "10.1590/x" },
            Xml = new XDocument(new XElement("article")),
            OtherNumber = "00123",
            Confirm = new NoopConfirmer(),
        };

    [Fact]
    public void Run_AllInjectorsSucceed_RunsInRegistrationOrder()
    {
        var sequence = new List<string>();
        var i1 = new StubInjector("first", RuleSeverity.Optional, (_, report) =>
        {
            sequence.Add("first");
            report.Info("first", "1");
        });
        var i2 = new StubInjector("second", RuleSeverity.Critical, (_, report) =>
        {
            sequence.Add("second");
            report.Info("second", "2");
        });
        var i3 = new StubInjector("third", RuleSeverity.Optional, (_, report) =>
        {
            sequence.Add("third");
            report.Info("third", "3");
        });
        var pipeline = new Phase3Pipeline(new IJatsInjector[] { i1, i2, i3 });
        var report = new Report();

        pipeline.Run(CreateContext(), report);

        Assert.Equal(new[] { "first", "second", "third" }, sequence);
        Assert.Collection(
            report.Entries,
            e => Assert.Equal(("first", "1"), (e.Rule, e.Message)),
            e => Assert.Equal(("second", "2"), (e.Rule, e.Message)),
            e => Assert.Equal(("third", "3"), (e.Rule, e.Message)));
    }

    [Fact]
    public void Run_OptionalInjectorThrows_LogsErrorAndContinues()
    {
        var i2Executed = false;
        var i1 = new StubInjector(
            "i1",
            RuleSeverity.Optional,
            (_, _) => throw new InvalidOperationException("boom"));
        var i2 = new StubInjector(
            "i2",
            RuleSeverity.Optional,
            (_, report) =>
            {
                i2Executed = true;
                report.Info("i2", "ran");
            });
        var pipeline = new Phase3Pipeline(new IJatsInjector[] { i1, i2 });
        var report = new Report();

        pipeline.Run(CreateContext(), report);

        Assert.True(i2Executed);
        Assert.Collection(
            report.Entries,
            e => Assert.Equal(("i1", ReportLevel.Error, "boom"), (e.Rule, e.Level, e.Message)),
            e => Assert.Equal(("i2", ReportLevel.Info, "ran"), (e.Rule, e.Level, e.Message)));
    }

    [Fact]
    public void Run_CriticalInjectorThrows_LogsErrorAndRethrows_AndStopsPipeline()
    {
        var i2Executed = false;
        var i1 = new StubInjector(
            "i1",
            RuleSeverity.Critical,
            (_, _) => throw new InvalidOperationException("fatal"));
        var i2 = new StubInjector("i2", RuleSeverity.Optional, (_, _) => i2Executed = true);
        var pipeline = new Phase3Pipeline(new IJatsInjector[] { i1, i2 });
        var report = new Report();

        var ex = Assert.Throws<InvalidOperationException>(() => pipeline.Run(CreateContext(), report));

        Assert.Equal("fatal", ex.Message);
        Assert.False(i2Executed);
        var entry = Assert.Single(report.Entries);
        Assert.Equal(("i1", ReportLevel.Error, "fatal"), (entry.Rule, entry.Level, entry.Message));
    }

    [Theory]
    [InlineData(RuleSeverity.Critical)]
    [InlineData(RuleSeverity.Optional)]
    public void Run_OperationCanceledException_RethrowsImmediately_RegardlessOfSeverity(RuleSeverity severity)
    {
        var i2Executed = false;
        var i1 = new StubInjector(
            "i1",
            severity,
            (_, _) => throw new OperationCanceledException("cancelled"));
        var i2 = new StubInjector("i2", RuleSeverity.Optional, (_, _) => i2Executed = true);
        var pipeline = new Phase3Pipeline(new IJatsInjector[] { i1, i2 });
        var report = new Report();

        Assert.Throws<OperationCanceledException>(() => pipeline.Run(CreateContext(), report));

        Assert.False(i2Executed);
        Assert.Empty(report.Entries);
    }

    [Fact]
    public void Run_ResolvedFromDi_RunsInjectorsInRegistrationOrder()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IJatsInjector>(
            new StubInjector("First", RuleSeverity.Optional, (_, report) => report.Info("First", "1")));
        services.AddSingleton<IJatsInjector>(
            new StubInjector("Second", RuleSeverity.Optional, (_, report) => report.Info("Second", "2")));
        services.AddSingleton<IJatsInjector>(
            new StubInjector("Third", RuleSeverity.Optional, (_, report) => report.Info("Third", "3")));
        services.AddSingleton<Phase3Pipeline>();

        using var provider = services.BuildServiceProvider();
        var pipeline = provider.GetRequiredService<Phase3Pipeline>();
        var report = new Report();

        pipeline.Run(CreateContext(), report);

        Assert.Collection(
            report.Entries,
            e => Assert.Equal(("First", "1"), (e.Rule, e.Message)),
            e => Assert.Equal(("Second", "2"), (e.Rule, e.Message)),
            e => Assert.Equal(("Third", "3"), (e.Rule, e.Message)));
    }
}
