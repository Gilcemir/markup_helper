---
provider: manual
pr:
round: 1
round_created_at: 2026-05-06T17:49:40Z
status: resolved
file: DocFormatter.Cli/CliApp.cs
line: 55
severity: low
author: claude-code
provider_ref:
---

# Issue 008: CLI accepts any file path; non-.docx fails as a critical pipeline abort

## Review Comment

`RunSingleFile` dispatches on `File.Exists(path)` without checking the
extension:

```csharp
if (File.Exists(path))
{
    return RunSingleFile(path, stdout, stderr);
}
```

If the user accidentally passes a `.txt`, `.pdf`, or any other binary, the
flow goes: `File.Copy` to `formatted/<name>.docx` → `WordprocessingDocument.Open`
throws (file is not a Word package) → `FileProcessor.Process` catches generic
`Exception` → marks aborted → tries to record the last `[ERROR]` (which won't
exist because the pipeline never ran) → falls back to literal "critical
pipeline abort" → exit code 2.

The user-facing failure is misleading: the file IS a problem with the input
type, not a "critical pipeline abort." A `formatted/<name>.docx` copy is also
left on disk briefly before `TryDelete` removes it.

Suggested fix: in `CliApp.RunSingleFile` (or before it dispatches), check the
file extension:

```csharp
if (!string.Equals(Path.GetExtension(path), ".docx", StringComparison.OrdinalIgnoreCase))
{
    stderr.WriteLine($"error: only .docx files are supported, got '{Path.GetExtension(path)}'");
    return ExitUsageError;
}
```

Same guard applies to `RunBatch` (`Directory.EnumerateFiles` already filters
by `*.docx`, so batch is fine — single-file is the only path that needs the
check). Add a unit test that passes a `.txt` path and asserts exit code 1
with the expected error.

## Triage

- Decision: `VALID`
- Root cause: `RunSingleFile` dispatches purely on `File.Exists` without
  inspecting the extension. Non-`.docx` paths reach `WordprocessingDocument.Open`
  and surface as a misleading "critical pipeline abort" with exit code 2.
- Fix approach: in `CliApp.RunSingleFile` (or in `Run` before dispatch),
  reject paths whose extension isn't `.docx` (case-insensitive) with exit
  code 1 and a specific error message. Add a unit test that passes a `.txt`
  path and asserts exit 1 + the expected stderr.
- Notes:
