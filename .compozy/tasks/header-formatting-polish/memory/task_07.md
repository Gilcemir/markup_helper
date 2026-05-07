# Task Memory: task_07.md

Keep only task-local execution context here. Do not duplicate facts that are obvious from the repository, task file, PRD documents, or git history.

## Objective Snapshot
- Implemented `ExtractCorrespondingAuthorRule` (Optional) in `DocFormatter.Core/Rules/ExtractCorrespondingAuthorRule.cs`. Rule does Pass A (affiliation trailer cleanup + email/ORCID extraction) and Pass B (author `*` identification, superscript-aware), then promotes the affiliation ORCID onto the corresponding author when the author has no prior ORCID.

## Important Decisions
- Plain-text → run-offset mapping helper lives inline in the rule (not pushed into `HeaderParagraphLocator`). The TechSpec leaves the location flexible and the run-splitting logic is private to this rule's needs.
- Pass A nulls `ctx.CorrespondingAffiliationParagraph` after `Remove()` to honor the "publisher must not orphan its own paragraph reference" invariant in `FormattingContext`.
- Pass B counts authors by walking plain-text runs and incrementing on `_options.AuthorSeparators` matches; superscript runs are skipped for separator counting (only checked for `*`). Aligns with the order `ExtractAuthorsRule` produces in `ctx.Authors`.

## Learnings
- `dotnet format` enforces 4-space indentation inside switch-expression bodies; the initial draft used aligned indentation that triggered WHITESPACE errors. Run `make format`/`dotnet format DocFormatter.sln` after writing new switch blocks before claiming the task green.

## Files / Surfaces
- `DocFormatter.Core/Rules/ExtractCorrespondingAuthorRule.cs` (new).
- `DocFormatter.Tests/ExtractCorrespondingAuthorRuleTests.cs` (new) — 16 tests, full pass.

## Errors / Corrections
- None blocking.

## Ready for Next Run
- task_08 (`RewriteAbstractRule`) consumes `ctx.CorrespondingEmail`. The rule now populates it for the canonical `* E-mail:` path; task_08 owns the typed-line fallback (per ADR-003 split).
- task_10 must register this rule **before** `RewriteHeaderMvpRule` in `CliApp.BuildServiceProvider`.
