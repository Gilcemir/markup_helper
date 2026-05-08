---
status: completed
title: Implement MoveHistoryRule
type: backend
complexity: high
dependencies:
  - task_02
---

# Task 03: Implement MoveHistoryRule

## Overview
Add `MoveHistoryRule` as a new `IFormattingRule` (Optional severity, pipeline position #10) that detects the three consecutive paragraphs `Received: …`, `Accepted: …`, `Published: …` in the front-matter region and reorders them to sit immediately above the `INTRODUCTION` anchor. The rule is the only Phase 3 rule that may reorder paragraphs (per ADR-002 / INV-01); on any ambiguity (anchor missing, partial block, out-of-order, non-adjacent) it does nothing and emits a precise `[WARN]` so the editor can inspect manually.

<critical>
- ALWAYS READ the PRD and TechSpec before starting
- REFERENCE TECHSPEC for implementation details — do not duplicate here
- FOCUS ON "WHAT" — describe what needs to be accomplished, not how
- MINIMIZE CODE — show code only to illustrate current structure or problem areas
- TESTS REQUIRED — every task MUST include tests in deliverables
</critical>

<requirements>
- MUST create `MoveHistoryRule` as `public sealed class` implementing `IFormattingRule` with `Severity = RuleSeverity.Optional` and `Name = nameof(MoveHistoryRule)`.
- MUST detect a history paragraph using the case-insensitive regex `^(received|accepted|published)\s*[:\-]\s*.+` on the trimmed paragraph text.
- MUST require the three matching paragraphs to be adjacent (no non-empty paragraph between them) and in the fixed order `Received → Accepted → Published`.
- MUST consider only the FIRST qualifying block in body order; markers occurring after the `INTRODUCTION` anchor MUST be ignored (treated as body content).
- MUST resolve the destination anchor using `BodySectionDetector.FindIntroductionAnchor`. If null, MUST emit `[WARN]` with `AnchorMissingMessage` and return without mutation.
- MUST early-return with `[INFO] AlreadyAdjacentMessage` when the three history paragraphs are already the immediate predecessors of the `INTRODUCTION` paragraph (idempotency on re-runs).
- MUST detach the three history paragraphs and re-insert them as immediate predecessors of the anchor in their original order. MUST NOT mutate paragraph properties (`pPr`, `spacing`, `pageBreakBefore`, `pStyle`).
- MUST emit precise `[WARN]` messages for each documented skip condition: `partial_block`, `out_of_order`, `not_adjacent`. MUST emit `[INFO] NotFoundMessage` when no `Received:` marker exists at all (silent skip — not a warning).
- MUST expose every emitted message string as a `public const string` so `DiagnosticWriter` (task_06) can match by reference rather than by literal.
- MUST satisfy INV-01: the multiset of non-empty trimmed body texts MUST be identical before and after the rule runs, in every test case (happy path AND every skip case).
- MUST NOT throw under any input; rule-level exceptions are caught by the pipeline as `[ERROR]` and the document is preserved.
</requirements>

## Subtasks
- [x] 3.1 Create `MoveHistoryRule.cs` with class scaffold, message constants, and `Apply` method skeleton.
- [x] 3.2 Implement history-block detection: regex match per paragraph, collection of candidates before the anchor, adjacency check, order check.
- [x] 3.3 Implement anchor lookup via `BodySectionDetector.FindIntroductionAnchor` and the early-return on adjacency.
- [x] 3.4 Implement the OOXML reorder operation (detach the three paragraphs from their parent and re-insert before the anchor in original order).
- [x] 3.5 Wire each skip condition to the corresponding `[WARN]`/`[INFO]` report message constant.
- [x] 3.6 Extend `Phase3DocxFixtureBuilder` with helpers to construct documents with the seven history-block patterns: well-formed, partial (only Received+Published), out-of-order (Published before Received), not-adjacent (extra paragraph between), missing entirely, already-adjacent (idempotent), and history-after-anchor (must be ignored).
- [x] 3.7 Add `MoveHistoryRuleTests.cs` covering each pattern plus the INV-01 multiset assertion.

## Implementation Details
New files:
- `DocFormatter.Core/Rules/MoveHistoryRule.cs`.
- `DocFormatter.Tests/MoveHistoryRuleTests.cs`.

Modified files:
- `DocFormatter.Tests/Fixtures/Phase3/Phase3DocxFixtureBuilder.cs` — add history-block construction helpers.

Refer to TechSpec section "Core Interfaces" for the message constant declarations and class shape, to PRD "Article history block move" for the detection rules and skip conditions, and to PRD "Report messages" for the exact `[INFO]`/`[WARN]` text. The OOXML reorder uses `Paragraph.Remove()` followed by `parent.InsertBefore(paragraph, anchor)`, in original order — both methods are `DocumentFormat.OpenXml` primitives already used by Phase 1+2 rules.

DI registration is intentionally OUT OF SCOPE for this task; it lives in task_07 alongside the integration test.

### Relevant Files
- `DocFormatter.Core/Rules/BodySectionDetector.cs` — Provides `FindIntroductionAnchor` (implemented in task_02).
- `DocFormatter.Core/Pipeline/IFormattingRule.cs` — Defines the `Apply(WordprocessingDocument, FormattingContext, IReport)` contract.
- `DocFormatter.Core/Pipeline/IReport.cs` — Provides `Info(rule, message)` and `Warn(rule, message)` for emitting structured entries.
- `DocFormatter.Core/Rules/RewriteAbstractRule.cs` — Reference pattern for an Optional rule that mutates paragraphs without reordering; mirror its message-constant pattern.
- `DocFormatter.Core/Rules/EnsureAuthorBlockSpacingRule.cs` — Reference pattern for a rule that inserts/relocates paragraphs in the body.
- `DocFormatter.Tests/Fixtures/Phase3/Phase3DocxFixtureBuilder.cs` — Extended with history-block construction helpers.

### Dependent Files
- `DocFormatter.Core/Reporting/DiagnosticWriter.cs` (task_06) — Will read the public message constants by reference (not by literal string) to reconstruct the `DiagnosticHistoryMove` record from report entries.
- `DocFormatter.Cli/CliApp.cs` (task_07) — Will register `MoveHistoryRule` via `services.AddTransient<IFormattingRule, MoveHistoryRule>()` after `LocateAbstractAndInsertElocationRule`.

### Related ADRs
- [ADR-001: Two discrete Optional rules over a single combined rule](../adrs/adr-001-two-discrete-rules.md) — This task creates one of the two rules mandated by ADR-001.
- [ADR-002: Strict content preservation invariant (INV-01)](../adrs/adr-002-content-preservation-invariant.md) — Binds the rule to the multiset-preservation invariant; the unit tests must assert it.
- [ADR-004: `INTRODUCTION` as detection anchor](../adrs/adr-004-introduction-as-detection-anchor.md) — Defines the destination anchor used both for placement and for confining history-detection scope.

## Deliverables
- `MoveHistoryRule` class with `Apply` implementation and all message constants.
- Extended `Phase3DocxFixtureBuilder` with the seven history-block patterns.
- `MoveHistoryRuleTests` with the test cases listed in `## Tests`, including the INV-01 multiset assertion in EVERY test.
- Unit tests with 80%+ coverage **(REQUIRED)**.
- Integration tests verifying the rule on synthetic full-document fixtures alongside Phase 1+2 paragraphs **(REQUIRED)**.

## Tests
- Unit tests:
  - [x] Happy path: well-formed three-paragraph history block before `INTRODUCTION` is moved to immediately before the anchor; paragraph count unchanged; `[INFO] MovedMessagePrefix` emitted with anchor position.
  - [x] Idempotent: running `Apply` twice on the same input produces identical document state; the second invocation emits `[INFO] AlreadyAdjacentMessage` with no mutation.
  - [x] Already-adjacent at first run: input where history is already immediately before `INTRODUCTION` triggers `[INFO] AlreadyAdjacentMessage` with no mutation.
  - [x] Anchor missing: emits `[WARN] AnchorMissingMessage`; document unchanged; INV-01 holds.
  - [x] Partial block (only `Received` and `Published`, no `Accepted`): emits `[WARN]` whose message starts with `PartialBlockMessagePrefix`; no mutation; INV-01 holds.
  - [x] Out of order (`Published` paragraph appears before `Received`): emits `[WARN]` whose message starts with `OutOfOrderMessagePrefix`; no mutation; INV-01 holds.
  - [x] Not adjacent (a non-empty paragraph appears between two markers): emits `[WARN]` whose message starts with `NotAdjacentMessagePrefix`; no mutation; INV-01 holds.
  - [x] Not found (no `Received:` paragraph anywhere): emits `[INFO] NotFoundMessage`; no mutation; INV-01 holds.
  - [x] History markers after anchor are ignored (a `Received:` body paragraph after `INTRODUCTION` does NOT trigger detection if no front-matter block exists).
  - [x] Regex case-insensitivity: `received:`, `RECEIVED:`, `Received -` all match; `Received` (no colon/dash) does not match.
  - [x] Paragraph properties on the moved paragraphs are preserved (snapshot `pPr` before/after; assert no mutation).
  - [x] INV-01 assertion: in every test above, `MultisetOfBodyTexts(before) == MultisetOfBodyTexts(after)` over non-empty trimmed `<w:t>` values.
- Integration tests:
  - [x] Construct a synthetic `WordprocessingDocument` with Phase 1+2 front-matter (DOI, title, authors, abstract) plus a well-formed history block plus an `INTRODUCTION` and body sections; run `MoveHistoryRule.Apply`; assert the history block is now immediately before `INTRODUCTION`, the rest of the document is unchanged, and the report contains the expected `[INFO]` entry.
- Test coverage target: >=80%
- All tests must pass

## Success Criteria
- All tests passing
- Test coverage >=80%
- The rule is idempotent on re-runs (verified by the idempotent test case)
- Every test case asserts the INV-01 multiset invariant
- All emitted messages are `public const string` and consumed by reference (verified by task_06 once it lands)
- The rule does NOT throw on any documented edge case; pipeline-level `[ERROR]` capture is not exercised by these tests
