---
provider: manual
pr:
round: 1
round_created_at: 2026-05-11T18:23:22Z
status: resolved
file: DocFormatter.Core/Rules/Phase2/EmitHistTagRule.cs
line: 92
severity: medium
author: claude-code
provider_ref:
---

# Issue 005: Idempotency short-circuit returns silently — no diagnostic signal

## Review Comment

When the rule detects that a `[hist`, `[received`, `[accepted`, or `[histdate` literal already exists in the body, it returns immediately:

```csharp
if (candidates.AnyHistLiteral)
{
    // Idempotency: a prior run already wrapped the block.
    return;
}
```

No `report.Info(...)` call, no warn, nothing. From the operator's perspective (reading `diagnostic.json`), this case is indistinguishable from "the rule didn't run" or "no hist paragraphs found at all." Compare with the other paths in this rule — both the success path (line 176) and the missing-received path (line 100) emit a report entry. The diagnostic builder in `DiagnosticWriter.BuildHistDiagnostic` keys off these entries, so this silent return leads the `Phase2.Hist` field to fall back to `Missing` confidence even though the document is correctly tagged.

Emit an info entry so the operator sees what happened:

```csharp
if (candidates.AnyHistLiteral)
{
    report.Info(Name, "[hist] block already present; rule skipped (idempotent)");
    return;
}
```

Same pattern applies to any other rule with a similar silent idempotency check — worth a brief audit across `EmitCorrespTagRule` (`EmitCorrespTagRule.cs:163-167` returns `(null, null)` silently when `[corresp` already exists).

## Triage

- Decision: `RESOLVED`
- Notes: `EmitHistTagRule` now emits `report.Info(Name, HistAlreadyTaggedMessage)` on the idempotent skip path. `EmitCorrespTagRule` was refactored to use a `CorrespLookup` record so the "already-tagged" case (`CorrespAlreadyTaggedMessage`) is reported separately from the "not found" case. Both new message constants are exposed for tests/diagnostics.
