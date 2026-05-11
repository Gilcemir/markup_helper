---
provider: manual
pr:
round: 1
round_created_at: 2026-05-11T18:23:22Z
status: resolved
file: DocFormatter.Core/Rules/Phase2/EmitAuthorXrefsRule.cs
line: 564
severity: medium
author: claude-code
provider_ref:
---

# Issue 003: Regex objects allocated and compiled inside hot loops in author rewriter

## Review Comment

`RewriteAuthorInner` allocates `Regex` instances inside loops on every paragraph and every match:

1. Lines 430–432 — `afterXrefStarPattern` is built with `RegexOptions.Compiled` *inside* `RewriteAuthorInner`, which runs once per author paragraph. Each call re-allocates and re-compiles the regex.

2. Lines 564–565 — inside the `PlainTextAffLabelPattern.Replace(...)` MatchEvaluator, the code does `var already = new Regex($@"\[xref ref-type=""aff"" rid=""{Regex.Escape(affId)}""\]");` for *every match*. With N authors the loop allocates N regex objects, each compiled on first use. `RegexOptions.Compiled` is not even set, so this is the slow interpreted path on top of the per-match allocation.

The other patterns in this file are correctly hoisted as `static readonly Regex ... new(..., RegexOptions.Compiled)`. Bring these two into the same pattern:

```csharp
private static readonly Regex AfterXrefStarPattern = new(
    @"(\[xref ref-type=""aff""[^\]]*\][^\[]*\[/xref\])\s*\*",
    RegexOptions.Compiled);
```

For the per-match check inside `PlainTextAffLabelPattern.Replace`, the regex is just looking for a literal substring — replace with `inner.Contains($"[xref ref-type=\"aff\" rid=\"{affId}\"]", StringComparison.Ordinal)`:

```csharp
var marker = $"[xref ref-type=\"aff\" rid=\"{affId}\"]";
if (inner.Contains(marker, StringComparison.Ordinal))
{
    return m.Value;
}
```

Same correctness, no regex allocation, no compile cost.

## Triage

- Decision: `RESOLVED`
- Notes: Hoisted `AfterXrefStarPattern` to a static readonly compiled regex. Replaced the per-match `new Regex(…)` inside the `PlainTextAffLabelPattern.Replace` evaluator with a literal `inner.Contains(existingMarker, StringComparison.Ordinal)` check — same correctness, no regex compile.
