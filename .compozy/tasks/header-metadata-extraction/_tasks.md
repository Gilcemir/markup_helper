# Header Metadata Extraction — Task List

## Tasks

| # | Title | Status | Complexity | Dependencies |
|---|-------|--------|------------|--------------|
| 01 | Solution skeleton with Directory.Build.props and NuGet packages | completed | low | — |
| 02 | Pipeline contracts in DocFormatter.Core | completed | medium | task_01 |
| 03 | Domain models and FormattingOptions constants | completed | low | task_02 |
| 04 | FormattingPipeline orchestrator with severity model | completed | medium | task_02 |
| 05 | ExtractTopTableRule for DOI and ELOCATION extraction | completed | medium | task_03, task_04 |
| 06 | ParseHeaderLinesRule for section and article title | completed | low | task_05 |
| 07 | ExtractOrcidLinksRule with relationship cleanup | completed | high | task_03, task_04 |
| 08 | ParseAuthorsRule with comprehensive xUnit tests | completed | high | task_03, task_07 |
| 09 | RewriteHeaderMvpRule for the four-field output layout | completed | medium | task_05, task_06, task_08 |
| 10 | LocateAbstractAndInsertElocationRule with bilingual heuristic | completed | medium | task_04, task_05 |
| 11 | CLI bootstrap with single-file and batch flows plus report writer | completed | high | task_05, task_06, task_07, task_08, task_09, task_10 |
| 12 | Diagnostic JSON serializer per ADR-004 schema | completed | medium | task_11 |
| 13 | Windows publish target and end-to-end production article validation | pending | low | task_11, task_12 |
