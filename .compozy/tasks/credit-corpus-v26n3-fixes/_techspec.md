# TechSpec — credit-corpus-v26n3-fixes

## Executive Summary

Eight targeted changes across `DocFormatter.Core` (Jats, Rules, Reporting) and
`DocFormatter.Cli`, no new project or package. Phase 3 gets a wider
author-keyed grammar with shape-based lookback (ADR-003), a two-tier resolver
with variant candidates (ADR-004), a verification-only `contrib-names`
injector (ADR-002), and a `CreditOutcome` record on `Phase3Context` that feeds
both the diagnostic and the batch summary (ADR-005). Phase 1 tokenizes
plain-text ORCIDs and warns on repeated name tokens (ADR-006).

Primary trade-off: the parser becomes more permissive to admit the dominant
CBAB layout, relying on the unchanged confidence gate (mapped terms + unique
author) rather than on strict syntax to prevent wrong injections. Validation
is one manual run over a copy of v26n3 plus the existing unit suite and the
Phase 2 diff gate.

## System Architecture

### Component Overview

Phase 3 flow per XML (unchanged shape, new data):

```
DocxSourceReader ──► DocxSource{CreditStatementRaw, CreditHeaderFound}
                              │
Phase3Pipeline: OtherId → EditedBy → DataAvailability → ContribNames(new) → CreditRoles
                                                              │                 │
                                                        WARN per broken   CreditStatementParser → AuthorInitialsResolver
                                                        <surname>               │
                                                                        ctx.Credit = CreditOutcome
                                                                                │
Phase3Processor ──► DiagnosticWriter.WritePhase3(…, ctx.Credit) ──► .diagnostic.json (gated on WARN+)
                ──► Phase3Outcome{Credit, BrokenNames} ──► WritePhase3BatchSummary
```

Phase 1: `ExtractAuthorsRule.AppendRunTokens` emits `Orcid` tokens from plain
text; `FlagSuspicions` adds the repeated-token WARN. `RewriteHeaderMvpRule` is
untouched and now receives a clean name.

## Implementation Design

### Core Interfaces

New records on `Phase3Context` (settable, written only by
`CreditRolesInjector`):

```csharp
public sealed record CreditEntryOutcome(
    string AuthorKey,
    IReadOnlyList<string> Terms,
    string Resolution,               // "resolved" | "notFound" | "ambiguous"
    IReadOnlyList<string> UnknownTerms,
    bool Applied);

public sealed record CreditOutcome(
    string? Raw,
    CreditShape Shape,
    IReadOnlyList<CreditEntryOutcome> Entries,
    string Disposition);             // "autoApplied" | "confirmed" | "skipped" | "freeText" | "prose" | "headerEmpty" | "absent"

public sealed class Phase3Context { /* existing */ public CreditOutcome? Credit { get; set; } }
```

New injector:

```csharp
public sealed class ContribNamesInjector : IJatsInjector
{
    public string Name => "contrib-names";
    public RuleSeverity Severity => RuleSeverity.Optional;
    // WARN per <contrib> whose <surname> is empty, contains digits, or matches
    // the ORCID pattern; never mutates ctx.Xml.
    public void Apply(Phase3Context ctx, IReport report);
}
```

`DiagnosticWriter.WritePhase3(string filePath, string sourceFileName,
XDocument xml, IReport report, IReadOnlyDictionary<string, ConfirmDisposition>
recorded, CreditOutcome? credit)` — one added parameter on both overloads and
on `BuildPhase3Document`.

### Data Models

- `DocxSource`: add `bool CreditHeaderFound { get; init; }`.
  `CreditStatementRaw` semantics unchanged (null when no body).
- `DiagnosticPhase3`: add `DiagnosticCreditStatement? CreditStatement` =
  `(string? Raw, string Shape, IReadOnlyList<DiagnosticCreditEntry> Entries)`
  with `DiagnosticCreditEntry(string AuthorKey, IReadOnlyList<string> Terms,
  string Resolution, IReadOnlyList<string> UnknownTerms, bool Applied)`.
  Serialized by the existing camelCase options. `DiagnosticPhase3Tag`
  unchanged.
- CLI `Phase3Outcome`: add `CreditOutcome? Credit` and `int BrokenNames`
  (count of report entries with rule `contrib-names` at WARN).

Batch summary format (ADR-005): existing header and per-file line unchanged;
for files with `Credit.Disposition != autoApplied` or `BrokenNames > 0`,
append indented lines:

