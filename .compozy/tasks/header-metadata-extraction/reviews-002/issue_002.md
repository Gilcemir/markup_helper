---
provider: manual
pr:
round: 2
round_created_at: 2026-05-06T18:49:17Z
status: resolved
file: DocFormatter.Core/Rules/ExtractOrcidLinksRule.cs
line: 1
severity: high
author: claude-code
provider_ref:
---

# Issue 002: Merge ExtractOrcidLinksRule and ParseAuthorsRule into a single rule

## Review Comment

`ExtractOrcidLinksRule` and `ParseAuthorsRule` operate on the **same**
paragraph (the authors paragraph located by
`HeaderParagraphLocator.FindAuthorsParagraph`) and produce two halves of the
same logical record (`Author { Name, AffiliationLabels, OrcidId, Confidence }`).
Splitting them into two rules forces a fragile cross-rule contract:
`FormattingContext.OrcidStaging` is an internal `Dictionary<int, string>`
keyed by **run-index** in the authors paragraph, populated by the ORCID rule
and consumed by the parser rule.

This contract has three problems demonstrated by issue 001:

1. **Run indices shift under DOM mutation.** The ORCID rule calls
   `authors.ReplaceChild(replacement, hyperlink)` while it walks the
   hyperlinks. After the first replacement, the index it just registered for
   the second ORCID is computed against a paragraph whose run layout has
   already changed. The two rules agree on the indices today only because
   the substitution introduces exactly one new `Run` per removed `Hyperlink`,
   not because there is a semantic invariant.
2. **The ORCID rule must decide what to do with hyperlink content.** When the
   hyperlink wraps an icon, the right move is to drop the icon. When it
   wraps the author name, the right move is to preserve the name. The rule
   has no way to make that decision in isolation — only the author-parsing
   pass knows what is or isn't a name. The current implementation hardcodes
   "drop everything", and that's the bug.
3. **The author parser is blind to hyperlinks.** `ParseAuthorsRule.cs:51`
   uses `paragraph.Elements<Run>()`, which silently skips any `<w:hyperlink>`
   child. So even after fixing the ORCID rule, any future non-ORCID hyperlink
   in the authors paragraph (e.g., link to author profile, e-mail) would
   reintroduce the same class of bug.

A single rule walking paragraph children in document order, treating each
`<w:r>` and `<w:hyperlink>` uniformly as a source of text (and, for ORCID
hyperlinks, also as a source of the current author's `OrcidId`), eliminates
all three problems and removes `OrcidStaging` as a public seam.

## Triage

- Decision: `VALID`
- Root cause: the boundary between "extract ORCID metadata" and "parse author
  names" was drawn at the wrong place. Both operate on the same DOM region
  and produce parts of the same record; the cross-rule handshake
  (`OrcidStaging` keyed by run-index) is incidental coupling that broke as
  soon as a real production article put author content inside the same
  hyperlink as the ORCID URL.
- Fix approach:
  1. Introduce `DocFormatter.Core/Rules/ExtractAuthorsRule.cs`
     (`RuleSeverity.Optional`) replacing both `ExtractOrcidLinksRule` and
     `ParseAuthorsRule`. The new rule:
     - locates the authors paragraph via `HeaderParagraphLocator` once;
     - walks `paragraph.ChildElements` in order. For each `Hyperlink`,
       extracts its inner text (concat of `<w:t>` descendants) and fires the
       same tokenizer logic against it as for plain runs; if the hyperlink's
       relationship target contains the ORCID URL marker and matches
       `OrcidIdRegex`, attaches the matched ID to the current author builder
       and queues the relationship for deletion. For each `Run`, applies the
       existing superscript-label-vs-text logic. Skips `pPr` and any other
       non-content children;
     - emits the existing `[INFO]/[WARN]` messages with the same constants
       (re-export them as `public const` on the new rule, or move them to a
       shared `AuthorParseMessages` static class — pick whichever requires
       fewer test edits);
     - performs the existing free-standing ORCID badge warning and
       relationship-cleanup pass at the end.
  2. Delete `ExtractOrcidLinksRule.cs` and `ParseAuthorsRule.cs`. Delete
     `FormattingContext.OrcidStaging` (no longer needed).
  3. Update DI registration in `DocFormatter.Cli/CliApp.cs`: replace the two
     existing `AddSingleton<IFormattingRule, ...>()` lines for those rules
     with one `AddSingleton<IFormattingRule, ExtractAuthorsRule>()` in the
     same position (between `ParseHeaderLinesRule` and `RewriteHeaderMvpRule`).
  4. Test reorganisation:
     - merge `DocFormatter.Tests/ExtractOrcidLinksRuleTests.cs` and
       `DocFormatter.Tests/ParseAuthorsRuleTests.cs` into
       `DocFormatter.Tests/ExtractAuthorsRuleTests.cs`. Re-run every existing
       scenario against the merged rule (icon-only ORCID hyperlink, ID-text
       ORCID hyperlink, missing authors paragraph, suspicious suffix split,
       free-standing badge image, etc.).
     - add the new fixture for the production case: ORCID hyperlink wrapping
       the author name (Issue 001's reproducer).
     - update the pipeline integration tests in `RewriteHeaderMvpRuleTests`,
       `LocateAbstractAndInsertElocationRuleTests`, and `CliIntegrationTests`
       to register the merged rule instead of the two old ones.
  5. Update memory and ADRs:
     - rewrite the task_07 / task_08 handoff lines in
       `.compozy/tasks/header-metadata-extraction/memory/MEMORY.md` to reflect
       the merged rule and the removal of `OrcidStaging`;
     - add a new ADR (e.g., `adrs/ADR-006-merge-orcid-and-authors.md`)
       capturing the decision to merge: motivation (issue 001), the rejected
       alternative (preserve hyperlink text inside `ExtractOrcidLinksRule`),
       and the new rule boundary.
- Notes:
  - This is a refactor, not a feature. Final acceptance: `make test` passes
    with the same or higher count, and `make run FILE=examples/1_AR_5449_2.docx`
    produces a formatted document whose authors block contains both names
    correctly (closes issue 001).
  - `[InternalsVisibleTo("DocFormatter.Tests")]` is already wired (see
    task_07 handoff in MEMORY.md) — the merged rule can keep test-only seams
    internal.
  - Do **not** loosen the existing ORCID URL marker check in
    `FormattingOptions.OrcidUrlMarker`; the merged rule still keys ORCID
    capture off URL containment, only the *text-preservation* step is new.
