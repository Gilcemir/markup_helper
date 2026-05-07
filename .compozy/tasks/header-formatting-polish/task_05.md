---
status: completed
title: Implement ApplyHeaderAlignmentRule
type: backend
complexity: medium
dependencies:
  - task_01
  - task_03
  - task_04
---

# Task 05: Implement ApplyHeaderAlignmentRule

## Overview
Add the first of the four Phase 2 Optional rules: set `Justification` on the DOI paragraph (right), section paragraph (right), and title paragraph (center). The rule reads the three paragraph references that tasks 03 and 04 published into `FormattingContext` and applies the alignment property idempotently — if a paragraph already has the target alignment, the rule rewrites it without warning.

<critical>
- ALWAYS READ the PRD and TechSpec before starting
- REFERENCE TECHSPEC for implementation details — do not duplicate here
- FOCUS ON "WHAT" — describe what needs to be accomplished, not how
- MINIMIZE CODE — show code only to illustrate current structure or problem areas
- TESTS REQUIRED — every task MUST include tests in deliverables
</critical>

<requirements>
- MUST live in `DocFormatter.Core/Rules/ApplyHeaderAlignmentRule.cs` and implement `IFormattingRule` with `Severity = Optional` and `Name = nameof(ApplyHeaderAlignmentRule)`.
- MUST apply right alignment to `ctx.DoiParagraph`, right alignment to `ctx.SectionParagraph`, and center alignment to `ctx.TitleParagraph` via OOXML `Justification`.
- MUST log an `[INFO]` summarizing which paragraphs were aligned (booleans for DOI/section/title) using report keys consistent with the diagnostic JSON producer (task_09).
- MUST log a `[WARN]` per missing paragraph reference (e.g., "DOI paragraph not found in context") and continue with the others.
- MUST be idempotent: when the paragraph already carries the target `Justification`, the rule still writes it but does NOT emit a `[WARN]` and the resulting state matches the target.
- MUST NOT throw on any null context field — every code path is best-effort.
</requirements>

## Subtasks
- [x] 5.1 Create the rule class with the standard constructor pattern (no constructor dependencies needed beyond what `IFormattingRule.Apply` provides).
- [x] 5.2 Implement the three alignment writes using `JustificationValues.Right` for DOI/section and `JustificationValues.Center` for title.
- [x] 5.3 Emit per-missing-paragraph `[WARN]`s and a summary `[INFO]` reporting which alignments succeeded.
- [x] 5.4 Write `ApplyHeaderAlignmentRuleTests` covering all-three-present, single-missing variants (×3), all-missing, and pre-aligned (idempotency).

## Implementation Details
New file under `DocFormatter.Core/Rules/`. The rule signature mirrors `LocateAbstractAndInsertElocationRule` (Optional, no `_options` dependency required — alignment values are constants). Use `paragraph.ParagraphProperties ??= new ParagraphProperties()` and replace its `Justification` element. See TechSpec "Component Overview" row for `ApplyHeaderAlignmentRule` and ADR-001 for ordering.

### Relevant Files
- `DocFormatter.Core/Rules/LocateAbstractAndInsertElocationRule.cs` — closest sibling pattern (Optional rule with paragraph mutation).
- `DocFormatter.Core/Rules/RewriteHeaderMvpRule.cs` — example of paragraph property creation.
- `DocFormatter.Core/Pipeline/FormattingContext.cs` — source of paragraph references (extended in task_01).
- `DocFormatter.Core/Pipeline/IFormattingRule.cs` — interface to implement.
- `DocFormatter.Core/Pipeline/IReport.cs` / `Report.cs` — `Info`/`Warn` API.

### Dependent Files
- `DocFormatter.Core/Reporting/DiagnosticDocument.cs` — task_09 will populate `DiagnosticAlignment` from this rule's report entries.
- `DocFormatter.Cli/CliApp.cs` — task_10 will register this rule in DI.

### Related ADRs
- [ADR-001: Four discrete Optional rules](../adrs/adr-001-four-discrete-rules.md) — defines the rule's pipeline position (after `RewriteHeaderMvpRule`).

## Deliverables
- `DocFormatter.Core/Rules/ApplyHeaderAlignmentRule.cs` implementing the three alignment writes.
- `DocFormatter.Tests/ApplyHeaderAlignmentRuleTests.cs` covering all branches.
- Unit tests with 80%+ coverage **(REQUIRED)**.
- Idempotency test fixture **(REQUIRED)**.

## Tests
- Unit tests:
  - [ ] All three context paragraph references populated → DOI paragraph carries `Justification.Val == Right`, section `Right`, title `Center`; report contains a single `[INFO]` summarizing all three booleans true.
  - [ ] `DoiParagraph` null only → section + title aligned; one `[WARN]` for the missing DOI reference.
  - [ ] `SectionParagraph` null only → DOI + title aligned; one `[WARN]` for the missing section reference.
  - [ ] `TitleParagraph` null only → DOI + section aligned; one `[WARN]` for the missing title reference.
  - [ ] All three null → three `[WARN]`s; document body unchanged.
  - [ ] Pre-aligned paragraph (already `Right`) → no `[WARN]`, alignment property still present and equal to `Right`; report shows the alignment as applied.
  - [ ] Rule does not mutate `ctx.Authors`, `ctx.Doi`, or any non-paragraph context state.
- Integration tests:
  - [ ] End-to-end pipeline run with the new rule registered (covered by task_10) verifies the rule fires after `RewriteHeaderMvpRule` and produces the expected OOXML.
- Test coverage target: >=80%
- All tests must pass

## Success Criteria
- All tests passing
- Test coverage >=80%
- The rule is fully Optional — failures never abort the document.
- DOI/section/title paragraphs visibly carry the target alignment in the produced `.docx`.
