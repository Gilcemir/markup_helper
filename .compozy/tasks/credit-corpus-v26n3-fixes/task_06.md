---
status: pending
title: creditStatement block in the Phase 3 diagnostic
type: backend
complexity: medium
dependencies:
  - task_04
---

# Task 6: creditStatement block in the Phase 3 diagnostic

## Overview
Carry the raw CREDIT statement, its recognized shape and each author's resolution status into `.diagnostic.json` under `phase3.creditStatement`, so the operator diagnoses a pending article without reopening the docx. `WritePhase3` gains a `CreditOutcome?` parameter; `Phase3Processor` passes `ctx.Credit`. The WARN-or-above write gate is unchanged (ADR-005).

<critical>
- ALWAYS READ the PRD and TechSpec before starting
- REFERENCE TECHSPEC for implementation details — do not duplicate here
- FOCUS ON "WHAT" — describe what needs to be accomplished, not how
- MINIMIZE CODE — show code only to illustrate current structure or problem areas
- TESTS REQUIRED — every task MUST include tests in deliverables
</critical>

<requirements>
- MUST add `DiagnosticCreditStatement` and `DiagnosticCreditEntry` records and a nullable `CreditStatement` property on `DiagnosticPhase3` (TechSpec "Data Models"); `DiagnosticPhase3Tag` unchanged.
- MUST add a `CreditOutcome? credit` parameter to both `WritePhase3` overloads and to `BuildPhase3Document`, and map it into the new block (null when no outcome).
- MUST keep the existing gate: no file written when `report.HighestLevel < Warn`.
- MUST serialize with the existing `SerializerOptions` (camelCase, indented); the block appears as `phase3.creditStatement` with `raw`, `shape`, `entries[]`.
- MUST update `Phase3Processor` to pass `ctx.Credit` at the single call site.
- MUST update existing `DiagnosticWriterPhase3Tests` callers for the new parameter without changing their assertions.
</requirements>

## Subtasks
- [ ] 6.1 Add the records to `DiagnosticDocument.cs` and the property on `DiagnosticPhase3`.
- [ ] 6.2 Thread the `CreditOutcome?` parameter through `WritePhase3`/`BuildPhase3Document`/`BuildPhase3` and map it.
- [ ] 6.3 Pass `ctx.Credit` from `Phase3Processor`.
- [ ] 6.4 Extend `DiagnosticWriterPhase3Tests` and `DiagnosticDocumentTests` for the new block and JSON naming.
- [ ] 6.5 Run the full suite.

## Implementation Details
Modify `DocFormatter.Core/Reporting/DiagnosticWriter.cs` (`WritePhase3` ×2, `BuildPhase3Document`, `BuildPhase3`), `DocFormatter.Core/Reporting/DiagnosticDocument.cs` (`DiagnosticPhase3` record ~line 62), `DocFormatter.Cli/Phase3Processor.cs` (call at ~line 180). Map `CreditShape` to its enum name string. See TechSpec "Data Models".

### Relevant Files
- `DocFormatter.Core/Reporting/DiagnosticWriter.cs` — phase-3 writer and serializer options.
- `DocFormatter.Core/Reporting/DiagnosticDocument.cs` — record definitions.
- `DocFormatter.Cli/Phase3Processor.cs` — the only production caller of `WritePhase3`.
- `DocFormatter.Tests/DiagnosticWriterPhase3Tests.cs`, `DocFormatter.Tests/DiagnosticDocumentTests.cs` — tests to extend (fixed-time helper already exists).

### Dependent Files
- `DocFormatter.Core/Jats/CreditOutcome.cs` — source record (task_04).
- `DocFormatter.Tests/CliPhase3Tests.cs` — may read the diagnostic JSON in end-to-end checks.

### Related ADRs
- [ADR-005: The CRediT outcome is recorded on Phase3Context and read by the diagnostic and the batch summary](../adrs/adr-005.md) — parameter plumbing and gate decision.

## Deliverables
- `phase3.creditStatement` in the diagnostic JSON.
- Updated `WritePhase3` signature and caller.
- Unit tests with 80%+ coverage **(REQUIRED)**
- Integration tests: CLI single-file run writes the block when a WARN exists **(REQUIRED)**

## Tests
- Unit tests:
  - [ ] Outcome with 2 entries (one resolved/applied, one notFound) and a WARN in the report → JSON contains `"creditStatement"` with `"raw"`, `"shape": "authorKeyed"`, two entries with `"resolution": "resolved"`/`"notFound"` and `"applied": true/false`.
  - [ ] Prose outcome → `"shape": "prose"`, `"entries": []`, raw present.
  - [ ] `credit == null` → `"creditStatement": null` and the four tag blocks unchanged.
  - [ ] Report with only INFO → `WritePhase3` returns false and writes nothing (gate preserved), even with an outcome.
  - [ ] `DiagnosticDocumentTests` round-trip includes the new property.
- Integration tests:
  - [ ] `CliPhase3Tests`: single-file run over a fixture with an unresolved author produces a `.diagnostic.json` whose `phase3.creditStatement.entries` names that author as `notFound`.
- Test coverage target: >=80%
- All tests must pass

## Success Criteria
- All tests passing
- Test coverage >=80%
- Pending v26n3 articles' diagnostics (task_09) carry the full statement text and per-author status.
