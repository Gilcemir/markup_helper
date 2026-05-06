---
status: completed
title: Solution skeleton with Directory.Build.props and NuGet packages
type: infra
complexity: low
dependencies: []
---

# Task 1: Solution skeleton with Directory.Build.props and NuGet packages

## Overview
Bootstrap the .NET 10 LTS solution with three csproj projects (`DocFormatter.Core`, `DocFormatter.Cli`, `DocFormatter.Tests`) and a shared `Directory.Build.props` that locks the target framework, nullability, implicit usings, and `TreatWarningsAsErrors=true`. This is the foundation every subsequent task depends on; without it `dotnet build` cannot produce a green baseline.

<critical>
- ALWAYS READ the PRD and TechSpec before starting
- REFERENCE TECHSPEC for implementation details — do not duplicate here
- FOCUS ON "WHAT" — describe what needs to be accomplished, not how
- MINIMIZE CODE — show code only to illustrate current structure or problem areas
- TESTS REQUIRED — every task MUST include tests in deliverables
</critical>

<requirements>
- 1. The solution MUST contain exactly three projects: `DocFormatter.Core`, `DocFormatter.Cli`, `DocFormatter.Tests`. No `DocFormatter.Gui` in the MVP.
- 2. All projects MUST target `net10.0` and inherit common build properties from a single `Directory.Build.props` at the solution root.
- 3. `Directory.Build.props` MUST set `TreatWarningsAsErrors=true`, `Nullable=enable`, `ImplicitUsings=enable`, `LangVersion=latest`, and `EnforceCodeStyleInBuild=true`.
- 4. `DocFormatter.Core` MUST reference `DocumentFormat.OpenXml`, `Microsoft.Extensions.DependencyInjection`, `Microsoft.Extensions.DependencyInjection.Abstractions`, `Serilog`, `Serilog.Sinks.Console`, `Serilog.Sinks.File`.
- 5. `DocFormatter.Tests` MUST reference `xunit`, `Microsoft.NET.Test.Sdk`, `xunit.runner.visualstudio`, and the `DocFormatter.Core` project.
- 6. `DocFormatter.Cli` MUST reference only `DocFormatter.Core`.
- 7. `dotnet build` from the repository root MUST exit 0 with no warnings or errors.
</requirements>

## Subtasks
- [x] 1.1 Create `DocFormatter.sln` and the three csproj files at the repository root with the directory layout from TechSpec "Component Overview".
- [x] 1.2 Create `Directory.Build.props` with the property set from TechSpec "Build Order" step 1.
- [x] 1.3 Add NuGet package references to `DocFormatter.Core.csproj` per TechSpec "Build Order" step 2.
- [x] 1.4 Add NuGet package references and the Core project reference to `DocFormatter.Tests.csproj`.
- [x] 1.5 Add the Core project reference to `DocFormatter.Cli.csproj` and configure `OutputType=Exe`.
- [x] 1.6 Run `dotnet restore` followed by `dotnet build` and confirm both exit 0 with zero warnings.

## Implementation Details
Repo is greenfield (only `instructions.md` and `links.md` exist today). All project files are new. See TechSpec "Component Overview" for the project responsibility table and "Build Order" steps 1-2 for the property/package list.

### Relevant Files
- `instructions.md` — original spec; confirms solution layout (minus the Gui project per ADR-002)
- `.compozy/tasks/header-metadata-extraction/_techspec.md` — Build Order steps 1-2 and "Component Overview"

### Dependent Files
- `DocFormatter.sln` (new) — solution file referencing the three projects
- `Directory.Build.props` (new) — solution-wide build properties
- `DocFormatter.Core/DocFormatter.Core.csproj` (new)
- `DocFormatter.Cli/DocFormatter.Cli.csproj` (new)
- `DocFormatter.Tests/DocFormatter.Tests.csproj` (new)

### Related ADRs
- [ADR-002: Solution layout](adrs/adr-002.md) — three projects, no Gui
- [ADR-005: .NET 10 LTS with TreatWarningsAsErrors](adrs/adr-005.md) — exact property set

## Deliverables
- `DocFormatter.sln` referencing the three projects
- `Directory.Build.props` enforcing the property set from ADR-005
- Three populated csproj files with restored NuGet packages
- Clean `dotnet build` output (zero warnings, zero errors)
- Unit tests with 80%+ coverage **(REQUIRED)**
- Integration tests for [solution build verification] **(REQUIRED)**

## Tests
- Unit tests:
  - [x] No application code exists in this task; the unit-test project compiles green and `dotnet test` runs (zero tests pass) without errors.
- Integration tests:
  - [x] `dotnet build` from the solution root exits 0 with no warnings on macOS.
  - [x] `dotnet test DocFormatter.Tests` exits 0 (no tests yet, runner returns success).
  - [x] Removing `TreatWarningsAsErrors` and reintroducing a deliberate `unused-variable` causes the build to fail; restoring the property set restores the green baseline (manual verification, evidence in commit message).
- Test coverage target: >=80%
- All tests must pass

## Success Criteria
- All tests passing
- Test coverage >=80%
- `dotnet build` exits 0 with zero warnings
- Solution structure matches ADR-002 and the `Directory.Build.props` property set matches ADR-005 verbatim
