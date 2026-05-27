# Data Availability — `<sec sec-type="data-availability">` / `<fn fn-type="data-availability">`

> **Fase 3 (pós-Markup, JATS XML).** Curado de `_raw/SPS 1.10_pt.md`
> linhas 6214–6632. Paradigma JATS XML — **não** confundir com as
> bracket tags da fase 2. Fonte raw para detalhes ausentes: SPS 1.10
> nessas linhas (ver [[README]] / skill `scielo-source-reference`).

## O que é

Declaração de disponibilidade dos dados de pesquisa subjacentes ao
documento. Marcada de **duas formas alternativas**:

- **Seção**: `<sec sec-type="data-availability">` em `<body>` ou `<back>`.
- **Nota**: `<fn fn-type="data-availability">` dentro de `<fn-group>` em `<back>`.

O título/rótulo é **mandatório**: `<title>` na seção, `<label>` na nota.

## Quando é obrigatória

Obrigatória para `@article-type` ∈:
`data-article`, `brief-report`, `case-report`, `rapid-communication`,
`research-article`, `review-article`.

Opcional para os demais indexáveis — **exceto** Errata (`correction`),
Retratação (`retraction`/`partial-retraction`), Adendo (`addendum`) e
Manifestação de Preocupação (`expression-of-concern`), que **não** a contêm.

## Atributos obrigatórios

| Forma | Atributos |
|---|---|
| `<sec>` | `sec-type="data-availability"`, `specific-use` |
| `<fn>`  | `fn-type="data-availability"`, `specific-use` |

### Valores de `@specific-use`

| Valor | Significado |
|---|---|
| `data-available` | Dados disponíveis em repositório. |
| `data-available-upon-request` | Disponíveis apenas mediante solicitação. |
| `data-in-article` | Disponíveis no corpo do documento. |
| `data-not-available` | Não disponíveis. |
| `uninformed` | Uso não informado / nenhum dado gerado ou utilizado. |

## Exemplos XML

Nota em `<back>`:

```xml
<back>
  <fn-group>
    <fn fn-type="data-availability" specific-use="data-available" id="fn1">
      <label>Data Availability Statement</label>
      <p>…</p>
    </fn>
  </fn-group>
</back>
```

Seção:

```xml
<sec sec-type="data-availability" specific-use="data-available-upon-request">
  <title>Disponibilidade de Dados</title>
  <p>…</p>
</sec>
```

Quando `specific-use="data-in-article"`, recomenda-se `<xref>` apontando
para as partes do documento (`ref-type="bibr|table|fig"`).

## Fase 3 — derivação e injeção

| O quê | Determinístico? | Fonte |
|---|---|---|
| **Texto da declaração** (`<p>`) | Sim — está no documento do autor | seção/parágrafo "Disponibilidade de Dados" do `.docx`/XML |
| **`@specific-use`** | **Não, é julgamento** | classificação do texto em 1 dos 5 valores (ver corpus abaixo) |
| **`sec` vs `fn`** | Convenção | escolher 1 forma e padronizar |
| **Posição no XML** | Sim | regra de placement abaixo |

> ⚠️ O projeto MathML antigo **não classificava** `@specific-use` — o
> valor vinha de input manual (default `data-available-upon-request`).
> A classificação texto→valor é a parte nova e não-trivial da fase 3;
> o corpus condensado abaixo serve para semear uma heurística.

### Regra de placement (determinística, validada no MathML)

- **Section + `location=body`**: append como último filho de `//body`.
- **Section + `location=back`**: se existe `<ack>`, inserir **logo após**
  o `<ack>`; senão **antes** do primeiro filho de `<back>`; senão como
  último filho.
- **Footnote**: localizar/criar `<fn-group>` em `<back>` (mesma regra
  ack-relativa), e anexar o `<fn>` ao grupo.
- Namespace herdado do root do documento.

## Corpus condensado para classificação (texto → `@specific-use`)

Exemplos representativos por valor. **Corpus completo: SPS 1.10 linhas
6380–6632.**

- **`data-available`** — dados depositados em repositório, com link/DOI:
  *"The data that support the findings of this study are openly available
  in [repositório] at [DOI/URL]."*
- **`data-available-upon-request`** — mediante solicitação:
  *"The datasets generated and/or analyzed during the current study are
  available from the corresponding author upon reasonable request."*
- **`data-in-article`** — dados no próprio documento:
  *"Os dados de pesquisa estão disponíveis no artigo / em suas tabelas e
  figuras."* (acompanhar de `<xref>`).
- **`data-not-available`** — indisponíveis:
  *"The data are not available / cannot be shared due to [ético/legal]."*
- **`uninformed`** — nenhum dado de pesquisa gerado ou utilizado:
  *"Não se aplica / no new data were created or analyzed in this study."*
