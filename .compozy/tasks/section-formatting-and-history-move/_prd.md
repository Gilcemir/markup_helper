# PRD: Section Formatting and History Move (DocFormatter Phase 3)

## Overview

DocFormatter normalizes scientific articles submitted as `.docx` to the editorial format of an internal journal. Phase 1 (`header-metadata-extraction`) extracted DOI, section, title, authors, ELOCATION, and rewrote the front-matter header. Phase 2 (`header-formatting-polish`) added header alignment, author block spacing, abstract reformatting, and corresponding-author email surfacing. After running Phase 1+2 on the eleven production articles in `examples/`, two manual finishing tasks remain in the editor's daily workflow:

- **Article history block placement.** Each article carries three consecutive paragraphs `Received: …`, `Accepted: …`, `Published: …` immediately after the affiliations. The journal's editorial format requires this block to appear immediately above the `INTRODUCTION` section, separated from `Keywords` by the abstract block. Editors currently cut and paste the three lines manually.
- **Section and sub-section visual promotion.** Authors submit articles with section headings (`INTRODUCTION`, `MATERIAL AND METHODS`, `RESULTS AND DISCUSSION`, `REFERENCES`, …) as bold all-caps paragraphs at body font size, left-aligned or justified. Sub-sections (`Plant sampling, DNA extraction, and sequencing`, `GMDA software features`, …) appear as bold mixed-case paragraphs at the same size and alignment. The journal's editorial format requires sections at 16pt bold centered and sub-sections at 14pt bold centered. Editors currently click each heading and adjust manually.

This PRD bundles both behaviours into Phase 3. The editor's daily workflow gets a complete body-level normalization on top of the existing front-matter normalization. The architecture stays aligned with the master pipeline plan in `instructions.md` and with the one-rule-per-responsibility convention established in Phase 1+2.

## Goals

- **Eliminate manual cut-paste and click-formatting** for article history placement and section/sub-section visual promotion across the eleven articles in `examples/`.
- **Guarantee strict content preservation** (INV-01, ADR-002): Phase 3 may reorder three specific paragraphs and mutate alignment / font size on existing paragraphs, but must never delete or hide text. A non-formatted output is preferred over an output with content loss.
- **Stay non-blocking**: both new behaviours are `Optional` — a failure in either rule logs `[WARN]`, the document is still produced.
- **Keep diagnostics actionable**: the diagnostic JSON gains two new objects (`formatting.history_move`, `formatting.section_promotion`) so a batch summary tells the editor which files need a manual look without opening them.
- **Initial milestone**: re-run the same eleven articles end-to-end with both behaviours landing correctly without manual intervention on at least 9 of 11 papers; remaining papers either skip with `[WARN]` and a clear reason, or surface a legitimate edge case the editor reviews manually.

## User Stories

**Editor (primary persona — internal staff using the CLI today):**

- As an editor, I want the article history block (`Received` / `Accepted` / `Published`) moved to immediately above `INTRODUCTION` automatically, so I do not cut and paste those three lines for every paper.
- As an editor, I want every section heading (`INTRODUCTION`, `REFERENCES`, all uppercase headings) to come out at 16pt bold centered, so I do not click each one to reformat it.
- As an editor, I want every sub-section (mixed-case bold heading inside the body) to come out at 14pt bold centered, with the same convention applied uniformly across the document.
- As an editor, I want the article-type label at the top (`ARTICLE`, `CULTIVAR RELEASE`, …) and the article title to keep the formatting Phase 2 already applied — they must NOT be reformatted as sections, even if they happen to look bold and uppercase.
- As an editor, when an article lacks `INTRODUCTION` or has an irregular history block, I want the tool to leave that paper alone and emit a warning, so I know to inspect it manually rather than receive a corrupted output.
- As an editor, I want the diagnostic JSON to record exactly what Phase 3 did and why it skipped any rule, so my batch summary tells me which files to re-open.
- As an editor, I want absolute confidence that no text disappears from any document. If the tool is unsure how to format something, it must skip the formatting and preserve the content as-authored.

**Developer (secondary persona — single-developer maintainer):**

- As the developer, I want each new behaviour in its own rule class so failures localize and tests stay focused.
- As the developer, I want the detection predicates documented as ADRs so future contributors understand why size is not used in detection (ADR-003) and why `INTRODUCTION` anchors the scope (ADR-004).
- As the developer, I want the strict content preservation invariant (INV-01, ADR-002) testable at unit level — every Phase 3 rule fixture asserts that the multiset of non-empty trimmed body texts is preserved.

