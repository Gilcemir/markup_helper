---
status: completed
title: CLI bootstrap with single-file and batch flows plus report writer
type: backend
complexity: high
dependencies:
  - task_05
  - task_06
  - task_07
  - task_08
  - task_09
  - task_10
---

# Task 11: CLI bootstrap with single-file and batch flows plus report writer

## Overview
Implement `DocFormatter.Cli/Program.cs` and supporting files that wire the DI container, parse one positional argument, dispatch to either single-file or batch processing, run the pipeline against each `.docx`, save outputs to a `formatted/` subfolder, and write `<name>.report.txt` for every run plus `_batch_summary.txt` for batch runs. The CLI never overwrites the source `.docx`.

<critical>
- ALWAYS READ the PRD and TechSpec before starting
- REFERENCE TECHSPEC for implementation details — do not duplicate here
- FOCUS ON "WHAT" — describe what needs to be accomplished, not how
- MINIMIZE CODE — show code only to illustrate current structure or problem areas
- TESTS REQUIRED — every task MUST include tests in deliverables
</critical>

<requirements>
- 1. The CLI MUST accept a single positional argument: a `.docx` file path or a folder path. `--help` and `--version` MUST be supported with no other flags.
- 2. The CLI MUST NOT take a `System.CommandLine` dependency; argument parsing is bare-handled.
- 3. For a file argument: copy source to `<dir>/formatted/<name>.docx`, run the pipeline against the copy, save, and write `<dir>/formatted/<name>.report.txt`.
- 4. For a folder argument: enumerate `*.docx` (non-recursive), process each one independently, write per-file outputs into `<folder>/formatted/`, and at the end write `<folder>/formatted/_batch_summary.txt` with one line per file (`✓` success, `⚠` warning count, `✗` reason on Critical fail).
- 5. The original input `.docx` MUST never be opened in read-write mode; only the copy is mutated.
- 6. The DI container MUST register `FormattingOptions` (singleton), `IReport`/`Report` (transient — fresh per file), every `IFormattingRule` (transient, in pipeline order), and `FormattingPipeline` (transient).
- 7. Logging MUST use Serilog with both Console and File sinks; the file sink writes to `<formatted>/_app.log`.
- 8. Exit codes MUST be: 0 success, 1 usage error, 2 Critical pipeline abort on a single-file run.
</requirements>

## Subtasks
- [x] 11.1 Create `DocFormatter.Cli/Program.cs` with the entry point, argument dispatch, and DI bootstrap.
- [x] 11.2 Create `DocFormatter.Cli/FileProcessor.cs` (or equivalent) that handles single-file orchestration: copy → pipeline → save → write report.
- [x] 11.3 Extend the orchestrator with batch enumeration and `_batch_summary.txt` writing.
- [x] 11.4 Implement `DocFormatter.Core/Reporting/ReportWriter.cs` that serializes `IReport.Entries` to `.report.txt` lines per TechSpec "Monitoring".
- [x] 11.5 Wire Serilog Console+File sinks; ensure log file lives under `formatted/_app.log`.
- [x] 11.6 Add xUnit tests covering: single-file happy path, batch mode summary, `--help` output, missing-path error code 1, Critical abort exit code 2.

## Implementation Details
Files split between `DocFormatter.Cli/` (entry + orchestration) and `DocFormatter.Core/Reporting/` (report writer — kept in Core because it serializes Core types). See TechSpec "CLI surface" for the argument contract and "Monitoring and Observability" for the report format.

### Relevant Files
- `.compozy/tasks/header-metadata-extraction/_techspec.md` — "CLI surface" and "Monitoring and Observability"
- `.compozy/tasks/header-metadata-extraction/_prd.md` — User Experience flows

### Dependent Files
- `DocFormatter.Cli/Program.cs` (new)
- `DocFormatter.Cli/FileProcessor.cs` (new)
- `DocFormatter.Core/Reporting/ReportWriter.cs` (new)
- `DocFormatter.Tests/CliIntegrationTests.cs` (new)

### Related ADRs
- [ADR-002: Solution layout](adrs/adr-002.md) — keeps Cli small, Core does the work

## Deliverables
- Working CLI with single-file and batch modes
- `report.txt` writer in Core
- xUnit tests for each subtask 11.6 case
- Unit tests with 80%+ coverage **(REQUIRED)**
- Integration tests for [end-to-end CLI execution] **(REQUIRED)**

## Tests
- Unit tests:
  - [ ] `ReportWriter` given a report with one Info, one Warn, one Error: produces three lines in order, each formatted `[LEVEL] <RuleName> — <message>`.
  - [ ] Argument dispatch with no arguments: prints usage to stderr, returns exit code 1.
  - [ ] Argument dispatch with `--help`: prints usage to stdout, returns exit code 0.
  - [ ] Argument dispatch with `--version`: prints assembly informational version, returns exit code 0.
  - [ ] Argument dispatch with a path that does not exist: prints "path not found" to stderr, returns exit code 1.
- Integration tests:
  - [ ] Single-file mode on a fixture `.docx`: a `formatted/<name>.docx` and `formatted/<name>.report.txt` are produced; the source file's last-write time is unchanged.
  - [ ] Batch mode on a folder containing two valid fixtures and one fixture missing the top table: `_batch_summary.txt` records 2 ✓ and 1 ✗; only the two valid files have `<name>.docx` outputs.
  - [ ] Single-file mode on a fixture missing the top table: process exit code is 2; no output `.docx` produced; `report.txt` records the Critical abort.
- Test coverage target: >=80%
- All tests must pass

## Success Criteria
- All tests passing
- Test coverage >=80%
- The source `.docx` is never modified (verified by file-hash comparison in tests)
- Exit codes match TechSpec "CLI surface"
