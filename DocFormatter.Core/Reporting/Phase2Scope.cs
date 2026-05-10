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
/// Task 06 contents = Stage-1 content-stable baseline + the two new emitter
/// tags. Note that <c>author</c> and <c>xref</c> are NOT in the set yet — their
/// attributes (<c>rid</c>, <c>corresp</c>, <c>deceased</c>, <c>eqcontr</c>,
/// the corresp / aff xref decomposition) are owned by task 07. Until task 07
/// ships, including them in scope would surface task-07 gaps as task-06 diff
/// failures. Once task 07 ships, it re-adds <c>author</c> and <c>xref</c>
/// alongside <c>authorid</c> and <c>corresp</c>.
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
        // before/<id>.docx and after/<id>.docx (no Phase 3/4 attrs in flight).
        // Note: fname / surname are intentionally NOT in scope yet. Several
        // corpus articles (e.g. 5523) carry partially-tagged [fname]/[surname]
        // pairs in the BEFORE that the AFTER decomposes differently — that
        // tag-boundary churn is task 07's responsibility. Until task 07 ships
        // we peel both wrappers symmetrically and let the diff align on
        // plain-text content.
        "doc",
        "doctitle",
        "doi",
        "label",
        "normaff",
        "toctitle",

        // Task 06 emitters.
        "kwdgrp",
        "xmlabstr",
    };
}
