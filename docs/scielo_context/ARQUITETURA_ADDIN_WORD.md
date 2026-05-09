# Arquitetura do "add-in" SciELO Markup para Word

> Análise do código presente em `src/scielo/bin/MarkupExe/` e
> `src/scielo/bin/markup/` deste repositório arquivado. Os entregáveis
> auxiliares (`TAG_INDEX.md`, `UI_MENUS.md`, dump de macros, índices) ficam
> em `_analysis/`.

## 1. Visão geral em uma página

O "add-in" do SciELO **não é um .dotm/.docm distribuído pelo Word como
add-in**. É um conjunto de três peças separadas que se conectam em
runtime:

1. **Launcher VB6** — `Markup.exe` (compilado a partir de
   `src/scielo/bin/MarkupExe/proj/`). Faz `ShellExecute "winword.exe /l
   <dir>\markup.prg"`. Ou seja: pede ao Word para carregar
   `markup.prg` como **global template** (`/l`).
2. **Global template Word** — `src/scielo/bin/markup/markup.prg` (e
   `tables.prg`) é um arquivo OLE/Composite Document (assinatura: "Microsoft
   Word, Template: markup.prg") com o nome trocado para `.prg`. Dentro
   dele há **macros VBA** (não VB6, não WordBasic) — ~700 procedimentos
   distribuídos por módulos `MainModule.bas`, `clsMarkup.cls`, `clsConfig.cls`,
   `clsListBar.cls`, `clsMarkupXMLBody.cls`, `clsAttrList.cls`, etc. Toda a
   inserção de tag SGML/XML em colchetes acontece aqui.
3. **Recursos não-VBA** — arquivos em `src/scielo/bin/markup/app_core/` e
   `src/scielo/bin/markup/app_modules/`:
   - `default.mds`, `<lang>_conf.mds`, `<lang>_bars.mds`, `<lang>_temp.mds`,
     `<lang>_attb.mds`, `link.mds`, `tree.txt`, `tree_<lang>.txt` —
     arquivos de configuração *texto* lidos pelas macros.
   - `automata.dll` — DLL nativa Windows que executa **autômatos finitos
     determinísticos** (FSM). É chamada via `Declare Function
     AutomataParser Lib "automata.dll"` (`markup_macros.txt:12091`). Os
     autômatos são definidos em `app_modules/automata1/*.amd`
     (descrições FSM, **não regex**).
   - `app_modules/automata2/*.xsl` + `MarcaAutomaticamente.bat` — caminho
     alternativo (XSLT) usado para marcação Vancouver.
   - `app_modules/refIdentifier/*.jar` (Java + Lucene + zeus + dom4j) —
     serviço externo "Auto-Markup Web Services" (botão `buttonAutomaticWS`).
     `PubMed.py` é apenas um watchdog Python que polla o `<output>.xml`
     gerado pelo job Java.
   - `bin/SGMLPars/parser.exe` — parser SGML (caminho registrado em
     `pt_conf.mds:parser`). Não está neste repositório, é externo.
   - Scripts Python no caller (`bin/markup_xml/...`, fora do escopo aqui):
     transformam o `.txt` SGML salvo em XML JATS final.

```
┌────────────────────┐  ShellExecute   ┌──────────────────────────────┐
│  Markup.exe (VB6)  │ ───/l markup.prg│  Microsoft Word + markup.prg │
│ Marc.bas:Sub Main  │ ──────────────► │  (global template c/ macros) │
│ lê start.mds, p.mds│                 │                              │
│ acha winword.exe   │                 │  Document_New → BuildMarkupBar│
└────────────────────┘                 │  CommandBars dinâmicas (UI)  │
                                       │                              │
                                       │  ┌──────────────────────┐    │
                                       │  │ usuário clica botão  │    │
                                       │  │   caption = "<tag>"  │    │
                                       │  └──────────┬───────────┘    │
                                       │             ▼                │
                                       │  MainModule.Main             │
                                       │     ↓ Select Case ActionCtl  │
                                       │     ↓ Else → SubElse         │
                                       │     ↓                        │
                                       │  clsMarkup.InsertTag(label)  │
                                       │     ├── Select Case label    │
                                       │     │   ├── "*authors"  → mark_authors        │
                                       │     │   ├── "fname-surname" → markup_fname_and_surname │
                                       │     │   ├── "*kwdgrp"  → markup_keywords      │
                                       │     │   ├── "aff"      → markup_aff_label     │
                                       │     │   ├── …          → …                    │
                                       │     │   └── (default)                         │
                                       │     │                                         │
                                       │     ├── stag = buildStartTag(tag,conf,…)      │
                                       │     │   = "[" & tag & attrs & "]"             │
                                       │     ├── ftag = BuildFinishTag(tag,conf)       │
                                       │     │   = "[/" & tag & "]"                    │
                                       │     └── tag_text_range(doc, stag, ftag, color)│
                                       │             │                                 │
                                       │             ▼                                 │
                                       │   range.InsertBefore "[tag …]"                │
                                       │   range.InsertAfter  "[/tag]"                 │
                                       │   Font.Name="Arial", Font.Color=cor           │
                                       │                              │
                                       └──────────────────────────────┘
                                                       │
                                       botões "Auto"   │ buttonParser
                                                       ▼
                  ┌──────────────────────┐    ┌─────────────────┐
                  │ AutomataParser DLL   │    │ parser.exe SGML │
                  │ (lê *.amd FSM)       │    │ valida e gera   │
                  └──────────────────────┘    │ XML/HTML        │
                                              └─────────────────┘
```

## 2. Launcher VB6 — `Markup.exe`

Fontes em `src/scielo/bin/MarkupExe/proj/`:

| Arquivo | Papel |
|---|---|
| `Marcacao.vbp` | Projeto VB6 (`Type=Exe`, `ExeName32="Markup.exe"`). Startup `Sub Main`. Compila para um EXE de 24 KB (`src/scielo/bin/markup/Markup.exe`). |
| `Marc.bas` | Único módulo. `Sub Main()` lê `App.path & "\start.mds"` (caminho do `winword.exe`) e `App.path & "\p.mds"` (parâmetros do `ShellExecute`). Se o `winword.exe` for encontrado (`Dir(path) = f`), chama `callWord` que faz `ShellExecute(0, "Open", winword.exe, " /l " & App.path & "\markup.prg", …, SW_SHOWNORMAL)`. Caso contrário mostra `DepePath.frm` para o usuário corrigir o caminho. |
| `DepePath.frm` / `DepePath2010.frm` | Form simples ("Finding WORD") com `TextBox` + OK/Cancel. Salva o caminho corrigido em `start.mds`. |
| `clsConfig.cls` | (Decorativo aqui — o launcher não usa o STAGO/ETAGO; apenas o template VBA usa.) |

**Em uma frase:** o `Markup.exe` é um *bootstrapper*: localiza o `winword.exe`,
lança o Word com `/l <…>\markup.prg` (que faz o Word carregar o template
global), e termina. Toda a marcação acontece de fato dentro do Word.

## 3. O global template `markup.prg`

- Formato: OLE2 (`file` confirma "Composite Document File V2 Document").
  Cabeçalho diz `Title: Markup DTD-Scielo 2.0`, `Template: markup.prg`,
  `Last Saved Time/Date: Fri May 29 18:18:00 2020`.
- Foi criado salvando um `.dot` do Word como `markup.prg`. Não está
  versionado como texto — dá pra abri-lo no Word (Alt+F11) que o IDE VBA
  mostra os módulos.
- `tables.prg` é praticamente um clone de `markup.prg` para uma DTD
  ligeiramente diferente (presença de `defitem`/`fngrp` e ausência de
  `suffix`/`fn` — ver diff em `_analysis/markup_macros.txt` vs
  `_analysis/tables_macros.txt`).

### 3.1 Como inserimos cada tag

Detalhe completo em `_analysis/TAG_INDEX.md`. Resumo do mecanismo:

1. Os arquivos `app_core/<lang>_bars.mds` definem **todas as tags**
   da DTD com formato `tag;tooltip;wdColor;has_attr;has_children;link`.
2. Em `Document_New` (`markup_macros.txt:9`) o template não faz nada;
   o usuário clica no popup "StartUp" → item `MainModule.Main` →
   primeira execução chama `SubStartMarkup` (`markup_macros.txt:12706+`),
   que carrega `clsConfig.LoadPublicValues` (`markup_macros.txt:7791`) e
   `clsListBar.CreateBars` (`markup_macros.txt:7259`).
3. `CreateBars` percorre `tree.get_tree` (de `tree.txt`) e cria um
   `CommandBars.add(name:="mkpbar"+nome)` por nó-pai, depois adiciona
   um `CommandBarButton` por filho com `caption = child.name` (= nome da
   tag) e `OnAction = MainModule.Main`.
4. Ao clicar:
   - `Sub Main()` (`markup_macros.txt:12337`) lê
     `CommandBars.ActionControl`. Se o tooltip casa com botões fixos
     (Stop/Save/Parser/Validate/etc.) trata aqui mesmo. Caso contrário
     cai em `SubElse`.
   - `Sub SubElse(...)` (`markup_macros.txt:13307`) é o handler genérico
     de tag. Pega `button_label = button.caption`, valida hierarquia
     (`VerifyHierarchy`), pede atributos via `frmAttribute_show` se
     `link.mds` tem entrada para a tag, e chama
     `m.InsertTag(button_label, lBar.color(button_label), conf, …,
     lAttr, lk, i)` (linha 13403).
5. `Public Function InsertTag(...)` (`markup_macros.txt:371`) é o
   **roteador**. Faz dois trabalhos:
   - **Especialização** via `Select Case tag_button_label` (linhas
     438–650). Casos `*authors`, `*author`, `fname-surname`,
     `*kwdgrp`, `aff`, `refs`, `tabwrap`, `figgrp`, `equation`, `sec`,
     `*list`, `*deflist`, `versegrp`, `*fn`, `*fngrp`, `xmlbody`,
     `xmlabstr`, `ack`, `*page-fn` chamam helpers em `clsMarkup`
     ou `clsMarkupXMLBody` que emitem **vários** pares
     `[tag…]/[…/tag]` em sequência.
   - **Caminho default** (`If DO_CONTENT_TAG Then`, linhas 653–679):
     ```vba
     stag = buildStartTag(tag, conf, attl, linkl, inter)   ' "[tag attr=\"v\"]"
     ftag = BuildFinishTag(tag, conf)                      ' "[/tag]"
     Call tag_text_range(doc, stag, ftag, listbar.color(tag), fix_end)
     ```
6. `Sub tag_text_range(text_range, open_tag, close_tag, color, …)`
   (`markup_macros.txt:3699`) é onde o texto é **fisicamente** inserido
   no documento Word:
   ```vba
   before.InsertBefore open_tag    ' "[tag …]"
   after.InsertAfter  close_tag    ' "[/tag]"
   ' aplica Font.Name="Arial", Font.Color=color (vem de listbar.color(tag))
   ```
   Antes da inserção, `fix_end` faz um trim do `range.End` para não
   pegar o caractere de parágrafo final.

### 3.2 Os delimitadores `[`, `[/`, `]`

Definidos em `clsConfig` como `Public STAGO As String`, `ETAGO`, `TAGC`
(`markup_macros.txt:7541–7546`) e carregados em
`LoadPublicValues` (`markup_macros.txt:7791`) a partir de
`app_core/<lang>_conf.mds`:

```
src/scielo/bin/markup/app_core/pt_conf.mds:
   STAGO,[
   ETAGO,[/
   TAGC,]
```

Isso é determinante para `buildStartTag` e `BuildFinishTag` (linhas 1003 e
1072 do dump).

### 3.3 Atributos das tags

`Public Function buildStartTag(tag, conf, attl, link, inter)`
(`markup_macros.txt:1003`) acrescenta atributos consultando, em ordem:

- `inter.start_tag_attributes` se `isRootTag(tag)` (formulário modal
  preenchido pelo usuário ao marcar `[doc]`/`[article]`/`[text]`);
- `link.ReturnAttr(...)` se a tag tem entrada em `app_core/link.mds`
  (mapa tag → lista-de-atributos esperados);
- `attl.ReturnAttr(...)` se houver entradas em `<lang>_attb.mds`;
- formato final: `" name=\"valor\""` por `writeAttribute`
  (`markup_macros.txt:1063`), removendo aspas duplicadas do valor.

### 3.4 Inline (italic/bold/sup/sub)

Esses **não são botões**. São aplicados em massa por
`Function mark_styles(filename)` (`markup_macros.txt:3033`) que itera
`["sup","sub","italic","bold"]` chamando
`Private Sub keepStyle(format)` (`markup_macros.txt:3116`). `keepStyle`
usa `Find.Execute` com `Font.<estilo> = True` para localizar trechos
formatados pelo Word, e chama
`tag_text_range(sel, "[<estilo>]", "[/<estilo>]", wdColorDarkTeal)`,
zerando depois `Font.<estilo> = False` para evitar dupla marcação. Isso
roda durante o `executeButtonSave`/`executeButtonParser`, não no clique
de tag.

## 4. Marcação automática de referências

Há **dois** mecanismos completamente distintos, ambos acionados pelos
botões fixos da barra "Markup":

### 4.1 `automata1` — DLL nativa lendo `.amd`

`AutomataParser` (em `automata.dll`) é declarada via DllImport em
`markup_macros.txt:12091`:
```vba
Declare PtrSafe Function AutomataParser Lib "automata.dll" _
    (ByVal filename As String, ByVal TTName As String, _
     ByVal InputString As String, ByVal OutputString As String) As Long
```

Os `.amd` em `app_modules/automata1/` **não são regex**. São descrições
declarativas de um **autômato finito** com formato:
```
<state-name>          ' nome do FSM (ex.: "vcitat")
<initial>             ' estado inicial
<final>               ' estado final
<from>;<to>;<NT|T>;<tag-emitida>;<separador>[;<negação>]
…
```

Exemplos extraídos:

- **APA** (`app_modules/automata1/apa.amd:1–13`):
  ```
  pcitat
  o1
  o3
  o1;o2;NT;pcontrib;". "
  o2;o3;NT;piserial;"."
  o1;o3;NT;pmonog;")."
  ```
  Lê: "Ao processar uma `pcitat`, se a partir do estado `o1` eu casar `". "`
  então emito `[pcontrib]` na transição `o1→o2`. De `o2` casando `"."`
  emito `[piserial]` e termino em `o3`."

- **Vancouver** (`app_modules/automata1/vancouv.amd:1–9`):
  ```
  vcitat
  c1
  c5
  c1;c2;T;no;". ";" "
  c1;c3;NT;vcontrib;". "
  c3;c5;NT;viserial;"."
  ```
  O sexto campo (`" "`) é o **terminador alternativo** que invalida a
  transição.

- **ISO-690** (`iso690.amd:1–10`) e **NBR-6023** (`nbr6023.amd:1–10`)
  têm a mesma forma com prefixos diferentes (`ico*`, `ac*`).

Existem ainda os arquivos `<lang>_tg<norma>.amd` (ex.
`pt_tgvanc.amd`, `pt_tgapa.amd`) — são **mapas tag → label
localizado** usados pelas dialogs de inserção/edição, não FSMs:
```
no;Número
vcontrib;Contribuição
viserial;Publicação Seriada
author;Autor;role,id
```

### 4.2 `automata2` — pipeline XSLT

Acionado pelo botão `buttonAutomatic3` para Vancouver com input já em
formato XML (PMC-NLM). `MarcaAutomaticamente.bat` chama
`xsltproc`/equivalente em `vancouv.xsl` (e `apa6023.xsl`, `iso.xsl`,
`other.xsl`, `refs.xsl`). Cada XSLT tem regras como:

```xml
<xsl:template match="*[@citation-type='journal']">
  [vcontrib]<xsl:apply-templates select="person-group | collab"/>
   <xsl:apply-templates select="article-title"/>
  [/vcontrib].
  [viserial] … [/viserial]
</xsl:template>
<xsl:template match="given-names">
  [fname]<xsl:apply-templates/>[/fname]
</xsl:template>
<xsl:template match="year">
  [date dateiso="<xsl:value-of select="."/>0000"]…[/date]
</xsl:template>
```

(extraído de `app_modules/automata2/vancouv.xsl`).

### 4.3 `refIdentifier` — não é Python, é Java

Apesar do diretório conter `PubMed.py`, o componente principal é Java:
`Marcador.jar` (RefBib.PubMedCentral / RefBib.JFrameMain) +
`Lucene.jar`/`lucene-core-2.3.2.jar` + `dom4j-1.6.1.jar` +
`zeus.jar` + `FOLLibrary.jar`, com base de dados local em `db/`. O
`PubMed.bat`/`PubMed.sh` invoca:
```
java -DLucene_Path=db -cp .;Marcador.jar;dom4j-1.6.1.jar;
     FOLLibrary.jar;Lucene.jar;lucene-core-2.3.2.jar;zeus.jar
     RefBib.PubMedCentral -infile:%1 -outfile:%2
```
`PubMed.py` é apenas um wrapper de timeout (espera o `</ref-list>`
aparecer no output e copia o resultado, com retry).

## 5. Como cada tag é inserida (pointer para detalhe)

Tabela completa: **`_analysis/TAG_INDEX.md`**. Aqui só o esqueleto:

- **Caminho default (>200 tags)**: clique → `Main` → `SubElse` →
  `InsertTag` → `buildStartTag`/`BuildFinishTag` → `tag_text_range`
  → `range.InsertBefore "[tag …]"` + `range.InsertAfter "[/tag]"`.
- **Especializações com macro dedicada** (`Select Case` em `InsertTag`,
  `markup_macros.txt:438–650`):
  - `*authors` → `mark_authors` (`:4799`) — emite múltiplos `[pauthor]`
    e `[et-al]`.
  - `*author`/`*oauthor`/`*pauthor` → `markup_author` (`:5083`) —
    emite `[<author_tag> role="nd"]…[/<author_tag>]` + `[fname]`/`[surname]`.
  - `fname-surname`/`fname-spanish-surname`/`surname-fname` →
    `markup_fname_and_surname` (`:5388`), …
  - `*kwdgrp` → `markup_keywords` (`:38`) — `[sectitle]`+`[kwd]`.
  - `aff`/`normaff`/`afftrans` → `markup_aff_label` (`:8099`) — emite
    `[label]…[/label]`.
  - `refs` (com tag `vancouv`, `apa`, `abnt6023`, `iso690`, `other`) →
    `add_citation_tag` (`:4517`) — `[vcitat]`/`[acitat]`/etc. por parágrafo.
  - `ref` → `mark_textref` (`:4621`) — `[text-ref]…[/text-ref]` para
    "_____ Idem.".
  - `tabwrap`/`figgrp`/`equation` → `markupGraphics` +
    `markup_label_and_caption` — emitem `[graphic href="…"]`,
    `[label]`, `[caption]`.
  - `*list`/`*deflist`/`versegrp`/`*fn`/`*fngrp`/`*page-fn`/`xmlbody`/
    `xmlabstr`/`ack`/`sec` → helpers em `clsMarkupXMLBody`.
- **Inline `[italic]`/`[bold]`/`[sup]`/`[sub]`**: NÃO vêm de botão.
  São aplicados pela função `mark_styles` (`:3033`) → `keepStyle` (`:3116`)
  durante save/parser, varrendo `Font.italic/bold/Superscript/Subscript`
  do Word.

## 6. Como reproduzir localmente (validação manual)

> Necessário Word para Windows (32-bit funciona melhor; 64-bit também,
> mas as macros têm `#If VBA7 Then ... Declare PtrSafe`). Não funciona no
> Word para macOS — o repositório é arquivado e o template depende de
> `automata.dll` Win32.

1. Renomeie `markup.prg` para `markup.dot` (apenas para o Word
   reconhecer extensão).
2. No Word: **Arquivo → Abrir → markup.dot** (não duplo-clique para não
   instalar globalmente).
3. **Alt+F11** abre o Visual Basic Editor — você vai ver os módulos
   `MainModule`, `clsMarkup`, `clsConfig`, `clsListBar`,
   `clsMarkupXMLBody`, `clsAttrList`, `clsAttribute`, `clsLink`,
   `clsLkList`, `clsInterface`, `clsNode`, `ClsConversionTable`, etc.
4. Para *executar* o add-in de verdade, é preciso o tree completo de
   `src/scielo/bin/markup/` (com `app_core/`, `app_modules/`,
   `automata.dll`, `default.mds`) no diretório que `Markup.exe` espera
   (típico: `C:\Scielo\Bin\Markup\`). Caso contrário o
   `LoadPublicValues` falha lendo `start.mds` / `default.mds` /
   `<lang>_conf.mds`.
5. Para testar manualmente uma macro de inserção sem `Markup.exe`,
   na janela Imediato (Ctrl+G) faça:
   ```vba
   Dim m As New clsMarkup
   m.mConf.LoadPublicValues
   ' selecione um trecho no documento e:
   Dim r As Range : Set r = Selection.Range
   m.tag_text_range r, "[doctitle language=""pt""]", "[/doctitle]", _
                    wdColorBlue
   ```
   Você vai ver `[doctitle …]…[/doctitle]` ao redor do texto, em fonte
   Arial azul.

## 7. Gap analysis — tags documentadas vs. botões da UI

- **`markup_tags.rst` lista 219 nomes** (após limpar falsos positivos).
- **`tree.txt` lista 239 nomes de botões/nodes**.
- **Coberta na UI mas não documentada**: 23 tags
  (`anonymous`, `authorid`, `code`, `cpholder`, `cpright`, `cpyear`,
  `credit`, `ctrbid`, `custom`, `customgrp`, `elocatid`,
  `fname-spanish-surname`, `fname-surname`, `histdate`, `institid`,
  `licinfo`, `name`, `oprrole`, `page-fn`, `prefix`, `suffix`, `value`,
  `xhtml`).
  - Algumas (`fname-surname`, `fname-spanish-surname`) são pseudo-tags
    de UI (cada botão chama uma macro composta) — não viram tag SGML.
  - Outras parecem extensões mais novas que não foram refletidas no
    `markup_tags.rst`.
- **Documentada mas sem botão**: apenas `authid` (alias de `authorid`).

## 8. Observações importantes / ressalvas

1. **`mixed-citation` (JATS)** não existe em SciELO Markup. O equivalente
   funcional é `[vcitat]`/`[pcitat]`/`[acitat]`/`[icitat]`/`[ocitat]`/`[ref]`.
   A conversão para o `mixed-citation` JATS acontece a *jusante*,
   no pipeline Python `markup_xml/`, não nestas macros.
2. **`fpage`/`lpage`/`vol`/`num`/`pubtype`** também são JATS — em SciELO
   Markup essas informações chegam como atributos de `[doc]`/`[ref …]`
   ou pelas tags `pages`/`volid`/`issueno`. Ver tabela em
   `_analysis/TAG_INDEX.md`.
3. As macros têm muito código duplicado (ex.: `keepStyle`, `keepstylebkp2`,
   `keepStyleBkp`, `markup_fname_and_surname` vs. `oldmarkup_fname_and_surname`
   `markup_macros.txt:5540`) — versões antigas mantidas mortas.
4. Há referências a paths Windows hard-coded (`C:\Scielo\Bin\Markup\`)
   em `pt_conf.mds`/`en_conf.mds`/`es_conf.mds` (campo `directory`).
   `LoadPublicValues` os usa diretamente.

## 9. Anexos em `_analysis/`

| Arquivo | Conteúdo |
|---|---|
| `markup_macros.txt` | Saída completa de `olevba --decode markup.prg` (116 359 linhas). |
| `tables_macros.txt` | idem para `tables.prg` (107 642 linhas). |
| `macros_index.txt` | índice `Sub`/`Function` de `markup_macros.txt` (697 entradas). |
| `tables_macros_index.txt` | idem para `tables_macros.txt` (664 entradas). |
| `markup_strings_brackets.txt` | Fallback `strings | grep '\[/?[a-z]'` confirmando os literais de tag dentro do binário. |
| `tags_in_tree.txt` | 239 tags-botão extraídas de `tree.txt`. |
| `tags_in_docs.txt` | 219 tags extraídas de `markup_tags.rst`. |
| `tags_in_tag_text_range.txt` | 27 tags com literal `tag_text_range(..., "[<tag>]", ...)` direto no VBA (especializações). |
| `tags_in_tree_not_in_docs.txt` | gap análise (UI sem doc). |
| `tags_in_docs_not_in_tree.txt` | gap análise (doc sem botão). |
| `TAG_INDEX.md` | tabela completa tag ↔ macro. |
| `UI_MENUS.md` | barra "Markup" e CommandBars dinâmicas. |
