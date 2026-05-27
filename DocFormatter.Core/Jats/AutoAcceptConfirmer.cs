namespace DocFormatter.Core.Jats;

/// <summary>
/// Non-interactive <see cref="IConfirmer"/> policy that writes the injector's
/// best-guess proposal without any prompt or I/O, recording it as
/// <see cref="ConfirmDisposition.AutoApplied"/> (ADR-006). Used by the golden
/// corpus tests and unattended batch runs (<c>--non-interactive=accept</c>).
/// </summary>
public sealed class AutoAcceptConfirmer : IConfirmer
{
    /// <inheritdoc />
    public ConfirmResult Confirm(Proposal proposal)
    {
        ArgumentNullException.ThrowIfNull(proposal);
        return new ConfirmResult(proposal.ProposedValue, ConfirmDisposition.AutoApplied);
    }
}
