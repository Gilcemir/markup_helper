---
status: completed
title: Implement PromoteSectionsRule
type: backend
complexity: medium
dependencies:
  - task_02
---

# Task 04: Implement PromoteSectionsRule

## Overview
Add `PromoteSectionsRule` as a new `IFormattingRule` (Optional severity, pipeline position #11) that, starting from the `INTRODUCTION` anchor through the end of the body, mutates `<w:jc>` to `center` and `<w:sz>`/`<w:szCs>` to `32` (16pt) on paragraphs matching `IsSection`, and `<w:sz>`/`<w:szCs>` to `28` (14pt) on paragraphs matching `IsSubsection`. The rule is purely cosmetic per ADR-002: it adds or replaces paragraph and run properties but never adds, removes, or reorders text content.

<critical>
- ALWAYS READ the PRD and TechSpec before starting
- REFERENCE TECHSPEC for implementation details — do not duplicate here
- FOCUS ON "WHAT" — describe what needs to be accomplished, not how
- MINIMIZE CODE — show code only to illustrate current structure or problem areas
- TESTS REQUIRED — every task MUST include tests in deliverables
</critical>

<requirements>
- MUST create `PromoteSectionsRule` as `public sealed class` implementing `IFormattingRule` with `Severity = RuleSeverity.Optional` and `Name = nameof(PromoteSectionsRule)`.
- MUST resolve the anchor via `BodySectionDetector.FindIntroductionAnchor`. If null, MUST emit `[WARN]` with `AnchorMissingMessage` and return without mutation.
- MUST iterate body paragraphs starting at the anchor (inclusive) through the end of the body in document order.
- MUST skip paragraphs that satisfy `BodySectionDetector.IsInsideTable`.
- MUST skip paragraphs whose object identity equals `FormattingContext.SectionParagraph`, `FormattingContext.TitleParagraph`, or `FormattingContext.DoiParagraph` (defence in depth, per TechSpec). Comparison MUST use `ReferenceEquals`, not value equality.
- For paragraphs matching `IsSection`: MUST set `<w:jc>` to `center` on the paragraph and set `<w:sz w:val="32"/>` and `<w:szCs w:val="32"/>` on every text-bearing run. Bold MUST be preserved as-is (already true for the predicate to match).
- For paragraphs matching `IsSubsection`: MUST set `<w:jc>` to `center` and `<w:sz w:val="28"/>`/`<w:szCs w:val="28"/>` on every text-bearing run.
- MUST mutate ONLY `<w:jc>` on `ParagraphProperties` and `<w:sz>`/`<w:szCs>` on existing `RunProperties`. MUST NOT replace `ParagraphProperties` wholesale; MUST NOT mutate any other property (`<w:b>`, `<w:i>`, `<w:color>`, `<w:lang>`, etc.).
- MUST be idempotent: re-applying the rule to its own output produces identical OOXML.
- MUST emit `[INFO]` summary `promoted {N} sections (16pt center) and {M} sub-sections (14pt center)` after a successful pass, plus the `[INFO] INTRODUCTION anchor at body position {P}` entry.
- MUST expose every emitted message as a `public const string` (or string prefix constant) for `DiagnosticWriter` consumption.
- MUST satisfy INV-01: multiset of non-empty trimmed body texts is identical before and after, in every test.
- MUST NOT throw on any input.
</requirements>

## Subtasks
- [x] 4.1 Create `PromoteSectionsRule.cs` with class scaffold, message constants, and `Apply` method skeleton.
- [x] 4.2 Implement anchor lookup and the anchor-missing skip path with `[WARN]` emission.
- [x] 4.3 Implement the per-paragraph iteration with table-filter, context-skip-list filter, and predicate dispatch (section vs sub-section).
- [x] 4.4 Implement the property mutation: ensure `ParagraphProperties` exists, set `<w:jc>` to `center`; ensure each run has `RunProperties`, set `<w:sz>` and `<w:szCs>`.
- [x] 4.5 Aggregate the counts (sections, sub-sections, skipped-inside-tables, skipped-before-anchor) and emit the `[INFO]` summary.
- [x] 4.6 Extend `Phase3DocxFixtureBuilder` with helpers to construct a body containing: anchor `INTRODUCTION`, multiple section candidates after the anchor, multiple sub-section candidates after the anchor, candidates inside `<w:tbl>` (must be skipped), candidates before the anchor (must be skipped), and paragraphs referenced by `FormattingContext` (must be skipped).
- [x] 4.7 Add `PromoteSectionsRuleTests.cs` covering all skip and mutation paths plus INV-01.

## Implementation Details
New files:
- `DocFormatter.Core/Rules/PromoteSectionsRule.cs`.
- `DocFormatter.Tests/PromoteSectionsRuleTests.cs`.

Modified files:
- `DocFormatter.Tests/Fixtures/Phase3/Phase3DocxFixtureBuilder.cs` — add section/sub-section content construction helpers.

Refer to TechSpec section "Implementation Design" for the property-mutation strategy (mutate, never replace), to PRD "Section and sub-section promotion" for the exact mutation specification (jc=center, sz=32 for section, sz=28 for sub-section), and to ADR-005's existing precedent in `RewriteHeaderMvpRule.CreateBaseRunProperties` for setting both `<w:sz>` and `<w:szCs>`. The skip-list reference equality check uses `object.ReferenceEquals` against `FormattingContext.SectionParagraph` / `TitleParagraph` / `DoiParagraph`.

DI registration is intentionally OUT OF SCOPE for this task; it lives in task_07.

### Relevant Files
- `DocFormatter.Core/Rules/BodySectionDetector.cs` — Provides `FindIntroductionAnchor`, `IsSection`, `IsSubsection`, `IsInsideTable` (implemented in task_02).
- `DocFormatter.Core/Pipeline/FormattingContext.cs` — Provides `SectionParagraph`, `TitleParagraph`, `DoiParagraph` for the skip-list.
- `DocFormatter.Core/Rules/RewriteHeaderMvpRule.cs` — Reference pattern for setting both `<w:sz>` and `<w:szCs>` and for the "mutate, never replace" property strategy.
- `DocFormatter.Core/Rules/ApplyHeaderAlignmentRule.cs` — Reference pattern for setting `<w:jc>` on a paragraph (preserves other `pPr` children).
- `DocFormatter.Tests/Fixtures/Phase3/Phase3DocxFixtureBuilder.cs` — Extended with body-section content helpers.

### Dependent Files
- `DocFormatter.Core/Reporting/DiagnosticWriter.cs` (task_06) — Will reconstruct `DiagnosticSectionPromotion` from this rule's report entries.
- `DocFormatter.Cli/CliApp.cs` (task_07) — Will register `PromoteSectionsRule` after `MoveHistoryRule`.

### Related ADRs
- [ADR-001: Two discrete Optional rules over a single combined rule](../adrs/adr-001-two-discrete-rules.md) — This task creates the second of the two rules mandated by ADR-001.
- [ADR-002: Strict content preservation invariant (INV-01)](../adrs/adr-002-content-preservation-invariant.md) — Confines mutations to `<w:jc>` and `<w:sz>`/`<w:szCs>`; tests must assert multiset preservation.
- [ADR-003: Discard font size from detection predicate](../adrs/adr-003-discard-font-size-from-detection.md) — Confirms detection ignores the source `<w:sz>` (the rule reformats regardless of source size).
- [ADR-004: `INTRODUCTION` as detection anchor](../adrs/adr-004-introduction-as-detection-anchor.md) — Defines the lower bound of the detection scope.
- [ADR-005: Resolve `<w:b>` via OOXML cascade chain](../adrs/adr-005-bold-cascade-resolver.md) — Section/sub-section predicates depend on cascade-resolved bold.

## Deliverables
- `PromoteSectionsRule` class with `Apply` implementation and all message constants.
- Extended `Phase3DocxFixtureBuilder` with section/sub-section/table/context-skip helpers.
- `PromoteSectionsRuleTests` with all test cases listed in `## Tests`, including the INV-01 multiset assertion in EVERY test.
- Unit tests with 80%+ coverage **(REQUIRED)**.
- Integration tests verifying the rule on synthetic fixtures combining sections, sub-sections, table content, and Phase 1+2 paragraphs **(REQUIRED)**.

## Tests
- Unit tests:
  - [ ] Happy path: a section paragraph after the anchor receives `<w:jc w:val="center"/>` and every run gets `<w:sz w:val="32"/>` and `<w:szCs w:val="32"/>`.
  - [ ] Happy path: a sub-section paragraph after the anchor receives `<w:jc w:val="center"/>` and every run gets `<w:sz w:val="28"/>` and `<w:szCs w:val="28"/>`.
  - [ ] The `INTRODUCTION` anchor itself IS reformatted as a section (per ADR-004 implementation note).
  - [ ] Paragraphs before the anchor are untouched (snapshot `<w:jc>` and `<w:sz>` before/after; assert unchanged).
  - [ ] Paragraphs inside `<w:tbl>` are untouched even when their text matches the section predicate.
  - [ ] Paragraphs referenced by `FormattingContext.SectionParagraph` / `TitleParagraph` / `DoiParagraph` are untouched even when above the anchor.
  - [ ] A paragraph referenced by `FormattingContext.SectionParagraph` AFTER the anchor would still be skipped (defence in depth).
  - [ ] Anchor missing: emits `[WARN] AnchorMissingMessage`; no paragraph is mutated.
  - [ ] Idempotent: running the rule twice produces byte-identical OOXML on the affected paragraphs.
  - [ ] A run with NO existing `RunProperties` element receives a newly-created `RunProperties` containing `<w:sz>` and `<w:szCs>`.
  - [ ] A run with EXISTING `RunProperties` (e.g., already declares `<w:b/>`) has its `<w:sz>`/`<w:szCs>` set or replaced without losing other properties (`<w:b/>` preserved).
  - [ ] A paragraph with EXISTING `ParagraphProperties` containing other children (e.g., `<w:pageBreakBefore/>`) has only `<w:jc>` mutated; other children preserved.
  - [ ] A paragraph with NO existing `ParagraphProperties` receives a newly-created `ParagraphProperties` containing `<w:jc w:val="center"/>`.
  - [ ] The `[INFO]` summary message correctly reports the counts (e.g., `promoted 5 sections (16pt center) and 3 sub-sections (14pt center)`).
  - [ ] INV-01 assertion: in every test above, multiset of non-empty trimmed body texts is preserved.
- Integration tests:
  - [ ] Construct a synthetic `WordprocessingDocument` with multiple sections (`INTRODUCTION`, `MATERIAL AND METHODS`, `RESULTS`, `REFERENCES`), multiple sub-sections, table content, and Phase 1+2 front-matter; run `PromoteSectionsRule.Apply`; assert correct mutation on body paragraphs and zero mutation on table/front-matter paragraphs.
- Test coverage target: >=80%
- All tests must pass

## Success Criteria
- All tests passing
- Test coverage >=80%
- Re-applying the rule to its own output produces byte-identical OOXML (idempotency)
- Every test case asserts INV-01
- The rule never throws and never mutates outside the documented mutation surface (`<w:jc>`, `<w:sz>`, `<w:szCs>`)
- All emitted messages are `public const string` and consumed by reference downstream
