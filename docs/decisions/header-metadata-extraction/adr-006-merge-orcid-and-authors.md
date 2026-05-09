# ADR-006: Merge ExtractOrcidLinksRule and ParseAuthorsRule into a single ExtractAuthorsRule

## Status

Accepted

## Date

2026-05-06

## Context

The MVP shipped with two separate rules operating on the same paragraph (the authors paragraph located by `HeaderParagraphLocator.FindAuthorsParagraph`):

1. `ExtractOrcidLinksRule` (task_07) — found `<w:hyperlink>` elements whose URL contained `orcid.org`, extracted the ORCID ID from the URL, replaced the hyperlink with a plain `Run` containing the ID, and registered the run-index of the replacement in `FormattingContext.OrcidStaging` (`internal Dictionary<int, string>`).
2. `ParseAuthorsRule` (task_08) — re-located the same paragraph, iterated `paragraph.Elements<Run>()`, tokenized text by author separators (`, ` and ` and `), captured superscript labels as affiliation markers, and consumed `OrcidStaging` to attach ORCIDs to authors.

This split rested on a contract — `OrcidStaging` keyed by run-index — that was inherently fragile: the ORCID rule mutated the DOM with `ReplaceChild` *while* it computed indices, and the parser later iterated the post-mutation DOM expecting those indices to mean the same thing.

The bug surfaced when the editor ran the tool on `examples/1_AR_5449_2.docx`. That production article wraps each author **name** inside an ORCID-targeted hyperlink (rather than an icon image with a separate ORCID hyperlink). The ORCID rule replaced both hyperlinks with `Run`s containing only the bare ORCID IDs — destroying the author names — and the parser, blind to hyperlink content because of `Elements<Run>()`, produced two `Confidence=Low` records with empty `Name`. Diagnostic and report files captured the failure but the rewritten document had **no author lines at all**.

This is documented in `.compozy/tasks/header-metadata-extraction/reviews-002/issue_001.md` (the bug) and `issue_002.md` (this refactor). Both are marked resolved by the change recorded here.

## Decision

`ExtractOrcidLinksRule` and `ParseAuthorsRule` are merged into a single `DocFormatter.Core/Rules/ExtractAuthorsRule.cs` (`RuleSeverity.Optional`). The merged rule:

- Locates the authors paragraph via `HeaderParagraphLocator.FindAuthorsParagraph` once.
- Walks `paragraph.ChildElements` in document order. For each child:
  - **`Run` (non-superscript):** feeds the text into the author-name tokenizer (separators: `, ` and ` and `).
  - **`Run` (superscript):** splits text by `,` and appends each token as an affiliation label on the current author.
  - **`Hyperlink`:** resolves its relationship target URL.
    - If the URL contains the ORCID URL marker and the URL matches the ORCID-ID regex: stages the ID on the current author builder. If the hyperlink's inner content is recognised as **badge content** (contains `<w:drawing>`, is empty/whitespace, matches the ORCID-ID regex, or equals the literal text `"ORCID"`), the inner text is dropped; otherwise the inner text is fed into the same tokenizer as a regular Run. The hyperlink is removed from the DOM and the relationship is queued for deletion.
    - If the URL contains the ORCID URL marker but does not match the ID regex: emits a `[WARN]` (preserving the legacy "garbled ID" diagnostic) and tokenizes the inner text as name content (so a name inside a hyperlink with a malformed URL is still preserved).
    - If the URL does not contain the ORCID URL marker: tokenizes the inner text as name content. The hyperlink and its relationship stay in the DOM (no cleanup).
- Runs the same `FlagSuspicions` pass (suspicious-suffix, empty-fragment, non-alphabetic) as the legacy `ParseAuthorsRule`.
- Emits `Author` records via `ctx.Authors.Add(...)` with the same `[INFO]/[WARN]` messages.
- Performs the legacy free-standing-ORCID-badge warning pass (`<w:drawing>` siblings of the hyperlink, or any drawing with an embed pointing at an ORCID URL, that is not inside a hyperlink).
- Deletes the hyperlink relationships marked for cleanup, after a final reference check.

`FormattingContext.OrcidStaging` is removed. The CLI DI registration in `DocFormatter.Cli/CliApp.cs` registers one rule (`ExtractAuthorsRule`) in the position previously held by the two old rules, between `ParseHeaderLinesRule` and `RewriteHeaderMvpRule`.

The test layout collapses `ExtractOrcidLinksRuleTests.cs` and `ParseAuthorsRuleTests.cs` into `ExtractAuthorsRuleTests.cs`. The pipeline integration tests in `RewriteHeaderMvpRuleTests` and `LocateAbstractAndInsertElocationRuleTests` register `ExtractAuthorsRule` in place of the two old rules. A new test (`Apply_WithOrcidHyperlinkWrappingAuthorName_PreservesNameAndAttachesId` and the corresponding `Pipeline_FullPipeline_WithIssue001Reproducer_ProducesBothNamedAuthors`) reproduces the production fixture from issue 001 and asserts the two named authors come out with `Confidence=High`.

## Alternatives Considered

### Alternative 1: Patch `ExtractOrcidLinksRule` only

