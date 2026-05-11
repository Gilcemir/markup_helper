# Workflow Memory

Durable, cross-task context. No facts derivable from the repo/PRD/git.

## Current State

- Tasks 01–09 complete. Full Phase 2 release shipped. 565/565 tests green. `make phase2-verify` exits 0 with `[PASS]` on all 10 corpus pairs at the final cumulative scope `{accepted, authorid, corresp, doc, doctitle, doi, hist, histdate, kwdgrp, label, normaff, received, toctitle, xmlabstr, xref}`.
- Phase 1 `.docx` for `examples/phase-1/1_AR_5449_2.docx` produces SHA-256 `9c7be60ae57828228d73926303bf83ee515ecb1cfa434a2d74aafec8b1d76250` — unchanged by tasks 02–09 (none touch a Phase 1 rule; `FormattingContext` additions are nullable optional fields Phase 1 ignores).
- ADR-008 has a slot reserved for operator confirmation of Markup author auto-mark on 5313/5449 post Phase 1 fix.

## Shared Decisions

- ADR-008: `*` corresp markers are folded onto the trailing affiliation label in `AuthorBuilder.AddLabel`. `Author.AffiliationLabels` may now contain merged `"<aff><asterisks>"` entries (e.g. `"1*"`, `"2*"`).
- Phase 1 corpus snapshot for `examples/formatted/*.diagnostic.json` is the validation gate; `make run-all` + `diff` against a saved copy is the canonical check.
- `TagEmitter` is the single emission primitive for every Phase 2 rule. Public surface: `OpeningTag`, `ClosingTag`, `WrapParagraphContent`, `InsertOpeningBefore`, `InsertClosingAfter`. Anti-duplication tag names (`author`/`fname`/`surname`/`kwd`/`normaff`/`doctitle`/`doi`) are documented on the class but not enforced — rules choose their own tag names.
- **`Phase2DiffUtility.Compare` strip is symmetric and content-preserving.** Both sides go through `StripOutOfScope`; out-of-scope tag pairs have brackets/attributes removed but their (recursively stripped) content kept and trimmed at the edges. In-scope tag pairs kept verbatim with attributes. `NormalizeForCompare` trims spaces around newlines, collapses consecutive newlines, collapses runs of horizontal whitespace.
- **`Phase2Scope.Current` lists tags whose attributes/structure are STABLE between produced and expected at the current release point**, not the cumulative emission set. After task 07 it is `{authorid, corresp, doc, doctitle, doi, kwdgrp, label, normaff, toctitle, xmlabstr, xref}`. The sentinel test in `Phase2ScopeTests` pins exact contents. `author/fname/surname` stay OUT (Markup auto-marks them per ADR-001).
- **EmitElocationTagRule rewrites the existing `[doc]` opening tag in place.** Locates standalone `e\d+` paragraph, joins all `<w:t>` Texts in `[doc]`, regex-replaces `elocatid` and `issueno` (issueno derived from `e<article(4)><volid(2)><issueno(1)><order>` position 7; left untouched if format doesn't match), writes back, removes the standalone paragraph.
- **Corpus tag-name = `xmlabstr`, NOT `abstract`.** Same precedent for `[authorid]` using `authidtp="orcid"` (not `ctrbidtp`), `[author]` `corresp="y"|"n"` (not `yes`/`no`), and required `eqcontr="nd"`. The empirical AFTER-corpus UNION is authoritative over PRD prose.
- **EmitAuthorXrefsRule patches existing `[author role="nd"]…[/author]` literals in place** — does NOT emit new `[author]`/`[fname]`/`[surname]` (anti-duplication, ADR-001). Enriches opening attributes; expands `<digit>(,<digit>)*\*` corresp markers into structured xrefs (comma preserved between `[xref aff]` and `[xref corresp]`); wraps plain ORCIDs in `[authorid authidtp="orcid"]`; handles unicode superscript labels. Plain-text author paragraphs (no `[author]` shell) get the same transforms.
- **`HistDateParser` is a pure function** (no `FormattingContext`/`IReport`/no other Phase 2 module). Three entry points (`ParseReceived`/`ParseAccepted`/`ParsePublished`) each consume their own header — English (`Received`/`Accepted`/`Published`) and Portuguese (`Recebido em`/`Aceito em`/`Publicado em`) — followed by `:`, `on`, `em`, or whitespace; dispatches to a shared shape parser recognizing ISO `YYYY-MM-DD`, `<Day> <Month> <Year>` (English full + 3-letter abbrev), `<Month> <Day>[,] <Year>`, Portuguese `<Day> de <Mês> de <Year>`, and bare year. `HistDate.ToDateIso()` owns `YYYYMMDD` zero-padding (`00` for missing month/day) — call it; do NOT re-implement.
- **EmitHistTagRule rewrites each history paragraph's inline content in place.** Three Phase-1 paragraphs (`Received: <date>`, `Accepted: <date>`, `Published: <date>`) stay as three paragraphs; `[hist]` opens at start of first emitted child paragraph; each paragraph wraps its own date phrase in `[received dateiso="…"]…[/received]` (or `[accepted …]` / `[histdate dateiso="…" datetype="pub"]`); `[/hist]` closes at end of last emitted paragraph. Label words (`Received: `, `Accepted: `, `Published: `) preserved verbatim BEFORE the child opening tag. Reason codes: `hist_received_missing` / `hist_received_unparseable` skip the entire block; `hist_accepted_unparseable` / `hist_published_unparseable` warn but still emit `[hist]` with the children that did parse. Locator regexes are English-only (`Received`/`Accepted`/`Published`) — mirrors Phase 1 `MoveHistoryRule`. Idempotency: any pre-existing `[hist` / `[received` / `[accepted` / `[histdate` literal aborts the rule.

## Shared Learnings

- The `1,*` / `1,2,*` failure shape (corresp asterisk in its own superscript run, comma-joined into the produced label) is present in 8 of 11 corpus articles.
- `dotnet test` does not collect coverage in this repo (no `coverlet.collector`). 80% coverage targets are qualitative until added.
- Corpus refinements committed at tasks 06–07 (durable per ADR-003): 5313 P8–P14 + 5424 P15 BEFORE-side missing-space artifacts patched; 5293 BEFORE corresp trailing space trimmed; 5523 BEFORE broken `[fname]`/`[surname]` wrappers stripped; 5313 AFTER ORCIDs of authors 4–6 wrapped in `[authorid authidtp="orcid"]`.

## Open Risks

- `examples/phase-2/before/<id>.docx` is NOT clean source — it already contains Stage-1 SciELO bracket markup. With the symmetric+content-keep strip, only tags whose attributes are stable need to be in `Phase2Scope.Current`.

## Handoffs

- For the next feature (post-promotion): Phase 2 pipeline runs in isolation (`AddPhase2Rules()`); does NOT chain Phase 1 rules. Each Phase 2 emitter must extract whatever it needs from the body. The same body-parse-then-populate pattern is shared across all six emitters (`EmitElocationTagRule`, `EmitAbstractTagRule`, `EmitKwdgrpTagRule`, `EmitAuthorXrefsRule`, `EmitCorrespTagRule`, `EmitHistTagRule`).
- Final corpus AFTER tag union (task 05 empirical probe, still authoritative): `accepted, author, authorid, corresp, doc, doctitle, doi, email, fname, hist, histdate, kwd, kwdgrp, label, normaff, p, received, sectitle, suffix, surname, toctitle, xmlabstr, xref`. `revised` is NOT in the union — no corpus pair currently exercises a revised paragraph. Strict-ordering DTD requirement (`revised*` between `received` and `accepted?`) is encoded in the rule but has no corpus coverage.
- All corpus amendments at tasks 06/07 (5313 / 5424 BEFORE author paragraphs; 5293 BEFORE corresp trailing space; 5523 BEFORE broken `[fname]`/`[surname]` wrappers; 5313 AFTER ORCIDs for authors 4–6) are durable per ADR-003 and stay in place.
- `HistoryDates.Revised` is reserved as `IReadOnlyList<HistDate>` for future articles that carry a Revised paragraph. The current `EmitHistTagRule` locator does not match a Revised paragraph; if/when one appears in the corpus, add a `RevisedMarker` regex and a per-paragraph parser entry point and the strict-ordering emission loop will handle it transparently.
