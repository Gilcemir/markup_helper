---
provider: manual
pr:
round: 1
round_created_at: 2026-05-06T17:49:40Z
status: resolved
file: DocFormatter.Core/Rules/LocateAbstractAndInsertElocationRule.cs
line: 66
severity: high
author: claude-code
provider_ref:
---

# Issue 003: Abstract locator skips paragraphs whose first run is whitespace

## Review Comment

`FindAbstractParagraph` only inspects the first `Run` descendant of each
paragraph:

```csharp
var firstRun = paragraph.Descendants<Run>().FirstOrDefault();
if (firstRun is null || !IsBold(firstRun))
{
    continue;
}

var text = firstRun.InnerText.TrimStart();
if (text.Length == 0)
{
    continue;
}
```

If the first run is whitespace-only or non-bold, the entire paragraph is
skipped — even when a *subsequent* bold run starts with "Abstract" or
"Resumo". This shape is common in `.docx` files produced by Word: leading
empty/style-init runs, paragraph numbers, or a non-bold space run before the
bold heading word.

Real-world impact: `LocateAbstractAndInsertElocationRule` is the rule that
inserts the ELOCATION line above Abstract. Missing the abstract paragraph
silently omits ELOCATION from the output document with a `[WARN]` — exactly
the silent-error case the PRD wants to avoid (PRD Risks: "editor stops
trusting the tool after one bad output").

The PRD heuristic is "first paragraph whose **first run** is bold and whose
text starts with..." but this is tighter than the spirit. Suggested fix:
scan runs in document order and find the first run that satisfies
`IsBold(run) && InnerText.TrimStart().StartsWith(marker, ...)`, OR
concatenate the leading bold run(s) and check the prefix against the markers.
Add a unit test where the abstract paragraph has a leading empty run before
the bold "Abstract" run, and assert ELOCATION is inserted.

## Triage

- Decision: `VALID`
- Root cause: the locator tests only the first run of each paragraph. Word
  commonly emits a leading whitespace-only or styling-init run before the
  bold heading run, so any abstract paragraph with such a leading run is
  silently skipped — and ELOCATION is omitted from the output document.
- Fix approach: scan runs of each paragraph in document order; ignore leading
  whitespace-only runs; on the first non-whitespace run, evaluate the bold +
  marker-prefix predicate and either accept the paragraph or move on to the
  next paragraph. Add a unit test where the abstract paragraph has an empty
  leading run before the bold "Abstract".
- Notes:
