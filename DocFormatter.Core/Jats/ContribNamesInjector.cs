using System.Text.RegularExpressions;
using System.Xml.Linq;
using DocFormatter.Core.Pipeline;

namespace DocFormatter.Core.Jats;

/// <summary>
/// Verification-only Phase 3 rule (ADR-002): warns, once per
/// <c>&lt;contrib&gt;</c>, when its <c>&lt;surname&gt;</c> is empty, matches
/// the ORCID pattern, or contains digits — the shapes SciELO Markup produces
/// when a plain-text ORCID or an affiliation label is promoted into the byline.
/// The rule never writes to the XML and never touches
/// <see cref="Phase3Context.Credit"/>, so a broken name is visible in every
/// article's report — including one without a CREDIT statement — without
/// blocking the <c>credit-roles</c> injector that runs after it.
/// </summary>
/// <remarks>
/// Severity is <see cref="RuleSeverity.Optional"/>: the byline is fixed in
/// Markup independently of CRediT, so a defect here is a pendency for the
/// operator, not a reason to abort the document. A well-formed contributor set
/// emits nothing (no INFO noise). A <c>&lt;contrib&gt;</c> with no
/// <c>&lt;surname&gt;</c> element at all (e.g. a <c>&lt;collab&gt;</c>) is not a
/// broken name and is skipped. The ORCID pattern is local rather than the
/// Phase 1 <c>FormattingOptions</c> regex because the Phase 3 pipeline does not
/// receive <c>FormattingOptions</c>, and the pattern is unanchored so an ORCID
/// glued to another token still matches.
/// </remarks>
public sealed partial class ContribNamesInjector : IJatsInjector
{
    private const string ContribName = "contrib";
    private const string SurnameName = "surname";

    /// <inheritdoc />
    /// <summary>The report label of this rule; the batch summary counts its WARN entries by it.</summary>
    public const string RuleName = "contrib-names";

    public string Name => RuleName;

    /// <inheritdoc />
    public RuleSeverity Severity => RuleSeverity.Optional;

    /// <inheritdoc />
    public void Apply(Phase3Context ctx, IReport report)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        ArgumentNullException.ThrowIfNull(report);

        var contribs = ctx.Xml.Descendants().Where(e => e.Name.LocalName == ContribName).ToList();
        for (var i = 0; i < contribs.Count; i++)
        {
            var surname = contribs[i].Descendants().FirstOrDefault(e => e.Name.LocalName == SurnameName);
            if (surname is null)
            {
                continue;
            }

            var defect = Describe(surname.Value.Trim());
            if (defect is not null)
            {
                // 1-based index: the operator counts contributors as they appear in
                // Markup, not as a zero-based list.
                report.Warn(Name, $"<{ContribName}> #{i + 1} {defect}; fix the byline in SciELO Markup.");
            }
        }
    }

    /// <summary>
    /// The defect wording for a trimmed surname text, or <see langword="null"/>
    /// when the surname is well-formed. ORCID is checked before the digit test so
    /// a surname that is (or contains) an ORCID is named as such rather than as
    /// generic digits.
    /// </summary>
    private static string? Describe(string surname)
    {
        if (surname.Length == 0)
        {
            return $"has an empty <{SurnameName}>";
        }

        if (OrcidRegex().IsMatch(surname))
        {
            return $"has an ORCID in <{SurnameName}> '{surname}' instead of a name";
        }

        if (surname.Any(char.IsDigit))
        {
            return $"has digits in <{SurnameName}> '{surname}'";
        }

        return null;
    }

    [GeneratedRegex(@"\d{4}-\d{4}-\d{4}-\d{3}[\dX]", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex OrcidRegex();
}
