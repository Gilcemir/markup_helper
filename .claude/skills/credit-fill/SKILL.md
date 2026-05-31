---
name: credit-fill
description: Audita XMLs JATS contra DOCXs SciELO e preenche <role> CRediT em <contrib-group> usando julgamento da LLM nos casos prose/ambíguos que o pipeline determinístico (DocFormatter.Core.Jats.CreditRolesInjector) não resolve. Espelha ADR-001/005/007 mas com LLM como gatekeeper de confiança. Throwaway para a edição intermediária cbab-26-02; a próxima edição vai normalizar o input no SciELO Markup e dispensar esta skill. Não use para outras edições sem revisar premissas (ADRs em docs/decisions/phase-3-jats-tags/).
---

# credit-fill — CRediT injection via LLM judgment

Skill **throwaway** para a edição cbab-26-02. Preenche `<role>` CRediT em XMLs onde o pipeline determinístico do CLI não conseguiu — basicamente todos os casos prose, frases compostas com termos custom, e iniciais portuguesas com suffix.

## Pré-condições

Antes de invocar, verifique:

1. **`python3` disponível** (stdlib só — sem deps).
2. **`extract.py` em** `.claude/skills/credit-fill/extract.py`.
3. **Tabela CRediT canonical** em `docs/scielo_context/jats/credit_roles.md`.
4. **Git limpo nos XMLs alvo**: rode `git status --porcelain <xml-dir>`. Se aparecer qualquer modificação não-commitada nos XMLs alvo, **aborte** com mensagem: *"Commit/stash mudanças nos XMLs antes de invocar credit-fill."*

## Invocação

`/credit-fill` (defaults: `examples/phase-3/expected/` + `examples/phase-3/scielo_markup/`).

Args opcionais:

- `--xml-dir <DIR>`: estado pós-CLI dos XMLs.
- `--docx-dir <DIR>`: DOCXs SciELO Markup pareados.
- `--only <ID>`: processa só um artigo (`--only e51362627` ou `--only 5293`).

Pareamento: últimos 4 dígitos do `e\d+` no nome do XML batem com o nome do DOCX (ADR-004).

## Procedimento

### Fase 0 — Sanity checks

Verifique as pré-condições acima. Aborte cedo se algo falhar.

### Fase 1 — Audit determinístico

Rode:

```bash
python3 .claude/skills/credit-fill/extract.py audit \
  --xml-dir <xml-dir> --docx-dir <docx-dir> [--only <ID>]
```

Stdout é JSON. Estrutura por artigo:

- `elocation_id`, `xml_path`, `docx_path`
- `section`: `{heading_matched, section}` ou `null`
- `contrib_group`: `{block, authors[]}` onde cada autor tem `surname`, `given_names`, `suffix`, `orcid`, `current_roles[]` com `{content_type, label, slug}`
- `structured`: tentativa de parse (`shape`, `role_keyed`, `author_keyed`, `orphans`)
- `docx_assignment`: `{idx → [slugs]}` quando o parse estruturado teve sucesso
- `extras`, `missing`: comparação contra XML quando parse foi bem
- `classification`: `no-op-no-section` | `no-op-match` | `needs-judgment` | `error`
- `reason`: por que caiu em `needs-judgment`/`error`

### Fase 2 — Plan-then-confirm

Apresente plano global ao operador. Formato sugerido:

```
PLAN — credit-fill <N> articles

  e51362627  no-op-match            17/17 authors verified deterministically
  e52932623  needs-judgment         prose-style; LLM judgment
  e53132629  needs-judgment         role-keyed, names with commas
  e54192624  needs-judgment         6 authors missing roles (deterministic seed available)
  ...

  Total: 15
  - <K> no-op-match     (skip, no LLM, no edit)
  - <K> no-op-no-section (skip with flag)
  - <K> needs-judgment  (process via LLM; possible operator prompts)
```

Use `AskUserQuestion`:

- Pergunta: "Prosseguir com este plano?"
- Opções: "Sim, processar" / "Mostrar detalhes de um artigo" / "Abortar"

Se "Mostrar detalhes", peça qual elocation-id e mostre o JSON parcial daquele artigo (section text, contrib-group authors, structured parse). Volte ao plano.

Se "Abortar", saia sem editar nada.

### Fase 3 — Processamento per-article

Para cada artigo `needs-judgment` (em ordem do JSON):

#### 3.1 Carregar contexto

Do JSON: `section.section`, `contrib_group.authors`, `contrib_group.block`, `structured`, `docx_assignment` (se houver).

#### 3.2 Raciocinar sobre o mapeamento

Você (agente) propõe `proposed_assignment: {author_idx → [slugs]}`.

