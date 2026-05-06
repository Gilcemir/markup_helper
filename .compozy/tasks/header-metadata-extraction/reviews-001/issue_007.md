---
provider: manual
pr:
round: 1
round_created_at: 2026-05-06T17:49:40Z
status: resolved
file: validation/case-001.md
line: 0
severity: medium
author: claude-code
provider_ref:
---

# Issue 007: Phase-1 done criterion lacks tracked Windows validation evidence

## Review Comment

The PRD's MVP success criterion is explicit:

> Initial milestone: one production article processed correctly end-to-end via
> the CLI.
> ...
> **Success criterion to proceed to Phase 2**: one real production article
> runs end-to-end on Windows, producing a correctly rewritten header with all
> four fields in the right place; editor confirms it would have saved real time.

`task_13` (Windows publish target and end-to-end production article validation)
is marked `completed` in `_tasks.md`. There is a `validation/` directory in
the working tree, but it is **untracked** (not part of any commit). Combined
with Issue 001 (DoiRegex won't match production input), there is no auditable
evidence that the validation actually succeeded on a real production article
on Windows 10 — which is precisely the gate the PRD set for declaring Phase 1
done.

Possible interpretations:
1. Validation ran, succeeded against a contrived test input that happens to use
   an unwrapped DOI, and evidence was left untracked.
2. Validation ran on a real production article, the DOI line was missing, and
   that result was not flagged as a Phase-1 blocker.
3. Validation has not actually been run on Windows yet.

Suggested fix:

- Move `validation/` outputs into the repo (commit) or generate fresh evidence:
  - the input fixture used (or a checksum + reference to one of the
    `examples/*.docx` files)
  - the produced `formatted/<name>.docx`, `<name>.report.txt`, and any
    `<name>.diagnostic.json`
  - a screenshot or screen-record of the rewritten header opened in Word on
    Windows 10
- Add a short `validation/README.md` recording: which fixture, what host OS,
  which `.exe` build (commit hash), pass/fail, and any deviations from the
  PRD's expected header layout.
- If the run revealed the DOI gap from Issue 001, document it here so the
  Phase-1 sign-off is honest.

## Triage

- Decision: `VALID`
- Root cause: `validation/case-001.md` exists in the working tree but was
  untracked, so the Phase-1 acceptance evidence — even the empty template
  produced by task_13 — was not auditable. On inspection the template is
  also unfilled (most fields read `_to be filled by editor`), confirming
  the actual editor-driven Windows 10 acceptance run has not happened yet.
- Fix approach: track `validation/case-001.md` so task_13's deliverable
  becomes part of the repo. Annotate the issue notes that the editor-driven
  Windows 10 run is still pending (the human side of this issue cannot be
  resolved by code). The fix is intentionally limited to making the existing
  template auditable — actually executing the validation requires Windows +
  a production article from the editorial team and is outside this batch.
- Notes: After Issue 001 is fixed (DOI URL stripping), the editor's
  acceptance run is the next gate. Once they fill in case-001.md and sign
  off, Phase 1 can be declared done.
