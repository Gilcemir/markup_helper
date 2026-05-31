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
    // Lowercase name particles dropped when building candidate initials (e.g.
    // "Antônio Teixeira do" → AT, "Danilo … da Silva" → D…S).
    private static readonly HashSet<string> Particles = new(StringComparer.OrdinalIgnoreCase)
    {
        "da", "das", "de", "del", "della", "di", "do", "dos", "du", "e",
        "la", "le", "van", "von", "y",
    };

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

        // Bare initials: match against each contributor's candidate initials.
        var matches = contribs.Where(c => CandidateInitials(c).Contains(NormalizeInitials(key))).ToList();
        return Classify(matches);
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
        // disambiguates; otherwise the key stays ambiguous and is prompted.
        if (initials != null)
        {
            var narrowed = bySurname.Where(c => CandidateInitials(c).Contains(NormalizeInitials(initials))).ToList();
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

    // A trailing initials token is two or more letters, all uppercase (e.g.
    // "DAPS", "OJ"); a single capital like a one-letter name is not treated as
    // initials to avoid swallowing a short surname.
    private static bool IsInitialsToken(string token)
        => token.Length >= 2 && token.All(char.IsLetter) && token.All(char.IsUpper);

    private static bool SurnameMatches(XElement contrib, string surname)
    {
        var target = Fold(surname);
        return SurnameTokens(contrib).Any(t => Fold(t) == target);
    }

    private static IEnumerable<string> SurnameTokens(XElement contrib)
        => ChildText(contrib, "surname").Split(' ', StringSplitOptions.RemoveEmptyEntries);

    /// <summary>
    /// The candidate initials strings for a contributor: combinations of the
    /// given-name initials, the surname initial(s), and a suffix initial, with
    /// lowercase particles dropped. Covers the corpus orderings (e.g.
    /// <c>Antônio Teixeira do</c> + <c>Amaral</c> + <c>Júnior</c> → <c>ATAJ</c>).
    /// </summary>
    private static HashSet<string> CandidateInitials(XElement contrib)
    {
        var given = Initials(ChildText(contrib, "given-names"));
        var surname = Initials(ChildText(contrib, "surname"));
        var suffix = Initials(ChildText(contrib, "suffix"));

        return new HashSet<string>(StringComparer.Ordinal)
        {
            given + surname + suffix,
            given + surname,
            surname + given,
            given,
        };
    }

    private static string Initials(string text)
    {
        var builder = new StringBuilder();
        foreach (var token in text.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            if (Particles.Contains(token))
            {
                continue;
            }

            var folded = Fold(token);
            if (folded.Length > 0)
            {
                builder.Append(char.ToUpperInvariant(folded[0]));
            }
        }

        return builder.ToString();
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