```
1984-7033-cbab-26-03-e56132631.xml ✓ prompted
  credit: applied TVB, QHTP; pending NHN (notFound)
  contrib-names: 1 broken surname(s)
1984-7033-cbab-26-03-e55362631.xml ✓
  credit: header found, body empty
```

Prose: `credit: free prose (not auto-applied)`. Unknown terms:
`pending X (unknown term: Metodology)`.

### Parser algorithm (`TryParseAuthorKeyed`)

```
for segment in text.Split('.'), skipping blanks:
  colon = segment.IndexOf(':'); if colon < 0 → return false
  keys  = SplitTrim(segment[..colon], ';', ',');  if empty → return false
  pieces = SplitTrim(segment[(colon+1)..], ';', ',')
  terms = []; pending = []
  for piece in pieces:
    c = piece.IndexOf(':')
    if c >= 0:
        if terms.Count == 0 → return false           // entry without terms
        flush(keys, terms)
        keys = pending + [piece[..c].Trim()]; pending = []
        terms = piece[(c+1)..].Trim() is non-empty ? [that] : []
    elif IsInitialsToken(piece): pending.Add(piece)
    elif pending.Count > 0 → return false             // initials followed by a term, not by ':'
    else terms.Add(piece)
  if pending.Count > 0 or terms.Count == 0 → return false
  flush(keys, terms)
require anyMappedTerm && !builder.IsEmpty
```

`IsInitialsToken` moves to `internal static` on `AuthorInitialsResolver`
(≥2 letters, all uppercase). Role-keyed detection is unchanged and still runs
first.

### Reader (`ExtractSection`)

Per piece: `trimmed = StripParagraphTags(piece).Trim()` where
`StripParagraphTags` removes `[p]` and `[/p]` case-insensitively; then the
existing `[` / `REFERENCES` break. `Parse` sets
`CreditHeaderFound = segments.Contains("CREDIT STATEMENT")`.

`CreditRolesInjector.Apply`: when raw is blank and `CreditHeaderFound`,
`report.Warn(Name, "CREDIT STATEMENT header found but the body is empty")`,
`ctx.Credit = headerEmpty`, return; when the header is absent, existing INFO
plus `ctx.Credit = absent`.

### Resolver (`AuthorInitialsResolver`)

`CandidateInitials` → `(HashSet<string> Full, HashSet<string> GivenOnly)`.
`InitialsVariants(text) : IReadOnlyList<string>`: tokens split on space; each
token expands to a list of variants (hyphen `-`/`‐`/`–` → `{first initial,
all sub-initials}`; capitalized particle → `{"", initial}`; lowercase
particle → `{""}`; else `{initial}`), combined by cartesian product.
`Full` = every `g+s+x`, `g+s`, `s+g` over the variant lists; `GivenOnly` =
given variants. `Resolve` for a bare key: match `Full` across contribs; if
none, match `GivenOnly`; `Classify`. `ResolveBySurname` narrowing uses
`Full ∪ GivenOnly`.

### Phase 1 (`ExtractAuthorsRule`)

- New generated regex `PlainOrcidRegex` =
  `(?:https?://(?:www\.)?orcid\.org/)?(?<id>\d{4}-\d{4}-\d{4}-\d{3}[\dX])(?![\dX])`
  (no leading `\b`, so a glued `Bruzi0000-…` splits).
- `AppendRunTokens(run, tokens, report)` (non-superscript path): split text
  around matches into `Text` / `Orcid` / `Text` tokens; INFO
  `extracted ORCID '…' from plain text`.
- `ConsumeTokens` Orcid case: if `OrcidId` is set and differs →
  `builder.Warn("conflicting ORCID …")` (new `Warn` that does not change
  confidence); else `??=` as today.
- `FlagSuspicions`: tokens of the name; if `Fold(last)` equals `Fold(any
  earlier token)` → `builder.Warn("last name token '…' repeats an earlier
  token; SciELO Markup mark_authors will tag the first occurrence")`.

### API Endpoints

Not applicable (CLI tool). No new subcommands or flags.

## Integration Points

SciELO Markup consumes Phase 1 output and produces Phase 3 input; no code
integration, behavior documented in ADR-003/006. No external services.

## Impact Analysis

