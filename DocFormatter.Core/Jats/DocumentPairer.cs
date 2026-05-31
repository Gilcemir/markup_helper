using System.Xml.Linq;

namespace DocFormatter.Core.Jats;

/// <summary>
/// A docx considered during pairing: its file path plus the parsed read-only
/// <see cref="DocxSource"/> whose <c>[doc]</c> header keys (elocatid, doi) are
/// matched against the XML.
/// </summary>
public sealed record DocxCandidate(string Path, DocxSource Source);

/// <summary>
/// A verified docx ↔ XML ↔ <c>other.txt</c> triple: the two paths, the parsed
/// docx source, and the three keys that survived cross-checking (ADR-004). The
/// parsed <see cref="Source"/> is carried so the caller need not re-read the docx
/// it was matched on.
/// </summary>
public sealed class PairedDocument
{
    /// <summary>The XML package path (the injection target).</summary>
    public required string XmlPath { get; init; }

    /// <summary>The paired read-only docx source path.</summary>
    public required string DocxPath { get; init; }

    /// <summary>The parsed docx, already read while matching on elocatid.</summary>
    public required DocxSource Source { get; init; }

    /// <summary>The elocation-id the docx and XML agreed on, e.g. <c>e54492621</c>.</summary>
    public required string ElocationId { get; init; }

    /// <summary>The DOI both sides carried (verified equal).</summary>
    public required string Doi { get; init; }

    /// <summary>The verbatim <c>other</c> number from <c>other.txt</c> (leading zeros kept).</summary>
    public required string OtherNumber { get; init; }
}

/// <summary>
/// Outcome of a pairing attempt: either a verified <see cref="PairedDocument"/>
/// or a descriptive fail-loud <see cref="Error"/>. A failure is a reported,
/// non-silent reason the caller can use to skip the document (non-zero outcome)
/// without aborting the rest of the batch — it is never an empty/half-matched
/// pair (ADR-004).
/// </summary>
public sealed class PairingResult
{
    private PairingResult(PairedDocument? pair, string? error)
    {
        Pair = pair;
        Error = error;
    }

    /// <summary>True when a verified pair was produced.</summary>
    public bool IsPaired => Pair is not null;

    /// <summary>The verified pair, or <see langword="null"/> on failure.</summary>
    public PairedDocument? Pair { get; }

    /// <summary>The fail-loud reason, or <see langword="null"/> on success.</summary>
    public string? Error { get; }

    /// <summary>Builds a successful result around <paramref name="pair"/>.</summary>
    public static PairingResult Success(PairedDocument pair)
        => new(pair ?? throw new ArgumentNullException(nameof(pair)), null);

    /// <summary>Builds a fail-loud result carrying <paramref name="reason"/>.</summary>
    public static PairingResult Failure(string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        return new PairingResult(null, reason);
    }
}

/// <summary>
/// Pairs a JATS XML package with its source docx and <c>other.txt</c> entry on
/// the elocation-id, cross-checking the DOI on both sides and failing loudly on
/// any missing or mismatched key (ADR-004). This guards against the top PRD risk:
/// applying the wrong source document to an XML.
/// </summary>
/// <remarks>
/// The three inputs share no common filename — the docx is named by a submission
/// id, the XML/PDF by the SciELO basename — so the docx is located by scanning
/// the markup-source directory for the one whose <c>[doc]@elocatid</c> header
/// matches the XML's <c>&lt;elocation-id&gt;</c>. Because that scan reads each
/// candidate into a <see cref="DocxSource"/>, the matched source is returned in
/// the <see cref="PairedDocument"/> so it is not read twice.
/// </remarks>
public static class DocumentPairer
{
    /// <summary>
    /// Loads <paramref name="xmlPath"/>, scans <paramref name="markupSourceDir"/>
    /// for the docx whose header elocatid matches, and pairs them against
    /// <paramref name="otherTable"/>.
    /// </summary>
    /// <exception cref="DirectoryNotFoundException">
    /// <paramref name="markupSourceDir"/> does not exist — a configuration error,
    /// distinct from a per-document pairing failure (which returns a result).
    /// </exception>
    public static PairingResult Pair(string xmlPath, string markupSourceDir, OtherTable otherTable)
    {
        ArgumentException.ThrowIfNullOrEmpty(xmlPath);
        ArgumentException.ThrowIfNullOrEmpty(markupSourceDir);
        ArgumentNullException.ThrowIfNull(otherTable);

        var xml = XDocument.Load(xmlPath, LoadOptions.PreserveWhitespace);
        var candidates = LoadCandidates(markupSourceDir);
        return Pair(xml, xmlPath, candidates, otherTable);
    }

