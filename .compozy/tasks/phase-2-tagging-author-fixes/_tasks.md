# Phase-2 Tagging and Stage-1 Author Fixes — Task List

## Tasks

| # | Title | Status | Complexity | Dependencies |
|---|-------|--------|------------|--------------|
| 01 | Phase 1 — Fix `ExtractAuthorsRule` so SciELO Markup auto-marks authors on 5313 and 5449 (+ ADR-008) | completed | high | — |
| 02 | `TagEmitter` helper — emit SciELO `[tag attr="v"]…[/tag]` literals as OpenXML Runs | completed | medium | — |
| 03 | `Phase2DiffUtility` — body-text extraction with scope-filtered string compare | completed | medium | — |
| 04 | `RuleRegistration` — `AddPhase1Rules` / `AddPhase2Rules` extension methods on `IServiceCollection` | completed | low | task_02 |
| 05 | CLI subcommand dispatcher — `phase2` and `phase2-verify` in `CliApp.Run` + Makefile targets | completed | medium | task_03, task_04 |
| 06 | Phase 2 release — `EmitElocationTagRule`, `EmitAbstractTagRule`, `EmitKwdgrpTagRule` + corpus integration test | completed | high | task_02, task_04, task_05 |
| 07 | Phase 3 release — `EmitCorrespTagRule` and `EmitAuthorXrefsRule` (xref aff/corresp, `[authorid]`, author attrs) | completed | high | task_06 |
| 08 | `HistDateParser` (TDD) — phrase-inventory parser → `HistDate` records → `dateiso` `YYYYMMDD` | completed | medium | — |
| 09 | Phase 4 release — `EmitHistTagRule` (`received`/`revised*`/`accepted?` + `[histdate datetype="pub"]`) | completed | high | task_07, task_08 |
