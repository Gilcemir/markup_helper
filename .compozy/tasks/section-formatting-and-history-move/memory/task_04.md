# Task Memory: task_04.md

Keep only task-local execution context here. Do not duplicate facts that are obvious from the repository, task file, PRD documents, or git history.

## Objective Snapshot

- Implemented `PromoteSectionsRule` (Optional, position #11) with idempotent jc/sz/szCs mutation, anchor-scoped iteration, table + context skip, and INFO summary + anchor-position emission.

## Important Decisions

- Iteration uses `body.Descendants<Paragraph>().ToList()` from anchor index forward so paragraphs nested inside `<w:tbl>` show up in document order and get filtered by `IsInsideTable` (and would naturally feed `SkippedParagraphsInsideTables` for task_06 if it parses messages — current rule only emits the two required INFO messages).
- Anchor position reported as the index in `body.Elements<Paragraph>().ToList()` (direct-children index), matching `MoveHistoryRule.MovedMessagePrefix` convention.
- Idempotency is "second run is a no-op" rather than "second run re-applies the same values": after first run the anchor's alignment becomes Center, so `IsSection` returns false, `FindIntroductionAnchor` returns null, and the rule emits `[WARN] AnchorMissingMessage` while leaving OOXML byte-identical. Test asserts OOXML byte-identity only (per spec).

## Learnings

- `JustificationValues` in OpenXml SDK 3.x is a struct whose default `ToString()` prints `JustificationValues { }`. Compare via `==` against `JustificationValues.Center` instead of stringifying.
- Setting `paragraph.ParagraphProperties.Justification = new Justification { Val = ... }` replaces only the `<w:jc>` child; sibling pPr children (e.g., `PageBreakBefore`) are preserved. Same for `RunProperties.FontSize` / `FontSizeComplexScript` typed setters.
- `paragraph.ParagraphProperties ??= new ParagraphProperties()` and `run.RunProperties ??= new RunProperties()` are the safe "create-if-absent" idiom; OpenXml inserts the new element at the correct (first-child) position via the typed setter.

## Files / Surfaces

- New: `DocFormatter.Core/Rules/PromoteSectionsRule.cs`.
- New: `DocFormatter.Tests/PromoteSectionsRuleTests.cs` (16 tests, all assert INV-01).
- Modified: `DocFormatter.Tests/Fixtures/Phase3/Phase3DocxFixtureBuilder.cs` — added `BuildSectionParagraph` and `BuildSubsectionParagraph` helpers (thin wrappers over `BuildParagraph` with `runDirectBold: true`).

## Errors / Corrections

- First test run failed 7/16 due to `JustificationValues.ToString()` printing struct name. Fix: replaced `Assert.Equal("Center", GetJustification(p))` with `Assert.True(IsCenterJustified(p))` that compares struct values via `==`.

## Ready for Next Run

- task_06 (`DiagnosticWriter`) needs to reconstruct `DiagnosticSectionPromotion` from report entries. Currently `PromoteSectionsRule` only emits two INFO messages: anchor position (`AnchorPositionMessagePrefix`+P) and summary (`SummaryPromotedPrefix`+N+`SummarySectionsInfix`+M+`SummarySubsectionsSuffix`). It does NOT emit `SkippedParagraphsInsideTables` / `SkippedParagraphsBeforeAnchor` counts in messages — task_06 will either parse what's available, leave those counts at 0, or request adding extra INFO messages here. Counts ARE tracked internally during iteration; exposing them is a one-liner if needed.
