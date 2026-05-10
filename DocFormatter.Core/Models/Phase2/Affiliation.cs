namespace DocFormatter.Core.Models.Phase2;

/// <summary>
/// Affiliation entry exposed to Phase 2 emitter rules. <see cref="Id"/> is the
/// SciELO XML rid token (e.g. <c>aff1</c>); <see cref="Label"/> is the visible
/// superscript label that appears next to author names (e.g. <c>1</c>,
/// <c>²</c>). The remaining fields are populated only when the upstream rule
/// has high-confidence access to them; <c>null</c> means "not extracted".
/// </summary>
public sealed record Affiliation(
    string Id,
    string Label,
    string? Orgname = null,
    string? Orgdiv1 = null,
    string? Country = null);
