---
status: pending
title: Manual acceptance run over a copy of CBAB v26n3
type: chore
complexity: medium
dependencies:
  - task_01
  - task_02
  - task_03
  - task_04
  - task_05
  - task_06
  - task_07
---

# Task 9: Manual acceptance run over a copy of CBAB v26n3

## Overview
Run Phase 3 non-interactively over a scratch copy of the v26n3 edition (L01–L03) and compare reports, diagnostics and batch summaries with the PRD acceptance table, fixing any defect found. The evidence folder is read-only and is never written; this is the single validation cycle decided in ADR-001.

<critical>
- ALWAYS READ the PRD and TechSpec before starting
- REFERENCE TECHSPEC for implementation details — do not duplicate here
- FOCUS ON "WHAT" — describe what needs to be accomplished, not how
- MINIMIZE CODE — show code only to illustrate current structure or problem areas
- TESTS REQUIRED — every task MUST include tests in deliverables
</critical>

<requirements>
- MUST copy `/Users/gilcemir.angelo/Documents/personal_workspace/Trabalho/CBAB/v26/n3/L0X/markup_xml/{other.txt, scielo_package/*.xml}` for X in 1..3 into the session scratchpad, and place the 15 `n3/*.docx` into each `<copy>/L0X/markup_xml/scielo_markup/` (the CLI walks up to `other.txt` and looks for `scielo_markup/` beside it).
- MUST run `dotnet run --project DocFormatter.Cli -- phase3 <copy>/L0X/markup_xml/scielo_package --non-interactive=accept` for each L0X.
- MUST NOT write anything under the evidence folder.
- MUST verify: 5501, 5642, 5412, 5547, 5441, 5528, 5536, 5524, 5568, 5656 AutoApplied for all authors with no `credit-roles` WARN; 5613 pending only NHN; 5316 pending only MAF, JAN, AGS, JSP; 5529 only TTR; 5617 only JGS; 5719 only MRC; zero "free prose" in any report; `contrib-names` WARNs 7/1/1 on 5316/5613/5501.
- MUST record the three batch summaries and any deviation in the task memory/notes, and fix root causes in code (with tests) rather than adjusting expectations.
</requirements>

## Subtasks
- [ ] 9.1 Build the scratch copy in the expected layout.
- [ ] 9.2 Run Phase 3 for L01, L02, L03.
- [ ] 9.3 Compare `_batch_summary.txt`, `.report.txt` and `.diagnostic.json` against the acceptance table.
- [ ] 9.4 Fix deviations in code with regression tests; re-run until the table is met.
- [ ] 9.5 Note the results (summaries pasted) for the release notes.

## Implementation Details
No production code is expected to change unless a deviation appears. Pairing: `CliApp.TryResolvePhase3Layout` (upward walk, `scielo_markup/` preference) and `DocumentPairer.Pair` (elocation-id + DOI). Acceptance table: PRD "Phased Rollout Plan — Step 1" and the evidence prompt `PROMPT_credit_roles_v26n3.md`.

### Relevant Files
- `DocFormatter.Cli/CliApp.cs` — `RunPhase3Batch`, `TryResolvePhase3Layout`.
- `DocFormatter.Core/Jats/DocumentPairer.cs` — pairing rules that the copy layout must satisfy.
- `/Users/gilcemir.angelo/Documents/personal_workspace/Trabalho/CBAB/v26/n3/` — read-only evidence (docx, XML, other.txt, prior outputs for comparison).

### Dependent Files
- Any file touched by tasks 01–07 if a deviation is found.

### Related ADRs
- [ADR-001](../adrs/adr-001.md) — single validation cycle over v26n3.

## Deliverables
- Three batch summaries matching the acceptance table.
- Regression tests for any deviation fixed **(REQUIRED)**
- Full unit suite green after fix-ups **(REQUIRED)**

## Tests
- Unit tests:
  - [ ] Any deviation found gets a unit test reproducing it before the fix.
- Integration tests:
  - [ ] L01 summary: e55012633, e55242632, e55282635, e55472634 clean; e56132631 pending NHN only, 1 broken surname.
  - [ ] L02 summary: e55682637, e56422639 clean; e53162636 pending MAF, JAN, AGS, JSP with 7 broken surnames; e55292638 pending TTR; e561726310 pending JGS.
  - [ ] L03 summary: e541226312, e544126313, e553626311, e565626314 clean; e571926315 pending MRC.
  - [ ] `grep -l "free prose" */formatted-phase3/*.report.txt` returns nothing.
- Test coverage target: >=80%
- All tests must pass

## Success Criteria
- All tests passing
- Test coverage >=80%
- Acceptance table met on all 15 articles; evidence folder unchanged (`git`-free check: modification times unchanged).
