# TechSpec: Phase-2 Tagging and Stage-1 Author Fixes

## Executive Summary

This feature delivers two streams of work in DocFormatter (.NET 10 / C#): (1) a fix to the existing Stage-1 author rule so the SciELO Markup Word plugin successfully auto-tags authors on articles 5313 and 5449; (2) a new "Phase 2" pipeline that pre-marks SciELO XML 4.0 tags (`[elocation]`, `[abstract]`, `[kwdgrp]`, author-block xrefs / corresp / `[authorid]`, `[hist]`) into the `.docx` body, reducing operator effort downstream. See `_prd.md` for business context.

The implementation reuses every existing pipeline abstraction (`IFormattingRule`, `FormattingPipeline`, `FormattingContext`, `IReport`, `DiagnosticWriter`) by registering a new rule set under `DocFormatter.Core/Rules/Phase2/` and dispatching it via two new hand-rolled CLI subcommands (`phase2`, `phase2-verify`). A small body-text-extraction diff utility validates each release against the curated `examples/phase-2/{before,after}/` corpus. The primary trade-off: by reusing `FormattingPipeline` and `FormattingContext` for both phases instead of building a parallel pipeline, we avoid duplicating ~200 lines of orchestration code at the cost of mild discipline (Phase 2 fields on `FormattingContext` must be optional and Phase-1-friendly).

## System Architecture

### Component Overview

| Component | Type | Purpose |
|-----------|------|---------|
| `DocFormatter.Core/Rules/Phase2/` (new folder) | New | Houses every `IFormattingRule` that emits a Phase 2 tag literal into the `.docx`. |
| `DocFormatter.Core/Models/Phase2/` (new folder) | New | Structured types crossing rule boundaries (Affiliation, CorrespAuthor, AbstractMarker, KeywordsGroup, HistoryDates). |
| `DocFormatter.Core/TagEmission/TagEmitter.cs` (new file) | New | Static helper that emits SciELO `[tag attr="v"]…[/tag]` literals as OpenXML Run/Text constructs, respecting the `Space=Preserve` invariant and the `markup_sup_as` superscript trap. |
| `DocFormatter.Core/Reporting/Phase2DiffUtility.cs` (new file) | New | Body-text extraction + scope-filtered string comparison for `phase2-verify`. |
| `DocFormatter.Core/Pipeline/RuleRegistration.cs` (new file) | New | Extension methods `AddPhase1Rules` and `AddPhase2Rules` on `IServiceCollection`. |
| `DocFormatter.Core/Rules/ExtractAuthorsRule.cs` | Modified | Phase 1 fix: adjust heuristic so SciELO Markup's `mark_authors` succeeds on 5313 and 5449. |
| `DocFormatter.Core/Pipeline/FormattingContext.cs` | Modified (sparingly) | Add only fields shared across Phase 2 rules. Each new field is nullable; Phase 1 rules ignore. |
| `DocFormatter.Cli/CliApp.cs` | Modified | Subcommand dispatcher at the top of `Run()`. New `RunPhase2` and `RunPhase2Verify` handlers. |
| `DocFormatter.Tests/Fixtures/Phase2/` | Extended | New fixture builders for keywords/abstract/hist/corresp/affiliation paragraphs. |
| `examples/phase-2/{before,after}/` | Read-only corpus | Validation gate input. 10 article pairs (5136, 5293, 5313, 5419, 5424, 5434, 5449, 5458, 5523, 5549). |

**Data flow per `phase2 <input>` invocation**:

```
CliApp.Run(["phase2", "input.docx"])
  └→ RunPhase2(input)
       └→ ServiceCollection.AddPhase2Rules() + AddCommonInfra()
            └→ FormattingPipeline.Run(doc, ctx, report)
                 ├→ EmitElocationTagRule.Apply(...)
                 ├→ EmitAbstractTagRule.Apply(...)        // uses TagEmitter
                 ├→ EmitKwdgrpTagRule.Apply(...)          // uses TagEmitter
                 ├→ EmitCorrespTagRule.Apply(...)         // uses TagEmitter, reads ctx.Authors
                 ├→ EmitAuthorXrefsRule.Apply(...)        // emits xref + authorid
                 └→ EmitHistTagRule.Apply(...)            // uses HistDateParser + TagEmitter
       └→ DiagnosticWriter.Write(...)
       └→ Save .docx + .report.txt + .diagnostic.json under <sourceDir>/formatted-phase2/
```

**Data flow per `phase2-verify <before> <after>`**:

```
CliApp.Run(["phase2-verify", "examples/phase-2/before", "examples/phase-2/after"])
  └→ RunPhase2Verify(before, after)
       └→ For each before/<id>.docx:
            ├→ Run Phase 2 pipeline → temp file
            ├→ Phase2DiffUtility.Compare(temp, after/<id>.docx, currentScope)
            │     ├→ extract body text (with [tag] literals) from each
            │     ├→ strip out-of-scope tags from after-text
            │     └→ string-compare; on mismatch return offset + context
            └→ print [PASS]/[FAIL] line
       └→ exit code 0 if all pass, 1 otherwise
```

## Implementation Design

### Core Interfaces

**`IFormattingRule`** (existing, unchanged — `DocFormatter.Core/Pipeline/IFormattingRule.cs:5-12`):

```csharp
public interface IFormattingRule
{
    string Name { get; }
    RuleSeverity Severity { get; }
    void Apply(WordprocessingDocument doc, FormattingContext ctx, IReport report);
}
```

Every Phase 2 emitter rule implements `IFormattingRule` with `Severity = Optional` (per ADR-002), returns a sensible `Name` (e.g., `"EmitAbstractTagRule"`), and applies `[tag]` literals via the new `TagEmitter` helper.

**`TagEmitter`** (new — `DocFormatter.Core/TagEmission/TagEmitter.cs`):

```csharp
public static class TagEmitter
{
    public static Run OpeningTag(string tagName, IReadOnlyList<(string Key, string Value)> attrs);
    public static Run ClosingTag(string tagName);
    public static void WrapParagraphContent(Paragraph paragraph, string tagName, IReadOnlyList<(string, string)> attrs);
    public static void InsertOpeningBefore(Paragraph anchor, string tagName, IReadOnlyList<(string, string)> attrs);
    public static void InsertClosingAfter(Paragraph anchor, string tagName);
}
```

Implementation contract:
- All emitted Runs use `RewriteHeaderMvpRule.CreateBaseRunProperties()` (existing helper) for consistent styling.
- All `Text` elements set `Space = SpaceProcessingModeValues.Preserve`.
- Attribute values are emitted with double quotes and no internal escaping (DTD 4.0 invariant from `docs/scielo_context/README.md`).
- When wrapping a paragraph that contains a superscript run, the helper zeroes `Run.RunProperties.VerticalTextAlignment` on the wrapped runs (per `markup_sup_as` invariant).

**`HistDateParser`** (new, Phase 4 only — `DocFormatter.Core/Rules/Phase2/HistDateParsing/HistDateParser.cs`):

```csharp
public static class HistDateParser
{
    public static HistDate? ParseReceived(string text);
    public static HistDate? ParseAccepted(string text);
    public static HistDate? ParsePublished(string text);
}

public sealed record HistDate(int Year, int? Month, int? Day, string SourceText)
{
    public string ToDateIso();
}
```

`ToDateIso()` returns `YYYYMMDD` with `00` padding when `Month` or `Day` is null. The parser is implemented Phase 4 per ADR-007 (rewrite from scratch, not a copy).

**`Phase2DiffUtility`** (new — `DocFormatter.Core/Reporting/Phase2DiffUtility.cs`):

```csharp
public static class Phase2DiffUtility
{
    public static DiffResult Compare(string producedDocxPath, string expectedDocxPath, IReadOnlyCollection<string> inScopeTags);
}

public sealed record DiffResult(bool IsMatch, int? FirstDivergenceOffset, string? ProducedContext, string? ExpectedContext);
```

### Data Models

New types under `DocFormatter.Core/Models/Phase2/` (introduced as needed, not all up front):

```csharp
public sealed record Affiliation(string Label, string Orgname, string? Orgdiv1, string? Country);
public sealed record CorrespAuthor(int AuthorIndex, string? Email, string? Orcid);
public sealed record HistoryDates(HistDate? Received, IReadOnlyList<HistDate> Revised, HistDate? Accepted, HistDate? Published);
public sealed record KeywordsGroup(string Language, IReadOnlyList<string> Keywords);
public sealed record AbstractMarker(string Language, Paragraph BodyParagraph);
```

`FormattingContext` extension (only when shared across Phase 2 rules):

```csharp
// Added to FormattingContext.cs
public IReadOnlyList<Affiliation>? Affiliations { get; set; }
public CorrespAuthor? CorrespAuthor { get; set; }
public KeywordsGroup? Keywords { get; set; }
public AbstractMarker? Abstract { get; set; }
public HistoryDates? History { get; set; }
```

Each field is nullable. A rule that needs the field but finds it null calls `report.Warn(…)` and returns (per ADR-002).

`DiagnosticDocument` extension: add an optional `Phase2` block parallel to the existing `Formatting`:

```csharp
public sealed record DiagnosticPhase2(
    DiagnosticField Elocation,
    DiagnosticField Abstract,
    DiagnosticField Keywords,
    DiagnosticField Corresp,
    DiagnosticField Hist);
```

`DiagnosticDocument` gains an optional `DiagnosticPhase2? Phase2` field. Phase 1 invocations leave it null; Phase 2 invocations populate it.

### API Endpoints

Not applicable. DocFormatter is a CLI tool, not a service.

**CLI surface**, per ADR-005:

| Command                                            | Behavior                                                                                                  | Exit code              |
|----------------------------------------------------|------------------------------------------------------------------------------------------------------------|------------------------|
| `docformatter <input>`                             | Phase 1 (existing). Output → `<sourceDir>/formatted/`.                                                     | 0 ok / 1 fatal         |
| `docformatter phase2 <input>`                      | Phase 2 pipeline. Output → `<sourceDir>/formatted-phase2/`.                                                | 0 ok / 1 fatal         |
| `docformatter phase2-verify <before> <after>`      | Run Phase 2 over each `<before>/*.docx`; diff against `<after>/*.docx`; print pass/fail + first divergence. | 0 all pass / 1 any fail|

`<input>` can be a single `.docx` or a directory (mirrors existing `RunSingleFile`/`RunBatch` split).

## Impact Analysis

| Component | Impact Type | Description and Risk | Required Action |
|-----------|-------------|----------------------|------------------|
| `DocFormatter.Core/Rules/ExtractAuthorsRule.cs` | Modified | Adjust heuristic to fix Markup auto-mark on 5313 and 5449. **Risk**: regression on currently-passing articles. **Mitigation**: regression-run the existing rule's unit tests + new fixture cases. | Phase 1 implementation. |
| `DocFormatter.Core/Pipeline/FormattingContext.cs` | Modified | Add 5 nullable Phase 2 fields. **Risk**: low — additive only. **Mitigation**: nullable annotations enforced by `TreatWarningsAsErrors`. | Add fields incrementally as Phases 2-4 land. |
| `DocFormatter.Core/Reporting/DiagnosticDocument.cs` | Modified | Add optional `Phase2` block. **Risk**: schema consumers (humans + future tooling) need to handle the new field. **Mitigation**: nullable + JSON property naming follows existing camelCase convention. | Per Phase 2 release. |
| `DocFormatter.Cli/CliApp.cs` | Modified | Add subcommand dispatcher + `RunPhase2`/`RunPhase2Verify` handlers. **Risk**: an input named `phase2` could collide with subcommand. **Mitigation**: dispatcher checks file/dir existence on the token before treating as subcommand (ADR-005). | Phase 2 release. |
| `DocFormatter.Core/Rules/Phase2/*.cs` | New | 5-7 emitter rules. **Risk**: anti-duplication invariants — pre-marking the wrong tag breaks Markup. **Mitigation**: scope strictly to ADR-001 list; unit tests assert no `[author]`/`[fname]`/`[surname]`/`[kwd]` emission. | Per release in rollout order. |
| `DocFormatter.Core/TagEmission/TagEmitter.cs` | New | Centralizes literal emission. **Risk**: bugs here propagate to every Phase 2 rule. **Mitigation**: dense unit-test coverage, every public method tested. | Phase 2 (first release). |
| `DocFormatter.Core/Reporting/Phase2DiffUtility.cs` | New | Diff gate. **Risk**: false positives from whitespace; false negatives from over-eager out-of-scope strip. **Mitigation**: corpus dry-run before each release; manual inspection of first failing pair. | Phase 2 (first release; reused thereafter). |
| `DocFormatter.Tests/Fixtures/Phase2/*.cs` | New / extended | Builder methods for keywords/abstract/hist paragraphs. **Risk**: low. | Per release. |
| `examples/phase-2/{before,after}/` | Read-only | Corpus for diff gate. **Risk**: corpus drift if articles get hand-edited. **Mitigation**: treat as immutable per release; updates require an explicit commit + release-note. | Used by `phase2-verify`. |
| `Makefile` | Modified | Add `make phase2 FILE=...` and `make phase2-verify` targets. **Risk**: low. | Phase 2 release. |

## Testing Approach

### Unit Tests

Pattern follows existing conventions (xUnit, `Phase2DocxFixtureBuilder`-style in-memory `.docx` synthesis):

- **TagEmitter**: covers `OpeningTag`/`ClosingTag`/`WrapParagraphContent`/`InsertOpeningBefore`/`InsertClosingAfter`. Asserts: attribute serialization, `Space=Preserve`, RunProperties correctness, superscript-zero behavior. ~10-15 tests.
- **Each Phase 2 emitter rule**: golden-path test (recognized input → expected tag literals in expected position) + skip-and-warn test (unrecognized input → no tag emitted, `IReport` records a warning entry with rule name and reason code). 2-4 tests per rule.
- **HistDateParser**: TDD-derived test inventory (one test per recognized phrase shape). Per ADR-007, the test suite is written before the parser code.
- **Phase2DiffUtility**: tag-extraction correctness (with/without out-of-scope tags), whitespace normalization, scope filtering. ~6 tests.
- **ExtractAuthorsRule (modified for Phase 1 fix)**: regression test for the existing rule + new fixture covering the 5313/5449 failure shape.

Mock requirements: none for the rule layer (uses real OpenXML in-memory). `IReport` is real; tests inspect its accumulated entries.

### Integration Tests

- **Corpus integration test (per release)**: a single xUnit test (`Phase2CorpusTests.AllPairsMatch`) iterates `examples/phase-2/before/*.docx`, runs the Phase 2 pipeline, calls `Phase2DiffUtility.Compare` against `after/<id>.docx` with the current release's scope. Asserts all 10 pairs pass. The same test runs in CI and via `make phase2-verify`.
- **Phase 1 regression test**: ensure the existing `examples/*.docx` golden outputs remain unchanged after the `ExtractAuthorsRule` fix.

Test data: `examples/phase-2/{before,after}/` (corpus); fixtures via `DocFormatter.Tests/Fixtures/Phase2/` (synthetic).

Environment: pure xUnit — no Word, no SciELO Markup, no OS-specific dependencies.

## Development Sequencing

### Build Order

1. **Phase 1 author fix** — root-cause investigation, ADR-008 (to be created during implementation), modification to `ExtractAuthorsRule`. Validation: SciELO Markup runs cleanly on 5313 and 5449 post-fix. *No dependencies on later steps.*
2. **`TagEmitter` helper** — implement `DocFormatter.Core/TagEmission/TagEmitter.cs` with full unit tests. *Depends on step 1 (no overlap, but Phase 1 should ship first per ADR-001 rollout).*
3. **`Phase2DiffUtility`** — implement `DocFormatter.Core/Reporting/Phase2DiffUtility.cs` with unit tests. *Depends on step 2 only for project layout; functionally independent.*
4. **DI extension methods** — `RuleRegistration.cs` with `AddPhase1Rules`, `AddPhase2Rules` (the Phase 2 set is empty initially; rules added in subsequent steps). *Depends on step 2 (collocates with Phase 2 infrastructure).*
5. **CLI subcommand dispatcher** — `CliApp.cs` modifications: `RunPhase2`, `RunPhase2Verify`, subcommand routing. *Depends on steps 3 and 4.*
6. **First Phase 2 emitter rules (Easy tags release)** — `EmitElocationTagRule`, `EmitAbstractTagRule`, `EmitKwdgrpTagRule` plus their fixtures and unit tests. *Depends on steps 2, 4.*
7. **Easy-tags release validation** — corpus integration test + Phase 2 release scope constants set to `["elocation", "abstract", "kwdgrp"]`. *Depends on steps 5, 6.*
8. **Author-block emitter rules (Medium tags release)** — `EmitCorrespTagRule`, `EmitAuthorXrefsRule` with fixtures and unit tests. May introduce `Affiliation`/`CorrespAuthor` records in `Models/Phase2/`. *Depends on steps 6, 7 (sequencing per ADR-001 rollout).*
9. **Medium-tags release validation** — extend release scope, run corpus diff gate. *Depends on step 8.*
10. **`HistDateParser` (TDD)** — write phrase-inventory unit tests, then implement parser. Per ADR-007. *Depends on no earlier step technically; queued behind 9 per ADR-001.*
11. **`EmitHistTagRule` (Hard tags release)** — Phase 4 emitter. Unit tests + corpus integration. *Depends on steps 8, 10.*
12. **Hard-tags release validation** — full corpus passes with cumulative scope `["elocation", "abstract", "kwdgrp", "corresp", "xref", "authorid", "hist"]`. *Depends on step 11.*

### Technical Dependencies

Blocking dependencies external to the team / repository:

- None. All work is local code in this repository.

Internal dependencies between rules:

- `EmitCorrespTagRule` reads `ctx.Authors` and `ctx.CorrespondingEmail`/`CorrespondingOrcid`/`CorrespondingAuthorIndex`, which are populated by the **Phase 1** `ExtractAuthorsRule` and `ExtractCorrespondingAuthorRule`. Phase 2 invocations therefore assume the input `.docx` was produced by Phase 1. The CLI documents this; if Phase 2 is run on a non-Phase-1 input, the Phase 2 rules skip-and-warn.
- `EmitAuthorXrefsRule` reads `ctx.Authors[].AffiliationLabels` (already extracted by `ExtractAuthorsRule`). Same dependency as above.

## Monitoring and Observability

DocFormatter has no live-running service component. "Observability" reduces to per-run artifacts:

- **Structured log**: existing `_app.log` (Serilog file sink in `examples/formatted/_app.log`). Phase 2 invocations write to `examples/formatted-phase2/_app.log` (parallel structure). Each rule logs: rule name, severity outcome, time spent (if material).
- **Per-run report**: `<input>.report.txt` (existing, human-readable summary).
- **Per-run diagnostic JSON**: `<input>.diagnostic.json` with the new optional `Phase2` block. Operator reads this to see which Phase 2 tags were emitted and which were skipped with reasons.
- **Batch summary**: `_batch_summary.txt` (existing, generated by `RunBatch`). Will be extended to include a Phase-2-specific row when `phase2` subcommand processes a directory.

There are no alerting thresholds (no online service). The relevant operational signal is the `phase2-verify` exit code at release time.

## Technical Considerations

### Key Decisions

- **Pipeline organization**: reuse `FormattingPipeline` + `IFormattingRule` for Phase 2; rules in `Rules/Phase2/`; DI selects rule sets. Rationale: 100% reuse of stable orchestration code; trade-off is shared `FormattingContext` requires discipline. Alternative rejected: a parallel `Phase2Pipeline` with its own interface — duplicates orchestration with no architectural payoff (ADR-004).
- **CLI dispatch**: extend the hand-rolled parser with subcommand dispatch (`phase2`, `phase2-verify`); preserve existing default behavior. Rationale: zero new dependencies, scales to more Phase 2 operations. Alternative rejected: adopt `System.CommandLine` — scope creep that delays MVP (ADR-005).
- **Diff utility**: body-text extraction with regex-based out-of-scope tag stripping. Rationale: ~100 lines, no new deps, "good enough" for a 10-pair corpus. Alternative rejected: structured tag-tree parser — diminishing returns at this corpus size (ADR-006).
- **Phase 4 date-parser**: rewrite from scratch, using `Marcador_de_referencia` as algorithmic reference only. Rationale: avoids dragging foreign conventions; idiomatic DocFormatter from line 1. Alternative rejected: direct file copy — adaptation overhead larger than rewrite (ADR-007).

### Known Risks

- **Risk — Phase 1 fix introduces regression on currently-passing articles**: any change to `ExtractAuthorsRule` could break the 9 articles currently extracted with high confidence.
  **Mitigation**: rerun the entire Phase 1 unit test suite + run Phase 1 over `examples/*.docx` and compare diagnostic JSON output to a saved snapshot.
- **Risk — `TagEmitter` mishandles superscript runs and trips `markup_sup_as`**: per `docs/scielo_context/REENTRANCE.md`, pre-marked `[label]` over a superscript needs `Font.Superscript = false` zeroed.
  **Mitigation**: dedicated unit test for `WrapParagraphContent` over a superscript paragraph; assert post-condition.
- **Risk — `Phase2DiffUtility` produces false positives from whitespace**: OpenXML insertion creates incidental space differences.
  **Mitigation**: aggressive whitespace normalization (collapse, trim per-paragraph). Tighten case-by-case as failures arise.
- **Risk — Out-of-scope tag stripping over-strips a tag with unusual attributes**: regex-based stripping has edge cases.
  **Mitigation**: SciELO bracket syntax is tightly constrained. Validate against the corpus at every release.
- **Risk — Corpus is too small (10 pairs)**: real-world articles may exhibit patterns not represented.
  **Mitigation**: corpus is additive. New article patterns get added as new pairs and become part of the gate (per ADR-003).
- **Risk — `HistDateParser` rewrite drifts from `AccessedOnHandler.cs` behavior**: subtle date-edge-case differences.
  **Mitigation**: TDD with phrase inventory derived from the original (per ADR-007).

### Open Questions

- **Q1 — Output directory naming**: `formatted-phase2/` vs `phase2-output/` vs `formatted/phase2/`. Defaulting to `formatted-phase2/` (parallel to existing `formatted/`). Trivial to revisit.
- **Q2 — `RuleRegistration` location**: `DocFormatter.Cli` (where DI is composed today) vs `DocFormatter.Core` (where rules live). Slightly favors `DocFormatter.Core` for testability, but this is a trivial decision deferable to implementation.
- **Q3 — `_batch_summary.txt` schema for Phase 2**: format reuse vs new schema. Resolve at Phase 2 implementation.
- **Q4 — Phase 1 author fix root cause**: not yet known. Discovered during Phase 1 implementation. Will be captured in **ADR-008** at that time.

## Architecture Decision Records

ADRs documenting decisions made during PRD brainstorming and technical design:

- [ADR-001: Rollout Strategy — Help SciELO Markup, Don't Replace It](adrs/adr-001.md) — Adopt incremental rollout (Phase 1: author fix → Phase 4: hist); reject big-bang and full-Markup-replacement alternatives.
- [ADR-002: Failure Policy for Phase 2 Rules — Skip and Warn](adrs/adr-002.md) — When a heuristic cannot identify its target with high confidence, skip the tag and record a structured warning in `diagnostic.json`; never abort, never emit partial markup.
- [ADR-003: Diff-Based Validation Gate Using `examples/phase-2/{before,after}/`](adrs/adr-003.md) — Each release passes when its in-scope tags match the curated `after/` corpus across all 10 pairs.
- [ADR-004: Pipeline Organization — Reuse `FormattingPipeline` with DI-Selected Rule Sets](adrs/adr-004.md) — Phase 2 reuses `IFormattingRule`/`FormattingPipeline`/`FormattingContext`; rules live in `Rules/Phase2/`; DI extension methods select rule sets.
- [ADR-005: CLI Dispatch — Hand-Rolled Subcommands `phase2` and `phase2-verify`](adrs/adr-005.md) — Extend the existing hand-rolled parser with subcommand dispatch; reject `System.CommandLine` migration as scope creep.
- [ADR-006: Diff Utility — Body-Text Extraction with Out-of-Scope Tag Stripping](adrs/adr-006.md) — Plain-text extraction preserving SciELO `[tag]` literals; regex-based out-of-scope strip on the `after/` side; first-divergence reporting.
- [ADR-007: Phase 4 Date-Parser Port — Rewrite from Scratch Using `Marcador_de_referencia` as Reference](adrs/adr-007.md) — Implement `HistDateParser` from scratch in DocFormatter conventions; use the original repo as algorithmic inventory only.

ADR-008 will be created during Phase 1 implementation to document the root cause of the SciELO Markup author auto-mark failure on articles 5313 and 5449.
