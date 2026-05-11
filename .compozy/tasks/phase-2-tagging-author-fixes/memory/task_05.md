# Task Memory: task_05.md

Keep only task-local execution context here. Do not duplicate facts that are obvious from the repository, task file, PRD documents, or git history.

## Objective Snapshot

Implemented `phase2` / `phase2-verify` CLI subcommands with hand-rolled dispatcher (ADR-005), Makefile targets, and `Phase2Scope.Current` constant. Phase 1 default invocation byte-equivalent on `examples/1_AR_5449_2.docx`.

## Important Decisions

- `Phase2Scope.Current` is a **single cumulative set** containing both the Stage-1 baseline tags found in `examples/phase-2/before/` (`author, doc, doctitle, doi, fname, label, normaff, surname, toctitle, xref`) AND any Phase 2 release tags appended by tasks 06/07/09. Single source of truth for the gate scope; sentinel test pins exact contents.
- `FileProcessor` was parameterized with an `outputSubdirName` constructor argument (default `"formatted"`). `RunSingleFile` / `RunBatch` now share their bodies between Phase 1 and Phase 2 via private overloads that take the subdir name and a `Func<ServiceProvider>` builder. No duplication of the orchestration code.
- Dropped a "literal `phase2` file precedence" unit test: testing it requires `Directory.SetCurrentDirectory`, which is process-wide and would race against xUnit's parallel-collection scheduling. Dispatcher precedence is enforced by the `if (!File.Exists(first) && !Directory.Exists(first))` guard in `CliApp.Run`; the rest of the dispatch is covered by happy-path tests.

## Learnings

- `make phase2-verify` at task 05 lock-in produces `[FAIL] <id>` for every corpus pair because the `[doc ...]` attributes (`issueno`, `elocatid`) differ between `before/` (placeholders `xxxx`, `1`) and `after/` (final values, `2`). Task 06's `EmitElocationTagRule` is what closes that gap.
- `Run_Phase2Verify_OutOfScopeTagInAfterIsStrippedBeforeCompare_PassesAtTask05Scope` proves the strip path on synthetic data: an after file containing `[kwdgrp ...]...[/kwdgrp]` not in `Phase2Scope.Current` is correctly stripped down to its before equivalent.
- `Phase2DiffUtility.Compare` requires `producedDocxPath` to exist on disk. The verify handler checks `outcome.Kind == ProcessOutcomeKind.CriticalAbort` BEFORE calling `Compare` (FileProcessor deletes the produced copy on critical abort).

## Files / Surfaces

- `DocFormatter.Cli/CliApp.cs` — added subcommand dispatcher, `RunPhase2`, `RunPhase2Verify`, `BuildPhase1ServiceProvider`/`BuildPhase2ServiceProvider`, exit-code constant `ExitVerifyMismatch`, refactored `RunSingleFile`/`RunBatch` into private overloads.
- `DocFormatter.Cli/FileProcessor.cs` — added `outputSubdirName` parameter (constructor overload preserves default `"formatted"`).
- `DocFormatter.Core/Reporting/Phase2Scope.cs` — new file with `public static readonly IReadOnlySet<string> Current`.
- `Makefile` — `phase2` (`FILE=`) and `phase2-verify` targets, help text updated.
- `DocFormatter.Tests/Phase2ScopeTests.cs` — sentinel + ordinal-comparer tests.
- `DocFormatter.Tests/CliPhase2Tests.cs` — 13 integration tests covering help text, dispatcher routing, single-file Phase 2, directory Phase 2, verify happy path, verify mismatch, missing counterpart, out-of-scope strip, missing dirs, empty dir, Phase 1 regression.

## Errors / Corrections

- Initial design considered keeping `Phase2Scope.Current` empty at task 05; abandoned after empirical probe showed `before/` carries 10 Stage-1 bracket tags that the strip would otherwise erase from `after/` once task 06 wires emitters. Workflow memory had already flagged this risk for tasks 05/06/07/09.

## Ready for Next Run

Task 06 (Phase 2 release: `EmitElocationTagRule`, `EmitAbstractTagRule`, `EmitKwdgrpTagRule`) starts here. When task 06 lands:
- Append release tags to `Phase2Scope.Current` (`elocation` won't be there if it's emitted as an attribute on `[doc ...]`; check empirically — task 05 corpus probe shows the `after/` corpus uses tag names `xmlabstr` (not `abstract`), `kwdgrp`, `kwd`, `p`, `sectitle`).
- Update the `Phase2ScopeTests.Current_AtTask05Snapshot_ContainsOnlyStage1BaselineTags` sentinel to match the new contents.
- Verify `make phase2-verify` exits 0 against the unmodified corpus.
