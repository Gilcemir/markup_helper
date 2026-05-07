# TechSpec: Header Formatting Polish (DocFormatter Phase 2)

## Executive Summary

Phase 2 layers four `Optional` formatting rules on top of the existing five-rule MVP pipeline so that DOI/section/title alignment, author-block spacing, abstract reframing, and corresponding-author email surfacing all happen without manual finishing. The rules share state through additive `FormattingContext` properties and live alongside their MVP siblings in `DocFormatter.Core/Rules/`. The diagnostic JSON gets one new optional sibling (`formatting`) so the editor can spot which papers need a human look.

The primary technical trade-off: the four new rules read and mutate live `Paragraph` references that flow through `FormattingContext`, which keeps the rules decoupled from each other but ties the pipeline to a fixed ordering and to a "no rule deletes a paragraph it published downstream" invariant. Picking discrete sibling rules (ADR-001) over a single consolidated rewrite buys per-rule severity, per-rule diagnostics, and per-rule tests at the cost of two more pipeline registrations and four more files.

## System Architecture

### Component Overview

The MVP pipeline is preserved as-is. Four new rule classes slot in at specific positions; one shared context type and one shared diagnostic schema are extended.

| Component | Type | Responsibility | Boundaries |
|---|---|---|---|
| `ExtractCorrespondingAuthorRule` | new rule (Optional) | Detect `* E-mail:` trailer in affiliation paragraphs, extract email + ORCID, strip the trailer, mark the corresponding author. | Reads original DOM (runs before `RewriteHeaderMvpRule`); writes `FormattingContext.CorrespondingEmail/Orcid` and mutates affiliation paragraphs. |
| `ApplyHeaderAlignmentRule` | new rule (Optional) | Set `Justification` on DOI (right), section (right), title (center) paragraphs. | Runs after `RewriteHeaderMvpRule`; reads paragraph references from `FormattingContext`. |
| `EnsureAuthorBlockSpacingRule` | new rule (Optional) | Insert a blank paragraph between the last author paragraph and the first affiliation paragraph if not already present. | Runs after alignment; uses `FormattingContext.AuthorBlockEndParagraph` (set by `RewriteHeaderMvpRule` after the rewrite) as the starting anchor, walks forward to the next non-blank paragraph. |
| `RewriteAbstractRule` | new rule (Optional) | Split abstract paragraph into bold-`Abstract` heading + plain-text body (preserving internal italic per ADR-002); detect any pre-existing "corresponding author"-ish paragraph (typo-tolerant, case-insensitive); replace it with the canonical `Corresponding author: <email>` line, or insert a fresh canonical line above the heading. Pre-existing line is also used as a fallback email source when `ExtractCorrespondingAuthorRule` did not extract one. | Runs before `LocateAbstractAndInsertElocationRule`; locates abstract via `_options.AbstractMarkers`; consumes `FormattingContext.CorrespondingEmail` and `_options.CorrespondingAuthorLabelRegex`. |
| `FormattingContext` | extended | Carry shared paragraph references and corresponding-author state. | New nullable fields (back-compatible); MVP-set fields untouched. |
| `FormattingOptions` | extended | Add `EmailRegex` and `CorrespondingMarkerRegex`. | Two more `[GeneratedRegex]` members; existing options unchanged. |
| `DiagnosticDocument` / `DiagnosticWriter` | extended | Surface rule outcomes as a sibling `formatting` section. | Additive; old consumers ignore the new key; populated only when one of the four rules logs `[WARN]`/`[ERROR]`. |

### Data Flow Between Components

