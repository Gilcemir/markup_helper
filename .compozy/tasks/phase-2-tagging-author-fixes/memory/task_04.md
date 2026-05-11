# Task Memory: task_04.md

Keep only task-local execution context here. Do not duplicate facts that are obvious from the repository, task file, PRD documents, or git history.

## Objective Snapshot

Extract the 11 inline `AddTransient<IFormattingRule, …>` calls from `CliApp.BuildServiceProvider` into `IServiceCollection` extension methods `AddPhase1Rules` and `AddPhase2Rules` under `DocFormatter.Core/Pipeline/RuleRegistration.cs`. Phase 1 default behavior must remain byte-equivalent.

## Important Decisions

- `AddPhase2Rules` is intentionally an empty body (only a comment marker). Tasks 06/07/09 will add the emitter rules.
- Both methods guard `ArgumentNullException.ThrowIfNull(services)` to keep the public extension surface defensive at the boundary, and tests assert this.
- `FormattingPipeline` and `IReport` registrations stay in `CliApp.BuildServiceProvider` rather than moving into the Phase 1/2 extension methods, because they are pipeline infrastructure rather than rule-set selection. This keeps `AddPhaseXRules` semantics narrow.

## Learnings

- Phase 1 rules depend on `FormattingOptions` via constructor injection. Unit tests that *resolve* rules via `IServiceProvider` must register `FormattingOptions`; tests that only inspect descriptors do not.
- The output `.docx` is bit-deterministic across runs (SHA-256 stable), but `diagnostic.json` carries `extractedAt: DateTime.UtcNow`, so JSON byte-equivalence cannot be asserted without normalizing that field.

## Files / Surfaces

- New: `DocFormatter.Core/Pipeline/RuleRegistration.cs` (extension class).
- New: `DocFormatter.Tests/RuleRegistrationTests.cs` (8 tests).
- Modified: `DocFormatter.Cli/CliApp.cs` (`BuildServiceProvider` now delegates to `AddPhase1Rules`; removed `using DocFormatter.Core.Rules`).

## Errors / Corrections

- First test run failed with `Unable to resolve service for type 'FormattingOptions'`. Fix: register `FormattingOptions` in the two unit tests that actually resolve `IFormattingRule` instances.
- Mid-task `git stash --keep-index` was used to reach a clean tree for snapshotting; immediately popped with `git stash pop` to restore working changes.

## Ready for Next Run

- Task 05 (CLI dispatcher) is the next consumer: it should call `AddPhase1Rules()` for the default subcommand and `AddPhase2Rules()` for `phase2` / `phase2-verify`.
- Tasks 06/07/09 extend `AddPhase2Rules` by adding their `EmitXxxTagRule` registrations in the order they appear in the techspec data-flow diagram.
