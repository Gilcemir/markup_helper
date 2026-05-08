---
status: completed
title: Extend DiagnosticWriter to emit Phase 3 diagnostic entries
type: backend
complexity: medium
dependencies:
  - task_03
  - task_04
  - task_05
---

# Task 06: Extend DiagnosticWriter to emit Phase 3 diagnostic entries

## Overview
Wire `DiagnosticWriter` to populate the new `HistoryMove` and `SectionPromotion` record properties on `DiagnosticFormatting` from the `[INFO]`/`[WARN]` report entries emitted by `MoveHistoryRule` (task_03) and `PromoteSectionsRule` (task_04). This follows the same filter-by-rule-name + reconstruct-from-message-constants pattern already used by Phase 1+2 diagnostics. Critically, the writer references the rules' `public const string` message constants by reference (e.g., `MoveHistoryRule.AnchorMissingMessage`) rather than by literal string, so a typo or rename in either source is a compile error rather than a silent data loss.

<critical>
- ALWAYS READ the PRD and TechSpec before starting
- REFERENCE TECHSPEC for implementation details — do not duplicate here
- FOCUS ON "WHAT" — describe what needs to be accomplished, not how
- MINIMIZE CODE — show code only to illustrate current structure or problem areas
- TESTS REQUIRED — every task MUST include tests in deliverables
</critical>

<requirements>
- MUST add a private method `BuildHistoryMove(IReport report)` to `DiagnosticWriter` that returns `DiagnosticHistoryMove?`. It MUST filter report entries by `nameof(MoveHistoryRule)`, identify the entry's outcome by matching against the rule's `public const string` message constants/prefixes, and construct the record accordingly.
- MUST add a private method `BuildSectionPromotion(IReport report)` to `DiagnosticWriter` returning `DiagnosticSectionPromotion?`. It MUST filter by `nameof(PromoteSectionsRule)` and construct the record from the rule's emitted message(s) and counts.
- MUST return `null` from each builder when the corresponding rule emitted NO report entries (the rule did not run or the document did not exercise it).
- MUST set `Applied=true, SkippedReason=null, AnchorFound=true` on `DiagnosticHistoryMove` when the rule emitted the `MovedMessagePrefix` `[INFO]`. MUST set `Applied=false` and the appropriate `SkippedReason` for each `[WARN]` (`anchor_missing`, `partial_block`, `out_of_order`, `not_adjacent`). MUST set `Applied=false, SkippedReason="not_found"` for the `NotFoundMessage` `[INFO]`. MUST set `Applied=true, SkippedReason=null, ParagraphsMoved=0` for `AlreadyAdjacentMessage`.
- MUST extract numeric fields (`FromIndex`, `ToIndexBeforeIntro`, `ParagraphsMoved`, `AnchorParagraphIndex`, `SectionsPromoted`, `SubsectionsPromoted`, `SkippedParagraphsInsideTables`, `SkippedParagraphsBeforeAnchor`) from rule message text where present. The rules MAY need to emit additional structured data alongside their message constants — coordinate with task_03 and task_04 to expose these counts (e.g., as additional `[INFO]` entries or as numeric tokens parsed from the summary message).
- MUST extend `DiagnosticWriter.BuildFormatting` (or its existing equivalent) to assign `HistoryMove` and `SectionPromotion` on the returned `DiagnosticFormatting`.
- MUST reference rule message constants by name (e.g., `MoveHistoryRule.AnchorMissingMessage`), never by string literal.
- MUST NOT change the existing trigger condition for emitting the JSON file (`report.HighestLevel >= ReportLevel.Warn`). New `[INFO]`-only Phase 3 outputs are still serialized when other rules emit warnings; pure-info Phase 3 runs do not force a JSON write on their own (matches Phase 1+2 behaviour).
- MUST NOT throw on degenerate report inputs (e.g., a `MoveHistoryRule` entry with an unexpected message format); on an unrecognized message, the builder MAY return a record with `Applied=false, SkippedReason="unknown"` or fall back to `null`, but it MUST log nothing to the report (no recursion).
</requirements>

## Subtasks
- [x] 6.1 Inspect the existing `DiagnosticWriter.BuildFormatting` (or equivalent) to understand the filter-by-rule-name pattern used by Phase 1+2 builders; mirror that pattern.
- [x] 6.2 Implement `BuildHistoryMove` matching every documented `MoveHistoryRule` message and mapping to the record fields.
- [x] 6.3 Implement `BuildSectionPromotion` matching every documented `PromoteSectionsRule` message; parse counts from the summary message format established in task_04.
- [x] 6.4 Wire both builders into `BuildFormatting` so the returned `DiagnosticFormatting` has `HistoryMove` and `SectionPromotion` populated when applicable.
- [x] 6.5 If task_03 or task_04 emit message formats that lack the numeric data needed (e.g., `from_index`, `to_index_before_intro`), coordinate by adding additional structured `[INFO]` entries OR by encoding the numbers in the existing message text in a parseable form. Document the message-to-field mapping in code comments or a small mapping helper inside `DiagnosticWriter`.
- [x] 6.6 Extend `DiagnosticWriterTests.cs` with cases for each combination of rule outcomes.

## Implementation Details
Modified files:
- `DocFormatter.Core/Reporting/DiagnosticWriter.cs` — add `BuildHistoryMove`, `BuildSectionPromotion`, and the `BuildFormatting` extension.
- `DocFormatter.Tests/DiagnosticWriterTests.cs` — add Phase 3 cases.

Refer to TechSpec section "Impact Analysis" for the message-constants-by-reference contract, to PRD "Diagnostic JSON extension" for the exact JSON shape, and to the existing `DiagnosticWriter.BuildFormatting` Phase 1+2 builders (e.g., `BuildAlignment`, `BuildAbstract`) for the filter-by-rule-name reference pattern. If the existing builders use a `FilterByRule(report, ruleName)` helper, reuse it; if not, follow the same conventions.

