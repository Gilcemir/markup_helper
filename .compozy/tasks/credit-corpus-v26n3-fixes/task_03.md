---
status: pending
title: DocxSourceReader ignores `[p]`/`[/p]` in trailing sections and exposes CreditHeaderFound
type: bugfix
complexity: low
dependencies: []
---

# Task 3: DocxSourceReader ignores `[p]`/`[/p]` in trailing sections and exposes CreditHeaderFound

## Overview
Stop `ExtractSection` from ending a section at a `[p]…[/p]` body paragraph, and record on `DocxSource` whether the CREDIT STATEMENT header was present. This recovers 5536's statement, which today is silently reported as absent, and gives the injector what it needs to WARN on an emptied section (INV-02).

<critical>
- ALWAYS READ the PRD and TechSpec before starting
- REFERENCE TECHSPEC for implementation details — do not duplicate here
- FOCUS ON "WHAT" — describe what needs to be accomplished, not how
- MINIMIZE CODE — show code only to illustrate current structure or problem areas
- TESTS REQUIRED — every task MUST include tests in deliverables
</critical>

<requirements>
- MUST strip `[p]` and `[/p]` (case-insensitive) from each segment before the bracket/REFERENCES break test in `ExtractSection`, for both trailing sections.
- MUST still end the section at any other bracket tag (`[refs]`, `[ack]`, `[corresp]`, `[sectitle]`, …) or a `REFERENCES` heading.
- MUST add `bool CreditHeaderFound` to `DocxSource`, true when the `CREDIT STATEMENT` header segment exists regardless of body content.
- MUST keep `CreditStatementRaw` null when the body is empty.
- MUST keep the docx read-only (ADR-002 of phase-3-jats-tags).
</requirements>

## Subtasks
- [ ] 3.1 Add the `[p]`/`[/p]` strip step to `ExtractSection`.
- [ ] 3.2 Add `CreditHeaderFound` to `DocxSource` and set it in `Parse`.
- [ ] 3.3 Add reader tests for the `[p]` body, the `[refs]`/`[ack]` terminators, and the header-with-empty-body case.
- [ ] 3.4 Run the full suite.

## Implementation Details
Modify `DocFormatter.Core/Jats/DocxSourceReader.cs` (`ExtractSection`, `Parse`) and `DocFormatter.Core/Jats/DocxSource.cs`. Tests use the public `DocxSourceReader.Parse(IReadOnlyList<string>)` overload with paragraph lists, no real docx needed. See TechSpec "Reader (`ExtractSection`)".

### Relevant Files
- `DocFormatter.Core/Jats/DocxSourceReader.cs` — section extraction logic.
- `DocFormatter.Core/Jats/DocxSource.cs` — model gaining `CreditHeaderFound`.
- `DocFormatter.Tests/Jats/DocxSourceReaderTests.cs` — existing paragraph-list tests to extend.

### Dependent Files
- `DocFormatter.Core/Jats/CreditRolesInjector.cs` — reads `CreditHeaderFound` in task_04.
- `DocFormatter.Core/Jats/DocumentPairer.cs` — constructs `DocxSource` via the reader; no change expected.
- `DocFormatter.Tests/Fixtures/Phase3/Phase3DocxFixtureBuilder.cs` — may gain a `[p]`-tagged CREDIT paragraph builder for task_04's integration fixture.

### Related ADRs
- [ADR-003: Widen the author-keyed CREDIT grammar with shape-based lookback, and never drop a present statement](../adrs/adr-003.md) — declares INV-02 and the `[p]` handling.

## Deliverables
- `ExtractSection` tolerant to `[p]`/`[/p]`.
- `DocxSource.CreditHeaderFound`.
- Unit tests with 80%+ coverage **(REQUIRED)**
- Integration tests: existing pairing/pipeline tests green **(REQUIRED)**

## Tests
- Unit tests:
  - [ ] Paragraphs ["CREDIT STATEMENT", "[p]GHZ; IRC: Conceptualization.[/p]", "[refs][sectitle]REFERENCES[/sectitle]"] → `CreditStatementRaw` = "GHZ; IRC: Conceptualization." and `CreditHeaderFound` = true.
  - [ ] Paragraphs ["CREDIT STATEMENT", "X: Methodology.", "[refs]..."] → raw stops before `[refs]`.
  - [ ] Paragraphs ["CREDIT STATEMENT", "X: Methodology.", "[ack][sectitle]ACKNOWLEDGEMENTS[/sectitle]"] → raw stops before `[ack]`.
  - [ ] Paragraphs ["CREDIT STATEMENT", "[refs]..."] → raw null, `CreditHeaderFound` true.
  - [ ] Paragraphs without the header → raw null, `CreditHeaderFound` false.
  - [ ] DATA AVAILABILITY body tagged `[p]…[/p]` is also extracted.
  - [ ] Mixed case `[P]…[/P]` is stripped.
- Integration tests:
  - [ ] `DocumentPairerTests` and `Phase3PipelineIntegrationTests` pass unchanged.
- Test coverage target: >=80%
- All tests must pass

## Success Criteria
- All tests passing
- Test coverage >=80%
- Reading 5536.docx (manual check in task_09) yields the full statement instead of null.
