namespace DocFormatter.Core.Jats;

/// <summary>
/// A value an injector proposes to write when it cannot decide deterministically,
/// presented to the <see cref="IConfirmer"/> gate (ADR-006). Carries the target
/// <paramref name="Tag"/> name, the <paramref name="ProposedValue"/>, and a
/// human-readable <paramref name="Reason"/> explaining why confirmation is needed.
/// </summary>
public sealed record Proposal(string Tag, string ProposedValue, string Reason)
{
    /// <summary>
    /// Whether the operator may answer this proposal with the document-scoped
    /// free-text outcome (<see cref="ConfirmDisposition.FreeText"/>). Set only by
    /// the credit-roles unrecognized/unresolved branch (ADR-007); the default is
    /// <see langword="false"/> so every other proposal keeps offering only
    /// confirm/override/skip. An interactive policy must present free text only
    /// when this is <see langword="true"/>, and it is never auto-selected.
    /// </summary>
    public bool AllowsFreeText { get; init; }

    /// <summary>
    /// Whether the operator may replace the proposed value with their own
    /// (<see cref="ConfirmDisposition.Overridden"/>). The default is
    /// <see langword="true"/>. The credit-roles prompt sets it to
    /// <see langword="false"/> (ADR-005/ADR-007): there is no single string the
    /// operator could type that meaningfully re-maps CRediT, so a free-form
    /// override would be a no-op recorded misleadingly as "Overridden" — only
    /// accept / decline / free-text are meaningful there.
    /// </summary>
    public bool AllowsOverride { get; init; } = true;
}

/// <summary>
/// The outcome of a <see cref="IConfirmer.Confirm"/> call: the final
/// <paramref name="Value"/> to write and the <paramref name="Disposition"/> that
/// produced it, both fed to the per-file report for auditability (ADR-006).
/// </summary>
public sealed record ConfirmResult(string Value, ConfirmDisposition Disposition);

/// <summary>
/// How a value reached the document, recorded in the report so auto and
/// human-mediated decisions are distinguishable.
/// </summary>
public enum ConfirmDisposition
{
    /// <summary>Written automatically without prompting (deterministic or auto-accept policy).</summary>
    AutoApplied,

    /// <summary>The operator confirmed the proposed value unchanged.</summary>
    Confirmed,

    /// <summary>The operator replaced the proposed value with their own.</summary>
    Overridden,

    /// <summary>No value was written; the proposal was declined.</summary>
    Skipped,

    /// <summary>
    /// The operator chose the document-scoped free-text outcome (ADR-007): emit
    /// every role for this document as <c>&lt;role&gt;</c> without
    /// <c>@content-type</c>, honoring the SPS per-document all-or-nothing rule.
    /// Offered only when <see cref="Proposal.AllowsFreeText"/> is set, and never
    /// auto-selected by a non-interactive policy.
    /// </summary>
    FreeText,
}
