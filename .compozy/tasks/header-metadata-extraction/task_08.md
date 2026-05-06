---
status: completed
title: ParseAuthorsRule with comprehensive xUnit tests
type: backend
complexity: high
dependencies:
    - task_03
    - task_07
---

# Task 8: ParseAuthorsRule with comprehensive xUnit tests

## Overview
Implement the rule that splits the authors paragraph into individual `Author` records using `FormattingOptions.AuthorSeparators`, attaches superscript affiliation labels per author, merges the staged ORCID IDs from task_07, and emits confidence-tagged `[WARN]` entries when the parse is uncertain. This is the highest-risk rule in the MVP per the PRD; it carries all of the project's automated unit-test coverage (PRD testing scope: option B).

<critical>
- ALWAYS READ the PRD and TechSpec before starting
- REFERENCE TECHSPEC for implementation details — do not duplicate here
- FOCUS ON "WHAT" — describe what needs to be accomplished, not how
- MINIMIZE CODE — show code only to illustrate current structure or problem areas
- TESTS REQUIRED — every task MUST include tests in deliverables
</critical>

<requirements>
- 1. The rule MUST locate the authors paragraph via `HeaderParagraphLocator` introduced in task_07.
- 2. The rule MUST walk the paragraph's runs in document order, classifying each run as text, superscript (vertical alignment = `superscript`), or replaced-orcid (a plain run produced by task_07 carrying an ORCID ID).
- 3. The rule MUST split into authors using `AuthorSeparators` applied to the concatenated text stream; superscript runs encountered between separators MUST be attached to the current author's `AffiliationLabels`.
- 4. ORCID IDs staged in `OrcidStaging` MUST be attached to the author whose run range covers the staging key.
- 5. The rule MUST emit one `Author` record per detected author into `FormattingContext.Authors`.
- 6. The rule MUST emit `[WARN]` entries when: (a) any name fragment looks suspicious (matches the regex `,\s*(Jr|Sr|II|III|IV)\.?\s*$` after splitting), (b) the count of distinct superscript labels does not match the count of detected affiliation paragraphs (note: affiliation parsing is out of scope for the MVP, so this check is deferred — log `[INFO]` only that the count is N), or (c) a name fragment is empty or contains no alphabetic characters.
- 7. The rule MUST set per-author `confidence` (used downstream by the diagnostic JSON in task_12): `high` when the parse encounters none of the suspicions above, `low` otherwise.
- 8. The rule severity MUST be `Optional`; failures do not abort the pipeline. If the authors paragraph is missing entirely, the rule MUST log `[WARN]` and leave `Authors` empty.
</requirements>

## Subtasks
- [x] 8.1 Create `DocFormatter.Core/Rules/ParseAuthorsRule.cs` implementing `IFormattingRule` with `Severity=Optional`.
- [x] 8.2 Implement the run-walking tokenizer that produces a stream of `(text|superscript|orcid)` tokens.
- [x] 8.3 Implement the separator-driven split that produces one `Author` record per name, attaching superscripts and ORCID IDs.
- [x] 8.4 Implement the confidence/warning logic for the four uncertainty cases listed in requirement 6.
- [x] 8.5 Add the eight unit tests listed below in `DocFormatter.Tests/ParseAuthorsRuleTests.cs`.

## Implementation Details
File is new under `DocFormatter.Core/Rules/`. See TechSpec "Rules" rule #4 and "Testing Approach" for the canonical eight scenarios. The rule consumes `OrcidStaging` from the `FormattingContext` (internal to Core, established by task_07). Test fixtures use programmatic `WordprocessingDocument` construction in-memory rather than binary `.docx` fixtures on disk.

### Relevant Files
- `.compozy/tasks/header-metadata-extraction/_techspec.md` — "Rules" rule #4 and "Testing Approach"
- `.compozy/tasks/header-metadata-extraction/_prd.md` — Core Features #4 (parsing semantics)
- `instructions.md` — original spec's `ParseAuthorsRule` description

### Dependent Files
- `DocFormatter.Core/Rules/ParseAuthorsRule.cs` (new)
- `DocFormatter.Tests/ParseAuthorsRuleTests.cs` (new)
- `DocFormatter.Tests/Fixtures/Authors/AuthorsParagraphFactory.cs` (new helper that builds in-memory `WordprocessingDocument` instances for each test case)

### Related ADRs
- [ADR-003: ORCID extraction](adrs/adr-003.md) — defines the ORCID-attached output format
- [ADR-001: Esqueleto alinhado ao spec](adrs/adr-001.md)

## Deliverables
- `ParseAuthorsRule` implementation
- `AuthorsParagraphFactory` test helper
- Eight xUnit unit tests covering every named scenario in the TechSpec
- Unit tests with 80%+ coverage **(REQUIRED)**
- Integration tests for [pipeline integration with task_07 staging] **(REQUIRED)**

## Tests
- Unit tests:
  - [x] **Single author, no ORCID, one label**: paragraph "Maria Silva¹" → `Authors=[{Name="Maria Silva", AffiliationLabels=["1"], OrcidId=null, confidence=high}]`.
  - [x] **Multiple authors, comma-separated**: paragraph "A¹, B², C¹" → three authors with respective single-label lists, all `confidence=high`.
  - [x] **Trailing " and " separator**: paragraph "A¹, B² and C³" → three authors split correctly; the " and " separator does not appear in any author's name.
  - [x] **ORCID extracted by task_07**: paragraph "José Silva¹" with `OrcidStaging[runIndex]="0000-0002-1825-0097"` → `Authors[0].OrcidId="0000-0002-1825-0097"`, `confidence=high`.
  - [x] **ORCID via file-URL marker (user's edge case)**: same as above but the staging entry came from a `file:///` URL — handled identically because staging is already a string ID.
  - [x] **Multiple affiliation labels**: paragraph "Jane Doe¹,²" (where ¹,² is one superscript run with content "1,2") → `AffiliationLabels=["1","2"]`.
  - [x] **Suspicious comma in name**: paragraph "Smith, Jr.¹, Jane Doe²" → first author detected as "Smith" (split caused incorrect break) with `confidence=low` and a `[WARN]` entry naming the suspicion.
  - [x] **Authors paragraph missing**: locator returns null → `Authors=[]`, one `[WARN]` entry "authors paragraph not found".
- Integration tests:
  - [x] Pipeline run with `ExtractTopTableRule` → `ParseHeaderLinesRule` → `ExtractOrcidLinksRule` → `ParseAuthorsRule`: when the authors paragraph contains one ORCID link, the resulting `Author` record carries the extracted ID and the original hyperlink no longer exists in the document.
- Test coverage target: >=80%
- All tests must pass

## Success Criteria
- All tests passing
- Test coverage >=80% on `ParseAuthorsRule` and `AuthorsParagraphFactory`
- All eight TechSpec scenarios covered as named test methods
- The "Smith, Jr." case produces a `low` confidence and a `[WARN]` entry without crashing
