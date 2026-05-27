# Task Memory: task_12.md

Keep only task-local execution context here. Do not duplicate facts that are obvious from the repository, task file, PRD documents, or git history.

## Objective Snapshot
DONE. `Phase3CorpusTests` (accept-mode golden diff + fail-mode ambiguity assertion)
+ `Phase3DiffUtility` (Core/Reporting) + 15 curated `examples/phase-3/expected/*.xml`
goldens. All 781 tests pass; new code 91.66% line cov.

## Important Decisions
- NEW `Phase3DiffUtility` (line-level LCS diff) instead of extending
  `Phase2DiffUtility` — Phase2's is docx body-text/bracket-tag specific and not
  reusable for XML line comparison. Injected-only = (no source line deleted/
  modified) AND (every contiguous inserted block carries ≥1 of the 4 tag
  signatures). Block-granularity is essential: fn/sec child lines (`<label>`,
  `<p>`, `<title>`, closing tags) carry no signature themselves but ride along
  in the block.
- Test runs the BATCH (folder) accept/fail once each (2 CLI runs total), not 15×
  per-file — equivalent to "run each package", far faster. Per-package byte-equal
  + injected-only assertions loop over the batch outputs.
- Fail-mode ambiguous set detected by ABSENCE of produced `<base>.xml` (a prompt
  aborts before `jdoc.Save`; corpus has no pairing failures so absence==prompted).
  The "aborted on prompt" log goes through Serilog, NOT the passed stdout/stderr
  writers, so stderr parsing is not viable.

## Learnings
- accept-mode reality: 15 processed, only `e51362627` (docx 5136, author-keyed,
  all terms exact + DA auto-classified) is FULLY AUTO. The other 14 prompt → the
  documented ambiguous set hardcoded in `AmbiguousBasenames`. fail-mode batch:
  processed=1, failed=14, exit=ExitCriticalAbort (batch continues past aborts).
- Prose CREDIT under accept does NOT inject roles (best-guess = skip); golden has
  the other 3 tags but no `<role>`. Verified all 15 goldens are PURE insertions
  vs source (0 deleted/modified lines) — confirms whitespace-preserving writer.
- Corpus XML is CRLF; writer preserves it. Largest source ~3293 lines (LCS DP
  fine transiently).
- docx basename → XML elocation map: docx `5136`→`e51362627`, `5523`→`e55232626`,
  `5313`→`e53132629`, `5449`→`e54492621`, `5293`→`e52932623`, `5570`→`e557026213`.

## Files / Surfaces
- NEW `DocFormatter.Core/Reporting/Phase3DiffUtility.cs` (+`Phase3DiffResult`).
- NEW `DocFormatter.Tests/Phase3/Phase3CorpusTests.cs` (4 tests),
  `Phase3DiffUtilityTests.cs` (7 tests).
- NEW `examples/phase-3/expected/*.xml` (15 goldens). NOTE: `examples/` is
  gitignored repo-wide ("real customer documents — never commit"); goldens live
  locally + tests walk up to find them — IDENTICAL convention to
  `Phase2CorpusTests` reading gitignored `examples/phase-2/after/`. CI without the
  corpus errors in `ResolveCorpusRoot`, as Phase2 already does.

## Errors / Corrections
- (resolved) Earlier BLOCKED note said task_11 was pending — it is now completed;
  the `phase3` subcommand + `--non-interactive` flag exist and work.
