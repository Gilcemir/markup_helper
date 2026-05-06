# Task Memory: task_12.md

Keep only task-local execution context here. Do not duplicate facts that are obvious from the repository, task file, PRD documents, or git history.

## Objective Snapshot

Diagnostic JSON serializer per ADR-004 schema. Writer is a Core static class that emits `<name>.diagnostic.json` next to the formatted `.docx` only when `report.HighestLevel >= Warn`. Wired through `FileProcessor.Process` after `ReportWriter.Write`.

## Important Decisions

- Writer lives in `DocFormatter.Core/Reporting/` (not Cli) so unit tests in `DocFormatter.Tests` can reach it without a CLI roundtrip; Cli's `FileProcessor` calls it. This matches the ReportWriter placement and Phase-1 testing-scope choice.
- `IReadOnlyList<...>` properties on `DiagnosticDocument`, `DiagnosticFields`, and `DiagnosticAuthor` need explicit `Equals`/`GetHashCode` overrides — default record equality on `IReadOnlyList` is reference-based and would break the round-trip integration assertion. Same pattern Author already used.
- DateTime is exposed as a real `DateTime` on the DTO (not pre-formatted string) plus a private `Iso8601SecondsDateTimeConverter` so callers/tests can compare temporal values cleanly. Format is `yyyy-MM-ddTHH:mm:ssZ`. `TruncateToSeconds` runs at Build time so the round-trip equality holds.
- Issues filter out `Info` entries deliberately — ADR-004's level enum is `warn|error` only and the `issues` field is a problem-list. Info still lands in `report.txt`.

## Learnings

- `JsonStringEnumConverter(JsonNamingPolicy.CamelCase)` produces the lowercase strings `high|medium|low|missing` directly from the `FieldConfidence` enum without per-value attributes.
- `DiagnosticWriter.JsonOptions` is exposed publicly so tests can deserialize with the same options used at write time — necessary because the converters live inside the writer.
- For "trigger one WARN" integration fixture, omitting the Abstract paragraph fires `LocateAbstractAndInsertElocationRule.AbstractNotFoundMessage`. Reused the existing `BuildBody(includeTopTable, includeAbstract)` signature; only added `WriteDocxWithoutAbstract`.

## Files / Surfaces

- `DocFormatter.Core/Reporting/FieldConfidence.cs` (new) — enum {High, Medium, Low, Missing}
- `DocFormatter.Core/Reporting/DiagnosticDocument.cs` (new) — `DiagnosticDocument`, `DiagnosticFields`, `DiagnosticField`, `DiagnosticAuthor`, `DiagnosticIssue`
- `DocFormatter.Core/Reporting/DiagnosticWriter.cs` (new) — static `Write(...)` returns bool; `Build(...)` for tests; `JsonOptions` accessor
- `DocFormatter.Cli/FileProcessor.cs` (modified) — calls `DiagnosticWriter.Write` after `ReportWriter.Write`, regardless of abort, with original source file basename
- `DocFormatter.Tests/DiagnosticWriterTests.cs` (new) — 11 unit tests covering all subtask 12.5 scenarios
- `DocFormatter.Tests/CliIntegrationTests.cs` (modified) — added 3 integration tests + `WriteDocxWithoutAbstract` fixture builder

## Errors / Corrections

- (none — first pass compiled and 113/113 tests passed)

## Ready for Next Run

- task_13 (Windows publish + production article validation) can now consume `<name>.diagnostic.json` as part of acceptance evidence. The deserialized DTO is the contract.
