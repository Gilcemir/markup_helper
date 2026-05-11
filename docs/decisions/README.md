# Decisions — DocFormatter

This directory holds **durable decisions** for the project: ADRs
(Architecture Decision Records) produced during the implementation of
each feature, grouped by feature in their own subfolders.

## How this relates to other docs

- **`CLAUDE.md`** — global instructions for agents writing code in this
  project. *How* to work.
- **`docs/scielo_context/`** — external reference for the legacy SciELO
  system (DTDs, hierarchy, Markup Word UI). Knowledge about the *other*
  system, not decisions of this project.
- **`docs/decisions/<feature>/`** — decisions of this project, with
  rationale and alternatives considered. *Why* we chose X over Y.
- **`docs/INVARIANTS.md`** — numbered rules (`INV-NN`) extracted from
  ADRs that must not be broken in future changes. Consolidated, short
  version for quick consultation.

## How to populate this directory

ADRs are born in `.compozy/tasks/<feature>/adrs/` during feature
implementation. Once the feature is complete and stable, run:

```
/promote-feature <feature-name>
```

The `promote-feature` skill copies the ADRs to
`docs/decisions/<feature>/`, updates this README with entries in the
indexes below, populates `docs/INVARIANTS.md` with declared invariants,
and removes the originals from `.compozy/`. From then on, this
directory is the single source of truth.

## Conventions

- **Language**: all generated artifacts under `docs/decisions/` and
  `docs/INVARIANTS.md` are written in **English**. This includes the
  per-feature READMEs, INVARIANTS entries, decision summaries in this
  index, and ADR contents. `CLAUDE.md` is the only project doc kept in
  Portuguese (project-specific instructions for the maintainer).
- **Naming**: `adr-NNN-kebab-case.md` numbered per feature, starting at
  `001`.
- Each feature gets a generated `README.md` with a short summary and a
  list of its ADRs.
- Invariants in ADRs are marked with `INV-NN — <title>` (e.g.,
  `INV-01 — Content preservation`). The `promote-feature` skill greps
  for this pattern to populate `docs/INVARIANTS.md`.
- Format of an ADR's body: see examples in already-promoted features.
- **Index entry format**: `- **<subject>** — <one-sentence decision> →
  <feature>/<file>`. Bold subject is greppable; the sentence summarizes
  *what* was decided (not just the ADR title).

---

## Index by domain

Decisions grouped by problem area. Use this to answer *"what did we
decide about X?"*.

### Architecture

Pipeline, project layout, frameworks/runtime, structural cross-cutting
decisions.

- **Phase 2 pipeline reuse** — Same `FormattingPipeline` +
  `IFormattingRule` + `FormattingContext`; Phase 2 rules live under
  `Rules/Phase2/` and rule sets compose via `AddPhase1Rules` /
  `AddPhase2Rules` DI extension methods.
  → phase-2-tagging-author-fixes/adr-004
- **Phase 2 rollout strategy** — Incremental "help SciELO Markup, don't
  replace it" approach across 4 phases (author fix → easy tags → author
  xrefs → `[hist]`); rejects full Stage-2 replacement and big-bang
  delivery. → phase-2-tagging-author-fixes/adr-001
- **Phase 2 rule failure policy** — A heuristic that cannot identify
  its target with high confidence MUST skip insertion and record a
  machine-readable reason code in `diagnostic.json`; never abort, never
  emit partial markup. → phase-2-tagging-author-fixes/adr-002
- **Pipeline architecture (MVP)** — 4 sibling rules (one per extracted
  field), not a single consolidated rule. Establishes the pattern for
  later phases. → metadata/adr-001
- **Project layout** — Core + Cli + Tests in 3 projects; no Gui in MVP.
  → metadata/adr-002
- **Runtime** — .NET 10 LTS + `TreatWarningsAsErrors`; overrides the
  `.NET 8` default from the spec. → metadata/adr-005

### Parsing

Text extraction, tokenization, heuristics and regex for field detection
(authors, ORCID, e-mail, anchors, detection predicates).

- **`<w:b>` cascade resolver** — Determines real bold by walking the
  OOXML cascade (`rPrChange` → `rPr` → `pPr/pPrChange` → styles);
  supersedes the run-only stance from section/adr-003.
  → section/adr-005
- **`INTRODUCTION` as anchor** — Literal `INTRODUCTION` heading serves
  as a positional anchor to scope the section-promotion rules.
  → section/adr-004
- **Authors + ORCID merge** — After a production bug,
  `ExtractOrcidLinksRule` and `ParseAuthorsRule` were merged into a
  single `ExtractAuthorsRule` (refactor of metadata/adr-003).
  → metadata/adr-006
