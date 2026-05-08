---
status: completed
title: Extend DiagnosticDocument with Phase 3 record types
type: backend
complexity: low
dependencies: []
---

# Task 05: Extend DiagnosticDocument with Phase 3 record types

## Overview
Add two new C# record types — `DiagnosticHistoryMove` and `DiagnosticSectionPromotion` — to the diagnostic schema, and extend `DiagnosticFormatting` with two nullable properties referencing them. This task is purely additive on the schema; both new properties are nullable so existing diagnostic JSON consumers that read only Phase 1+2 keys continue to work unchanged. The schema change unblocks `DiagnosticWriter` (task_06) but does not yet wire up the report-entry filtering — that is the next task.

<critical>
- ALWAYS READ the PRD and TechSpec before starting
- REFERENCE TECHSPEC for implementation details — do not duplicate here
- FOCUS ON "WHAT" — describe what needs to be accomplished, not how
- MINIMIZE CODE — show code only to illustrate current structure or problem areas
- TESTS REQUIRED — every task MUST include tests in deliverables
</critical>

<requirements>
- MUST add `DiagnosticHistoryMove` as a `public sealed record` with the exact constructor parameter order and types specified in the TechSpec "Data Models" section: `bool Applied`, `string? SkippedReason`, `bool AnchorFound`, `int? FromIndex`, `int? ToIndexBeforeIntro`, `int ParagraphsMoved`.
- MUST add `DiagnosticSectionPromotion` as a `public sealed record` with: `bool Applied`, `string? SkippedReason`, `bool AnchorFound`, `int? AnchorParagraphIndex`, `int SectionsPromoted`, `int SubsectionsPromoted`, `int SkippedParagraphsInsideTables`, `int SkippedParagraphsBeforeAnchor`.
- MUST extend `DiagnosticFormatting` (existing `public sealed record`) with two nullable properties: `DiagnosticHistoryMove? HistoryMove` and `DiagnosticSectionPromotion? SectionPromotion`. Property order in the record signature MUST place the new ones at the END, after the existing properties, to keep value-equality semantics aligned with positional construction.
- MUST keep all existing `DiagnosticFormatting` properties (`AlignmentApplied`, `AbstractFormatted`, `AuthorBlockSpacingApplied`, `CorrespondingEmail`) at the same position and type. The change MUST be additive only.
- MUST ensure the JSON serializer (the existing one used by Phase 1+2) emits `null` properties as omitted (or matches the existing convention used for `AbstractFormatted` etc.) so backward compatibility holds.
- MUST NOT introduce new namespaces; the new records live in the same namespace as `DiagnosticDocument` (`DocFormatter.Core.Reporting`).
- MUST NOT modify `DiagnosticWriter` in this task; the writer extension is task_06.
- The PRD-specified `skipped_reason` values (`anchor_missing`, `partial_block`, `out_of_order`, `not_adjacent`, `not_found`, `null`) MUST be representable by `string?`; this task does not introduce an enum.
</requirements>

## Subtasks
- [x] 5.1 Add `DiagnosticHistoryMove` record to `DiagnosticDocument.cs` with the documented properties.
- [x] 5.2 Add `DiagnosticSectionPromotion` record to `DiagnosticDocument.cs` with the documented properties.
- [x] 5.3 Extend `DiagnosticFormatting` with `HistoryMove` and `SectionPromotion` nullable properties at the end of the parameter list.
- [x] 5.4 Verify JSON serialization round-trip preserves `null` values (omitted in output) for both new properties.
- [x] 5.5 Update or add tests in the diagnostic test file to cover construction, equality, and JSON serialization of the new records.

## Implementation Details
Modified file:
- `DocFormatter.Core/Reporting/DiagnosticDocument.cs` — add two new records and extend `DiagnosticFormatting`.

Modified test file (if it exists; otherwise add a small one):
- `DocFormatter.Tests/DiagnosticDocumentTests.cs` (or the existing `DiagnosticWriterTests.cs` if no dedicated document test file exists) — add cases for the new records.

