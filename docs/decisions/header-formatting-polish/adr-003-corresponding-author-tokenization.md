# ADR-003: Marker tokenization and email regex for the corresponding-author rule

## Status

Accepted

## Date

2026-05-07

## Context

`ExtractCorrespondingAuthorRule` must detect a `*` marker that ties an affiliation paragraph to a corresponding author and surface the trailing email + ORCID. The `*` may appear in three forms in the input corpus:

1. **Plain text** in the affiliation paragraph: `2 Universidade Y * E-mail: foo@y.edu ORCID: https://orcid.org/0000-0002-1825-0097`
2. **Superscript** in the authors paragraph next to a label: `Maria Silva` + sup(`1,2*`)
3. **Plain text** in the authors paragraph right after a name: `Maria Silva*`

The marker token is the **combined `* E-mail:`** in the affiliation paragraph, not a bare `*`. A bare `*` could be a footnote or unrelated emphasis. The same affiliation paragraph carries the email (and possibly ORCID) that needs to surface.

The PRD's open question asks for a literal email regex shape; the editorial corpus uses ASCII institutional emails.

## Decision

### Marker tokenization

`ExtractCorrespondingAuthorRule` operates in two passes:

**Pass A — affiliation cleanup.** Walk every paragraph between the last `AuthorParagraphs` entry and the first paragraph that starts with one of `_options.AbstractMarkers`. For each paragraph, build the plain-text representation of the paragraph (concatenated `Text` elements + `Break` → `\n`). Search for the regex `\* *E-?mail *:` (case-insensitive, `*` is a literal asterisk). On match:

1. Take the substring from the matched `*` to the end of the paragraph as the trailer.
2. Remove all OOXML runs whose text content begins at or after the matched `*` from the paragraph. Runs that straddle the boundary (text before `*` and after `*` in the same run) are split: the run's `Text` is shortened to the pre-`*` portion; a new run is **not** created for the post-`*` portion (it is dropped).
3. Trim trailing whitespace from the paragraph.
4. Stash the paragraph reference in `FormattingContext.CorrespondingAffiliationParagraph` for diagnostic introspection.
5. Apply the email regex to the trailer; on hit, set `FormattingContext.CorrespondingEmail`.
6. Apply `_options.OrcidIdRegex` to the trailer; on hit, set `FormattingContext.CorrespondingOrcid`.

If the paragraph is empty after cleanup (the affiliation institutional text was empty before the `*`), remove the paragraph from the body. The PRD's open question defaults to "remove".

**Pass B — corresponding-author identification.** Walk the runs of every paragraph in `FormattingContext.AuthorParagraphs`. For each run:

- If the run is superscript and its text contains a literal `*`, the immediately preceding non-superscript run's accumulated name is the corresponding author.
- If the run is plain text and its text contains `*` immediately after an author name (whitespace then `*`, or end-of-name then `*`), the same identification applies.

Only the **first** `*` found wins. Subsequent `*` markers log `[WARN]`.

The matching is done against `FormattingContext.Authors` by exact name comparison. If no author matches (i.e., the `*` was found but the name boundary is ambiguous), the rule logs `[WARN]` and does not mutate any author, but **still** populates `CorrespondingEmail` from the affiliation pass.

### Email regex

The regex literal:

```text
[A-Za-z0-9._%+\-]+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,}
```

It is added to `FormattingOptions.EmailRegex` as a `[GeneratedRegex]` member, paralleling `DoiRegex`/`OrcidIdRegex`. Case-insensitive; culture-invariant. The regex is intentionally pragmatic and ASCII-only.

### Pre-existing "corresponding author" line detection

`RewriteAbstractRule` may also encounter a paragraph the original author typed by hand — `Corresponding Author: foo@x.com`, but possibly with typos (`Coresponding`), in lowercase, with a localized word (`Autor`), or with a non-canonical separator (`-`, ` —`, none). The matcher must accept all of these without matching `Correspondence:` (different concept used in some journals for letters to the editor).

The chosen pattern, anchored at paragraph start with `^\s*` and case-insensitive:

```text
^\s*c[oa]rr?es?p[a-z]*\s+au[a-z]*\b\s*[:\-—]?
```

- `c[oa]rr?es?p[a-z]*` — `Corresp`, `Coresp`, `Carresp`, `Corespon`, `Corresponding`, `Correspondent`, `Correspondign`, etc.
- `\s+au[a-z]*\b` — `Author`, `Authors`, `autor`, `Auther` etc., as a full word.
- `\s*[:\-—]?` — optional trailing separator.
- Crucially, requires the second word to start with `au` — `Correspondence` (no `au` continuation) does **not** match.

The regex lives in `FormattingOptions.CorrespondingAuthorLabelRegex`. `RewriteAbstractRule` runs it against the plain-text representation of each paragraph between the author block and the abstract paragraph; first hit wins.

### Fallback email recovery

When `ExtractCorrespondingAuthorRule` did not populate `CorrespondingEmail` (no `*` marker in the affiliation block), `RewriteAbstractRule` attempts a **fallback** on the matched pre-existing line: it runs `EmailRegex` against the paragraph's plain text. On hit, `CorrespondingEmail` is set and the paragraph is replaced with the canonical version. On miss, the paragraph is left untouched (the rule does not destroy author-typed content with no recoverable signal).

