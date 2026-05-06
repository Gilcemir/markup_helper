---
provider: manual
pr:
round: 1
round_created_at: 2026-05-06T17:49:40Z
status: resolved
file: DocFormatter.Cli/FileProcessor.cs
line: 67
severity: medium
author: claude-code
provider_ref:
---

# Issue 006: Outer try/catch swallows OperationCanceledException

## Review Comment

`FileProcessor.Process` wraps the pipeline run in a broad catch:

```csharp
try
{
    using var doc = WordprocessingDocument.Open(copyPath, isEditable: true);
    pipeline.Run(doc, ctx, report);
}
catch (Exception ex)
{
    aborted = true;
    _logger.Error(ex, "pipeline aborted for {File}", name);
}
```

`FormattingPipeline.Run` carefully rethrows `OperationCanceledException`
without recording it as a rule error (line 22), and the workflow memory
explicitly notes: "Rule implementations MUST NOT catch
`OperationCanceledException` — the orchestrator rethrows OCE before any
severity-based handling, so swallowing it inside a rule breaks cancellation
propagation." The same principle applies one level up. This catch silently
converts cancellation into a `CriticalAbort` outcome.

The MVP doesn't yet thread a `CancellationToken` through the pipeline, so
this is currently latent. But Phase 2 will plausibly want batch cancellation
(Ctrl+C on a large folder), and the next person who wires that up will be
surprised to find OCE absorbed here.

Suggested fix:

```csharp
catch (OperationCanceledException)
{
    throw;
}
catch (Exception ex)
{
    aborted = true;
    _logger.Error(ex, "pipeline aborted for {File}", name);
}
```

Same pattern as `FormattingPipeline.Run`. Optional: add a TODO at the top of
`Process` noting that a cancellation token will need to be threaded through
when batch cancellation lands.

## Triage

- Decision: `VALID`
- Root cause: the broad `catch (Exception ex)` block in `FileProcessor.Process`
  also catches `OperationCanceledException`, breaking cooperative cancellation
  semantics. The pipeline carefully propagates OCE; this catch absorbs it.
- Fix approach: add `catch (OperationCanceledException) { throw; }` ahead of
  the generic catch (same pattern as `FormattingPipeline.Run`). No new test —
  the MVP doesn't yet thread a `CancellationToken` and adding one would be
  out of scope; the change is a one-line guard that becomes meaningful when
  cancellation is wired in Phase 2.
- Notes:
