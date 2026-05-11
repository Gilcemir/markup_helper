using System.Text;
using System.Text.RegularExpressions;
using DocFormatter.Core.Models;
using DocFormatter.Core.Models.Phase2;
using DocFormatter.Core.Pipeline;
using DocFormatter.Core.TagEmission;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace DocFormatter.Core.Rules.Phase2;

/// <summary>
/// Patches every <c>[author role="nd"]…[/author]</c> literal block in the
/// body so the corresponding AFTER-corpus shape lines up. The rule:
///   1. enriches the <c>[author]</c> opening tag with <c>rid</c>,
///      <c>corresp</c>, <c>deceased</c> and <c>eqcontr</c> attributes;
///   2. wraps any plain ORCID inside the block in
///      <c>[authorid authidtp="orcid"]…[/authorid]</c>;
///   3. for the corresponding author, expands a plain
///      <c>&lt;digit&gt;[,]?\s*\*</c> trailer into
///      <c>[xref ref-type="aff" rid="aff&lt;digit&gt;"]&lt;digit&gt;[/xref][xref ref-type="corresp" rid="c1"]*[/xref]</c>;
///   4. populates <c>ctx.Authors</c>, <c>ctx.Affiliations</c> and
///      <c>ctx.CorrespondingAuthorIndex</c> from the parsed data.
///
/// <para>
/// Per ADR-001 the rule does NOT introduce <c>[author]</c>, <c>[fname]</c>,
/// <c>[surname]</c> or <c>[normaff]</c> literals. Author paragraphs whose
/// BEFORE shape is plain text (no <c>[author]</c> shell) are left untouched —
/// SciELO Markup auto-marks those tags downstream, and pre-marking would
/// duplicate per <c>docs/scielo_context/REENTRANCE.md</c>.
/// </para>
///
/// <para>
/// Per ADR-002 the rule skips and warns rather than aborting. Reason code
/// <see cref="AuthorsMissingMessage"/> is recorded when no
/// <c>[author role="nd"]</c> literals are found.
/// </para>
/// </summary>
public sealed class EmitAuthorXrefsRule : IFormattingRule
{
    public const string AuthorsMissingMessage = "authors_missing";

    private const string CorrespId = "c1";

    // Match the entire [author role="nd"]…[/author] block (non-greedy on inner
    // text). Group 1 captures the inner content. Singleline lets `.` cross
    // paragraph boundaries — should not happen in practice (one author per
    // paragraph) but defensive against incidental \n in the joined text.
    private static readonly Regex AuthorBlockPattern = new(
        @"\[author role=""nd""\](.*?)\[/author\]",
        RegexOptions.Singleline | RegexOptions.Compiled);

    private static readonly Regex AffXrefPattern = new(
        @"\[xref ref-type=""aff"" rid=""(?<rid>[A-Za-z]\w*)""\]",
        RegexOptions.Compiled);

    private static readonly Regex CorrespXrefPattern = new(
        @"\[xref ref-type=""corresp""[^\]]*\]",
        RegexOptions.Compiled);

    // ORCID is canonically 4-4-4-4 hex/digit, but the BEFORE corpus carries at
    // least one author with a malformed 4-4-4-3 ORCID (5419 author 7,
    // "0009-0006-2754-561"). The AFTER corpus wraps it anyway, so the regex
    // accepts the canonical form OR the short variant.
    private static readonly Regex OrcidPattern = new(
        @"\d{4}-\d{4}-\d{4}-\d{3}[\dX]?",
        RegexOptions.Compiled);

    private static readonly Regex AlreadyTaggedOrcidPattern = new(
        @"\[authorid[^\]]*\]\d{4}-\d{4}-\d{4}-\d{3}[\dX]?\[/authorid\]",
        RegexOptions.Compiled);

    // Plain-text "<digit>[,]?\s*\*" right after [/surname] (no [xref] in
    // between). The matching `\s*\*` is greedy on the asterisks; the digit may
    // be followed by an optional comma+whitespace before the asterisk
    // (`1*`, `1 *`, `1, *`, `1,*`).
    private static readonly Regex PlainCorrespMarkerPattern = new(
        @"\[/surname\](?<gap>\s*)(?<aff>\d+)(?<sep>\s*,?\s*)\*(?<trailing>\s*)",
        RegexOptions.Compiled);

