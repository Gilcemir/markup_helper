---
status: completed
title: 'Phase 3 release — `EmitCorrespTagRule` and `EmitAuthorXrefsRule`'
type: backend
complexity: high
dependencies:
  - task_06
---

# Task 07: Phase 3 release — `EmitCorrespTagRule` and `EmitAuthorXrefsRule`

## Overview
Phase 3 closes the author-block markup gap that Markup leaves behind: `[corresp id="c1"]…[/corresp]`, per-author `xref ref-type="aff"` tags, `xref ref-type="corresp" rid="c1"` for the corresponding author, `[authorid ctrbidtp="orcid"]` for ORCID-bearing authors, and the author attributes `rid` / `corresp` / `deceased="n"`. Authors themselves (`[author]`, `[fname]`, `[surname]`) remain Markup's job per ADR-001 anti-duplication. After this task, the corpus diff gate passes for the cumulative scope `{elocation, abstract, kwdgrp, corresp, xref, authorid}`.

<critical>
- ALWAYS READ the PRD and TechSpec before starting
- REFERENCE TECHSPEC for implementation details — do not duplicate here
- FOCUS ON "WHAT" — describe what needs to be accomplished, not how
- MINIMIZE CODE — show code only to illustrate current structure or problem areas
- TESTS REQUIRED — every task MUST include tests in deliverables
</critical>

<requirements>
- MUST implement `EmitCorrespTagRule` that wraps the corresponding-author block in `[corresp id="c1"]…[/corresp]`. The rule reads `ctx.CorrespondingEmail`, `ctx.CorrespondingOrcid`, `ctx.CorrespondingAuthorIndex`, `ctx.CorrespondingAffiliationParagraph` (already populated by the existing `ExtractCorrespondingAuthorRule`).
- MUST implement `EmitAuthorXrefsRule` that, for each author in `ctx.Authors`:
  - emits one `xref ref-type="aff" rid="<affId>"` per affiliation label the author bears,
  - emits `xref ref-type="corresp" rid="c1"` for the corresponding author,
  - emits `[authorid ctrbidtp="orcid"]…[/authorid]` for any author with an ORCID,
  - emits the author attributes `rid="<list>"`, `corresp="yes"|"no"`, `deceased="n"` on the author element produced by Markup. Since Markup auto-marks `[author]`, the attributes are emitted on the literal text scaffold that Markup recognizes (per `docs/scielo_context/REENTRANCE.md`).
- MUST add the structured records `Affiliation` and `CorrespAuthor` under `DocFormatter.Core/Models/Phase2/`.
- MUST extend `FormattingContext` with nullable `Affiliations?` and `CorrespAuthor?` (extracted from existing `ctx.Authors[].AffiliationLabels` and the existing corresponding-author fields).
- MUST update `Phase2Scope.Current` to `{elocation, abstract, kwdgrp, corresp, xref, authorid}`.
- MUST register both rules under `AddPhase2Rules()`.
- MUST extend the `DiagnosticDocument.Phase2` block with `Corresp` and per-author `Xref` summary fields.
- MUST NOT emit `[author]`, `[fname]`, `[surname]`, `[normaff]` (anti-duplication; assertions in unit tests).
- Each rule MUST follow ADR-002 skip-and-warn on missing inputs.
</requirements>

## Subtasks
- [x] 7.1 Add `Affiliation` and `CorrespAuthor` records under `DocFormatter.Core/Models/Phase2/`.
- [x] 7.2 Extend `FormattingContext` with `Affiliations?` and `CorrespAuthor?` nullable fields.
- [x] 7.3 Implement `EmitCorrespTagRule` under `DocFormatter.Core/Rules/Phase2/` using `TagEmitter`.
- [x] 7.4 Implement `EmitAuthorXrefsRule` under `DocFormatter.Core/Rules/Phase2/` using `TagEmitter` (xref + authorid + author attribute emission).
- [x] 7.5 Register both rules under `AddPhase2Rules()`.
- [x] 7.6 Extend `DiagnosticDocument` and `DiagnosticWriter` with the new `Corresp` and `Xref` summaries.
- [x] 7.7 Update `Phase2Scope.Current` and re-run `Phase2CorpusTests.AllPairsMatch` against the cumulative scope.

## Implementation Details
Both rules live under `DocFormatter.Core/Rules/Phase2/`. They depend on data populated by Phase 1 rules: `ExtractAuthorsRule` already exposes `ctx.Authors` with each author's affiliation labels and ORCID; `ExtractCorrespondingAuthorRule` already populates `ctx.CorrespondingEmail`, `CorrespondingOrcid`, `CorrespondingAuthorIndex`, `CorrespondingAffiliationParagraph`. So Phase 2 rules consume what Phase 1 left in the context — no re-extraction. See TechSpec "Internal dependencies between rules" and "Component Overview" `Rules/Phase2/` row. The author-attribute emission needs the literal scaffold Markup recognizes — see `docs/scielo_context/REENTRANCE.md` for the exact shape that survives `mark_authors`.

