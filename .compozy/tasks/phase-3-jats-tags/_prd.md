# PRD: Phase 3 — JATS Tag Injection

## Overview

Phase 3 of DocFormatter post-processes the JATS XML produced by SciELO Markup,
injecting four SciELO Publishing Schema (SPS 1.10) markings that the Markup tool
cannot generate. Today these four markings are applied by hand (and partly via an
AI agent), which is over-engineering for a problem that is mostly deterministic.

The four tags:

| Tag | Source of value |
|---|---|
| `<article-id pub-id-type="other">` | external `other.txt` (TSV: package name → 5-digit number) |
| `<fn fn-type="edited-by">` (responsible editor) | docx markup source ("Scientific Editor: \<name\>") |
| `<sec sec-type="data-availability">` | docx markup source (statement text) + classification |
| `<role>` CRediT (author roles) | docx markup source ("CREDIT STATEMENT" section) |

**Who it is for:** the SciELO production operator preparing article packages —
the person who currently inserts these four markings manually.

**Why it is valuable:** it removes repetitive manual editing from package
preparation, replacing it with deterministic derivation plus a thin layer of
human confirmation only where genuine judgment is required. It also captures
placement logic that was proven in an earlier (abandoned) project, so the
knowledge is not lost.

The predecessor project (MathML) was abandoned because it demanded manual input
for *every* value through a UI — the same effort as marking by hand. Phase 3's
defining constraint is to **infer first and ask only when uncertain**.

## Goals

- Inject all four tags into article-package XML with correct SPS 1.10 placement.
- Eliminate manual entry for deterministic values (the `other` number, editor
  name + role) and for the document-derived text (data-availability statement,
  CRediT roles).
- Require human input **only** for genuinely ambiguous judgment calls, never for
  values the tool can derive confidently.
- Produce a per-file report recording every decision (auto-applied and
  confirmed) so output is auditable after the fact.
- Reuse the existing DocFormatter conventions (CLI command, per-file report,
  example/golden corpus) so phase 3 feels like a natural continuation of
  phases 1/2.

## User Stories

**Primary persona — SciELO production operator**

- As an operator, I want the `other` number injected automatically from
  `other.txt` so I never type 5-digit numbers by hand.
- As an operator, I want the responsible editor's name and role pulled from the
  docx and written into `<author-notes>` so I do not re-enter editorial metadata.
- As an operator, I want the data-availability statement text lifted from the
  docx and placed in `<back>`, with its category proposed for me, so I only
  decide the category when the text is genuinely unclear.
- As an operator, I want author contribution roles mapped to CRediT and attached
  to the right author so I do not hand-build `<role>` elements.
- As an operator, I want the tool to stop and ask me only when it is unsure, so
  the common case runs with no interaction.
- As an operator, I want a report of what the tool did to each file so I can
  trust and audit the output.

## Core Features

### 1. `other` number injection (deterministic)

Look up the package's `other` value in `other.txt` and insert
`<article-id pub-id-type="other">` immediately after the existing
`<article-id pub-id-type="doi">`. Fail loudly (no silent guess) if the XML has
no DOI or the package has no entry in `other.txt`.

### 2. Responsible editor injection (deterministic, name-only)

Read "Scientific Editor: \<name\>" (and any equivalent role line) from the docx.
Emit `<fn fn-type="edited-by">` inside the single `<author-notes>` element
(creating it if absent), with the role in `<label>` (e.g. `SCIENTIFIC EDITOR:`)
and the name in `<p>`. ORCID is **not required** by SPS 1.10; emit name + role
only and omit `<ext-link>` when no ORCID is available (best-effort).

### 3. Data-availability statement injection (text deterministic, category judged)

Lift the statement text from the docx ("DATA AVAILABILITY" section). Emit it as
`<sec sec-type="data-availability">` with a `<title>`, inserted after `<ack>` in
`<back>` (placement rules per
`docs/scielo_context/jats/data_availability.md`). Classify `@specific-use` into
one of the five values using a keyword heuristic seeded from the curated corpus;
auto-apply when confident, prompt for confirmation when ambiguous.

### 4. CRediT roles injection (mapping deterministic, recognition judged)

Parse the "CREDIT STATEMENT" section (author initials → role list) from the
docx. Match initials to the corresponding `<contrib>` in the XML, map each
written term to its CRediT URL via the exact lookup table, and emit one
`<role content-type="…">` per role inside the contributor, after `<xref>`.
Honor SPS's all-or-nothing CRediT rule per document. Auto-apply confident
matches; prompt when initials cannot be matched to an author or a term is not
recognized.

### 5. Confidence-gated confirmation and reporting

Deterministic and high-confidence derivations are applied automatically.
Genuinely ambiguous judgment calls pause for inline confirmation (propose →
confirm/override → write). Every decision, including auto-applied ones, is
written to a per-file report.

### Feature interaction

All four tags operate on a single paired (docx, XML) document plus `other.txt`.
The docx is read-only; the XML is the only artifact modified. Tags are
independent in placement but share the same source-pairing and reporting flow.

## User Experience

1. The operator points phase 3 at a package (single file or a folder of paired
   docx + XML), with `other.txt` available.
2. For each document, the tool pairs the docx to its XML, derives the four
   tags, and writes them into the XML.
