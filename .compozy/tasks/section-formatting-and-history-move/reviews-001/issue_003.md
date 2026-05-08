---
provider: manual
pr:
round: 1
round_created_at: 2026-05-08T12:27:28Z
status: resolved
file: DocFormatter.Core/Rules/PromoteSectionsRule.cs
line: 0
severity: high
author: claude-code
provider_ref:
---

# Issue 003: PromoteSectionsRule não foi implementada

## Review Comment

A PRD/TechSpec especifica duas regras `Optional` para a Phase 3:
`MoveHistoryRule` (#10, implementada) e `PromoteSectionsRule` (#11, ausente).
Não há arquivo `DocFormatter.Core/Rules/PromoteSectionsRule.cs` nem
`PromoteSectionsRuleTests.cs` no commit range `76d3cbc..HEAD`. Esta era a
entrega da task_04, que ficou `pending` por causa do encerramento prematuro
do Compozy.

Impacto: o objetivo central da PRD — uniformizar a formatação de seções e
sub-seções no corpo do artigo (`<w:jc>` center + `<w:sz>` 16pt/14pt) —
permanece sem implementação. Toda a infraestrutura preparada por
`BodySectionDetector` (`IsSection`, `IsSubsection`, `IsInsideTable`,
cascade walker de bold) está presente, então a implementação fica
substancialmente reduzida a (a) iterar do anchor até o fim do body,
(b) aplicar o predicate, (c) mutar `<w:jc>` e adicionar/atualizar
`<w:sz>`/`<w:szCs>` com cuidado para preservar `ParagraphProperties`
existentes (TechSpec §Impact Analysis: "Mutate only `<w:jc>` and run-level
`<w:sz>`/`<w:szCs>`; never replace the entire `ParagraphProperties` element").

Fix: seguir o roteiro do task_04.md e a especificação de testes em
`_techspec.md` §"PromoteSectionsRuleTests" (8 cenários, incluindo
defence-in-depth via `FormattingContext.SectionParagraph` reference equality
e tabela-descendant filter).

## Triage

- Decision: `VALID`
- Notes: Resolvida no commit `8f895b6` (task_04): `DocFormatter.Core/Rules/PromoteSectionsRule.cs` (130 linhas) implementa anchor lookup, iteração do anchor até fim do body, filtros (IsInsideTable + context-skip), e mutação `<w:jc>` center + `<w:sz>`/`<w:szCs>` 32/28 half-points. `PromoteSectionsRuleTests` traz 16 cenários cobrindo happy path, anchor missing, table filter, idempotência, defence-in-depth via FormattingContext. Follow-ups de observabilidade (skip counters) e edge case de text-box vão para `reviews-002/`.
