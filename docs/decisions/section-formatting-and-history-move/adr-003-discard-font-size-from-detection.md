# ADR-003: Discard font size from section/sub-section detection predicate

## Status

Accepted

## Date

2026-05-07

## Context

The user-supplied specification for Phase 3 defines a section as a paragraph that is `bold + size 12 + left-aligned + ALL UPPERCASE`, and a sub-section as `bold + size 12 + left-aligned + mixed case`. The natural translation of "size 12" into OOXML is `<w:sz w:val="24"/>` (24 half-points = 12pt) on every text-bearing run.

Empirical inspection of three production articles in `examples/` (`1_AR_5449_2.docx`, `5_AR_5434_3.docx`, `7_CR_5136_3.docx`) reveals that the `<w:sz>` element is **absent from text runs in two of the three documents**. The visual size of 12pt is achieved by inheritance through the OOXML formatting cascade:

| Document | Source of "12pt" |
|---|---|
| `1_AR_5449_2` | Explicit `<w:sz w:val="24"/>` on each run and on `<w:pPr><w:rPr>` |
| `5_AR_5434_3` | Inherited from `<w:docDefaults><w:rPrDefault><w:rPr><w:sz w:val="24"/>` |
| `7_CR_5136_3` | Inherited from `<w:style w:styleId="Normal"><w:rPr><w:sz w:val="24"/>` (some sections also via `<w:pStyle w:val="sec">`) |

A predicate that filters by literal `sz="24"` on the run rejects sections in two-thirds of the corpus.

Two options exist: implement a full OOXML cascade resolver (run → paragraph defaults → applied paragraph style → linked character style → document Normal style → `docDefaults`) and use the resolved size; or remove size from the predicate entirely.

## Decision

Remove font size from the section/sub-section detection predicate. The predicate uses only:

1. Paragraph is **not** a descendant of `<w:tbl>` (excludes table cells).
2. Paragraph is **not** the same instance as `FormattingContext.SectionParagraph`, `TitleParagraph`, or `DoiParagraph` (excludes the rewritten header).
3. Concatenated trimmed paragraph text has length ≥ 3 and contains at least one letter.
4. Bold characters cover ≥ 90% of non-whitespace characters in the paragraph (tolerates cosmetic non-bold runs like trailing whitespace, tab markers, or asterisk annotations).
5. `<w:jc>` is `left`, `both`, or absent (justified is the dominant value in the corpus, not left).
6. For section: every letter is upper-case (`char.IsLetter && !char.IsLower`). For sub-section: at least one letter is lower-case.

The `INTRODUCTION` anchor (ADR-004) provides the upper bound on detection scope, eliminating false positives from header content (article-type label, article title) that would otherwise share the bold-caps signature.

## Alternatives Considered

### Alternative 1: Implement OOXML cascade resolver for `<w:sz>`

- **Description**: Walk run rPr → paragraph rPr → paragraph pStyle → linked character style → Normal style → docDefaults, in that order, returning the first explicit `<w:sz>` value found.
- **Pros**:
  - Stays faithful to the user's original spec.
  - Reusable for other size-dependent rules.
- **Cons**:
  - ~150 lines of code with multiple state branches.
  - Style chains can be `basedOn` another style, requiring recursive resolution.
  - The resolved size is rarely a useful discriminator: empirical predicate already has zero false positives without it.
- **Why rejected**: High implementation cost, low marginal value. The bold + caps + outside-table + below-anchor predicate is already 100% precise on the test corpus. The cost of a cascade resolver is justified only if a real size-based discrimination need emerges.

### Alternative 2: Filter by size only when explicitly set on the run

- **Description**: If `<w:sz>` is present on the run, require its value to be `24`. If absent, accept the paragraph.
- **Pros**:
  - Cheap to implement.
  - Catches the rare case of a body paragraph with `<w:sz w:val="32"/>` set explicitly that happens to also be bold and caps.
- **Cons**:
  - Inconsistent semantics: same visual outcome (12pt) succeeds or fails based on internal authoring history.
  - The "rare case" the rule would catch has not been observed in the corpus.
- **Why rejected**: Adds inconsistency without addressing a real failure mode.

## Consequences

### Positive

- Detection is robust across all three style cascades observed in the corpus.
- Implementation is simpler (no resolver, no style-chain traversal).
- The predicate can be tested purely from `<w:p>` content without needing access to the styles part.

### Negative

- The predicate cannot distinguish a 12pt bold caps paragraph from a 16pt bold caps paragraph. **Note**: this is acceptable because (a) the rule's job is to reformat both to 16pt, so the source size is irrelevant to the outcome; (b) the anchor scope confines detection to body content where 16pt headings are not authored manually.

### Risks

- **Risk**: A future input convention where the body has explicitly-sized 14pt bold caps paragraphs that should NOT be promoted (e.g., "FIGURE 1." labels). **Mitigation**: empirically not observed; the predicate would catch them as sections, but the reformat operation (set to 16pt center) is reversible by re-running with input as-authored. If the editor reports such a regression, this ADR can be superseded.

## Implementation Notes

- The 90% bold-character threshold is computed by counting non-whitespace characters per run and summing those whose run has `<w:b>` set (with `val` absent or not in `{"0", "false"}`).
- Whitespace-only runs are excluded from the denominator.
- `<w:b>` may also be inherited from a paragraph style or from `<w:pPr><w:rPr><w:b/>`. The predicate **only checks the run's own `<w:rPr>`** to keep the implementation bounded; if this proves insufficient on real data, this decision can be revisited via a follow-up ADR.

## References

- [PRD: Section Formatting and History Move](../_prd.md)
- [ADR-001: Two discrete Optional rules over a single combined rule](adr-001-two-discrete-rules.md)
- [ADR-004: INTRODUCTION as detection anchor](adr-004-introduction-as-detection-anchor.md)
- ECMA-376 §17.3.2 (Run Properties), §17.7 (Styles)
