# TAG_INDEX — tags SciELO ↔ macros VBA

Convenções:
- "Arquivo:linha" se refere ao arquivo extraído por `olevba`
  (`_analysis/markup_macros.txt`), não ao binário `markup.prg`. As macros
  reais residem em `src/scielo/bin/markup/markup.prg` (OLE) e foram
  decodificadas para o `.txt` para inspeção.
- A maioria dos botões da UI cai num caminho **default**:
  `InsertTag` → `buildStartTag` (`[tag attr=...]`) +
  `BuildFinishTag` (`[/tag]`) → `tag_text_range` (insere antes/depois da
  seleção). Esse default é executado quando o `Select Case
  tag_button_label` dentro de `InsertTag` não tem cláusula específica.
- Macros com prefixo `markup_*` ou `mark_*` são as **especializações**:
  acionadas por `Select Case` antes da rotina genérica e tipicamente
  emitem **vários** pares de tags (ex.: `markup_fname_and_surname` emite
  `[fname]…[/fname]` + `[surname]…[/surname]` + `[suffix]…[/suffix]`).

## Onde os delimitadores `[`, `[/`, `]` são definidos

| Símbolo | Valor | Onde é lido |
|---|---|---|
| `STAGO` | `[` | `clsConfig.LoadPublicValues` em `markup_macros.txt:7791` lê de `app_core/<lang>_conf.mds` |
| `ETAGO` | `[/` | idem |
| `TAGC`  | `]` | idem |

Conferido em `src/scielo/bin/markup/app_core/pt_conf.mds` (linhas
`STAGO,[ ; ETAGO,[/ ; TAGC,]`).

## Funções utilitárias (referência)

| Função | Local | O que faz |
|---|---|---|
| `Public Function buildStartTag(tag, conf, attl, link, inter)` | `markup_macros.txt:1003` | Monta `STAGO + tag + atributos + TAGC`. Atributos vêm de `clsLink`/`clsAttrList`/`clsInterface` (lidos de `link.mds` + `<lang>_attb.mds` + form). |
| `Public Function BuildFinishTag(tag, conf, force)` | `markup_macros.txt:1072` | Monta `ETAGO + tag + TAGC`, pulando se `isEmptyTag(tag)`. |
| `Public Function writeAttribute(attName, attVal)` | `markup_macros.txt:1063` | Formata `" name=\"valor\""` (sem aspas duplicadas). |
| `Sub tag_text_range(text_range, open_tag, close_tag, color, font_size, fix_end)` | `markup_macros.txt:3699` | Núcleo da inserção. Usa `range.InsertBefore` / `range.InsertAfter` no Word, fixando `Font.Name="Arial"` + `Font.Color=color`, e ajustando o fim para não engolir parágrafo final. |
| `Public Function InsertTag(tag_button_label, color, conf, ...)` | `markup_macros.txt:371` | Dispatcher principal: aplica especializações (`Select Case`) e, no caminho default, chama `tag_text_range(doc, stag, ftag, …)`. |

## Tabela tag → macro

Estado: `default` = inserção genérica via `InsertTag`/`tag_text_range`;
nome de função = especialização chamada explicitamente.

### Bibliografia / metadados de artigo

| Tag | Função/macro VBA | Arquivo:linha | O que insere | Atributos |
|---|---|---|---|---|
| `doc` (root, marcação automática total) | `InsertTag` (`Case "doc"` chama `clsMarkupXMLBody.mark_template`) → ainda escreve `[doc …]`/`[/doc]` via default | `markup_macros.txt:445–453` (Case), `11157` (`mark_template`) | `[doc]` antes do documento todo, `[/doc]` no fim; `mark_template` chama `mark_template_author` e `mark_template_aff` para emitir `[author]`/`[aff]` em massa. | atributos do form `frmMainElement` (root) |
| `article`, `text` (root tags) | `InsertTag` default (root) | `markup_macros.txt:404–408` | `[article …]`/`[/article]` ou `[text …]`/`[/text]` ao redor do documento inteiro (`ActiveDocument.Select`). | atributos do form `frmMainElement` |
| `front`, `body`, `back` | default | `markup_macros.txt:741` | `[front]…[/front]` etc. via `tag_text_range`. | — |
| `doctitle` | default | uso explícito em `mark_template`: `markup_macros.txt:11185` `tag_text_range(r, "[doctitle language=…]", "[/doctitle]", …)`. Quando vem do botão "doctitle", é o caminho default. | `[doctitle language="pt|en|es"]…[/doctitle]` | `language` |
| `toctitle` | default | uso explícito em mark_template: `markup_macros.txt` (próximo ao `doctitle`) e default | `[toctitle]…[/toctitle]` | — |
| `subtitle`, `alttitle`, `chptitle`, `arttitle`, `series` | default | `Select Case` em `InsertTag` faz find/replace de `reftype=` para algumas (ex. `arttitle` força `reftype="journal"` em `markup_macros.txt:466`); resto é default | `[<tag>]…[/<tag>]` | conforme `<lang>_attb.mds` |