    // [/surname]<digit>,<digit>* — two affs followed by corresp marker. Two
    // digits separated by a comma. Handles 5449's "1,2*" shape.
    private static readonly Regex PlainTwoAffCorrespPattern = new(
        @"\[/surname\](?<gap>\s*)(?<aff1>\d+),(?<aff2>\d+)(?<sep>\s*)\*(?<trailing>\s*)",
        RegexOptions.Compiled);

    // Plain-text author paragraphs (no [author] shell). Patterns require an
    // ORCID-shape lookahead so a stray digit-asterisk pair elsewhere in the
    // body never matches. The lookbehind only rejects DIGIT predecessors
    // (a name like "Silva1*" should match — letters are fine).
    private static readonly Regex PlainTextOneAffCorrespPattern = new(
        @"(?<!\d)(?<aff>\d+)(?<sep>\s*,?\s*)\*(?=\s*\d{4}-\d{4}-\d{4}-\d{3})",
        RegexOptions.Compiled);

    private static readonly Regex PlainTextTwoAffCorrespPattern = new(
        @"(?<!\d)(?<aff1>\d+),(?<aff2>\d+)(?<sep>\s*)\*(?=\s*\d{4}-\d{4}-\d{4}-\d{3})",
        RegexOptions.Compiled);

    // Plain-text aff label: a lone "<digit>\s+" before an ORCID shape, after
    // a non-digit predecessor. Used for plain-text author paragraphs where
    // each author has an aff label but no [xref] markup (e.g. 5313 authors
    // 8-14: "Silvana Silva Red Quintal 1 0000-…").
    private static readonly Regex PlainTextAffLabelPattern = new(
        @"(?<!\d)(?<aff>\d+)(?<gap>\s+)(?=\d{4}-\d{4}-\d{4}-\d{3})",
        RegexOptions.Compiled);

    // Captures `[xref ref-type="aff" rid="…"]<label>[/xref] *` (the corresp
    // marker `*` immediately after an aff xref). Used in step 4 to convert the
    // bare `*` into a structured `[xref ref-type="corresp"]*[/xref]`.
    private static readonly Regex AfterXrefStarPattern = new(
        @"(\[xref ref-type=""aff""[^\]]*\][^\[]*\[/xref\])\s*\*",
        RegexOptions.Compiled);

    // Unicode superscript aff label (¹²³⁴⁵⁶⁷⁸⁹⁰): the OpenXML body sometimes
    // carries the aff label as a real Unicode superscript character, with no
    // wrapping [xref] (5523 authors). Pattern wraps `<sup>` (optionally
    // followed by `*` for the corresp author) into structured xrefs, anchored
    // by the upcoming ORCID-shape.
    private static readonly Regex UnicodeSupAffPattern = new(
        @"(?<sup>[²³¹⁰-⁹]+)(?<corresp>\*?)(?<gap>\s*)(?=\d{4}-\d{4}-\d{4}-\d{3})",
        RegexOptions.Compiled);

    public string Name => nameof(EmitAuthorXrefsRule);

    public RuleSeverity Severity => RuleSeverity.Optional;

