using DocumentFormat.OpenXml.Wordprocessing;

namespace DocFormatter.Core.Models.Phase2;

/// <summary>
/// Identifies the corresponding author so the <c>[corresp id="c1"]…[/corresp]</c>
/// emitter (and the per-author xref emitter) can consume the same value.
/// <see cref="AuthorIndex"/> is the position into <c>ctx.Authors</c>;
/// <see cref="Email"/> and <see cref="Orcid"/> mirror the Phase 1 extraction.
/// <see cref="Paragraph"/> is the corresp paragraph in the body (the one
/// holding <c>* E-mail: …</c> or <c>Corresponding author: …</c>) that the
/// emitter wraps.
/// </summary>
public sealed record CorrespAuthor(
    int? AuthorIndex,
    string? Email,
    string? Orcid,
    Paragraph Paragraph);
