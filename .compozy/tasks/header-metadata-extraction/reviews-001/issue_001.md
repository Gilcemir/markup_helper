---
provider: manual
pr:
round: 1
round_created_at: 2026-05-06T17:49:40Z
status: resolved
file: DocFormatter.Core/Options/FormattingOptions.cs
line: 17
severity: critical
author: claude-code
provider_ref:
---

# Issue 001: DoiRegex won't match URL-wrapped DOIs in production input

## Review Comment

`DoiRegex` is anchored: `^10\.\d{4,9}/[-._;()/:A-Z0-9]+$`. This fails on the
exact input shape that production articles use today.

The workflow memory (`memory/MEMORY.md`, task_05 handoff) records:

> Real `examples/*.docx` wrap the DOI in a hyperlink to `http://dx.doi.org/...`;
> the anchored DoiRegex will not match that form, so production-article runs
> currently land in the `Doi=null` + cross-cell-scan-miss branch. Task 09/11
> will need a URL-prefix strip (or the regex must be relaxed) before the rewrite
> rule emits Line 1.

This was flagged as an open work item for tasks 09/11 but neither task addressed
it. As shipped:

- `ExtractTopTableRule` (line 68) tests the cell text against the anchored regex
  → fails for `http://dx.doi.org/10.1234/abc`.
- The cross-cell fallback also uses the same anchored regex → fails.
- `ctx.Doi` is set to `null`, `RewriteHeaderMvpRule` skips Line 1 with a `[WARN]`,
  and the produced `.docx` is missing the DOI line.

This blocks the PRD's MVP success criterion ("one production article processed
correctly end-to-end with correct DOI"). Confirmed by checking the 12 fixtures
in `examples/`: they are the actual production articles the editor will run
through the tool.

Suggested fix: either relax the regex by removing anchors and stripping URL
prefixes before matching, or add a normalization step inside
`ExtractTopTableRule.GetCellPlainText` (or a dedicated `NormalizeDoi` helper)
that detects a `http(s)://(dx.)?doi.org/` prefix and trims it before regex
matching. Add a test fixture for the URL-wrapped DOI cell and assert
`ctx.Doi == "10.1234/abc"`.

## Triage

- Decision: `VALID`
- Root cause: `DoiRegex` is anchored (`^...$`) and the production input wraps
  the DOI in `http(s)://(dx.)?doi.org/` URL prefixes. The regex never matches,
  the cross-cell scan also doesn't match, and `ctx.Doi` ends up null — directly
  blocking the PRD's MVP success criterion.
- Fix approach: introduce a `DoiUrlPrefixes` list on `FormattingOptions` and
  normalize the cell text (and any cell scanned in fallback) by stripping a
  matching prefix before the regex check. Keep the regex anchored so it remains
  a strict DOI shape check; URL stripping is the seam. Add a unit test using a
  cell whose text is `http://dx.doi.org/10.1234/abc` and assert
  `ctx.Doi == "10.1234/abc"`.
- Notes:
