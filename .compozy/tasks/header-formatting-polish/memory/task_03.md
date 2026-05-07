# Task Memory: task_03.md

Keep only task-local execution context here. Do not duplicate facts that are obvious from the repository, task file, PRD documents, or git history.

## Objective Snapshot
- Status: completed.
- ParseHeaderLinesRule now publishes SectionParagraph and TitleParagraph after validation.

## Important Decisions
- Captured paragraph references in the same loop that captures the section/title strings (no second pass) — keeps the `<w:br/>`-split case (section + title in same paragraph) producing identical references for both fields.
- Both context writes happen AFTER the existing missing-section / missing-title throws, so error paths leave the fields null.

## Learnings

## Files / Surfaces
- `DocFormatter.Core/Rules/ParseHeaderLinesRule.cs` — added two paragraph-tracking locals + two ctx assignments next to existing `ctx.ArticleTitle = title`.
- `DocFormatter.Tests/ParseHeaderLinesRuleTests.cs` — extended canonical test, added separate-paragraphs-with-blanks test, added missing-title error-path assertion, extended `<w:br/>`-split test, extended pipeline integration test.

## Errors / Corrections

## Ready for Next Run
- task_04 (RewriteHeaderMvpRule) can now assume task_03 published `SectionParagraph` / `TitleParagraph` upstream; the alignment rule (task_05) will consume all three references together.
