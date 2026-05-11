using DocumentFormat.OpenXml.Wordprocessing;

namespace DocFormatter.Core.Models.Phase2;

/// <summary>
/// Identifies the keywords paragraph so the <c>[kwdgrp language="…"]…[/kwdgrp]</c>
/// emitter knows where to wrap. <see cref="Keywords"/> retains the parsed
/// individual terms; the emitter wraps each in <c>[kwd]…[/kwd]</c> per
/// ADR-001's 2026-05-11 follow-up note — Phase 2 owns the inner
/// <c>[kwd]</c> / <c>[sectitle]</c> / <c>[p]</c> / <c>[email]</c> literals
/// alongside the outer wrappers, since Markup does not duplicate them when
/// pre-marked.
/// </summary>
public sealed record KeywordsGroup(
    string Language,
    Paragraph Paragraph,
    IReadOnlyList<string> Keywords);
