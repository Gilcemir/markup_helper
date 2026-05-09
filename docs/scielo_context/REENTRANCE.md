# REENTRANCE — convivência de pré-marcação com SciELO Markup

> Cenário: o `DocFormatter` já injetou tags em colchetes no `.docx`
> (fase 2). O operador abre esse `.docx` no Word com `markup.prg`
> carregado e clica em botões da barra. **O que acontece com as tags
> que já estão lá?**

## TL;DR

| Operação do operador | Comportamento sobre tags pré-marcadas |
|---|---|
| Selecionar texto **dentro** de uma tag pré-marcada e clicar a **mesma tag** | ⚠️ **Insere uma SEGUNDA tag** ao redor da seleção. Não há detecção idempotente. Resultado: `[author][author]João[/author][/author]`. |
| Selecionar texto pré-marcado e clicar uma tag **diferente** | Aceita se a hierarquia (`tree.txt`) permite e os colchetes estão balanceados (`isWellFormed`). |
| Selecionar texto que tem colchete **não balanceado** (ex.: pegou `João[/author], Maria` pegando só o `[/author]`) | ❌ **`VerifyHierarchy = False`** → `MsgBox msgInserting` → operação cancelada. |
| Clicar em botão **de auto-marcação** (`[doc]`, `*authors`, `*kwdgrp`, `aff`/`normaff`, `xmlbody`, `xmlabstr`, `figgrp`, `tabwrap`, `equation`, `*list`, `*deflist`, `versegrp`, `*fn`, `*fngrp`, `*page-fn`, `ack`, `sec`) sobre uma região que tu já pré-marcou | 💥 **Destrutivo**: as macros `mark_template`, `mark_template_author`, `mark_template_aff`, `markup_keywords`, `mark_xmlbody`, `mark_authors`, `markup_aff_label`, `markup_sup_as` etc. **não checam tags existentes**. Vão re-aplicar e duplicar. |

**Conclusão prática:** pré-marcação é segura **só** se o operador
**não clica em auto-mark sobre regiões já marcadas**. Treine a equipe.

## Detalhes do mecanismo de proteção

### `VerifyHierarchy` (`_analysis/markup_macros.txt:1492`)

Toda inserção manual passa por aqui antes de chegar no `InsertTag`.
Sequência:

```vba
1. has_square_brackets()  ' (markup_macros.txt:1622)
   ' Procura "[" e "]" na seleção. Se nenhum, ok1 = True.
2. Se tem brackets: chama isWellFormed(seleção)
   ' (markup_macros.txt:2940)
   ' Faz parsing simples de pilha: [tag] empilha, [/tag] desempilha
   ' e exige bater com o topo. Retorna True se balanceado.
3. Se ok1 = False: VerifyHierarchy = False (rejeita).
4. Se ok1 = True: verify_father(currentBar, father)
   ' Onde "father" é a tag detectada IMEDIATAMENTE antes da seleção
   ' (via verify_hier_previous_element em markup_macros.txt:1668).
   ' tree.txt define quem pode ser pai de quem.
```

**Implicações para fase 2:**
- Se você pré-marca de modo bem-formado, o operador pode inserir
  **outras** tags ao redor sem problema.
- Se você quebrar a estrutura (deixar `[author]` aberto sem fechar),
  qualquer inserção subsequente do operador na mesma região será
  bloqueada. Isso é uma **boa salvaguarda**.

### `isWellFormed` (`_analysis/markup_macros.txt:2940`)

Algoritmo simplificado:

```
text = seleção.texto
pilha = ""
while existir [...]:
    se [/<tag>] na pilha-topo: pop
    se [<tag> ...]:            push
    se desbalanceado:          retorna False
retorna True se pilha vazia, False se sobrou tag
```

Nota: ele só olha o **balanço**, não valida hierarquia DTD nem
duplicação. Logo, `[author][author]X[/author][/author]` passa por
`isWellFormed` (está balanceado) — apenas o `parser.exe` no fim do
fluxo vai pegar o erro de DTD.

### Auto-mark macros que **não verificam** existência prévia

Estas macros foram escritas assumindo "documento limpo". Aplicam
`tag_text_range` cegamente:

| Macro | Local | Tags emitidas | Trigger no UI |
|---|---|---|---|
| `mark_template` | `markup_macros.txt:11157` | `[doi]`, `[toctitle]`, `[doctitle …]` | botão `[doc]` (root) |
| `mark_template_author` | `:11234` | `[author role="nd"]`, `[fname]`, `[surname]`, `[xref ref-type="aff"]` | chamado por `mark_template` |
| `mark_template_aff` | `:11345` | `[normaff id="aff…"]`, `[label]` | chamado por `mark_template` |
| `markup_keywords` | `:38` | `[sectitle]`, `[kwd]` | botão `*kwdgrp` |
| `mark_authors` | `:4799` | `[pauthor]`, `[et-al]` | botão `*authors` |
| `markup_author` | `:5083` | `[author role="nd"]`, `[fname]`, `[surname]` | botões `*author`/`*oauthor`/`*pauthor` |
| `markup_aff_label` → `markup_sup_as("label",…)` | `:8099`, `:10625` | `[label][sup]…[/sup][/label]` | botão `aff`/`normaff`/`afftrans` |
| `mark_xmlbody`, `mark_sec`, `mark_xmlabstr`, `markup_ack` | `:8456`, `:8518`, `:8364`, `:8117` | `[sec]`, `[sectitle]`, `[p]`, `[xmlabstr]` | botões `xmlbody`/`sec`/`xmlabstr`/`ack` |
| `markup_label_and_caption` | `:8996` | `[label]`, `[caption]` | chamado por `figgrp`/`tabwrap` |
| `mark_textref` | `:4621` | `[text-ref]` | botão `ref` |
| `add_citation_tag` | `:4517` | `[vcitat]`/`[pcitat]`/`[acitat]`/etc. | botão `refs` |
| `markup_all_the_footnotes` | `:5982` | `[fn]` por footnote do Word | botão `*page-fn` |

