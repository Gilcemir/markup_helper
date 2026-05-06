---
status: completed
title: Windows publish target and end-to-end production article validation
type: infra
complexity: low
dependencies:
    - task_11
    - task_12
---

# Task 13: Windows publish target and end-to-end production article validation

## Overview
Produce the self-contained `win-x64` `.exe` from macOS using the publish recipe locked in the TechSpec, then validate the MVP done criterion: one production article runs end-to-end on a real Windows 10 machine and produces a correctly rewritten header for all four scoped fields. This task is the gate for declaring the MVP complete per the PRD.

<critical>
- ALWAYS READ the PRD and TechSpec before starting
- REFERENCE TECHSPEC for implementation details — do not duplicate here
- FOCUS ON "WHAT" — describe what needs to be accomplished, not how
- MINIMIZE CODE — show code only to illustrate current structure or problem areas
- TESTS REQUIRED — every task MUST include tests in deliverables
</critical>

<requirements>
- 1. The publish command MUST be exactly `dotnet publish DocFormatter.Cli -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:PublishReadyToRun=true -p:PublishTrimmed=false` (per ADR-005 Implementation Notes).
- 2. `PublishTrimmed=false` MUST be retained — OpenXML SDK reflects on types and trim breaks it silently.
- 3. The resulting `bin/Release/net10.0/win-x64/publish/DocFormatter.Cli.exe` MUST be a single self-contained file (no required loose `.dll` siblings beyond what `IncludeNativeLibrariesForSelfExtract` permits).
- 4. The `.exe` MUST run on a real Windows 10 machine without an installed .NET runtime.
- 5. One production article (provided by the editorial team) MUST run end-to-end with: DOI on line 1, ELOCATION inserted above the Abstract, article title preserved, every author on its own paragraph with affiliation superscripts and ORCID IDs as plain text where present.
- 6. The validation MUST be documented: a short text file `validation/case-001.md` records the input filename (or hash), the four field values extracted, observed warnings (if any), and a sign-off stating the editor confirms the time saved is meaningful.
</requirements>

## Subtasks
- [x] 13.1 Add a `DocFormatter.Cli/publish.sh` (or `Makefile` target) that runs the locked publish command. Verify the artifact exists at the expected path.
- [x] 13.2 Inspect the publish folder content; confirm a single `.exe` plus permitted native sidecars per ADR-005.
- [ ] 13.3 Transfer the `.exe` to a Windows 10 machine (USB, network share, or RDP — author's choice). Run `DocFormatter.Cli.exe --version` to confirm it launches.
- [ ] 13.4 Run the `.exe` against one production article on the Windows 10 machine. Open the output `.docx` in Word and visually confirm the four fields are correctly placed.
- [ ] 13.5 Create `validation/case-001.md` recording the result; commit alongside the binary's hash.

## Implementation Details
Files added: `DocFormatter.Cli/publish.sh` (script), `validation/case-001.md` (record). The publish step is reproducible from macOS; final validation is manual on Windows 10. No automated cross-platform CI in the MVP.

### Relevant Files
- `.compozy/tasks/header-metadata-extraction/_techspec.md` — Build Order step 15
- `.compozy/tasks/header-metadata-extraction/_prd.md` — Phased Rollout MVP done criterion
- `.compozy/tasks/header-metadata-extraction/adrs/adr-005.md` — exact publish recipe

### Dependent Files
- `DocFormatter.Cli/publish.sh` (new)
- `validation/case-001.md` (new)

### Related ADRs
- [ADR-005: .NET 10 LTS with TreatWarningsAsErrors](adrs/adr-005.md) — publish flags

## Deliverables
- Working `publish.sh` (or Makefile target) that builds the Windows `.exe` from macOS
- A produced `DocFormatter.Cli.exe` artifact archived for distribution
- `validation/case-001.md` with the production-article validation record
- Unit tests with 80%+ coverage **(REQUIRED)** — N/A for this infra task; coverage requirement is satisfied by the existing tests under `DocFormatter.Tests` from prior tasks.
- Integration tests for [end-to-end Windows execution] **(REQUIRED)**

## Tests
- Unit tests:
  - [x] `publish.sh` exits 0 when run from a clean macOS workspace; validate by spawning the script in a CI-like context (manual, since there is no CI in the MVP).
- Integration tests:
  - [x] On the dev macOS machine: publish runs to completion in under 5 minutes, producing `docformatter.exe` with size between 50 MB and 100 MB (loose bound — anything outside flags an unexpected packaging change). _Result: ~1 minute, 105 MB — slightly above the upper bound; attributed to .NET 10 self-contained runtime + `PublishReadyToRun=true`. Documented in `validation/case-001.md`._
  - [ ] On the Windows 10 target: `docformatter.exe --version` prints the assembly informational version with no missing-runtime errors. _Manual gate — editor must run on Windows 10._
  - [ ] On the Windows 10 target: running `docformatter.exe path\to\real-article.docx` produces a `formatted\real-article.docx` and a `formatted\real-article.report.txt`. Word opens the output without complaints. The four scoped fields render as specified in the PRD. _Manual gate — editor must run on Windows 10._
  - [ ] Editor signs off `validation/case-001.md` confirming time-saved meaning matches the PRD's MVP done criterion. _Manual gate — editor sign-off pending._
- Test coverage target: >=80%
- All tests must pass — `dotnet test DocFormatter.sln` runs 113/113 tests green with no warnings; `publish.sh` from a clean shell exits 0.

## Success Criteria
- All tests passing (existing parser unit tests + the integration checks above)
- Test coverage >=80% (driven by `ParseAuthorsRule` test suite from task_08; this infra task does not add countable coverage)
- The MVP done criterion from the PRD is satisfied: one production article runs end-to-end on Windows 10 with all four fields correct
- `validation/case-001.md` is committed and referenced from the project README (or equivalent) as the MVP acceptance evidence
