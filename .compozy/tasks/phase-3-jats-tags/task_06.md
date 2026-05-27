---
status: completed
title: XmlWriter whitespace-preserving helper
type: backend
complexity: medium
dependencies: []
---

# Task 6: XmlWriter whitespace-preserving helper

## Overview
Provide a helper to load the JATS XML preserving its existing whitespace and save
it without reformatting, plus a way to build injected elements indented to match
the surrounding style. This keeps golden-corpus diffs limited to the injected
tags and honors "the XML is the only artifact modified."

<critical>
- ALWAYS READ the PRD and TechSpec before starting
- REFERENCE TECHSPEC for implementation details — do not duplicate here
- FOCUS ON "WHAT" — describe what needs to be accomplished, not how
- MINIMIZE CODE — show code only to illustrate current structure or problem areas
- TESTS REQUIRED — every task MUST include tests in deliverables
</critical>

<requirements>
- MUST load with `XDocument.Load(path, LoadOptions.PreserveWhitespace)`.
- MUST save without re-indenting existing nodes (formatting disabled), preserving
  the XML declaration and any DOCTYPE.
- MUST preserve the document namespaces (`xlink`, `mml`) so injected elements
  inherit them from the root.
- MUST provide a helper to construct an injected `XElement` (and its children)
  with indentation/newlines matching the surrounding sibling style, so untouched
  lines remain byte-identical.
- MUST NOT alter the `specific-use` version attribute (e.g. `sps-1.9`); version
  drift is flagged elsewhere, not rewritten here (TechSpec Known Risks).
</requirements>

## Subtasks
- [x] 6.1 Implement whitespace-preserving load.
- [x] 6.2 Implement non-reformatting save with declaration/DOCTYPE intact.
- [x] 6.3 Provide an indented-element builder matching sibling indentation.
- [x] 6.4 Verify namespace inheritance for injected elements.

## Implementation Details
Create `DocFormatter.Core/Jats/JatsXmlWriter.cs` (or `XmlIo.cs`). Uses
`System.Xml.Linq`. See TechSpec "Technical Considerations" (whitespace-preserving
XML write) and "Integration Points" (namespace/DOCTYPE preservation). Injectors
(task_07–task_10) use the element builder; the CLI (task_11) calls load/save.

### Relevant Files
- `DocFormatter.Core/Jats/JatsXmlWriter.cs` — new helper (create).
- `examples/phase-3/scielo_package/*.xml` — indentation/namespace reference (tab-indented, `xmlns:xlink`/`xmlns:mml`).

### Dependent Files
- All four injectors (task_07–task_10) build elements via the helper.
- CLI wiring (task_11) loads and saves the XDocument.

### Related ADRs
- [ADR-003: Dedicated IJatsInjector pipeline](../adrs/adr-003.md) — XDocument is the mutation target.

## Deliverables
- `JatsXmlWriter` with preserve-whitespace load, non-reformatting save, and an indented-element builder.
- Unit tests with 80%+ coverage **(REQUIRED)**
- Integration test asserting a minimal diff after a no-op load/save **(REQUIRED)**

## Tests
- Unit tests:
  - [x] Load+save of an unmodified corpus XML produces byte-identical output (round-trip).
  - [x] XML declaration and DOCTYPE survive the round-trip.
  - [x] An injected element built via the helper carries tab indentation matching its siblings.
  - [x] An injected element using `xlink:href` resolves the root-declared namespace (no redundant xmlns).
- Integration tests:
  - [x] Inserting one element into a corpus XML and saving changes only the injected lines (diff shows the new tag and nothing else).
- Test coverage target: >=80% (achieved 97.43% line / 100% method for JatsXmlWriter + JatsDocument)
- All tests must pass (634 passing)

## Success Criteria
- All tests passing
- Test coverage >=80%
- No-op round-trip is byte-identical; injected nodes match surrounding indentation; namespaces preserved.