**Nenhuma** delas tem branch tipo `If InStr(text, "[<tag>]") > 0 Then
Exit Sub`. Todas confiam que o doc está intocado.

### Caso especial: `markup_sup_as` (afeta diretamente sua fase-2 de aff/xref)

`markup_sup_as` (`_analysis/markup_macros.txt:10625`) é o que marca
`[label]`/`[xref]` para chamadas em superescrito. Ele:

1. Faz `Find.Execute` com `Font.Superscript = True` para localizar.
2. Quando acha, chama `tag_text_range(sel, "[label][sup]", "[/sup][/label]", …)`.
3. **`selection.Font.Superscript = False`** — limpa o flag depois.

**Implicação prática para fase 2:**
- Se seu `DocFormatter` já marcou `[label]1[/label]` e **manteve** o
  `1` em superescrito → `markup_sup_as` vai achar de novo o `1` e
  produzir `[label][sup][label]1[/label][/sup][/label]`. **Quebra.**
- **Solução**: se você pré-marca um `[label]` ou `[xref]`, **remova
  a formatação superescrito** do caractere envolvido. Aí
  `markup_sup_as` ignora.

## Recomendações concretas para o `DocFormatter`

### Convivência segura (operador disciplinado)

1. Documente para a equipe: **"se o cabeçalho já tem `[author]`, não
   clique em `*authors`/`*author`/botões automáticos do front-matter"**.
2. Pré-marque tudo o que conseguir, deixando o operador só com:
   - `[doc]` (root, abre o formulário com SPS/ISSN/dateiso/…)
   - `[xmlbody]` (corpo do artigo)
   - `[refs]` (já automatizado externamente, segundo você)

### Convivência defensiva (operador imprevisível)

Se você quer que mesmo um operador clique-feliz não estrague, opções:

- **Não pré-marque `[doctitle]`/`[toctitle]`/`[doi]`** — esses são
  destruídos por `mark_template` quando o operador clica `[doc]`.
  Deixe que `mark_template` os crie e a fase 2 só **valida/corrige**
  depois.
- **Pré-marque preferencialmente tags do `%i.float;`** (`[corresp]`,
  `[xref]`, `[graphic]`, `[supplmat]`, `[ign]`) — essas raramente
  são alvo de auto-mark.
- **Limpe formatação Word** das chamadas de aff/corresp (tirar
  superescrito) para neutralizar `markup_sup_as`.
- **Use uma marca-d'água invisível** (ex.: estilo de caractere
  `MarkedByDocFormatter`) nos runs já marcados para a equipe SciELO
  identificar visualmente — o `markup.prg` ignora estilos
  desconhecidos do Word.

### Não há comando "delete-all-tags"

O template tem `delete_all_tags` (`markup_macros.txt:759`) e
`DeleteTag` (`:786`), mas estes são acionados pelo botão "Delete tag"
da barra Markup, requerem seleção pontual e não são usados em
auto-mark. Logo: **não há rota acidental** que apague suas
pré-marcações em massa — a destruição vem só de re-aplicação.

## Resumo decisório

> **Pre-mark + Operator = OK** se você:
> 1. **Não pré-marca** `[doctitle]`, `[toctitle]`, `[doi]`,
>    `[author role=…]`, `[fname]`, `[surname]`, `[normaff]`, `[label]`
>    superscrito, `[kwd]`, `[sectitle]`-de-resumo, `[p]`-do-corpo —
>    são alvos de `mark_template`/`mark_template_author`/
>    `mark_template_aff`/`markup_keywords`/`mark_xmlbody`.
> 2. **Pré-marca** com segurança: `[corresp]`, `[email]`, `[hist]`,
>    `[received]`, `[accepted]`, `[revised]`, `[ctrbid]` (ORCID),
>    `[abstract language="…"]`, `[kwdgrp language="…"]` (mas não os
>    `[kwd]` filhos se a equipe vai clicar `*kwdgrp`).
> 3. **Treina** a equipe: clicar só em root (`[doc]`) e nos botões de
>    consumo (`Save`, `Parser`, `Generate XML`); evitar
>    auto-mark sobre seções já marcadas.
> 4. **Limpa** formatação Word (`Font.Superscript=False`,
>    `Font.Italic=False`) onde já injetou tags inline-related, para
>    desativar `markup_sup_as` e `keepStyle`.

> **Pre-mark + Operator = QUEBRA** se você:
> - Pré-marcar `[author]`/`[fname]`/`[surname]` e o operador clicar
>   `*author`/`*authors` em cima.
> - Pré-marcar `[normaff]` e o operador clicar `aff`/`normaff` na linha
>   da afiliação.
> - Pré-marcar `[label]X[/label]` mantendo `X` superescrito.
> - Pré-marcar `[doctitle]` e o operador clicar `[doc]` (root).