### Relevant Files
- `DocFormatter.Core/TagEmission/TagEmitter.cs` — emission primitive (task 02).
- `DocFormatter.Core/Rules/ExtractAuthorsRule.cs` — populates `ctx.Authors` (~527 lines).
- `DocFormatter.Core/Rules/ExtractCorrespondingAuthorRule.cs` — populates corresp-author fields.
- `DocFormatter.Core/Models/Author.cs` — author record consumed by xref rule.
- `DocFormatter.Core/Pipeline/FormattingContext.cs` — extend with `Affiliations?`, `CorrespAuthor?`.
- `examples/phase-2/before/*.docx` and `examples/phase-2/after/*.docx` — corpus to pass against.
- `docs/scielo_context/REENTRANCE.md` — anti-duplication & literal scaffold guidance.

### Dependent Files
- `DocFormatter.Core/Pipeline/RuleRegistration.cs` — `AddPhase2Rules()` extended.
- `DocFormatter.Core/Reporting/DiagnosticDocument.cs` and `DiagnosticWriter.cs` — `DiagnosticPhase2` extended with `Corresp` and `Xref` summaries.
- `DocFormatter.Tests/Phase2/EmitCorrespTagRuleTests.cs`, `EmitAuthorXrefsRuleTests.cs` — new test files.
- `DocFormatter.Tests/Phase2CorpusTests.cs` — re-runs with extended scope (test code unchanged; `Phase2Scope.Current` changes).
- Task 09 — extends `Phase2Scope.Current` further with `hist`.

### Related ADRs
- [ADR-001: Rollout Strategy](adrs/adr-001.md) — Phase 3 release scope (corresp, xref, authorid, author attrs).
- [ADR-002: Failure Policy — Skip and Warn](adrs/adr-002.md).
- [ADR-003: Diff-Based Validation Gate](adrs/adr-003.md) — Cumulative scope expanding here.
- [ADR-004: Pipeline Organization](adrs/adr-004.md).

## Deliverables
- Two new rule files under `DocFormatter.Core/Rules/Phase2/`.
- New records `Affiliation` and `CorrespAuthor` under `DocFormatter.Core/Models/Phase2/`.
- Extended `FormattingContext` with two nullable Phase 2 fields.
- Extended `DiagnosticDocument.Phase2` with `Corresp` and `Xref` summaries.
- Updated `Phase2Scope.Current` set.
- `Phase2CorpusTests.AllPairsMatch` green for all 10 pairs at the cumulative scope.
- Per-rule unit tests with 80%+ coverage **(REQUIRED)**.
- Integration test exercising Phase 2 + Phase 3 emitters end-to-end on a synthetic multi-author fixture **(REQUIRED)**.

## Tests
- Unit tests:
  - [x] `EmitCorrespTagRule` golden path: asterisk-led `* E-mail: …` paragraph → `[corresp id="c1"]…[/corresp]` wraps it; `Corresponding author: …` shape also covered.
  - [x] `EmitCorrespTagRule` skip-and-warn: no corresponding-author signal → no tag emitted; warning with reason code `corresp_block_not_found`.
  - [x] `EmitAuthorXrefsRule` golden path: an author with two affiliation labels emits `rid="aff1 aff2"`.
  - [x] `EmitAuthorXrefsRule` golden path: corresponding author with plain `<digit>(,<digit>)*\*` trailer expands into structured xrefs (aff + corresp) preserving the comma.
  - [x] `EmitAuthorXrefsRule` golden path: author with ORCID emits `[authorid authidtp="orcid"]<orcid>[/authorid]`; author without ORCID emits no `[authorid]`. (Note: corpus uses `authidtp`, not `ctrbidtp`.)
  - [x] `EmitAuthorXrefsRule` author attributes: `rid` lists the affiliation labels; `corresp="y"` only on the corresponding author; `deceased="n"` and `eqcontr="nd"` always present. (Note: corpus uses `y`/`n`, not `yes`/`no`.)
  - [x] `EmitAuthorXrefsRule` skip-and-warn: no author paragraphs → no tags emitted; warning with reason code `authors_missing`.
  - [x] Anti-duplication: rule does NOT introduce additional `[author]`, `[fname]`, `[surname]`, `[normaff]` literals (count remains 1 per pre-existing shell, none for `[normaff]` / `[kwd]`).
- Integration tests:
  - [x] `Phase2CorpusTests.AllPairsMatch` passes for all 10 pairs at the new `Phase2Scope.Current = {authorid, corresp, doc, doctitle, doi, kwdgrp, label, normaff, toctitle, xmlabstr, xref}`.
  - [x] Synthetic 3-author fixture (one corresponding, three with ORCID) → produced `.docx` body contains all expected `xref`, `[authorid]`, `[corresp]` literals in order.
  - [x] `make phase2-verify` exits 0 against the corpus after this task.
  - [x] `diagnostic.json` for a sample run contains the extended `phase2` block with `corresp` and per-author `xref` summaries (verified by `Phase2PipelineIntegrationTests`).
- Test coverage target: >=80%
- All tests must pass

## Success Criteria
- All tests passing
- Test coverage >=80%
- `Phase2CorpusTests.AllPairsMatch` green at the Phase 3 cumulative scope.
- `phase2-verify` exits 0 on the corpus.
- No anti-duplication-list tag is emitted by either rule.
