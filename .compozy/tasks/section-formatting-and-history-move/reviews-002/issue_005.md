---
provider: manual
pr:
round: 2
round_created_at: 2026-05-08T13:47:48Z
status: resolved
file: DocFormatter.Core/Rules/PromoteSectionsRule.cs
line: 60
severity: low
author: claude-code
provider_ref:
---

# Issue 005: PromoteSectionsRule não filtra parágrafos dentro de text boxes

## Review Comment

A iteração na linha 56 usa `body.Descendants<Paragraph>()`, que percorre
todos os parágrafos descendentes, inclusive os aninhados em
`<w:txbxContent>` (text boxes), `<w:sdt>` (structured document tags) e
outros containers. O filtro `BodySectionDetector.IsInsideTable` só remove
parágrafos cujo ancestral é `<w:tbl>`. Um heading "MATERIAL AND METHODS"
posicionado dentro de uma caixa de texto após o anchor seria promovido a
16pt center mesmo estando fora do fluxo principal.

A TechSpec §"Known Risks" (linha 252) reconhece text boxes como out of
scope: "A document with `INTRODUCTION` declared inside a `<w:txbxContent>`
… would be missed. … `BodySectionDetector.FindIntroductionAnchor` scans
`body.Elements<Paragraph>()` (direct children) and `body.Iter<Paragraph>()`
only via `IsInsideTable` for filtering. Text boxes are out of scope".

Mas `PromoteSectionsRule` quebra essa simetria: o anchor lookup
(`body.Elements<Paragraph>`) é body-only, mas a iteração subsequente
(`body.Descendants<Paragraph>`) é recursiva. Em arquivos com text-box
"sidebar" depois do INTRODUCTION, conteúdos não relacionados podem ser
promovidos a 16pt center sem aviso.

Fix sugerido — reduzir escopo a body direto:

```csharp
var allParagraphs = body.Elements<Paragraph>().ToList();
var anchorIndex = allParagraphs.IndexOf(anchor);
```

Isso fecha o gap de simetria com `FindIntroductionAnchor` e elimina
necessidade do `IsInsideTable` filter (que continua sendo defesa em
profundidade contra futuros wrappers). Custo: zero risco de regressão nos
testes atuais (todos os fixtures usam parágrafos diretos do body), e
elimina uma classe inteira de edge cases.

## Triage

- Decision: `VALID`
- Notes: Iterator do loop principal trocado para `body.Elements<Paragraph>().ToList()` (alinhado com `FindIntroductionAnchor`). `IsInsideTable` filter mantido como defesa em profundidade (no-op com Elements, mas protege contra futuros refactors). Para preservar o counter `skippedInTables` requerido pelo issue_003 da mesma rodada, contagem é feita à parte por `CountParagraphsInsideTables(body)` em uma passagem separada via Descendants — escopo expansivo mas só pra observabilidade. Texto-boxes aninhados ficam fora da promoção pelo mesmo mecanismo do iterator restrito a body direct children.
