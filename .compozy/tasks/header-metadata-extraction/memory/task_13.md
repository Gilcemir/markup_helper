# Task Memory: task_13.md

Keep only task-local execution context here. Do not duplicate facts that are obvious from the repository, task file, PRD documents, or git history.

## Objective Snapshot

Build the locked Windows `.exe` from macOS via `DocFormatter.Cli/publish.sh`, then have the editor run it on Windows 10 against one production article and sign off `validation/case-001.md`. Subtasks 13.1, 13.2 are completable from macOS; 13.3, 13.4, 13.5 sign-off are manual gates that require a real Windows machine and the editor.

## Important Decisions

- Used `bash` shell script (not Makefile) for `publish.sh` — repo has no existing `Makefile` and a single shell script with `set -euo pipefail` is enough for the one-step command.
- `publish.sh` resolves the solution dir from the script's own location (`SCRIPT_DIR`) so it can be invoked from anywhere; ADR-005 just locks the flag set, not the cwd.
- The artifact filename is `docformatter.exe` (lowercase), driven by `<AssemblyName>docformatter</AssemblyName>` in `DocFormatter.Cli.csproj` (set in task_11). Task spec referenced `DocFormatter.Cli.exe` in the Path section but the actual produced filename is `docformatter.exe` — followed the project file, not the doc.
- Did NOT touch nor create a project README. Success criterion mentions referencing `validation/case-001.md` from the README "or equivalent"; the case file itself describes the gate explicitly. Adding a README would expand scope beyond this infra task.

## Learnings

- macOS publish to `win-x64` with `PublishSingleFile=true`, `IncludeNativeLibrariesForSelfExtract=true`, `PublishReadyToRun=true`, `PublishTrimmed=false` produces **105 MB** on .NET 10 — about 5% above the task spec's 50–100 MB loose bound. The bound was written before the recipe was actually run; .NET 10 self-contained + ReadyToRun precompiled native code legitimately exceeds it. Documented in `validation/case-001.md` rather than relaxed silently.
- Publish folder contains exactly: `docformatter.exe`, `docformatter.pdb`, `DocFormatter.Core.pdb`. The two `.pdb` files are debug symbols and not required at runtime — ADR-005's "single self-contained file" requirement is satisfied (no loose `.dll` siblings).
- Publish runtime on the dev macOS machine: ~1 minute (well under the 5-minute integration-test cap).
- Solution-wide tests: 113/113 pass with no warnings after the publish, confirming nothing in the publish recipe regressed the build.
- SHA-256 of artifact: `3a4b50b5568cec8be2c566c1877a4c1b91270d5503082f39aa4713c2aef15704` — recorded in `validation/case-001.md` for the editor to verify after transfer.

## Files / Surfaces

- `DocFormatter.Cli/publish.sh` (new, +x): wraps the locked publish command and prints artifact path + size.
- `validation/case-001.md` (new): MVP acceptance evidence — build artifact section pre-filled; environment, smoke test, field values, warnings, Word visual check, and editor sign-off sections are placeholders for the editor.
- `.compozy/tasks/header-metadata-extraction/task_13.md` frontmatter corrected from `status: completed` → `status: pending` (was wrong — no work had been done when the file was authored). Subtasks 13.1, 13.2 ticked.
- `.compozy/tasks/header-metadata-extraction/_tasks.md` left at `pending` for task 13 — the master file already matched the truth.

## Errors / Corrections

- The task spec described the produced artifact as `DocFormatter.Cli.exe`, but `DocFormatter.Cli.csproj` sets `AssemblyName=docformatter`, so the actual file is `docformatter.exe`. Used the real filename throughout `publish.sh` and `validation/case-001.md`.

## Ready for Next Run

Manual gate remaining for the editor (gilcemir.filho@educbank.com.br):
1. Transfer `DocFormatter.Cli/bin/Release/net10.0/win-x64/publish/docformatter.exe` to a Windows 10 machine with no .NET runtime installed.
2. Run `docformatter.exe --version` and record output in `validation/case-001.md` Smoke test section.
3. Run `docformatter.exe path\to\real-article.docx` against one production article from the editorial team. Open `formatted\real-article.docx` in Word. Verify the four scoped fields, fill the field values + Word visual check sections, and sign off.
4. Once signed off, flip `task_13.md` frontmatter to `status: completed` and update `_tasks.md` task 13 row to `completed`. That closes the MVP gate per the PRD.

If the editor's Windows run reveals a real bug (e.g., DOI line missing because the production article wraps DOI in a hyperlink — see workflow MEMORY task_05 handoff note about `http://dx.doi.org/...` URL prefix not matching the anchored `DoiRegex`), file a follow-up task before re-running validation.
