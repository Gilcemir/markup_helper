# Prompt para Claude Code CLI — DocFormatter

> Use este documento como prompt inicial em uma sessão do Claude Code.
> Recomendação: salve como `CLAUDE.md` na raiz do projeto após o primeiro setup,
> assim sessões futuras carregam o contexto automaticamente.

---

## Missão

Construir **DocFormatter**, aplicação desktop para normalizar/formatar artigos científicos `.docx` segundo regras editoriais fixas de uma revista. O input é um artigo cru submetido por autores; o output é o mesmo artigo com estrutura e formatação padronizadas, pronto pra próximas etapas do fluxo editorial.

**Contexto operacional:**
- Desenvolvedor único, ambiente de desenvolvimento **macOS**.
- Aplicação roda em **Windows 10** (target final).
- Distribuição: `.exe` portátil self-contained, sem instalador.
- Usuários finais conhecidos (interno/pequeno grupo).

---

## Stack obrigatória — NÃO questionar, decisões fechadas

| Camada | Tecnologia |
|---|---|
| Linguagem / Runtime | C# / .NET 8 LTS |
| Manipulação `.docx` | `DocumentFormat.OpenXml` (NuGet, oficial Microsoft) |
| GUI | Avalonia 11 (cross-platform — dev no Mac, build pro Win) |
| DI / Config | `Microsoft.Extensions.DependencyInjection` |
| Logging | Serilog (sinks: Console + File) |
| Testes | xUnit + golden files |
| Build alvo | `win-x64`, self-contained, single-file, ReadyToRun, **sem trim** |

**Razões pras escolhas (não desviar):**
- Avalonia escolhida porque WinForms não roda em Mac (dev impossível).
- OpenXML SDK direto porque manipulação é estrutural pesada (deletar/inserir tabelas, parsear runs com formatação mista, etc.) — bibliotecas de mais alto nível (Xceed DocX) limitam.
- Trim **desligado** porque OpenXML SDK usa reflection — trim quebra silenciosamente.

---

## Estrutura de solution

```
DocFormatter.sln
├── DocFormatter.Core/        net8.0     ← lógica pura, multiplataforma
│   ├── Pipeline/
│   │   ├── IFormattingRule.cs
│   │   ├── FormattingPipeline.cs
│   │   ├── FormattingContext.cs
│   │   ├── IReport.cs
│   │   ├── Report.cs
│   │   └── RuleSeverity.cs
│   ├── Rules/                            ← uma classe por regra (lista abaixo)
│   ├── Options/
│   │   └── FormattingOptions.cs          ← constantes hardcoded
│   └── Models/                           ← Author, Affiliation, AbstractBlock, etc.
├── DocFormatter.Cli/         net8.0     ← entry CLI
│   └── Program.cs
├── DocFormatter.Gui/         net8.0     ← Avalonia, depende de Core
│   ├── App.axaml
│   ├── MainWindow.axaml
│   └── ViewModels/
└── DocFormatter.Tests/       net8.0     ← xUnit
    ├── Samples/
    │   └── case-001/
    │       ├── input.docx
    │       └── expected.docx
    └── PipelineTests.cs
```

---

## Modelo de domínio

### Formato de ENTRADA esperado

Todo artigo de entrada segue esta convenção (rigida — desvio = falha de detecção):

1. **Tabela 3×1 no topo** com colunas: `id`, `elocation`, `doi`.
2. **Próxima linha:** seção do sumário (ex: "Original Article").
3. **Próxima linha:** título do artigo.
4. **Próxima linha:** autores, separados por `,` e por ` and `.
   - Cada autor pode ter ORCID (hiperlink pra `orcid.org/...`) — número e/ou ícone verde.
5. **Próximas linhas:** afiliações, uma por linha. Cada afiliação **começa com sobrescrito** (label que casa com label sobrescrito ao lado do nome do autor).
6. **Bloco history:** linhas com "Received: ...", "Accepted: ...", "Published: ...".
7. **Abstract:** parágrafo no formato `<bold>Abstract</bold> - <italic>texto do abstract</italic>`.
8. **Keywords:** parágrafo no formato `<bold>Keywords</bold> - <italic>k1, k2, k3</italic>`.
9. **Corpo do artigo** com seções:
   - **Major sections:** `TEXTO TODO EM CAIXA ALTA`, bold, fonte 12pt.
   - **Minor sections:** Texto Capitalizado, bold, fonte 12pt.
10. **References** ao final.
11. **Imagens e tabelas** intercaladas no corpo.

### Formato de SAÍDA esperado

Aplicar as transformações:

- **Linha 1:** DOI extraído da tabela superior (ou seção se DOI ausente).
- **Linha 2:** seção do sumário.
- **Linha 3:** título do artigo.
- **Linhas seguintes:** títulos traduzidos (se existirem).
- **Linha em branco** separando título de autores.
- **Cada autor em uma linha**, com label sobrescrito de afiliação preservado.
- **Linha em branco** separando autores de afiliações.
- **Cada afiliação em uma linha** com label sobrescrito.
- **Linha em branco** separando afiliação de Abstract.
- **Abstract:** título "Abstract" em **negrito** num parágrafo; conteúdo no parágrafo seguinte.
- **Keywords:** título "Keywords" em **negrito**; valores separados por vírgula ou ponto-e-vírgula.
- **Major sections:** bold, **16pt**.
- **Minor sections:** bold, **14pt**.
- **Sub-minor sections** (se existirem): bold, **13pt**.
- **Quotes** (parágrafos com estilo "Quote"): recuo esquerdo de **4cm**.
- **Hiperlinks** (reais e field codes): removidos, texto preservado.
- **Hiperlinks ORCID** (URL contém `orcid.org`): **removidos por completo** — texto, ícone e relacionamento.
- **Tabelas:** label/legenda no parágrafo antes do corpo; notas no parágrafo depois.
- **Citações** (autor/data e numéricas): **NÃO TOCAR**.
- **Footnotes:** **NÃO TOCAR** (footnotes reais do Word continuam funcionando).

---

## Pipeline (ordem fixa, severidade declarada)

| # | Regra | Severidade | Função |
|---|---|---|---|
| 1 | `DetectInputFormatRule` | **Critical** | Guarda — verifica tabela 3×1 com DOI no topo. Se ausente, aborta com mensagem clara: "este arquivo não está no formato de entrada esperado — pode já estar formatado." |
| 2 | `ExtractTopTableRule` | **Critical** | Extrai DOI/elocation/id pra `FormattingContext`. **Deleta a tabela.** |
| 3 | `ParseHeaderLinesRule` | **Critical** | Lê seção do sumário e título (linhas posicionais). |
| 4 | `RemoveOrcidLinksRule` | Optional | Limpa hiperlinks `orcid.org` da linha de autores (texto + drawing + relacionamento + imagem órfã). **Roda antes do parse de autores pra não contaminar o split.** |
| 5 | `ParseAuthorsRule` | Optional | Split por `,` e ` and `; identifica labels sobrescritos. |
| 6 | `ParseAffiliationsRule` | Optional | Linhas começando com sobrescrito. |
| 7 | `ParseHistoryRule` | Optional | Received/Accepted/Published. |
| 8 | `ParseAbstractRule` | Optional | Captura `<bold>Abstract</bold> - <italic>...</italic>`. |
| 9 | `ParseKeywordsRule` | Optional | Mesma lógica do Abstract. |
| 10 | `RewriteHeaderRule` | **Critical** | Reescreve o cabeçalho do documento no formato de saída usando `FormattingContext`. |
| 11 | `PromoteSectionsRule` | Optional | CAPS bold 12pt → bold 16pt; bold 12pt (não-CAPS) → bold 14pt. |
| 12 | `RemoveHyperlinksRule` | Optional | Remove `<w:hyperlink>` reais e field codes `HYPERLINK`; preserva texto. **NÃO mexe em texto azul/sublinhado digitado pelo autor.** |
| 13 | `NormalizeQuotesRule` | Optional | Aplica `w:ind w:left="2268"` (4cm) em parágrafos com `pStyle = "Quote"` (variantes: "Quotation", "Citação"). |
| 14 | `NormalizeTableLabelsRule` | Optional | Garante label/legenda no parágrafo anterior à tabela; notas no posterior. Separadores aceitos: `": "`, `" - "`, `". "`. |

**Cortados explicitamente:** `NormalizeCitationsRule`, `NormalizeFootnotesRule`. Não implementar.

---

## Decisões arquiteturais (não revisitar)

### Compartilhamento de dados entre regras
`FormattingContext` tipado, populado pelas regras de extração (1–9), consumido pelas de transformação (10–14). NÃO usar parágrafos placeholder no documento como mecanismo de troca.

```csharp
public class FormattingContext
{
    public string? Doi { get; set; }
    public string? ElocationId { get; set; }
    public string? Id { get; set; }
    public string? SectionTitle { get; set; }
    public string? ArticleTitle { get; set; }
    public List<Author> Authors { get; } = new();
    public List<Affiliation> Affiliations { get; } = new();
    public ArticleHistory? History { get; set; }
    public AbstractBlock? Abstract { get; set; }
    public List<string> Keywords { get; } = new();
}
```

