---
name: promote-feature
description: Encerra uma feature do `.compozy/tasks/<feature>/`. Promove ADRs para `docs/decisions/<feature>/`, popula `docs/INVARIANTS.md` com invariantes declarados, atualiza o índice cross-feature em `docs/decisions/README.md` e **apaga o diretório transitório inteiro da feature** (`_prd.md`, `_techspec.md`, `task_*.md`, `memory/`, `reviews-*/`, `adrs/`). Apenas decisões, invariantes e índice ficam preservados; PRDs, TechSpecs, task specs e memory são considerados obsoletos pós-implementação. Use quando o usuário rodar `/promote-feature <name>` ou pedir para encerrar/promover uma feature concluída.
---

# Promote Feature Decisions

Encerra uma feature concluída: extrai conhecimento durável (ADRs +
invariantes), persiste em `docs/decisions/<feature>/` e em
`docs/INVARIANTS.md`, atualiza o índice cross-feature, e **remove
todo o diretório transitório `.compozy/tasks/<feature>/`** — incluindo
PRD, TechSpec, task specs, memory e reviews.

**Filosofia**: o código é a fonte de verdade do *como*; ADRs e
invariantes são a fonte de verdade do *por que / regras invioláveis*.
PRDs/TechSpecs/tasks/memory documentam o processo de implementação,
que deixa de ser útil pós-merge — git history preserva o que precisar.

## Quando usar

Usuário rodou `/promote-feature <feature-name>` ou pediu equivalente
(*"promove as decisões da feature X"*, *"mova as ADRs da feature X
para docs"*). Não rode sem nome explícito de feature.

## Pré-condições (validar antes de qualquer mudança)

1. `.compozy/tasks/<feature>/adrs/` existe e tem ≥1 arquivo `.md`. Se
   não, aborte com mensagem clara.
2. `docs/decisions/<feature>/` **não** existe. Se existir, aborte:
   *"feature já promovida — veja docs/decisions/<feature>/"*. Isso é
   non-overwrite idempotency intencional.
3. `docs/decisions/README.md` e `docs/INVARIANTS.md` existem. Se não,
   aborte e peça ao usuário para bootstrap.
4. Working tree limpo nos paths que serão tocados. Se houver mudanças
   pendentes em `.compozy/tasks/<feature>/adrs/` ou em `docs/`, avise
   e pergunte se prossegue.

## Passos

### 1. Copiar ADRs

Para cada `*.md` em `.compozy/tasks/<feature>/adrs/`, copiar para
`docs/decisions/<feature>/<mesmo-nome>` preservando conteúdo
byte-by-byte. Não reformate. Não reescreva cross-references entre
ADRs (paths podem ficar stale, mas reescrever corretamente é
arriscado — o usuário corrige no review).

### 2. Extrair invariantes → `docs/INVARIANTS.md`

Rode `grep -rnE 'INV-[0-9]+' docs/decisions/<feature>/` para localizar
declarações de invariantes.

Para cada invariante:

- Capture `INV-NN` (o número é a chave).
- Extraia título da linha (texto após `—` ou `-` separador).
- Extraia descrição de 1–2 linhas das linhas adjacentes.
- Leia `docs/INVARIANTS.md`. Se `## INV-NN` já existe, **skip** e
  registre no resumo final (anote divergência se o texto diferir do
  existente — não sobrescreva).
- Caso contrário, append uma seção:

  ```
  ## INV-NN — <título>
  <descrição>
  Fonte: docs/decisions/<feature>/<adr-file>.md
  ```

  Mantenha ordem numérica ascendente no arquivo.

### 3. Gerar `docs/decisions/<feature>/README.md`

- **Título**: humanize o nome da feature (ex.:
  `header-metadata-extraction` → `Header Metadata Extraction`).
- **Sumário**: 2–3 linhas extraídas do primeiro parágrafo não-heading
  de `.compozy/tasks/<feature>/_prd.md`. Se `_prd.md` não existe, use
  placeholder: *"(Resumo a preencher manualmente.)"*
- **Lista de ADRs**: 1 linha por ADR com o título (1ª linha `#` do
  arquivo). Formato: `- [adr-NNN-name](adr-NNN-name.md) — <título>`
- **Se invariantes adicionados**: 1 linha linkando para
  `docs/INVARIANTS.md` listando os números `INV-NN` contribuídos por
  esta feature.

### 4. Classificar ADRs por domínio

Para cada ADR copiada, proponha 1 dos 4 domínios canônicos:

- **architecture** — pipeline, layout de projetos, frameworks/runtime,
  decisões estruturais cross-cutting.
- **parsing** — extração de texto, tokenização, regex/heurísticas de
  detecção de campos (autores, ORCID, e-mail, anchors).
- **formatting** — manipulação OOXML, regras de marcação, transformação
  de conteúdo (bold cascade, section promotion, italic preservation).
- **tooling** — superfície CLI, diagnostic output, build/CI, layout de
  arquivos de saída.

Use o título da ADR e o primeiro parágrafo como input para a
heurística. Quando ambíguo, escolha o domínio que melhor reflete o
*efeito primário* da decisão.

Apresente a classificação ao usuário em tabela compacta:

```
ADR                                      | Domínio sugerido
-----------------------------------------|------------------
adr-001-mvp-pipeline.md                  | architecture
adr-002-three-projects-layout.md         | architecture
adr-003-orcid-extraction.md              | parsing
...
```

Pergunte: *"Confirmar todas? (`ok` para aprovar, ou liste overrides
como `adr-003=tooling`)"*. Aplique overrides ao mapa de classificação.

