---
status: completed
title: Domain models and FormattingOptions constants
type: backend
complexity: low
dependencies:
    - task_02
---

# Task 3: Domain models and FormattingOptions constants

## Overview
Implement the `Author` domain record and the `FormattingOptions` class that holds every regex, separator, and marker the rules consume. `FormattingOptions` is the single seam future multi-profile work depends on; for the MVP it ships with hardcoded constants registered as a singleton in DI.

<critical>
- ALWAYS READ the PRD and TechSpec before starting
- REFERENCE TECHSPEC for implementation details — do not duplicate here
- FOCUS ON "WHAT" — describe what needs to be accomplished, not how
- MINIMIZE CODE — show code only to illustrate current structure or problem areas
- TESTS REQUIRED — every task MUST include tests in deliverables
</critical>

<requirements>
- 1. `Author` MUST be a positional record with `Name` (string), `AffiliationLabels` (`IReadOnlyList<string>`), and `OrcidId` (`string?`).
- 2. `FormattingOptions` MUST expose `DoiRegex`, `OrcidIdRegex`, `OrcidUrlMarker`, `AuthorSeparators`, and `AbstractMarkers` per TechSpec "Data Models".
- 3. `OrcidIdRegex` MUST be the pattern `\b\d{4}-\d{4}-\d{4}-\d{3}[\dX]\b` (case-insensitive on the trailing X).
- 4. `OrcidUrlMarker` MUST be the substring `"orcid.org"` so both `https://orcid.org/...` and pseudo-targets that include the substring are caught (per ADR-003).
- 5. `AuthorSeparators` MUST be `[", ", " and "]` in that order.
- 6. `AbstractMarkers` MUST be the case-insensitive set `["abstract", "resumo"]`.
- 7. `FormattingOptions` MUST be immutable after construction (init-only properties or private setters).
</requirements>

## Subtasks
- [x] 3.1 Create `DocFormatter.Core/Models/Author.cs` as a sealed positional record.
- [x] 3.2 Create `DocFormatter.Core/Options/FormattingOptions.cs` with the values listed in TechSpec "Data Models".
- [x] 3.3 Compile-precompute regex instances in `FormattingOptions` (use `[GeneratedRegex]` for `DoiRegex` and `OrcidIdRegex`).
- [x] 3.4 Add unit tests covering the regex patterns against representative input strings.

## Implementation Details
Files are new under `DocFormatter.Core/Models/` and `DocFormatter.Core/Options/`. The `FormattingContext.Authors` collection introduced in task_02 will store instances of this record. See TechSpec "Data Models" for the full constant list and ADR-003 for the ORCID regex rationale.

### Relevant Files
- `.compozy/tasks/header-metadata-extraction/_techspec.md` — "Data Models" table
- `.compozy/tasks/header-metadata-extraction/adrs/adr-003.md` — ORCID regex justification

### Dependent Files
- `DocFormatter.Core/Models/Author.cs` (new)
- `DocFormatter.Core/Options/FormattingOptions.cs` (new)
- `DocFormatter.Core/Pipeline/FormattingContext.cs` (modified to import the namespace)
- `DocFormatter.Tests/FormattingOptionsTests.cs` (new)

### Related ADRs
- [ADR-003: ORCID extraction](adrs/adr-003.md) — locks the ORCID regex and URL marker
- [ADR-001: Esqueleto alinhado ao spec](adrs/adr-001.md) — `FormattingOptions` is the future multi-profile seam

## Deliverables
- `Author` record and `FormattingOptions` class
- Unit tests for the regex patterns
- Unit tests with 80%+ coverage **(REQUIRED)**
- Integration tests for [option-driven rule wiring] **(REQUIRED)**

## Tests
- Unit tests:
  - [x] `DoiRegex` matches `"10.1234/abc.def"` and `"10.12345/path-with_special:chars/123"`.
  - [x] `DoiRegex` does NOT match `"abc/10.1234"` or `"10.1/short"`.
  - [x] `OrcidIdRegex` matches `"0000-0002-1825-0097"` and `"0000-0001-2345-678X"`.
  - [x] `OrcidIdRegex` does NOT match `"0000-0002-1825-009"` (missing last digit) or `"0000-0002-18250097"` (missing hyphen).
  - [x] `Author` record equality is value-based: two records with same name, labels, and ORCID compare equal.
  - [x] `Author` with `OrcidId == null` is allowed and prints without the ORCID segment when ToString'd.
- Integration tests:
  - [x] `FormattingOptions` registered as a singleton via `IServiceCollection.AddSingleton` resolves the same instance from two different scopes.
- Test coverage target: >=80%
- All tests must pass

## Success Criteria
- All tests passing
- Test coverage >=80%
- All regex patterns precompiled via `[GeneratedRegex]` (or equivalent) — no per-call `new Regex()`
- `FormattingOptions` is immutable after construction
