---
status: completed
title: 'CLI subcommand dispatcher — `phase2` and `phase2-verify` in `CliApp.Run` + Makefile targets'
type: backend
complexity: medium
dependencies:
  - task_03
  - task_04
---

# Task 05: CLI subcommand dispatcher — `phase2` and `phase2-verify` in `CliApp.Run` + Makefile targets

## Overview
The CLI grows from a single positional-argument parser to a subcommand dispatcher: `docformatter <input>` (existing default), `docformatter phase2 <input>` (new — runs the Phase 2 pipeline into `formatted-phase2/`), and `docformatter phase2-verify <before> <after>` (new — runs the corpus diff gate). Backward compatibility is non-negotiable. Makefile gets matching `phase2` and `phase2-verify` targets so the user can run them via `make`.

<critical>
- ALWAYS READ the PRD and TechSpec before starting
- REFERENCE TECHSPEC for implementation details — do not duplicate here
- FOCUS ON "WHAT" — describe what needs to be accomplished, not how
- MINIMIZE CODE — show code only to illustrate current structure or problem areas
- TESTS REQUIRED — every task MUST include tests in deliverables
</critical>

<requirements>
- MUST extend `CliApp.Run(args)` with a first-token dispatcher: if `args[0]` is `phase2` route to `RunPhase2(args[1..])`; if `phase2-verify` route to `RunPhase2Verify(args[1..])`; otherwise apply existing Phase 1 handling.
- MUST preserve every existing CLI behavior: `docformatter file.docx`, `docformatter directory/`, `docformatter -h`, `docformatter --help`, `docformatter --version`, exit codes 0 / 1 / 2.
- `RunPhase2 <input>` MUST run the Phase 2 pipeline (built via `services.AddPhase2Rules()` from task 04) and write `<input>.docx`, `<input>.report.txt`, `<input>.diagnostic.json` to `<sourceDir>/formatted-phase2/`.
- `RunPhase2 <directory>` MUST mirror the existing `RunBatch` shape: iterate `*.docx` files in the directory, write per-file outputs, plus a `_batch_summary.txt` and `_app.log` under `<directory>/formatted-phase2/`.
- `RunPhase2Verify <beforeDir> <afterDir>` MUST: for each `*.docx` in `<beforeDir>`, run the Phase 2 pipeline to a temp file, call `Phase2DiffUtility.Compare(temp, <afterDir>/<sameName>, currentScope)`, print `[PASS] <id>` or `[FAIL] <id>` (with first-divergence context on FAIL), and exit 0 if all pass / 1 if any fail.
- MUST resolve subcommand vs. file/dir ambiguity per ADR-005: if `args[0]` token names an existing file or directory, treat as Phase 1 input; otherwise interpret as subcommand.
- MUST add Makefile targets `phase2` (parameterized by `FILE=`) and `phase2-verify` (defaults to the corpus dirs).
- The current-release scope used by `phase2-verify` MUST live in a single named constant set (e.g., `Phase2Scope.Current`), updatable by tasks 06 / 07 / 09 as each release ships.
</requirements>

## Subtasks
- [x] 5.1 Add the subcommand dispatcher at the top of `CliApp.Run` with the file-existence-vs.-subcommand fallback rule.
- [x] 5.2 Implement `RunPhase2(string input)` for the single-file case (output dir `formatted-phase2/`).
- [x] 5.3 Implement `RunPhase2` directory branch mirroring the existing `RunBatch` shape (per-file outputs + `_batch_summary.txt`).
- [x] 5.4 Implement `RunPhase2Verify(string beforeDir, string afterDir)` that iterates pairs and calls `Phase2DiffUtility.Compare`.
- [x] 5.5 Define `Phase2Scope` (or equivalent) in `DocFormatter.Core` — a single static class holding the current cumulative tag-name set.
- [x] 5.6 Update the help text emitted by `-h` / `--help` to document the new subcommands.
- [x] 5.7 Add Makefile targets: `phase2` (uses `FILE=`) and `phase2-verify` (uses `examples/phase-2/before` and `examples/phase-2/after`).

