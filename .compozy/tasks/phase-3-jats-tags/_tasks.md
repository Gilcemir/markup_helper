# Phase 3 — JATS Tag Injection — Task List

## Tasks

| # | Title | Status | Complexity | Dependencies |
|---|-------|--------|------------|--------------|
| 01 | DocxSource model and DocxSourceReader | completed | medium | — |
| 02 | OtherTable loader | completed | low | — |
| 03 | DocumentPairer with DOI verification | completed | medium | task_01, task_02 |
| 04 | Phase 3 contracts and pipeline | completed | medium | — |
| 05 | IConfirmer implementations | completed | low | task_04 |
| 06 | XmlWriter whitespace-preserving helper | completed | medium | — |
| 07 | OtherIdInjector | completed | medium | task_02, task_04, task_06 |
| 08 | EditedByInjector | completed | medium | task_01, task_04, task_06 |
| 09 | DataAvailabilityInjector | completed | medium | task_01, task_04, task_06 |
| 10 | CreditRolesInjector | completed | high | task_01, task_04, task_06 |
| 11 | CLI wiring and DI registration | completed | high | task_03, task_04, task_05, task_06, task_07, task_08, task_09, task_10 |
| 12 | Phase 3 golden-corpus tests | completed | medium | task_11 |
| 13 | Free-text CRediT role fallback (operator-chosen, document-scoped) | completed | high | task_05, task_10 |
