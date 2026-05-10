using DocumentFormat.OpenXml.Wordprocessing;

namespace DocFormatter.Core.Models.Phase2;

/// <summary>
/// Identifies the keywords paragraph so the <c>[kwdgrp language="…"]…[/kwdgrp]</c>
/// emitter knows where to wrap. <see cref="Keywords"/> retains the parsed
/// individual terms for diagnostics; the rule itself does not emit a
/// <c>[kwd]</c> per term — Markup auto-marks those (anti-duplication invariant
/// from <c>docs/scielo_context/REENTRANCE.md</c>).
/// </summary>
public sealed record KeywordsGroup(
    string Language,
    Paragraph Paragraph,
    IReadOnlyList<string> Keywords);
