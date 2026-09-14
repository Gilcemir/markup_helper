# Task Memory: task_09.md

Keep only task-local execution context here. Do not duplicate facts that are obvious from the repository, task file, PRD documents, or git history.

## Objective Snapshot

- Done 2026-09-14: three staged runs in the session scratchpad; acceptance table met on run3 (package XMLs with the previously injected `<role>` elements stripped). Evidence folder untouched (mtimes checked before/after). One code fix-up shipped (see Decisions). Suite 897 → 901.

## Important Decisions

- Evidence state: 9 of 15 `scielo_package/*.xml` already carry CRediT `<role>` (docformatter URLs) — the original run's outputs were copied back over the inputs after the user's run. `L0X/markup_xml/src/*.xml` are NOT the Phase 3 inputs: raw Markup XML with an undeclared `&nbsp;` entity; the XML loader rejects them. Faithful inputs = package XMLs minus `<role>` elements.
- Fix-up: an idempotency-skipped contributor ("already has <role>") was reported in the summary as `pending X (confirmed)`. Added `CreditEntryOutcome.AlreadyPresent` (Emit/EmitFreeText return `EmitResult{Applied, AlreadyPresent}`), `DiagnosticCreditEntry.AlreadyPresent`, and summary wording `already present A, B; pending …`. Re-run over already-injected XML now reads correctly (run1 re-run).

## Learnings

- Staging layout that pairs: `<copy>/L0X/markup_xml/{other.txt, scielo_package/<package-basename>.xml, scielo_markup/*.docx}`; run `dotnet run --project DocFormatter.Cli --no-build -- phase3 <copy>/L0X/markup_xml/scielo_package --non-interactive=accept`.
- Results (run3): L01 e55012633 ✓ +1 broken surname; e55242632/e55282635/e55472634 ✓; e56132631 applied TVB, QHTP; pending NHN; 1 broken. L02 e53162636 applied PSA, ATB, RRP; pending MAF, JAN, AGS, JSP; 7 broken; e55292638 pending TTR only; e55682637 ✓; e561726310 pending JGS only; e56422639 ✓. L03 e541226312/e544126313/e553626311/e565626314 ✓; e571926315 pending MRC only. No "free prose" in any report.

## Files / Surfaces

- Modified: `DocFormatter.Core/Jats/CreditOutcome.cs`, `CreditRolesInjector.cs` (EmitResult), `Reporting/DiagnosticDocument.cs`, `Reporting/DiagnosticWriter.cs`, `DocFormatter.Cli/CliApp.cs`.
- Tests: `CreditRolesInjectorTests` (+1, `Contrib` helper gained `roles:`), `CliPhase3Tests` (+2), `DiagnosticWriterPhase3Tests` (+1).

## Errors / Corrections

- First run (run1) used the package XMLs as-is and showed `applied none; pending … (confirmed)` for already-injected articles → traced to input state + misleading wording; fixed the wording, not the expectation.
- `src/*.xml` attempt (run2) failed 12/15 on `&nbsp;` — abandoned as input source.

## Ready for Next Run

- task_10: README should show the `already present` variant of the summary block and mention `alreadyPresent` in the diagnostic entry; ADR-005 implementation notes may mention the flag.
