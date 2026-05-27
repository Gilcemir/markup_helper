# Task Memory: task_09.md

Keep only task-local execution context here. Do not duplicate facts that are obvious from the repository, task file, PRD documents, or git history.

## Objective Snapshot
DONE. `DataAvailabilityInjector` (Optional) + `DataAvailabilityClassifier`
inject `<sec sec-type="data-availability" specific-use="…">` (title + statement
`<p>`) after `<ack>` in `<back>`. @specific-use auto-applied when the keyword
classifier is confident, else proposed via `IConfirmer`. Idempotent on sec/fn.

## Important Decisions
- **Confidence = exactly ONE of the five categories matches** (any keyword).
  Zero matches OR 2+ matches → not confident → propose. Keyword *counts* only
  break ties for the best-guess proposal value, never decide confidence. This
  is more conservative than "highest score wins" and avoids the substring trap
  (e.g. "on request" ⊂ "upon request" self-inflates a single category — it would
  never tie under a score-max model).
- **Section `<title>` = "Data Availability Statement"** (English constant). Corpus
  is `language="en"`; matches the `<label>` shown for the `<fn>` form in
  `data_availability.md`. Not derived from text.
- **Default proposed value = `data-available-upon-request`** (most common; mirrors
  the MathML manual default noted in `data_availability.md`).
- Severity **Optional** (like EditedBy): absent DA text or absent `<back>` →
  Info + skip, never abort. Confirmer `Skipped`/blank value → Warn + skip (the
  `@specific-use` attribute is mandatory, so no sec is written without it).
- Confident path does NOT call the confirmer (disposition `AutoApplied`); only the
  ambiguous path calls `ctx.Confirm.Confirm`. Both auto paths report `AutoApplied`
  but the reason text differs ("auto-classified" vs "proposed '<v>'").

## Learnings
- Corpus `<back>` shape: `<back>` depth 1, `<ack>` then `<ref-list>` at depth 2.
  e54492621 docx (5449) DA text = "…corresponding author upon request" →
  confidently `data-available-upon-request`. Used for the integration test.
- Placement reuses the task_08 container patterns: `InsertAfter(ack, sec, depth)`;
  for before-first-child use `firstChild.AddBeforeSelf(sec)` + `sec.AddAfterSelf(Indent(depth))`;
  empty `<back>` seeds child + closing indent. Depth derived via `IndentDepthOf`.

## Files / Surfaces
- NEW `DocFormatter.Core/Jats/DataAvailabilityInjector.cs`
- NEW `DocFormatter.Core/Jats/DataAvailabilityClassifier.cs`
- NEW `DocFormatter.Tests/Jats/DataAvailabilityInjectorTests.cs` (15 tests incl. corpus integration)
- NEW `DocFormatter.Tests/Jats/DataAvailabilityClassifierTests.cs` (12 tests)
- Coverage of the two new classes: Line 98.14% / Branch 94.44% (coverlet).

## Errors / Corrections
- First confidence model (unique max keyword score) FAILED the tie test because
  "on request" is a substring of "upon request" → upon-request always scored ≥2.
  Switched to category-count model (see Decisions). Lesson: keyword substrings
  inside the same category double-count under a score-max scheme.

## Ready for Next Run
- task_11 must register `DataAvailabilityInjector` in `AddPhase3Injectors()` in the
  pipeline order Other→EditedBy→DataAvailability→CreditRoles.
- task_12 golden corpus: with `AutoAcceptConfirmer`, e54492621 gets
  `specific-use="data-available-upon-request"` after `<ack>`. Other corpus docs
  whose DA text is ambiguous will emit the proposed best-guess under `accept`.
