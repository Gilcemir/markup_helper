---
status: completed
title: LocateAbstractAndInsertElocationRule with bilingual heuristic
type: backend
complexity: medium
dependencies:
  - task_04
  - task_05
---

# Task 10: LocateAbstractAndInsertElocationRule with bilingual heuristic

## Overview
Implement the rule that scans the document body for the first paragraph whose first run is bold and whose text starts with `Abstract` or `Resumo` (case-insensitive), then inserts a new paragraph immediately above it carrying the value of `ctx.ElocationId`. If no Abstract paragraph is found, the rule logs `[WARN]` and leaves the document unchanged; severity is `Optional`.

<critical>
- ALWAYS READ the PRD and TechSpec before starting
- REFERENCE TECHSPEC for implementation details — do not duplicate here
- FOCUS ON "WHAT" — describe what needs to be accomplished, not how
- MINIMIZE CODE — show code only to illustrate current structure or problem areas
- TESTS REQUIRED — every task MUST include tests in deliverables
</critical>

<requirements>
- 1. The rule MUST scan only the document body — explicitly skip footnotes, headers, and footers.
- 2. The match is the first paragraph whose first run satisfies: `RunProperties.Bold == true` AND `InnerText.TrimStart()` starts with one of `FormattingOptions.AbstractMarkers` (case-insensitive comparison).
- 3. On match, insert a new paragraph immediately above the matched paragraph. The inserted paragraph contains a single run with `ctx.ElocationId` as plain text and no special formatting.
- 4. If `ctx.ElocationId` is null or empty, the rule MUST log `[WARN]` and skip insertion.
- 5. If no paragraph matches the heuristic, the rule MUST log `[WARN]` ("Abstract paragraph not found, ELOCATION not inserted") and leave the document unchanged.
- 6. Severity is `Optional`; this rule never aborts the pipeline.
- 7. The rule MUST NOT modify the matched Abstract paragraph itself.
</requirements>

## Subtasks
- [x] 10.1 Create `DocFormatter.Core/Rules/LocateAbstractAndInsertElocationRule.cs` implementing `IFormattingRule` with `Severity=Optional`.
- [x] 10.2 Implement the body-only paragraph scan with the bold-prefix heuristic from `FormattingOptions.AbstractMarkers`.
- [x] 10.3 Insert the ELOCATION paragraph immediately above the match.
- [x] 10.4 Handle the four "skip" cases (no marker found, empty ElocationId, etc.) with the right `[WARN]` messages.
- [x] 10.5 Add xUnit tests covering: English match, Portuguese match, mixed-case match, no match (warn), Abstract appearing in a footnote (skipped), null `ElocationId` (warn).

## Implementation Details
File is new under `DocFormatter.Core/Rules/`. See TechSpec "Rules" rule #6 and "Known Risks" for the footnote-exclusion guidance.

### Relevant Files
- `.compozy/tasks/header-metadata-extraction/_techspec.md` — "Rules" rule #6
- `.compozy/tasks/header-metadata-extraction/_prd.md` — Core Features #5 (ELOCATION position)
- `.compozy/tasks/header-metadata-extraction/_prd.md` — Open Questions: Abstract location heuristic (resolved)

### Dependent Files
- `DocFormatter.Core/Rules/LocateAbstractAndInsertElocationRule.cs` (new)
- `DocFormatter.Tests/LocateAbstractAndInsertElocationRuleTests.cs` (new)

### Related ADRs
- [ADR-001: Esqueleto alinhado ao spec](adrs/adr-001.md)

## Deliverables
- `LocateAbstractAndInsertElocationRule` implementation
- xUnit tests for each scenario in subtask 10.5
- Unit tests with 80%+ coverage **(REQUIRED)**
- Integration tests for [pipeline + Abstract-bearing fixture] **(REQUIRED)**

## Tests
- Unit tests:
  - [x] Body where the third paragraph has bold first run "Abstract — On the behavior...": ELOCATION paragraph inserted at index 2 (above the match).
  - [x] Body where the third paragraph has bold first run "Resumo — Sobre o comportamento...": ELOCATION inserted, message language ignored.
  - [x] Body where the matching first run reads "ABSTRACT": case-insensitive match still succeeds.
  - [x] Body with no bold "Abstract"/"Resumo" paragraph: no insertion, `[WARN]` "Abstract paragraph not found".
  - [x] Body whose only "Abstract"-prefixed paragraph is inside a footnote: rule does not match, `[WARN]` emitted.
  - [x] Context with `ElocationId=null`: rule does not insert, `[WARN]` "ElocationId is null, skipping insertion".
  - [x] Context with `ElocationId=""`: same behavior as null.
- Integration tests:
  - [x] Full pipeline run on a fixture with both an Abstract paragraph and a populated `ElocationId`: ELOCATION line appears immediately above Abstract; downstream content unchanged.
- Test coverage target: >=80%
- All tests must pass

## Success Criteria
- All tests passing
- Test coverage >=80%
- Footnote/header/footer paragraphs are not considered for matching
- Empty/null `ElocationId` is handled with a `[WARN]`, not an exception
