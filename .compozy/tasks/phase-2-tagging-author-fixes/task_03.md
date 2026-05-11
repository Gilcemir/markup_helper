---
status: completed
title: '`Phase2DiffUtility` — body-text extraction with scope-filtered string compare'
type: backend
complexity: medium
dependencies: []
---

# Task 03: `Phase2DiffUtility` — body-text extraction with scope-filtered string compare

## Overview
Each Phase 2 release is gated by an objective diff against the curated `examples/phase-2/{before,after}/` corpus (ADR-003). This task delivers the comparison primitive: extract body text from each `.docx` (preserving SciELO `[tag]` literals), strip out-of-scope tags from the `after/` side, and report the first divergence with surrounding context. The utility is reused unchanged across Phases 2, 3, and 4 with a different scope set per release.

<critical>
- ALWAYS READ the PRD and TechSpec before starting
- REFERENCE TECHSPEC for implementation details — do not duplicate here
- FOCUS ON "WHAT" — describe what needs to be accomplished, not how
- MINIMIZE CODE — show code only to illustrate current structure or problem areas
- TESTS REQUIRED — every task MUST include tests in deliverables
</critical>

<requirements>
- MUST expose the public API from TechSpec "Core Interfaces → Phase2DiffUtility": `Compare(producedDocxPath, expectedDocxPath, inScopeTags) → DiffResult(IsMatch, FirstDivergenceOffset, ProducedContext, ExpectedContext)`.
- MUST extract a flat body-text string from each `.docx` by concatenating every paragraph's runs with `\n` separators, preserving SciELO `[tag …]…[/tag]` literals exactly.
- MUST normalize whitespace per paragraph: collapse repeated whitespace runs to a single space; trim leading/trailing whitespace.
- MUST strip every `[tagname …]…[/tagname]` pair from the **expected** (`after/`) text whose `tagname` is NOT in `inScopeTags`. MUST NOT strip from the produced text (it should already only contain in-scope tags).
- MUST report `FirstDivergenceOffset` as the byte index in the normalized expected text, with ~80 chars of context on each side via `ProducedContext` / `ExpectedContext`.
- MUST tolerate nested tags during stripping (one level of nesting is sufficient for SciELO 4.0; deeper is acceptable bonus).
- MUST live under `DocFormatter.Core/Reporting/Phase2DiffUtility.cs` as a `static class`.
</requirements>

## Subtasks
- [x] 3.1 Create `DocFormatter.Core/Reporting/Phase2DiffUtility.cs` with `Compare(...)` returning `DiffResult`.
- [x] 3.2 Implement body-text extraction: open the `.docx` via `WordprocessingDocument.Open(path, false)`; iterate `MainDocumentPart.Document.Body.Descendants<Paragraph>()`; concatenate run text per paragraph; join paragraphs with `\n`.
- [x] 3.3 Implement whitespace normalization: collapse `\s+` → single space, trim per paragraph, then concatenate.
- [x] 3.4 Implement out-of-scope stripping: a regex-driven pass that removes `[tag …]…[/tag]` pairs whose name is not in `inScopeTags`.
- [x] 3.5 Implement first-divergence reporting: walk both strings character-by-character; on mismatch, slice context windows of ~80 chars on each side.
- [x] 3.6 Define the `DiffResult` record alongside the utility.

## Implementation Details
The utility is pure I/O + string manipulation — no DI, no rule pipeline. Lives in `DocFormatter.Core/Reporting/Phase2DiffUtility.cs`. The expected text receives the strip pass; the produced text does not. SciELO bracket syntax is tightly constrained (per `docs/scielo_context/README.md`), so a careful regex is sufficient — no full parser needed (per ADR-006). See TechSpec "Core Interfaces → Phase2DiffUtility" for the public surface and "Known Risks → false positives from whitespace / over-stripping" for edge-case discipline.