3. When a judgment call is confident, it is applied silently and logged. When
   ambiguous, the tool prints the proposed value and waits for the operator to
   confirm or correct it before writing.
4. The tool writes the modified XML and a per-file report listing every
   injected tag, the chosen value, and whether it was auto-applied or confirmed.
5. The operator reviews the report; nothing requires re-typing values that were
   already in the docx or `other.txt`.

Consistency: phase 3 follows the existing CLI/report conventions of phases 1/2,
so an operator already familiar with DocFormatter needs no new mental model
beyond the occasional confirmation prompt.

## High-Level Technical Constraints

- Output XML must conform to SciELO Publishing Schema (SPS) 1.10; tag placement
  follows the rules curated in `docs/scielo_context/jats/*.md` (ported from the
  prior MathML project and traceable to `_raw/SPS 1.10_pt.md`).
- Inputs: a JATS XML package, its paired docx markup source, and `other.txt`.
  The docx is read-only; only the XML is modified.
- Namespaces of injected elements are inherited from the document root.
- The tool must not emit a partially-wrong tag silently: deterministic
  preconditions (e.g. DOI present for `other`) fail loudly, and auto-applied
  judgments are recorded for audit.

## Non-Goals (Out of Scope)

- Rewriting phases 1/2 or the SciELO Markup tool.
- Bracket tags `[tag]` (that is phase 2).
- References/citations (already handled by external automation).
- Resolving editor ORCIDs (name + role only; ORCID is best-effort if present).
- A dedicated reusable skill for phase 3 (deferred until the implementation
  matures).
- Producing the `other` numbers themselves (assigned externally by SciELO;
  consumed from `other.txt`).

## Phased Rollout Plan

All four tags ship together in the MVP (per ADR scope decision); phasing here is
about hardening, not about which tags are included.

### MVP (Phase 1)

- All four tags injected with SPS 1.10 placement.
- Confidence-gated confirmation: deterministic/high-confidence auto-applied;
  ambiguous cases prompt inline.
- Per-file report of every decision.
- Single-file and folder/batch processing, consistent with phases 1/2.
- Success criteria: every file in the `examples/phase-3/` corpus is processed;
  all four tags placed correctly; auto-applied decisions match expected output;
  ambiguous cases surfaced rather than mis-applied.

### Phase 2

- Tune confidence thresholds for the data-availability classifier and CRediT
  matching against a larger corpus to reduce both wrong auto-applies and
  unnecessary prompts.
- Optional editor-ORCID enrichment from an external file, if a source is found.

### Phase 3

- Consider a batch review mode (propose → review → apply) if inline prompts
  prove too frequent at scale.
- Extract a reusable skill once the workflow has stabilized.

## Success Metrics

- **Manual-entry reduction**: zero manual typing for `other`, editor, and the
  document-derived text across the example corpus.
- **Interaction rate**: fraction of files processed with no confirmation prompt
  (higher is better); track which tag drives prompts.
- **Correctness**: injected tags validate against SPS 1.10 and match expected
  placement/values on the golden corpus; zero silently-wrong auto-applies.
- **Auditability**: every injected tag appears in the per-file report with its
  value and auto/confirmed status.

## Risks and Mitigations

- **Confidence miscalibration causes wrong silent auto-applies.** Mitigation:
  conservative initial thresholds favoring prompts; record all auto decisions
  in the report; tune against the corpus. (See ADR-001.)
- **docx ↔ XML mis-pairing applies the wrong source.** Mitigation: pair on a
  strong key (DOI) and fail loudly on ambiguous/missing matches. (See ADR-002.)
- **docx prose varies across journals/authors**, breaking section detection.
  Mitigation: anchor on observed headers (`DATA AVAILABILITY`,
  `CREDIT STATEMENT`, `Scientific Editor:`); flag misses for confirmation rather
  than guessing.
- **Prompt fatigue** if many cases are ambiguous. Mitigation: keep the gate
  tight; revisit a batch review mode (deferred Phase 3) if needed.
- **Adoption risk**: if confirmation feels as heavy as manual marking, operators
  revert to doing it by hand (the failure mode that killed MathML). Mitigation:
  the whole design optimizes for rare prompts; measure interaction rate.

## Architecture Decision Records

- [ADR-001: Confidence-gated automatic injection with inline confirmation](adrs/adr-001.md)
  — auto-apply deterministic/high-confidence values, prompt only for ambiguous
  judgment calls, log every decision.
- [ADR-002: Source values from the paired docx markup source, inject into the XML](adrs/adr-002.md)
  — read DA/CRediT/editor from the docx and `other` from `other.txt`; the XML is
  the only artifact modified.

## Open Questions

- **docx ↔ XML pairing key**: DOI match vs. the numeric id embedded in the XML
  filename — which is authoritative? (TechSpec.)
- **"Ambiguous" thresholds**: concrete criteria/confidence per tag that decide
  auto-apply vs. prompt. (TechSpec.)
- **Idempotency / re-run**: if a target tag already exists in the XML, skip,
  overwrite, or flag? (TechSpec / corpus check.)
- **CRediT initials → author matching**: how to derive author initials from
  `<contrib>` names robustly across naming conventions, and what to do when a
  statement lists initials with no matching author.
- **Batch + inline prompts UX**: how confirmation behaves mid-batch (per-file
  pause vs. collect-and-ask). May motivate the deferred batch review mode.
