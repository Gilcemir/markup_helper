# Task Memory: task_10.md

Keep only task-local execution context here. Do not duplicate facts that are obvious from the repository, task file, PRD documents, or git history.

## Objective Snapshot

- Done 2026-09-14: README (Phase 1 ORCID/repeated-token notes; Phase 3 `contrib-names`, CRediT grammar, resolver variants, batch summary block incl. `already present`, `phase3.creditStatement`), CLI usage text, ADR-005 implementation note on `alreadyPresent`. Full gate green (901 tests, phase2-verify 10/10).

## Important Decisions

- README wording for the summary block copies `CliApp.DescribeCreditPendency` literally; the example uses real v26n3 basenames (e56132631, e571926315).
- ADRs 001–004, 006 reviewed against the code: no drift. ADR-005 gained the `alreadyPresent` note. INV-02 stays in ADR-003 as `**INV-02 — …**` (promote-feature greps `INV-[0-9]+`; the promoted INV-01 uses a colon form and was handled).

## Learnings

- `Help_DocumentsPhase3Subcommand` only asserts "phase3" and "--non-interactive", so usage wording can evolve without touching the test.

## Files / Surfaces

- Modified: `README.md`, `DocFormatter.Cli/CliApp.cs` (usage string only), `adrs/adr-005.md`.

## Errors / Corrections

- None.

## Ready for Next Run

- Post-merge: `/promote-feature credit-corpus-v26n3-fixes` (6 ADRs, INV-02) then `make release VERSION=v0.3.1` on `main`.