## Core Features

### 1. Article history block move

The tool relocates the three consecutive paragraphs `Received: …`, `Accepted: …`, `Published: …` to sit immediately above the `INTRODUCTION` paragraph, preserving each paragraph's text and run-level formatting.

**Detection of the history block**:

- Each marker is matched by a case-insensitive regex `^(received|accepted|published)\s*[:\-]\s*.+` on the trimmed paragraph text.
- The three matching paragraphs must be **adjacent** in body order (no non-empty paragraph between them).
- The three must appear in the fixed order `Received → Accepted → Published`.
- Only the first qualifying block in body order is considered. Markers occurring after the `INTRODUCTION` anchor are ignored (they are body content, not the editorial history block).

**Detection of the destination anchor (`INTRODUCTION`)**:

- The first paragraph whose trimmed text matches `^INTRODUCTION[\s.:]*$` AND that satisfies the section predicate (see Feature 2 below).
- Anchor matching is case-sensitive on the literal `INTRODUCTION`.

**Move operation**:

- The three history paragraphs are detached from their current position and inserted as the immediate predecessors of the `INTRODUCTION` paragraph, in their original order.
- No paragraph properties (`pPr`, `spacing`, `pageBreakBefore`, custom `pStyle`) are mutated. The Word renderer reflows pagination naturally.
- If the three history paragraphs are already immediately before the `INTRODUCTION` paragraph, the rule emits `[INFO] history already adjacent to INTRODUCTION — no-op` and returns without mutating the document. This makes the rule trivially idempotent on re-runs.

**Skip conditions** (the rule does NOT move and emits a `[WARN]` with a reason code):

- `anchor_missing`: no paragraph matches the `INTRODUCTION` regex and section predicate.
- `partial_block`: only one or two of `Received` / `Accepted` / `Published` were found.
- `out_of_order`: the three markers appeared in a different order than `Received → Accepted → Published`.
- `not_adjacent`: a non-empty paragraph appears between the three markers.

**Skip silently** (`[INFO]`, no warning):

- `not_found`: no `Received:` marker exists in the document at all. The article may legitimately lack a history block.

### 2. Section and sub-section promotion

The tool reformats every section and sub-section paragraph in the body of the document.

**Detection scope**:

- Detection begins at the `INTRODUCTION` anchor (inclusive) and continues through the end of the body.
- Paragraphs that are descendants of `<w:tbl>` are skipped (table cells are never sections).
- Paragraphs whose object identity equals `FormattingContext.SectionParagraph`, `FormattingContext.TitleParagraph`, or `FormattingContext.DoiParagraph` are skipped (defence in depth on top of the anchor scope).

**Section predicate** (paragraph P is a section if):

1. P is in the detection scope (above).
2. Concatenated trimmed text of P has length ≥ 3 and contains at least one letter.
3. ≥ 90% of non-whitespace characters in P are inside runs whose `<w:rPr><w:b/>` is set with `val` absent or not in `{"0", "false"}`.
4. Every letter (`char.IsLetter`) in the trimmed text is upper-case (`!char.IsLower`).
5. P's `<w:jc>` value is `left`, `both`, or absent.

**Sub-section predicate** (paragraph P is a sub-section if):

- All of the above except (4): the trimmed text contains at least one lower-case letter (`char.IsLower`).

**Formatting applied**:

- Section: paragraph `<w:jc>` set to `center`; every text-bearing run's `<w:rPr><w:sz>` set to `32` (16pt half-points). Bold is preserved as-is (already true for the predicate to match).
- Sub-section: paragraph `<w:jc>` set to `center`; every text-bearing run's `<w:rPr><w:sz>` set to `28` (14pt half-points). Bold is preserved as-is.

**Skip conditions** (`[WARN]` with reason):

- `anchor_missing`: no `INTRODUCTION` anchor found. The entire rule is a no-op.

**No-op silently**:

- A document with `INTRODUCTION` but no other sections or sub-sections (e.g., a very short article body) emits an `[INFO]` summary with `sections_promoted=1, subsections_promoted=0`.

### 3. Diagnostic JSON extension

The existing `formatting` object inside the diagnostic JSON gains two new keys, populated whenever the corresponding rule ran:

