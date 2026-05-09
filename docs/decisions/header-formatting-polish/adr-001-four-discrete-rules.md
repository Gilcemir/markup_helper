# ADR-001: Four discrete Optional rules over a single consolidated rewrite

## Status

Accepted

## Date

2026-05-07

## Context

The MVP shipped with `RewriteHeaderMvpRule` (Critical) writing the four MVP fields (DOI, section, title, authors). This PRD extends the front-matter normalization with four behavior groups:

1. Alignment of DOI (right), section (right), title (center).
2. Blank line below the affiliation block (above the abstract).
3. Abstract paragraph rewritten as `**Abstract**` (bold, own line) + body without the structural italic wrapper, preserving intentional internal italic emphasis.
4. Corresponding-author contact: extract `* E-mail:` + ORCID from affiliation, clean the trailing `*` content from the affiliation line, and insert a `Corresponding author: foo@bar.com` paragraph immediately before the abstract.

The user explicitly chose `Optional` severity for all four (`* E-mail:` failure included — a paper without a declared corresponding author must still produce output). Because the existing `RewriteHeaderMvpRule` is `Critical` and any failure aborts the document, piling these new behaviors inside it would force them to share its severity.

The project's pipeline architecture is built on the principle "one rule, one concern". Five rules already follow this contract; tests, diagnostic JSON, and ADR-006 of the previous task explicitly invoke this principle as the reason refactors happen.

## Decision

Implement four new sibling rules in `DocFormatter.Core/Rules/`, each `Optional`, each with its own xUnit test file:

- `ExtractCorrespondingAuthorRule` — runs **before** `RewriteHeaderMvpRule`. Detects the affiliation paragraph containing `* E-mail:`, extracts the email and (when applicable) the ORCID, removes the `*…` trailing content from that affiliation line, marks the corresponding author in `FormattingContext`, and stages a "Corresponding author: …" paragraph for insertion. The rule **does not insert** the email line yet — it stores the email and a target-paragraph reference in the context for `RewriteAbstractRule` (or a successor) to insert immediately above the abstract.
- `ApplyHeaderAlignmentRule` — runs **after** `RewriteHeaderMvpRule`. Reads paragraph references already known to the pipeline (DOI paragraph: top of body after `RewriteHeaderMvpRule`; section/title paragraphs: tracked in `FormattingContext` by `ParseHeaderLinesRule`) and applies `Justification` (right / right / center).
- `EnsureAuthorBlockSpacingRule` — runs **after** alignment. Reads `FormattingContext.AuthorBlockEndParagraph` (set by `RewriteHeaderMvpRule` to the last new author paragraph) and walks forward to the next non-blank paragraph (the first affiliation). Inserts a blank paragraph immediately before that affiliation paragraph if the immediately preceding paragraph is not already blank.
- `RewriteAbstractRule` — runs **before** `LocateAbstractAndInsertElocationRule`. Locates the abstract paragraph (reusing `_options.AbstractMarkers`), rewrites it as one paragraph containing only `Abstract` in bold, followed by a paragraph with the abstract body **without the structural italic wrapper**. Internal italic runs (e.g., scientific names) are preserved by keeping the existing run-level italic only when the run does not span the entire content. Also performs the corresponding-author email insertion: if `FormattingContext.CorrespondingEmail` is populated, inserts a paragraph immediately before the new "Abstract" paragraph with the literal text `Corresponding author: <email>`.

`FormattingContext` gains:

- `string? CorrespondingEmail` — set by `ExtractCorrespondingAuthorRule`, consumed by `RewriteAbstractRule`.
- `Paragraph? SectionParagraph`, `Paragraph? TitleParagraph` — populated by `ParseHeaderLinesRule` for `ApplyHeaderAlignmentRule` to consume.
- `Paragraph? CorrespondingAffiliationParagraph` — the affiliation paragraph that contained the `*` trailer (already mutated by `ExtractCorrespondingAuthorRule`).
- `Paragraph? DoiParagraph` — set by `RewriteHeaderMvpRule` (it already creates the DOI paragraph; just stash the reference).
- `Paragraph? AuthorBlockEndParagraph` — set by `RewriteHeaderMvpRule` to the **last new author paragraph** it inserts; consumed by `EnsureAuthorBlockSpacingRule` as the starting point for the "blank between authors and affiliations" decision (the original author paragraphs are removed during the rewrite, so a stale reference would not work).

The diagnostic JSON schema is extended with `correspondingEmail`, `alignmentApplied`, `abstractFormatted`, `authorBlockSpacingApplied` fields populated only when the corresponding rule emits a `[WARN]`/`[ERROR]`.

## Alternatives Considered

### Alternative 1: Two consolidated rules

- **Description**: `ApplyHeaderFormattingRule` (alignment + spacing) and `RewriteAbstractAndCorrespondingRule` (abstract + email + ORCID).
- **Pros**: Fewer files (2 rules + 2 test files instead of 4+4); shorter pipeline registration.
- **Cons**: Mixes unrelated concerns within each class. A failure inside the alignment helper of `ApplyHeaderFormattingRule` could abort the spacing helper too unless explicit try/catch surrounds each helper, reproducing the "small rules" pattern internally with worse ergonomics. Diagnostic JSON would have to manually flag which sub-rule failed.
- **Why rejected**: The project already has five sibling rules averaging ~150 lines each. Two new bigger rules would deviate from a pattern that was reinforced by ADR-006 of the previous task. Aggregation here buys nothing the small-rule layout does not provide.

