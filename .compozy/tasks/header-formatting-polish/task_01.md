---
status: completed
title: Extend FormattingContext with Phase 2 cross-rule state
type: backend
complexity: low
dependencies: []
---

# Task 01: Extend FormattingContext with Phase 2 cross-rule state

## Overview
Add the nullable cross-rule state required by ADR-001 and ADR-003 to `FormattingContext`: paragraph references that flow between rules (DOI/section/title/author-block-end/corresponding-affiliation) and corresponding-author scalars (email, ORCID, author index). All four Phase 2 rules read or write these properties; this task makes them addressable without changing any existing MVP behavior.

<critical>
- ALWAYS READ the PRD and TechSpec before starting
- REFERENCE TECHSPEC for implementation details — do not duplicate here
- FOCUS ON "WHAT" — describe what needs to be accomplished, not how
- MINIMIZE CODE — show code only to illustrate current structure or problem areas
- TESTS REQUIRED — every task MUST include tests in deliverables
</critical>

<requirements>
- MUST add the seven new nullable properties listed in TechSpec "Core Interfaces": `DoiParagraph`, `SectionParagraph`, `TitleParagraph`, `AuthorBlockEndParagraph`, `CorrespondingAffiliationParagraph`, `CorrespondingEmail`, `CorrespondingOrcid`, plus `CorrespondingAuthorIndex` (int?).
- MUST keep all existing MVP-set fields and their behavior untouched (`Doi`, `ElocationId`, `ArticleTitle`, `Authors`, `AuthorParagraphs`).
- New properties MUST default to `null` (no constructor arguments, no list initialization needed for these).
- MUST NOT introduce a runtime initializer that hides the publishing rule's responsibility to set the reference.
- SHOULD document the "do not delete a paragraph published in context" invariant inline (see TechSpec Impact Analysis).
</requirements>

## Subtasks
- [x] 1.1 Add `Paragraph?` properties for `DoiParagraph`, `SectionParagraph`, `TitleParagraph`, `AuthorBlockEndParagraph`, `CorrespondingAffiliationParagraph`.
- [x] 1.2 Add `string? CorrespondingEmail`, `string? CorrespondingOrcid`, `int? CorrespondingAuthorIndex`.
- [x] 1.3 Add a short comment describing the invariant that publishing rules must not remove a paragraph stored in context.
- [x] 1.4 Add a unit-test fixture covering the new properties (defaults, idempotent assignment).

## Implementation Details
File `DocFormatter.Core/Pipeline/FormattingContext.cs` is the only source change. Properties are pure auto-properties; no behavior. See TechSpec section "Implementation Design → Core Interfaces" for the property list.

### Relevant Files
- `DocFormatter.Core/Pipeline/FormattingContext.cs` — class to extend.
- `DocFormatter.Core/Pipeline/IFormattingRule.cs` — confirms the contract that consumes the context.

### Dependent Files
- `DocFormatter.Core/Rules/ParseHeaderLinesRule.cs` — task_03 will write `SectionParagraph`/`TitleParagraph`.
- `DocFormatter.Core/Rules/RewriteHeaderMvpRule.cs` — task_04 will write `DoiParagraph`/`AuthorBlockEndParagraph`.
- `DocFormatter.Core/Rules/ApplyHeaderAlignmentRule.cs` — task_05 will read three paragraph references (file does not exist yet).
- `DocFormatter.Core/Rules/EnsureAuthorBlockSpacingRule.cs` — task_06 will read `AuthorBlockEndParagraph` (file does not exist yet).
- `DocFormatter.Core/Rules/ExtractCorrespondingAuthorRule.cs` — task_07 will write `Corresponding*` properties (file does not exist yet).
- `DocFormatter.Core/Rules/RewriteAbstractRule.cs` — task_08 will read `CorrespondingEmail`/`CorrespondingAuthorIndex` (file does not exist yet).
- `DocFormatter.Tests/` — new `FormattingContextTests.cs` covering defaults; existing tests in `FormattingPipelineTests.cs` and rule tests must still compile.

### Related ADRs
- [ADR-001: Four discrete Optional rules](../adrs/adr-001-four-discrete-rules.md) — defines the cross-rule state surface.
- [ADR-003: Marker tokenization and email regex](../adrs/adr-003-corresponding-author-tokenization.md) — defines the corresponding-author scalars.

## Deliverables
- `FormattingContext` extended with eight new properties (five `Paragraph?` references + three corresponding-author scalars).
- Inline comment documenting the publish/no-delete invariant.
- Unit tests with 80%+ coverage **(REQUIRED)**.
- New `FormattingContextTests` xUnit file or equivalent assertions in an existing test file **(REQUIRED)**.

## Tests
- Unit tests:
  - [x] All new nullable properties default to `null` on a freshly constructed `FormattingContext`.
  - [x] Each new property round-trips a value (set → get returns the same instance/scalar).
  - [x] Setting a paragraph reference does not mutate `Authors`, `AuthorParagraphs`, or any MVP scalar.
  - [x] `CorrespondingAuthorIndex` accepts any non-negative integer; the type is `int?`.
- Integration tests:
  - [x] Existing `FormattingPipelineTests` continues to pass with the extended type (no behavior change for MVP rules).
- Test coverage target: >=80%
- All tests must pass

## Success Criteria
- All tests passing
- Test coverage >=80%
- `FormattingContext` exposes the eight new properties matching TechSpec naming exactly.
- No regression in existing MVP rule tests.
