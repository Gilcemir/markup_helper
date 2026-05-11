---
status: completed
title: '`TagEmitter` helper — emit SciELO `[tag attr="v"]…[/tag]` literals as OpenXML Runs'
type: backend
complexity: medium
dependencies: []
---

# Task 02: `TagEmitter` helper — emit SciELO `[tag attr="v"]…[/tag]` literals as OpenXML Runs

## Overview
Every Phase 2 emitter rule needs the same primitive: insert a SciELO bracket-syntax tag literal into the Word document text flow as an OpenXML `Run`. Centralizing this primitive in a single static helper avoids each rule reinventing run construction, attribute serialization, the `Space=Preserve` invariant, and the `markup_sup_as` superscript trap. Bugs here propagate to every Phase 2 rule, so this helper carries dense unit-test coverage.

<critical>
- ALWAYS READ the PRD and TechSpec before starting
- REFERENCE TECHSPEC for implementation details — do not duplicate here
- FOCUS ON "WHAT" — describe what needs to be accomplished, not how
- MINIMIZE CODE — show code only to illustrate current structure or problem areas
- TESTS REQUIRED — every task MUST include tests in deliverables
</critical>

<requirements>
- MUST expose the public API defined in TechSpec "Core Interfaces → TagEmitter": `OpeningTag`, `ClosingTag`, `WrapParagraphContent`, `InsertOpeningBefore`, `InsertClosingAfter`.
- MUST emit attribute values with double quotes and no internal escaping (DTD 4.0 invariant from `docs/scielo_context/README.md`).
- MUST set `Space = SpaceProcessingModeValues.Preserve` on every emitted `Text` element.
- MUST reuse `RewriteHeaderMvpRule.CreateBaseRunProperties()` for run styling (Times New Roman 12pt) — do not invent a parallel base style.
- MUST zero `Run.RunProperties.VerticalTextAlignment` on wrapped runs whose original value was superscript, when `WrapParagraphContent` operates on a paragraph containing superscripted runs (the `markup_sup_as` trap from `docs/scielo_context/REENTRANCE.md`).
- MUST NOT emit any tag whose name appears in the anti-duplication list: `author`, `fname`, `surname`, `kwd`, `normaff`, `doctitle`, `doi`. The helper does not enforce this with an exception (rules choose their tag names) but documentation MUST flag it.
- SHOULD live under `DocFormatter.Core/TagEmission/` (new folder), as a single `static class TagEmitter`.
</requirements>

## Subtasks
- [x] 2.1 Create `DocFormatter.Core/TagEmission/TagEmitter.cs` with the static API surface defined in the TechSpec.
- [x] 2.2 Implement `OpeningTag` and `ClosingTag` as pure `Run` factories using `CreateBaseRunProperties()` and `Space=Preserve`.
- [x] 2.3 Implement `InsertOpeningBefore` and `InsertClosingAfter` to mutate the document tree by inserting Runs adjacent to a paragraph anchor.
- [x] 2.4 Implement `WrapParagraphContent` to wrap an existing paragraph's runs with opening + closing literals, including the superscript-zero handling.
- [x] 2.5 Add an internal helper for attribute serialization that emits `key="value"` pairs separated by single spaces, in the order received.
- [x] 2.6 Document the anti-duplication tag list in an XML doc comment on the class so future rule authors are warned.

## Implementation Details
The helper goes in a new folder `DocFormatter.Core/TagEmission/TagEmitter.cs`. Run-properties are produced by `DocFormatter.Core/Rules/RewriteHeaderMvpRule.cs` `CreateBaseRunProperties()` (lines ~116-126) — promote it to `internal` (or `public`) so `TagEmitter` can call it without copying. `Space = SpaceProcessingModeValues.Preserve` matches the existing convention used elsewhere in the rules. See TechSpec "Core Interfaces → TagEmitter" for the exact public surface.

### Relevant Files
- `DocFormatter.Core/Rules/RewriteHeaderMvpRule.cs` — owns `CreateBaseRunProperties()`; visibility may need to relax to `internal`.
- `DocFormatter.Core/Pipeline/IFormattingRule.cs` — confirms the call shape Phase 2 rules will use.
- `docs/scielo_context/README.md` — DTD 4.0 invariants on bracket syntax and attribute quoting.
- `docs/scielo_context/REENTRANCE.md` — `markup_sup_as` trap and anti-duplication tag list.

### Dependent Files
- All Phase 2 emitter rules (created in tasks 06, 07, 09) consume `TagEmitter`.
- `DocFormatter.Tests/TagEmitterTests.cs` — new test file.
- `DocFormatter.Tests/Fixtures/Phase2/Phase2DocxFixtureBuilder.cs` — fixtures with superscript-bearing paragraphs.

### Related ADRs
- [ADR-004: Pipeline Organization — Reuse `FormattingPipeline` with DI-Selected Rule Sets](adrs/adr-004.md) — `TagEmitter` lives under `DocFormatter.Core/TagEmission/`, separate from `Rules/Phase2/`, so it can be shared without coupling rules to each other.

## Deliverables
- New file `DocFormatter.Core/TagEmission/TagEmitter.cs` with the public API from the TechSpec.
- Visibility adjustment on `CreateBaseRunProperties()` if needed.
- New test file `DocFormatter.Tests/TagEmitterTests.cs`.
- Unit tests with 80%+ coverage **(REQUIRED)**.
- Integration tests covering the superscript-zero path on a real OpenXML paragraph **(REQUIRED)**.

## Tests
- Unit tests:
  - [x] `OpeningTag("abstract", [("language","en")])` produces a Run whose Text equals `[abstract language="en"]` with `Space=Preserve`.
  - [x] `ClosingTag("abstract")` produces a Run whose Text equals `[/abstract]` with `Space=Preserve`.
  - [x] `OpeningTag` with multiple attributes emits them in the order supplied, separated by single spaces.
  - [x] `OpeningTag` with empty attribute list emits `[tagname]` (no trailing space).
  - [x] `OpeningTag` with attribute value containing a literal `]` is emitted as-is (DTD 4.0 raw value, no escaping).
  - [x] Emitted Runs use the same RunProperties shape as `CreateBaseRunProperties()` (Times New Roman 12pt).
  - [x] `InsertOpeningBefore(p, …)` places the opening Run as the previous sibling of `p`'s first inline.
  - [x] `InsertClosingAfter(p, …)` places the closing Run as the next sibling of `p`'s last inline.
  - [x] `WrapParagraphContent(p, "abstract", …)` produces opening Run, then `p`'s original runs, then closing Run, in that order.
- Integration tests:
  - [x] `WrapParagraphContent` over a paragraph containing one superscript run zeros `VerticalTextAlignment` on that run; non-superscript siblings remain untouched.
  - [x] Round-trip: emit opening + closing on a real `WordprocessingDocument`, save and reload it, assert the literals appear in document text in the expected order.
- Test coverage target: >=80%
- All tests must pass

## Success Criteria
- All tests passing
- Test coverage >=80%
- `TagEmitter` is the single emission primitive used by every Phase 2 rule (verified by inspection in tasks 06, 07, 09).
- No emitted tag literal trips Markup's `markup_sup_as` (verified indirectly via the corpus diff gate in task 06).
