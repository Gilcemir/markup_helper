# Task Memory: task_02.md

Keep only task-local execution context here. Do not duplicate facts that are obvious from the repository, task file, PRD documents, or git history.

## Objective Snapshot

Build `TagEmitter` static helper that emits SciELO `[tag attr="v"]…[/tag]` literals as OpenXML Runs. Single-source-of-truth for every Phase 2 emitter rule.

## Important Decisions

- `CreateBaseRunProperties()` was already `internal static`; since `TagEmitter` lives in the same `DocFormatter.Core` assembly, no visibility relax was needed. Subtask "visibility adjustment if needed" resolved as no-op.
- Used `paragraph.InsertBefore` / `InsertAfter` skipping `ParagraphProperties` so the opening Run never lands before `<w:pPr>` (Word would reject).
- Superscript zeroing strategy: remove the `VerticalTextAlignment` element entirely (cleaner XML; equivalent to setting Val=Baseline).

## Learnings

- The `WordprocessingDocument` round-trip test must call `mainPart.Document.Save()` before disposing or the modifications never reach the package on the inner `using` block.

## Files / Surfaces

- New: `DocFormatter.Core/TagEmission/TagEmitter.cs`.
- New: `DocFormatter.Tests/TagEmitterTests.cs` (18 tests, all passing).
- Read-only: `DocFormatter.Core/Rules/RewriteHeaderMvpRule.cs` (consumes `CreateBaseRunProperties()`).

## Errors / Corrections

- Initial test build failed with CS8602 on `Document.Body` access; fixed by adding `!` on `Document` too. The full chain `MainDocumentPart!.Document!.Body!` is the existing convention.

## Ready for Next Run

- Tasks 06, 07, 09 (Phase 2 emitter rules) consume `TagEmitter` directly. They must NOT pass any of `author`, `fname`, `surname`, `kwd`, `normaff`, `doctitle`, `doi` as the tag name (anti-duplication invariant; documented on the class).
