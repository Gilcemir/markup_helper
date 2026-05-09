---
name: scielo-source-reference
description: Referência de último recurso ao código-fonte original da SciELO (PC-Programs) — macros VBA brutas e DTDs SGML originais. Use SOMENTE quando `docs/scielo_context/` não contém a resposta para uma decisão sobre tag/regra SciELO, E você já leu o arquivo relevante de `docs/scielo_context/`. Os docs curados são autoritativos para casos normais — esta skill é fallback raro para detalhes ausentes na documentação curada. Não use para programação geral, code review, ou qualquer coisa fora de semântica SciELO legada.
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

## Localização

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
