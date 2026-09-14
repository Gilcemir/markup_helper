# Task Memory: task_05.md

Keep only task-local execution context here. Do not duplicate facts that are obvious from the repository, task file, PRD documents, or git history.

## Objective Snapshot

- Verification-only `contrib-names` injector (WARN per broken `<surname>`), registered before `credit-roles`. Completed 2026-09-14; suite 860 → 872.

## Important Decisions

- ORCID check runs before the digit check, so "3 0000-0003-3513-3391" (5501) is worded as ORCID, not generic digits.
- A `<contrib>` with no `<surname>` element (e.g. `<collab>`) is skipped silently — not a broken byline.
- Local `[GeneratedRegex]` ORCID pattern, unanchored (no `\b`), so a glued ORCID still matches; `FormattingOptions` is not available to Phase 3.
- Message shape: `<contrib> #N has … <surname> '…'; fix the byline in SciELO Markup.` — task_07 can count entries by `Rule == "contrib-names"` at WARN.

## Learnings

- Integration test relies on ADR-004 tier 2: with surname = ORCID and given-names "Gabriel Henrique Iglesias", key "GHI" still resolves via given-only initials, so `credit-roles` auto-applies while `contrib-names` warns — the 5316 mechanism.
- `Phase3PipelineIntegrationTests.RolesOf` looks contribs up by surname text; passing the ORCID string as the "surname" works for the broken contributor.

## Files / Surfaces

- New: `DocFormatter.Core/Jats/ContribNamesInjector.cs`, `DocFormatter.Tests/Jats/ContribNamesInjectorTests.cs` (11 tests).
- Modified: `RuleRegistration.AddPhase3Injectors` (+doc comment), `RuleRegistrationTests` (five injectors), `Phase3PipelineIntegrationTests` (+1 broken-surname pipeline test).

## Errors / Corrections

- None.

## Ready for Next Run

- task_07 needs `BrokenNames` = count of report entries with rule `contrib-names` at WARN; no structured outcome is stored on `Phase3Context` for this rule (by design, ADR-002).
