---
status: completed
title: FormattingPipeline orchestrator with severity model
type: backend
complexity: medium
dependencies:
    - task_02
---

# Task 4: FormattingPipeline orchestrator with severity model

## Overview
Implement the `FormattingPipeline` class that runs an ordered list of `IFormattingRule` instances against a `WordprocessingDocument`, populating a shared `FormattingContext` and `IReport`. The orchestrator enforces the severity contract: `Critical` exceptions abort the pipeline; `Optional` exceptions are converted to `[ERROR]` entries and execution continues.

<critical>
- ALWAYS READ the PRD and TechSpec before starting
- REFERENCE TECHSPEC for implementation details — do not duplicate here
- FOCUS ON "WHAT" — describe what needs to be accomplished, not how
- MINIMIZE CODE — show code only to illustrate current structure or problem areas
- TESTS REQUIRED — every task MUST include tests in deliverables
</critical>

<requirements>
- 1. `FormattingPipeline` MUST accept `IEnumerable<IFormattingRule>` via constructor (DI-friendly).
- 2. The pipeline MUST run rules in the registration order; order is fixed by registration, not by attribute or sorting.
- 3. When a `Critical` rule throws, the pipeline MUST log `[ERROR]` to the report and rethrow so the caller aborts the file.
- 4. When an `Optional` rule throws, the pipeline MUST log `[ERROR]` to the report with the exception message and continue with the next rule.
- 5. The pipeline MUST NOT swallow `OperationCanceledException` regardless of severity.
- 6. The pipeline MUST NOT mutate the document or context except through rule invocations.
- 7. The pipeline MUST be stateless across runs; calling `Run` twice on the same instance with different documents yields independent outcomes.
</requirements>

## Subtasks
- [x] 4.1 Create `DocFormatter.Core/Pipeline/FormattingPipeline.cs` with the constructor and `Run(WordprocessingDocument, FormattingContext, IReport)` method.
- [x] 4.2 Implement the try/catch loop matching the spec's severity behavior (TechSpec "Implementation Design — Severity rationale").
- [x] 4.3 Re-raise `OperationCanceledException` unconditionally.
- [x] 4.4 Add xUnit tests covering Critical-abort, Optional-continue, and registration order.

## Implementation Details
File is new under `DocFormatter.Core/Pipeline/`. See TechSpec "Core Interfaces" for the contract and "Implementation Design — Rules" for the severity behavior table. Tests use stub rules (`SealedClass : IFormattingRule`) implemented inside the test project; do not introduce a public test-double type into Core.

### Relevant Files
- `.compozy/tasks/header-metadata-extraction/_techspec.md` — "Core Interfaces" and "Rules" sections
- `instructions.md` — original spec's "Modelo de severidade" section

### Dependent Files
- `DocFormatter.Core/Pipeline/FormattingPipeline.cs` (new)
- `DocFormatter.Tests/FormattingPipelineTests.cs` (new)

### Related ADRs
- [ADR-001: Esqueleto alinhado ao spec com 4 regras](adrs/adr-001.md) — locks the orchestration shape

## Deliverables
- `FormattingPipeline` class
- xUnit tests covering severity semantics and registration order
- Unit tests with 80%+ coverage **(REQUIRED)**
- Integration tests for [pipeline + minimal rule sequence] **(REQUIRED)**

## Tests
- Unit tests:
  - [x] Two rules in order R1 (Optional) and R2 (Critical): both run when neither throws; `Report.Entries` contains exactly the [INFO] entries those rules emit, in order.
  - [x] R1 (Optional) throws `InvalidOperationException("boom")`: `Run` does NOT throw, `Report` contains an `[ERROR]` entry from R1 with message "boom", and R2 still executes.
  - [x] R1 (Critical) throws `InvalidOperationException("fatal")`: `Run` rethrows, `Report` contains an `[ERROR]` entry from R1 before the rethrow, and R2 does NOT execute.
  - [x] R1 throws `OperationCanceledException`: regardless of severity, `Run` rethrows the cancellation immediately and no further rules execute.
  - [x] Calling `Run` twice with two different `FormattingContext` instances yields independent contexts (no leakage between runs).
- Integration tests:
  - [x] `FormattingPipeline` registered with three stub rules in DI runs them in DI registration order, verified by sequential `Report.Info` entries.
- Test coverage target: >=80%
- All tests must pass

## Success Criteria
- All tests passing
- Test coverage >=80%
- Pipeline uses `IEnumerable<IFormattingRule>` constructor injection
- Severity behavior matches TechSpec "Rules" table
