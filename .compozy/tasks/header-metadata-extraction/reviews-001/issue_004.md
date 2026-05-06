---
provider: manual
pr:
round: 1
round_created_at: 2026-05-06T17:49:40Z
status: resolved
file: DocFormatter.Core/Rules/ExtractTopTableRule.cs
line: 188
severity: medium
author: claude-code
provider_ref:
---

# Issue 004: GetCellPlainText drops <w:br/> and <w:tab/> separators

## Review Comment

`GetCellPlainText` concatenates `<w:t>` runs with no separator within a paragraph
and joins paragraphs with `'\n'`:

```csharp
private static string GetCellPlainText(TableCell cell)
{
    var paragraphs = cell.Elements<Paragraph>()
        .Select(p => string.Concat(p.Descendants<Text>().Select(t => t.Text)));
    return string.Join('\n', paragraphs);
}
```

`Descendants<Text>()` only collects `<w:t>` elements. Soft line breaks
(`<w:br/>`) and tabs (`<w:tab/>`) — both common in cells where the editor
typed "id<Enter>ART01" via Shift+Enter (which Word records as `<w:br/>`,
not as a paragraph break) — are silently dropped. The header-detection logic
in `DetectHeader` then sees `idART01` as a single line and falls into the
positional fallback path with a `[WARN]`, even though the cell was correctly
formatted.

Suggested fix: walk `paragraph.Descendants()` and emit `'\n'` for `Break`
elements and `'\t'` for `TabChar` elements alongside `Text` content, e.g.:

```csharp
foreach (var node in paragraph.Descendants())
{
    if (node is Text t) builder.Append(t.Text);
    else if (node is Break) builder.Append('\n');
    else if (node is TabChar) builder.Append('\t');
}
```

Add a unit test using a cell with `<w:r><w:t>id</w:t><w:br/><w:t>ART01</w:t></w:r>`
and assert header mapping succeeds without falling back.

## Triage

- Decision: `VALID`
- Root cause: `Descendants<Text>()` collects only `<w:t>` nodes; `<w:br/>`
  (Shift+Enter inside a cell — the most natural way to type "id" + "ART01"
  on two lines without a paragraph break) and `<w:tab/>` are dropped, so the
  cell text becomes `idART01` and the header detector falls through to the
  positional path with a `[WARN]`.
- Fix approach: walk paragraph descendants and emit characters per node type
  (`Text` → text, `Break` → `\n`, `TabChar` → `\t`). Add a unit test that
  builds a cell with `<w:r><w:t>id</w:t><w:br/><w:t>ART01</w:t></w:r>` and
  asserts the header path is taken (no positional warning).
- Notes:
