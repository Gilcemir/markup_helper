---
status: completed
title: OtherIdInjector
type: backend
complexity: medium
dependencies:
  - task_02
  - task_04
  - task_06
---

# Task 7: OtherIdInjector

## Overview
Inject `<article-id pub-id-type="other">` immediately after the DOI
`<article-id>`, using the value from `other.txt`. This is the fully
deterministic tag: it fails loudly when the XML has no DOI and skips when an
`other` id is already present.

<critical>
- ALWAYS READ the PRD and TechSpec before starting
- REFERENCE TECHSPEC for implementation details — do not duplicate here
- FOCUS ON "WHAT" — describe what needs to be accomplished, not how
- MINIMIZE CODE — show code only to illustrate current structure or problem areas
- TESTS REQUIRED — every task MUST include tests in deliverables
</critical>

<requirements>
- MUST locate `//article-id[@pub-id-type='doi']` and insert
  `<article-id pub-id-type="other">VALUE</article-id>` immediately after it.
- MUST be a critical injector: if no DOI `<article-id>` exists, throw to abort the
  document (no orphan `other`).
- MUST be idempotent: if an `<article-id pub-id-type="other">` already exists,
  skip and report "already present" (ADR-005 / PRD idempotency).
- MUST use `Phase3Context.OtherNumber`; if it is null/not-found, report and abort
  the tag (deterministic precondition).
- MUST build the element via the task_06 helper so indentation matches siblings
  and the namespace is inherited.
</requirements>

## Subtasks
- [x] 7.1 Detect an existing `other` article-id and skip-if-present.
- [x] 7.2 Locate the DOI article-id; abort loudly if missing.
- [x] 7.3 Insert the `other` element immediately after the DOI.
- [x] 7.4 Report the applied value (or skip/abort reason).

## Implementation Details
Create `DocFormatter.Core/Jats/OtherIdInjector.cs` implementing `IJatsInjector`
(task_04). Placement rule per `docs/scielo_context/jats/article_id_other.md`.
Uses the XML helper (task_06) and the `other` value resolved upstream from
`OtherTable` (task_02). See TechSpec injection-rules table.

### Relevant Files
- `DocFormatter.Core/Jats/OtherIdInjector.cs` — new (create).
- `docs/scielo_context/jats/article_id_other.md` — placement rule.
- `DocFormatter.Core/Jats/JatsXmlWriter.cs` — element builder (task_06).

### Dependent Files
- `AddPhase3Injectors()` registration (task_11).
- Golden-corpus tests (task_12).

### Related ADRs
- [ADR-001: Confidence-gated injection](../adrs/adr-001.md) — deterministic, fail-loud precondition.
- [ADR-004: Pair on elocation-id, verify with DOI](../adrs/adr-004.md) — `other` source.

## Deliverables
- `OtherIdInjector` placing the `other` id after the DOI, idempotent and fail-loud.
- Unit tests with 80%+ coverage **(REQUIRED)**
- Integration test on a corpus XML **(REQUIRED)**

## Tests
- Unit tests:
  - [x] XML with a DOI article-id and `OtherNumber="00201"` gets `<article-id pub-id-type="other">00201</article-id>` immediately after the DOI.
  - [x] XML with no DOI article-id throws (document aborted), nothing inserted.
  - [x] XML already containing an `other` article-id is left unchanged and reported as skipped.
  - [x] Null `OtherNumber` aborts the tag with a reported reason.
  - [x] Inserted element inherits the root namespace and sibling indentation.
- Integration tests:
  - [x] Running over `…e54492621.xml` with its `other=00201` yields the tag in the correct position.
- Test coverage target: >=80%
- All tests must pass

## Success Criteria
- All tests passing
- Test coverage >=80%
- `other` id placed correctly for all corpus packages; missing-DOI and present-id cases handled without silent error.
