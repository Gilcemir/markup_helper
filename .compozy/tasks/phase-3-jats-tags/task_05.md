---
status: completed
title: IConfirmer implementations
type: backend
complexity: low
dependencies:
  - task_04
---

# Task 5: IConfirmer implementations

## Overview
Provide the three `IConfirmer` policies: `ConsoleConfirmer` (interactive prompt,
the CLI default), `AutoAcceptConfirmer` (writes the proposed value, for tests and
batch), and `FailOnPromptConfirmer` (errors on any prompt, for strict CI). These
realize the confidence gate's behavior across interactive and non-interactive
runs (ADR-006).

<critical>
- ALWAYS READ the PRD and TechSpec before starting
- REFERENCE TECHSPEC for implementation details — do not duplicate here
- FOCUS ON "WHAT" — describe what needs to be accomplished, not how
- MINIMIZE CODE — show code only to illustrate current structure or problem areas
- TESTS REQUIRED — every task MUST include tests in deliverables
</critical>

<requirements>
- MUST implement `ConsoleConfirmer` that prints the proposal (tag, proposed
  value, reason) and reads a confirm/override response, returning a
  `ConfirmResult` with disposition `Confirmed` or `Overridden`.
- MUST implement `AutoAcceptConfirmer` that returns the proposed value with
  disposition `AutoApplied` without any I/O.
- MUST implement `FailOnPromptConfirmer` that throws/returns a fail signal when
  `Confirm` is called, so a prompt aborts the run with a non-zero outcome.
- MUST keep all three behind the `IConfirmer` interface so injectors are agnostic
  to the policy.
- `ConsoleConfirmer` MUST write to the injected output streams (consistent with
  `CliApp.Run(args, out, err)`), not hard-coded `Console`.
</requirements>

## Subtasks
- [x] 5.1 Implement `ConsoleConfirmer` with confirm/override parsing over injected streams.
- [x] 5.2 Implement `AutoAcceptConfirmer`.
- [x] 5.3 Implement `FailOnPromptConfirmer`.
- [x] 5.4 Ensure all three are interchangeable via `IConfirmer`.

## Implementation Details
Create under `DocFormatter.Core/Jats/` (or `Jats/Confirmation/`). Depends on the
`IConfirmer`/`Proposal`/`ConfirmResult` contracts from task_04. Stream handling
should mirror how `CliApp` threads `TextWriter` for out/err. See TechSpec
"Core Interfaces" and ADR-006.

### Relevant Files
- `DocFormatter.Core/Jats/ConsoleConfirmer.cs`, `AutoAcceptConfirmer.cs`, `FailOnPromptConfirmer.cs` — new (create).
- `DocFormatter.Core/Jats/IConfirmer.cs` — contract (task_04).
- `DocFormatter.Cli/CliApp.cs` — stream-threading reference.

### Dependent Files
- CLI wiring (task_11) selects a policy from `--non-interactive` and injects it.
- Golden-corpus tests (task_12) run with `AutoAcceptConfirmer`.

### Related ADRs
- [ADR-006: Confirmer with non-interactive policy](../adrs/adr-006.md) — the three policies and their use.
- [ADR-001: Confidence-gated injection](../adrs/adr-001.md) — gate semantics.

## Deliverables
- Three `IConfirmer` implementations, interchangeable via the interface.
- Unit tests with 80%+ coverage **(REQUIRED)**
- Integration test confirming policy swap changes outcome **(REQUIRED)**

## Tests
- Unit tests:
  - [x] `AutoAcceptConfirmer.Confirm` returns the proposed value with `AutoApplied`.
  - [x] `FailOnPromptConfirmer.Confirm` signals failure (throw or fail result).
  - [x] `ConsoleConfirmer` with scripted "accept" input returns `Confirmed` + proposed value.
  - [x] `ConsoleConfirmer` with a scripted override returns `Overridden` + the new value.
  - [x] `ConsoleConfirmer` writes the proposal reason to the injected output stream.
- Integration tests:
  - [x] The same `Proposal` yields a written value under `AutoAccept` and an abort under `FailOnPrompt`.
- Test coverage target: >=80%
- All tests must pass

## Success Criteria
- All tests passing
- Test coverage >=80%
- Policies are swappable without injector changes; `FailOnPrompt` reliably aborts on any prompt.
