# TechSpec: Section Formatting and History Move (DocFormatter Phase 3)

## Executive Summary

Phase 3 introduces two `IFormattingRule` implementations into the existing pipeline architecture: `MoveHistoryRule` (reorders the three article history paragraphs to immediately precede `INTRODUCTION`) and `PromoteSectionsRule` (mutates `<w:jc>` and `<w:sz>` on body paragraphs that match the section/sub-section predicate). Both rules are `Optional` severity, both anchor on the first body paragraph matching `^INTRODUCTION[\s.:]*$`, and both are bound by the strict content-preservation invariant (INV-01) defined in ADR-002.

A new internal static helper `BodySectionDetector` centralizes the anchor lookup and predicate logic, mirroring the existing `HeaderParagraphLocator` pattern. The detection predicate resolves `<w:b>` through the OOXML cascade chain (run → paragraph rPr → `pStyle` → `basedOn`) per ADR-005, achieving 11/11 coverage on the production corpus instead of the 9/11 a strict run-only check would deliver. The diagnostic JSON (`DiagnosticFormatting`) is extended with two additive record fields, populated by `DiagnosticWriter` from rule message constants — the same pattern already used by Phase 2 rules.

The primary technical trade-off: ~80 lines of cascade-walker code in `BodySectionDetector` are accepted in exchange for closing the 18% coverage gap on production articles where bold is set via `pStyle`. Synthetic OOXML fixtures cover all detection edge cases; no golden-file integration tests are introduced (per the testing decision in this TechSpec).

## System Architecture

### Component Overview

