using System.Xml.Linq;
using DocFormatter.Core.Pipeline;

namespace DocFormatter.Core.Jats;

/// <summary>
/// Injects one <c>&lt;fn fn-type="edited-by"&gt;</c> per responsible-editor role
/// line found on the paired docx (<see cref="DocxSource.AssociateEditor"/> /
/// <see cref="DocxSource.ScientificEditor"/>) into the single
/// <c>&lt;author-notes&gt;</c>, creating <c>&lt;author-notes&gt;</c> when absent.
/// The role is emitted in <c>&lt;label&gt;</c> (e.g. <c>SCIENTIFIC EDITOR:</c>) and
/// the name in <c>&lt;p&gt;</c>; <c>&lt;ext-link&gt;</c> is omitted because the
/// editor ORCID is not available from the author's docx (PRD: name + role only).
/// Placement follows <c>docs/scielo_context/jats/responsible_editor.md</c>.
/// </summary>
/// <remarks>
/// Severity is <see cref="RuleSeverity.Optional"/>: the responsible editor is
/// editorial metadata the author's docx may not carry (parts of the corpus have
/// no editor line), so an absent editor — or an unusual <c>&lt;article-meta&gt;</c>
/// with no place to host a new <c>&lt;author-notes&gt;</c> — is reported and
/// skipped rather than aborting the document. The injector is idempotent
/// (ADR-005): if any <c>edited-by</c> <c>&lt;fn&gt;</c> already exists it is left
/// untouched and reported as skipped, so operator hand-edits survive re-runs.
/// </remarks>
public sealed class EditedByInjector : IJatsInjector
{
    private const string AuthorNotesName = "author-notes";
    private const string ArticleMetaName = "article-meta";
    private const string AffName = "aff";
    private const string ContribGroupName = "contrib-group";
    private const string FnName = "fn";
    private const string LabelName = "label";
    private const string ParagraphName = "p";
    private const string FnTypeAttribute = "fn-type";
    private const string EditedByType = "edited-by";

    /// <inheritdoc />
    public string Name => "edited-by";

    /// <inheritdoc />
    public RuleSeverity Severity => RuleSeverity.Optional;

    /// <inheritdoc />
    public void Apply(Phase3Context ctx, IReport report)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        ArgumentNullException.ThrowIfNull(report);

        // The responsible editor is a list of (role-label, name) pairs read from
        // the docx, associate-first to match the curated example ordering.
        var editors = CollectEditors(ctx.Source);
        if (editors.Count == 0)
        {
            report.Info(Name, "No responsible editor on the docx source; skipped.");
            return;
        }

        // Idempotency first (ADR-005): never duplicate an existing edited-by fn.
        if (ctx.Xml.Descendants().Any(IsEditedByFn))
        {
            report.Info(Name, $"<{FnName} {FnTypeAttribute}=\"{EditedByType}\"> already present; skipped.");
            return;
        }

        var authorNotes = ctx.Xml.Descendants().FirstOrDefault(e => e.Name.LocalName == AuthorNotesName);
        if (authorNotes != null)
        {
            AppendToExisting(authorNotes, editors);
        }
        else if (!TryCreateAuthorNotes(ctx.Xml, editors, out authorNotes))
        {
            report.Info(
                Name,
                $"No <{AuthorNotesName}> and no <{AffName}>/<{ContribGroupName}> anchor in <{ArticleMetaName}> to create one; skipped.");
            return;
        }

        var applied = string.Join("; ", editors.Select(e => $"{e.Label} {e.Name}"));
        report.Info(Name, $"Injected {editors.Count} edited-by fn(s) into <{AuthorNotesName}>: {applied}.");
    }

    /// <summary>
    /// Appends one fn per editor after the last element child of an existing
    /// <c>&lt;author-notes&gt;</c> (e.g. after the <c>&lt;corresp&gt;</c>, which is
    /// preserved), or as its children when it is empty.
    /// </summary>
    private static void AppendToExisting(XElement authorNotes, IReadOnlyList<(string Label, string Name)> editors)
    {
        var fnDepth = IndentDepthOf(authorNotes) + 1;
        var ns = authorNotes.Name.Namespace;

        XNode? anchor = authorNotes.Elements().LastOrDefault();
        if (anchor == null)
        {
            // Empty <author-notes>: seed children directly, then close at its depth.
            foreach (var editor in editors)
            {
                authorNotes.Add(JatsXmlWriter.Indent(fnDepth));
                authorNotes.Add(BuildFn(ns, fnDepth, editor));
            }

            authorNotes.Add(JatsXmlWriter.Indent(fnDepth - 1));
            return;
        }

        foreach (var editor in editors)
        {
            var fn = BuildFn(ns, fnDepth, editor);
            JatsXmlWriter.InsertAfter((XElement)anchor, fn, fnDepth);
            anchor = fn;
        }
    }

    /// <summary>
    /// Creates a single <c>&lt;author-notes&gt;</c> hosting the fn(s) and inserts it
    /// at the JATS-valid slot after the last <c>&lt;aff&gt;</c> (else the last
    /// <c>&lt;contrib-group&gt;</c>) of <c>&lt;article-meta&gt;</c>. Returns
    /// <see langword="false"/> when no such anchor exists.
    /// </summary>
    private static bool TryCreateAuthorNotes(
        XDocument xml,
        IReadOnlyList<(string Label, string Name)> editors,
        out XElement authorNotes)
    {
        authorNotes = null!;

        var articleMeta = xml.Descendants().FirstOrDefault(e => e.Name.LocalName == ArticleMetaName);
        var anchor = articleMeta?.Elements().LastOrDefault(e => e.Name.LocalName == AffName)
            ?? articleMeta?.Elements().LastOrDefault(e => e.Name.LocalName == ContribGroupName);
        if (anchor == null)
        {
            return false;
        }

        var depth = IndentDepthOf(anchor);
        var ns = anchor.Name.Namespace;
        var fns = editors.Select(e => BuildFn(ns, depth + 1, e)).ToList();
        authorNotes = JatsXmlWriter.BuildElement(ns + AuthorNotesName, depth, attributes: null, fns);
        JatsXmlWriter.InsertAfter(anchor, authorNotes, depth);
        return true;
    }

    /// <summary>
    /// Builds <c>&lt;fn fn-type="edited-by"&gt;&lt;label&gt;ROLE:&lt;/label&gt;&lt;p&gt;name&lt;/p&gt;&lt;/fn&gt;</c>
    /// at <paramref name="depth"/>, inheriting the document namespace from
    /// <paramref name="ns"/> so no redundant <c>xmlns</c> is emitted.
    /// </summary>
    private static XElement BuildFn(XNamespace ns, int depth, (string Label, string Name) editor)
    {
        var label = JatsXmlWriter.BuildLeaf(ns + LabelName, editor.Label);
        var paragraph = JatsXmlWriter.BuildLeaf(ns + ParagraphName, editor.Name);
        return JatsXmlWriter.BuildElement(
            ns + FnName,
            depth,
            new[] { new XAttribute(FnTypeAttribute, EditedByType) },
            new[] { label, paragraph });
    }

    /// <summary>
    /// The (role-label, name) pairs found on the docx source, in associate-then-
    /// scientific order (the curated <c>responsible_editor.md</c> ordering). Blank
    /// names are dropped.
    /// </summary>
    private static List<(string Label, string Name)> CollectEditors(DocxSource source)
    {
        var editors = new List<(string Label, string Name)>();
        if (!string.IsNullOrWhiteSpace(source.AssociateEditor))
        {
            editors.Add(("ASSOCIATE EDITOR:", source.AssociateEditor.Trim()));
        }

        if (!string.IsNullOrWhiteSpace(source.ScientificEditor))
        {
            editors.Add(("SCIENTIFIC EDITOR:", source.ScientificEditor.Trim()));
        }

        return editors;
    }

    private static bool IsEditedByFn(XElement element)
        => element.Name.LocalName == FnName
            && string.Equals((string?)element.Attribute(FnTypeAttribute), EditedByType, StringComparison.Ordinal);

    /// <summary>
    /// The indentation depth (tab count) of the line carrying <paramref name="anchor"/>,
    /// read from its preceding whitespace text node so injected siblings align with
    /// it. Returns 0 when no leading whitespace is present.
    /// </summary>
    private static int IndentDepthOf(XElement anchor)
    {
        if (anchor.PreviousNode is not XText text)
        {
            return 0;
        }

        var value = text.Value;
        var newLineIndex = value.LastIndexOf('\n');
        var indent = newLineIndex >= 0 ? value[(newLineIndex + 1)..] : value;
        return indent.Count(c => c == '\t');
    }
}
