# TechSpec: Header Metadata Extraction (DocFormatter MVP)

## Executive Summary

The MVP delivers a `.NET 10 LTS` console application that mutates `.docx` files in place inside a `formatted/` subfolder. The architecture is a strict pipeline of `IFormattingRule` instances composed by `FormattingPipeline`, each rule reading and writing through a shared `FormattingContext` and a structured `Report`. Six rules ship with the MVP — four extraction rules from the original spec, one custom rewrite rule, and one auxiliary Abstract-locator rule. The build is strict: `TreatWarningsAsErrors=true` is set solution-wide, so unreviewed warnings cannot accumulate.

The primary technical trade-off is **investing in pipeline scaffolding before extracting any fields**: the MVP carries the cost of `FormattingPipeline` + `FormattingContext` + `Report` + `Severity` + DI wiring even though only six rules use them in Phase 1. The payoff is that Phase 2 and Phase 3 rules slot in without refactor, and the structured `Report` already drives the diagnostic JSON contract.

## System Architecture

### Component Overview

The MVP solution `DocFormatter.sln` contains three projects (per ADR-002), all targeting `net10.0` with `TreatWarningsAsErrors=true` enforced via `Directory.Build.props` (per ADR-005):

| Project | Responsibility | Depends on |
|---|---|---|
| `DocFormatter.Core` | Pipeline contract, rule implementations, domain models, formatting options | OpenXML SDK, MSDI, Serilog |
| `DocFormatter.Cli` | Argument parsing, filesystem orchestration (single file vs folder), DI bootstrap, exit codes | Core |
| `DocFormatter.Tests` | Unit tests for `ParseAuthorsRule` (xUnit) | Core |

Data flow for a single input file:

1. `Cli/Program.cs` parses one positional argument (file path or folder path), wires DI, and instantiates `FormattingPipeline`.
2. For each `.docx` to process, the CLI opens a `WordprocessingDocument` in read-write mode against a copy in `formatted/`.
3. The pipeline runs each rule in order. Each rule mutates the document and/or populates `FormattingContext`, and writes structured entries to `Report`.
4. The CLI saves the document, writes `<name>.report.txt` from `Report`, and writes `<name>.diagnostic.json` if any `[WARN]` or `[ERROR]` was logged.
5. In batch mode, after all files are processed, the CLI writes `_batch_summary.txt`.

### External system interactions

None for the MVP. No network calls, no databases. The application reads and writes the local filesystem only.

## Implementation Design

### Core Interfaces

The pipeline rests on three shared types. `IFormattingRule` is the unit of work:

```csharp
public interface IFormattingRule
{
    string Name { get; }
    RuleSeverity Severity { get; }
    void Apply(WordprocessingDocument doc, FormattingContext ctx, IReport report);
}

public enum RuleSeverity { Critical, Optional }
```

`FormattingContext` carries cross-rule state. The MVP populates only the four scoped fields:

```csharp
public sealed class FormattingContext
{
    public string? Doi { get; set; }
    public string? ElocationId { get; set; }
    public string? ArticleTitle { get; set; }
    public List<Author> Authors { get; } = new();
}

public sealed record Author(
    string Name,
    IReadOnlyList<string> AffiliationLabels,
    string? OrcidId);
```

`IReport` collects severity-tagged entries that drive both `report.txt` and the diagnostic JSON:

```csharp
public interface IReport
{
    void Info(string rule, string message);
    void Warn(string rule, string message);
    void Error(string rule, string message);
    IReadOnlyList<ReportEntry> Entries { get; }
    ReportLevel HighestLevel { get; }
}
```

### Data Models

`FormattingOptions` (registered as a singleton via DI) holds hardcoded constants:

