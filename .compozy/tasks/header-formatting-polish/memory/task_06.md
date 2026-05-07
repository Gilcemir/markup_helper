# Task Memory: task_06.md

Keep only task-local execution context here. Do not duplicate facts that are obvious from the repository, task file, PRD documents, or git history.

## Objective Snapshot
- Implemented `EnsureAuthorBlockSpacingRule` (Optional) that walks forward from `ctx.AuthorBlockEndParagraph` to the first non-blank paragraph and inserts a blank paragraph before it when the immediately preceding paragraph is not already blank.

## Important Decisions
- Used `paragraph.NextSibling<Paragraph>()` to walk only Paragraph siblings (skips Tables/SectionProperties), which matches the TechSpec language ("walk forward through `body.Elements<Paragraph>()`") and keeps the rule oblivious to non-paragraph body content.
- "Immediately preceding paragraph" check is implemented by tracking the last paragraph visited during the forward walk; for the no-blank-between case the previous paragraph is the anchor itself (non-blank → insert), and for the blank-already-there case the previous paragraph is the trailing blank visited before the affiliation (blank → no-op).

## Learnings
- `body.InsertBefore(new Paragraph(), affiliation)` produces a paragraph with no `Text` descendants — `Descendants<Text>()` returns empty for it, so `IsBlank` correctly reports it as blank on a re-run, giving the rule idempotency without any extra bookkeeping.

## Files / Surfaces
- `DocFormatter.Core/Rules/EnsureAuthorBlockSpacingRule.cs` (new)
- `DocFormatter.Tests/EnsureAuthorBlockSpacingRuleTests.cs` (new, 7 tests)

## Errors / Corrections

## Ready for Next Run
- task_09 can now read the rule's `[INFO]`/`[WARN]` entries via the constants `BlankLineInsertedMessage`, `BlankLineAlreadyPresentMessage`, `MissingAuthorBlockEndMessage`, `MissingAffiliationMessage` to populate `DiagnosticFormatting.AuthorBlockSpacingApplied`.
- task_10 will register the rule in `CliApp.BuildServiceProvider` after `ApplyHeaderAlignmentRule` and before `RewriteAbstractRule`.
