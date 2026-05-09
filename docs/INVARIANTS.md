# Invariants — DocFormatter

Numbered rules extracted from ADRs that **must not be violated** in
future changes. Each invariant originates in a decision recorded in
`docs/decisions/`.

Before implementing a rule that touches an already-decided area, read
this file. If your change appears to break an invariant, read the
source ADR to understand why — and propose an explicit revocation (via
a new ADR) instead of breaking it silently.

## Format of each entry

```
## INV-NN — <short title>
<1–2 sentences describing the rule>
Source: docs/decisions/<feature>/<adr-file>.md
```

Numbering is global (does not reset per feature). Do not reuse numbers:
the `promote-feature` skill fails when it tries to add an `INV-NN`
already present.

---

## INV-01 — Strict Content Preservation

No body text may disappear from the document under any circumstance.
Phase 3 rules may reorder the three paragraphs of the history block and
mutate `<w:jc>` (Justification) and `<w:sz>` (FontSize) on existing
paragraphs — but they are forbidden from removing, hiding, or creating
paragraphs / runs / text nodes / breaks / drawings / hyperlinks. On any
ambiguity, the rule does not act and emits `[WARN]` with a reason code.
Every rule has a test that compares the multiset of non-empty trimmed
body texts before and after, asserting an empty difference.

Source: docs/decisions/section-formatting-and-history-move/adr-002-content-preservation-invariant.md
