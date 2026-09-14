---
status: completed
title: Author-keyed CREDIT grammar with `;`/`,` separators and initials-block lookback
type: bugfix
complexity: medium
dependencies:
  - task_01
---

# Task 2: Author-keyed CREDIT grammar with `;`/`,` separators and initials-block lookback

## Overview
Rewrite `TryParseAuthorKeyed` so that `;` and `,` separate terms and `;`, `,` and `.` separate author entries, using a shape-based lookback to attach initials-block pieces to the entry opened by the next `KEY:` piece. This parses 8 of the 15 v26n3 statements that today fall to Prose or produce junk terms, and removes the false "free prose" label from 5501, 5642 and 5412.

<critical>
- ALWAYS READ the PRD and TechSpec before starting
- REFERENCE TECHSPEC for implementation details — do not duplicate here
- FOCUS ON "WHAT" — describe what needs to be accomplished, not how
- MINIMIZE CODE — show code only to illustrate current structure or problem areas
- TESTS REQUIRED — every task MUST include tests in deliverables
</critical>

<requirements>
- MUST implement the algorithm in TechSpec "Parser algorithm (`TryParseAuthorKeyed`)": `.`-split segments, `;`/`,`-split pieces, `:` opens a new entry, initials-block pieces (via `AuthorInitialsResolver.IsInitialsToken`) are pending keys for the next entry, all other pieces are terms.
- MUST fall back to Prose when a segment has no `:`, an entry has no terms, pending keys are never closed by a `:` piece, an initials block is followed by a term instead of `:`, or no term maps via `CreditTermTable.TryMap`.
- MUST split keys before the first `:` on both `;` and `,`.
- MUST keep `TryParseRoleKeyed` unchanged and still tried first.
- MUST keep the four existing parser tests green unchanged (`Parse_AuthorKeyed_SharesTermsAcrossInitialBlock`, `Parse_Prose_IsProse`, `Parse_CompoundLabelRoleKeyed_FallsBackToProse`, `Parse_AuthorKeyed_WithMalformedSegment_FallsBackToProse`).
- MUST preserve `EntryBuilder` semantics (first-seen author order, de-duplicated terms).
</requirements>

## Subtasks
- [x] 2.1 Rewrite `TryParseAuthorKeyed` per the TechSpec algorithm, reusing `IsInitialsToken` from task_01.
- [x] 2.2 Update the XML doc comments on `Parse`/`TryParseAuthorKeyed` to describe the widened grammar and the lookback rule.
- [x] 2.3 Add corpus tests using the real v26n3 statement texts (from the read-only evidence prompt) with exact per-author term expectations.
- [x] 2.4 Add negative tests for the new Prose fallbacks.
- [x] 2.5 Run the full suite; `CreditRolesInjectorTests` and pipeline tests stay green.

## Implementation Details
Modify `DocFormatter.Core/Jats/CreditStatementParser.cs` (`TryParseAuthorKeyed`, `SplitTrim` to accept multiple separators). Statement texts for tests: `/Users/gilcemir.angelo/Documents/personal_workspace/Trabalho/CBAB/v26/n3/PROMPT_credit_roles_v26n3.md` (read-only; copy the strings into the test file). See ADR-003 for the alternatives rejected.

### Relevant Files
- `DocFormatter.Core/Jats/CreditStatementParser.cs` — parser to rewrite.
- `DocFormatter.Core/Jats/AuthorInitialsResolver.cs` — provides `IsInitialsToken` (task_01).
- `DocFormatter.Core/Jats/CreditTermTable.cs` — `TryMap`/`Normalize` used for the mapped-term discriminator; already folds `Writing  review & editing` (double space) correctly.
- `DocFormatter.Tests/Jats/CreditStatementParserTests.cs` — existing tests to preserve and extend.

### Dependent Files
- `DocFormatter.Core/Jats/CreditRolesInjector.cs` — consumes `CreditStatement.Entries`; more statements now reach `BuildPlan`.
- `DocFormatter.Tests/Jats/CreditRolesInjectorTests.cs` — end-to-end expectations.

### Related ADRs
- [ADR-003: Widen the author-keyed CREDIT grammar with shape-based lookback, and never drop a present statement](../adrs/adr-003.md) — the grammar implemented here.

## Deliverables
- New `TryParseAuthorKeyed` accepting `;`/`,` with lookback.
- Corpus tests for 5642, 5412, 5501, 5547, 5613, 5528, 5441.
- Unit tests with 80%+ coverage **(REQUIRED)**
- Integration tests: existing injector/pipeline tests green **(REQUIRED)**

## Tests
- Unit tests:
  - [x] 5642 text → AuthorKeyed; EVT has exactly 11 terms, all mapping via `TryMap`.
  - [x] 5412 text → AuthorKeyed; AN and SR each have the same 12 terms.
  - [x] 5501 text → AuthorKeyed with 5 authors; CDC = [Methodology, Supervision, Validation, Resources, Writing - review & editing]; CFA has 12 terms including Project administration and Funding acquisition; no junk term contains `;` or `:`.
  - [x] 5547 text → AuthorKeyed with 5 authors (EAA, ASGC, HSP, LCM, PGSM); ASGC has 8 terms; no term contains `;`.
  - [x] 5613 text → AuthorKeyed with NHN, TVB, QHTP; QHTP = [Data curation, Formal analysis].
  - [x] 5528 text → AuthorKeyed; JILR contains "Writing - Original Draft" and "Writing - review & editing" as separate terms; ORJC has Validation.
  - [x] 5441 text → AuthorKeyed; CFA and JAC each have the same de-duplicated list (Conceptualization, Methodology, Data curation, Formal analysis, Investigation, Software, Visualization, Writing - original draft, Writing - review & editing — 9 terms; the original "7 terms" count was a slip) and no term contains `;`.
  - [x] "ABC: Methodology; DEF" (pending key never closed) → Prose.
  - [x] "ABC: Methodology; DEF; Software" (initials block followed by a term) → Prose.
  - [x] "ABC: ; DEF: Software" (entry without terms) → Prose.
  - [x] The four existing tests pass unchanged.
- Integration tests:
  - [x] `CreditRolesInjectorTests` pass; a `;`-separated statement injected end-to-end is covered in task_04.
- Test coverage target: >=80%
- All tests must pass

## Success Criteria
- All tests passing
- Test coverage >=80%
- All 15 v26n3 statements parse as AuthorKeyed (5536 once task_03 reads it); none returns Prose.
