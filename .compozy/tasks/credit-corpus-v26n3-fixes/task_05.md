---
status: completed
title: contrib-names verification injector registered before credit-roles
type: backend
complexity: medium
dependencies: []
---

# Task 5: contrib-names verification injector registered before credit-roles

## Overview
Add a verification-only Phase 3 rule that warns, per contributor, when `<surname>` is empty, contains digits, or matches an ORCID, so the broken bylines of 5316, 5613 and 5501 become visible in every article's report even when no CREDIT statement exists. The rule never modifies the XML and never blocks CRediT injection (ADR-002).

<critical>
- ALWAYS READ the PRD and TechSpec before starting
- REFERENCE TECHSPEC for implementation details — do not duplicate here
- FOCUS ON "WHAT" — describe what needs to be accomplished, not how
- MINIMIZE CODE — show code only to illustrate current structure or problem areas
- TESTS REQUIRED — every task MUST include tests in deliverables
</critical>

<requirements>
- MUST implement `IJatsInjector` with `Name == "contrib-names"` and `Severity == RuleSeverity.Optional`.
- MUST emit one WARN per offending `<contrib>` (1-based index and the offending surname text in the message), distinguishing empty, digits, and ORCID-pattern cases in the wording.
- MUST NOT mutate `ctx.Xml` or `ctx.Credit`.
- MUST be registered in `RuleRegistration.AddPhase3Injectors` immediately before `CreditRolesInjector`.
- MUST emit nothing on a well-formed contributor set (no INFO noise).
</requirements>

## Subtasks
- [x] 5.1 Create `ContribNamesInjector` in `DocFormatter.Core/Jats/`.
- [x] 5.2 Register it in `AddPhase3Injectors` before `CreditRolesInjector` and update `RuleRegistrationTests` order assertions.
- [x] 5.3 Add `ContribNamesInjectorTests` covering the three shapes, the clean case, and XML immutability.
- [x] 5.4 Run the full suite, including `Phase3PipelineTests` that enumerate registered injectors.

## Implementation Details
New file `DocFormatter.Core/Jats/ContribNamesInjector.cs`; modify `DocFormatter.Core/Pipeline/RuleRegistration.cs` (`AddPhase3Injectors`, lines ~73-83). Use a local ORCID regex (`\d{4}-\d{4}-\d{4}-\d{3}[\dX]`) rather than `FormattingOptions`, which the Jats pipeline does not receive. Iterate `ctx.Xml.Descendants()` with `Name.LocalName == "contrib"` as `CreditRolesInjector` does. See TechSpec "Core Interfaces".

### Relevant Files
- `DocFormatter.Core/Jats/IJatsInjector.cs` — contract to implement.
- `DocFormatter.Core/Jats/CreditRolesInjector.cs` — pattern for contrib enumeration and report usage.
- `DocFormatter.Core/Pipeline/RuleRegistration.cs` — registration order.
- `DocFormatter.Tests/RuleRegistrationTests.cs` — asserts registered Phase 3 injectors.
- `DocFormatter.Tests/Jats/CreditRolesInjectorTests.cs` — `Contrib`/`ArticleWithContribs` helpers to copy or share.

### Dependent Files
- `DocFormatter.Cli/CliApp.cs` — `BuildPhase3ServiceProvider` picks the rule up via DI; batch summary counts its WARNs in task_07.
- `DocFormatter.Tests/Jats/Phase3PipelineTests.cs` — may assert injector count/order.

### Related ADRs
- [ADR-002: A broken contributor name warns but never blocks CRediT injection](../adrs/adr-002.md) — policy implemented here.

## Deliverables
- `ContribNamesInjector` registered before `credit-roles`.
- Unit tests with 80%+ coverage **(REQUIRED)**
- Integration tests: DI registration order verified **(REQUIRED)**

## Tests
- Unit tests:
  - [x] `<surname/>` empty → exactly one WARN mentioning "empty".
  - [x] `<surname>0009-0008-3948-7334</surname>` → one WARN mentioning ORCID.
  - [x] `<surname>3 0000-0003-3513-3391</surname>` → one WARN (digits/ORCID).
  - [x] `<surname>Bruzi</surname>` and `<surname>Barboza-Barquero</surname>` → no report entries.
  - [x] Article with 7 broken and 1 clean contributor → 7 WARNs.
  - [x] Serialized XML before and after `Apply` is identical.
- Integration tests:
  - [x] `RuleRegistrationTests`: Phase 3 injector order is OtherId, EditedBy, DataAvailability, ContribNames, CreditRoles.
  - [x] Pipeline run over a fixture with one broken surname produces the WARN and still injects CRediT for the resolvable authors.
- Test coverage target: >=80%
- All tests must pass

## Success Criteria
- All tests passing
- Test coverage >=80%
- 5316/5613/5501 reports (task_09) show 7/1/1 `contrib-names` WARNs respectively.
