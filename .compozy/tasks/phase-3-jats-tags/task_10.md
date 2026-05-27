---
status: completed
title: CreditRolesInjector
type: backend
complexity: high
dependencies:
  - task_01
  - task_04
  - task_06
---

# Task 10: CreditRolesInjector

## Overview
Inject CRediT `<role content-type="…">` elements into each `<contrib>`, after its
`<xref>` elements. Parse the structured CREDIT shapes (role-keyed and
author-keyed) and auto-apply only when every term exact-matches the CRediT table
and every author initial resolves to one contributor; free prose, unknown terms,
or unresolved authors are proposed for confirmation (ADR-005).

<critical>
- ALWAYS READ the PRD and TechSpec before starting
- REFERENCE TECHSPEC for implementation details — do not duplicate here
- FOCUS ON "WHAT" — describe what needs to be accomplished, not how
- MINIMIZE CODE — show code only to illustrate current structure or problem areas
- TESTS REQUIRED — every task MUST include tests in deliverables
</critical>

<requirements>
- MUST parse the two structured CREDIT shapes: role-keyed (`Role: initials; …`)
  and author-keyed (`Initials: Role, Role; …`).
- MUST map terms to CRediT URLs by exact match against the table (case-insensitive,
  dash/`&`-normalized, e.g. `Writing - Original Draft` → `Writing – original draft`).
- MUST resolve author initials to a single `<contrib>` by building candidate
  initials from `<surname>`/`<given-names>`; a non-unique or absent match is
  unresolved.
- MUST auto-apply a contributor's roles ONLY when every listed term maps AND the
  author resolves uniquely; otherwise build a `Proposal` and confirm.
- MUST enforce SPS all-or-nothing CRediT per document: if any emitted role would
  lack `@content-type`, none get it.
- MUST emit one `<role>` per role, inside the matched `<contrib>`, after its
  `<xref>` elements; MUST be idempotent (skip a contrib that already has any
  `<role>`).
- MUST treat free-prose statements (no structured shape) as unresolved → prompt.
</requirements>

## Subtasks
- [x] 10.1 Detect the CREDIT shape (role-keyed / author-keyed / prose).
- [x] 10.2 Normalize and exact-match terms to CRediT URLs.
- [x] 10.3 Resolve initials to a unique `<contrib>` or mark unresolved.
- [x] 10.4 Gate per contributor: auto when all terms+author resolve, else propose+confirm.
- [x] 10.5 Enforce all-or-nothing `@content-type`; skip contribs already having roles.
- [x] 10.6 Emit `<role>` elements after `<xref>` and report dispositions.

## Implementation Details
Create `DocFormatter.Core/Jats/CreditRolesInjector.cs` plus helpers
(`CreditStatementParser.cs`, `CreditTermTable.cs`, `AuthorInitialsResolver.cs`).
Table and rules per `docs/scielo_context/jats/credit_roles.md`. Consumes the raw
CREDIT body from `DocxSource` (task_01), the element builder (task_06), and
`IConfirmer` via the context. See TechSpec injection-rules table and ADR-005.
Corpus shapes: 5523/5313 role-keyed, 5136 author-keyed, 5449/5293/5570 prose.

### Relevant Files
- `DocFormatter.Core/Jats/CreditRolesInjector.cs`, `CreditStatementParser.cs`, `CreditTermTable.cs`, `AuthorInitialsResolver.cs` — new (create).
- `docs/scielo_context/jats/credit_roles.md` — term→URL table, all-or-nothing rule, placement.
- `examples/phase-3/scielo_markup/{5523,5313,5136,5449,5293,5570}.docx` — shape variety.

### Dependent Files
- `AddPhase3Injectors()` registration (task_11).
- Golden-corpus tests (task_12).

### Related ADRs
- [ADR-005: CRediT auto only for structured/exact-term statements](../adrs/adr-005.md) — the core gate.
- [ADR-001: Confidence-gated injection](../adrs/adr-001.md) — auto vs prompt.

## Deliverables
- `CreditRolesInjector` + parser/table/resolver helpers, idempotent and gated per ADR-005.
- Unit tests with 80%+ coverage **(REQUIRED)**
- Integration test across structured and prose corpus docs **(REQUIRED)**

## Tests
- Unit tests:
  - [x] Role-keyed `Conceptualization: Lopes DAPS, Nascimento IRN` maps both authors' Conceptualization role to the correct contribs (auto).
  - [x] Author-keyed `ATAJ: Conceptualization, Methodology` emits two `<role>` with correct URLs for the resolved author (auto).
  - [x] `Writing - Original Draft` normalizes and exact-matches `writing-original-draft`.
  - [x] An unrecognized term forces a `Proposal` (no silent auto).
  - [x] Prose statement (5449/5570) is treated as unresolved → prompt, not auto-applied.
  - [x] Initials matching no contrib, or two contribs, mark the author unresolved → prompt.
  - [x] A contrib already having any `<role>` is skipped and reported.
  - [x] `<role>` elements are inserted after the contrib's `<xref>` elements.
- Integration tests:
  - [x] 5523 (role-keyed) auto-applies all roles end-to-end via the injector.
  - [x] 5449 (prose) surfaces a confirmation proposal rather than writing roles.
- Test coverage target: >=80%
- All tests must pass

## Success Criteria
- All tests passing
- Test coverage >=80%
- Structured + exact-term statements auto-apply correctly; prose/unknown/unresolved cases prompt; all-or-nothing CRediT honored; placement after `<xref>` correct.