If task_03 / task_04 do not currently emit numeric counts in their messages in a parseable format, raise the gap during this task: either extend their message text (coordinated update across tasks 03/04/06) or have them emit an additional structured `[INFO]` entry with the counts, then parse here. Document the choice in the code.

### Relevant Files
- `DocFormatter.Core/Reporting/DiagnosticWriter.cs` — Modified; the existing Phase 1+2 builders (`BuildAlignment`, `BuildAbstract`, `BuildCorrespondingEmail`, etc.) are reference patterns.
- `DocFormatter.Core/Reporting/DiagnosticDocument.cs` — Provides the new `DiagnosticHistoryMove` and `DiagnosticSectionPromotion` records (added in task_05).
- `DocFormatter.Core/Rules/MoveHistoryRule.cs` — Provides the message constants consumed by reference (`AnchorMissingMessage`, `MovedMessagePrefix`, etc.).
- `DocFormatter.Core/Rules/PromoteSectionsRule.cs` — Provides the message constants consumed by reference.
- `DocFormatter.Core/Pipeline/IReport.cs` — Provides the `Entries`/`HighestLevel` surface used for filtering.
- `DocFormatter.Tests/DiagnosticWriterTests.cs` — Modified; mirror the existing Phase 2 test cases for the new Phase 3 cases.

### Dependent Files
- `DocFormatter.Cli/CliApp.cs` (task_07) — Triggers `DiagnosticWriter.Build` after pipeline execution; the integration test in task_07 will validate end-to-end JSON output.
- `DocFormatter.Cli/FileProcessor.cs` (already exists) — Calls `DiagnosticWriter.Build`; no change needed but the task should verify the call path is unchanged.

### Related ADRs
- [ADR-001: Two discrete Optional rules over a single combined rule](../adrs/adr-001-two-discrete-rules.md) — Justifies two separate builders (`BuildHistoryMove`, `BuildSectionPromotion`) rather than one combined builder.

## Deliverables
- `BuildHistoryMove` and `BuildSectionPromotion` private methods in `DiagnosticWriter`.
- `BuildFormatting` extension that wires both builders.
- Updated `DiagnosticWriterTests.cs` with Phase 3 cases covering every documented rule outcome.
- Unit tests with 80%+ coverage **(REQUIRED)**.
- Integration tests verifying end-to-end JSON output for documents exercising both Phase 3 rules **(REQUIRED)**.

## Tests
- Unit tests:
  - [ ] `BuildHistoryMove` returns `null` when no `MoveHistoryRule` entries are present in the report.
  - [ ] `BuildHistoryMove` returns `Applied=true, SkippedReason=null, AnchorFound=true, ParagraphsMoved=3` for a synthesized `[INFO]` matching `MovedMessagePrefix`.
  - [ ] `BuildHistoryMove` returns `Applied=true, SkippedReason=null, ParagraphsMoved=0` for `AlreadyAdjacentMessage`.
  - [ ] `BuildHistoryMove` returns `Applied=false, SkippedReason="anchor_missing", AnchorFound=false` for `[WARN] AnchorMissingMessage`.
  - [ ] `BuildHistoryMove` returns `Applied=false, SkippedReason="partial_block"` for a `[WARN]` matching `PartialBlockMessagePrefix`.
  - [ ] `BuildHistoryMove` returns `Applied=false, SkippedReason="out_of_order"` for `OutOfOrderMessagePrefix`.
  - [ ] `BuildHistoryMove` returns `Applied=false, SkippedReason="not_adjacent"` for `NotAdjacentMessagePrefix`.
  - [ ] `BuildHistoryMove` returns `Applied=false, SkippedReason="not_found"` for `[INFO] NotFoundMessage`.
  - [ ] `BuildSectionPromotion` returns `null` when no `PromoteSectionsRule` entries are present.
  - [ ] `BuildSectionPromotion` returns `Applied=true, SkippedReason=null, AnchorFound=true` with the correct counts (`SectionsPromoted`, `SubsectionsPromoted`) parsed from the summary message.
  - [ ] `BuildSectionPromotion` returns `Applied=false, SkippedReason="anchor_missing", AnchorFound=false` for `[WARN] AnchorMissingMessage`.
  - [ ] `BuildFormatting` correctly populates `HistoryMove` and `SectionPromotion` on the returned `DiagnosticFormatting` when both rules ran.
  - [ ] `BuildFormatting` leaves both Phase 3 properties as `null` when neither rule was registered (existing Phase 1+2 documents continue to serialize unchanged).
  - [ ] Existing Phase 1+2 `DiagnosticWriterTests` still pass with no regression.
- Integration tests:
  - [ ] Construct a synthetic report with a mix of Phase 1+2 entries plus Phase 3 entries (one `[INFO]` from each rule, one `[WARN]` from each rule); call `DiagnosticWriter.Build`; assert the resulting `DiagnosticDocument.Formatting` has all four Phase 1+2 properties plus `HistoryMove` and `SectionPromotion` populated correctly; assert JSON round-trip equality.
- Test coverage target: >=80%
- All tests must pass

## Success Criteria
- All tests passing
- Test coverage >=80%
- Every documented rule outcome (info / each warn variant) maps to a unique `DiagnosticHistoryMove` or `DiagnosticSectionPromotion` shape, verified by a dedicated test case
- Message constants are referenced by name in `DiagnosticWriter`; renaming a constant in `MoveHistoryRule` or `PromoteSectionsRule` breaks compilation, never silently regresses the JSON
- Existing Phase 1+2 diagnostic JSON output is byte-identical for documents that do not exercise the new rules