### Autoria

| Tag | Função/macro VBA | Arquivo:linha | O que insere | Atributos |
|---|---|---|---|---|
| `*author`, `*oauthor`, `*pauthor` (autom.) | `InsertTag` `Case "*author","*oauthor","*pauthor"` → `markup_author(doc, tag)` | `markup_macros.txt:543–545` (Case), `5083` (`markup_author`) | Chama `markup_surname_and_fname` e depois `tag_text_range(selected, "[author role=\"nd\"]", "[/author]", …)` (linha 5125–5128). Em seguida `mark_all_of_a_kind` propaga para o resto da lista de referências. | `role="nd"` por padrão (autoria normal) |
| `*authors` (grupo de autores) | `InsertTag` `Case "*authors"` → `mark_authors(doc)` | `markup_macros.txt:456` (Case), `4799` (`mark_authors`) | Detecta separadores `;`, `,`, `&`, `and`, `y`, `e`, e `et al`. Para cada autor emite `[pauthor]…[/pauthor]` ou similar (`tag_text_range(dimrange, "[pauthor]", "[/pauthor]", …)` em `markup_macros.txt:4920/4946`). Marca `[et-al]…[/et-al]` se presente. | — |
| `author` (manual) | default | `markup_macros.txt` default | `[author role="…"]…[/author]` | `role`, `id` (do `link.mds`) |
| `fname-surname` | `InsertTag` `Case "fname-surname"` → `markup_fname_and_surname(doc)` | `markup_macros.txt:534` (Case), `5388` (`markup_fname_and_surname`) | Split por espaço, detecta sufixos PT (`Jr`, `Neto`, `Filho`, `Sobrinho`); emite `[fname]…[/fname]` + `[surname]…[/surname]` (+ opcional `[suffix]…[/suffix]`) via `tag_text_range`. **Não** emite `[author]` (o `author` precisa ter sido marcado antes). | — |
| `fname-spanish-surname` | `InsertTag` `Case "fname-spanish-surname"` → `markup_fname_and_spanish_surname(doc)` | `markup_macros.txt:537` (Case), `5464` (`markup_fname_and_spanish_surname`) | Considera dois sobrenomes no estilo espanhol; emite `[fname]…[/fname]` + `[surname]…[/surname]` (concatenando os dois últimos). | — |
| `surname-fname` | `InsertTag` `Case "surname-fname"` → `markup_surname_and_fname(doc, True)` | `markup_macros.txt:540` (Case), `5248` (`markup_surname_and_fname`) | Mesmo que acima mas inverte a ordem; usa parâmetro `surname_first`. | — |
| `fname` | default | — | `[fname]…[/fname]` | — |
| `surname` | default | — | `[surname]…[/surname]` | — |
| `suffix` | default; também emitido por `markup_fname_and_surname` | — | `[suffix]…[/suffix]` | — |
| `corpauth`, `cauthor`, `ocorpaut` | default | — | `[corpauth]…[/corpauth]` etc.; `cauthor` usado nos refs para autoria corporativa em modelo "other". | — |
| `et-al` | inserido implicitamente em `mark_authors` (`tag_text_range(etal_range, "[et-al]", "[/et-al]", …)` em `markup_macros.txt:4827`) e em `markup_author` (`markup_macros.txt:5147`); também via default ao apertar o botão. | `[et-al]…[/et-al]` | — |
| `role` | NÃO é tag — é **atributo** de `[author …]`. Inserido por `writeAttribute("role", val)`. | `markup_macros.txt:1063`, e diretamente em `markup_author`: `attr = "  role=" & Chr(34) & "nd" & Chr(34)` (`markup_macros.txt:5113–5117`). | atributo dentro de `[author role="nd"]` | — |
| `authorid`, `authid` | default | `tree.txt` | `[authorid]…[/authorid]` (existe `authid` na doc, alias) | — |
| `anonym`/`anonymous` | default | — | `[anonym]…[/anonym]` | — |

