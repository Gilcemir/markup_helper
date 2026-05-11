# Phase 2 Tagging and Author Fixes

DocFormatter is a CLI that pre-formats `.docx` files before the SciELO
Markup Word plugin tags them, reducing manual rework on the path to
JATS XML. This feature addresses two unmet needs: a Stage 1 author
handoff failure where Markup's `mark_authors` macro silently dropped
authors on articles 5313 / 5449 even when DocFormatter extracted them
with high confidence, and a Stage 2 pre-marking gap for six DTD 4.0
tag groups (`elocation`, `xmlabstr`, `kwdgrp`, author-block xrefs and
`authorid`, `corresp`, `hist`) that Markup either ignores or marks
incompletely. Delivered as an incremental four-phase rollout gated on
the curated `examples/phase-2/{before,after}/` corpus.

## ADRs

- [adr-001](adr-001.md) — Rollout Strategy — Help SciELO Markup, Don't Replace It
- [adr-002](adr-002.md) — Failure Policy for Phase 2 Rules — Skip and Warn
- [adr-003](adr-003.md) — Diff-Based Validation Gate Using `examples/phase-2/{before,after}/`
- [adr-004](adr-004.md) — Pipeline Organization — Reuse `FormattingPipeline` with DI-Selected Rule Sets
- [adr-005](adr-005.md) — CLI Dispatch — Hand-Rolled Subcommands `phase2` and `phase2-verify`
- [adr-006](adr-006.md) — Diff Utility — Body-Text Extraction with Out-of-Scope Tag Stripping
- [adr-007](adr-007.md) — Phase 4 Date-Parser Port — Rewrite from Scratch Using `Marcador_de_referencia` as Reference
- [adr-007-phrase-inventory](adr-007-phrase-inventory.md) — Phrase Inventory — `HistDateParser` Recognized Shapes
- [adr-008](adr-008.md) — Root Cause of Markup `mark_authors` Failure on 5313 / 5449
