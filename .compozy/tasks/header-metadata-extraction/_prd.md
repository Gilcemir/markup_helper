# PRD: Header Metadata Extraction (DocFormatter MVP)

## Overview

DocFormatter is a desktop CLI that normalizes scientific articles submitted as `.docx` to the editorial format of an internal journal. The MVP scopes the work to the four header fields that consume the most manual time today: **Authors**, **DOI**, **Article Title**, and **ELOCATION**.

For each input file, the tool extracts these four fields from the predefined input layout, deletes the top 3×1 control table, rewrites only those four fields in the journal's output format, and leaves the rest of the document untouched. When extraction is uncertain or fails, the tool emits a structured diagnostic file alongside the human-readable report so the editor knows exactly which articles need manual review.

This unblocks the editor's daily workflow without requiring the full 14-rule pipeline, and establishes the architectural skeleton (`FormattingPipeline`, `FormattingContext`, `Report`) that future extraction rules will plug into.

## Goals

- **Cut manual editing time** for the four header fields, with author parsing being the highest-value reduction.
- **Validate the pipeline architecture** with one real article end-to-end before investing in the remaining ten rules.
- **Surface uncertainty explicitly** so editors trust the output without re-reading every field.
- **Ship to Windows 10** as a portable `.exe` while keeping development on macOS viable.
- **Initial milestone**: one production article processed correctly end-to-end via the CLI.

## User Stories

**Editor (primary persona — internal staff who currently retypes header fields manually):**
- As an editor, I want to drop a `.docx` onto the CLI and get back a formatted `.docx` plus a report, so I stop retyping DOI, title, ELOCATION and authors by hand.
- As an editor, I want to point the CLI at a folder and have all `.docx` files processed in one go, so I can clear an entire issue's submissions with one command.
- As an editor, I want a clear, structured warning when author parsing is uncertain, so I know which file to open and review manually instead of trusting the output blindly.
- As an editor, I want the original file untouched and the output saved into a `formatted/` subfolder, so I can re-run the tool without losing my source.

**Developer (secondary persona — single-developer maintainer):**
- As the developer, I want the four MVP rules to live in the same pipeline architecture described in the spec, so adding affiliations, abstract, keywords and body rules later does not require a rewrite.
- As the developer, I want to develop and smoke-test on macOS but ship a Windows `.exe`, so I am not blocked by my development environment.

## Core Features

### 1. Detect input layout and extract top-table identifiers

The tool inspects the document's first element. If it finds a 3×1 table with columns `id`, `elocation`, `doi`, it extracts the values into the in-memory metadata context and removes the table from the document. If the table is absent, the run aborts with a clear message ("this file is not in the expected input format — it may already be formatted, or come from a different source"). DOI may be missing from the table; ELOCATION is required for the rule to consider extraction successful.

### 2. Extract section title and article title

After the top table, the tool reads two positional lines: the journal section (e.g., "Original Article") and the article title. Both are captured into the metadata context. The section line stays in place in the document; the title line stays in place. Translated titles are out of scope for the MVP.

### 3. Extract ORCID identifiers from author hyperlinks

Before author splitting happens, the tool walks the authors paragraph and, for every hyperlink whose target URL contains `orcid.org`, extracts the ORCID identifier (15-digit pattern with hyphens, optionally ending in `X`). The hyperlink itself is converted to plain text — the link relationship and any orphan ORCID icon image are removed — and the extracted ID is attached to the corresponding author record. Non-ORCID hyperlinks and manually-typed blue/underlined text are not touched.

### 4. Parse authors into structured records

The tool splits the authors paragraph by `,` and ` and ` and produces one author record per name. Each record preserves the superscript affiliation label(s) attached to the name and includes the ORCID ID extracted in Feature 3 (or null if absent). The output is a list available in the metadata context.

The rewritten output renders each author on its own line as `<name><affiliation labels> <orcid-id>` when an ORCID was found, or `<name><affiliation labels>` when not. The affiliation labels remain in superscript formatting; the ORCID ID is plain text.

When the parser cannot reach a confident result — for example, the number of superscript labels does not align with detectable affiliation labels later in the document, the split produces a fragment that does not look like a name, or an ORCID URL did not match the expected ID pattern — the rule still produces a best-effort list **and** records `[WARN]` entries describing the suspicion. The pipeline never aborts on author uncertainty.

### 5. Rewrite the four fields in the output header

Once the metadata context is populated, the tool rewrites only the four MVP fields:
- **Line 1**: DOI value.
- **Section line**: stays where it was (already on its own line in the input, immediately after the now-deleted table).
- **Title line**: stays where it was.
- **Authors block**: the original single-line authors paragraph is replaced by one paragraph per author, with affiliation superscripts preserved on each name. A blank line separates the title from the authors block.
- **ELOCATION line**: a new paragraph containing the ELOCATION identifier, inserted immediately above the Abstract paragraph. If the Abstract paragraph cannot be located, ELOCATION is omitted from the document and a `[WARN]` is logged; DOI/Title/Authors are still written.

