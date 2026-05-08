---
provider: manual
pr:
round: 1
round_created_at: 2026-05-08T12:27:28Z
status: resolved
file: DocFormatter.Core/Reporting/DiagnosticWriter.cs
line: 99
severity: high
author: claude-code
provider_ref:
---

# Issue 004: BuildFormatting early-return ignora entradas das regras Phase 3

## Review Comment

`BuildFormatting` decide se devolve `null` (omitindo o objeto `formatting` no
JSON) com base apenas em entradas das regras Phase 2:

```csharp
if (!HasWarnOrError(alignment)
    && !HasWarnOrError(spacing)
    && !HasWarnOrError(email)
    && !HasWarnOrError(abs))
{
    return null;
}
```

Quando o issue 002 for resolvido e os campos Phase 3 começarem a ser
populados, essa lógica vai descartar silenciosamente sinais de
`MoveHistoryRule` (`anchor_missing`, `partial_block`, `out_of_order`,
`not_adjacent`) e `PromoteSectionsRule` em documentos onde Phase 2 não
produziu nenhum `[WARN]`. Consumidores do batch perdem o sinal exatamente
nos casos em que ele importa: artigos onde só a Phase 3 falhou.

Fix sugerido: incluir as duas novas regras no early-return e rotear suas
entradas adiante para os respectivos `Build*` methods. Note também que o
trigger global de escrita do arquivo (linha 29 `report.HighestLevel < ReportLevel.Warn`)
deve continuar funcionando porque as próprias regras Phase 3 emitem
`[WARN]` nos seus skip-paths.

```csharp
var historyEntries = FilterByRule(report, nameof(MoveHistoryRule));
var promotionEntries = FilterByRule(report, nameof(PromoteSectionsRule));

if (!HasWarnOrError(alignment) && !HasWarnOrError(spacing)
    && !HasWarnOrError(email) && !HasWarnOrError(abs)
    && !HasWarnOrError(historyEntries) && !HasWarnOrError(promotionEntries))
{
    return null;
}
```

## Triage

- Decision: `VALID`
- Notes: Resolvida no commit `5bed297` (task_06): `BuildFormatting` agora computa `hasPhase12Signal` e `hasPhase3Signal` separadamente (linhas 101-109) e só retorna null se ambos forem falsos. Sinais de `MoveHistoryRule`/`PromoteSectionsRule` em documentos onde Phase 2 ficou silenciosa agora aparecem corretamente no JSON.