    public void Apply(WordprocessingDocument doc, FormattingContext ctx, IReport report)
    {
        ArgumentNullException.ThrowIfNull(doc);
        ArgumentNullException.ThrowIfNull(ctx);
        ArgumentNullException.ThrowIfNull(report);

        var body = doc.MainDocumentPart?.Document?.Body;
        if (body is null)
        {
            report.Warn(Name, AuthorsMissingMessage);
            return;
        }

        var authorRegionEnd = FindAuthorRegionEnd(body);
        var allAffIds = new SortedSet<string>(StringComparer.Ordinal);
        var authors = new List<Author>();
        int? correspIndex = null;
        var seenAuthorIndex = 0;
        var anyProcessed = false;

        foreach (var paragraph in body.Elements<Paragraph>())
        {
            if (paragraph == authorRegionEnd)
            {
                break;
            }

            var joined = JoinParagraphText(paragraph);
            var hasAuthorShell = AuthorBlockPattern.IsMatch(joined);
            var hasOrcidLikeContent = OrcidPattern.IsMatch(joined);
            var hasAffXref = AffXrefPattern.IsMatch(joined);

            if (!hasAuthorShell && !hasOrcidLikeContent && !hasAffXref)
            {
                continue;
            }

            var (rewritten, perAuthorMeta) = RewriteParagraphText(joined, hasAuthorShell);
            if (rewritten == joined && perAuthorMeta.Count == 0)
            {
                continue;
            }

            WriteParagraphText(paragraph, rewritten);
            anyProcessed = true;

            foreach (var meta in perAuthorMeta)
            {
                authors.Add(new Author(
                    Name: meta.NameHint ?? string.Empty,
                    AffiliationLabels: meta.AffIds.ToArray(),
                    OrcidId: meta.Orcid));
                foreach (var aff in meta.AffIds)
                {
                    allAffIds.Add(aff);
                }
                if (meta.IsCorresp && correspIndex is null)
                {
                    correspIndex = seenAuthorIndex;
                }
                seenAuthorIndex++;
            }
        }

        if (!anyProcessed)
        {
            report.Warn(Name, AuthorsMissingMessage);
            return;
        }

        if (authors.Count == 0)
        {
            report.Warn(Name, AuthorsMissingMessage);
            return;
        }

        // Only populate ctx.Authors when Phase 1 didn't already (Phase 2 in
        // isolation needs the data; Phase 1+2 chained should defer to Phase 1).
        if (ctx.Authors.Count == 0)
        {
            ctx.Authors.AddRange(authors);
        }

        if (correspIndex is not null && ctx.CorrespondingAuthorIndex is null)
        {
            ctx.CorrespondingAuthorIndex = correspIndex;
        }

        // Same "first writer wins" precedence as ctx.Authors / corresp index:
        // if Phase 1 already populated Affiliations (possibly with richer
        // Orgname/Orgdiv1/Country fields), defer to it.
        if (ctx.Affiliations is null)
        {
            ctx.Affiliations = allAffIds
                .Select(id => new Affiliation(id, ExtractLabelFromAffId(id)))
                .ToArray();
        }

        report.Info(
            Name,
            $"patched {authors.Count} author block(s); {allAffIds.Count} distinct affiliation(s); "
            + (correspIndex is { } i ? $"corresp at index {i}" : "no corresp marker found"));
    }

    // The author region runs from the top of the body to (but not including)
    // the first paragraph that opens a downstream block: a [normaff], a
    // [corresp], an [xmlabstr], a [hist], a [kwdgrp], or a corresp marker
    // paragraph (`* E-mail:` / `Corresponding author:`). Affiliation lines
    // that lack [normaff] markup look almost identical to author lines (digit
    // + text), so a paragraph that starts with `<digit>\s+<word>` and has no
    // ORCID-shape is considered a plain-text affiliation and ends the region.
    private static Paragraph? FindAuthorRegionEnd(Body body)
    {
        var seenAuthorContent = false;
        foreach (var paragraph in body.Elements<Paragraph>())
        {
            var text = JoinParagraphText(paragraph);

            if (text.Contains("[normaff", StringComparison.Ordinal)
                || text.Contains("[corresp", StringComparison.Ordinal)
                || text.Contains("[xmlabstr", StringComparison.Ordinal)
                || text.Contains("[hist", StringComparison.Ordinal)
                || text.Contains("[kwdgrp", StringComparison.Ordinal)
                || text.Contains("Scientific Editor:", StringComparison.OrdinalIgnoreCase))
            {
                return paragraph;
            }

            // Heuristic: once we've seen author content, a paragraph that
            // looks like a plain-text affiliation ("1 University of …", "²
            // Departamento de …") closes the region.
            if (seenAuthorContent && IsPlainAffiliationLikeParagraph(text))
            {
                return paragraph;
            }

            if (AuthorBlockPattern.IsMatch(text)
                || AffXrefPattern.IsMatch(text)
                || OrcidPattern.IsMatch(text))
            {
                seenAuthorContent = true;
            }

            // Corresp marker paragraph closes the region too.
            if (StartsWithCorrespParagraphMarker(text))
            {
                return paragraph;
            }
        }
        return null;
    }