### Afiliações / contato

| Tag | Função/macro VBA | Arquivo:linha | O que insere | Atributos |
|---|---|---|---|---|
| `aff`, `normaff`, `afftrans` | `InsertTag` `Case "aff", "normaff", "afftrans"` → `clsMarkupXMLBody.markup_aff_label(doc, lf)` antes de cair no default | `markup_macros.txt:572–582` (Case), `8099` (`markup_aff_label`) | `markup_aff_label` localiza o supescrito (label "1", "a") e marca `[label]…[/label]`. Em seguida `InsertTag` cai no default e emite `[aff id="aff1" …]…[/aff]` (ou `[normaff …]`, `[afftrans …]`). | `id`, `iso2862`, `orgname`, etc. (do `link.mds`/`<lang>_attb.mds`) |
| `orgname`, `orgdiv`, `orgdiv1`, `orgdiv2`, `country`, `state`, `city`, `zipcode`, `email` | default | — | `[orgname]…[/orgname]` etc. | — |
| `corresp` | default | — | `[corresp]…[/corresp]` | — |

### Resumo / Palavras-chave / Seções

| Tag | Função/macro VBA | Arquivo:linha | O que insere | Atributos |
|---|---|---|---|---|
| `xmlabstr` | `InsertTag` `Case "xmlabstr"` → `clsMarkupXMLBody.mark_xmlabstr(doc)` (`markup_macros.txt:592–597`, `8364`) | gera múltiplos `[sec]/[sectitle]/[p]` dentro do resumo | `[xmlabstr]…[/xmlabstr]` (default) e marcação interna automática | — |
| `abstract` | default | — | `[abstract …]…[/abstract]` | `language` |
| `keygrp` | default | — | `[keygrp]…[/keygrp]` | `language` |
| `*kwdgrp` | `InsertTag` `Case "*kwdgrp"` → `clsMarkup.markup_keywords(doc)` (`markup_macros.txt:548–550`, `38`) | Localiza o ":" → emite `[sectitle]…[/sectitle]` para o título "Palavras-chave". Depois `[kwd]…[/kwd]` para a primeira palavra-chave. Find/replace nos separadores `;`, `,`, `. ` → reescreve para `"[/kwd]; [kwd]"` etc., para fechar/abrir cada `kwd`. Não envelopa um `[kwdgrp]` automaticamente. | — |
| `kwdgrp`, `kwd`, `keyword`, `subkey` | default | — | `[kwdgrp]…[/kwdgrp]`, `[kwd]…[/kwd]` | — |
| `sec` | `InsertTag` `Case "sec"` → `clsMarkupXMLBody.mark_sec(doc)` (`markup_macros.txt:583–591`, `8518`) | Localiza estilos `Heading 1/2/3` e/ou padrão `[sectitle]`/`[p]` → emite `[sec][sectitle]…[/sectitle]` e fecha `[/sec]` ao próximo heading. | — |
| `sectitle`, `subsec`, `subkey` | default; `sectitle` usado abundantemente nas especializações | `markup_macros.txt:57, 277, 4583, 8400…` | `[sectitle]…[/sectitle]` etc. | — |

### Citações / referências

A SciELO Markup **não** usa o `mixed-citation` do JATS. Em vez disso adota
um modelo norma-específico:

| Norma | Tag-grupo | Tag-citação |
|---|---|---|
| Vancouver | `vancouv` | `vcitat` |
| APA | `apa` | `pcitat` |
| ABNT/NBR-6023 | `abnt6023` (também `nbr6023`) | `acitat` |
| ISO-690 | `iso690` | `icitat` |
| Outras / desconhecidas | `other` | `ocitat` |
| Genérico | `refs` | `ref` |

