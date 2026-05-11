---
status: completed
title: '`HistDateParser` (TDD) — phrase-inventory parser → `HistDate` records → `dateiso` `YYYYMMDD`'
type: backend
complexity: medium
dependencies: []
---

# Task 08: `HistDateParser` (TDD) — phrase-inventory parser → `HistDate` records → `dateiso` `YYYYMMDD`

## Overview
Phase 4's `[hist]` block needs accurate dates extracted from natural-language phrases like "Received on 12 March 2024", "Accepted: 2024-04-15", "Published 2024". This task delivers `HistDateParser` from scratch in DocFormatter conventions (per ADR-007), built TDD: write the phrase-inventory tests first (derived from `Marcador_de_referencia/BibliographyHandlers/AccessedOnHandler.cs` as algorithmic reference), then implement. The parser is independent of every other Phase 2 task and can be picked up in parallel.

<critical>
- ALWAYS READ the PRD and TechSpec before starting
- REFERENCE TECHSPEC for implementation details — do not duplicate here
- FOCUS ON "WHAT" — describe what needs to be accomplished, not how
- MINIMIZE CODE — show code only to illustrate current structure or problem areas
- TESTS REQUIRED — every task MUST include tests in deliverables
</critical>

<requirements>
- MUST expose the public API from TechSpec "Core Interfaces → HistDateParser":
  - `HistDate? ParseReceived(string text)`
  - `HistDate? ParseAccepted(string text)`
  - `HistDate? ParsePublished(string text)`
  - `record HistDate(int Year, int? Month, int? Day, string SourceText)` with `string ToDateIso()`.
- `ToDateIso()` MUST return `YYYYMMDD` zero-padded; when `Month` is null emit `00` for month; when `Day` is null emit `00` for day (per `docs/scielo_context/README.md` invariant 5).
- MUST recognize at least the phrase shapes catalogued from `Marcador_de_referencia/BibliographyHandlers/AccessedOnHandler.cs`: English month names (full and abbreviated), ISO `YYYY-MM-DD`, year-only, mixed forms.
- MUST be implemented from scratch (not copied) per ADR-007. The original C# file is read for inventory only — no verbatim code.
- MUST live under `DocFormatter.Core/Rules/Phase2/HistDateParsing/HistDateParser.cs` as a `static class`.
- MUST follow strict TDD: every recognized phrase shape gets its xUnit test FIRST; the test fails initially; then the parser is implemented to make it pass. Commit history (or PR review) MUST reflect this — review will look for the test-first ordering.
- MUST return `null` (not throw) on unrecognized input. Callers (the Phase 4 emitter rule) skip-and-warn per ADR-002.
- MUST NOT depend on any other Phase 2 component (TagEmitter, FormattingContext, IReport). It is a pure text-in / record-out function.
</requirements>

## Subtasks
- [x] 8.1 Read `Marcador_de_referencia/BibliographyHandlers/AccessedOnHandler.cs` (via the GitHub URL) and extract a phrase inventory: every distinct date-phrase shape it recognizes, with one example per shape.
- [x] 8.2 Translate the inventory into a markdown table inside ADR-007 (or a sibling notes file under `adrs/`) for traceability.
- [x] 8.3 Write the `HistDateParser` xUnit test class with one test per phrase shape — all initially failing.
- [x] 8.4 Implement `HistDate` record with `ToDateIso()` (zero-padding logic).
- [x] 8.5 Implement `HistDateParser` static methods to make the tests pass, one phrase shape at a time.
- [x] 8.6 Add tests for `ToDateIso()` covering full date / month-missing / day-missing / both-missing.
- [x] 8.7 Add tests for the three entry points (`ParseReceived`, `ParseAccepted`, `ParsePublished`) covering header-text variation (e.g., "Received:" vs "Received on" vs "Recebido em").

## Implementation Details
The parser lives at `DocFormatter.Core/Rules/Phase2/HistDateParsing/HistDateParser.cs`. It does NOT depend on `IReport` or any context — pure functional. The three entry points share an internal "find the date span after this prefix" helper. See TechSpec "Core Interfaces → HistDateParser" for the exact public surface and "Known Risks → HistDateParser rewrite drifts" for the TDD-mitigation rationale.

