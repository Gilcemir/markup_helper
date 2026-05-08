---
status: completed
title: Register Phase 3 rules in DI and add end-to-end integration test
type: backend
complexity: medium
dependencies:
  - task_03
  - task_04
  - task_06
---

# Task 07: Register Phase 3 rules in DI and add end-to-end integration test

## Overview
Wire `MoveHistoryRule` and `PromoteSectionsRule` into the CLI's dependency-injection graph at the end of the existing `IFormattingRule` registration block (positions #10 and #11 per the TechSpec) and add an end-to-end integration test that exercises the full Phase 1+2+3 pipeline on a synthetic fixture. This is the only task that touches `DocFormatter.Cli`, and its success criterion is that the eleven-article corpus runs through the pipeline producing the diagnostic JSON shape specified in the PRD.

<critical>
- ALWAYS READ the PRD and TechSpec before starting
- REFERENCE TECHSPEC for implementation details — do not duplicate here
- FOCUS ON "WHAT" — describe what needs to be accomplished, not how
- MINIMIZE CODE — show code only to illustrate current structure or problem areas
- TESTS REQUIRED — every task MUST include tests in deliverables
</critical>

<requirements>
- MUST register `MoveHistoryRule` via `services.AddTransient<IFormattingRule, MoveHistoryRule>()` immediately after the existing `LocateAbstractAndInsertElocationRule` registration in `CliApp.BuildServiceProvider`.
- MUST register `PromoteSectionsRule` via `services.AddTransient<IFormattingRule, PromoteSectionsRule>()` immediately after the `MoveHistoryRule` registration.
- MUST preserve the existing relative order of all Phase 1+2 registrations; the only change is two appended `AddTransient` lines.
- MUST verify (via the existing `BuildServiceProvider_RegistersFormattingRulesInTechSpecOrder` test or its equivalent) that the resolved sequence of `IFormattingRule` instances ends with `MoveHistoryRule` then `PromoteSectionsRule`. If a rule-order assertion test exists, MUST extend it to include the two new entries; otherwise add a new test asserting the Phase 1+2+3 order.
- MUST add an integration test in `CliIntegrationTests.cs` that uses a synthetic fixture (built from `Phase3DocxFixtureBuilder` and existing Phase 1+2 fixtures) to exercise:
  1. A document with a well-formed history block, an `INTRODUCTION` anchor, multiple sections, multiple sub-sections, and Phase 1+2 front-matter; verify the output document has the history block immediately above `INTRODUCTION`, sections at 16pt center, sub-sections at 14pt center.
  2. The diagnostic JSON file (when triggered by a Phase 1+2 warning unrelated to Phase 3) contains `formatting.history_move.applied=true, paragraphs_moved=3` and `formatting.section_promotion.applied=true` with non-zero counts.
- MUST add a second integration scenario: an `anchor_missing` document (no `INTRODUCTION`) where both rules emit `[WARN]` and the diagnostic JSON shows `formatting.history_move.skipped_reason="anchor_missing"` and `formatting.section_promotion.skipped_reason="anchor_missing"`.
- MUST NOT load any real `examples/*.docx` file in tests; per ADR / TechSpec testing decision, all integration tests use synthetic fixtures (`Phase3DocxFixtureBuilder`).
- MUST satisfy INV-01 in the integration tests: assert the multiset of non-empty trimmed body texts is preserved end-to-end across the full Phase 1+2+3 pipeline.
</requirements>

## Subtasks
- [x] 7.1 Add the two `services.AddTransient<IFormattingRule, ...>()` registrations in `CliApp.BuildServiceProvider` after `LocateAbstractAndInsertElocationRule`.
- [x] 7.2 Update or add a rule-order test in `CliIntegrationTests.cs` (or its equivalent) asserting the final pipeline order ends with `MoveHistoryRule, PromoteSectionsRule`.
- [x] 7.3 Add the happy-path Phase 1+2+3 integration test using `Phase3DocxFixtureBuilder` plus existing Phase 1+2 fixture helpers.
- [x] 7.4 Add the `anchor_missing` integration test verifying both Phase 3 rules emit `[WARN]` with the correct `skipped_reason`.
- [x] 7.5 Verify INV-01 holds across the full pipeline run by snapshotting the body-text multiset before invoking the CLI and re-comparing against the output document.

## Implementation Details
Modified files:
- `DocFormatter.Cli/CliApp.cs` — append two `AddTransient` registrations.
- `DocFormatter.Tests/CliIntegrationTests.cs` — extend the existing rule-order test (if present) and add two new integration test methods.
- `DocFormatter.Tests/Fixtures/Phase3/Phase3DocxFixtureBuilder.cs` — extend if a full Phase 1+2+3 fixture builder method is needed; otherwise compose existing builders.

Refer to TechSpec section "Development Sequencing" step 11 for the integration test scope, to PRD "Diagnostic JSON extension" for the expected JSON shape after a successful Phase 3 run, and to the existing `Run_Phase2_WithCorrespondingMarker_AppliesAllFourBehaviorsEndToEnd` test (in `CliIntegrationTests.cs`) as the reference pattern for full-pipeline integration tests.

