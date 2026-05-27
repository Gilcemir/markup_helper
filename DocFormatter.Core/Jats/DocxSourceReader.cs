using System.Text;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace DocFormatter.Core.Jats;

/// <summary>
/// Reads a SciELO Markup <c>.docx</c> into a <see cref="DocxSource"/>. The docx
/// is opened read-only and never modified (ADR-002).
/// </summary>
/// <remarks>
/// The header keys are extracted with a focused parse of the single <c>[doc]</c>
/// bracket tag rather than a full bracket-tag interpreter (ADR-004). The three
/// trailing sections are untagged prose, so they are located by anchoring on the
/// headers observed across the corpus (<c>Scientific Editor:</c>,
/// <c>DATA AVAILABILITY</c>, <c>CREDIT STATEMENT</c>) and read order-independently
/// — the corpus mixes both DA-before-CS and CS-before-DA orderings, and at least
/// one document glues the next header onto the end of the preceding body.
/// </remarks>
public sealed partial class DocxSourceReader
{
    private static readonly string[] SectionHeaders = { "DATA AVAILABILITY", "CREDIT STATEMENT" };

    /// <summary>
    /// Opens <paramref name="docxPath"/> read-only and parses it into a
    /// <see cref="DocxSource"/>.
    /// </summary>
    /// <exception cref="InvalidDataException">
    /// The document is missing its body, the <c>[doc]</c> header, or the
    /// mandatory <c>elocatid</c>/<c>[doi]</c> keys.
    /// </exception>
    public DocxSource Read(string docxPath)
    {
        ArgumentException.ThrowIfNullOrEmpty(docxPath);

        // isEditable: false — the docx is a read-only source and must never be
        // written back (ADR-002).
        using var doc = WordprocessingDocument.Open(docxPath, false);
        var body = doc.MainDocumentPart?.Document?.Body
            ?? throw new InvalidDataException($"docx '{docxPath}' is missing its document body");

        var paragraphs = ReadParagraphTexts(body);
        return Parse(paragraphs, docxPath);
    }

    /// <summary>
    /// Parses the document's paragraph texts (in document order) into a
    /// <see cref="DocxSource"/>. Exposed for unit testing without a real docx.
    /// <paramref name="origin"/> is used only for error messages.
    /// </summary>
    public static DocxSource Parse(IReadOnlyList<string> paragraphs, string origin = "<text>")
    {
        ArgumentNullException.ThrowIfNull(paragraphs);

        var (elocationId, doi) = ParseHeaderKeys(paragraphs, origin);
        var segments = BuildSegments(paragraphs);

        return new DocxSource
        {
            ElocationId = elocationId,
            Doi = doi,
            ScientificEditor = FindEditor(paragraphs, ScientificEditorRegex()),
            AssociateEditor = FindEditor(paragraphs, AssociateEditorRegex()),
            DataAvailabilityText = ExtractSection(segments, "DATA AVAILABILITY"),
            CreditStatementRaw = ExtractSection(segments, "CREDIT STATEMENT"),
        };
    }

    private static List<string> ReadParagraphTexts(Body body)
    {
        var paragraphs = new List<string>();
        foreach (var paragraph in body.Descendants<Paragraph>())
        {
            paragraphs.Add(string.Concat(paragraph.Descendants<Text>().Select(t => t.Text)));
        }

        return paragraphs;
    }

    private static (string ElocationId, string Doi) ParseHeaderKeys(
        IReadOnlyList<string> paragraphs,
        string origin)
    {
        // The [doc] header and its [doi] tag share the first paragraph in the
        // corpus, but scan defensively for the first paragraph carrying each key.
        string? elocationId = null;
        string? doi = null;

        foreach (var text in paragraphs)
        {
            if (elocationId is null)
            {
                var match = ElocationRegex().Match(text);
                if (match.Success)
                {
                    elocationId = match.Groups[1].Value.Trim();
                }
            }

            if (doi is null)
            {
                var match = DoiRegex().Match(text);
                if (match.Success)
                {
                    doi = match.Groups[1].Value.Trim();
                }
            }

            if (elocationId is not null && doi is not null)
            {
                break;
            }
        }

        if (string.IsNullOrEmpty(elocationId))
        {
            throw new InvalidDataException(
                $"docx '{origin}' is missing the [doc] header elocatid key");
        }

        if (string.IsNullOrEmpty(doi))
        {
            throw new InvalidDataException(
                $"docx '{origin}' is missing the [doi]…[/doi] header key");
        }

        return (elocationId, doi);
    }

