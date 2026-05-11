# Task Memory: task_08.md

Keep only task-local execution context here. Do not duplicate facts that are obvious from the repository, task file, PRD documents, or git history.

## Objective Snapshot

Implement `HistDateParser` from scratch (per ADR-007) under `DocFormatter.Core/Rules/Phase2/HistDateParsing/`, TDD-first, with the `HistDate` record + `ToDateIso()` and three entry points (`ParseReceived/Accepted/Published`).

## Important Decisions

- **`HistDate` collocated with the parser** (`Rules/Phase2/HistDateParsing/HistDate.cs`), not under `Models/Phase2/`. The record is the parser's natural return value and has no other consumers; collocation keeps `Models/Phase2/` focused on `FormattingContext`-shaped types (Affiliation, CorrespAuthor, KeywordsGroup, AbstractMarker).
- **Phrase inventory lives in `adrs/adr-007-phrase-inventory.md`** (sibling notes), not appended to ADR-007 proper. ADR-007 stays a decision document; the inventory is a TDD artifact.
- **Portuguese support shipped (best effort)**: the task spec called Portuguese a stretch shape ("either recognized or returns null with no exception"). Implementing it was cheap (one extra regex + accent-strip helper) and aligns with SciELO's Brazilian context. The Portuguese test asserts the lenient contract (recognized OR null), so the parser remains task-spec-compliant even if Portuguese is later removed.
- **Year-only validation gate is `1000–2999`** (covers all plausible publication years; rejects 4-digit numerals that aren't years).
- **Calendar validation via `DateTime.DaysInMonth`** rather than `DateTime.TryParse`. The reference handler used `TryParse` but it accepts locale-specific shapes (e.g., `"2024-3-12"` parses as ISO under invariant culture but the parser already enforces strict 2-digit ISO at regex level). `DaysInMonth` is locale-free.
- **`StripSeparator` rejects junk after a header** (e.g., `"Receivedfoo: 12 March 2024"` returns null) by requiring the next char to be `:` or whitespace. Without this, `"Received".StartsWith("Receivedfoo")` would be false but `"ReceivedX".StartsWith("Received")` is true and would otherwise produce a phantom match on `"X 12 March 2024"`.

## Learnings

- The `before/` corpus uses a uniform `Header: <day> <FullEnglishMonth> <year>` shape across all 10 articles. The reference `AccessedOnHandler` shape (`Month Day, Year`) does not appear in `[hist]` paragraphs but is still required by task spec ("mixed forms"); both shapes are recognized.
- `Marcador_de_referencia/AppRegexes.AccessedOn()` is `(.*)(Accessed on) (.*)` — the header phrase that triggers the parse is `"Accessed on"` (not `"Received"`/`"Accepted"`/`"Published"`). The reference is bibliography-oriented; my parser is hist-oriented and uses different headers entirely.
- `Months[i].StartsWith(splits[0], OrdinalIgnoreCase)` in the reference is a quirky behavior — `"Ma"` matches both March and May, last-write-wins picks May. My parser deliberately uses **exact match** against full names + abbreviations, which is stricter and avoids the quirk.

## Files / Surfaces

Created:
- `DocFormatter.Core/Rules/Phase2/HistDateParsing/HistDate.cs` (record + `ToDateIso()`).
- `DocFormatter.Core/Rules/Phase2/HistDateParsing/HistDateParser.cs` (static class, three entry points + private helpers).
- `DocFormatter.Tests/Phase2/HistDateParserTests.cs` (79 test cases, theory + facts).
- `.compozy/tasks/phase-2-tagging-author-fixes/adrs/adr-007-phrase-inventory.md` (phrase inventory + corpus sweep).

Untouched (verified):
- All other Phase 1/Phase 2 sources unchanged. `make phase2-verify` still PASS for all 10 corpus pairs.

## Errors / Corrections

- Initial round-trip Theory had a transcription error: `[InlineData("Published", "13 March 2026", "20260313")]` for article 5523, which actually has `Published: 18 March 2026` → `20260318`. Fixed before running tests.

## Ready for Next Run

- Task 09 (`EmitHistTagRule`) can call `HistDateParser.ParseReceived/Accepted/Published` directly. Headers handled: `Received[: | on | em] …` / `Accepted[: | on | em] …` / `Published[: | on | em | bare-space] …`.
- The parser is pure (no FormattingContext, no IReport). Task 09's emitter rule is responsible for skip-and-warn semantics (per ADR-002) when the parser returns null.
- `dateiso` formatting (`YYYYMMDD` with `00` padding) is owned by `HistDate.ToDateIso()`. Task 09 must NOT re-implement zero-padding — call `histDate.ToDateIso()` and emit verbatim.