The `services.AddTransient` ordering is critical: the pipeline executes rules in registration order. Phase 3 rules must run AFTER `LocateAbstractAndInsertElocationRule` so they see the post-Phase-2 paragraph references in `FormattingContext`. Insert at the END of the existing block, in the order `MoveHistoryRule` then `PromoteSectionsRule` (per the TechSpec).

### Relevant Files
- `DocFormatter.Cli/CliApp.cs` — Modified; the existing `services.AddTransient<IFormattingRule, ...>()` block (lines ~199–219 per the codebase exploration) is the insertion site.
- `DocFormatter.Tests/CliIntegrationTests.cs` — Modified; existing tests `Run_Phase2_WithCorrespondingMarker_AppliesAllFourBehaviorsEndToEnd` and `BuildServiceProvider_RegistersFormattingRulesInTechSpecOrder` (or equivalents) are the reference patterns.
- `DocFormatter.Tests/Fixtures/Phase3/Phase3DocxFixtureBuilder.cs` — Possibly modified; provides the fixture surface for both new integration scenarios.
- `DocFormatter.Tests/Fixtures/Phase2/Phase2DocxFixtureBuilder.cs` — Reference; the integration test composes Phase 2 front-matter with Phase 3 body content.
- `DocFormatter.Core/Rules/MoveHistoryRule.cs` (task_03) — Registered here.
- `DocFormatter.Core/Rules/PromoteSectionsRule.cs` (task_04) — Registered here.

### Dependent Files
- None — this is the terminal task in the dependency chain.

### Related ADRs
- [ADR-001: Two discrete Optional rules over a single combined rule](../adrs/adr-001-two-discrete-rules.md) — Confirms the two-rule pipeline registration pattern.
- [ADR-002: Strict content preservation invariant (INV-01)](../adrs/adr-002-content-preservation-invariant.md) — End-to-end integration tests must assert multiset preservation across the full pipeline.
- [ADR-004: `INTRODUCTION` as detection anchor](../adrs/adr-004-introduction-as-detection-anchor.md) — Defines the `anchor_missing` scenario that the second integration test exercises.

## Deliverables
- Two `AddTransient` registrations appended to `CliApp.BuildServiceProvider`.
- Updated rule-order test confirming the pipeline now ends with `MoveHistoryRule, PromoteSectionsRule`.
- Two new integration tests in `CliIntegrationTests.cs`: happy-path Phase 1+2+3 and `anchor_missing` Phase 3.
- Unit tests with 80%+ coverage on the rule-order and integration paths **(REQUIRED)**.
- Integration tests for the end-to-end Phase 1+2+3 pipeline **(REQUIRED)**.

## Tests
- Unit tests:
  - [ ] `BuildServiceProvider_RegistersFormattingRulesInTechSpecOrder` (extended): the resolved `IFormattingRule[]` ends with `MoveHistoryRule` followed by `PromoteSectionsRule`.
  - [ ] All Phase 1+2 rules retain their relative order (no regression in the existing assertion).
- Integration tests:
  - [ ] **Happy path**: synthetic fixture with Phase 1+2 front-matter, well-formed history block, `INTRODUCTION` anchor, two sections (`MATERIAL AND METHODS`, `RESULTS`), one sub-section, and one table containing a bold-caps paragraph; running `CliApp.Run` end-to-end produces an output `.docx` where (a) history is immediately before `INTRODUCTION`; (b) sections have `<w:jc w:val="center"/>` and `<w:sz w:val="32"/>`; (c) sub-section has `<w:sz w:val="28"/>`; (d) table-nested paragraph is unchanged; (e) Phase 1+2 mutations are preserved.
  - [ ] **Anchor missing**: synthetic fixture without `INTRODUCTION`; CLI run completes; both Phase 3 rules emit `[WARN]`; the diagnostic JSON contains `formatting.history_move.applied=false, skipped_reason="anchor_missing"` and `formatting.section_promotion.skipped_reason="anchor_missing"`; INV-01 holds (no body text lost).
  - [ ] **Diagnostic JSON shape**: after the happy-path run, parse the JSON and assert it matches the structure documented in the PRD "Diagnostic JSON extension" example, including all Phase 1+2 keys plus the two new Phase 3 keys.
  - [ ] **INV-01 end-to-end**: in BOTH integration scenarios, the multiset of non-empty trimmed body texts in the output document equals the multiset in the input document.
- Test coverage target: >=80%
- All tests must pass

## Success Criteria
- All tests passing
- Test coverage >=80%
- Phase 3 rules execute in positions #10 and #11 of the pipeline as specified by the TechSpec
- The end-to-end integration test produces a `DiagnosticDocument` whose JSON matches the PRD example shape
- INV-01 holds end-to-end on every integration scenario
- The full Phase 1+2+3 pipeline runs without exceptions on the synthetic fixtures; production validation against `examples/*.docx` is deferred to manual editor review (out of scope for this task per the TechSpec)
