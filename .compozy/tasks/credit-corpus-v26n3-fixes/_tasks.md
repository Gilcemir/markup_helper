# credit-corpus-v26n3-fixes — Task List

## Tasks

| # | Title | Status | Complexity | Dependencies |
|---|-------|--------|------------|--------------|
| 01 | Tiered and variant candidate initials in AuthorInitialsResolver | completed | medium | — |
| 02 | Author-keyed CREDIT grammar with `;`/`,` separators and initials-block lookback | completed | medium | task_01 |
| 03 | DocxSourceReader ignores `[p]`/`[/p]` in trailing sections and exposes CreditHeaderFound | completed | low | — |
| 04 | CreditOutcome on Phase3Context recorded by CreditRolesInjector, header-empty WARN, integration fixture | completed | medium | task_02, task_03 |
| 05 | contrib-names verification injector registered before credit-roles | completed | medium | — |
| 06 | creditStatement block in the Phase 3 diagnostic | completed | medium | task_04 |
| 07 | Batch summary pendency block for CRediT and broken names | completed | medium | task_04, task_05, task_06 |
| 08 | Plain-text ORCID token and repeated-token WARN in ExtractAuthorsRule | completed | medium | — |
| 09 | Manual acceptance run over a copy of CBAB v26n3 | completed | medium | task_01, task_02, task_03, task_04, task_05, task_06, task_07 |
| 10 | Documentation and v0.3.1 release preparation | completed | low | task_08, task_09 |
