# Section Formatting and History Move (DocFormatter Phase 3) — Task List

## Tasks

| # | Title | Status | Complexity | Dependencies |
|---|-------|--------|------------|--------------|
| 01 | Create BodySectionDetector skeleton with bold cascade resolver | completed | high | — |
| 02 | Implement BodySectionDetector predicates and INTRODUCTION anchor lookup | completed | medium | task_01 |
| 03 | Implement MoveHistoryRule | completed | high | task_02 |
| 04 | Implement PromoteSectionsRule | completed | medium | task_02 |
| 05 | Extend DiagnosticDocument with Phase 3 record types | completed | low | — |
| 06 | Extend DiagnosticWriter to emit Phase 3 diagnostic entries | completed | medium | task_03, task_04, task_05 |
| 07 | Register Phase 3 rules in DI and add end-to-end integration test | completed | medium | task_03, task_04, task_06 |
