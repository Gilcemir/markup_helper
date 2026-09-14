# credit-corpus-v26n3-fixes — Task List

## Tasks

| # | Title | Status | Complexity | Dependencies |
|---|-------|--------|------------|--------------|
| 01 | Tiered and variant candidate initials in AuthorInitialsResolver | pending | medium | — |
| 02 | Author-keyed CREDIT grammar with `;`/`,` separators and initials-block lookback | pending | medium | task_01 |
| 03 | DocxSourceReader ignores `[p]`/`[/p]` in trailing sections and exposes CreditHeaderFound | pending | low | — |
| 04 | CreditOutcome on Phase3Context recorded by CreditRolesInjector, header-empty WARN, integration fixture | pending | medium | task_02, task_03 |
| 05 | contrib-names verification injector registered before credit-roles | pending | medium | — |
| 06 | creditStatement block in the Phase 3 diagnostic | pending | medium | task_04 |
| 07 | Batch summary pendency block for CRediT and broken names | pending | medium | task_04, task_05, task_06 |
| 08 | Plain-text ORCID token and repeated-token WARN in ExtractAuthorsRule | pending | medium | — |
| 09 | Manual acceptance run over a copy of CBAB v26n3 | pending | medium | task_01, task_02, task_03, task_04, task_05, task_06, task_07 |
| 10 | Documentation and v0.3.1 release preparation | pending | low | task_08, task_09 |
