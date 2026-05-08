---
status: completed
title: Create BodySectionDetector skeleton with bold cascade resolver
type: backend
complexity: high
dependencies: []
---

# Task 01: Create BodySectionDetector skeleton with bold cascade resolver

## Overview
Introduce the new internal static helper `BodySectionDetector` that centralizes Phase 3 detection logic, mirroring the existing `HeaderParagraphLocator` pattern. This task creates the file with method skeletons and implements the only non-trivial piece — `IsBoldEffective`, which resolves `<w:b>` through the OOXML cascade chain (run direct → paragraph rPr → `pStyle` → `basedOn`) per ADR-005. Without this, 2 of 11 production articles whose section headings derive bold via `pStyle` would lose all Phase 3 formatting.

<critical>
- ALWAYS READ the PRD and TechSpec before starting
- REFERENCE TECHSPEC for implementation details — do not duplicate here
- FOCUS ON "WHAT" — describe what needs to be accomplished, not how
- MINIMIZE CODE — show code only to illustrate current structure or problem areas
- TESTS REQUIRED — every task MUST include tests in deliverables
</critical>

<requirements>
- MUST create `BodySectionDetector` as `internal static class` under namespace `DocFormatter.Core.Rules`, mirroring `HeaderParagraphLocator` placement and visibility.
- MUST expose method skeletons for `FindIntroductionAnchor`, `IsSection`, `IsSubsection`, `IsInsideTable`, and `internal static IsBoldEffective(Run run, Paragraph paragraph, MainDocumentPart? mainPart)`. Skeletons (other than `IsBoldEffective`) MUST return `false`/`null` and will be implemented in task_02.
- MUST fully implement `IsBoldEffective` with cascade resolution in this order: (1) run direct `rPr/b`; (2) paragraph default `pPr/rPr/b`; (3) paragraph style chain via `pStyle` walking `basedOn` ancestors, checking both the style's `<w:rPr><w:b/>` and the style's `<w:pPr><w:rPr><w:b/>`. MUST stop at no-bold-found and return `false`.
- MUST honor OOXML bold semantics: element absent ⇒ `false`; element present with `val` absent or `val` not in `{"0", "false"}` ⇒ `true`; `val` in `{"0", "false"}` ⇒ explicit `false` (an override that disables inherited bold).
- MUST NOT consult `<w:docDefaults>` (per ADR-005 explicit decision).
- MUST guard the cascade walker with: (a) a `HashSet<string>` of visited style IDs (cycle protection); (b) a hard depth limit of 10 hops; (c) graceful degradation when `mainPart.StyleDefinitionsPart` is null or a referenced `styleId` is not found — return `false`, never throw.
- MUST NOT throw on any input; all error paths return `false`.
- SHOULD short-circuit on first hit (e.g., when run-direct bold is set, do not walk paragraph or styles).
- MUST introduce the `Phase3DocxFixtureBuilder` test fixture under `DocFormatter.Tests/Fixtures/Phase3/` with sufficient surface to construct OOXML documents that exercise each of the six cascade patterns enumerated in the tests.
</requirements>

## Subtasks
- [x] 1.1 Create `BodySectionDetector.cs` with class scaffold, namespace, and method signatures matching the TechSpec "Core Interfaces" section.
- [x] 1.2 Implement `IsBoldEffective` with the four-layer cascade, cycle protection, and depth limit.
- [x] 1.3 Create `Phase3DocxFixtureBuilder` test fixture with helpers to construct paragraphs/runs/styles for each cascade pattern (run direct bold, paragraph rPr bold, single-level pStyle, two-level pStyle via basedOn, cyclic basedOn chain, missing styles part).
- [x] 1.4 Add `BodySectionDetectorTests.cs` covering each cascade pattern plus the `<w:b w:val="false"/>` explicit override.
- [x] 1.5 Verify all unit tests pass and `BodySectionDetector` is accessible from `DocFormatter.Core.Rules` for downstream tasks (no public surface change).

## Implementation Details
Create the helper file alongside other rule helpers:
- New file: `DocFormatter.Core/Rules/BodySectionDetector.cs`.
- New test file: `DocFormatter.Tests/BodySectionDetectorTests.cs`.
- New fixture builder: `DocFormatter.Tests/Fixtures/Phase3/Phase3DocxFixtureBuilder.cs`.

Refer to TechSpec section "Core Interfaces" for the exact method signatures of `BodySectionDetector` and to the "Implementation Design" notes on cycle protection and depth limit. Follow `HeaderParagraphLocator` conventions for visibility (`internal static`), namespace, and file organization. The fixture builder should be additive — later tasks will extend it; this task only adds the surface required to drive cascade tests.

