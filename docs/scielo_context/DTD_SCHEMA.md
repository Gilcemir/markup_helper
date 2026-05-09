# DTD_SCHEMA — schema autoritativo das tags SciELO Markup

> Fonte: `src/scielo/bin/SGMLPars/*.dtd`. Esses são os DTDs SGML que o
> `parser.exe`/`SGMLPars.dll` usa para validar o documento marcado
> antes de produzir o XML final. Se a marcação inserida no `.docx` viola
> o DTD, o `convert.exe` falha e o operador vê erro de validação.
>
> Existem várias versões; o pipeline atual da SciELO usa a 4.0:
>
> - `art4_0.dtd` (Article DTD raiz com `[article]`)
> - `text4_0.dtd` (Text DTD raiz com `[text]`)
> - `doc4_0.dtd` (Doc DTD raiz com `[doc]` — usado para artigos novos
>   formato XML)
> - `common4_0.dtd` (importado pelas três acima — tem `aff`, `corresp`,
>   `author`, `xref`, etc.)
> - `citation4_0.dtd` (importado também — tem `vcitat`, `pcitat`,
>   `acitat`, etc.)
>
> **Para o seu projeto, foque em `[doc]` (root para artigos novos) +
> `common4_0.dtd` (afiliações, autores, xref).**

> Notação SGML usada nestes DTDs:
> - `<!ELEMENT t - - (X, Y, Z)>` — sequência **obrigatória nesta ordem**
> - `(X | Y)` — escolha exclusiva
> - `(X, Y)` — sequência ordenada
> - `(X & Y)` — todas as opções, **ordem livre**
> - `(X)*` — zero ou mais
> - `(X)+` — um ou mais
> - `(X)?` — opcional
> - `(#PCDATA)` — só texto
> - `CDATA` em `<!ATTLIST>` — string livre
> - `ID` — identificador único no documento
> - `IDREF`/`IDREFS` — referência(s) a outros IDs
> - `#REQUIRED` — atributo obrigatório
> - `#IMPLIED` — atributo opcional
> - `+(%i.float;)` — pode conter qualquer elemento "flutuante" (xref,
>   aff, corresp, ign, tabwrap, figgrp, equation, cltrial, uri, sciname,
>   quote, element, graphic, supplmat, related, product, sup, cpright,
>   licinfo)

---

## Root: `[doc]` (artigo no formato novo)

```dtd
<!ELEMENT doc - - (
    toctitle | doi | related |
    doctitle |
    (((author+, onbehalf?) | corpauth)+, normaff*) |
    ((abstract | xmlabstr), kwdgrp) |
    confgrp | funding | hist |
    xmlbody |
    ack | glossary | deflist | app |
    refs |
    fngrp | cc |
    docresp | subdoc | appgrp
)+
+(%i.float;)>
```

Cada filho aparece **0+ vezes** em qualquer ordem. Para sua fase 2,
o que importa é gerar conteúdo válido para os filhos individualmente —
o `[doc]` em si é marcado pelo operador.

**Atributos de `[doc]` (do operador, não da fase 2):** `sps`, `acron`,
`jtitle`, `issn`, `pubname`, `license`, `dateiso`, `fpage`, `lpage`,
`doctopic`, `language` são **#REQUIRED**.

---

## Tags da fase 2

### `[hist]` — histórico

```dtd
<!ELEMENT hist - - (received, revised*, accepted?) >

<!ELEMENT received - - (#PCDATA) >
<!ATTLIST received  dateiso  CDATA #REQUIRED >

<!ELEMENT revised  - - (#PCDATA) >
<!ATTLIST revised   dateiso  CDATA #REQUIRED >

<!ELEMENT accepted - - (#PCDATA) >
<!ATTLIST accepted  dateiso  CDATA #REQUIRED >
```

**Constraints duros:**
- `[hist]` exige **exatamente um** `[received]`, **na primeira posição**.
- `[revised]` é opcional, pode aparecer 0+ vezes, **antes** de `[accepted]`.
- `[accepted]` é opcional, no máximo 1, **na última posição**.
- Todos os filhos exigem `dateiso="YYYYMMDD"` (CDATA mas convenção é 8 dígitos).

**Regra para LLM gerar código:** "se detectou só `Recebido em DD/MM/AAAA`,
emita `[hist][received dateiso=\"YYYYMMDD\"]…[/received][/hist]` — não
emita `[accepted]` vazio."

### `[doctitle]`

```dtd
<!ELEMENT doctitle - - (subtitle | (#PCDATA))  >
<!ATTLIST doctitle  language CDATA #IMPLIED >
```

