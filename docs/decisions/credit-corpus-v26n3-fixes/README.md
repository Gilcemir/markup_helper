# Credit Corpus v26n3 Fixes

Running Phase 3 on the 15 articles of CBAB v26n3 left 13 with a warning or a
false negative on CRediT roles, although every CREDIT statement in the edition
is structured (author-keyed). The tool rejected the `;` separator the authors
use, silently dropped a statement tagged `[p]`, resolved initials too loosely in
one direction and too strictly in another, and wrote a diagnostic without the
text the operator needs. Upstream, Phase 1 let a plain-text ORCID stay inside
the author name, which SciELO Markup then promoted to the surname. This feature
fixes the deterministic defects in both phases, makes broken contributor names
visible, and gives the operator an edition-level view of what is pending.

## ADRs

- [adr-001](adr-001.md) — Ship the CBAB v26n3 CRediT corrections as one patch release across Phase 1 and Phase 3
- [adr-002](adr-002.md) — A broken contributor name warns but never blocks CRediT injection
- [adr-003](adr-003.md) — Widen the author-keyed CREDIT grammar with shape-based lookback, and never drop a present statement
- [adr-004](adr-004.md) — Tiered, broadened candidate initials in the author resolver
- [adr-005](adr-005.md) — The CRediT outcome is recorded on Phase3Context and read by the diagnostic and the batch summary
- [adr-006](adr-006.md) — A plain-text ORCID in the byline is tokenized like a hyperlink ORCID

## Invariants

This feature contributes `INV-02` to [docs/INVARIANTS.md](../../INVARIANTS.md).
