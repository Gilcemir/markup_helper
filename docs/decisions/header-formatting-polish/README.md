# Header Formatting Polish

Phase 2: adds DOI/section/title alignment, spacing between the author
block and affiliations, abstract reformatting (bold "Abstract" + plain
body), and e-mail/ORCID extraction from the corresponding author.
Four new `Optional` rules layered on top of the MVP pipeline.

## ADRs

- [adr-001-four-discrete-rules](adr-001-four-discrete-rules.md) — Four discrete Optional rules over a single consolidated rewrite
- [adr-002-italic-preservation-heuristic](adr-002-italic-preservation-heuristic.md) — Structural-italic stripping heuristic for the abstract body
- [adr-003-corresponding-author-tokenization](adr-003-corresponding-author-tokenization.md) — Marker tokenization and email regex for the corresponding-author rule
- [adr-004-diagnostic-formatting-section](adr-004-diagnostic-formatting-section.md) — Additive `formatting` section in the diagnostic JSON
