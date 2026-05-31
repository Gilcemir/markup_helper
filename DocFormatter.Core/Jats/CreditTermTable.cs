using System.Globalization;
using System.Text;

namespace DocFormatter.Core.Jats;

/// <summary>
/// A single contributor role resolved from a written term: its display
/// <paramref name="Display"/> (the canonical SPS spelling for a CRediT match,
/// e.g. <c>Writing – original draft</c>, or the verbatim written term for a
/// free-text role) and the <paramref name="ContentTypeUrl"/> that goes in
/// <c>&lt;role content-type="…"&gt;</c> — <see langword="null"/> for an
/// operator-chosen free-text role with no <c>@content-type</c> (ADR-007).
/// </summary>
public sealed record CreditRole(string Display, string? ContentTypeUrl);

/// <summary>
/// The CRediT term→URL lookup table from
/// <c>docs/scielo_context/jats/credit_roles.md</c>. A written term is mapped by an
/// <em>exact</em> match against the 14 CRediT terms after normalization
/// (case-insensitive, dash- and <c>&amp;</c>-normalized) — there is no synonym or
/// fuzzy layer, so a term that does not normalize to a table entry is reported
/// unrecognized and gated to a prompt rather than guessed (ADR-005).
/// </summary>
/// <remarks>
/// The <c>content-type</c> value uses <c>http://credit.niso.org/contributor-roles/&lt;slug&gt;/</c>.
/// This is the SPS-canonical form carried in the XML attribute (raw
/// <c>SPS 1.10</c> examples and the <c>credit_roles.md</c> XML sample), distinct
/// from the human CRediT website (<c>https://</c>) referenced by the doc's prose.
/// </remarks>
public static class CreditTermTable
{
    private const string UrlFormat = "http://credit.niso.org/contributor-roles/{0}/";

    // The 14 CRediT terms in the credit_roles.md table order. Display carries the
    // canonical SPS spelling (en-dash + literal & for the two "Writing" terms);
    // Slug builds the content-type URL. Lookup is by the normalized display form.
    private static readonly (string Display, string Slug)[] Definitions =
    {
        ("Conceptualization", "conceptualization"),
        ("Data curation", "data-curation"),
        ("Formal analysis", "formal-analysis"),
        ("Funding acquisition", "funding-acquisition"),
        ("Investigation", "investigation"),
        ("Methodology", "methodology"),
        ("Project administration", "project-administration"),
        ("Resources", "resources"),
        ("Software", "software"),
        ("Supervision", "supervision"),
        ("Validation", "validation"),
        ("Visualization", "visualization"),
        ("Writing – original draft", "writing-original-draft"),
        ("Writing – review & editing", "writing-review-editing"),
    };

    private static readonly Dictionary<string, CreditRole> ByNormalizedTerm = BuildIndex();

    /// <summary>
    /// Maps <paramref name="term"/> to its <see cref="CreditRole"/> by exact match
    /// against the normalized CRediT table. Returns <see langword="false"/> for a
    /// blank or unrecognized term (the caller surfaces it for confirmation).
    /// </summary>
    public static bool TryMap(string? term, out CreditRole role)
    {
        if (!string.IsNullOrWhiteSpace(term))
        {
            return ByNormalizedTerm.TryGetValue(Normalize(term), out role!);
        }

        role = null!;
        return false;
    }

    /// <summary>
    /// Normalizes a written term to its comparison key: lowercased, with
    /// <c>&amp;</c> read as the word <c>and</c> and every dash variant treated as a
    /// space, then whitespace-collapsed. This folds the corpus spellings
    /// (<c>Writing - Original Draft</c>, <c>Writing – original draft</c>;
    /// <c>… review &amp; editing</c>, <c>… review and editing</c>) onto a single
    /// key per CRediT term.
    /// </summary>
    public static string Normalize(string term)
    {
        ArgumentNullException.ThrowIfNull(term);

        // Decode a literal "&amp;" first so it folds to the same "and" as a bare &.
        var lower = term.Replace("&amp;", "&", StringComparison.OrdinalIgnoreCase).ToLowerInvariant();

        var builder = new StringBuilder(lower.Length);
        var previousWasSpace = true; // leading-space suppressor
        foreach (var c in lower)
        {
            if (c == '&')
            {
                AppendToken(builder, " and ", ref previousWasSpace);
                continue;
            }

            if (IsDash(c) || char.IsWhiteSpace(c))
            {
                if (!previousWasSpace)
                {
                    builder.Append(' ');
                    previousWasSpace = true;
                }

                continue;
            }

            builder.Append(c);
            previousWasSpace = false;
        }

        return builder.ToString().Trim();
    }

    private static void AppendToken(StringBuilder builder, string token, ref bool previousWasSpace)
    {
        // token is " and " — only the surrounding single spaces are subject to
        // the collapse rule, so rebuild it through the space-aware path.
        if (!previousWasSpace)
        {
            builder.Append(' ');
        }

        builder.Append("and ");
        previousWasSpace = true;
    }

    // Folds hyphen/dash variants plus look-alikes that survive copy-paste/OCR:
    // the math minus (U+2212), fullwidth (U+FF0D) and small (U+FE63) hyphen-minus.
    private static bool IsDash(char c) => c is '-' or '‐' or '‑' or '‒' or '–' or '—' or '―' or '−' or '－' or '﹣';

    private static Dictionary<string, CreditRole> BuildIndex()
    {
        var index = new Dictionary<string, CreditRole>(StringComparer.Ordinal);
        foreach (var (display, slug) in Definitions)
        {
            var url = string.Format(CultureInfo.InvariantCulture, UrlFormat, slug);
            index[Normalize(display)] = new CreditRole(display, url);
        }

        return index;
    }
}
