# Contexto SciELO — base de conhecimento para gerar código no DocFormatter

> **Para o agente que lê este arquivo:** este diretório contém o
> resultado de uma análise estática completa do sistema SciELO Markup
> (legacy, arquivado em `scieloorg/PC-Programs`). É a fonte autoritativa
> de informação para implementar regras C# em `DocFormatter.Core/Rules/`
> que detectem padrões em `.docx` e emitam tags SciELO em colchetes
> (`[tag attr="v"]…[/tag]`).
>
> Os arquivos foram **curados** — eles condensam um dump VBA de 2,6 MB
> (116 mil linhas) e cinco DTDs SGML em um conjunto de markdowns
> específicos. **Leia este README inteiro antes de carregar qualquer
> outro arquivo deste diretório.**

## TL;DR do sistema modelado

A SciELO opera um pipeline com 3 estágios:

1. **DocFormatter (este projeto)** — formata `.docx` e (fase 2) pré-marca
   algumas tags facilmente detectáveis, deixando o `.docx` no formato
   esperado pelo Markup.
2. **SciELO Markup** — operador humano abre o `.docx` no Word com o
   global template `markup.prg` carregado, e clica em botões da toolbar
   "Markup" para inserir as tags restantes (manuais ou automáticas).
   Resultado: `.docx` com texto + literais `[tag]…[/tag]` no fluxo.
3. **`parser.exe` + `convert.exe`** (Win32, externo) — lê o texto exportado,
   valida contra DTD SGML, gera XML JATS final.

O `DocFormatter` **não substitui** os estágios 2 e 3; ele só prepara
melhor a entrada do estágio 2 e (fase 2) já preenche tags fáceis.

## Decisão arquitetural já tomada

- **Foco em DTD 4.0** (`art4_0.dtd` + `text4_0.dtd` + `doc4_0.dtd` +
  `common4_0.dtd` + `citation4_0.dtd`).
- **Delimitadores fixos**: `[` (STAGO), `[/` (ETAGO), `]` (TAGC).
  Atributos: `[tag name="value"]` com aspas duplas, sem aspas no valor.
- **Convenção de IDs**: `aff1`/`aff2`/…, `c1`/`c2`/… (corresp), `r1`/`r2`/…
  (refs), `fn1` (footnotes).
- **`role` em `[author]`**: `nd` (autor), `ed` (editor), `tr` (tradutor),
  `org` (organizador). Default = `nd`.
- **`dateiso`**: `YYYYMMDD` (zerado quando faltar mês/dia: `20230600`,
  `20230000`).
- **Lista de tags da fase 2**: `authors`, `author`, `fname`, `surname`,
  `aff`, `corresp`, `authorid` (= `[ctrbid ctrbidtp="orcid"]`), `hist`
  (`received`/`accepted`/`revised`), `kwdgrp`/`kwd`, `abstract`,
  `doctitle`/`subtitle`, `doi`, `email`, `url`, `xref ref-type="aff"`,
  `xref ref-type="corresp" rid="c1"`. **Refs ficam fora** (já existe
  automação externa).

## Roteamento por tarefa — qual arquivo ler

| Sua tarefa de codificação | Leia primeiro | Depois | Pode ignorar |
|---|---|---|---|
| Implementar detecção/emissão de uma das tags da fase 2 | `DTD_SCHEMA.md` | `TAG_INDEX.md` (heurísticas que o Markup já usa), `REENTRANCE.md` (o que evitar) | resto |
| Decidir se uma tag pode ser inserida em determinado contexto (validação hierárquica) | `HIERARCHY.md` | `DTD_SCHEMA.md` para confirmar com o DTD oficial | resto |
| Entender o que pode quebrar quando o operador SciELO abre o `.docx` pré-marcado | `REENTRANCE.md` | `TAG_INDEX.md` (lista de auto-marks que não checam existência) | resto |
| Decidir formato de atributo / valores aceitos | `DTD_SCHEMA.md` (seção "Atributos com valores controlados") | `_raw/_reverse_index.txt` se precisar do mapa cru | resto |
| Investigar comportamento exato de uma macro VBA da SciELO | TAG_INDEX.md tem o ponteiro `markup_macros.txt:LINHA` | dump bruto em `/Users/educbank/Documents/personal_workspace/PC-Programs/_analysis/markup_macros.txt` | — |
| Entender o pipeline geral / contexto histórico | `ARQUITETURA_ADDIN_WORD.md` | `UI_MENUS.md` se precisar ver fluxo de toolbar | restante |

## Tabela de arquivos

