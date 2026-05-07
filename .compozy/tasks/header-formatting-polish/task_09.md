---
status: completed
title: Extend DiagnosticDocument and DiagnosticWriter with formatting section
type: backend
complexity: medium
dependencies:
  - task_05
  - task_06
  - task_07
  - task_08
---

# Task 09: Extend DiagnosticDocument and DiagnosticWriter with formatting section

## Overview
Add the additive `formatting` section to the diagnostic JSON schema described in ADR-004 and PRD User Experience item 3, and teach `DiagnosticWriter` how to populate each sub-object from `report.Entries` keyed by the four new rule names. The section is null on green runs and on warn/error runs that did not involve the four Phase 2 rules; otherwise only the affected sub-objects are populated.

<critical>
- ALWAYS READ the PRD and TechSpec before starting
- REFERENCE TECHSPEC for implementation details — do not duplicate here
- FOCUS ON "WHAT" — describe what needs to be accomplished, not how
- MINIMIZE CODE — show code only to illustrate current structure or problem areas
- TESTS REQUIRED — every task MUST include tests in deliverables
</critical>

<requirements>
- MUST add the four new record types listed in TechSpec "Data Models" to `DocFormatter.Core/Reporting/DiagnosticDocument.cs`: `DiagnosticFormatting`, `DiagnosticAlignment`, `DiagnosticAbstract`, `DiagnosticCorrespondingEmail`.
- MUST add a nullable `DiagnosticFormatting? Formatting` property to `DiagnosticDocument` between `Fields` and `Issues` so JSON serialization order matches ADR-004.
- MUST update `DiagnosticDocument.Equals` and `GetHashCode` to include the new property.
- MUST update `DiagnosticWriter.Build` to construct the `Formatting` section by inspecting `report.Entries` keyed by `nameof(ApplyHeaderAlignmentRule)`, `nameof(EnsureAuthorBlockSpacingRule)`, `nameof(ExtractCorrespondingAuthorRule)`, `nameof(RewriteAbstractRule)`.
- The `Formatting` section MUST be `null` when none of the four rules emitted `[WARN]`/`[ERROR]`. When at least one did, only the affected sub-objects MUST be non-null.
- `DiagnosticAlignment` booleans (`Doi`/`Section`/`Title`) MUST be `false` only when the alignment rule explicitly logged a `[WARN]` for that paragraph; otherwise `true`.
- `DiagnosticAbstract.InternalItalicPreserved` MUST be `true` when ADR-002's mixed-italic branch ran and `false` when the structural-wrapper branch stripped italic.
- `DiagnosticCorrespondingEmail.Value` MUST carry the extracted email when extraction succeeded; `Value == null` and `Reason` populated when the marker was found but the email regex failed.
- Existing legacy fields (`File`, `Status`, `ExtractedAt`, `Fields`, `Issues`) MUST remain unchanged in shape and behavior; legacy consumers must keep working.
- Serializer settings (camelCase, `JsonIgnoreCondition.Never`) MUST stay as-is.
</requirements>

## Subtasks
- [x] 9.1 Add the four new record types in `DiagnosticDocument.cs` with explicit `Equals`/`GetHashCode` if needed (matching the pattern of existing records).
- [x] 9.2 Insert the `Formatting` property into `DiagnosticDocument` and update its custom `Equals`/`GetHashCode`.
- [x] 9.3 Add a private `BuildFormatting(IReport report)` helper in `DiagnosticWriter` returning `DiagnosticFormatting?`, populated only when at least one of the four rule names contributed a `[WARN]`/`[ERROR]`.
- [x] 9.4 Wire `BuildFormatting` into `DiagnosticWriter.Build`.
- [x] 9.5 Extend `DiagnosticWriterTests` with the fixtures listed in TechSpec "Testing Approach → DiagnosticWriterTests".
- [x] 9.6 Verify legacy JSON output (when no Phase 2 rule produces warnings) keeps the same shape modulo the additive `formatting: null` key.

