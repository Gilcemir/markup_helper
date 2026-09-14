# Task Memory: task_03.md

Keep only task-local execution context here. Do not duplicate facts that are obvious from the repository, task file, PRD documents, or git history.

## Objective Snapshot

- `ExtractSection` strips `[p]`/`[/p]` (case-insensitive) before the bracket/REFERENCES break; `DocxSource.CreditHeaderFound` set from the segment list. Done 2026-09-14.

## Important Decisions

- Strip implemented as a `[GeneratedRegex(@"\[/?p\]")]` replace over the whole segment (not just prefix/suffix), so a `[p]` glued mid-segment is also removed; the strip runs before the empty-segment `continue`, so a bare `[p][/p]` paragraph is treated as blank rather than a terminator.
- `CreditHeaderFound` is computed from `segments` (post `SplitOnHeaders`), so a header glued to the previous body (5640 shape) also sets it. Non-required `init` bool → the many existing `new DocxSource { … }` test constructions compile unchanged.

## Learnings

- Red signal: 4 of the 7 new tests failed behaviorally (3 `[p]` extractions + header flag) once the property existed; after the strip: 27/27 reader tests, full suite 848/848 (841 + 7).
- `CreditRolesInjector` still emits the INFO "No CREDIT statement" line when raw is blank; the header-empty WARN path is task_04's scope.

## Files / Surfaces

- `DocFormatter.Core/Jats/DocxSourceReader.cs` — `Parse` (flag), `ExtractSection`, new `StripParagraphTags`, `ParagraphTagRegex`.
- `DocFormatter.Core/Jats/DocxSource.cs` — `CreditHeaderFound`, doc tweak on `CreditStatementRaw`.
- `DocFormatter.Tests/Jats/DocxSourceReaderTests.cs` — 7 new facts in a "[p]-tagged bodies and CreditHeaderFound" section.

## Errors / Corrections

- None.

## Ready for Next Run

- task_04: `ctx.Source.CreditHeaderFound && string.IsNullOrWhiteSpace(raw)` is the headerEmpty branch; `Phase3DocxFixtureBuilder` has no `[p]` CREDIT paragraph helper yet — add one there for the integration fixture.