### 5. Atualizar `docs/decisions/README.md`

Formato de entry no índice por domínio (curado, otimizado para LLM
consumir e decidir se abre o arquivo):

```
- **<assunto curto>** — <decisão em 1 frase, com símbolos de código
  inline quando relevante (ex.: `<w:b>`, `MoveHistoryRule`)>; cross-refs
  inline (ex.: "supersede X", "refator de Y", "INV-NN"). → <feature>/<adr-file>
```

Não cole o título da ADR cru. Leia o conteúdo do ADR (Decision +
Context) e sintetize:

- **Assunto** = o sujeito da decisão (greppable). Bold.
- **Frase** = o *que* foi escolhido, não o que se discutiu. ≤25
  palavras. Inclua símbolos de código (regras, tags OOXML, paths) que
  ajudam grep.
- **Cross-refs** inline quando aplicável (supersession, refator,
  invariante).

Para cada ADR:

- Adicione 1 entry à seção do domínio confirmado, em **ordem
  alfabética por assunto** (não por título da ADR).
- Adicione 1 linha à seção *Índice por feature* no formato:
  `- [<feature>/](<feature>/) — N ADRs — <descrição curta da fase>`.
- Remova o marcador `*(vazio até primeira promoção)*` da seção que
  recebeu entradas.

### 6. Remover diretório transitório da feature

Rode: `git rm -r .compozy/tasks/<feature>/`

Apaga tudo: `_prd.md`, `_techspec.md`, `_tasks.md`, `task_*.md`,
`memory/`, `reviews-*/`, e qualquer outro artefato. As ADRs já foram
copiadas no passo 1 e o sumário já foi extraído do `_prd.md` no passo
3 — é seguro deletar agora.

Use `git rm` (não `rm`) para que a remoção fique staged. Não commite.

**Atenção à ordem**: este passo vem depois do passo 3 (que lê
`_prd.md`) e do passo 4 (que classifica ADRs lendo seus conteúdos
copiados em `docs/decisions/<feature>/`). Não inverta a ordem — se
deletar antes, perde input para o sumário e para a classificação.

### 7. Resumo final

Imprima ao usuário, em formato curto:

- Arquivos copiados: contagem.
- Classificação por domínio aplicada (lista final).
- Invariantes adicionados: lista de `INV-NN` + título.
- Invariantes pulados: lista com motivo (já presente, ou divergência
  textual — sinalizar para reconciliação manual).
- Caminhos a revisar antes de commitar:
  - `docs/decisions/<feature>/` (árvore nova)
  - `docs/decisions/README.md` (índices atualizados)
  - `docs/INVARIANTS.md` (se mudou)
  - `git status` (confirmar que `.compozy/tasks/<feature>/` inteiro
    está staged como deleted, incluindo `_prd.md`, `_techspec.md`,
    `_tasks.md`, `task_*.md`, `memory/` e `reviews-*/`)
- Lembrete: *"Não commitei. Revise e crie o commit você mesmo. O
  diretório transitório foi apagado — git history é o backup."*

## Restrições

- **Idioma dos artefatos gerados**: todo conteúdo que esta skill
  escreve em `docs/decisions/` (READMEs por feature, entries no índice
  cross-feature) e em `docs/INVARIANTS.md` (entries `INV-NN`) deve ser
  redigido em **inglês**. ADRs já são em inglês na maioria; preserve
  byte-by-byte e não traduza ao copiar. `CLAUDE.md` permanece em
  português; não toque o idioma dele.
- **Não** reescreva cross-references entre ADRs durante a cópia.
- **Não** modifique ADRs além do README de feature que você gera.
- **Não** preserve PRD, TechSpec, task specs, memory ou reviews em
  `docs/`. Eles vão para o lixo via `git rm` no passo 6 — o histórico
  de git é o backup. Resista à tentação de "arquivar por garantia":
  cada arquivo preservado é ruído futuro.
- **Não** delete arquivos fora de `.compozy/tasks/<feature>/`. Em
  particular, não toque outras features em `.compozy/tasks/` nem o
  diretório `.compozy/` em si.
- **Não** commite. Sempre deixe as mudanças staged para o usuário.
- **Não** retente automaticamente em caso de conflito — exponha o
  conflito e deixe o usuário decidir.

## Edge cases

- ADR sem invariante: ok, passo 2 é no-op para essa ADR.
- Duas ADRs declaram o mesmo `INV-NN`: mantenha a primeira encontrada,
  skip a segunda, sinalize no resumo.
- `_prd.md` ausente: README de feature usa placeholder; não falhe.
- Invariante declarado inline sem separador `—` (ex.: `INV-05`
  seguido só de descrição): trate a próxima linha não-vazia como
  descrição, título vira `INV-05`. Não tente ser esperto.
- Domínio ambíguo: na proposta inicial, escolha o melhor candidato e
  marque `(ambíguo)` ao lado do nome para o usuário decidir.
