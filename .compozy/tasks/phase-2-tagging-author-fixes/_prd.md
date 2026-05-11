# PRD: Phase-2 Tagging and Stage-1 Author Fixes

> **Naming note**: SciELO's XML production has three workflow **stages** (Stage 1 = pre-format `.docx`; Stage 2 = SciELO Markup auto/manual tagging in Word; Stage 3 = `parser.exe`/`convert.exe` → JATS XML). DocFormatter today implements Stage 1. This PRD adds DocFormatter capabilities that span Stage 1 and Stage 2. To avoid clashing with that vocabulary, this document calls its rollout slices **Phase 1 / Phase 2 / Phase 3 / Phase 4**. "Stage" always refers to SciELO; "Phase" always refers to this PRD's release sequence.

## Overview

DocFormatter is a CLI that pre-formats `.docx` files before the SciELO Markup Word plugin tags them, reducing manual rework on the path to JATS XML. Two unmet needs motivate this feature:

- **Stage 1 author handoff failures**: For some articles (concretely 5313 and 5449), the SciELO Markup plugin's `mark_authors` macro fails to auto-tag part or all of the author block in the `.docx` produced by DocFormatter Stage 1, even though DocFormatter itself extracted the authors with `confidence=high`. The operator must then re-mark every author manually.
- **Stage 2 tags Markup does not handle**: Several DTD 4.0 tags (`elocation`, author `xref`/`authorid`/attributes, `[corresp]`, `[abstract]`, `[kwdgrp]`/`[kwd]`, `[hist]` with `received`/`accepted`/`histdate datetype="pub"`) are not auto-marked by Markup or are marked incompletely. They are required by the DTD and currently demand manual intervention or fail downstream validation.

The user is the article preparer (currently a single operator) responsible for shipping JATS XML to SciELO. The value is direct time savings per article and fewer downstream parser validation failures.

## Goals

- **G1 — Eliminate manual author re-marking on the two known regression articles.** Articles 5313 and 5449 must pass through SciELO Markup's `mark_authors` without the operator having to re-mark any author.
- **G2 — Pre-mark the six target Stage-2 tag groups** (`elocation`, `[kwdgrp]`/`[kwd]`, `[abstract]`, `[corresp]` + author-block xrefs/ORCID/attributes, `[hist]`) such that DocFormatter's output on each `examples/phase-2/before/<id>.docx` matches `examples/phase-2/after/<id>.docx` within the scope of each release.
- **G3 — Preserve all anti-duplication invariants** documented in `docs/scielo_context/REENTRANCE.md`. No new rule pre-marks `[author]`, `[fname]`, `[surname]`, `[kwd]`, `[normaff]`, or any tag that Markup auto-marks without prior-existence checks.
- **G4 — Ship incremental value.** Each release closes a measurable gap; failure of a later release does not invalidate earlier ones.

## User Stories

**Primary persona — article preparer (the user)**

- As an article preparer, I want SciELO Markup to auto-tag every author in my `.docx` without me having to re-mark any of them, so I can move directly to validating the corresponding-author block.
- As an article preparer, I want DocFormatter to pre-mark the `[abstract]`, `[kwdgrp]`, and `[elocation]` tags in their final form, so I do not have to manually wrap each section before running Markup.
- As an article preparer, I want DocFormatter to fill in the `xref ref-type="aff"`, `xref ref-type="corresp"`, ORCID `[authorid]`, and the `rid`/`corresp`/`deceased="n"` attributes on each author, so Markup's manual author-cleanup step becomes a verification step instead of a data-entry step.
- As an article preparer, I want DocFormatter to emit `[hist][received].../[accepted].../[histdate datetype="pub"].../[/hist]` populated from the dates already in the document, so I do not have to look them up and tag them by hand.
- As an article preparer, when DocFormatter cannot find what a rule needs, I want a clear note in `diagnostic.json` telling me what was missing, so I know exactly where to intervene manually.

## Core Features

Grouped by release. Each release ships independently; later releases assume earlier ones. All features apply only to the Phase 2 pipeline; the existing Stage 1 pipeline continues to run unchanged unless modified by Phase 1 (the author fix).

### Phase 1 — Stage 1 author handoff fix (MVP)

- **Investigate the Markup `mark_authors` failure on articles 5313 and 5449**: identify the difference between the Stage-1 output for these articles and the Stage-1 output of articles where Markup succeeds. Document the root cause in an ADR.
- **Adjust the existing Stage-1 author-handling rule** (or add a new rule) so the produced `.docx` no longer triggers the failure.
- **Validate**: re-running Markup on the post-fix output of 5313 and 5449 must auto-mark every author with no manual re-marking required.

### Phase 2 — Easy Stage-2 tags

- **`[elocation]` finalization**: emit the article's elocation identifier in the form expected by the `after/` corpus.
- **`[abstract]` section**: wrap the abstract in `[abstract xmlabstr="..." language="en"]…[/abstract]`. `language="en"` is hard-coded for now (the corpus is English-only).
- **`[kwdgrp]` / `[kwd]`**: wrap the keywords block in `[kwdgrp language="en"]…[/kwdgrp]` and split the keywords by `,` or `;` separators. Per ADR-001, individual `[kwd]` items remain unmarked because Markup auto-marks them.

### Phase 3 — Author-block xrefs and corresponding-author tagging

- **`[corresp id="c1"]…[/corresp]`**: identify and wrap the corresponding-author block.
- **`xref ref-type="aff"`** for each author: emit one `xref` per affiliation label the author currently bears.
- **`xref ref-type="corresp" rid="c1"`** for the corresponding author.
- **`[authorid ctrbidtp="orcid"]`** for each author who has an ORCID. Currently extracted via hyperlink in `ExtractAuthorsRule`; this release surfaces it in the marked-up output.
- **Author attributes**: `rid` (linked affiliation IDs), `corresp` (boolean for the corresponding author), `deceased="n"` (default).
- **Anti-duplication constraint**: the `[author]`, `[fname]`, `[surname]` tags themselves are NOT pre-marked. Markup auto-marks them and would duplicate.

### Phase 4 — `[hist]` with date parsing

- **`[hist]` block** with the strict ordering required by the DTD: `received` (required, first), `revised*` (zero or more), `accepted?` (optional, last), plus `[histdate datetype="pub"]` for publication date.
- **Date detection**: parse "Received on…", "Accepted on…", "Published on…" patterns from the document. The detection logic is ported from the user's existing C# project at https://github.com/Gilcemir/Marcador_de_referencia/blob/master/BibliographyHandlers/AccessedOnHandler.cs.
- **`dateiso` format**: `YYYYMMDD` zero-padded when month or day is missing (per `docs/scielo_context/README.md` invariant 5).
- **Skip-and-warn applies**: when a date cannot be detected with confidence, the `[hist]` block is omitted entirely, with a structured warning in `diagnostic.json`.

## User Experience

The user is one person running the CLI on a set of `.docx` files. The relevant journey is per-article:

