---
status: completed
title: Implement RewriteAbstractRule
type: backend
complexity: high
dependencies:
  - task_01
  - task_02
  - task_07
---

# Task 08: Implement RewriteAbstractRule

## Overview
Add the fourth Phase 2 Optional rule: split the abstract paragraph into a bold `Abstract` heading paragraph and a plain-text body paragraph (preserving genuinely localized italic per ADR-002), and surface the `Corresponding author: <email>` line immediately above the new heading. The rule also detects pre-existing typed "corresponding author"-ish lines (typo-tolerant via ADR-003), replaces them with the canonical line when an email is available, and falls back to extracting an email from the typed line when `ExtractCorrespondingAuthorRule` did not produce one.

<critical>
- ALWAYS READ the PRD and TechSpec before starting
- REFERENCE TECHSPEC for implementation details — do not duplicate here
- FOCUS ON "WHAT" — describe what needs to be accomplished, not how
- MINIMIZE CODE — show code only to illustrate current structure or problem areas
- TESTS REQUIRED — every task MUST include tests in deliverables
</critical>

<requirements>
- MUST live in `DocFormatter.Core/Rules/RewriteAbstractRule.cs` and implement `IFormattingRule` with `Severity = Optional`, `Name = nameof(RewriteAbstractRule)`, and a single `FormattingOptions` constructor dependency.
- MUST locate the abstract paragraph using `_options.AbstractMarkers` (same logic as `LocateAbstractAndInsertElocationRule`).
- MUST split the abstract paragraph into two new paragraphs: a heading carrying a single bold run with the literal text `Abstract` (English, even when the source uses `Resumo`), and a body paragraph containing the original body content with the structural italic wrapper conditionally stripped per ADR-002.
- The italic decision MUST follow ADR-002 exactly: every non-whitespace-bearing run italic → strip italic from every run and emit `[INFO]` "structural italic wrapper removed from abstract body"; otherwise leave run-level italic intact.
- MUST scan front-matter paragraphs (between the last author paragraph and the abstract paragraph) for a pre-existing line matching `_options.CorrespondingAuthorLabelRegex` and apply the four-branch action table from PRD Feature 5 (email available + line found / email available + no line / no email + line found / no email + no line).
- When `ctx.CorrespondingEmail` is null AND a pre-existing typed line is matched, MUST run `_options.EmailRegex` against that paragraph; on match, populate `ctx.CorrespondingEmail` and replace the line with the canonical version (`[INFO]` "recovered email from pre-existing corresponding-author line"). On miss, MUST leave the typed line untouched.
- When inserting the canonical line, MUST place it as a fresh paragraph immediately before the new heading paragraph with the literal text `Corresponding author: <email>` and the document's default font/size (matching `RewriteHeaderMvpRule.CreateBaseRunProperties()`).
- MUST emit `[WARN]` "Abstract paragraph not found" and no-op when no abstract paragraph is located. In this branch, no corresponding-author insertion happens.
- MUST NOT touch the Keywords paragraph or any non-abstract content.
- MUST run BEFORE `LocateAbstractAndInsertElocationRule` so the ELOCATION rule continues to insert above the (now-rewritten) heading paragraph.
</requirements>

## Subtasks
- [x] 8.1 Implement abstract location (reuse the marker logic from `LocateAbstractAndInsertElocationRule`) and the split into heading + body paragraphs.
- [x] 8.2 Implement the ADR-002 italic heuristic (`BodyItalicIsStructuralWrapper`) and conditional italic stripping.
- [x] 8.3 Implement the front-matter scan for pre-existing typed lines using `CorrespondingAuthorLabelRegex` and the four-branch action table.
- [x] 8.4 Implement the canonical `Corresponding author: <email>` paragraph builder and insertion immediately above the heading.
- [x] 8.5 Normalize `Resumo` heading text to `Abstract` while leaving the body language untouched.
- [x] 8.6 Write `RewriteAbstractRuleTests` covering every branch listed in TechSpec "Testing Approach → Unit Tests".
- [x] 8.7 Verify the rule runs cleanly when `ctx.CorrespondingEmail` is null and no typed line exists (full no-op for the email path; heading split still happens).

