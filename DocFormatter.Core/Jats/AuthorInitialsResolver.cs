using System.Globalization;
using System.Text;
using System.Xml.Linq;

namespace DocFormatter.Core.Jats;

/// <summary>Whether an author key resolved to exactly one contributor.</summary>
public enum ResolveStatus
{
    /// <summary>Exactly one <c>&lt;contrib&gt;</c> matched.</summary>
    Resolved,

    /// <summary>No <c>&lt;contrib&gt;</c> matched.</summary>
    NotFound,

    /// <summary>More than one <c>&lt;contrib&gt;</c> matched.</summary>
    Ambiguous,
}

/// <summary>
/// The outcome of resolving an author key: the matched <paramref name="Contrib"/>
/// (only when <see cref="ResolveStatus.Resolved"/>) and the
/// <paramref name="Status"/>.
/// </summary>
public sealed record AuthorResolution(XElement? Contrib, ResolveStatus Status);

/// <summary>
/// Resolves a written CREDIT author key to a single <c>&lt;contrib&gt;</c> by its
/// <c>&lt;surname&gt;</c>/<c>&lt;given-names&gt;</c>. ADR-005 requires a
/// <em>unique</em> match: a key matching zero or several contributors is reported
/// unresolved so the injector prompts rather than attaching roles to a guessed
/// author. Two key forms are handled — <c>Surname Initials</c> /
/// <c>Surname, I. N.</c> (matched by surname, narrowed by initials on a tie) and a
/// bare all-uppercase initials block such as <c>ATAJ</c> (matched against
/// candidate initials built from the contributor's names).
/// </summary>
public static class AuthorInitialsResolver
{
    // Name particles. Lookup is case-insensitive; the token's own casing decides
    // the variants: a lowercase particle ("Antônio Teixeira do" → AT) is always
    // dropped, a capitalized one ("Truong Van" → T and TV) yields both variants.
    private static readonly HashSet<string> Particles = new(StringComparer.OrdinalIgnoreCase)
    {
        "da", "das", "de", "del", "della", "di", "do", "dos", "du", "e",
        "la", "le", "van", "von", "y",
    };

    // Hyphens that split a compound token into sub-tokens: ASCII, U+2010 HYPHEN,
    // U+2013 EN DASH (Word substitutes the latter two).
    private static readonly char[] Hyphens = { '-', '\u2010', '\u2013' };

    /// <summary>
    /// Resolves <paramref name="authorKey"/> against <paramref name="contribs"/>.
    /// </summary>
    public static AuthorResolution Resolve(string authorKey, IReadOnlyList<XElement> contribs)
    {
        ArgumentNullException.ThrowIfNull(authorKey);
        ArgumentNullException.ThrowIfNull(contribs);

        var key = authorKey.Trim().Trim('.', ',', ';').Trim();
        if (key.Length == 0 || contribs.Count == 0)
        {
            return new AuthorResolution(null, ResolveStatus.NotFound);
        }

        var (surname, initials) = SplitKey(key);
        if (surname != null)
        {
            return ResolveBySurname(surname, initials, contribs);
        }

        // Bare initials (ADR-004): full-name candidates are matched first across
        // every contributor; the given-names-only tier is a fallback used only
        // when no contributor matched a full candidate. Uniqueness is required
        // within the tier that matched.
        var normalized = NormalizeInitials(key);
        var candidates = contribs.Select(c => (Contrib: c, Candidates: CandidateInitials(c))).ToList();

        var full = candidates.Where(x => x.Candidates.Full.Contains(normalized)).Select(x => x.Contrib).ToList();
        if (full.Count > 0)
        {
            return Classify(full);
        }

        var givenOnly = candidates.Where(x => x.Candidates.GivenOnly.Contains(normalized)).Select(x => x.Contrib).ToList();
        return Classify(givenOnly);
    }

    private static AuthorResolution ResolveBySurname(
        string surname,
        string? initials,
        IReadOnlyList<XElement> contribs)
    {
        var bySurname = contribs.Where(c => SurnameMatches(c, surname)).ToList();
        if (bySurname.Count == 1)
        {
            return new AuthorResolution(bySurname[0], ResolveStatus.Resolved);
        }

        if (bySurname.Count == 0)
        {
            return new AuthorResolution(null, ResolveStatus.NotFound);
        }

        // Several contributors share the surname: only a unique initials match
        // (in either candidate tier) disambiguates; otherwise the key stays
        // ambiguous and is prompted.
        if (initials != null)
        {
            var normalized = NormalizeInitials(initials);
            var narrowed = bySurname.Where(c => CandidateInitials(c).ContainsInAnyTier(normalized)).ToList();
            if (narrowed.Count == 1)
            {
                return new AuthorResolution(narrowed[0], ResolveStatus.Resolved);
            }
        }

        return new AuthorResolution(null, ResolveStatus.Ambiguous);
    }

    private static AuthorResolution Classify(IReadOnlyList<XElement> matches) => matches.Count switch
    {
        1 => new AuthorResolution(matches[0], ResolveStatus.Resolved),
        0 => new AuthorResolution(null, ResolveStatus.NotFound),
        _ => new AuthorResolution(null, ResolveStatus.Ambiguous),
    };

