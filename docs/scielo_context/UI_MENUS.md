# UI / Toolbar do template Word (`markup.prg`)

Toda a UI é construída via Word **CommandBars** dinamicamente, em tempo de
abertura do template global. Não há `.frx`/menu hardcoded com um botão por
tag. O modelo é totalmente **data-driven**: o template lê
`app_core/<lang>_bars.mds` + `app_core/tree.txt` e gera as barras.

## Pontos de entrada

| O que | Onde | Descrição |
|---|---|---|
| Menu raiz "StartUp" (visível no Word ao abrir o template) | `_analysis/markup_macros.txt:16412–16450` (`Sub` que adiciona o `CommandBarPopup` "StartUp") | Cria um popup com 2 itens: `Main` (`OnAction = "MainModule.Main"`) e `MainConfig` (`OnAction = "MainModule.MainConfig"`). É a porta de entrada do usuário. |
| Barra principal "Markup" | `_analysis/markup_macros.txt:7343–7386` | 17 botões fixos: Stop, Change, Delete, Automatic, AutomaticWS, Automatic3, Save, Parser, GenerateXML, FilesAndDTDReport, XMLValidate, ContentValidation, Preview, PMCValidate, PMCPreview, ViewMarkup, Help. Os tooltips são lidos de `clsConfig.button*` (que vem de `pt_conf.mds`). Todos compartilham `OnAction = MainModule.Main` (o dispatcher). |
| Barras de tags (uma por nó pai do `tree.txt`) | `_analysis/markup_macros.txt:7259–7340` (`CreateBars`) | Para cada nó-pai em `tree.txt` cria `CommandBars.add(name:="mkpbar"+nodename)`. Cada filho do nó vira `CommandBarButton` com `b.caption = child.name` (= nome da tag). Todos os botões de tag também têm `OnAction = conf.MainModule` (= `MainModule.Main`). Adiciona ainda botões "↓" (FaceId 40) e "↑" (FaceId 38) para navegar a hierarquia. |
| KillBars (limpeza ao fechar) | `_analysis/markup_macros.txt:7389–7400` | Apaga as barras `mkpbar<nodename>`. |

## Como o clique do usuário vira tag

```
botão clicado (caption = "fname", "aff", "doctitle", ...)
        │  OnAction = "MainModule.Main"
        ▼
Sub Main()                           _analysis/markup_macros.txt:12337
        │ button = CommandBars.ActionControl
        │ Select Case button.TooltipText  (botões fixos da barra "Markup")
        │ Case Else  →  SubElse(...)      (qualquer botão de tag)
        ▼
Sub SubElse(...)                     _analysis/markup_macros.txt:13307
        │ button_label = button.caption           ' = nome da tag
        │ ok = m.InsertTag(button_label, lBar.color(button_label),
        │                  conf, ..., lAttr, lk, i)
        ▼
Public Function InsertTag(...)       _analysis/markup_macros.txt:371
        │ stag = buildStartTag(tag, conf, ...)    ' "[tag attr=...]"
        │ ftag = BuildFinishTag(tag, conf)        ' "[/tag]"
        │ Call tag_text_range(doc, stag, ftag, color)
        ▼
Sub tag_text_range(...)              _analysis/markup_macros.txt:3699
          before.InsertBefore open_tag           ' insere "[tag…]" antes
          after.InsertAfter  close_tag           ' insere "[/tag]" depois
          (mais aplica Font.Name = Arial, Font.color = color do listbar)
```

## O que carrega a configuração das barras

`Public Sub LoadPublicValues` (`_analysis/markup_macros.txt:7791`) abre, em
ordem, três arquivos de texto separados por vírgula:

1. `markup\default.mds` — idioma e responsável.
2. `markup\app_core\<lang>_conf.mds` — onde mora **STAGO=`[`, ETAGO=`[/`,
   TAGC=`]`** e os labels dos botões fixos
   (`buttonStop`, `buttonChange`, `buttonAutomatic`, …).
3. `markup\app_core\<lang>_bars.mds` (via `clsConfig.fileBar`) — define a
   árvore de tags com `tag;tooltip;cor(WdColor);has_attr;has_children;link`.
   Exemplo (`pt_bars.mds`):
   ```
   aff;Identifica a institui��o da qual o autor faz parte;14;#TRUE#;#TRUE#;aff
   orgdiv;Identifica a divis�o de uma organiza��o;14;#TRUE#;#FALSE#;
   ```
   O 3º campo é a `WdColor` que `tag_text_range` aplica ao texto delimitado.

## Tooltip e localização

- Os tooltips das tags vêm de `tree_<lang>.txt`
  (`src/scielo/bin/markup/app_core/tree_pt.txt` etc.) — uma linha
  `nome_tag;descrição` por tag. `CreateBars` injeta isso em
  `b.TooltipText = child_node.definition`
  (`_analysis/markup_macros.txt:7310`).
- Os labels dos botões fixos vêm de `<lang>_conf.mds`.

## Botões fixos da barra "Markup" → ações

| Item # | Tooltip (conf.button*) | Ação despachada por `MainModule.Main` |
|---|---|---|
| 1 | buttonStop | `SubExit` (sair) |
| 2 | buttonChange | `SubChange` (editar tag/atributo) |
| 3 | buttonDelete | `SubDelete` (apagar tag) |
| 4 | buttonAutomatic | `SubAutomatic1(…, "[1]" …)` — automata1 (FSM por norma) |
| 5 | buttonAutomaticWS | `SubAutoMarkupServices` — refIdentifier (Java/Lucene) |
| 6 | buttonAutomatic3 | `SubAutomatic1(…, "[3]" …)` — automata1 com norma diferente |
| 7 | buttonSave | `executeButtonSave` |
| 8 | buttonParser | `executeButtonParser` (chama `parser.exe` SGML) |
| 9 | buttonGenerateXML | `executeButtonGenerateXML` (script Python) |
| 10 | buttonFilesAndDTDReport | `DisplayLogFile` |
| 11 | buttonXMLValidate | `DisplayValidationReport` |
| 12 | buttonContentValidation | `DisplayContentValidationReport` |
| 13 | buttonPreview | `PreviewFulltext` |
| 14 | buttonPMCValidate | `PMCDisplayValidationReport` |
| 15 | buttonPMCPreview | `PMCPreviewFulltext` |
| 16 | buttonViewMarkup | `open_sgml_file` |
| 17 | buttonHelp | `SubHelp` |

(referência: `_analysis/markup_macros.txt:12337–12700`).

## Itens não localizados em `markup.prg`

- Não existe um **botão único por tag definido manualmente no template** — o
  template não traz uma toolbar pré-fabricada com 200 botões; todos os
  botões de tag são gerados em runtime a partir de `tree.txt`/`*_bars.mds`.
- Não há atalho de teclado (`KeyBinding`) específico para inserir tags.
