# Task Memory: task_02.md

Keep only task-local execution context here. Do not duplicate facts that are obvious from the repository, task file, PRD documents, or git history.

## Objective Snapshot
Surface ADR-003 regex literals (`EmailRegex`, `CorrespondingMarkerRegex`, `CorrespondingAuthorLabelRegex`) on `FormattingOptions` so `ExtractCorrespondingAuthorRule` (task_07) and `RewriteAbstractRule` (task_08) can consume them without re-introducing the patterns locally.

## Important Decisions

## Learnings
- `Regex.IsMatch` (used by `Assert.Matches` / `Assert.DoesNotMatch`) checks substring match; the EmailRegex still rejects `foo@bar`, `foo@.edu`, `@y.edu` as expected because `[A-Za-z0-9.\-]+\.[A-Za-z]{2,}` requires both a non-empty domain head and a real TLD after a literal dot.

## Files / Surfaces
- `DocFormatter.Core/Options/FormattingOptions.cs` — added three properties + three `[GeneratedRegex]` partial methods.
- `DocFormatter.Tests/FormattingOptionsTests.cs` — added match/no-match `Theory`s plus same-instance `Fact`s for each new regex.

## Errors / Corrections

## Ready for Next Run
- task_07 / task_08 can rely on `_options.EmailRegex`, `_options.CorrespondingMarkerRegex`, `_options.CorrespondingAuthorLabelRegex` directly.
