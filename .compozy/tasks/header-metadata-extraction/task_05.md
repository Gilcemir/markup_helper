---
status: completed
title: ExtractTopTableRule for DOI and ELOCATION extraction
type: backend
complexity: medium
dependencies:
    - task_03
    - task_04
---

# Task 5: ExtractTopTableRule for DOI and ELOCATION extraction

## Overview
Implement the first rule in the pipeline: locate the 3×1 control table at the top of the document, extract the `doi` and `elocation` cell values into `FormattingContext`, validate the DOI against `FormattingOptions.DoiRegex`, and delete the table from the document. This rule is `Critical`: if the table is missing or unrecognizable, the pipeline aborts with the message specified in the original spec ("este arquivo não está no formato de entrada esperado — pode já estar formatado, ou ser de outra fonte").

<critical>
- ALWAYS READ the PRD and TechSpec before starting
- REFERENCE TECHSPEC for implementation details — do not duplicate here
- FOCUS ON "WHAT" — describe what needs to be accomplished, not how
- MINIMIZE CODE — show code only to illustrate current structure or problem areas
- TESTS REQUIRED — every task MUST include tests in deliverables
</critical>

<requirements>
- 1. The rule MUST identify the top-of-document table by document order: the first `<w:tbl>` element directly under `<w:body>` whose grid has exactly three columns and exactly one row.
- 2. The rule MUST attempt to match cell content first by header text (`id`, `elocation`, `doi`) when present; if all three cells are header-less, fall back to positional order `[id, elocation, doi]` and log `[WARN]`.
- 3. The DOI cell value MUST be validated against `DoiRegex`. If it does not match, the rule MUST scan the other two cells for a DOI-shaped value and use the match if found, logging `[WARN]`. If no cell matches, set `Doi=null` and log `[WARN]`.
- 4. `FormattingContext.ElocationId` MUST be set from the elocation cell. An empty elocation is allowed but logs `[WARN]`.
- 5. The rule MUST delete the matched table from the document body after extraction.
- 6. If no qualifying 3×1 table is found at the top, the rule MUST throw `InvalidOperationException` with the spec's exact abort message; severity `Critical` ensures the pipeline aborts.
- 7. The rule MUST NOT mutate any other element in the document.
</requirements>

## Subtasks
- [x] 5.1 Create `DocFormatter.Core/Rules/ExtractTopTableRule.cs` implementing `IFormattingRule` with `Severity=Critical`.
- [x] 5.2 Implement the table-discovery logic and the header-vs-positional cell mapping.
- [x] 5.3 Implement DOI regex validation with the cross-cell fallback scan.
- [x] 5.4 Delete the matched table; preserve every other element.
- [x] 5.5 Add xUnit tests covering: table present with header text, table without headers, missing DOI in expected cell, table absent (Critical abort), wrong table dimensions ignored.

## Implementation Details
File is new under `DocFormatter.Core/Rules/`. See TechSpec "Rules" table (rule #1) for the contract and "Known Risks" for the column-order risk that drives the fallback logic. The rule consumes `FormattingOptions` via constructor injection.

### Relevant Files
- `.compozy/tasks/header-metadata-extraction/_techspec.md` — "Rules" and "Known Risks"
- `instructions.md` — original spec's pipeline rule #1 and #2

### Dependent Files
- `DocFormatter.Core/Rules/ExtractTopTableRule.cs` (new)
- `DocFormatter.Tests/ExtractTopTableRuleTests.cs` (new)
- `DocFormatter.Tests/Fixtures/Headers/*.docx` (new, in-memory factory recommended)

### Related ADRs
- [ADR-001: Esqueleto alinhado ao spec](adrs/adr-001.md)

## Deliverables
- `ExtractTopTableRule` implementation
- xUnit tests covering the five scenarios in subtask 5.5
- Unit tests with 80%+ coverage **(REQUIRED)**
- Integration tests for [pipeline-driven rule execution] **(REQUIRED)**

## Tests
- Unit tests:
  - [ ] Table with cells `[id=ART01, elocation=e2024001, doi=10.1234/abc]` and headers `[id, elocation, doi]`: `Doi="10.1234/abc"`, `ElocationId="e2024001"`, table deleted.
  - [ ] Table without headers, cells `[ART01, e2024001, 10.1234/abc]`: same outputs as above plus `[WARN]` "headers absent, fell back to positional mapping".
  - [ ] Table where the doi cell contains `"not-a-doi"` and the elocation cell contains `"10.5678/xyz"`: `Doi="10.5678/xyz"` (cross-cell scan), `ElocationId="10.5678/xyz"` (still extracted as written), `[WARN]` for the cross-cell fallback.
  - [ ] Document with no table at the top: `Run` throws `InvalidOperationException` with the spec's exact message.
  - [ ] Document with a 2×1 table at the top: rule treats it as "no qualifying table" and aborts (does not mutate the 2×1 table).
  - [ ] Document where the first element is a paragraph and the table appears later: rule treats it as "no qualifying table" and aborts.
- Integration tests:
  - [ ] When run inside `FormattingPipeline` with severity `Critical`, an aborted run leaves the document untouched (table not deleted on abort).
- Test coverage target: >=80%
- All tests must pass

## Success Criteria
- All tests passing
- Test coverage >=80%
- Critical abort message matches TechSpec/spec verbatim
- No mutation when the rule aborts