`language` é **opcional**. Conteúdo é texto plano OU um `[subtitle]`.
O `[subtitle]` aqui não está claro como conviveria com PCDATA — na prática
o formato comum é `[doctitle language="pt"]Título[/doctitle]` seguido por
`[subtitle]Sub[/subtitle]` separado.

Valores observados de `language`: `pt`, `en`, `es`, `fr`, `de` (de
`detect_language` em `markup_macros.txt:11203`). Use ISO-639-1 dois
caracteres.

### `[abstract]`

```dtd
<!ELEMENT abstract - - (#PCDATA) >
<!ATTLIST abstract  language CDATA #REQUIRED >
```

`language` é **#REQUIRED**. Conteúdo é PCDATA (texto livre, sem filhos
estruturados — diferente do `[xmlabstr]` que tem `[sec]`/`[p]`).

### `[xmlabstr]` (alternativa estruturada)

```dtd
<!ELEMENT xmlabstr - - (sectitle?, (sec | p)+) >
<!ATTLIST xmlabstr  language CDATA #REQUIRED >
```

Use quando o resumo tem múltiplos parágrafos ou subseções.

### `[kwdgrp]` + `[kwd]`

```dtd
<!ELEMENT kwdgrp   - - (sectitle?, (kwd)+)  >
<!ATTLIST kwdgrp   language CDATA #REQUIRED >

<!ELEMENT kwd  - - (#PCDATA) >
```

- `[kwdgrp]` exige `language` (#REQUIRED) e **pelo menos uma** `[kwd]`.
- `[sectitle]` opcional (ex.: `[sectitle]Palavras-chave[/sectitle]`).
- O **idioma é por grupo** — gere um `[kwdgrp]` por idioma.

### `[corresp]`

```dtd
<!ELEMENT corresp - - (label? & email+ & (#PCDATA)?) >
<!ATTLIST corresp  id ID #REQUIRED >
```

- `id` é **#REQUIRED** e tipo `ID` (único no documento; convenção SciELO:
  `c1`, `c2`, …).
- Exige **pelo menos um** `[email]`.
- Pode ter `[label]` e PCDATA, ordem livre (`&`).

**Para vincular o autor correspondente:** o `[author]` ganha o atributo
`rid="c1"` (IDREFS) ou `corresp="c1"` (CDATA). Veja seção `[author]`
abaixo.

### `[aff]` (afiliação simples)

```dtd
<!ELEMENT aff - - (label? & role? & city? & state? & country? &
                   zipcode? & email* & (#PCDATA)?) >
<!ATTLIST aff
    id       ID    #REQUIRED
    orgname  CDATA #REQUIRED
    orgdiv1  CDATA #IMPLIED
    orgdiv2  CDATA #IMPLIED
    orgdiv3  CDATA #IMPLIED >
```

⚠️ **`orgname` é ATRIBUTO em `[aff]`, não filho.** Para a fase 2 isso
importa muito: você não escreve `[aff][orgname]USP[/orgname]…[/aff]`.
Você escreve:

```
[aff id="aff1" orgname="USP" orgdiv1="Faculdade de Medicina"]
   [label]1[/label]
   [city]São Paulo[/city]
   [state]SP[/state]
   [country]Brasil[/country]
   [email]autor@usp.br[/email]
   PCDATA livre opcional
[/aff]
```

