# Task Memory: task_01.md

Keep only task-local execution context here. Do not duplicate facts that are obvious from the repository, task file, PRD documents, or git history.

## Objective Snapshot

Bootstrap the .NET 10 LTS solution: `Directory.Build.props`, `DocFormatter.sln`, three csproj files (Core, Cli, Tests) with required NuGet packages and project references. Status: implementation complete, verified, awaiting manual commit.

## Important Decisions

- Used `dotnet new sln --format sln` because the .NET 10 SDK now defaults to the new `.slnx` XML format and the task explicitly requires `DocFormatter.sln`.
- Wrote csproj files manually (no `dotnet new console`/`classlib`/`xunit` scaffolding) so the per-project `<TargetFramework>` is not duplicated alongside the one in `Directory.Build.props`.
- `DocFormatter.Cli/Program.cs` contains a single top-level statement `return 0;` — minimum needed to satisfy `OutputType=Exe` while staying within the task's "no application code" scope.

## Learnings

- Empty class library (`DocFormatter.Core` with zero `.cs` files but package refs) compiles clean under `TreatWarningsAsErrors=true` on net10.0.
- `dotnet test` on a project with zero discovered tests still exits 0 — runner emits an informational "no tests available" message but does not fail.
- Manual integration check confirmed: `int unused = 42;` in Program.cs raises `error CS0219` (warnings-as-errors gate is live).

## Files / Surfaces

- New: `Directory.Build.props`, `DocFormatter.sln`
- New: `DocFormatter.Core/DocFormatter.Core.csproj` (OpenXml 3.5.1, MEDI 10.0.7, MEDI.Abstractions 10.0.7, Serilog 4.3.1, Serilog.Sinks.Console 6.1.1, Serilog.Sinks.File 7.0.0)
- New: `DocFormatter.Cli/DocFormatter.Cli.csproj` (OutputType=Exe, ProjectReference → Core), `DocFormatter.Cli/Program.cs`
- New: `DocFormatter.Tests/DocFormatter.Tests.csproj` (xunit 2.9.3, Microsoft.NET.Test.Sdk 18.5.1, xunit.runner.visualstudio 3.1.5, ProjectReference → Core)

## Errors / Corrections

- First `dotnet new sln` produced `DocFormatter.slnx` (new default format). Removed and recreated with `--format sln` to match the task spec.

## Ready for Next Run

Task 02 (pipeline contracts in `DocFormatter.Core`) can start. Solution builds clean (`dotnet build`: 0 errors, 0 warnings), so any new compile failure originates from task 02 code.