## Implementation Details
The dispatcher hooks into `DocFormatter.Cli/CliApp.cs` (~248 lines) at the top of `Run()`. Today the entry method handles `--help`, `--version`, then routes to `RunSingleFile` or `RunBatch` based on path-vs-directory. The new dispatcher adds an upstream check: if `args.Length > 0 && !File.Exists(args[0]) && !Directory.Exists(args[0])` and `args[0] in {"phase2", "phase2-verify"}`, route to the corresponding handler. See TechSpec "Data flow per `phase2 <input>`" and "Data flow per `phase2-verify`" for the exact orchestration. `RunPhase2` builds DI via `services.AddCommonInfra().AddPhase2Rules()` (the precise common-infra factoring is left to implementation), runs `FormattingPipeline.Run`, and persists artifacts under `<sourceDir>/formatted-phase2/`. The Makefile pattern follows existing `run` and `run-all` targets in `Makefile`.

### Relevant Files
- `DocFormatter.Cli/CliApp.cs` — entry point modified (~248 lines).
- `DocFormatter.Cli/FileProcessor.cs` — current per-file processing helper; may be reused or duplicated for the Phase 2 path.
- `Makefile` — add `phase2` and `phase2-verify` targets (~93 lines).
- `examples/phase-2/before/` and `examples/phase-2/after/` — default verify-target inputs.

### Dependent Files
- `DocFormatter.Core/Pipeline/RuleRegistration.cs` — `AddPhase2Rules()` is called from `RunPhase2`.
- `DocFormatter.Core/Reporting/Phase2DiffUtility.cs` — called from `RunPhase2Verify`.
- `DocFormatter.Tests/CliIntegrationTests.cs` — extend with `phase2` and `phase2-verify` cases.

### Related ADRs
- [ADR-005: CLI Dispatch — Hand-Rolled Subcommands `phase2` and `phase2-verify`](adrs/adr-005.md) — Codifies the subcommand approach over `--phase2` flag and over `System.CommandLine` adoption.
- [ADR-003: Diff-Based Validation Gate](adrs/adr-003.md) — Defines the gate semantics consumed by `RunPhase2Verify`.

## Deliverables
- Modified `CliApp.cs` with subcommand dispatcher and `RunPhase2` / `RunPhase2Verify` handlers.
- New `Phase2Scope` static class (or equivalent) holding the current tag-name set.
- Updated help text mentioning `phase2` and `phase2-verify`.
- Updated `Makefile` with `phase2` (`FILE=`) and `phase2-verify` targets.
- Unit tests with 80%+ coverage **(REQUIRED)**.
- Integration tests for both subcommands end-to-end **(REQUIRED)**.

## Tests
- Unit tests:
  - [ ] Dispatcher: `args = ["phase2", "x.docx"]` → routes to `RunPhase2("x.docx")` (when `phase2` is not also an existing file/dir).
  - [ ] Dispatcher: `args = ["phase2-verify", "before/", "after/"]` → routes to `RunPhase2Verify("before/", "after/")`.
  - [ ] Dispatcher: `args = ["phase2"]` (a file literally named `phase2`) → treated as Phase 1 input when the file exists.
  - [ ] Dispatcher: `args = ["file.docx"]` → routes to existing Phase 1 single-file handler (regression).
  - [ ] Help text contains the literal strings `phase2` and `phase2-verify`.
  - [ ] `Phase2Scope.Current` exposes the expected set per release (sentinel test that fails when a future release forgets to update).
- Integration tests:
  - [ ] `CliApp.Run(["phase2", "<temp>.docx"])` writes `<temp>.docx`, `<temp>.report.txt`, `<temp>.diagnostic.json` under `<tempDir>/formatted-phase2/` and returns exit 0.
  - [ ] `CliApp.Run(["phase2", "<tempDir>"])` over a directory with 2 `.docx` files writes per-file outputs plus `_batch_summary.txt` and `_app.log`.
  - [ ] `CliApp.Run(["phase2-verify", "<beforeDir>", "<afterDir>"])` over byte-identical pairs prints `[PASS] <id>` for each and returns exit 0.
  - [ ] `CliApp.Run(["phase2-verify", "<beforeDir>", "<mutatedAfterDir>"])` prints `[FAIL] <id>` with first-divergence context and returns exit 1.
  - [ ] `CliApp.Run(["x.docx"])` (Phase 1 default) still produces output under `<sourceDir>/formatted/` (regression).
  - [ ] `make phase2-verify` exits 0 against the unmodified corpus (assuming task 06's emitters are in place).
- Test coverage target: >=80%
- All tests must pass

## Success Criteria
- All tests passing
- Test coverage >=80%
- All existing Phase 1 CLI behaviors unchanged (verified by `CliIntegrationTests` regression).
- `make phase2 FILE=examples/phase-2/before/5136.docx` produces output under `examples/phase-2/before/formatted-phase2/`.
- `make phase2-verify` exits 0 against the corpus once task 06 lands.