    /// <summary>
    /// Pairs a pre-loaded <paramref name="xml"/> (identified by
    /// <paramref name="xmlPath"/> for reporting and the <c>other.txt</c> lookup)
    /// against already-parsed <paramref name="candidates"/> and
    /// <paramref name="otherTable"/>. The core matcher, exposed for testing
    /// without real docx or XML files.
    /// </summary>
    public static PairingResult Pair(
        XDocument xml,
        string xmlPath,
        IReadOnlyList<DocxCandidate> candidates,
        OtherTable otherTable)
    {
        ArgumentNullException.ThrowIfNull(xml);
        ArgumentException.ThrowIfNullOrEmpty(xmlPath);
        ArgumentNullException.ThrowIfNull(candidates);
        ArgumentNullException.ThrowIfNull(otherTable);

        var basename = Path.GetFileName(xmlPath);

        // The article's own elocation-id and DOI live in front/article-meta. The
        // XML also carries many <elocation-id> inside <back> reference citations,
        // so only the direct children of <article-meta> are the article's keys.
        var articleMeta = xml.Root
            ?.Elements().FirstOrDefault(e => e.Name.LocalName == "front")
            ?.Elements().FirstOrDefault(e => e.Name.LocalName == "article-meta");
        if (articleMeta is null)
        {
            return PairingResult.Failure(
                $"XML '{basename}' has no <front>/<article-meta> to read the elocation-id and DOI from.");
        }

        var xmlElocationId = articleMeta
            .Elements().FirstOrDefault(e => e.Name.LocalName == "elocation-id")
            ?.Value.Trim();
        if (string.IsNullOrEmpty(xmlElocationId))
        {
            return PairingResult.Failure(
                $"XML '{basename}' is missing <elocation-id> in <article-meta>; cannot pair.");
        }

        var xmlDoi = articleMeta
            .Elements().FirstOrDefault(e =>
                e.Name.LocalName == "article-id"
                && string.Equals((string?)e.Attribute("pub-id-type"), "doi", StringComparison.Ordinal))
            ?.Value.Trim();
        if (string.IsNullOrEmpty(xmlDoi))
        {
            return PairingResult.Failure(
                $"XML '{basename}' is missing <article-id pub-id-type=\"doi\">; cannot verify the pair.");
        }

        // Primary key: match docx header elocatid to the XML elocation-id.
        var matches = candidates
            .Where(c => string.Equals(c.Source.ElocationId, xmlElocationId, StringComparison.Ordinal))
            .ToList();
        if (matches.Count == 0)
        {
            return PairingResult.Failure(
                $"No docx in the markup source matches elocation-id '{xmlElocationId}' for XML '{basename}'.");
        }

        if (matches.Count > 1)
        {
            var names = string.Join(", ", matches.Select(m => Path.GetFileName(m.Path)));
            return PairingResult.Failure(
                $"Ambiguous pairing for elocation-id '{xmlElocationId}': {matches.Count} docx match ({names}); refusing to guess.");
        }

        var match = matches[0];
        var docxName = Path.GetFileName(match.Path);

        // Verification key: the two independently-read DOIs must agree. DOIs are
        // case-insensitive (DOI Handbook), so compare trimmed and case-folded.
        var docxDoi = match.Source.Doi.Trim();
        if (!string.Equals(docxDoi, xmlDoi, StringComparison.OrdinalIgnoreCase))
        {
            return PairingResult.Failure(
                $"DOI mismatch for elocation-id '{xmlElocationId}': docx '{docxName}' has '{docxDoi}' but XML '{basename}' has '{xmlDoi}'.");
        }

        // The other number is keyed by the XML/PDF basename in other.txt.
        if (!otherTable.TryGetOther(basename, out var otherNumber) || otherNumber is null)
        {
            return PairingResult.Failure(
                $"No other.txt entry for basename '{Path.GetFileNameWithoutExtension(basename)}' (XML '{basename}').");
        }

        return PairingResult.Success(new PairedDocument
        {
            XmlPath = xmlPath,
            DocxPath = match.Path,
            Source = match.Source,
            ElocationId = xmlElocationId,
            Doi = xmlDoi,
            OtherNumber = otherNumber,
        });
    }

    private static IReadOnlyList<DocxCandidate> LoadCandidates(string markupSourceDir)
    {
        var reader = new DocxSourceReader();
        var candidates = new List<DocxCandidate>();
        foreach (var path in Directory.EnumerateFiles(markupSourceDir, "*.docx"))
        {
            // Word lock files (~$...) are not real documents.
            if (Path.GetFileName(path).StartsWith("~$", StringComparison.Ordinal))
            {
                continue;
            }

            DocxSource source;
            try
            {
                source = reader.Read(path);
            }
            catch (InvalidDataException)
            {
                // A docx lacking usable [doc] header keys cannot be the pair;
                // skip it as a non-candidate. A genuinely missing pair still
                // surfaces as the "no docx matches elocation-id" failure.
                continue;
            }

            candidates.Add(new DocxCandidate(path, source));
        }

        return candidates;
    }
}