1. The user runs `docformatter phase2 input.docx` (or equivalent — exact CLI shape deferred to TechSpec) on a `.docx` that has already been through Phase 1 / Stage 1.
2. DocFormatter writes `<input>.docx` (with new tags pre-marked), `<input>.report.txt`, and `<input>.diagnostic.json` to a phase-2 output directory (kept distinct from Stage 1's `examples/formatted/` to avoid mixing).
3. The user opens the resulting `.docx` in Word with the SciELO Markup plugin loaded.
4. Markup completes the remaining auto-tagging (`[author]`, `[fname]`, `[surname]`, `[kwd]`, refs, etc.). With this PRD's features delivered, the operator's manual tagging effort drops to verification of the pre-marked Phase 2 sections.
5. The user runs `parser.exe`/`convert.exe` to produce the final JATS XML.

For diagnostics: when a Phase 2 rule cannot mark its target, `diagnostic.json` contains an entry under `issues` with the rule name, a machine-readable reason code, and a human-readable message. The user reads this file when the produced `.docx` looks unexpectedly thin in the Phase 2 sections.

There is no UI beyond the CLI and the resulting files. The user is technical and reads JSON.

## High-Level Technical Constraints

These are non-negotiable boundaries for any implementation:

- **Output format**: `[tag attr="value"]…[/tag]` literals injected into the Word document text flow. Delimiters `[` (STAGO), `[/` (ETAGO), `]` (TAGC) are fixed. Attribute values use double quotes; raw values do not.
- **DTD 4.0 compliance**: `[author]` requires `role="nd"` (default). `[aff]` exposes `orgname` as an **attribute**, not a child element. `[hist]` ordering is `(received, revised*, accepted?)`. `dateiso` is `YYYYMMDD` zero-padded.
- **Anti-duplication invariants**: do not pre-mark any tag that Markup auto-marks without prior-existence checks (`[author]`, `[fname]`, `[surname]`, `[kwd]`, `[normaff]`, `[doctitle]`, `[doi]`).
- **Superscript trap**: pre-marking `[label]` on a superscript run requires zeroing `Font.Superscript = false` on that run, otherwise `markup_sup_as` duplicates.
- **Stage 1 must not regress**: existing 11-rule Stage-1 pipeline output behavior is preserved except where Phase 1 deliberately changes the author rule.
- **Performance**: not a constraint; runs are operator-paced (one article at a time).
- **Privacy / compliance**: not relevant. Manuscripts are scientific and bound for public publication.

## Non-Goals (Out of Scope)

- **Replacing SciELO Markup Stage 2.** This PRD chooses to help Markup, not replace it (per ADR-001, alternative C). Tags Markup auto-marks reliably remain Markup's responsibility.
- **References / `[refs]`**: explicitly out of scope per `docs/scielo_context/README.md` ("Refs ficam fora — já existe automação externa").
- **Multi-language abstracts and keywords**: `language="en"` is hard-coded for the initial rollout. Multilingual support is deferred.
- **Article 5548**: present in `examples/formatted/` but not in `examples/phase-2/{before,after}/`. Not part of the validation corpus.
- **Footnote handling, table captions, figure captions, math, Greek glyph normalization**: out of scope.
- **Markup auto-mark behaviors that fail in ways unrelated to author handoff**: only the 5313/5449 author failure is in scope for Phase 1. Other Markup quirks are out of scope.
- **A GUI, web interface, or batch UI**: CLI only.
- **Continuous integration of the diff gate**: ADR-003 leaves CI integration as a future improvement; the gate runs locally pre-release for now.

## Phased Rollout Plan

### MVP (Phase 1) — Stage 1 author handoff fix

- Root-cause investigation of articles 5313 and 5449.
- ADR documenting the cause.
- Code adjustment in the Stage 1 author rule.
- Success criteria to proceed: SciELO Markup auto-marks every author on the post-fix `.docx` for both articles without manual intervention.

### Phase 2 — Easy Stage 2 tags

- Implement `[elocation]`, `[abstract]`, `[kwdgrp]` rules.
- Wire the new pipeline behind a separate subcommand or flag.
- Success criteria to proceed: diff between DocFormatter's Phase 2 output and `examples/phase-2/after/<id>.docx` is empty for the in-scope tags across all 10 corpus pairs.

### Phase 3 — Author-block xrefs, corresp, ORCID, attributes

- Implement `[corresp]`, author `xref` (aff and corresp), `[authorid]`, author attributes.
- Success criteria to proceed: diff covers Phase 2 + Phase 3 tags across all 10 corpus pairs.

### Phase 4 — `[hist]` with date parsing

- Port date-parsing from `Marcador_de_referencia` to a Phase 2 rule.
- Implement `[hist]` emission with strict ordering and `dateiso` formatting.
- Long-term success criteria: diff covers Phase 2 + Phase 3 + Phase 4 tags across all 10 corpus pairs. After this release, the typical operator path through Stage 2 reduces to verification + manual fill-ins for tags Markup auto-marks unreliably.

## Success Metrics

- **Primary metric — corpus diff pass rate**: number of corpus pairs (out of 10) for which `docformatter phase2 examples/phase-2/before/<id>.docx` produces a `.docx` whose in-scope tags equal `examples/phase-2/after/<id>.docx`. Target: 10 / 10 per release scope (per ADR-003).
- **Phase-1-specific**: SciELO Markup auto-marks all authors on 5313 and 5449 after the Phase 1 fix. Binary pass/fail per article.
- **Diagnostic clarity**: when a rule skips, the `diagnostic.json` warning contains a rule name and reason code that the operator can act on without guessing.
- **Secondary qualitative metric — operator time saved**: the user qualitatively reports rework time reduction per article. Not a release gate; used to confirm that diff-pass correlates with the real pain.

## Risks and Mitigations

- **Risk — Markup invariants drift**: a SciELO update (rare, but possible) changes Markup's auto-mark behavior, breaking ADR-002's "skip-and-warn" assumption.
  **Mitigation**: re-run the diff corpus after any SciELO toolchain update.
- **Risk — Corpus is too small to catch real-world variance**: the 10 pairs may not represent edge cases that appear in production articles.
  **Mitigation**: corpus is additive — when a real-world article reveals a missed pattern, add it to `examples/phase-2/{before,after}/` and re-run gate against the larger set.
- **Risk — Phase 4 date-parsing port stalls**: bringing in code from a separate repo (`Marcador_de_referencia`) introduces uncertainty about its applicability and maintenance burden.
  **Mitigation**: Phase 4 is intentionally last. Phases 1–3 deliver value independently. If Phase 4 proves intractable, descoping it does not break Phases 1–3.
- **Risk — Adoption: only one operator uses this**: if the operator (the user) changes role or workflow, the project loses its primary user.
  **Mitigation**: documentation (this PRD, ADRs, `docs/scielo_context/`) is sufficient for another preparer to take over without tribal knowledge.
- **Risk — Hard-coded `language="en"` excludes non-English manuscripts**: a future article in another language would not be handled correctly.
  **Mitigation**: deferred to a future PRD when multilingual content enters the corpus.

## Architecture Decision Records

- [ADR-001: Rollout Strategy — Help SciELO Markup, Don't Replace It](adrs/adr-001.md) — Adopt incremental rollout (Phase 1: author fix; Phase 2: easy tags; Phase 3: author block xrefs; Phase 4: `[hist]`); reject big-bang and full-Markup-replacement alternatives.
- [ADR-002: Failure Policy for Phase 2 Rules — Skip and Warn](adrs/adr-002.md) — When a heuristic cannot identify its target with high confidence, skip the tag and record a structured warning in `diagnostic.json`; never abort, never emit partial markup.
- [ADR-003: Diff-Based Validation Gate Using `examples/phase-2/{before,after}/`](adrs/adr-003.md) — Each release passes when its in-scope tags match the curated `after/` corpus across all 10 pairs.

## Open Questions

- **Q1 — Diff scoping mechanics**: how exactly does the diff utility compare two `.docx` files at the tag level while ignoring whitespace and out-of-scope tags? Resolution: TechSpec.
- **Q2 — CLI shape**: subcommand (`docformatter phase2`) vs. flag (`docformatter --stage=2`) vs. another form? Resolution: TechSpec.
- **Q3 — Output directory naming and layout**: `examples/formatted-phase2/`? `examples/phase2-output/`? Resolution: TechSpec.
- **Q4 — Phase 1 author fix root cause**: not yet known. Investigation is part of Phase 1 itself; the root cause will be documented in an additional ADR created during execution.
- **Q5 — Phase 4 date-parsing import strategy**: copy the relevant files from `Marcador_de_referencia` into this repo, or vendor as a submodule, or rewrite based on it? Resolution: TechSpec or Phase 4 implementation kickoff.
