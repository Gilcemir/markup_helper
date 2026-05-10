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
    public void Current_AtTask07Snapshot_ContainsStableStage1TagsAndTaskEmitterTags()
    {
        // Task 07 adds three tags to scope: `xref`, `authorid`, `corresp` —
        // the structures EmitAuthorXrefsRule and EmitCorrespTagRule now own.
        // `author`, `fname`, `surname` and `normaff` remain OUT of scope per
        // ADR-001 anti-duplication: SciELO Markup auto-marks them. The diff
        // peels them symmetrically and aligns on the inner plain text.
        var expected = new HashSet<string>(StringComparer.Ordinal)
        {
            "authorid",
            "corresp",
            "doc",
            "doctitle",
            "doi",
            "kwdgrp",
            "label",
            "normaff",
            "toctitle",
            "xmlabstr",
            "xref",
        };

        Assert.Equal(expected, Phase2Scope.Current);
    }

    [Fact]
    public void Current_OmitsTagsOwnedByLaterReleases()
    {
        // Anti-duplication tags (Markup auto-marks them). Phase 2 must not
        // pre-emit them and the diff peels them symmetrically.
        Assert.DoesNotContain("author", Phase2Scope.Current);
        Assert.DoesNotContain("fname", Phase2Scope.Current);
        Assert.DoesNotContain("surname", Phase2Scope.Current);

        // Task 09 (Phase 4) ownership.
        Assert.DoesNotContain("hist", Phase2Scope.Current);
        Assert.DoesNotContain("histdate", Phase2Scope.Current);
        Assert.DoesNotContain("received", Phase2Scope.Current);
        Assert.DoesNotContain("accepted", Phase2Scope.Current);
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
    }
}
