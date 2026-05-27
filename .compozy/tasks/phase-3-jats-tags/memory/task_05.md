# Task Memory: task_05.md

Keep only task-local execution context here. Do not duplicate facts that are obvious from the repository, task file, PRD documents, or git history.

## Objective Snapshot
DONE: three `IConfirmer` policies in `DocFormatter.Core/Jats/`: `ConsoleConfirmer`
(interactive default), `AutoAcceptConfirmer` (write proposal, AutoApplied),
`FailOnPromptConfirmer` (abort on any prompt). All swappable via `IConfirmer`.

## Important Decisions
- `FailOnPromptConfirmer` throws `PromptNotAllowedException : OperationCanceledException`
  (NEW type in `FailOnPromptConfirmer.cs`). Rationale: `Phase3Pipeline` swallows
  an Optional injector's plain exception but ALWAYS rethrows OCE regardless of
  severity → deriving from OCE is what makes "reliably aborts on any prompt"
  true even for optional injectors. Carries the `Proposal`.
- `ConsoleConfirmer(TextReader input, TextWriter output)` — injected streams, not
  `Console` (mirrors `CliApp.Run(args,out,err)`). Protocol: empty/whitespace/EOF
  line → `Confirmed` + proposed value; any other line → `Overridden` + trimmed text.
- Kept scope to Confirmed/Overridden for console (no Skipped path) — task didn't
  require a decline branch. CLI wiring (task_11) selects policy from `--non-interactive`.

## Learnings
- xUnit `Assert.Throws<OperationCanceledException>` requires EXACT type → fails
  on the derived type. Use `Assert.ThrowsAny<OperationCanceledException>` to
  assert the is-a relationship.

## Files / Surfaces
- NEW: `DocFormatter.Core/Jats/{ConsoleConfirmer,AutoAcceptConfirmer,FailOnPromptConfirmer}.cs`
- NEW tests: `DocFormatter.Tests/Jats/{ConfirmerTests,ConfirmerPolicySwapIntegrationTests}.cs`

## Errors / Corrections

## Ready for Next Run
- task_11 (CLI): inject `ConsoleConfirmer(Console.In, stdout)` by default; map
  `--non-interactive=accept|fail` → `AutoAcceptConfirmer`/`FailOnPromptConfirmer`.
  Catch `PromptNotAllowedException` (or OCE) at CLI boundary → non-zero exit.
