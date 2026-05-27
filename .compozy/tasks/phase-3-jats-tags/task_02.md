---
status: completed
title: OtherTable loader
type: backend
complexity: low
dependencies: []
---

# Task 2: OtherTable loader

## Overview
Load `other.txt` (a TSV of `<pdf-basename>\t<5-digit-other>`) into a lookup keyed
by basename without extension. This supplies the only value that is not derivable
from any document — the externally-assigned `other` number.

<critical>
- ALWAYS READ the PRD and TechSpec before starting
- REFERENCE TECHSPEC for implementation details — do not duplicate here
- FOCUS ON "WHAT" — describe what needs to be accomplished, not how
- MINIMIZE CODE — show code only to illustrate current structure or problem areas
- TESTS REQUIRED — every task MUST include tests in deliverables
</critical>

<requirements>
- MUST parse `other.txt` as tab-separated `<pdf-basename>\t<other>` lines.
- MUST key the lookup on the basename with the extension stripped, so an XML/PDF
  basename resolves directly.
- MUST preserve the `other` value exactly as written (5-digit, zero-padded); MUST
  NOT reformat or numerically parse it.
- MUST expose a lookup that signals "not found" distinctly (so the caller can
  fail loudly per ADR-001), not a silent empty string.
- SHOULD ignore blank lines and tolerate trailing whitespace.
</requirements>

## Subtasks
- [x] 2.1 Define an `OtherTable` type with a load entry point and a lookup method.
- [x] 2.2 Parse the TSV, stripping the extension from each key.
- [x] 2.3 Provide a lookup returning a found/not-found result for a basename.
- [x] 2.4 Ignore blank lines and tolerate trailing whitespace.

## Implementation Details
Create `DocFormatter.Core/Jats/OtherTable.cs`. See TechSpec "Data Models"
(other.txt row) and the `OtherTable` component in "Component Overview". The table
is read once per run by the CLI (task_11) and queried by `DocumentPairer`
(task_03) and `OtherIdInjector` (task_07).

### Relevant Files
- `DocFormatter.Core/Jats/OtherTable.cs` — new loader (create).
- `examples/phase-3/other.txt` — input format reference.
- `docs/scielo_context/jats/article_id_other.md` — TSV format and lookup rule.

### Dependent Files
- `DocFormatter.Core/Jats/DocumentPairer.cs` — checks an entry exists (task_03).
- `DocFormatter.Core/Jats/OtherIdInjector.cs` — reads the value (task_07).

### Related ADRs
- [ADR-004: Pair on elocation-id, verify with DOI](../adrs/adr-004.md) — `other.txt` keyed by elocation-id basename.

## Deliverables
- `OtherTable` loader with basename-keyed lookup and a found/not-found result.
- Unit tests with 80%+ coverage **(REQUIRED)**
- Integration test loading the real `examples/phase-3/other.txt` **(REQUIRED)**

## Tests
- Unit tests:
  - [ ] `1984-7033-cbab-26-02-e54492621.pdf\t00201` is retrievable by basename `1984-7033-cbab-26-02-e54492621`.
  - [ ] Lookup for an absent basename returns not-found (not empty string).
  - [ ] `other` value `00201` is preserved exactly (leading zeros intact).
  - [ ] Blank lines and trailing whitespace are ignored.
- Integration tests:
  - [ ] Loading `examples/phase-3/other.txt` yields one entry per non-blank line, each resolvable by its XML basename.
- Test coverage target: >=80%
- All tests must pass

## Success Criteria
- All tests passing
- Test coverage >=80%
- Every XML basename in the corpus resolves to its `other` number; missing keys are reported as not-found.
