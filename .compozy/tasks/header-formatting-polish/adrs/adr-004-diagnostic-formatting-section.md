# ADR-004: Additive `formatting` section in the diagnostic JSON

## Status

Accepted

## Date

2026-05-07

## Context

The MVP `DiagnosticDocument` exposes `file`, `status`, `extractedAt`, `fields` (DOI/ELOCATION/title/authors), and `issues`. Phase 2 introduces four behaviors whose effective state the editor wants to flag in the diagnostic JSON, but only when at least one of them emitted a `[WARN]`/`[ERROR]`. The PRD requires backward compatibility — consumers reading only the legacy keys must keep working.

Two natural shapes:

1. **Inline fields**, scattered across `DiagnosticFields`: e.g., add `correspondingEmail` next to `doi`. Existing record changes shape, but new fields are nullable and JSON deserializers ignore unknown keys.
2. **A new `formatting` sub-object**, added at the top level of `DiagnosticDocument`. Existing fields untouched.

The MVP's existing record uses positional constructor with `DiagnosticField`/`DiagnosticAuthor` types; sprinkling more loosely-typed fields into `DiagnosticFields` would dilute its meaning ("fields" today means "extracted scalar fields with confidence", not "rule outcomes").

## Decision

Add a single sibling property to `DiagnosticDocument`:

```csharp
public sealed record DiagnosticDocument(
    string File,
    string Status,
    DateTime ExtractedAt,
    DiagnosticFields Fields,
    DiagnosticFormatting? Formatting,
    IReadOnlyList<DiagnosticIssue> Issues);
```

Where `DiagnosticFormatting`:

```csharp
public sealed record DiagnosticFormatting(
    DiagnosticAlignment? AlignmentApplied,
    DiagnosticAbstract? AbstractFormatted,
    bool? AuthorBlockSpacingApplied,
    DiagnosticCorrespondingEmail? CorrespondingEmail);

public sealed record DiagnosticAlignment(bool Doi, bool Section, bool Title);

public sealed record DiagnosticAbstract(
    bool HeadingRewritten,
    bool BodyDeitalicized,
    bool InternalItalicPreserved);

public sealed record DiagnosticCorrespondingEmail(string? Value, string? Reason);
```

`Formatting` is `null` (and serialized as JSON `null` or omitted depending on `JsonIgnoreCondition`) when none of the four rules emitted a `[WARN]`/`[ERROR]`. When at least one of them did, `Formatting` is non-null and **only the affected sub-objects are populated**; unaffected sub-objects remain `null`.

Serializer keeps `JsonIgnoreCondition.Never` (matching MVP behavior) so consumers see explicit `"formatting": null` rather than a missing key — keeps the schema discoverable without losing backward compatibility (legacy consumers ignore the unknown key).

## Alternatives Considered

### Alternative 1: Inline fields in `DiagnosticFields`

- **Description**: Add `correspondingEmail`, `alignmentApplied`, etc., as fields on `DiagnosticFields`.
- **Pros**: Flatter JSON.
- **Cons**: `DiagnosticFields` currently means "extracted scalar values for the document" — alignment/spacing are rule outcomes, not extracted values. Mixing the two confuses the schema. Tests on `DiagnosticFields` (equality, serialization) would have to grow alongside.
- **Why rejected**: Schema cohesion; rule outcomes belong in their own bucket.

### Alternative 2: Per-rule top-level keys (`alignment`, `abstract`, etc.)

- **Description**: Four sibling keys at the document root.
- **Pros**: Direct mapping rule → key.
- **Cons**: Pollutes the top-level namespace with four nullable objects. Future rules (`PromoteSectionsRule`, `NormalizeQuotesRule`) would each add their own top-level key.
- **Why rejected**: A single `formatting` sub-object scales better as more rules ship.

### Alternative 3: Always populate `formatting` regardless of warnings

- **Description**: Build the section on every document, even when all four rules succeed.
- **Pros**: Diagnostic file always carries the most complete picture.
- **Cons**: PRD explicitly says: "populated only when the corresponding rule emitted `[WARN]`/`[ERROR]`". Diagnostic JSON exists today to flag papers that need a manual look — populating it on green runs noises the signal. (And `DiagnosticWriter` already short-circuits and writes nothing when `HighestLevel < Warn`.)
- **Why rejected**: PRD compliance and signal-to-noise.

## Consequences

### Positive

- Clean separation: `fields` = extracted data, `formatting` = rule outcomes.
- Future rules (e.g., `KeywordsRewriteRule`) drop into `DiagnosticFormatting` with no schema break.
- Serialization paths stay declarative (records + System.Text.Json + camelCase).

### Negative

- Two new record types (`DiagnosticFormatting`, `DiagnosticAlignment`, `DiagnosticAbstract`, `DiagnosticCorrespondingEmail`) live in `DiagnosticDocument.cs`.
- The `null` cascade ("only the affected sub-objects are populated") requires the writer to inspect specific report entries by rule name.

### Risks

- **Risk**: An external consumer parses `DiagnosticDocument` with strict mode and rejects unknown keys.
  - **Mitigation**: `Formatting` is additive; legacy consumers parse the existing keys and ignore the new one. No internal consumer is in scope.
- **Risk**: The writer needs to know which rule produced which entry to populate the sub-objects.
  - **Mitigation**: `ReportEntry.Rule` is already the rule's class name. Writer matches `nameof(...)` against the class names.

## Implementation Notes

- New types live in `DocFormatter.Core/Reporting/DiagnosticDocument.cs` (no new file).
- `DiagnosticWriter.Build` builds the `DiagnosticFormatting` from `report.Entries`, scoped to entries whose `Rule` matches one of the four new rule names.
- A `DiagnosticAlignment` field is `false` only if the alignment rule explicitly logged a `[WARN]` for that paragraph (e.g., "title paragraph not found in context"). When alignment succeeds for that paragraph, the field is `true`.
- `DiagnosticAbstract.InternalItalicPreserved` is `true` when the heuristic from ADR-002 took the "mixed italic" branch; `false` when the structural-wrapper branch ran (italic was stripped). This mirrors the editor's question: "did my species-name italic survive?"
- A new `DiagnosticCorrespondingEmail` carries `Value` (the extracted email) when extraction succeeded, or `Value=null` + `Reason` when the `*` marker was found but the email regex failed.
- Tests in `DiagnosticWriterTests.cs` add a fixture per branch (formatting absent on green, formatting populated on warn).

## References

- [PRD: Header Formatting Polish](../_prd.md) — User Experience section, item 3
- `DocFormatter.Core/Reporting/DiagnosticDocument.cs`
- `DocFormatter.Core/Reporting/DiagnosticWriter.cs`