### Modelo de severidade
- **Critical:** exception aborta o pipeline; output não é gerado; status do arquivo = falha.
- **Optional:** exception vira `[ERROR]` no relatório; pipeline continua; output é gerado em best-effort.

```csharp
foreach (var rule in _rules)
{
    try { rule.Apply(doc, ctx, report); }
    catch (Exception ex) when (rule.Severity == RuleSeverity.Optional)
    {
        report.Error(rule.Name, ex.Message); // continua
    }
    // se Critical e estourar, exception sobe e pipeline aborta
}
```

### Idempotência
**Não fazer** rules idempotentes individualmente. Confiar no `DetectInputFormatRule` (regra #1) como guarda — se input já foi formatado, ele detecta a ausência da tabela superior e aborta. Mensagem deve ser específica: *"este arquivo não está no formato de entrada esperado — pode já estar formatado, ou ser de outra fonte."*

### Modo de erro / relatório
Modelo "pipeline + relatório":
- Programa faz best-effort.
- Cada regra registra `[INFO]`, `[WARN]`, `[ERROR]` no `Report`.
- Output sempre gerado (exceto em Critical fail).
- Relatório `<nome>.report.txt` salvo ao lado do output.
- UI mostra resumo colorido (verde / amarelo / vermelho) + textbox com log.

Formato de cada linha do relatório:
```
[INFO]  Tabela superior detectada (3 col × 1 linha) — DOI extraído: 10.1234/abc
[WARN]  Autor 3 ("J. Silva") sem sobrescrito de afiliação
[ERROR] Bloco "References" não encontrado
```

### Output e nomenclatura
- Sempre **gerar arquivo novo**, nunca sobrescrever original.
- Subpasta `formatted/` ao lado do input:
  - `pasta/artigo.docx` → `pasta/formatted/artigo.docx` + `pasta/formatted/artigo.report.txt`.
- Re-run no mesmo input **sobrescreve** silenciosamente o output anterior (intencional).
- Em batch: `pasta/formatted/_batch_summary.txt` com resumo consolidado.

### Constantes
**Hardcoded em `FormattingOptions`** (16pt, 14pt, 13pt, 4cm, regex DOI, separadores, etc.). Injetada via DI pra ficar testável e preparada pra multi-perfil futuro, mas só uma fonte por enquanto. **Sem JSON de configuração externa.**

### Modo de uso
Híbrido: drop de **arquivo único OU pasta** funcionam. UI detecta tipo do path.

---

## Quality bar

### Golden file testing
Para cada artigo real disponível, montar caso em `Tests/Samples/case-NNN/`:
- `input.docx` — artigo cru.
- `expected.docx` — output formatado à mão (referência).
- Teste roda pipeline em `input.docx`, compara com `expected.docx`.
- Comparação não pode ser byte-a-byte (OOXML tem ruído de timestamp/IDs). Usar comparação semântica: parágrafos, runs, formatação.

### Cobertura mínima
- Cada regra do pipeline tem pelo menos 1 teste unitário isolado (input simples, validar context populado / documento transformado).
- Pelo menos 3 golden files (caminho feliz, caso com warnings, caso de Critical fail).

---

## Fases de implementação

**IMPORTANTE:** trabalhar em fases. Ao final de cada fase, **parar e pedir confirmação** antes de seguir.

### Fase 1 — Esqueleto e infra (PRIMEIRA TAREFA)
1. Criar `DocFormatter.sln` com 4 projetos (Core, Cli, Gui, Tests).
2. Adicionar pacotes NuGet: `DocumentFormat.OpenXml`, `Avalonia` (+ `Avalonia.Desktop`, `Avalonia.Themes.Fluent`), `Serilog`, `Serilog.Sinks.Console`, `Serilog.Sinks.File`, `Microsoft.Extensions.DependencyInjection`, `xunit`, `Microsoft.NET.Test.Sdk`, `xunit.runner.visualstudio`.
3. Implementar tipos do Pipeline: `IFormattingRule`, `FormattingPipeline`, `FormattingContext`, `IReport`, `Report`, `RuleSeverity`.
4. Implementar `FormattingOptions` com defaults da spec (16/14/13pt, 4cm, regex DOI, etc.).
5. Implementar `DocFormatter.Cli` minimal: aceita 1 argumento (path de arquivo), instancia pipeline vazia, salva output em `formatted/` + report.
6. `dotnet build` deve passar limpo.
7. **PARAR. Pedir review.**

### Fase 2 — Primeira regra + primeiro teste end-to-end
1. Implementar `DetectInputFormatRule` (Critical).
2. Adicionar primeiro caso em `Tests/Samples/case-001/` — pedir ao usuário um `input.docx` real (ou criar mockup com tabela 3×1 + DOI no topo).
3. Implementar comparador semântico de docx pra golden file testing.
4. Escrever 2 testes: caso feliz (input válido, regra passa) e caso falha (sem tabela superior, regra aborta).
5. CLI agora reconhece arquivo válido vs inválido e gera relatório correto.
6. **PARAR. Pedir review.**

### Fase 3 — Regras de extração (4–9)
1. Implementar `ExtractTopTableRule`, `ParseHeaderLinesRule`, `RemoveOrcidLinksRule`, `ParseAuthorsRule`, `ParseAffiliationsRule`, `ParseHistoryRule`, `ParseAbstractRule`, `ParseKeywordsRule`.
2. Cada uma com teste unitário isolado.
3. Após esta fase, `FormattingContext` está completamente populado pra um input válido.
4. Output ainda não é reescrito — só extração. Validar via inspeção do context (logado em `[INFO]`).
5. **PARAR. Pedir review.**

### Fase 4 — Reescrita do header e transformações de corpo
1. Implementar `RewriteHeaderRule` (Critical) — usa `FormattingContext` pra escrever cabeçalho no formato de saída.
2. Implementar `PromoteSectionsRule`, `RemoveHyperlinksRule`, `NormalizeQuotesRule`, `NormalizeTableLabelsRule`.
3. Golden files agora podem ser comparados end-to-end.
4. **PARAR. Pedir review.**

### Fase 5 — GUI Avalonia
1. `MainWindow` com: drop zone (aceita arquivo ou pasta), botão "Formatar", `ListBox` de status por arquivo, textbox de log.
2. Resumo colorido por arquivo (✓ verde / ⚠ amarelo / ✗ vermelho).
3. Programa roda nativo no Mac pra testes da UI.
4. **PARAR. Pedir review.**

### Fase 6 — Empacotamento Windows
1. `dotnet publish -r win-x64 -c Release --self-contained -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:PublishReadyToRun=true -p:PublishTrimmed=false`.
2. Validar `.exe` portátil em Windows 10 (usuário responsável pela máquina de teste).
3. **PARAR. Pedir review final.**

---

## Restrições e armadilhas conhecidas

- **NÃO** habilitar `PublishTrimmed` — quebra OpenXML SDK silenciosamente.
- **NÃO** instalar bibliotecas docx alternativas (Xceed DocX, Aspose, etc.) — fica em OpenXML SDK puro.
- **NÃO** usar WinForms — não roda em Mac, inviabiliza desenvolvimento.
- **NÃO** criar instalador (Inno Setup, MSI) — distribuição é `.exe` direto.
- **NÃO** assinar binário (sem code signing certificate) — usuário aceita aviso do SmartScreen.
- **NÃO** implementar `NormalizeCitationsRule` ou `NormalizeFootnotesRule` — explicitamente fora do escopo.
- **NÃO** mexer em field codes de Mendeley/Zotero/EndNote (`<w:instrText>ADDIN CSL_CITATION...`) — só logar `[INFO]` informando presença, preservar intactos.
- **NÃO** mexer em texto azul/sublinhado digitado manualmente — só hiperlinks reais (`<w:hyperlink>`) e field codes (`HYPERLINK`).
- **NÃO** sobrescrever o arquivo original — sempre subpasta `formatted/`.
- **NÃO** assumir que o input tem estilos do Word ("Heading 1", etc.) — autor formata visualmente.
- **NÃO** criar JSON de configuração externa — constantes hardcoded em `FormattingOptions`.
- **NÃO** comparar docx byte-a-byte em testes — usar comparação semântica.
- **NÃO** usar GitHub Actions ou pipeline CI nesta primeira versão — distribuição é manual.

---

## Convenções de código

- C# 12, nullable reference types **on**, implicit usings **on**.
- Nomes de classes/métodos em **inglês**; comentários e mensagens de relatório em **PT-BR**.
- Cada regra é uma classe pública selada (`public sealed class XyzRule : FormattingRule`).
- Sem `static` em código de domínio — tudo via DI pra testabilidade.
- Logging de regra: `report.Info(ruleName, mensagem)`, `report.Warn(...)`, `report.Error(...)`.
- Excepts em regras Optional não devem propagar — pipeline trata; mas se for inesperado, deixar subir pra log central.

---

## Primeira ação

1. Salvar este documento como `CLAUDE.md` na raiz do repositório (criar repo se não existir).
2. Executar **Fase 1 completa** descrita acima.
3. Reportar status:
   - Estrutura criada.
   - `dotnet build` resultado.
   - Próximos passos esperados.
4. **PARAR e aguardar instruções pra Fase 2.**

Não pular fases. Não tentar implementar regras antes da Fase 2. Não criar GUI antes da Fase 5.