using System.Xml.Linq;
using DocFormatter.Core.Pipeline;

namespace DocFormatter.Core.Jats;

/// <summary>
/// Injects <c>&lt;article-id pub-id-type="other"&gt;</c> immediately after the DOI
/// <c>&lt;article-id&gt;</c>, using the value resolved upstream from
/// <c>other.txt</c> (<see cref="Phase3Context.OtherNumber"/>). This is the fully
/// deterministic injector (TechSpec injection-rules table): there is no proposal
/// or confirmation. Placement follows
/// <c>docs/scielo_context/jats/article_id_other.md</c> — locate the DOI, fail if
/// absent (no orphan <c>other</c>), and insert right after it.
/// </summary>
/// <remarks>
/// Severity is <see cref="RuleSeverity.Critical"/>: a missing DOI or a missing
/// <c>other</c> number is a fail-loud precondition (ADR-001, ADR-004) that aborts
/// the document rather than emitting half-paired XML. The injector is idempotent
/// (ADR-005): if an <c>other</c> <c>article-id</c> already exists it is left
/// untouched and reported as skipped, so operator hand-edits survive re-runs.
/// </remarks>
public sealed class OtherIdInjector : IJatsInjector
{
    private const string ArticleIdName = "article-id";
    private const string PubIdTypeAttribute = "pub-id-type";
    private const string DoiType = "doi";
    private const string OtherType = "other";

    /// <inheritdoc />
    public string Name => "other-id";

    /// <inheritdoc />
    public RuleSeverity Severity => RuleSeverity.Critical;

    /// <inheritdoc />
    public void Apply(Phase3Context ctx, IReport report)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        ArgumentNullException.ThrowIfNull(report);

        // Idempotency first (ADR-005): never clobber an existing 'other' id.
        var existing = FindArticleId(ctx.Xml, OtherType);
        if (existing != null)
        {
            report.Info(Name, $"<{ArticleIdName} {PubIdTypeAttribute}=\"{OtherType}\"> already present (\"{existing.Value}\"); skipped.");
            return;
        }

        // Fail loudly when the structural anchor is missing — no orphan 'other'.
        var doi = FindArticleId(ctx.Xml, DoiType)
            ?? throw new InvalidOperationException(
                $"No <{ArticleIdName} {PubIdTypeAttribute}=\"{DoiType}\"> found; cannot place the 'other' id. Document aborted.");

        // The value is supplied externally (other.txt); a missing entry is a
        // deterministic precondition failure, not a guessable value.
        if (string.IsNullOrWhiteSpace(ctx.OtherNumber))
        {
            throw new InvalidOperationException(
                "OtherNumber is missing (no other.txt entry resolved); cannot inject the 'other' id. Tag aborted.");
        }

        // Inherit the DOI element's namespace so the injected node needs no
        // redundant xmlns, and match the DOI line's indentation depth.
        var injected = JatsXmlWriter.BuildLeaf(
            doi.Name.Namespace + ArticleIdName,
            ctx.OtherNumber,
            new[] { new XAttribute(PubIdTypeAttribute, OtherType) });
        JatsXmlWriter.InsertAfter(doi, injected, IndentDepthOf(doi));

        report.Info(Name, $"Inserted <{ArticleIdName} {PubIdTypeAttribute}=\"{OtherType}\">{ctx.OtherNumber}</{ArticleIdName}> after the DOI.");
    }

    private static XElement? FindArticleId(XDocument xml, string pubIdType)
        => xml.Descendants()
            .FirstOrDefault(e =>
                e.Name.LocalName == ArticleIdName
                && string.Equals((string?)e.Attribute(PubIdTypeAttribute), pubIdType, StringComparison.Ordinal));

    /// <summary>
    /// The indentation depth (tab count) of the line carrying <paramref name="anchor"/>,
    /// read from its preceding whitespace text node so the injected sibling aligns
    /// with it. Returns 0 when no leading whitespace is present.
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
