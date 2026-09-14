# Workflow Memory

Keep only durable, cross-task context here. Do not duplicate facts that are obvious from the repository, PRD documents, or git history.

## Current State

- task_01 (resolver tiers/variants) completed 2026-09-14; committed on `feat/credit-corpus-v26n3-fixes`.
- task_02 (author-keyed grammar with `;`/`,` and lookback) completed 2026-09-14; suite at 841 tests.
- task_03 (reader `[p]` strip + `DocxSource.CreditHeaderFound`) completed 2026-09-14; suite at 848 tests.
- task_04 (`CreditOutcome` on `Phase3Context`, header-empty WARN, `[p]`+`;` pipeline fixture) completed 2026-09-14; suite at 860 tests.
- task_05 (`contrib-names` verification injector before `credit-roles`) completed 2026-09-14; suite at 872 tests.
- task_06 (`phase3.creditStatement` block in the diagnostic; `WritePhase3` takes `CreditOutcome?`) completed 2026-09-14; suite at 879 tests.
- task_07 (batch summary pendency block; `Phase3Outcome.Credit`/`BrokenNames`) completed 2026-09-14; suite at 890 tests.
- task_08 (Phase 1 plain-text ORCID token + repeated-token WARN) completed 2026-09-14; suite at 897 tests; `make phase2-verify` 10/10 PASS.
- task_09 (manual acceptance over v26n3 copy) completed 2026-09-14; table met on all 15; fix-up `CreditEntryOutcome.AlreadyPresent`; suite at 901 tests.
- task_10 (README, CLI usage, ADR drift) completed 2026-09-14. All 10 tasks done; branch ready for PR → merge → `/promote-feature` → `make release VERSION=v0.3.1`.

## Shared Decisions

- Tracking files (`task_*.md`, `_tasks.md`, `memory/`) are left out of task commits; only source and test files are committed.

## Shared Learnings

- Verification pipeline that works: `dotnet build DocFormatter.sln` (warnings are errors), `dotnet format DocFormatter.sln --verify-no-changes --include <changed files>`, `dotnet test DocFormatter.sln`. Baseline before this feature: 811 tests.
- No coverage collector is installed (no coverlet package, no global tool); the "80% coverage" criterion can only be argued by test enumeration, not measured.

## Open Risks

- The v26n3 evidence `scielo_package/*.xml` now carry roles from the original run (outputs copied back); `src/*.xml` are raw Markup XML with `&nbsp;` and do not parse. Any future re-validation must strip `<role>` from the package XMLs first (see memory/task_09.md).

- Whole-solution `dotnet format --verify-no-changes` fails on `DocFormatter.Tests/Phase2/Phase2PipelineIntegrationTests.cs` (4 WHITESPACE errors, pre-existing on main). Use `--include` on changed files until someone fixes it in scope.

## Handoffs

- All 15 v26n3 statement texts parse `AuthorKeyed` at the parser level (verified by a throwaway sweep in task_02); remaining pendencies are reader (5536, task_03) and resolver/docx issues, not grammar.
- `DocxSource.CreditHeaderFound` is available for task_04's header-empty WARN; the reader tests in `DocxSourceReaderTests` show the `[p]` paragraph shapes to mirror in the pipeline fixture.
- `DiagnosticWriter.WritePhase3(..., recordedDispositions, CreditOutcome? credit)` — the parameter is required; `Phase3Processor` is the only production caller. `CliPhase3Tests` shows a synthetic single-file layout (other.txt + fixture docx + hand-written XML) that avoids the `examples/phase-3` corpus.
- `ctx.Credit` (`CreditOutcome`) is populated after `credit-roles`; disposition/resolution strings are the consts in `CreditDisposition`/`CreditResolution` (`DocFormatter.Core/Jats/CreditOutcome.cs`) — tasks 06/07 should reference them, not literals. `Phase3DocxFixtureBuilder.WriteMarkupDocxWithCreditStatement` writes a pairable Markup docx for pipeline/CLI tests.
- Batch summary pendency wording lives in `CliApp.DescribeCreditPendency`/`Phase3PendencyLines` (internal, unit-tested line-for-line); README (task_10) should quote it, not paraphrase.
- `contrib-names` writes nothing to `Phase3Context`; task_07's `BrokenNames` must be derived from report entries (`Rule == "contrib-names"`, level WARN). Message format: `<contrib> #N has … <surname> '…'; fix the byline in SciELO Markup.`
- Real corpus texts live as `Corpus*` consts in `CreditStatementParserTests.cs`; reuse them for injector/pipeline fixtures (task_04) instead of retyping from the evidence prompt.
