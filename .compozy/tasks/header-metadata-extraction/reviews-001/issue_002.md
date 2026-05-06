---
provider: manual
pr:
round: 1
round_created_at: 2026-05-06T17:49:40Z
status: resolved
file: DocFormatter.Core/Rules/RewriteHeaderMvpRule.cs
line: 38
severity: high
author: claude-code
provider_ref:
---

# Issue 002: Critical abort on empty Authors contradicts "best-effort" PRD promise

## Review Comment

`RewriteHeaderMvpRule` is registered as `RuleSeverity.Critical` and throws
`InvalidOperationException(EmptyAuthorsMessage)` when `ctx.Authors.Count == 0`:

```csharp
if (ctx.Authors.Count == 0)
{
    throw new InvalidOperationException(EmptyAuthorsMessage);
}
```

This contradicts the PRD in two places:

- Feature 4: "the rule still produces a best-effort list **and** records `[WARN]`
  entries describing the suspicion. **The pipeline never aborts on author
  uncertainty.**"
- Risks: "Author parsing produces plausible but wrong output, editor does not
  notice. Mitigation: emit `[WARN]`..."

Concrete failure: `ExtractOrcidLinksRule` and `ParseAuthorsRule` are both
Optional and exit early with a `[WARN]` when `HeaderParagraphLocator` cannot
find the third non-empty paragraph (e.g., the article has only two paragraphs
between the table and the abstract, or uses extra blank paragraphs that shift
the index). `ctx.Authors` stays empty, then this Critical rule throws,
`FormattingPipeline` rethrows, `FileProcessor` deletes the output `.docx`, and
the CLI returns exit code 2. The editor gets no DOI line, no rewritten header,
no `formatted/<name>.docx` — for an article where DOI/ELOCATION/Title were all
extractable.

Suggested fix: replace the `throw` with `report.Warn(Name, EmptyAuthorsMessage);
return;` (downgrades to graceful degradation), or restructure so empty-authors
short-circuits only the authors block and DOI/ELOCATION still get written. Add
a unit test: pipeline run on a document with table + section + title + abstract
(no authors paragraph) produces an output `.docx` with the DOI line written and
a `[WARN]` recording missing authors, and the run exits 0 (warning), not 2.

## Triage

- Decision: `VALID`
- Root cause: `RewriteHeaderMvpRule` is `RuleSeverity.Critical` and throws on
  empty Authors. Combined with Optional upstream rules (Orcid, ParseAuthors)
  that quietly leave `ctx.Authors` empty when the authors paragraph isn't
  located, this propagates to a pipeline abort — contradicting the PRD's
  "best-effort, never abort on author uncertainty" promise.
- Fix approach: change the empty-authors path from `throw` to
  `report.Warn(...)` and skip the authors-block work entirely (do not call
  `HeaderParagraphLocator` either, to avoid removing the wrong paragraph when
  authors is genuinely missing). DOI insertion still runs unconditionally.
  Update the existing test that expects the throw to expect the warn-and-skip
  behavior; add a pipeline-level test that asserts a document with no authors
  paragraph still produces an output `.docx` with the DOI line.
- Notes:
