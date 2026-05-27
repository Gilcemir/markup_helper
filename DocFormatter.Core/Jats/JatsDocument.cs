using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace DocFormatter.Core.Jats;

/// <summary>
/// A JATS XML document loaded with its surrounding text preserved so that a
/// load → mutate → save cycle leaves every untouched line byte-identical
/// (TechSpec "Whitespace-preserving XML write"; ADR-002 "the XML is the only
/// artifact modified").
/// </summary>
/// <remarks>
/// <para>
/// <see cref="System.Xml.Linq"/> cannot round-trip a SciELO package
/// byte-for-byte on its own: the XML parser normalises line endings and drops
/// the formatting inside the <c>DOCTYPE</c>, and <see cref="XmlWriter"/> always
/// re-canonicalises the declaration and emits empty elements as <c>&lt;x /&gt;</c>
/// rather than the corpus' <c>&lt;x/&gt;</c>. To stay byte-identical this type
/// keeps the original prolog (declaration + <c>DOCTYPE</c> + leading whitespace)
/// and epilog verbatim and only re-serialises the root element, undoing the two
/// systematic <see cref="XmlWriter"/> divergences (newline convention and the
/// empty-element space) on the way out.
/// </para>
/// <para>
/// The <c>specific-use</c> version attribute (e.g. <c>sps-1.9</c>) is never read
/// or rewritten here; version drift is flagged elsewhere, not corrected on write
/// (TechSpec "Known Risks").
/// </para>
/// </remarks>
public sealed class JatsDocument
{
    private readonly string _prolog;
    private readonly string _epilog;
    private readonly string _newLine;
    private readonly bool _hasByteOrderMark;

    internal JatsDocument(XDocument document, string prolog, string epilog, string newLine, bool hasByteOrderMark)
    {
        Document = document;
        _prolog = prolog;
        _epilog = epilog;
        _newLine = newLine;
        _hasByteOrderMark = hasByteOrderMark;
    }

    /// <summary>
    /// The parsed document. Injectors mutate this tree; everything outside the
    /// root element (declaration, <c>DOCTYPE</c>, surrounding whitespace) is
    /// fixed and reproduced verbatim on <see cref="Save"/>.
    /// </summary>
    public XDocument Document { get; }

    /// <summary>The line ending detected in the source (<c>\r\n</c> or <c>\n</c>).</summary>
    public string NewLine => _newLine;

    /// <summary>
    /// Serialises the document and writes it to <paramref name="path"/> without
    /// re-indenting existing nodes. The declaration, <c>DOCTYPE</c> and any
    /// byte-order mark are reproduced exactly; only the root element subtree is
    /// re-serialised, so a no-op cycle is byte-identical and an injection changes
    /// only the injected lines.
    /// </summary>
    public void Save(string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        File.WriteAllText(path, Serialize(), new UTF8Encoding(_hasByteOrderMark));
    }

    /// <summary>
    /// Produces the full document text: the preserved prolog, the re-serialised
    /// root element, and the preserved epilog. Exposed for diffing in tests.
    /// </summary>
    public string Serialize() => _prolog + SerializeRoot() + _epilog;

    private string SerializeRoot()
    {
        var root = Document.Root
            ?? throw new InvalidOperationException("The JATS document has no root element to serialise.");

        var settings = new XmlWriterSettings
        {
            OmitXmlDeclaration = true,          // the declaration lives in the preserved prolog
            Indent = false,                     // never reformat existing nodes
            NewLineHandling = NewLineHandling.Replace,
            NewLineChars = _newLine,             // restore the source line ending the parser normalised away
            ConformanceLevel = ConformanceLevel.Fragment,
        };

        var builder = new StringBuilder();
        using (var writer = XmlWriter.Create(builder, settings))
        {
            root.WriteTo(writer);
        }

        // XmlWriter emits empty elements as "<x />"; the SciELO corpus writes
        // "<x/>". Text content never yields a literal " />" (XmlWriter escapes
        // '>' as "&gt;"), so this rewrite only touches empty-element closers.
        var serialized = RestoreTextContentQuotes(builder.ToString());
        return serialized.Replace(" />", "/>", StringComparison.Ordinal);
    }

    // XmlWriter writes a double quote in element text content as a bare '"', but
    // the SciELO corpus escapes it as "&quot;" (the corpus never uses a bare '"'
    // in text content). Restoring the entity form in text content only — leaving
    // markup, attribute delimiters, comments and CDATA untouched — is the last
    // systematic XmlWriter divergence, so a no-op cycle stays byte-identical and
    // an injection diffs to the injected tags alone (the writer's documented
    // contract; ADR-002 "the XML is the only artifact modified").
    private static string RestoreTextContentQuotes(string xml)
    {
        var builder = new StringBuilder(xml.Length);
        var i = 0;
        while (i < xml.Length)
        {
            var c = xml[i];
            if (c == '<')
            {
                var end = ScanMarkupEnd(xml, i);
                builder.Append(xml, i, end - i);
                i = end;
                continue;
            }

            if (c == '"')
            {
                builder.Append("&quot;");
            }
            else
            {
                builder.Append(c);
            }
            i++;
        }

        return builder.ToString();
    }

    // Given <paramref name="start"/> at a '<', returns the index just past the
    // markup construct beginning there (tag, comment, CDATA, or processing
    // instruction). Attribute quoting is honored so a '>' inside an attribute
    // value or a comment does not end the construct early.
    private static int ScanMarkupEnd(string text, int start)
    {
        if (Matches(text, start, "<!--"))
        {
            var close = text.IndexOf("-->", start + 4, StringComparison.Ordinal);
            return close < 0 ? text.Length : close + 3;
        }

        if (Matches(text, start, "<![CDATA["))
        {
            var close = text.IndexOf("]]>", start + 9, StringComparison.Ordinal);
            return close < 0 ? text.Length : close + 3;
        }

        var quote = '\0';
        for (var i = start + 1; i < text.Length; i++)
        {
            var c = text[i];
            if (quote != '\0')
            {
                if (c == quote)
                {
                    quote = '\0';
                }
            }
            else if (c is '"' or '\'')
            {
                quote = c;
            }
            else if (c == '>')
            {
                return i + 1;
            }
        }

        return text.Length;
    }

    private static bool Matches(string text, int index, string token) =>
        index + token.Length <= text.Length
        && string.CompareOrdinal(text, index, token, 0, token.Length) == 0;
}
