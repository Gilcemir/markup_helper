# Task Memory: task_11.md

Keep only task-local execution context here. Do not duplicate facts that are obvious from the repository, task file, PRD documents, or git history.

## Objective Snapshot

- CLI bootstrap (single-file + batch), `ReportWriter` in Core, Serilog wiring, `_batch_summary.txt`, exit codes 0/1/2.

## Important Decisions

- `CliApp.Run(args, stdout, stderr)` is internal; `[InternalsVisibleTo("DocFormatter.Tests")]` lets the Tests project drive arg-dispatch unit tests without spawning a process. `DocFormatter.Tests` now also `<ProjectReference>`s `DocFormatter.Cli` for the same reason.
- Critical-abort path deletes `formatted/<name>.docx` AFTER the OpenXML using-block disposes (OpenXML auto-saves on dispose; explicit deletion is the only way to honor "no output `.docx` is produced" on abort). Ignored `IOException` / `UnauthorizedAccessException` only.
- Cli's `AssemblyName` is `docformatter` so `dotnet publish` lands a `docformatter.exe`, matching the CLI command shown in PRD/TechSpec usage strings.
- Serilog logger built per-run (not the static `Log.Logger`) so parallel xUnit tests do not collide on global state. File sink path is `<formatted>/_app.log` with no rolling interval (filename must stay stable per spec).

## Learnings

- `dotnet run --project ... -- <args>` does propagate the inner exit code; the surrounding shell sees exit=1/2 correctly. Smoke-tested with `--version`, `--help`, missing path, real `examples/1_AR_5449_2.docx`.
- `WordprocessingDocument.Open(path, isEditable: true)` saves on dispose unconditionally. The CLI's safety contract is: copy-first, mutate-the-copy, delete-the-copy on abort. The original is never opened in r/w.

## Files / Surfaces

- `DocFormatter.Cli/Program.cs` — top-level entry, delegates to `CliApp.Run`.
- `DocFormatter.Cli/CliApp.cs` — arg parsing, DI bootstrap, single-file + batch dispatch, Serilog wiring, batch summary.
- `DocFormatter.Cli/FileProcessor.cs` — per-file orchestration (copy → pipeline → save → report).
- `DocFormatter.Cli/Properties/AssemblyInfo.cs` — `[InternalsVisibleTo]`.
- `DocFormatter.Cli/DocFormatter.Cli.csproj` — added MEDI + Serilog packages, `AssemblyName=docformatter`.
- `DocFormatter.Core/Reporting/ReportWriter.cs` — `[LEVEL] <Rule> — <message>` line writer.
- `DocFormatter.Tests/CliIntegrationTests.cs` + `DocFormatter.Tests/ReportWriterTests.cs` — 11 new tests (8 CLI + 3 ReportWriter); 99/99 pass.

## Errors / Corrections

- Initial `Write_CreatesParentDirectoryIfMissing` cleanup deleted the wrong path (used `Path.GetDirectoryName(Path.GetDirectoryName(tempDir))`). Corrected by tracking the root tempdir explicitly.

## Ready for Next Run

- task_12 (diagnostic JSON) plugs into the same DI container and `Report` instance produced by `FileProcessor`. Add the writer as another transient and call it after `ReportWriter.Write` when `report.HighestLevel >= Warn`.
- task_13 (Windows publish) should set `<PublishSingleFile>true</PublishSingleFile>` etc. on `DocFormatter.Cli.csproj`; the `AssemblyName=docformatter` is already in place so the resulting binary will be `docformatter.exe`.