| Field | Value | Used by |
|---|---|---|
| `DoiRegex` | `^10\.\d{4,9}/[-._;()/:A-Z0-9]+$` (case-insensitive) | `ExtractTopTableRule` |
| `OrcidIdRegex` | `\b\d{4}-\d{4}-\d{4}-\d{3}[\dX]\b` | `ExtractOrcidLinksRule` |
| `OrcidUrlMarker` | `"orcid.org"` (substring match) | `ExtractOrcidLinksRule` |
| `AuthorSeparators` | `[", ", " and "]` | `ParseAuthorsRule` |
| `AbstractMarkers` | `["abstract", "resumo"]` (case-insensitive prefix on bold first run) | `LocateAbstractAndInsertElocationRule` |

The diagnostic JSON shape is locked in ADR-004. Serialized with `System.Text.Json` using camelCase, written only when `Report.HighestLevel >= Warn`.

### CLI surface

The CLI accepts one positional argument:

| Form | Behavior |
|---|---|
| `docformatter <path-to-file.docx>` | Processes a single file, writes outputs to `<dir>/formatted/`. |
| `docformatter <path-to-folder>` | Processes every `*.docx` inside the folder (non-recursive), writes outputs to `<folder>/formatted/`, plus `_batch_summary.txt`. |
| `docformatter --help` | Prints usage. |
| `docformatter --version` | Prints assembly version. |

Argument parsing uses bare `string[] args` plus a small dispatch. No `System.CommandLine` dependency for the MVP.

Exit codes: `0` success (file or batch ran, regardless of warnings); `1` usage error (missing or invalid path); `2` Critical rule aborted on the only file (single-file mode only).

### Rules

Six rules ship in the MVP, registered in order via DI:

| # | Rule | Severity | Mutation | Context populated |
|---|---|---|---|---|
| 1 | `ExtractTopTableRule` | Critical | Deletes the top 3×1 table | `Doi`, `ElocationId` |
| 2 | `ParseHeaderLinesRule` | Critical | None | `ArticleTitle` (and an internal section reference for downstream layout) |
| 3 | `ExtractOrcidLinksRule` | Optional | Replaces `<w:hyperlink>` with plain text, removes ORCID relationship and orphan icon | Per-author `OrcidId` (staged in a side table keyed by paragraph offset) |
| 4 | `ParseAuthorsRule` | Optional | None | `Authors` list (merges with the staged ORCID table) |
| 5 | `RewriteHeaderMvpRule` | Critical | Writes DOI line, splits authors into one paragraph per author with affiliation labels in superscript and ORCID ID in plain text | None (consumes context only) |
| 6 | `LocateAbstractAndInsertElocationRule` | Optional | Inserts a paragraph with the ELOCATION value immediately above the located Abstract paragraph | None |

Severity rationale (informs ADR-001):
- Rules 1, 2, 5 are Critical: without them the output is meaningless.
- Rules 3, 4, 6 are Optional: editor can fix authors or ELOCATION manually if the heuristic fails.

## Impact Analysis

| Component | Impact Type | Description and Risk | Required Action |
|---|---|---|---|
| `DocFormatter.sln` | new | Solution file with 3 csproj references. | Create. |
| `DocFormatter.Core/Pipeline/*` | new | `IFormattingRule`, `FormattingPipeline`, `FormattingContext`, `IReport`, `Report`, `RuleSeverity`. Foundation for all future rules. | Implement first. |
| `DocFormatter.Core/Options/FormattingOptions.cs` | new | Hardcoded constants. Future multi-profile work depends on this seam. | Implement before any rule. |
| `DocFormatter.Core/Models/Author.cs` | new | Domain record. | Implement before `ParseAuthorsRule`. |
| `DocFormatter.Core/Rules/*.cs` | new | Six rule implementations (one file per rule). | Implement after pipeline + options + models. |
| `DocFormatter.Cli/Program.cs` | new | Entry point. Risk: filesystem edge cases (path with spaces, locked file, read-only folder). | Implement after rules. Wrap file I/O in try/catch with user-facing messages. |
| `DocFormatter.Tests/ParseAuthorsRuleTests.cs` | new | xUnit tests targeting the highest-risk rule. | Implement alongside the rule. |
| `Directory.Build.props` | new | Solution-wide compiler settings. | Create with the solution. |

