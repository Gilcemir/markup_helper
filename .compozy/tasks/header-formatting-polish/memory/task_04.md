# Task Memory: task_04.md

Keep only task-local execution context here. Do not duplicate facts that are obvious from the repository, task file, PRD documents, or git history.

## Objective Snapshot
- Status: completed.
- RewriteHeaderMvpRule now publishes `DoiParagraph` (in the DOI insertion branch) and `AuthorBlockEndParagraph` (last new author paragraph appended in the rewrite branch).

## Important Decisions
- Captured the last author paragraph during the build loop (new local `lastAuthorParagraph`) instead of indexing the body afterwards — avoids depending on the placeholder blank `Paragraph()` position and stays null naturally when `renderableAuthors` is empty (only empty-name records).
- Assigned `ctx.AuthorBlockEndParagraph` AFTER the original author paragraphs are removed, mirroring the rule's existing flow; the new paragraph instances are not affected by the remove pass.
- Skipped the inline `Paragraph?` declaration inside the foreach to keep diff small; nothing forced a refactor.

## Learnings
- `Apply_WithEmptyAuthorsList_*` test path: `ctx.Authors.Count == 0` → the `else` branch never runs, so `AuthorBlockEndParagraph` MUST stay null without explicit assignment. Same outcome when `ctx.Authors` has only empty-name records (renderableAuthors empty), but the path taken is different — covered by the dedicated `Apply_WhenOnlyEmptyNameAuthorRecords_*` test.

## Files / Surfaces
- `DocFormatter.Core/Rules/RewriteHeaderMvpRule.cs` — added one local + two `ctx` assignments inside the existing branches; no message/severity/order changes.
- `DocFormatter.Tests/RewriteHeaderMvpRuleTests.cs` — extended four existing tests with `Same`/`Null` assertions on the new context fields, added focused empty-name-only test.

## Errors / Corrections

## Ready for Next Run
- task_05 (`ApplyHeaderAlignmentRule`) can now read `ctx.DoiParagraph` (Right alignment), and combined with task_03's `SectionParagraph`/`TitleParagraph` has all three header references it needs.
- task_06 (`EnsureAuthorBlockSpacingRule`) can read `ctx.AuthorBlockEndParagraph` as the starting anchor; remember it can be null when the document has no renderable authors — rule must `[WARN]` and no-op rather than throw.
