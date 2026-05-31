namespace DocFormatter.Core.Jats;

/// <summary>
/// Strict non-interactive <see cref="IConfirmer"/> policy
/// (<c>--non-interactive=fail</c>, ADR-006): any attempt to prompt is an error.
/// A newly-ambiguous document that reaches the gate aborts the run with a
/// non-zero outcome, turning "a document became ambiguous" into a loud CI signal.
/// </summary>
/// <remarks>
/// Throws <see cref="PromptNotAllowedException"/>, which derives from
/// <see cref="OperationCanceledException"/> so that <c>Phase3Pipeline</c> rethrows
/// it immediately regardless of the calling injector's
/// <see cref="Pipeline.RuleSeverity"/> — an optional injector's plain exception
/// would otherwise be logged and swallowed, defeating the strict policy.
/// </remarks>
public sealed class FailOnPromptConfirmer : IConfirmer
{
    /// <inheritdoc />
    public ConfirmResult Confirm(Proposal proposal)
    {
        ArgumentNullException.ThrowIfNull(proposal);
        throw new PromptNotAllowedException(proposal);
    }
}

/// <summary>
/// Raised by <see cref="FailOnPromptConfirmer"/> when an injector requests
/// confirmation under the strict non-interactive policy. Derives from
/// <see cref="OperationCanceledException"/> so the Phase 3 pipeline aborts the
/// document immediately rather than logging and continuing.
/// </summary>
public sealed class PromptNotAllowedException : OperationCanceledException
{
    /// <summary>The proposal that triggered the disallowed prompt.</summary>
    public Proposal Proposal { get; }

    public PromptNotAllowedException(Proposal proposal)
        : base(BuildMessage(proposal))
    {
        Proposal = proposal;
    }

    private static string BuildMessage(Proposal proposal)
    {
        ArgumentNullException.ThrowIfNull(proposal);
        return $"non-interactive=fail: confirmation required for '{proposal.Tag}' "
            + $"(proposed '{proposal.ProposedValue}'): {proposal.Reason}";
    }
}
