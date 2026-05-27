---
status: completed
title: CLI wiring and DI registration
type: backend
complexity: high
dependencies:
  - task_03
  - task_04
  - task_05
  - task_06
  - task_07
  - task_08
  - task_09
  - task_10
---

# Task 11: CLI wiring and DI registration

## Overview
Wire Phase 3 into the CLI: a `phase3` subcommand with `--non-interactive=accept|fail`,
an `AddPhase3Injectors()` DI registration and service-provider builder, per-document
orchestration (pair → read docx → load XML → run pipeline → save → report), batch
processing, and a Phase 3 diagnostic section. This makes Phase 3 runnable end-to-end.

<critical>
- ALWAYS READ the PRD and TechSpec before starting
- REFERENCE TECHSPEC for implementation details — do not duplicate here
- FOCUS ON "WHAT" — describe what needs to be accomplished, not how
- MINIMIZE CODE — show code only to illustrate current structure or problem areas
- TESTS REQUIRED — every task MUST include tests in deliverables
</critical>

<requirements>
- MUST add a `phase3 <file.xml | folder>` subcommand in `CliApp.Run`, following
  the existing `phase2` dispatch and disambiguation rules.
- MUST parse `--non-interactive=accept|fail` and select `AutoAcceptConfirmer` /
  `FailOnPromptConfirmer`; default to `ConsoleConfirmer`.
- MUST add `AddPhase3Injectors()` registering the four injectors in order plus a
  Phase 3 service-provider builder, mirroring `AddPhase2Rules()`.
- MUST orchestrate per document: pair (task_03), read docx (task_01), load XML
  (task_06), build `Phase3Context`, run `Phase3Pipeline` (task_04), save XML
  (task_06), write `.report.txt` and `.diagnostic.json`.
- MUST write outputs under a phase-3 output directory consistent with phase 1/2
  conventions, and emit a batch summary (counts: processed, prompted, skipped,
  failed).
- MUST extend `DiagnosticWriter`/`DiagnosticDocument` with a Phase 3 section
  (four tags, values, dispositions).
- MUST return the established exit codes; a prompt under `--non-interactive=fail`
  yields a non-zero exit.
</requirements>

## Subtasks
- [x] 11.1 Add the `phase3` subcommand and `--non-interactive` parsing in `CliApp`.
- [x] 11.2 Implement `AddPhase3Injectors()` and the Phase 3 service provider.
- [x] 11.3 Implement per-document orchestration (pair → read → load → run → save → report).
- [x] 11.4 Implement batch processing and the batch summary.
- [x] 11.5 Extend the diagnostic writer with a Phase 3 section.
- [x] 11.6 Map exit codes, including fail-on-prompt.

## Implementation Details
Modify `DocFormatter.Cli/CliApp.cs` (subcommand dispatch + provider builders) and
`DocFormatter.Core/Pipeline/RuleRegistration.cs` (add `AddPhase3Injectors`).
Extend `DocFormatter.Core/Reporting/DiagnosticDocument.cs` and `DiagnosticWriter.cs`.
Follow the phase2 patterns in `CliApp` and `FileProcessor`. See TechSpec
"Component Overview", "API Endpoints", and "Monitoring and Observability".

### Relevant Files
- `DocFormatter.Cli/CliApp.cs` — subcommand dispatch, provider builders, flag parsing (modify).
- `DocFormatter.Cli/FileProcessor.cs` — per-file orchestration reference (model phase3 orchestration on it).
- `DocFormatter.Core/Pipeline/RuleRegistration.cs` — add `AddPhase3Injectors()` (modify).
- `DocFormatter.Core/Reporting/DiagnosticWriter.cs`, `DiagnosticDocument.cs` — add Phase 3 section (modify).
- All `DocFormatter.Core/Jats/*` — components being wired.

### Dependent Files
- `DocFormatter.Tests` golden-corpus tests invoke `phase3` via `CliApp.Run` (task_12).

### Related ADRs
- [ADR-003: Dedicated IJatsInjector pipeline](../adrs/adr-003.md) — subcommand + DI mirror phase 1/2.
- [ADR-006: Confirmer with non-interactive policy](../adrs/adr-006.md) — `--non-interactive` flag.
- [ADR-004: Pair on elocation-id, verify with DOI](../adrs/adr-004.md) — per-document pairing in orchestration.

## Deliverables
- `phase3` subcommand, `--non-interactive` flag, `AddPhase3Injectors()`, orchestration, batch summary, Phase 3 diagnostics.
- Unit tests with 80%+ coverage **(REQUIRED)**
- Integration tests exercising the CLI end-to-end **(REQUIRED)**

## Tests
- Unit tests:
  - [x] `phase3` dispatch resolves a single XML file and a folder of XML files.
  - [x] `--non-interactive=accept` selects `AutoAcceptConfirmer`; `=fail` selects `FailOnPromptConfirmer`; absent selects `ConsoleConfirmer`.
  - [x] `AddPhase3Injectors()` registers the four injectors in OtherId→EditedBy→DataAvailability→CreditRoles order.
  - [x] Diagnostic document includes the Phase 3 section with tag values and dispositions.
  - [x] A document that would prompt under `--non-interactive=fail` produces a non-zero exit code.
- Integration tests:
  - [x] `CliApp.Run(["phase3", corpusXml, "--non-interactive=accept"])` produces a modified XML, `.report.txt`, and `.diagnostic.json`.
  - [x] Batch run over the corpus folder emits a batch summary with processed/prompted/skipped/failed counts.
- Test coverage target: >=80%
- All tests must pass

## Success Criteria
- All tests passing
- Test coverage >=80%
- Phase 3 runs end-to-end for single-file and batch; outputs and exit codes consistent with phase 1/2; diagnostics include Phase 3 data.
