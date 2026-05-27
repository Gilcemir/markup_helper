---
status: completed
title: Free-text CRediT role fallback (operator-chosen, document-scoped)
type: backend
complexity: high
dependencies:
  - task_05
  - task_10
---

# Task 13: Free-text CRediT role fallback (operator-chosen, document-scoped)

## Overview
Today `CreditRolesInjector` only emits `<role content-type="…">` for terms that
exact-match the CRediT table and silently drops every author whose statement has
an unrecognized term or unresolved author — losing valid contributions. This task
adds the spec-faithful "Case A" path: when gated, the operator may choose to emit
the document's roles as free text **without** `@content-type`, honoring the SPS
per-document all-or-nothing rule. The choice is always operator-driven, never an
automatic fallback (ADR-007).

<critical>
- ALWAYS READ the PRD and TechSpec before starting
- REFERENCE TECHSPEC for implementation details — do not duplicate here
- READ ADR-007 (primary spec for this task), ADR-005, and ADR-006 before starting
- FOCUS ON "WHAT" — describe what needs to be accomplished, not how
- MINIMIZE CODE — show code only to illustrate current structure or problem areas
- TESTS REQUIRED — every task MUST include tests in deliverables
</critical>

<requirements>
- MUST add a document-scoped free-text outcome to the confirmer contract (e.g. a
  new `ConfirmDisposition.FreeText`) meaning "emit all roles for this document as
  `<role>` without `@content-type`".
- On that disposition, `CreditRolesInjector` MUST emit one `<role>` (no
  `@content-type`) for **every** parsed author/term across the document —
  including terms that would have matched CRediT — so no `<role>` in the document
  carries `@content-type` (SPS per-document all-or-nothing).
- The free-text option MUST be strictly operator-chosen: `ConsoleConfirmer` offers
  it as an explicit choice **only** when the proposal marks it as allowed (the
  credit-roles unrecognized/unresolved branch). It MUST NOT be auto-selected.
- `AutoAcceptConfirmer` MUST NOT select free-text — it keeps current behavior
  (`AutoApplied`, clean CRediT subset). `FailOnPromptConfirmer` MUST still abort
  on any prompt, including a free-text-eligible one.
- The chosen disposition MUST be recorded per tag in `.report.txt` and
  `.diagnostic.json` so a free-text document is auditable.
- Roles already present on a `<contrib>` MUST still be respected (idempotency
  unchanged from task_10).
- OUT OF SCOPE (do NOT implement here): splitting a composite label like
  `Methodology and Molecular data analysis` on "and"; matching author keys against
  `<suffix>` (the "Neto" `NotFound`). These are tracked separately.
</requirements>

## Subtasks
- [x] 13.1 Extend the confirmer contract with a document-scoped free-text disposition and an "allows free-text" signal on the proposal.
- [x] 13.2 Teach `ConsoleConfirmer` to offer the free-text choice only when allowed; have `AutoAccept`/`FailOnPrompt` keep their current outcomes.
- [x] 13.3 In `CreditRolesInjector`, on free-text disposition emit every author/term as `<role>` with no `@content-type`, document-wide.
- [x] 13.4 Ensure the document ends up uniform: zero `@content-type` on any role when free-text is chosen.
- [x] 13.5 Record the free-text disposition in the report and diagnostic outputs.
- [x] 13.6 Cover the new path with unit and corpus integration tests (e54582628 / docx 5458).

## Implementation Details
Extend the `Proposal`/`ConfirmResult`/`ConfirmDisposition` contracts in
`DocFormatter.Core/Jats/Proposal.cs`. The credit-roles gate already issues a
single `Confirm` for the unrecognized/unresolved branch in
`CreditRolesInjector.Apply` — that is the natural place to interpret the new
disposition and switch the whole document to free-text emission. The three
policies live in `DocFormatter.Core/Jats/{ConsoleConfirmer,AutoAcceptConfirmer,
FailOnPromptConfirmer}.cs`. A new enum value ripples into the report/diagnostic
mapping in `DocFormatter.Core/Reporting/{DiagnosticWriter,DiagnosticDocument}.cs`.
The CLI policy selection in `CliApp.TrySelectConfirmer` stays as-is unless the
non-interactive contract needs the new value surfaced. There is no TechSpec
section for this path (it postdates the TechSpec); ADR-007 is the spec of record.