    /// <summary>
    /// Splits an author key into a surname (or <see langword="null"/> for a bare
    /// initials block) and its trailing initials (or <see langword="null"/>).
    /// <c>Surname, I. N.</c> splits on the comma; <c>Surname Initials</c> takes a
    /// trailing all-uppercase token as the initials and the first token as the
    /// surname; a single all-uppercase token is treated as a bare initials block.
    /// </summary>
    private static (string? Surname, string? Initials) SplitKey(string key)
    {
        var comma = key.IndexOf(',', StringComparison.Ordinal);
        if (comma >= 0)
        {
            var surname = key[..comma].Trim();
            var initials = key[(comma + 1)..].Trim();
            return (FirstToken(surname), initials.Length == 0 ? null : initials);
        }

        var tokens = key.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (tokens.Length == 1)
        {
            return IsInitialsToken(tokens[0]) ? (null, tokens[0]) : (tokens[0], null);
        }

        var last = tokens[^1];
        return IsInitialsToken(last)
            ? (tokens[0], last)
            : (tokens[0], null);
    }

    private static string FirstToken(string value)
    {
        var tokens = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return tokens.Length == 0 ? value : tokens[0];
    }

    /// <summary>
    /// Whether <paramref name="token"/> is an initials block: two or more letters,
    /// all uppercase (e.g. <c>DAPS</c>, <c>OJ</c>). A single capital like a
    /// one-letter name is not treated as initials to avoid swallowing a short
    /// surname. Shared with <see cref="CreditStatementParser"/>.
    /// </summary>
    internal static bool IsInitialsToken(string token)
        => token.Length >= 2 && token.All(char.IsLetter) && token.All(char.IsUpper);

    private static bool SurnameMatches(XElement contrib, string surname)
    {
        var target = Fold(surname);
        return SurnameTokens(contrib).Any(t => Fold(t) == target);
    }

    private static IEnumerable<string> SurnameTokens(XElement contrib)
        => ChildText(contrib, "surname").Split(' ', StringSplitOptions.RemoveEmptyEntries);

    /// <summary>
    /// The two candidate tiers for a contributor (ADR-004). <see cref="Full"/>
    /// holds every full combination — given+surname+suffix, given+surname and
    /// surname+given — over the initials variants of each name part (e.g.
    /// <c>Antônio Teixeira do</c> + <c>Amaral</c> + <c>Júnior</c> → <c>ATAJ</c>);
    /// <see cref="GivenOnly"/> holds the given-names initials alone, used only as
    /// a fallback when no contributor matches a full candidate.
    /// </summary>
    private sealed record CandidateTiers(HashSet<string> Full, HashSet<string> GivenOnly)
    {
        public bool ContainsInAnyTier(string initials) => Full.Contains(initials) || GivenOnly.Contains(initials);
    }

    private static CandidateTiers CandidateInitials(XElement contrib)
    {
        var given = InitialsVariants(ChildText(contrib, "given-names"));
        var surname = InitialsVariants(ChildText(contrib, "surname"));
        var suffix = InitialsVariants(ChildText(contrib, "suffix"));

        var full = new HashSet<string>(StringComparer.Ordinal);
        foreach (var g in given)
        {
            foreach (var s in surname)
            {
                full.Add(g + s);
                full.Add(s + g);
                foreach (var x in suffix)
                {
                    full.Add(g + s + x);
                }
            }
        }

        return new CandidateTiers(full, new HashSet<string>(given, StringComparer.Ordinal));
    }

    /// <summary>
    /// Every initials string a name part can be written as. Each space-separated
    /// token contributes a list of variants — a hyphenated token its first initial
    /// and one initial per sub-token (<c>Barboza-Barquero</c> → B, BB); a
    /// capitalized particle dropped and kept (<c>Van</c> → "", V); a lowercase
    /// particle dropped; any other token its initial — combined by cartesian
    /// product. A name part with no letters yields the single empty string.
    /// </summary>
    private static IReadOnlyList<string> InitialsVariants(string text)
    {
        IReadOnlyList<string> variants = new[] { string.Empty };
        foreach (var token in text.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            variants = Combine(variants, TokenVariants(token));
        }

        return variants;
    }

    private static IReadOnlyList<string> TokenVariants(string token)
    {
        if (Particles.Contains(token))
        {
            var initial = Initial(token);
            return initial.Length > 0 && char.IsUpper(token[0])
                ? new[] { string.Empty, initial }
                : new[] { string.Empty };
        }

        var subInitials = token
            .Split(Hyphens, StringSplitOptions.RemoveEmptyEntries)
            .Select(Initial)
            .Where(i => i.Length > 0)
            .ToList();

        return subInitials.Count switch
        {
            0 => new[] { string.Empty },
            1 => new[] { subInitials[0] },
            _ => new[] { subInitials[0], string.Concat(subInitials) },
        };
    }

    private static string Initial(string token)
    {
        var folded = Fold(token);
        return folded.Length == 0 ? string.Empty : char.ToUpperInvariant(folded[0]).ToString();
    }

    private static IReadOnlyList<string> Combine(IReadOnlyList<string> prefixes, IReadOnlyList<string> suffixes)
    {
        var combined = new List<string>(prefixes.Count * suffixes.Count);
        foreach (var prefix in prefixes)
        {
            foreach (var suffix in suffixes)
            {
                combined.Add(prefix + suffix);
            }
        }

        return combined;
    }

    private static string NormalizeInitials(string value)
        => Fold(new string(value.Where(char.IsLetter).ToArray())).ToUpperInvariant();

    private static string ChildText(XElement contrib, string localName)
    {
        var child = contrib.Descendants().FirstOrDefault(e => e.Name.LocalName == localName);
        return child?.Value.Trim() ?? string.Empty;
    }

    /// <summary>Lowercases and strips diacritics so accented names compare equal.</summary>
    private static string Fold(string value)
    {
        var decomposed = value.ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);
        foreach (var c in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(c);
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }
}
