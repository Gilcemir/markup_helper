# Section Formatting and History Move

Phase 3: moves the history block (`Received` / `Accepted` / `Published`)
to immediately above `INTRODUCTION`, and visually promotes section /
sub-section headings (section = 16pt centered bold; sub-section = 14pt
centered bold). Introduces **INV-01 — Strict Content Preservation** as a
binding invariant for the phase.

Invariants contributed: [INV-01](../../INVARIANTS.md)

## ADRs

- [adr-001-two-discrete-rules](adr-001-two-discrete-rules.md) — Two discrete Optional rules over a single combined rule
- [adr-002-content-preservation-invariant](adr-002-content-preservation-invariant.md) — Strict content preservation invariant (INV-01)
- [adr-003-discard-font-size-from-detection](adr-003-discard-font-size-from-detection.md) — Discard font size from section/sub-section detection predicate
- [adr-004-introduction-as-detection-anchor](adr-004-introduction-as-detection-anchor.md) — `INTRODUCTION` as detection anchor for section/sub-section scope
- [adr-005-bold-cascade-resolver](adr-005-bold-cascade-resolver.md) — Resolve `<w:b>` via OOXML cascade chain (supersedes the run-only stance from ADR-003)
