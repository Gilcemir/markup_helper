# ADR-001: Two discrete Optional rules over a single combined rule

## Status

Accepted

## Date

2026-05-07

## Context

Phase 3 introduces two new behaviours into the DocFormatter pipeline:

1. **Move article history block** (the three consecutive paragraphs `Received: …`, `Accepted: …`, `Published: …`) from its current position to immediately above the `INTRODUCTION` section.
2. **Promote sections and sub-sections** in the body to the journal's editorial format (`16pt + bold + center` for sections; `14pt + bold + center` for sub-sections).

Both behaviours operate on the body of the document, both depend on the existence of the `INTRODUCTION` paragraph as an anchor, and both must satisfy the strict content-preservation invariant defined in ADR-002.

The pipeline already follows a one-rule-per-responsibility convention (nine rules across Phase 1+2, each implementing `IFormattingRule`). The decision is whether to extend that pattern with two new rules or to combine the two new behaviours into a single rule.

## Decision

Implement two distinct rules:

- `MoveHistoryRule` — pipeline position #10, severity `Optional`. Reorders the three history paragraphs to sit immediately before `INTRODUCTION`. Does not mutate any paragraph property.
- `PromoteSectionsRule` — pipeline position #11, severity `Optional`. Mutates `<w:jc>` and `<w:sz>` on paragraphs identified as section or sub-section. Does not reorder anything.

Both rules consume `INTRODUCTION` as a positional anchor but neither mutates it (other than `PromoteSectionsRule` formatting the anchor as a section). The two rules commute: applying either one before the other produces the same final document state.

## Alternatives Considered

### Alternative 1: Single combined rule `Phase3PostProcessRule`

- **Description**: One class implements both behaviours inside a single `Apply()` method.
- **Pros**:
  - One fewer class, one fewer DI registration.
  - Anchor lookup runs once.
- **Cons**:
  - An exception in either sub-task contaminates the other (both report as one `[ERROR]` even when only one failed).
  - Tests grow heavier (a single test fixture covers two unrelated behaviours).
  - Diagnostic JSON loses granularity: a single combined object instead of `formatting.history_move` and `formatting.section_promotion`.
  - Regresses the established architectural pattern of one rule per responsibility.
- **Why rejected**: Marginal gain (one class, one anchor lookup) does not offset the loss of failure isolation, test focus, and diagnostic granularity.

### Alternative 2: Two rules plus a runtime invariant rule `AssertContentPreservedRule` (Critical)

- **Description**: Two rules as in the chosen design, plus a third Critical rule that verifies `set(non-empty texts before pipeline) == set(non-empty texts after pipeline)` and aborts the run if any text was lost.
- **Pros**:
  - Strongest possible runtime guarantee for ADR-002 (`INV-01`).
  - Catches regressions introduced by future rules without test coverage.
- **Cons**:
  - Adds a full-document scan at the end of every pipeline run.
  - High risk of false positives from comparison noise (NBSP vs space, Unicode normalization, trailing whitespace).
  - Critical severity means a noisy comparison aborts otherwise-good outputs.
- **Why rejected**: The invariant is already enforced by (a) per-rule unit tests asserting paragraph-set preservation and (b) golden-file comparisons in end-to-end tests. Adding a runtime invariant trades complexity and noise for redundancy. Reconsider in a future phase if a real regression is observed.

## Consequences

### Positive

- Each rule is independently testable and independently disable-able.
- Diagnostic JSON gains two distinct objects (`formatting.history_move`, `formatting.section_promotion`) with rule-specific fields.
- Failure of one rule does not block the other (both `Optional`).
- Pattern matches the nine-rule precedent of Phase 1+2.

### Negative

- Two anchor lookups for `INTRODUCTION` (one per rule). The cost is negligible: a linear scan of body paragraphs on documents with ~50–500 paragraphs.

### Risks

- A future rule could place a transient state into `FormattingContext` that one of these two rules accidentally invalidates. **Mitigation**: neither rule writes to `FormattingContext`; they only read `SectionParagraph`, `TitleParagraph`, `DoiParagraph` for skip-list purposes.

## Implementation Notes

- Register both rules in DI, in order, after `ExtractCorrespondingAuthorRule` (current rule #9).
- `MoveHistoryRule.Apply()` must early-return when the three history paragraphs are already immediately above `INTRODUCTION` (idempotent no-op).
- Each rule extends the existing `formatting` object inside the diagnostic JSON; neither replaces or removes existing keys.

## References

- [PRD: Section Formatting and History Move](../_prd.md)
- [ADR-002: Strict content preservation invariant (INV-01)](adr-002-content-preservation-invariant.md)
- [ADR-003: Discard font size from detection predicate](adr-003-discard-font-size-from-detection.md)
- [ADR-004: INTRODUCTION as detection anchor](adr-004-introduction-as-detection-anchor.md)
- Existing precedent: nine `IFormattingRule` classes in `DocFormatter.Core/Rules/`.
