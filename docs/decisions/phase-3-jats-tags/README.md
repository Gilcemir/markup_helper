# Phase 3 — JATS Tag Injection

Phase 3 post-processes the JATS XML produced by SciELO Markup, injecting four
SciELO Publishing Schema (SPS 1.10) markings the Markup tool cannot generate
(`article-id pub-id-type="other"`, `fn fn-type="edited-by"`,
`sec sec-type="data-availability"`, and CRediT `<role>`). It infers values
deterministically from a paired docx source plus an external `other.txt`,
asking for human confirmation only on genuinely ambiguous judgment calls.

## ADRs

- [adr-001](adr-001.md) — Confidence-gated automatic injection with inline confirmation
- [adr-002](adr-002.md) — Source values from the paired docx markup source, inject into the XML
- [adr-003](adr-003.md) — Dedicated IJatsInjector pipeline for XML post-processing
- [adr-004](adr-004.md) — Pair docx ↔ XML ↔ other.txt on elocation-id, verify with DOI
- [adr-005](adr-005.md) — CRediT auto-mapping only for structured statements with exact terms
- [adr-006](adr-006.md) — Confirmation gate via IConfirmer with an injectable non-interactive policy
- [adr-007](adr-007.md) — Free-text role fallback as an operator-chosen, document-scoped disposition
