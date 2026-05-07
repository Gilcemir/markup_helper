# Task Memory: task_05.md

Keep only task-local execution context here. Do not duplicate facts that are obvious from the repository, task file, PRD documents, or git history.

## Objective Snapshot
Implementar `ApplyHeaderAlignmentRule` (Optional) que aplica Right/Right/Center via OOXML `Justification` aos parágrafos de DOI/seção/título publicados pelas tasks 03/04 no `FormattingContext`.

## Important Decisions
- Reescrita idempotente: remove qualquer `Justification` existente e reaplica o valor alvo. Mantém a regra trivialmente determinística e dispensa branch de "já alinhado".
- WARN apenas quando o `Paragraph?` no contexto é null. Pré-alinhado **não** emite WARN (idempotência exigida pelo spec).
- Constantes públicas para as 3 mensagens de WARN — task_09 chaveia diagnóstico por elas em `DiagnosticWriter.BuildAlignment`.
- `ArgumentNullException.ThrowIfNull` permanece nos 3 args top-level (`doc`, `ctx`, `report`) — guardas de contrato de método, não conflitam com o requirement de "não throw em campo nulo do contexto".

## Learnings
- `paragraph.ParagraphProperties ??= new ParagraphProperties()` preserva outras propriedades (ex. `ParagraphStyleId`) ao acrescentar `Justification`. Coberto pelo teste `Apply_WithExistingParagraphPropertiesButNoJustification_PreservesOtherProperties`.

## Files / Surfaces
- Cria: `DocFormatter.Core/Rules/ApplyHeaderAlignmentRule.cs`, `DocFormatter.Tests/ApplyHeaderAlignmentRuleTests.cs`.
- Lê (não muta): `FormattingContext.DoiParagraph` / `SectionParagraph` / `TitleParagraph`.
- Constantes consumidas downstream por: `DocFormatter.Core/Reporting/DiagnosticWriter.cs` (task_09), registro DI em `DocFormatter.Cli/CliApp.cs` (task_10).

## Errors / Corrections

## Ready for Next Run
- task_09 deve usar `ApplyHeaderAlignmentRule.Missing{Doi,Section,Title}ParagraphMessage` para popular `DiagnosticAlignment(doiOk, sectionOk, titleOk)` — a flag `*Ok` é `true` quando o WARN correspondente está ausente.
- task_10 deve posicionar a regra após `RewriteHeaderMvpRule` (que publica DoiParagraph) e antes de `EnsureAuthorBlockSpacingRule` (ADR-001).
