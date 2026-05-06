# Task Memory: task_10.md

## Objective Snapshot

- Implement `LocateAbstractAndInsertElocationRule` (Optional severity) and tests.
- Insert a paragraph holding `ctx.ElocationId` immediately above the first body paragraph whose first run is bold and starts (case-insensitively) with any `FormattingOptions.AbstractMarkers` value.

## Important Decisions

- Constructor takes `FormattingOptions` (consistent with `ExtractTopTableRule` / `ExtractOrcidLinksRule`); rule reuses the singleton `AbstractMarkers` rather than redefining the marker list.
- Match uses `paragraph.Descendants<Run>().FirstOrDefault()` so a hyperlink-wrapped first run still works; bold detection mirrors the OpenXML default-true rule (`Bold` element present and `Val` either null or true).
- Public constants `AbstractNotFoundMessage` and `MissingElocationIdMessage` follow the message-as-public-const pattern from tasks 05/06/08/09 so tests and downstream tooling reference them, not literals.
- `OutputXml` byte-equality is used to assert "document unchanged" in negative-path tests; cheaper and more robust than re-walking the tree.

## Learnings

- Footnote/header/footer paragraphs live in their own `OpenXmlPart`s (`FootnotesPart`, `HeaderPart`, `FooterPart`) — `body.Elements<Paragraph>()` already excludes them with no extra filtering required.
- `Bold` in OpenXML is "default true" when the element is present without `Val`; an explicit `Val=false` means "off". `IsBold` must check both states.

## Files / Surfaces

- `DocFormatter.Core/Rules/LocateAbstractAndInsertElocationRule.cs` (new)
- `DocFormatter.Tests/LocateAbstractAndInsertElocationRuleTests.cs` (new, 11 tests including 6-rule pipeline)

## Errors / Corrections

- None.

## Ready for Next Run

- task_11 (CLI bootstrap) must register `LocateAbstractAndInsertElocationRule` last in the rule chain (after `RewriteHeaderMvpRule`) so the Abstract paragraph still exists at the moment of lookup; the rule depends only on `ctx.ElocationId` so it does not require any rule besides `ExtractTopTableRule`.
- Coverage tooling (coverlet collector) is not installed in the test project; coverage was assessed structurally rather than measured. Adding `coverlet.collector` is a small follow-up if a numeric threshold becomes mandatory.