| Arquivo | Tamanho | Para que serve |
|---|---:|---|
| `DTD_SCHEMA.md` | 14 KB | **Schema autoritativo** das tags na DTD 4.0. Define: ordem dos filhos (`(received, revised*, accepted?)`), atributos `#REQUIRED`/`#IMPLIED`, valores convencionais. **Use isto como verdade absoluta** para gerar tags válidas. |
| `HIERARCHY.md` | 34 KB | Mapa pai→filhos extraído de `tree.txt`. Tem **índice reverso** (qual pai pode conter cada tag). Use para **validar contexto** antes de inserir. ⚠️ Diverge da DTD em alguns pontos — DTD prevalece. |
| `REENTRANCE.md` | 9 KB | **Crítico para fase 2.** Lista o que **NÃO pré-marcar** (porque o Markup re-aplica em cima e duplica) e o que pré-marcar com segurança. Inclui a armadilha do `markup_sup_as` x superescrito. |
| `TAG_INDEX.md` | 20 KB | Tabela tag↔macro VBA. Para cada tag, mostra **se há heurística específica** que a SciELO usa (ex.: separadores `;`/`,`/`&`/`and`/`y`/`e`/`et al` em `mark_authors`). Útil para o C# **espelhar** a heurística do Markup. |
| `ARQUITETURA_ADDIN_WORD.md` | 22 KB | Visão geral do pipeline SciELO. Leia se precisar de contexto histórico ou for explicar para alguém. |
| `UI_MENUS.md` | 6 KB | Como a barra de botões do Markup é construída. Útil só se for entender por que certo botão dispara certa macro. |

## Indices crus (`_raw/`)

Use só se precisar de busca textual:

| Arquivo | Conteúdo |
|---|---|
| `_raw/_reverse_index.txt` | TSV: `tag\t<pais separados por vírgula>` (gerado de `tree.txt`) |
| `_raw/tags_in_tree.txt` | Lista de 239 tags-botão da UI |
| `_raw/tags_in_docs.txt` | Lista de 219 tags do `markup_tags.rst` da SciELO |
| `_raw/tags_in_tag_text_range.txt` | 27 tags com inserção literal especializada no VBA |
| `_raw/markup_strings_brackets.txt` | Validação cruzada via `strings` no binário |
| `_raw/macros_index.txt`, `tables_macros_index.txt` | Índice `Sub`/`Function` dos dois dumps VBA |

## Dump bruto (escape hatch)

Quando uma resposta exige o código VBA literal de uma macro citada em
`TAG_INDEX.md` por linha (ex.: "ver `markup_macros.txt:5083`"):

```
/Users/educbank/Documents/personal_workspace/PC-Programs/_analysis/markup_macros.txt
```

(2,6 MB, 116 359 linhas). **Não leia inteiro** — use `Bash` com
`sed -n 'INI,FIMp'` ou `Read` com `offset`/`limit`. As linhas citadas
em `TAG_INDEX.md` são as referências canônicas.

## 5 invariantes que o agente NÃO PODE esquecer

1. **`orgname` em `[aff]` é ATRIBUTO, não filho.** Forma correta:
   `[aff id="aff1" orgname="USP" orgdiv1="…"][label]1[/label]…[/aff]`.
   (Em `[normaff]` é o oposto: `orgname` é filho.)
   Fonte: `DTD_SCHEMA.md` seção `[aff]`.
2. **Pré-marcar `[label]` superescrito quebra `markup_sup_as`** — após
   pré-marcar, **zere `Font.Superscript = false`** no run envolvido,
   senão a auto-marcação SciELO duplica.
   Fonte: `REENTRANCE.md` seção "markup_sup_as".
3. **Não pré-marcar `[doctitle]`/`[doi]`/`[normaff]`/`[author]`/`[fname]`/
   `[surname]`/`[kwd]` se a equipe SciELO clicar `[doc]` (root) ou os
   botões `*authors`/`*kwdgrp`/`aff`** — essas auto-marks **não checam**
   existência prévia e duplicam tudo.
   Fonte: `REENTRANCE.md` "Auto-mark macros que não verificam".
4. **`role="nd"` é obrigatório em `[author]`** (DTD `#REQUIRED`).
   Padrão SciELO para autor normal. Não emita `[author]…[/author]` sem
   `role`.
   Fonte: `DTD_SCHEMA.md` seção `[author]`.
5. **`[hist]` é estritamente ordenado**: `(received, revised*, accepted?)`.
   `received` é obrigatório e primeiro; `accepted` é opcional e último.
   Fonte: `DTD_SCHEMA.md` seção `[hist]`.

## Idioma

Os arquivos estão em **pt-BR**. O código no `DocFormatter` está em C#
(.NET 10). Comentários nos arquivos VBA citados estão em pt-BR antigo
com encoding latin-1 (artefato do código legado SciELO de 2000–2020).
