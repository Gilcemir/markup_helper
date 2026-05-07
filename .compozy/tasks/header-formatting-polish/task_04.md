---
status: completed
title: Stash DOI and author-block-end paragraphs in RewriteHeaderMvpRule
type: backend
complexity: low
dependencies:
  - task_01
---

# Task 04: Stash DOI and author-block-end paragraphs in RewriteHeaderMvpRule

## Overview
`ApplyHeaderAlignmentRule` (task_05) needs the DOI paragraph reference, and `EnsureAuthorBlockSpacingRule` (task_06) needs the last author paragraph the rewrite produced. Both are created inside `RewriteHeaderMvpRule`; this task stashes them in `FormattingContext.DoiParagraph` and `FormattingContext.AuthorBlockEndParagraph` without altering the MVP rewrite itself.

<critical>
- ALWAYS READ the PRD and TechSpec before starting
- REFERENCE TECHSPEC for implementation details — do not duplicate here
- FOCUS ON "WHAT" — describe what needs to be accomplished, not how
- MINIMIZE CODE — show code only to illustrate current structure or problem areas
- TESTS REQUIRED — every task MUST include tests in deliverables
</critical>

<requirements>
- MUST set `ctx.DoiParagraph` to the freshly built DOI paragraph after it is inserted at the top of the body. When `ctx.Doi` is null and the DOI line is not produced, `ctx.DoiParagraph` MUST remain null.
- MUST set `ctx.AuthorBlockEndParagraph` to the last new author paragraph appended to the body. When `ctx.Authors` is empty (no author paragraphs rendered), `ctx.AuthorBlockEndParagraph` MUST remain null.
- MUST NOT alter existing report messages (`MissingDoiMessage`, `EmptyAuthorsMessage`, `MissingAuthorsParagraphMessage`) or the rule's `Critical` severity.
- MUST NOT change the order or structure of the MVP rewrite (DOI insertion, author paragraph insertion, original-paragraph removal).
</requirements>

## Subtasks
- [x] 4.1 Capture the DOI `Paragraph` reference at the point where it is currently created and inserted; assign it to `ctx.DoiParagraph`.
- [x] 4.2 Capture the last `Paragraph` appended in the author-rendering loop; assign it to `ctx.AuthorBlockEndParagraph` after the original author paragraphs are removed.
- [x] 4.3 Keep the assignments inside the existing branches so null-context-state matches null inputs (no DOI, no authors).
- [x] 4.4 Extend `RewriteHeaderMvpRuleTests` with the two new context assertions.

## Implementation Details
File `DocFormatter.Core/Rules/RewriteHeaderMvpRule.cs` already builds the DOI paragraph (`BuildPlainParagraph(ctx.Doi)`) and the author paragraphs (`BuildAuthorParagraph(author)` inside the renderable-authors loop). Add two assignments next to the existing inserts. See TechSpec "Impact Analysis" row for `RewriteHeaderMvpRule.cs`.

### Relevant Files
- `DocFormatter.Core/Rules/RewriteHeaderMvpRule.cs` — rule to extend.
- `DocFormatter.Core/Pipeline/FormattingContext.cs` — destination of the new references (added in task_01).
- `DocFormatter.Tests/RewriteHeaderMvpRuleTests.cs` — pattern for new tests.

### Dependent Files
- `DocFormatter.Core/Rules/ApplyHeaderAlignmentRule.cs` — task_05 consumes `DoiParagraph` (file does not exist yet).
- `DocFormatter.Core/Rules/EnsureAuthorBlockSpacingRule.cs` — task_06 consumes `AuthorBlockEndParagraph` (file does not exist yet).

### Related ADRs
- [ADR-001: Four discrete Optional rules](../adrs/adr-001-four-discrete-rules.md) — describes the publishing rule's responsibility for `DoiParagraph` and `AuthorBlockEndParagraph`.

## Deliverables
- `RewriteHeaderMvpRule` populating `ctx.DoiParagraph` and `ctx.AuthorBlockEndParagraph`.
- Updated `RewriteHeaderMvpRuleTests` covering both context writes and their null cases.
- Unit tests with 80%+ coverage **(REQUIRED)**.
- Coverage of the "no DOI" and "no authors" branches for the new context state **(REQUIRED)**.

## Tests
- Unit tests:
  - [x] After running with a non-null `ctx.Doi`, `ctx.DoiParagraph` is the same paragraph instance now sitting at the top of the body.
  - [x] After running with `ctx.Doi == null`, `ctx.DoiParagraph` remains `null` and the existing `[WARN]` is still emitted.
  - [x] After running with one or more renderable authors, `ctx.AuthorBlockEndParagraph` equals the last new author paragraph appended.
  - [x] After running with `ctx.Authors` empty, `ctx.AuthorBlockEndParagraph` remains `null` (no author rewrite happened).
  - [x] Existing tests (`MissingArticleTitleMessage`, ORCID rendering, affiliation labels) keep passing.
- Integration tests:
  - [x] `FormattingPipelineTests` end-to-end run sets both new context fields when the MVP succeeds.
- Test coverage target: >=80%
- All tests must pass

## Success Criteria
- All tests passing
- Test coverage >=80%
- DOI and author-block-end paragraph references are visible to downstream rules via `FormattingContext`.
- No regression in MVP rewrite output structure or messages.
