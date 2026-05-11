using System.Text;
using DocFormatter.Core.Rules;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;

namespace DocFormatter.Core.TagEmission;

/// <summary>
/// Emits SciELO bracket-syntax tag literals (e.g. <c>[abstract language="en"]…[/abstract]</c>)
/// into a Word document text flow as OpenXML <see cref="Run"/> elements.
///
/// <para>
/// All emitted runs use <see cref="RewriteHeaderMvpRule.CreateBaseRunProperties"/>
/// (Times New Roman 12pt) and set <c>Space = Preserve</c> on the wrapped
/// <see cref="Text"/>. Attribute values are emitted with double quotes and no
/// internal escaping per the DTD 4.0 invariant documented in
/// <c>docs/scielo_context/README.md</c>.
/// </para>
///
/// <para>
/// <b>Anti-duplication invariant.</b> SciELO Markup auto-mark macros
/// (see <c>docs/scielo_context/REENTRANCE.md</c>) re-emit certain tags
/// without checking for prior existence. Pre-marking any of the names
/// below with this helper will cause Markup to duplicate them:
/// <c>author</c>, <c>fname</c>, <c>surname</c>, <c>normaff</c>,
/// <c>doctitle</c>, <c>doi</c>. The inner Phase-2 tags <c>sectitle</c>,
/// <c>kwd</c>, <c>p</c> and <c>email</c> are <b>owned by Phase 2</b>
/// (see ADR-001 follow-up note) and are safe to pre-mark. This helper
/// does not enforce the list (rules choose their own tag names); callers
/// must not pass the still-restricted names.
/// </para>
///
/// <para>
/// <b>Superscript trap.</b> When wrapping a paragraph that contains
/// superscripted runs, <see cref="WrapParagraphContent"/> zeroes
/// <see cref="VerticalTextAlignment"/> on those runs to neutralize
/// SciELO Markup's <c>markup_sup_as</c> macro, which would otherwise
/// re-mark the superscript and produce nested literals.
/// </para>
/// </summary>
public static class TagEmitter
{
    public static Run OpeningTag(
        string tagName,
        IReadOnlyList<(string Key, string Value)> attrs)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tagName);
        ArgumentNullException.ThrowIfNull(attrs);

        return BuildColoredRun(BuildOpeningLiteral(tagName, attrs), TagColors.Lookup(tagName));
    }

    public static Run ClosingTag(string tagName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tagName);
        return BuildColoredRun($"[/{tagName}]", TagColors.Lookup(tagName));
    }

    /// <summary>
    /// Builds a <see cref="Run"/> for an already-formed tag literal string
    /// (e.g. <c>[xref ref-type="aff" rid="aff1"]</c>). Used by emitters that
    /// produce the literal via string transformations and need to wrap it in
    /// its own colored Run after the fact. The tag name is extracted from
    /// the literal so the color lookup can run.
    /// </summary>
    public static Run TagLiteralRun(string literal)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(literal);
        var name = ExtractTagName(literal);
        var color = name is null ? null : TagColors.Lookup(name);
        return BuildColoredRun(literal, color);
    }

    private static string? ExtractTagName(string literal)
    {
        // Accept "[name…]", "[/name]", or "[name]". The first run of word
        // characters after the opening bracket (and optional leading slash)
        // is the tag name.
        if (literal.Length < 2 || literal[0] != '[')
        {
            return null;
        }
        var start = literal[1] == '/' ? 2 : 1;
        var end = start;
        while (end < literal.Length)
        {
            var c = literal[end];
            if (!char.IsLetterOrDigit(c) && c != '-' && c != '_')
            {
                break;
            }
            end++;
        }
        return end > start ? literal[start..end] : null;
    }

    public static void InsertOpeningBefore(
        Paragraph anchor,
        string tagName,
        IReadOnlyList<(string Key, string Value)> attrs)
    {
        ArgumentNullException.ThrowIfNull(anchor);

        var run = OpeningTag(tagName, attrs);
        var firstInline = FindFirstInline(anchor);
        if (firstInline is null)
        {
            anchor.AppendChild(run);
        }
        else
        {
            anchor.InsertBefore(run, firstInline);
        }
    }

    public static void InsertClosingAfter(Paragraph anchor, string tagName)
    {
        ArgumentNullException.ThrowIfNull(anchor);

        var run = ClosingTag(tagName);
        var lastInline = FindLastInline(anchor);
        if (lastInline is null)
        {
            anchor.AppendChild(run);
        }
        else
        {
            anchor.InsertAfter(run, lastInline);
        }
    }

    public static void WrapParagraphContent(
        Paragraph paragraph,
        string tagName,
        IReadOnlyList<(string Key, string Value)> attrs)
    {
        ArgumentNullException.ThrowIfNull(paragraph);

        ZeroSuperscriptOnDescendantRuns(paragraph);
        InsertOpeningBefore(paragraph, tagName, attrs);
        InsertClosingAfter(paragraph, tagName);
    }

    private static Run BuildColoredRun(string text, string? color)
    {
        var rPr = RewriteHeaderMvpRule.CreateBaseRunProperties();
        if (!string.IsNullOrEmpty(color))
        {
            rPr.AppendChild(new Color { Val = color });
        }
        return new Run(rPr, new Text(text) { Space = SpaceProcessingModeValues.Preserve });
    }

    private static string BuildOpeningLiteral(
        string tagName,
        IReadOnlyList<(string Key, string Value)> attrs)
    {
        if (attrs.Count == 0)
        {
            return $"[{tagName}]";
        }

        var sb = new StringBuilder();
        sb.Append('[').Append(tagName);
        foreach (var (key, value) in attrs)
        {
            sb.Append(' ').Append(key).Append("=\"").Append(value ?? string.Empty).Append('"');
        }
        sb.Append(']');
        return sb.ToString();
    }

    private static OpenXmlElement? FindFirstInline(Paragraph paragraph)
    {
        foreach (var child in paragraph.ChildElements)
        {
            if (child is ParagraphProperties)
            {
                continue;
            }
            return child;
        }
        return null;
    }

    private static OpenXmlElement? FindLastInline(Paragraph paragraph)
    {
        OpenXmlElement? last = null;
        foreach (var child in paragraph.ChildElements)
        {
            if (child is ParagraphProperties)
            {
                continue;
            }
            last = child;
        }
        return last;
    }

    private static void ZeroSuperscriptOnDescendantRuns(Paragraph paragraph)
    {
        foreach (var run in paragraph.Descendants<Run>())
        {
            var vert = run.RunProperties?.VerticalTextAlignment;
            if (vert?.Val?.Value == VerticalPositionValues.Superscript)
            {
                vert.Remove();
            }
        }
    }
}
