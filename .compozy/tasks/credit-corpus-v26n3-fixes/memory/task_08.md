# Task Memory: task_08.md

Keep only task-local execution context here. Do not duplicate facts that are obvious from the repository, task file, PRD documents, or git history.

## Objective Snapshot

- Done 2026-09-14: plain-text ORCID → `TokenKind.Orcid` in `AppendRunTokens` (INFO per extraction), `AttachOrcid` warns on a conflicting second id, `WarnOnRepeatedLastToken` in `FlagSuspicions`; both warnings via new `AuthorBuilder.Warn` (confidence unchanged). Suite 890 → 897; `make phase2-verify` 10/10 PASS.

## Important Decisions

- `AttachOrcid` replaces the silent `??=`: a second, different ORCID on the same author (hyperlink + plain text, or two hyperlinks) is now a WARN. Same-id duplicates stay silent (case-insensitive compare for the X check digit).
- Only `Run` children are scanned for plain-text ORCIDs; non-ORCID hyperlink inner text is left as before (no corpus case).
- `FoldName` (lowercase + strip diacritics) is local to the rule; `AuthorInitialsResolver.Fold` is private in another namespace and not worth exposing for this.

## Learnings

- The existing hyperlink test with inner text "0000-…" still passes because badge detection runs on hyperlink inner text before any token is emitted; plain-text scanning never sees hyperlink content.

## Files / Surfaces

- Modified: `DocFormatter.Core/Rules/ExtractAuthorsRule.cs` (+`PlainOrcidRegex`, `AppendPlainTextTokens`, `AttachOrcid`, `WarnOnRepeatedLastToken`, `FoldName`, `AuthorBuilder.Warn`).
- Tests: `ExtractAuthorsRuleTests` (+7).

## Errors / Corrections

- None.

## Ready for Next Run

- task_10 README note: Phase 1 now reports "extracted ORCID '…' from plain text" (INFO) and "last name token '…' repeats an earlier token; SciELO Markup mark_authors will tag the first occurrence as the surname" (WARN).
