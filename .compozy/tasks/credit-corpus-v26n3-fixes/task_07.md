---
status: pending
title: Batch summary pendency block for CRediT and broken names
type: backend
complexity: medium
dependencies:
  - task_04
  - task_05
  - task_06
---

# Task 7: Batch summary pendency block for CRediT and broken names

## Overview
Extend `_batch_summary.txt` so each article with a CRediT pendency or a broken contributor name gets indented detail lines (applied authors, pending authors with reason, broken-name count), letting the operator close an edition from one file. Clean articles keep their single line.

<critical>
- ALWAYS READ the PRD and TechSpec before starting
- REFERENCE TECHSPEC for implementation details — do not duplicate here
- FOCUS ON "WHAT" — describe what needs to be accomplished, not how
- MINIMIZE CODE — show code only to illustrate current structure or problem areas
- TESTS REQUIRED — every task MUST include tests in deliverables
</critical>

<requirements>
- MUST add `CreditOutcome? Credit` and `int BrokenNames` to the CLI `Phase3Outcome` and populate them in `Phase3Processor` (`BrokenNames` = count of report entries with rule `contrib-names` at WARN).
- MUST keep the existing header line and per-file line format unchanged.
- MUST append indented lines only when `Credit.Disposition != "autoApplied"` or `BrokenNames > 0`, using the wording in TechSpec "Data Models — Batch summary format" (`credit: applied …; pending X (notFound)`, `credit: free prose (not auto-applied)`, `credit: header found, body empty`, `contrib-names: N broken surname(s)`).
- MUST list unknown terms per pending author (`pending X (unknown term: …)`).
- MUST NOT alter Phase 1/2 batch summaries.
</requirements>

## Subtasks
- [ ] 7.1 Extend `Phase3Outcome` and its construction in `Phase3Processor`.
- [ ] 7.2 Extend `WritePhase3BatchSummary` with the pendency block.
- [ ] 7.3 Add `CliPhase3Tests` for clean, pending, prose, header-empty and broken-name articles.
- [ ] 7.4 Run the full suite.

## Implementation Details
Modify `DocFormatter.Cli/CliApp.cs` (`WritePhase3BatchSummary` ~lines 651-675, `Phase3Outcome` record) and `DocFormatter.Cli/Phase3Processor.cs` (outcome construction; the per-file `IReport` is in scope there). See TechSpec "Data Models" for the exact line formats.

### Relevant Files
- `DocFormatter.Cli/CliApp.cs` — batch loop and summary writer.
- `DocFormatter.Cli/Phase3Processor.cs` — builds the outcome per file, has `ctx.Credit` and the report.
- `DocFormatter.Tests/CliPhase3Tests.cs` — existing batch-mode tests and fixtures.

### Dependent Files
- `DocFormatter.Core/Jats/CreditOutcome.cs` — consumed here.
- `DocFormatter.Core/Jats/ContribNamesInjector.cs` — its rule name is the counting key.
- `README.md` — summary format documented in task_10.

### Related ADRs
- [ADR-005](../adrs/adr-005.md) — outcome as the single source for the summary.
- [ADR-002](../adrs/adr-002.md) — broken names reported as a pendency, not a blocker.

## Deliverables
- Pendency block in `_batch_summary.txt`.
- Unit tests with 80%+ coverage **(REQUIRED)**
- Integration tests: batch run over a fixture folder **(REQUIRED)**

## Tests
- Unit tests:
  - [ ] Outcome autoApplied, BrokenNames 0 → only the existing line, no indented lines.
  - [ ] Outcome confirmed with applied [TVB, QHTP] and pending NHN notFound → line `  credit: applied TVB, QHTP; pending NHN (notFound)`.
  - [ ] Pending author with unknown term → `pending X (unknown term: Metodology)`.
  - [ ] Prose → `  credit: free prose (not auto-applied)`.
  - [ ] headerEmpty → `  credit: header found, body empty`.
  - [ ] BrokenNames 7 → `  contrib-names: 7 broken surname(s)`.
  - [ ] Phase 1/2 `WriteBatchSummary` output unchanged (existing tests).
- Integration tests:
  - [ ] Batch run over a fixture package with one clean and one pending article → summary has the block only under the pending file.
- Test coverage target: >=80%
- All tests must pass

## Success Criteria
- All tests passing
- Test coverage >=80%
- v26n3 batch summaries (task_09) list exactly the pendencies of the PRD acceptance table.
