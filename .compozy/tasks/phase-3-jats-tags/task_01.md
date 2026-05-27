---
status: completed
title: DocxSource model and DocxSourceReader
type: backend
complexity: medium
dependencies: []
---

# Task 1: DocxSource model and DocxSourceReader

## Overview
Parse the read-only `.docx` markup source into a `DocxSource` carrying the
`[doc]` header keys (elocation-id, DOI) and the three trailing untagged sections
(responsible editor, data availability, CREDIT statement). This is the source of
three of the four injected values and the docx side of document pairing.

<critical>
- ALWAYS READ the PRD and TechSpec before starting
- REFERENCE TECHSPEC for implementation details — do not duplicate here
- FOCUS ON "WHAT" — describe what needs to be accomplished, not how
- MINIMIZE CODE — show code only to illustrate current structure or problem areas
- TESTS REQUIRED — every task MUST include tests in deliverables
</critical>

<requirements>
- MUST read the docx with `DocumentFormat.OpenXml` (already referenced) as
  read-only; the docx MUST never be modified.
- MUST extract `elocatid` and the `[doi]…[/doi]` value from the `[doc …]` header
  near the top of the document.
- MUST extract the trailing untagged sections by anchoring on observed headers:
  `Scientific Editor:` (and an optional associate-editor role line),
  `DATA AVAILABILITY`, and `CREDIT STATEMENT`.
- MUST populate `DocxSource` fields as nullable when a section is absent (absence
  is a valid state to be reported downstream, not an exception here).
- MUST tolerate non-breaking spaces and surrounding bracket-tag content adjacent
  to the target sections (corpus uses `\xa0` and `[refs]`/`[corresp]` neighbors).
- MUST expose the raw CREDIT statement text unparsed (shape detection belongs to
  task_10).
</requirements>

## Subtasks
- [x] 1.1 Define the `DocxSource` model with header keys and the three section fields.
- [x] 1.2 Read docx paragraph text via OpenXml into a normalized plain-text form.
- [x] 1.3 Parse the `[doc]` header for `elocatid` and `[doi]`.
- [x] 1.4 Locate and extract the editor line(s), data-availability body, and raw CREDIT body.
- [x] 1.5 Return nullable fields for missing sections without throwing.

## Implementation Details
Create under `DocFormatter.Core/Jats/`. See TechSpec "Data Models" for the
`DocxSource` shape and "Component Overview" for `DocxSourceReader`'s role.
Reuse the OpenXml access pattern used by phase 1/2 rules (read
`MainDocumentPart.Document.Body` paragraphs). The `[doc]` header is a single
bracket tag; extract `elocatid` and `[doi]` with a focused parse, not a full
bracket-tag interpreter (see ADR-004).

### Relevant Files
- `DocFormatter.Core/Jats/DocxSource.cs` — new model (create).
- `DocFormatter.Core/Jats/DocxSourceReader.cs` — new reader (create).
- `DocFormatter.Core/Rules/ExtractAuthorsRule.cs` — reference for OpenXml paragraph-text reading.
- `examples/phase-3/scielo_markup/*.docx` — input shapes (header + trailing sections).

### Dependent Files
- `DocFormatter.Core/Jats/DocumentPairer.cs` — consumes header keys (task_03).
- `DocFormatter.Core/Jats/EditedByInjector.cs`, `DataAvailabilityInjector.cs`, `CreditRolesInjector.cs` — consume the section fields.

### Related ADRs
- [ADR-002: Source values from the paired docx markup source](../adrs/adr-002.md) — why values come from the docx.
- [ADR-004: Pair on elocation-id, verify with DOI](../adrs/adr-004.md) — why the header keys are extracted here.

## Deliverables
- `DocxSource` model and `DocxSourceReader` returning a populated `DocxSource`.
- Unit tests with 80%+ coverage **(REQUIRED)**
- Integration tests reading real corpus docx files **(REQUIRED)**

## Tests
- Unit tests:
  - [x] `[doc]` header with `elocatid="e54492621"` and `[doi]10.1590/…` yields both keys.
  - [x] Editor line `Scientific Editor: Luiz Antônio dos Santos Dias` (incl. `\xa0` variant) parses the name.
  - [x] `DATA AVAILABILITY` body text extracted, stopping before `[refs]`/REFERENCES.
  - [x] `CREDIT STATEMENT` raw body returned verbatim (role-keyed, author-keyed, and prose forms).
  - [x] Document missing the DATA AVAILABILITY section yields `DataAvailabilityText == null` (no throw).
  - [x] Document missing the editor line yields `ScientificEditor == null`.
- Integration tests:
  - [x] Reading `examples/phase-3/scielo_markup/5449.docx` populates editor, DA prose text, and raw prose CREDIT.
  - [x] Reading `5523.docx` populates a role-keyed CREDIT body and DA text.
- Test coverage target: >=80%
- All tests must pass

## Success Criteria
- All tests passing
- Test coverage >=80%
- `DocxSource` correctly extracted from every docx in `examples/phase-3/scielo_markup/`.
- Docx files are opened read-only and never written.
