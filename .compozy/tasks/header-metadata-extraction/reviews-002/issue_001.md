---
provider: manual
pr:
round: 2
round_created_at: 2026-05-06T18:49:17Z
status: resolved
file: DocFormatter.Core/Rules/ExtractOrcidLinksRule.cs
line: 81
severity: critical
author: claude-code
provider_ref:
---

# Issue 001: ORCID hyperlink wrapping the author name destroys the name

## Review Comment

When an ORCID hyperlink wraps the **author name** (rather than a separate
ORCID badge/icon), the current implementation drops the name during ORCID
extraction. The two header-relevant author records end up with `name == ""`
in `ctx.Authors`, and the rewritten document has no author lines at all.

Reproduced live on `examples/1_AR_5449_2.docx`. Output evidence in
`examples/formatted/1_AR_5449_2.report.txt`:

```
[INFO] ExtractOrcidLinksRule — extracted ORCID '0009-0007-2181-5830' at run index 0
[INFO] ExtractOrcidLinksRule — extracted ORCID '0000-0002-7970-9359' at run index 3
[WARN] ParseAuthorsRule — author #1 (''): empty name fragment
[WARN] ParseAuthorsRule — author #2 (''): empty name fragment
[INFO] ParseAuthorsRule — detected 2 author(s) with 3 distinct affiliation label(s)
[INFO] RewriteHeaderMvpRule — rewrote header with 0 author paragraph(s) (skipped 2 empty-name record(s))
```

`examples/formatted/1_AR_5449_2.diagnostic.json` confirms the parser collected
the affiliation labels (`["1"]`, `["1","2","*"]`) and ORCIDs correctly, but
`name` is empty for both records.

### Source structure of the authors paragraph

`word/document.xml` for `1_AR_5449_2.docx`:

```
<w:p>
  <w:pPr/>
  <w:hyperlink r:id="rId9">    <!-- target: https://orcid.org/0009-0007-2181-5830 -->
    <w:r><w:t>Thi Thanh Nga Le</w:t></w:r>
  </w:hyperlink>
  <w:r><w:rPr><w:vertAlign w:val="superscript"/></w:rPr><w:t>1</w:t></w:r>
  <w:r><w:t> and </w:t></w:r>
  <w:hyperlink r:id="rId10">   <!-- target: https://orcid.org/0000-0002-7970-9359 -->
    <w:r><w:t>Hoang Dang Khoa Do</w:t></w:r>
  </w:hyperlink>
  <w:r><w:rPr><w:vertAlign w:val="superscript"/></w:rPr><w:t>1,2</w:t></w:r>
  <w:r><w:rPr><w:vertAlign w:val="superscript"/></w:rPr><w:t>*</w:t></w:r>
</w:p>
```

The ORCID hyperlinks (`rId9`, `rId10`) wrap the author names, not separate
icon images. This is a real production input shape that the editor will hit.

### Failure path

`DocFormatter.Core/Rules/ExtractOrcidLinksRule.cs:78-84`:

```csharp
var runIndex = CountRunChildrenBefore(authors, hyperlink);
var runProperties = FindFirstTextRunProperties(hyperlink);

var replacement = BuildPlainRun(orcidId, runProperties);
authors.ReplaceChild(replacement, hyperlink);

ctx.OrcidStaging[runIndex] = orcidId;
```

`ReplaceChild(replacement, hyperlink)` swaps the entire hyperlink (including
its inner `<w:r><w:t>Author Name</w:t></w:r>`) for a single `Run` whose only
text is the bare ORCID ID. The author name is gone from the DOM.

`ParseAuthorsRule.TokenizeAndSplit` (line 51) then iterates only
`paragraph.Elements<Run>()` — direct `<w:r>` children of the paragraph — so
even hyperlinks the ORCID rule did not touch would be invisible to the
tokenizer. With this input, every name fragment between the superscripts and
the staged ORCID indices is empty, producing two `Confidence=Low` records
with empty `Name` and the warnings above.

## Triage

- Decision: `VALID`
- Root cause: `ExtractOrcidLinksRule.Apply` discards the inner text of an
  ORCID-targeted hyperlink when replacing it with a plain ORCID-ID run. PRD §3
  ("the hyperlink itself is converted to plain text — the link relationship
  and any orphan ORCID icon image are removed") was implemented under the
  assumption that the hyperlink content is always an icon, but production
  articles also use the form where the hyperlink wraps the author name. The
  parser side is also blind to non-Run paragraph children, which compounds the
  issue when any non-ORCID hyperlink appears in the authors paragraph.
- Fix approach: resolved by issue 002 (merging the ORCID extraction and
  author parsing into a single rule that walks paragraph children once and
  treats hyperlink text as part of the current author name regardless of the
  hyperlink target). The merged rule must:
  - preserve the inner text of any hyperlink it encounters in the authors
    paragraph (ORCID or not), feeding it into the current author-name builder
    just like a plain Run;
  - still capture the ORCID ID from the hyperlink's URL and attach it to the
    current author;
  - still delete the now-unused hyperlink relationships and warn on
    free-standing ORCID badge images.
  Add an integration test fixture mirroring `1_AR_5449_2.docx`'s authors
  paragraph (two ORCID-targeted hyperlinks wrapping author names plus
  superscript affiliation runs) and assert that both authors come out with
  correct `Name`, `AffiliationLabels`, `OrcidId`, and `Confidence=High`.
- Notes:
  - This issue is verified resolved when running the CLI on
    `examples/1_AR_5449_2.docx` produces a `formatted/1_AR_5449_2.docx` with
    two author lines containing the names "Thi Thanh Nga Le" and
    "Hoang Dang Khoa Do" (plus their superscript labels and ORCIDs), and the
    report has zero `[WARN]` lines from the new merged rule on this input.