No existing code is modified — the project is greenfield.

## Testing Approach

### Unit Tests

Per ADR/PRD scope: `DocFormatter.Tests` covers `ParseAuthorsRule` only.

- **Components under test**: `ParseAuthorsRule` and the helpers it owns (run-walking tokenizer, ORCID merger).
- **Mock boundaries**: `FormattingContext` and `Report` are real (they have no I/O). `WordprocessingDocument` is built in-memory from `MemoryStream` for each test; fixtures live in `DocFormatter.Tests/Fixtures/Authors/*.docx`.
- **Critical scenarios**:
  1. Single author, no ORCID, one affiliation label.
  2. Multiple authors separated by commas only.
  3. Multiple authors with the trailing `" and "` pattern (`A, B and C`).
  4. Author with ORCID hyperlink resolves to the ID and removes the link.
  5. Author whose ORCID hyperlink target is `file:///` with `orcid.org` in the path (the user-confirmed edge case).
  6. Author with multiple affiliation labels (`Jane Doe^1,2`).
  7. Suspicious comma case: name containing `, Jr.` — expected to produce `[WARN]` with confidence `low`.
  8. Authors paragraph not found — expected `Warn` and empty `Authors` list.

### Integration Tests

Out of scope for the MVP (per the PRD's testing-scope decision). The success criterion is one production article processed end-to-end and validated visually. Phase 2 introduces golden-file integration tests with a semantic `.docx` comparer.

## Development Sequencing

### Build Order

1. **`Directory.Build.props` + `DocFormatter.sln` + three empty csproj files.** No dependencies. `Directory.Build.props` sets `TargetFramework=net10.0`, `LangVersion=latest`, `Nullable=enable`, `ImplicitUsings=enable`, `TreatWarningsAsErrors=true`, `EnforceCodeStyleInBuild=true`. Verifies `dotnet build` is green at zero.
2. **NuGet packages installed in each project.** Depends on step 1. Core pulls `DocumentFormat.OpenXml`, `Microsoft.Extensions.DependencyInjection`, `Microsoft.Extensions.DependencyInjection.Abstractions`, `Serilog`, `Serilog.Sinks.Console`, `Serilog.Sinks.File`. Cli pulls Core. Tests pulls Core, `xunit`, `Microsoft.NET.Test.Sdk`, `xunit.runner.visualstudio`.
3. **Pipeline contracts in Core: `IFormattingRule`, `RuleSeverity`, `FormattingContext`, `IReport`, `Report`, `ReportEntry`, `ReportLevel`.** Depends on step 2. No rule implementations yet.
4. **`FormattingOptions` and `Author` model.** Depends on step 3.
5. **`FormattingPipeline` orchestrator.** Depends on step 3. Implements the try/catch pattern from the spec: Critical exceptions abort, Optional exceptions become `[ERROR]` and the loop continues.
6. **`ExtractTopTableRule` (rule #1).** Depends on step 5. Cannot be tested end-to-end yet — verify by populating `FormattingContext` and asserting via temporary CLI logging.
7. **`ParseHeaderLinesRule` (rule #2).** Depends on step 6.
8. **`ExtractOrcidLinksRule` (rule #3).** Depends on step 5.
9. **`ParseAuthorsRule` (rule #4) + its xUnit tests.** Depends on step 8 (ORCID staging table) and step 4 (Author model). The unit tests from the Testing Approach section land here.
10. **`RewriteHeaderMvpRule` (rule #5).** Depends on steps 6, 7, 9 (it consumes the populated context).
11. **`LocateAbstractAndInsertElocationRule` (rule #6).** Depends on step 5 only (operates on the document, not the context beyond reading `ElocationId`).
12. **`Cli/Program.cs`: argument parsing, filesystem orchestration, DI bootstrap, single-file flow.** Depends on steps 5–11 (needs all rules registered).
13. **Cli batch flow + `_batch_summary.txt`.** Depends on step 12.
14. **Diagnostic JSON serializer + writer.** Depends on step 12. Lives in Cli (the writer) and reads from `Report` and `FormattingContext` (Core).
15. **Windows publish target validated from macOS.** Depends on all preceding steps. Run `dotnet publish DocFormatter.Cli -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:PublishReadyToRun=true -p:PublishTrimmed=false` and verify the resulting `.exe` size and `bin/Release/net10.0/win-x64/publish/` contents. `PublishTrimmed=false` is mandatory because OpenXML SDK uses reflection. Final validation on a Windows 10 machine with one production article.

### Technical Dependencies

- `DocumentFormat.OpenXml` (3.x) — official Microsoft NuGet, no special licensing concerns.
- A Windows 10 machine for the final acceptance test (developer-provided per the PRD).
- One production `.docx` from the editorial team for the acceptance test.

## Monitoring and Observability

Operational visibility is the `report.txt` plus the diagnostic JSON. There is no centralized telemetry — the application is a personal CLI.

- **Per-file `report.txt`**: One line per `Report` entry, format `[LEVEL] <RuleName> — <message>`. Generated for every run, including successful ones.
- **Per-file `<name>.diagnostic.json`**: Generated only when `Report.HighestLevel >= Warn`. Schema in ADR-004.
- **`_batch_summary.txt`**: Generated in batch mode. One line per file: `<filename> ✓` (success), `<filename> ⚠ <issue-count>`, or `<filename> ✗ <reason>` (Critical fail).
- **Serilog console sink**: emits `[LEVEL] message` at the same severity as `Report`. Useful while running interactively.
- **Serilog file sink**: emits to `formatted/_app.log` per run, rolling daily. Captures stack traces for unhandled exceptions that escape Optional rules.

There are no alerts, no metrics, no dashboards in the MVP.

## Technical Considerations

### Key Decisions

- **Decision**: Three projects (Core + Cli + Tests). **Rationale**: matches the PRD's testing-scope decision and the spec's project layout while keeping Avalonia out of the MVP. **Trade-offs**: ~30 minutes of solution rewiring when GUI lands in Phase 3. **Alternatives rejected**: two-project (loses Tests) and four-project with empty Gui stub (carries dead Avalonia dependencies). See ADR-002.
- **Decision**: ORCID is extracted as plain text after the author name, not removed. **Rationale**: editorial workflow values the identifier; removing it forces re-entry. **Trade-offs**: ~20 lines of extra logic vs the spec's removal behavior. **Alternatives rejected**: keep the hyperlink (inconsistent with DOI rendering); pure removal (loses information). See ADR-003.
- **Decision**: Diagnostic JSON has a per-field block plus an issues list. **Rationale**: editors must locate which articles need review without opening each `.docx`. **Trade-offs**: two structures (context fields + issue log) to keep in sync. **Alternatives rejected**: issues-only (insufficient signal); full context dump (couples to internal model). See ADR-004.
- **Decision**: ELOCATION is positioned above the Abstract paragraph, located by case-insensitive bold prefix `Abstract` or `Resumo`. **Rationale**: simplest heuristic that covers the journal's English and Portuguese articles. **Trade-offs**: misses Spanish/French articles (not in scope). **Alternatives rejected**: paragraph-style match (input articles do not use Word styles consistently per the spec).
- **Decision**: No `System.CommandLine` dependency. **Rationale**: one positional argument plus `--help` and `--version`; bare arg parsing is ~30 lines and avoids a dependency. **Trade-offs**: argument expansion in Phase 2 may want a real parser. **Alternatives rejected**: `System.CommandLine` (over-engineered for one positional arg).
- **Decision**: No JSON Schema file shipped for the diagnostic JSON. **Rationale**: schema is small, locked in ADR-004, and has no external consumer in the MVP. **Trade-offs**: future consumers must derive validation from prose. **Alternatives rejected**: ship a `.schema.json` file (premature).
- **Decision**: Target .NET 10 LTS instead of the spec's .NET 8 LTS, and enforce `TreatWarningsAsErrors` solution-wide. **Rationale**: .NET 10 is the active LTS at MVP start; warnings-as-errors prevents unreviewed warnings from accumulating in a single-developer project. **Trade-offs**: any third-party assembly that has not yet been recompiled for .NET 10 forces a downlevel reference (none expected for the MVP dependency set); a stray nullability warning blocks the build. **Alternatives rejected**: keep .NET 8 (forces migration during Phase 2); .NET 9 STS (out of support); per-project warning policy (gray zones). See ADR-005.

### Known Risks

- **Risk**: Author runs are split across multiple `<w:r>` elements with mixed formatting (very common in `.docx` produced by Word's autocorrect). Likelihood: high.
  - **Mitigation**: `ParseAuthorsRule` walks Runs in document order, accumulating text into the current author and flushing on separator detection. This is exercised by the unit tests.
- **Risk**: ORCID badge image is not nested inside the hyperlink but sits as a sibling `<w:drawing>` immediately before or after the link. Likelihood: medium (depends on how the author inserted ORCID).
  - **Mitigation**: MVP detects only nested badges. A standalone badge pre/post the hyperlink stays in the document. `[WARN]` is logged if the badge target relationship references `orcid.org` resources but no nested removal occurred. Phase 2 can address freestanding badges if real-world articles surface the case.
- **Risk**: Top-table column order in production articles deviates from `id|elocation|doi`. Likelihood: low (the input format is rigid per the spec) but high impact if it happens.
  - **Mitigation**: `ExtractTopTableRule` matches columns by header text where present; if all three columns are header-less, it falls back to position. A `[WARN]` is logged when falling back. The DOI cell is also validated against `DoiRegex`; if the cell labeled `doi` does not match, the rule retries by content scanning the other cells.
- **Risk**: `LocateAbstractAndInsertElocationRule` matches a paragraph in a footnote or a sidebar that happens to start with bold "Abstract". Likelihood: low.
  - **Mitigation**: rule only scans the document body (`Body.Elements<Paragraph>()`), skipping footnotes and headers/footers. ELOCATION is inserted at the first match, document order.
- **Risk**: macOS-built `.exe` differs from a Windows-built `.exe` in subtle ways (cross-compilation surprises with native libs in `Microsoft.Extensions.DependencyInjection` or OpenXML SDK).
  - **Mitigation**: PRD's MVP done criterion explicitly requires running the article on Windows 10. macOS builds are for fast iteration; final validation is on the target OS.

## Architecture Decision Records

- [ADR-001: Esqueleto alinhado ao spec com 4 regras de extração](adrs/adr-001.md) — Implement the canonical pipeline skeleton from the original spec, populated only with the rules needed for the four MVP fields.
- [ADR-002: Solution layout — Core + Cli + Tests, no Gui in MVP](adrs/adr-002.md) — Three projects from day 1; defer Avalonia GUI to Phase 3.
- [ADR-003: ORCID extraction overrides spec's "remove" behavior](adrs/adr-003.md) — Extract the ORCID ID as plain text after the author name; remove the link relationship and orphan icon.
- [ADR-004: Diagnostic JSON schema — per-field with confidence + issues list](adrs/adr-004.md) — Lock the structured schema editors and future tooling will depend on.
- [ADR-005: .NET 10 LTS with TreatWarningsAsErrors, overriding the spec's .NET 8 default](adrs/adr-005.md) — Target the current LTS runtime and break the build on warnings.