```
ExtractTopTableRule  →  ParseHeaderLinesRule  →  ExtractAuthorsRule  →
ExtractCorrespondingAuthorRule (NEW)
   ├─ stash affiliation paragraph reference
   ├─ strip `*…` trailer (mutates paragraph)
   ├─ ctx.CorrespondingEmail / ctx.CorrespondingOrcid
   └─ tag corresponding author in ctx.Authors

RewriteHeaderMvpRule (existing — also stashes ctx.DoiParagraph
                       and ctx.AuthorBlockEndParagraph = last new author paragraph)
   ↓
ApplyHeaderAlignmentRule (NEW)
   └─ Justification on ctx.DoiParagraph / SectionParagraph / TitleParagraph

EnsureAuthorBlockSpacingRule (NEW)
   ├─ start from ctx.AuthorBlockEndParagraph
   ├─ walk forward to the next non-blank paragraph (= first affiliation)
   └─ if the paragraph immediately before that affiliation is not blank,
      insert a blank Paragraph there

RewriteAbstractRule (NEW)
   ├─ scan front matter for pre-existing "corresponding author"-ish paragraph
   │     (typo-tolerant, case-insensitive; uses CorrespondingAuthorLabelRegex)
   ├─ if found and ctx.CorrespondingEmail is null: try EmailRegex on its text
   │     → on hit, populate ctx.CorrespondingEmail (fallback path)
   ├─ resolve action:
   │     email available + pre-existing line found → replace line with canonical
   │     email available + no pre-existing line    → insert canonical above abstract
   │     no email     + pre-existing line found    → leave line untouched
   │     no email     + no pre-existing line       → no-op
   ├─ build bold "Abstract" heading paragraph
   └─ split body, optionally strip structural italic wrapper (ADR-002)

LocateAbstractAndInsertElocationRule (existing — unchanged)
   └─ insert ELOCATION above the (now-rewritten) abstract heading paragraph
```

`ParseHeaderLinesRule` is updated to also stash `SectionParagraph` and `TitleParagraph` references in `FormattingContext`. `RewriteHeaderMvpRule` is updated to stash `DoiParagraph` after creating it. Neither change alters their MVP behavior; both are pure context-write additions.

### External System Interactions

None. All work happens against the in-memory `WordprocessingDocument` opened by `FileProcessor`.

## Implementation Design

### Core Interfaces

The shared rule contract is unchanged:

```csharp
public interface IFormattingRule
{
    string Name { get; }
    RuleSeverity Severity { get; }
    void Apply(WordprocessingDocument doc, FormattingContext ctx, IReport report);
}
```

`FormattingContext` is extended with the cross-rule state required by ADR-001 and ADR-003:

```csharp
public sealed class FormattingContext
{
    // existing
    public string? Doi { get; set; }
    public string? ElocationId { get; set; }
    public string? ArticleTitle { get; set; }
    public List<Author> Authors { get; } = new();
    public List<Paragraph> AuthorParagraphs { get; } = new();

    // new — populated by Phase 2 rules, consumed downstream
    public Paragraph? DoiParagraph { get; set; }
    public Paragraph? SectionParagraph { get; set; }
    public Paragraph? TitleParagraph { get; set; }
    public Paragraph? AuthorBlockEndParagraph { get; set; }
    public Paragraph? CorrespondingAffiliationParagraph { get; set; }
    public string? CorrespondingEmail { get; set; }
    public string? CorrespondingOrcid { get; set; }
    public int? CorrespondingAuthorIndex { get; set; }
}
```

`FormattingOptions` adds two regexes (compiled once via `[GeneratedRegex]` to match the existing pattern):

```csharp
public sealed partial class FormattingOptions
{
    // existing: DoiRegex, OrcidIdRegex, ElocationRegex, OrcidUrlMarker,
    //           AuthorSeparators, AbstractMarkers, DoiUrlPrefixes

    public Regex EmailRegex { get; } = BuildEmailRegex();
    public Regex CorrespondingMarkerRegex { get; } = BuildCorrespondingMarkerRegex();
    public Regex CorrespondingAuthorLabelRegex { get; } = BuildCorrespondingAuthorLabelRegex();

    [GeneratedRegex(@"[A-Za-z0-9._%+\-]+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,}",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex BuildEmailRegex();

    [GeneratedRegex(@"\* *E-?mail *:",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex BuildCorrespondingMarkerRegex();

    // Permissive: anchors on "c[oa]rresp" prefix + "auth/aut/autor"-like word.
    // Catches "Corresponding Author", "coresponding author", "Correspondent
    // autor", "correspondign Author" and similar typos. Trailing separator
    // (":", " -", " —") is optional. Whitespace tolerated between words.
    [GeneratedRegex(@"^\s*c[oa]rr?es?p[a-z]*\s+au[a-z]*\b\s*[:\-—]?",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex BuildCorrespondingAuthorLabelRegex();
}
```

### Data Models

The MVP `Author` record is unchanged; the corresponding-author marker is captured via `FormattingContext.CorrespondingAuthorIndex` rather than mutating `Author`. Author ORCID promotion (ADR-001 step) is done by re-creating the `Author` record at the index with the new ORCID:

```csharp
if (ctx.CorrespondingAuthorIndex is { } idx
    && ctx.Authors[idx].OrcidId is null
    && ctx.CorrespondingOrcid is not null)
{
    var a = ctx.Authors[idx];
    ctx.Authors[idx] = a with { OrcidId = ctx.CorrespondingOrcid };
}
```

The diagnostic schema gains one nullable sibling (ADR-004):

```csharp
public sealed record DiagnosticDocument(
    string File,
    string Status,
    DateTime ExtractedAt,
    DiagnosticFields Fields,
    DiagnosticFormatting? Formatting,
    IReadOnlyList<DiagnosticIssue> Issues);

public sealed record DiagnosticFormatting(
    DiagnosticAlignment? AlignmentApplied,
    DiagnosticAbstract? AbstractFormatted,
    bool? AuthorBlockSpacingApplied,
    DiagnosticCorrespondingEmail? CorrespondingEmail);

public sealed record DiagnosticAlignment(bool Doi, bool Section, bool Title);
public sealed record DiagnosticAbstract(bool HeadingRewritten, bool BodyDeitalicized, bool InternalItalicPreserved);
public sealed record DiagnosticCorrespondingEmail(string? Value, string? Reason);
```

`DiagnosticFormatting` is `null` on green runs and on warn/error runs that did not involve any of the four new rules. Each sub-object is populated only if the matching rule produced a `[WARN]`/`[ERROR]`.

### API Surface (CLI)

No CLI surface change. The existing single-file and batch-folder modes already drive the pipeline through `FileProcessor.Process`. The pipeline registration in `CliApp.BuildServiceProvider` gains four lines:

```csharp
services.AddTransient<IFormattingRule, ExtractTopTableRule>();
services.AddTransient<IFormattingRule, ParseHeaderLinesRule>();
services.AddTransient<IFormattingRule, ExtractAuthorsRule>();
services.AddTransient<IFormattingRule, ExtractCorrespondingAuthorRule>();   // NEW
services.AddTransient<IFormattingRule, RewriteHeaderMvpRule>();
services.AddTransient<IFormattingRule, ApplyHeaderAlignmentRule>();          // NEW
services.AddTransient<IFormattingRule, EnsureAuthorBlockSpacingRule>();      // NEW
services.AddTransient<IFormattingRule, RewriteAbstractRule>();               // NEW
services.AddTransient<IFormattingRule, LocateAbstractAndInsertElocationRule>();
```

The order matters: each rule's preconditions (paragraphs in context, body mutations from earlier rules) are documented in ADR-001.

## Integration Points

None. The component graph stays internal to `DocFormatter.Core` / `DocFormatter.Cli`. No new build target, no new runtime dependency.

## Impact Analysis

