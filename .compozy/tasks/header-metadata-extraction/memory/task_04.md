# Task Memory: task_04.md

Keep only task-local execution context here. Do not duplicate facts that are obvious from the repository, task file, PRD documents, or git history.

## Objective Snapshot

Ship `FormattingPipeline` (DI-friendly) honoring the severity contract: log `[ERROR]` and continue for Optional, log `[ERROR]` and rethrow for Critical, never swallow `OperationCanceledException`.

## Important Decisions

- Constructor materializes `IEnumerable<IFormattingRule>` to `IFormattingRule[]` (`ToArray`) so the pipeline can be replayed with deferred sequences without re-enumerating side-effecting `yield` blocks.
- Order of catch clauses matters: typed `catch (OperationCanceledException) { throw; }` precedes the general `catch (Exception ex)` so OCE is never logged or swallowed regardless of severity.
- Pipeline does not log a report entry for `OperationCanceledException` — task spec only requires non-swallowing; a cancellation is not an error.
- No null-arg validation added: parameters are non-nullable in the API and callers are internal.

## Learnings

- Stub rule pattern (private sealed nested class implementing `IFormattingRule`) keeps the test surface narrow without exporting a test-double from Core.
- Empty `WordprocessingDocument` from `MemoryStream` is enough — pipeline never inspects the doc, so the test just needs a valid instance to satisfy the non-null parameter.

## Files / Surfaces

- `DocFormatter.Core/Pipeline/FormattingPipeline.cs` (new)
- `DocFormatter.Tests/FormattingPipelineTests.cs` (new — 7 test cases incl. Theory over OCE × severity)

## Errors / Corrections

- None.

## Ready for Next Run

- task_05 (`ExtractTopTableRule`) can register as the first `IFormattingRule` in DI; pipeline shape is stable.
- Pipeline constructor signature is `FormattingPipeline(IEnumerable<IFormattingRule>)` — wire rules with `services.AddSingleton<IFormattingRule, TConcrete>()` calls, in execution order.
