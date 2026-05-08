---
provider: manual
pr:
round: 2
round_created_at: 2026-05-08T13:47:48Z
status: resolved
file: DocFormatter.Core/Rules/PromoteSectionsRule.cs
line: 84
severity: medium
author: claude-code
provider_ref:
---

# Issue 003: SkippedParagraphsInsideTables e BeforeAnchor sempre zero no diagnóstico

## Review Comment

O schema `DiagnosticSectionPromotion` reserva dois contadores —
`SkippedParagraphsInsideTables` e `SkippedParagraphsBeforeAnchor` — para
observabilidade do filtro de `IsInsideTable` e do iterator começando no
anchor. O comentário do `BuildSectionPromotion`
(`DiagnosticWriter.cs:325–328`) admite que estes campos "are not currently
emitted by the rule and stay at zero".

A `PromoteSectionsRule.Apply` (linhas 56–80) realmente faz o `continue`
para parágrafos dentro de tabelas e arranca o loop a partir do anchor, mas
nunca conta nem reporta. O summary message
("`{SummaryPromotedPrefix}{X}{SummarySectionsInfix}{Y}{SummarySubsectionsSuffix}`")
só carrega `sectionsPromoted` e `subsectionsPromoted` — perdemos os dois
sinais mais úteis para diagnosticar "por que esse arquivo só promoveu 2
seções".

Fix sugerido em `PromoteSectionsRule.Apply`:

```csharp
var skippedInTables = 0;
var skippedBeforeAnchor = anchorBodyIndex; // ou descendents-based count
// ... no loop:
if (BodySectionDetector.IsInsideTable(paragraph)) { skippedInTables++; continue; }
// ... ao final, ampliar a INFO summary:
report.Info(Name,
    $"{SummaryPromotedPrefix}{sectionsPromoted}{SummarySectionsInfix}"
    + $"{subsectionsPromoted}{SummarySubsectionsSuffix} "
    + $"(skipped: {skippedInTables} in tables, {skippedBeforeAnchor} before anchor)");
```

E `DiagnosticWriter.BuildSectionPromotion` ganha um parser para os dois
contadores extra. Alternativa pragmática: dropar os dois campos do schema
se ninguém vai consumir.

## Triage

- Decision: `VALID`
- Notes: Adicionadas três consts em `PromoteSectionsRule` (`SkipCountsMessagePrefix`, `SkipCountsInTablesInfix`, `SkipCountsBeforeAnchorSuffix`) e novo helper `CountParagraphsInsideTables(body)`. `Apply` agora emite uma TERCEIRA INFO message no formato `"skipped {N} paragraphs inside tables and {M} paragraphs before anchor"`. `BuildSectionPromotion` ganhou parser `TryParseSkipCounts` e popula `SkippedParagraphsInsideTables` e `SkippedParagraphsBeforeAnchor`. Tests `Apply_SuccessPath_*` (PromoteSectionsRule) e `Build_BothPhase3RulesRan_*`/`Write_Phase3JsonRoundTrip_*` (DiagnosticWriter) atualizados.
