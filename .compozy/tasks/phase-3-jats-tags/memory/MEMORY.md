# Workflow Memory

Keep only durable, cross-task context here. Do not duplicate facts that are obvious from the repository, PRD documents, or git history.

## Current State
ALL 13 TASKS DONE. Phase 3 (JATS tag injection) is fully implemented & verified:
800 tests pass. Code under `DocFormatter.Core/Jats/` (reader, pairer, other-table,
contracts, pipeline, confirmers, XML writer, 4 injectors), CLI `phase3` subcommand
in `DocFormatter.Cli/{CliApp,Phase3Processor}.cs`, diagnostics in
`DocFormatter.Core/Reporting/`, and the golden corpus gate
`DocFormatter.Tests/Phase3/` + `examples/phase-3/expected/`. Per-file structure
and coverage are derivable from the repo — see each `task_NN.md` for specifics.
Feature is ready for `promote-feature`.

## Shared Decisions (durable)
- `Phase3Pipeline` mirrors `FormattingPipeline`: OCE→rethrow always; other
  Exception→`report.Error(name,msg)` + rethrow iff `RuleSeverity.Critical`.
  Reuses `RuleSeverity`/`IReport`/`Report` from `Pipeline` ns — no new report type.
- `FailOnPromptConfirmer` throws `PromptNotAllowedException : OperationCanceledException`.
  Deriving from OCE is what makes a prompt reliably abort even an Optional
  injector (pipeline swallows plain optional exceptions but ALWAYS rethrows OCE).
  CLI catches it at the boundary → `ExitCriticalAbort`.
- XML write is HYBRID, not pure XLinq: `JatsDocument` keeps the original prolog
  (decl+DOCTYPE+leading ws) + epilog verbatim and re-serialises ONLY the root
  (`Indent=false`, `NewLineHandling.Replace`+source newline, fragment, then
  `" />"→"/>"`). Only way to get a byte-identical no-op round-trip (parser
  normalises CRLF/DOCTYPE; `XmlWriter` writes `<x />`). Result: golden diffs are
  PURE insertions of injected tags (validated across all 15 corpus docs).
- INJECTOR PATTERN (07→10): (1) idempotency skip-if-present FIRST; (2) anchor by
  `Name.LocalName==…` (ns-robust); (3) injected name = `anchor.Name.Namespace +
  "local"` (inherits ns); (4) derive `depth` from anchor's preceding XText tab
  count, never hardcode; (5) Critical injector fails loud via throw (pipeline's
  catch reports `ex.Message` — do NOT also `report.Error` inside Apply).
  `InsertAfter` puts an indent XText between anchor and injected, so
  `anchor.NextNode` is XText not the element.
- CONTAINER append/create (08→10 for `<back>`/`<contrib>`): to append INTO a
  container, anchor on its last element child + `InsertAfter(lastChild, node,
  containerDepth+1)`; to CREATE, `BuildElement(name, anchorDepth, …, children)`
  (children at anchorDepth+1) + `InsertAfter(slotAnchor, container, anchorDepth)`.
- CREDIT content-type URL is `http://` (SPS-canonical XML form), NOT the
  `https://` prose "Base:". Term normalization folds case + `&`/`&amp;`→`and` +
  all dashes→space. Auto ONLY when structured + every term exact-maps + every
  author resolves uniquely; else Proposal+confirm.
- FREE-TEXT (ADR-007, task_13): `ConfirmDisposition.FreeText` + opt-in
  `Proposal.AllowsFreeText` (default false; set ONLY on credit-roles
  unrecognized/unresolved gate). `ConsoleConfirmer` offers `[f]`→FreeText only
  when allowed; `AutoAccept` never picks it; `FailOnPrompt` still aborts. On
  FreeText the injector emits the VERBATIM written term of every RESOLVED author
  as `<role>` with NO `@content-type` (incl. CRediT-matching terms) → doc uniform.
  `CreditRole.ContentTypeUrl` is now nullable (null = free text). Diagnostic needs
  no enum-switch change (disposition is ToString→camelCase → "freeText"); the
  injector's "Injected … (FreeText)" Info drives applied-detection. Idempotency
  unchanged. STILL OPEN (out of scope, tracked): split composite label on "and"
  in `CreditStatementParser`; match author key vs `<suffix>` in
  `AuthorInitialsResolver.SurnameMatches` (blocks "Neto"/"Júnior"/"Filho").
- DA classifier: confident = exactly ONE of the five SPS categories matches any
  keyword; 0 or 2+ → propose best-guess (most keyword hits, default
  `data-available-upon-request`). Do NOT use "highest score wins" (intra-category
  substrings self-inflate, e.g. "on request"⊂"upon request").
- Golden gate (task_12): `Phase3DiffUtility` is a NEW line-level LCS diff (NOT an
  extension of docx-specific `Phase2DiffUtility`); injected-only = no source line
  deleted/modified AND every contiguous inserted block carries ≥1 of the 4 tag
  signatures (block granularity lets fn/sec child lines ride along).

## Shared Learnings (durable)
- Corpus quirks (`examples/phase-3/scielo_markup`): `[doc]`+`[doi]` share para 0;
  DOI may have leading space → Trim. Editor line uses NBSP/narrow-NBSP; 5487 &
  5640 have NO editor line. DA/CS ordering varies (5517/5548/5570 are DA-before-
  CS); 5640 glues `CREDIT STATEMENT` onto the DA body (header-anchored extraction
  handles it). Only docx 5136 (XML e51362627) is fully auto; the other 14 prompt.
- `examples/` is gitignored repo-wide ("real customer documents — never commit").
  Golden corpora (phase-2 `after/`, phase-3 `expected/`) live LOCALLY only; tests
  walk up to find them and error in `Resolve*Root` if absent. Established
  convention, not a bug.
- Coverage tooling: repo has none configured. `dotnet-coverage` profiler does NOT
  attach on macOS. Use `coverlet.console` global tool (IL-based) at
  `$HOME/.dotnet/tools/coverlet` (call by full path), against the built test dll;
  default json output, read the ASCII summary table (`--format text` is invalid).

## Open Risks
- Pre-existing `dotnet format --verify-no-changes` failures in
  `DocFormatter.Tests/Phase2/Phase2PipelineIntegrationTests.cs` (committed in
  7c49a32). Unrelated to phase-3; repo format gate is red there. New phase-3
  files are format-clean.
