namespace DocFormatter.Core.Jats;

/// <summary>
/// The Phase 3 confirmation gate (ADR-006). An injector that cannot derive a
/// value with high confidence builds a <see cref="Proposal"/> and calls
/// <see cref="Confirm"/>; the policy implementation decides whether to accept the
/// proposal, take an operator override, or skip. Injectors are unaware of the
/// concrete policy — it is wired per run (interactive console, auto-accept, or
/// fail-on-prompt).
/// </summary>
public interface IConfirmer
{
    /// <summary>
    /// Resolves <paramref name="proposal"/> into the final value to write and the
    /// disposition that produced it.
    /// </summary>
    ConfirmResult Confirm(Proposal proposal);
}
