# Task Memory: task_06.md

Keep only task-local execution context here. Do not duplicate facts that are obvious from the repository, task file, PRD documents, or git history.

## Objective Snapshot

- Add critical-severity `ParseHeaderLinesRule` that reads section + article title from the first two non-empty paragraphs after the deleted top table; populates only `ctx.ArticleTitle`. Section is consumed for diagnostic output (read-only) but not stored on `FormattingContext`, per ADR-001's "rest intact" decision.

## Important Decisions

- Rule walks `body.Elements<Paragraph>()` directly (no positional offset against the deleted table); since `ExtractTopTableRule` removes the table, the paragraph sequence is the document body's natural ordering. Tests confirm correct ordering when the rule runs alone (no pre-existing table) and inside the pipeline after `ExtractTopTableRule`.
- Error messages exposed as `public const MissingSectionMessage` / `MissingTitleMessage` so downstream rules and pipeline tests can assert against them without re-typing the wording (mirrors the convention from `ExtractTopTableRule.CriticalAbortMessage`).
- Loop short-circuits on the second non-empty paragraph (per task spec: "still uses the first non-empty paragraph after section as the title and ignores subsequent ones"). Test `Apply_OnlyConsidersFirstNonEmptyTitleParagraph_AndIgnoresSubsequentParagraphs` covers this.

## Learnings

- Plain text reading via `string.Concat(p.Descendants<Text>().Select(t => t.Text))` already used by `ExtractTopTableRule.GetCellPlainText`; reused here at paragraph scope. No need for a shared helper yet — different element type and only two call sites.

## Files / Surfaces

- `DocFormatter.Core/Rules/ParseHeaderLinesRule.cs` (new) — `Severity = Critical`, no doc mutation.
- `DocFormatter.Tests/ParseHeaderLinesRuleTests.cs` (new) — 7 unit tests + 1 pipeline integration test (rules 1+2 chained).

## Errors / Corrections

## Ready for Next Run

- Task 06 verified PASS: 48/48 tests, build 0/0 errors-warnings. Master tasks file updated; auto-commit disabled per run flag — diff is staged for manual review.
- Task 09 (`RewriteHeaderMvpRule`) consumes `ctx.ArticleTitle`. Note that the SECTION text is currently NOT persisted on the context — task 09 must either re-read the section from the body or task 06's contract must be extended (task spec explicitly defers this to task 09's design call).