- `id` (#REQUIRED ID) — convenção `aff1`, `aff2`, …
- `orgname` (#REQUIRED CDATA atributo)
- `orgdiv1`/`orgdiv2`/`orgdiv3` opcionais
- Filhos: `label?`, `role?`, `city?`, `state?`, `country?`, `zipcode?`,
  `email*` (0+), e PCDATA opcional, **todos em ordem livre** (`&`).

### `[normaff]` (afiliação normalizada)

```dtd
<!ELEMENT normaff - - (label? & role? & orgname & orgdiv1? & orgdiv2? &
                       city? & state? & country? & zipcode? & email* &
                       (#PCDATA)?) >
<!ATTLIST normaff
    id       ID    #REQUIRED
    ncountry CDATA #REQUIRED
    norgname CDATA #REQUIRED
    icountry CDATA #REQUIRED >
```

Diferença para `[aff]`:
- `orgname`, `orgdiv1`, `orgdiv2` são **filhos**, não atributos.
- Atributos extras `ncountry` (nome canônico do país), `norgname`
  (nome canônico da org), `icountry` (código ISO do país) são todos
  **#REQUIRED**.
- É a versão "validada" da afiliação após cruzar com base de
  instituições.

**Para fase 2, prefira `[aff]` (mais simples).** O `[normaff]` é
gerado pela equipe de validação depois.

### `[author]`

```dtd
<!ELEMENT author - - ((%m.name;) | previous ) >
<!ATTLIST author
    role     NAMES  #REQUIRED
    rid      IDREFS #IMPLIED
    corresp  CDATA  #IMPLIED
    deceased CDATA  #IMPLIED
    eqcontr  CDATA  #IMPLIED >
```

Onde `m.name = (fname? & surname & ctrbid*)`.

- `role` é **#REQUIRED** (`NAMES` = lista de nomes; valores observados:
  `nd`, `ed`, `tr`, `org`).
- `rid` (#IMPLIED IDREFS): lista de IDs separados por espaço apontando
  para `[aff]` e/ou `[corresp]`. Ex.: `rid="aff1 c1"`.
- Conteúdo: `[fname]?` + `[surname]` (obrigatório) + `[ctrbid]*` em
  ordem livre. **Não há `prefix`/`suffix` no DTD 4.0** (apesar de
  estarem em `tree.txt`).

**Para vincular autor → aff/corresp via `rid` (recomendado, sem
`[xref]`):**
```
[author role="nd" rid="aff1 c1"]
   [fname]João[/fname]
   [surname]Silva[/surname]
   [ctrbid ctrbidtp="orcid"]0000-0001-2345-6789[/ctrbid]
[/author]
```

**Ou via `[xref]` em vez de `rid` (mais visível, aceito também):**
```
[author role="nd"]
   [fname]João[/fname]
   [surname]Silva[/surname]
[/author]
[xref ref-type="aff" rid="aff1"]1[/xref]
```

Ambos validam contra o DTD; `markup_macros.txt:11286` usa o segundo
estilo (auto-marca via `mark_template_author`).

### `[ctrbid]` (ORCID e similares)

```dtd
<!ELEMENT ctrbid - - (#PCDATA) >
<!ATTLIST ctrbid  ctrbidtp NAMES #REQUIRED >
```

- `ctrbidtp` é **#REQUIRED**. Valores observados: `orcid`, `lattes`.
- Conteúdo é o ID puro (sem URL completa), ex.:
  `[ctrbid ctrbidtp="orcid"]0000-0001-2345-6789[/ctrbid]`.

### `[fname]`, `[surname]`, `[onbehalf]`

```dtd
<!ELEMENT fname    - - (#PCDATA) >
<!ELEMENT surname  - - (#PCDATA) >
<!ELEMENT onbehalf - - (#PCDATA) >
```

Sem atributos. PCDATA puro.

### `[corpauth]` (autor corporativo)

```dtd
<!ELEMENT corpauth - - ((%m.org;) | previous ) >
```

Onde `m.org = (orgname? & orgdiv?)`.

```
[corpauth]
   [orgname]Ministério da Saúde[/orgname]
   [orgdiv]Departamento X[/orgdiv]
[/corpauth]
```

### `[xref]`

```dtd
<!ELEMENT xref - - (#PCDATA | graphic) >
<!ATTLIST xref
    ref-type CDATA  #IMPLIED
    rid      IDREFS #REQUIRED
    label    CDATA  #IMPLIED >
```

- `rid` é **#REQUIRED** — IDREFS (uma ou mais IDs separadas por espaço).
- `ref-type` é #IMPLIED. Valores que vimos no VBA: `aff`, `bibr`, `corresp`.
- Conteúdo: PCDATA (geralmente o número/marcador) ou um `[graphic]`.

```
[xref ref-type="aff" rid="aff1"]1[/xref]
[xref ref-type="corresp" rid="c1"]*[/xref]
[xref ref-type="bibr" rid="r5"]5[/xref]
```

### `[email]`, `[city]`, `[state]`, `[country]`, `[zipcode]`

```dtd
<!ELEMENT email   - - (#PCDATA)>
<!ELEMENT city    - - (#PCDATA)>
<!ELEMENT state   - - (#PCDATA)>
<!ELEMENT country - - (#PCDATA)>
<!ELEMENT zipcode - - (#PCDATA)>
```

Todos PCDATA puro, sem atributos. Não há valores fechados — o DTD
aceita qualquer string. Padrões SciELO observados:
- `[country]` — nome do país por extenso em pt/es/en. A normalização
  para ISO-3166 é feita na geração de `[normaff icountry="…"]`.
- `[state]` — sigla 2 letras (`SP`, `RJ`).
- `[city]` — nome livre.

### `[doi]`

```dtd
<!ELEMENT doi - - (#PCDATA) >
```

Sem atributos. Conteúdo é o DOI puro (sem `https://doi.org/` na frente):
```
[doi]10.1590/abc123[/doi]
```

### `[label]`, `[caption]`

```dtd
<!ELEMENT label   - - (#PCDATA | sup) >
<!ELEMENT caption - - (#PCDATA) >
```

`[label]` aceita PCDATA + `[sup]` inline.

### `[date]`

```dtd
<!ELEMENT date - - (#PCDATA) >
<!ATTLIST date
    dateiso  CDATA #IMPLIED
    specyear CDATA #IMPLIED >
```

`dateiso` é opcional aqui (mas sempre fornecido na prática). Formato:
`YYYYMMDD` (use `0000` se faltar mês/dia, ex.: `20230000` = "ano 2023,
mês desconhecido").

---

## Tags inline (para preservar)

```dtd
<!ELEMENT sup - - (#PCDATA)>
```

Os DTDs 4.0 oficialmente só listam `[sup]` como inline-element no
`%i.float;`. As tags `[bold]`, `[italic]`, `[sub]` aparecem no fluxo
SciELO mas **não estão no DTD 4.0** — provavelmente são tratadas como
entities ou substituídas no pipeline. Para fase 2, **não emita
`[bold]`/`[italic]` no .docx** — deixe o `keepStyle` do markup.prg
fazer isso (lê `Font.italic`/`Font.bold` do Word). Você só precisa
garantir que o `.docx` tem **formatação de fonte real** (não um asterisco
em volta da palavra).

---

## Tags que NÃO ESTÃO na sua lista de fase 2 (para você não confundir)

- `[refs]`/`[ref]`/`[vcitat]`/`[pcitat]`/etc. — referências bibliográficas.
  Estrutura complexa, ver `citation4_0.dtd`. Você disse que isso já tem
  automação externa.
- `[tabwrap]`, `[figgrp]`, `[graphic]`, `[equation]` — exigem
  ancoragem Word (run/anchor de imagem/tabela). Deixe para o operador.
- `[xmlbody]`, `[sec]`, `[subsec]`, `[p]` — corpo do artigo. O
  `mark_xmlbody` cuida via heading-styles.
- `[fngrp]`/`[fn]` — notas de rodapé. Use Word footnotes; o macro
  `markup_all_the_footnotes` converte.

---

## Atributos com valores controlados (que o DTD permite mas você deve
respeitar)

Embora o DTD diga `CDATA` (string livre), na prática a SciELO espera:

| Tag/Atributo | Valores aceitos pela SciELO |
|---|---|
| `[author role]` | `nd` (autor), `ed` (editor), `tr` (tradutor), `org` (organizador) |
| `[doctitle language]`, `[abstract language]`, `[kwdgrp language]`, `[title language]`, `[subtitle language]` | `pt`, `en`, `es`, `fr`, `de` (ISO-639-1) |
| `[author corresp]` | a ID do `[corresp]` correspondente, ex.: `c1` |
| `[xref ref-type]` | `aff`, `corresp`, `bibr` (citação), `fn`, `table`, `figure` |
| `[ctrbid ctrbidtp]` | `orcid`, `lattes`, `researcherid`, `scopusid` |
| `[ref reftype]` | `journal`, `book`, `report`, `patent`, `confproc`, `thesis` |
| `dateiso` (em `[received]`/`[accepted]`/`[revised]`/`[date]`/`[cited]`) | `YYYYMMDD` zerado: `20230615`, `20230600`, `20230000` |

---

## Convenção de IDs

Para gerar IDs únicas:

| Tag | Convenção da SciELO | Exemplo |
|---|---|---|
| `[aff]`, `[normaff]` | `aff` + N | `aff1`, `aff2` |
| `[corresp]` | `c` + N | `c1`, `c2` |
| `[ref]` | `r` + N | `r1`, `r5` |
| `[fn]` | `fn` + N | `fn1` |
| `[fntable]` | `fntable` + N | `fntable1` |

Numeração começa em 1, sequencial pela ordem de aparição no documento.

---

## Para validar localmente

Se você quiser testar uma marcação contra o DTD sem rodar o
`parser.exe` Windows, use `xmllint` ou `onsgmls`:

```bash
# Salve seu .docx como texto plano com as colchetes (sem o XML envelope)
# Adicione manualmente o doctype no início:
echo '<!DOCTYPE doc SYSTEM "doc4_0.dtd">' > teste.sgml
cat seu_arquivo_marcado.txt >> teste.sgml

# Valide com onsgmls (do pacote OpenSP):
onsgmls -s -wno-explicit-sgml-decl \
    src/scielo/bin/SGMLPars/doc4_0.dtd \
    teste.sgml
```

(Não há regra de Make pronta neste repo — você teria que recriar o
DECL `art.dcl` no diretório de trabalho.)
