using System.Xml.Linq;
using DocFormatter.Core.Jats;
using DocFormatter.Core.Pipeline;
using Xunit;

namespace DocFormatter.Tests.Jats;

/// <summary>
/// Drives the same <see cref="Proposal"/> through <see cref="Phase3Pipeline"/>
/// under two interchangeable <see cref="IConfirmer"/> policies and asserts the
/// policy alone changes the outcome — value written under
/// <see cref="AutoAcceptConfirmer"/>, run aborted under
/// <see cref="FailOnPromptConfirmer"/> — without any injector change.
/// </summary>
public sealed class ConfirmerPolicySwapIntegrationTests
{
    /// <summary>
    /// An optional injector that always routes through the gate, then writes the
    /// confirmed value as an attribute on the document root.
    /// </summary>
    private sealed class GatedInjector : IJatsInjector
    {
        public string Name => "gated";

        // Optional: the pipeline would normally swallow an optional injector's
        // exception, so this proves FailOnPrompt aborts regardless of severity.
        public RuleSeverity Severity => RuleSeverity.Optional;

        public void Apply(Phase3Context ctx, IReport report)
        {
            var proposal = new Proposal("data-availability", "open-data", "ambiguous statement");
            var result = ctx.Confirm.Confirm(proposal);
            ctx.Xml.Root!.SetAttributeValue("specific-use", result.Value);
            report.Info(Name, $"{result.Disposition}:{result.Value}");
        }
    }

    private static Phase3Context CreateContext(IConfirmer confirmer)
        => new()
        {
            Source = new DocxSource { ElocationId = "e54492621", Doi = "10.1590/x" },
            Xml = new XDocument(new XElement("article")),
            OtherNumber = "00123",
            Confirm = confirmer,
        };

    [Fact]
    public void SameProposal_UnderAutoAccept_WritesTheProposedValue()
    {
        var pipeline = new Phase3Pipeline(new IJatsInjector[] { new GatedInjector() });
        var ctx = CreateContext(new AutoAcceptConfirmer());
        var report = new Report();

        pipeline.Run(ctx, report);

        Assert.Equal("open-data", (string?)ctx.Xml.Root!.Attribute("specific-use"));
    }

    [Fact]
    public void SameProposal_UnderFailOnPrompt_AbortsAndWritesNothing()
    {
        var pipeline = new Phase3Pipeline(new IJatsInjector[] { new GatedInjector() });
        var ctx = CreateContext(new FailOnPromptConfirmer());
        var report = new Report();

        Assert.Throws<PromptNotAllowedException>(() => pipeline.Run(ctx, report));

        Assert.Null(ctx.Xml.Root!.Attribute("specific-use"));
    }
}
