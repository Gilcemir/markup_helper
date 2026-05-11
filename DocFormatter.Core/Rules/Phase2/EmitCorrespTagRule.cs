using System.Text;
using System.Text.RegularExpressions;
using DocFormatter.Core.Models.Phase2;
using DocFormatter.Core.Pipeline;
using DocFormatter.Core.TagEmission;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace DocFormatter.Core.Rules.Phase2;

/// <summary>
/// Wraps the corresponding-author paragraph in <c>[corresp id="c1"]…[/corresp]</c>
/// and emits the inner <c>[email]…[/email]</c> wrapper around the address. The
/// paragraph is identified by either an asterisk-led <c>* E-mail: …</c> shape
/// (most articles) or a leading <c>Corresponding author: …</c> phrase (5458,
/// 5523, 5549 in the corpus). Each emitted tag literal lives in its own
/// <see cref="Run"/> so the Word Markup VBA <c>color(tag)</c> mapping paints
/// per-tag colors instead of staining the whole paragraph.
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
    public const string CorrespAlreadyTaggedMessage = "corresp_already_tagged";

    private const string TagName = "corresp";
    private const string EmailTagName = "email";
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

        var lookup = FindCorrespParagraph(body);
        if (lookup.AlreadyTagged)
        {
            // Idempotency: a prior run wrapped this paragraph. Signal it so
            // the diagnostic JSON can distinguish "already done" from
            // "nothing found."
            report.Info(Name, CorrespAlreadyTaggedMessage);
            return;
        }
        if (lookup.Paragraph is null || lookup.PlainText is null)
        {
            report.Warn(Name, CorrespBlockNotFoundMessage);
            return;
        }

        var paragraph = lookup.Paragraph;
        var plainText = lookup.PlainText;

        // Rebuild the paragraph as a sequence of independent Runs so the Word
        // Markup VBA `color(tag)` mapping can paint each tag literal in its
        // own color. The trailing whitespace of the original paragraph is
        // preserved verbatim — the AFTER corpus mostly mirrors the BEFORE
        // shape (5313, 5419, 5434, 5449 retain a trailing space inside
        // `[/corresp]`; 5293 / 5424 do not).
        var emailMatch = EmailPattern.Match(plainText);
        var emailValue = emailMatch.Success ? emailMatch.Value : null;
        RewriteCorrespParagraph(paragraph, plainText, emailMatch);

        var email = emailValue ?? ctx.CorrespondingEmail;
        // Same "first writer wins" precedence used by EmitAuthorXrefsRule for
        // ctx.Authors / Affiliations: defer to whatever Phase 1 or an earlier
        // Phase 2 rule already published.
        if (ctx.CorrespAuthor is null)
        {
            ctx.CorrespAuthor = new CorrespAuthor(
                ctx.CorrespondingAuthorIndex,
                email,
                ctx.CorrespondingOrcid,
                paragraph);
        }

        if (emailValue is null)
        {
            report.Info(Name, $"wrapped corresp paragraph in [{TagName} id=\"{CorrespId}\"]");
        }
        else
        {
            report.Info(
                Name,
                $"wrapped corresp paragraph in [{TagName} id=\"{CorrespId}\"] with inner [{EmailTagName}]");
        }
    }

    private static void RewriteCorrespParagraph(Paragraph paragraph, string text, Match emailMatch)
    {
        var pPr = paragraph.GetFirstChild<ParagraphProperties>()?.CloneNode(deep: true)
            as ParagraphProperties;

        paragraph.RemoveAllChildren();
        if (pPr is not null)
        {
            paragraph.AppendChild(pPr);
        }

        paragraph.AppendChild(TagEmitter.OpeningTag(TagName, OpeningAttrs));

        if (!emailMatch.Success)
        {
            if (text.Length > 0)
            {
                paragraph.AppendChild(BuildPlainRun(text));
            }
            paragraph.AppendChild(TagEmitter.ClosingTag(TagName));
            return;
        }

        var prefix = text[..emailMatch.Index];
        var suffix = text[(emailMatch.Index + emailMatch.Length)..];

        if (prefix.Length > 0)
        {
            paragraph.AppendChild(BuildPlainRun(prefix));
        }

        paragraph.AppendChild(TagEmitter.OpeningTag(EmailTagName, Array.Empty<(string, string)>()));
        paragraph.AppendChild(BuildPlainRun(emailMatch.Value));
        paragraph.AppendChild(TagEmitter.ClosingTag(EmailTagName));

        if (suffix.Length > 0)
        {
            paragraph.AppendChild(BuildPlainRun(suffix));
        }

        paragraph.AppendChild(TagEmitter.ClosingTag(TagName));
    }

    private static Run BuildPlainRun(string text)
        => new(
            RewriteHeaderMvpRule.CreateBaseRunProperties(),
            new Text(text) { Space = SpaceProcessingModeValues.Preserve });

    private static CorrespLookup FindCorrespParagraph(Body body)
    {
        foreach (var paragraph in body.Elements<Paragraph>())
        {
            var text = ParagraphPlainText(paragraph);

            // Idempotency: a paragraph already carrying a [corresp …] literal
            // means a prior run wrapped this block. Don't re-wrap, but signal
            // it to the caller separately from "not found."
            if (text.Contains("[corresp", StringComparison.Ordinal))
            {
                return new CorrespLookup(null, null, AlreadyTagged: true);
            }

            if (AsteriskMarkerPattern.IsMatch(text)
                || CorrespondingAuthorPattern.IsMatch(text))
            {
                return new CorrespLookup(paragraph, text, AlreadyTagged: false);
            }
        }
        return new CorrespLookup(null, null, AlreadyTagged: false);
    }

    private sealed record CorrespLookup(Paragraph? Paragraph, string? PlainText, bool AlreadyTagged);

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
