using System.Text;
using System.Text.RegularExpressions;
using DocFormatter.Core.Models.Phase2;
using DocFormatter.Core.Pipeline;
using DocFormatter.Core.TagEmission;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace DocFormatter.Core.Rules.Phase2;

/// <summary>
/// Wraps the corresponding-author paragraph in <c>[corresp id="c1"]…[/corresp]</c>.
/// The paragraph is identified by either an asterisk-led <c>* E-mail: …</c>
/// shape (most articles) or a leading <c>Corresponding author: …</c> phrase
/// (5458, 5523, 5549 in the corpus). Per ADR-001 anti-duplication invariants,
/// this rule does NOT pre-mark the inner <c>[email]</c> wrapper — Markup
/// auto-marks it (task 09 / future) and pre-marking would duplicate.
///
/// <para>
/// Per ADR-002 the rule skips and warns rather than aborting. Reason code
/// <see cref="CorrespBlockNotFoundMessage"/> is recorded when no paragraph in
/// the body matches the recognized shapes.
/// </para>
/// </summary>
public sealed class EmitCorrespTagRule : IFormattingRule
{
    public const string CorrespBlockNotFoundMessage = "corresp_block_not_found";

    private const string TagName = "corresp";
    private const string CorrespId = "c1";

    private static readonly IReadOnlyList<(string Key, string Value)> OpeningAttrs =
        new[] { ("id", CorrespId) };

    // "* E-mail: name@host" / "*Email: name@host" / "* Email: name@host" — case
    // insensitive, optional whitespace and dash. The asterisk anchors the line.
    private static readonly Regex AsteriskMarkerPattern = new(
        @"^\s*\*\s*E\s*-?\s*mail\s*:",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // "Corresponding author: name@host" — used when the corresp paragraph
    // doesn't carry the asterisk marker (5458, 5523, 5549).
    private static readonly Regex CorrespondingAuthorPattern = new(
        @"^\s*Corresponding\s+author\s*:",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex EmailPattern = new(
        @"[\w.+-]+@[\w.-]+\.[A-Za-z]{2,}",
        RegexOptions.Compiled);

    public string Name => nameof(EmitCorrespTagRule);

    public RuleSeverity Severity => RuleSeverity.Optional;

    public void Apply(WordprocessingDocument doc, FormattingContext ctx, IReport report)
    {
        ArgumentNullException.ThrowIfNull(doc);
        ArgumentNullException.ThrowIfNull(ctx);
        ArgumentNullException.ThrowIfNull(report);

        var body = doc.MainDocumentPart?.Document?.Body;
        if (body is null)
        {
            report.Warn(Name, CorrespBlockNotFoundMessage);
            return;
        }

        var (paragraph, plainText) = FindCorrespParagraph(body);
        if (paragraph is null || plainText is null)
        {
            report.Warn(Name, CorrespBlockNotFoundMessage);
            return;
        }

        // Preserve the paragraph's trailing whitespace verbatim — the AFTER
        // corpus mostly mirrors the BEFORE shape (5313, 5419, 5434, 5449
        // retain a trailing space inside `[/corresp]`; 5293 / 5424 do not).
        // Trimming is therefore left to corpus amendment when the BEFORE
        // diverges from the AFTER on whitespace.
        TagEmitter.InsertOpeningBefore(paragraph, TagName, OpeningAttrs);
        TagEmitter.InsertClosingAfter(paragraph, TagName);

        var email = ExtractEmail(plainText) ?? ctx.CorrespondingEmail;
        ctx.CorrespAuthor = new CorrespAuthor(
            ctx.CorrespondingAuthorIndex,
            email,
            ctx.CorrespondingOrcid,
            paragraph);

        report.Info(Name, $"wrapped corresp paragraph in [{TagName} id=\"{CorrespId}\"]");
    }

    private static (Paragraph? Paragraph, string? PlainText) FindCorrespParagraph(Body body)
    {
        foreach (var paragraph in body.Elements<Paragraph>())
        {
            var text = ParagraphPlainText(paragraph);

            // Skip paragraphs that already carry a [corresp …] literal — the
            // rule must be idempotent against re-runs (test fixtures may seed
            // the wrapper). The inner [email]…[/email] is allowed: that's
            // Markup's territory (task 09 / future).
            if (text.Contains("[corresp", StringComparison.Ordinal))
            {
                return (null, null);
            }

            if (AsteriskMarkerPattern.IsMatch(text)
                || CorrespondingAuthorPattern.IsMatch(text))
            {
                return (paragraph, text);
            }
        }
        return (null, null);
    }

    private static string? ExtractEmail(string text)
    {
        var match = EmailPattern.Match(text);
        return match.Success ? match.Value : null;
    }

    private static string ParagraphPlainText(Paragraph paragraph)
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
            }
        }
        return sb.ToString();
    }

}
