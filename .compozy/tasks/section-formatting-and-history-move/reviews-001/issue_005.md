---
provider: manual
pr:
round: 1
round_created_at: 2026-05-08T12:27:28Z
status: resolved
file: DocFormatter.Core/Rules/MoveHistoryRule.cs
line: 28
severity: medium
author: claude-code
provider_ref:
---

# Issue 005: HistoryMarkerRegex perde separadores en-dash e em-dash

## Review Comment

O regex usado para detectar as três linhas de histórico aceita apenas dois
pontos ASCII e hífen-menos:

```csharp
private static readonly Regex HistoryMarkerRegex = new(
    @"^(received|accepted|published)\s*[:\-]\s*.+",
    RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
```

Templates editoriais acadêmicos frequentemente usam en-dash (`–`, U+2013) ou
em-dash (`—`, U+2014) como separador, especialmente após edição automática
do Word ("Received – 12 Jan 2024"). Em documentos assim a regra não
encontra nenhum marker e cai em `[INFO] history block not found` — o
histórico nunca é movido.

Este projeto já corrigiu o mesmo padrão em outra regra na Phase 2 (commit
`5dbf9b1` "fix: triagem de sugestões do CodeRabbit (regex en-dash, alignment
typed setter, supressão de warns repetidos)"). A solução já validada no
codebase é incluir explicitamente os dois caracteres Unicode na character
class. Sugestão:

```csharp
@"^(received|accepted|published)\s*[:\-–—]\s*.+"
```

Os testes em `MoveHistoryRuleTests.Apply_RegexCaseInsensitivityAndSeparators_AllVariantsMatch`
(linhas 349–386) cobrem `:` e `-` mas não cobrem en-dash. Adicionar uma
linha `[InlineData("Received – 2024-01-15", ...)]` ao `[Theory]` evita a
regressão futura.

## Triage

- Decision: `VALID`
- Notes: Character class ampliado de `[:\-]` para `[:\-–—]` em `MoveHistoryRule.HistoryMarkerRegex:28`. Test theory `Apply_RegexCaseInsensitivityAndSeparators_AllVariantsMatch` ganhou dois novos `[InlineData]` (en-dash U+2013 e em-dash U+2014). Mesmo padrão já validado pelo commit `5dbf9b1` da Phase 2.
