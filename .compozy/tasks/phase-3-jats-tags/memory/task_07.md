# Task Memory: task_07.md

Keep only task-local execution context here. Do not duplicate facts that are obvious from the repository, task file, PRD documents, or git history.

## Objective Snapshot
DONE. `OtherIdInjector` injects `<article-id pub-id-type="other">` right after the
DOI `<article-id>`, using `Phase3Context.OtherNumber`. Critical, idempotent,
fail-loud.

## Important Decisions
- Severity = Critical. Both fail paths (missing DOI, null/blank OtherNumber)
  THROW `InvalidOperationException`; the pipeline's catch reports the message as
  `report.Error(name, ex.Message)` and rethrows (Critical) → the "reported
  reason" is surfaced by the pipeline, not double-logged inside Apply.
- Idempotency check runs FIRST (before DOI/value checks) so a re-run with an
  existing `other` skips cleanly even if upstream value is now absent.
- Injected element name = `doi.Name.Namespace + "article-id"` (inherits the
  DOI's namespace, not hardcoded none) → handles a default-namespace doc and
  emits no redundant xmlns.
- Indentation depth is derived from the DOI's preceding XText (tab count after
  last `\n`), NOT hardcoded — corpus DOI sits at depth 3.

## Learnings
- `InsertAfter(anchor, injected, depth)` inserts an indent XText BETWEEN anchor
  and injected, so `anchor.NextNode` is XText. To grab the injected element in
  tests use `anchor.NodesAfterSelf().First(n => n is XElement)`.
- xUnit analyzer xUnit2031 forbids `.Where(...)` before `Assert.Single`; use the
  `Assert.Single(collection, predicate)` overload.
- Element lookup by `Name.LocalName == "article-id"` (not XName) keeps it robust
  to a default namespace.

## Files / Surfaces
- NEW `DocFormatter.Core/Jats/OtherIdInjector.cs` (IJatsInjector).
- NEW `DocFormatter.Tests/Jats/OtherIdInjectorTests.cs` (8 tests incl. corpus
  integration over e54492621.xml / other=00201).

## Errors / Corrections
- (resolved) initial test cast `doi.NextNode` to XElement → InvalidCastException
  (it is the indent XText). Fixed via NodesAfterSelf().

## Ready for Next Run
- Pattern set for injectors 08–10: derive depth from anchor's preceding XText,
  inherit anchor namespace, build via JatsXmlWriter helpers, idempotency first.
- Coverage 91.89% line on the injector (coverlet, see shared memory tooling note).
