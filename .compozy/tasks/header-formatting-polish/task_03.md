---
status: completed
title: Stash section and title paragraphs in ParseHeaderLinesRule
type: backend
complexity: low
dependencies:
  - task_01
---

# Task 03: Stash section and title paragraphs in ParseHeaderLinesRule

## Overview
`ApplyHeaderAlignmentRule` (task_05) needs `Paragraph` references for the section line (right-aligned) and the title line (centered). `ParseHeaderLinesRule` already locates both lines positionally; this task adds two writes to `FormattingContext.SectionParagraph` and `FormattingContext.TitleParagraph` without changing the rule's MVP outputs.

<critical>
- ALWAYS READ the PRD and TechSpec before starting
- REFERENCE TECHSPEC for implementation details — do not duplicate here
- FOCUS ON "WHAT" — describe what needs to be accomplished, not how
- MINIMIZE CODE — show code only to illustrate current structure or problem areas
- TESTS REQUIRED — every task MUST include tests in deliverables
</critical>

<requirements>
- MUST set `ctx.SectionParagraph` to the `Paragraph` whose first non-blank logical line is identified as the section title.
- MUST set `ctx.TitleParagraph` to the `Paragraph` whose logical line is identified as the article title.
- MUST preserve the rule's existing throw behavior on missing section or title (`MissingSectionMessage` / `MissingTitleMessage`) and `ctx.ArticleTitle` assignment.
- MUST NOT change `RuleSeverity.Critical` or rule name.
- The two paragraph references MAY refer to the same `Paragraph` instance when section and title share a paragraph split by `<w:br/>` (logical-line splitting already handled in MVP code).
</requirements>

## Subtasks
- [x] 3.1 Capture the `Paragraph` reference whose iteration loop identifies the section line.
- [x] 3.2 Capture the `Paragraph` reference whose iteration loop identifies the title line.
- [x] 3.3 Assign both references to `ctx.SectionParagraph` and `ctx.TitleParagraph` after validation passes.
- [x] 3.4 Add tests asserting both references are populated for the canonical fixture.
- [x] 3.5 Add tests covering the `<w:br/>`-split case where section and title live in the same paragraph.

## Implementation Details
File `DocFormatter.Core/Rules/ParseHeaderLinesRule.cs` already iterates paragraphs and tracks logical lines via `GetParagraphLogicalLines`. The change is two assignments next to the existing `ctx.ArticleTitle = title` line. See TechSpec "Impact Analysis" row for `ParseHeaderLinesRule.cs`.

### Relevant Files
- `DocFormatter.Core/Rules/ParseHeaderLinesRule.cs` — rule to extend.
- `DocFormatter.Core/Pipeline/FormattingContext.cs` — destination of the new references (added in task_01).
- `DocFormatter.Tests/ParseHeaderLinesRuleTests.cs` — pattern for new tests.
- `DocFormatter.Tests/Fixtures/` — fixture builders for synthetic documents.

### Dependent Files
- `DocFormatter.Core/Rules/ApplyHeaderAlignmentRule.cs` — task_05 consumes both references (file does not exist yet).
- `DocFormatter.Tests/ParseHeaderLinesRuleTests.cs` — must add new assertions.

### Related ADRs
- [ADR-001: Four discrete Optional rules](../adrs/adr-001-four-discrete-rules.md) — defines the rule's contract to publish paragraph references.

## Deliverables
- `ParseHeaderLinesRule` populating `ctx.SectionParagraph` and `ctx.TitleParagraph`.
- Updated `ParseHeaderLinesRuleTests` covering the two new context writes.
- Unit tests with 80%+ coverage **(REQUIRED)**.
- Coverage of the `<w:br/>` split case **(REQUIRED)**.

## Tests
- Unit tests:
  - [x] Canonical fixture (section in paragraph A, title in paragraph B, separate paragraphs) → `ctx.SectionParagraph == A`, `ctx.TitleParagraph == B`.
  - [x] Single-paragraph fixture (section + `<w:br/>` + title in same paragraph) → both context fields point to the same `Paragraph` instance.
  - [x] Existing test asserting `MissingSectionMessage` is thrown when no section paragraph exists keeps passing; context fields are not assigned in that error path.
  - [x] Existing test asserting `MissingTitleMessage` is thrown when no title is found keeps passing.
  - [x] `ctx.ArticleTitle` remains the same string value the MVP currently sets.
- Integration tests:
  - [x] `FormattingPipelineTests` end-to-end run sets both new context fields without breaking downstream MVP rules.
- Test coverage target: >=80%
- All tests must pass

## Success Criteria
- All tests passing
- Test coverage >=80%
- Section and title paragraph references are visible to downstream rules via `FormattingContext`.
- No regression in MVP `ArticleTitle` extraction or error messages.
