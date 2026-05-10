using System.Text;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace DocFormatter.Core.Reporting;

/// <summary>
/// Result of a <see cref="Phase2DiffUtility.Compare(string,string,IReadOnlyCollection{string})"/>
/// invocation. When the two body-text projections match within scope,
/// <see cref="IsMatch"/> is <c>true</c> and every other field is <c>null</c>.
/// On mismatch, <see cref="FirstDivergenceOffset"/> is the character index in
/// the (post-strip) expected text and the context windows hold up to 80
/// characters on each side of the divergence, clamped at string boundaries.
/// </summary>
public sealed record DiffResult(
    bool IsMatch,
    int? FirstDivergenceOffset,
    string? ProducedContext,
    string? ExpectedContext);

/// <summary>
/// Body-text comparator for the Phase 2 release gate (ADR-003 / ADR-006).
/// Extracts a flat string from each <c>.docx</c> (preserving SciELO bracket-syntax
/// tag literals verbatim), strips out-of-scope tag pairs from the expected side,
/// and reports the first character-level divergence with surrounding context.
/// </summary>
public static class Phase2DiffUtility
{
    private const int ContextRadiusChars = 80;

    private static readonly Regex TagPairPattern = new(
        @"\[(\w+)([^\]]*)\](.*?)\[/\1\]",
        RegexOptions.Singleline | RegexOptions.Compiled);

    private static readonly Regex WhitespaceRunPattern = new(
        @"\s+",
        RegexOptions.Compiled);

    public static DiffResult Compare(
        string producedDocxPath,
        string expectedDocxPath,
        IReadOnlyCollection<string> inScopeTags)
    {
        ArgumentException.ThrowIfNullOrEmpty(producedDocxPath);
        ArgumentException.ThrowIfNullOrEmpty(expectedDocxPath);
        ArgumentNullException.ThrowIfNull(inScopeTags);

        var produced = ExtractBodyText(producedDocxPath);
        var expectedRaw = ExtractBodyText(expectedDocxPath);
        var expected = StripOutOfScope(expectedRaw, inScopeTags);

        var divergence = FindFirstDivergenceOffset(produced, expected);
        if (divergence is null)
        {
            return new DiffResult(true, null, null, null);
        }

        var offset = divergence.Value;
        return new DiffResult(
            IsMatch: false,
            FirstDivergenceOffset: offset,
            ProducedContext: SliceContext(produced, offset),
            ExpectedContext: SliceContext(expected, offset));
    }

    internal static string ExtractBodyText(string docxPath)
    {
        ArgumentException.ThrowIfNullOrEmpty(docxPath);

        using var doc = WordprocessingDocument.Open(docxPath, isEditable: false);
        var body = doc.MainDocumentPart?.Document?.Body;
        if (body is null)
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        var first = true;
        foreach (var paragraph in body.Descendants<Paragraph>())
        {
            // Only emit text from leaf paragraphs to avoid double-counting when
            // OpenXML nests paragraphs (rare, e.g. textbox content inside a paragraph).
            if (paragraph.Descendants<Paragraph>().Any())
            {
                continue;
            }

            var raw = ConcatenateParagraphRunText(paragraph);
            var normalized = NormalizeParagraphWhitespace(raw);

            if (!first)
            {
                sb.Append('\n');
            }
            sb.Append(normalized);
            first = false;
        }

        return sb.ToString();
    }

    private static string ConcatenateParagraphRunText(Paragraph paragraph)
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
                    sb.Append(' ');
                    break;
                case TabChar:
                    sb.Append(' ');
                    break;
            }
        }
        return sb.ToString();
    }

    internal static string NormalizeParagraphWhitespace(string paragraphText)
    {
        ArgumentNullException.ThrowIfNull(paragraphText);
        return WhitespaceRunPattern.Replace(paragraphText, " ").Trim();
    }

    internal static string StripOutOfScope(
        string text,
        IReadOnlyCollection<string> inScopeTags)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(inScopeTags);

        var inScope = inScopeTags as HashSet<string>
            ?? new HashSet<string>(inScopeTags, StringComparer.Ordinal);
        return StripOutOfScopeRecursive(text, inScope);
    }

    private static string StripOutOfScopeRecursive(string text, HashSet<string> inScope)
    {
        return TagPairPattern.Replace(text, match =>
        {
            var tag = match.Groups[1].Value;
            if (!inScope.Contains(tag))
            {
                return string.Empty;
            }

            var attrs = match.Groups[2].Value;
            var content = match.Groups[3].Value;
            var strippedContent = StripOutOfScopeRecursive(content, inScope);
            return string.Concat("[", tag, attrs, "]", strippedContent, "[/", tag, "]");
        });
    }

    internal static int? FindFirstDivergenceOffset(string a, string b)
    {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);

        var min = Math.Min(a.Length, b.Length);
        for (var i = 0; i < min; i++)
        {
            if (a[i] != b[i])
            {
                return i;
            }
        }
        return a.Length == b.Length ? null : min;
    }

    internal static string SliceContext(string text, int offset)
    {
        ArgumentNullException.ThrowIfNull(text);
        var clamped = Math.Clamp(offset, 0, text.Length);
        var start = Math.Max(0, clamped - ContextRadiusChars);
        var end = Math.Min(text.Length, clamped + ContextRadiusChars);
        return text[start..end];
    }
}
