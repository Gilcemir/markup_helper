---
name: scielo-source-reference
description: Referência de último recurso às fontes raw da SciELO — (1) código-fonte original PC-Programs (macros VBA + DTDs SGML 4.0, fase 2 bracket tags) e (2) SPS 1.10 (`_raw/SPS 1.10_pt.md`, JATS XML, fase 3). Use SOMENTE quando `docs/scielo_context/` não contém a resposta para uma decisão sobre tag/regra SciELO, E você já leu o arquivo curado relevante. Os docs curados são autoritativos para casos normais — esta skill é fallback raro para detalhes ausentes. Não use para programação geral, code review, ou qualquer coisa fora de semântica SciELO legada.
---

# SciELO Source Reference (último recurso)

Aponta para o código-fonte original da SciELO (PC-Programs), arquivado.
**Source de último recurso** quando os docs curados em
`docs/scielo_context/` não respondem a pergunta concreta sobre uma
tag/regra SciELO.

## Pré-condições antes de invocar esta skill

1. Você já leu `docs/scielo_context/README.md` e o arquivo específico
   recomendado (DTD_SCHEMA.md, HIERARCHY.md, REENTRANCE.md, etc.).
2. A resposta que você precisa **não está** lá.
3. A pergunta é especificamente sobre semântica/comportamento do
   sistema SciELO original — não sobre o código deste projeto.

Se as 3 condições não se aplicam, **não use** esta skill.

## Duas fontes raw

| Fonte | Paradigma | Curado em | Fallback raw |
|---|---|---|---|
| **PC-Programs** (VBA + DTD SGML 4.0) | Fase 2 — bracket tags `[tag]` | `DTD_SCHEMA.md`, `TAG_INDEX.md`, `HIERARCHY.md`, `REENTRANCE.md` | `PC-Programs/` (abaixo) |
| **SPS 1.10** (JATS XML) | Fase 3 — pós-Markup `<tag>` | `jats/*.md` | `docs/scielo_context/_raw/SPS 1.10_pt.md` |

Para a fase 3, leia primeiro o arquivo `jats/` da tag; só caia no SPS
raw (nas linhas citadas no topo de cada arquivo `jats/`) se faltar detalhe.

## Localização (PC-Programs)

```
/Users/educbank/Documents/personal_workspace/PC-Programs/
```

Repositório read-only e arquivado. **Não modifique.**

## Arquivos relevantes

- `src/scielo/bin/markup/markup.prg` — global template Word com macros
  VBA (origem das regras de marcação automática que rodam dentro do
  Word da SciELO).
- `src/scielo/bin/SGMLPars/*.dtd` — DTDs SGML originais. Versão alvo:
  4.0.
- `_analysis/markup_macros.txt` — dump descompilado das macros VBA
  (2,6 MB). Para navegar sem estourar contexto, use `sed -n` com
  offset, ex.: `sed -n '500,700p' _analysis/markup_macros.txt`.

## Como usar

1. Identifique a pergunta específica (ex.: *"qual é a ordem exata dos
   filhos de `<aff>` segundo a DTD original?"*).
2. Vá direto ao arquivo relevante (DTD para schema, markup_macros.txt
   para comportamento de auto-marcação, markup.prg para fluxo Word).
3. Leia em janelas pequenas. Não despeje o arquivo inteiro.
4. **Se encontrar informação útil que está faltando em
   `docs/scielo_context/`**: sugira ao usuário adicionar a info à
   documentação curada — assim agentes futuros não precisam recorrer
   a este fallback.

## Restrições

- Não modifique nada em `PC-Programs/`.
- Não use esta skill como atalho para evitar ler `docs/scielo_context/`
  primeiro. A documentação curada existe para evitar mergulho no dump
  bruto.
- Não cite o caminho `PC-Programs/` em ADRs ou docs de decisão como
  source of truth normativa — cite o arquivo correspondente em
  `docs/scielo_context/`. Se ele não existe ainda, crie/atualize.
