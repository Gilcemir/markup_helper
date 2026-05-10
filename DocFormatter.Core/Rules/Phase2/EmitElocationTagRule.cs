using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using DocFormatter.Core.Pipeline;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace DocFormatter.Core.Rules.Phase2;

/// <summary>
/// Closes the gap between <c>before/&lt;id&gt;.docx</c> (placeholder
/// <c>elocatid="xxx"</c> in the <c>[doc]</c> opening tag plus a standalone
/// <c>e&lt;digits&gt;</c> paragraph in the body) and the
/// <c>after/&lt;id&gt;.docx</c> shape (real <c>elocatid</c> in the opening
/// tag, no separate paragraph). The rule:
///   1. locates the standalone elocation paragraph (a paragraph whose
///      normalized text matches <c>e&lt;digits&gt;</c>);
///   2. rewrites the <c>[doc]</c> opening-tag literal in place, replacing the
///      <c>elocatid</c> attribute with the discovered ID and the
///      <c>issueno</c> attribute with the digit derived from the ID's
///      conventional position (this journal's elocation IDs encode
///      <c>e&lt;article&gt;&lt;volid&gt;&lt;issueno&gt;&lt;order&gt;</c>);
///   3. removes the standalone paragraph from the body.
///
/// <para>
/// Per ADR-002 the rule skips and warns rather than aborting. Reason codes:
/// <see cref="ElocationParagraphMissingMessage"/> when no
/// <c>e&lt;digits&gt;</c> paragraph is found, and
/// <see cref="DocOpeningTagMissingMessage"/> when no Run text contains the
/// <c>elocatid=</c> attribute.
/// </para>
/// </summary>
public sealed class EmitElocationTagRule : IFormattingRule
{
    public const string ElocationParagraphMissingMessage = "elocation_id_missing";
    public const string DocOpeningTagMissingMessage = "doc_opening_tag_missing";

    private static readonly Regex StandaloneElocationPattern = new(
        @"^e\d{6,}$",
        RegexOptions.Compiled);

    private static readonly Regex ElocatidAttrPattern = new(
        @"elocatid=""[^""]*""",
        RegexOptions.Compiled);

    private static readonly Regex IssuenoAttrPattern = new(
        @"issueno=""[^""]*""",
        RegexOptions.Compiled);

    public string Name => nameof(EmitElocationTagRule);

    public RuleSeverity Severity => RuleSeverity.Optional;

    public void Apply(WordprocessingDocument doc, FormattingContext ctx, IReport report)
    {
        ArgumentNullException.ThrowIfNull(doc);
        ArgumentNullException.ThrowIfNull(ctx);
        ArgumentNullException.ThrowIfNull(report);

        var body = doc.MainDocumentPart?.Document?.Body;
        if (body is null)
        {
            report.Warn(Name, DocOpeningTagMissingMessage);
            return;
        }

        var (elocationParagraph, elocationId) = FindStandaloneElocation(body);
        if (elocationParagraph is null || elocationId is null)
        {
            report.Warn(Name, ElocationParagraphMissingMessage);
            return;
        }

        var docParagraph = FindDocOpeningParagraph(body);
        if (docParagraph is null)
        {
            report.Warn(Name, DocOpeningTagMissingMessage);
            return;
        }

        ctx.ElocationId ??= elocationId;

        var issueno = DeriveIssuenoFromElocationId(elocationId);
        if (!RewriteDocOpeningAttributes(docParagraph, elocationId, issueno))
        {
            report.Warn(Name, DocOpeningTagMissingMessage);
            return;
        }

        elocationParagraph.Remove();

        report.Info(Name, $"set [doc] elocatid=\"{elocationId}\"");
    }

    private static (Paragraph? Paragraph, string? Id) FindStandaloneElocation(Body body)
    {
        foreach (var paragraph in body.Elements<Paragraph>())
        {
            var text = ParagraphPlainText(paragraph).Trim();
            if (StandaloneElocationPattern.IsMatch(text))
            {
                return (paragraph, text);
            }
        }
        return (null, null);
    }

    private static Paragraph? FindDocOpeningParagraph(Body body)
    {
        foreach (var paragraph in body.Elements<Paragraph>())
        {
            if (ParagraphPlainText(paragraph).Contains("[doc ", StringComparison.Ordinal))
            {
                return paragraph;
            }
        }
        return null;
    }

    // Rewrites the [doc … elocatid="…" … issueno="…" …] opening tag attributes
    // in place. The original `.docx` typically splits the opening tag across
    // many Text nodes (Word inserts spell-check anchors), so a per-Text replace
    // misses substrings that straddle node boundaries. Concatenate every Text
    // in the paragraph, regex-replace on the joined string, then put the
    // entire corrected text back into the FIRST Text and clear the rest. This
    // collapses Word's spell-check fragmentation into a single Run; that loss
    // is inconsequential here (the [doc] paragraph is metadata the SciELO
    // production pipeline reads as plain text).
    private static bool RewriteDocOpeningAttributes(
        Paragraph paragraph,
        string newElocatid,
        string? newIssueno)
    {
        var texts = paragraph.Descendants<Text>().ToList();
        if (texts.Count == 0)
        {
            return false;
        }

        var sb = new StringBuilder();
        foreach (var t in texts)
        {
            sb.Append(t.Text);
        }

        var joined = sb.ToString();
        var rewritten = ElocatidAttrPattern.Replace(
            joined,
            $"elocatid=\"{newElocatid}\"",
            count: 1);

        if (ReferenceEquals(rewritten, joined) || rewritten == joined)
        {
            // No elocatid="…" attribute matched; do not touch the paragraph.
            return false;
        }

        if (newIssueno is not null)
        {
            rewritten = IssuenoAttrPattern.Replace(
                rewritten,
                $"issueno=\"{newIssueno}\"",
                count: 1);
        }

        texts[0].Text = rewritten;
        texts[0].Space = SpaceProcessingModeValues.Preserve;
        for (var i = 1; i < texts.Count; i++)
        {
            texts[i].Text = string.Empty;
        }

        return true;
    }

    // The corpus journal encodes elocation IDs as
    // e<article(4)><volid(2)><issueno(1)><order_no_pad>. Position 7 is the
    // issue digit. If a future journal uses a different layout, the rule
    // returns null and the [doc] issueno attribute stays untouched (safe).
    private static string? DeriveIssuenoFromElocationId(string id)
    {
        if (id.Length < 8 || id[0] != 'e')
        {
            return null;
        }

        var c = id[7];
        if (!char.IsDigit(c))
        {
            return null;
        }

        return c.ToString(CultureInfo.InvariantCulture);
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
                    sb.Append('\n');
                    break;
            }
        }
        return sb.ToString();
    }
}