```json
"formatting": {
  "header_alignment":   { ... },
  "abstract_rewrite":   { ... },
  "author_block_spacing": { ... },
  "corresponding_author": { ... },
  "history_move": {
    "applied": true,
    "skipped_reason": null,
    "anchor_found": true,
    "from_index": 9,
    "to_index_before_intro": 13,
    "paragraphs_moved": 3
  },
  "section_promotion": {
    "applied": true,
    "skipped_reason": null,
    "anchor_found": true,
    "anchor_paragraph_index": 14,
    "sections_promoted": 7,
    "subsections_promoted": 3,
    "skipped_paragraphs_inside_tables": 18,
    "skipped_paragraphs_before_anchor": 2
  }
}
```

`skipped_reason` ∈ `{"anchor_missing", "partial_block", "out_of_order", "not_adjacent", "not_found", null}`. The two existing diagnostic consumers (the report renderer and the batch summary) already iterate over `formatting.*` keys, so the additive change requires no change in those consumers.

### 4. Report messages

`MoveHistoryRule` emits one of:

| Level | Message |
|---|---|
| `[INFO]` | `history moved (3 paragraphs placed before INTRODUCTION at position {N})` |
| `[INFO]` | `history already adjacent to INTRODUCTION — no-op` |
| `[INFO]` | `history block not found — nothing to move` |
| `[WARN]` | `INTRODUCTION anchor not found — history move skipped` |
| `[WARN]` | `history partial: Received={r} Accepted={a} Published={p} — not moved` |
| `[WARN]` | `history out of order ({order}) — not moved` |
| `[WARN]` | `history not adjacent (gap of {N} non-empty paragraphs between markers) — not moved` |

`PromoteSectionsRule` emits one of:

| Level | Message |
|---|---|
| `[INFO]` | `INTRODUCTION anchor at body position {P}` |
| `[INFO]` | `promoted {N} sections (16pt center) and {M} sub-sections (14pt center)` |
| `[WARN]` | `INTRODUCTION anchor not found — section formatting skipped` |

## User Experience

The CLI surface and report layout are unchanged. The editor's experience differs in:

1. **Re-running on the same articles in `examples/` produces a more complete output.** Running `docformatter examples/1_AR_5449_2.docx` writes `examples/formatted/1_AR_5449_2.docx` with the article history block moved to immediately above `INTRODUCTION`, all uppercase section headings centered at 16pt bold, and all mixed-case sub-section headings centered at 14pt bold. Phase 1+2 outputs (header alignment, abstract reformat, corresponding-author insertion, etc.) are preserved.
2. **Batch summary still shows ✓ / ⚠ / ✗.** A paper without a history block reports `[INFO]` (not `[WARN]`) and stays ✓. A paper without an `INTRODUCTION` anchor reports `[WARN]` for both rules and shows ⚠ in the summary.
3. **Diagnostic JSON gains two additive keys** under the existing `formatting` section. Consumers that read only Phase 1 or Phase 2 keys keep working.

## High-Level Technical Constraints

- The work runs entirely inside the existing `DocFormatter.Core` / `DocFormatter.Cli` projects; no new build target or runtime dependency.
- Each new rule must satisfy the existing `IFormattingRule.Apply` contract. Both are `Optional`.
- The diagnostic JSON schema must remain backward-compatible (additive only).
- The OOXML primitives needed (paragraph reorder, `<w:jc>`, `<w:sz>` mutation, table-descendant detection) are already exercised in Phase 1+2.
- INV-01 (ADR-002) binds both rules: deletion of paragraphs, runs, or text nodes is forbidden; reordering is restricted to `MoveHistoryRule` and only for the three history paragraphs.

## Non-Goals (Out of Scope)

