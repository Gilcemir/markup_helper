using System.Xml.Linq;
using DocFormatter.Core.Jats;
using Xunit;

namespace DocFormatter.Tests.Jats;

public sealed class Phase3ContractsTests
{
    [Fact]
    public void ConfirmResult_CarriesValueAndDisposition()
    {
        var result = new ConfirmResult("e54492621", ConfirmDisposition.Confirmed);

        Assert.Equal("e54492621", result.Value);
        Assert.Equal(ConfirmDisposition.Confirmed, result.Disposition);
    }

    [Fact]
    public void Proposal_CarriesTagValueAndReason()
    {
        var proposal = new Proposal("other-id", "00123", "no DOI sibling found");

        Assert.Equal("other-id", proposal.Tag);
        Assert.Equal("00123", proposal.ProposedValue);
        Assert.Equal("no DOI sibling found", proposal.Reason);
    }

    [Theory]
    [InlineData(ConfirmDisposition.AutoApplied)]
    [InlineData(ConfirmDisposition.Confirmed)]
    [InlineData(ConfirmDisposition.Overridden)]
    [InlineData(ConfirmDisposition.Skipped)]
    [InlineData(ConfirmDisposition.FreeText)]
    public void ConfirmDisposition_AllValuesRoundTripThroughConfirmResult(ConfirmDisposition disposition)
    {
        var result = new ConfirmResult("v", disposition);

        Assert.Equal(disposition, result.Disposition);
    }

    [Fact]
    public void Proposal_AllowsFreeText_DefaultsToFalse()
    {
        var proposal = new Proposal("credit-roles", "apply CRediT to: X", "unrecognized term");

        Assert.False(proposal.AllowsFreeText);
    }

    [Fact]
    public void Proposal_AllowsFreeText_IsSettableViaInitializer()
    {
        var proposal = new Proposal("credit-roles", "apply CRediT to: X", "unrecognized term")
        {
            AllowsFreeText = true,
        };

        Assert.True(proposal.AllowsFreeText);
    }

    [Fact]
    public void Phase3Context_ExposesSourceXmlOtherNumberAndConfirmer()
    {
        var source = new DocxSource { ElocationId = "e54492621", Doi = "10.1590/x" };
        var xml = new XDocument(new XElement("article"));
        var confirmer = new PassthroughConfirmer();

        var ctx = new Phase3Context
        {
            Source = source,
            Xml = xml,
            OtherNumber = "00123",
            Confirm = confirmer,
        };

        Assert.Same(source, ctx.Source);
        Assert.Same(xml, ctx.Xml);
        Assert.Equal("00123", ctx.OtherNumber);
        Assert.Same(confirmer, ctx.Confirm);
    }

    [Fact]
    public void Phase3Context_OtherNumber_DefaultsToNull()
    {
        var ctx = new Phase3Context
        {
            Source = new DocxSource { ElocationId = "e1", Doi = "10.1590/x" },
            Xml = new XDocument(new XElement("article")),
            Confirm = new PassthroughConfirmer(),
        };

        Assert.Null(ctx.OtherNumber);
    }

    private sealed class PassthroughConfirmer : IConfirmer
    {
        public ConfirmResult Confirm(Proposal proposal)
            => new(proposal.ProposedValue, ConfirmDisposition.AutoApplied);
    }
}
