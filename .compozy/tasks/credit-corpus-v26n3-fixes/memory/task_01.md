# Task Memory: task_01.md

Keep only task-local execution context here. Do not duplicate facts that are obvious from the repository, task file, PRD documents, or git history.

## Objective Snapshot

- Two-tier (Full / GivenOnly) variant-based candidate initials in `AuthorInitialsResolver`; `IsInitialsToken` internal for task_02. Done 2026-09-14.

## Important Decisions

- Tiers modeled as a private nested record `CandidateTiers(Full, GivenOnly)` with `ContainsInAnyTier` for the surname-narrowing union; no public surface change.
- Cartesian product is a simple fold (`Combine`) over per-token variant lists; a name part with no letters yields `{""}` so the product never collapses to empty.
- A hyphenated token whose sub-tokens fold to a single initial (e.g. `"-Silva"`) yields one variant, not a duplicate pair.
- Particle "kept" variant only when `char.IsUpper(token[0])`; lookup stays case-insensitive as ADR-004 says.

## Learnings

- Red signal before the change: 7 of 26 resolver tests failed (SA, LBB, LB, unicode hyphens, TVB, MDS, given-only fallback, hyphen narrowing). After: 828/828 green (811 baseline + 17 new).
- Surname made of an ORCID (5316-style) folds to a digit initial (`"0"`), so Full candidates never match a letters-only key and the GivenOnly tier is what resolves it. Tested.

## Files / Surfaces

- `DocFormatter.Core/Jats/AuthorInitialsResolver.cs` — Resolve bare branch, ResolveBySurname narrowing, CandidateTiers/CandidateInitials, InitialsVariants/TokenVariants/Initial/Combine, Hyphens set, IsInitialsToken internal.
- `DocFormatter.Tests/Jats/AuthorInitialsResolverTests.cs` — 11 new test methods (17 cases) appended after `Resolve_Empty_IsNotFound`.

## Errors / Corrections

- None during implementation.

## Ready for Next Run

- task_02 can call `AuthorInitialsResolver.IsInitialsToken(string)` (internal, same assembly).
- Follow-up (out of scope): `dotnet format --verify-no-changes` on the whole solution fails with 4 pre-existing WHITESPACE errors in `DocFormatter.Tests/Phase2/Phase2PipelineIntegrationTests.cs` (lines 261–263); present on the committed tree before this task.
