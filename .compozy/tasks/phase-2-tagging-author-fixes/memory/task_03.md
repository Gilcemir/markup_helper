# Task Memory: task_03.md

Keep only task-local execution context here. Do not duplicate facts that are obvious from the repository, task file, PRD documents, or git history.

## Objective Snapshot

`Phase2DiffUtility` body-text comparator delivered as a `static class` at `DocFormatter.Core/Reporting/Phase2DiffUtility.cs`, with `Compare(produced, expected, inScope) → DiffResult` plus `internal` helpers exposed via the existing `InternalsVisibleTo("DocFormatter.Tests")` attribute (already configured in `DocFormatter.Core/Properties/AssemblyInfo.cs`). 22 new tests; 407/407 green.

## Important Decisions

- **Recursive strip with `[…]` regex `\[(\w+)([^\]]*)\](.*?)\[/\1\]` (Singleline + Compiled).** When the matched tag is in scope, recurse into its content so nested out-of-scope tags inside an in-scope wrapper still get stripped. Without recursion, e.g. `[abstract]A [xref]Z[/xref] B[/abstract]` with scope={abstract} would leak the `[xref]…[/xref]` into the comparison.
- **Strip applied to expected only**, per the task requirement and ADR-006 ("Do not strip from the produced text"). Self-compare correctness therefore depends on the in-scope set covering every tag actually present in the file. The corpus integration test `Compare_RealCorpusFileAgainstItselfWithFullScope_ReturnsIsMatchTrue` discovers the tag set dynamically from the file body via a `[/?(\w+)` regex so it remains correct as the corpus evolves.
- **Body-text extraction** uses `Body.Descendants<Paragraph>()` filtered to leaf paragraphs (`!Descendants<Paragraph>().Any()`) to avoid double-counting nested paragraph content (rare textbox case). `Break` and `TabChar` map to a space; the per-paragraph `\s+`→" "+Trim normalization absorbs them.
- **`SliceContext` returns up to 80 chars on each side** of the divergence offset, clamped to `[0, text.Length]`. The offset itself is clamped before slicing to keep `text[start..end]` safe when produced/expected differ in length.

## Learnings

- **`examples/phase-2/before/<id>.docx` is NOT clean input.** It already carries Stage-1 SciELO bracket markup (`[author]`, `[doctitle]`, `[normaff]`, `[xref]`, `[doi]`, `[fname]`, `[surname]`, `[label]`, `[toctitle]`, `[doc]`) in body text. Verified via `unzip + grep '<w:t>…</w:t>' /tmp/_before5136/word/document.xml`. The task spec's listed integration assertion ("self-compare with `inScopeTags={abstract,kwdgrp,elocation}` → `IsMatch=true`") cannot hold under strict spec semantics for this corpus, because the strip would erase those Stage-1 tag pairs from the expected side only. The integration test was written with a dynamically-discovered scope to honor the spec's *intent* (round-trip on real corpus) while staying correct under the strict strip rule.
- **`dotnet test` localized output** (`Aprovado/Com falha/Aviso/Erro`) is the only signal — there is no English flag here. Final summary line format: `Aprovado!  – Com falha:     0, Aprovado:    N, Ignorado:     0, Total:    N`.
- **Non-greedy regex with same-name nesting (`[t][t]X[/t][/t]`) is mis-matched** as `[t][t]X[/t]` + orphan `[/t]`. Acceptable per ADR-006; SciELO 4.0 doesn't nest same-name tags. Documented in source comments.

## Files / Surfaces

- `DocFormatter.Core/Reporting/Phase2DiffUtility.cs` (new, 162 lines).
- `DocFormatter.Tests/Phase2DiffUtilityTests.cs` (new, 22 facts).

## Errors / Corrections

- **First test run:** `Compare_RealCorpusFileAgainstItselfWithFullScope` failed (offset 388). Root cause: dynamic tag-name discovery used the linear pair regex, which only sees outer (top-level) pairs and misses tags nested inside an outer match (e.g. `[fname]/[surname]` inside `[author]…[/author]`). Those nested names were absent from the discovered scope, so `Compare`'s recursive strip removed them from expected while leaving them in produced. Fix: discovery now uses `\[/?(\w+)` to enumerate every bracket-tag occurrence regardless of nesting, then dedupes via `HashSet`.

## Ready for Next Run

- `Phase2DiffUtility.Compare` is the canonical entry point for task_05 (`RunPhase2Verify` CLI handler) and task_06 (`Phase2CorpusTests.AllPairsMatch` xUnit integration test).
- Internal helpers (`ExtractBodyText`, `NormalizeParagraphWhitespace`, `StripOutOfScope`, `FindFirstDivergenceOffset`, `SliceContext`) are exposed via the existing `InternalsVisibleTo("DocFormatter.Tests")` and are safe to use directly from tests in tasks 05/06.
