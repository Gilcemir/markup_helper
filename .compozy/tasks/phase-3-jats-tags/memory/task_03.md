# Task Memory: task_03.md

Keep only task-local execution context here. Do not duplicate facts that are obvious from the repository, task file, PRD documents, or git history.

## Objective Snapshot
Create `DocumentPairer` (ADR-004): pair XML↔docx↔other.txt on elocation-id,
cross-check DOI, fail loud (return reported reason, never throw to abort batch /
never guess) on missing elocation-id, DOI disagreement, no matching docx, or
missing other.txt entry. Consumes task_01 `DocxSource`/`DocxSourceReader` +
task_02 `OtherTable`.

## Important Decisions
- Return a `PairingResult` (Success(PairedDocument) | Failure(reason)) instead of
  throwing — fail-loud here = a reported, skippable reason so one bad pair does
  NOT abort the batch. Config errors (missing dir) still throw naturally.
- `PairedDocument` carries the already-parsed `DocxSource` (matching by elocatid
  requires reading each candidate docx anyway) — avoids the CLI re-reading it.
  Slight deviation from techspec data-flow which lists `DocxSourceReader.Read`
  as a separate step; the path is still exposed for that flow.
- XML keys read from `front/article-meta` DIRECT children only — there are many
  `<elocation-id>` per file (reference citations in `<back>`); only the
  article-meta one is the article's own. `<article-id pub-id-type="doi">` is also
  scoped to article-meta direct children.
- Two Pair overloads: file-based `Pair(xmlPath, markupSourceDir, otherTable)` +
  pre-loaded core `Pair(XDocument, xmlPath, IReadOnlyList<DocxCandidate>,
  otherTable)` for unit tests without real docx/files.
- DOI compared trimmed + OrdinalIgnoreCase (DOIs are case-insensitive); elocatid
  compared Ordinal.

## Learnings
- DONE & verified: 742 tests pass, new files format-clean, coverage 97.22% line /
  93.33% branch / 100% method on the DocumentPairer types (coverlet).
- Corpus is 1:1:1 (15 docx excl. `~$5449.docx` lock, 15 XML, 15 other.txt rows);
  every XML pairs. The `~$`-prefixed Word lock file MUST be filtered when scanning
  (would throw on open).
- docx submission-id filenames (5449.docx) do NOT encode the elocation-id; only
  the header `[doc]@elocatid` does — so matching requires reading every candidate.

## Files / Surfaces
- NEW `DocFormatter.Core/Jats/DocumentPairer.cs` (+ PairingResult, PairedDocument,
  DocxCandidate).
- NEW `DocFormatter.Tests/Jats/DocumentPairerTests.cs`.

## Errors / Corrections

## Ready for Next Run
