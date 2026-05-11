# ADR-007 Phrase Inventory — `HistDateParser` Recognized Shapes

> Sibling notes to [ADR-007](adr-007.md). Captured as the TDD source of truth
> for `DocFormatter.Core/Rules/Phase2/HistDateParsing/HistDateParser.cs`.
>
> Per ADR-007, the `Marcador_de_referencia/BibliographyHandlers/AccessedOnHandler.cs`
> file is consulted as **algorithmic reference only**. No code is copied. The
> tables below catalogue (a) every phrase shape `AccessedOnHandler` recognizes,
> and (b) every phrase shape the SciELO Phase 2 `before/` corpus actually
> contains. The parser implementation supports the union, plus the additional
> shapes called out in `task_08.md` (ISO, year-only, English abbreviations).

## Reference: `AccessedOnHandler.cs`

| Pattern (in-file) | Example | Notes |
|-------------------|---------|-------|
| `(.*)(Accessed on) (.*)` (`AppRegexes.AccessedOn()`) | `Accessed on July 20, 2025.` | Body must contain the literal `Accessed on`. |
| `<MonthName> <Day>[,.] <Year>` (`GetDateTime`) | `July 20, 2025` | Splits on space; expects three tokens. |

`GetDateTime` semantics (paraphrased, not copied):

- **Month** — iterates the full English month-name array; takes the
  *last* index `i` for which `Months[i].StartsWith(splits[0], OrdinalIgnoreCase)`
  is true. Side effect: `"Jan"` → January (only match); `"Ma"` → May
  (March overwritten by May); `"June"` → June (exact match wins).
- **Day** — `splits[1].TrimEnd(',', ' ', '.')`; parsed as `int`.
- **Year** — `splits[2].Substring(0, 4)`; parsed as `int`.
- **Validation** — `DateTime.TryParse($"{year}-{month}-{day}")`.
- **Failure** — returns `null`. The handler then falls through to its base.

The reference therefore recognizes one shape (`Month Day, Year`, English,
full or abbreviated month name) and rejects everything else.

## Phase 2 Corpus (`examples/phase-2/before/*.docx`)

Empirical sweep across all 10 articles (paragraphs containing one of
`Received` / `Accepted` / `Published`):

| Header | Example phrase                          | Articles                                                    |
|--------|-----------------------------------------|-------------------------------------------------------------|
| `Received:` | `Received: 31 January 2025`         | 5136, 5293, 5313, 5419, 5424, 5434, 5449, 5458, 5523, 5549 |
| `Accepted:` | `Accepted: 7 January 2026`          | 5136, 5293, 5313, 5419, 5424, 5434, 5449, 5458, 5523, 5549 |
| `Published:` | `Published: 24 February 2026`      | 5136, 5293, 5313, 5419, 5424, 5434, 5449, 5458, 5523, 5549 |

The shape inside the colon is uniform: `<Day> <FullEnglishMonth> <Year>`,
day with or without leading zero, full English month name, 4-digit year.
The corpus does **not** include the `AccessedOnHandler` shape
(`Month Day, Year`) for `received/accepted/published`; it appears only in
bibliography prose (`Accessed on July 20, 2025.`), which is out of scope
for `[hist]`.

## Phrase Inventory — Parser Behavior

| # | Shape                                  | Example                              | Months        | Notes                                                                                  |
|---|----------------------------------------|--------------------------------------|---------------|----------------------------------------------------------------------------------------|
| 1 | `<Day> <Month> <Year>`                 | `12 March 2024`                      | English full  | Corpus shape after header strip. Day 1–2 digits, optional leading zero.                |
| 2 | `<Day> <Month> <Year>` (abbrev month)  | `12 Mar 2024`                        | English abbr  | `Jan`–`Dec`. Three-letter, case-insensitive.                                           |
| 3 | `<Month> <Day>, <Year>`                | `March 12, 2024`                     | English full  | `AccessedOnHandler` shape. Comma optional. Day may have trailing `.`.                  |
| 4 | `<Month> <Day>, <Year>` (abbrev month) | `Mar 12, 2024`                       | English abbr  | Same as #3 with abbreviated month.                                                     |
| 5 | ISO `YYYY-MM-DD`                       | `2024-04-15`                         | numeric       | Strict: 4-digit year, 2-digit month, 2-digit day; hyphen separator.                    |
| 6 | Year-only                              | `2024`                               | —             | `HistDate(2024, null, null, "2024")`. `ToDateIso()` → `20240000`.                      |
| 7 | Portuguese `<Day> de <Mês> de <Year>`  | `12 de março de 2024`                | Portuguese    | Stretch shape. `janeiro`–`dezembro`, accent-insensitive (`marco`/`março`).             |
| 8 | Header prefix `Received:` / `Accepted:` / `Published:` | `Received: 31 January 2025` | n/a    | Each entry point strips its own header (case-insensitive) and dispatches to the shared shape parser. |
| 9 | Header prefix `Received on` / `Accepted on` / `Published` (no colon, no preposition) | `Received on 12 March 2024` / `Published 2024` | n/a | Same as #8 with `on` or bare whitespace separator. |
| 10 | Header prefix Portuguese `Recebido em` / `Aceito em` / `Publicado em` | `Recebido em 12 de março de 2024` | Portuguese | Stretch. Returns `null` if the date span fails parse. |

`null` outcomes (each entry point):

- Empty / whitespace input.
- Date span absent (`Hello world`).
- Header word missing (`12 March 2024` passed to `ParseReceived`).
- Date in unsupported shape (`12/03/2024` — slashes, no header).
- Validation failure (`30 February 2024` — `DateTime.TryParse` rejects).

## `HistDate.ToDateIso()`

`YYYYMMDD`, zero-padded. When `Month` or `Day` is null, `00` is emitted in
that position (per `docs/scielo_context/README.md` invariant 5):

| `HistDate`                                 | `ToDateIso()` |
|--------------------------------------------|---------------|
| `(2024, 3, 12, "12 March 2024")`           | `20240312`    |
| `(2024, 12, 5, "5 December 2024")`         | `20241205`    |
| `(2024, 3, null, "March 2024")`            | `20240300`    |
| `(2024, null, null, "2024")`               | `20240000`    |
