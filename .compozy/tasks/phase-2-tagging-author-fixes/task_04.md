---
status: completed
title: '`RuleRegistration` — `AddPhase1Rules` / `AddPhase2Rules` extension methods'
type: refactor
complexity: low
dependencies:
  - task_02
---

# Task 04: `RuleRegistration` — `AddPhase1Rules` / `AddPhase2Rules` extension methods

## Overview
The CLI dispatcher (task 05) needs to select between Phase 1 and Phase 2 rule sets at DI composition time. Today, all 11 Phase 1 rules are registered inline in `CliApp.cs:199-221`. This task extracts the registration into two extension methods on `IServiceCollection` so each subcommand wires only its own rule set, and the Phase 2 set has a clean place to grow as tasks 06 / 07 / 09 land.

<critical>
- ALWAYS READ the PRD and TechSpec before starting
- REFERENCE TECHSPEC for implementation details — do not duplicate here
- FOCUS ON "WHAT" — describe what needs to be accomplished, not how
- MINIMIZE CODE — show code only to illustrate current structure or problem areas
- TESTS REQUIRED — every task MUST include tests in deliverables
</critical>

<requirements>
- MUST expose two public extension methods: `AddPhase1Rules(this IServiceCollection)` and `AddPhase2Rules(this IServiceCollection)`, each returning the same `IServiceCollection` for chaining.
- `AddPhase1Rules` MUST register the existing 11 Phase 1 rules in the same order they appear today in `CliApp.cs:199-221`, with the same lifetimes (transient).
- `AddPhase2Rules` MUST be present and wireable but MAY have an empty rule set initially — tasks 06 / 07 / 09 add their rules to it.
- MUST live under `DocFormatter.Core/Pipeline/RuleRegistration.cs` (per TechSpec component table).
- MUST NOT change `FormattingPipeline`, `IFormattingRule`, or any individual rule class as part of this task.
- After this task, `CliApp.cs` MUST call `services.AddPhase1Rules()` for the existing default invocation; behavior of `docformatter <input>` MUST be byte-equivalent to before.
</requirements>

## Subtasks
- [x] 4.1 Create `DocFormatter.Core/Pipeline/RuleRegistration.cs` with both extension methods.
- [x] 4.2 Move the 11 Phase 1 `services.AddTransient<IFormattingRule, …>()` calls from `CliApp.cs` into `AddPhase1Rules`, preserving order and lifetime.
- [x] 4.3 Define `AddPhase2Rules` with an explicit empty body (or a single `// rules added by tasks 06/07/09` marker comment).
- [x] 4.4 Update `CliApp.cs` to call `services.AddPhase1Rules()` in the default code path.
- [x] 4.5 Verify Phase 1 still runs end-to-end on `examples/1_AR_5449_2.docx` (baseline) with byte-equivalent output to the pre-refactor run.

## Implementation Details
The current registration sits in `DocFormatter.Cli/CliApp.cs:199-221` as 11 inline `AddTransient<IFormattingRule, …>` calls in a fixed order (ExtractTopTableRule → ParseHeaderLinesRule → ExtractAuthorsRule → ExtractCorrespondingAuthorRule → RewriteHeaderMvpRule → ApplyHeaderAlignmentRule → LocateAbstractAndInsertElocationRule → MoveHistoryRule → RewriteAbstractRule → EnsureAuthorBlockSpacingRule → PromoteSectionsRule). The order matters because `FormattingPipeline` iterates the registered enumerable. See TechSpec "System Architecture → Component Overview" `RuleRegistration.cs` row.

### Relevant Files
- `DocFormatter.Cli/CliApp.cs` — current registration site (lines 199-221).
- `DocFormatter.Core/Pipeline/FormattingPipeline.cs` — iterates the registered rules; consumes `IEnumerable<IFormattingRule>`.
- `DocFormatter.Core/Pipeline/IFormattingRule.cs` — registration interface.

### Dependent Files
- All Phase 1 rule classes under `DocFormatter.Core/Rules/*.cs` (registered, not modified).
- Task 05 — CLI dispatcher will call `AddPhase1Rules` or `AddPhase2Rules` per subcommand.
- Tasks 06 / 07 / 09 — extend `AddPhase2Rules` with their emitter rules.

### Related ADRs
- [ADR-004: Pipeline Organization — Reuse `FormattingPipeline` with DI-Selected Rule Sets](adrs/adr-004.md) — Codifies that rule-set selection happens at DI composition time via dedicated extension methods.

## Deliverables
- New file `DocFormatter.Core/Pipeline/RuleRegistration.cs` with `AddPhase1Rules` and `AddPhase2Rules`.
- Updated `DocFormatter.Cli/CliApp.cs` calling `AddPhase1Rules()` in the default path.
- Unit tests with 80%+ coverage **(REQUIRED)**.
- Integration test confirming Phase 1 default behavior is byte-equivalent post-refactor **(REQUIRED)**.

## Tests
- Unit tests:
  - [x] After `new ServiceCollection().AddPhase1Rules()`, resolving `IEnumerable<IFormattingRule>` returns 11 rules.
  - [x] The resolved rule sequence appears in the documented order (assert by `Name` property of each rule).
  - [x] Each Phase 1 rule resolves with `ServiceLifetime.Transient`.
  - [x] After `new ServiceCollection().AddPhase2Rules()`, resolving `IEnumerable<IFormattingRule>` returns 0 rules (until tasks 06/07/09 land).
  - [x] `AddPhase1Rules` returns the same `IServiceCollection` instance (fluent chaining).
  - [x] `AddPhase2Rules` returns the same `IServiceCollection` instance (fluent chaining).
- Integration tests:
  - [x] Run Phase 1 over `examples/1_AR_5449_2.docx` via `CliApp.Run`; assert produced `.docx` is byte-equivalent (or diagnostic-JSON-equivalent) to a saved pre-refactor snapshot.
- Test coverage target: >=80%
- All tests must pass

## Success Criteria
- All tests passing
- Test coverage >=80%
- `CliApp.cs` no longer contains inline `AddTransient<IFormattingRule, …>` calls (verified by grep).
- Phase 1 behavior is unchanged (verified via integration test snapshot).
