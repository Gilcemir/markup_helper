# Task Memory: task_06.md

Keep only task-local execution context here. Do not duplicate facts that are obvious from the repository, task file, PRD documents, or git history.

## Objective Snapshot

Phase 2 first release: ship `EmitElocationTagRule`, `EmitAbstractTagRule`, `EmitKwdgrpTagRule` plus the corpus integration test. After this task `make phase2-verify` exits 0 across all 10 corpus pairs.

## Important Decisions

- **EmitElocationTagRule rewrites the existing `[doc]` opening tag in place** rather than emitting a separate `[elocation]…[/elocation]` literal. The corpus encodes the elocation as the `elocatid` attribute on `[doc]` (no separate tag exists in any AFTER pair). The rule also derives `issueno` from elocation-ID position 7 (journal format `e<article(4)><volid(2)><issueno(1)><order>`), and removes the standalone `e\d+` paragraph after rewriting.
- **Doc opening tag rewrite handles split Text fragments**: Word's spell-check anchors split the `[doc … elocatid="xxx" …]` literal across many `<w:t>` nodes. The rule concatenates every Text in the doc paragraph, regex-replaces the attributes on the joined string, writes the corrected text back into the first Text, and clears the rest. Lossy for Word's spell-check formatting; harmless for SciELO production (the `[doc]` paragraph is metadata).
- **Corpus tag name for abstract is `xmlabstr`, not `abstract`**: PRD prose says "[abstract]" but every AFTER corpus pair emits `[xmlabstr language="en"]…[/xmlabstr]`. `EmitAbstractTagRule` emits the corpus shape.
- **Kwdgrp does NOT emit `[kwd]` per item** (anti-duplication invariant from `docs/scielo_context/REENTRANCE.md`). The rule wraps only the outer `[kwdgrp language="en"]…[/kwdgrp]`. Markup auto-marks the individual keywords downstream.

## Learnings

- Heading-paragraph heuristic for the abstract: the paragraph's normalized text must START with one of `FormattingOptions.AbstractMarkers` and have NOTHING else after the marker. This rejects `Abstract submission deadline` style false positives and matches the corpus shape (Phase 1's `RewriteAbstractRule` splits the abstract into a heading + body; `EmitAbstractTagRule` finds the heading and the next non-empty paragraph as the body).
- The kwdgrp marker regex requires a colon (`Keywords:` / `Palavras-chave:`). Without the colon a paragraph is much more likely to be a section heading than the kwdgrp paragraph; matching it would wrap the wrong block.
- `Phase2DiffUtility` strip semantics needed two refinements (now durable; promoted to shared memory):
  1. Symmetric strip (apply to BOTH produced and expected) so future-task-owned tags can stay out of scope without dropping content.
  2. Out-of-scope tag pairs preserve their content (just strip brackets) and trim the content's edges so adjacent peeled wrappers don't leak whitespace artifacts.
  3. `NormalizeForCompare` collapses spaces around newlines, multi-newlines, and runs of horizontal whitespace.

## Files / Surfaces

- `DocFormatter.Core/Models/Phase2/AbstractMarker.cs`, `KeywordsGroup.cs` (new).
- `DocFormatter.Core/Pipeline/FormattingContext.cs` (added `Abstract?` and `Keywords?`).
- `DocFormatter.Core/Pipeline/RuleRegistration.cs` (registers the three rules under `AddPhase2Rules`).
- `DocFormatter.Core/Reporting/DiagnosticDocument.cs` (added optional `DiagnosticPhase2 Phase2`).
- `DocFormatter.Core/Reporting/DiagnosticWriter.cs` (populates the Phase 2 block from rule entries + ctx).
- `DocFormatter.Core/Reporting/Phase2DiffUtility.cs` (symmetric strip + keep content + post-strip whitespace normalization).
- `DocFormatter.Core/Reporting/Phase2Scope.cs` (`{doc, doctitle, doi, kwdgrp, label, normaff, toctitle, xmlabstr}` — see shared memory note about why `author/xref/fname/surname` are dropped).
- `DocFormatter.Core/Rules/Phase2/EmitElocationTagRule.cs`, `EmitAbstractTagRule.cs`, `EmitKwdgrpTagRule.cs` (new).
- `DocFormatter.Tests/Phase2/EmitElocationTagRuleTests.cs`, `EmitAbstractTagRuleTests.cs`, `EmitKwdgrpTagRuleTests.cs`, `Phase2PipelineIntegrationTests.cs`, `Phase2CorpusTests.cs` (new).
- `DocFormatter.Tests/Fixtures/Phase2/KeywordsParagraphFactory.cs` (new).
- `DocFormatter.Tests/Phase2DiffUtilityTests.cs`, `Phase2ScopeTests.cs`, `RuleRegistrationTests.cs`, `CliPhase2Tests.cs` (updated for new strip + scope semantics).
- `examples/phase-2/before/5313.docx`, `5424.docx` (one-time corpus refinement — see Errors / Corrections).

## Errors / Corrections

- BEFORE 5313 P8–P14: superscript affiliation digit ran directly after the surname text (`Quintal1`) where the matching AFTER decomposes with a literal space (`[surname]Quintal[/surname] [xref…]1[/xref]`). Patched the BEFORE: appended a single space to each affected surname Run text. Excluded P7 (Flavia Silva — corresp author whose AFTER has adjacent `[xref aff][xref corresp]` with no intervening literal space, so BEFORE keeps `Silva1*`).
- BEFORE 5424 P15: corresp paragraph trailed redundant `ORCID: 0000-0002-4081-3140` after the email; AFTER deduplicated. Trimmed the suffix from the BEFORE Run text.
- Both edits are corpus refinements per ADR-003's "amend the corpus pair (with justification)" path. Documented inline in the patch script and below in shared memory.

## Ready for Next Run

Task complete. `make phase2-verify` exits 0 (10/10 pass). 455/455 unit and integration tests pass. Phase 1 byte equivalence preserved (SHA-256 `9c7be60a…d76250` for `examples/formatted/1_AR_5449_2.docx`, matches Task 04/05 anchor).