### Relevant Files
- `DocFormatter.Core/Jats/CreditRolesInjector.cs` — the gate + emission; add the free-text branch and the no-`@content-type` emit.
- `DocFormatter.Core/Jats/Proposal.cs` — `ConfirmDisposition`, `Proposal`, `ConfirmResult` contracts to extend.
- `DocFormatter.Core/Jats/ConsoleConfirmer.cs` — present the free-text option when allowed.
- `DocFormatter.Core/Jats/AutoAcceptConfirmer.cs`, `FailOnPromptConfirmer.cs` — preserve conservative behavior.
- `docs/scielo_context/jats/credit_roles.md` — free-text-without-`@content-type` rule and per-document uniformity.

### Dependent Files
- `DocFormatter.Core/Reporting/DiagnosticWriter.cs`, `DiagnosticDocument.cs` — switch over `ConfirmDisposition` must handle the new value.
- `DocFormatter.Cli/Phase3Processor.cs` — `RecordingConfirmer` records dispositions; verify the new value flows through.
- `DocFormatter.Tests/Jats/{CreditRolesInjectorTests,ConfirmerTests,ConfirmerPolicySwapIntegrationTests}.cs` — extend.
- `DocFormatter.Tests/Phase3/Phase3CorpusTests.cs` — corpus coverage for e54582628.

### Related ADRs
- [ADR-007: Free-text role fallback as an operator-chosen, document-scoped disposition](../adrs/adr-007.md) — the decision this task implements.
- [ADR-005: CRediT auto-mapping only for structured/exact-term statements](../adrs/adr-005.md) — amended by ADR-007.
- [ADR-006: Confirmation gate via IConfirmer](../adrs/adr-006.md) — extended contract.
- [ADR-001: Confidence-gated injection](../adrs/adr-001.md) — auto vs prompt and auditability.

## Deliverables
- A document-scoped free-text disposition end-to-end: contract → confirmer → injector → report/diagnostic.
- `CreditRolesInjector` emits uniform free-text roles (no `@content-type`) when chosen, with no silent author drops.
- Conservative non-interactive behavior preserved (`accept` never auto-picks free-text; `fail` aborts).
- Unit tests with 80%+ coverage **(REQUIRED)**
- Integration test over corpus doc e54582628 / docx 5458 **(REQUIRED)**

## Tests
- Unit tests:
  - [x] On a free-text-eligible proposal, `ConsoleConfirmer` scripted to pick the free-text option returns `ConfirmDisposition.FreeText`.
  - [x] `ConsoleConfirmer` does NOT offer free-text for a proposal that disallows it (e.g. a data-availability proposal) — only confirm/override are available.
  - [x] `AutoAcceptConfirmer` returns `AutoApplied` (never `FreeText`) for a free-text-eligible proposal.
  - [x] `FailOnPromptConfirmer` aborts on a free-text-eligible proposal.
  - [x] `CreditRolesInjector` on `FreeText` emits `<role>` for every author/term including CRediT-matching ones (e.g. `Conceptualization` written as plain text, no `@content-type`).
  - [x] After a `FreeText` injection, the document has zero `@content-type` attributes on any `<role>`.
  - [x] The `FreeText` disposition is written to `.report.txt` and `.diagnostic.json`.
  - [x] `DiagnosticWriter` maps the new `ConfirmDisposition` value without throwing.
- Integration tests:
  - [x] End-to-end on e54582628 (docx 5458): choosing free-text emits roles for the previously-dropped resolved authors (Nascimento, Ishikawa) plus Costa/Borel/Araújo, all without `@content-type`. NOTE: `Neto` (surname=Paiva/suffix=Neto) stays `NotFound` and is reported, not placed — suffix resolution is explicitly OUT OF SCOPE for this task, so the test asserts Neto is surfaced (not silently dropped) rather than receiving a role.
  - [x] End-to-end on e54582628 under `--non-interactive=accept`: behavior is unchanged (clean CRediT subset only; no free-text auto-selected).
- Test coverage target: >=80% (achieved: 97.28% line on changed types)
- All tests must pass (800 passed / 0 failed)

## Success Criteria
- All tests passing
- Test coverage >=80%
- Free-text is reachable only by explicit operator choice; `accept`/`fail` stay conservative.
- A free-text document is uniform (no `@content-type` on any role) and loses no declared contributions.
- The free-text disposition is recorded for audit; the two out-of-scope bugs are untouched.
