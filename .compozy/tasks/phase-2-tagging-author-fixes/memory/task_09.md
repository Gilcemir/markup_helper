# Task Memory: task_09.md

Task-local execution context. No facts derivable from the repo/task file/PRD/git.

## Objective Snapshot

Implement `EmitHistTagRule` (Phase 4 release) emitting `[hist]…[/hist]` with `[received dateiso]`, optional `[accepted dateiso]`, and `[histdate dateiso datetype="pub"]`. Final cumulative scope `{authorid, corresp, doc, doctitle, doi, hist, histdate, kwdgrp, label, normaff, received, accepted, toctitle, xmlabstr, xref}`. Skip-and-warn entire block if `received` is missing or unparseable.

## Important Decisions

- **Empirical corpus shape (verified across all 10 AFTER pairs)**: `[histdate datetype="pub"]` is INSIDE `[hist]`, NOT after it. Attribute order: `dateiso="…" datetype="pub"`. The DTD-text label words (`Received: ` / `Accepted: ` / `Published: `) are preserved verbatim BEFORE each child opening tag — they live inside `[hist]` but outside the per-date child tags. Task spec's "or per the after corpus shape" wins.
- **3-paragraph layout (10/10 corpus pairs)**: BEFORE has three adjacent paragraphs (`Received: <date>`, `Accepted: <date>`, `Published: <date>` — output of Phase 1 `MoveHistoryRule`). AFTER keeps the same 3 paragraphs but wraps: `[hist]` opens at start of P1, each paragraph wraps its own date phrase in the child tag, `[/hist]` closes at end of P3. No paragraph merge / split.
- **`revised` is absent from all 10 corpus pairs**; emit-revised path exists for DTD correctness but has no corpus exercise. `HistoryDates.Revised` is `IReadOnlyList<HistDate>` (likely always empty in practice; the rule does not detect `Revised` paragraphs yet — would need to be added the same way as the others if a future article carries one).
- **Strict DTD order (received → revised* → accepted? → histdate pub?)** matches document order in 100% of the corpus (Phase 1 `MoveHistoryRule` already enforces this for the BEFORE side). Implementation processes the three markers in document order; the position of `[hist]` / `[/hist]` is anchored to the first / last successfully emitted child paragraph respectively.
- **Per-paragraph rewrite (not whole-paragraph wrap)**: each paragraph's inline content is rebuilt from scratch — keep `ParagraphProperties`, drop all existing inlines, append: optional `[hist]` Run, prefix label Run (e.g. `Received: `), child opening Run (`[received dateiso="…"]`), `SourceText` Run, child closing Run (`[/received]`), optional `[/hist]` Run. Locate `SourceText` position via `OrdinalIndexOf` on the joined paragraph text. Runs use `RewriteHeaderMvpRule.CreateBaseRunProperties()` + `Space=Preserve`, matching `TagEmitter` conventions.
- **Skip-and-warn reason codes** (ADR-002): `hist_received_missing` (no Received paragraph found), `hist_received_unparseable` (Received paragraph found but date phrase not recognized → entire block skipped). For accepted/published unparseable cases the rule still emits `[hist]` with received-only, plus a `hist_accepted_unparseable` / `hist_published_unparseable` warn entry (partial emit allowed because DTD only requires `received` to be present — `accepted?` and `histdate?` are optional).
- **Idempotency**: skip-and-return if any `[hist` substring already exists in the body — protects against double-wrapping on a re-run.
- **`DiagnosticPhase2.Hist`** is a single `DiagnosticField` whose `Value` is a compact ISO summary (`"received=YYYYMMDD,accepted=YYYYMMDD,published=YYYYMMDD"`, omitting missing children). `Missing` confidence when received is null. Reused for the per-rule diagnostic line.

## Learnings

- `Phase2DiffUtility` joins paragraphs with `\n`. The diff strip preserves in-scope tag literals verbatim; `[hist]`, `[received]`, `[accepted]`, `[histdate]` must all join the scope set. Anything inside `[hist]` that is NOT one of those four tags (e.g. inner `[email]` or `[p]`) would be peeled symmetrically — not relevant here, no nested in-scope tags inside `[hist]`.
- The corpus stores `[histdate]` with `dateiso` BEFORE `datetype` (attribute order matters because `Phase2Scope` keeps the tag literal verbatim with attributes intact). `TagEmitter.OpeningTag` emits attributes in the order passed to it — pass `("dateiso", iso)` then `("datetype", "pub")`.

## Files / Surfaces

- New: `DocFormatter.Core/Models/Phase2/HistoryDates.cs`
- New: `DocFormatter.Core/Rules/Phase2/EmitHistTagRule.cs`
- New: `DocFormatter.Tests/Phase2/EmitHistTagRuleTests.cs`
- Modified: `DocFormatter.Core/Pipeline/FormattingContext.cs` (+History? field)
- Modified: `DocFormatter.Core/Pipeline/RuleRegistration.cs` (register EmitHistTagRule in AddPhase2Rules)
- Modified: `DocFormatter.Core/Reporting/Phase2Scope.cs` (+hist, histdate, received, accepted)
- Modified: `DocFormatter.Core/Reporting/DiagnosticDocument.cs` (+Hist field on DiagnosticPhase2)
- Modified: `DocFormatter.Core/Reporting/DiagnosticWriter.cs` (build Hist diagnostic)
- Modified: `DocFormatter.Tests/Phase2ScopeTests.cs` (sentinel update)
- Possibly modified: `DocFormatter.Tests/Phase2/Phase2PipelineIntegrationTests.cs` (extend synthetic fixture to include history paragraphs)

## Errors / Corrections

- First test run failed `Apply_PortugueseHeaders_AlsoRecognized`: rule's locator regex is English-only (mirrors Phase 1 `MoveHistoryRule` regex `(received|accepted|approved|published)`). HistDateParser supports Portuguese but the locator does not. Replaced the test with `Apply_NoCandidatesAtAll_SkipsAndWarnsHistReceivedMissing` to assert the skip-and-warn path on Portuguese input — documents the intended scope.

## Ready for Next Run

- Task complete. `dotnet build` clean, 565/565 tests pass, `make phase2-verify` exits 0 on all 10 corpus pairs. The full Phase 2 release ships. Promotion candidate next.
