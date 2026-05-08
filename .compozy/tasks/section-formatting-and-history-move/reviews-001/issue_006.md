---
provider: manual
pr:
round: 1
round_created_at: 2026-05-08T12:27:28Z
status: resolved
file: DocFormatter.Tests/CliIntegrationTests.cs
line: 0
severity: medium
author: claude-code
provider_ref:
---

# Issue 006: Teste de integração end-to-end da Phase 3 ausente

## Review Comment

A TechSpec §"Integration Tests" diz: "`CliIntegrationTests.cs` is extended
with one new case: a synthetic fixture exercising both Phase 3 rules
end-to-end alongside the Phase 1+2 pipeline. Assertions verify (a) the
diagnostic JSON contains both new sections only when warnings fire; (b) the
history block is placed correctly when an `INTRODUCTION` anchor exists; (c)
the `Phase3DocxFixtureBuilder` output matches expected paragraph order."

Esse caso era a entrega final da task_07 e não foi executado. Sem ele:

- A regressão do issue 002 (HistoryMove sempre `null`) não é detectada pelo
  CI mesmo após a issue 002 ser corrigida — só os unit tests do
  `DiagnosticWriter` ficam de guarda, e eles têm escopo isolado de regra.
- A integração entre `MoveHistoryRule` (mutação de body) e regras Phase 1+2
  que rodam antes/depois (especialmente `LocateAbstractAndInsertElocationRule`,
  que também mexe em índices de parágrafo) não tem cobertura de pipeline.
- `Phase3DocxFixtureBuilder` (já criado pela task_01) não tem
  smoke-test público que prove que o fixture forma um documento válido para
  o `WordprocessingDocument` reabrir após `Save` (importante quando o builder
  for reutilizado pelas regras 04/06).

Fix: adicionar um único caso no `CliIntegrationTests` exercitando o pipeline
completo (DI registrado por issue 001) sobre um documento sintético com
header, abstract, history block, INTRODUCTION e body sections. Validar:
ordem de parágrafos, conteúdo do `*.report.txt`, presença/conteúdo do
`*.diagnostic.json` (`historyMove.applied=true`, `sectionPromotion.applied=true`).

## Triage

- Decision: `VALID`
- Notes: Resolvida no commit `d271d9f` (task_07): `CliIntegrationTests.cs` ganha 3 cenários E2E — DI ordering (linhas 255-278), `Run_Phase3_HappyPath_MovesHistoryAndPromotesSectionsEndToEnd` (linhas 400-486) e `Run_Phase3_AnchorMissing_BothRulesEmitWarn_DiagnosticReportsSkippedReason` (linha 488+). Validam paragraph order, JSON diagnostic conteúdo, report.txt, e text-preservation invariant.
