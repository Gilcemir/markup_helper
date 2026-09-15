namespace DocFormatter.Core.Jats;

/// <summary>The structural shape a CREDIT statement was recognized as.</summary>
public enum CreditShape
{
    /// <summary>Free narrative prose: no structured shape, never auto-applied (ADR-005).</summary>
    Prose,

    /// <summary>Role-keyed: <c>Term: author, author; Term: …</c>.</summary>
    RoleKeyed,

    /// <summary>Author-keyed: <c>Initials: term, term. Initials: …</c>.</summary>
    AuthorKeyed,
}

/// <summary>
/// One contributor's contributions parsed from a CREDIT statement: the written
/// <paramref name="AuthorKey"/> (initials/name as the author wrote it) and the
/// ordered, de-duplicated written role <paramref name="Terms"/> for that author.
/// </summary>
public sealed record CreditEntry(string AuthorKey, IReadOnlyList<string> Terms);

/// <summary>
/// A parsed CREDIT statement: the recognized <paramref name="Shape"/> and the
/// per-author <paramref name="Entries"/> (empty for <see cref="CreditShape.Prose"/>).
/// </summary>
public sealed record CreditStatement(CreditShape Shape, IReadOnlyList<CreditEntry> Entries);

/// <summary>
/// Parses the raw CREDIT statement body into the two structured shapes ADR-005
/// recognizes (role-keyed and author-keyed), normalizing both onto a per-author
/// <see cref="CreditEntry"/> list. Anything that is not cleanly structured —
/// free prose (e.g. <c>"All authors contributed to the study's conception"</c>),
/// or a layout the detectors cannot disambiguate — is returned as
/// <see cref="CreditShape.Prose"/> so the injector prompts instead of guessing.
/// </summary>
public static class CreditStatementParser
{
    /// <summary>
    /// Detects the shape of <paramref name="raw"/> and parses it. Role-keyed is
    /// tried first (every <c>;</c>-chunk is <c>label: values</c> and the first
    /// label maps to a CRediT term), then author-keyed (<c>keys: terms</c>
    /// entries separated by <c>.</c>, <c>;</c> or <c>,</c>, terms separated by
    /// <c>;</c> or <c>,</c>, at least one term mapping — ADR-003), else prose.
    /// </summary>
    public static CreditStatement Parse(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return new CreditStatement(CreditShape.Prose, Array.Empty<CreditEntry>());
        }

        var text = raw.Trim();
        if (TryParseRoleKeyed(text, out var roleEntries))
        {
            return new CreditStatement(CreditShape.RoleKeyed, roleEntries);
        }

        if (TryParseAuthorKeyed(text, out var authorEntries))
        {
            return new CreditStatement(CreditShape.AuthorKeyed, authorEntries);
        }

