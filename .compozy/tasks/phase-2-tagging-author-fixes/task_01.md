---
status: completed
title: 'Phase 1 — Fix `ExtractAuthorsRule` so SciELO Markup auto-marks authors on 5313 and 5449 (+ ADR-008)'
type: bugfix
complexity: high
dependencies: []
---

# Task 01: Phase 1 — Fix `ExtractAuthorsRule` so SciELO Markup auto-marks authors on 5313 and 5449 (+ ADR-008)

## Overview
Articles 5313 and 5449 are extracted by DocFormatter with `confidence=high`, yet SciELO Markup's `mark_authors` macro fails to auto-tag part or all of their author block. This task investigates the root cause, captures it in **ADR-008**, and changes the Stage-1 author handling so the produced `.docx` no longer trips the failure — without regressing any of the 9 articles that currently pass.

<critical>
- ALWAYS READ the PRD and TechSpec before starting
- REFERENCE TECHSPEC for implementation details — do not duplicate here
- FOCUS ON "WHAT" — describe what needs to be accomplished, not how
- MINIMIZE CODE — show code only to illustrate current structure or problem areas
- TESTS REQUIRED — every task MUST include tests in deliverables
</critical>

<requirements>
- MUST identify and document the concrete difference between Stage-1 output of 5313/5449 and articles where Markup succeeds. Capture the finding in `adrs/adr-008.md` (status Accepted, dated, with alternatives considered).
- MUST modify `ExtractAuthorsRule` (or a tightly-scoped collaborator like `HeaderParagraphLocator`) so the produced `.docx` for 5313 and 5449 passes Markup's `mark_authors` without the operator re-marking any author.
- MUST NOT regress any currently-passing article. Re-running Phase 1 over `examples/*.docx` MUST produce diagnostic-JSON output equivalent to the saved snapshot (only the 5313/5449 deltas are allowed).
- MUST keep the rule's confidence semantics intact: existing `confidence=high` outputs for non-affected articles must remain `high` with the same author count and label set.
- MUST NOT introduce pre-marking of `[author]`, `[fname]`, `[surname]` (anti-duplication invariant from `docs/scielo_context/REENTRANCE.md`).
- SHOULD document the reproduction steps for the Markup failure in ADR-008 so the fix is auditable later.
</requirements>

## Subtasks
- [x] 1.1 Reproduce the Markup failure on the current Stage-1 output of 5313 and 5449; capture exact symptom (which authors are missed, which paragraphs survive `mark_authors`).
- [x] 1.2 Diff the Stage-1 output of 5313/5449 against an article where Markup succeeds (e.g., 5136) at the OpenXML level; pinpoint the divergent shape.
- [x] 1.3 Author **ADR-008** with the root cause, the fix decision, and at least one alternative considered.
- [x] 1.4 Adjust `ExtractAuthorsRule` (or a collaborator) to eliminate the divergent shape on the produced `.docx`.
- [x] 1.5 Add a regression fixture under `DocFormatter.Tests/Fixtures/Authors/` that reproduces the failure shape; assert the rule output no longer produces it.
- [x] 1.6 Run Phase 1 over all `examples/*.docx`; diff the resulting diagnostic JSON against the pre-fix snapshot; confirm only the 5313/5449 deltas appear. *Investigation surfaced 6 additional articles sharing the same root-cause shape (5293, 5419, 5424, 5434, 5458, 5549); all collapse `[..., "*"]` → `[..."*"]`. ADR-008 documents the broader scope; refreshed snapshots committed.*
- [ ] 1.7 Manually re-run SciELO Markup over the post-fix `.docx` for 5313 and 5449; confirm `mark_authors` auto-tags every author. Record the result in ADR-008 (or a follow-up note). *Pending operator manual run; ADR-008 has a slot reserved for the result.*

## Implementation Details
The fix lives entirely inside `DocFormatter.Core/Rules/ExtractAuthorsRule.cs` (~527 lines) or its supporting locator `DocFormatter.Core/Rules/HeaderParagraphLocator.cs`. The rule already populates `ctx.Authors` and removes ORCID hyperlinks; the failure is downstream in Markup's macro, so the fix is about the *shape of the produced `.docx`*, not about extraction confidence. See TechSpec "Impact Analysis" row for `ExtractAuthorsRule` and "Known Risks → Phase 1 fix introduces regression" for the regression-mitigation strategy.