    private static string? FindEditor(IReadOnlyList<string> paragraphs, Regex editorRegex)
    {
        foreach (var text in paragraphs)
        {
            var match = editorRegex.Match(NormalizeWhitespace(text));
            if (!match.Success)
            {
                continue;
            }

            var name = match.Groups[1].Value.Trim();
            return name.Length == 0 ? null : name;
        }

        return null;
    }

    /// <summary>
    /// Flattens the paragraph list into segments where each section header is
    /// isolated as its own segment. This lets a header glued onto the end of a
    /// preceding body paragraph (observed in the corpus) be treated the same as
    /// a standalone header paragraph.
    /// </summary>
    private static List<string> BuildSegments(IReadOnlyList<string> paragraphs)
    {
        var segments = new List<string>();
        foreach (var paragraph in paragraphs)
        {
            SplitOnHeaders(paragraph, segments);
        }

        return segments;
    }

    private static void SplitOnHeaders(string text, List<string> segments)
    {
        var rest = text;
        while (true)
        {
            var index = -1;
            var matchedHeader = string.Empty;
            foreach (var header in SectionHeaders)
            {
                var at = rest.IndexOf(header, StringComparison.OrdinalIgnoreCase);
                if (at >= 0 && (index < 0 || at < index))
                {
                    index = at;
                    matchedHeader = header;
                }
            }

            if (index < 0)
            {
                segments.Add(rest);
                return;
            }

            segments.Add(rest[..index]);
            segments.Add(matchedHeader);
            rest = rest[(index + matchedHeader.Length)..];
        }
    }

    private static string? ExtractSection(List<string> segments, string canonicalHeader)
    {
        var start = segments.FindIndex(s => string.Equals(s, canonicalHeader, StringComparison.Ordinal));
        if (start < 0)
        {
            return null;
        }

        var body = new List<string>();
        for (var i = start + 1; i < segments.Count; i++)
        {
            var segment = segments[i];

            // A canonical header marker (added by SplitOnHeaders) ends this
            // section regardless of which section it introduces.
            if (IsSectionHeader(segment))
            {
                break;
            }

            var trimmed = segment.Trim();
            if (trimmed.Length == 0)
            {
                // Empty segment: the split artifact before a glued header or a
                // blank paragraph between the header and its body.
                continue;
            }

            // The trailing prose ends at the next bracket-tagged paragraph
            // ([refs], [corresp], …) or a bare REFERENCES heading.
            if (trimmed[0] == '[' || trimmed.StartsWith("REFERENCES", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            body.Add(trimmed);
        }

        return body.Count == 0 ? null : string.Join(" ", body);
    }

    private static bool IsSectionHeader(string segment)
    {
        foreach (var header in SectionHeaders)
        {
            if (string.Equals(segment, header, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Replaces non-breaking (U+00A0) and narrow no-break (U+202F) spaces with a
    /// regular space, collapses whitespace runs, and trims. The corpus uses both
    /// kinds of non-breaking space inside and around the editor line.
    /// </summary>
    private static string NormalizeWhitespace(string text)
    {
        var builder = new StringBuilder(text.Length);
        var previousWasSpace = false;
        foreach (var c in text)
        {
            // char.IsWhiteSpace covers the regular space plus the non-breaking
            // (U+00A0) and narrow no-break (U+202F) spaces the corpus uses.
            if (char.IsWhiteSpace(c))
            {
                if (!previousWasSpace)
                {
                    builder.Append(' ');
                }

                previousWasSpace = true;
            }
            else
            {
                builder.Append(c);
                previousWasSpace = false;
            }
        }

        return builder.ToString().Trim();
    }

    [GeneratedRegex(@"elocatid\s*=\s*""([^""]*)""", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex ElocationRegex();

    [GeneratedRegex(@"\[doi\](.*?)\[/doi\]", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Singleline)]
    private static partial Regex DoiRegex();

    [GeneratedRegex(@"scientific\s+editor\s*:\s*(.+)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex ScientificEditorRegex();

    [GeneratedRegex(@"associate\s+editor\s*:\s*(.+)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex AssociateEditorRegex();
}
