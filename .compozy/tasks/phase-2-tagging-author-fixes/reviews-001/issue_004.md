---
provider: manual
pr:
round: 1
round_created_at: 2026-05-11T18:23:22Z
status: resolved
file: DocFormatter.Core/Rules/Phase2/EmitAuthorXrefsRule.cs
line: 593
severity: medium
author: claude-code
provider_ref:
---

# Issue 004: ORCID "already tagged" check uses 200-char magic window instead of actual match span

## Review Comment

When wrapping plain ORCIDs in `[authorid …]…[/authorid]`, the rule first collects positions of ORCIDs that are *already* wrapped:

```csharp
var alreadyTagged = new HashSet<int>();
foreach (Match m in AlreadyTaggedOrcidPattern.Matches(inner))
{
    alreadyTagged.Add(m.Index);
}
// ...
foreach (Match m in OrcidPattern.Matches(inner))
{
    var inWrapper = false;
    foreach (var taggedStart in alreadyTagged)
    {
        if (m.Index > taggedStart && m.Index < taggedStart + 200)
        {
            inWrapper = true;
            break;
        }
    }
    // ...
}
```

The `+ 200` is a magic radius. `AlreadyTaggedOrcidPattern` already produces a `Match` whose `Index` and `Length` describe the exact span; the actual wrapper is `[authorid authidtp="orcid"]XXXX-XXXX-XXXX-XXXX[/authorid]`, ~52 chars — well inside 200 today, but the guard is fragile against future authorid attribute variants and silently includes false positives when two ORCIDs sit close together.

Replace with the precise span:

```csharp
var taggedSpans = AlreadyTaggedOrcidPattern.Matches(inner)
    .Select(m => (Start: m.Index, End: m.Index + m.Length))
    .ToArray();
// ...
var inWrapper = taggedSpans.Any(s => m.Index >= s.Start && m.Index < s.End);
```

Also: a `HashSet<int>` whose only operation is `foreach` adds no value over an array. Either use a sorted array (binary search) or just a flat list. Using `HashSet` here is mildly misleading (no de-dup benefit — match indices are already unique).

## Triage

- Decision: `RESOLVED`
- Notes: Replaced the `HashSet<int>` start-only collection plus `+200` window with an array of precise `(Start, End)` spans from `AlreadyTaggedOrcidPattern.Matches`. The inWrapper check now uses `m.Index >= span.Start && m.Index < span.End`.
