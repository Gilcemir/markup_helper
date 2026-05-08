---
provider: manual
pr:
round: 2
round_created_at: 2026-05-08T13:47:48Z
status: resolved
file: DocFormatter.Core/Reporting/DiagnosticWriter.cs
line: 316
severity: low
author: claude-code
provider_ref:
---

# Issue 004: Fallback unknown em BuildHistoryMove/BuildSectionPromotion afirma AnchorFound=false

## Review Comment

`BuildHistoryMove` (linhas 316–322) e `BuildSectionPromotion` (linhas
379–388) terminam com um fallback "unknown" quando há entradas mas
nenhuma message-pattern conhecida foi reconhecida. Ambos retornam
`AnchorFound: false` nesse caso.

Mas a precondição de chegar ao fallback é apenas "houve entradas e nenhuma
foi reconhecida" — isso **não prova** que o anchor não foi encontrado, só
que o writer não soube classificar a mensagem. Um dos cenários reais que
chega aqui: `MoveHistoryRule` recebeu um novo formato de mensagem (futura
extensão) e o writer ainda não tem o pattern.

Defaultar para `AnchorFound: false` injeta um sinal falso em consumidores
que filtram por `historyMove.anchorFound == false` para "documentos sem
INTRODUCTION".

Fix sugerido: usar `AnchorFound: true` no fallback (a maioria das mensagens
do rule só são emitidas após anchor ser localizado, e
`AnchorMissing/anchor_missing` já tem path próprio antes), ou dropar o
fallback e devolver `null` para a fala "sem informação confiável".

```csharp
return new DiagnosticHistoryMove(
    Applied: false,
    SkippedReason: "unknown",
    AnchorFound: true,   // mais conservador
    FromIndex: null,
    ToIndexBeforeIntro: null,
    ParagraphsMoved: 0);
```

Mesmo ajuste em `BuildSectionPromotion` (linha 382).

## Triage

- Decision: `VALID`
- Notes: Fallback "unknown" em `BuildHistoryMove` (linha 322) e `BuildSectionPromotion` (linha 388) agora retornam `AnchorFound: true`. Justificativa: o fallback só é alcançado quando há entradas mas nenhuma message-pattern reconhecida — a maioria das mensagens só são emitidas após anchor ser encontrado, então `true` é o default conservador.
