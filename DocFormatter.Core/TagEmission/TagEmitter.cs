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
/// <c>author</c>, <c>fname</c>, <c>surname</c>, <c>kwd</c>,
/// <c>normaff</c>, <c>doctitle</c>, <c>doi</c>. This helper does not
/// enforce the list (rules choose their own tag names); callers must
/// not pass these names.
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

        return BuildRun(BuildOpeningLiteral(tagName, attrs));
    }

    public static Run ClosingTag(string tagName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tagName);
        return BuildRun($"[/{tagName}]");
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

    private static Run BuildRun(string text)
        => new(
            RewriteHeaderMvpRule.CreateBaseRunProperties(),
            new Text(text) { Space = SpaceProcessingModeValues.Preserve });

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
