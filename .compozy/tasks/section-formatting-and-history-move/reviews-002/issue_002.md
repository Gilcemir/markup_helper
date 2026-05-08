---
provider: manual
pr:
round: 2
round_created_at: 2026-05-08T13:47:48Z
status: resolved
file: DocFormatter.Core/Rules/MoveHistoryRule.cs
line: 156
severity: medium
author: claude-code
provider_ref:
---

# Issue 002: DiagnosticHistoryMove.FromIndex permanece sempre null

## Review Comment

O schema `DiagnosticHistoryMove(... FromIndex, ToIndexBeforeIntro, ...)`
foi desenhado para rastrear *de onde* o histórico foi movido e *para onde*.
Mas `MoveHistoryRule.MovedMessagePrefix` ("history moved (3 paragraphs
placed before INTRODUCTION at position {finalAnchorIndex})") só carrega o
índice final, então o `BuildHistoryMove` (`DiagnosticWriter.cs:287`) é
forçado a setar `FromIndex: null` em todo cenário — o comentário do método
nas linhas 217–219 do writer admite isso explicitamente.

Resultado: o campo aparece no JSON em todo arquivo da Phase 3 como
`"fromIndex": null`, ruído puro. Consumidores que esperam usar isso para
calcular o "salto" do bloco de histórico não têm o dado.

Duas opções de fix (escolher uma):

1. **Fazer o rule emitir `receivedIndex`**: ampliar `MovedMessagePrefix`
   para `"history moved from {receivedIndex} to {finalAnchorIndex - 3}"`
   (ou similar) e atualizar o parser. Mantém o schema vivo.
2. **Remover `FromIndex` do schema**: TechSpec §"Data Models" não justifica
   o campo além de "rastrear origem"; se ninguém consome, dropar.

Opção (1) é a mais alinhada com o intuito original — o `receivedIndex` já
existe na execução do rule (linha 87 do `MoveHistoryRule.cs`), só não é
exposto na mensagem.

## Triage

- Decision: `VALID`
- Notes: Aplicada opção 1 (rule emite `receivedIndex`). Adicionada const `MovedMessageOriginInfix = " from index "`; `MoveHistoryRule.Apply` agora emite `${MovedMessagePrefix}{finalAnchorIndex}{MovedMessageOriginInfix}{receivedIndex})`. Novo helper `TryParseMovedIndices` em `DiagnosticWriter` extrai os dois inteiros e popula `ToIndexBeforeIntro` + `FromIndex`. Tests `MoveHistoryRuleTests.Apply_WellFormed*` e `Apply_IntegrationWithPhase1And2*` ajustados (Contains + EndsWith em vez de assert direto no introIndex). DiagnosticWriterTests `Build_HistoryMove_MovedInfo_*` agora valida `FromIndex == 5`.
