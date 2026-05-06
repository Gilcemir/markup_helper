# Task Memory: task_07.md

Keep only task-local execution context here. Do not duplicate facts that are obvious from the repository, task file, PRD documents, or git history.

## Objective Snapshot

- Implement `ExtractOrcidLinksRule` (Optional severity) + shared `HeaderParagraphLocator` and an internal `OrcidStaging` dictionary on `FormattingContext`. ADR-003 governs the behavior contract.

## Important Decisions

- Replacement is one new `<w:r>` per hyperlink, RunProperties cloned from the first inner Run that owns text. Keeps the "run-index" key in `OrcidStaging` stable.
- Hyperlink iteration uses `paragraph.Elements<Hyperlink>()` (direct children only). Schema-wise, hyperlinks under the authors paragraph are always direct children in the input format; nested hyperlinks aren't covered by the MVP heuristic.
- Free-standing badges scan uses `paragraph.Descendants<Drawing>()` filtering by `Ancestors<Hyperlink>().Any()`; this runs after replacement, so old nested drawings are already gone with their hyperlink.
- Relationship deletion is guarded by `IsRelationshipStillReferenced` (scans every descendant attribute for the rId) — keeps the door open for shared rIds without breaking the orphan-cleanup requirement.
- Test access to `internal OrcidStaging` is enabled via a new `DocFormatter.Core/Properties/AssemblyInfo.cs` with `[assembly: InternalsVisibleTo("DocFormatter.Tests")]`.

## Learnings

- `using DocumentFormat.OpenXml.Drawing` collides with `Wordprocessing` (Run, Paragraph, Hyperlink, RunProperties all duplicate). Import only what's needed (`using Blip = DocumentFormat.OpenXml.Drawing.Blip;`).
- `MainDocumentPart.AddHyperlinkRelationship(uri, isExternal=true)` works for in-memory test docs; OpenXml accepts `file:///...#orcid.org/...` URIs and exposes them via `Uri.ToString()` with the fragment intact, which is what the substring marker check needs.
- `IdPartPair.OpenXmlPart` on an in-memory `WordprocessingDocument` returns `null` for relationships added with `AddExternalRelationship`; `ResolveRelationshipTarget` falls back to `ExternalRelationships` first to handle this.

## Files / Surfaces

- New: `DocFormatter.Core/Rules/HeaderParagraphLocator.cs`
- New: `DocFormatter.Core/Rules/ExtractOrcidLinksRule.cs`
- New: `DocFormatter.Core/Properties/AssemblyInfo.cs`
- New: `DocFormatter.Tests/ExtractOrcidLinksRuleTests.cs`
- Modified: `DocFormatter.Core/Pipeline/FormattingContext.cs` (added `internal Dictionary<int, string> OrcidStaging`)

## Errors / Corrections

- First build pass surfaced 6 `CS0104` ambiguity errors from `using DocumentFormat.OpenXml.Drawing`; fixed by replacing the namespace import with a `Blip` alias only.

## Ready for Next Run

- task_08 (`ParseAuthorsRule`) consumes `ctx.OrcidStaging` and reuses `HeaderParagraphLocator.FindAuthorsParagraph(body)`. The staging keys are run-indexes within the post-mutation authors paragraph (count of `Run` siblings before the original hyperlink).