| Component | Impact Type | Description and Risk | Required Action |
|---|---|---|---|
| `Jats/CreditStatementParser.cs` | modified | New author-keyed algorithm; medium risk (grammar) | Rewrite `TryParseAuthorKeyed`; keep all existing tests green; add 7 corpus tests |
| `Jats/AuthorInitialsResolver.cs` | modified | Tiered/variant candidates; `IsInitialsToken` internal | Refactor `CandidateInitials`/`Initials`; add 6 tests |
| `Jats/DocxSourceReader.cs`, `DocxSource.cs` | modified | `[p]` strip, `CreditHeaderFound`; low risk | Add property, adjust `ExtractSection`; 2 tests |
| `Jats/ContribNamesInjector.cs` | new | Verification-only rule; low risk | Implement; register before CreditRoles in `RuleRegistration.AddPhase3Injectors`; tests |
| `Jats/CreditRolesInjector.cs`, `Phase3Context.cs` | modified | Records `CreditOutcome` at every exit; header-empty WARN | Add property/records; set on all paths; extend injector tests |
| `Reporting/DiagnosticWriter.cs`, `DiagnosticDocument.cs` | modified | New parameter and record; existing callers/tests updated | Update signatures, `DiagnosticWriterPhase3Tests` |
| `Cli/Phase3Processor.cs`, `Cli/CliApp.cs` | modified | Pass `ctx.Credit`; extend `Phase3Outcome`; summary block | Update `WritePhase3BatchSummary`; `CliPhase3Tests` |
| `Rules/ExtractAuthorsRule.cs` | modified | Plain ORCID token, repeated-token WARN; medium risk (Phase 2 gate) | Implement; `ExtractAuthorsRuleTests`; run `make phase2-verify` |
| `Pipeline/RuleRegistration.cs`, `RuleRegistrationTests` | modified | One more Phase 3 injector | Register; assert order |
| `docs/INVARIANTS.md`, `docs/decisions/` | modified | INV-02 and 6 ADRs via `/promote-feature` at the end | Run promote-feature when merge-ready |

## Testing Approach

### Unit Tests

- `CreditStatementParserTests`: real texts of 5642, 5412 (`;` between terms →
  AuthorKeyed, all terms map), 5501 (`;` between entries and terms; 5 authors
  with exact term lists, e.g. CDC = Methodology, Supervision, Validation,
  Resources, Writing – review & editing), 5547 and 5613 (`;` + `.` between
  entries), 5528 and 5441 (isolated `;` in a `,` list). Negative: pending
  initials never closed → Prose; entry without terms → Prose. Existing four
  tests unchanged.
- `DocxSourceReaderTests`: `[p]…[/p]` body extracted; `[refs]` / `[ack]`
  still end the section; header with empty body → `CreditHeaderFound = true`,
  raw null.
- `AuthorInitialsResolverTests`: "SA" → Abid with Khan present; "LBB" and
  "LB" → Barboza-Barquero; "TVB" and "TB" → Bui; "TTR" / "JGS" / "MRC" →
  NotFound; two contributors both matching in Tier 1 → Ambiguous.
- `ContribNamesInjectorTests`: empty surname, digits, ORCID → one WARN each;
  clean → no entries; XML unchanged (string compare).
- `CreditRolesInjectorTests`: `ctx.Credit` populated on autoApplied,
  confirmed, skipped, prose, headerEmpty, absent paths.
- `DiagnosticWriterPhase3Tests`: `creditStatement` block present with
  raw/shape/entries; null when no outcome.
- `CliPhase3Tests`: batch summary indented block for pending/broken; none for
  clean.
- `ExtractAuthorsRuleTests`: plain ORCID after name; glued to surname; with
  `https://orcid.org/` prefix; before superscript label; conflicting with
  hyperlink ORCID → WARN; "Nguyen Hoai Nguyen" → WARN, confidence High.

### Integration Tests

- `Phase3PipelineIntegrationTests`: extend the happy-path fixture with a
  `;`-separated statement and a `[p]`-tagged CREDIT paragraph; assert roles
  injected and no WARN.
- Existing `Phase3CorpusTests` and `make phase2-verify` must stay green.
- Manual acceptance (task-documented, not automated): copy v26n3 to the
  scratchpad as
  `<copy>/L0X/markup_xml/{other.txt, scielo_package/*.xml, scielo_markup/*.docx}`
  (the CLI walks up to `other.txt` and looks for `scielo_markup/` beside it,
  so the 15 docx go into each `L0X/markup_xml/scielo_markup/`), run
  `dotnet run --project DocFormatter.Cli -- phase3 <copy>/L0X/markup_xml/scielo_package --non-interactive=accept`
  for L01–L03, and check the batch summaries against the PRD acceptance table.
  The evidence folder is never written.

