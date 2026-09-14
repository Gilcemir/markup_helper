---
status: completed
title: Plain-text ORCID token and repeated-token WARN in ExtractAuthorsRule
type: bugfix
complexity: medium
dependencies: []
---

# Task 8: Plain-text ORCID token and repeated-token WARN in ExtractAuthorsRule

## Overview
Make Phase 1 extract an ORCID written as plain text in the byline (including glued to the surname or prefixed by the orcid.org URL) as the author's `OrcidId`, and warn when the last token of a name repeats an earlier token. This prevents SciELO Markup's `mark_authors` from promoting the ORCID to `<surname>` (5316) and announces the `Nguyen Hoai Nguyen` mis-split (5613) before it happens.

<critical>
- ALWAYS READ the PRD and TechSpec before starting
- REFERENCE TECHSPEC for implementation details — do not duplicate here
- FOCUS ON "WHAT" — describe what needs to be accomplished, not how
- MINIMIZE CODE — show code only to illustrate current structure or problem areas
- TESTS REQUIRED — every task MUST include tests in deliverables
</critical>

<requirements>
- MUST add a generated regex for plain-text ORCIDs without a leading word boundary and with an optional `https://orcid.org/` prefix (TechSpec "Phase 1").
- MUST emit `TokenKind.Orcid` tokens from non-superscript run text in `AppendRunTokens`, with the surrounding text as `Text` tokens, and an INFO report line per extraction.
- MUST warn (without lowering confidence) when a builder already holds a different ORCID from a hyperlink.
- MUST warn (without lowering confidence) in `FlagSuspicions` when the folded last token of a name equals a folded earlier token; the message names Markup's first-occurrence behavior.
- MUST NOT pre-mark `[author]`/`[fname]`/`[surname]` (Phase 2 ADR-001 invariant).
- MUST keep `make phase2-verify` green over `examples/phase-2/{before,after}/`.
</requirements>

## Subtasks
- [x] 8.1 Add the plain-text ORCID regex and the tokenizer split in `AppendRunTokens` (pass the report into the static method).
- [x] 8.2 Add an `AuthorBuilder.Warn` that records a warning without changing confidence; use it for conflicting ORCID and repeated token.
- [x] 8.3 Add the repeated-token check in `FlagSuspicions` using diacritics/case folding.
- [x] 8.4 Extend `ExtractAuthorsRuleTests` with the byline shapes below, using `AuthorsParagraphFactory`.
- [x] 8.5 Run `make test` and `make phase2-verify`.

## Implementation Details
Modify `DocFormatter.Core/Rules/ExtractAuthorsRule.cs` (`AppendRunTokens`, `ConsumeTokens` Orcid case, `FlagSuspicions`, `AuthorBuilder`). `FormattingOptions.OrcidIdRegex` (`\b`-anchored) stays for URLs. `RewriteHeaderMvpRule.BuildAuthorParagraph` is unchanged and will now place the ORCID after the label. See TechSpec "Phase 1 (`ExtractAuthorsRule`)" and ADR-006.

### Relevant Files
- `DocFormatter.Core/Rules/ExtractAuthorsRule.cs` — tokenizer, consumer, suspicion flags.
- `DocFormatter.Core/Options/FormattingOptions.cs` — existing ORCID regex and URL marker for reference.
- `DocFormatter.Tests/ExtractAuthorsRuleTests.cs` — existing tests.
- `DocFormatter.Tests/Fixtures/Authors/AuthorsParagraphFactory.cs` — `TextRun`, `SuperscriptRun`, `Hyperlink` builders.

### Dependent Files
- `DocFormatter.Core/Rules/RewriteHeaderMvpRule.cs` — consumes `Author.Name`/`OrcidId`; output layout changes only for bylines that carried plain ORCIDs.
- `DocFormatter.Core/Reporting/DiagnosticWriter.cs` — Phase 1 diagnostic `HasAuthorid` now true for plain ORCIDs.
- `examples/phase-2/{before,after}/` — Phase 2 diff gate corpus (read-only).

### Related ADRs
- [ADR-006: A plain-text ORCID in the byline is tokenized like a hyperlink ORCID](../adrs/adr-006.md) — token approach and repeated-token WARN.

## Deliverables
- Plain-text ORCID extraction and repeated-token WARN.
- Unit tests with 80%+ coverage **(REQUIRED)**
- Integration tests: `make phase2-verify` green **(REQUIRED)**

## Tests
- Unit tests:
  - [x] Run "Pablo de Sousa Arantes 0009-0008-3948-7334" + superscript "1" → name "Pablo de Sousa Arantes", OrcidId "0009-0008-3948-7334", label "1", one INFO.
  - [x] Run "Adriano Teodoro Bruzi0000-0001-6909-5157" (glued) → name "Adriano Teodoro Bruzi", OrcidId set.
  - [x] Run "Ana Silva https://orcid.org/0000-0002-1825-0097" → name "Ana Silva", OrcidId "0000-0002-1825-0097", URL not left in the name.
  - [x] Hyperlink ORCID A plus plain-text ORCID B on the same author → OrcidId A kept, one WARN mentioning conflict, confidence High.
  - [x] Two authors "Ana Silva 0000-0001-0000-0001, Bruno Costa 0000-0002-0000-0002" → each author gets its own ORCID.
  - [x] Name "Nguyen Hoai Nguyen" → one WARN naming the repeated token, confidence High.
  - [x] Name "Ana Maria Silva" → no repeated-token WARN.
  - [x] Existing 5313/5449 shape tests unchanged.
- Integration tests:
  - [x] `make phase2-verify` reports no diff against `examples/phase-2/after/`.
- Test coverage target: >=80%
- All tests must pass

## Success Criteria
- All tests passing
- Test coverage >=80%
- Phase 2 diff gate green.
