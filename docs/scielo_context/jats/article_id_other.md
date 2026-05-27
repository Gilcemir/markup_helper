# `<article-id pub-id-type="other">` — número Other

> **Fase 3 (pós-Markup, JATS XML).** Curado de `_raw/SPS 1.10_pt.md`
> linhas 2375–2430. Fonte raw para detalhes ausentes: SPS 1.10 nessas
> linhas (ver [[README]]).

## O que é

`<article-id>` aparece em `<article-meta>` (e em `<front-stub>` de
sub-artigo) uma ou mais vezes. Atributo obrigatório `@pub-id-type`:

| Valor | Significado |
|---|---|
| `doi` | Digital Object Identifier. |
| `other` | Numeração sequencial de **5 dígitos** que ordena documentos na modalidade de Publicação Contínua (PC) no sumário online. |

Esta extração cobre **apenas `other`** (o `doi` já é tratado pelo Markup).

## Quando `other` é obrigatório

1. Periódicos em **Publicação Contínua (PC)** com paginação digital
   (`elocation-id`).
2. Periódicos em modalidade **regular** quando o fascículo tem:
   - paginação não-arábica (ex.: romana), ou
   - paginação não sequencial reiniciada por documento, ou
   - paginações sobrepostas entre documentos.

   Nesses casos, **todos** os documentos do fascículo recebem `other` de
   5 dígitos, iniciando em `00001`, na ordem do sumário.

Formato: sempre **5 dígitos**, ex.: `00603`.

## Exemplo XML

```xml
<article-meta>
  <article-id pub-id-type="doi">10.1590/S2237-96222023000200017</article-id>
  <article-id pub-id-type="other">00603</article-id>
</article-meta>
```

## Fase 3 — derivação e injeção

| O quê | Determinístico? | Fonte |
|---|---|---|
| **Valor do `other`** | Sim, dado o txt | **arquivo `other.txt`** (input externo) |
| **Posição no XML** | Sim | logo **após** o `<article-id pub-id-type="doi">` |

> ⚠️ O número `other` **não é derivável do documento** — é atribuído
> externamente pela SciELO via "Planilha de Other: Controle de Seções"
> (SPS linha ~1028). No projeto MathML antigo isso era digitado na UI
> (a fricção que levou ao abandono). Na fase 3, a fonte é um TXT.

### Formato do input `other.txt`

TSV `<nome-do-arquivo-pdf>\t<other-5-dígitos>`. O basename do PDF casa
com o basename do XML do pacote. Exemplo (`examples/phase-3/other.txt`):

```
1984-7033-cbab-26-02-e54492621.pdf	00201
1984-7033-cbab-26-02-e54242622.pdf	00202
…
```

Lookup: basename do XML → linha do txt → valor `other`.

### Regra de placement (determinística, validada no MathML)

1. Localizar `//article-id[@pub-id-type='doi']`.
2. **Falhar** se não houver DOI (não criar `other` solto).
3. Inserir `<article-id pub-id-type="other">VALOR</article-id>`
   **imediatamente após** o elemento do DOI, herdando o namespace dele.
