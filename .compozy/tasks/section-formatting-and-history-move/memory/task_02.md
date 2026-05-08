# Task Memory: task_02.md

Keep only task-local execution context here. Do not duplicate facts that are obvious from the repository, task file, PRD documents, or git history.

## Objective Snapshot

Replaced the obsolete `Skeleton_*` placeholders in `BodySectionDetectorTests.cs` with the full predicate + anchor matrix mandated by the task spec. The four `BodySectionDetector` methods themselves were already shipped in commit `11f937a` (under task_03's commit because task_03 needed them).

## Important Decisions

- Did not touch `BodySectionDetector.cs` or `Phase3DocxFixtureBuilder.cs`: both already satisfy the task's requirements (alignment param, `BuildParagraphWithRuns`, `WrapInTable`, `WrapInNestedTable`, `BuildIntroductionAnchorParagraph`).
- Modeled the 90% boundary test as `"HEADERVAL"` (9 bold) + `"X"` (1 non-bold) → exactly 9/10 = 90%; `boldNonWhitespace * 10 >= totalNonWhitespace * 9` evaluates true at the boundary.
- Kept `IsSubsection_AllCapsParagraphAlsoIsSection_ReturnsFalse` asserting `IsSection=true` AND `IsSubsection=false` on the same paragraph to prove mutual exclusivity in one shot.
- Integration test classifies `Abstract` heading as `IsSubsection=true`: the predicate is purely visual; the rule layer (PromoteSectionsRule) excludes it via `FormattingContext` reference equality.

## Learnings

## Files / Surfaces

- `DocFormatter.Tests/BodySectionDetectorTests.cs` — replaced 4 `Skeleton_*` tests with 28 new tests (3 IsInsideTable + 11 IsSection + 3 IsSubsection + 13 FindIntroductionAnchor + 1 integration). Total in file: 51 (23 IsBoldEffective from task_01 + 28 new).

## Errors / Corrections

- Shared memory previously claimed task_02 work was "complete in working tree (not yet committed)" but tests were still placeholders — the implementation was committed under task_03, only the tests were missing.

## Ready for Next Run

- task_04 (`PromoteSectionsRule`) can rely on `IsSection`, `IsSubsection`, `IsInsideTable`, and `FindIntroductionAnchor` being fully covered by tests; failures in those methods will surface in this file rather than in `PromoteSectionsRuleTests`.
