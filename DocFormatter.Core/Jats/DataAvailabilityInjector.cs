using System.Xml.Linq;
using DocFormatter.Core.Pipeline;

namespace DocFormatter.Core.Jats;

/// <summary>
/// Injects a <c>&lt;sec sec-type="data-availability" specific-use="…"&gt;</c> with
/// a mandatory <c>&lt;title&gt;</c> and the statement <c>&lt;p&gt;</c> taken from
/// the paired docx (<see cref="DocxSource.DataAvailabilityText"/>). Placement
/// follows <c>docs/scielo_context/jats/data_availability.md</c> (location=back):
/// immediately after <c>&lt;ack&gt;</c> in <c>&lt;back&gt;</c>, else before the
/// first child of <c>&lt;back&gt;</c>, else as its last child. The statement text
/// is deterministic; only the <c>@specific-use</c> category is a judgment call,
/// classified by <see cref="DataAvailabilityClassifier"/> and auto-applied when
/// confident or proposed to the <see cref="IConfirmer"/> gate otherwise.
/// </summary>
/// <remarks>
/// Severity is <see cref="RuleSeverity.Optional"/>: the data-availability
/// statement is author-supplied metadata the docx may not carry, and a document
/// with no <c>&lt;back&gt;</c> has no valid placement slot — both are reported and
/// skipped rather than aborting the document. The injector is idempotent
/// (ADR-005): if a data-availability <c>&lt;sec&gt;</c> or <c>&lt;fn&gt;</c>
/// already exists it is left untouched and reported as skipped, so operator
/// hand-edits survive re-runs.
/// </remarks>
public sealed class DataAvailabilityInjector : IJatsInjector
{
    private const string BackName = "back";
    private const string AckName = "ack";
    private const string SecName = "sec";
    private const string FnName = "fn";
    private const string TitleName = "title";
    private const string ParagraphName = "p";
    private const string SecTypeAttribute = "sec-type";
    private const string FnTypeAttribute = "fn-type";
    private const string SpecificUseAttribute = "specific-use";
    private const string DataAvailabilityType = "data-availability";

    /// <summary>
    /// The mandatory section title. The corpus is English (<c>language="en"</c>),
    /// so the standard SPS English label is used; the value mirrors the
    /// <c>&lt;label&gt;</c> shown for the <c>&lt;fn&gt;</c> form in
    /// <c>data_availability.md</c>.
    /// </summary>
    private const string SectionTitle = "Data Availability Statement";

    /// <inheritdoc />
    public string Name => "data-availability";

    /// <inheritdoc />
    public RuleSeverity Severity => RuleSeverity.Optional;

    /// <inheritdoc />
    public void Apply(Phase3Context ctx, IReport report)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        ArgumentNullException.ThrowIfNull(report);

        // The statement text is author-supplied; an absent section is a valid
        // downstream-reportable state, not an error (DocxSource contract).
        var statement = ctx.Source.DataAvailabilityText;
        if (string.IsNullOrWhiteSpace(statement))
        {
            report.Info(Name, "No data-availability statement on the docx source; skipped.");
            return;
        }

        statement = statement.Trim();

        // Idempotency first (ADR-005): never duplicate either marked-up form.
        if (ctx.Xml.Descendants().Any(IsDataAvailability))
        {
            report.Info(Name, $"<{SecName}>/<{FnName}> {DataAvailabilityType} already present; skipped.");
            return;
        }

        var back = ctx.Xml.Descendants().FirstOrDefault(e => e.Name.LocalName == BackName);
        if (back == null)
        {
            report.Info(Name, $"No <{BackName}> element to host the data-availability section; skipped.");
            return;
        }

        // @specific-use is the only non-deterministic part: classify, then either
        // auto-apply a confident result or propose it to the confirmer (ADR-001).
        if (!TryResolveSpecificUse(ctx, statement, out var specificUse, out var disposition, out var reason))
        {
            report.Warn(Name, $"Data-availability category not resolved ({reason}); skipped.");
            return;
        }

        var where = Place(back, specificUse, statement);

