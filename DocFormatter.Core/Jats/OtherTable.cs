namespace DocFormatter.Core.Jats;

/// <summary>
/// The <c>other.txt</c> lookup: a TSV of <c>&lt;pdf-basename&gt;\t&lt;5-digit-other&gt;</c>
/// loaded into a map keyed by basename with its extension stripped, so an XML or
/// PDF basename resolves directly (ADR-004,
/// <c>docs/scielo_context/jats/article_id_other.md</c>). The <c>other</c> number
/// is the only Phase 3 value that is <em>not</em> derivable from any document — it
/// is assigned externally by SciELO — so this table is the sole source for it.
/// </summary>
/// <remarks>
/// The table is read once per run by the CLI and queried by the document pairer
/// and the <c>other</c>-id injector. Lookups return a found/not-found result (the
/// <see cref="TryGetOther"/> pattern) rather than a silent empty string, so a
/// missing key can be reported as a fail-loud precondition (ADR-001) instead of
/// emitting an orphan tag. The <c>other</c> value is preserved verbatim
/// (zero-padded, never numerically parsed) — leading zeros are significant.
/// </remarks>
public sealed class OtherTable
{
    private readonly IReadOnlyDictionary<string, string> _byBasename;

    private OtherTable(IReadOnlyDictionary<string, string> byBasename) => _byBasename = byBasename;

    /// <summary>Number of distinct basenames in the table.</summary>
    public int Count => _byBasename.Count;

    /// <summary>
    /// Reads and parses the <c>other.txt</c> file at <paramref name="path"/>.
    /// </summary>
    /// <exception cref="InvalidDataException">A non-blank line is not valid
    /// <c>&lt;basename&gt;\t&lt;other&gt;</c>, or two lines map the same basename to
    /// different values.</exception>
    public static OtherTable Load(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        return Parse(File.ReadAllLines(path), path);
    }

    /// <summary>
    /// Parses TSV <paramref name="lines"/> into an <see cref="OtherTable"/>. Exposed
    /// for unit testing without a file. <paramref name="origin"/> is used only for
    /// error messages. Blank (and whitespace-only) lines are ignored and trailing
    /// whitespace is tolerated.
    /// </summary>
    /// <exception cref="InvalidDataException">A non-blank line is not valid
    /// <c>&lt;basename&gt;\t&lt;other&gt;</c>, or two lines map the same basename to
    /// different values.</exception>
    public static OtherTable Parse(IEnumerable<string> lines, string origin = "<text>")
    {
        ArgumentNullException.ThrowIfNull(lines);

        var map = new Dictionary<string, string>(StringComparer.Ordinal);
        var lineNumber = 0;
        foreach (var rawLine in lines)
        {
            lineNumber++;
            var line = (rawLine ?? string.Empty).Trim();
            if (line.Length == 0)
            {
                continue;
            }

            // TSV: split on the first tab only; the value keeps its exact characters
            // (leading zeros) — only surrounding whitespace is trimmed.
            var tab = line.IndexOf('\t');
            if (tab < 0)
            {
                throw new InvalidDataException(
                    $"other.txt '{origin}' line {lineNumber} is not tab-separated '<basename>\\t<other>': \"{line}\"");
            }

            var basename = line[..tab].Trim();
            var other = line[(tab + 1)..].Trim();
            if (basename.Length == 0 || other.Length == 0)
            {
                throw new InvalidDataException(
                    $"other.txt '{origin}' line {lineNumber} has an empty basename or other value: \"{line}\"");
            }

            var key = StripExtension(basename);
            if (map.TryGetValue(key, out var existing))
            {
                if (!string.Equals(existing, other, StringComparison.Ordinal))
                {
                    throw new InvalidDataException(
                        $"other.txt '{origin}' line {lineNumber}: basename '{key}' maps to both '{existing}' and '{other}'");
                }

                continue;
            }

            map[key] = other;
        }

        return new OtherTable(map);
    }

    /// <summary>
    /// Looks up the <c>other</c> number for <paramref name="basename"/> (an XML or
    /// PDF basename, with or without extension). Returns <see langword="true"/> and
    /// the exact stored value when found; <see langword="false"/> with
    /// <paramref name="other"/> set to <see langword="null"/> when absent, so the
    /// caller can fail loudly rather than treat a miss as an empty value.
    /// </summary>
    public bool TryGetOther(string basename, out string? other)
    {
        ArgumentException.ThrowIfNullOrEmpty(basename);
        return _byBasename.TryGetValue(StripExtension(basename), out other);
    }

    // Strip a single trailing extension (".pdf"/".xml"/none) without touching the
    // dots inside a SciELO basename — GetFileNameWithoutExtension removes only the
    // final extension and any directory part.
    private static string StripExtension(string basename)
        => Path.GetFileNameWithoutExtension(basename);
}