| Component | Impact Type | Description and Risk | Required Action |
|---|---|---|---|
| `DocFormatter.Core/Pipeline/FormattingContext.cs` | modified | Adds 7 nullable fields. Risk: rules holding stale `Paragraph` references after another rule deletes the paragraph. | Document the "do not delete a paragraph published in context" invariant in code comments; cover in tests. |
| `DocFormatter.Core/Options/FormattingOptions.cs` | modified | Adds `EmailRegex` and `CorrespondingMarkerRegex`. Low risk. | None beyond the regex shape captured in ADR-003. |
| `DocFormatter.Core/Rules/ParseHeaderLinesRule.cs` | modified | Stashes `SectionParagraph` and `TitleParagraph` in context. Risk: positional logic for "second non-empty paragraph" must survive splitting `<w:br/>` lines (already handled by existing helper). | Add two assignments and one xUnit test; ensure backward-compatible ArticleTitle population. |
| `DocFormatter.Core/Rules/RewriteHeaderMvpRule.cs` | modified | Stashes the freshly built `DoiParagraph` and the last new author paragraph as `AuthorBlockEndParagraph` in context. Risk: skipping the DOI assignment when `ctx.Doi` is null; skipping `AuthorBlockEndParagraph` when no authors are rendered. | Two assignments after the existing inserts; add coverage. |
| `DocFormatter.Core/Rules/ExtractCorrespondingAuthorRule.cs` | new | Implements ADR-003. ~150 lines. | New file + xUnit test file with matrix on `*` placement, mixed-run trailer, ORCID conflict. |
| `DocFormatter.Core/Rules/ApplyHeaderAlignmentRule.cs` | new | ~50 lines. Sets `Justification` on three paragraphs. | New file + xUnit test file; one test per missing-paragraph case. |
| `DocFormatter.Core/Rules/EnsureAuthorBlockSpacingRule.cs` | new | ~60 lines. Reads `ctx.AuthorBlockEndParagraph`, walks forward to the next non-blank paragraph (the first affiliation), inserts a blank before it if missing. | New file + xUnit test file; covers "blank already there", "blank inserted", "anchor missing". |
| `DocFormatter.Core/Rules/RewriteAbstractRule.cs` | new | ~220 lines. Implements ADR-002 italic heuristic + heading split + email insertion + pre-existing "corresponding author" line detection (typo-tolerant) + fallback email recovery. | New file + xUnit test file; covers uniform-italic, mixed-italic, missing-marker, email-insertion, replacement-of-typed-line, and fallback-email-from-typed-line branches. |
| `DocFormatter.Core/Reporting/DiagnosticDocument.cs` | modified | Adds `Formatting` sibling and four record types per ADR-004. | Update equality logic in records (existing pattern); update `DiagnosticWriter.Build`. |
| `DocFormatter.Core/Reporting/DiagnosticWriter.cs` | modified | Builds `DiagnosticFormatting` from `report.Entries` keyed by rule name. | Add helper `BuildFormatting`; cover in `DiagnosticWriterTests`. |
| `DocFormatter.Cli/CliApp.cs` | modified | Registers four new rules in the DI container in the order specified. | Append four `AddTransient` calls; verify by `CliIntegrationTests`. |
| `DocFormatter.Tests/Fixtures/Authors/AuthorsParagraphFactory.cs` | modified (likely) | May need helpers for affiliation paragraphs and abstract paragraphs. | Add factory methods per the new fixture needs. |
| `examples/` corpus | inspected, not modified | Re-running with Phase 2 rules must not regress any of the eleven articles. | Manual editor review (success criteria in PRD). |

## Testing Approach

### Unit Tests

One xUnit test file per new rule, mirroring the MVP's testing pattern (`<RuleName>Tests.cs`).

**`ExtractCorrespondingAuthorRuleTests`**

- Affiliation has `* E-mail: foo@x.com` plain text → email extracted, trailer stripped, paragraph survives with the leading affiliation text.
- Affiliation has `* E-mail: foo@x.com ORCID: https://orcid.org/0000-0002-1825-0097` → email + ORCID extracted; ORCID attached to corresponding author when author had no ORCID.
- Same trailer but author already had an ORCID → affiliation ORCID dropped silently (no `[WARN]`).
- `*` is in superscript run on the authors line, label-side (`1,2*`) → corresponding author identified by adjacency.
- `*` is plain text after author name (`Maria Silva*`) → corresponding author identified.
- `*` appears twice → `[WARN]`; first wins.
- Paper has no `*` → `[INFO]` ("no corresponding author marker found"); rule is a no-op for the rest of the pipeline.
- `*` present but email regex fails on the trailer → `[WARN]`; trailer still stripped; `CorrespondingEmail` stays null.
- Affiliation paragraph is empty after stripping → paragraph removed from body.

**`ApplyHeaderAlignmentRuleTests`**

- All three paragraphs in context → all three carry `Justification` (Right / Right / Center) afterward.
- `DoiParagraph` is null → only section + title aligned; `[WARN]` for DOI.
- `SectionParagraph` is null → only DOI + title aligned; `[WARN]` for section.
- `TitleParagraph` is null → only DOI + section aligned; `[WARN]` for title.
- All three null → three `[WARN]`s; rule is a no-op.
- A paragraph already had the target alignment → no `[WARN]`, property still written, idempotent state.

**`EnsureAuthorBlockSpacingRuleTests`**

- Last author paragraph followed directly by the first affiliation paragraph → blank paragraph inserted between them; `AuthorBlockSpacingApplied = true`.
- Last author paragraph followed by an existing blank, then the first affiliation → no insertion; the rule observes the existing blank.
- `ctx.AuthorBlockEndParagraph` is null (extraction failed earlier or no authors rendered) → `[WARN]`; rule is a no-op.
- No non-blank paragraph follows the author block (degenerate body) → `[WARN]`; rule is a no-op.

**`RewriteAbstractRuleTests`**

