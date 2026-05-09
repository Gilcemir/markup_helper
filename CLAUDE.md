## Antes de implementar regra que insere tag SciELO

**Sempre leia `docs/scielo_context/README.md`** primeiro. Ele tem o
roteamento por tarefa (qual arquivo de contexto consultar para qual
decisão) e os 5 invariantes que **não podem ser violados**.

Em particular:
- Atributos obrigatórios e ordem dos filhos: `docs/scielo_context/DTD_SCHEMA.md`.
- Pais permitidos para cada tag (validação hierárquica):
  `docs/scielo_context/HIERARCHY.md`.
- O que NÃO pré-marcar (auto-marks da SciELO duplicam):
  `docs/scielo_context/REENTRANCE.md`.

## Convenções do projeto

- Linguagem: C# / .NET 10.
- Estilo de código: vide `Directory.Build.props` e a estrutura existente
  em `DocFormatter.Core/Rules/`.
- Testes em `DocFormatter.Tests/`. Toda regra nova precisa de teste.
- Output do CLI: vide `README.md` (raiz) — gera `formatted/`,
  `.report.txt`, `.diagnostic.json` ao lado do input.

## Promovendo decisões ao concluir feature

Quando uma feature em `.compozy/tasks/<name>/` está implementada,
mergeada e estável, rode `/promote-feature <name>` para mover ADRs
para `docs/decisions/<name>/` e atualizar `docs/INVARIANTS.md`.
Decisões em `docs/decisions/` e regras em `docs/INVARIANTS.md` são
source of truth — leia antes de propor mudanças que toquem áreas já
decididas.
