# Task Memory: task_13.md

Keep only task-local execution context here. Do not duplicate facts that are obvious from the repository, task file, PRD documents, or git history.

## Objective Snapshot
Add an operator-chosen, document-scoped `ConfirmDisposition.FreeText` to the CRediT
gate (ADR-007). When chosen on the credit-roles unrecognized/unresolved branch, emit
one `<role>` (NO `@content-type`) per WRITTEN term for every RESOLVED author across
the doc — including terms that would have CRediT-matched — so the doc is uniform
(zero `@content-type`). Strictly operator-chosen: ConsoleConfirmer offers it only when
`Proposal.AllowsFreeText`; AutoAccept stays `AutoApplied`; FailOnPrompt still aborts.
Record disposition in `.report.txt` + `.diagnostic.json`.

## Important Decisions
- Contract: add `bool AllowsFreeText { get; init; }` (default false) to `Proposal`
  (keeps positional ctor calls intact) + `ConfirmDisposition.FreeText`. Injector
  sets `AllowsFreeText=true` ONLY on the unrecognized/unresolved gate (NOT prose,
  NOT data-availability).
- ConsoleConfirmer: when AllowsFreeText, offer `[f]` = free text → `FreeText`;
  Enter = Confirmed; other text = Overridden. When not allowed, unchanged.
- Free-text emit = resolved contribs only; role text = verbatim written term; idem-
  potency skip-if-present unchanged. Report msg starts "Injected ... (FreeText)" so
  the diagnostic's applied-detection picks it up → disposition "freeText".
- `ReadCreditRolesValue` left as-is (counts CRediT-typed roles), so a free-text doc
  shows value=null + disposition=freeText. Audit signal is the disposition.

## Learnings
- No exhaustive switch over `ConfirmDisposition` exists in Core; the diagnostic
  stringifies it (`ToCamelCase(disposition.ToString())`) so a new enum value can't
  throw — `FreeText` → `"freeText"`. No DiagnosticWriter/DiagnosticDocument code
  change was needed for the new value.
- `RecordingConfirmer` (Phase3Processor) is enum-agnostic — it just stores
  `result.Disposition` per tag, so `FreeText` flows to the diagnostic with no change.
- e54582628 free-text resolved set = Nascimento, Ishikawa, Costa, Borel, Araújo.
  Costa/Borel/Araújo were the clean subset; Nascimento/Ishikawa were dropped only
  due to unrecognized terms (Methodology+Molecular…, Writing and Review). Free text
  recovers them. Neto stays NotFound (suffix bug, out of scope).
- Final: build clean, 800 tests pass (+19), changed-type coverage 97.28% line.

## Files / Surfaces
Core: Proposal.cs, CreditRolesInjector.cs, ConsoleConfirmer.cs (+ AutoAccept/FailOnPrompt
unchanged but covered). Reporting: DiagnosticWriter/DiagnosticDocument need no enum
switch (disposition is ToString→camelCase, won't throw). Tests: CreditRolesInjectorTests,
ConfirmerTests, ConfirmerPolicySwapIntegrationTests, DiagnosticWriterPhase3Tests,
Phase3ContractsTests, Phase3CorpusTests (e54582628).

## Errors / Corrections
- CONFLICT (resolved by honoring OUT OF SCOPE): task Tests bullet wants free-text to
  emit a role for `Neto` too, but `Neto VBP` resolves NotFound (surname=Paiva,
  suffix=Neto) and fixing suffix resolution is explicitly OUT OF SCOPE. Decision:
  free-text emits for resolved authors (Nascimento, Ishikawa, Costa, Borel, Araújo);
  Neto stays unresolved and is REPORTED (not silently dropped). Integration test
  asserts that, not a Neto role. Flag in final report.

## Ready for Next Run
DONE (pending manual commit; --auto-commit=false). Two out-of-scope follow-ups
remain (tracked, NOT in this task): (a) split composite CRediT label on "and"
(`CreditStatementParser`); (b) match author key against `<suffix>`
(`AuthorInitialsResolver.SurnameMatches`) so "Neto"/"Júnior"/"Filho" resolve.
The second is what blocks Neto from receiving a free-text role today.
