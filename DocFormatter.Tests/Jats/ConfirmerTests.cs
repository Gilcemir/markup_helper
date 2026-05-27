using DocFormatter.Core.Jats;
using Xunit;

namespace DocFormatter.Tests.Jats;

public sealed class ConfirmerTests
{
    private static Proposal SampleProposal()
        => new("data-availability", "open-data", "statement matched open-data keywords");

    private static Proposal FreeTextEligibleProposal()
        => new("credit-roles", "apply CRediT to: Costa AES", "unrecognized term(s): Choreography")
        {
            AllowsFreeText = true,
        };

    [Fact]
    public void AutoAccept_ReturnsProposedValue_WithAutoApplied()
    {
        var confirmer = new AutoAcceptConfirmer();

        var result = confirmer.Confirm(SampleProposal());

        Assert.Equal("open-data", result.Value);
        Assert.Equal(ConfirmDisposition.AutoApplied, result.Disposition);
    }

    [Fact]
    public void FailOnPrompt_Throwing_SignalsFailureCarryingTheProposal()
    {
        var confirmer = new FailOnPromptConfirmer();
        var proposal = SampleProposal();

        var ex = Assert.Throws<PromptNotAllowedException>(() => confirmer.Confirm(proposal));

        Assert.Same(proposal, ex.Proposal);
    }

    [Fact]
    public void FailOnPrompt_Exception_DerivesFromOperationCanceled_ForReliableAbort()
    {
        // Phase3Pipeline always rethrows OperationCanceledException regardless of
        // injector severity, so deriving from it guarantees a prompt aborts.
        var confirmer = new FailOnPromptConfirmer();

        Assert.ThrowsAny<OperationCanceledException>(() => confirmer.Confirm(SampleProposal()));
    }

    [Fact]
    public void Console_AcceptInput_ReturnsConfirmed_WithProposedValue()
    {
        using var input = new StringReader("\n");
        using var output = new StringWriter();
        var confirmer = new ConsoleConfirmer(input, output);

        var result = confirmer.Confirm(SampleProposal());

        Assert.Equal("open-data", result.Value);
        Assert.Equal(ConfirmDisposition.Confirmed, result.Disposition);
    }

    [Fact]
    public void Console_BlankWhitespaceInput_ReturnsConfirmed()
    {
        using var input = new StringReader("   \n");
        using var output = new StringWriter();
        var confirmer = new ConsoleConfirmer(input, output);

        var result = confirmer.Confirm(SampleProposal());

        Assert.Equal("open-data", result.Value);
        Assert.Equal(ConfirmDisposition.Confirmed, result.Disposition);
    }

    [Fact]
    public void Console_OverrideInput_ReturnsOverridden_WithNewValue()
    {
        using var input = new StringReader("restricted-data\n");
        using var output = new StringWriter();
        var confirmer = new ConsoleConfirmer(input, output);

        var result = confirmer.Confirm(SampleProposal());

        Assert.Equal("restricted-data", result.Value);
        Assert.Equal(ConfirmDisposition.Overridden, result.Disposition);
    }

    [Fact]
    public void Console_OverrideInput_IsTrimmed()
    {
        using var input = new StringReader("  restricted-data  \n");
        using var output = new StringWriter();
        var confirmer = new ConsoleConfirmer(input, output);

        var result = confirmer.Confirm(SampleProposal());

        Assert.Equal("restricted-data", result.Value);
        Assert.Equal(ConfirmDisposition.Overridden, result.Disposition);
    }

    [Fact]
    public void Console_WritesTagValueAndReason_ToInjectedOutput()
    {
        using var input = new StringReader("\n");
        using var output = new StringWriter();
        var confirmer = new ConsoleConfirmer(input, output);

        confirmer.Confirm(SampleProposal());

        var written = output.ToString();
        Assert.Contains("data-availability", written);
        Assert.Contains("open-data", written);
        Assert.Contains("statement matched open-data keywords", written);
    }

    [Fact]
    public void Console_EndOfStreamInput_Throws()
    {
        // Empty/closed stream → ReadLine returns null: there is no operator to
        // answer. Silently accepting would defeat the fail-loud intent (ADR-001),
        // so the interactive confirmer aborts and points to --non-interactive.
        using var input = new StringReader(string.Empty);
        using var output = new StringWriter();
        var confirmer = new ConsoleConfirmer(input, output);

        var ex = Assert.Throws<ConfirmationInputUnavailableException>(
            () => confirmer.Confirm(SampleProposal()));
        Assert.Contains("--non-interactive", ex.Message);
    }

