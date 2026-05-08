# ADR-005: Resolve `<w:b>` via OOXML cascade chain (supersedes the run-only stance from ADR-003)

## Status

Accepted

## Date

2026-05-07

## Context

ADR-003 specified that section/sub-section detection inspects only the run's own `<w:rPr><w:b/>` element, citing implementation simplicity and an empirical sample of three articles where direct run-level bold was sufficient. The TechSpec phase widened the empirical check to all eleven articles in `examples/` and produced a different picture.

The bold attribute is set in three different layers across the corpus:

| Pattern | Articles | Layer where `<w:b>` is found |
|---|---|---|
| Direct on run | 9/11 (`1`, `2`, `3`, `4`, `5`, `7`, `8`, `9`, `11`) | `<w:r><w:rPr><w:b/>` |
| Via `<w:pStyle>`, single inheritance | 1/11 (`6_AR_5523_2`) | `<w:pStyle w:val="Ttulo1">` → style `Ttulo1` has `<w:b/>` |
| Via `<w:pStyle>`, two-level inheritance | 1/11 (`10_AR_5549_2`) | `<w:pStyle w:val="Ttulo">` → `Ttulo` has no `<w:b/>` → `basedOn="SemEspaamento"` → `SemEspaamento` has `<w:b/>` |

The strict run-only rule fails on **2 of 11 articles** (~18% of the production corpus). For both articles, the predicate misses the `INTRODUCTION` anchor, both rules emit `[WARN] anchor_missing`, and Phase 3 produces no formatting at all — a 100% loss of Phase 3 value on those articles, even though INTRODUCTION is visually bold to the editor.

The detection predicate must answer "is this run's text rendered bold?" rather than "is `<w:b>` set directly on this run?". OOXML resolves bold via a cascade: run rPr → paragraph default rPr (`<w:pPr><w:rPr>`) → applied paragraph style (`<w:pStyle>`) → `basedOn` ancestor styles up to `Normal` → document defaults (`<w:docDefaults>`).

## Decision

Replace the run-only check with an effective-bold resolver. Implement an internal helper `IsBoldEffective(Run run, Paragraph paragraph, MainDocumentPart mainPart)` that resolves the cascade in this order:

1. **Run direct**: if `run.RunProperties?.Bold` is set, return its boolean value (the OOXML convention is `val` absent or not in `{"0", "false"}` ⇒ `true`).
2. **Paragraph default**: if the parent paragraph has `pPr/rPr/b`, return its value.
3. **Paragraph style chain**: if `pPr/pStyle/@val` references a style, walk the style and its `basedOn` ancestors looking for `<w:b/>`:
   - First in the style's `<w:rPr><w:b/>`.
   - Then in the style's `<w:pPr><w:rPr><w:b/>`.
4. **No bold found**: return `false`.

The 90% bold-character threshold (FormattingRule predicate) is computed using `IsBoldEffective` per run, weighted by non-whitespace character count.

The cascade walker protects against:

- **Cycles** (a corrupt `basedOn` pointing at a style that points back): a `HashSet<string>` of visited style IDs short-circuits any revisit.
- **Unbounded depth**: a hard limit of 10 hops aborts the walk and returns `false` (no real-world style chain exceeds 4–5 levels).
- **Missing styles part**: if `mainPart.StyleDefinitionsPart` is null or the referenced style is not found, the walk stops and returns `false` (graceful degradation, never throws).

`<w:docDefaults>` is intentionally **not** consulted. Bold is essentially never set in `docDefaults` in real-world Word output (it would make the entire document bold by default, which is not a valid editorial pattern). Empirical inspection of the eleven articles confirms `docDefaults/rPrDefault` never declares bold.

## Alternatives Considered

### Alternative 1: Maintain run-only (ADR-003 unchanged)

- **Description**: Keep the strict run-only predicate and accept that articles 6 and 10 skip Phase 3 with `[WARN] anchor_missing`.
- **Pros**:
  - No new code.
  - Predicate is local to the paragraph (no styles part dependency).
- **Cons**:
  - 2 of 11 production articles lose all Phase 3 formatting.
  - Editor manually formats those two articles, defeating the goal of zero-touch Phase 3.
