---
status: completed
title: DocumentPairer with DOI verification
type: backend
complexity: medium
dependencies:
  - task_01
  - task_02
---

# Task 3: DocumentPairer with DOI verification

## Overview
Given an XML package path, locate its paired docx and `other.txt` entry by
elocation-id and cross-check the DOI on both sides, failing loudly on any
missing or mismatched key. This prevents applying the wrong source document to an
XML — a top PRD risk.

<critical>
- ALWAYS READ the PRD and TechSpec before starting
- REFERENCE TECHSPEC for implementation details — do not duplicate here
- FOCUS ON "WHAT" — describe what needs to be accomplished, not how
- MINIMIZE CODE — show code only to illustrate current structure or problem areas
- TESTS REQUIRED — every task MUST include tests in deliverables
</critical>

<requirements>
- MUST pair on elocation-id as the primary key: docx `[doc]@elocatid` ↔ XML
  `<elocation-id>` ↔ `other.txt` basename.
- MUST extract the DOI independently from the docx (`[doi]`) and the XML
  (`<article-id pub-id-type="doi">`) and assert they are equal.
- MUST fail loudly (skip the document with a reported reason, non-zero outcome)
  when: the elocation-id is missing on either side, the two DOIs disagree, or no
  `other.txt` entry exists for the basename.
- MUST NOT proceed on a half-matched pair or guess a pairing.
- MUST locate the docx by scanning the markup-source directory for the docx whose
  header `elocatid` matches (the docx filename does not encode the elocation-id).
</requirements>

## Subtasks
- [x] 3.1 Read the XML elocation-id and DOI from the package.
- [x] 3.2 Resolve the paired docx by matching header `elocatid` across the source dir.
- [x] 3.3 Verify docx DOI equals XML DOI.
- [x] 3.4 Confirm an `other.txt` entry exists for the basename.
- [x] 3.5 Return a paired result or a descriptive fail-loud error.

## Implementation Details
Create `DocFormatter.Core/Jats/DocumentPairer.cs`. Consumes `DocxSource`
(task_01) for header keys and `OtherTable` (task_02) for the entry check. Reads
the XML minimally for `<elocation-id>` and the DOI `<article-id>`. See TechSpec
"Component Overview" (DocumentPairer) and ADR-004 for the exact rule.

### Relevant Files
- `DocFormatter.Core/Jats/DocumentPairer.cs` — new pairer (create).
- `DocFormatter.Core/Jats/DocxSource.cs` — header keys (task_01).
- `DocFormatter.Core/Jats/OtherTable.cs` — entry existence (task_02).
- `examples/phase-3/scielo_package/*.xml`, `scielo_markup/*.docx`, `other.txt` — pairing inputs.

### Dependent Files
- `DocFormatter.Cli` phase3 orchestration — calls the pairer per document (task_11).

### Related ADRs
- [ADR-004: Pair on elocation-id, verify with DOI](../adrs/adr-004.md) — the pairing and fail-loud rule.
- [ADR-002: Source values from the paired docx](../adrs/adr-002.md) — three-input model.

## Deliverables
- `DocumentPairer` returning a paired (docx, xml, other-number) result or a descriptive error.
- Unit tests with 80%+ coverage **(REQUIRED)**
- Integration test pairing the real corpus **(REQUIRED)**

## Tests
- Unit tests:
  - [x] Matching elocation-id + equal DOIs + present `other` entry returns a successful pair.
  - [x] DOI mismatch between docx and XML returns a fail-loud error naming the conflict.
  - [x] XML missing `<elocation-id>` returns a fail-loud error.
  - [x] No docx in the source dir with the matching `elocatid` returns a fail-loud error.
  - [x] Basename absent from `other.txt` returns a fail-loud error.
- Integration tests:
  - [x] Every XML in `examples/phase-3/scielo_package/` pairs to its docx and `other` entry with DOIs agreeing.
- Test coverage target: >=80%
- All tests must pass

## Success Criteria
- All tests passing
- Test coverage >=80%
- All corpus packages pair successfully; every fail path produces a reported, non-silent error.
