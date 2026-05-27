# Task Memory: task_11.md

Keep only task-local execution context here. Do not duplicate facts that are obvious from the repository, task file, PRD documents, or git history.

## Objective Snapshot
Wire Phase 3 into the CLI: `phase3` subcommand + `--non-interactive=accept|fail`,
`AddPhase3Injectors()` + Phase 3 service provider, per-document orchestration
(pair → read docx → load XML → run pipeline → save → report), batch summary,
Phase 3 diagnostic section, exit-code mapping (fail-on-prompt → non-zero).

## Important Decisions
- BLOCKER CLEARED (2026-05-27): the stale blocker below was from an earlier run;
  task_02..task_10 are ALL implemented now (shared MEMORY confirms; files exist in
  `DocFormatter.Core/Jats/`). Proceeding with the real wiring.
- PATH RESOLUTION (no extra CLI flags per task — only `--non-interactive`):
  given the phase3 XML input (file or folder), walk up from the package dir
  (≤4 ancestors, incl. self) to the first dir containing `other.txt` = the
  phase-3 ROOT. Markup dir = `<root>/scielo_markup` if it exists else `<root>`.
  Matches corpus layout `examples/phase-3/{scielo_package,scielo_markup,other.txt}`.
  No `other.txt` found → ExitUsageError. task_12 must mirror this layout.
- All four injectors have parameterless ctors → register
  `AddTransient<IJatsInjector, …>()` in OtherId→EditedBy→DataAvailability→CreditRoles
  order. `Phase3Pipeline(IEnumerable<IJatsInjector>)` consumes them. IConfirmer is
  NOT in DI (depends on flag + stdin/stdout); passed into the orchestrator.
- DIAGNOSTIC: new `DiagnosticPhase3(OtherId,EditedBy,DataAvailability,CreditRoles)`
  of `DiagnosticPhase3Tag(Tag,Value?,Disposition)`. VALUE = ground truth from the
  post-injection XML (article-id[other], edited-by fn names, sec specific-use,
  role count). DISPOSITION = real ConfirmDisposition captured via a RecordingConfirmer
  wrapper when a tag prompts; else inferred from report entries
  (applied/skipped/absent/failed). Added as optional `Phase3=null` param on
  DiagnosticDocument (existing ctors unaffected). New `DiagnosticWriter.WritePhase3`
  /`BuildPhase3Document` path (no FormattingContext); honors the existing
  ">= Warn" write gate. Doi/Elocation in Fields read from XML article-meta.
- EXIT CODES: fail-on-prompt (PromptNotAllowedException/OCE) and critical injector
  abort → ExitCriticalAbort(2). Single-file: pairing skip/fail → 2. Batch: lenient
  like phase1/2 (exit 0 + summary) EXCEPT any Failed (incl. fail-on-prompt) → 2.
- BATCH SUMMARY counts: processed / prompted / skipped (pairing failure) / failed.

## Learnings
- Injectors report under `Name` ("other-id"/"edited-by"/"data-availability"/
  "credit-roles") as the entry Rule; success msgs start "Inserted"/"Injected",
  idempotent skips contain "already present", absent-source "No …".
- DA & Credit Proposals use `Tag = Name`; recording confirmer keyed by tag aligns.
- `JatsXmlWriter.Load(path)→JatsDocument`; mutate `jdoc.Document` (= ctx.Xml) then
  `jdoc.Save(outPath)`. Prolog captured at Load — safe to save to a different path.

## Files / Surfaces
To modify: `DocFormatter.Cli/CliApp.cs`, new `DocFormatter.Cli/Phase3Processor.cs`
(model on `FileProcessor`), `DocFormatter.Core/Pipeline/RuleRegistration.cs`,
`DocFormatter.Core/Reporting/DiagnosticDocument.cs` + `DiagnosticWriter.cs`.

## Errors / Corrections
- Phase 3 diagnostic disposition MUST reflect the actual write outcome, not the
  confirmer's vote: free-prose CREDIT makes AutoAcceptConfirmer return AutoApplied,
  but the injector declines to write any <role>. `ResolvePhase3Disposition` therefore
  honors the recorded ConfirmDisposition ONLY when a success Info ("Inserted"/
  "Injected") proves the tag was written; otherwise it derives skipped/absent/failed.

## DONE (2026-05-27)
- All requirements + subtasks 11.1–11.6 implemented & verified.
  - `AddPhase3Injectors()` in RuleRegistration.cs (4 injectors, run order, transient).
  - `BuildPhase3ServiceProvider()` + `phase3` dispatch + `--non-interactive` parsing
    + `TrySelectConfirmer` + `TryResolvePhase3Layout` + single/batch + batch summary
    + stdin `Run` overload + usage text, all in CliApp.cs.
  - New `Phase3Processor.cs` (+ `RecordingConfirmer`): pair → Load → Phase3Context →
    Phase3Pipeline → Save → report + diagnostic; uses pair.Source/OtherNumber (no docx re-read).
  - `DiagnosticPhase3`/`DiagnosticPhase3Tag` + optional `Phase3` on DiagnosticDocument
    + `DiagnosticWriter.WritePhase3`/`BuildPhase3Document` (value from XML, disposition
    from report + recorded ConfirmDispositions; honors ">= Warn" write gate).
- Tests: CliPhase3Tests.cs, DiagnosticWriterPhase3Tests.cs, +AddPhase3Injectors in
  RuleRegistrationTests.cs. Full suite 770 passed / 0 failed; build 0 warn/0 err.
  Coverage (coverlet): CliApp 83.8%, Phase3Processor 87.6%, DiagnosticWriter 91.2%,
  RuleRegistration & RecordingConfirmer 100% — all ≥80%.
- Smoke-tested on corpus e54492621: other-id 00201 injected, prompted on prose CREDIT,
  diagnostic.json Phase3 section correct.

## Ready for Next Run
- task_12 (golden corpus) MUST mirror the layout convention: phase-3 ROOT holding
  `other.txt` + `scielo_markup/` (docx) + the XML package; the CLI walks up ≤4 dirs
  from the XML's dir to find `other.txt`. Output dir = `<pkgdir>/formatted-phase3/`.
  Invoke `CliApp.Run(["phase3", tempXml, "--non-interactive=accept"], …)`.
- NOT auto-committed (run flag --auto-commit=false); diff left for manual review.
- Format gate still red on pre-existing `Phase2PipelineIntegrationTests.cs` only.
