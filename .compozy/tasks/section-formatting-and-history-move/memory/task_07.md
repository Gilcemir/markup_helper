# Task Memory: task_07.md

Keep only task-local execution context here. Do not duplicate facts that are obvious from the repository, task file, PRD documents, or git history.

## Objective Snapshot

Wire Phase 3 rules at positions #10/#11 of `CliApp.BuildServiceProvider`, extend rule-order test, and add happy-path + anchor-missing integration tests asserting INV-01 end-to-end.

## Important Decisions

- Triggered diagnostic-JSON write in the happy-path test via `Phase2Options(MalformedEmail: true)`. Phase 3 INFO-only signals do not write the file by themselves (`report.HighestLevel >= Warn` is the gate); the task spec explicitly requires "a Phase 1+2 warning unrelated to Phase 3" as the trigger.
- Modified `DocxFixtureBuilder.WriteValidDocx` (and siblings) to append a bold `INTRODUCTION` paragraph so existing happy-path tests do not regress to ⚠ via `MoveHistoryRule.AnchorMissingMessage` once Phase 3 rules become active. Without this, four pre-existing `CliIntegrationTests` regress.
- INV-01 in integration tests is asserted as a **Phase-3-text multiset preservation** check (`AssertPhase3TextsPreserved`): the count of each Phase-3-relevant string in the output `.docx` must be `>=` the count in the input. Phase 1+2 mutates other body texts (top table extraction, abstract heading split, email trailer removal), so a strict end-to-end multiset equality is not achievable; the helper instead enumerates the Phase 3 strings the fixture introduces and asserts they all survive.

## Learnings

- `Phase2DocxFixtureBuilder` private helpers (`BuildTopTable`, `BuildAuthorsParagraph`, etc.) are not exposed; the cleanest extension was to add `internal static List<OpenXmlElement> BuildPrologueElements(Phase2Options options)` and have Phase 3 fixtures call it before appending Phase 3 elements to the body list.
- C# record positional parameters allow named-arg construction, so `new Phase2Options(IncludeCorrespondingMarker: true, MalformedEmail: true)` works without redefining the record.
- The pipeline's `ExtractTopTableRule` removes the entire top table (including the `id`/`elocation`/`doi` header texts); `RewriteHeaderMvpRule` reinserts only the DOI value as a new paragraph and `LocateAbstractAndInsertElocationRule` reinserts the elocation. Header keys and the id value are gone from the body — this is why end-to-end multiset equality fails and INV-01 must be checked against the Phase-3-relevant subset.

## Files / Surfaces

- `DocFormatter.Cli/CliApp.cs` — appended `MoveHistoryRule` (#10) and `PromoteSectionsRule` (#11) registrations.
- `DocFormatter.Tests/CliIntegrationTests.cs` — extended rule-order test, added two integration tests (`Run_Phase3_HappyPath_*`, `Run_Phase3_AnchorMissing_*`), added INTRODUCTION anchor to `DocxFixtureBuilder.BuildBody`.
- `DocFormatter.Tests/Fixtures/Phase2/Phase2DocxFixtureBuilder.cs` — exposed `BuildPrologueElements` as `internal static`.
- `DocFormatter.Tests/Fixtures/Phase3/Phase3DocxFixtureBuilder.cs` — added `WritePhase123HappyPathDocx` / `WritePhase123AnchorMissingDocx` plus Phase 3 text constants.

## Errors / Corrections

- First test run: 4 pre-existing `CliIntegrationTests` regressed because `WriteValidDocx` lacked an `INTRODUCTION` anchor. Fixed by appending `BuildIntroductionAnchorParagraph()` to `BuildBody`.
- Second run: happy-path test failed because no diagnostic JSON was written (Phase 3 INFO-only does not trigger the file). Fixed by switching the happy-path fixture to `MalformedEmail: true` (Phase 1+2 warn from `ExtractCorrespondingAuthorRule`). Removed the now-incompatible "Corresponding author:" assertion.

## Ready for Next Run

- Phase 3 is fully wired into the CLI pipeline; the dependency chain for this PRD is complete.
- All 356 tests pass on `make build test`.

