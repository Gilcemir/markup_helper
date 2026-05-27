---
status: completed
title: Phase 3 contracts and pipeline
type: backend
complexity: medium
dependencies: []
---

# Task 4: Phase 3 contracts and pipeline

## Overview
Define the Phase 3 abstraction layer: the `IJatsInjector` interface,
`Phase3Context`, the `IConfirmer` interface, the `Proposal`/`ConfirmResult`
records, and a `Phase3Pipeline` runner. This is the contract skeleton every
injector and the CLI depend on, typed correctly for docx→XML injection (ADR-003).

<critical>
- ALWAYS READ the PRD and TechSpec before starting
- REFERENCE TECHSPEC for implementation details — do not duplicate here
- FOCUS ON "WHAT" — describe what needs to be accomplished, not how
- MINIMIZE CODE — show code only to illustrate current structure or problem areas
- TESTS REQUIRED — every task MUST include tests in deliverables
</critical>

<requirements>
- MUST define `IJatsInjector` with a `Name`, a `Severity` (reuse the existing
  `RuleSeverity` enum), and `Apply(Phase3Context, IReport)` — see TechSpec
  "Core Interfaces".
- MUST define `Phase3Context` holding the parsed `DocxSource`, the target
  `XDocument`, the nullable `OtherNumber`, and an `IConfirmer`.
- MUST define `IConfirmer.Confirm(Proposal) → ConfirmResult` and the
  `Proposal`/`ConfirmResult` records plus the `ConfirmDisposition` enum
  (AutoApplied / Confirmed / Overridden / Skipped).
- MUST define `Phase3Pipeline` that runs registered injectors in order, mirroring
  `FormattingPipeline`'s error model: optional injectors log-and-continue,
  critical injectors rethrow, `OperationCanceledException` always rethrows.
- MUST reuse the existing `IReport` for reporting; MUST NOT introduce a new
  report type.
</requirements>

## Subtasks
- [x] 4.1 Define `IJatsInjector` and `Phase3Context`.
- [x] 4.2 Define `IConfirmer`, `Proposal`, `ConfirmResult`, `ConfirmDisposition`.
- [x] 4.3 Implement `Phase3Pipeline` ordered runner with the phase 1/2 error model.
- [x] 4.4 Wire reporting through the existing `IReport`.

## Implementation Details
Create under `DocFormatter.Core/Jats/`. Model `Phase3Pipeline` on
`DocFormatter.Core/Pipeline/FormattingPipeline.cs` (catch/log/rethrow-on-critical).
See TechSpec "Core Interfaces" for the exact signatures — reference, do not
duplicate. `RuleSeverity` lives in `DocFormatter.Core/Pipeline/RuleSeverity.cs`.

### Relevant Files
- `DocFormatter.Core/Jats/IJatsInjector.cs`, `Phase3Context.cs`, `IConfirmer.cs`, `Proposal.cs`, `Phase3Pipeline.cs` — new (create).
- `DocFormatter.Core/Pipeline/FormattingPipeline.cs` — error-model template.
- `DocFormatter.Core/Pipeline/RuleSeverity.cs` — reused severity enum.
- `DocFormatter.Core/Pipeline/IReport.cs` — reused report contract.

### Dependent Files
- All four injectors (task_07–task_10) implement `IJatsInjector`.
- `IConfirmer` implementations (task_05) implement `IConfirmer`.
- CLI wiring (task_11) constructs `Phase3Context` and runs `Phase3Pipeline`.

### Related ADRs
- [ADR-003: Dedicated IJatsInjector pipeline](../adrs/adr-003.md) — why a parallel pipeline.
- [ADR-001: Confidence-gated injection](../adrs/adr-001.md) — the confirmer's purpose.
- [ADR-006: Confirmer with non-interactive policy](../adrs/adr-006.md) — `IConfirmer` contract.

## Deliverables
- `IJatsInjector`, `Phase3Context`, `IConfirmer`, `Proposal`/`ConfirmResult`/`ConfirmDisposition`, `Phase3Pipeline`.
- Unit tests with 80%+ coverage **(REQUIRED)**
- Integration tests exercising the pipeline with stub injectors **(REQUIRED)**

## Tests
- Unit tests:
  - [x] Pipeline runs injectors in registration order (assert call sequence with stubs).
  - [x] An optional injector that throws is logged and the pipeline continues.
  - [x] A critical injector that throws aborts the pipeline (exception rethrown).
  - [x] `OperationCanceledException` from any injector is rethrown regardless of severity.
  - [x] `ConfirmResult` carries the final value and disposition.
- Integration tests:
  - [x] A two-stub pipeline over a `Phase3Context` produces ordered report entries.
- Test coverage target: >=80%
- All tests must pass

## Success Criteria
- All tests passing
- Test coverage >=80%
- Contracts compile and are consumed unchanged by injectors and CLI; pipeline error model matches phase 1/2.
