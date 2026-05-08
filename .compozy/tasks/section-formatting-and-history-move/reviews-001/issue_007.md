---
provider: manual
pr:
round: 1
round_created_at: 2026-05-08T12:27:28Z
status: resolved
file: DocFormatter.Core/Rules/MoveHistoryRule.cs
line: 115
severity: low
author: claude-code
provider_ref:
---

# Issue 007: Branch de partial-block contém código morto

## Review Comment

Em `MoveHistoryRule.Apply`:

```csharp
if (received is null)
{
    report.Info(Name, NotFoundMessage);
    return;
}

if (accepted is null || published is null)
{
    var rFlag = received is not null ? 1 : 0;   // sempre 1
    var aFlag = accepted is not null ? 1 : 0;
    var pFlag = published is not null ? 1 : 0;
    report.Warn(
        Name,
        $"{PartialBlockMessagePrefix}Received={rFlag} Accepted={aFlag} Published={pFlag} — not moved");
    return;
}
```

Após o early-return da linha 107, a única forma de chegar à linha 115 é com
`received != null`. Logo `rFlag` é sempre `1` — código morto disfarçado de
ramo defensivo. Não é bug, mas o leitor é forçado a reconciliar a intenção
"talvez Received esteja faltando" com o early-return acima, dois minutos
de cada vez. Substituir pelo literal `1`:

```csharp
report.Warn(
    Name,
    $"{PartialBlockMessagePrefix}Received=1 Accepted={(accepted is not null ? 1 : 0)} "
        + $"Published={(published is not null ? 1 : 0)} — not moved");
```

Ou — se quiser preservar simetria visual — extrair o `Flag(p) => p is not null ? 1 : 0`
como helper local. A escolha é cosmética.

## Triage

- Decision: `VALID`
- Notes: Linha `var rFlag = received is not null ? 1 : 0;` removida; substituída pelo literal `1` direto na string interpolada (`MoveHistoryRule.cs:115-119`). Cobertura de testes para o branch partial-block continua via `Apply_PartialBlock_*` (assertions com `StartsWith(PartialBlockMessagePrefix)`), inalterado.
