# ADR-002: Structural-italic stripping heuristic for the abstract body

## Status

Accepted

## Date

2026-05-07

## Context

The input abstract paragraph arrives as `*Abstract - body...*` with italic applied to every run (the structural wrapper). The output requires the body in plain text **with intentional internal italic preserved** (e.g., `*Aedes aegypti*`).

Two run-level layouts are possible after parsing:

1. **Uniform italic** — every run with non-whitespace content carries `<w:i/>`. This is the wrapper the user wants removed.
2. **Mixed italic** — some runs italic (intentional emphasis), some not. The non-italic runs prove the wrapper is not what marks species names.

A naive strategy (always strip italic from all runs) flattens species-name emphasis. A naive strategy (never strip italic) leaves the visible wrapper. The PRD documents the trade-off as a known limitation: a pathological abstract whose author italicized 100% of the body for emphasis would lose that emphasis under any all-or-nothing rule.

## Decision

`RewriteAbstractRule` decides per-paragraph as follows, after splitting the heading from the body:

1. Walk the body's runs. For each run with non-whitespace text, record whether the run-level italic property (`<w:i/>` or inherited via the run-properties chain — read by `RunProperties.Italic`) is set to `true`.
2. **If every** non-whitespace-bearing run is italic, treat the italic as the structural wrapper:
   - Strip italic from every run (set `Italic.Val = false` or remove the `Italic` element entirely).
   - Emit `[INFO]` ("structural italic wrapper removed from abstract body").
3. **If at least one** non-whitespace-bearing run is not italic, leave run-level italic settings untouched. Internal italic remains exactly as authored.
4. Whitespace-only runs (e.g., a single space between two text runs) do not influence the decision; their italic property is preserved either way.

The rule does not introspect the run's text content (no species-name detection). The only signal is the italic distribution across the body.

## Alternatives Considered

### Alternative 1: Always strip italic from the entire body

- **Description**: After detaching the heading, set `Italic = false` on every body run.
- **Pros**: Simplest implementation; deterministic.
- **Cons**: Destroys species-name emphasis (a real and frequent pattern in the corpus).
- **Why rejected**: The PRD explicitly requires intentional internal italic to survive.

### Alternative 2: Always preserve run-level italic (do nothing)

- **Description**: Only rewrite the heading line; leave the body's italic distribution as-is.
- **Pros**: Zero risk of stripping intentional emphasis.
- **Cons**: Leaves the structural wrapper visible — the user sees the abstract still rendered fully italic. The PRD's primary goal is not met.
- **Why rejected**: Defeats the feature.

### Alternative 3: Per-run heuristic based on text content (e.g., italicize only Latin binomials)

- **Description**: Detect species-name patterns in italic runs and preserve only those.
- **Pros**: Highest accuracy in the common case.
- **Cons**: Adds NLP-level heuristics (Latin binomial detection, gene-name detection, abbreviation detection) far outside the scope of an Optional formatting rule. Brittle and would need a curated dictionary.
- **Why rejected**: Drastic complexity for marginal gain over the chosen heuristic.

### Alternative 4: Compare against the heading run's italic (if heading was italic, strip body italic)

- **Description**: Use the wrapper italic on `Abstract` itself as the trigger for stripping body italic.
- **Pros**: Simple boolean check.
- **Cons**: The MVP-rewritten heading is a freshly built run; the original heading run may have been merged with surrounding text or carried different formatting. Coupling body behavior to a single heading-run inspection is fragile and opaque.
- **Why rejected**: Indirect signal; the per-run uniformity check is direct and equivalent.

## Consequences

### Positive

- Predictable, testable rule with two clear branches.
- The 9/11 papers with the canonical `*Abstract - ...*` shape are fully de-italicized.
- Papers whose authors italicized only species names retain their intent.
- The rule never depends on text content; locale-neutral.

### Negative

- A pathological paper with 100% intentional italic in the body (no non-italic runs) is misclassified and gets flattened.
  - Mitigation: emit `[INFO]` whenever stripping happens, so the editor can spot-check.
- Authors who italicize *every word* of a sentence containing a species name (e.g., a quoted Latin sentence) trigger the stripping path.

### Risks

- **Risk**: A run may carry italic via paragraph-level or style-level properties rather than run-level `<w:i/>`. Reading only the run-level property would miscount.
  - **Mitigation**: Use the OpenXML SDK's effective-italic resolution if available, or document that the heuristic operates on run-level italic only and accept the rare style-driven false negative. Phase 1 starts with run-level reading; if a fixture exposes a style-driven case, extend then.
- **Risk**: Mixed-italic detection counts whitespace runs and triggers the wrong branch.
  - **Mitigation**: Only consider runs whose text contains at least one non-whitespace character.

## Implementation Notes

- The check lives inside `RewriteAbstractRule.Apply` as a private helper `bool BodyItalicIsStructuralWrapper(Paragraph body)`.
- The italic property is read via `Run.RunProperties?.Italic` and treated as `true` when the element exists with no `Val` or with `Val == OnOffOnlyValues.On` (OpenXML's omitted-attribute default).
- Stripping is done by removing the `Italic` element from each run's `RunProperties`. The element is not set to `false` because in OOXML the default is "off" when absent.
- The rule never introspects text length, language, or character class.

## References

- [PRD: Header Formatting Polish](../_prd.md) — Risks section: "A paper italicizes 100% of the abstract body intentionally"
- [ADR-001: Four discrete Optional rules](adr-001-four-discrete-rules.md)