        report.Info(
            Name,
            $"Injected <{SecName} {SecTypeAttribute}=\"{DataAvailabilityType}\" {SpecificUseAttribute}=\"{specificUse}\"> " +
            $"{where} ({disposition}: {reason}).");
    }

    /// <summary>
    /// Resolves the <c>@specific-use</c> value and how it was chosen. A confident
    /// classification is auto-applied; otherwise a <see cref="Proposal"/> is sent
    /// to the confirmer and its <see cref="ConfirmResult"/> decides the value and
    /// disposition. Returns <see langword="false"/> when the confirmer declines
    /// (or yields a blank value), as the mandatory attribute cannot then be written.
    /// </summary>
    private bool TryResolveSpecificUse(
        Phase3Context ctx,
        string statement,
        out string specificUse,
        out ConfirmDisposition disposition,
        out string reason)
    {
        var classification = DataAvailabilityClassifier.Classify(statement);
        if (classification.IsConfident)
        {
            specificUse = classification.SpecificUse;
            disposition = ConfirmDisposition.AutoApplied;
            reason = "auto-classified";
            return true;
        }

        var proposal = new Proposal(Name, classification.SpecificUse, classification.Reason);
        var result = ctx.Confirm.Confirm(proposal);
        disposition = result.Disposition;
        reason = $"proposed '{classification.SpecificUse}'";

        if (result.Disposition == ConfirmDisposition.Skipped || string.IsNullOrWhiteSpace(result.Value))
        {
            specificUse = string.Empty;
            return false;
        }

        specificUse = result.Value;
        return true;
    }

    /// <summary>
    /// Places the section per the back-relative placement rule: after
    /// <c>&lt;ack&gt;</c> when present, else before the first child of
    /// <c>&lt;back&gt;</c>, else as its sole/last child. The section is built at
    /// the resolved depth so its <c>&lt;title&gt;</c>/<c>&lt;p&gt;</c> children
    /// indent correctly.
    /// </summary>
    private static string Place(XElement back, string specificUse, string statement)
    {
        var ns = back.Name.Namespace;

        var ack = back.Elements().FirstOrDefault(e => e.Name.LocalName == AckName);
        if (ack != null)
        {
            var depth = IndentDepthOf(ack);
            JatsXmlWriter.InsertAfter(ack, BuildSection(ns, depth, specificUse, statement), depth);
            return $"after <{AckName}>";
        }

        var firstChild = back.Elements().FirstOrDefault();
        if (firstChild != null)
        {
            var depth = IndentDepthOf(firstChild);
            var sec = BuildSection(ns, depth, specificUse, statement);
            firstChild.AddBeforeSelf(sec);
            sec.AddAfterSelf(JatsXmlWriter.Indent(depth));
            return $"before the first child of <{BackName}>";
        }

        // Empty <back>: seed the section as its sole child, then close at back depth.
        var childDepth = IndentDepthOf(back) + 1;
        back.Add(JatsXmlWriter.Indent(childDepth));
        back.Add(BuildSection(ns, childDepth, specificUse, statement));
        back.Add(JatsXmlWriter.Indent(childDepth - 1));
        return $"as the last child of <{BackName}>";
    }

    /// <summary>
    /// Builds <c>&lt;sec sec-type="data-availability" specific-use="…"&gt;
    /// &lt;title&gt;…&lt;/title&gt;&lt;p&gt;…&lt;/p&gt;&lt;/sec&gt;</c> at
    /// <paramref name="depth"/>, inheriting <paramref name="ns"/> so no redundant
    /// <c>xmlns</c> is emitted.
    /// </summary>
    private static XElement BuildSection(XNamespace ns, int depth, string specificUse, string statement)
    {
        var title = JatsXmlWriter.BuildLeaf(ns + TitleName, SectionTitle);
        var paragraph = JatsXmlWriter.BuildLeaf(ns + ParagraphName, statement);
        return JatsXmlWriter.BuildElement(
            ns + SecName,
            depth,
            new[]
            {
                new XAttribute(SecTypeAttribute, DataAvailabilityType),
                new XAttribute(SpecificUseAttribute, specificUse),
            },
            new[] { title, paragraph });
    }

    private static bool IsDataAvailability(XElement element)
        => (element.Name.LocalName == SecName
                && string.Equals((string?)element.Attribute(SecTypeAttribute), DataAvailabilityType, StringComparison.Ordinal))
            || (element.Name.LocalName == FnName
                && string.Equals((string?)element.Attribute(FnTypeAttribute), DataAvailabilityType, StringComparison.Ordinal));

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
