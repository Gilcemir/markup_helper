# Task Memory: task_07.md

Keep only task-local execution context here. Do not duplicate facts that are obvious from the repository, task file, PRD documents, or git history.

## Objective Snapshot

Phase 3 release: `EmitCorrespTagRule` + `EmitAuthorXrefsRule`. Wrap `[corresp id="c1"]…[/corresp]`, emit per-author `xref ref-type="aff"` / `xref ref-type="corresp" rid="c1"`, wrap ORCIDs in `[authorid authidtp="orcid"]`, and patch `[author role="nd"]` opening tag attrs (`rid`, `corresp`, `deceased`, `eqcontr`). Ship without re-emitting `[author]`/`[fname]`/`[surname]`/`[normaff]` (anti-duplication, ADR-001). Pass corpus diff at scope `{authorid, corresp, doc, doctitle, doi, kwdgrp, label, normaff, toctitle, xmlabstr, xref}`.

## Important Decisions

- **Phase 2 runs in isolation** — `AddPhase2Rules()` doesn't chain Phase 1. EmitAuthorXrefsRule parses the body itself, populating `ctx.Authors`/`Affiliations`/`CorrespondingAuthorIndex` so EmitCorrespTagRule can use them. This avoids touching Phase 1 rules to re-handle pre-tagged input.
- **Author rewrite is in-place text patching, not new tag emission.** Author shells `[author role="nd"]…[/author]` are already in BEFORE (from a prior Markup pass). The rule joins all `<w:t>` text in the paragraph, regex-rewrites attributes / inserts xrefs / wraps ORCIDs, then writes the result into the first Text and clears the rest (same trick used by EmitElocationTagRule).
- **Plain-text author paragraphs** (no `[author]` shell, e.g. 5449) get the same xref/authorid wrapping. Anti-duplication still holds because the rule never introduces `[author]`/`[fname]`/`[surname]`.
- **Author opening tag attrs**: `role="nd" rid="<aff_list>" corresp="y|n" deceased="n" eqcontr="nd"`. `eqcontr="nd"` was added beyond the PRD's listed three because the AFTER corpus has it on every author. Empirical corpus is the source of truth.
- **Authorid uses `authidtp` not `ctrbidtp`.** Same corpus-as-truth rule.
- **Corresp marker conversion preserves the comma**. `[/surname]1,*ORCID` → `[/surname][xref ref-type="aff" rid="aff1"]1[/xref],[xref ref-type="corresp" rid="c1"]*[/xref]ORCID`. The comma in the BEFORE plain text matches the AFTER between the two xrefs.
- **Unicode superscript labels** (¹²³⁴⁵⁶⁷⁸⁹⁰) wrapped via dedicated regex. 5523 carries the aff label as a real Unicode superscript; the pattern maps to its ASCII form for the `aff<n>` rid.
- **Asterisk after unicode superscript stays plain** in the rule output (not wrapped in `[xref ref-type="corresp"]`). Matches AFTER 5523 author 7's editor-chosen shape.

## Learnings

- ORCID lookbehind `(?<![\dA-Za-z])` was wrong for plain-text author paragraphs where the digit is preceded by a name letter (e.g. "Silva1*"). Fix: `(?<!\d)` — only reject digits in the lookbehind, allow letters. Letters are ALWAYS the previous character in `<name><digit>*` shapes; rejecting them broke every plain-text corresp author.
- Corpus heterogeneity is real: AFTER 5313 authors 4–6 had plain ORCIDs while authors 7+ got `[authorid]` wrappers. Per ADR-003, amended AFTER 5313 to be consistent (added `[authorid]` for the three authors).
- `python-docx` modifies run text without writing to the heavily-fragmented `word/document.xml` directly. Use it (not zipfile + str.replace) for surgical AFTER amendments.
- Phase 2 corpus integration tests are the final gate: every regex/edge case shows up there. Pre-validate by writing a "dump-diffs" helper that prints `produced` vs `expected` post-strip text per failing pair — much faster than guessing.

## Files / Surfaces

- `DocFormatter.Core/Models/Phase2/Affiliation.cs` (new)
- `DocFormatter.Core/Models/Phase2/CorrespAuthor.cs` (new)
- `DocFormatter.Core/Pipeline/FormattingContext.cs` (added `Affiliations?` + `CorrespAuthor?`)
- `DocFormatter.Core/Rules/Phase2/EmitCorrespTagRule.cs` (new)
- `DocFormatter.Core/Rules/Phase2/EmitAuthorXrefsRule.cs` (new)
- `DocFormatter.Core/Pipeline/RuleRegistration.cs` (added two transient registrations)
- `DocFormatter.Core/Reporting/Phase2Scope.cs` (added `authorid, corresp, xref`)
- `DocFormatter.Core/Reporting/DiagnosticDocument.cs` (extended `DiagnosticPhase2` with `Corresp`, `Xref[]`; new `DiagnosticAuthorXref` record)
- `DocFormatter.Core/Reporting/DiagnosticWriter.cs` (BuildCorrespDiagnostic, BuildAuthorXrefDiagnostic)
- `DocFormatter.Tests/Phase2/EmitCorrespTagRuleTests.cs` (new, 6 tests)
- `DocFormatter.Tests/Phase2/EmitAuthorXrefsRuleTests.cs` (new, 10 tests)
- `DocFormatter.Tests/Phase2/Phase2PipelineIntegrationTests.cs` (added 3-author fixture test)
- `DocFormatter.Tests/Phase2ScopeTests.cs`, `RuleRegistrationTests.cs`, `CliPhase2Tests.cs` (sentinels + smoke updated)
- `examples/phase-2/before/5293.docx`, `examples/phase-2/before/5523.docx`, `examples/phase-2/after/5313.docx` (corpus amendments documented in shared MEMORY.md)

## Errors / Corrections

- Initial `TrimTrailingWhitespace` on the corresp paragraph stripped trailing space unconditionally. Wrong: 4/10 AFTERs preserve the BEFORE trailing space. Reverted; amended BEFORE 5293 instead.
- First plain-text corresp regex used `(?<![\dA-Za-z])` — rejected the letter that always precedes the corresp digit. Fixed to `(?<!\d)`.
- Original `OrcidPattern` required 4-4-4-4 strictly. 5419 author 7 ORCID is 4-4-4-3 (typo). Relaxed to `\d{3}[\dX]?`.
- Corpus integration test failures didn't show full divergence context (80-char windows). Added a temporary `DumpDiffsTests` that writes `produced` and `expected` post-strip text to `/tmp/<id>.{produced,expected}.txt`; ran `diff` to localize each gap. Removed before commit.

## Ready for Next Run

Implementation complete. Verification:
- 472/472 tests green (was 455 before task 07).
- `make phase2-verify` exits 0; 10/10 [PASS].
- Phase 1 byte SHA-256 unchanged on `examples/1_AR_5449_2.docx` (`9c7be60a…d76250`).

Task 08 (`HistDateParser` TDD) is independent — no blocker on task 07 deliverables.
