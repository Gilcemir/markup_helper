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
- **ORCID extraction** — Extracts (does not remove) ORCID URLs from
  the authors line and stores in metadata; intentional divergence from
  the MVP spec. → metadata/adr-003
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
- [section-formatting-and-history-move/](section-formatting-and-history-move/)
  — 5 ADRs — Phase 3: move history block, visually promote section /
  sub-section, INV-01 (strict content preservation).