**Se `docx_assignment` já está preenchido** (parse determinístico parcial funcionou): use-o como ponto de partida. Ajuste apenas onde guards 1-5 indicarem problema.

**Caso contrário** (prose ou shape misto): leia a `section.section` e a lista de autores, e raciocine:

1. **Identifique autores mencionados**. Em prose, podem ser:
   - iniciais agrupadas: `ATAJ`, `DRSJ`, `KB Viandro`
   - nome parcial: `Andrey Luis Bruyns de Sousa`
   - sobrenome só: `Roza`
   - coletivo: `Both authors`, `All authors`, `The authors`
2. **Atenção a suffixes portugueses**: `Neto`, `Júnior`, `Filho` aparecem no XML como `<suffix>`, **não** `<surname>`. Iniciais como `Neto VBP` exigem incluir o suffix na resolução.
3. **Identifique termos**. Em prose, mapeie verbos cuidadosamente:
   - "conceived" → Conceptualization
   - "designed" → (sozinho não basta; "designed the study" → Methodology)
   - "wrote the manuscript" → Writing – original draft
   - "revised" → Writing – review & editing
   - "analyzed the data" → Formal analysis
   - "collected" → Data curation **ou** Investigation (ambíguo — flag pro operador)
   - "supervised" → Supervision
   - "supported" / "helped" → **não mapeia** (vague)
4. **Frases compostas com "and"**: split. `"Methodology and Molecular data analysis"` → `["Methodology" (CRediT)`, `"Molecular data analysis" (custom)]`.
5. **Coletivos** ("All authors X"): aplica X a **todos** os autores no `contrib_group`.

#### 3.3 Aplicar os 5 guards

Para cada `(author_idx, slug)` proposto:

1. **G1 — termo ∈ CRediT.** Use `credit_slugs` do JSON (e.g., `"conceptualization"` ∈ keys). Se a LLM propôs um termo que não tem slug correspondente, marque como **custom** (não rejeita — vai pro fluxo free-text).
2. **G2 — autor existe.** `author_idx ∈ range(len(authors))`. Trivial.
3. **G3 — texto-fonte aparece no DOCX.** Verifique que tanto o nome/iniciais do autor (ou referência coletiva) **quanto** o termo (raw ou normalizado) aparecem na `section.section`. Substring case-insensitive accent-stripped. Se falhar, **adicione à lista `verification_failures`** — não escreve aquela atribuição, vai pro 3.4 como prompt explícito.
4. **G4 — uniformidade @content-type per documento.** Se **qualquer** termo do documento é custom (G1 falhou), o documento inteiro vira `free-text` mode (sem `@content-type` em **nenhuma** role) — decisão do operador na 3.4.
5. **G5 — sem duplicatas.** Mesmo slug não aparece 2x no mesmo autor.

#### 3.4 Apresentar ao operador

Monte uma mensagem mostrando:

- **Section bruta** (citação literal).
- **Autores do `contrib_group`** numerados, com `<surname>`, `<given-names>`, `<suffix>`.
- **Mapeamento proposto** (autor X → [roles canonicalizadas]).
- **Termos custom detectados** (se houver), cada um com interpretação proposta:
  - typo (e.g., `"Writing and Review"` → `"Writing – review & editing"`)
  - custom genuíno (e.g., `"Molecular data analysis"` — fica como free-text)
- **Autores sem atribuição** (se houver) — operador decide se omite ou intervém.
- **Verification failures** (G3 falhou em X).
- **Aviso de free-text mode** se aplicável: *"Aceitar `<termo custom>` força o documento inteiro para free-text. Isso remove `@content-type` de **todas** as roles, inclusive das já matched-CRediT."*

Em seguida invoque `AskUserQuestion`:

- Pergunta: "Como aplicar este artigo?"
- Header: "credit-fill: <elocation_id>"
- Opções (até 4):
  - **"Aceitar mapeamento proposto"**
  - **"Corrigir uma atribuição"**
  - **"Ajustar typo vs custom"** (só se houver termos custom)
  - **"Pular este artigo"**

**Se "Aceitar"** → vai pra 3.5.

**Se "Corrigir"** → segunda interação (texto livre via "Other" no AskUserQuestion ou pergunta nova): peça override específico ("ATAJ deve ter qual conjunto de roles?"). Itere 3.3+3.4 até ok.

**Se "Ajustar typo vs custom"** → AskUserQuestion por termo: opções `"Tratar como typo de <X>"` / `"Manter como custom term"`. Aplique decisão, itere 3.3+3.4.

**Se "Pular"** → log no report, próximo artigo.

#### 3.5 Escrever o XML