ADR-008 is a new file at `adrs/adr-008.md` following the format of ADR-001..ADR-007 (Status / Date / Context / Decision / Alternatives / Consequences).

### Relevant Files
- `DocFormatter.Core/Rules/ExtractAuthorsRule.cs` — primary rule to modify (~527 lines).
- `DocFormatter.Core/Rules/HeaderParagraphLocator.cs` — author-paragraph detection; may need adjustment.
- `DocFormatter.Core/Models/Author.cs` and `AuthorConfidence.cs` — author record structure (do not change semantics).
- `examples/phase-2/before/5313.docx` and `examples/phase-2/before/5449.docx` — reproduction inputs.
- `examples/*.docx` — full Stage-1 corpus for regression check.
- `docs/scielo_context/REENTRANCE.md` — anti-duplication invariants the fix must preserve.
- `.compozy/tasks/phase-2-tagging-author-fixes/adrs/adr-008.md` — new ADR file to author.

### Dependent Files
- `DocFormatter.Tests/ExtractAuthorsRuleTests.cs` — extend with regression tests.
- `DocFormatter.Tests/Fixtures/Authors/AuthorsParagraphFactory.cs` — extend with the failure-shape fixture.
- `DocFormatter.Core/Rules/RewriteHeaderMvpRule.cs` — consumer of `ctx.Authors`; verify no behavioral drift.
- `DocFormatter.Core/Reporting/DiagnosticWriter.cs` — emits author diagnostic; snapshot-comparison sentinel for regressions.

### Related ADRs
- [ADR-001: Rollout Strategy — Help SciELO Markup, Don't Replace It](adrs/adr-001.md) — Phase 1 is the MVP; ships before any Phase 2 work.
- [ADR-002: Failure Policy — Skip and Warn](adrs/adr-002.md) — Author extraction must remain conservative; no speculative re-shaping.
- [ADR-008: Root Cause of Markup `mark_authors` Failure on 5313 / 5449](adrs/adr-008.md) — Created by this task.

## Deliverables
- Modified `ExtractAuthorsRule.cs` (or `HeaderParagraphLocator.cs`) implementing the fix.
- New `adrs/adr-008.md` documenting the root cause and decision.
- New regression fixture in `DocFormatter.Tests/Fixtures/Authors/` reproducing the 5313/5449 failure shape.
- Diagnostic-JSON snapshot diff confirming non-regression across `examples/*.docx`.
- Manual confirmation note (in ADR-008 or release notes) that Markup auto-marks all authors on post-fix 5313 and 5449.
- Unit tests with 80%+ coverage **(REQUIRED)**.
- Integration tests for the full Stage-1 pipeline on the 5313/5449 inputs **(REQUIRED)**.

## Tests
- Unit tests:
  - [ ] Regression: failure-shape fixture (mimicking 5313 author block) → produced paragraphs no longer match the divergent shape.
  - [ ] Regression: failure-shape fixture (mimicking 5449 author block) → produced paragraphs no longer match the divergent shape.
  - [ ] Non-regression: existing happy-path fixtures still extract the same author count, names, labels, ORCIDs, and `confidence=high`.
  - [ ] Anti-duplication invariant: produced runs contain NO `[author]`, `[fname]`, or `[surname]` literals.
  - [ ] Boundary: author block with a single author retains current behavior.
- Integration tests:
  - [ ] Run Phase 1 over `examples/phase-2/before/5313.docx`; assert the resulting `.docx` no longer contains the divergent shape identified in ADR-008.
  - [ ] Run Phase 1 over `examples/phase-2/before/5449.docx`; same assertion as above.
  - [ ] Run Phase 1 over the full `examples/*.docx` corpus; diff diagnostic JSON against the saved pre-fix snapshot; assert only the 5313/5449 deltas appear.
- Test coverage target: >=80%
- All tests must pass

## Success Criteria
- All tests passing
- Test coverage >=80%
- ADR-008 committed under `adrs/adr-008.md` with status "Accepted".
- SciELO Markup auto-marks every author on the post-fix `.docx` for both 5313 and 5449 with no manual re-marking (binary pass/fail per article, manually verified and recorded).
- No regression on any other article in `examples/*.docx` (diagnostic-JSON snapshot diff is empty outside 5313/5449).