    [Fact]
    public void Console_SkipToken_ReturnsSkipped_WithNoValue()
    {
        using var input = new StringReader($"{ConsoleConfirmer.SkipToken}\n");
        using var output = new StringWriter();
        var confirmer = new ConsoleConfirmer(input, output);

        var result = confirmer.Confirm(SampleProposal());

        Assert.Equal(ConfirmDisposition.Skipped, result.Disposition);
        Assert.Equal(string.Empty, result.Value);
    }

    [Fact]
    public void Console_NoOverrideProposal_TypedValue_RepromptsInsteadOfOverriding()
    {
        // credit-roles sets AllowsOverride=false: a typed value cannot re-map it, so
        // it must re-prompt rather than record a no-op "Overridden" (P11).
        var proposal = new Proposal("credit-roles", "apply CRediT to: Costa AES", "unresolved author(s): Neto VBP")
        {
            AllowsFreeText = true,
            AllowsOverride = false,
        };
        // First line is a stray value (invalid here → re-prompt); second is Enter.
        using var input = new StringReader("Conceptualization\n\n");
        using var output = new StringWriter();
        var confirmer = new ConsoleConfirmer(input, output);

        var result = confirmer.Confirm(proposal);

        Assert.Equal(ConfirmDisposition.Confirmed, result.Disposition);
        Assert.Contains("invalid option", output.ToString());
    }

    // ── ADR-007: operator-chosen, document-scoped free-text outcome ───────────

    [Fact]
    public void Console_FreeTextEligible_PickingFreeTextToken_ReturnsFreeText()
    {
        using var input = new StringReader(ConsoleConfirmer.FreeTextToken + "\n");
        using var output = new StringWriter();
        var confirmer = new ConsoleConfirmer(input, output);

        var result = confirmer.Confirm(FreeTextEligibleProposal());

        Assert.Equal(ConfirmDisposition.FreeText, result.Disposition);
    }

    [Fact]
    public void Console_FreeTextEligible_TokenIsCaseInsensitive()
    {
        using var input = new StringReader(ConsoleConfirmer.FreeTextToken.ToUpperInvariant() + "\n");
        using var output = new StringWriter();
        var confirmer = new ConsoleConfirmer(input, output);

        var result = confirmer.Confirm(FreeTextEligibleProposal());

        Assert.Equal(ConfirmDisposition.FreeText, result.Disposition);
    }

    [Fact]
    public void Console_FreeTextEligible_OffersTheChoiceInThePrompt()
    {
        using var input = new StringReader("\n");
        using var output = new StringWriter();
        var confirmer = new ConsoleConfirmer(input, output);

        confirmer.Confirm(FreeTextEligibleProposal());

        var written = output.ToString();
        Assert.Contains($"[{ConsoleConfirmer.FreeTextToken}]", written, StringComparison.Ordinal);
        Assert.Contains("free text", written, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Console_FreeTextEligible_EnterStillConfirmsTheCleanSubset()
    {
        // Free text must never be the implicit default — Enter accepts the proposal.
        using var input = new StringReader("\n");
        using var output = new StringWriter();
        var confirmer = new ConsoleConfirmer(input, output);

        var result = confirmer.Confirm(FreeTextEligibleProposal());

        Assert.Equal(ConfirmDisposition.Confirmed, result.Disposition);
    }

    [Fact]
    public void Console_FreeTextDisallowed_DoesNotOfferOrSelectFreeText()
    {
        // A proposal that disallows free text (e.g. data-availability) offers only
        // confirm/override; the free-text token is treated as an ordinary override.
        using var input = new StringReader(ConsoleConfirmer.FreeTextToken + "\n");
        using var output = new StringWriter();
        var confirmer = new ConsoleConfirmer(input, output);

        var result = confirmer.Confirm(SampleProposal());

        Assert.Equal(ConfirmDisposition.Overridden, result.Disposition);
        Assert.Equal(ConsoleConfirmer.FreeTextToken, result.Value);
        Assert.DoesNotContain("free text", output.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AutoAccept_FreeTextEligible_StillReturnsAutoApplied_NeverFreeText()
    {
        var confirmer = new AutoAcceptConfirmer();

        var result = confirmer.Confirm(FreeTextEligibleProposal());

        Assert.Equal(ConfirmDisposition.AutoApplied, result.Disposition);
        Assert.NotEqual(ConfirmDisposition.FreeText, result.Disposition);
    }

    [Fact]
    public void FailOnPrompt_FreeTextEligible_StillAborts()
    {
        var confirmer = new FailOnPromptConfirmer();

        Assert.Throws<PromptNotAllowedException>(() => confirmer.Confirm(FreeTextEligibleProposal()));
    }
}