Affiliations, history block, abstract body, keywords, article body, tables, images and references are not touched in any way.

### 6. Always emit a human-readable report; emit a diagnostic JSON only on warnings or errors

For every processed file the tool writes a `<name>.report.txt` next to the output, with `[INFO]`, `[WARN]` and `[ERROR]` lines describing what each rule did. When at least one `[WARN]` or `[ERROR]` was logged, the tool **also** writes a `<name>.diagnostic.json` containing a structured summary of which fields are uncertain or missing, so the editor can scan a folder and locate problematic files without opening each `.txt`.

### 7. Single-file and batch CLI

The CLI accepts either a single `.docx` path or a folder. When given a folder, it processes every `.docx` inside and writes a `_batch_summary.txt` with one line per file (✓/⚠/✗). The tool always writes outputs into a `formatted/` subfolder next to the input, never overwriting the source file. Re-running on the same input silently overwrites the previous output (intentional).

## User Experience

**Primary flow — single file:**
1. Editor opens a terminal in the folder containing the article.
2. Editor runs `docformatter article.docx`.
3. CLI prints a one-line summary (e.g., "✓ formatted in 1.2s, 1 warning") and exits.
4. Editor opens `formatted/article.docx` to verify the header. If the summary mentioned a warning, the editor opens `formatted/article.diagnostic.json` (or `formatted/article.report.txt`) to see exactly what was uncertain.

**Primary flow — batch:**
1. Editor runs `docformatter ./inbox/`.
2. CLI processes every `.docx` in the folder and prints a colored per-file summary plus a final tally.
3. Editor reads `inbox/formatted/_batch_summary.txt` to see which files have warnings or errors.
4. Editor opens only the flagged files for manual review.

**Failure flow — input is already formatted (or otherwise unrecognized):**
1. CLI prints a clear, specific error: "this file is not in the expected input format — it may already be formatted, or come from a different source."
2. No output `.docx` is produced. The `.report.txt` records the abort reason.
3. Editor moves the file aside and proceeds with the rest of the batch.

**Discoverability:** `docformatter --help` lists the two usage modes, output locations, and exit codes.

## High-Level Technical Constraints

- The output `.docx` must remain a valid Word document — no destructive edits beyond the four scoped fields and the deletion of the top control table.
- The tool must run cross-platform during development (macOS) and ship as a self-contained portable `.exe` for Windows 10. No installer.
- ORCID hyperlinks must be converted to plain text (link relationship and orphan icon removed) with the extracted ID preserved next to the author name. Failing to clean the relationship contaminates the parse and leaves dead links in the output.
- The original input file must never be overwritten or modified in place.
- No external configuration file — formatting constants are baked into the build.
- No code signing certificate available; users will accept Windows SmartScreen warnings.

## Non-Goals (Out of Scope)

- The other 10 rules from the original spec: affiliations parsing, history block, abstract, keywords, section promotion (16/14/13pt), generic hyperlink removal, quote normalization, table label normalization, citation normalization, footnote normalization. All deferred to later phases.
- Translated titles. The output spec mentions them but the MVP does not extract or write them.
- Writing the section line, abstract body, keywords or any body content. They remain exactly as they were in the input.
- Visual marking of uncertain fields inside the `.docx` (e.g., Word comments, yellow highlight). Diagnostic information lives in the JSON and report files only.
- A graphical interface (Avalonia GUI). CLI only.
- A full SciELO/JATS XML export. The diagnostic JSON is for editor triage, not for downstream system consumption.
- A Windows installer, code-signed binary, or auto-updater.
- CI/CD pipeline. Distribution is manual.
- Idempotency at the rule level. The input-format guard is the only protection against double-processing.

## Phased Rollout Plan

### MVP (Phase 1) — Header metadata for the four fields

- Pipeline skeleton: `IFormattingRule`, `FormattingPipeline`, `FormattingContext`, `IReport`, `Report`, `RuleSeverity`, `FormattingOptions`.
- Four extraction rules: top-table extraction, header lines parsing, ORCID stripping, author parsing.
- One rewrite rule for the four fields.
- One auxiliary rule that locates the Abstract paragraph and inserts the ELOCATION line above it.
- CLI accepting a single `.docx` or a folder, writing into `formatted/` with `.report.txt` always and `.diagnostic.json` on warnings or errors.
- Windows `.exe` publishable from macOS via `dotnet publish -r win-x64 --self-contained`.
- **Success criterion to proceed to Phase 2**: one real production article runs end-to-end on Windows, producing a correctly rewritten header with all four fields in the right place; editor confirms it would have saved real time.

### Phase 2 — Trust and breadth

