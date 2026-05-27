using System.Xml.Linq;
using DocFormatter.Core.Jats;
using DocFormatter.Core.Pipeline;
using Xunit;

namespace DocFormatter.Tests.Jats;

/// <summary>
/// Exercises <see cref="Phase3Pipeline"/> end-to-end over a real
/// <see cref="Phase3Context"/> with stub injectors that both mutate the target
/// <see cref="XDocument"/> and route a value through the <see cref="IConfirmer"/>
/// gate, asserting ordered report entries and that only the XML is changed.
/// </summary>
public sealed class Phase3PipelineIntegrationTests
{
    private sealed class AppendChildInjector : IJatsInjector
    {
        private readonly string _childName;
        private readonly Func<Phase3Context, string> _valueFactory;

        public AppendChildInjector(string name, string childName, Func<Phase3Context, string> valueFactory)
        {
            Name = name;
            _childName = childName;
            _valueFactory = valueFactory;
        }

        public string Name { get; }

        public RuleSeverity Severity => RuleSeverity.Optional;

        public void Apply(Phase3Context ctx, IReport report)
        {
            var value = _valueFactory(ctx);
            ctx.Xml.Root!.Add(new XElement(_childName, value));
            report.Info(Name, $"injected {_childName}={value}");
        }
    }

    private sealed class RecordingConfirmer : IConfirmer
    {
        public List<Proposal> Seen { get; } = new();

        public ConfirmResult Confirm(Proposal proposal)
        {
            Seen.Add(proposal);
            return new ConfirmResult(proposal.ProposedValue, ConfirmDisposition.AutoApplied);
        }
    }

    [Fact]
    public void Run_TwoStubPipeline_ProducesOrderedReportEntries_AndMutatesOnlyXml()
    {
        var source = new DocxSource
        {
            ElocationId = "e54492621",
            Doi = "10.1590/1984-70332026v26n2a16",
            ScientificEditor = "Jane Roe",
        };
        var xml = new XDocument(new XElement("article", new XElement("front")));
        var confirmer = new RecordingConfirmer();
        var ctx = new Phase3Context
        {
            Source = source,
            Xml = xml,
            OtherNumber = "00123",
            Confirm = confirmer,
        };

        // First injector derives deterministically from OtherNumber; second routes
        // the editor name through the confirmer gate.
        var otherId = new AppendChildInjector("other-id", "article-id", c => c.OtherNumber!);
        var editedBy = new AppendChildInjector(
            "edited-by",
            "fn",
            c => c.Confirm.Confirm(new Proposal("edited-by", c.Source.ScientificEditor!, "editor line")).Value);
        var pipeline = new Phase3Pipeline(new IJatsInjector[] { otherId, editedBy });
        var report = new Report();

        pipeline.Run(ctx, report);

        // Ordered report entries.
        Assert.Collection(
            report.Entries,
            e => Assert.Equal(("other-id", "injected article-id=00123"), (e.Rule, e.Message)),
            e => Assert.Equal(("edited-by", "injected fn=Jane Roe"), (e.Rule, e.Message)));

        // XML mutated in registration order; source untouched.
        var children = xml.Root!.Elements().Select(e => e.Name.LocalName).ToArray();
        Assert.Equal(new[] { "front", "article-id", "fn" }, children);
        Assert.Equal("00123", xml.Root!.Element("article-id")!.Value);
        Assert.Equal("Jane Roe", xml.Root!.Element("fn")!.Value);
        Assert.Equal("Jane Roe", source.ScientificEditor);

        // Confirmer gate was exercised exactly once.
        var seen = Assert.Single(confirmer.Seen);
        Assert.Equal("edited-by", seen.Tag);
        Assert.Equal("Jane Roe", seen.ProposedValue);
    }
}
