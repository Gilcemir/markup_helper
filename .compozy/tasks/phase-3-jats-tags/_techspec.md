# TechSpec: Phase 3 — JATS Tag Injection

## Executive Summary

Phase 3 adds a post-Markup step to DocFormatter that injects four SciELO
Publishing Schema (SPS 1.10) tags into the generated JATS XML: `<article-id
pub-id-type="other">`, `<fn fn-type="edited-by">`, `<sec
sec-type="data-availability">`, and CRediT `<role>` elements. Values are derived
from three inputs — a read-only paired `.docx` markup source, an external
`other.txt` TSV, and the XML itself — and only the XML is modified. The design
mirrors the phase 1/2 rule pipeline but is correctly typed for docx→XML
injection: a new `IJatsInjector` interface, a `Phase3Context` carrying the
parsed docx source plus the target `XDocument`, four injectors, and a new
`phase3` CLI subcommand (ADR-003). Documents are paired on elocation-id with a
DOI cross-check and fail loudly on mismatch (ADR-004).

The defining trade-off is **precision over automation rate**: deterministic and
high-confidence derivations are written automatically, but anything ambiguous —
free-prose CRediT statements, unrecognized terms, unresolved author initials, an
unclear data-availability category — pauses for inline confirmation through an
`IConfirmer` gate (ADR-001, ADR-005, ADR-006) rather than guessing. Re-runs are
idempotent: an injector skips and reports any target tag already present. XML is
written with whitespace preserved so golden-corpus diffs show only the injected
tags. The cost is that prose-style documents still prompt; the benefit is zero
silently-wrong output, which is the failure mode that killed the predecessor
MathML project.

## System Architecture

### Component Overview

New code lives under `DocFormatter.Core/Jats/`. The CLI gains a `phase3`
subcommand alongside `phase2`.

- **`Phase3Command` (CLI, `DocFormatter.Cli`)** — parses `phase3 <file|folder>`
  plus `--non-interactive=accept|fail`, resolves the input set, loads
  `other.txt`, builds the DI scope, and orchestrates per-document processing.
  Mirrors the existing phase2 dispatch in `CliApp.Run`.
- **`DocumentPairer`** — given an XML path, locates its paired docx and the
  `other.txt` entry by elocation-id, verifies the DOI on both sides, and fails
  loudly on any missing/mismatched key (ADR-004).
- **`DocxSourceReader`** — parses the read-only docx into a `DocxSource`:
  the `[doc]` header keys (elocatid, doi) and the trailing untagged sections
  (`Scientific Editor:`, `DATA AVAILABILITY`, `CREDIT STATEMENT`).
- **`OtherTable`** — loads `other.txt` (TSV) into a basename→other-number map.
- **`Phase3Pipeline`** — runs the registered `IJatsInjector`s in order over a
  `Phase3Context`, catching/logging per injector (critical injectors rethrow),
  reusing the phase 1/2 pipeline error model.
- **Four `IJatsInjector`s** — `OtherIdInjector`, `EditedByInjector`,
  `DataAvailabilityInjector`, `CreditRolesInjector`. Each is idempotent
  (skip-if-present) and emits report entries.
- **`IConfirmer`** — `ConsoleConfirmer` (interactive default), `AutoAcceptConfirmer`
  and `FailOnPromptConfirmer` (non-interactive policies, ADR-006).
- **`XmlWriter` helper** — loads with `LoadOptions.PreserveWhitespace`, saves
  with formatting disabled, indents only injected nodes (minimal diff).
- **Reuse** — `IReport` / `ReportWriter` for the per-file `.report.txt`;
  `DiagnosticWriter` extended with a Phase 3 section.

### Data flow

```
phase3 <input> --non-interactive=accept
   │
   ├─ OtherTable.Load(other.txt)                         → basename→other map
   │
   └─ for each XML in input:
        DocumentPairer.Pair(xmlPath, otherTable)         → (docx, xml, other#)  [fail loud on mismatch]
        DocxSourceReader.Read(docx)                       → DocxSource
        XmlWriter.Load(xmlPath, PreserveWhitespace)       → XDocument
        Phase3Context{ Source, Xml, OtherNumber, Confirm } 
        Phase3Pipeline.Run(ctx, report)                   → mutates XDocument
            OtherIdInjector → EditedByInjector → DataAvailabilityInjector → CreditRolesInjector
        XmlWriter.Save(XDocument)                          → modified .xml
        ReportWriter.Write(.report.txt); DiagnosticWriter.Write(.diagnostic.json)
```

The docx is never modified. `other.txt` is read once per run.

## Implementation Design

### Core Interfaces