| Component | Type | Responsibility |
|---|---|---|
| `MoveHistoryRule` | new `IFormattingRule` (Optional, position #10) | Detect the three-paragraph history block; if `INTRODUCTION` anchor exists and the block is well-formed, reorder the three paragraphs to be immediate predecessors of the anchor. Idempotent on re-runs (early-returns on adjacency). |
| `PromoteSectionsRule` | new `IFormattingRule` (Optional, position #11) | From the `INTRODUCTION` anchor through end of body, mutate `<w:jc>` and font size on paragraphs matching the section predicate (16pt center) or sub-section predicate (14pt center). |
| `BodySectionDetector` | new internal static class | Shared detection helper: anchor lookup, section/sub-section predicates, OOXML bold cascade resolution, table-descendant filter. |
| `DiagnosticFormatting` | modified record (additive) | Gains `HistoryMove` and `SectionPromotion` properties of two new record types. |
| `DiagnosticWriter` | modified | Two new private build methods filter report entries by rule name and reconstruct the structured diagnostic objects from message constants. |
| `CliApp.BuildServiceProvider` | modified | Registers the two new rules at the end of the `IFormattingRule` registration block. |

Data flow: each rule reads the document body via `doc.MainDocumentPart.Document.Body`, consults `BodySectionDetector` for anchor and predicate checks, mutates the OOXML tree (reorder for `MoveHistoryRule`; property mutation for `PromoteSectionsRule`), and emits report entries via `IReport.Info/Warn`. After the pipeline completes, `DiagnosticWriter.Build` filters the report and produces the `DiagnosticDocument` written to `<file>.diagnostic.json`.

## Implementation Design

### Core Interfaces

`BodySectionDetector` is the central new type. It exposes pure functions over OOXML primitives:

```csharp
namespace DocFormatter.Core.Rules;

internal static class BodySectionDetector
{
    public static Paragraph? FindIntroductionAnchor(Body body, MainDocumentPart? mainPart);

    public static bool IsSection(Paragraph paragraph, MainDocumentPart? mainPart);

    public static bool IsSubsection(Paragraph paragraph, MainDocumentPart? mainPart);

    public static bool IsInsideTable(Paragraph paragraph);

    internal static bool IsBoldEffective(Run run, Paragraph paragraph, MainDocumentPart? mainPart);
}
```

Each rule follows the established `IFormattingRule` contract:

```csharp
public sealed class MoveHistoryRule : IFormattingRule
{
    public const string AnchorMissingMessage = "INTRODUCTION anchor not found — history move skipped";
    public const string AlreadyAdjacentMessage = "history already adjacent to INTRODUCTION — no-op";
    public const string MovedMessagePrefix = "history moved (3 paragraphs placed before INTRODUCTION at position ";
    public const string PartialBlockMessagePrefix = "history partial: ";
    public const string OutOfOrderMessagePrefix = "history out of order ";
    public const string NotAdjacentMessagePrefix = "history not adjacent ";
    public const string NotFoundMessage = "history block not found — nothing to move";

    public string Name => nameof(MoveHistoryRule);
    public RuleSeverity Severity => RuleSeverity.Optional;
    public void Apply(WordprocessingDocument doc, FormattingContext ctx, IReport report);
}
```

`PromoteSectionsRule` follows the same shape with its own message constants and a constructor taking no dependencies (the predicate logic is in `BodySectionDetector`, not configurable via `FormattingOptions`).

### Data Models

Two new records extend the diagnostic schema in `DiagnosticDocument.cs`:

```csharp
public sealed record DiagnosticHistoryMove(
    bool Applied,
    string? SkippedReason,
    bool AnchorFound,
    int? FromIndex,
    int? ToIndexBeforeIntro,
    int ParagraphsMoved);

public sealed record DiagnosticSectionPromotion(
    bool Applied,
    string? SkippedReason,
    bool AnchorFound,
    int? AnchorParagraphIndex,
    int SectionsPromoted,
    int SubsectionsPromoted,
    int SkippedParagraphsInsideTables,
    int SkippedParagraphsBeforeAnchor);
```

`DiagnosticFormatting` gains two nullable properties:

```csharp
public sealed record DiagnosticFormatting(
    DiagnosticAlignment? AlignmentApplied,
    DiagnosticAbstract? AbstractFormatted,
    bool? AuthorBlockSpacingApplied,
    DiagnosticCorrespondingEmail? CorrespondingEmail,
    DiagnosticHistoryMove? HistoryMove,
    DiagnosticSectionPromotion? SectionPromotion);
```

The trigger for emitting the JSON file remains `report.HighestLevel >= ReportLevel.Warn` (existing behaviour); both new objects are `null` when their rule emitted only `[INFO]` entries.

`FormattingContext` is **not** extended. Both rules call `BodySectionDetector.FindIntroductionAnchor` independently. Two linear scans of a 50–500-paragraph body cost microseconds and remove the need for shared state.

### API Endpoints

Not applicable. Phase 3 is internal pipeline logic; no public API surface is added.

## Integration Points

Not applicable. Phase 3 is fully internal to `DocFormatter.Core` and `DocFormatter.Cli`. No external services, files outside the existing input `.docx` and output JSON, or third-party libraries are involved.

## Impact Analysis

| Component | Impact Type | Description and Risk | Required Action |
|---|---|---|---|
| `DocFormatter.Core/Rules/MoveHistoryRule.cs` | new | New `IFormattingRule` class. Risk: paragraph reorder corrupts OOXML tree if pre-existing references to the moved paragraphs are held by Phase 1+2 rules. | Verify no `FormattingContext` reference points to a history paragraph (none of the current context fields target the history block — confirmed by inspection). |
| `DocFormatter.Core/Rules/PromoteSectionsRule.cs` | new | New `IFormattingRule` class. Risk: mutating `<w:jc>` on a paragraph that already has page-break or other layout properties could regress visual output. | Mutate only `<w:jc>` and run-level `<w:sz>`/`<w:szCs>`; never replace the entire `ParagraphProperties` element. |
| `DocFormatter.Core/Rules/BodySectionDetector.cs` | new | New static helper class. Risk: cascade walker bug returns wrong bold value, leading to false positives or false negatives. | Cascade walker has explicit cycle protection (HashSet) and depth limit (10); unit tests cover direct, single-level, two-level, and cyclic style chains. |
| `DocFormatter.Core/Reporting/DiagnosticDocument.cs` | modified (additive) | New record types and two new nullable properties on `DiagnosticFormatting`. Risk: external JSON consumers depend on the existing schema. | The change is additive: every existing field stays at the same JSON path. Property nulls keep the JSON omitting them when not relevant. Existing `Equals`/`GetHashCode` on `DiagnosticDocument` already includes `Formatting`; record auto-generation handles the new fields. |
| `DocFormatter.Core/Reporting/DiagnosticWriter.cs` | modified | New private methods `BuildHistoryMove` and `BuildSectionPromotion` filter report entries and reconstruct objects. Risk: a typo in a message constant breaks the diagnostic. | Both rules expose every emitted message as a `public const string`; `DiagnosticWriter` uses `nameof(Rule)` and the rule's constants by reference, never by literal. |
| `DocFormatter.Cli/CliApp.cs` | modified | Two new `services.AddTransient<IFormattingRule, ...>()` registrations at the end of the rule block. Risk: registration order accidentally placed before a Phase 1+2 rule, breaking anchor visibility. | Both registrations go after the existing `LocateAbstractAndInsertElocationRule` line; pipeline order matches PRD's stated #10/#11 positions. |
| `DocFormatter.Tests/` | new files | One xUnit fixture per rule plus one for `BodySectionDetector`. Risk: tests depend on implementation details that change. | Tests target observable behaviour (predicate true/false, paragraph order, mutated property values), not internal helpers. |

## Testing Approach

### Unit Tests

Per ADR-005 testing decision, **synthetic OOXML fixtures only**. No golden-file end-to-end tests with real `examples/*.docx`. Rationale: synthetic fixtures cover all branches deterministically and isolate failure modes; production validation is performed via manual editor review on the eleven articles after Phase 3 ships.

A new test fixture builder `Phase3DocxFixtureBuilder` (under `DocFormatter.Tests/Fixtures/Phase3/`) constructs in-memory `WordprocessingDocument` instances. The builder exposes parameters to:

- Place an `INTRODUCTION` paragraph at a configurable position.
- Insert a configurable number of "section" candidate paragraphs (caps, configurable bold cascade pattern).
- Insert a configurable number of "sub-section" candidate paragraphs (mixed-case, configurable bold cascade pattern).
- Insert a history block in three patterns: well-formed, partial, out-of-order, non-adjacent, missing.
- Place candidate paragraphs inside `<w:tbl>` to verify the table-descendant filter.
- Define paragraph styles with `basedOn` chains to verify cascade resolution (direct, 1-level, 2-level, cyclic, missing styles part).

**`BodySectionDetectorTests`** (new file):

- `IsBoldEffective` returns `true` for each of: run direct bold, paragraph `pPr/rPr/b`, single-level `pStyle`, two-level `pStyle` via `basedOn`.
- `IsBoldEffective` returns `false` for cyclic `basedOn` chains (no infinite loop, depth bounded).
- `IsBoldEffective` returns `false` when `MainDocumentPart.StyleDefinitionsPart` is null.
- `IsBoldEffective` respects `<w:b w:val="false"/>` as an explicit non-bold override.
- `IsSection` returns `true` only for paragraphs with all-caps text, ≥ 90% bold-effective characters, alignment ∈ {left, both, none}, and ≥ 3-character text.
- `IsSubsection` returns `true` only for paragraphs with at least one lower-case letter, otherwise identical to `IsSection`.
- `IsInsideTable` returns `true` for paragraphs that are descendants of `<w:tbl>` at any nesting depth.
- `FindIntroductionAnchor` returns the first paragraph matching `^INTRODUCTION[\s.:]*$` AND `IsSection`. Verified: `INTRODUCTION`, `INTRODUCTION:`, `INTRODUCTION ` accepted; `INTRODUCTION bla bla`, `INTRODUÇÃO`, `1. INTRODUCTION` rejected.

**`MoveHistoryRuleTests`** (new file):

- Happy path: well-formed three-paragraph block before `INTRODUCTION` is moved to immediately before `INTRODUCTION`. Assert paragraph count unchanged, multiset of body texts unchanged (INV-01), three history paragraphs are now immediate predecessors of `INTRODUCTION`.
- Idempotent: running the rule twice on the same fixture produces identical output. The second invocation emits `[INFO] AlreadyAdjacentMessage`.
- Anchor missing: emits `[WARN] AnchorMissingMessage`, document unchanged.
- Partial block (only `Received` and `Published`): emits `[WARN]` matching `PartialBlockMessagePrefix`, document unchanged.
- Out of order (`Published` before `Received`): emits `[WARN]` matching `OutOfOrderMessagePrefix`, document unchanged.
- Non-adjacent (a non-empty paragraph between markers): emits `[WARN]` matching `NotAdjacentMessagePrefix`, document unchanged.
- Not found (no history paragraphs): emits `[INFO] NotFoundMessage`, document unchanged.
- INV-01 assertion: in every test, `MultisetOfBodyTexts(before) == MultisetOfBodyTexts(after)`.

**`PromoteSectionsRuleTests`** (new file):

- Happy path with sections and sub-sections: each section paragraph after the anchor receives `<w:jc w:val="center"/>` and run `<w:sz w:val="32"/>`/`<w:szCs w:val="32"/>`; each sub-section receives `<w:jc w:val="center"/>` and `<w:sz w:val="28"/>`/`<w:szCs w:val="28"/>`.
- Paragraphs before the anchor are untouched (verified by snapshotting `<w:jc>` and `<w:sz>` of those paragraphs before/after).
- Paragraphs inside `<w:tbl>` are untouched even when their text matches the predicate.
- Paragraphs referenced by `FormattingContext.SectionParagraph`/`TitleParagraph`/`DoiParagraph` are untouched even when above the anchor (defence in depth).
- Anchor missing: emits `[WARN] AnchorMissingMessage`, no paragraphs mutated.
- Idempotent: running the rule twice produces identical output (re-applying the same `<w:jc>`/`<w:sz>` is a no-op).
- INV-01 assertion: in every test.

**`DiagnosticWriterTests`** is extended with cases that exercise `HistoryMove` and `SectionPromotion` reconstruction from synthesized report entries.

### Integration Tests

`CliIntegrationTests.cs` is extended with one new case: a synthetic fixture exercising both Phase 3 rules end-to-end alongside the Phase 1+2 pipeline. Assertions verify (a) the diagnostic JSON contains both new sections only when warnings fire; (b) the history block is placed correctly when an `INTRODUCTION` anchor exists; (c) the `Phase3DocxFixtureBuilder` output matches expected paragraph order.

The integration test does not load any `examples/*.docx` file; that validation is manual editor review post-merge.

## Development Sequencing

### Build Order

1. **`BodySectionDetector` skeleton** (no dependencies). Class file with method signatures, all returning `false`/`null`.
2. **`IsBoldEffective` cascade walker** (depends on step 1). Implement run direct check, paragraph rPr check, pStyle resolution with cycle protection and depth limit. Tests for all six cascade patterns.
3. **`IsInsideTable`, `IsSection`, `IsSubsection`** (depend on step 2). Predicate composition over `IsBoldEffective`. Tests for the 90% threshold, all-caps vs mixed-case, alignment filter.
4. **`FindIntroductionAnchor`** (depends on step 3). Linear scan + regex match + section predicate. Tests for accepted/rejected variants.
5. **`MoveHistoryRule`** (depends on step 4). Detect the three history paragraphs by regex; validate adjacency and order; if anchor exists, detach and re-insert before anchor; emit precise message constants. Tests for all skip conditions and idempotency.
6. **`PromoteSectionsRule`** (depends on steps 3 and 4). Locate anchor; iterate from anchor to end of body; for each non-table paragraph not in the context skip-list, apply section or sub-section formatting. Tests for in-scope and out-of-scope paragraphs, table filter, idempotency.
7. **`DiagnosticHistoryMove` and `DiagnosticSectionPromotion` records** (no dependencies). Add to `DiagnosticDocument.cs`.
8. **Extend `DiagnosticFormatting` record** (depends on step 7). Add `HistoryMove` and `SectionPromotion` nullable properties. Update `DiagnosticDocument.Equals`/`GetHashCode` if needed (record auto-handles).
9. **Extend `DiagnosticWriter.BuildFormatting`** (depends on steps 5, 6, 8). Add `BuildHistoryMove` and `BuildSectionPromotion` private methods. Filter by rule name; map message constants to record fields; null when no relevant entries.
10. **Register rules in `CliApp.BuildServiceProvider`** (depends on steps 5, 6). Add the two `services.AddTransient<IFormattingRule, ...>()` calls after `LocateAbstractAndInsertElocationRule`.
11. **Integration test in `CliIntegrationTests`** (depends on steps 5, 6, 9, 10). End-to-end run on a synthetic fixture with both rules engaged.

### Technical Dependencies

- `DocumentFormat.OpenXml` (already in solution) — required for `Paragraph`, `Run`, `RunProperties`, `Justification`, `FontSize`, `FontSizeComplexScript`, `WordprocessingDocument`, `MainDocumentPart`, `StyleDefinitionsPart`.
- `Microsoft.Extensions.DependencyInjection` (already in solution) — for the two new `AddTransient` registrations.
- xUnit (already in solution) — for test fixtures and assertions.
- No new packages, no new build targets, no infrastructure changes.

## Monitoring and Observability

The CLI surface is unchanged. Operational visibility for Phase 3 lives in two existing channels:

- **`<file>.report.txt`** — every rule emits structured `[INFO]`/`[WARN]` entries; the report writer prefixes each line with the level and rule name. Phase 3 entries appear chronologically alongside Phase 1+2 entries.
- **`<file>.diagnostic.json`** — when `report.HighestLevel >= Warn`, the JSON file is written with the full `DiagnosticFormatting` object including the two new sections. A batch consumer can grep for `formatting.history_move.applied=false` or `formatting.section_promotion.skipped_reason="anchor_missing"` to find papers that need manual review.

The batch summary file (`_batch_summary.txt`) — already produced by `FileProcessor` — does not change shape. The per-file row continues to show `✓`/`⚠`/`✗` based on `report.HighestLevel`.

No new metrics, log sinks, or alerting are added. Phase 3 is operationally indistinguishable from Phase 1+2 from the editor's perspective.

## Technical Considerations

### Key Decisions

- **Decision**: Centralize detection in `BodySectionDetector` static helper.
  - **Rationale**: matches the established `HeaderParagraphLocator` pattern; both new rules reuse anchor lookup and the cascade walker; testability of detection logic is decoupled from rule mutation logic.
  - **Trade-offs**: one new file; slight indirection from rule to helper.
  - **Alternatives rejected**: inline detection per rule (duplication; harder to test); extension methods on `Paragraph` (more idiomatic but inconsistent with existing project style).

- **Decision**: Resolve `<w:b>` via OOXML cascade (run → paragraph rPr → `pStyle` → `basedOn`).
  - **Rationale**: 2 of 11 production articles store bold via `pStyle` chains; without cascade resolution they lose all Phase 3 formatting.
  - **Trade-offs**: ~80 lines of cascade-walker code with cycle protection and depth limit.
  - **Alternatives rejected**: strict run-only (2/11 miss); run + paragraph rPr only (no improvement); naive "any pStyle ⇒ bold" (catastrophic false positives).
  - See [ADR-005](adrs/adr-005-bold-cascade-resolver.md).

- **Decision**: Two independent rules, no shared state in `FormattingContext`.
  - **Rationale**: the rules commute; anchor lookup costs microseconds even when run twice; introducing context fields couples the rules and complicates the order-of-execution invariants.
  - **Trade-offs**: two linear body scans per pipeline run.
  - **Alternatives rejected**: cache anchor in `FormattingContext.IntroductionParagraph` (couples rules; adds context field that other rules might mistakenly read).

- **Decision**: Synthetic OOXML fixtures only; no golden-file `examples/` integration tests.
  - **Rationale**: synthetic fixtures cover all branches deterministically and produce focused failure messages; the cost of curating eleven `expected.docx` files is high and brittle.
  - **Trade-offs**: real-world OOXML quirks not captured in tests until manual editor review.
  - **Alternatives rejected**: golden files for all 11 articles (high curation cost, brittle to OOXML noise); 2 golden files for run-direct + cascade (still requires manual `expected.docx` curation; covered by synthetic fixtures with the same precision).

- **Decision**: Set both `<w:sz>` and `<w:szCs>` when promoting size.
  - **Rationale**: matches `RewriteHeaderMvpRule.CreateBaseRunProperties` precedent; covers Latin and complex script ranges uniformly.
  - **Trade-offs**: none observed.

### Known Risks

- **Risk**: Bold inheritance via `<w:rStyle>` (character style) is not resolved by the cascade walker. **Likelihood**: low — empirical inspection of all 11 articles shows zero `<w:rStyle>` usage on section headings. **Mitigation**: add a follow-up ADR if a counter-example surfaces.
- **Risk**: `FormattingContext.SectionParagraph`/`TitleParagraph` references may be stale if Phase 1+2 mutated the document and dropped the original references. **Likelihood**: low — Phase 1+2 publishes the references after their last mutation. **Mitigation**: defence in depth — `PromoteSectionsRule` skips paragraphs by reference equality (`ReferenceEquals`) AND by anchor-scope position; either filter alone is sufficient.
- **Risk**: A document with `INTRODUCTION` declared inside a `<w:txbxContent>` (text box) or other non-body container would be missed. **Likelihood**: very low — editorial template uses standard body paragraphs. **Mitigation**: `BodySectionDetector.FindIntroductionAnchor` scans `body.Elements<Paragraph>()` (direct children) and `body.Iter<Paragraph>()` only via `IsInsideTable` for filtering. Text boxes are out of scope; anchor not found ⇒ both rules emit `[WARN] anchor_missing` and skip — a falha-segura outcome (INV-01).
- **Risk**: Performance degradation on very long documents (>1000 paragraphs) due to two linear scans plus per-paragraph cascade resolution. **Likelihood**: low — cascade resolution short-circuits on first hit (typically run-direct in 9/11 articles); body sizes empirically 50–500 paragraphs. **Mitigation**: monitor in production; introduce a per-pipeline anchor cache only if real measurements show it matters.

## Architecture Decision Records

- [ADR-001: Two discrete Optional rules over a single combined rule](adrs/adr-001-two-discrete-rules.md) — Implement `MoveHistoryRule` and `PromoteSectionsRule` as separate `IFormattingRule` siblings instead of one combined rule.
- [ADR-002: Strict content preservation invariant (INV-01)](adrs/adr-002-content-preservation-invariant.md) — Phase 3 cannot delete or hide text; `MoveHistoryRule` is the only rule that may reorder, and only the three history paragraphs.
- [ADR-003: Discard font size from detection predicate](adrs/adr-003-discard-font-size-from-detection.md) — `<w:sz>` is absent on most body paragraphs in the corpus due to OOXML cascade; size is not part of the section/sub-section predicate.
- [ADR-004: `INTRODUCTION` as detection anchor](adrs/adr-004-introduction-as-detection-anchor.md) — The first paragraph matching `^INTRODUCTION[\s.:]*$` and the section predicate is the positional anchor for both rules.
- [ADR-005: Resolve `<w:b>` via OOXML cascade chain](adrs/adr-005-bold-cascade-resolver.md) — Resolve bold through run → paragraph rPr → `pStyle` → `basedOn` to cover the 2 of 11 articles where bold is set via paragraph styles.
