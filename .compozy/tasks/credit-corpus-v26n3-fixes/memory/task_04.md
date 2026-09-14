# Task Memory: task_04.md

Keep only task-local execution context here. Do not duplicate facts that are obvious from the repository, task file, PRD documents, or git history.

## Objective Snapshot

- Done 2026-09-14: `CreditOutcome`/`CreditEntryOutcome` + `CreditDisposition`/`CreditResolution` constants, settable `Phase3Context.Credit`, injector records it on all 7 exits, header-empty WARN (INV-02), `[p]` + `;` pipeline integration fixture. Suite 848 → 860.

## Important Decisions

- Disposition/resolution vocabularies are `const string`s in `CreditDisposition`/`CreditResolution` (same file as the records) so tasks 06/07 compare against names, not literals.
- `Shape` is `CreditShape.Prose` for headerEmpty/absent (no body to parse), matching `CreditStatementParser.Parse(null)`.
- `Applied` is per-run truth: `Emit`/`EmitFreeText` return the set of author keys actually written, so an idempotency-skipped contrib (already had `<role>`) is `Applied == false` even under `autoApplied`.
- Gated path where the confirmer returns `AutoApplied` (e.g. `AutoAcceptConfirmer`) is still recorded as `confirmed`: the disposition describes the document path, not the confirmer's enum.

## Learnings

- Bare initials "AS" against "Ana Beatriz Silva"/"Ana Carolina Silva" is NotFound (full candidates are ABS/SAB); use "Ana Silva" + "Antônio Souza" for a Tier-1 ambiguity fixture.
- Running the four registered Phase 3 injectors in a test only needs `<article-id pub-id-type="doi">` in the XML; EditedBy/DataAvailability INFO-skip on missing source text.

## Files / Surfaces

- New `DocFormatter.Core/Jats/CreditOutcome.cs`; modified `CreditRolesInjector.cs` (PlanItem gains `Status` + `UnknownTerms`), `Phase3Context.cs`.
- Tests: `CreditRolesInjectorTests` (helper `ApplyAndAssertOutcome` wraps every `Apply` and asserts `ctx.Credit != null`; `CreateContext` gained `creditHeaderFound`), `Phase3PipelineIntegrationTests` (now IDisposable with temp dir), `Phase3DocxFixtureBuilder.WriteMarkupDocxWithCreditStatement` + `MarkupDocHeaderText`/`MarkupDoi`/`ParagraphTaggedSemicolonCreditText`.

## Errors / Corrections

- Blanket sed of `new CreditRolesInjector().Apply(` → helper also rewrote the call inside the helper (infinite recursion); fixed before build.

## Ready for Next Run

- task_06 reads `ctx.Credit` (`Raw`, `Shape.ToString()`, `Entries`) into `DiagnosticPhase3.CreditStatement`; task_07 reads `Disposition`/entries for the summary block.
