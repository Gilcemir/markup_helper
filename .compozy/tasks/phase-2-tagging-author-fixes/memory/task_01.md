# Task Memory: task_01.md

Keep only task-local execution context here. Do not duplicate facts that are obvious from the repository, task file, PRD documents, or git history.

## Objective Snapshot

Phase 1 author handoff fix: make SciELO Markup's `mark_authors` macro auto-tag every author on the post-fix `.docx` for articles 5313 and 5449. Investigation captured in ADR-008. Fix scoped to `ExtractAuthorsRule` per task spec; `RewriteHeaderMvpRule` consumes the result unchanged.

## Important Decisions

- Root cause is the comma between digit-aff label and `*` corresp marker in the produced superscript (`1,*` for 5313, `1,2,*` for 5449). Markup's `mark_authors` (PC-Programs/_analysis/markup_macros.txt:4799) chooses `,` as `sep_authors` when the range has `,` and no `;`, mis-splitting at the inner comma.
- Fix: in `AuthorBuilder.AddLabel`, fold any label that is purely `*` into the trailing entry of `Labels`. Keeps the comma-join in `RewriteHeaderMvpRule` untouched but produces canonical `1*` / `1,2*` shapes (same as 5136).
- Rejected: editing `RewriteHeaderMvpRule` (out of scope per task spec); introducing a separate `Author.CorrespMarker` field (Phase 3 scope, would change every article's label set).
- Asterisk-only label with no preceding entry (e.g. author has only `[SUP]'*'`) is kept as `["*"]` — merge requires a previous label so information is preserved for Phase 3.

## Learnings

- The `1,*` / `1,2,*` failure shape is present in **8 of 11** examples files, not just 5313 and 5449. The task spec wording "(only the 5313/5449 deltas are allowed)" was based on the user's manual Markup tests against those two files; the same root cause affects 5293, 5419, 5424, 5434, 5458, 5549. Refreshed snapshot reflects the universal fix; ADR-008 records the broader scope explicitly.
- `examples/formatted/*.diagnostic.json` is the de-facto pre/post-fix snapshot — there is no separate `Phase2DiffUtility` for Phase 1 yet, so the corpus check is `make run-all` + `diff` against a copy of the pre-fix files (kept under `/tmp/markup-helper-investigation/pre-fix-diagnostics/` during this run).
- `dotnet test` in this repo has no XPlat Code Coverage collector configured; coverage % cannot be reported numerically without adding `coverlet.collector`. The task spec's 80% target is therefore qualitative for this run (29 tests against ExtractAuthorsRule, including 8 new ADR-008 cases).

## Files / Surfaces

- `DocFormatter.Core/Rules/ExtractAuthorsRule.cs` — added `IsCorrespMarker` predicate and merge branch in `AuthorBuilder.AddLabel`.
- `DocFormatter.Tests/Fixtures/Authors/AuthorsParagraphFactory.cs` — added `Build5313FailureShape` and `Build5449FailureShape` named-fixture builders.
- `DocFormatter.Tests/ExtractAuthorsRuleTests.cs` — updated `Apply_WithOrcidHyperlinkWrappingAuthorName_PreservesNameAndAttachesId` (label assertion) and added 7 new `Apply_With*` cases covering the 5313 shape, 5449 shape, comma-internal variant, leading asterisk, multiple asterisks, 5136 baseline, and the anti-duplication invariant.
- `.compozy/tasks/phase-2-tagging-author-fixes/adrs/adr-008.md` — new ADR (status Accepted, dated 2026-05-10).
- `examples/formatted/*.diagnostic.json` (8 files) — refreshed by `make run-all` to reflect the post-fix label shape.
- `examples/formatted/*.docx` (11 files) and `examples/formatted/_app.log`, `_batch_summary.txt` — refreshed by `make run-all`.

## Errors / Corrections

- Initial assumption: only 5313 and 5449 needed fixing. Investigation showed 8 articles share the root cause; ADR-008 documents the broader scope and the snapshot refresh covers all 8.

## Ready for Next Run

- Manual operator verification of `mark_authors` on the post-fix `.docx` for 5313 and 5449 is the only outstanding item. ADR-008 has a verification slot reserved; once the operator confirms, append the result there.
- Phase 3 (task_07, `EmitAuthorXrefsRule`) consumes `Author.AffiliationLabels`. Post-ADR-008, an entry may be a merged `"<aff><asterisks>"` form (e.g. `"2*"`). Phase 3 must split: aff portion = `TrimEnd('*')`, corresp marker present iff the original ends in `*`. This is a one-line transformation but easy to miss.
