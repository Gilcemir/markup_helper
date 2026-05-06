---
status: completed
title: ExtractOrcidLinksRule with relationship cleanup
type: backend
complexity: high
dependencies:
    - task_03
    - task_04
---

# Task 7: ExtractOrcidLinksRule with relationship cleanup

## Overview
Implement the rule that walks the authors paragraph, replaces every `<w:hyperlink>` whose target URL contains `orcid.org` with a plain text `<w:r><w:t>` containing the extracted ORCID ID, removes the corresponding relationship from `document.xml.rels`, and drops any nested `<w:drawing>` ORCID badge image. Extracted IDs are staged in a side table keyed by paragraph offset so `ParseAuthorsRule` (task_08) can attach them to the matching author record. This rule overrides the spec's "remove ORCID" behavior per ADR-003.

<critical>
- ALWAYS READ the PRD and TechSpec before starting
- REFERENCE TECHSPEC for implementation details — do not duplicate here
- FOCUS ON "WHAT" — describe what needs to be accomplished, not how
- MINIMIZE CODE — show code only to illustrate current structure or problem areas
- TESTS REQUIRED — every task MUST include tests in deliverables
</critical>

<requirements>
- 1. The rule MUST locate the authors paragraph using the same heuristic as `ParseAuthorsRule` (third non-empty paragraph after the deleted top table). For decoupling, expose a shared helper in `DocFormatter.Core/Rules/HeaderParagraphLocator.cs` consumed by both rules.
- 2. For every `<w:hyperlink>` in the authors paragraph, the rule MUST resolve `r:id` against `MainDocumentPart.HyperlinkRelationships` and inspect the target URI.
- 3. If the target URI contains `FormattingOptions.OrcidUrlMarker` (`"orcid.org"`), the rule MUST extract the ORCID ID using `OrcidIdRegex`. If no match, log `[WARN]` and leave the hyperlink intact.
- 4. On a successful match the rule MUST replace the entire `<w:hyperlink>` element with one or more `<w:r>` elements that emit the extracted ID as plain text, preserving the original run properties from the inner runs (so superscript/bold survive if present).
- 5. The rule MUST remove the corresponding `HyperlinkRelationship` from the part. Orphan relationships (those no longer referenced anywhere) MUST be cleaned up.
- 6. Any `<w:drawing>` nested inside the original `<w:hyperlink>` MUST be removed if its embedded blip references the same `r:id` (covers the green ORCID badge). Free-standing badges before/after the hyperlink stay intact in the MVP and trigger `[WARN]` if their image relationship target contains `orcid.org`.
- 7. Extracted IDs MUST be staged in a key/value structure attached to the rule output (e.g., a `Dictionary<int, string>` keyed by the run-index of the hyperlink within the paragraph) for `ParseAuthorsRule` to consume.
- 8. The rule severity MUST be `Optional`; failures here do not abort the pipeline.
</requirements>

## Subtasks
- [x] 7.1 Create `DocFormatter.Core/Rules/HeaderParagraphLocator.cs` (shared between this rule and task_08) returning the authors paragraph or `null`.
- [x] 7.2 Create `DocFormatter.Core/Rules/ExtractOrcidLinksRule.cs` implementing `IFormattingRule` with `Severity=Optional`.
- [x] 7.3 Implement hyperlink iteration with relationship resolution and ORCID URL detection.
- [x] 7.4 Implement the in-place replacement of `<w:hyperlink>` with plain runs that preserve inner run properties.
- [x] 7.5 Implement relationship cleanup and nested badge removal.
- [x] 7.6 Stage extracted IDs in a per-run-index dictionary on the context (extend `FormattingContext` with an internal `OrcidStaging` dictionary not exposed publicly outside Core).
- [x] 7.7 Add xUnit tests covering nested-badge case, file-URL ORCID case, and missing-ID case.

## Implementation Details
Files are new under `DocFormatter.Core/Rules/`. See TechSpec "Rules" rule #3 and ADR-003 for the behavior contract. The `OrcidStaging` field is internal to Core — it lives on `FormattingContext` with `internal` visibility and is consumed only by `ParseAuthorsRule`.

### Relevant Files
- `.compozy/tasks/header-metadata-extraction/_techspec.md` — "Rules" rule #3 and "Known Risks"
- `.compozy/tasks/header-metadata-extraction/adrs/adr-003.md` — ORCID extraction contract
- `instructions.md` — original spec's `RemoveOrcidLinksRule` description

### Dependent Files
- `DocFormatter.Core/Rules/HeaderParagraphLocator.cs` (new, shared with task_08)
- `DocFormatter.Core/Rules/ExtractOrcidLinksRule.cs` (new)
- `DocFormatter.Core/Pipeline/FormattingContext.cs` (modified — add internal `OrcidStaging` dictionary)
- `DocFormatter.Tests/ExtractOrcidLinksRuleTests.cs` (new)

### Related ADRs
- [ADR-003: ORCID extraction overrides spec's "remove" behavior](adrs/adr-003.md)

## Deliverables
- `HeaderParagraphLocator` helper
- `ExtractOrcidLinksRule` implementation
- xUnit tests covering each named edge case
- Unit tests with 80%+ coverage **(REQUIRED)**
- Integration tests for [pipeline rule sequence with task_08 consumer] **(REQUIRED)**

## Tests
- Unit tests:
  - [x] Authors paragraph with one hyperlink targeting `https://orcid.org/0000-0002-1825-0097`: `<w:hyperlink>` replaced with a run carrying `0000-0002-1825-0097`, the relationship removed, and `OrcidStaging` contains the ID at the hyperlink's run index.
  - [x] Authors paragraph with one hyperlink targeting `file:///C:/article.docx#orcid.org/0000-0002-1825-0097`: same outcome (URL marker detected, ID extracted from the path).
  - [x] Authors paragraph with a hyperlink targeting `https://example.com`: hyperlink left intact, `OrcidStaging` empty, no `[WARN]`.
  - [x] Authors paragraph with a hyperlink targeting `https://orcid.org/garbled-id`: hyperlink left intact, `[WARN]` emitted.
  - [x] Authors paragraph with a hyperlink containing a nested `<w:drawing>` whose blip references the same `r:id`: drawing removed alongside the hyperlink replacement.
  - [x] Authors paragraph with a free-standing `<w:drawing>` before the hyperlink whose image relationship target contains `orcid.org`: drawing left intact, `[WARN]` emitted.
  - [x] No authors paragraph found (e.g., document missing the third non-empty paragraph): rule logs `[WARN]` and exits without mutation.
- Integration tests:
  - [x] Pipeline run with `ExtractTopTableRule` → `ParseHeaderLinesRule` → `ExtractOrcidLinksRule`: the staging dictionary is non-empty when the input has ORCID links and is consumable from the next rule's context.
- Test coverage target: >=80%
- All tests must pass

## Success Criteria
- All tests passing
- Test coverage >=80%
- ORCID hyperlinks are converted to plain text with the ID preserved per ADR-003
- Removed hyperlink relationships do not leave orphans in `document.xml.rels`
