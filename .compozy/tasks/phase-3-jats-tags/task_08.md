---
status: completed
title: EditedByInjector
type: backend
complexity: medium
dependencies:
  - task_01
  - task_04
  - task_06
---

# Task 8: EditedByInjector

## Overview
Inject `<fn fn-type="edited-by">` (role in `<label>`, name in `<p>`) into the
single `<author-notes>` element, creating `<author-notes>` if absent. The
responsible-editor name and role come from the docx; ORCID is omitted when not
present.

<critical>
- ALWAYS READ the PRD and TechSpec before starting
- REFERENCE TECHSPEC for implementation details — do not duplicate here
- FOCUS ON "WHAT" — describe what needs to be accomplished, not how
- MINIMIZE CODE — show code only to illustrate current structure or problem areas
- TESTS REQUIRED — every task MUST include tests in deliverables
</critical>

<requirements>
- MUST read the editor role + name from `DocxSource` (`Scientific Editor:` and any
  associate-editor role line).
- MUST emit `<fn fn-type="edited-by">` with the role in `<label>` (e.g.
  `SCIENTIFIC EDITOR:`) and the name in `<p>`; MUST NOT use `<title>`/`<bold>`/
  `<italic>` for the label.
- MUST omit `<ext-link>` when no ORCID is available (ORCID best-effort, name+role
  only per PRD).
- MUST target the single `<author-notes>` element, creating it if absent and
  appending the `fn` (an existing `<corresp>` must remain).
- MUST be idempotent: if any `<fn fn-type="edited-by">` already exists, skip and
  report.
- MUST support multiple editor roles (append one `fn` per role line found).
</requirements>

## Subtasks
- [x] 8.1 Skip-if-present when an `edited-by` fn already exists.
- [x] 8.2 Locate or create the single `<author-notes>`.
- [x] 8.3 Build one `<fn fn-type="edited-by">` per editor role line (label + name).
- [x] 8.4 Append the fn(s) and report the applied values.

## Implementation Details
Create `DocFormatter.Core/Jats/EditedByInjector.cs` implementing `IJatsInjector`.
Placement rule per `docs/scielo_context/jats/responsible_editor.md`. Consumes the
editor fields from `DocxSource` (task_01) and the element builder (task_06).
Corpus example: `Scientific Editor: Luiz Antônio dos Santos Dias` (no ORCID).

### Relevant Files
- `DocFormatter.Core/Jats/EditedByInjector.cs` — new (create).
- `docs/scielo_context/jats/responsible_editor.md` — placement and label rules.
- `DocFormatter.Core/Jats/DocxSource.cs` — editor fields (task_01).

### Dependent Files
- `AddPhase3Injectors()` registration (task_11).
- Golden-corpus tests (task_12).

### Related ADRs
- [ADR-002: Source values from the paired docx](../adrs/adr-002.md) — editor name from docx.
- [ADR-001: Confidence-gated injection](../adrs/adr-001.md) — deterministic name+role.

## Deliverables
- `EditedByInjector` emitting edited-by fn(s) in `<author-notes>`, idempotent.
- Unit tests with 80%+ coverage **(REQUIRED)**
- Integration test on a corpus XML **(REQUIRED)**

## Tests
- Unit tests:
  - [ ] Editor name with no ORCID yields `<fn fn-type="edited-by"><label>SCIENTIFIC EDITOR:</label><p>Name</p></fn>` (no `<ext-link>`).
  - [ ] XML with an existing `<author-notes>` (corresp only) gets the fn appended, corresp preserved.
  - [ ] XML with no `<author-notes>` gets one created with the fn.
  - [ ] Two editor role lines produce two `<fn fn-type="edited-by">` entries.
  - [ ] XML already containing an edited-by fn is left unchanged and reported as skipped.
- Integration tests:
  - [ ] Running over `…e54492621.xml` injects the scientific editor into `<author-notes>` after the existing corresp.
- Test coverage target: >=80%
- All tests must pass

## Success Criteria
- All tests passing
- Test coverage >=80%
- edited-by fn placed correctly with label+name; ORCID omitted when absent; single author-notes invariant maintained.