- **Corresponding-author tokenization** — `* E-mail:` marker + email
  regex, two-pass; strips the `* E-mail: … ORCID: …` trailer from the
  affiliation. → polish/adr-003
- **HistDateParser phrase inventory** — Recognized date shapes
  catalogued from `AccessedOnHandler.cs` plus the SciELO `before/`
  corpus, extended with ISO / year-only / English-abbrev forms; TDD
  source of truth for the Phase 4 parser.
  → phase-2-tagging-author-fixes/adr-007-phrase-inventory
- **`mark_authors` 5313/5449 fix** — `AuthorBuilder.AddLabel` merges a
  pure-`*` label onto the trailing affiliation digit (`1,*` → `1*`)
  so Markup's `mark_authors` macro stops misreading the joined comma
  as an inter-author separator. → phase-2-tagging-author-fixes/adr-008
- **ORCID extraction** — Extracts (does not remove) ORCID URLs from
  the authors line and stores in metadata; intentional divergence from
  the MVP spec. → metadata/adr-003
- **Phase 4 date parser** — Rewrite `HistDateParser` from scratch in
  DocFormatter conventions; treat
  `Marcador_de_referencia/BibliographyHandlers/AccessedOnHandler.cs`
  as algorithmic reference only — no code copy, no vendor submodule.
  → phase-2-tagging-author-fixes/adr-007
- **Section detection without font size** — Section / sub-section
  predicate ignores font size; uses bold + caps + alignment only.
  → section/adr-003

### Formatting

OOXML manipulation, marcação rules, content transformation (bold
cascade, section promotion, italic preservation).

- **Phase 2 = 4 sibling Optional rules** — Not consolidated into a
  single rewrite; each behavior (alignment, spacing, abstract,
  corresp) is an independent Optional rule. → polish/adr-001
- **Phase 3 = 2 Optional rules** — `MoveHistoryRule` and
  `PromoteSectionsRule` kept separate, not combined into one rule.
  → section/adr-001
- **Italic structural stripping** — Uniformity heuristic: strip italic
  only if EVERY run in the paragraph is italic; preserves intentional
  italics (species names, emphasis). → polish/adr-002
- **`PromoteSectionsRule` cosmetic-only (INV-01)** — Mutates only
  `<w:jc>` and `<w:sz>`; forbidden to remove/create/reorder
  paragraphs. `MoveHistoryRule` is the only rule that reorders, and
  only the 3 history paragraphs. Fail-safe on any ambiguity.
  → section/adr-002

### Tooling

CLI surface, diagnostic output, build/CI, output file layout.

- **Diagnostic JSON: `formatting` section** — Phase 2 adds a
  `formatting` section to the diagnostic JSON; additive, does not
  change the extraction fields. → polish/adr-004
- **Diagnostic JSON schema** — Per-field with `confidence` + `issues`
  list, not issues-only. → metadata/adr-004
- **Phase 2 diff gate** — Each release passes only when all 10
  `examples/phase-2/{before,after}/` pairs match within the cumulative
  in-scope tag set; failure descopes the release or amends the corpus
  with justification. → phase-2-tagging-author-fixes/adr-003
- **Phase 2 diff utility** — Body-text extraction preserving SciELO
  `[tag]` literals; out-of-scope tag pairs symmetrically stripped
  (brackets gone, inner content kept) before string compare; first
  divergence reported with ±80 chars of context.
  → phase-2-tagging-author-fixes/adr-006
- **`phase2` / `phase2-verify` CLI** — Hand-rolled subcommand
  dispatcher inside `CliApp.Run` extends the existing parser; rejects
  `System.CommandLine` migration as scope creep.
  → phase-2-tagging-author-fixes/adr-005

---

## Index by feature

Decisions grouped by implementation phase. Use this to answer *"what
was decided during feature X?"*.

- [header-formatting-polish/](header-formatting-polish/) — 4 ADRs —
  Phase 2: alignment, spacing, abstract format, corresponding-author
  e-mail.
- [header-metadata-extraction/](header-metadata-extraction/) — 6 ADRs —
  DocFormatter MVP: header extraction and rewrite (DOI, title, authors,
  ELOCATION) plus the initial architecture.
- [phase-2-tagging-author-fixes/](phase-2-tagging-author-fixes/) — 9
  ADRs — Phase 2: pre-mark SciELO XML 4.0 Stage-2 tags (`elocation`,
  `xmlabstr`, `kwdgrp`, author xrefs/`authorid`/`corresp`, `[hist]`
  with date parsing) plus a Stage-1 fix for `mark_authors` on
  5313/5449.
- [section-formatting-and-history-move/](section-formatting-and-history-move/)
  — 5 ADRs — Phase 3: move history block, visually promote section /
  sub-section, INV-01 (strict content preservation).