- Paragraph `*Abstract - lorem ipsum*` with uniform italic → produces two paragraphs (`**Abstract**` heading + plain-text body); `[INFO]` ("structural italic wrapper removed"); `BodyDeitalicized = true`, `InternalItalicPreserved = false` (no italic remained because none was localized).
- Paragraph `*Abstract - lorem* *Aedes aegypti* *more text*` with mixed italic (some non-italic runs) → only the heading is rewritten; body keeps run-level italic exactly as authored; `BodyDeitalicized = false`, `InternalItalicPreserved = true`.
- `Resumo - …` paragraph → heading normalized to `Abstract` (English) per PRD open-question default; body language untouched.
- `ctx.CorrespondingEmail` populated, no pre-existing typed line → `Corresponding author: foo@x.com` paragraph inserted immediately before the bold heading.
- `ctx.CorrespondingEmail` populated **and** front matter contains `Corresponding Author: foo@x.com` (canonical case) → typed line removed, canonical line takes its place; `[INFO]` logged.
- `ctx.CorrespondingEmail` populated **and** front matter contains `coresponding author - foo@x.com` (lowercase, missing 'r', dash separator) → typed line matched by `CorrespondingAuthorLabelRegex`, removed, canonical line inserted.
- `ctx.CorrespondingEmail` populated **and** front matter contains `Correspondent Autor foo@x.com` (Portuguese-ish, no separator) → typed line matched, removed, canonical line inserted.
- `ctx.CorrespondingEmail` is null **and** front matter contains `Corresponding Author: bar@y.edu` → fallback path: email regex hits inside the typed paragraph, `ctx.CorrespondingEmail` set to `bar@y.edu`, typed line replaced with canonical version; `[INFO]` ("recovered email from pre-existing corresponding-author line").
- `ctx.CorrespondingEmail` is null **and** front matter contains `Corresponding author:` (no email at all) → typed line is left in place untouched; no canonical insertion; no `[WARN]` (PRD: do not destroy author content).
- `ctx.CorrespondingEmail` is null **and** no pre-existing typed line → no-op for corresponding-author insertion.
- Abstract paragraph not found → `[WARN]` ("Abstract paragraph not found"); rule is a no-op; corresponding-author email NOT inserted.
- Marker found but no separator (hyphen/colon) after it → `[WARN]`; heading still rewritten; body remains the original (post-marker) text.
- `CorrespondingAuthorLabelRegex` false-positive guard: a paragraph starting with `Correspondence:` (different intent — used for "letters to the editor") does **not** match, since the regex requires the second word to start with `au`. Verify with a test fixture.

**`DiagnosticWriterTests` (extended)**

- All four rules silent → `Formatting` is `null` in the JSON.
- Only alignment rule warns about title → `Formatting.AlignmentApplied = { doi: true, section: true, title: false }`; other sub-objects null.
- Corresponding-author email regex failed → `Formatting.CorrespondingEmail = { value: null, reason: "..." }`.
- Multiple sub-objects populated → all match the per-rule reports.

**`FormattingContextTests` (new, optional)**

- New nullable properties default to `null`.
- Author re-creation via `with { OrcidId = … }` preserves other fields.

### Integration Tests

`CliIntegrationTests` (existing) is extended with one end-to-end fixture using a `*`-marked production article from `examples/` (e.g., `5_AR_5434_3.docx` if it carries the `* E-mail:` trailer; otherwise the smallest article with the marker).

The end-to-end test asserts the produced `.docx` body has:
- DOI paragraph right-aligned (`<w:jc w:val="right"/>`).
- Section paragraph right-aligned.
- Title paragraph centered.
- Exactly one blank paragraph between the last author paragraph and the first affiliation paragraph.
- A bold-only-`Abstract` heading paragraph followed by a body paragraph whose runs lack the structural italic wrapper.

Test data setup uses the existing `AuthorsParagraphFactory` plus a new `AbstractParagraphFactory` for synthetic fixtures; production articles flow through unmodified.

Mocks are not used — every rule operates on a real `WordprocessingDocument` (in-memory `MemoryStream`).

## Development Sequencing

### Build Order

