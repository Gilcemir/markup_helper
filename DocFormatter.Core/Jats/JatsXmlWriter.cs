using System.Text;
using System.Xml.Linq;

namespace DocFormatter.Core.Jats;

/// <summary>
/// Loads a JATS XML package preserving its existing whitespace and builds
/// injected elements indented to match the surrounding sibling style, so the
/// only golden-corpus diff is the injected tags (TechSpec "Whitespace-preserving
/// XML write"). The loaded <see cref="JatsDocument"/> owns the non-reformatting
/// save; this type owns loading and the element-construction helpers the
/// injectors (task_07–task_10) use.
/// </summary>
public static class JatsXmlWriter
{
    /// <summary>The corpus indentation unit: one tab per nesting level.</summary>
    public const string DefaultIndentUnit = "\t";

    // Indentation text nodes always carry '\n'; the XML parser normalises every
    // line ending to '\n' in memory, and JatsDocument.Save restores the source
    // convention. Building with the source newline here would double-convert.
    private const string InMemoryNewLine = "\n";

    /// <summary>
    /// Loads <paramref name="path"/> with <see cref="LoadOptions.PreserveWhitespace"/>
    /// and captures the declaration, <c>DOCTYPE</c> and surrounding text so the
    /// document can be saved without reformatting. The document namespaces
    /// (<c>xlink</c>, <c>mml</c>) are declared on the root and inherited by any
    /// injected element.
    /// </summary>
    public static JatsDocument Load(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        var document = XDocument.Load(path, LoadOptions.PreserveWhitespace);

        var bytes = File.ReadAllBytes(path);
        var hasByteOrderMark = bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF;
        var offset = hasByteOrderMark ? 3 : 0;
        var text = new UTF8Encoding(false).GetString(bytes, offset, bytes.Length - offset);

        var rootName = document.Root?.Name.LocalName
            ?? throw new InvalidOperationException($"'{path}' has no root element.");

        var (start, end) = FindRootBounds(text, rootName, path);
        var prolog = text[..start];
        var epilog = text[end..];
        var newLine = text.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";

        return new JatsDocument(document, prolog, epilog, newLine, hasByteOrderMark);
    }

    /// <summary>
    /// The whitespace text node that places a sibling at indentation
    /// <paramref name="depth"/>: a newline followed by <paramref name="depth"/>
    /// indent units. Use it as the separator before an injected element.
    /// </summary>
    public static XText Indent(int depth, string indentUnit = DefaultIndentUnit)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(depth);
        ArgumentNullException.ThrowIfNull(indentUnit);

        return new XText(InMemoryNewLine + string.Concat(Enumerable.Repeat(indentUnit, depth)));
    }

    /// <summary>
    /// Builds a leaf element (<c>&lt;name attrs&gt;value&lt;/name&gt;</c>) with no
    /// internal whitespace. <paramref name="name"/> may carry a namespace (e.g.
    /// <c>xlink:href</c> on an attribute); the prefix resolves against the root's
    /// declarations when the element is serialised in the tree, so no redundant
    /// <c>xmlns</c> is emitted.
    /// </summary>
    public static XElement BuildLeaf(XName name, string? value, IEnumerable<XAttribute>? attributes = null)
    {
        ArgumentNullException.ThrowIfNull(name);

        var element = new XElement(name);
        AddAttributes(element, attributes);
        if (value != null)
        {
            element.Add(value);
        }

        return element;
    }

    /// <summary>
    /// Builds a block element whose <paramref name="children"/> are indented one
    /// level deeper than <paramref name="depth"/> and whose end tag aligns back
    /// to <paramref name="depth"/>, matching the surrounding sibling block style.
    /// </summary>
    /// <param name="depth">Indentation level of this element's start tag.</param>
    public static XElement BuildElement(
        XName name,
        int depth,
        IEnumerable<XAttribute>? attributes,
        IEnumerable<XElement> children,
        string indentUnit = DefaultIndentUnit)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(children);
        ArgumentOutOfRangeException.ThrowIfNegative(depth);

        var element = new XElement(name);
        AddAttributes(element, attributes);

        var any = false;
        foreach (var child in children)
        {
            any = true;
            element.Add(Indent(depth + 1, indentUnit));
            element.Add(child);
        }

        if (any)
        {
            element.Add(Indent(depth, indentUnit));
        }

        return element;
    }

    /// <summary>
    /// Inserts <paramref name="injected"/> immediately after <paramref name="anchor"/>
    /// as a sibling, prefixing the indentation whitespace for
    /// <paramref name="depth"/> so the existing following lines stay
    /// byte-identical and the diff shows only the injected element.
    /// </summary>
    public static void InsertAfter(XElement anchor, XElement injected, int depth, string indentUnit = DefaultIndentUnit)
    {
        ArgumentNullException.ThrowIfNull(anchor);
        ArgumentNullException.ThrowIfNull(injected);

        anchor.AddAfterSelf(injected);
        injected.AddBeforeSelf(Indent(depth, indentUnit));
    }

    private static void AddAttributes(XElement element, IEnumerable<XAttribute>? attributes)
    {
        if (attributes == null)
        {
            return;
        }

        foreach (var attribute in attributes)
        {
            element.Add(attribute);
        }
    }

    private static (int Start, int End) FindRootBounds(string text, string rootName, string path)
    {
        // The root start tag is the first "<rootName" followed by a name
        // boundary. The DOCTYPE references the name as " article" (no '<') and
        // sibling tags such as "<article-id" begin only inside the root, so the
        // first boundary-terminated match is the root opening tag.
        var open = "<" + rootName;
        var index = text.IndexOf(open, StringComparison.Ordinal);
        while (index >= 0)
        {
            var after = index + open.Length;
            if (after >= text.Length || IsNameBoundary(text[after]))
            {
                var closeTag = "</" + rootName + ">";
                var closeIndex = text.LastIndexOf(closeTag, StringComparison.Ordinal);
                if (closeIndex < 0)
                {
                    // A self-closing root has no separate end tag.
                    closeIndex = text.IndexOf("/>", index, StringComparison.Ordinal);
                    return (index, closeIndex >= 0 ? closeIndex + 2 : text.Length);
                }

                return (index, closeIndex + closeTag.Length);
            }

            index = text.IndexOf(open, after, StringComparison.Ordinal);
        }

        throw new InvalidOperationException($"Could not locate the <{rootName}> root element in '{path}'.");
    }

    private static bool IsNameBoundary(char c) => c is ' ' or '\t' or '\r' or '\n' or '>' or '/';
}
