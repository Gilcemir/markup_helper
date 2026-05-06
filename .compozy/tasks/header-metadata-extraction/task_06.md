---
status: completed
title: ParseHeaderLinesRule for section and article title
type: backend
complexity: low
dependencies:
    - task_05
---

# Task 6: ParseHeaderLinesRule for section and article title

## Overview
Implement the rule that reads the two positional paragraphs immediately after the deleted top-of-document table: the first paragraph carries the journal section (e.g., "Original Article") and the second paragraph carries the article title. This rule populates `FormattingContext.ArticleTitle`. Section is read for downstream layout but not currently stored on the context (per PRD's "rest intact" decision the section paragraph stays where it is).

<critical>
- ALWAYS READ the PRD and TechSpec before starting
- REFERENCE TECHSPEC for implementation details — do not duplicate here
- FOCUS ON "WHAT" — describe what needs to be accomplished, not how
- MINIMIZE CODE — show code only to illustrate current structure or problem areas
- TESTS REQUIRED — every task MUST include tests in deliverables
</critical>

<requirements>
- 1. The rule MUST read paragraphs by position relative to the now-empty top of the document body (table already deleted by task_05).
- 2. The first non-empty paragraph after the deleted table MUST be treated as the section title.
- 3. The next non-empty paragraph after the section MUST be treated as the article title and assigned to `FormattingContext.ArticleTitle`.
- 4. If either paragraph is missing or empty, the rule MUST throw `InvalidOperationException`; severity `Critical`.
- 5. The rule MUST NOT mutate the document; it only reads.
- 6. Whitespace-only paragraphs MUST be skipped, not counted as the section or title.
</requirements>

## Subtasks
- [x] 6.1 Create `DocFormatter.Core/Rules/ParseHeaderLinesRule.cs` implementing `IFormattingRule` with `Severity=Critical`.
- [x] 6.2 Walk `body.Elements<Paragraph>()` after the deletion point, skipping whitespace-only paragraphs.
- [x] 6.3 Assign the second non-empty paragraph's plain text (run inner-text concatenated) to `ctx.ArticleTitle`.
- [x] 6.4 Add xUnit tests covering happy path, missing section, missing title, whitespace-only paragraphs interleaved.

## Implementation Details
File is new under `DocFormatter.Core/Rules/`. See TechSpec "Rules" rule #2. Translated titles are explicitly out of scope (PRD Non-Goals); if multiple paragraphs exist between section and authors block, the rule still uses the **first** non-empty paragraph after section as the title and ignores subsequent ones until task_09 (RewriteHeaderMvpRule) decides what to do with the rest.

### Relevant Files
- `.compozy/tasks/header-metadata-extraction/_techspec.md` — "Rules" rule #2
- `instructions.md` — original spec's pipeline rule #3

### Dependent Files
- `DocFormatter.Core/Rules/ParseHeaderLinesRule.cs` (new)
- `DocFormatter.Tests/ParseHeaderLinesRuleTests.cs` (new)

### Related ADRs
- [ADR-001: Esqueleto alinhado ao spec](adrs/adr-001.md)

## Deliverables
- `ParseHeaderLinesRule` implementation
- xUnit tests for the four scenarios in subtask 6.4
- Unit tests with 80%+ coverage **(REQUIRED)**
- Integration tests for [pipeline-driven rule execution] **(REQUIRED)**

## Tests
- Unit tests:
  - [x] Body with paragraphs `["Original Article", "On the Behavior of...", "John Doe..."]`: `ArticleTitle="On the Behavior of..."`.
  - [x] Body with only one paragraph after the deleted table: rule throws `InvalidOperationException` referencing the missing title.
  - [x] Body with paragraphs `["", "Original Article", "  ", "On the Behavior of..."]`: empty and whitespace-only paragraphs are skipped; `ArticleTitle="On the Behavior of..."`.
  - [x] Body with no paragraphs after the deleted table: rule throws `InvalidOperationException` referencing the missing section.
  - [x] Title paragraph composed of multiple Runs (`<w:r>On the </w:r><w:r>Behavior</w:r>`): `ArticleTitle="On the Behavior"`.
- Integration tests:
  - [x] Run inside `FormattingPipeline` immediately after `ExtractTopTableRule` populates `Doi/ElocationId` and `ParseHeaderLinesRule` populates `ArticleTitle` — both fields present on the context.
- Test coverage target: >=80%
- All tests must pass

## Success Criteria
- All tests passing
- Test coverage >=80%
- Rule does not mutate the document
- Critical-severity abort messages cite the missing element by name (section vs title)
