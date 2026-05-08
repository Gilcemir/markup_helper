# Task Memory: task_05.md

Keep only task-local execution context here. Do not duplicate facts that are obvious from the repository, task file, PRD documents, or git history.

## Objective Snapshot

- Add `DiagnosticHistoryMove`/`DiagnosticSectionPromotion` records and extend `DiagnosticFormatting` with two trailing nullable properties; tests for construction, equality, JSON round-trip.

## Important Decisions

- JSON keys for the new properties resolve to `historyMove` / `sectionPromotion` (camelCase) because the existing serializer (`DiagnosticWriter.JsonOptions`) uses `JsonNamingPolicy.CamelCase` for all `formatting.*` keys (`alignmentApplied`, `correspondingEmail`, etc.). The PRD shows snake_case (`history_move`) only as illustrative; the task spec requires "matches the existing convention". Tests assert the camelCase form.
- Null Phase 3 sub-objects are emitted as `"historyMove": null` (not omitted), because `DefaultIgnoreCondition.Never` is the existing convention shared with `alignmentApplied`/`abstractFormatted`. The task spec parenthetical "(matching the existing convention)" overrides the literal "omitted" wording.
- `DiagnosticWriter.BuildFormatting` was updated to pass `HistoryMove: null, SectionPromotion: null` so the existing record positional constructor remains the single point of construction. task_06 will populate these via dedicated `BuildHistoryMove`/`BuildSectionPromotion` helpers — this task does NOT add them.

## Learnings

- The full xUnit suite is 294 tests after this change (up from 270). New file `DocFormatter.Tests/DiagnosticDocumentTests.cs` adds 24 tests (parametrised theories included).

## Files / Surfaces

- Modified: `DocFormatter.Core/Reporting/DiagnosticDocument.cs`
- Modified: `DocFormatter.Core/Reporting/DiagnosticWriter.cs` (positional ctor extension only)
- New: `DocFormatter.Tests/DiagnosticDocumentTests.cs`

## Errors / Corrections

## Ready for Next Run

- task_06 (`DiagnosticWriter` Phase 3 emission) can populate `HistoryMove` / `SectionPromotion` via the records' positional constructors. JSON serialization is already verified end-to-end.