- **Why rejected**: The 18% miss rate is significant and avoidable. The fail-safe behaviour (`anchor_missing` skip) preserves content but does not deliver value. Implementing the cascade is bounded in scope and one-time.

### Alternative 2: Run-direct + paragraph default rPr only (no style resolution)

- **Description**: Check run `rPr/b` and paragraph `pPr/rPr/b`, but do not resolve `pPr/pStyle`.
- **Pros**:
  - Simpler than the full cascade.
  - No styles part dependency.
- **Cons**:
  - Empirically does **not** improve coverage over run-only on this corpus: both problem articles have empty `pPr/rPr` and rely entirely on `pStyle` for bold.
- **Why rejected**: The simplification provides zero coverage gain on the actual data. There is no reason to prefer it over either Alternative 1 (no code) or the chosen Alternative 0 (full cascade).

### Alternative 3: Treat any paragraph with `<w:pStyle>` set as "potentially bold"

- **Description**: Skip cascade resolution; if a paragraph has any `pStyle`, assume bold for predicate purposes.
- **Pros**:
  - Trivial implementation.
- **Cons**:
  - Massive false-positive risk: `pStyle="Normal"`, `pStyle="Body Text"`, `pStyle="Header"`, `pStyle="Footer"` would all be treated as section candidates.
  - Would catastrophically reformat body paragraphs that happen to use a custom style.
- **Why rejected**: Unsafe. Violates INV-01's spirit (do not act under uncertainty).

## Consequences

### Positive

- **Coverage**: 11 of 11 articles in `examples/` resolve bold correctly; Phase 3 formatting fires on every article that has an `INTRODUCTION` anchor.
- **Correctness**: the predicate now answers a question that maps to what the editor sees in Word, not an OOXML-internal accident of authoring.
- **Reusability**: `IsBoldEffective` is also useful for `MoveHistoryRule` if a future change adds bold-aware history detection (currently history paragraphs are matched only by text prefix, so this does not bite immediately).

### Negative

- **Implementation cost**: ~80 lines of code for the cascade walker plus tests.
- **New dependency** on `MainDocumentPart.StyleDefinitionsPart`. The helper must accept a nullable styles part and degrade gracefully when absent. Phase 3 never throws if styles are missing.
- **Test surface**: cascade behaviour requires fixture documents that exercise (a) direct run bold, (b) paragraph rPr bold, (c) one-level pStyle inheritance, (d) two-level pStyle inheritance, (e) cycle protection, (f) missing styles part. Synthetic OOXML fixtures cover all six.

### Risks

- **Risk**: A future Word version introduces a new cascade layer (e.g., character styles via `<w:rStyle>`). **Mitigation**: this ADR documents the layers we resolve. A new layer would require an ADR-NNN extension. The safe default of `false` ensures content preservation regardless.
- **Risk**: A document declares bold via `<w:docDefaults>`, which we intentionally skip. **Mitigation**: empirical evidence shows this does not occur. If reported, ADR-005 can be extended with one more cascade layer.

## Implementation Notes

- The helper lives in `BodySectionDetector` (the static helper for Phase 3 detection logic) as `internal static bool IsBoldEffective(Run run, Paragraph paragraph, MainDocumentPart? mainPart)`.
- Callers pass `doc.MainDocumentPart` from the `Apply()` method's `WordprocessingDocument doc` parameter.
- `<w:b>` semantics: the element's existence with no `val` attribute means bold; `val="true"` or `val="1"` means bold; `val="false"` or `val="0"` means explicitly not bold (an override that disables bold inherited from a higher layer).
- Cycle protection uses a `HashSet<string>` of style IDs visited during a single resolution call. Cross-paragraph state is not retained.
- The depth limit of 10 hops is defensive; real Word documents have chains of 1–4 levels.

## References

- [PRD: Section Formatting and History Move](../_prd.md)
- [ADR-003: Discard font size from detection predicate](adr-003-discard-font-size-from-detection.md) — superseded only on the bold-detection portion; the size-discard decision remains in force.
- ECMA-376 §17.7.3 (Style Hierarchy), §17.7.5 (`basedOn` resolution)
- Empirical findings: cascade traces for articles `6_AR_5523_2` (1-level: `Ttulo1` → `Normal`) and `10_AR_5549_2` (2-level: `Ttulo` → `SemEspaamento` → `Normal`).
