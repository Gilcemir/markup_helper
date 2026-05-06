# Task Memory: task_05.md

Keep only task-local execution context here. Do not duplicate facts that are obvious from the repository, task file, PRD documents, or git history.

## Objective Snapshot

- Implement `ExtractTopTableRule` (Critical) under `DocFormatter.Core/Rules/`. Detect the top 3×1 table, populate `FormattingContext.Doi` and `ElocationId`, delete the table, abort with the spec's verbatim PT-BR message when no qualifying table is at the top.

## Important Decisions

- "Top-of-document table" = first body child that is not `SectionProperties` MUST be a `Table`. Paragraph-first or non-Table-first ⇒ abort. Matches techspec test #6.
- Header detection requires ALL three cells to carry recognized headers covering exactly `{id, elocation, doi}`; any partial / mixed case falls through to positional mapping. Cleanest interpretation of "if all three cells are header-less, fall back to positional".
- DOI validation uses `FormattingOptions.DoiRegex` (anchored `^10\.…$`). Cross-cell fallback also uses `IsMatch` against the same regex (anchored), not a substring scan. Test 3 only puts DOI-shaped values into the alternate cell, so anchored matching is sufficient.
- Abort string lives as `public const string ExtractTopTableRule.CriticalAbortMessage` so tests can assert against it without duplicating the magic text.

## Learnings

- `WordprocessingDocument.Create(stream, …)` does NOT add a `MainDocumentPart`. Tests must call `doc.AddMainDocumentPart()` and assign `mainPart.Document = new Document(new Body(...))` before exercising body-touching rules. Existing `FormattingPipelineTests` works without this only because it never reads the body.
- `MainDocumentPart.Document` is nullable in OpenXml 3.5.1 — chained access requires `MainDocumentPart!.Document!.Body!` (or a helper). Pure `MainDocumentPart!.Document.Body!` triggers `CS8602` and the build fails under `TreatWarningsAsErrors=true`.
- Real production `examples/*.docx` use header-less cells and put the DOI inside a hyperlink wrapping a full URL (e.g. `http://dx.doi.org/10.1590/...`). With the anchored DoiRegex this currently lands as `Doi=null` + `[WARN]` plus a cross-cell scan miss. Out of scope for task_05 (the spec's tests use bare DOI strings) but worth flagging for task_09/task_11.

## Files / Surfaces

- `DocFormatter.Core/Rules/ExtractTopTableRule.cs` — new rule, Critical severity, ctor takes `FormattingOptions`.
- `DocFormatter.Tests/ExtractTopTableRuleTests.cs` — 12 tests (10 unit + 2 pipeline integration).
- No other production file touched.

## Errors / Corrections

- First test pass failed `TreatWarningsAsErrors` with 10× `CS8602` from chained `doc.MainDocumentPart!.Document.Body!.Elements<…>()`. Fixed by introducing a `GetBody(doc)` helper that bangs all three nullables once. Lesson: when reaching into `MainDocumentPart`, build a helper from the start.

## Ready for Next Run

- task_05 complete: build 0/0, 40/40 tests pass (28 baseline + 12 new). Status flipped to `completed`. Auto-commit disabled — diff staged for manual review.
- Downstream tasks (06, 09, 10, 11) can now rely on `ctx.ElocationId` / `ctx.Doi` being populated when the input format is valid, and on Critical abort otherwise.
