---
status: completed
title: CreditOutcome on Phase3Context recorded by CreditRolesInjector, header-empty WARN, integration fixture
type: backend
complexity: medium
dependencies:
  - task_02
  - task_03
---

# Task 4: CreditOutcome on Phase3Context recorded by CreditRolesInjector, header-empty WARN, integration fixture

## Overview
Give the CRediT injector a single structured outcome (`CreditOutcome`) written to `Phase3Context` on every exit path, so the diagnostic (task_06) and the batch summary (task_07) read one source of truth. Also add the WARN for a CREDIT header with an empty body, and extend the pipeline integration fixture with a `;`-separated statement inside a `[p]` paragraph.

<critical>
- ALWAYS READ the PRD and TechSpec before starting
- REFERENCE TECHSPEC for implementation details — do not duplicate here
- FOCUS ON "WHAT" — describe what needs to be accomplished, not how
- MINIMIZE CODE — show code only to illustrate current structure or problem areas
- TESTS REQUIRED — every task MUST include tests in deliverables
</critical>

<requirements>
- MUST add the `CreditOutcome` and `CreditEntryOutcome` records and a settable `Phase3Context.Credit` property as defined in TechSpec "Core Interfaces".
- MUST set `ctx.Credit` on every exit of `CreditRolesInjector.Apply`: absent, headerEmpty, prose, autoApplied, confirmed, skipped, freeText.
- MUST emit `report.Warn(Name, …)` when `CreditStatementRaw` is blank and `Source.CreditHeaderFound` is true; keep the INFO line only when the header is absent (INV-02).
- MUST fill each `CreditEntryOutcome` with the written terms, the unknown terms, the resolution status (`resolved`/`notFound`/`ambiguous`) and whether roles were applied.
- MUST NOT change any injected XML shape or the confirmation prompts.
- MUST extend `Phase3PipelineIntegrationTests` with a docx whose CREDIT paragraph is `[p]`-tagged and uses `;` between terms and between entries, asserting roles injected and no WARN.
</requirements>

## Subtasks
- [x] 4.1 Add `CreditOutcome`/`CreditEntryOutcome` (new file in `DocFormatter.Core/Jats/`) and `Phase3Context.Credit`.
- [x] 4.2 Record the outcome in `CreditRolesInjector.Apply` on every path, deriving entry statuses from `BuildPlan`'s `PlanItem`s.
- [x] 4.3 Add the header-empty WARN branch before the prose check.
- [x] 4.4 Extend `CreditRolesInjectorTests` for the outcome on each disposition and for the header-empty WARN.
- [x] 4.5 Extend the Phase 3 integration fixture and test with the `[p]` + `;` statement.

## Implementation Details
Modify `DocFormatter.Core/Jats/CreditRolesInjector.cs` (`Apply`, `BuildPlan` output reuse, `PlanItem` already carries `AuthorKey`, `Contrib`, `Roles`, `WrittenTerms`, `IsClean`), `DocFormatter.Core/Jats/Phase3Context.cs`, new `DocFormatter.Core/Jats/CreditOutcome.cs`. Fixture builders live in `DocFormatter.Tests/Fixtures/Phase3/Phase3DocxFixtureBuilder.cs` (`BuildSectionParagraph`, `WritePhase123HappyPathDocx`). See TechSpec "Core Interfaces" and "Reader" for the header-empty behavior.

### Relevant Files
- `DocFormatter.Core/Jats/CreditRolesInjector.cs` — every exit path sets the outcome.
- `DocFormatter.Core/Jats/Phase3Context.cs` — gains `Credit`.
- `DocFormatter.Core/Jats/CreditStatementParser.cs` — `CreditShape` reused in the outcome.
- `DocFormatter.Tests/Jats/CreditRolesInjectorTests.cs` — `Contrib(...)`/`ArticleWithContribs(...)` helpers.
- `DocFormatter.Tests/Jats/Phase3PipelineIntegrationTests.cs`, `DocFormatter.Tests/Fixtures/Phase3/Phase3DocxFixtureBuilder.cs` — integration fixture.

### Dependent Files
- `DocFormatter.Core/Reporting/DiagnosticWriter.cs` — consumes `CreditOutcome` in task_06.
- `DocFormatter.Cli/Phase3Processor.cs`, `DocFormatter.Cli/CliApp.cs` — consume `ctx.Credit` in tasks 06/07.
- `DocFormatter.Tests/Jats/Phase3PipelineTests.cs` — constructs `Phase3Context`; unaffected by an optional settable property.

### Related ADRs
- [ADR-005: The CRediT outcome is recorded on Phase3Context and read by the diagnostic and the batch summary](../adrs/adr-005.md) — the record and its single writer.
- [ADR-003](../adrs/adr-003.md) — INV-02 (header-empty WARN).

## Deliverables
- `CreditOutcome` records and `Phase3Context.Credit`.
- Header-empty WARN in `credit-roles`.
- Integration fixture with `[p]` + `;` statement.
- Unit tests with 80%+ coverage **(REQUIRED)** — 11 new unit tests (no coverage collector installed; argued by enumeration of every `Apply` exit path)
- Integration tests for the pipeline fixture **(REQUIRED)**

## Tests
- Unit tests:
  - [x] Clean statement, all authors resolve → `Credit.Disposition == "autoApplied"`, every entry `Applied == true`, `Resolution == "resolved"`.
  - [x] One author NotFound with auto-accept confirmer → `Disposition == "confirmed"`, that entry `Resolution == "notFound"`, `Applied == false`, others applied.
  - [x] Confirmer returns Skipped → `Disposition == "skipped"`, no entry applied, entries still listed with statuses.
  - [x] Unknown term "Metodology" → entry `UnknownTerms == ["Metodology"]`.
  - [x] Prose statement → `Shape == Prose`, `Entries` empty, `Disposition == "prose"`, `Raw` carries the text.
  - [x] `CreditStatementRaw` null and `CreditHeaderFound` true → one WARN containing "body is empty", `Disposition == "headerEmpty"`, no INFO "No CREDIT statement".
  - [x] `CreditHeaderFound` false → existing INFO, `Disposition == "absent"`.
- Integration tests:
  - [x] Pipeline over a docx with `[p]ABC; DEF: Conceptualization; Methodology. GHI: Software.[/p]` and matching contribs → `<role>` injected for all three, report has no WARN.
- Test coverage target: >=80%
- All tests must pass

## Success Criteria
- All tests passing
- Test coverage >=80%
- `ctx.Credit` is non-null after every `Apply` invocation in the test suite.
