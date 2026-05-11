---
provider: manual
pr:
round: 1
round_created_at: 2026-05-11T18:23:22Z
status: resolved
file: DocFormatter.Core/Models/Phase2/KeywordsGroup.cs
line: 8
severity: low
author: claude-code
provider_ref:
---

# Issue 006: Stale docstring contradicts ADR-001 follow-up — rule does emit [kwd]

## Review Comment

The `KeywordsGroup` record's XML doc-comment claims:

```csharp
/// <see cref="Keywords"/> retains the parsed
/// individual terms for diagnostics; the rule itself does not emit a
/// <c>[kwd]</c> per term — Markup auto-marks those (anti-duplication invariant
/// from <c>docs/scielo_context/REENTRANCE.md</c>).
```

This contradicts both ADR-001's 2026-05-11 follow-up note ("Owned by Phase 2 (DO emit, in their own Run): [sectitle] and [kwd] inside [kwdgrp]…") and the actual implementation in `EmitKwdgrpTagRule.cs:106-117`, which explicitly emits `[kwd]term[/kwd]` for every parsed term. A reader trusting this docstring would draw the wrong conclusion about the anti-duplication contract.

Update the comment to match reality:

```csharp
/// <see cref="Keywords"/> retains the parsed individual terms; the emitter
/// wraps each term in <c>[kwd]…[/kwd]</c> per ADR-001's 2026-05-11 follow-up
/// note (Phase 2 owns inner [kwd] / [sectitle] / [p] / [email] literals).
```

## Triage

- Decision: `RESOLVED`
- Notes: Rewrote the `KeywordsGroup` XML doc-comment to reflect ADR-001's 2026-05-11 follow-up note (Phase 2 owns inner [kwd]/[sectitle]/[p]/[email]).
