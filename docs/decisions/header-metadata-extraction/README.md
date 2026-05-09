# Header Metadata Extraction

DocFormatter MVP: extracts DOI, section, title, authors, and ELOCATION
from the header of a `.docx`, deletes the 3×1 control table, and
rewrites those fields in the journal's output format. Establishes the
architectural skeleton (`FormattingPipeline`, `FormattingContext`,
`Report`) into which later phases plug.

## ADRs

- [adr-001](adr-001.md) — Esqueleto alinhado ao spec com 4 regras de extração
- [adr-002](adr-002.md) — Solution layout — Core + Cli + Tests, no Gui in MVP
- [adr-003](adr-003.md) — ORCID extraction overrides spec's "remove" behavior
- [adr-004](adr-004.md) — Diagnostic JSON schema — per-field with confidence + issues list
- [adr-005](adr-005.md) — .NET 10 LTS with TreatWarningsAsErrors, overriding the spec's .NET 8 default
- [adr-006-merge-orcid-and-authors](adr-006-merge-orcid-and-authors.md) — Merge ExtractOrcidLinksRule and ParseAuthorsRule into a single ExtractAuthorsRule
