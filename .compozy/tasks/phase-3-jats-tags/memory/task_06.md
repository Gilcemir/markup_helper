# Task Memory: task_06.md

Keep only task-local execution context here. Do not duplicate facts that are obvious from the repository, task file, PRD documents, or git history.

## Objective Snapshot
DONE. Whitespace-preserving JATS load/save + indented-element builder so golden
diffs show only injected tags. Byte-identical no-op round-trip required.

## Important Decisions
- Pure `XDocument` round-trip CANNOT be byte-identical on this corpus: the parser
  normalises CRLF→LF and drops DOCTYPE-internal whitespace (`<!DOCTYPE article\r\n
  PUBLIC` → collapsed); `XmlWriter` emits empty elements as `<x />` (corpus uses
  `<x/>`). All three confirmed by spike against a real package file.
- Chosen design (hybrid): keep the original PROLOG (decl + DOCTYPE + leading ws)
  and EPILOG verbatim as strings; re-serialise ONLY the root element via
  `XmlWriter` with `Indent=false`, `NewLineHandling.Replace`+`NewLineChars=source`,
  `ConformanceLevel.Fragment`, `OmitXmlDeclaration=true`; then `Replace(" />","/>")`.
  → byte-identical no-op AND one-line diff per injected element.
- `JatsXmlWriter.Load` returns a `JatsDocument` wrapper carrying prolog/epilog/
  newline/BOM. The XDocument it exposes is what feeds `Phase3Context.Xml`; the
  wrapper owns `Save`. CLI (task_11) keeps the wrapper to save (output path may
  differ from input, so prolog must be captured at load, not re-read on save).

## Learnings
- Indentation text nodes must carry `\n` (in-memory normalised form); Save's
  `NewLineHandling.Replace` restores the file convention. Building them with the
  source `\r\n` would double-convert.
- xlink/mml namespace inheritance is automatic ONLY when the injected node is
  serialised as part of the root (root declares the prefix). Serialising the
  element STANDALONE re-emits a redundant `xmlns:xlink`. Test must assert against
  the full `Serialize()`, not the element's `ToString()`.
- `XElement.ToString()` keeps the `<x />` space; the corpus `<x/>` form only
  appears after `JatsDocument.Serialize/Save`. Watch test expectations.

## Files / Surfaces
- NEW `DocFormatter.Core/Jats/JatsXmlWriter.cs` (static: Load, Indent, BuildLeaf,
  BuildElement, InsertAfter) + `JatsDocument.cs` (Document, NewLine, Save, Serialize).
- NEW tests `DocFormatter.Tests/Jats/JatsXmlWriterTests.cs` (18 incl. builder/ns/decl)
  + `JatsXmlWriterIntegrationTests.cs` (corpus no-op byte-identical + 1-line diff).

## Errors / Corrections
- First builder test expected `<title-group></title-group>`; `XElement.ToString`
  yields `<title-group />`. Fixed expectation (see Learnings).

## Ready for Next Run
- Injectors 07–10 build nodes via `JatsXmlWriter.BuildLeaf/BuildElement` and place
  them with `InsertAfter(anchor, node, depth)` (depth = anchor's tab count).
- CLI (11): `var jdoc = JatsXmlWriter.Load(path); ... ctx.Xml = jdoc.Document;
  pipeline.Run(...); jdoc.Save(outPath);`.
- Coverage 97.43% line / 100% method for the two classes (`coverlet` full path,
  default json, read ASCII table — per shared memory).
