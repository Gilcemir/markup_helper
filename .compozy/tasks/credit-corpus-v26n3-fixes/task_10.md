---
status: pending
title: Documentation and v0.3.1 release preparation
type: docs
complexity: low
dependencies:
  - task_08
  - task_09
---

# Task 10: Documentation and v0.3.1 release preparation

## Overview
Document the new operator-visible behavior (the `contrib-names` rule, the batch summary pendency block, the diagnostic `creditStatement` block, plain-text ORCID handling in Phase 1) and leave the branch ready for merge and the `v0.3.1` tag. ADR promotion (`/promote-feature`) and the tag itself happen after merge and are not part of this task.

<critical>
- ALWAYS READ the PRD and TechSpec before starting
- REFERENCE TECHSPEC for implementation details — do not duplicate here
- FOCUS ON "WHAT" — describe what needs to be accomplished, not how
- MINIMIZE CODE — show code only to illustrate current structure or problem areas
- TESTS REQUIRED — every task MUST include tests in deliverables
</critical>

<requirements>
- MUST update `README.md` Phase 3 section: list `contrib-names` among the rules, show the batch summary pendency block format, mention `phase3.creditStatement` in the diagnostic, and note the widened CREDIT grammar (`;`/`,`).
- MUST update the README Phase 1 notes for plain-text ORCID extraction and the repeated-token WARN.
- MUST update the CLI usage/help text if it enumerates Phase 3 rules or summary contents.
- MUST verify all six ADRs in `adrs/` reflect the final implementation (adjust wording if a task deviated) and that ADR-003 carries `INV-02` in the promote-able format.
- MUST confirm commit messages on the branch name the defect each closes (release notes are generated from commits).
- MUST NOT run `make release` or `/promote-feature` (post-merge steps).
</requirements>

## Subtasks
- [ ] 10.1 Update README Phase 3 and Phase 1 sections.
- [ ] 10.2 Update CLI help text if applicable and its test.
- [ ] 10.3 Review ADRs 001–006 against the implemented code; fix drift.
- [ ] 10.4 Verify the branch's commit history is release-notes ready and `make test` plus `make phase2-verify` are green.

## Implementation Details
Files: `README.md` (Phase 3 section, "Versionamento"/release notes unaffected), `DocFormatter.Cli/CliApp.cs` usage text (~line 757) and its test if one asserts the usage string, `.compozy/tasks/credit-corpus-v26n3-fixes/adrs/*.md`. Post-merge procedure remains `make release VERSION=v0.3.1` on `main`.

### Relevant Files
- `README.md` — operator documentation.
- `DocFormatter.Cli/CliApp.cs` — usage text.
- `.compozy/tasks/credit-corpus-v26n3-fixes/adrs/` — ADRs to be promoted later.
- `docs/decisions/README.md`, `docs/INVARIANTS.md` — targets of the later `/promote-feature` (not edited here).

### Dependent Files
- `DocFormatter.Tests/CliPhase3Tests.cs` — if it asserts usage output.

### Related ADRs
- [ADR-001](../adrs/adr-001.md) — release as `v0.3.1`, commit hygiene for generated notes.

## Deliverables
- README and CLI help updated.
- ADRs verified against implementation.
- Unit tests with 80%+ coverage **(REQUIRED)** — usage-text test updated if applicable.
- Integration tests: `make test` and `make phase2-verify` green on the final branch **(REQUIRED)**

## Tests
- Unit tests:
  - [ ] CLI usage test (if present) matches the updated help text.
- Integration tests:
  - [ ] `make test` exits 0.
  - [ ] `make phase2-verify` exits 0.
- Test coverage target: >=80%
- All tests must pass

## Success Criteria
- All tests passing
- Test coverage >=80%
- README describes every new operator-visible behavior; branch ready for PR to `main` and `v0.3.1` tag.
