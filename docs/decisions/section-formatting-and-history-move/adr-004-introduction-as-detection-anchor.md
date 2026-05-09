# ADR-004: `INTRODUCTION` as detection anchor for section/sub-section scope

## Status

Accepted

## Date

2026-05-07

## Context

The detection predicate for sections (bold + caps) and sub-sections (bold + mixed case), as scoped in ADR-003, is purely visual. Without a positional constraint, it would match any bold paragraph in the document, including:

- The article-type label rewritten by Phase 1 (`ARTICLE`, `CULTIVAR RELEASE`, `ORIGINAL RESEARCH`, etc.) — bold and all-caps.
- The article title rewritten by Phase 1 — bold and mixed case.
- The `Abstract` heading rewritten by Phase 2's `RewriteAbstractRule` — bold and mixed case.
- The `Corresponding author:` line inserted by Phase 2 — potentially bold depending on style cascade.

Phase 1+2 already published references to the relevant header paragraphs in `FormattingContext` (`SectionParagraph`, `TitleParagraph`, `DoiParagraph`). Skipping these by reference handles three of the four cases above. But this only works if Phase 1+2 ran successfully and populated the context. A degraded run, or a future change to the context model, would expose the false positives.

A more robust safeguard is positional: identify the first body paragraph that anchors the article body and treat everything before it as out of scope for Phase 3 detection.

In the editor's stated convention, the first section of every article is always `INTRODUCTION`. Empirical inspection of three articles confirmed this; the editor confirmed this is the canonical convention.

## Decision

Adopt the first paragraph matching `^INTRODUCTION[\s.:]*$` (after trimming) and satisfying the section predicate (bold-caps, outside table, alignment ∈ {Left, Both, none}) as the **detection anchor** for Phase 3:

1. `PromoteSectionsRule` only considers paragraphs at or after this anchor in body order.
2. `MoveHistoryRule` only considers history-marker paragraphs that appear before this anchor (i.e., in the front matter region).
3. If the anchor cannot be found, both rules emit a `[WARN]` with reason `anchor_missing` and skip their work entirely.

The regex `^INTRODUCTION[\s.:]*$` accepts `INTRODUCTION`, `INTRODUCTION:`, `INTRODUCTION.`, and `INTRODUCTION ` (trailing whitespace) but rejects `INTRODUCTION bla bla bla`, `1. INTRODUCTION`, and `INTRODUÇÃO`. Combined with the requirement that the anchor must also be the first qualifying caps-bold paragraph in the body, the rule rejects degenerate cases without false positives.

## Alternatives Considered

### Alternative 1: Skip-by-reference only (`SectionParagraph`, `TitleParagraph`, `DoiParagraph`)

- **Description**: Rely entirely on `FormattingContext` references to skip the rewritten header paragraphs.
- **Pros**:
  - No new positional concept; reuses existing context state.
- **Cons**:
  - Does not skip Phase 2 insertions like the bold `Abstract` heading or `Corresponding author:` line, which are not stored in the context as named references.
  - Fails open: if Phase 1 fails and `SectionParagraph` is null, `ARTICLE` is reformatted as a section, regressing Phase 2's right-alignment.
  - Couples Phase 3 to the exact field set of `FormattingContext`; future context refactors could silently break the safeguard.
- **Why rejected**: The user explicitly raised "what about article-type label" as a worry. A positional safeguard makes the constraint declarative rather than reference-based.

### Alternative 2: Multi-language anchor list (`INTRODUCTION`, `INTRODUÇÃO`, `INTRODUCCIÓN`, …)

- **Description**: Match any of a configurable list of language variants.
- **Pros**:
  - Covers Portuguese papers if the journal ever publishes them.
- **Cons**:
  - The journal publishes in English; no Portuguese papers exist in `examples/`.
  - Adds configuration surface for a non-existent need (YAGNI).
- **Why rejected**: The editor confirmed the journal is English-only. If Portuguese papers ever appear, ADR-004 can be superseded with a multi-language list. For now, anchor is `INTRODUCTION` (English only).

### Alternative 3: Anchor by paragraph index instead of text content

- **Description**: Treat "the Nth paragraph" as the boundary, where N is determined by Phase 1+2 metadata.
- **Pros**:
  - Independent of section name.
- **Cons**:
  - There is no reliable index to use: the front-matter paragraph count varies per article.
  - Requires tracking the boundary in `FormattingContext`, adding state.
- **Why rejected**: Anchoring by content is more robust to future Phase 1+2 changes that insert or remove front-matter paragraphs.

## Consequences

### Positive

- Detection scope is positional and declarative: paragraphs before the anchor are exempt regardless of how Phase 1+2 chose to lay them out.
- The article-type label (`ARTICLE`, `CULTIVAR RELEASE`, …), article title, `Abstract` heading, `Corresponding author:` line, and any future Phase 2 front-matter insertion are all automatically excluded.
- The anchor is also reused by `MoveHistoryRule` as the destination for the move; one concept does double duty.
- Detection scope ends at the document boundary; `REFERENCES` is correctly classified as the last section, and post-`REFERENCES` content (figure/table captions, bibliography entries) does not match the predicate (not bold-caps, or inside `<w:tbl>`).

### Negative

- A document genuinely lacking `INTRODUCTION` (extremely rare in this journal's submissions) gets no Phase 3 formatting at all. **Note**: this is the falha-segura behaviour mandated by ADR-002 — better to skip than to corrupt.
- The literal `INTRODUCTION` is hard-coded. A typo (`INTRODUTION`) or rare alternative spelling causes a graceful skip with `[WARN]`.

### Risks

- **Risk**: A future article uses `INTRODUCTION` as a body word in caps without it being the first section heading. **Mitigation**: the predicate also requires bold and the paragraph text to match the regex exactly (no surrounding text), making the collision essentially impossible.
- **Risk**: An article uses a slight variant like `INTRODUCTION (Background)`. **Mitigation**: the regex rejects this; the rule emits `[WARN] anchor_missing` and the editor knows to inspect. If this variant becomes common, the regex can be relaxed.

## Implementation Notes

- Anchor lookup is a single linear scan of body paragraphs. It runs once per rule that needs it (`MoveHistoryRule` and `PromoteSectionsRule`) — two scans total. Cost is negligible for documents up to a few hundred paragraphs.
- The "first qualifying caps-bold paragraph" check guards against the regex alone matching a degenerate paragraph. Both conditions must hold.
- The anchor paragraph itself is **included** in `PromoteSectionsRule`'s formatting pass: `INTRODUCTION` is the first section to be reformatted to `16pt + bold + center`.
- Comparison is **case-sensitive** on the literal string `INTRODUCTION` (not `Introduction`), per the empirical convention.

## References

- [PRD: Section Formatting and History Move](../_prd.md)
- [ADR-001: Two discrete Optional rules over a single combined rule](adr-001-two-discrete-rules.md)
- [ADR-002: Strict content preservation invariant](adr-002-content-preservation-invariant.md)
- [ADR-003: Discard font size from detection predicate](adr-003-discard-font-size-from-detection.md)
