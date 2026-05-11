---
status: completed
title: 'Phase 4 release — `EmitHistTagRule` (`received` / `revised*` / `accepted?` + `[histdate datetype="pub"]`)'
type: backend
complexity: high
dependencies:
  - task_07
  - task_08
---

# Task 09: Phase 4 release — `EmitHistTagRule` (`received` / `revised*` / `accepted?` + `[histdate datetype="pub"]`)

## Overview
Final Phase 2 release ships the `[hist]` block: `received` (required, first), `revised*` (zero or more), `accepted?` (optional, last), plus `[histdate datetype="pub"]` for the publication date. The rule consumes `HistDateParser` (task 08) per candidate paragraph, assembles the block in the strict DTD ordering, and emits it via `TagEmitter`. After this task, the corpus diff gate passes for the cumulative scope `{elocation, abstract, kwdgrp, corresp, xref, authorid, hist}`.

<critical>
- ALWAYS READ the PRD and TechSpec before starting
- REFERENCE TECHSPEC for implementation details — do not duplicate here
- FOCUS ON "WHAT" — describe what needs to be accomplished, not how
- MINIMIZE CODE — show code only to illustrate current structure or problem areas
- TESTS REQUIRED — every task MUST include tests in deliverables
</critical>

<requirements>
- MUST implement `EmitHistTagRule` under `DocFormatter.Core/Rules/Phase2/` consuming `HistDateParser` and `ctx.History` (new) to produce the `[hist]` block.
- MUST emit children in the strict DTD ordering: `received` first, then `revised*` (zero or more, in document order), then `accepted?` (optional, last). Then `[histdate datetype="pub"]` follows the closing `[/hist]` (or per the `after/` corpus shape — verify against examples).
- Every emitted child MUST carry `dateiso="YYYYMMDD"` zero-padded via `HistDate.ToDateIso()` (task 08).
- MUST add the `HistoryDates` record under `DocFormatter.Core/Models/Phase2/` per TechSpec "Data Models" with the four fields (`Received`, `Revised`, `Accepted`, `Published`).
- MUST extend `FormattingContext` with nullable `History?` field.
- MUST register the rule under `AddPhase2Rules()`.
- MUST update `Phase2Scope.Current` to the full cumulative set `{elocation, abstract, kwdgrp, corresp, xref, authorid, hist}`.
- MUST extend `DiagnosticDocument.Phase2` with a `Hist` summary field.
- MUST follow ADR-002 skip-and-warn: if `received` is missing or undetectable, the entire `[hist]` block is omitted and a structured warning is logged. NEVER emit a partial `[hist]` (would violate DTD).
- MUST NOT emit any `[hist]`-related literal whose date child cannot pass `dateiso` validation.
</requirements>

## Subtasks
- [x] 9.1 Add `HistoryDates` record under `DocFormatter.Core/Models/Phase2/`.
- [x] 9.2 Extend `FormattingContext` with `History?` nullable field.
- [x] 9.3 Implement `EmitHistTagRule` under `DocFormatter.Core/Rules/Phase2/` using `TagEmitter` and `HistDateParser`.
- [x] 9.4 Implement the strict-ordering and skip-on-missing-received logic.
- [x] 9.5 Register the rule under `AddPhase2Rules()`.
- [x] 9.6 Extend `DiagnosticDocument.Phase2` and `DiagnosticWriter` with the `Hist` summary.
- [x] 9.7 Update `Phase2Scope.Current` to the full cumulative set; re-run `Phase2CorpusTests.AllPairsMatch`.

## Implementation Details
The rule lives at `DocFormatter.Core/Rules/Phase2/EmitHistTagRule.cs`. It locates candidate paragraphs (informed by Phase 1 `MoveHistoryRule` heuristics — those paragraphs already contain "Received", "Accepted", "Published" text), feeds each through the matching `HistDateParser` entry point, assembles `HistoryDates`, and emits the `[hist]` block via `TagEmitter`. See TechSpec "Component Overview" `Rules/Phase2/` row, "Data Models → HistoryDates", and "Known Risks → HistDateParser drift" for context. Strict ordering — required by DTD 4.0 — is enforced inside the rule, not by the parser.

`Phase2DiffUtility` already supports the `hist` tag-name in the scope set; no diff-utility change is needed.

### Relevant Files
- `DocFormatter.Core/TagEmission/TagEmitter.cs` — emission primitive (task 02).
- `DocFormatter.Core/Rules/Phase2/HistDateParsing/HistDateParser.cs` — date parser (task 08).
- `DocFormatter.Core/Pipeline/FormattingContext.cs` — extend with `History?`.
- `DocFormatter.Core/Rules/MoveHistoryRule.cs` — Phase 1 rule that locates the history paragraph; reference for paragraph-location heuristic.
- `examples/phase-2/before/*.docx` and `examples/phase-2/after/*.docx` — corpus to pass against.
- `docs/scielo_context/README.md` — `dateiso` `YYYYMMDD` invariant and `[hist]` ordering rule.