        return new CreditStatement(CreditShape.Prose, Array.Empty<CreditEntry>());
    }

    /// <summary>
    /// Role-keyed: <c>Conceptualization: Lopes DAPS, Nascimento IRN; Methodology: …</c>.
    /// Each <c>;</c>-chunk must be <c>label: comma-separated-authors</c>, and the
    /// first label must map to a CRediT term (the discriminator vs author-keyed).
    /// </summary>
    private static bool TryParseRoleKeyed(string text, out IReadOnlyList<CreditEntry> entries)
    {
        entries = Array.Empty<CreditEntry>();

        var builder = new EntryBuilder();
        var firstLabelChecked = false;
        foreach (var rawChunk in text.Split(';'))
        {
            var chunk = rawChunk.Trim().TrimEnd('.').Trim();
            if (chunk.Length == 0)
            {
                continue; // trailing separator artifact
            }

            var colon = chunk.IndexOf(':', StringComparison.Ordinal);
            if (colon < 0)
            {
                return false; // not a role-keyed chunk
            }

            var label = chunk[..colon].Trim();
            if (!firstLabelChecked)
            {
                if (!CreditTermTable.TryMap(label, out _))
                {
                    return false; // first label is not a CRediT term → not role-keyed
                }

                firstLabelChecked = true;
            }

            var authors = SplitTrim(chunk[(colon + 1)..], ',');
            if (authors.Count == 0 || label.Length == 0)
            {
                return false;
            }

            foreach (var author in authors)
            {
                builder.Add(author, label);
            }
        }

        if (!firstLabelChecked || builder.IsEmpty)
        {
            return false;
        }

        entries = builder.Build();
        return true;
    }

    /// <summary>
    /// Author-keyed (ADR-003): <c>ATAJ: Conceptualization; Methodology. DRSJ; TOS: Investigation, Data curation.</c>
    /// The text is split into <c>.</c>-segments; each must be <c>keys: rest</c>,
    /// where <c>keys</c> are <c>;</c>/<c>,</c>-separated initials sharing the same
    /// terms. <c>rest</c> is split on <c>;</c> and <c>,</c> into pieces read by
    /// shape: a piece with a <c>:</c> opens a new entry (<c>CDC: Methodology</c>);
    /// a piece that is an initials block (<see cref="AuthorInitialsResolver.IsInitialsToken"/>)
    /// is a pending key attached to the entry the next <c>:</c> piece opens
    /// (<c>Conceptualization; CFA; MN; CDC: Methodology</c> → CFA, MN and CDC share
    /// Methodology); any other piece is a term of the current entry. The statement
    /// is not author-keyed when a segment has no <c>:</c>, an entry ends without
    /// terms, pending keys are never closed by a <c>:</c> piece, an initials block
    /// is followed by a term, or no term maps to a CRediT term (the discriminator).
    /// </summary>
    private static bool TryParseAuthorKeyed(string text, out IReadOnlyList<CreditEntry> entries)
    {
        entries = Array.Empty<CreditEntry>();

        var builder = new EntryBuilder();
        var anyMappedTerm = false;
        foreach (var rawSegment in text.Split('.'))
        {
            var segment = rawSegment.Trim();
            if (segment.Length == 0)
            {
                continue; // trailing separator artifact (e.g. the closing '.')
            }

            var colon = segment.IndexOf(':', StringComparison.Ordinal);
            if (colon < 0)
            {
                return false; // a non-empty segment that is not "keys: …" → not cleanly author-keyed
            }

            var keys = SplitTrim(segment[..colon], ';', ',');
            if (keys.Count == 0)
            {
                return false; // ": terms" with no key
            }

            var terms = new List<string>();
            var pending = new List<string>();
            foreach (var piece in SplitTrim(segment[(colon + 1)..], ';', ','))
            {
                var pieceColon = piece.IndexOf(':', StringComparison.Ordinal);
                if (pieceColon >= 0)
                {
                    if (terms.Count == 0)
                    {
                        return false; // the entry being closed has no terms
                    }

                    anyMappedTerm |= Flush(builder, keys, terms);

                    // The pending initials blocks plus this piece's own key open the next entry.
                    pending.Add(piece[..pieceColon].Trim());
                    keys = pending.Where(k => k.Length > 0).ToList();
                    if (keys.Count == 0)
                    {
                        return false; // ": terms" with no key
                    }

                    pending = new List<string>();
                    terms = new List<string>();
                    var firstTerm = piece[(pieceColon + 1)..].Trim();
                    if (firstTerm.Length > 0)
                    {
                        terms.Add(firstTerm);
                    }
                }
                else if (AuthorInitialsResolver.IsInitialsToken(piece))
                {
                    pending.Add(piece); // co-key of the entry the next ':' piece opens
                }
                else if (pending.Count > 0)
                {
                    return false; // an initials block followed by a term instead of ':'
                }
                else
                {
                    terms.Add(piece);
                }
            }

            if (pending.Count > 0 || terms.Count == 0)
            {
                return false; // keys never closed by ':' / entry without terms → prompt
            }

            anyMappedTerm |= Flush(builder, keys, terms);
        }

        if (!anyMappedTerm || builder.IsEmpty)
        {
            return false;
        }

        entries = builder.Build();
        return true;
    }

    /// <summary>
    /// Adds every <paramref name="terms"/> to every <paramref name="keys"/> and
    /// reports whether any term maps to a CRediT term.
    /// </summary>
    private static bool Flush(EntryBuilder builder, IReadOnlyList<string> keys, IReadOnlyList<string> terms)
    {
        foreach (var key in keys)
        {
            foreach (var term in terms)
            {
                builder.Add(key, term);
            }
        }

        return terms.Any(t => CreditTermTable.TryMap(t, out _));
    }

    private static List<string> SplitTrim(string value, params char[] separators)
        => value.Split(separators)
            .Select(s => s.Trim())
            .Where(s => s.Length > 0)
            .ToList();

    /// <summary>
    /// Accumulates terms per author, preserving the first-seen author order and the
    /// term order while de-duplicating repeated terms for the same author.
    /// </summary>
    private sealed class EntryBuilder
    {
        private readonly List<string> _order = new();
        private readonly Dictionary<string, List<string>> _terms = new(StringComparer.Ordinal);

        public bool IsEmpty => _order.Count == 0;

        public void Add(string authorKey, string term)
        {
            if (!_terms.TryGetValue(authorKey, out var list))
            {
                list = new List<string>();
                _terms[authorKey] = list;
                _order.Add(authorKey);
            }

            if (!list.Contains(term, StringComparer.Ordinal))
            {
                list.Add(term);
            }
        }

        public IReadOnlyList<CreditEntry> Build()
            => _order.Select(key => new CreditEntry(key, _terms[key])).ToList();
    }
}