1. **Construa novo `<contrib-group>` block**:
   - Mantenha cada `<contrib>` literal (contrib-id, name, xref) com indentação tab original.
   - Após o último `<xref>` em cada `<contrib>` (ou após o último child antes de `</contrib>` se sem xref), insira uma `<role>` por slug aprovado, indentado com 5 tabs:
     - CRediT mode: `<role content-type="{credit_url_base}{slug}/">{credit_slugs[slug]}</role>`
     - Free-text mode: `<role>{credit_slugs[slug] ou texto custom original}</role>` (sem `content-type`)
   - **Free-text mode também reescreve roles pré-existentes** que tinham `@content-type`, removendo-o.
2. **Edite o XML** usando a tool `Edit`:
   - `old_string = contrib_group.block` (do JSON, único no arquivo por construção).
   - `new_string =` block reconstruído.
3. **Apend imediato ao report** (Fase 4 abaixo).
4. Próximo artigo.

### Fase 4 — Report final

Escreva `<report-dir>/run-report.md`. Default `report-dir`: `.compozy/tasks/credit-fill-edition-<derived>/` onde `<derived>` é, por exemplo, `cbab-26-02` se o XML tem esse padrão no filename. Crie o diretório se não existir.

Estrutura:

```markdown
# credit-fill run report

- **Timestamp**: <ISO 8601>
- **XML dir**: <abs path>
- **DOCX dir**: <abs path>
- **Articles**: <N>

## Summary

| Outcome | Count |
|---|---|
| no-op-match (deterministic) | K |
| no-op-no-section            | K |
| operator-confirmed CRediT   | K |
| operator-confirmed free-text| K |
| operator-corrected          | K |
| skipped                     | K |
| flagged: extras in XML      | K |
| errors                      | K |

## Per-article

### e<ID> — <outcome>

- **Section**: > <literal citation>
- **Mapping applied**: autor → [roles]
- **Free-text mode**: yes/no
- **Warnings**: <verification_failures, extras, suffix-resolved, ...>

<details><summary>Reasoning context</summary>

Inputs that drove the mapping:
- structured shape detected: <shape>
- docx_assignment (deterministic seed): <if any>
- guards triggered: <list>

</details>
```

**Sem `git add`/`git commit`** — o operador commita à mão depois de revisar.

## Edge cases & gotchas

- **Section heading não bate**: audit retorna `no-op-no-section` mesmo se DOCX tem section. Sintoma: artigo silently skipped. Mitigação: revise `SECTION_HEADINGS` em `extract.py` se report mostrar zero auto-confirmed em DOCXs que você sabe ter section.
- **Suffix português** (`Neto`, `Júnior`, `Filho`): aparece em `<suffix>`. Audit já trata. LLM deve sempre considerar suffix ao resolver iniciais.
- **Coletivos** (`Both authors`, `All authors`, `The authors`): Guard 3 pode falhar nominalmente porque nome do autor não aparece. Tratar coletivos como *match para qualquer autor mencionado coletivamente* — G3 considera "presente" se houver o coletivo na section.
- **Frases compostas**: split em `and`/`&`/`,` antes de mapear. Se uma das partes resulta em custom term, documento inteiro vai pra free-text.
- **Iniciais ambíguas** (e.g., `TOS` bate com dois autores): audit retorna `needs-judgment` com candidate count >1. Operador resolve.
- **DOCX com texto kerned** (e.g., `"C onceived"`): LLM normaliza naturalmente. Não tenta arrumar via regex.
- **Roles extras no XML** (não atribuídas pelo DOCX): apenas **flag no report**. **Nunca remove**, a menos que entre em free-text mode (onde a remoção do `@content-type` é parte da uniformização, mas o role text continua).

## Não-objetivos

- **Não toca outros tags JATS** (`<article-id>`, `<fn>`, `<sec>`, etc.). Esses são responsabilidade do CLI Phase 3.
- **Não faz validação SPS completa**. Assume XML upstream bem-formado.
- **Não commita automaticamente**. Operador revisa `git diff` e commita.
- **Não remove `<role>` existentes** exceto pra reescrita uniforme em free-text mode.
- **Não substitui o pipeline determinístico** — é fallback de LLM para os casos onde aquele explicitamente delega ao operador (ADR-005/007).

## Referências

- `docs/scielo_context/jats/credit_roles.md` — tabela CRediT canonical.
- `docs/decisions/phase-3-jats-tags/adr-001.md` — confidence gate.
- `docs/decisions/phase-3-jats-tags/adr-005.md` — CRediT auto-mapping só em estruturados.
- `docs/decisions/phase-3-jats-tags/adr-007.md` — free-text disposition.
- `DocFormatter.Core/Jats/CreditRolesInjector.cs` — pipeline determinístico equivalente (Windows-only).
