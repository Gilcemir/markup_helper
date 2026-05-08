---
provider: manual
pr:
round: 1
round_created_at: 2026-05-08T12:27:28Z
status: resolved
file: DocFormatter.Core/Reporting/DiagnosticWriter.cs
line: 112
severity: high
author: claude-code
provider_ref:
---

# Issue 002: DiagnosticWriter sempre escreve HistoryMove e SectionPromotion como null

## Review Comment

Em `DiagnosticWriter.BuildFormatting` (linhas 107–113) os dois novos campos
`HistoryMove` e `SectionPromotion` foram adicionados ao construtor com valores
literais `null`. O commit message do task_05 (`655f7c0`) admite que "a
população real desses campos fica para a task_06". Mas task_06 não foi
executada, então essa ponte nunca foi construída.

Consequência: mesmo que `MoveHistoryRule` venha a ser registrada (issue 001),
todos os `*.diagnostic.json` continuarão exibindo `historyMove: null` e
`sectionPromotion: null` em qualquer cenário — `[INFO] history moved`,
`[WARN] anchor_missing`, `[WARN] partial`, etc. Editores que dependem do JSON
para triagem em batch perdem 100% da observabilidade da Phase 3.

Fix: implementar `BuildHistoryMove(filterByRule(report, nameof(MoveHistoryRule)))`
mapeando as `MoveHistoryRule.*Message*` constants para os campos do
`DiagnosticHistoryMove` (`Applied`, `SkippedReason`, `AnchorFound`, `FromIndex`,
`ToIndexBeforeIntro`, `ParagraphsMoved`). Mesmo padrão para `BuildSectionPromotion`.
TechSpec §"Data Models" lista o mapping exato e §"Build Order" passo 9
descreve o método.

## Triage

- Decision: `VALID`
- Notes: Resolvida no commit `5bed297` (task_06): `DiagnosticWriter.BuildFormatting` agora chama `BuildHistoryMove(historyMoveEntries)` e `BuildSectionPromotion(sectionPromotionEntries)` em vez de hard-code null. Os dois novos métodos privados (linhas 220-323 e 329-399 do writer) parseiam mensagens das regras Phase 3 e reconstroem os records. Integration test `Run_Phase3_HappyPath_*` valida `historyMove.applied=true, paragraphsMoved=3` e `sectionPromotion.applied=true` no JSON gerado. Follow-ups de precisão dos campos vão para `reviews-002/`.