## Implementation Details
Modify `DocFormatter.Core/Reporting/DiagnosticDocument.cs` and `DocFormatter.Core/Reporting/DiagnosticWriter.cs` only. Match the existing record pattern (sealed records with explicit `Equals`/`GetHashCode` when sequence members are involved). The writer should pattern-match `entry.Rule == nameof(...)` and aggregate per-rule outcomes (e.g., counting `[WARN]`s mentioning "DOI"/"section"/"title" for `DiagnosticAlignment`). See ADR-004 "Implementation Notes" for the writer hints.

### Relevant Files
- `DocFormatter.Core/Reporting/DiagnosticDocument.cs` — record types to extend.
- `DocFormatter.Core/Reporting/DiagnosticWriter.cs` — builder to extend.
- `DocFormatter.Core/Pipeline/Report.cs` and `ReportEntry.cs` — entry shape (`Rule`, `Level`, `Message`).
- `DocFormatter.Tests/DiagnosticWriterTests.cs` — pattern for new test fixtures.

### Dependent Files
- `DocFormatter.Cli/FileProcessor.cs` — calls `DiagnosticWriter.Write`; should not need changes if the new property is additive.
- Any external consumer of the diagnostic JSON (none in this repo) — schema remains backward-compatible.

### Related ADRs
- [ADR-004: Additive `formatting` section](../adrs/adr-004-diagnostic-formatting-section.md) — defines the schema and writer responsibilities.
- [ADR-002: Italic preservation heuristic](../adrs/adr-002-italic-preservation-heuristic.md) — drives the `InternalItalicPreserved` flag semantics.

## Deliverables
- `DiagnosticDocument` extended with the `Formatting` property and four new record types.
- `DiagnosticWriter.Build` populating the section per rule.
- `DiagnosticWriterTests` extended with green/warn fixtures.
- Unit tests with 80%+ coverage **(REQUIRED)**.
- Backward-compatibility test confirming the legacy keys are unchanged on a green run **(REQUIRED)**.

## Tests
- Unit tests:
  - [x] All four rules silent (no `[WARN]`/`[ERROR]`) → serialized JSON has `"formatting": null`.
  - [x] Only `ApplyHeaderAlignmentRule` warned about the title paragraph → `DiagnosticFormatting.AlignmentApplied = { Doi: true, Section: true, Title: false }`; other sub-objects null.
  - [x] Only `ApplyHeaderAlignmentRule` warned about all three paragraphs → `AlignmentApplied = { false, false, false }`.
  - [x] `ExtractCorrespondingAuthorRule` emitted `[WARN]` for "email could not be extracted" → `DiagnosticCorrespondingEmail = { Value: null, Reason: "<msg>" }`.
  - [x] `RewriteAbstractRule` ran the structural-italic branch → `DiagnosticAbstract.HeadingRewritten == true`, `BodyDeitalicized == true`, `InternalItalicPreserved == false`.
  - [x] `RewriteAbstractRule` ran the mixed-italic branch → `BodyDeitalicized == false`, `InternalItalicPreserved == true`.
  - [x] `EnsureAuthorBlockSpacingRule` reported a missing anchor (`[WARN]`) → `AuthorBlockSpacingApplied == false`; reported "blank already present" (`[INFO]`) → `AuthorBlockSpacingApplied == true`.
  - [x] Multiple rules contribute simultaneously → all matching sub-objects populated, untouched ones remain `null`.
  - [x] `DiagnosticDocument.Equals` returns `true` when two equal documents (with or without `Formatting`) are compared and `false` when only `Formatting` differs.
- Integration tests:
  - [ ] `FileProcessor` end-to-end (covered by task_10) writes a diagnostic JSON whose `formatting` section reflects the rules' actual outcomes for a `*`-marked fixture.
- Test coverage target: >=80%
- All tests must pass

## Success Criteria
- All tests passing
- Test coverage >=80%
- Diagnostic JSON has the `formatting` sibling per ADR-004; legacy keys are unchanged.
- Each sub-object's null/non-null state is driven by the corresponding rule's report entries.