| Tag | Função/macro VBA | Arquivo:linha | O que insere |
|---|---|---|---|
| `refs` (e `vancouv`/`apa`/`abnt6023`/`iso690`/`other`) | `InsertTag` `Case "refs"` → escolhe `citation_tag = Mid(tag,1,1)+"citat"` (ou `"ref"` se tag=`refs`) e chama `add_citation_tag(doc, citation_tag)` (`markup_macros.txt:553–567`, `4517`) | `add_citation_tag` itera parágrafos do bloco e marca cada parágrafo com `[<citation_tag>]…[/<citation_tag>]` (ex. `[vcitat]…[/vcitat]`). O wrapper externo `[refs]…[/refs]` (ou `[vancouv]…[/vancouv]`) vem do default. | conforme norma |
| `ref` | `InsertTag` `Case "ref"` → `mark_textref(doc)` (`markup_macros.txt:570`, `4621`) | `mark_textref` localiza padrão `_____` (linhas de ditto/idem) e emite `[text-ref]…[/text-ref]`. Em outros lugares emite `[ref id="r<n>" reftype="<journal|book|…>"]…[/ref]` (`markup_macros.txt:4609`). | `id`, `reftype` |
| `vcitat`, `acitat`, `ocitat`, `pcitat`, `icitat` | default ou via `add_citation_tag` | — | `[vcitat]…[/vcitat]` etc. |
| `vcontrib`, `pcontrib`, `acontrib`, `ocontrib`, `icontrib` | default; e gerados pelo automata1 (`automata.dll` lendo `vancouv.amd`) | — | `[vcontrib]…[/vcontrib]` etc. |
| `vmonog`, `pmonog`, `amonog`, `omonog`, `imonog` | default; gerados pelo automata1 | — | `[vmonog]…[/vmonog]` etc. (`mpubinfo`/`spubinfo` agrupam dados de publicação) |
| `viserial`, `piserial`, `aiserial`, `oiserial`, `iiserial` | default; gerados pelo automata1 | — | `[viserial]…[/viserial]` etc. |
| `vstitle`/`stitle`/`source` (titulo de publicação seriada) | `InsertTag` `Case "*stitle","*sertitle","*source"` → `markup_all_the_sertitles(doc, tag)` (`markup_macros.txt:546`, `5198`) | Localiza variantes do mesmo título no documento e marca todas; default fecha o par. | — |
| `text-ref` | `mark_textref` (acima) | `markup_macros.txt:4621` | `[text-ref]…[/text-ref]` para a "linha" `_____ Idem.` em referências |
| `xref` | usado em pós-processamento (`mark_template`/`mark_xmlbody` fazem `tag_text_range(t, "[xref ref-type=\"aff\" rid=\"…\"]", "[/xref]", …)` em `markup_macros.txt:11257`, `8224`); botão default emite `[xref]…[/xref]` | `markup_macros.txt:11257`, `8224`, `5850` | `[xref ref-type="aff|bibr|…" rid="…"]…[/xref]` | `ref-type`, `rid` |

### Páginas / volume / fascículo / data

Em SciELO Markup, **não existem** as tags `fpage`/`lpage`/`vol`/`num`/`pubtype`
do JATS. O mapeamento real é:

