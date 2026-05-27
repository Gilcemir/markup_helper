# Task Memory: task_02.md

Keep only task-local execution context here. Do not duplicate facts that are obvious from the repository, task file, PRD documents, or git history.

## Objective Snapshot
DONE. `OtherTable` loader: `other.txt` TSV → basename→other map, found/not-found
lookup. Sole source for the externally-assigned `other` number. 14 tests pass,
93.33% line cov.

## Important Decisions
- API: `static Load(path)` (reads file) + `static Parse(IEnumerable<string>, origin)`
  for unit testing without a file — mirrors `DocxSourceReader.Read`/`Parse` split.
- Lookup is `bool TryGetOther(basename, out string? other)` → false + null on miss
  (NOT empty string), so caller fails loud per ADR-001. Mirrors
  `CreditTermTable.TryMap`.
- Key = `Path.GetFileNameWithoutExtension(basename)` on BOTH store and query, so a
  `.pdf`/`.xml`/extensionless basename all resolve. SciELO basenames have no
  interior dots, so single-extension strip is safe.
- Value preserved verbatim — only surrounding whitespace trimmed, never parsed
  (leading zeros significant, e.g. `00201`).
- Fail-loud extras (beyond MUSTs): line without a tab → `InvalidDataException`;
  duplicate basename with CONFLICTING value → throw; duplicate with SAME value
  tolerated.

## Learnings
- Integration test path resolution: walk up from `AppContext.BaseDirectory` for
  `examples/phase-3/other.txt` (same pattern as other Jats corpus tests).

## Files / Surfaces
- NEW `DocFormatter.Core/Jats/OtherTable.cs`
- NEW `DocFormatter.Tests/Jats/OtherTableTests.cs`

## Errors / Corrections
- none

## Ready for Next Run
- task_03 (`DocumentPairer`) consumes `OtherTable.TryGetOther` — missing entry is
  a fail-loud pairing error (ADR-004). task_07 `OtherIdInjector` already reads the
  resolved `Phase3Context.OtherNumber` (no direct table dependency).
