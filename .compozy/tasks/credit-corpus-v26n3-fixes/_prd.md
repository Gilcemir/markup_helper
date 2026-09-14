# PRD — credit-corpus-v26n3-fixes

## Overview

Running Phase 3 on the 15 articles of CBAB v26n3 left 13 with a warning or a
false negative on CRediT roles, although every CREDIT statement in the edition
is structured (author-keyed). The tool rejects the `;` separator the authors
use, silently drops a statement tagged `[p]`, resolves initials too loosely in
one direction and too strictly in another, and writes a diagnostic without the
text the operator needs. Upstream, Phase 1 lets a plain-text ORCID stay inside
the author name, and SciELO Markup then promotes it to the surname; a name
whose last token repeats an earlier one is mis-split by Markup as well.

This feature fixes the deterministic defects in both phases, makes broken
contributor names visible, and gives the operator an edition-level view of what
is pending. It is for the markup operator who runs DocFormatter on each CBAB
edition. Its value is fewer manual CRediT interventions per edition and no
silent loss of author-supplied data.

## Goals

- On the v26n3 corpus, 10 of 15 articles auto-apply CRediT for every author
  with no warning; the remaining 5 are pending only on docx defects the tool
  cannot fix.
- Zero "free prose" verdicts on v26n3.
- Every article with a broken `<surname>` (empty, digits, ORCID) is flagged in
  its report and counted in the batch summary.
- Every CREDIT statement present in a docx is either parsed or reported as
  unparsed; a present header with an empty body is a warning, never an info.
- A Phase 1 run on a byline with a plain-text ORCID extracts it as the author's
  ORCID instead of leaving it in the name.
- Ship as `v0.3.1`.

## User Stories

**Markup operator (primary)**

- As the operator, I want author-keyed statements that use `;` between roles or
  between authors to be parsed, so that I stop confirming roles by hand for the
  dominant CBAB writing style.
- As the operator, I want a CREDIT paragraph tagged `[p]` to be read like any
  other, so that a complete statement is never reported as absent.
- As the operator, I want "SA" to resolve to Saleem Abid when Sajjad Ahmad Khan
  is also an author, "LBB" to resolve to Barboza-Barquero, and "TVB" to resolve
  to Truong Van Bui, so that correct initials do not fall to a prompt.
- As the operator, I want a warning naming each contributor whose surname is
  empty or holds an ORCID, so that I fix the byline in Markup before the XML
  ships.
- As the operator, I want the diagnostic JSON to carry the statement text and
  each author's resolution status, so that I diagnose a pending article without
  reopening the docx.
- As the operator, I want the batch summary to list, per article, which authors
  auto-applied, which are pending and why, and how many names are broken, so
  that I can close an edition from one file.

**Markup operator, Phase 1 (secondary)**

- As the operator, I want a plain-text ORCID in the byline moved out of the
  author's name, so that Markup's auto-mark does not turn it into the surname.
- As the operator, I want a warning when an author's last name token repeats an
  earlier token, so that I know Markup will mis-split that name before it does.

## Core Features

**P0 — CREDIT statement grammar (Phase 3)**

- Author-keyed statements accept `;` and `,` between roles, and `;`, `,` and
  `.` between author entries.
- When several authors share a role list and are themselves separated by `;`
  (5501 style), the tool recognizes them as one entry.
- A statement that is not cleanly `authors: roles` throughout still falls to
  the prose path and prompts. Existing prose fixtures remain prose.
- The "free prose" label is applied only to statements that are actually prose.

**P0 — CREDIT section reading (Phase 3)**

- A `[p]…[/p]` paragraph after the CREDIT STATEMENT header is read as body
  text.
- Any other bracket tag or a REFERENCES heading still ends the section.
- Header present with empty body → warning.

**P0 — Initials resolution (Phase 3)**

- Full-name candidates (given + surname, with or without suffix, in both
  orders) take precedence; the given-names-only candidate is used only when no
  full candidate matches.
- Hyphenated surnames yield both one-initial and two-initial candidates.
- A capitalized particle ("Van", "Da") yields candidates with and without it.
- Uniqueness stays mandatory; ambiguous or absent keys still prompt.

**P0 — Contributor name verification (Phase 3)**

- A verification-only rule reports each `<contrib>` whose surname is empty,
  contains digits, or matches an ORCID.
- It never modifies the XML and never blocks CRediT injection (ADR-002).

**P0 — Diagnostic and batch summary (Phase 3)**

- The CRediT section of the diagnostic JSON always includes the raw statement,
  its recognized shape, and each parsed entry with the author key, roles, and
  resolution status.
- The batch summary aggregates per article: auto-applied authors, pending
  authors with reason (not found, ambiguous, unknown term), and broken-name
  count.

**P0 — Byline extraction (Phase 1)**

