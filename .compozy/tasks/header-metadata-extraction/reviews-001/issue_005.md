---
provider: manual
pr:
round: 1
round_created_at: 2026-05-06T17:49:40Z
status: resolved
file: DocFormatter.Core/Rules/ExtractTopTableRule.cs
line: 57
severity: medium
author: claude-code
provider_ref:
---

# Issue 005: Positional fallback assigns cell[1] to ElocationId without shape check

## Review Comment

When `TryHeaderMapping` returns null (headers not present), the rule trusts
the column order blindly:

```csharp
report.Warn(Name, "headers absent, fell back to positional mapping");
idValue = cellTexts[0];
elocationValue = cellTexts[1];
doiValue = cellTexts[2];
```

The DOI value gets a regex re-validation and a cross-cell scan if it doesn't
match. ELOCATION gets nothing — whatever is in cell[1] becomes `ctx.ElocationId`
verbatim. If a production article reorders cells (e.g., `[id, doi, elocation]`),
the DOI string ends up as `ElocationId` and gets inserted as the ELOCATION line
above the abstract. This is exactly the kind of silent corruption the PRD's
risk register warns against ("Top-table column order in production articles
deviates from `id|elocation|doi`. Likelihood: low ... but **high impact** if
it happens").

The current "headers absent, fell back to positional mapping" warning does not
distinguish between "all three cells parsed positionally with confidence" and
"we have no idea which column is which." With column-order ambiguity, the
ELOCATION cell could plausibly be any of the three.

Suggested fix: validate ELOCATION shape too. The PRD shows ELOCATION values
like `e2024001` — a stable pattern (`e\d+` or `e[0-9a-z]+`). Define an
`ElocationRegex` in `FormattingOptions`, and in the positional-fallback path
scan all three cells for an ELOCATION-shaped value. If none match, set
`ctx.ElocationId = null` and emit a `[WARN]` rather than silently writing a
wrong value to the document.

## Triage

- Decision: `VALID`
- Root cause: in the positional fallback branch, ELOCATION is taken from
  cell[1] verbatim with no shape validation. If the column order deviates,
  a DOI or arbitrary string ends up serialized as the ELOCATION line above
  the abstract — exactly the silent-corruption scenario in the PRD risk
  register.
- Fix approach: add `ElocationRegex` to `FormattingOptions` (`^[eE]\d+$`).
  In the positional-fallback branch, validate the proposed ELOCATION value;
  if it doesn't match, scan the other cells for an ELOCATION-shaped value;
  if none match, set the value to empty (which downstream renders as a
  missing field with `[WARN]`). Update the existing
  `Apply_WhenDoiCellInvalid_FindsDoiInAnotherCell_AndLogsWarn` test (cell[1]
  there is a DOI string and shouldn't survive as ELOCATION) and add a new
  test where columns are reordered to `[id, doi, elocation]` and assert the
  fix recovers ELOCATION from cell[2].
- Notes:
