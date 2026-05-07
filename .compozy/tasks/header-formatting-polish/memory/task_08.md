# Task Memory: task_08.md

Keep only task-local execution context here. Do not duplicate facts that are obvious from the repository, task file, PRD documents, or git history.

## Objective Snapshot
- Implemented `RewriteAbstractRule` (Phase 2 Optional) that splits the abstract paragraph into a bold `Abstract` heading + plain-text body and surfaces the `Corresponding author: <email>` line above the heading. Status: completed.

## Important Decisions
- Heading paragraph built fresh; body paragraph = the original abstract paragraph mutated in-place (leading content stripped). Preserves original `ParagraphProperties` on the body.
- Plain-text offset to OOXML run mapping reuses the truncation pattern from `ExtractCorrespondingAuthorRule` (`MeasureChildLength` + a forward-from-start variant).
- Italic decision uses `RunProperties.Italic` element presence + `Val` (null treated as `true`, OOXML default). Element is removed entirely when stripping (matches ADR-002).
- Separator detection accepts `-`, `:`, `—`, and `–` (en dash). When no separator is found, body is preserved as-is plus a `[WARN]` (`MissingSeparatorMessage`).
- INFO message text differentiates the four-branch action table:
  - email available + typed line found (no recovery) → `replaced pre-existing corresponding-author line: '<text>'`.
  - no email + typed line found + email recovered → only `recovered email from pre-existing corresponding-author line` (no replacement INFO).
  - email available + no typed line → `Corresponding author line inserted`.
- When `AuthorParagraphs` is empty, front-matter scan starts from the body's first paragraph; the abstract paragraph itself is the stop boundary.

## Learnings
- `RewriteHeaderMvpRule.CreateBaseRunProperties()` returns `RunProperties` containing `RunFonts` + `FontSize` + `FontSizeComplexScript`; appending `Bold` after these keeps the heading run consistent with other rewritten paragraphs.
- xUnit tests can construct `Italic` runs via `new RunProperties(new Italic())`; OpenXML SDK round-trips correctly without requiring a `Val=true` attribute.

## Files / Surfaces
- Added: `DocFormatter.Core/Rules/RewriteAbstractRule.cs`.
- Added: `DocFormatter.Tests/RewriteAbstractRuleTests.cs` (17 tests).
- Touched task_08 tracking files only; no edits to other rule classes.

## Errors / Corrections
- Initial draft considered building heading + body both as fresh paragraphs and removing the original. Switched to mutating the original (becomes the body) so paragraph-level properties survive and existing `LocateAbstractAndInsertElocationRule` references would still target the same paragraph if needed.

## Ready for Next Run
- task_09 will key on `RewriteAbstractRule` constants: `StructuralItalicRemovedMessage`, `ResumoNormalizedMessage`, `AbstractNotFoundMessage`, `CanonicalLineInsertedMessage`, `RecoveredEmailMessage`, `MissingSeparatorMessage`, `ReplacedTypedLineMessagePrefix` (a prefix; INFO message text continues with `'<original text>'`).
- task_10 must register `RewriteAbstractRule` BEFORE `LocateAbstractAndInsertElocationRule` (per the rule's `Apply` requirement).