- **Multi-language anchor.** Detection of the `INTRODUCTION` anchor is English-only and case-sensitive on the literal string. Portuguese (`INTRODUÇÃO`), Spanish (`INTRODUCCIÓN`), and other variants are out of scope. The journal publishes in English; multi-language support stays open for a future phase.
- **Sub-minor sections (13pt).** The original master plan in `instructions.md` allowed three heading levels. Phase 3 promotes only two (16pt and 14pt). Sub-minor headings, if any, are left as-authored.
- **Figure and table caption formatting.** `FIGURE 1.`, `TABLE 1.`, and similar captions are not detected and not reformatted by Phase 3. They naturally do not match the predicate (not full-paragraph bold-caps).
- **Header content reformatting.** The article-type label (`ARTICLE`, `CULTIVAR RELEASE`, …) and article title keep the formatting Phase 2 applied. Both the anchor scope (ADR-004) and the context skip-list ensure they are not reformatted.
- **Typo correction in section headings.** A document with `ACKNOWLEDMENTS` (missing `G`, observed in `5_AR_5434_3.docx`) is treated as a legitimate section and reformatted as such. Phase 3 does NOT correct the typo.
- **Spacing normalization around the moved history block.** No blank paragraphs are inserted, removed, or normalized between Keywords / history / `INTRODUCTION`. The Word renderer reflows naturally.
- **Reformatting bibliography entries after `REFERENCES`.** Bibliography entries are not bold-caps and do not match the predicate; no special handling.
- **Mutation of `<w:b>`, `<w:i>`, run colour, run language, or any other property** beyond `<w:jc>` and `<w:sz>`. Phase 3 is intentionally minimal in its mutation surface (INV-01).
- **Idempotency via runtime checks.** Both rules are naturally idempotent (`MoveHistoryRule` early-returns on adjacency; `PromoteSectionsRule` re-applies the same `<w:jc>`/`<w:sz>`). No runtime guard is added because `DetectInputFormatRule` (when implemented) already aborts re-runs on already-formatted input.
- **GUI.** Still CLI-only. Avalonia work stays out of this PRD.

## Phased Rollout Plan

### MVP (Phase 1) — this PRD

Both behaviours ship together:

