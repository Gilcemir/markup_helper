---
provider: manual
pr:
round: 1
round_created_at: 2026-05-11T18:23:22Z
status: resolved
file: DocFormatter.Core/Rules/Phase2/EmitAuthorXrefsRule.cs
line: 209
severity: medium
author: claude-code
provider_ref:
---

# Issue 002: Inconsistent ctx propagation — Authors guarded, Affiliations and corresp overwrite unconditionally

## Review Comment

`EmitAuthorXrefsRule.Apply` carefully avoids clobbering `ctx.Authors` populated by an upstream Phase 1 rule:

```csharp
// Only populate ctx.Authors when Phase 1 didn't already (Phase 2 in
// isolation needs the data; Phase 1+2 chained should defer to Phase 1).
if (ctx.Authors.Count == 0)
{
    ctx.Authors.AddRange(authors);
}
```

…and applies the same guard to `CorrespondingAuthorIndex` (line 204). But two lines later it unconditionally overwrites `ctx.Affiliations`:

```csharp
ctx.Affiliations = allAffIds
    .Select(id => new Affiliation(id, ExtractLabelFromAffId(id)))
    .ToArray();
```

Same with the diagnostic-build path: `ctx.CorrespAuthor` is set inside `EmitCorrespTagRule.Apply` without an "only if null" check (`EmitCorrespTagRule.cs:89-93`).

If/when a Phase 1 rule starts populating `ctx.Affiliations` (e.g. with extra `Orgname`/`Orgdiv1`/`Country` data the Affiliation record was designed for — see `Models/Phase2/Affiliation.cs:10-15`), this rule will silently strip those richer fields back down to just `(Id, Label)`.

Either (a) apply the same `if (ctx.Affiliations is null)` guard used for `Authors`/`CorrespondingAuthorIndex`, or (b) document that Phase 2 is the sole owner of `Affiliations` and `CorrespAuthor` and remove the guards on Authors/CorrespondingAuthorIndex for consistency. The current mix is the bug-prone shape.

## Triage

- Decision: `RESOLVED`
- Notes: Option (a) applied — `ctx.Affiliations` and `ctx.CorrespAuthor` now both guarded with `if (ctx.… is null)` so Phase 1 data is preserved. Matches the existing pattern for `ctx.Authors` / `ctx.CorrespondingAuthorIndex`.
