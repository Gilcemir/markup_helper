# PRD: Header Formatting Polish (DocFormatter Phase 2)

## Overview

DocFormatter normalizes scientific articles submitted as `.docx` to the editorial format of an internal journal. The MVP (`header-metadata-extraction`) shipped with five rules that extract DOI, section, title, authors, and ELOCATION, and rewrite the header to the journal's output layout. After running on the eleven production articles in `examples/`, the editor still spends manual time on four normalization tasks the MVP intentionally deferred:

- **Alignment**: DOI, section, and title arrive in the body but are not aligned (DOI/section right, title centered).
- **Author block spacing**: a blank line between the author block and the affiliation block is inconsistent across articles.
- **Abstract format**: the input renders as `*Abstract - body text*` (italicized, single line, hyphen separator). The journal format is `**Abstract**` on its own bold line, body on the next paragraph in plain text (with intentional internal italics like scientific names preserved).
- **Corresponding-author contact**: when a paper marks a corresponding author with `*` in the affiliation block (e.g., `2 Universidade Y… * E-mail: foo@y.edu ORCID: https://orcid.org/0000-0002-1825-0097`), the `*…` trailer must be stripped from the affiliation, the email surfaced as a standalone line immediately above the abstract (`Corresponding author: foo@y.edu`), and the ORCID attached to the corresponding author when that author has not already been ORCID-linked from the authors line.

This PRD bundles the four behaviors into one phase. The editor's daily workflow gets a complete front-matter normalization without the manual finishing pass; the architecture stays aligned with the master pipeline plan in `instructions.md`.

## Goals

- **Eliminate manual front-matter touch-ups** on alignment, abstract framing, blank-line spacing, and corresponding-author email surfacing across the eleven articles in `examples/`.
- **Surface uncertainty** through the existing report (`*.report.txt`) and an extended diagnostic JSON (`*.diagnostic.json`) so the editor can scan a batch and find which papers need a manual look.
- **Preserve intentional emphasis** inside the abstract body (e.g., scientific names in italic) while stripping the structural italic wrapper.
- **Stay non-blocking**: every new behavior is `Optional` — a failure in any of the four rules logs `[WARN]` and the document is still produced.
- **Initial milestone**: re-run the same eleven articles end-to-end, with all four behaviors landing correctly without manual intervention on a sample agreed with the editor.

## User Stories

**Editor (primary persona — internal staff using the CLI today):**

- As an editor, I want the DOI, section, and title aligned automatically when I run the tool, so I do not have to click into each line and adjust the alignment myself.
- As an editor, I want the abstract paragraph rewritten as a bold "Abstract" line followed by a plain-text body, so I do not have to retype the heading and strip the italic from the body — but I want italics inside the body preserved (e.g., species names) so I do not lose the author's emphasis.
- As an editor, I want the corresponding author's email surfaced as its own line right above the abstract, so the contact information is in the journal's expected position without me copy-pasting it from the affiliation block.
- As an editor, when the corresponding author has only an ORCID URL in the affiliation (and no ORCID link in the authors line), I want that ORCID attached to that author automatically, so the rendered output already shows the ID next to the name.
- As an editor, I want the affiliation paragraph cleaned of the `* E-mail: … ORCID: …` trailer once the email/ORCID are extracted, so the affiliation block reads cleanly.
- As an editor, I want a blank line between the last author paragraph and the first affiliation paragraph, so the front matter has a consistent visual rhythm.
- As an editor, I want each new rule to run independently — a paper without a corresponding author should still be aligned and have its abstract reformatted, even if the corresponding-author rule has nothing to do.
- As an editor, I want the diagnostic JSON to flag papers where the new rules emitted a warning, so a batch summary tells me which files to open.

**Developer (secondary persona — single-developer maintainer):**

- As the developer, I want each new behavior to live in its own rule class, so failures localize and tests stay focused.
- As the developer, I want shared paragraph references to flow through `FormattingContext` (not relocated by each rule), so rules do not duplicate paragraph-finding logic.

## Core Features

### 1. Header alignment

The tool aligns three paragraphs in the rewritten output:

- **DOI line** → right-aligned. The DOI paragraph is the one created by `RewriteHeaderMvpRule` at the top of the body.
- **Section line** → right-aligned. The section paragraph is the original first non-empty paragraph after the (now-deleted) top table; `ParseHeaderLinesRule` already locates it as part of MVP extraction.
- **Title line** → centered. Same as section, identified positionally as the second non-empty paragraph.

Bilingual translated titles are explicitly out of scope: only the article's main title (`FormattingContext.ArticleTitle`'s paragraph) is centered. If the input already has the alignment applied, the rule still writes the property — the result is the same and no warning is logged.