- **Description**: Preserve the inner text of hyperlinks before `ReplaceChild` instead of dropping it. Keep the two-rule split and the `OrcidStaging` contract.
- **Pros**: Minimal change; ~10 lines edited; no test reorganisation.
- **Cons**: The `OrcidStaging` run-index contract remains as a fragile coupling between rules. `ParseAuthorsRule.Elements<Run>()` would still be blind to non-ORCID hyperlinks, so any future paragraph that puts an author profile/email link inside the authors line would reintroduce the same class of bug.
- **Why rejected**: A real production article already exposed the boundary as wrong. Patching the symptom leaves the structural problem in place and continues to bias towards "incidental coupling" instead of "one rule, one logical record".

### Alternative 2: Keep two rules but key `OrcidStaging` by something more stable

- **Description**: Replace the run-index key with a per-author identifier (e.g., the hyperlink's inner text hash, or the hyperlink's `r:id` attribute). The ORCID rule still mutates the DOM but the parser indirects through the new key.
- **Pros**: Less invasive than a full merge; keeps the two rules visible in the pipeline.
- **Cons**: Still requires a cross-rule data structure. Picking the right key is itself fragile (a hyperlink could lack a stable identity), and the hyperlink-text-is-name case still requires the parser to read hyperlink content — which is exactly what motivates the merge. The two-rule split persists without buying a meaningful separation of concerns.
- **Why rejected**: The boundary is in the wrong place; relabelling the key does not fix that.

### Alternative 3: Merge but keep two rules visible in the pipeline (one orchestrating, one delegating)

- **Description**: A composite "ExtractAuthorsRule" that internally calls helpers named after the old rules, preserving the visual two-step structure.
- **Pros**: Makes the pipeline composition look familiar.
- **Cons**: Pure cosmetic; runtime is identical to the merged rule but with extra ceremony. Adds layers without adding value.
- **Why rejected**: The architecture's principle of "one rule, one concern" is better served by *changing what the concern is* than by faking the old split.

## Consequences

### Positive

- The `OrcidStaging` cross-rule contract is gone. Author records are produced in a single pass that owns both DOM mutation and value extraction for the authors paragraph.
- A real production article (issue 001 reproducer) now produces correct output without warnings on the author parse step.
- Hyperlinks containing author names are no longer destroyed regardless of their target URL (ORCID, profile link, or anything else).
- Test files and fixtures consolidate into one location; future contributors looking for "how authors are extracted" find a single rule with a single test file.

### Negative

- The first pipeline rule that *both* mutates the DOM and produces a value record now has a larger surface area than its peers. New contributors must read a longer rule file to understand author extraction.
- ADR-004 (the `Author` model and diagnostic-JSON contract) is unchanged, but task_07 / task_08 handoffs in `memory/MEMORY.md` no longer correspond to physical files in `Rules/`. The MEMORY.md update reflects this; future archaeology has to bridge the rename.

### Risks

- **Risk**: The badge-detection heuristic (`IsBadgeContent`) may misclassify a real author name as badge content (e.g., a single-word pen name like "Madonna" — would still parse correctly, but a name that exactly equals "ORCID" would be dropped).
  - **Mitigation**: The four heuristics (drawing inside, empty/whitespace, ORCID-ID regex match, literal "ORCID") cover the realistic icon-style hyperlinks. The literal-"ORCID" case is the only one where a real name could collide; the cost is one warning entry visible in the diagnostic JSON, not a silent corruption.
- **Risk**: A hyperlink whose inner Runs include their own superscripts (e.g., a hyperlink wrapping `Author Name<sup>1</sup>` together) flattens the superscript into the author name in the merged rule, because hyperlink content is read as a single text stream rather than walked run-by-run.
  - **Mitigation**: The current production fixture (`1_AR_5449_2.docx`) and the existing test fixtures place the superscript outside the hyperlink, matching how Word's own UI emits this structure. If a future article puts a superscript inside a hyperlink we revisit this with a real example.

## Implementation Notes

- The merged rule lives in `DocFormatter.Core/Rules/ExtractAuthorsRule.cs`. The pre-existing files `ExtractOrcidLinksRule.cs` and `ParseAuthorsRule.cs` are deleted, along with their test counterparts.
- `FormattingContext.OrcidStaging` is removed (was `internal Dictionary<int, string>`). No replacement seam is added; the merged rule does not need cross-call state.
- `[InternalsVisibleTo("DocFormatter.Tests")]` (already wired in `DocFormatter.Core/Properties/AssemblyInfo.cs`) keeps test access to internals available, though the merged rule does not introduce new internal types beyond the private `AuthorBuilder`.
- `RuleSeverity` of the merged rule is `Optional` (matches the legacy two rules). A failure inside the rule still does not abort the pipeline; `RewriteHeaderMvpRule` continues to write DOI/ELOCATION even with empty `ctx.Authors`.

## References

- [Issue 001 — bug](../reviews-002/issue_001.md)
- [Issue 002 — refactor](../reviews-002/issue_002.md)
- [ADR-004 — Author model & diagnostic JSON contract](adr-004.md)
- [PRD: Header Metadata Extraction](../_prd.md)