- Diagnostic UX improvements driven by real-use feedback (e.g., visual marking inside the `.docx` if silent author errors are reported).
- Two more extraction rules: affiliations and history block.
- First xUnit golden-file test set (3 cases: happy path, warnings, critical fail).
- **Success criterion to proceed to Phase 3**: editors process at least one full issue's submissions through the tool with no manual rework on the four MVP fields.

### Phase 3 — Full pipeline

- Remaining rules from the spec: abstract, keywords, section promotion (16/14/13pt), generic hyperlink removal, quote normalization, table label normalization.
- Optional Avalonia GUI per the original spec.
- **Long-term success**: the original 14-rule pipeline (minus the explicitly-cut citation/footnote rules) is fully implemented and the journal's submission pipeline runs through DocFormatter end-to-end.

## Success Metrics

- **Coverage**: one production article processed end-to-end on Windows 10 with correct DOI, Title, ELOCATION position, and authors split (MVP done criterion).
- **Diagnostic precision**: every batch run reports zero false `[INFO]` and zero unflagged silent errors on the test article — i.e., if the four fields are correct, no `.diagnostic.json` is generated; if any field is uncertain, the JSON points to it.
- **Time saved (qualitative for MVP)**: editor confirms after the first real run that the manual time saved on those four fields is meaningful enough to keep using the tool. Quantitative measurement is deferred to Phase 2 when the volume justifies it.
- **Stability**: no production article causes the tool to crash with an unhandled exception. Critical failures abort cleanly with a clear message.

## Risks and Mitigations

- **Author parsing produces plausible but wrong output, editor does not notice.**
  - Mitigation: emit `[WARN]` and a `.diagnostic.json` whenever superscript label counts are suspicious or split fragments do not look like names. Re-evaluate visual marking inside the `.docx` in Phase 2 if silent errors are observed in real use.
- **Editor stops trusting the tool after one bad output.**
  - Mitigation: the diagnostic file is loud by design (it appears whenever there is any uncertainty), and the original file is never modified — the editor can always fall back to manual processing without recovery work.
- **Real production articles deviate from the assumed input format.**
  - Mitigation: the input-format guard aborts with a specific message instead of producing garbled output. The `.report.txt` records the abort reason so the developer can extend the parser for the new variant.
- **Windows `.exe` build differs from macOS dev behavior.**
  - Mitigation: Phase 1 done criterion explicitly requires the article to run on Windows 10. macOS dev is for fast iteration; final validation is on the target OS.
- **Adoption stalls because four fields is "not enough".**
  - Mitigation: the user-confirmed driver is "authors is the most painful, and these four are simplest" — even partial automation of authors should produce noticeable relief. Phase 2 adds breadth.

## Architecture Decision Records

- [ADR-001: Esqueleto alinhado ao spec com 4 regras de extração](adrs/adr-001.md) — Implement the canonical pipeline skeleton from the original spec, populated with only the four extraction rules needed for the MVP plus a rewrite rule and an Abstract-locator rule, instead of either a throwaway script or a wider safety net with golden tests.
- [ADR-002: Solution layout — Core + Cli + Tests, no Gui in MVP](adrs/adr-002.md) — Three csproj projects from day 1; defer Avalonia GUI to Phase 3.
- [ADR-003: ORCID extraction overrides spec's "remove" behavior](adrs/adr-003.md) — Extract the ORCID ID from the hyperlink, render as plain text after the author name, remove the link relationship and orphan icon.
- [ADR-004: Diagnostic JSON schema — per-field with confidence + issues list](adrs/adr-004.md) — Lock the structured schema for `<name>.diagnostic.json` so editors can scan a folder and locate problematic files.
- [ADR-005: .NET 10 LTS with TreatWarningsAsErrors, overriding the spec's .NET 8 default](adrs/adr-005.md) — Target the current LTS runtime; break the build on any compiler warning.

## Open Questions

- **DOI fallback when missing from the top table**: the original spec mentions "DOI extracted from the top table (or section if DOI absent)" for the output's line 1. The MVP currently treats DOI as best-effort: if absent, line 1 is omitted and a `[WARN]` is logged. Confirm whether the section line should actually be promoted to line 1 in that case, or if leaving DOI out is acceptable for now.
- **Translated titles**: explicitly out of scope for the MVP, but the input may already contain them on the lines between the main title and the authors. Should those lines be preserved in place, or stripped, or flagged in the diagnostic? Current default: preserve in place (rest-intact rule).
- **Abstract location heuristic** (resolved): the MVP locates the first paragraph whose first run is bold and whose text starts with "Abstract" or "Resumo" (case-insensitive). If neither is found, ELOCATION is omitted with a `[WARN]`.
- **ELOCATION format in the output line**: should the rewritten line read "ELOCATION: e2024001", just "e2024001", or follow another convention agreed with the editorial team?
- **Diagnostic JSON schema**: free-form is acceptable for the MVP, but if downstream tooling will read it later (e.g., to feed an error queue or a dashboard), the schema should be locked early. No consumer is identified yet.
