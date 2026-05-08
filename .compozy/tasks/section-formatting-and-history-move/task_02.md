---
status: completed
title: Implement BodySectionDetector predicates and INTRODUCTION anchor lookup
type: backend
complexity: medium
dependencies:
  - task_01
---

# Task 02: Implement BodySectionDetector predicates and INTRODUCTION anchor lookup

## Overview
Replace the skeleton method bodies in `BodySectionDetector` (created in task_01) with the actual section/sub-section predicates, the table-descendant filter, and the `INTRODUCTION` anchor lookup. These four functions are the shared detection layer that both Phase 3 rules will consume; they encapsulate the predicate decisions ratified by ADR-003 (size discarded), ADR-004 (anchor regex `^INTRODUCTION[\s.:]*$`), and ADR-005 (bold via cascade).

<critical>
- ALWAYS READ the PRD and TechSpec before starting
- REFERENCE TECHSPEC for implementation details — do not duplicate here
- FOCUS ON "WHAT" — describe what needs to be accomplished, not how
- MINIMIZE CODE — show code only to illustrate current structure or problem areas
- TESTS REQUIRED — every task MUST include tests in deliverables
</critical>

<requirements>
- MUST implement `IsInsideTable(Paragraph)` to return `true` when the paragraph is a descendant of `<w:tbl>` at any nesting depth (use ancestor traversal).
- MUST implement `IsSection(Paragraph, MainDocumentPart?)` returning `true` only when ALL of the following hold: trimmed concatenated text length ≥ 3; the text contains at least one letter; ≥ 90% of non-whitespace characters live in runs whose `IsBoldEffective` returns `true`; every letter in the trimmed text is upper-case (`char.IsLetter && !char.IsLower`); paragraph `<w:jc>` is in `{Left, Both, none}`. The 90% threshold MUST count non-whitespace characters per run as the denominator and bold-effective non-whitespace characters as the numerator.
- MUST implement `IsSubsection(Paragraph, MainDocumentPart?)` with the same conditions as `IsSection` except the all-caps check is replaced by "trimmed text contains at least one lower-case letter" (`char.IsLower`). A paragraph that is `IsSection` MUST NOT also be `IsSubsection` (mutual exclusivity).
- MUST implement `FindIntroductionAnchor(Body, MainDocumentPart?)` to return the first `Paragraph` (in body document order) whose trimmed text matches the case-sensitive regex `^INTRODUCTION[\s.:]*$` AND for which `IsSection` returns `true`. MUST return `null` when no such paragraph exists.
- MUST scan only direct `Paragraph` children of `Body` (not paragraphs nested inside `<w:tbl>` or `<w:txbxContent>`); the existing TechSpec note on text boxes applies — text boxes are out of scope.
- SHOULD use a single linear scan and short-circuit on the first match.
- MUST NOT throw on any input; degenerate cases (empty body, paragraph with no runs, text-less paragraph) return `false`/`null`.
- MUST NOT consult `<w:sz>` in any predicate (per ADR-003).
- MUST NOT mutate any input (predicates and lookup are pure functions).
</requirements>

## Subtasks
- [x] 2.1 Implement `IsInsideTable` via ancestor traversal stopping at `<w:tbl>` or document root.
- [x] 2.2 Implement the bold-character-ratio computation used by `IsSection` and `IsSubsection`, calling `IsBoldEffective` per run and weighting by non-whitespace character count.
- [x] 2.3 Implement `IsSection` composing the bold ratio, all-caps check, alignment filter, length minimum, and letter-presence check.
- [x] 2.4 Implement `IsSubsection` reusing the same composition with the lower-case-letter substitution.
- [x] 2.5 Implement `FindIntroductionAnchor` with the case-sensitive regex and the `IsSection` predicate gate.
- [x] 2.6 Extend `Phase3DocxFixtureBuilder` with helpers needed for predicate tests (configurable text content, configurable alignment, paragraphs inside `<w:tbl>`, mixed-case vs all-caps content, partial-bold runs to exercise the 90% threshold).
- [x] 2.7 Add comprehensive unit tests in `BodySectionDetectorTests.cs` (extends the test file from task_01).

## Implementation Details
Modify the file created in task_01:
- `DocFormatter.Core/Rules/BodySectionDetector.cs` — replace skeleton bodies with full implementations.
- `DocFormatter.Tests/BodySectionDetectorTests.cs` — add new test methods alongside existing cascade tests.
- `DocFormatter.Tests/Fixtures/Phase3/Phase3DocxFixtureBuilder.cs` — add predicate-fixture helpers.

Refer to TechSpec section "Implementation Design" for predicate composition and to PRD "Section predicate" / "Sub-section predicate" lists for the exact rule conjunctions. Refer to ADR-004 for accepted/rejected anchor variants (e.g., `INTRODUCTION:`, `INTRODUCTION ` accepted; `INTRODUCTION bla bla`, `INTRODUÇÃO`, `1. INTRODUCTION` rejected).

### Relevant Files
- `DocFormatter.Core/Rules/BodySectionDetector.cs` — Modify; replace skeleton method bodies with actual logic. `IsBoldEffective` already implemented in task_01 is consumed unchanged.
- `DocFormatter.Tests/BodySectionDetectorTests.cs` — Modify; add predicate test cases.
- `DocFormatter.Tests/Fixtures/Phase3/Phase3DocxFixtureBuilder.cs` — Modify; add helpers for table-nested paragraphs, mixed-case vs all-caps content, alignment variations, partial-bold runs.
- `DocFormatter.Core/Pipeline/FormattingContext.cs` — Reference only; `FormattingContext` is NOT extended (per TechSpec decision); both rules call `FindIntroductionAnchor` independently.

