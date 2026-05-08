# Workflow Memory

Keep only durable, cross-task context here. Do not duplicate facts that are obvious from the repository, PRD documents, or git history.

## Current State

- task_01 complete: `BodySectionDetector` skeleton + `IsBoldEffective` cascade walker live, with `Phase3DocxFixtureBuilder` and 23 unit tests.
- task_02 complete: `BodySectionDetector` predicates and `FindIntroductionAnchor` implementations (shipped in 11f937a) now have full test coverage in `BodySectionDetectorTests.cs` (51 tests total: 23 IsBoldEffective + 28 predicate/anchor/integration).
- task_03 complete: `MoveHistoryRule` and `MoveHistoryRuleTests` live (15 tests, INV-01 multiset asserted in every case).
- task_04 complete: `PromoteSectionsRule` and `PromoteSectionsRuleTests` live (16 tests, INV-01 in every case). Mutation surface limited to `<w:jc>` (paragraph) and `<w:sz>`/`<w:szCs>` (text-bearing runs). Emits `AnchorPositionMessagePrefix`+`P` and `SummaryPromotedPrefix`+`N`+`SummarySectionsInfix`+`M`+`SummarySubsectionsSuffix` info entries; `AnchorMissingMessage` warn entry on miss.
- task_05 complete: `DiagnosticHistoryMove` / `DiagnosticSectionPromotion` records added; `DiagnosticFormatting` extended with two trailing nullable properties (`HistoryMove`, `SectionPromotion`). `DiagnosticDocumentTests` covers construction/equality/JSON round-trip.
- task_06 complete: `DiagnosticWriter.BuildFormatting` now reconstructs `HistoryMove` / `SectionPromotion` by filter-by-rule-name on `MoveHistoryRule` / `PromoteSectionsRule` and parsing numeric tokens from existing message constants. Phase 3 INFO-only signals now also force `Formatting` population (Phase 1+2 INFO-only does NOT, behaviour preserved). 13 new `DiagnosticWriterTests` cover every rule outcome + JSON round-trip; full suite 354 pass.
- task_07 complete: `MoveHistoryRule` (#10) and `PromoteSectionsRule` (#11) wired into `CliApp.BuildServiceProvider`. Two new `CliIntegrationTests` (`Run_Phase3_HappyPath_*` and `Run_Phase3_AnchorMissing_*`) exercise the full pipeline on synthetic fixtures and assert INV-01. `Phase2DocxFixtureBuilder.BuildPrologueElements` exposed as `internal static` for cross-fixture composition. `DocxFixtureBuilder.WriteValidDocx` updated to include an `INTRODUCTION` anchor so existing happy-path tests do not regress. Full suite 356 pass.

## Shared Decisions

- `Phase3DocxFixtureBuilder` (`DocFormatter.Tests/Fixtures/Phase3/`) is the single fixture builder for Phase 3 tests. Tasks 02–04 and 07 extend it (style helpers, paragraph builder, document creator) instead of forking new builders.
- `BodySectionDetector` is `internal static` in `DocFormatter.Core.Rules`. `IsBoldEffective` is `internal static`; `FindIntroductionAnchor`, `IsSection`, `IsSubsection`, `IsInsideTable` are `public static` per the TechSpec "Core Interfaces" signatures.

## Shared Learnings

- `OnOffValue` lives at `DocumentFormat.OpenXml` root namespace; tests using `OnOffValue.FromBoolean(...)` need `using DocumentFormat.OpenXml;`.
- `StyleParagraphProperties` does NOT expose `ParagraphMarkRunProperties` as a typed property; access it via `GetFirstChild<ParagraphMarkRunProperties>()`. Same goes for `ParagraphProperties` when the typed setter is undesirable.
- `[InternalsVisibleTo("DocFormatter.Tests")]` is already wired for `DocFormatter.Core` and `DocFormatter.Cli`; new internal types are testable without additional setup.

## Open Risks

- Diagnostic JSON file is only written when `report.HighestLevel >= Warn`. Phase 3 INFO-only runs do not produce `<file>.diagnostic.json`. Any future test or batch consumer expecting the file on a clean Phase 3 run must inject a Phase 1+2 warn (e.g., `Phase2Options(MalformedEmail: true)`) or change the trigger contract in `DiagnosticWriter.Write`.
- INV-01 cannot be asserted as strict end-to-end multiset equality in integration tests: `ExtractTopTableRule` removes header keys and the id value, and `ExtractCorrespondingAuthorRule` strips the email trailer from affiliation 2. Future Phase 3+ integration tests must check INV-01 as Phase-3-text subset preservation (see `AssertPhase3TextsPreserved` pattern).

## Handoffs

- task_04 (`PromoteSectionsRule`) consumes `IsSection`, `IsSubsection`, `IsInsideTable`, `FindIntroductionAnchor` — these are now fully test-covered. The 90% bold ratio is the predicate boundary the rule depends on; any change to the threshold must update the tests `IsSection_BoldRatioBelowNinetyPercent_ReturnsFalse` and `IsSection_BoldRatioExactlyNinetyPercent_ReturnsTrue`.
- task_03 published the seven `MoveHistoryRule` message constants (`AnchorMissingMessage`, `AlreadyAdjacentMessage`, `MovedMessagePrefix`, `PartialBlockMessagePrefix`, `OutOfOrderMessagePrefix`, `NotAdjacentMessagePrefix`, `NotFoundMessage`). DiagnosticWriter (task_06) MUST consume these by reference, never by literal. `MovedMessagePrefix` ends with `at position ` and the rule appends the post-move anchor body-paragraph index plus `)`.
- task_03 added `Phase3DocxFixtureBuilder.BuildHistoryParagraph(label, detail, separator=":", alignment?)`, `BuildIntroductionAnchorParagraph(text="INTRODUCTION", alignment?)` (run-direct bold so the section predicate passes), and `BuildBlankParagraph()`. task_04 should reuse these instead of building paragraph helpers ad-hoc.
- task_05 published `DiagnosticHistoryMove(Applied, SkippedReason, AnchorFound, FromIndex, ToIndexBeforeIntro, ParagraphsMoved)` and `DiagnosticSectionPromotion(Applied, SkippedReason, AnchorFound, AnchorParagraphIndex, SectionsPromoted, SubsectionsPromoted, SkippedParagraphsInsideTables, SkippedParagraphsBeforeAnchor)`. `DiagnosticFormatting` ctor now ends with `HistoryMove, SectionPromotion` (positional). JSON keys serialize as `historyMove` / `sectionPromotion` (camelCase, matching the existing `formatting.*` convention); nulls emit as `null` (not omitted). task_06 must populate these via the existing `DiagnosticWriter.JsonOptions` serializer and reuse `MoveHistoryRule.*Message` constants by reference.
- task_04 published `PromoteSectionsRule.AnchorMissingMessage`, `AnchorPositionMessagePrefix` (+P), `SummaryPromotedPrefix` / `SummarySectionsInfix` / `SummarySubsectionsSuffix` (composing `promoted {N} sections (16pt center) and {M} sub-sections (14pt center)`). DiagnosticWriter (task_06) MUST consume by reference. Rule does NOT currently expose `SkippedParagraphsInsideTables` / `SkippedParagraphsBeforeAnchor` counts via report messages; if task_06 needs them, add new INFO messages with prefix constants — counts are already tracked during the iteration in task_04.
- task_04 added `Phase3DocxFixtureBuilder.BuildSectionParagraph(text, alignment?)` and `BuildSubsectionParagraph(text, alignment?)` (both wrap `BuildParagraph(text, runDirectBold: true, alignment)`). Use in any future test needing bold-caps / bold-mixed-case body paragraphs. Existing `WrapInTable(params Paragraph[])` is the canonical helper for table-skip fixtures.
- task_06 chose minimal-coordination: NO new constants on `MoveHistoryRule` / `PromoteSectionsRule`; the writer parses what is already emitted. Fields the rules don't expose stay at default (`HistoryMove.FromIndex` = null; `SectionPromotion.SkippedParagraphsInsideTables` / `SkippedParagraphsBeforeAnchor` = 0). If a future task needs real values for these, add new public-const-string prefixes on the rules and emit additional `[INFO]` entries — the writer's parser already has helpers (`ParseTrailingInteger`, `TryParsePromotionSummary`) to extend.