### Dependent Files
- `DocFormatter.Core/Pipeline/RuleRegistration.cs` — `AddPhase2Rules()` extended.
- `DocFormatter.Core/Reporting/DiagnosticDocument.cs` and `DiagnosticWriter.cs` — `DiagnosticPhase2.Hist` added.
- `DocFormatter.Tests/Phase2/EmitHistTagRuleTests.cs` — new test file.
- `DocFormatter.Tests/Phase2CorpusTests.cs` — re-runs at the full cumulative scope (test code unchanged; `Phase2Scope.Current` changes).

### Related ADRs
- [ADR-001: Rollout Strategy](adrs/adr-001.md) — Phase 4 is the final release.
- [ADR-002: Failure Policy — Skip and Warn](adrs/adr-002.md) — `[hist]` is omitted entirely when `received` cannot be detected.
- [ADR-003: Diff-Based Validation Gate](adrs/adr-003.md) — Cumulative scope reaches its final form here.
- [ADR-007: Phase 4 Date-Parser Port — Rewrite from Scratch](adrs/adr-007.md) — Constrains how the parser dependency is built.

## Deliverables
- New rule file `DocFormatter.Core/Rules/Phase2/EmitHistTagRule.cs`.
- New record `HistoryDates` under `DocFormatter.Core/Models/Phase2/`.
- Extended `FormattingContext` with one nullable Phase 2 field.
- Extended `DiagnosticDocument.Phase2` with the `Hist` summary.
- Updated `Phase2Scope.Current` to the full cumulative set.
- `Phase2CorpusTests.AllPairsMatch` green for all 10 pairs at the cumulative scope.
- Per-rule unit tests with 80%+ coverage **(REQUIRED)**.
- Integration test exercising Phase 2 + Phase 3 + Phase 4 end-to-end on a synthetic fixture **(REQUIRED)**.

## Tests
- Unit tests:
  - [ ] Golden path with received + accepted: `History = (Received: 2024-03-12, Revised: [], Accepted: 2024-04-15, Published: 2024-05-01)` → emits `[hist][received dateiso="20240312"]…[/received][accepted dateiso="20240415"]…[/accepted][/hist][histdate datetype="pub" dateiso="20240501"]…[/histdate]` (or per `after/` corpus shape).
  - [ ] Golden path with received only: emits `[hist][received dateiso="…"]…[/received][/hist]` (no `accepted`).
  - [ ] Golden path with received + 2 revisions + accepted: revisions emitted in document order between `received` and `accepted`.
  - [ ] Skip-and-warn: `Received` is null → no `[hist]` literal emitted; warning with reason code `hist_received_missing`.
  - [ ] Skip-and-warn: `History` is null → no `[hist]` literal emitted; warning with reason code `hist_dates_unrecognized`.
  - [ ] Strict ordering: revised emitted BEFORE accepted; accepted emitted LAST inside `[hist]`.
  - [ ] `dateiso` zero-padding: month-only date (`(2024, 3, null)`) emits `dateiso="20240300"`.
  - [ ] `dateiso` zero-padding: year-only date emits `dateiso="20240000"`.
  - [ ] Anti-duplication: produced runs contain NO `[author]`, `[fname]`, `[surname]`, `[kwd]`, `[normaff]` literals from this rule.
  - [ ] Skip on parser failure: when `HistDateParser` returns null for the received candidate, the rule skips the entire block (does not partially emit).
- Integration tests:
  - [ ] `Phase2CorpusTests.AllPairsMatch` passes for all 10 pairs at the full cumulative scope `{elocation, abstract, kwdgrp, corresp, xref, authorid, hist}`.
  - [ ] Synthetic fixture with received + accepted + published paragraphs → produced `.docx` body contains the `[hist]` block in strict order.
  - [ ] `make phase2-verify` exits 0 against the unmodified corpus after this task.
  - [ ] `diagnostic.json` for a sample run contains the `phase2.hist` summary with the parsed dates.
- Test coverage target: >=80%
- All tests must pass

## Success Criteria
- All tests passing
- Test coverage >=80%
- `Phase2CorpusTests.AllPairsMatch` green at the full cumulative scope.
- `phase2-verify` exits 0 on the corpus.
- No `[hist]` block ever emitted without `received` (DTD safety).
- Operator path through Stage 2 reduces to verification + manual fill-ins for tags Markup auto-marks unreliably (PRD G2 reached).