- `MoveHistoryRule` (Optional, pipeline position #10).
- `PromoteSectionsRule` (Optional, pipeline position #11).
- Diagnostic JSON extended with `formatting.history_move` and `formatting.section_promotion`.
- Tests: one xUnit fixture per rule covering happy path and each documented skip condition; one end-to-end fixture re-running an article from `examples/` through the entire pipeline and comparing against a golden output.
- INV-01 unit assertion in both rule fixtures: multiset of non-empty trimmed body texts is preserved before and after the rule runs.

**Success criteria to proceed to Phase 2**: at least 9 of 11 articles in `examples/` produce ✓ outputs with both rules landing without manual editing; the remaining articles either ⚠ with a clear `skipped_reason` that the editor accepts as a legitimate edge case, or ✓ with the editor verifying no manual touch-up is needed on the two new behaviours.

### Phase 2 (separate PRD)

- Multi-language anchor support (`INTRODUÇÃO`, `INTRODUCCIÓN`, …) when first non-English submission appears.
- `DetectInputFormatRule` (Critical, pipeline position #1) for re-run protection.
- Quote indentation, hyperlink stripping, table label normalization (master plan rules 12–14).
- Keywords paragraph rewrite if not already covered.

### Phase 3 (separate PRD)

- Avalonia GUI surface for drag-and-drop.
- Multi-profile configuration (different journals).

## Success Metrics

- **Manual edits per article on the two new behaviours** drops to zero on at least 9 of 11 articles in the `examples/` corpus.
- **Wall-clock time per article** measured by the editor on a five-paper sample after Phase 3 ships; aim for an additional 20% reduction relative to Phase 2-only output.
- **% of batch runs without `[WARN]` from the two new rules** on the production corpus reaches at least 80%; the remaining 20% are surfaced in the diagnostic JSON with a precise `skipped_reason`.
- **INV-01 verification**: 0 cases of content loss in any test run. The multiset assertion passes on all 11 articles end-to-end.
- **Subjective editor acceptance**: editor reviews five articles after the rules ship and signs off without requesting structural changes to the rules themselves.

## Risks and Mitigations

- **Risk**: A document genuinely lacks the `INTRODUCTION` anchor (rare in this journal but possible for a non-research article type).
  - **Mitigation**: Both rules emit `[WARN] anchor_missing` and skip their work. The output matches the input on Phase 3 behaviours. The editor sees ⚠ in the batch summary and inspects manually. INV-01 ensures no content was harmed.
- **Risk**: A paper has the history block split into more than three paragraphs (e.g., a `Revised:` line is interleaved).
  - **Mitigation**: `MoveHistoryRule` requires exactly three matching paragraphs in the order `Received → Accepted → Published`; any deviation triggers `partial_block`, `out_of_order`, or `not_adjacent` warnings and the rule does nothing. The editor handles the unusual paper manually.
- **Risk**: A section heading uses a less-common variant (e.g., `INTRODUCTION (Background)`).
  - **Mitigation**: The anchor regex is tight by design (`^INTRODUCTION[\s.:]*$`). The variant fails the regex; `[WARN] anchor_missing` fires; the editor inspects. If this variant becomes common, the regex can be relaxed in a follow-up.
- **Risk**: A future Phase 4 introduces a rule that adds bold-caps content after `INTRODUCTION` in the body, which would then be reformatted as a section.
  - **Mitigation**: ADR-001 and ADR-002 are referenced from any future ADR proposing such a rule. Pipeline order can be adjusted: a content-adding rule that should not be reformatted runs after Phase 3.
- **Risk**: An author submits a paper where the INTRODUCTION word is in body text in caps as part of a sentence, not as a heading.
  - **Mitigation**: The anchor predicate requires exact text match (`^INTRODUCTION[\s.:]*$`) AND the paragraph to be bold-caps. A bold-caps standalone paragraph reading `INTRODUCTION` is by editorial convention always the intro heading. A bold-caps body paragraph that happens to read `INTRODUCTION` followed by sentence text fails the anchor regex.
- **Risk**: The editor expects the same look-and-feel as Phase 2 outputs and may be surprised by the visual change in body sections.
  - **Mitigation**: The output format described in this PRD is the journal's canonical format (per `instructions.md` "formato de saída"). Roll out by reprocessing the eleven articles and reviewing them with the editor before announcing.

## Architecture Decision Records

- [ADR-001: Two discrete Optional rules over a single combined rule](adrs/adr-001-two-discrete-rules.md) — Implement `MoveHistoryRule` and `PromoteSectionsRule` as separate `IFormattingRule` siblings instead of one combined rule.
- [ADR-002: Strict content preservation invariant (INV-01)](adrs/adr-002-content-preservation-invariant.md) — Bind Phase 3 to a falha-segura discipline: deletion is forbidden; reordering is confined to the three history paragraphs; in any doubt, the rule does nothing.
- [ADR-003: Discard font size from detection predicate](adrs/adr-003-discard-font-size-from-detection.md) — The `<w:sz>` element is absent on most paragraphs in the corpus due to OOXML cascade; detection uses bold + caps + alignment + scope only, not size.
- [ADR-004: `INTRODUCTION` as detection anchor](adrs/adr-004-introduction-as-detection-anchor.md) — The first paragraph matching `^INTRODUCTION[\s.:]*$` and the section predicate is the positional anchor for both rules; everything before it is exempt from Phase 3 detection.

## Open Questions

- **`<w:b>` inheritance via paragraph or character style.** ADR-003 specifies that the bold check inspects only the run's own `<w:rPr>`. If a section heading inherits bold via `<w:pPr><w:rPr><w:b/>` or via a `<w:pStyle>` reference (instead of repeating `<w:b/>` on each run), the predicate may miss it. Empirically not observed in the three audited articles; needs verification across the full eleven-article corpus during the TechSpec phase. If a counter-example is found, ADR-003 may need to be supplemented with limited paragraph-level bold inheritance.
- **`pStyle="sec"` cascade in `7_CR_5136_3`.** Some sections in `7_CR_5136_3.docx` use `<w:pStyle w:val="sec">` to derive their bold formatting. The current predicate inspects runs' direct `<w:rPr><w:b/>`. Whether the `sec` style still places `<w:b/>` directly on each run, or only on the style definition, must be confirmed during TechSpec by inspecting the styles part of that document.
- **Idempotency under future `DetectInputFormatRule`.** Re-running the pipeline on a Phase 3-output is currently safe by accident (top table is gone in Phase 1, `MoveHistoryRule` early-returns on adjacency, `PromoteSectionsRule` re-applies same values). If `DetectInputFormatRule` is implemented per the master plan, this concern goes away. If not, the natural idempotency is sufficient but should be documented as a property, not a guarantee.
- **REFERENCES alternative spelling**. The user's spec mentions `REFERENCE` (singular) as a valid alternative for the last section. The detection predicate handles both naturally (any caps-bold paragraph is a section). No special-case logic needed; this open question is to confirm with the editor that no other singular/plural alternates exist for `ACKNOWLEDGMENTS`, `CREDIT STATEMENT`, etc.
