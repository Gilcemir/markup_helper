---
status: completed
title: DataAvailabilityInjector
type: backend
complexity: medium
dependencies:
  - task_01
  - task_04
  - task_06
---

# Task 9: DataAvailabilityInjector

## Overview
Inject `<sec sec-type="data-availability">` with a `<title>`, placed after
`<ack>` in `<back>`, using the statement text from the docx. The `@specific-use`
category is classified by a keyword heuristic: auto-applied when confident,
otherwise proposed for inline confirmation.

<critical>
- ALWAYS READ the PRD and TechSpec before starting
- REFERENCE TECHSPEC for implementation details — do not duplicate here
- FOCUS ON "WHAT" — describe what needs to be accomplished, not how
- MINIMIZE CODE — show code only to illustrate current structure or problem areas
- TESTS REQUIRED — every task MUST include tests in deliverables
</critical>

<requirements>
- MUST use the data-availability text from `DocxSource` as the `<p>` body.
- MUST emit the section form `<sec sec-type="data-availability" specific-use="…">`
  with a mandatory `<title>`, placed immediately after `<ack>` in `<back>` (else
  before the first child of `<back>`, else as last child) per the placement rule.
- MUST classify `@specific-use` into one of the five SPS values via a keyword
  heuristic seeded from `data_availability.md`; auto-apply when confident,
  otherwise build a `Proposal` and call `Phase3Context.Confirm`.
- MUST record the chosen `@specific-use` and its disposition (auto vs confirmed)
  in the report.
- MUST be idempotent: if a data-availability `sec` or `fn` already exists, skip
  and report.
</requirements>

## Subtasks
- [x] 9.1 Skip-if-present when a data-availability sec/fn exists.
- [x] 9.2 Classify the statement text into one of five `@specific-use` values with a confidence signal.
- [x] 9.3 Auto-apply when confident; otherwise propose and confirm.
- [x] 9.4 Build the `<sec>` with `<title>` + `<p>` and place it after `<ack>` in `<back>`.
- [x] 9.5 Report the value and disposition.

## Implementation Details
Create `DocFormatter.Core/Jats/DataAvailabilityInjector.cs` and a keyword
classifier (e.g. `DataAvailabilityClassifier.cs`). Placement and the five values
per `docs/scielo_context/jats/data_availability.md`. Uses `DocxSource` (task_01),
the element builder (task_06), and `IConfirmer` via the context (task_04). See
TechSpec injection-rules table and ADR-005.

### Relevant Files
- `DocFormatter.Core/Jats/DataAvailabilityInjector.cs`, `DataAvailabilityClassifier.cs` — new (create).
- `docs/scielo_context/jats/data_availability.md` — five values, placement, keyword corpus.
- `DocFormatter.Core/Jats/DocxSource.cs` — DA text (task_01).

### Dependent Files
- `AddPhase3Injectors()` registration (task_11).
- Golden-corpus tests (task_12).

### Related ADRs
- [ADR-001: Confidence-gated injection](../adrs/adr-001.md) — auto vs prompt gate.
- [ADR-005: Structured/exact-term auto](../adrs/adr-005.md) — confidence philosophy (conservative).

## Deliverables
- `DataAvailabilityInjector` + keyword classifier emitting the `<sec>` after `<ack>`, idempotent.
- Unit tests with 80%+ coverage **(REQUIRED)**
- Integration test on a corpus XML **(REQUIRED)**

## Tests
- Unit tests:
  - [x] "available from the corresponding author upon reasonable request" classifies as `data-available-upon-request` (auto).
  - [x] Text with a repository link/DOI classifies as `data-available` (auto).
  - [x] "no new data were created or analyzed" classifies as `uninformed` (auto).
  - [x] Ambiguous text triggers a `Proposal` and uses the confirmer's result.
  - [x] Built `<sec>` is placed immediately after `<ack>` in `<back>` with a `<title>` and the statement `<p>`.
  - [x] XML already containing a data-availability sec/fn is left unchanged and reported as skipped.
- Integration tests:
  - [x] Running over a corpus XML (with `AutoAcceptConfirmer`) injects the section after `<ack>` with the classified `@specific-use`.
- Test coverage target: >=80%
- All tests must pass

## Success Criteria
- All tests passing
- Test coverage >=80%
- Section placed correctly with a mandatory title; confident texts auto-classified; ambiguous texts surfaced for confirmation; every choice reported.
