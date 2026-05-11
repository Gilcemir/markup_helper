---
provider: manual
pr:
round: 1
round_created_at: 2026-05-11T18:23:22Z
status: resolved
file: DocFormatter.Core/Rules/Phase2/EmitAbstractTagRule.cs
line: 194
severity: high
author: claude-code
provider_ref:
---

# Issue 001: FindHeadingParagraph aborts entire body scan on first marker-prefix paragraph

## Review Comment

`FindHeadingParagraph` walks `body.Elements<Paragraph>()` looking for the abstract heading. Inside the inner `foreach (var marker in _options.AbstractMarkers)` loop, when a paragraph starts with a marker (e.g. "Abstract") but has more content after it (`afterMarker.IsEmpty == false`), the method does `return null;` (line 194). That `return` exits the *entire* `FindHeadingParagraph` function — every later paragraph is skipped.

Consequence: if any body paragraph that is **not** the heading happens to start with "Abstract" (e.g. "Abstract submission deadline: ..."), the real heading paragraph that appears later is never inspected. The rule then logs `AbstractHeadingNotFoundMessage` and the entire `[xmlabstr]` block is skipped, even though the heading does exist downstream.

The intent (judging from the comment at lines 192–194 "Reject paragraphs where the marker is just a prefix of a longer sentence") was to reject *this* paragraph, not abandon the search. Replace `return null;` with `break;` (or `continue` of the outer foreach) so the next paragraph is examined:

```csharp
if (afterMarker.IsEmpty)
{
    return paragraph;
}
// Marker is a prefix of a longer sentence — not the heading. Try next paragraph.
break;
```

Add a unit test that places a non-heading "Abstract submission ..." paragraph before the real "Abstract" heading and asserts the rule still emits `[xmlabstr]`.

## Triage

- Decision: `RESOLVED`
- Notes: Changed `return null;` → `break;` in `FindHeadingParagraph`. Added regression test `Apply_NonHeadingAbstractPrefixBeforeRealHeading_StillWrapsRealHeading` in `EmitAbstractTagRuleTests.cs`.