1. **Extend `FormattingContext`** (no dependencies). Add the seven new nullable properties; the project compiles unchanged.
2. **Extend `FormattingOptions`** (no dependencies). Add `EmailRegex` and `CorrespondingMarkerRegex`.
3. **Update `ParseHeaderLinesRule`** (depends on step 1). Stash `SectionParagraph` and `TitleParagraph`; add tests asserting the new context state.
4. **Update `RewriteHeaderMvpRule`** (depends on step 1). Stash `DoiParagraph` and `AuthorBlockEndParagraph` (= last new author paragraph it inserts); add tests asserting both references are set.
5. **Implement `ApplyHeaderAlignmentRule`** (depends on steps 1, 3, 4). Smallest of the four rules; lands first to validate the context-paragraph-reference pattern end-to-end.
6. **Implement `EnsureAuthorBlockSpacingRule`** (depends on steps 1 and 4). Read `ctx.AuthorBlockEndParagraph`, walk forward to the next non-blank paragraph (the first affiliation), insert a blank before it if the preceding paragraph is not already blank.
7. **Implement `ExtractCorrespondingAuthorRule`** (depends on steps 1, 2). Pre-`RewriteHeaderMvpRule` rule; touches affiliation paragraphs and corresponding-author identification.
8. **Implement `RewriteAbstractRule`** (depends on steps 1, 2, 7 — needs `ctx.CorrespondingEmail` from step 7). Heaviest of the four rules: italic heuristic + heading split + email insertion.
9. **Extend `DiagnosticDocument` and `DiagnosticWriter`** (depends on steps 5–8 producing the report entries that drive the new section).
10. **Wire the four rules in `CliApp.BuildServiceProvider`** (depends on steps 5–8). Single edit; respects the order from ADR-001.
11. **Add the end-to-end `CliIntegrationTests` fixture** (depends on step 10).
12. **Run the eleven `examples/` articles through the new pipeline**; capture the resulting diagnostic JSONs and review with the editor (PRD Phase 1 success criterion).

### Technical Dependencies

- `DocumentFormat.OpenXml` — already in the project; the new rules use only types already imported (`Justification`, `RunProperties.Italic`, `RunProperties.Bold`, `Paragraph`, `Run`, `Text`, `Break`).
- `System.Text.RegularExpressions.GeneratedRegexAttribute` — already in use (matches MVP options).
- No external services, no infra, no team-deliverable blockers.

## Monitoring and Observability

The CLI emits a per-file `<name>.report.txt` and a per-file `<name>.diagnostic.json` (only when `Warn` or higher fires). Phase 2 keeps both.

Per-rule report entries (already keyed by rule class name, see `Report.cs`):

- `ExtractCorrespondingAuthorRule`: `[INFO]` no marker found; `[WARN]` second `*`; `[WARN]` email regex failed; `[INFO]` ORCID promoted to author.
- `ApplyHeaderAlignmentRule`: `[WARN]` per missing paragraph; `[INFO]` "alignment applied" with the booleans.
- `EnsureAuthorBlockSpacingRule`: `[INFO]` "blank line inserted between authors and affiliations"; `[INFO]` "blank line already present"; `[WARN]` author block end not located; `[WARN]` no affiliation paragraph found after author block.
- `RewriteAbstractRule`: `[INFO]` "structural italic wrapper removed"; `[INFO]` "Resumo normalized to Abstract"; `[WARN]` abstract paragraph not found; `[INFO]` "Corresponding author line inserted"; `[INFO]` "replaced pre-existing corresponding-author line: '<text>'"; `[INFO]` "recovered email from pre-existing corresponding-author line".

`DiagnosticDocument.Formatting` is the structured surface for these (ADR-004). The batch summary remains `<file>.docx ✓` / `⚠ <count>` / `✗ <reason>` — phase 2 does not change the summary format.

No alerting thresholds are needed (CLI tool, single-developer, no SRE handover).

## Technical Considerations

### Key Decisions

**Decision**: Four discrete `Optional` rules sharing state via `FormattingContext` (ADR-001).
**Rationale**: Failure isolation, per-rule severity, per-rule diagnostics, alignment with the existing five-rule sibling pattern.
**Trade-offs**: Eight new files (4 rules + 4 tests) and four extra DI registrations in exchange for testability and granular `[WARN]` surfaces.
**Alternatives rejected**: Two-rule consolidation (loses severity granularity); piling logic into `RewriteHeaderMvpRule` (forces Critical severity on cosmetic concerns).

