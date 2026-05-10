namespace DocFormatter.Core.Reporting;

/// <summary>
/// Cumulative in-scope tag set used by the <c>phase2-verify</c> diff gate
/// (ADR-003 / ADR-006). The set is passed verbatim to
/// <see cref="Phase2DiffUtility.Compare(string,string,System.Collections.Generic.IReadOnlyCollection{string})"/>:
/// every tag pair NOT in this set has its brackets removed (content kept) on
/// BOTH sides before the string compare runs (symmetric strip introduced by
/// task 06). The set therefore lists the tags whose <em>attributes and
/// structure</em> are guaranteed to match between produced and expected at
/// this release point.
///
/// <para>
/// Task 07 adds <c>xref</c>, <c>authorid</c>, and <c>corresp</c> — the three
/// tags whose attributes/structure the new rules now control. <c>author</c>,
/// <c>fname</c>, <c>surname</c> and <c>normaff</c> remain OUT of scope per
/// ADR-001 anti-duplication: SciELO Markup auto-marks them and Phase 2 must
/// not pre-mark. The symmetric content-keep strip handles those tags by
/// peeling wrappers on both sides; the diff aligns on the inner plain text.
/// </para>
///
/// <para>
/// The sentinel test in <c>Phase2ScopeTests</c> pins the exact contents and
/// fails when a future release ships without updating this set.
/// </para>
/// </summary>
public static class Phase2Scope
{
    public static readonly IReadOnlySet<string> Current = new HashSet<string>(StringComparer.Ordinal)
    {
        // Stage-1 baseline whose structure already matches between
        // before/<id>.docx and after/<id>.docx (no Phase 4 attrs in flight).
        "doc",
        "doctitle",
        "doi",
        "label",
        "normaff",
        "toctitle",

        // Task 06 emitters.
        "kwdgrp",
        "xmlabstr",

        // Task 07 emitters / patches.
        "authorid",
        "corresp",
        "xref",
    };
}