Refer to TechSpec section "Data Models" for the exact record signatures and to PRD section "Diagnostic JSON extension" for the JSON shape that the records must be able to produce. The JSON consumer already iterates over `formatting.*` keys, so adding nullable properties is a backward-compatible additive change as confirmed in the PRD.

This task does NOT touch `DiagnosticWriter`; it does NOT introduce report-entry filtering; it does NOT depend on the rules from tasks 03 and 04. It is parallelizable with those tasks.

### Relevant Files
- `DocFormatter.Core/Reporting/DiagnosticDocument.cs` — Modified; existing records (`DiagnosticDocument`, `DiagnosticFormatting`, `DiagnosticAlignment`, `DiagnosticAbstract`, `DiagnosticCorrespondingEmail`) are reference patterns for the new records' shape and visibility.
- `DocFormatter.Tests/DiagnosticWriterTests.cs` — Reference for existing diagnostic test conventions (record construction, JSON serialization round-trip).

### Dependent Files
- `DocFormatter.Core/Reporting/DiagnosticWriter.cs` (task_06) — Will populate the new properties via `BuildHistoryMove` and `BuildSectionPromotion` private methods.
- Any external JSON consumer of the diagnostic file (per PRD: the report renderer and the batch summary). They iterate `formatting.*` keys generically, so this additive change requires no consumer change.

### Related ADRs
- [ADR-001: Two discrete Optional rules over a single combined rule](../adrs/adr-001-two-discrete-rules.md) — Justifies two separate diagnostic objects (`HistoryMove`, `SectionPromotion`) rather than a combined Phase 3 object.

## Deliverables
- `DiagnosticHistoryMove` and `DiagnosticSectionPromotion` records added to `DiagnosticDocument.cs`.
- `DiagnosticFormatting` extended with `HistoryMove` and `SectionPromotion` nullable properties.
- Tests asserting record construction, value-equality semantics, and JSON serialization round-trip.
- Unit tests with 80%+ coverage on the new records **(REQUIRED)**.
- Integration tests verifying JSON output for a `DiagnosticFormatting` containing the new records **(REQUIRED)**.

## Tests
- Unit tests:
  - [ ] `DiagnosticHistoryMove` constructed with `Applied=true, SkippedReason=null, AnchorFound=true, FromIndex=9, ToIndexBeforeIntro=13, ParagraphsMoved=3` exposes those exact values.
  - [ ] `DiagnosticHistoryMove` value-equality holds: two instances with identical fields compare equal.
  - [ ] `DiagnosticSectionPromotion` constructed with `Applied=true, SkippedReason=null, AnchorFound=true, AnchorParagraphIndex=14, SectionsPromoted=7, SubsectionsPromoted=3, SkippedParagraphsInsideTables=18, SkippedParagraphsBeforeAnchor=2` exposes those exact values.
  - [ ] `DiagnosticFormatting` with `HistoryMove=null, SectionPromotion=null` plus existing Phase 1+2 fields populated round-trips through JSON without those null properties appearing in the output (matching the existing convention for `AlignmentApplied=null` etc.).
  - [ ] `DiagnosticFormatting` with `HistoryMove` and `SectionPromotion` populated serializes them under JSON keys `history_move` and `section_promotion` (matching the PRD specified shape).
  - [ ] `SkippedReason` can be set to each documented value (`"anchor_missing"`, `"partial_block"`, `"out_of_order"`, `"not_adjacent"`, `"not_found"`) and round-trips through JSON.
  - [ ] Existing `DiagnosticFormatting` tests (Phase 1+2) still pass — value equality for documents without the new properties is unchanged.
- Integration tests:
  - [ ] A `DiagnosticDocument` containing a fully-populated `DiagnosticFormatting` (all four Phase 1+2 fields plus the two new Phase 3 fields) serializes to JSON matching the PRD example structure and deserializes back to an equal document.
- Test coverage target: >=80%
- All tests must pass

## Success Criteria
- All tests passing
- Test coverage >=80%
- The diagnostic JSON schema is fully backward-compatible: every existing field stays at the same JSON path; null Phase 3 properties are omitted on serialization
- The records are ready to be populated by `DiagnosticWriter` in task_06