### Relevant Files
- `examples/phase-2/before/*.docx` and `examples/phase-2/after/*.docx` — the 10-pair corpus this utility validates (5136, 5293, 5313, 5419, 5424, 5434, 5449, 5458, 5523, 5549).
- `docs/scielo_context/README.md` — DTD 4.0 vocabulary (informs the closed set of tag names that can appear).
- `DocFormatter.Core/Reporting/DiagnosticWriter.cs` — sibling file for layout reference (~673 lines).

### Dependent Files
- `DocFormatter.Tests/Phase2DiffUtilityTests.cs` — new test file.
- Task 05 — CLI dispatcher invokes this from `RunPhase2Verify`.
- Task 06 — `Phase2CorpusTests.AllPairsMatch` calls this from xUnit.

### Related ADRs
- [ADR-003: Diff-Based Validation Gate](adrs/adr-003.md) — Defines the gate semantics this utility implements.
- [ADR-006: Diff Utility — Body-Text Extraction with Out-of-Scope Tag Stripping](adrs/adr-006.md) — Codifies approach A (regex strip), rejecting structured-parser and off-the-shelf-diff alternatives.

## Deliverables
- New file `DocFormatter.Core/Reporting/Phase2DiffUtility.cs` with the `Compare` method and `DiffResult` record.
- New test file `DocFormatter.Tests/Phase2DiffUtilityTests.cs`.
- Unit tests with 80%+ coverage **(REQUIRED)**.
- Integration tests over at least 2 corpus pairs (one passing, one synthetically mutated to fail) **(REQUIRED)**.

## Tests
- Unit tests:
  - [x] Body-text extraction: a 3-paragraph fixture with text "alpha", "beta", "gamma" yields `"alpha\nbeta\ngamma"`.
  - [x] Whitespace normalization: paragraph with `"  alpha   beta  "` normalizes to `"alpha beta"`.
  - [x] Tag preservation: paragraph containing `[abstract language="en"]body[/abstract]` retains the literal verbatim.
  - [x] Out-of-scope strip: with `inScopeTags = {"abstract"}`, expected text `"[kwdgrp language=\"en\"]K1, K2[/kwdgrp][abstract]X[/abstract]"` strips `[kwdgrp …]…[/kwdgrp]` and keeps `[abstract]X[/abstract]`.
  - [x] Out-of-scope strip preserves leading/trailing context around the stripped pair.
  - [x] Equal strings → `IsMatch=true`, `FirstDivergenceOffset=null`.
  - [x] Strings diverging at offset 7 → `IsMatch=false`, `FirstDivergenceOffset=7`, both contexts populated with ~80 chars on each side (truncated at string ends).
  - [x] Empty `inScopeTags` strips every recognized SciELO tag pair from the expected text.
  - [x] Tag with attributes containing a literal `=` (e.g., `dateiso="20240101"`) is matched and stripped correctly.
- Integration tests:
  - [x] Compare two byte-identical `.docx` files → `IsMatch=true`.
  - [x] Compare a `.docx` against a copy with one paragraph mutated → `IsMatch=false`, divergence offset within the mutated paragraph.
  - [x] Compare `examples/phase-2/before/5136.docx` against itself with sufficient `inScopeTags` → `IsMatch=true`. *Deviation note:* `before/5136.docx` already contains Stage-1 SciELO bracket markup (`[author]`, `[doctitle]`, `[normaff]`, `[xref]`, `[doi]`, `[fname]`, `[surname]`, `[label]`, `[toctitle]`, `[doc]`); under the strict "strip expected only" semantic, the listed scope `{abstract,kwdgrp,elocation}` would not yield `IsMatch=true`. The integration test discovers the actual tag set from the file dynamically (so the assertion holds) and the finding is recorded in shared workflow memory as an Open Risk for tasks 05/06/07/09 to consider when defining cumulative `inScopeTags`.
- Test coverage target: >=80%
- All tests must pass

## Success Criteria
- All tests passing
- Test coverage >=80%
- `Phase2DiffUtility.Compare` is the only diff entry point used by `phase2-verify` and the corpus integration test.
- False-positive rate from whitespace on the corpus is zero (verified during task 06 corpus dry-run).