| Conceito | Tag SciELO | Função | Atributos |
|---|---|---|---|
| Faixa de páginas | `pages` | default; também emitido por XSLT `automata2/vancouv.xsl:`fpage/lpage` → `[pages]<fpage>-<lpage>[/pages]` | — |
| Página inicial / página seq | atributo `fpage_seq` em `[doc]` (mensagem `msg_help_doc_fpage_seq`) | form `frmMainElement` | atributo |
| Volume | `volid` | default | — |
| Número da revista (issue) | `issueno` | default | — |
| Suplemento | `suppl`, `supplno`, `supplvol` | default | — |
| ID alternativo (e-locator) | `elocatid` | default | — |
| Ano | `cpyear` (copyright), parte de `[date dateiso="YYYYMMDD"]…[/date]` | default; `mark_template`/`automata2/vancouv.xsl` constroem `[date dateiso="…"]<year>[/date]` | `dateiso` |
| Data de publicação | `date`, `dperiod`, `histdate`, `accepted`, `received`, `revised`, `cited`, `update` | default | `dateiso` (no formato `YYYYMMDD`) |
| Tipo de publicação | atributo `reftype` em `[ref reftype="journal\|book\|report\|patent\|confproc\|thesis"]`; também `pubtype` não-existe como tag, mas o `[ref reftype=…]` é trocado por `Case "reportid"`, `Case "patentno"`, `Case "confname"`, `Case "thesgrp"`, `Case "arttitle"` em `InsertTag` (`markup_macros.txt:462–470`) usando `selection.Find.Execute(findText:="reftype=\"book\"", replacewith:="reftype=\"<novo>\"")`. | atributo |

### Inline (formatação preservada como tag)

| Tag | Função/macro VBA | Arquivo:linha | Como entra |
|---|---|---|---|
| `[italic]…[/italic]` | `Private Sub keepStyle("italic")` → `tag_text_range(sel, "[italic]", "[/italic]", wdColorDarkTeal)` | `markup_macros.txt:3116` (def. `keepStyle`); chamada via `Function mark_styles(filename)` em `markup_macros.txt:3033` (loop sobre `["sup","sub","italic","bold"]`). Também na rotina `keepStyleBkp` (`markup_macros.txt:3384`). | acionado durante `mark_template`/save → percorre o documento procurando trechos com `Font.italic = True` e os envelopa. |
| `[bold]…[/bold]` | `keepStyle("bold")` | idem | idem (Word `Font.Bold = True` → `[bold]…[/bold]`). |
| `[sup]…[/sup]` | `keepStyle("sup")` | idem; também emitido manualmente em `markup_macros.txt:10682–10683` para sufixos como `[sup]…[/sup]`. | Word `Font.Superscript = True`. |
| `[sub]…[/sub]` | `keepStyle("sub")` | idem | Word `Font.Subscript = True`. |

### Listas, equações, tabelas, figuras

| Tag | Função/macro VBA | Arquivo:linha | O que insere |
|---|---|---|---|
| `*list`, `list` | `InsertTag` `Case "*list"` → `clsMarkupXMLBody.mark_list(doc)` (`markup_macros.txt:602–610`, `8121`) | Limpa `[li]/[/li]/[p]/[/p]/[quote]/[/quote]` antigos, depois itera parágrafos e marca cada um com `[li]…[/li]`. |
| `*deflist`, `deflist`, `def`, `defitem` | `InsertTag` `Case "*deflist"` → `mark_deflist(doc)` (`markup_macros.txt:614`, `233`) | Marca `[deflist]…[/deflist]`, dentro `[defitem]…[/defitem]` e `[def]…[/def]`. |
| `equation` | `InsertTag` `Case "equation"` → `clsMarkupXMLBody.markupGraphics(doc, True)` (`markup_macros.txt:480–504`) | Apaga `[p]`/`[/p]`, depois emite `[graphic href="?"]…[/graphic]` ao redor de cada imagem dentro do range. Wrapper `[equation]…[/equation]` vem do default. |
| `tabwrap` | `InsertTag` `Case "tabwrap"` → `clsMarkupXMLBody.mark_tabwrap_content(doc)` (`markup_macros.txt:472–479`, `9350`) | Marca tabela: `[table]…[/table]`, `[thead]/[tbody]`, `[tr]…[/tr]`, `[caption]…[/caption]`, `[label]…[/label]`. Wrapper `[tabwrap]…[/tabwrap]` do default. |
| `figgrp` | `InsertTag` `Case "figgrp"` → `markupGraphics` + `markup_label_and_caption` (`markup_macros.txt:506–531`, `8996`) | Detecta padrão `Figura N – legenda` separado por `* * `; emite `[label]…[/label]` para o número e `[caption]…[/caption]` para o resto. |
| `graphic` | inserido por `markupGraphics` (`tag_text_range(g_range, "[graphic href=\"<arquivo>\"]", "[/graphic]", …)` em `markup_macros.txt:8876, 8894, 8911`) | `[graphic href="<nome|?>"]…[/graphic]` | `href` |
| `caption`, `label` | default; usados extensivamente pelas rotinas de figgrp/tabwrap/aff. | `markup_macros.txt:9025, 9033, 9122, 9130, 11361, 11696, 11720, 11736` | `[label]…[/label]`, `[caption]…[/caption]` |

### Notas de rodapé

| Tag | Função/macro VBA | Arquivo:linha | O que insere |
|---|---|---|---|
| `*fngrp` | `InsertTag` `Case "*fngrp"` → `mark_fngrp(doc, linkl, attl, inter)` (`markup_macros.txt:622–629`, `11635`) | Wrapper `[fngrp]…[/fngrp]` |
| `*fn` | `InsertTag` `Case "*fn"` → `mark_fn(doc)` (`markup_macros.txt:631–637`, `11687`) | `[fn id="fn<n>" fntype="other"]…[/fn]` (linha 11678) |
| `*page-fn` | `InsertTag` `Case "*page-fn"` → `markup_all_the_footnotes` (`markup_macros.txt:550–552`, `5982`) | percorre o `Word.Footnotes` e converte cada nota em `[fn]…[/fn]` |
| `fntable` | `tag_text_range(fn, "[fntable id=\"…\"]", "[/fntable]", …)` em `markup_macros.txt:9286` | `[fntable id="fntable<n>"]…[/fntable]` |

### Outros

| Tag | Função/macro VBA | Arquivo:linha |
|---|---|---|
| `quote`, `sigblock` | `InsertTag` `Case "quote", "sigblock"` chama `removeTag(doc, "p")` antes do default (`markup_macros.txt:471–473`). |
| `xmlbody` | `InsertTag` `Case "xmlbody"` → `clsMarkupXMLBody.mark_xmlbody(doc, lf)` (`markup_macros.txt:586–591`, `8456`). Marcação automática completa do `[body]`. |
| `versegrp`, `versline` | `InsertTag` `Case "versegrp"` → `mark_verse_group(doc)` (`markup_macros.txt:610–622`, `8503`) — marca `[versline]…[/versline]` por linha. |
| `ack` (acknowledgments) | `Case "ack"` → `markup_ack(doc)` → `mark_sectitle_and_paragraphs` (`markup_macros.txt:8117`, e default fecha `[ack]/[/ack]`). |
| `pubname-loc / publoc-pubname` (`*publoc-name`/`*pubname-publoc`) | `Case "*pubname-loc","*pubname-publoc"` → `mark_publisher(doc, "pubname", "publoc")` (`markup_macros.txt:459–463`, `174`) — marca `[pubname]…[/pubname]` e `[publoc]…[/publoc]` em ordem flexível. |
| `url`, `uri`, `doi` | default; `doi` também marcado em `mark_template` (`markup_macros.txt:11171` `tag_text_range(r, "[doi]", "[/doi]", …)`). |
| `sciname` | inserido por find/replace em `markup_macros.txt:10503,10566,10576` (`selection.text = "[sciname]" & selection.text & "[/sciname]"`). |
| `fontsymbol` | inserido em `markup_macros.txt:4220` (`tag_text_range(r, "[fontsymbol]", "[/fontsymbol]", wdColorOrange)`) — para preservar caracteres em fonte Symbol. |

## Tags que aparecem em `markup_tags.rst` mas **não** têm botão em `tree.txt`

Apenas dois itens, ambos artefatos da extração:
- `authid` — alias documental de `authorid` (presente em `tree.txt`).
- `Element` — falso positivo (cabeçalho RST).

→ Cobertura UI ↔ documentação está praticamente em paridade.

## Tags em `tree.txt` que **não** estão em `markup_tags.rst`

23 itens — provavelmente legado/internos sem documentação:

```
anonymous, authorid, code, cpholder, cpright, cpyear, credit, ctrbid,
custom, customgrp, elocatid, fname-spanish-surname, fname-surname,
histdate, institid, licinfo, name, oprrole, page-fn, prefix, suffix,
value, xhtml
```

(ver `_analysis/tags_in_tree_not_in_docs.txt`)