    private static bool IsPlainAffiliationLikeParagraph(string text)
    {
        // A line that starts with "1 University …" / "² Faculty …" and has no
        // ORCID looks like an affiliation, not an author.
        if (OrcidPattern.IsMatch(text))
        {
            return false;
        }
        if (AffXrefPattern.IsMatch(text))
        {
            return false;
        }
        if (AuthorBlockPattern.IsMatch(text))
        {
            return false;
        }
        var trimmed = text.TrimStart();
        if (trimmed.Length < 4)
        {
            return false;
        }
        // Lead: ASCII digit (any) or Unicode superscript digit ⁰¹²³⁴⁵⁶⁷⁸⁹.
        if (!(char.IsDigit(trimmed[0])
            || trimmed[0] is '⁰' or '¹' or '²' or '³' or '⁴'
                          or '⁵' or '⁶' or '⁷' or '⁸' or '⁹'))
        {
            return false;
        }
        // 20-char minimum is a heuristic floor: shortest plausible affiliation
        // ("1 Universidade X" is ~17 chars) clears it; bare author tokens
        // like "1 Silva" do not. Tuned against the 10-pair corpus.
        return trimmed.Length > 20;
    }

    private static bool StartsWithCorrespParagraphMarker(string text)
    {
        var trimmed = text.TrimStart();
        if (trimmed.StartsWith("* E", StringComparison.OrdinalIgnoreCase)
            || trimmed.StartsWith("*E", StringComparison.OrdinalIgnoreCase)
            || trimmed.StartsWith("Corresponding author:", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        return false;
    }

    private (string Rewritten, IReadOnlyList<AuthorMeta> Meta) RewriteParagraphText(
        string joined,
        bool hasAuthorShell)
    {
        var meta = new List<AuthorMeta>();

        if (hasAuthorShell)
        {
            var rewritten = AuthorBlockPattern.Replace(joined, match =>
            {
                var inner = match.Groups[1].Value;
                var (newInner, perAuthor) = RewriteAuthorInner(inner);
                meta.Add(perAuthor);
                return string.Concat("[author", BuildAuthorAttrs(perAuthor), "]", newInner, "[/author]");
            });
            return (rewritten, meta);
        }

        // Plain-text author paragraph (no [author] shell). Apply the inner
        // transforms (corresp expansion + ORCID wrap) directly. Per ADR-001
        // anti-duplication we do NOT wrap the result in [author]/[fname]/
        // [surname] — Markup auto-marks those tags downstream.
        var (rewrittenInner, perAuthorMeta) = RewriteAuthorInner(joined);
        if (rewrittenInner == joined)
        {
            return (joined, meta);
        }
        meta.Add(perAuthorMeta);
        return (rewrittenInner, meta);
    }

    private static (string Inner, AuthorMeta Meta) RewriteAuthorInner(string inner)
    {
        // Step 1: detect existing aff xrefs.
        var affIds = new List<string>();
        foreach (Match m in AffXrefPattern.Matches(inner))
        {
            var rid = m.Groups["rid"].Value;
            if (!affIds.Contains(rid, StringComparer.Ordinal))
            {
                affIds.Add(rid);
            }
        }

        // Step 2: detect existing corresp xref or plain corresp markers.
        var hasCorrespXref = CorrespXrefPattern.IsMatch(inner);

        // Step 3: convert plain "<digit><digit>*" / "<digit>,<digit>*" trailers
        // (corresp author with no [xref] markup at all) into structured
        // [xref aff][xref corresp]. Two-aff form runs first so its match wins
        // before the single-aff form sees the same text.
        var twoAffMatch = PlainTwoAffCorrespPattern.Match(inner);
        if (twoAffMatch.Success)
        {
            var aff1 = twoAffMatch.Groups["aff1"].Value;
            var aff2 = twoAffMatch.Groups["aff2"].Value;
            var aff1Id = "aff" + aff1;
            var aff2Id = "aff" + aff2;
            if (!affIds.Contains(aff1Id, StringComparer.Ordinal))
            {
                affIds.Add(aff1Id);
            }
            if (!affIds.Contains(aff2Id, StringComparer.Ordinal))
            {
                affIds.Add(aff2Id);
            }
            inner = PlainTwoAffCorrespPattern.Replace(
                inner,
                m =>
                {
                    var trailing = m.Groups["trailing"].Value;
                    return string.Concat(
                        "[/surname]",
                        m.Groups["gap"].Value,
                        $"[xref ref-type=\"aff\" rid=\"{aff1Id}\"]{aff1}[/xref]",
                        ",",
                        $"[xref ref-type=\"aff\" rid=\"{aff2Id}\"]{aff2}[/xref]",
                        $"[xref ref-type=\"corresp\" rid=\"{CorrespId}\"]*[/xref]",
                        trailing);
                },
                count: 1);
            hasCorrespXref = true;
        }
        else
        {
            var oneAffMatch = PlainCorrespMarkerPattern.Match(inner);
            if (oneAffMatch.Success)
            {
                var aff = oneAffMatch.Groups["aff"].Value;
                var affId = "aff" + aff;
                if (!affIds.Contains(affId, StringComparer.Ordinal))
                {
                    affIds.Add(affId);
                }
                inner = PlainCorrespMarkerPattern.Replace(
                    inner,
                    m =>
                    {
                        // Preserve the comma (and surrounding whitespace) the
                        // operator typed between the aff label and the
                        // asterisk — the AFTER corpus keeps that punctuation
                        // verbatim between the two xref tags.
                        var sep = m.Groups["sep"].Value;
                        var trailing = m.Groups["trailing"].Value;
                        return string.Concat(
                            "[/surname]",
                            m.Groups["gap"].Value,
                            $"[xref ref-type=\"aff\" rid=\"{affId}\"]{aff}[/xref]",
                            sep,
                            $"[xref ref-type=\"corresp\" rid=\"{CorrespId}\"]*[/xref]",
                            trailing);
                    },
                    count: 1);
                hasCorrespXref = true;
            }
        }

        // Step 4: detect already-corresp via plain "*" between [/xref] and the
        // ORCID. Pattern only triggers when a plain `*` survives steps 2-3 in a
        // shape that wasn't a "no xref at all" case (e.g. `[/xref]*` literal).
        if (!hasCorrespXref && AfterXrefStarPattern.IsMatch(inner))
        {
            inner = AfterXrefStarPattern.Replace(
                inner,
                m => string.Concat(
                    m.Groups[1].Value,
                    $"[xref ref-type=\"corresp\" rid=\"{CorrespId}\"]*[/xref]"),
                count: 1);
            hasCorrespXref = true;
        }

        // Step 4b: plain-text author paragraphs (no [/surname] anchor and no
        // [xref] yet). The ORCID-shape lookahead bounds the match safely.
        if (!hasCorrespXref)
        {
            var plainTwoAffMatch = PlainTextTwoAffCorrespPattern.Match(inner);
            if (plainTwoAffMatch.Success)
            {
                var aff1 = plainTwoAffMatch.Groups["aff1"].Value;
                var aff2 = plainTwoAffMatch.Groups["aff2"].Value;
                var aff1Id = "aff" + aff1;
                var aff2Id = "aff" + aff2;
                if (!affIds.Contains(aff1Id, StringComparer.Ordinal))
                {
                    affIds.Add(aff1Id);
                }
                if (!affIds.Contains(aff2Id, StringComparer.Ordinal))
                {
                    affIds.Add(aff2Id);
                }
                inner = PlainTextTwoAffCorrespPattern.Replace(
                    inner,
                    m => string.Concat(
                        $"[xref ref-type=\"aff\" rid=\"{aff1Id}\"]{aff1}[/xref]",
                        ",",
                        $"[xref ref-type=\"aff\" rid=\"{aff2Id}\"]{aff2}[/xref]",
                        $"[xref ref-type=\"corresp\" rid=\"{CorrespId}\"]*[/xref]"),
                    count: 1);
                hasCorrespXref = true;
            }
            else
            {
                var plainOneAffMatch = PlainTextOneAffCorrespPattern.Match(inner);
                if (plainOneAffMatch.Success)
                {
                    var aff = plainOneAffMatch.Groups["aff"].Value;
                    var affId = "aff" + aff;
                    if (!affIds.Contains(affId, StringComparer.Ordinal))
                    {
                        affIds.Add(affId);
                    }
                    inner = PlainTextOneAffCorrespPattern.Replace(
                        inner,
                        m =>
                        {
                            var sep = m.Groups["sep"].Value;
                            return string.Concat(
                                $"[xref ref-type=\"aff\" rid=\"{affId}\"]{aff}[/xref]",
                                sep,
                                $"[xref ref-type=\"corresp\" rid=\"{CorrespId}\"]*[/xref]");
                        },
                        count: 1);
                    hasCorrespXref = true;
                }
            }
        }

        // Step 4d: Unicode superscript aff labels (¹²³ … ⁰-⁹). These appear
        // when an author paragraph carries the label as a real superscript
        // character with no [xref] wrap (5523 authors). Wrap each into
        // [xref ref-type="aff" rid="aff<n>"] and, if a `*` follows, also emit
        // the corresp xref.
        if (!hasCorrespXref)
        {
            var sawUnicodeCorresp = false;
            inner = UnicodeSupAffPattern.Replace(inner, m =>
            {
                var sup = m.Groups["sup"].Value;
                var aff = MapSuperscriptDigitsToAscii(sup);
                if (aff.Length == 0)
                {
                    return m.Value;
                }
                var affId = "aff" + aff;
                if (!affIds.Contains(affId, StringComparer.Ordinal))
                {
                    affIds.Add(affId);
                }
                var sb = new StringBuilder();
                sb.Append("[xref ref-type=\"aff\" rid=\"")
                    .Append(affId)
                    .Append("\"]")
                    .Append(sup)
                    .Append("[/xref]");
                if (m.Groups["corresp"].Length > 0)
                {
                    // The asterisk after the unicode superscript marks the
                    // corresp author. Emit it as plain `*` (matches the 5523
                    // AFTER shape, where the editor opted not to wrap the
                    // marker in [xref ref-type="corresp"]).
                    sb.Append('*');
                    sawUnicodeCorresp = true;
                }
                sb.Append(m.Groups["gap"].Value);
                return sb.ToString();
            });
            if (sawUnicodeCorresp)
            {
                hasCorrespXref = true;
            }
        }

        // Step 4c: plain-text aff label (no corresp). A lone "<digit>\s+" right
        // before an ORCID, with no [xref] tag in between, becomes a structured
        // [xref ref-type="aff" rid="aff<digit>"] tag. Triggers on authors that
        // arrived with the aff label as plain text (5313 authors 8-14).
        if (!hasCorrespXref)
        {
            // Only run for paragraphs without an existing [xref ref-type="aff"]
            // for the digits we're about to wrap. The Replace below applies to
            // every match by default; restrict to digits whose aff isn't
            // already cited.
            inner = PlainTextAffLabelPattern.Replace(inner, m =>
            {
                var aff = m.Groups["aff"].Value;
                var affId = "aff" + aff;
                // If this aff is already wrapped via [xref ref-type="aff"
                // rid="affN"]<aff>[/xref] anywhere in inner, skip — the digit
                // here is a different occurrence (e.g. body content) and
                // shouldn't be silently wrapped. The literal prefix is enough
                // to detect the wrapper; no regex needed.
                var existingMarker = $"[xref ref-type=\"aff\" rid=\"{affId}\"]";
                if (inner.Contains(existingMarker, StringComparison.Ordinal))
                {
                    return m.Value;
                }
                if (!affIds.Contains(affId, StringComparer.Ordinal))
                {
                    affIds.Add(affId);
                }
                return $"[xref ref-type=\"aff\" rid=\"{affId}\"]{aff}[/xref]" + m.Groups["gap"].Value;
            });
        }

        // Step 5: wrap plain ORCID(s) in [authorid authidtp="orcid"]…[/authorid].
        // Skip ORCIDs already inside an [authorid] wrapper. The wrapper's
        // precise span comes from AlreadyTaggedOrcidPattern.Matches — no
        // magic radius needed.
        var taggedSpans = AlreadyTaggedOrcidPattern.Matches(inner)
            .Select(m => (Start: m.Index, End: m.Index + m.Length))
            .ToArray();
        var orcid = (string?)null;
        var sb = new StringBuilder();
        var cursor = 0;
        foreach (Match m in OrcidPattern.Matches(inner))
        {
            // Skip if this ORCID match falls inside an already-tagged span.
            var inWrapper = false;
            foreach (var span in taggedSpans)
            {
                if (m.Index >= span.Start && m.Index < span.End)
                {
                    inWrapper = true;
                    break;
                }
            }
            sb.Append(inner, cursor, m.Index - cursor);
            if (inWrapper)
            {
                sb.Append(m.Value);
            }
            else
            {
                sb.Append("[authorid authidtp=\"orcid\"]")
                    .Append(m.Value)
                    .Append("[/authorid]");
                orcid ??= m.Value;
            }
            cursor = m.Index + m.Length;
        }
        sb.Append(inner, cursor, inner.Length - cursor);
        inner = sb.ToString();

        var nameHint = ExtractNameHint(inner);
        return (inner, new AuthorMeta(affIds, hasCorrespXref, orcid, nameHint));
    }

    private static string? ExtractNameHint(string inner)
    {
        // Best-effort name extraction from [fname]…[/fname] [surname]…[/surname].
        var fnameMatch = Regex.Match(inner, @"\[fname\](?<v>[^\[]*)\[/fname\]");
        var surnameMatch = Regex.Match(inner, @"\[surname\](?<v>[^\[]*)\[/surname\]");
        if (!fnameMatch.Success && !surnameMatch.Success)
        {
            return null;
        }
        var fname = fnameMatch.Success ? fnameMatch.Groups["v"].Value.Trim() : string.Empty;
        var surname = surnameMatch.Success ? surnameMatch.Groups["v"].Value.Trim() : string.Empty;
        var combined = (fname + " " + surname).Trim();
        return combined.Length == 0 ? null : combined;
    }

    private static string BuildAuthorAttrs(AuthorMeta meta)
    {
        var sb = new StringBuilder();
        sb.Append(" role=\"nd\"");
        if (meta.AffIds.Count > 0)
        {
            sb.Append(" rid=\"").Append(string.Join(' ', meta.AffIds)).Append('"');
        }
        sb.Append(" corresp=\"").Append(meta.IsCorresp ? "y" : "n").Append('"');
        sb.Append(" deceased=\"n\"");
        sb.Append(" eqcontr=\"nd\"");
        return sb.ToString();
    }

    // Matches a single SciELO bracket-syntax tag literal: `[tag]`,
    // `[tag attr="v"]`, or `[/tag]`. Used by WriteParagraphText to split the
    // rewritten string into per-tag and per-text-run segments so the Word
    // Markup VBA `color(tag)` mapping paints each tag literal independently.
    private static readonly Regex TagLiteralPattern = new(
        @"\[/?\w+[^\]]*\]",
        RegexOptions.Compiled);

    private static void WriteParagraphText(Paragraph paragraph, string newText)
    {
        var pPr = paragraph.GetFirstChild<ParagraphProperties>()?.CloneNode(deep: true)
            as ParagraphProperties;

        paragraph.RemoveAllChildren();
        if (pPr is not null)
        {
            paragraph.AppendChild(pPr);
        }

        if (newText.Length == 0)
        {
            return;
        }

        var cursor = 0;
        foreach (Match m in TagLiteralPattern.Matches(newText))
        {
            if (m.Index > cursor)
            {
                paragraph.AppendChild(BuildPlainRun(newText[cursor..m.Index]));
            }
            // Each tag literal goes in its own colored Run so the Word
            // Markup VBA `color(tag)` mapping paints per-tag.
            paragraph.AppendChild(TagEmitter.TagLiteralRun(m.Value));
            cursor = m.Index + m.Length;
        }
        if (cursor < newText.Length)
        {
            paragraph.AppendChild(BuildPlainRun(newText[cursor..]));
        }
    }

    private static Run BuildPlainRun(string text)
        => new(
            RewriteHeaderMvpRule.CreateBaseRunProperties(),
            new Text(text) { Space = SpaceProcessingModeValues.Preserve });

    private static string JoinParagraphText(Paragraph paragraph)
    {
        var sb = new StringBuilder();
        foreach (var node in paragraph.Descendants())
        {
            switch (node)
            {
                case Text t:
                    sb.Append(t.Text);
                    break;
                case Break:
                    sb.Append('\n');
                    break;
            }
        }
        return sb.ToString();
    }

    private static string MapSuperscriptDigitsToAscii(string sup)
    {
        var sb = new StringBuilder(sup.Length);
        foreach (var c in sup)
        {
            switch (c)
            {
                case '⁰': sb.Append('0'); break;
                case '¹': sb.Append('1'); break;
                case '²': sb.Append('2'); break;
                case '³': sb.Append('3'); break;
                case '⁴': sb.Append('4'); break;
                case '⁵': sb.Append('5'); break;
                case '⁶': sb.Append('6'); break;
                case '⁷': sb.Append('7'); break;
                case '⁸': sb.Append('8'); break;
                case '⁹': sb.Append('9'); break;
                default: return string.Empty;
            }
        }
        return sb.ToString();
    }

    private static string ExtractLabelFromAffId(string affId)
    {
        // "aff1" → "1"; "affA" → "A"; otherwise echo the id (best effort).
        if (affId.StartsWith("aff", StringComparison.OrdinalIgnoreCase) && affId.Length > 3)
        {
            return affId[3..];
        }
        return affId;
    }

    private sealed record AuthorMeta(
        IReadOnlyList<string> AffIds,
        bool IsCorresp,
        string? Orcid,
        string? NameHint);
}
