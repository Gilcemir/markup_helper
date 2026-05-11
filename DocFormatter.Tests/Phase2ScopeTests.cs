using DocFormatter.Core.Reporting;
using Xunit;

namespace DocFormatter.Tests;

/// <summary>
/// Sentinel tests for <see cref="Phase2Scope.Current"/>. These pin the exact
/// contents of the cumulative scope set so a future task that ships new Phase 2
/// emitter rules without updating the scope set fails loudly here.
/// </summary>
public sealed class Phase2ScopeTests
{
    [Fact]
    public void Current_AtTask09Snapshot_ContainsStableStage1TagsAndAllPhase2EmitterTags()
    {
        // Task 09 (the Phase 4 release) adds four tags to scope: `hist`,
        // `histdate`, `received`, `accepted` — the structures
        // EmitHistTagRule now owns. The cumulative scope is the final form;
        // `author`, `fname`, `surname` and `normaff` remain OUT of scope per
        // ADR-001 anti-duplication: SciELO Markup auto-marks them. The diff
        // peels them symmetrically and aligns on the inner plain text.
        var expected = new HashSet<string>(StringComparer.Ordinal)
        {
            "accepted",
            "authorid",
            "corresp",
            "doc",
            "doctitle",
            "doi",
            "hist",
            "histdate",
            "kwdgrp",
            "label",
            "normaff",
            "received",
            "toctitle",
            "xmlabstr",
            "xref",
        };

        Assert.Equal(expected, Phase2Scope.Current);
    }

    [Fact]
    public void Current_OmitsTagsOwnedByMarkupAutoMark()
    {
        // Anti-duplication tags (Markup auto-marks them). Phase 2 must not
        // pre-emit them and the diff peels them symmetrically.
        Assert.DoesNotContain("author", Phase2Scope.Current);
        Assert.DoesNotContain("fname", Phase2Scope.Current);
        Assert.DoesNotContain("surname", Phase2Scope.Current);

        // `kwd` stays out: anti-duplication invariant from
        // docs/scielo_context/REENTRANCE.md — Markup auto-marks individual
        // keywords. Task 06 emits only `kwdgrp`.
        Assert.DoesNotContain("kwd", Phase2Scope.Current);
    }

    [Fact]
    public void Current_UsesOrdinalComparer()
    {
        // The diff strip in Phase2DiffUtility builds its lookup with
        // StringComparer.Ordinal — Phase2Scope.Current must match that.
        Assert.Contains("xmlabstr", Phase2Scope.Current);
        Assert.DoesNotContain("XMLABSTR", Phase2Scope.Current);
        Assert.Contains("hist", Phase2Scope.Current);
        Assert.DoesNotContain("HIST", Phase2Scope.Current);
    }
}
