---
status: completed
title: Implement ExtractCorrespondingAuthorRule
type: backend
complexity: high
dependencies:
  - task_01
  - task_02
---

# Task 07: Implement ExtractCorrespondingAuthorRule

## Overview
Add the third Phase 2 Optional rule: detect the `* E-mail:` trailer in an affiliation paragraph, extract email + ORCID, strip the trailer from the affiliation, identify the corresponding author by their `*` marker (in superscript or plain text), and stash the result on `FormattingContext`. The rule runs before `RewriteHeaderMvpRule` so it operates on the original DOM. PRD Feature 4 and ADR-003 specify the exact behavior.

<critical>
- ALWAYS READ the PRD and TechSpec before starting
- REFERENCE TECHSPEC for implementation details — do not duplicate here
- FOCUS ON "WHAT" — describe what needs to be accomplished, not how
- MINIMIZE CODE — show code only to illustrate current structure or problem areas
- TESTS REQUIRED — every task MUST include tests in deliverables
</critical>

<requirements>
- MUST live in `DocFormatter.Core/Rules/ExtractCorrespondingAuthorRule.cs` and implement `IFormattingRule` with `Severity = Optional`, `Name = nameof(ExtractCorrespondingAuthorRule)`, and a single `FormattingOptions` constructor dependency.
- MUST scan paragraphs between the last `ctx.AuthorParagraphs` entry and the first paragraph starting with one of `_options.AbstractMarkers` for the `* E-mail:` marker (using `_options.CorrespondingMarkerRegex`).
- On match, MUST strip every run from the matched `*` to the end of the paragraph, splitting any run that straddles the boundary so the pre-`*` text is preserved.
- MUST stash the affiliation `Paragraph` reference in `ctx.CorrespondingAffiliationParagraph` and apply `_options.EmailRegex` and `_options.OrcidIdRegex` to the stripped trailer; populate `ctx.CorrespondingEmail` / `ctx.CorrespondingOrcid` on success.
- MUST identify the corresponding author by walking `ctx.AuthorParagraphs`: superscript-`*` directly after a label run, OR plain-text `*` immediately after a name. The first match wins; subsequent matches MUST emit a `[WARN]`. Set `ctx.CorrespondingAuthorIndex` to the matched author's index in `ctx.Authors`.
- When the matched author has no existing ORCID and `ctx.CorrespondingOrcid` is non-null, MUST replace that author with `a with { OrcidId = ctx.CorrespondingOrcid }`. When the author already has an ORCID, MUST drop the affiliation ORCID silently (no `[WARN]`).
- When the affiliation paragraph becomes empty after stripping, MUST remove it from the body.
- MUST emit `[INFO]` "no corresponding author marker found" when the affiliation block has no `*` (full no-op for the rest of the rule).
- MUST emit `[WARN]` "corresponding-author marker found but email could not be extracted" when `*` is present but the email regex fails on the trailer; the trailer is still stripped (best-effort cleanup).
- MUST treat single-author papers without a `*` marker as valid input (no `[WARN]`).
</requirements>

## Subtasks
- [x] 7.1 Implement Pass A: scan affiliation paragraphs, find the marker via `CorrespondingMarkerRegex`, strip trailing runs (splitting straddling runs), and populate `CorrespondingAffiliationParagraph` / `CorrespondingEmail` / `CorrespondingOrcid`.
- [x] 7.2 Implement Pass B: walk `ctx.AuthorParagraphs` to identify the first author with a `*` marker (superscript-aware) and set `CorrespondingAuthorIndex`.
- [x] 7.3 Promote the affiliation ORCID onto the corresponding author's record only when the author has no prior ORCID.
- [x] 7.4 Emit the report messages described in TechSpec "Monitoring and Observability" for this rule.
- [x] 7.5 Remove the affiliation paragraph when its institutional text is empty after stripping.
- [x] 7.6 Write `ExtractCorrespondingAuthorRuleTests` covering all branches listed in TechSpec "Testing Approach → Unit Tests".

## Implementation Details
New file under `DocFormatter.Core/Rules/`. Use the same `[GeneratedRegex]` consumption pattern as `ExtractAuthorsRule` (read regexes from `_options`). The plain-text → run-offset mapping helper can live alongside the rule or in `HeaderParagraphLocator` (TechSpec leaves the location flexible). For superscript detection, mirror `IsSuperscript(Run)` from `ExtractAuthorsRule.cs`. See TechSpec "Implementation Design → Data Models" for the immutable `Author` re-creation pattern (`a with { OrcidId = ... }`).