## Development Sequencing

### Build Order

1. `AuthorInitialsResolver`: `IsInitialsToken` internal, tiered variant
   candidates + tests — no dependencies.
2. `CreditStatementParser` new algorithm + corpus tests — depends on step 1
   (`IsInitialsToken`).
3. `DocxSource.CreditHeaderFound` + `ExtractSection` `[p]` strip + tests —
   no dependencies.
4. `CreditOutcome` records, `Phase3Context.Credit`, `CreditRolesInjector`
   records outcome and header-empty WARN + tests — depends on steps 2 and 3.
5. `ContribNamesInjector` + registration + tests — no dependencies (parallel
   to steps 1–4).
6. `DiagnosticWriter` / `DiagnosticDocument` credit block + tests — depends on
   step 4.
7. CLI: `Phase3Processor` passes outcome; `Phase3Outcome` / batch summary
   block + tests — depends on steps 4, 5 and 6.
8. `ExtractAuthorsRule` plain ORCID token + repeated-token WARN + tests;
   `make phase2-verify` — no dependencies (parallel to steps 1–7).
9. Manual acceptance run over the v26n3 copy; fix-ups — depends on steps 1–7.
10. Docs: INV-02 already declared in ADR-003; `/promote-feature`; merge;
    `make release VERSION=v0.3.1` — depends on steps 8 and 9.

### Technical Dependencies

None external. .NET 10 SDK and the read-only v26n3 evidence folder on the
maintainer's machine.

## Monitoring and Observability

Operator-facing only: `.report.txt` lines (`contrib-names` WARNs;
`credit-roles` header-empty WARN; Phase 1 ORCID INFO and repeated-token
WARN), `.diagnostic.json` `phase3.creditStatement`, and the batch summary
pendency block. No telemetry.

## Technical Considerations

### Key Decisions

- Shape-based lookback for `;`-separated co-keys (ADR-003); alternatives:
  map-based lookback, reject, pre-normalize.
- Two-tier candidate matching with variant generation (ADR-004);
  alternatives: strict generation, drop given-only.
- `contrib-names` as its own injector, before `credit-roles`, warn-only
  (ADR-002); alternative: inside `credit-roles`.
- Outcome on `Phase3Context`, diagnostic gate unchanged (ADR-005);
  alternatives: recompute, structured report, always write.
- Plain ORCID as tokenizer token (ADR-006); alternative: post-process name.
- Keys before the first `:` also split on `,` for symmetry with the lookback;
  the dotted `Surname, I. N.` key form was already broken by the `.` split and
  is out of scope.
- Repeated-token WARN does not lower `AuthorConfidence` (the name is correct;
  Markup is what fails) — requires a `Warn` method on `AuthorBuilder` distinct
  from `MarkLow`.

### Known Risks

- Parser permissiveness admits an uppercase non-key token as a key →
  `NotFound` → prompt, visible in the diagnostic. Low impact.
- `ExtractAuthorsRule` change perturbs the Phase 2 curated corpus → gate
  fails → fix before release. Detected by `make phase2-verify`.
- `WritePhase3` signature change ripples to `DiagnosticWriterPhase3Tests` and
  `Phase3Processor`; mechanical.
- 5316 root cause is inferred; synthetic tests cover the mechanism, not the
  original file.

## Architecture Decision Records

- [ADR-001: Ship the CBAB v26n3 CRediT corrections as one patch release across Phase 1 and Phase 3](adrs/adr-001.md) — full scope, single release, `v0.3.1`.
- [ADR-002: A broken contributor name warns but never blocks CRediT injection](adrs/adr-002.md) — `contrib-names` is verification-only.
- [ADR-003: Widen the author-keyed CREDIT grammar with shape-based lookback, and never drop a present statement](adrs/adr-003.md) — `;`/`,` separators, initials-block lookback, `[p]` strip, INV-02.
- [ADR-004: Tiered, broadened candidate initials in the author resolver](adrs/adr-004.md) — full candidates before given-only; hyphen and particle variants.
- [ADR-005: The CRediT outcome is recorded on Phase3Context and read by the diagnostic and the batch summary](adrs/adr-005.md) — one computation, gate unchanged.
- [ADR-006: A plain-text ORCID in the byline is tokenized like a hyperlink ORCID](adrs/adr-006.md) — token path, repeated-token WARN.