```csharp
namespace DocFormatter.Core.Jats;

public interface IJatsInjector
{
    string Name { get; }                       // report label, e.g. "other-id"
    RuleSeverity Severity { get; }             // Critical vs Optional (reuse phase1/2 enum)
    void Apply(Phase3Context ctx, IReport report);
}

public sealed class Phase3Context
{
    public required DocxSource Source { get; init; }   // parsed read-only docx
    public required XDocument Xml { get; init; }        // mutation target
    public string? OtherNumber { get; init; }           // from other.txt (may be null → fail in OtherIdInjector)
    public required IConfirmer Confirm { get; init; }   // inline gate (ADR-006)
}

public interface IConfirmer
{
    // Returns the final value to write: the proposal, or an operator override.
    ConfirmResult Confirm(Proposal proposal);
}
```

```csharp
public sealed record Proposal(string Tag, string ProposedValue, string Reason);
public sealed record ConfirmResult(string Value, ConfirmDisposition Disposition);
public enum ConfirmDisposition { AutoApplied, Confirmed, Overridden, Skipped }
```

Injectors detect idempotency first (skip-if-present + report, ADR-005/PRD), then
either write deterministically, or build a `Proposal` and call
`ctx.Confirm.Confirm(...)`. Errors follow the phase 1/2 convention: optional
injectors log and continue; critical preconditions (e.g. missing DOI for
`other`) throw to abort the document.

### Data Models

```csharp
public sealed class DocxSource
{
    public required string ElocationId { get; init; }   // [doc]@elocatid
    public required string Doi { get; init; }           // [doi]…[/doi]
    public string? ScientificEditor { get; init; }      // "Scientific Editor: <name>"
    public string? AssociateEditor { get; init; }       // optional second role line
    public string? DataAvailabilityText { get; init; }  // DATA AVAILABILITY body
    public string? CreditStatementRaw { get; init; }    // CREDIT STATEMENT body (raw)
}

public sealed record CreditEntry(string AuthorKey, IReadOnlyList<string> Terms);
public sealed record EditorNote(string Role, string Name, string? OrcidUri);
```

- **`other.txt`** — TSV `<pdf-basename>\t<5-digit-other>`; loaded into
  `Dictionary<string,string>` keyed by basename minus extension.
- **CRediT term→URL** — static table from `credit_roles.md`
  (`https://credit.niso.org/contributor-roles/<slug>/`), exact-match with
  case/dash/`&` normalization.
- **data-availability `@specific-use`** — keyword heuristic mapping the
  statement text to one of five values, seeded from `data_availability.md`.

### Tag injection rules

| Injector | Source | Placement | Auto vs prompt |
|---|---|---|---|
| `OtherIdInjector` | `other.txt` (by elocation-id) | `<article-id pub-id-type="other">` immediately after the DOI `article-id` | Deterministic; **throws** if no DOI; skip-if-present |
| `EditedByInjector` | docx `Scientific Editor:` / role lines | `<fn fn-type="edited-by">` (`<label>` role, `<p>` name) in the single `<author-notes>` (create if absent) | Deterministic name+role; ORCID omitted when absent; skip-if-present |
| `DataAvailabilityInjector` | docx `DATA AVAILABILITY` text | `<sec sec-type="data-availability">` with `<title>`, after `<ack>` in `<back>` | Text deterministic; `@specific-use` auto when keyword-confident, else prompt |
| `CreditRolesInjector` | docx `CREDIT STATEMENT` | `<role content-type="…">` inside each `<contrib>`, after `<xref>` | Auto only when structured + all terms exact-match + all initials resolve; else prompt (ADR-005) |

### API Endpoints

Not applicable — DocFormatter is a CLI tool, no network API. The CLI surface is:

```
docformatter phase3 <file.xml | folder> [--non-interactive=accept|fail]
```

- Default (no flag): interactive `ConsoleConfirmer`.
- `--non-interactive=accept`: write best-guess proposals (tests, batch).
- `--non-interactive=fail`: abort on any prompt (strict CI).
- Output mirrors phase 2: per-file modified `.xml`, `.report.txt`,
  `.diagnostic.json`, plus a batch summary.

## Integration Points

No external services. Integration is purely filesystem: the JATS XML package,
its paired docx, and `other.txt`. Namespaces (`xlink`, `mml`) and the XML
declaration/DOCTYPE are inherited from the document root and preserved on write.

## Impact Analysis