### Relevant Files
- `DocFormatter.Core/Rules/ExtractAuthorsRule.cs` — precedent for tokenization, superscript detection, and `_options` consumption.
- `DocFormatter.Core/Rules/HeaderParagraphLocator.cs` — existing helper for paragraph scanning between markers.
- `DocFormatter.Core/Options/FormattingOptions.cs` — provides `CorrespondingMarkerRegex`, `EmailRegex`, `OrcidIdRegex`, `AbstractMarkers` (extended in task_02).
- `DocFormatter.Core/Pipeline/FormattingContext.cs` — destination of the corresponding-author state (extended in task_01).
- `DocFormatter.Core/Models/Author.cs` — record type used for the `with { ... }` re-creation.

### Dependent Files
- `DocFormatter.Core/Rules/RewriteAbstractRule.cs` — task_08 reads `ctx.CorrespondingEmail` (file does not exist yet).
- `DocFormatter.Core/Reporting/DiagnosticDocument.cs` — task_09 reads this rule's report entries to populate `DiagnosticCorrespondingEmail`.
- `DocFormatter.Cli/CliApp.cs` — task_10 registers this rule in DI **before** `RewriteHeaderMvpRule`.

### Related ADRs
- [ADR-003: Marker tokenization and email regex](../adrs/adr-003-corresponding-author-tokenization.md) — defines the two-pass design, the regex literals, and the empty-affiliation cleanup rule.
- [ADR-001: Four discrete Optional rules](../adrs/adr-001-four-discrete-rules.md) — pipeline ordering (this rule runs **before** `RewriteHeaderMvpRule`).

## Deliverables
- `DocFormatter.Core/Rules/ExtractCorrespondingAuthorRule.cs` implementing both passes.
- `DocFormatter.Tests/ExtractCorrespondingAuthorRuleTests.cs` covering the matrix of `*` placement, mixed-run trailers, ORCID conflicts, and warning paths.
- Unit tests with 80%+ coverage **(REQUIRED)**.
- Coverage for "trailer split across multiple runs" and "affiliation paragraph empty after cleanup" **(REQUIRED)**.

## Tests
- Unit tests:
  - [ ] Affiliation reading `2 Universidade Y * E-mail: foo@y.edu` → `CorrespondingEmail == "foo@y.edu"`; affiliation paragraph keeps `2 Universidade Y` and drops the trailer.
  - [ ] Affiliation reading `... * E-mail: foo@y.edu ORCID: https://orcid.org/0000-0002-1825-0097` → both `CorrespondingEmail` and `CorrespondingOrcid` populated; ORCID promoted to the corresponding author when their existing ORCID is `null`.
  - [ ] Same trailer where the corresponding author already has an ORCID → affiliation ORCID dropped silently; author record's ORCID is unchanged; no `[WARN]`.
  - [ ] Authors paragraph contains `Maria Silva` followed by superscript `1,2*` → `CorrespondingAuthorIndex` points to Maria Silva.
  - [ ] Authors paragraph contains plain-text `Maria Silva*` → `CorrespondingAuthorIndex` points to Maria Silva.
  - [ ] Two `*` markers across authors → first wins; second emits a `[WARN]`.
  - [ ] No `*` anywhere → `[INFO]` "no corresponding author marker found"; rule is a full no-op for the rest of the pipeline.
  - [ ] Marker present but email regex fails on trailer → `[WARN]`; trailer still stripped; `CorrespondingEmail` stays `null`.
  - [ ] Trailer split across multiple OOXML runs (`*` in run A, `E-mail:` in run B) → marker still detected; trailer fully stripped via run-boundary mapping.
  - [ ] Affiliation paragraph that contained ONLY the `*…` content → paragraph removed from the body after cleanup.
- Integration tests:
  - [ ] End-to-end run with a `*`-marked production fixture (covered by task_10) yields the cleaned affiliation, populated `ctx.Corresponding*`, and the surfaced corresponding author.
- Test coverage target: >=80%
- All tests must pass

## Success Criteria
- All tests passing
- Test coverage >=80%
- Rule operates as Optional and never aborts the document.
- The PRD's user stories around corresponding-author extraction are satisfied (email + optional ORCID + clean affiliation).