## Implementation Details
New file under `DocFormatter.Core/Rules/`. The rule reuses the run-properties builder from `RewriteHeaderMvpRule` (`CreateBaseRunProperties()` is `internal static`). Italic stripping operates on `Run.RunProperties?.Italic`; remove the element entirely rather than setting `Val=false`. Front-matter scanning walks paragraphs from `ctx.AuthorParagraphs[^1].NextSibling()` (or, if `AuthorParagraphs` is empty, from the body's first paragraph) up to but not including the abstract paragraph. See TechSpec "Data Flow Between Components" for the action ordering.

### Relevant Files
- `DocFormatter.Core/Rules/LocateAbstractAndInsertElocationRule.cs` — precedent for abstract location.
- `DocFormatter.Core/Rules/RewriteHeaderMvpRule.cs` — `CreateBaseRunProperties()` and paragraph builder patterns.
- `DocFormatter.Core/Options/FormattingOptions.cs` — provides `AbstractMarkers`, `CorrespondingAuthorLabelRegex`, `EmailRegex` (extended in task_02).
- `DocFormatter.Core/Pipeline/FormattingContext.cs` — source of `CorrespondingEmail` and the corresponding-author state (extended in task_01).
- `DocFormatter.Core/Pipeline/IFormattingRule.cs` — interface to implement.

### Dependent Files
- `DocFormatter.Core/Reporting/DiagnosticDocument.cs` — task_09 will populate `DiagnosticAbstract` and `DiagnosticCorrespondingEmail` from this rule's report entries.
- `DocFormatter.Cli/CliApp.cs` — task_10 registers this rule **before** `LocateAbstractAndInsertElocationRule`.

### Related ADRs
- [ADR-002: Structural-italic stripping heuristic](../adrs/adr-002-italic-preservation-heuristic.md) — defines the italic decision rule.
- [ADR-003: Marker tokenization and email regex](../adrs/adr-003-corresponding-author-tokenization.md) — defines `CorrespondingAuthorLabelRegex` and the fallback email recovery path.
- [ADR-001: Four discrete Optional rules](../adrs/adr-001-four-discrete-rules.md) — pipeline ordering.

## Deliverables
- `DocFormatter.Core/Rules/RewriteAbstractRule.cs` implementing the heading split, italic heuristic, and corresponding-author email insertion.
- `DocFormatter.Tests/RewriteAbstractRuleTests.cs` covering every branch.
- Unit tests with 80%+ coverage **(REQUIRED)**.
- Coverage of mixed-italic vs uniform-italic, the four-branch action table, the false-positive guard for `Correspondence:`, and the `Resumo` → `Abstract` normalization **(REQUIRED)**.

## Tests
- Unit tests:
  - [x] Uniform italic: `*Abstract - lorem ipsum*` → two paragraphs (`**Abstract**` + plain-text body); `[INFO]` "structural italic wrapper removed"; body has no run-level italic remaining.
  - [x] Mixed italic: `*Abstract - lorem* *Aedes aegypti* *more text*` with at least one non-italic run → only the heading is rewritten; the body keeps its original run-level italic settings (no stripping).
  - [x] `Resumo - …` source → heading paragraph carries the literal text `Abstract`; body keeps the source language untouched.
  - [x] `ctx.CorrespondingEmail = "foo@x.com"` and no pre-existing line → a `Corresponding author: foo@x.com` paragraph is inserted immediately above the heading.
  - [x] `ctx.CorrespondingEmail = "foo@x.com"` and front matter contains `Corresponding Author: foo@x.com` (canonical) → typed line removed, canonical line inserted; `[INFO]` logged.
  - [x] `ctx.CorrespondingEmail = "foo@x.com"` and front matter contains `coresponding author - foo@x.com` (lowercase, missing 'r', dash separator) → matched and replaced with the canonical line.
  - [x] `ctx.CorrespondingEmail = "foo@x.com"` and front matter contains `Correspondent Autor foo@x.com` (no separator, Portuguese-ish) → matched and replaced with the canonical line.
  - [x] `ctx.CorrespondingEmail == null` and front matter contains `Corresponding Author: bar@y.edu` → fallback recovery sets `ctx.CorrespondingEmail = "bar@y.edu"`, replaces typed line with canonical version; `[INFO]` "recovered email from pre-existing corresponding-author line".
  - [x] `ctx.CorrespondingEmail == null` and front matter contains `Corresponding author:` (no email) → typed line is left untouched; no canonical insertion; no `[WARN]`.
  - [x] `ctx.CorrespondingEmail == null` and no typed line → no email paragraph is inserted; heading split still happens.
  - [x] Abstract paragraph not found → `[WARN]` "Abstract paragraph not found"; rule no-ops; no email paragraph is inserted.
  - [x] False-positive guard: a paragraph starting with `Correspondence:` does NOT match the typed-line regex.
  - [x] Marker found but no `-`/`:` separator after `Abstract` → `[WARN]` logged; heading still rewritten; body remains the original post-marker text.
- Integration tests:
  - [ ] End-to-end pipeline (covered by task_10) on a `*`-marked production fixture verifies the bold `Abstract` heading, the email line above it, and a body without the structural italic wrapper.
- Test coverage target: >=80%
- All tests must pass

## Success Criteria
- All tests passing
- Test coverage >=80%
- Rule operates as Optional and never aborts the document.
- All four PRD action-table branches are exercised by tests; the fallback email recovery path is verified.
- Internal italic emphasis (e.g., species names) is preserved when the body contains any non-italic run.
