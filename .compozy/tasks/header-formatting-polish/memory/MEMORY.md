# Workflow Memory

Keep only durable, cross-task context here. Do not duplicate facts that are obvious from the repository, PRD documents, or git history.

## Current State

## Shared Decisions
- `FormattingContext` is the cross-rule state surface for Phase 2: rules publish `Paragraph?` references and corresponding-author scalars on it. Invariant: a publishing rule MUST NOT delete its own published paragraph; a downstream rule that removes such a paragraph MUST null the field first. Documented inline in `DocFormatter.Core/Pipeline/FormattingContext.cs`.

## Shared Learnings
- xUnit + DocumentFormat.OpenXml: a bare `new Paragraph()` is enough to exercise context property round-trip — no `WordprocessingDocument` required.

## Open Risks

## Handoffs
- task_03 wrote `SectionParagraph` / `TitleParagraph`; task_04 wrote `DoiParagraph` / `AuthorBlockEndParagraph`. task_05 consumed them in `ApplyHeaderAlignmentRule`; task_06 consumed `AuthorBlockEndParagraph` in `EnsureAuthorBlockSpacingRule`. task_07 published `CorrespondingAffiliationParagraph` / `CorrespondingEmail` / `CorrespondingOrcid` / `CorrespondingAuthorIndex` and promoted the affiliation ORCID onto the matched author when that author had no prior ORCID.
- `AuthorBlockEndParagraph` is null when no renderable authors were appended (empty `ctx.Authors` OR all-empty-name records). Consumers must treat null as "no anchor → `[WARN]` and no-op", not as a precondition violation.
- task_09 will populate `DiagnosticFormatting.AuthorBlockSpacingApplied` from `EnsureAuthorBlockSpacingRule` entries; the rule exposes `BlankLineInsertedMessage`, `BlankLineAlreadyPresentMessage`, `MissingAuthorBlockEndMessage`, and `MissingAffiliationMessage` constants for keying. task_07's `ExtractCorrespondingAuthorRule` exposes `NoMarkerMessage`, `SecondMarkerMessage`, `EmailExtractionFailedMessage`, and `OrcidPromotedMessage` for the same purpose.
- task_08 (`RewriteAbstractRule`) is the rule that consumes `ctx.CorrespondingEmail`; task_07 only handles the `* E-mail:` affiliation path (per ADR-003 the typed-line fallback belongs to task_08). task_08's `RewriteAbstractRule` exposes message constants `StructuralItalicRemovedMessage`, `ResumoNormalizedMessage`, `AbstractNotFoundMessage`, `CanonicalLineInsertedMessage`, `RecoveredEmailMessage`, `MissingSeparatorMessage`, and `ReplacedTypedLineMessagePrefix` (a prefix; the rest of the INFO message contains `'<original text>'`) for task_09 to key on.
- task_10 must register `ExtractCorrespondingAuthorRule` BEFORE `RewriteHeaderMvpRule` so it operates on the original DOM, and must keep `EnsureAuthorBlockSpacingRule` after `ApplyHeaderAlignmentRule` and before `RewriteAbstractRule` in `CliApp.BuildServiceProvider`. `RewriteAbstractRule` MUST run before `LocateAbstractAndInsertElocationRule` so ELOCATION still inserts above the rewritten heading paragraph.
- task_09 wired `DiagnosticDocument.Formatting` and `DiagnosticWriter.BuildFormatting`. `Formatting` is null when none of the four Phase 2 rules emit `[WARN]`/`[ERROR]`. Per-rule sub-object semantics: alignment booleans flip to `false` only on the matching `Missing*ParagraphMessage` warn; abstract collapses to `(false,false,false)` on `AbstractNotFoundMessage`, otherwise `BodyDeitalicized` follows the presence of `StructuralItalicRemovedMessage` info; spacing toggles between `true`/`false` based on warn presence and is the only sub-object also populated when the rule logged only INFO; corresponding-email is populated only on `EmailExtractionFailedMessage` warn (other ExtractCorrespondingAuthorRule warns leave the sub-object null).
- task_10 wired the four Phase 2 rules in `CliApp.BuildServiceProvider` per ADR-001 order and exposed the method as `internal` so tests can assert ordering directly via `GetServices<IFormattingRule>()`. Phase 1 success criterion validated: all eleven `examples/` articles emit `formatting: null` (no Phase 2 warns) and the four rules log INFO only.
- Final-pipeline body order after RewriteAbstractRule + LocateAbstractAndInsertElocationRule both run: `[..., affiliation_n, Corresponding author: <email>, ELOCATION, **Abstract** heading, body]` — ELOCATION lands between the email line and the heading because LocateAbstractAndInsertElocationRule's `FindAbstractParagraph` matches on the bold heading paragraph created by RewriteAbstractRule and inserts before it. Future rules that need the email line adjacent to the heading must reorder this insertion or move ELOCATION earlier.
