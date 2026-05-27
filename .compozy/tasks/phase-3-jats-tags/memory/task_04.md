# Task Memory: task_04.md

Keep only task-local execution context here. Do not duplicate facts that are obvious from the repository, task file, PRD documents, or git history.

## Objective Snapshot
DONE. Phase 3 contract skeleton: `IJatsInjector`, `Phase3Context`, `IConfirmer`,
`Proposal`/`ConfirmResult`/`ConfirmDisposition`, `Phase3Pipeline`. All under
`DocFormatter.Core/Jats/`. Consumed unchanged by injectors (07–10), confirmers
(05), CLI (11).

## Important Decisions
- `Phase3Pipeline` is a byte-for-byte copy of `FormattingPipeline`'s loop
  (try/catch OCE→rethrow, catch Exception→report.Error+rethrow-if-Critical),
  just retyped IJatsInjector/Phase3Context. Keeps the two pipelines' error model
  provably identical.
- Reused `RuleSeverity` (Pipeline ns) and `IReport`/`Report` — no new report type.
- `Phase3Context`: `Source`/`Xml`/`Confirm` are `required`; `OtherNumber` is
  nullable with default null (OtherIdInjector fails loud when null, per techspec).

## Learnings
- coverlet.console global tool's executable is `coverlet` (NOT `coverlet.console`),
  at `$HOME/.dotnet/tools/coverlet`; not on PATH in this shell → call full path.
  `--format text` is INVALID (only cobertura|json|lcov|opencover|teamcity); the
  ASCII summary table prints regardless of format, so use default json + read table.
- Jats-namespace coverage after this task: 100% line / 91.66% branch / 100% method
  (branch <100% is DocxSourceReader from task_01, not task_04 types).

## Files / Surfaces
- NEW: Jats/{IJatsInjector,Phase3Context,IConfirmer,Proposal,Phase3Pipeline}.cs
- NEW tests: Tests/Jats/{Phase3PipelineTests,Phase3ContractsTests,Phase3PipelineIntegrationTests}.cs
- Templates read: Pipeline/{FormattingPipeline,IFormattingRule,RuleSeverity,IReport,Report}.cs

## Errors / Corrections
- (none)

## Ready for Next Run
- task_05 (confirmers) and task_07–10 (injectors) can now implement against these
  contracts. Stub patterns for testing injectors live in Phase3PipelineTests
  (StubInjector) and the integration test (AppendChildInjector/RecordingConfirmer).
