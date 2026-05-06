---
status: completed
title: Diagnostic JSON serializer per ADR-004 schema
type: backend
complexity: medium
dependencies:
    - task_11
---

# Task 12: Diagnostic JSON serializer per ADR-004 schema

## Overview
Implement the writer that emits `<name>.diagnostic.json` next to the output `.docx` whenever the run logged at least one `[WARN]` or `[ERROR]` entry. The JSON shape is locked in ADR-004 — per-field block with `confidence` flags plus an ordered `issues` list. When the run is fully clean, the file is **not** written; `report.txt` remains the sole record.

<critical>
- ALWAYS READ the PRD and TechSpec before starting
- REFERENCE TECHSPEC for implementation details — do not duplicate here
- FOCUS ON "WHAT" — describe what needs to be accomplished, not how
- MINIMIZE CODE — show code only to illustrate current structure or problem areas
- TESTS REQUIRED — every task MUST include tests in deliverables
</critical>

<requirements>
- 1. The writer MUST be invoked from the CLI orchestrator after the pipeline finishes (success or Critical abort) and MUST write only if `report.HighestLevel >= ReportLevel.Warn`.
- 2. The serialized shape MUST match ADR-004 verbatim: `file`, `status`, `extractedAt`, `fields { doi, elocation, title, authors[] }`, `issues[]`.
- 3. JSON serialization MUST use `System.Text.Json` with camelCase property naming and indented output (`WriteIndented=true`).
- 4. `confidence` values MUST be a closed enum mapped to lowercase strings: `high|medium|low|missing`. Implement as `enum FieldConfidence` with a `JsonStringEnumConverter`.
- 5. `status` MUST be derived from `report.HighestLevel`: `Info → "ok"`, `Warn → "warning"`, `Error → "error"`. (Note: when status would be `"ok"` the file is not written per requirement 1.)
- 6. `extractedAt` MUST be UTC ISO-8601 with seconds precision.
- 7. The writer MUST handle a missing `ctx.Doi` (or any field) by emitting `value=null` and `confidence="missing"`.
- 8. The writer MUST emit `issues` in the same order as `report.Entries`, preserving the original `[WARN]/[ERROR]` levels.
</requirements>

## Subtasks
- [x] 12.1 Create `DocFormatter.Core/Reporting/DiagnosticDocument.cs` (DTO matching ADR-004) and `DiagnosticWriter.cs`.
- [x] 12.2 Define `FieldConfidence` enum and the per-rule confidence-assignment table from ADR-004 Implementation Notes.
- [x] 12.3 Implement the `Author` projection (name, affiliation labels, orcid, confidence) from `ctx.Authors`.
- [x] 12.4 Wire `DiagnosticWriter` into `FileProcessor` from task_11; only write when `HighestLevel >= Warn`.
- [x] 12.5 Add xUnit tests covering: clean run (no file written), warning run (file written with correct shape), error run (status="error"), missing fields (confidence="missing"), authors with mixed `confidence` levels.

## Implementation Details
Files are new under `DocFormatter.Core/Reporting/`. The writer reads from `FormattingContext` and `IReport` and writes via `File.WriteAllTextAsync` to the same `formatted/` directory used by task_11. See ADR-004 Implementation Notes for the exact confidence-assignment rules per rule.

### Relevant Files
- `.compozy/tasks/header-metadata-extraction/adrs/adr-004.md` — locked schema and confidence semantics
- `.compozy/tasks/header-metadata-extraction/_techspec.md` — "Monitoring and Observability"

### Dependent Files
- `DocFormatter.Core/Reporting/DiagnosticDocument.cs` (new)
- `DocFormatter.Core/Reporting/FieldConfidence.cs` (new)
- `DocFormatter.Core/Reporting/DiagnosticWriter.cs` (new)
- `DocFormatter.Cli/FileProcessor.cs` (modified — invoke the writer after pipeline run)
- `DocFormatter.Tests/DiagnosticWriterTests.cs` (new)

### Related ADRs
- [ADR-004: Diagnostic JSON schema](adrs/adr-004.md)

## Deliverables
- `DiagnosticWriter` and supporting DTOs
- Integration with `FileProcessor`
- xUnit tests for each scenario in subtask 12.5
- Unit tests with 80%+ coverage **(REQUIRED)**
- Integration tests for [CLI run producing diagnostic JSON] **(REQUIRED)**

## Tests
- Unit tests:
  - [ ] Report with only `[INFO]` entries: `DiagnosticWriter.Write` does not produce a file (return value indicates "not written").
  - [ ] Report with one `[WARN]`: produces JSON with `status="warning"`, `issues` array of length 1 with `level="warn"`.
  - [ ] Report with `[WARN]` and `[ERROR]`: `status="error"`, `issues` length 2 in entry order.
  - [ ] Context with `Doi=null`: serialized `fields.doi.value=null` and `fields.doi.confidence="missing"`.
  - [ ] Context with two authors, one with `confidence=low`: `fields.authors[1].confidence="low"`, the other `high`.
  - [ ] `extractedAt` is parseable as ISO-8601 UTC and within 1 second of `DateTime.UtcNow` at the call site.
  - [ ] Property names in serialized JSON are camelCase (e.g., `affiliationLabels`, not `AffiliationLabels`).
- Integration tests:
  - [ ] CLI single-file run on a fixture causing one `[WARN]`: a `<name>.diagnostic.json` file appears next to the output `.docx`, parsing it round-trips to the same `DiagnosticDocument` instance.
  - [ ] CLI single-file run on a fixture with no warnings: no `<name>.diagnostic.json` is produced.
- Test coverage target: >=80%
- All tests must pass

## Success Criteria
- All tests passing
- Test coverage >=80%
- JSON output validates against the ADR-004 schema verbatim (property names, enum values, nesting)
- The diagnostic file is absent on clean runs
