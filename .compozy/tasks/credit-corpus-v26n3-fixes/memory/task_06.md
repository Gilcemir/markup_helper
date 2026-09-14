# Task Memory: task_06.md

Keep only task-local execution context here. Do not duplicate facts that are obvious from the repository, task file, PRD documents, or git history.

## Objective Snapshot

- Done 2026-09-14: `DiagnosticCreditStatement`/`DiagnosticCreditEntry` records, nullable `DiagnosticPhase3.CreditStatement`, `CreditOutcome?` parameter on both `WritePhase3` overloads and `BuildPhase3Document`, `Phase3Processor` passes `ctx.Credit`. Suite 872 → 879.

## Important Decisions

- `credit` is a required parameter (not optional) so every caller states what it passes; the only production caller is `Phase3Processor`.
- `Shape` is serialized as the camelCase `CreditShape` name via the existing `ToCamelCase` helper (`authorKeyed`/`roleKeyed`/`prose`), consistent with the disposition vocabulary; kept as `string` per TechSpec.
- The block mirrors `CreditOutcome` one-to-one (no reparse); `Disposition` is not repeated in the block because `phase3.creditRoles.disposition` already carries the document disposition.
- New records override `Equals`/`GetHashCode` with `SequenceEqual` like the other list-bearing diagnostic records, so record equality and the JSON round-trip test work.

## Learnings

- A positional record parameter with a default (`DiagnosticCreditStatement? CreditStatement = null`) keeps existing `DiagnosticPhase3` constructions compiling and round-trips through System.Text.Json.
- A CLI end-to-end test needs only: `other.txt` (`<xml-basename>.pdf\t<other>`), a docx from `Phase3DocxFixtureBuilder.WriteMarkupDocxWithCreditStatement`, and an XML whose `elocation-id` is `e56132631` and DOI is `MarkupDoi` (the fixture's `[doc]` header keys). An unresolved key under `--non-interactive=accept` yields the WARN that opens the diagnostic gate.

## Files / Surfaces

- Modified: `DocFormatter.Core/Reporting/DiagnosticDocument.cs`, `DocFormatter.Core/Reporting/DiagnosticWriter.cs`, `DocFormatter.Cli/Phase3Processor.cs`.
- Tests: `DiagnosticWriterPhase3Tests` (+5, existing calls gained the `null` argument), `DiagnosticDocumentTests` (+1 round-trip), `CliPhase3Tests` (+1 synthetic end-to-end with an unresolved author).

## Errors / Corrections

- None.

## Ready for Next Run

- task_07 reads `ctx.Credit` and the report in `Phase3Processor.Process` (both in scope where `Phase3Outcome` is built); the diagnostic path needs nothing more.
