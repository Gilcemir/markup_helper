# Validation Case 001 — MVP acceptance evidence

This file records the manual end-to-end validation that gates the Phase 1 MVP
(per `.compozy/tasks/header-metadata-extraction/_prd.md` Phased Rollout). It
must be filled in by the editor after running `docformatter.exe` on a real
Windows 10 machine against one production article from the editorial team.

## Build artifact

- **Built from**: macOS via `DocFormatter.Cli/publish.sh` (locked recipe per
  `adrs/adr-005.md`).
- **Artifact path**: `DocFormatter.Cli/bin/Release/net10.0/win-x64/publish/docformatter.exe`
- **Artifact size**: 105 MB (110 625 668 bytes) — slightly above the task
  spec's loose 50–100 MB bound. Likely attributable to the .NET 10 self-contained
  runtime plus `PublishReadyToRun=true` precompiled native code; flagged here
  for the editor to confirm the binary still runs on the target machine.
- **SHA-256**:
  `3a4b50b5568cec8be2c566c1877a4c1b91270d5503082f39aa4713c2aef15704`
- **Publish folder contents**:
  - `docformatter.exe` (single-file self-extracting bundle)
  - `docformatter.pdb`, `DocFormatter.Core.pdb` (debug symbols — not required at runtime)

No loose `.dll` siblings are present, satisfying the ADR-005
self-contained-single-file requirement.

## Environment under test

- **OS**: _to be filled by editor — must be Windows 10 with NO .NET runtime installed_
- **Date of run**: _to be filled — YYYY-MM-DD_
- **Machine identifier (optional)**: _to be filled — e.g., editor laptop hostname_

## Smoke test

Run on the Windows 10 machine:

```
docformatter.exe --version
```

- **Output observed**: _to be filled — should print the assembly informational version with no missing-runtime errors_

## Production article

- **Source filename or hash**: _to be filled — e.g., `AR_5424_3.docx`, sha256 ..._
- **Editorial team contact**: _to be filled — who supplied the file and on what date_

Run on the Windows 10 machine:

```
docformatter.exe path\to\real-article.docx
```

Expected outputs alongside the input: `formatted\real-article.docx`,
`formatted\real-article.report.txt`, and `formatted\real-article.diagnostic.json`
(only if any `[WARN]`/`[ERROR]` was logged).

## Extracted field values

Record the four scoped fields exactly as they appear in the rewritten output:

- **DOI (Line 1)**: _to be filled_
- **Article Title (preserved in place)**: _to be filled_
- **Authors (one paragraph each, with affiliation superscripts and ORCID where present)**:
  - _to be filled_
- **ELOCATION (paragraph inserted above the Abstract)**: _to be filled_

## Observed warnings / diagnostics

- **`[WARN]` or `[ERROR]` entries in `report.txt`**: _to be filled — paste the relevant lines, or write "none"_
- **Was a `diagnostic.json` produced?**: _yes / no — to be filled_
- **If yes, summary of flagged fields**: _to be filled_

## Word visual check

Open `formatted\real-article.docx` in Microsoft Word on Windows 10 and confirm:

- [ ] Document opens without Word complaints (no recovery dialog, no corruption warning).
- [ ] DOI is on Line 1.
- [ ] ELOCATION sits immediately above the Abstract paragraph.
- [ ] Article title is preserved in its original position.
- [ ] Each author is on its own paragraph with affiliation superscripts and ORCID IDs as plain text where present.
- [ ] Affiliations, history block, abstract body, keywords, body, tables, images, and references are unchanged.

## Editor sign-off

> By signing below, I confirm that the rewritten header is correct for all four
> scoped fields and that the manual time saved on this article is meaningful
> enough to keep using the tool — satisfying the PRD's MVP done criterion.

- **Editor name**: _to be filled_
- **Date**: _to be filled — YYYY-MM-DD_
- **Signed**: _to be filled (initials or signature line)_
