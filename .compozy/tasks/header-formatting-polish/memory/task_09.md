# Task Memory: task_09.md

Keep only task-local execution context here. Do not duplicate facts that are obvious from the repository, task file, PRD documents, or git history.

## Objective Snapshot
- Status: completed.
- Added the additive `formatting` section to `DiagnosticDocument` and taught `DiagnosticWriter.Build` to populate it from `report.Entries` keyed by the four Phase 2 rule class names.

## Important Decisions
- Sub-object population gate: a sub-object is non-null only when its rule emitted at least one `[WARN]`/`[ERROR]`, EXCEPT `AuthorBlockSpacingApplied` which is also populated when the spacing rule emitted ANY entry (info or warn). Rationale: alignment/abstract/email need warn semantics to distinguish "rule ran" from "rule failed somewhere", but spacing has a clean binary INFO outcome (`BlankLineInserted` / `BlankLineAlreadyPresent`) that maps directly to `true`.
- Reused rule message constants (`ApplyHeaderAlignmentRule.MissingDoiParagraphMessage`, `RewriteAbstractRule.AbstractNotFoundMessage`, `RewriteAbstractRule.StructuralItalicRemovedMessage`, `EnsureAuthorBlockSpacingRule.MissingAuthorBlockEndMessage`, `ExtractCorrespondingAuthorRule.EmailExtractionFailedMessage`, etc.) for the writer's matching logic — keeps the writer in lockstep with rule wording without string duplication.
- For `DiagnosticAbstract`: `AbstractNotFound` warn collapses to `(false, false, false)`; otherwise `HeadingRewritten=true` and `BodyDeitalicized = StructuralItalicRemovedMessage info present` and `InternalItalicPreserved = !BodyDeitalicized`.
- For `DiagnosticCorrespondingEmail`: only populated on `EmailExtractionFailedMessage` warn, with `Value=null` and `Reason=<message>`. Other ExtractCorrespondingAuthorRule warns (e.g., `SecondMarkerMessage`) leave the sub-object null per ADR-004's narrow scope (the editor cares about email recoverability, not duplicate markers).

## Learnings
- `DocFormatter.Core.Reporting` was previously isolated from `DocFormatter.Core.Rules`. Pulling rule-name constants into the writer adds a `using DocFormatter.Core.Rules;` dependency. Same project, no DI/circular-reference issue.
- Default record equality on `DiagnosticFormatting` (and nested `DiagnosticAlignment`/`DiagnosticAbstract`/`DiagnosticCorrespondingEmail`) is structural and works without explicit `Equals`/`GetHashCode` overrides because none of them carry collections.
- `DiagnosticDocument.Equals`/`GetHashCode` is custom because of `Issues.SequenceEqual`; adding `Formatting` only required threading `Equals(Formatting, other.Formatting)` and `hash.Add(Formatting)` into the existing implementation.

## Files / Surfaces
- `DocFormatter.Core/Reporting/DiagnosticDocument.cs` — added `Formatting` parameter (between `Fields` and `Issues`) plus 4 record types.
- `DocFormatter.Core/Reporting/DiagnosticWriter.cs` — added `BuildFormatting`, `FilterByRule`, `HasWarnOrError`, `BuildAlignment`, `BuildAbstract`, `BuildSpacingApplied`, `BuildCorrespondingEmail`, `HasWarnMessage`, `HasInfoMessage` helpers.
- `DocFormatter.Tests/DiagnosticWriterTests.cs` — 14 new tests covering the matrix from the task spec.

## Errors / Corrections

## Ready for Next Run
- task_10 can now wire the four rules in `CliApp.BuildServiceProvider`; the diagnostic JSON will already carry the `formatting` section once the rules emit warnings end-to-end.
