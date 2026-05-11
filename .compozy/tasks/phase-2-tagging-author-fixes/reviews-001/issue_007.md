---
provider: manual
pr:
round: 1
round_created_at: 2026-05-11T18:23:22Z
status: resolved
file: DocFormatter.Core/TagEmission/TagEmitter.cs
line: 144
severity: low
author: claude-code
provider_ref:
---

# Issue 007: Dead private helper TagEmitter.BuildRun has no callers

## Review Comment

`private static Run BuildRun(string text) => BuildColoredRun(text, color: null);` at `TagEmitter.cs:144-145` has zero callers — neither the public surface (`OpeningTag`, `ClosingTag`, `TagLiteralRun`, `InsertOpeningBefore`, `InsertClosingAfter`, `WrapParagraphContent`) nor any other private method invokes it. Each emitter rule (`EmitAbstractTagRule`, `EmitKwdgrpTagRule`, `EmitCorrespTagRule`, `EmitHistTagRule`, `EmitAuthorXrefsRule`) has its own local `BuildPlainRun` helper rather than reaching for this one.

Delete the unused helper. If the intent was to provide a public uncolored-run builder for callers, expose it (`public static Run PlainRun(string text)`) and migrate the duplicated per-rule helpers to use it; either resolution removes the present "exists but unused" state.

## Triage

- Decision: `RESOLVED`
- Notes: Option (a) applied — deleted the unused private `BuildRun(string)` from `TagEmitter`. Each emitter rule keeps its own `BuildPlainRun` local helper.
