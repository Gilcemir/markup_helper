# Task Memory: task_07.md

Keep only task-local execution context here. Do not duplicate facts that are obvious from the repository, task file, PRD documents, or git history.

## Objective Snapshot

- Done 2026-09-14: `Phase3Outcome` gains `Credit`/`BrokenNames` (defaulted, so existing constructions compile); `Phase3Processor` fills them on the Processed path; `CliApp.Phase3PendencyLines` + `WritePhase3BatchSummary` (now internal) append the indented block. Suite 879 → 890.

## Important Decisions

- Pendency lines only for `Processed` outcomes; skipped/failed files keep their marker line (the reason is already there).
- `absent` disposition gets its own wording (`credit: no CREDIT STATEMENT on the docx`) — not in the TechSpec table, added because it is a pendency for an edition per SPS (CRediT required for 2+ authors).
- Pending reason precedence: resolution when not resolved, then unknown terms, else the document disposition (covers "operator skipped a fully resolvable document" → `A (skipped)`).
- `ContribNamesInjector.RuleName` const added so the CLI counts WARNs by the same string the rule reports under.

## Learnings

- A contributor whose `<surname>` is an ORCID only resolves when the full name sits in `<given-names>` (tier 2); a fixture with surname=ORCID and given-names="Ana Beatriz" makes "ABC" NotFound. Use the 5316 shape (full name in given-names) when a test needs both the WARN and a resolved author.
- `Phase3DocxFixtureBuilder.WriteMarkupDocxWithCreditStatement(path, text, elocationId, doi)` overload stages several distinct articles in one package; pairing rejects duplicate elocation ids across docx.

## Files / Surfaces

- Modified: `DocFormatter.Cli/Phase3Processor.cs`, `DocFormatter.Cli/CliApp.cs`, `DocFormatter.Core/Jats/ContribNamesInjector.cs` (const only).
- Tests: `CliPhase3Tests` (+10 unit on pendency lines/summary file, +1 synthetic batch run), `Phase3DocxFixtureBuilder` (overload + `MarkupElocationId` const).

## Errors / Corrections

- First draft of the batch test used surname=ORCID with short given-names → "ABC" NotFound; fixed the fixture, not the expectation.

## Ready for Next Run

- task_09 can read pendencies straight from `_batch_summary.txt`; format is asserted line-for-line in `WritePhase3BatchSummary_KeepsHeaderAndFileLines_AndIndentsPendencyUnderTheirFile`.
- task_10 documents the block format in README; copy the wording from `CliApp.DescribeCreditPendency`.