This split keeps the two extraction sources (`* E-mail:` marker vs. pre-existing typed line) in their natural rules — `ExtractCorrespondingAuthorRule` owns the affiliation-trailer path; `RewriteAbstractRule` owns the typed-line path because its detection happens at the same scan as the canonical-line replacement.

### Marker and label options summary

`FormattingOptions` gains:

- `CorrespondingMarkerRegex` — `\* *E-?mail *:` (compiled once via `[GeneratedRegex]`).
- `EmailRegex` — the email pattern above.
- `CorrespondingAuthorLabelRegex` — the typo-tolerant label pattern above.

`OrcidIdRegex` is reused.

## Alternatives Considered

### Alternative 1: Match the bare `*` as the marker

- **Description**: Trigger on any `*` in the affiliation paragraph and try to extract email afterward.
- **Pros**: Slightly simpler regex.
- **Cons**: False positives on footnotes (`*footnote text`) and authorial emphasis. Risks stripping legitimate paragraph content.
- **Why rejected**: PRD explicitly requires the combined `* E-mail:` token.

### Alternative 2: Use a stricter RFC 5322-compliant email regex

- **Description**: Adopt the full RFC 5322 lexical grammar.
- **Pros**: Correct on every legal email.
- **Cons**: ~6× longer regex; not maintainable; the corpus does not contain edge cases that need it.
- **Why rejected**: YAGNI; the pragmatic ASCII pattern matches every email in the corpus.

### Alternative 3: Single-pass rule that ties affiliation cleanup and author identification together

- **Description**: One pass that walks affiliations and authors simultaneously.
- **Pros**: One traversal.
- **Cons**: Author detection runs against a possibly non-aligned affiliation order (not all papers list affiliations in the same order as authors). Two passes are clearer and the cost is negligible (≤ 10 paragraphs each).
- **Why rejected**: Clarity and testability.

### Alternative 4: Strict literal match for pre-existing "Corresponding Author:" line

- **Description**: Require the literal `Corresponding Author:` (or case-insensitive variant) to detect typed lines.
- **Pros**: Zero false positives.
- **Cons**: Misses every typo the user explicitly raised — `coresponding`, `Correspondign`, `Autor`, dashes-instead-of-colon, etc. The whole point of the user's concern would be defeated.
- **Why rejected**: User constraint.

## Consequences

### Positive

- Marker detection is deterministic and centralized in two regexes registered in `FormattingOptions` (consistent with how DOI/ORCID/ELOCATION are wired).
- The affiliation paragraph reference is shared via `FormattingContext`, so downstream rules (or future `ParseAffiliationsRule`) can introspect it.
- The two-pass design lets `[WARN]`s fire from each pass independently and lands them with descriptive messages in the report.

### Negative

- The literal asterisk-in-superscript detection adds OOXML-specific logic (looking for `<w:vertAlign w:val="superscript"/>` on the run carrying `*`).
- Email regex is intentionally lax — international institutional addresses with non-ASCII local parts would not match. Out of scope per PRD.

### Risks

- **Risk**: An author has a name containing `*` as part of an annotation (rare).
  - **Mitigation**: First-`*`-wins; subsequent ones `[WARN]`. Author identification still runs and the rule's mutations remain best-effort.
- **Risk**: An affiliation paragraph has the trailer split across multiple OOXML runs, with `*` in one run and `E-mail:` in the next.
  - **Mitigation**: The plain-text representation concatenates all runs first, runs the regex against the full paragraph text, then maps the regex offset back to a run boundary. The split is handled at the offset, not the run level.
- **Risk**: A paragraph contains the `* E-mail:` marker but the email regex finds nothing usable.
  - **Mitigation**: PRD requires the trailer to still be stripped (best-effort cleanup) and a `[WARN]` to log; the rule emits both.
- **Risk**: `CorrespondingAuthorLabelRegex` matches a paragraph that is not actually a typed corresponding-author line (false positive — e.g., a methods sentence "the corresponding author of cited reference 12 …").
  - **Mitigation**: The regex is anchored with `^\s*` (paragraph start). The rule only runs the match on paragraphs between the author block and the abstract paragraph — body paragraphs are out of scope. This narrows the blast radius to the front matter.
- **Risk**: A typed line is matched and removed, but it carried trailing content the editor wanted to keep (e.g., a phone number).
  - **Mitigation**: Removal happens only when an email is available (either from the `*` marker or recovered from the typed line itself). When the rule has no email and the typed line is unparseable, the paragraph is left untouched.

## Implementation Notes

- Regex compilation lives in `FormattingOptions` with `[GeneratedRegex]` (matches existing pattern).
- The plain-text → offset → run mapping is a small helper added to `HeaderParagraphLocator` (or a new `AffiliationParagraphLocator` if `HeaderParagraphLocator` becomes too overloaded).
- Empty-affiliation cleanup uses `Paragraph.Remove()`; the next rule (`EnsureAuthorBlockSpacingRule`) recomputes the boundary from the surviving paragraphs in `FormattingContext.AuthorParagraphs`.

## References

- [PRD: Header Formatting Polish](../_prd.md) — Feature 4 and Open Questions section
- [ADR-001: Four discrete Optional rules](adr-001-four-discrete-rules.md)
- `FormattingOptions.cs` — existing `[GeneratedRegex]` patterns
