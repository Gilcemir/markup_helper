---
status: completed
title: Extend FormattingOptions with email and corresponding-author regexes
type: backend
complexity: low
dependencies: []
---

# Task 02: Extend FormattingOptions with email and corresponding-author regexes

## Overview
Add three compiled regex options that ADR-003 defines: an ASCII email pattern, the `* E-mail:` marker pattern, and a typo-tolerant pre-existing "corresponding author" label pattern. These options back `ExtractCorrespondingAuthorRule` (task_07) and `RewriteAbstractRule` (task_08). The MVP `DoiRegex`, `OrcidIdRegex`, and `ElocationRegex` patterns and `[GeneratedRegex]` style serve as the precedent.

<critical>
- ALWAYS READ the PRD and TechSpec before starting
- REFERENCE TECHSPEC for implementation details — do not duplicate here
- FOCUS ON "WHAT" — describe what needs to be accomplished, not how
- MINIMIZE CODE — show code only to illustrate current structure or problem areas
- TESTS REQUIRED — every task MUST include tests in deliverables
</critical>

<requirements>
- MUST add `EmailRegex`, `CorrespondingMarkerRegex`, and `CorrespondingAuthorLabelRegex` to `FormattingOptions` using `[GeneratedRegex]` and the literals captured in TechSpec "Implementation Design → Core Interfaces" (and ADR-003).
- All three regexes MUST be compiled with `RegexOptions.IgnoreCase | RegexOptions.CultureInvariant` to match existing patterns.
- MUST NOT change any existing MVP option (`DoiRegex`, `OrcidIdRegex`, `ElocationRegex`, `OrcidUrlMarker`, `AuthorSeparators`, `AbstractMarkers`, `DoiUrlPrefixes`).
- The class MUST remain `partial` (already required by `[GeneratedRegex]`).
- SHOULD reuse the existing private static partial pattern (one method per regex) so future contributors recognize the precedent.
</requirements>

## Subtasks
- [x] 2.1 Add the three `Regex` properties exposing the new patterns.
- [x] 2.2 Add the three private `[GeneratedRegex(...)]` partial methods producing the compiled regexes.
- [x] 2.3 Add unit tests covering positive and negative matches for each regex against representative strings drawn from the corpus.
- [x] 2.4 Verify backward compatibility by running existing `FormattingOptionsTests`.

## Implementation Details
File `DocFormatter.Core/Options/FormattingOptions.cs` is the only source change. The exact regex literals are pinned in ADR-003; do not invent new shapes. See TechSpec "Core Interfaces" for the property declarations and TechSpec/ADR-003 for the rationale (pragmatic ASCII email, combined `* E-mail:` token, permissive `c[oa]rresp...au...` label).

### Relevant Files
- `DocFormatter.Core/Options/FormattingOptions.cs` — class to extend.
- `DocFormatter.Tests/FormattingOptionsTests.cs` — existing test patterns to follow.

### Dependent Files
- `DocFormatter.Core/Rules/ExtractCorrespondingAuthorRule.cs` — task_07 consumes `EmailRegex` and `CorrespondingMarkerRegex` (file does not exist yet).
- `DocFormatter.Core/Rules/RewriteAbstractRule.cs` — task_08 consumes `EmailRegex` and `CorrespondingAuthorLabelRegex` (file does not exist yet).

### Related ADRs
- [ADR-003: Marker tokenization and email regex](../adrs/adr-003-corresponding-author-tokenization.md) — defines the literal regex shapes and rationale.

## Deliverables
- `FormattingOptions` exposing `EmailRegex`, `CorrespondingMarkerRegex`, `CorrespondingAuthorLabelRegex`.
- Three `[GeneratedRegex]` partial methods.
- Unit tests with 80%+ coverage **(REQUIRED)**.
- Match/no-match cases for each regex covering corpus-representative strings **(REQUIRED)**.

## Tests
- Unit tests:
  - [x] `EmailRegex` matches `foo@y.edu`, `first.last+tag@u-aberta.pt`, `Maria.Silva@usp.br`.
  - [x] `EmailRegex` does NOT match `foo@bar` (no TLD), `foo@.edu` (empty domain head), `@y.edu` (no local part).
  - [x] `CorrespondingMarkerRegex` matches `* E-mail:`, `*  E-mail :`, `*Email:`, `* email :` (case-insensitive, optional hyphen, optional spaces).
  - [x] `CorrespondingMarkerRegex` does NOT match a bare `*` on its own, nor `E-mail:` without the leading asterisk.
  - [x] `CorrespondingAuthorLabelRegex` matches `Corresponding Author:`, `coresponding author -`, `Correspondign Author`, `Correspondent Autor`, `corresponding author —`.
  - [x] `CorrespondingAuthorLabelRegex` does NOT match `Correspondence:` (second word does not start with `au`).
  - [x] All three regex properties return the same instance on subsequent reads (compiled once).
- Integration tests:
  - [x] Existing `FormattingOptionsTests` keeps passing.
- Test coverage target: >=80%
- All tests must pass

## Success Criteria
- All tests passing
- Test coverage >=80%
- The three new properties are accessible as `Regex` instances and conform to ADR-003 literals exactly.
- No regression in MVP option behavior.
