---
status: completed
title: Implement EnsureAuthorBlockSpacingRule
type: backend
complexity: medium
dependencies:
  - task_01
  - task_04
---

# Task 06: Implement EnsureAuthorBlockSpacingRule

## Overview
Add the second Phase 2 Optional rule: guarantee exactly one blank paragraph between the last author paragraph and the first affiliation paragraph. The rule starts from `ctx.AuthorBlockEndParagraph` (published by `RewriteHeaderMvpRule` in task_04), walks forward to the next non-blank paragraph (treated as the first affiliation), and inserts a blank `Paragraph` in front of it when the immediately preceding paragraph is not already blank.

<critical>
- ALWAYS READ the PRD and TechSpec before starting
- REFERENCE TECHSPEC for implementation details — do not duplicate here
- FOCUS ON "WHAT" — describe what needs to be accomplished, not how
- MINIMIZE CODE — show code only to illustrate current structure or problem areas
- TESTS REQUIRED — every task MUST include tests in deliverables
</critical>

<requirements>
- MUST live in `DocFormatter.Core/Rules/EnsureAuthorBlockSpacingRule.cs` and implement `IFormattingRule` with `Severity = Optional` and `Name = nameof(EnsureAuthorBlockSpacingRule)`.
- MUST start from `ctx.AuthorBlockEndParagraph` and walk forward through `body.Elements<Paragraph>()` to the first paragraph whose plain text contains at least one non-whitespace character.
- MUST consider a paragraph "blank" when it has no `Text` descendants with non-whitespace content (an empty paragraph or whitespace-only runs).
- MUST insert a single blank `Paragraph` immediately before the first affiliation paragraph when the immediately preceding paragraph is not already blank; MUST NOT insert anything when a blank already separates them.
- MUST log `[WARN]` and no-op when `ctx.AuthorBlockEndParagraph` is null OR when no non-blank paragraph follows it.
- MUST log `[INFO]` describing the outcome ("blank line inserted between authors and affiliations" / "blank line already present") matching the keys task_09 will read.
- MUST NOT touch the abstract paragraph, the DOI paragraph, or any author paragraph.
</requirements>

## Subtasks
- [x] 6.1 Create the rule class and helper to test paragraph blankness (whitespace-only runs count as blank).
- [x] 6.2 Implement the forward walk from `AuthorBlockEndParagraph` to find the first affiliation paragraph.
- [x] 6.3 Conditionally insert a blank paragraph using `body.InsertBefore(...)`.
- [x] 6.4 Emit `[INFO]`/`[WARN]` per the cases listed in TechSpec "Monitoring and Observability".
- [x] 6.5 Write `EnsureAuthorBlockSpacingRuleTests` covering the four primary cases listed in TechSpec "Testing Approach → Unit Tests".

## Implementation Details
New file under `DocFormatter.Core/Rules/`. The rule does not need `FormattingOptions`. Use `paragraph.NextSibling()` or iterate the parent's elements after locating `ctx.AuthorBlockEndParagraph`. See TechSpec "Component Overview" row for `EnsureAuthorBlockSpacingRule` and "Build Order" step 6.

### Relevant Files
- `DocFormatter.Core/Rules/RewriteHeaderMvpRule.cs` — publishes `ctx.AuthorBlockEndParagraph` (task_04 update).
- `DocFormatter.Core/Pipeline/FormattingContext.cs` — source of the anchor reference (extended in task_01).
- `DocFormatter.Core/Pipeline/IFormattingRule.cs` — interface to implement.
- `DocFormatter.Tests/Fixtures/` — for synthetic body fixtures with controlled paragraph layouts.

### Dependent Files
- `DocFormatter.Core/Reporting/DiagnosticDocument.cs` — task_09 will populate `DiagnosticFormatting.AuthorBlockSpacingApplied` from this rule's report.
- `DocFormatter.Cli/CliApp.cs` — task_10 will register this rule in DI.

### Related ADRs
- [ADR-001: Four discrete Optional rules](../adrs/adr-001-four-discrete-rules.md) — pipeline position (after `ApplyHeaderAlignmentRule`).

## Deliverables
- `DocFormatter.Core/Rules/EnsureAuthorBlockSpacingRule.cs` implementing the spacing logic.
- `DocFormatter.Tests/EnsureAuthorBlockSpacingRuleTests.cs` covering the four scenarios.
- Unit tests with 80%+ coverage **(REQUIRED)**.
- Coverage of "blank already present" idempotent path **(REQUIRED)**.

## Tests
- Unit tests:
  - [x] Last author paragraph followed directly by an affiliation paragraph (no blank) → a single blank `Paragraph` is inserted immediately before the affiliation; report logs the "inserted" `[INFO]`.
  - [x] Last author paragraph followed by an existing blank paragraph and then the affiliation → no insertion; report logs the "already present" `[INFO]`.
  - [x] `ctx.AuthorBlockEndParagraph == null` → rule logs `[WARN]` and does not mutate the body.
  - [x] Author block followed by only blank paragraphs (no affiliation) → rule logs `[WARN]` and does not insert anything.
  - [x] Whitespace-only paragraph between author block and affiliation counts as blank (no insertion).
  - [x] Re-running the rule a second time on the already-fixed body is a no-op (idempotent: produces the "already present" outcome).
- Integration tests:
  - [ ] End-to-end pipeline run (covered by task_10) verifies that, on a fixture without a blank between authors and affiliations, the resulting `.docx` has exactly one blank paragraph there.
- Test coverage target: >=80%
- All tests must pass

## Success Criteria
- All tests passing
- Test coverage >=80%
- Rule operates as Optional and never aborts the document.
- After the rule, the body has exactly one blank paragraph between the last author paragraph and the first affiliation.
