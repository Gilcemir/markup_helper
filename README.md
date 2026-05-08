# DocFormatter

CLI em .NET 10 que normaliza artigos científicos em `.docx` segundo regras editoriais fixas — extrai DOI/elocation/autores/afiliações/abstract/keywords da folha-rosto crua, reescreve o cabeçalho no formato de saída, promove níveis de seção e remove hyperlinks descartáveis (incluindo ORCID).

Desenvolvido no macOS, executado no Windows 10 como `docformatter.exe` self-contained.

---

## Como usar (Windows)

### Instalação (uma vez)

Abra um PowerShell e rode:

```powershell
iwr -useb https://raw.githubusercontent.com/Gilcemir/markup_helper/main/scripts/bootstrap.ps1 | iex
```

O bootstrap:

1. Cria `%USERPROFILE%\bin` se não existir.
2. Adiciona essa pasta ao **User PATH** (idempotente — não duplica).
3. Baixa `docformatter.exe` da última GitHub Release.
4. Baixa `docformatter-update.ps1` e `docformatter-update.cmd`.

**Abra um novo terminal** (pra pegar o PATH atualizado) e confira:

```powershell
docformatter --version
docformatter --help
```

### Comandos do dia-a-dia

```powershell
# Formatar um único arquivo
docformatter "C:\caminho\artigo.docx"
# → cria C:\caminho\formatted\artigo.docx + .report.txt + .diagnostic.json

# Formatar todos os .docx de uma pasta (não-recursivo)
docformatter "C:\caminho\pasta"
# → cria pasta\formatted\ com saídas + _batch_summary.txt + _app.log
```

Saída padrão da pasta `formatted/`:

| Arquivo | Conteúdo |
|---|---|
| `<nome>.docx` | docx formatado |
| `<nome>.report.txt` | relatório por regra do pipeline (`[INFO]` / `[WARN]` / `[ERROR]`) |
| `<nome>.diagnostic.json` | dump estruturado do `FormattingContext` extraído |
| `_app.log` | log Serilog da execução |
| `_batch_summary.txt` | (modo pasta) resumo `nome.docx ✓/⚠/✗` |

### Códigos de saída

| Código | Significado |
|---|---|
| `0` | sucesso (com ou sem warnings) |
| `1` | erro de uso (argumento, flag desconhecida, caminho inexistente, extensão errada) |
| `2` | abort crítico do pipeline (modo single-file) |

### Atualizar

```powershell
docformatter-update
```

Lê a versão local via `docformatter --version`, consulta a última Release no GitHub e baixa só se for diferente. `docformatter-update -Force` baixa de qualquer jeito. Se o `docformatter.exe` estiver em uso (terminal aberto rodando), o update orienta a fechar e tentar de novo.

---

## Desenvolvimento (macOS)

### Pré-requisitos

- .NET 10 SDK
- `make`, `bash`

### Tarefas frequentes

```bash
make build           # dotnet build (Debug)
make test            # dotnet test (solution)
make test-watch      # dotnet watch test

make run FILE=examples/1_AR_5449_2.docx   # CLI em arquivo único
make run-all                              # CLI em modo batch sobre examples/

make publish-mac     # binário osx-arm64 self-contained pra dev local
make publish-win     # delega pra DocFormatter.Cli/publish.sh (win-x64)

make logs            # tail no _app.log mais recente sob examples/
make clean           # limpa bin/, obj/, e formatted/ de examples/
```

### Estrutura do solution

```
DocFormatter.sln
├── DocFormatter.Core/    lógica pura (regras, pipeline, contexto, opções)
├── DocFormatter.Cli/     entry point (Program.cs → CliApp.Run)
└── DocFormatter.Tests/   xUnit — golden files em Samples/
```

`Directory.Build.props` trava `TargetFramework=net10.0`, `Nullable=enable`, `TreatWarningsAsErrors=true`.

---

## Releasing (workflow)

A distribuição é **tag-based**: você só "publica pro Windows" quando empurra uma tag `vX.Y.Z`. Commits em `main` não geram releases.

### Cortar uma release

```bash
make release VERSION=v0.2.0
```

O target valida:

- `VERSION` no formato `vMAJOR.MINOR.PATCH`
- working tree limpa (sem `git status` sujo)
- a tag ainda não existe
- branch atual é `main`

E então roda `git tag -a` + `git push origin <tag>`. O push da tag dispara o workflow `.github/workflows/release.yml`, que:

1. Faz checkout, instala .NET 10.
2. Extrai a versão da tag (`v0.2.0` → `0.2.0`) e o SHA curto (`abc1234`).
3. Roda `./DocFormatter.Cli/publish.sh` com `VERSION=0.2.0` e `INFORMATIONAL=0.2.0+abc1234`.
4. Cria a Release no GitHub anexando `docformatter.exe` e gerando release notes automáticas a partir dos commits.

A pasta `scripts/` (bootstrap + update + shim) **não é anexada à release** — é puxada direto do `raw.githubusercontent.com/.../main/scripts/`. Assim, melhorias na lógica de instalação/update aparecem pra qualquer máquina que rodar o bootstrap, sem precisar de nova tag.

### Versionamento exposto pelo CLI

```powershell
docformatter --version
# 0.2.0+abc1234
```

O `+abc1234` é o SHA curto do commit que gerou aquele exe — útil pra rastrear qual commit virou um binário específico. Build local sem env vars (`dotnet run` ou `make run`) imprime `1.0.0` (default do .NET) — esperado, é build de dev.

---

## Restrições conhecidas (não revisitar)

- `PublishTrimmed=false` é obrigatório — `DocumentFormat.OpenXml` usa reflection. Travado em ADR-005.
- Sem instalador. Sem code signing. Distribuição é `.exe` puro via Releases.
- Sem WinForms (não roda em macOS, inviabiliza desenvolvimento).
- Footnotes do Word, citations (autor/data, numéricas) e field codes de gerenciadores (Mendeley/Zotero/EndNote) **não são tocados** — preservados intactos.
- Sempre escreve em `formatted/<nome>.docx`; nunca sobrescreve o original.
