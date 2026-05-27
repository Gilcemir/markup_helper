# Task Memory: task_08.md

Keep only task-local execution context here. Do not duplicate facts that are obvious from the repository, task file, PRD documents, or git history.

## Objective Snapshot
`EditedByInjector : IJatsInjector` — inject `<fn fn-type="edited-by">` (role in
`<label>`, name in `<p>`, NO `<ext-link>`/ORCID — DocxSource has no ORCID field)
into the single `<author-notes>`, creating it when absent. Idempotent; one fn per
editor role line found.

## Important Decisions
- Severity = **Optional** (not Critical): editor data is best-effort and may be
  absent (corpus 5487/5640 have no editor line). Missing editor / missing anchor
  → report Info + return (no document abort), matching IJatsInjector Optional model.
- Roles + labels: `AssociateEditor` → `ASSOCIATE EDITOR:`, `ScientificEditor` →
  `SCIENTIFIC EDITOR:`. Emit in associate-then-scientific order to match the
  responsible_editor.md example.
- Create-path placement: `<author-notes>` is inserted after the last `<aff>` (else
  last `<contrib-group>`) in `<article-meta>` — the JATS-valid slot before pub-date.
  No anchor → Info skip.
- Append-path: InsertAfter(lastElementChildOfAuthorNotes, fn, fnDepth) chained;
  preserves existing `<corresp>`. fnDepth = IndentDepthOf(author-notes)+1.

## Learnings
- `XElement.ToString()` on a namespaced element ALWAYS emits `xmlns` when
  serialized standalone (detached from in-scope decls). To assert "no redundant
  xmlns", check `element.DescendantsAndSelf().Attributes()` has no
  `IsNamespaceDeclaration`, NOT `.ToString()`. (Mirrors OtherIdInjectorTests using
  `.Name.Namespace` instead of string match.)
- DONE & verified: 651 tests pass, build 0 warn/0 err, format clean on new files,
  EditedByInjector line coverage 89.47% (branch 75%).

## Files / Surfaces
- NEW: `DocFormatter.Core/Jats/EditedByInjector.cs`
- NEW: `DocFormatter.Tests/Jats/EditedByInjectorTests.cs`
- Reuses `JatsXmlWriter.BuildLeaf/BuildElement/InsertAfter`, DocxSource editor fields.
- Corpus author-notes shape (e54492621): author-notes @depth3, corresp @depth4.

## Errors / Corrections

## Ready for Next Run
