# Task Memory: task_06.md

Keep only task-local execution context here. Do not duplicate facts that are obvious from the repository, task file, PRD documents, or git history.

## Objective Snapshot

- Done: `DiagnosticWriter.BuildFormatting` now filters MoveHistoryRule and PromoteSectionsRule entries and populates `HistoryMove` / `SectionPromotion` on `DiagnosticFormatting` via two new private builders. Phase 1+2 outputs unchanged.

## Important Decisions

- Minimal-coordination strategy: parse numeric tokens already present in existing rule messages instead of changing rule message formats or emitting extra `[INFO]` entries. Avoided modifying `MoveHistoryRule` / `PromoteSectionsRule` to keep their 31 existing tests green and scope tight.
  - HistoryMove.ToIndexBeforeIntro: parsed from trailing `{N})` of `MovedMessagePrefix`.
  - SectionPromotion.AnchorParagraphIndex: parsed from `AnchorPositionMessagePrefix` trailing integer.
  - SectionPromotion.SectionsPromoted / SubsectionsPromoted: parsed via `TryParsePromotionSummary` between the three summary constants.
- Default-zero/null on fields the rules do not currently emit: `HistoryMove.FromIndex` (null), `SectionPromotion.SkippedParagraphsInsideTables` (0), `SectionPromotion.SkippedParagraphsBeforeAnchor` (0). Documented inline as comments at each builder.
- Extended `BuildFormatting`'s null-return guard to also fire when Phase 3 rules emitted any entry — Phase 3 INFO-only outputs now populate `Formatting` even when no Phase 1+2 rule warned. The JSON file write trigger (`HighestLevel >= Warn`) is unchanged.

## Learnings

- `DiagnosticFormatting` ctor is positional with 6 parameters ending in `HistoryMove, SectionPromotion`; constructing with `null` for the new fields is the safe default.
- `int.TryParse` accepts `ReadOnlySpan<char>` overloads, which lets us parse without allocating substring strings.
- Existing test `Build_NoPhase2RuleWarnsOrErrors_FormattingIsNull_LegacyKeysUnchanged` is the canonical regression for Phase 1+2-only behaviour after the null-return guard refactor; passes unchanged.

## Files / Surfaces

- `DocFormatter.Core/Reporting/DiagnosticWriter.cs` — added `BuildHistoryMove`, `BuildSectionPromotion`, `ParseTrailingInteger`, `TryParsePromotionSummary`; widened `BuildFormatting` filter set + null-return condition.
- `DocFormatter.Tests/DiagnosticWriterTests.cs` — added 13 Phase 3 cases (no-entry → null, every documented info/warn outcome, both-rules combo, all-six-subobjects integration, JSON round-trip).

## Errors / Corrections

## Ready for Next Run

- task_07 (DI registration + CliIntegrationTests) can rely on `DiagnosticWriter` populating Phase 3 fields without further coordination — pipe MoveHistoryRule and PromoteSectionsRule through DI, and a real-document run will produce JSON with `historyMove` / `sectionPromotion` populated automatically.
- If a future task needs `FromIndex` / `SkippedParagraphsInsideTables` / `SkippedParagraphsBeforeAnchor` populated with real values, the cleanest path is to add new public-const-string prefixes on the rules and emit additional `[INFO]` entries (e.g. `MoveHistoryRule.MovedFromPositionPrefix`, `PromoteSectionsRule.SkippedTablesPrefix`); the DiagnosticWriter parsers can then be extended in-place. Existing rule tests use `Assert.Single(... Info)` in some cases so they will need expansion.