### Alternative 2: Extend `RewriteHeaderMvpRule`

- **Description**: Pile alignment, spacing, abstract rewrite, email insertion, ORCID linking inside the existing rule.
- **Pros**: Single file change; no new pipeline registrations.
- **Cons**: The rule is `Critical` — any failure aborts the document, contradicting the user's explicit choice of `Optional` for all four behaviors. Surface area would push past 300 lines and mix four concerns in one class. Test reorganization is required (existing tests focus on header MVP behavior). Future rules (Keywords, section promotion) would have nowhere to slot in cleanly.
- **Why rejected**: Critical severity is an unacceptable side-effect; it forces all-or-nothing on cosmetic operations. Even if severity were lowered, a 4-concern class breaks the project's pattern.

### Alternative 3: Three rules — fold spacing into alignment

- **Description**: `ApplyHeaderAlignmentRule` also performs the blank-line-below-affiliations check; only three new rules total.
- **Pros**: Saves one file.
- **Cons**: Spacing depends on the affiliation block boundary (which has its own detection logic shared with future `ParseAffiliationsRule`). Bundling it with paragraph alignment couples two unrelated detections. Diagnostic granularity decreases.
- **Why rejected**: Marginal win; the saved file isn't worth diluting `ApplyHeaderAlignmentRule`'s purpose.

## Consequences

### Positive

- Each rule is independently testable, independently fail-safe (Optional), and independently observable in the diagnostic JSON.
- Pipeline composition stays declarative; a future contributor reads `CliApp.cs` and sees the order of operations as a single linear list.
- `FormattingContext` becomes the single source of truth for cross-rule paragraph references — no rule needs to relocate something the previous rule already found.
- Future rules in the master plan (`PromoteSectionsRule`, `NormalizeQuotesRule`, etc.) drop into the same layout without precedent inversion.

### Negative

- Eight new files (4 rules + 4 tests) for the implementation; pipeline registration in `CliApp.cs` grows from 5 to 9 lines.
- `FormattingContext` gains four new properties carrying live `Paragraph` references. Mutations elsewhere in the body could invalidate them — rules must be careful not to delete a paragraph they pass downstream.

### Risks

- **Risk**: `ExtractCorrespondingAuthorRule` runs before `RewriteHeaderMvpRule` in this design, so it must operate on the original (input) DOM. After the rewrite, paragraph indices shift; any later rule that needs the affiliation paragraph must rely on the reference stored in the context, not on positional lookup.
  - **Mitigation**: Store the affiliation `Paragraph` reference in `FormattingContext.CorrespondingAffiliationParagraph` before `RewriteHeaderMvpRule` runs. The reference remains valid as long as the paragraph is not removed (and `RewriteHeaderMvpRule` does not touch affiliation paragraphs).
- **Risk**: Internal italic preservation in the abstract is heuristic ("italic that wraps the entire paragraph" vs. "italic on a sub-run"). A pathological case where intentional italic spans 100% of the body would be misclassified as the structural wrapper.
  - **Mitigation**: The detection is "italic property is set on every run that contains non-whitespace text *and* removing it leaves the paragraph fully de-italicized" — i.e., we only strip italic when the alternative is to leave the wrapper that the user explicitly wants removed. Document the limitation in the user-facing PRD as a known edge case with a `[WARN]` in the report when stripped.

## Implementation Notes

- New rules live in `DocFormatter.Core/Rules/`. Test counterparts live in `DocFormatter.Tests/`. CLI registration in `DocFormatter.Cli/CliApp.cs` adds the four rules in the order described above.
- `FormattingOptions` gains an `EmailRegex` and a `CorrespondingMarker` (`"* E-mail:"` with case-insensitive matching). The email regex follows RFC 5322's local-part-friendly pragmatic pattern (no exotic unicode local parts; the editorial corpus uses ASCII institutional emails).
- Diagnostic JSON serializer (`DocFormatter.Core/Reporting/`) is updated to include the four new fields under a new `formatting` section; backward compatibility is preserved (consumers reading only the legacy keys keep working).
- ADR-001 of the previous task (`header-metadata-extraction`) introduced the canonical 14-rule plan from `instructions.md`. This ADR consumes slots adjacent to rule 8 (ParseAbstractRule) and rule 11 (PromoteSectionsRule) but does not yet implement those parent rules; behaviors are aligned with the spec for forward compatibility.

## References

- [PRD: Header Formatting Polish](../_prd.md)
- [Previous PRD: Header Metadata Extraction](../../header-metadata-extraction/_prd.md)
- [Previous ADR-006: Merge ORCID and Authors](../../header-metadata-extraction/adrs/adr-006-merge-orcid-and-authors.md)
- [`instructions.md`](../../../../instructions.md) — pipeline master plan, output format spec
