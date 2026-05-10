namespace DocFormatter.Core.Reporting;

/// <summary>
/// Cumulative in-scope tag set used by the <c>phase2-verify</c> diff gate
/// (ADR-003 / ADR-006). The set is passed verbatim to
/// <see cref="Phase2DiffUtility.Compare(string,string,System.Collections.Generic.IReadOnlyCollection{string})"/>:
/// every tag pair NOT in this set is stripped from the expected (after) side
/// before the string compare runs.
///
/// <para>
/// The set seeds with the Stage-1 bracket tags already carried by
/// <c>examples/phase-2/before/</c> so the strip never erases them from the
/// expected side. Tasks 06 / 07 / 09 each append the new Phase 2 release tags
/// they emit. The sentinel test in <c>Phase2ScopeTests</c> pins the exact
/// contents and fails when a future release ships without updating it.
/// </para>
/// </summary>
public static class Phase2Scope
{
    public static readonly IReadOnlySet<string> Current = new HashSet<string>(StringComparer.Ordinal)
    {
        "author",
        "doc",
        "doctitle",
        "doi",
        "fname",
        "label",
        "normaff",
        "surname",
        "toctitle",
        "xref",
    };
}
