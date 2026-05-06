# Task Memory: task_03.md

Keep only task-local execution context here. Do not duplicate facts that are obvious from the repository, task file, PRD documents, or git history.

## Objective Snapshot

- Implement `Author` value semantics (extending the task_02 stub) and ship `FormattingOptions` with the five hardcoded constants from the techspec.

## Important Decisions

- `Author` retains the techspec positional signature but ships custom `Equals`/`GetHashCode` (sequence equality on `AffiliationLabels` via `StringComparer.Ordinal`) and overrides `PrintMembers` to drop the `OrcidId` segment when null. This is required by the task's two `Author` test cases — default record equality is reference-equal on `IReadOnlyList<string>` and the auto-generated `ToString` always emits `OrcidId =`.
- `FormattingOptions` exposes the regexes as `Regex` instances built once via `[GeneratedRegex]` on private static partial methods, surfaced through `{ get; }` auto-properties initialized in their declarators (immutable, no per-call allocation). `RegexOptions.IgnoreCase | RegexOptions.CultureInvariant` for both.
- Test project gained `Microsoft.Extensions.DependencyInjection 10.0.7` (matches the Core version pinned at task_01) so the singleton integration test can build a real `ServiceProvider`.

## Learnings

- For `sealed record`, the `PrintMembers` override is `private bool PrintMembers(StringBuilder builder)` (no `protected virtual` — the compiler-generated method is private when the record is sealed).

## Files / Surfaces

- `DocFormatter.Core/Options/FormattingOptions.cs` (new)
- `DocFormatter.Core/Models/Author.cs` (extended with value equality + `PrintMembers`)
- `DocFormatter.Tests/FormattingOptionsTests.cs` (new — covers DOI/ORCID regex, constants, `Author` equality + null-ORCID `ToString`, DI singleton)
- `DocFormatter.Tests/DocFormatter.Tests.csproj` (added MEDI 10.0.7 reference)

## Errors / Corrections

- First test run failed two `Author` cases: equality (reference vs sequence) and `ToString` (`OrcidId =` substring matched "orcid"). Fixed by overriding `Equals`, `GetHashCode`, and `PrintMembers` on the record.

## Ready for Next Run

- Build green (0 warnings, 0 errors), 21/21 tests pass. Diff staged for manual review (auto-commit disabled).
- task_04 (`FormattingPipeline` orchestrator) and task_05 (`ExtractTopTableRule`) can now consume `FormattingOptions` as a singleton dependency. `Author` is final-shape for downstream rule tests.
