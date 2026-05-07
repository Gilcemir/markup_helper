# Task Memory: task_01.md

Keep only task-local execution context here. Do not duplicate facts that are obvious from the repository, task file, PRD documents, or git history.

## Objective Snapshot
Extend `FormattingContext` with 8 nullable Phase 2 properties (5 `Paragraph?` refs + 3 corresponding-author scalars) plus an invariant comment, with unit tests. Done.

## Important Decisions
- Used pure C# auto-properties with no constructor or list initializer — keeps the "publishing rule sets it" contract enforced by language semantics rather than runtime defaults.
- `int?` for `CorrespondingAuthorIndex` (not a struct/record) per TechSpec.
- Tests assert `Assert.Same` on the round-tripped paragraph instance to guarantee reference identity is preserved (matters for downstream rules that hold the same reference).

## Learnings
- TechSpec lines 88–106 list the canonical property names and types; matched verbatim.
- The single-line "publish/no-delete" invariant comment lives directly above the new property block in `FormattingContext.cs`.
- `Author` record requires `(Name, IReadOnlyList<string> AffiliationLabels, string? OrcidId)` for construction in tests.

## Files / Surfaces
- Edited: `DocFormatter.Core/Pipeline/FormattingContext.cs` (added 8 properties + invariant comment).
- Added: `DocFormatter.Tests/FormattingContextTests.cs` (15 xUnit tests).

## Errors / Corrections
None.

## Ready for Next Run
task_02 (FormattingOptions regexes) and task_03 (`ParseHeaderLinesRule` stash section/title) can now compile against the extended context.
