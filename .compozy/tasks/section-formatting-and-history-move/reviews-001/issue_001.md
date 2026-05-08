---
provider: manual
pr:
round: 1
round_created_at: 2026-05-08T12:27:28Z
status: resolved
file: DocFormatter.Cli/CliApp.cs
line: 214
severity: critical
author: claude-code
provider_ref:
---

# Issue 001: MoveHistoryRule e PromoteSectionsRule não registrados na DI

## Review Comment

`MoveHistoryRule` foi implementada e tem 12 testes verdes, mas não está registrada
no `BuildServiceProvider`. A última `services.AddTransient<IFormattingRule, ...>`
em `DocFormatter.Cli/CliApp.cs:214` ainda é `LocateAbstractAndInsertElocationRule`,
exatamente como antes da Phase 3. Resultado: a regra é código morto no pipeline
real — nenhum `.docx` processado pela CLI tem seu histórico reordenado, mesmo
que o usuário rode o binário recém-buildado. Como `PromoteSectionsRule` não foi
implementada (issue 003), aquela registration também está faltando.

Esta era a entrega da task_07 (não concluída pelo Compozy). Sem ela, todo o
trabalho de task_01/03/05 é invisível em produção.

Fix sugerido (após `PromoteSectionsRule` existir):

```csharp
services.AddTransient<IFormattingRule, LocateAbstractAndInsertElocationRule>();
services.AddTransient<IFormattingRule, MoveHistoryRule>();
services.AddTransient<IFormattingRule, PromoteSectionsRule>();
```

A ordem importa: ambas devem ficar após `LocateAbstractAndInsertElocationRule`
e antes de qualquer regra futura, posições #10 e #11 conforme TechSpec
"Component Overview".

## Triage

- Decision: `VALID`
- Notes: Issue era válida e bloqueante. Resolvida no commit `d271d9f` (task_07): `DocFormatter.Cli/CliApp.cs:215-216` adicionou `services.AddTransient<IFormattingRule, MoveHistoryRule>()` e `services.AddTransient<IFormattingRule, PromoteSectionsRule>()` após `LocateAbstractAndInsertElocationRule`, exatamente como sugerido. Integration test `Run_FormattingRulesOrder_DiContainerYieldsExpectedSequence` (em `CliIntegrationTests.cs:260-277`) bloqueia regressão de ordem de registro.
