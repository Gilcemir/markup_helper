# ADR-002: Strict content preservation invariant (INV-01)

## Status

Accepted

## Date

2026-05-07

## Context

The editor's strongest stated requirement for Phase 3 is that **no text content may disappear** from the document under any circumstance. Lost paragraphs, runs, or text nodes are unrecoverable in the editorial workflow because the source `.docx` may have been overwritten, the formatted version is what gets reviewed, and the editor lacks the cycles to byte-compare every output against its source.

Phase 1+2 rules also avoid content loss in practice, but the guarantee was implicit. With Phase 3 adding a rule that **moves paragraphs** for the first time (`MoveHistoryRule`), the risk surface widens: an OOXML reorder operation that throws mid-flight, or that places a paragraph in an invalid position, could detach the moved node from the body and leave it unreachable.

This ADR makes the invariant explicit, scopes it to Phase 3, and binds both new rules to a falha-segura (fail-safe) behaviour at every uncertainty.

## Decision

Adopt **INV-01: Strict Content Preservation** as a binding invariant for Phase 3:

1. **`PromoteSectionsRule` is purely cosmetic.** It mutates only `<w:jc>` (Justification) on `ParagraphProperties` and `<w:sz>` (FontSize) on existing `RunProperties`. It is forbidden from removing paragraphs, runs, text nodes, breaks, drawings, hyperlinks, or any other element; from reordering elements; from creating new paragraphs.

2. **`MoveHistoryRule` is the only rule that reorders paragraphs**, and only the three history paragraphs (`Received` / `Accepted` / `Published`). On any ambiguity — anchor `INTRODUCTION` not found, partial match of the three markers, out-of-order markers, non-empty paragraphs interleaved, multiple candidate blocks — the rule does not move anything and emits a `[WARN]` with a precise reason code.

3. **Both rules are `Optional` severity.** Any unexpected exception is captured by the pipeline, logged as `[ERROR]`, and pipeline execution continues. The document on disk is preserved.

4. **In any doubt, the rule does not act.** A non-formatted but content-preserved file is preferred over a formatted file with content loss.

5. **Tests assert the invariant at unit and integration level.** Each rule has a test that runs the rule on a fixture, collects `set(non-empty trimmed text)` of all `<w:t>` nodes in the body before and after, and asserts the set difference is empty.

## Alternatives Considered

### Alternative 1: Best-effort with no formal invariant

- **Description**: Trust the implementation to preserve content because the rules' behaviour is bounded; no explicit invariant or test.
- **Pros**:
  - Less code, fewer tests.
- **Cons**:
  - No regression protection if a future contributor extends `MoveHistoryRule` to handle additional paragraph types and accidentally drops content.
  - The user's stated constraint becomes a tribal-knowledge expectation rather than a verified property.
- **Why rejected**: The user explicitly stated content loss is unacceptable. Tribal knowledge does not survive personnel changes or AI-assisted refactors.

### Alternative 2: Runtime invariant rule `AssertContentPreservedRule` (Critical)

- **Description**: A final pipeline rule scans body text before and after Phase 3 and aborts on diff.
- **Pros**:
  - Catches regressions even from future rules outside Phase 3.
- **Cons**:
  - Risk of false positives from comparison noise (NBSP vs space, Unicode normalization).
  - Critical severity means a noisy comparison aborts an otherwise-good output, the worst possible failure mode for the editor.
  - Adds a full-document scan to every pipeline run.
- **Why rejected**: See ADR-001's rejection of the same alternative.

## Consequences

### Positive

- The strongest user constraint for Phase 3 is documented and verified by tests.
- Implementation is bounded: contributors know exactly which OOXML mutations are allowed.
- The fail-safe stance means any unexpected input produces a non-formatted but recoverable file rather than a corrupted one.
- The invariant is teachable: a future rule that wants to delete a paragraph triggers an explicit ADR-level conversation.

### Negative

- Some legitimate clean-up operations are out of scope (e.g., removing redundant blank paragraphs around the moved history block). Phase 3 leaves spacing as-is.

### Risks

- **Risk**: A future rule introduced under a different invariant could regress this one. **Mitigation**: ADR-002 must be referenced from any future ADR that proposes a deletion or reorder.
- **Risk**: Tests assert text preservation but not formatting preservation; a bug that scrambles run properties without losing text would pass. **Mitigation**: golden-file end-to-end tests catch formatting regressions. The text-set test is one layer of defence, not the only one.

## Implementation Notes

- The text-set assertion compares **non-empty trimmed** text values. Whitespace-only `<w:t>` nodes are filtered out before comparison; this avoids false alarms from cosmetic whitespace changes.
- Comparison is over a `multiset` (allowing duplicates of the same string); the test fails if any element of the input multiset is missing from the output multiset, even if the document gained an extra string.
- The assertion runs on the body of the document only; headers, footers, footnotes are out of scope (no Phase 3 rule touches them).

## References

- [PRD: Section Formatting and History Move](../_prd.md)
- [ADR-001: Two discrete Optional rules over a single combined rule](adr-001-two-discrete-rules.md)