| Component | Impact Type | Description and Risk | Required Action |
|-----------|-------------|---------------------|-----------------|
| `DocFormatter.Core/Jats/` | new | New package folder for Phase 3 (injectors, context, pairing, docx reader, confirmer). Low risk; isolated. | Create folder + types |
| `IJatsInjector` + `Phase3Pipeline` | new | Parallel pipeline typed for docx→XML (ADR-003). Low risk. | Implement |
| `CliApp.Run` | modified | Add `phase3` subcommand dispatch + `--non-interactive` parsing. Risk: disambiguation vs existing phase1 file-arg rule. | Extend dispatch, follow phase2 pattern |
| `RuleRegistration` | modified | Add `AddPhase3Injectors()` + a Phase 3 service provider builder. Low risk. | Add extension method |
| `DiagnosticWriter` / `DiagnosticDocument` | modified | Add a Phase 3 diagnostic section (per-tag value + disposition). Low risk. | Extend |
| `IReport` / `ReportWriter` | reused | No change; Phase 3 emits entries through the existing report. | None |
| `DocFormatter.Tests` | new | `Phase3CorpusTests` golden corpus under `examples/phase-3/`. | Add tests + expected XML |
| `examples/phase-3/expected/` | new | Expected post-injection XML per corpus document. | Curate golden files |

## Testing Approach

### Unit Tests

- **`DocumentPairer`**: elocation-id match, DOI cross-check pass/fail, missing
  `other.txt` entry, missing DOI — all fail-loud paths.
- **`DocxSourceReader`**: header key extraction; each trailing section present /
  absent; the three CREDIT shapes (role-keyed, author-keyed, prose); editor line
  with and without a second role.
- **`CreditRolesInjector`**: structured + all-terms-exact → auto; unrecognized
  term → prompt; unresolved/ambiguous initials → prompt; prose → prompt; SPS
  all-or-nothing enforcement. Use a mock `IConfirmer`.
- **`DataAvailabilityInjector`**: each `@specific-use` keyword class auto-applies;
  ambiguous text → prompt. Mock `IConfirmer`.
- **`OtherIdInjector` / `EditedByInjector`**: deterministic placement;
  skip-if-present idempotency; `OtherIdInjector` throws when no DOI.
- **`XmlWriter`**: whitespace preserved; only injected nodes change; namespaces
  and XML declaration intact.

### Integration Tests

- **`Phase3CorpusTests`** (mirrors `Phase2CorpusTests`): for each corpus
  document, copy inputs to a temp dir, run `CliApp.Run(["phase3", tempXml,
  "--non-interactive=accept"], …)`, diff produced XML against
  `examples/phase-3/expected/<basename>.xml`. Expected files encode auto + best-
  guess proposed values, exercising the proposal path (ADR-006).
- **Strict ambiguity check**: a CI run with `--non-interactive=fail` over the
  corpus asserts which documents prompt (documents the current ambiguity set).
- Test data lives under `examples/phase-3/` (existing docx/xml/other.txt) plus a
  new `expected/` directory of golden output.

## Development Sequencing

### Build Order

1. **`DocxSource` + `DocxSourceReader`** — parse the `[doc]` header keys and the
   three trailing sections. No dependencies.
2. **`OtherTable` loader** — parse `other.txt` TSV. No dependencies.
3. **`DocumentPairer`** — depends on steps 1–2 (needs docx keys + other table);
   implements elocation-id pairing with DOI verification, fail-loud.
4. **`IConfirmer` + the three policies** — depends on `Proposal`/`ConfirmResult`
   records (step 5 types). No behavioral dependency on injectors.
5. **`IJatsInjector` + `Phase3Context` + `Phase3Pipeline`** — defines the
   contracts and runner; depends on step 4 (`IConfirmer` in context).
6. **`XmlWriter` helper** — load/save with whitespace preservation and
   indentation of injected nodes. No dependencies; used by injectors in step 7.
7. **`OtherIdInjector`** — depends on steps 2,5,6.
8. **`EditedByInjector`** — depends on steps 1,5,6.
9. **`DataAvailabilityInjector`** — depends on steps 1,4,5,6 (keyword classifier
   + confirmer).
10. **`CreditRolesInjector`** — depends on steps 1,4,5,6 (term table, initials
    resolution, confirmer); most complex, built last.
11. **CLI wiring (`phase3` subcommand, `--non-interactive`, `AddPhase3Injectors`,
    DI provider, diagnostic section)** — depends on steps 5,7–10.
12. **`Phase3CorpusTests` + `examples/phase-3/expected/`** — depends on step 11
    (end-to-end CLI) and curated golden output.

### Technical Dependencies

- No new NuGet packages required: `DocumentFormat.OpenXml` (already referenced)
  reads the docx; `System.Xml.Linq` (`XDocument`) handles XML; DI and Serilog
  already wired. The CRediT term table and the data-availability keyword corpus
  come from `docs/scielo_context/jats/*.md`.

## Monitoring and Observability

