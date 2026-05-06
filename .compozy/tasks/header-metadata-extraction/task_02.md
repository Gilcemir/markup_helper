---
status: completed
title: Pipeline contracts in DocFormatter.Core
type: backend
complexity: medium
dependencies:
    - task_01
---

# Task 2: Pipeline contracts in DocFormatter.Core

## Overview
Implement the foundational pipeline contracts that every formatting rule and the orchestrator depend on: `IFormattingRule`, `RuleSeverity`, `FormattingContext`, `IReport`, `Report`, `ReportEntry`, `ReportLevel`. These types live in `DocFormatter.Core/Pipeline/` and have no rule-specific logic; they exist to fix the seams the rest of the system slots into.

<critical>
- ALWAYS READ the PRD and TechSpec before starting
- REFERENCE TECHSPEC for implementation details — do not duplicate here
- FOCUS ON "WHAT" — describe what needs to be accomplished, not how
- MINIMIZE CODE — show code only to illustrate current structure or problem areas
- TESTS REQUIRED — every task MUST include tests in deliverables
</critical>

<requirements>
- 1. `IFormattingRule` MUST expose `Name`, `Severity`, and `Apply(WordprocessingDocument doc, FormattingContext ctx, IReport report)` per TechSpec "Core Interfaces".
- 2. `RuleSeverity` MUST be a closed enum with exactly two values: `Critical` and `Optional`.
- 3. `FormattingContext` MUST hold the four scoped fields (`Doi`, `ElocationId`, `ArticleTitle`, `Authors`) and nothing else (YAGNI: do not pre-add `SectionTitle`, `Affiliations`, etc.).
- 4. `IReport` MUST expose `Info`, `Warn`, `Error`, an ordered `Entries` collection, and a `HighestLevel` property; the concrete `Report` MUST be a non-static class registered as scoped per file.
- 5. `ReportEntry` MUST capture `Rule`, `Level`, `Message`, and a UTC timestamp.
- 6. `ReportLevel` MUST be a closed enum: `Info`, `Warn`, `Error`. `HighestLevel` MUST return `Info` when the report is empty.
- 7. All public types MUST be sealed unless explicitly designed for extension (interfaces excepted).
</requirements>

## Subtasks
- [x] 2.1 Create `DocFormatter.Core/Pipeline/IFormattingRule.cs`, `RuleSeverity.cs`.
- [x] 2.2 Create `DocFormatter.Core/Pipeline/FormattingContext.cs` with the four-field surface from TechSpec "Core Interfaces".
- [x] 2.3 Create `DocFormatter.Core/Pipeline/IReport.cs`, `ReportLevel.cs`, `ReportEntry.cs`.
- [x] 2.4 Implement `DocFormatter.Core/Pipeline/Report.cs` as a non-static, thread-unsafe (single-file scope) sealed class.
- [x] 2.5 Add minimal xUnit tests covering `Report.HighestLevel` semantics and entry ordering.

## Implementation Details
All files are new under `DocFormatter.Core/Pipeline/`. See TechSpec "Core Interfaces" for the C# shape; do not duplicate the code blocks here. The `Author` record referenced from `FormattingContext.Authors` is implemented in task_03; `FormattingContext` declares `List<Author>` and forward-references it via `using DocFormatter.Core.Models`.

### Relevant Files
- `.compozy/tasks/header-metadata-extraction/_techspec.md` — "Core Interfaces" and "Component Overview"
- `instructions.md` — original spec's pipeline section for context

### Dependent Files
- `DocFormatter.Core/Pipeline/IFormattingRule.cs` (new)
- `DocFormatter.Core/Pipeline/RuleSeverity.cs` (new)
- `DocFormatter.Core/Pipeline/FormattingContext.cs` (new)
- `DocFormatter.Core/Pipeline/IReport.cs` (new)
- `DocFormatter.Core/Pipeline/Report.cs` (new)
- `DocFormatter.Core/Pipeline/ReportEntry.cs` (new)
- `DocFormatter.Core/Pipeline/ReportLevel.cs` (new)
- `DocFormatter.Tests/ReportTests.cs` (new)

### Related ADRs
- [ADR-001: Esqueleto alinhado ao spec com 4 regras](adrs/adr-001.md) — justifies investing in the full contract set in Phase 1

## Deliverables
- Seven new files under `DocFormatter.Core/Pipeline/` with the contract types
- One xUnit test file in `DocFormatter.Tests/ReportTests.cs`
- Unit tests with 80%+ coverage **(REQUIRED)**
- Integration tests for [contract types] **(REQUIRED)**

## Tests
- Unit tests:
  - [x] `Report.Info("R", "msg")` records exactly one entry with Level=Info, Rule="R", Message="msg".
  - [x] `Report.Warn` and `Report.Error` populate `Entries` in insertion order.
  - [x] `Report.HighestLevel` returns `Info` when no entries exist.
  - [x] `Report.HighestLevel` returns `Warn` after one Info and one Warn entry.
  - [x] `Report.HighestLevel` returns `Error` after one Error and any other entries.
  - [x] `ReportEntry.Timestamp` is UTC and within 1 second of the call to `Info/Warn/Error`.
- Integration tests:
  - [x] None at this task — contracts have no integration surface yet. The pipeline orchestrator (task_04) covers integration.
- Test coverage target: >=80%
- All tests must pass

## Success Criteria
- All tests passing
- Test coverage >=80%
- `dotnet build` continues to exit 0 with zero warnings (warnings-as-errors enforced)
- All contract types live under `DocFormatter.Core/Pipeline/` and match TechSpec "Core Interfaces"
