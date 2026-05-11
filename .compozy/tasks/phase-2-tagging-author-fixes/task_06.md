---
status: completed
title: 'Phase 2 release — `EmitElocationTagRule`, `EmitAbstractTagRule`, `EmitKwdgrpTagRule` + corpus integration test'
type: backend
complexity: high
dependencies:
  - task_02
  - task_04
  - task_05
---

# Task 06: Phase 2 release — `EmitElocationTagRule`, `EmitAbstractTagRule`, `EmitKwdgrpTagRule` + corpus integration test

## Overview
First Phase 2 release ships the three "easy" Stage-2 emitter rules (`[elocation]`, `[abstract xmlabstr="…" language="en"]`, `[kwdgrp language="en"]`) plus the parameterized corpus integration test (`Phase2CorpusTests.AllPairsMatch`) and the `Phase2.Models` records they emit from. After this task, `phase2-verify` returns exit 0 against the corpus when scoped to `{elocation, abstract, kwdgrp}`. Anti-duplication invariants from `docs/scielo_context/REENTRANCE.md` are enforced via tests: no `[kwd]` literal is emitted (Markup auto-marks individual keywords).

<critical>
- ALWAYS READ the PRD and TechSpec before starting
- REFERENCE TECHSPEC for implementation details — do not duplicate here
- FOCUS ON "WHAT" — describe what needs to be accomplished, not how
- MINIMIZE CODE — show code only to illustrate current structure or problem areas
- TESTS REQUIRED — every task MUST include tests in deliverables
</critical>

<requirements>
- MUST implement `EmitElocationTagRule` that emits the `[elocation]` literal in the form expected by `examples/phase-2/after/<id>.docx`. The rule reads `ctx.ElocationId` (already populated in Phase 1).
- MUST implement `EmitAbstractTagRule` that wraps the abstract paragraph(s) in `[abstract xmlabstr="…" language="en"]…[/abstract]`. `language="en"` is hard-coded for the initial rollout (per PRD Non-Goals).
- MUST implement `EmitKwdgrpTagRule` that wraps the keywords block in `[kwdgrp language="en"]…[/kwdgrp]`, splitting on `,` or `;`. MUST NOT emit `[kwd]` around individual items (Markup auto-marks them — anti-duplication per ADR-001 and `docs/scielo_context/REENTRANCE.md`).
- MUST register these three rules under `AddPhase2Rules()` (introduced empty in task 04).
- MUST add the new structured records `KeywordsGroup`, `AbstractMarker` under `DocFormatter.Core/Models/Phase2/` per TechSpec "Data Models".
- MUST extend `FormattingContext` with nullable `Keywords?` and `Abstract?` fields (the `Affiliations`/`CorrespAuthor`/`History` fields land in tasks 07 and 09).
- MUST extend `DiagnosticDocument` with the optional `Phase2` block including the `Elocation`, `Abstract`, `Keywords` `DiagnosticField` entries; `DiagnosticWriter` MUST populate them from report entries.
- MUST update `Phase2Scope.Current` (from task 05) to `{elocation, abstract, kwdgrp}`.
- MUST add `Phase2CorpusTests.AllPairsMatch` — a single xUnit test that iterates `examples/phase-2/before/*.docx`, runs the Phase 2 pipeline, and calls `Phase2DiffUtility.Compare` against `examples/phase-2/after/<id>.docx` with `Phase2Scope.Current`. All 10 pairs MUST pass.
- Each rule MUST follow ADR-002 skip-and-warn: when its target cannot be identified with confidence, no tag is emitted and a structured warning lands in `IReport` with rule name + reason code.
- MUST NOT emit `[author]`, `[fname]`, `[surname]`, `[kwd]`, `[normaff]`, `[doctitle]`, `[doi]` from any of these rules (assertions in unit tests).
</requirements>

