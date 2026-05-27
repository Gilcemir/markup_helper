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
    /// label maps to a CRediT term), then author-keyed (<c>.</c>-separated
    /// <c>keys: terms</c> entries whose terms map), else prose.
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
    /// Author-keyed: <c>ATAJ: Conceptualization, Methodology. DRSJ; TOS: …</c>.
    /// Entries are <c>.</c>-separated; each is <c>keys: terms</c> where keys are
    /// <c>;</c>-separated initials sharing the comma-separated <c>terms</c>. At
    /// least one term must map to a CRediT term (the discriminator).
    /// </summary>
    private static bool TryParseAuthorKeyed(string text, out IReadOnlyList<CreditEntry> entries)
    {
        entries = Array.Empty<CreditEntry>();

        var builder = new EntryBuilder();
        var anyMappedTerm = false;
        foreach (var rawEntry in text.Split('.'))
        {
            var entry = rawEntry.Trim();
            if (entry.Length == 0)
            {
                continue; // trailing separator artifact (e.g. the closing '.')
            }

            var colon = entry.IndexOf(':', StringComparison.Ordinal);
            if (colon < 0)
            {
                return false; // a non-empty segment that is not "keys: terms" → not cleanly author-keyed
            }

            var keys = SplitTrim(entry[..colon], ';');
            var terms = SplitTrim(entry[(colon + 1)..], ',');
            if (keys.Count == 0 || terms.Count == 0)
            {
                return false; // malformed author-keyed segment → fall back to prose and prompt
            }

            anyMappedTerm |= terms.Any(t => CreditTermTable.TryMap(t, out _));
            foreach (var key in keys)
            {
                foreach (var term in terms)
                {
                    builder.Add(key, term);
                }
            }
        }

        if (!anyMappedTerm || builder.IsEmpty)
        {
            return false;
        }

        entries = builder.Build();
        return true;
    }

    private static List<string> SplitTrim(string value, char separator)
        => value.Split(separator)
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
