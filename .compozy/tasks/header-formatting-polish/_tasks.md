# Header Formatting Polish — Task List

## Tasks

| # | Title | Status | Complexity | Dependencies |
|---|-------|--------|------------|--------------|
| 01 | Extend FormattingContext with Phase 2 cross-rule state | completed | low | — |
| 02 | Extend FormattingOptions with email and corresponding-author regexes | completed | low | — |
| 03 | Stash section and title paragraphs in ParseHeaderLinesRule | completed | low | task_01 |
| 04 | Stash DOI and author-block-end paragraphs in RewriteHeaderMvpRule | completed | low | task_01 |
| 05 | Implement ApplyHeaderAlignmentRule | completed | medium | task_01, task_03, task_04 |
| 06 | Implement EnsureAuthorBlockSpacingRule | completed | medium | task_01, task_04 |
| 07 | Implement ExtractCorrespondingAuthorRule | completed | high | task_01, task_02 |
| 08 | Implement RewriteAbstractRule | completed | high | task_01, task_02, task_07 |
| 09 | Extend DiagnosticDocument and DiagnosticWriter with formatting section | completed | medium | task_05, task_06, task_07, task_08 |
| 10 | Wire Phase 2 rules in CLI DI and add end-to-end integration test | completed | medium | task_05, task_06, task_07, task_08, task_09 |
