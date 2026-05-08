# Task Memory: task_03.md

Keep only task-local execution context here. Do not duplicate facts that are obvious from the repository, task file, PRD documents, or git history.

## Objective Snapshot

- Implement `MoveHistoryRule` (Optional, position #10) that detects the three-paragraph history block before the `INTRODUCTION` anchor and reorders it to immediately precede the anchor; emit precise `[INFO]`/`[WARN]` constants for every documented skip condition; preserve INV-01 multiset on every input.

## Important Decisions

- `MovedMessagePrefix` ends with `at position ` (trailing space) and the rule appends the anchor's body-paragraph index AFTER the move plus a closing `)`. DiagnosticWriter (task_06) can parse the trailing integer for `to_index_before_intro`.
- Adjacency check uses indexes among `body.Elements<Paragraph>().ToList()` — strict integer adjacency. Blank paragraphs between markers do not violate adjacency (only non-empty intervening paragraphs do).
- "Already adjacent" detection requires `received/accepted/published` to be at `anchorIndex-3,-2,-1` exactly; this guarantees idempotency on second invocation because the rule places them as immediate predecessors of anchor.
- "Not found" (silent INFO) fires only when no `Received:` marker exists. If `Received:` exists but `Accepted:` or `Published:` is missing, it's `partial_block` (WARN). This matches the PRD literal text.
- Detection scope is `body.Elements<Paragraph>()` (direct body children only) up to but not including `anchor`; markers in tables or after anchor are silently ignored.

## Learnings

- `body.Elements<Paragraph>()` returns `IEnumerable<Paragraph>` — must `.ToList()` before calling `IndexOf`. `IReadOnlyList<Paragraph>` does not expose `IndexOf` (only `MemoryExtensions.IndexOf` over `ReadOnlySpan` does), so test helpers should return `List<Paragraph>` if they need IndexOf.
- `Phase3DocxFixtureBuilder.BuildIntroductionAnchorParagraph` requires `runDirectBold: true` so `BodySectionDetector.FindIntroductionAnchor` accepts it (anchor must satisfy `IsSection` predicate, which needs ≥90% bold ratio).
- The "happy path" integration test must place a non-history paragraph between Published and the anchor (e.g., Keywords) to avoid the history being already-adjacent in the input — otherwise the rule emits `AlreadyAdjacentMessage` instead of `MovedMessagePrefix`.

## Files / Surfaces

- `DocFormatter.Core/Rules/MoveHistoryRule.cs` (new)
- `DocFormatter.Tests/MoveHistoryRuleTests.cs` (new) — 15 tests including the integration scenario
- `DocFormatter.Tests/Fixtures/Phase3/Phase3DocxFixtureBuilder.cs` — added `BuildHistoryParagraph`, `BuildIntroductionAnchorParagraph`, `BuildBlankParagraph`

## Errors / Corrections

- First write of `Phase3DocxFixtureBuilder` extension closed the class one brace too early; corrected before build.
- Initial integration test placed the history block immediately before the anchor (no separator), which made the rule emit `AlreadyAdjacent` instead of `Moved`; corrected by inserting `keywords` between Published and the anchor.

## Ready for Next Run

- task_04 (PromoteSectionsRule) and task_06 (DiagnosticWriter) consume the same anchor lookup and message constants. The constants are stable: `AnchorMissingMessage`, `AlreadyAdjacentMessage`, `MovedMessagePrefix`, `PartialBlockMessagePrefix`, `OutOfOrderMessagePrefix`, `NotAdjacentMessagePrefix`, `NotFoundMessage`.