ADR-007 explicitly chose rewrite-from-scratch over file-copy. Reading the source file remotely (via the GitHub URL in the PRD) is allowed; pasting code from it into this repo is not.

### Relevant Files
- `docs/scielo_context/README.md` — invariant 5 (`dateiso` `YYYYMMDD` zero-padding rule).
- `Marcador_de_referencia/BibliographyHandlers/AccessedOnHandler.cs` — algorithmic reference (read-only, via URL).
- `examples/phase-2/before/*.docx` and `examples/phase-2/after/*.docx` — corpus that includes received/accepted/published phrases (informs the phrase inventory).
- `DocFormatter.Core/Rules/MoveHistoryRule.cs` — Phase 1 rule that already locates the history paragraph; reference for paragraph-location heuristic (the parser itself is text-only, but the inventory lives in real fixture text).

### Dependent Files
- `DocFormatter.Tests/Phase2/HistDateParserTests.cs` — new test file (written FIRST per TDD).
- Task 09 — `EmitHistTagRule` calls this parser per candidate paragraph.

### Related ADRs
- [ADR-007: Phase 4 Date-Parser Port — Rewrite from Scratch](adrs/adr-007.md) — Codifies the rewrite-not-copy rule.
- [ADR-001: Rollout Strategy](adrs/adr-001.md) — Phase 4 is last; this task may be picked up in parallel with Phase 2 / Phase 3 work.

## Deliverables
- New file `DocFormatter.Core/Rules/Phase2/HistDateParsing/HistDateParser.cs` with the public API.
- New `HistDate` record (collocated or under `Models/Phase2/`).
- Phrase-inventory documentation (markdown table inside ADR-007 or a sibling notes file).
- New test file `DocFormatter.Tests/Phase2/HistDateParserTests.cs` written before the parser implementation.
- Unit tests with 80%+ coverage **(REQUIRED)**.
- Property-style tests over generated date strings to catch round-trip regressions **(REQUIRED — counts as integration for this pure-function module)**.

## Tests
- Unit tests:
  - [x] `ParseReceived("Received on 12 March 2024")` → `HistDate(2024, 3, 12, "12 March 2024")`.
  - [x] `ParseReceived("Received: 2024-04-15")` → `HistDate(2024, 4, 15, "2024-04-15")`.
  - [x] `ParseAccepted("Accepted: 2024-04-15")` → `HistDate(2024, 4, 15, "2024-04-15")`.
  - [x] `ParsePublished("Published 2024")` → `HistDate(2024, null, null, "2024")`.
  - [x] `ParseReceived("Recebido em 12 de março de 2024")` → either recognized (target) or returns null with no exception.
  - [x] Each abbreviated English month ("Jan", "Feb", …, "Dec") parses correctly when full date is supplied.
  - [x] Each full English month name ("January", …, "December") parses correctly.
  - [x] Two-digit day with leading zero ("on 05 March 2024") parses with `Day=5`.
  - [x] Year-only input (`"2024"`) returns `HistDate(2024, null, null, …)`.
  - [x] `ToDateIso()` for `(2024, 3, 12)` returns `"20240312"`.
  - [x] `ToDateIso()` for `(2024, 3, null)` returns `"20240300"`.
  - [x] `ToDateIso()` for `(2024, null, null)` returns `"20240000"`.
  - [x] `ToDateIso()` for `(2024, 12, 5)` returns `"20241205"` (single-digit day padded).
  - [x] Unrecognized input (`"Hello world"`) returns `null` from each of the three entry points.
  - [x] Empty / whitespace input returns `null` from each entry point.
- Integration tests:
  - [x] Round-trip: parse each known phrase from the inventory; assert `ToDateIso()` matches a hand-computed expected value.
  - [x] All `received` / `accepted` / `published` phrases extracted from `examples/phase-2/after/*.docx` parse to a non-null `HistDate` (sentinel test that fails when the inventory drifts from the corpus).
- Test coverage target: >=80%
- All tests must pass

## Success Criteria
- All tests passing
- Test coverage >=80%
- Phrase inventory documented in ADR-007 (or sibling notes) before parser implementation.
- Tests written before implementation (verifiable in commit history / PR diff).
- Zero verbatim copy from `Marcador_de_referencia` (verified by inspection during code review).
- Every `received` / `accepted` / `published` phrase present in the `after/` corpus parses successfully.
