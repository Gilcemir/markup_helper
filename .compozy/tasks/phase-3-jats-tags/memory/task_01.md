# Task Memory: task_01.md

Keep only task-local execution context here. Do not duplicate facts that are obvious from the repository, task file, PRD documents, or git history.

## Objective Snapshot
Parse read-only `.docx` markup source into a `DocxSource`: `[doc]` header keys
(elocatid, doi) + three trailing untagged sections (Scientific Editor, DATA
AVAILABILITY, CREDIT STATEMENT). Source of 3 of the 4 injected values + docx
side of pairing. New files under `DocFormatter.Core/Jats/`.

## Important Decisions
- Header keys (`ElocationId`, `Doi`) are `required` non-null → reader THROWS
  (`InvalidDataException`) if the `[doc]` header / keys are missing. Pairer
  (task_03) owns fail-loud reporting; reader enforces the model contract.
- Section extraction is **header-anchored, order-independent** (corpus mixes
  CS-before-DA and DA-before-CS). Body collected until next section header /
  bracket-tag paragraph (`[refs]`/`[corresp`) / REFERENCES.
- CREDIT body returned verbatim (only trimmed + multi-paragraph joined); shape
  detection deferred to task_10. DA body also kept faithful (NBSP preserved).
- Editor parsing normalizes NBSP (` `) and narrow-NBSP (` `) → space
  before regex; returns name with normal spaces.

## Learnings (corpus observations)
- `[doc]` header + `[doi]…[/doi]` are in the SAME first paragraph.
- 5313 DOI has a leading space inside the tag → must Trim.
- Editor whitespace variants in corpus: plain space, `\xa0` after colon,
  ` ` between "Scientific"/"Editor" and after colon, trailing `\xa0 `.
- 5487 and 5640 have NO `Scientific Editor:` line → ScientificEditor null.
- DA/CS ordering varies: most CS-before-DA, but 5517/5548/5570 are DA-before-CS.
- 5640 GLUED case: DA body paragraph ends with `…author.CREDIT STATEMENT`
  (header glued, no paragraph break) → must split header off the body.
- No `Associate Editor:` lines present anywhere in this corpus.

## Files / Surfaces
- NEW `DocFormatter.Core/Jats/DocxSource.cs`
- NEW `DocFormatter.Core/Jats/DocxSourceReader.cs`
- NEW `DocFormatter.Tests/Jats/DocxSourceReaderTests.cs`
- Ref: `DocFormatter.Core/Rules/ExtractAuthorsRule.cs` (OpenXml paragraph read)
- Ref: `DocFormatter.Tests/Phase2/Phase2CorpusTests.cs` (corpus dir resolution)

## Errors / Corrections
- C# string literals containing U+00A0/U+202F are hard to target with exact-match
  Edit (and accented chars vary by NFC/NFD). Used ASCII-only test literals +
  integration tests over the real corpus (5523 has `\xa0`) for NBSP coverage.

## Ready for Next Run
- DONE. `DocxSource` + `DocxSourceReader` implemented & verified.
- Coverage of `DocFormatter.Core.Jats.*`: Line 100% / Branch 91% / Method 100%.
- Full suite 590/590 green; build clean.
- Pre-existing `dotnet format` whitespace failures in
  `DocFormatter.Tests/Phase2/Phase2PipelineIntegrationTests.cs` (unrelated, not
  touched) — left for a separate cleanup, do NOT bundle into this task's diff.
- No auto-commit (per run config); diff left for manual review.
