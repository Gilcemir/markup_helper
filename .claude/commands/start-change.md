---
description: Inicia uma nova feature. Cria branch a partir de `main` + pasta `.compozy/tasks/<slug>/`. Tier pequena pula PRD; grande começa por PRD.
---

Você é o assistente que abre uma nova feature neste projeto. O dev fornece
0–1 input e você infere o resto, confirmando uma única vez com opção de
override.

<critical>NÃO faça wizard interativo com múltiplos steps. O fluxo inteiro
tem NO MÁXIMO 2 interações: (1) input inicial se não fornecido como
argumento, (2) confirmation com overrides.</critical>

## Contexto deste projeto

- Sem Jira, sem Notion, sem `.active-changes.json`. A "feature ativa" é a
  branch atual + a pasta correspondente em `.compozy/tasks/`.
- Branch principal: `main`. Não existe `develop`.
- Fluxo de feature usa **compozy** (skills `cy-*`), não os `gil-*` globais.
  Pasta de trabalho: `.compozy/tasks/<slug>/` com `_prd.md`, `_techspec.md`,
  `task_*.md`, `memory/`, `adrs/`. No fim, `/promote-feature <slug>` extrai
  ADRs/invariantes para `docs/decisions/` e apaga o transitório.

## Tiers (apenas dois)

| Tier      | Artefatos                                   | Quando usar |
|-----------|---------------------------------------------|-------------|
| `grande`  | PRD → TechSpec → Tasks → Execute → ADRs     | Feature com escopo amplo, decisões de produto/arquitetura, mudança que merece um PRD para alinhar antes de implementar. |
| `pequena` | TechSpec → Tasks → Execute → ADRs (pula PRD) | Bugfix, ajuste localizado, refatoração contida, melhoria pontual. O TechSpec serve de plano enxuto. |

Não invente tiers intermediários. Se ficar em dúvida, default = `grande`
e ofereça override no confirmation.

## Argumentos aceitos

- `/start-change` (sem args) → você pergunta uma única vez "O que vai fazer?"
- `/start-change <descrição livre>` → você infere tudo do texto

Sem suporte a IDs externos (Jira, Linear, etc) — não tente parsear.

## Fluxo

### Fase 1 — Capturar input

Se não houver argumento, use `AskUserQuestion`:

```
Header: "Nova feature"
Pergunta: "O que você vai fazer?"
Opções: Other → texto livre
```

Caso contrário, todo o argumento é a descrição.

### Fase 2 — Inferir tudo (silenciosamente)

Execute todos os passos abaixo sem perguntar.

#### 2.1 Gerar slug

A partir da descrição:

- Pegue 3–5 palavras significativas (descarte artigos/preposições curtas em
  pt e en: a, o, de, da, do, para, the, a, of, to, for, in, on).
- Lowercase + kebab-case.
- Sem acentos (translitere quando necessário).
- Exemplo: "Adicionar suporte a footnotes" → `adicionar-suporte-footnotes`.
- Exemplo: "fix bold cascade for headings" → `fix-bold-cascade-headings`.

#### 2.2 Inferir tier (heurística)

Default = `grande`. Marque `pequena` se a descrição começa com / contém
de forma proeminente uma destas palavras-gatilho:

- `fix`, `bug`, `hotfix`, `bugfix`
- `ajuste`, `ajustar`, `tweak`, `polish`
- `pequeno`, `pequena`, `small`, `tiny`
- `rename`, `renomear`
- `typo`, `corrigir typo`

Caso contrário → `grande`. O dev pode trocar no confirmation.

#### 2.3 Inferir prefixo de branch

- Se tier = `pequena` E gatilho foi `fix`/`bug`/`hotfix`/`bugfix` → `fix/`
- Senão se tier = `pequena` → `chore/`
- Senão (tier = `grande`) → `feat/`

Branch final: `<prefixo><slug>` (ex.: `feat/adicionar-suporte-footnotes`,
`fix/bold-cascade-headings`).

#### 2.4 Validar estado do repo

- `git rev-parse --show-toplevel` deve apontar para a raiz do projeto.
- `git branch --show-current` deve retornar `main`. Se for outra branch,
  **não bloqueie**: sinalize no confirmation que a feature será criada a
  partir da branch atual (não de `main`) e dê opção de cancelar/trocar.
- `git status --porcelain`: se sujo, sinalize no confirmation
  (`working tree sujo — git checkout vai carregar mudanças junto`). Não
  bloqueie; o dev decide.
- `git ls-remote --heads origin <branch-alvo>` e `git branch --list <branch-alvo>`:
  se a branch já existe localmente OU no remoto, sinalize no confirmation
  e ofereça (a) usar a existente via checkout, (b) escolher outro slug.

#### 2.5 Determinar próximo skill sugerido

- `grande` → `/cy-create-prd`
- `pequena` → `/cy-create-techspec`

### Fase 3 — Confirmation (uma única interação)

Use `AskUserQuestion`. Monte o resumo com tudo inferido. Exemplo:

```
Header: "Confirmar feature"
Pergunta:
"Feature: <slug>
 Tier: <tier>  (inferido por <gatilho ou 'default'>)
 Branch: <prefixo><slug>  (criada a partir de <main | branch atual>)
 Pasta: .compozy/tasks/<slug>/
 Próximo skill: <cy-create-prd | cy-create-techspec>

 <linha extra opcional: 'working tree sujo — checkout vai carregar mudanças'>
 <linha extra opcional: 'branch <X> já existe — confirme reuso ou troque o slug'>

 Confirmar?"

Opções:
  [1] Confirmar  (Recommended)
  [2] Mudar tier
  [3] Mudar slug
  [4] Mudar prefixo da branch (feat/fix/chore)
  [5] Cancelar
```

Comportamento dos overrides:

- `[2]`: pergunte tier (`grande` | `pequena`) e reaplique 2.3 + 2.5; mostre
  o novo resumo.
- `[3]`: pergunte o slug em texto livre; revalide 2.4 (colisão).
- `[4]`: pergunte prefixo (`feat/` | `fix/` | `chore/`); reaplique e
  reconfirme.
- `[5]`: aborte sem efeitos colaterais (não criou branch nem pasta ainda).

Se algum override for aplicado, repita o resumo e peça confirmação
novamente — **mas não adicione novas perguntas além do override**. Cada
ciclo extra é uma única `AskUserQuestion`.

### Fase 4 — Executar (sem mais perguntas)

Após confirmação:

#### 4.1 Branch

- Branch alvo já existe localmente: `git checkout <branch>`
- Branch alvo só existe no remoto: `git fetch origin <branch>:<branch>` →
  `git checkout <branch>`
- Branch alvo não existe em lugar nenhum:
  - Se branch atual é `main`: `git checkout -b <branch>`
  - Se branch atual é outra (e o dev confirmou criar a partir dela):
    `git checkout -b <branch>` (parte da branch atual; explicite isso no
    resumo final)

#### 4.2 Criar pasta da feature

`mkdir -p .compozy/tasks/<slug>/`

Não crie arquivos seed dentro. Os skills `cy-create-prd` /
`cy-create-techspec` materializam `_prd.md` / `_techspec.md`.

#### 4.3 Não comite

A criação da pasta vazia (sem arquivos) é invisível pro git. A criação
da branch já está aplicada (é estado do repo, não do índice). Nada a
commitar agora — o primeiro commit virá quando o cy-create-* gravar o
PRD/TechSpec.

### Fase 5 — Resumo final

Imprima, em formato curto:

```
Feature iniciada: <slug>
  Tier: <tier>
  Branch: <branch>  (criada a partir de <main | branch-origem>)
  Pasta: .compozy/tasks/<slug>/

Próximo passo:
  → /<cy-create-prd | cy-create-techspec>  (use /grill-me em paralelo
    para fechar gaps no documento à medida que ele é construído)
  → depois, /cy-create-tasks → /cy-execute-task (loop)
  → ao concluir, /promote-feature <slug>
```

Se a branch já existia ou o working tree estava sujo, repita a observação
correspondente como linha extra no fim do resumo.

## Restrições

- **Sem Jira, sem `.active-changes.json`, sem worktrees, sem
  jira-snapshot.md.** A branch atual + a pasta `.compozy/tasks/<slug>/`
  já carregam todo o estado de "feature ativa".
- **Sem commits automáticos.** Esta skill não comita. Não rode
  `git add`, `git commit`, `git push`.
- **Não modifique `main`.** A branch sempre é nova (ou existente, com
  consentimento explícito) — nunca commit direto em `main`.
- **Não invente tiers** além de `grande`/`pequena`. Não invente
  prefixos além de `feat/`/`fix/`/`chore/`.
- **Não crie arquivos** dentro de `.compozy/tasks/<slug>/` além da
  pasta em si. Os skills cy-create-* fazem isso.
- **Idioma**: o resumo final e a confirmation podem ser em pt-BR (este
  comando é do projeto). Slugs sempre em inglês ou pt-BR sem acentos,
  consistente com a descrição que o dev forneceu.

## Edge cases

- **Slug vazio depois do filtro de stop-words** (ex.: input "fix"
  sozinho): use o input cru kebab-cased como fallback (`fix`). Se ainda
  ficar vazio, peça ao dev um slug explícito antes do confirmation.
- **Branch atual já é uma feature branch existente** (não-`main`,
  não-`master`): default vira "criar nova feature a partir desta
  branch"; sinalize no confirmation. Se o dev preferir partir de `main`,
  ele cancela, faz `git checkout main`, e roda de novo.
- **Repositório com submodule ou worktree extra**: trate como repo
  comum; não invoque `git worktree`.
- **Slug colide com pasta `.compozy/tasks/<slug>/` existente**:
  sinalize no confirmation. Default = pedir novo slug; segunda opção =
  reusar (entrar na feature existente, sem recriar nada além do
  checkout da branch).