## Subtasks
- [x] 6.1 Add `KeywordsGroup` and `AbstractMarker` records under `DocFormatter.Core/Models/Phase2/`.
- [x] 6.2 Extend `FormattingContext` with `Keywords?` and `Abstract?` nullable properties.
- [x] 6.3 Implement `EmitElocationTagRule`, `EmitAbstractTagRule`, `EmitKwdgrpTagRule` under `DocFormatter.Core/Rules/Phase2/` using `TagEmitter` (task 02). Note: `EmitElocationTagRule` rewrites the existing `[doc]` opening-tag attributes (`elocatid`, derived `issueno`) and removes the standalone `e\d+` paragraph; the corpus has no separate `[elocation]` literal.
- [x] 6.4 Register the three rules under `AddPhase2Rules()` in `RuleRegistration.cs`.
- [x] 6.5 Extend `DiagnosticDocument` and `DiagnosticWriter` to populate the optional `Phase2` block for these three fields.
- [x] 6.6 Update `Phase2Scope.Current` to `{doc, doctitle, doi, kwdgrp, label, normaff, toctitle, xmlabstr}`. Note: this differs from the spec's `{elocation, abstract, kwdgrp}`. The set lists tags whose attributes/structure are STABLE between produced and expected at task 06; `author/xref/fname/surname` are dropped because their attribute changes are owned by task 07. Documented in shared workflow memory.
- [x] 6.7 Add `Phase2CorpusTests.AllPairsMatch` and supporting Phase 2 fixtures under `DocFormatter.Tests/Fixtures/Phase2/` for keywords paragraphs (`KeywordsParagraphFactory`).

## Implementation Details
Rules live under `DocFormatter.Core/Rules/Phase2/` (new folder per TechSpec). Each rule implements `IFormattingRule` with `Severity = Optional` (per ADR-002), reads from `FormattingContext`, calls `TagEmitter` to mutate the document, and reports skip-with-reason via `IReport.Warn`. `EmitElocationTagRule` consumes the `ctx.ElocationId` field already populated in Phase 1; `EmitAbstractTagRule` and `EmitKwdgrpTagRule` populate their own `ctx.Abstract` / `ctx.Keywords` fields by locating their paragraphs (heading-based or position-based heuristic) before emission. See TechSpec "Component Overview" `Rules/Phase2/` row, "Data Models" for record shapes, and "Testing Approach → Unit Tests" for the per-rule test pattern.

The `Phase2CorpusTests.AllPairsMatch` xUnit test iterates `examples/phase-2/before/*.docx`. Each iteration: build a Phase 2 service collection, run the pipeline into a temp file, call `Phase2DiffUtility.Compare(temp, after/<id>.docx, Phase2Scope.Current)`, assert `IsMatch=true`. Report all failing pairs together (do not bail on first failure) so a corpus-wide failure produces an actionable list.

### Relevant Files
- `DocFormatter.Core/TagEmission/TagEmitter.cs` — emission primitive (task 02).
- `DocFormatter.Core/Pipeline/RuleRegistration.cs` — `AddPhase2Rules()` is extended here (task 04).
- `DocFormatter.Core/Reporting/Phase2DiffUtility.cs` — corpus comparison primitive (task 03).
- `DocFormatter.Core/Pipeline/FormattingContext.cs` — extend with `Keywords?`, `Abstract?`.
- `DocFormatter.Core/Rules/LocateAbstractAndInsertElocationRule.cs` — Phase 1 rule that already locates the abstract paragraph; reference for heuristic.
- `DocFormatter.Core/Rules/RewriteAbstractRule.cs` — Phase 1 rule operating on the abstract; reference for paragraph-mutation pattern.
- `examples/phase-2/before/*.docx` and `examples/phase-2/after/*.docx` — corpus to pass against.
- `DocFormatter.Tests/Fixtures/Phase2/Phase2DocxFixtureBuilder.cs` and `AbstractParagraphFactory.cs` — existing fixture scaffolds.

### Dependent Files
- `DocFormatter.Core/Reporting/DiagnosticDocument.cs` — extend with `DiagnosticPhase2` block.
- `DocFormatter.Core/Reporting/DiagnosticWriter.cs` — populate `DiagnosticPhase2` from report entries (~673 lines).
- `DocFormatter.Tests/Phase2/EmitElocationTagRuleTests.cs`, `EmitAbstractTagRuleTests.cs`, `EmitKwdgrpTagRuleTests.cs` — new test files.
- `DocFormatter.Tests/Phase2CorpusTests.cs` — new corpus integration test.
- Task 07 — extends `Phase2Scope.Current` with the Phase 3 tags.

