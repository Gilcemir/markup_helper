using DocFormatter.Core.Rules.Phase2.HistDateParsing;

namespace DocFormatter.Core.Models.Phase2;

/// <summary>
/// Parsed history-block dates published by <c>EmitHistTagRule</c> for the
/// diagnostic block. <see cref="Received"/> is required for any
/// <c>[hist]…[/hist]</c> emission (DTD 4.0 invariant per
/// <c>docs/scielo_context/DTD_SCHEMA.md</c>); <see cref="Revised"/> is
/// zero-or-more in document order between <c>received</c> and
/// <c>accepted</c>; <see cref="Accepted"/> is optional and last inside the
/// hist block; <see cref="Published"/> populates the
/// <c>[histdate datetype="pub"]</c> literal that the corpus places before
/// <c>[/hist]</c>. When any of the optional fields is <see langword="null"/>,
/// the matching child tag was either absent in the source or could not be
/// parsed (per ADR-002 skip-and-warn).
/// </summary>
public sealed record HistoryDates(
    HistDate Received,
    IReadOnlyList<HistDate> Revised,
    HistDate? Accepted,
    HistDate? Published);