- An ORCID appearing as plain text inside an author name is extracted as that
  author's ORCID and removed from the name, with an informational report line;
  a conflict with a hyperlink ORCID is a warning.
- A name whose last token repeats an earlier token produces a warning
  explaining that Markup will tag the first occurrence.

## User Experience

The operator's flow does not change: run `phase1`, hand the docx to Markup, run
`phase3` on the package, read `_batch_summary.txt`, then open `.report.txt` and
`.diagnostic.json` only for pending articles. What changes is what those files
say:

1. The batch summary opens with the edition's pendency list, so an edition
   with 10 clean articles and 5 pending reads as such at a glance.
2. Each pending article's report separates "CRediT pending for author X (not
   found)" from "contributor #N has a broken surname", each with its own
   remedy.
3. The diagnostic JSON shows the statement as read and each author's status,
   so the docx is opened only to fix content.
4. In Phase 1, the report line for a plain-text ORCID tells the operator the
   byline was normalized; the repeated-token warning names the author.

No new commands or flags. Non-interactive mode (`--non-interactive=accept`)
behaves as today.

## High-Level Technical Constraints

- The docx remains read-only for Phase 3; the evidence folder for v26n3 is
  never modified (validation runs on a copy).
- The Phase 2 diff gate over `examples/phase-2/{before,after}/` must stay
  green.
- Bracket tags `[author]`, `[fname]`, `[surname]`, `[normaff]` stay owned by
  Markup; Phase 1 does not pre-mark them.
- Output stays SPS 1.10-compliant; the all-or-nothing rule for `@content-type`
  per document is unchanged.
- Release is tag-based (`make release VERSION=v0.3.1`).

## Non-Goals (Out of Scope)

- No heuristic to repair a broken `<surname>` or to guess the author behind
  inconsistent initials (TTR, JGS, MRC stay pending).
- No fix for 5501 author 5, whose layout indicates manual editing after
  Phase 2; it is covered by the warning only.
- No synonym or fuzzy layer for prose statements.
- No change to Markup macros or to the SciELO Markup workflow.
- No new CLI commands or flags.

## Phased Rollout Plan

Single release (ADR-001). Internal ordering for validation only:

### Step 1 — Phase 3 corrections

Grammar, reader, resolver, `contrib-names`, diagnostic, batch summary.
Success: v26n3 run on a copy meets the acceptance table (10 fully
auto-applied; 5613 pending only NHN; 5316 pending only MAF, JAN, AGS, JSP; 5529
only TTR; 5617 only JGS; 5719 only MRC; zero "free prose").

### Step 2 — Phase 1 corrections

Plain-text ORCID extraction and repeated-token warning. Success: unit tests
with synthetic bylines; `make phase2-verify` unchanged.

### Step 3 — Release

Docs updated (ADRs promoted, invariants if any), merge to `main`, tag
`v0.3.1`.

## Success Metrics

- Auto-applied CRediT on v26n3: 10/15 articles fully, up from 2/15.
- "Free prose" verdicts on v26n3: 0, down from 3.
- Silently lost statements: 0, down from 1.
- Broken-surname contributors reported: 9/9 (5316 ×7, 5613 ×1, 5501 ×1), up
  from 0.
- Docx reopenings needed to diagnose a pending article: 0 (statement text is
  in the diagnostic).

## Risks and Mitigations

- **Corpus bias**: the grammar is widened on one edition's evidence; a future
  edition may use yet another separator. Mitigation: the diagnostic now carries
  the raw text, so the next deviation is diagnosable from the JSON.
- **Operator ignores warnings**: a broken surname reaches the XML. Mitigation:
  the batch summary lists broken names as an edition pendency.
- **Phase 1 change alters Markup's behavior on other bylines**: Mitigation: the
  Phase 2 diff gate over the curated corpus is a release blocker.
- **Patch label under-communicates new behavior**: Mitigation: commit messages
  name each defect and the new rule so the generated release notes are
  explicit.

## Architecture Decision Records

- [ADR-001: Ship the CBAB v26n3 CRediT corrections as one patch release across Phase 1 and Phase 3](adrs/adr-001.md)
  — full scope, single release, `v0.3.1`.
- [ADR-002: A broken contributor name warns but never blocks CRediT injection](adrs/adr-002.md)
  — `contrib-names` is verification-only; resolution unchanged.

## Open Questions

- Root cause of 5316's byline (plain-text ORCID) is a strong inference; the
  Phase 1 input docx and reports are not available to confirm. The fix is
  covered by synthetic tests.
- Root cause of 5501 author 5 is undetermined (manual edit suspected); covered
  by the warning only.

## Evidence

- `/Users/gilcemir.angelo/Documents/personal_workspace/Trabalho/CBAB/v26/n3/PROMPT_credit_roles_v26n3.md`
  (read-only; full statement texts, docx ↔ XML pairing, acceptance table).