### Relevant Files
- `DocFormatter.Core/Rules/HeaderParagraphLocator.cs` — Reference pattern for an internal static helper class consumed by rules.
- `DocFormatter.Tests/Fixtures/Phase2/Phase2DocxFixtureBuilder.cs` — Reference pattern for an OOXML fixture builder; mirror its construction style for Phase 3.
- `DocFormatter.Tests/Fixtures/Authors/AuthorsParagraphFactory.cs` — Reference for building `Run`/`Text`/`RunProperties` helper methods (e.g., `TextRun`, `SuperscriptRun`).
- `DocFormatter.Core/Pipeline/IFormattingRule.cs` — Confirms downstream consumers (`MoveHistoryRule`, `PromoteSectionsRule`) will pass `WordprocessingDocument.MainDocumentPart`, so `IsBoldEffective` must accept a nullable `MainDocumentPart`.

### Dependent Files
- `DocFormatter.Core/Rules/MoveHistoryRule.cs` (created in task_03) — Will consume `BodySectionDetector.FindIntroductionAnchor` once task_02 implements it; transitively depends on this skeleton existing.
- `DocFormatter.Core/Rules/PromoteSectionsRule.cs` (created in task_04) — Will consume `IsSection`, `IsSubsection`, `IsInsideTable`, and `IsBoldEffective`.
- `DocFormatter.Tests/Fixtures/Phase3/Phase3DocxFixtureBuilder.cs` — Tasks 02–04 and 07 will extend this same builder rather than create new ones.

### Related ADRs
- [ADR-005: Resolve `<w:b>` via OOXML cascade chain](../adrs/adr-005-bold-cascade-resolver.md) — This task is the implementation of the cascade walker mandated by ADR-005, including cycle protection and the explicit `docDefaults` exclusion.
- [ADR-003: Discard font size from detection predicate](../adrs/adr-003-discard-font-size-from-detection.md) — Confirms the helper does not need to resolve `<w:sz>`; this task does not implement size resolution.

## Deliverables
- `BodySectionDetector` class with method skeletons and a fully implemented `IsBoldEffective` cascade walker.
- `Phase3DocxFixtureBuilder` test fixture with cascade-pattern construction helpers.
- `BodySectionDetectorTests` covering each cascade pattern listed in `## Tests`.
- Unit tests with 80%+ coverage on `IsBoldEffective` **(REQUIRED)**.
- Integration test for the cascade walker against synthetic OOXML documents **(REQUIRED)**.

## Tests
- Unit tests:
  - [x] `IsBoldEffective` returns `true` when run has direct `<w:b/>` set.
  - [x] `IsBoldEffective` returns `true` when run has no bold but paragraph `<w:pPr><w:rPr><w:b/>` is set.
  - [x] `IsBoldEffective` returns `true` when bold is inherited via single-level `pStyle` (style declares `<w:rPr><w:b/>`).
  - [x] `IsBoldEffective` returns `true` when bold is inherited via two-level `pStyle` chain (`basedOn` ancestor declares `<w:b/>`).
  - [x] `IsBoldEffective` returns `true` when style declares bold via `<w:pPr><w:rPr><w:b/>` (paragraph-default layer of the style).
  - [x] `IsBoldEffective` returns `false` when run, paragraph, and style chain all lack bold.
  - [x] `IsBoldEffective` returns `false` for a cyclic `basedOn` chain (style A → B → A) without infinite-looping.
  - [x] `IsBoldEffective` returns `false` (does not throw) when `mainPart` is null.
  - [x] `IsBoldEffective` returns `false` when `pStyle` references a styleId not present in the styles part.
  - [x] `IsBoldEffective` respects explicit override: `<w:b w:val="false"/>` on the run returns `false` even when paragraph or style declares bold.
  - [x] `IsBoldEffective` respects `<w:b w:val="0"/>` and `<w:b w:val="true"/>` and `<w:b w:val="1"/>` per OOXML semantics.
  - [x] `IsBoldEffective` aborts the walk after 10 hops on a pathologically deep chain and returns `false`.
- Integration tests:
  - [x] Construct a synthetic `WordprocessingDocument` exercising cascade pattern (single-level pStyle inheritance), run `IsBoldEffective` against each text run, and verify the bold-resolution matches the OOXML cascade semantics.
- Test coverage target: >=80%
- All tests must pass

## Success Criteria
- All tests passing
- Test coverage >=80%
- `BodySectionDetector.IsBoldEffective` resolves bold correctly for all six cascade patterns (run direct, paragraph rPr, 1-level pStyle, 2-level pStyle, cyclic chain, missing styles part) without throwing on any input
- The skeleton methods (`FindIntroductionAnchor`, `IsSection`, `IsSubsection`, `IsInsideTable`) compile and return `false`/`null`, ready for task_02 to implement
- `Phase3DocxFixtureBuilder` is extensible — task_02 can add predicate fixtures, task_03 can add history-block fixtures, task_04 can add section-content fixtures, all without rewriting existing helpers
