# Task Memory: task_02.md

Keep only task-local execution context here. Do not duplicate facts that are obvious from the repository, task file, PRD documents, or git history.

## Objective Snapshot

- `TryParseAuthorKeyed` rewritten per TechSpec algorithm (`.` segments; `;`/`,` pieces; `:` opens entry; initials-block lookback via `AuthorInitialsResolver.IsInitialsToken`). Done 2026-09-14.

## Important Decisions

- Kept the TechSpec order literally: a `:` piece first checks `terms.Count == 0` (entry without terms → Prose), then flushes, then builds the next keys from `pending + own key` (blank own key tolerated when pending exists; `": terms"` with no key at all → Prose).
- `Flush` helper returns "any term maps" so the discriminator stays one expression; `SplitTrim` takes `params char[]`, role-keyed caller unchanged.
- 5441 expectation in the task spec ("7 terms", listing 8) is a counting slip; the real text yields 9 terms for CFA/JAC (adds `Writing - review & editing`). Test asserts the exact 9-term list derived from the text.

## Learnings

- Red signal: 8 of 21 parser tests failed before the rewrite (all 7 corpus tests + comma-keys test); after: 21/21, full suite 841/841 (828 baseline + 13 new).
- A throwaway sweep confirmed the other v26n3 texts (5524, 5316, 5529, 5617, 5719, 5536 body) parse AuthorKeyed with every term mapping → all 15 statements are AuthorKeyed at parser level. Sweep file deleted, not committed.
- Role-keyed never captures the corpus texts because the first `;`-chunk label is an initials block, which does not map.

## Files / Surfaces

- `DocFormatter.Core/Jats/CreditStatementParser.cs` — `Parse` doc, `TryParseAuthorKeyed`, new `Flush`, `SplitTrim(params)`.
- `DocFormatter.Tests/Jats/CreditStatementParserTests.cs` — 7 corpus consts + 7 corpus facts, 1 negative theory (5 cases), 1 comma-keys fact, `AssertNoJunkTerms` helper.

## Errors / Corrections

- None.

## Ready for Next Run

- task_04's `;`-separated integration fixture can reuse any `Corpus*` const shape; `CreditStatement.Entries` for 5613 now yields NHN/TVB/QHTP so the injector prompt path for NHN (empty surname) is reachable end-to-end.
- Task spec success line "5536 once task_03 reads it" holds: the 5536 body text parses today; only the reader blocks it.