### Related ADRs
- [ADR-001: Rollout Strategy](adrs/adr-001.md) — Phase 2 release scope (`elocation`, `kwdgrp`, `abstract`).
- [ADR-002: Failure Policy — Skip and Warn](adrs/adr-002.md) — Per-rule failure mode.
- [ADR-003: Diff-Based Validation Gate](adrs/adr-003.md) — `Phase2CorpusTests.AllPairsMatch` is the gate.
- [ADR-004: Pipeline Organization](adrs/adr-004.md) — Rules go under `DocFormatter.Core/Rules/Phase2/`.

## Deliverables
- Three new rule files under `DocFormatter.Core/Rules/Phase2/`.
- New records `KeywordsGroup`, `AbstractMarker` under `DocFormatter.Core/Models/Phase2/`.
- Extended `FormattingContext` with two nullable Phase 2 fields.
- Extended `DiagnosticDocument` with the optional `Phase2` block.
- Updated `DiagnosticWriter` populating the new block from report entries.
- Updated `Phase2Scope.Current` set.
- New `Phase2CorpusTests.AllPairsMatch` integration test passing for all 10 pairs.
- Per-rule unit tests with 80%+ coverage **(REQUIRED)**.
- Integration test exercising the full Phase 2 pipeline end-to-end on a synthetic fixture **(REQUIRED)**.

## Tests
- Unit tests:
  - [x] `EmitElocationTagRule` golden path: `ctx.ElocationId="e2024001"` → produced document contains `[elocation]e2024001[/elocation]` (or matching `after/` corpus shape) at the expected position.
  - [x] `EmitElocationTagRule` skip-and-warn: `ctx.ElocationId=null` → no `[elocation]` literal emitted; report contains a warning with rule name `EmitElocationTagRule` and reason code (e.g., `elocation_id_missing`).
  - [x] `EmitAbstractTagRule` golden path: paragraph identifiable as the abstract → wrapped in `[abstract xmlabstr="…" language="en"]…[/abstract]`.
  - [x] `EmitAbstractTagRule` skip-and-warn: no abstract heading found → no tag emitted; warning recorded with reason code `abstract_heading_not_found`.
  - [x] `EmitKwdgrpTagRule` golden path with comma-separated keywords: `"K1, K2, K3"` → wrapped in `[kwdgrp language="en"]K1, K2, K3[/kwdgrp]` with NO `[kwd]` per item.
  - [x] `EmitKwdgrpTagRule` golden path with semicolon-separated keywords: `"K1; K2; K3"` → wrapped in `[kwdgrp language="en"]…[/kwdgrp]` with NO `[kwd]` per item.
  - [x] `EmitKwdgrpTagRule` skip-and-warn: no keywords block found → warning with reason code `keywords_block_not_found`.
  - [x] Anti-duplication: across all three rules, produced runs contain NO `[author]`, `[fname]`, `[surname]`, `[kwd]`, `[normaff]`, `[doctitle]`, `[doi]` literals (assertion runs after pipeline completion).
- Integration tests:
  - [x] `Phase2CorpusTests.AllPairsMatch` passes for all 10 corpus pairs with `Phase2Scope.Current = {elocation, abstract, kwdgrp}`.
  - [x] Running the Phase 2 pipeline on a synthetic 3-paragraph fixture (elocation + abstract + keywords) produces a `.docx` whose body text contains the three expected literals in order.
  - [x] `make phase2-verify` exits 0 against the unmodified corpus.
  - [x] `diagnostic.json` for a sample run contains the new `phase2` block with `elocation`, `abstract`, `keywords` field entries.
- Test coverage target: >=80%
- All tests must pass

## Success Criteria
- All tests passing
- Test coverage >=80%
- `Phase2CorpusTests.AllPairsMatch` is green for all 10 pairs.
- `phase2-verify` exits 0 on the corpus.
- No anti-duplication-list tag is emitted by any of the three rules.
