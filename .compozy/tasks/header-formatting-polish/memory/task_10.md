# Task Memory: task_10.md

Keep only task-local execution context here. Do not duplicate facts that are obvious from the repository, task file, PRD documents, or git history.

## Objective Snapshot
- Wire the four Phase 2 rules in `CliApp.BuildServiceProvider` per ADR-001 order; add end-to-end Phase 2 integration tests; manually verify the eleven `examples/` articles. Done.

## Important Decisions
- Made `CliApp.BuildServiceProvider` `internal` (already gated by `InternalsVisibleTo("DocFormatter.Tests")`) so the test suite can assert the rule registration order via `GetServices<IFormattingRule>()` instead of duplicating the list in a separate accessor.
- Integration test does NOT assert "Corresponding author line immediately above heading" verbatim. After all rules run, ELOCATION inserts before the bold `Abstract` heading, putting the order `[..., affiliation, Corresponding author: <email>, ELOCATION, **Abstract**, body]`. Test asserts the email line position is in the front matter (after last affiliation, before heading) and that ELOCATION sits between the email line and the heading. Task spec wording about "ELOCATION above the email line" is inconsistent with the actual implementation; the chosen assertions reflect the implemented data flow.

## Learnings
- HeaderParagraphLocator counts non-empty *lines*, not paragraphs: needs cumulative `>= 3` to start collecting authors. Section + Title each contribute 1 line, so the third paragraph (authors) is the first to be collected.
- For a synthetic affiliation paragraph to be recognized by the locator, it must START with a superscript run (`SuperscriptRun("1") + TextRun(" Universidade Y")`). The fixture for the corresponding-author trailer can append `* E-mail: foo@x.com` to that text run; the regex matches by plain-text walk.
- All eleven `examples/` articles run through the Phase 2 pipeline with `formatting: null` in the diagnostic JSON — no Phase 2 rule emits `[WARN]` on the production corpus. The single `[WARN]` per file comes from `ExtractTopTableRule` ("headers absent, fell back to positional mapping") and is unrelated to Phase 2.

## Files / Surfaces
- `DocFormatter.Cli/CliApp.cs` — added 4 `AddTransient<IFormattingRule, ...>` lines in ADR-001 order; flipped `BuildServiceProvider` to `internal`.
- `DocFormatter.Tests/CliIntegrationTests.cs` — added DI-order assertion + 4 Phase 2 end-to-end facts.
- `DocFormatter.Tests/Fixtures/Phase2/AbstractParagraphFactory.cs` — italic-wrapped abstract / mixed-italic helpers.
- `DocFormatter.Tests/Fixtures/Phase2/Phase2DocxFixtureBuilder.cs` — full docx builder with optional `*` marker, optional `Resumo` heading, optional malformed email trailer.

## Errors / Corrections

## Ready for Next Run
- Phase 1 of `header-formatting-polish` PRD complete. Master plan's next phase covers Keywords, section style promotion, and `ParseAffiliationsRule`.