### Dependent Files
- `DocFormatter.Core/Rules/MoveHistoryRule.cs` (task_03) — Will consume `FindIntroductionAnchor` to locate the destination of the move.
- `DocFormatter.Core/Rules/PromoteSectionsRule.cs` (task_04) — Will consume `FindIntroductionAnchor`, `IsSection`, `IsSubsection`, `IsInsideTable`.

### Related ADRs
- [ADR-003: Discard font size from detection predicate](../adrs/adr-003-discard-font-size-from-detection.md) — Establishes the predicate components (bold ratio, alignment, caps, length); this task implements them.
- [ADR-004: `INTRODUCTION` as detection anchor](../adrs/adr-004-introduction-as-detection-anchor.md) — Mandates the anchor regex `^INTRODUCTION[\s.:]*$` and the case-sensitive English-only convention.
- [ADR-005: Resolve `<w:b>` via OOXML cascade chain](../adrs/adr-005-bold-cascade-resolver.md) — `IsBoldEffective` (task_01) is invoked here to compute the 90% bold ratio.

## Deliverables
- Fully implemented `IsInsideTable`, `IsSection`, `IsSubsection`, and `FindIntroductionAnchor` in `BodySectionDetector`.
- Extended `Phase3DocxFixtureBuilder` with table-nesting, alignment, casing, and partial-bold helpers.
- Unit tests with 80%+ coverage on the four implemented methods **(REQUIRED)**.
- Integration tests verifying the predicates compose correctly on synthetic full-document fixtures **(REQUIRED)**.

## Tests
- Unit tests:
  - [ ] `IsInsideTable` returns `true` for a paragraph nested in a single `<w:tbl>`.
  - [ ] `IsInsideTable` returns `true` for a paragraph nested in a `<w:tbl>` inside another `<w:tbl>` (deep nesting).
  - [ ] `IsInsideTable` returns `false` for a top-level body paragraph.
  - [ ] `IsSection` returns `true` for a bold all-caps left-aligned paragraph with text length ≥ 3.
  - [ ] `IsSection` returns `true` when `<w:jc>` is `both` (justified — the dominant alignment in the corpus).
  - [ ] `IsSection` returns `true` when `<w:jc>` is absent.
  - [ ] `IsSection` returns `false` when `<w:jc>` is `center` or `right`.
  - [ ] `IsSection` returns `false` for a 2-character text (length below threshold).
  - [ ] `IsSection` returns `false` for a text containing only digits (no letters).
  - [ ] `IsSection` returns `false` when bold-effective character ratio is below 90% (e.g., 50% of letters are in non-bold runs).
  - [ ] `IsSection` returns `true` when bold-effective ratio is exactly 90% (boundary).
  - [ ] `IsSection` returns `false` for a mixed-case paragraph (contains a lower-case letter).
  - [ ] `IsSection` ignores whitespace in the bold-ratio denominator (a non-bold whitespace-only run does not bring the ratio down).
  - [ ] `IsSubsection` returns `true` for a bold mixed-case left-aligned paragraph with text length ≥ 3.
  - [ ] `IsSubsection` returns `false` for a paragraph that is also `IsSection` (mutual exclusivity).
  - [ ] `IsSubsection` returns `false` for a paragraph with no lower-case letter (all-caps).
  - [ ] `FindIntroductionAnchor` returns the first paragraph matching `INTRODUCTION` exactly (no trailing characters).
  - [ ] `FindIntroductionAnchor` accepts variants `INTRODUCTION:`, `INTRODUCTION.`, `INTRODUCTION ` (trailing whitespace).
  - [ ] `FindIntroductionAnchor` rejects `INTRODUCTION bla bla`, `INTRODUÇÃO`, `1. INTRODUCTION`, lowercase `Introduction`.
  - [ ] `FindIntroductionAnchor` rejects a paragraph whose text matches the regex but fails `IsSection` (e.g., not bold).
  - [ ] `FindIntroductionAnchor` returns the FIRST qualifying paragraph when multiple `INTRODUCTION` paragraphs exist.
  - [ ] `FindIntroductionAnchor` returns `null` when no qualifying paragraph exists.
  - [ ] `FindIntroductionAnchor` skips paragraphs nested in `<w:tbl>` even if they match the regex and bold-caps predicate.
- Integration tests:
  - [ ] Construct a synthetic `WordprocessingDocument` containing front-matter content (article-type label `ARTICLE`, title, abstract heading), an `INTRODUCTION` heading, body sections (`MATERIAL AND METHODS`, `RESULTS`), sub-sections (mixed-case bold), and a table; verify `FindIntroductionAnchor` returns the correct paragraph and the predicates classify each paragraph correctly.
- Test coverage target: >=80%
- All tests must pass

## Success Criteria
- All tests passing
- Test coverage >=80%
- All four predicates and the anchor lookup operate correctly on synthetic fixtures covering the documented accepted/rejected variants
- `IsSection` and `IsSubsection` are mutually exclusive on every input (no paragraph is both)
- `BodySectionDetector` is now feature-complete and ready to be consumed by `MoveHistoryRule` (task_03) and `PromoteSectionsRule` (task_04)
