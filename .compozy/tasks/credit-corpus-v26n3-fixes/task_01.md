---
status: pending
title: Tiered and variant candidate initials in AuthorInitialsResolver
type: bugfix
complexity: medium
dependencies: []
---

# Task 1: Tiered and variant candidate initials in AuthorInitialsResolver

## Overview
Make bare-initials resolution prefer full-name candidates over given-names-only candidates, and generate initials variants for hyphenated surnames and capitalized particles. This clears the v26n3 failures "SA" (Ambiguous), "LBB" (NotFound) and "TVB" (NotFound) while keeping the unique-match requirement of ADR-005 (phase-3-jats-tags).

<critical>
- ALWAYS READ the PRD and TechSpec before starting
- REFERENCE TECHSPEC for implementation details — do not duplicate here
- FOCUS ON "WHAT" — describe what needs to be accomplished, not how
- MINIMIZE CODE — show code only to illustrate current structure or problem areas
- TESTS REQUIRED — every task MUST include tests in deliverables
</critical>

<requirements>
- MUST build candidates in two tiers per contributor: Tier 1 = full combinations (given+surname+suffix, given+surname, surname+given); Tier 2 = given-names-only (TechSpec "Resolver").
- MUST match a bare initials key against Tier 1 across all contributors first, and use Tier 2 only when no contributor matched in Tier 1; uniqueness is required within the matching tier.
- MUST expand a hyphenated token (`-`, `‐` U+2010, `–` U+2013) into two variants: first initial only, and one initial per sub-token.
- MUST expand a capitalized particle (`Van`, `Da`, …) into two variants: dropped and kept; a lowercase particle is always dropped.
- MUST combine per-token variants by cartesian product to form the candidate sets.
- MUST keep `ResolveBySurname` narrowing working over the union of both tiers.
- MUST expose the initials-block predicate as `internal static bool IsInitialsToken(string)` for reuse by the parser (task_02).
- MUST leave "TTR", "JGS", "MRC" as NotFound for the v26n3 names (no name-token skipping variants).
</requirements>

## Subtasks
- [ ] 1.1 Replace the flat `CandidateInitials` set with a two-tier structure and update `Resolve` to try Tier 1 before Tier 2.
- [ ] 1.2 Replace `Initials(string)` with a variant generator handling hyphens and particle casing, combined by cartesian product.
- [ ] 1.3 Make `IsInitialsToken` internal for the parser.
- [ ] 1.4 Add resolver tests for the v26n3 cases and the tier/variant rules.
- [ ] 1.5 Run the full test suite; all existing resolver and injector tests stay green.

## Implementation Details
Modify `DocFormatter.Core/Jats/AuthorInitialsResolver.cs`: `Resolve` (bare-initials branch), `ResolveBySurname` (narrowing), `CandidateInitials`, `Initials`, `Particles`, `IsInitialsToken` visibility. See TechSpec "Resolver (`AuthorInitialsResolver`)" for the candidate and variant rules, and ADR-004 for rationale. `Fold` and `NormalizeInitials` are unchanged.

### Relevant Files
- `DocFormatter.Core/Jats/AuthorInitialsResolver.cs` — the resolver being changed (candidates, variants, tiers).
- `DocFormatter.Tests/Jats/AuthorInitialsResolverTests.cs` — existing tests and the `Contrib(surname, givenNames, suffix)` XElement helper to reuse.

### Dependent Files
- `DocFormatter.Core/Jats/CreditRolesInjector.cs` — calls `AuthorInitialsResolver.Resolve` in `BuildPlan`; behavior change is transparent.
- `DocFormatter.Core/Jats/CreditStatementParser.cs` — will consume `IsInitialsToken` in task_02.
- `DocFormatter.Tests/Jats/CreditRolesInjectorTests.cs` — end-to-end expectations on resolution must remain green.

### Related ADRs
- [ADR-004: Tiered, broadened candidate initials in the author resolver](../adrs/adr-004.md) — defines the tiers and variant generation implemented here.

## Deliverables
- Two-tier, variant-based candidate generation in `AuthorInitialsResolver`.
- `IsInitialsToken` available to the parser.
- Unit tests with 80%+ coverage **(REQUIRED)**
- Integration tests: existing `CreditRolesInjectorTests` green **(REQUIRED)**

## Tests
- Unit tests:
  - [ ] "SA" with contribs Abid/Saleem, Khan/Sajjad Ahmad, Sohail/Muhammad, Zahid/Saleem resolves to Abid (Tier 1 wins over Tier 2).
  - [ ] "LBB" resolves to Barboza-Barquero/Luis; "LB" also resolves to the same contributor.
  - [ ] "TVB" resolves to Bui/Truong Van; "TB" also resolves to Bui.
  - [ ] "MS" resolves to "Da Silva"/"Maria" (capitalized particle dropped variant) and "MDS" resolves too (kept variant).
  - [ ] "TTR" against Rocha/"Taine Teotônio Teixeira da" is NotFound; "JGS" against Simão/"Janine Magalhães Guedes" is NotFound; "MRC" against Costa/"Marcia" is NotFound.
  - [ ] Two contributors both matching a key in Tier 1 → Ambiguous (tiering does not remove ambiguity within a tier).
  - [ ] A key matching one contributor only in Tier 2 while no Tier 1 match exists → Resolved (Tier 2 fallback still works, e.g. 5316-style full name in given-names).
  - [ ] Existing tests (`ATAJ`, surname+initials forms) unchanged and green.
- Integration tests:
  - [ ] `CreditRolesInjectorTests` and `Phase3PipelineIntegrationTests` pass without modification.
- Test coverage target: >=80%
- All tests must pass

## Success Criteria
- All tests passing
- Test coverage >=80%
- "SA", "LBB", "TVB" resolve uniquely on the v26n3 contributor sets; "TTR", "JGS", "MRC" stay NotFound.
- No change in outcome for any existing resolver test.
