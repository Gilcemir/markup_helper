# Papel do Autor (CRediT) — `<role>`

> **Fase 3 (pós-Markup, JATS XML).** Curado de `_raw/SPS 1.10_pt.md`
> linhas 2777–2879. Fonte raw para detalhes ausentes: SPS 1.10 nessas
> linhas (ver [[README]]).

## O que é

`<role>` marca o papel/contribuição de cada autor, dentro de
`<contrib>` em `<contrib-group>` (zero ou mais vezes). O periódico pode
usar qualquer taxonomia; a SciELO **recomenda CRediT**.

- **Com CRediT**: `<role content-type="<URL-do-papel-CRediT>">Termo</role>`.
- **Sem CRediT / outra taxonomia**: `<role>texto livre</role>` **sem**
  `@content-type`.

## Regras

- **Um `<role>` por papel.** Nunca agrupar vários papéis numa só tag.
- **All-or-nothing CRediT**: ou adota os termos CRediT na íntegra (com
  `@content-type`), ou não usa `@content-type` em nenhum — mesmo que
  alguns papéis coincidam com termos CRediT.
- O uso de taxonomia é **por documento**, não por lote/revista.
- Tradução dos termos para outros idiomas fica a critério do periódico.
- **Não** usar `<fn fn-type="con">` para papéis de autor.

## Tabela CRediT (`@content-type` = URL)

Base: `https://credit.niso.org/contributor-roles/<slug>/`

| Termo | slug |
|---|---|
| Conceptualization | `conceptualization` |
| Data curation | `data-curation` |
| Formal analysis | `formal-analysis` |
| Funding acquisition | `funding-acquisition` |
| Investigation | `investigation` |
| Methodology | `methodology` |
| Project administration | `project-administration` |
| Resources | `resources` |
| Software | `software` |
| Supervision | `supervision` |
| Validation | `validation` |
| Visualization | `visualization` |
| Writing – original draft | `writing-original-draft` |
| Writing – review & editing | `writing-review-editing` |

## Exemplos XML

Com CRediT:

```xml
<contrib-group>
  <contrib contrib-type="author">
    <contrib-id contrib-id-type="orcid">1234-0001-9486-8465</contrib-id>
    <name><surname>Rosa</surname><given-names>Nathália</given-names></name>
    <xref ref-type="aff" rid="aff1">1</xref>
    <role content-type="http://credit.niso.org/contributor-roles/conceptualization/">Conceptualization</role>
    <role content-type="http://credit.niso.org/contributor-roles/data-curation/">Data curation</role>
    <role content-type="http://credit.niso.org/contributor-roles/writing-original-draft/">Writing – original draft</role>
  </contrib>
</contrib-group>
```

Sem taxonomia (texto livre, sem `@content-type`):

```xml
<role>conception</role>
<role>methodology</role>
<role>revision</role>
```

## Pareceres (revisão por pares aberta)

Em pareceres marcados como `<article>`/`<sub-article>`, `<role>` em
`<contrib-group>` leva `@specific-use` obrigatório:

| Valor | Significado |
|---|---|
| `reviewer` | Revisor/Parecerista |
| `editor` | Editor |

```xml
<role specific-use="reviewer">Parecerista</role>
```

## Fase 3 — derivação e injeção

| O quê | Determinístico? | Fonte |
|---|---|---|
| **Papéis por autor** | Parcial | seção "Author contributions" / "Contribuições" do documento, quando existe |
| **Mapeamento termo → URL CRediT** | Sim | tabela acima (lookup exato pelo termo) |
| **Posição no XML** | Sim | dentro do `<contrib>` correspondente, após `<xref>` |

> ⚠️ A associação papel↔autor depende de o documento declarar
> contribuições de forma estruturada; o mapeamento termo→URL é
> determinístico, mas reconhecer/normalizar o termo escrito pelo autor
> (sinônimos, idioma) é a parte de julgamento. Não havia implementação
> no projeto MathML antigo.
