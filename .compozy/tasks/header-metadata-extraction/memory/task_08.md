# Task Memory: task_08.md

## Objective Snapshot

`ParseAuthorsRule` (Optional severity) — splits the authors paragraph by `FormattingOptions.AuthorSeparators`, attaches superscript labels and staged ORCID IDs per author, tags each author with `AuthorConfidence`, and emits structured `[WARN]` entries on the suspicions enumerated in the task spec (req 6a–c).

## Important Decisions

- Extended the `Author` record with a fourth positional parameter `AuthorConfidence Confidence = AuthorConfidence.High` (new enum `AuthorConfidence { High, Medium, Low, Missing }` matching ADR-004's diagnostic JSON contract). Default `High` keeps task_03's value-equality tests passing without changes. `Equals` / `GetHashCode` / `PrintMembers` updated to include `Confidence`. `PrintMembers` still omits null `OrcidId`.
- Suspicion (a) is detected by checking each post-split fragment against `^(Jr|Sr|II|III|IV)\.?$` (case-insensitive). When matched at index `i > 0`, BOTH `builders[i-1]` and `builders[i]` are marked `Low` because both are casualties of the same bad split; warnings are emitted per author, prefixed with `author #N ('name'):`.
- Suspicion (b) is deferred per the task spec — affiliation paragraph parsing is out of MVP scope. The rule emits a single `[INFO]` summarizing the count of authors and distinct labels.
- Empty/whitespace-only paragraphs (zero parseable fragments) emit a single `[WARN]` ("authors paragraph yielded zero parseable name fragments") and add no `Author` records. Empty fragments inside a populated paragraph (e.g., `A, , B`) DO get emitted as empty-name `Author(Confidence=Low)` records so downstream rules see the parse holes.

## Learnings

- ORCID staging keys are correctly aligned by walking `paragraph.Elements<Run>()` directly — the index `i` in the walk equals the `runIndex` task_07 stored in `OrcidStaging`. Hyperlink children that survived task_07 are NOT walked (they remain inside the paragraph but are ignored by this rule). For the MVP this is acceptable since only ORCID hyperlinks appeared in author paragraphs.
- `[GeneratedRegex]` on a partial method works inside a `sealed partial class`; mirror the pattern from `FormattingOptions` rather than allocating a static `Regex` per call.
- `string.CompareOrdinal(text, position, separator, 0, separator.Length)` is the cleanest way to test a separator at a position without allocating substrings; longest-match wins so `" and "` beats `", "` if both could start at the same index (they cannot, given the literal characters, but the order is defensive).

## Files / Surfaces

- `DocFormatter.Core/Models/AuthorConfidence.cs` (new enum)
- `DocFormatter.Core/Models/Author.cs` (extended with `Confidence` field)
- `DocFormatter.Core/Rules/ParseAuthorsRule.cs` (new rule, ~210 lines)
- `DocFormatter.Tests/Fixtures/Authors/AuthorsParagraphFactory.cs` (new helper)
- `DocFormatter.Tests/ParseAuthorsRuleTests.cs` (new test file: 8 named scenarios + 1 integration test + 2 coverage tests for empty/no-alpha paths = 11 tests)

## Errors / Corrections

- First attempt skipped emission of empty-name builders, which broke the test that exercises `A, , B` (only 2 authors emitted instead of 3). Fixed by emitting empty fragments as `Author(Confidence=Low)` and reserving the "skip + warn" branch only for the all-empty-paragraph case.

## Ready for Next Run

- task_08 status: completed; tests green (68/68 solution-wide), build clean, 0 warnings.
- task_09 (`RewriteHeaderMvpRule`) consumes `ctx.Authors`. It will receive `Author` records with `Confidence` set; rendering should skip records whose `Name` is empty (these are flagged-low parse holes, not real authors). The rule SHOULD NOT crash on empty-name records; just omit them from the rewrite.
