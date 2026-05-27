---
status: completed
title: Phase 3 golden-corpus tests
type: test
complexity: medium
dependencies:
  - task_11
---

# Task 12: Phase 3 golden-corpus tests

## Overview
Add an end-to-end golden-corpus test for Phase 3, mirroring `Phase2CorpusTests`:
run `phase3 --non-interactive=accept` over each corpus package and diff the
produced XML against curated expected files. Add a strict `--non-interactive=fail`
run to document which corpus documents are ambiguous. This proves correct
placement and the auto/proposal behavior across the four tags.

<critical>
- ALWAYS READ the PRD and TechSpec before starting
- REFERENCE TECHSPEC for implementation details — do not duplicate here
- FOCUS ON "WHAT" — describe what needs to be accomplished, not how
- MINIMIZE CODE — show code only to illustrate current structure or problem areas
- TESTS REQUIRED — every task MUST include tests in deliverables
</critical>

<requirements>
- MUST add `Phase3CorpusTests` under `DocFormatter.Tests/Phase3/`, discovering the
  corpus by walking up to `examples/phase-3/` (as `Phase2CorpusTests` does).
- MUST run each package via `CliApp.Run(["phase3", tempXml, "--non-interactive=accept"], …)`
  and diff produced XML against `examples/phase-3/expected/<basename>.xml`.
- MUST curate `examples/phase-3/expected/` golden files encoding auto + best-guess
  proposed values, with only the injected tags differing from the source XML.
- MUST include a strict run with `--non-interactive=fail` asserting the set of
  documents that prompt (documents current ambiguity; updated intentionally).
- MUST assert the diff is limited to the injected tags (no incidental
  reformatting), validating the whitespace-preserving writer.
- SHOULD cover at least one document per CREDIT shape (role-keyed, author-keyed,
  prose) and varied data-availability categories.
</requirements>

## Subtasks
- [x] 12.1 Add `Phase3CorpusTests` with corpus discovery and per-package iteration.
- [x] 12.2 Curate `examples/phase-3/expected/` golden XML for the selected packages.
- [x] 12.3 Diff produced vs expected and assert injected-tags-only changes.
- [x] 12.4 Add the strict fail-on-prompt run asserting the ambiguous-document set.

## Implementation Details
Create `DocFormatter.Tests/Phase3/Phase3CorpusTests.cs` and
`examples/phase-3/expected/`. Reuse the corpus-discovery and temp-copy pattern
from `DocFormatter.Tests/Phase2/Phase2CorpusTests.cs`; reuse or extend a diff
utility (`Phase2DiffUtility`) for XML comparison. See TechSpec "Testing Approach"
and ADR-006 (accept policy encodes proposals).

### Relevant Files
- `DocFormatter.Tests/Phase3/Phase3CorpusTests.cs` — new (create).
- `examples/phase-3/expected/*.xml` — new golden output (create/curate).
- `DocFormatter.Tests/Phase2/Phase2CorpusTests.cs` — pattern to mirror.
- `DocFormatter.Core/Reporting/Phase2DiffUtility.cs` — XML diff reference.

### Dependent Files
- None downstream; this is the terminal verification task.

### Related ADRs
- [ADR-006: Confirmer with non-interactive policy](../adrs/adr-006.md) — accept for goldens, fail for strict CI.
- [ADR-005: CRediT auto/prose gate](../adrs/adr-005.md) — shape coverage expectations.

## Deliverables
- `Phase3CorpusTests` (accept-mode golden diff + fail-mode ambiguity assertion).
- Curated `examples/phase-3/expected/` golden XML files.
- Unit tests with 80%+ coverage **(REQUIRED)** (test helpers/diff usage)
- Integration tests across the corpus **(REQUIRED)**

## Tests
- Unit tests:
  - [ ] Corpus discovery walks up to `examples/phase-3/` and finds packages + expected files.
  - [ ] The diff helper reports a mismatch when produced XML differs from expected outside injected tags.
- Integration tests:
  - [ ] Each corpus package under `--non-interactive=accept` produces XML byte-equal to its expected golden file.
  - [ ] The produced/expected diff for each package is limited to the injected tags (no reformatting elsewhere).
  - [ ] `--non-interactive=fail` over the corpus prompts exactly on the documented ambiguous set (e.g. prose-CREDIT docs).
  - [ ] A role-keyed, an author-keyed, and a prose CREDIT document are each represented.
- Test coverage target: >=80%
- All tests must pass

## Success Criteria
- All tests passing
- Test coverage >=80%
- Every corpus package matches its golden output; diffs isolate injected tags; the ambiguous-document set is asserted and stable.
