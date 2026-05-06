# Task Memory: task_09.md

Keep only task-local execution context here. Do not duplicate facts that are obvious from the repository, task file, PRD documents, or git history.

## Objective Snapshot

- Implement `RewriteHeaderMvpRule` (Critical) consuming the populated `FormattingContext` to rewrite the four MVP header fields (DOI, section, title+blank-separator, authors) without touching anything below the authors block.

## Important Decisions

- Affiliation labels are emitted as a single superscript run with text `string.Join(",", labels)` because `Author.AffiliationLabels` is `IReadOnlyList<string>` — the original delimiter is not preserved on the record. This matches the visual rendering tests assert (e.g., `"Maria Silva1"` for `["1"]`).
- Empty-name `Author` records are filtered out before paragraph build (per task_08 memory). The Critical-throw branch fires only on the canonical empty `ctx.Authors` list, not on a "filtered to zero" case — keeps the rule strictly compliant with requirement #6's wording while still respecting task_08's parse-hole convention.
- DOI insertion uses `body.InsertBefore(doi, firstNonSectionPropertiesChild)` so the line lands at body[0] for normal documents and the trailing `SectionProperties` (when present in real-world docs) is not displaced.

## Learnings

- `Body.Elements<Paragraph>().Count` includes the inserted blank paragraph because `new Paragraph()` still serializes to `<w:p/>` — tests rely on this when asserting paragraph indices.
- `OuterXml` snapshots before/after on the elements below the authors block are sufficient to prove "byte-identical" preservation without serializing the whole document.
- `FormattingPipeline` already exposed in the test project — the integration test reuses the existing 4-rule fixture (from task_08) and just appends `RewriteHeaderMvpRule` plus stricter assertions on output paragraph order.

## Files / Surfaces

- `DocFormatter.Core/Rules/RewriteHeaderMvpRule.cs` — new file
- `DocFormatter.Tests/RewriteHeaderMvpRuleTests.cs` — new file (9 tests: DOI present, DOI null+warn, two authors mixed ORCID, no-labels, empty-name skip, empty Authors throw, null ArticleTitle throw, below-authors preservation, full 5-rule pipeline)

## Errors / Corrections

- None.

## Ready for Next Run

- task_10 (`LocateAbstractAndInsertElocationRule`) — independent of task_09; depends only on tasks 04 and 05. Should slot in after rule 5 in CLI registration order (per techspec rule table).
- task_11 CLI bootstrap will register rules in order: ExtractTopTable → ParseHeaderLines → ExtractOrcidLinks → ParseAuthors → RewriteHeaderMvp → LocateAbstractAndInsertElocation.
