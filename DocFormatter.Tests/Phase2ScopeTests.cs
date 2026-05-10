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
    public void Current_AtTask05Snapshot_ContainsOnlyStage1BaselineTags()
    {
        var expected = new HashSet<string>(StringComparer.Ordinal)
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

        Assert.Equal(expected, Phase2Scope.Current);
    }

    [Fact]
    public void Current_DoesNotContainPhase2ReleaseTagsUntilTask06ShipsThem()
    {
        // Tasks 06 / 07 / 09 each append the release tags they emit. Until
        // those tasks land the cumulative set must NOT advertise tags whose
        // emitters are not yet wired into AddPhase2Rules — otherwise the diff
        // gate would silently keep them on the expected side and produce
        // mismatches with no produced counterpart.
        Assert.DoesNotContain("kwdgrp", Phase2Scope.Current);
        Assert.DoesNotContain("kwd", Phase2Scope.Current);
        Assert.DoesNotContain("xmlabstr", Phase2Scope.Current);
        Assert.DoesNotContain("corresp", Phase2Scope.Current);
        Assert.DoesNotContain("authorid", Phase2Scope.Current);
        Assert.DoesNotContain("hist", Phase2Scope.Current);
        Assert.DoesNotContain("histdate", Phase2Scope.Current);
        Assert.DoesNotContain("received", Phase2Scope.Current);
        Assert.DoesNotContain("accepted", Phase2Scope.Current);
    }

    [Fact]
    public void Current_UsesOrdinalComparer()
    {
        // The diff strip in Phase2DiffUtility builds its lookup with
        // StringComparer.Ordinal — Phase2Scope.Current must match that.
        Assert.Contains("author", Phase2Scope.Current);
        Assert.DoesNotContain("AUTHOR", Phase2Scope.Current);
    }
}