**Decision**: Heuristic-based structural-italic stripping (ADR-002).
**Rationale**: Two-branch rule ("uniform italic" → strip; "mixed italic" → preserve) catches the canonical pattern in 9/11 corpus articles without needing NLP.
**Trade-offs**: Pathological "100%-italic intentional emphasis" abstracts get flattened. Mitigated by `[INFO]` whenever stripping happens.
**Alternatives rejected**: Always strip (loses species names); always preserve (defeats the feature); per-run NLP detection (drastic complexity).

**Decision**: Pragmatic ASCII email regex and single combined `* E-mail:` marker (ADR-003), plus a permissive case-insensitive regex (`CorrespondingAuthorLabelRegex`) for detecting pre-existing "corresponding author"-ish lines typed by hand.
**Rationale**: Corpus is institutional ASCII; the combined token avoids false positives from bare `*`. The label regex anchors on `c[oa]rresp...` + `au[t]hor` to tolerate missing letters, casing, localized variants (`Autor`), and optional trailing separator without false-matching `Correspondence:` (different intent).
**Trade-offs**: Non-ASCII institutional addresses would not match. The label regex is permissive enough to be slightly false-positive-prone — the `RewriteAbstractRule` only ever acts on a matched paragraph when it is positioned in the front matter (between authors and abstract), narrowing the false-positive blast radius.
**Alternatives rejected**: Full RFC 5322 regex (unmaintainable); bare `*` marker (false positives); strict `Corresponding Author:` literal match (would miss every typed variant the user explicitly raised).

**Decision**: Additive `formatting` sibling on `DiagnosticDocument` populated only on warn/error (ADR-004).
**Rationale**: Backward-compatible schema, signal-to-noise: green runs already write no diagnostic JSON; warn/error runs get the structured rule outcomes alongside the existing fields.
**Trade-offs**: Four new record types; the writer must inspect entries by rule name to populate sub-objects.
**Alternatives rejected**: Inline fields in `DiagnosticFields` (mixes concerns); per-rule top-level keys (pollutes namespace as more rules ship).

### Known Risks

- **Stale `Paragraph` references** — A rule downstream of one that publishes a paragraph reference must not delete that paragraph without clearing the context field. Mitigated by an explicit comment on each new context field: "valid until removed; the publishing rule does not remove its own publication; subsequent rules that remove a paragraph must null the field." All four rules in this PRD respect this; no rule deletes a paragraph another rule expects to find.
- **Italic detection on style-driven runs** — Run-level `<w:i/>` is the only italic source the rule reads. A run italicized via paragraph style or document style is not detected. The corpus has no such case in the eleven `examples/` articles; if a fixture exposes one, ADR-002 documents the extension path.
- **Abstract paragraph identification drift** — Multiple rules locate the abstract paragraph independently (this PRD adds two more callers). All four use the same `_options.AbstractMarkers` list, but the locator helpers are duplicated across rules. Mitigation: phase 2 tolerates the duplication; phase 3 (per master plan) introduces `ParseAbstractRule` which centralizes the locator. Cross-rule consistency is covered by the integration test.
- **Schedule slip on italic preservation** — PRD calls out this risk explicitly. Phase 1 fallback per PRD: ship "strip all italic + `[WARN]`" if mixed-italic preservation hits an unforeseen OOXML edge; the user-visible feature still works.

## Architecture Decision Records

ADRs documenting the key technical decisions made for this phase:

- [ADR-001: Four discrete Optional rules over a single consolidated rewrite](adrs/adr-001-four-discrete-rules.md) — Implement four sibling Optional rules in `DocFormatter.Core/Rules/` that share state via `FormattingContext`, instead of expanding `RewriteHeaderMvpRule`.
- [ADR-002: Structural-italic stripping heuristic for the abstract body](adrs/adr-002-italic-preservation-heuristic.md) — Strip italic from every body run only when uniform italic spans every non-whitespace run; otherwise preserve run-level italic to keep intentional emphasis.
- [ADR-003: Marker tokenization and email regex for the corresponding-author rule](adrs/adr-003-corresponding-author-tokenization.md) — Trigger on combined `* E-mail:` token; pragmatic ASCII email regex registered in `FormattingOptions`.
- [ADR-004: Additive `formatting` section in the diagnostic JSON](adrs/adr-004-diagnostic-formatting-section.md) — Add a sibling `formatting` sub-object to `DiagnosticDocument`, populated only when one of the four new rules emits `[WARN]`/`[ERROR]`.