If a target paragraph is missing from the context (e.g., extraction failed earlier in the pipeline), the rule logs a `[WARN]` for that specific line and continues with the others.

### 2. Author block spacing

The tool ensures exactly one blank paragraph between the last author paragraph and the first affiliation paragraph. The rule:

- Starts from the last paragraph of the (post-rewrite) author block.
- Walks forward to the next non-blank paragraph; that paragraph is treated as the first affiliation.
- Inserts a blank paragraph immediately before the first affiliation paragraph if the immediately preceding paragraph is not already blank.

The blank line above the authors block is already correct after `RewriteHeaderMvpRule` (it inserts an empty paragraph above the new author block); this rule only handles the line below the authors. The blank line above the abstract is out of scope: it is left as-authored.

### 3. Abstract paragraph rewrite

The current input is one paragraph in the form:

> *Abstract - lorem ipsum dolor sit amet, the entire body in italics.*

The output is two paragraphs:

> **Abstract**
>
> lorem ipsum dolor sit amet, the entire body in plain text. Internal italic emphasis (e.g., *Aedes aegypti*) is preserved.

The rule:

- Locates the abstract paragraph using the same markers as `LocateAbstractAndInsertElocationRule` (`Abstract`, `Resumo`, case-insensitive, on a leading non-whitespace run of the paragraph).
- Splits the paragraph into "heading" and "body" by removing the leading marker and the immediately following hyphen/colon separator.
- Replaces the original paragraph with two paragraphs:
  - **Heading paragraph**: a single run with the literal text `Abstract` (always English; `Resumo` is normalized to `Abstract`) in bold.
  - **Body paragraph**: the original body content, with the structural italic wrapper removed. Italic remains on individual runs whose italic property is **not** uniform across the entire body (i.e., italic that is genuinely localized emphasis stays).
- Does not touch the Keywords paragraph.

When the rule can detect that the entire body had italic applied to every non-whitespace run, it strips italic from all of them and emits an `[INFO]` ("structural italic wrapper removed from abstract body"). When italic is mixed (some runs italic, others not), it preserves the original run-level italic settings. This heuristic is documented as a known limitation: a pathological abstract whose author italicized 100% of the body for emphasis would be flattened.

### 4. Corresponding-author contact extraction and surfacing

The rule scans the affiliation block (paragraphs between the author block and the abstract) for a line containing the marker `* E-mail:` (case-insensitive, `*` may be the literal asterisk character or a superscript run containing `*`). When found:

- The full text from `*` to the end of the line is removed from the affiliation paragraph; everything before the `*` (the institutional affiliation text) is preserved as-is.
- The email address is extracted from the removed trailer.
- The ORCID URL/ID, when present in the trailer (`ORCID: https://orcid.org/...`), is also extracted.
- The corresponding author is identified as the author whose name is followed by a `*` — accepting both layouts: `*` inside the superscript run with affiliation labels (e.g., `1,2*`), and `*` in normal text right after the name. Only the **first** author marked with `*` is considered the corresponding author. Additional `*` markers log a `[WARN]` and are otherwise ignored.
- The extracted ORCID is attached to the corresponding author **only if** the author does not already have an ORCID from `ExtractAuthorsRule`. If the author already has an ORCID, the affiliation ORCID is dropped silently (no `[WARN]`); the existing extraction is treated as authoritative.
- The extracted email is stored in the context for the rewrite step (Feature 5 below) to insert.

If the affiliation block contains no `*` marker at all, the rule logs `[INFO]` ("no corresponding author marker found") and is a no-op for the rest of the pipeline. Single-author papers that omit the corresponding-author convention are valid input.

If `*` is present but the email regex cannot match (`* E-mail:` followed by garbled text), the rule logs `[WARN]` ("corresponding-author marker found but email could not be extracted"), still strips the `*…` trailer from the affiliation paragraph (best-effort cleanup), and does not insert the email line.

### 5. Corresponding-author email insertion

When the corresponding-author rule populated an email, a paragraph with the literal text:

```
Corresponding author: <email>
```

is inserted immediately before the rewritten **Abstract** heading paragraph (Feature 3). No blank lines are added or removed around it; the email line sits directly above the bold "Abstract" line. The paragraph uses the document's default font/size (consistent with other rewritten paragraphs in the MVP).

Before inserting, the rule scans the front matter (paragraphs between the author block and the abstract paragraph) for a **pre-existing "corresponding author" line** that the original author may have typed by hand. The matcher is permissive on purpose:

- Case-insensitive (`Corresponding Author`, `corresponding author`, `CORRESPONDING AUTHOR` all match).
- Tolerant of common misspellings and missing letters (`Coresponding`, `Correspondent`, `correspondign author`, etc.).
- Tolerant of localized variants (`Autor` for `Author`).
- Tolerant of the trailing separator (`:`, ` -`, ` —`, or none) and of optional whitespace around it.

The rule's behavior depends on whether `ExtractCorrespondingAuthorRule` populated an email:

- **Email available + pre-existing line found**: the pre-existing paragraph is removed and the canonical `Corresponding author: <email>` line takes its place. `[INFO]` ("replaced pre-existing corresponding-author line: '<original text>'") is logged.
- **Email available + no pre-existing line**: the canonical line is inserted immediately above the abstract.
- **Email NOT available + pre-existing line found**: the rule attempts to extract an email from the pre-existing line as a fallback (using the same `EmailRegex`). If an email is recovered, the pre-existing line is replaced with the canonical version and `[INFO]` is logged. If no email can be recovered, the pre-existing paragraph is **left in place untouched** — the rule does not destroy author-typed content.
- **Email NOT available + no pre-existing line**: no-op.

## User Experience

The CLI surface and report layout are unchanged. The editor's experience differs in:

1. **Re-running on the same articles in `examples/` produces a more complete output.** Running `docformatter examples/1_AR_5449_2.docx` writes `examples/formatted/1_AR_5449_2.docx` with DOI/section right-aligned, title centered, a blank line between the last author paragraph and the first affiliation, the abstract heading bolded, body de-italicized (with internal italics preserved), email line surfaced above the abstract, and the `* E-mail: …` trailer cleaned from the affiliation.
2. **Batch summary still shows ✓/⚠/✗.** A paper without a corresponding-author marker reports `[INFO]` (not `[WARN]`) and stays ✓. A paper where the email regex failed reports `[WARN]` and shows ⚠ in the summary.
3. **Diagnostic JSON gains four fields under a new `formatting` section.** They are populated only when the corresponding rule emitted `[WARN]`/`[ERROR]`:
   - `correspondingEmail`: the email actually extracted (when found), or `null` with `reason` when extraction failed despite the `*` marker.
   - `alignmentApplied`: object with `doi`, `section`, `title` booleans indicating which paragraphs were aligned (false implies the paragraph was missing from the context).
   - `abstractFormatted`: object with `headingRewritten`, `bodyDeitalicized`, `internalItalicPreserved` booleans.
   - `authorBlockSpacingApplied`: boolean — whether a blank line had to be inserted between the last author paragraph and the first affiliation paragraph (false means it was already there or the rule could not locate boundaries).

   Consumers that read only the legacy MVP keys keep working — the `formatting` section is additive.

## High-Level Technical Constraints

- The work runs entirely inside the existing `DocFormatter.Core` / `DocFormatter.Cli` projects; no new build target or runtime dependency.
- Each new rule must satisfy the existing pipeline contract (`IFormattingRule.Apply`). All four are `Optional`.
- The diagnostic JSON schema must remain backward-compatible (additive only).
- Web search was not consulted for this PRD — the formatting decisions are journal-internal editorial conventions captured by the user. The OOXML primitives needed (Justification, RunProperties.Bold/Italic, paragraph insertion) are already exercised in MVP rules.

## Non-Goals (Out of Scope)

- **Translated/bilingual titles**: only the main article title is centered. Translation paragraphs (when present) are left as input.
- **Keywords paragraph**: format normalization for `Keywords` is deferred. The MVP plan in `instructions.md` covers it as a separate rule.
- **Section style promotion**: bold-12pt → bold-16pt heading promotion (`PromoteSectionsRule` in the master plan) stays out of this PRD.
- **Multiple corresponding authors**: only the first `*`-marked author is supported. Additional markers warn and are ignored.
- **ORCID conflict resolution**: when the corresponding author already has an ORCID from the authors line, the affiliation ORCID is dropped silently. No conflict diagnostic is emitted.
- **Idempotency**: re-running the tool on already-formatted output is not supported by design; the existing `DetectInputFormatRule` aborts because the top table is gone. No new idempotency logic is added in any of the four rules.
- **Footnote / citation rules**: explicitly cut from the master plan; this PRD inherits that exclusion.
- **Affiliation parsing**: a structured `Affiliation` model and `ParseAffiliationsRule` are not introduced. Only the corresponding-author trailer cleanup is implemented.
- **GUI**: still CLI-only. Avalonia work is not in this PRD.

## Phased Rollout Plan

### MVP (Phase 1) — this PRD

All four behaviors ship together:

- `ApplyHeaderAlignmentRule` (Optional)
- `EnsureAuthorBlockSpacingRule` (Optional)
- `RewriteAbstractRule` (Optional, also performs corresponding-author email insertion)
- `ExtractCorrespondingAuthorRule` (Optional)
- `FormattingContext` extended with paragraph references and corresponding-author state.
- Diagnostic JSON extended with a `formatting` section.
- Tests: one xUnit file per rule, plus updated end-to-end fixture covering at least one production article from `examples/` with a `*` marker.

**Success criteria to proceed to Phase 2**: all eleven articles in `examples/` produce ✓ or ⚠ outputs that the editor accepts without manual editing of the four behaviors covered here, on at least 9/11 papers (the remaining 2 may have legitimate edge cases the editor reviews manually).

### Phase 2 (separate PRD)

- Keywords paragraph rewrite (`**Keywords**` line, comma-separated values).
- Section style promotion (`PromoteSectionsRule`).
- `ParseAffiliationsRule` for structured affiliation modeling.
- Quote indentation, hyperlink stripping, table label normalization (master plan rules 12–14).

### Phase 3 (separate PRD)

- Avalonia GUI surface for drag-and-drop.
- Multi-perfil configuration (different journals).

## Success Metrics

- **Manual edits per article on the four behaviors** drops to zero on 9/11 of the `examples/` corpus.
- **Wall-clock time per article** measured by the editor on a five-paper sample; aim for at least 30% reduction relative to current MVP-only output.
- **% of batch runs without `[WARN]` from the four new rules** on the production corpus reaches at least 80%; remaining 20% are surfaced in the diagnostic JSON.
- **Subjective editor acceptance**: editor reviews five articles after the rules ship and signs off without requesting structural changes to the rules themselves.

## Risks and Mitigations

- **Risk**: A paper italicizes 100% of the abstract body intentionally, and the rule strips that italic.
  - **Mitigation**: When the wrapper is removed, the rule emits `[INFO]` describing what was stripped so the editor can review on the rare occasion it matters. The PRD documents this as a known limitation rather than a heuristic to add.
- **Risk**: The corresponding-author marker `*` collides with a typed asterisk used by an author for unrelated purposes (e.g., a footnote in the affiliation text).
  - **Mitigation**: The rule requires the marker `* E-mail:` (asterisk followed by space-`E-mail:`) — the combined token is the trigger, not the asterisk alone. A bare `*` elsewhere in the affiliation is ignored.
- **Risk**: Editor adoption — the new behaviors change how output looks and might require workflow adjustments downstream (e.g., a copyeditor expecting the old layout).
  - **Mitigation**: The output format described here is the journal's canonical format already specified in `instructions.md` ("formato de saída"); downstream consumers are not surprised. Roll out by reprocessing the eleven articles and asking the editor to verify them before announcing.
- **Risk**: A paper has no corresponding-author marker but does have a single email tucked into one of the affiliations without `*` (informal convention).
  - **Mitigation**: Out of scope for this PRD; a `[WARN]` is not raised because there is no `*` to anchor on. Future iteration can extend the marker rule if the editor reports this is common.
- **Risk**: Schedule slip from underestimating the abstract italic-preservation logic.
  - **Mitigation**: Phase 1 carries an explicit fallback — if internal italic preservation proves harder than expected, ship with "strip all italic + `[WARN]`" and follow up. The user-visible feature still works; only the polish degrades.

## Architecture Decision Records

- [ADR-001: Four discrete Optional rules over a single consolidated rewrite](adrs/adr-001-four-discrete-rules.md) — Implement four sibling Optional rules in `DocFormatter.Core/Rules/` that share state via `FormattingContext`, instead of expanding `RewriteHeaderMvpRule`.

## Open Questions

- **Email regex shape**: the PRD describes "RFC 5322's pragmatic ASCII pattern". The TechSpec needs to pick the literal regex (likely `[\w._%+-]+@[\w.-]+\.[A-Za-z]{2,}`). Confirm with editor: are non-ASCII institutional emails ever expected? (Default: ASCII-only.)
- **Marker normalization for `Resumo`**: when a Portuguese paper uses `Resumo`, the rewritten heading is `Abstract` (English). Confirm: should the body language stay Portuguese? (Default: yes — only the heading is normalized; body stays in source language.)
- **Behavior when the affiliation paragraph contains the `*` trailer but the affiliation institutional text is empty after cleanup**: should the now-empty affiliation paragraph be removed, or kept as a blank paragraph? (Default: removed.)
- **ORCID URL canonicalization**: when the ORCID URL is `https://orcid.org/0000-0002-1825-0097/`, do we strip the trailing slash before parsing? (Default: yes — same regex as `ExtractAuthorsRule`.)
