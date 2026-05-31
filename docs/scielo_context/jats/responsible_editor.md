# Editor Responsável pelo Processo de Avaliação — `<fn fn-type="edited-by">`

> **Fase 3 (pós-Markup, JATS XML).** Curado de `_raw/SPS 1.10_pt.md`
> linhas 6633–6681. Fonte raw para detalhes ausentes: SPS 1.10 nessas
> linhas (ver [[README]]).

## O que é

Declaração do(a) editor(a) responsável pelo processo de avaliação do
documento aprovado, por transparência editorial. Quando o editor não
aceita ter o nome publicado, usa-se o nome da editora/editor-chefe (ou
ambos).

Marcada em `<fn fn-type="edited-by">` dentro de `<author-notes>`.

## Quando é obrigatória

Obrigatória para `@article-type` ∈:
`data-article`, `brief-report`, `case-report`, `rapid-communication`,
`research-article`, `review-article`. Opcional para os demais indexáveis.

## Marcação

- Elemento: `<fn>` com `@fn-type="edited-by"`, dentro de `<author-notes>`.
- Rótulo do cargo: **`<label>`** (ex.: `ASSOCIATE EDITOR:`,
  `SCIENTIFIC EDITOR:`). **Não** usar `<title>`, `<p>`, `<bold>` ou
  `<italic>` para o rótulo.
- Nome + ORCID do editor em `<p>`, com o ORCID em
  `<ext-link ext-link-type="uri">`.
- `<author-notes>` deve ocorrer **uma única vez** no documento.
- Pode haver múltiplos `<fn fn-type="edited-by">` (ex.: associate +
  scientific editor).

## Exemplo XML

```xml
<author-notes>
  <fn fn-type="edited-by">
    <label>ASSOCIATE EDITOR:</label>
    <p>Luana Patricia Marmitt <ext-link ext-link-type="uri" xlink:href="http://orcid.org/0000-0003-0526-7954">http://orcid.org/0000-0003-0526-7954</ext-link></p>
  </fn>
  <fn fn-type="edited-by">
    <label>SCIENTIFIC EDITOR:</label>
    <p>Juraci Almeida Cesar <ext-link ext-link-type="uri" xlink:href="http://orcid.org/0000-0003-0864-0486">http://orcid.org/0000-0003-0864-0486</ext-link></p>
  </fn>
</author-notes>
```

## Fase 3 — derivação e injeção

| O quê | Determinístico? | Fonte |
|---|---|---|
| **Nome + ORCID + cargo do editor** | Não derivável do docx do autor | **input externo** (metadado editorial — TBD, provavelmente análogo ao `other.txt`) |
| **Posição no XML** | Sim | criar/usar o único `<author-notes>`; anexar `<fn fn-type="edited-by">` |

> ⚠️ Dados do editor responsável **não estão** no manuscrito do autor —
> são metadados editoriais do periódico. A fonte do valor na fase 3
> precisa ser definida (input externo). Não havia implementação no
> projeto MathML antigo.
