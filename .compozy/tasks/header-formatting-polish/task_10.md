---
status: completed
title: Wire Phase 2 rules in CLI DI and add end-to-end integration test
type: backend
complexity: medium
dependencies:
  - task_05
  - task_06
  - task_07
  - task_08
  - task_09
---

# Task 10: Wire Phase 2 rules in CLI DI and add end-to-end integration test

## Overview
Register the four new rules in `CliApp.BuildServiceProvider` in the order defined by ADR-001, and add an end-to-end `CliIntegrationTests` fixture that runs the full pipeline against a synthetic `*`-marked article (or a small production article from `examples/`) and asserts the body's alignment, spacing, abstract, and corresponding-author outputs match the journal format. This task closes Phase 1 by verifying all four behaviors land together.

<critical>
- ALWAYS READ the PRD and TechSpec before starting
- REFERENCE TECHSPEC for implementation details — do not duplicate here
- FOCUS ON "WHAT" — describe what needs to be accomplished, not how
- MINIMIZE CODE — show code only to illustrate current structure or problem areas
- TESTS REQUIRED — every task MUST include tests in deliverables
</critical>

<requirements>
- MUST register the four new rules in `CliApp.BuildServiceProvider` in the exact order defined by TechSpec "API Surface (CLI)" / ADR-001:
  1. `ExtractTopTableRule`
  2. `ParseHeaderLinesRule`
  3. `ExtractAuthorsRule`
  4. `ExtractCorrespondingAuthorRule` *(new)*
  5. `RewriteHeaderMvpRule`
  6. `ApplyHeaderAlignmentRule` *(new)*
  7. `EnsureAuthorBlockSpacingRule` *(new)*
  8. `RewriteAbstractRule` *(new)*
  9. `LocateAbstractAndInsertElocationRule`
- MUST NOT change the order of the five MVP rules.
- MUST add an end-to-end `CliIntegrationTests` fixture using a `*`-marked synthetic document OR a small production article from `examples/` (per TechSpec "Testing Approach → Integration Tests").
- The end-to-end test MUST assert: DOI paragraph carries `<w:jc w:val="right"/>`, section paragraph `right`, title paragraph `center`; exactly one blank paragraph between the last author paragraph and the first affiliation; a bold-only `Abstract` heading paragraph followed by a body paragraph without the structural italic wrapper; a `Corresponding author: <email>` paragraph immediately above the heading.
- The integration test MUST run on a real `WordprocessingDocument` opened from a `MemoryStream` (no mocks).
- Existing `CliIntegrationTests` MUST keep passing without modification beyond the additive new fixture.
</requirements>

## Subtasks
- [x] 10.1 Append the four `services.AddTransient<IFormattingRule, ...>()` lines in the prescribed order in `CliApp.BuildServiceProvider`.
- [x] 10.2 Build (or extend) a fixture factory for an abstract paragraph and an affiliation paragraph carrying the `* E-mail:` trailer.
- [x] 10.3 Add the new end-to-end test asserting the four behavior groups together.
- [x] 10.4 Run the eleven `examples/` articles through the new pipeline manually (or via a small batch test) and capture the resulting diagnostic JSONs for the editor's review (PRD Phase 1 success criterion). Document the observed outcomes — this is verification, not part of the unit-test suite.
- [x] 10.5 Verify the existing `CliIntegrationTests` still pass.

## Implementation Details
Modify `DocFormatter.Cli/CliApp.cs` (registration) and `DocFormatter.Tests/CliIntegrationTests.cs` (new fixture). Reuse `DocFormatter.Tests/Fixtures/Authors/AuthorsParagraphFactory.cs` and add a new `AbstractParagraphFactory.cs` (or equivalent helper) under `Fixtures/`. See TechSpec "Component Overview" final row for the registration shape and "Testing Approach → Integration Tests" for fixture guidance.

### Relevant Files
- `DocFormatter.Cli/CliApp.cs` — DI registration (`BuildServiceProvider`, lines around 199–215).
- `DocFormatter.Tests/CliIntegrationTests.cs` — existing end-to-end test pattern.
- `DocFormatter.Tests/Fixtures/Authors/AuthorsParagraphFactory.cs` — fixture factory to extend.
- `examples/` — production articles available for manual verification.

### Dependent Files
- `DocFormatter.Cli/FileProcessor.cs` — wraps the pipeline; no change required if rules are simply registered.
- All four new rule files (tasks 05–08) and the diagnostic extension (task 09) — must be present and compile cleanly.

### Related ADRs
- [ADR-001: Four discrete Optional rules](../adrs/adr-001-four-discrete-rules.md) — pipeline ordering authority.
- [ADR-004: Additive `formatting` section](../adrs/adr-004-diagnostic-formatting-section.md) — verifies the diagnostic JSON shape under the integration test.

## Deliverables
- `CliApp.BuildServiceProvider` registering the four new rules in the prescribed order.
- New `CliIntegrationTests` fixture covering the four behavior groups end-to-end.
- New abstract/affiliation fixture factory under `DocFormatter.Tests/Fixtures/`.
- Manual verification log against the eleven `examples/` articles (PRD Phase 1 success criterion) — captured as a short note in the task PR description.
- Unit tests with 80%+ coverage **(REQUIRED)**.
- Integration tests for the four behaviors **(REQUIRED)**.

## Tests
- Unit tests:
  - [x] `CliApp.BuildServiceProvider` resolves an `IEnumerable<IFormattingRule>` whose order matches the TechSpec sequence (assertable via reflection or a thin test helper).
  - [x] Each new rule type can be instantiated through the DI container (no missing constructor dependencies).
- Integration tests:
  - [x] End-to-end: a `*`-marked synthetic `.docx` flows through the pipeline; asserting each of: DOI right-aligned, section right-aligned, title centered, exactly one blank paragraph between authors and affiliations, bold `Abstract` heading paragraph + plain-text body paragraph, `Corresponding author: <email>` paragraph in the front matter, ELOCATION paragraph between the email line and the heading (actual data flow — see task memory).
  - [x] End-to-end: a document with no `*` marker still produces ✓ output with the alignment, spacing, and abstract behaviors applied; the corresponding-author paragraph is absent.
  - [x] End-to-end: a `Resumo` source document produces an `Abstract` heading paragraph; body language stays Portuguese.
  - [x] End-to-end: the diagnostic JSON's `formatting` section is `null` when all four rules succeed silently and populated correctly when at least one warns (drives task_09 wiring through the CLI surface).
- Test coverage target: >=80%
- All tests must pass

## Success Criteria
- All tests passing
- Test coverage >=80%
- Pipeline registration matches ADR-001 ordering exactly.
- The eleven `examples/` articles run through the new pipeline without regression; the editor accepts at least 9/11 outputs without manual editing of the four behaviors covered (PRD Phase 1 success criterion).
- Diagnostic JSON shape for warn/error runs matches the ADR-004 schema end-to-end.
