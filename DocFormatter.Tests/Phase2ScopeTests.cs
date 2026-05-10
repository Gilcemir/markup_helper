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
    public void Current_AtTask06Snapshot_ContainsStableStage1TagsAndTask06Tags()
    {
        // Task 06 reset the in-scope set to "tags whose attributes are stable
        // between produced and expected at this release point". `author` and
        // `xref` are dropped from the Stage-1 baseline because their
        // attributes / occurrences differ between before/<id>.docx and
        // after/<id>.docx (task 07 owns those changes). The two new emitter
        // tags `kwdgrp` and `xmlabstr` join the set.
        var expected = new HashSet<string>(StringComparer.Ordinal)
        {
            "doc",
            "doctitle",
            "doi",
            "kwdgrp",
            "label",
            "normaff",
            "toctitle",
            "xmlabstr",
        };

        Assert.Equal(expected, Phase2Scope.Current);
    }

    [Fact]
    public void Current_OmitsTagsOwnedByLaterReleases()
    {
        // Tasks 07 / 09 each append the release tags they emit (and re-add
        // tags whose structure they finally fix, e.g. author / xref / fname /
        // surname). Until those tasks land the cumulative set must NOT
        // advertise them — otherwise the diff gate would surface their gaps
        // as false mismatches at task 06.
        Assert.DoesNotContain("author", Phase2Scope.Current);
        Assert.DoesNotContain("fname", Phase2Scope.Current);
        Assert.DoesNotContain("surname", Phase2Scope.Current);
        Assert.DoesNotContain("xref", Phase2Scope.Current);
        Assert.DoesNotContain("authorid", Phase2Scope.Current);
        Assert.DoesNotContain("corresp", Phase2Scope.Current);
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
