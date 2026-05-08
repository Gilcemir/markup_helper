---
provider: manual
pr:
round: 2
round_created_at: 2026-05-08T13:47:48Z
status: resolved
file: DocFormatter.Core/Reporting/DiagnosticWriter.cs
line: 292
severity: medium
author: claude-code
provider_ref:
---

# Issue 001: AlreadyAdjacent emite Applied=true com ParagraphsMoved=0

## Review Comment

`BuildHistoryMove` (linhas 292–301) responde ao caso `AlreadyAdjacentMessage`
com:

```csharp
return new DiagnosticHistoryMove(
    Applied: true,
    SkippedReason: null,
    AnchorFound: true,
    FromIndex: null,
    ToIndexBeforeIntro: null,
    ParagraphsMoved: 0);
```

Isso cria um vetor inconsistente que consumidores do JSON não conseguem
desambiguar: "trabalho aplicado" (`Applied=true`, `SkippedReason=null`) mas
"zero parágrafos movidos" (`ParagraphsMoved=0`). Para um editor que filtra
batches por `formatting.history_move.paragraphsMoved < 3` para encontrar
artigos que precisam de revisão, a re-execução de uma rule idempotente
geraria falso-positivos.

Há duas leituras consistentes — escolha uma:

1. **Idempotência reforçada**: `Applied: true, ParagraphsMoved: 3,
   SkippedReason: null` — o estado final tem 3 parágrafos no lugar correto,
   independente de quantos se moveram nesta passagem.
2. **Skip explícito**: `Applied: false, ParagraphsMoved: 0,
   SkippedReason: "already_adjacent"` — alinha com os outros skip reasons
   já documentados (`anchor_missing`, `partial_block`, `out_of_order`,
   `not_adjacent`, `not_found`) e adiciona "already_adjacent" como sexto.

Recomendo (1) porque combina com `MovedMessagePrefix` reportar a posição
final do anchor — o estado é "histórico já no lugar" tanto na primeira
quanto na segunda passagem. Os testes em `DiagnosticDocumentTests`
documentam apenas os reasons antigos; ajuste corresponderia.

## Triage

- Decision: `VALID`
- Notes: Aplicada opção 1 (idempotência reforçada). `BuildHistoryMove` para `AlreadyAdjacent` agora retorna `Applied: true, ParagraphsMoved: 3, AnchorFound: true`. Test `Build_HistoryMove_AlreadyAdjacentInfo_*` renomeado e ajustado (assertion `Equal(3, ParagraphsMoved)`).
