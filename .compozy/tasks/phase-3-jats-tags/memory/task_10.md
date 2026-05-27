# Task Memory: task_10.md

Keep only task-local execution context here. Do not duplicate facts that are obvious from the repository, task file, PRD documents, or git history.

## Objective Snapshot
CreditRolesInjector (task_10): inject `<role content-type=…>` per CRediT term into
each `<contrib>` after its `<xref>`. Auto ONLY when structured (role-keyed OR
author-keyed) + every term exact-maps + every author resolves uniquely; else
propose+confirm (ADR-005). Prose → always prompt. Idempotent; all-or-nothing.

## Important Decisions
- **CRediT URL form = `http://credit.niso.org/contributor-roles/<slug>/`** (NOT
  https). credit_roles.md prose says "Base: https://…", but that is the human
  CRediT website; the SPS-canonical `content-type` VALUE in the XML is `http://`
  — confirmed by raw `_raw/SPS 1.10_pt.md` line 2840+ AND credit_roles.md's own
  XML example (line 58). Golden corpus (task_12) must expect `http://`.
- Display text = canonical table term incl. en-dash: `Writing – original draft`,
  `Writing – review & editing` (en-dash U+2013, literal `&`).
- Term normalization key: lowercase + `&amp;`→`&` + `&`→` and ` + all dashes
  (`- – — ‑`)→space + collapse ws. So `Writing - Original Draft`,
  `Writing – original draft` all → `writing original draft`; `… & editing`,
  `… and editing` → `writing review and editing`.
- Shape detection order: RoleKeyed (every `;`-chunk is `label: vals` AND first
  label maps to a term) → AuthorKeyed (`.`-split entries `keys: terms`, keys
  `;`-split, terms map) → Prose. Unknown role-label in a non-first chunk stays
  structured but becomes an unknown term → gate→prompt.
- Gate: parser merges to unique per-author CreditEntry(ordered, deduped terms).
  Resolve author + map terms per entry. `allClean` (no unknown term, no
  unresolved author) → AUTO. Else build Proposal + ctx.Confirm; Skipped → write
  nothing; Confirmed/Overridden → apply only the CLEAN entries (subset). We NEVER
  emit a role without @content-type, so SPS all-or-nothing holds by construction.
- Severity Optional (CRediT is author metadata that may be absent), like DA/edited-by.
- Initials resolver: surname-first match (key `Surname Initials`/`Surname, I. N.`
  → first token vs `<surname>`, unique→Resolved; tie→narrow by initials else
  Ambiguous). Pure-uppercase key (`ATAJ`) → candidate initials = givenInitials +
  surnameInitial + suffixInitial (drop lowercase particles da/de/do/…); unique→
  Resolved. Accent-insensitive (FormD strip).

## Learnings
- Corpus CREDIT shapes verified: 5523 role-keyed (`Term: Surname Inits; …`, ends
  `.`), 5136 author-keyed (`Inits: terms.` + `A;B;…;Z: terms.`), 5313 messy
  role-keyed → falls to PROSE here (compound labels + comma-initials defeat clean
  detect; OK, prompts), 5449/5293/5570 prose. Only 5523 is an auto integration
  target; 5136 author resolution is fuzzy and NOT integration-tested.
- `CreditStatementRaw` has the `CREDIT STATEMENT` header stripped and body
  segments space-joined; `&amp;` is already decoded to `&` by OpenXml Text.Text.
- Reference authors use `<name>` in `<person-group>`, NOT `<contrib>`, so
  `Descendants(LocalName=="contrib")` selects only article authors. 5523 contribs
  all have unique surnames → resolve by surname alone.

## Files / Surfaces
- NEW `DocFormatter.Core/Jats/CreditTermTable.cs` (+ `CreditRole` record),
  `CreditStatementParser.cs` (+ `CreditShape` enum, `CreditEntry`,
  `CreditStatement` records), `AuthorInitialsResolver.cs` (+ `ResolveStatus`,
  `AuthorResolution`), `CreditRolesInjector.cs`.
- NEW tests `DocFormatter.Tests/Jats/CreditRolesInjectorTests.cs` (+ maybe
  CreditTermTableTests / parser / resolver tests).

## Errors / Corrections
- None. xUnit analyzers (xUnit2029/2031) initially rejected `Assert.Empty(seq.Where(..))`
  / `Assert.Single(seq.Where(..))` — switched to `Assert.DoesNotContain(seq, pred)`
  / `Assert.Single(seq, pred)`. Keep this in mind for future Jats tests.

## Ready for Next Run
- DONE & verified: build 0/0, 717 tests green, coverage 95.07% line on the four
  new types (≥80% target). Auto-commit disabled — diff left for manual review.
- task_11 must register `CreditRolesInjector` in `AddPhase3Injectors()` LAST in the
  pipeline order (OtherId→EditedBy→DataAvailability→CreditRoles per techspec).
- task_12 golden corpus: expect `<role content-type="http://credit.niso.org/…/">`
  (http, not https) and en-dash display for the two Writing terms; 5523 is the
  clean role-keyed auto case, 5449/5293/5570 + 5313 are prompt cases.
