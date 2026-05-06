---
status: completed
title: RewriteHeaderMvpRule for the four-field output layout
type: backend
complexity: medium
dependencies:
    - task_05
    - task_06
    - task_08
---

# Task 9: RewriteHeaderMvpRule for the four-field output layout

## Overview
Implement the `Critical` rule that consumes the populated `FormattingContext` and rewrites only the four MVP fields in the document header: insert a DOI line at position 1 (replacing the deleted table's slot), keep the existing section paragraph, keep the existing title paragraph, insert a blank line separating title and authors, and replace the original single-line authors paragraph with one paragraph per author (affiliation labels in superscript, ORCID ID in plain text after the labels). All other content stays exactly as found.

<critical>
- ALWAYS READ the PRD and TechSpec before starting
- REFERENCE TECHSPEC for implementation details — do not duplicate here
- FOCUS ON "WHAT" — describe what needs to be accomplished, not how
- MINIMIZE CODE — show code only to illustrate current structure or problem areas
- TESTS REQUIRED — every task MUST include tests in deliverables
</critical>

<requirements>
- 1. Line 1 of the body MUST be a paragraph whose plain text is the value of `ctx.Doi`. If `ctx.Doi` is null, the rule MUST log `[WARN]` and skip the DOI line; section becomes line 1.
- 2. The existing section paragraph and the existing title paragraph MUST stay in their original positions; this rule does not touch their content or formatting.
- 3. A blank paragraph MUST be inserted immediately after the title paragraph (before the authors block).
- 4. The original authors paragraph MUST be replaced by N paragraphs, one per `Author` in `ctx.Authors`. Each paragraph MUST contain: name + affiliation labels in superscript (preserving the original run formatting from task_07/task_08) + a single space + the ORCID ID as plain text when `OrcidId` is non-null.
- 5. The rule MUST NOT touch any element below the authors block (affiliations, history, abstract, body, references stay intact).
- 6. Severity is `Critical`: if `ctx.ArticleTitle` is null or `ctx.Authors` is empty, the rule MUST throw with a message that names the missing field.
- 7. The rule MUST be deterministic: running it twice on the same input (re-run scenario) yields identical output (idempotent at the document-mutation level once the input is the rewritten form is out of scope — the input-format guard handles that).
</requirements>

## Subtasks
- [x] 9.1 Create `DocFormatter.Core/Rules/RewriteHeaderMvpRule.cs` implementing `IFormattingRule` with `Severity=Critical`.
- [x] 9.2 Insert the DOI paragraph at the top of the body (or skip with `[WARN]` if null).
- [x] 9.3 Locate the original authors paragraph via `HeaderParagraphLocator`, replace it with N per-author paragraphs preserving superscript runs.
- [x] 9.4 Insert the blank paragraph between title and authors.
- [x] 9.5 Add xUnit tests covering: DOI present, DOI null, single author, multiple authors with mixed ORCID presence, empty `Authors` list (Critical abort).

## Implementation Details
File is new under `DocFormatter.Core/Rules/`. See TechSpec "Rules" rule #5 and "Implementation Design — Rules" for the rendering contract. The rule must reuse the run-property snapshots captured by task_08 to keep superscript formatting on affiliation labels.

### Relevant Files
- `.compozy/tasks/header-metadata-extraction/_techspec.md` — "Rules" rule #5
- `.compozy/tasks/header-metadata-extraction/_prd.md` — Core Features #5 (output layout)
- `instructions.md` — original spec's "Formato de SAÍDA esperado"

### Dependent Files
- `DocFormatter.Core/Rules/RewriteHeaderMvpRule.cs` (new)
- `DocFormatter.Tests/RewriteHeaderMvpRuleTests.cs` (new)

### Related ADRs
- [ADR-003: ORCID extraction](adrs/adr-003.md) — output format `<name><labels> <orcid-id>`
- [ADR-001: Esqueleto alinhado ao spec](adrs/adr-001.md)

## Deliverables
- `RewriteHeaderMvpRule` implementation
- xUnit tests for each scenario in subtask 9.5
- Unit tests with 80%+ coverage **(REQUIRED)**
- Integration tests for [end-to-end pipeline output] **(REQUIRED)**

## Tests
- Unit tests:
  - [x] Context with `Doi="10.1234/abc"`, `ArticleTitle="On X"`, `Authors=[{Name="Maria Silva", Labels=["1"], OrcidId=null}]`: body line 1 = "10.1234/abc", section paragraph unchanged, title "On X" unchanged, blank paragraph follows title, then "Maria Silva¹".
  - [x] Context with `Doi=null`: body line 1 is the section paragraph (DOI line skipped), `[WARN]` emitted, downstream layout unchanged.
  - [x] Context with two authors, second has ORCID: output is `["Author A¹"]` and `["Author B² 0000-0002-1825-0097"]` on separate paragraphs.
  - [x] Context with `Authors=[]`: rule throws `InvalidOperationException` referencing the empty author list.
  - [x] Document body containing affiliations, abstract, body, references: after the rule runs, every element below the authors block is byte-identical to the input.
- Integration tests:
  - [x] Full pipeline (`ExtractTopTableRule` → `ParseHeaderLinesRule` → `ExtractOrcidLinksRule` → `ParseAuthorsRule` → `RewriteHeaderMvpRule`) on a fixture with two authors and one ORCID produces the expected header layout end-to-end.
- Test coverage target: >=80%
- All tests must pass

## Success Criteria
- All tests passing
- Test coverage >=80%
- Content below the authors block is preserved verbatim
- Output respects the layout from PRD Core Features #5 and ADR-003
