# Task Memory: task_01.md

Keep only task-local execution context here. Do not duplicate facts that are obvious from the repository, task file, PRD documents, or git history.

## Objective Snapshot

- Create `BodySectionDetector` (internal static, `DocFormatter.Core.Rules`) skeleton + `IsBoldEffective` cascade walker per ADR-005.
- Add `Phase3DocxFixtureBuilder` extensible fixture under `DocFormatter.Tests/Fixtures/Phase3/`.
- Cover all six cascade patterns plus explicit-false override, val=0/1, deep chain.

## Important Decisions

- `ResolveBold` collapses to `bold.Val?.Value ?? true` — relies on the OpenXml SDK normalising `0/false/1/true/no-attr` into `OnOffValue`. Keeps the helper readable and keeps the explicit-false override correct.
- For paragraph and style `<w:pPr><w:rPr><w:b/>` reads, used `GetFirstChild<ParagraphMarkRunProperties>()` instead of any typed property. `StyleParagraphProperties` does not expose `ParagraphMarkRunProperties` as a typed member — only via `GetFirstChild<T>()`.
- Depth limit implemented as a 10-iteration `for` loop. The 10th hop is allowed to resolve bold; the 11th is the abort point (verified by test pair "depth=10 resolves" / "depth=12 aborts").

## Learnings

- `OnOffValue` lives in `DocumentFormat.OpenXml` (root namespace), not `Wordprocessing`. Tests must `using DocumentFormat.OpenXml;` for `OnOffValue.FromBoolean`.
- `[InternalsVisibleTo("DocFormatter.Tests")]` is wired in `DocFormatter.Core/Properties/AssemblyInfo.cs` — internal helpers/tests are accessible from the test project without extra config.

## Files / Surfaces

- `DocFormatter.Core/Rules/BodySectionDetector.cs` (new)
- `DocFormatter.Tests/Fixtures/Phase3/Phase3DocxFixtureBuilder.cs` (new)
- `DocFormatter.Tests/BodySectionDetectorTests.cs` (new — 23 tests)

## Errors / Corrections

- Initial test file referenced bare `OnOffValue` without `using DocumentFormat.OpenXml;` — build failed with CS0103. Added the namespace import; replaced inline `DocumentFormat.OpenXml.SpaceProcessingModeValues.Preserve` with the unqualified form for consistency.

## Ready for Next Run

- task_02 implements the predicate skeletons (`IsSection`, `IsSubsection`, `IsInsideTable`, `FindIntroductionAnchor`). It can extend `Phase3DocxFixtureBuilder` with table-wrapped paragraph helpers and uppercase/casing variants without touching this task's helpers.
- `BodySectionDetector.IsBoldEffective` is `internal static`; the predicate methods that rely on it are `public static` per the TechSpec "Core Interfaces" signature, so no visibility change needed downstream.