- **Per-file `.report.txt`**: one entry per injector with its value and
  `ConfirmDisposition` (AutoApplied / Confirmed / Overridden / Skipped), plus
  fail-loud reasons (no DOI, pairing mismatch, missing `other` entry).
- **`.diagnostic.json`**: Phase 3 section listing the four tags, chosen values,
  and dispositions; written when the report level reaches Warn (existing rule).
- **Batch summary**: counts of documents processed, prompted, skipped, failed —
  feeding the PRD interaction-rate metric (track which tag drives prompts).
- **Exit codes**: reuse existing conventions; `--non-interactive=fail` returns
  non-zero when any document would prompt.

## Technical Considerations

### Key Decisions

- **Pair on elocation-id, verify DOI, fail loudly (ADR-004).** Rationale: the
  docx `[doc]` header carries both keys and they unify all three inputs; two
  agreeing keys guard against mis-pairing. Trade-off: a small bracket-tag header
  parse. Rejected: DOI-only and elocation-only (no cross-check).
- **Dedicated `IJatsInjector` pipeline (ADR-003).** Rationale: the docx→XML
  paradigm doesn't fit `IFormattingRule`'s in-place-docx contract; a parallel
  pipeline keeps typing correct and the convention familiar. Trade-off: two
  pipeline abstractions coexist. Rejected: reuse `IFormattingRule` (leaky),
  single processor (diverges from convention).
- **CRediT auto only for structured + exact-term statements (ADR-005).**
  Rationale: corpus has prose statements with non-CRediT verbs; exact lookup is
  reliable only for structured shapes. Trade-off: prose documents always prompt.
  Rejected: synonym layer (maintenance/risk), always-prompt (friction), defer
  (drops a tag).
- **`IConfirmer` with injectable non-interactive policy (ADR-006).** Rationale:
  interactive UX plus deterministic tests/batch. Trade-off: `accept` can write a
  wrong proposal unattended (safe only in tests). Rejected: auto-only corpus,
  per-test scripts.
- **data-availability as `<sec>` after `<ack>`.** Rationale: more readable,
  matches the validated placement rule, common SciELO choice. Trade-off: none
  significant vs the `<fn>` form.
- **Idempotent skip-if-present.** Rationale: operators hand-edit and re-run;
  never clobber a manual correction. Trade-off: a stale auto value persists
  until manually removed.
- **Whitespace-preserving XML write.** Rationale: golden diffs show only injected
  tags; honors "XML is the only thing modified." Trade-off: must indent injected
  nodes manually.

### Known Risks

- **SPS version drift.** Sample XML declares `specific-use="sps-1.9"`, but the
  target is SPS 1.10. Likelihood: present in the corpus. Mitigation: the four
  tags' placement is identical across 1.9/1.10; do **not** silently rewrite the
  version attribute — flag it in the report and leave the decision to the
  operator. Needs confirmation against more corpus files.
- **CRediT initials resolution is fuzzy** across naming conventions (`Lopes
  DAPS`, `Viana, A. P.`, `ATAJ`, `TTN Le`). Mitigation: build candidate initials
  from `<surname>`/`<given-names>`, require a unique match, else prompt.
- **Confidence miscalibration → wrong auto-apply.** Mitigation: conservative
  thresholds favoring prompts; every auto decision in the report; tune against
  the corpus (PRD Phase 2).
- **Prose CRediT frequency lowers the no-interaction rate.** Mitigation:
  accepted for MVP; measure per-tag prompt rate; revisit a synonym layer or
  batch review with a larger corpus.

## Architecture Decision Records

- [ADR-001: Confidence-gated automatic injection with inline confirmation](adrs/adr-001.md) — auto-apply deterministic/high-confidence values, prompt only for ambiguous judgment calls, log every decision.
- [ADR-002: Source values from the paired docx markup source, inject into the XML](adrs/adr-002.md) — read DA/CRediT/editor from the docx and `other` from `other.txt`; the XML is the only artifact modified.
- [ADR-003: Dedicated IJatsInjector pipeline for XML post-processing](adrs/adr-003.md) — a parallel, correctly-typed docx→XML pipeline instead of reusing `IFormattingRule`.
- [ADR-004: Pair docx ↔ XML ↔ other.txt on elocation-id, verify with DOI](adrs/adr-004.md) — match on elocation-id, cross-check DOI, fail loudly on mismatch.
- [ADR-005: CRediT auto-mapping only for structured statements with exact terms](adrs/adr-005.md) — auto only when terms exact-match and initials resolve; prose and unrecognized terms prompt.
- [ADR-006: Confirmation gate via IConfirmer with an injectable non-interactive policy](adrs/adr-006.md) — `ConsoleConfirmer` for the CLI; `accept`/`fail` policies for deterministic tests and batch runs.
