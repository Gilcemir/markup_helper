using System.Text;
using System.Text.RegularExpressions;
using DocFormatter.Core.Models.Phase2;
using DocFormatter.Core.Pipeline;
using DocFormatter.Core.Rules.Phase2.HistDateParsing;
using DocFormatter.Core.TagEmission;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace DocFormatter.Core.Rules.Phase2;

/// <summary>
/// Emits the SciELO XML 4.0 history block as a span of three (or fewer)
/// adjacent paragraphs:
///
/// <code>
/// [hist]Received: [received dateiso="YYYYMMDD"]…[/received]
/// Accepted: [accepted dateiso="YYYYMMDD"]…[/accepted]
/// Published: [histdate dateiso="YYYYMMDD" datetype="pub"]…[/histdate][/hist]
/// </code>
///
/// <para>
/// The corpus shape (verified across all 10 AFTER pairs) keeps the original
/// three-paragraph layout produced by Phase 1's <c>MoveHistoryRule</c>; this
/// rule rewrites each paragraph's inline content in place, anchoring
/// <c>[hist]</c> to the first emitted paragraph and <c>[/hist]</c> to the
/// last. The DTD-required label words (<c>Received: </c>, <c>Accepted: </c>,
/// <c>Published: </c>) are preserved verbatim BEFORE each child opening tag.
/// </para>
///
/// <para>
/// Per ADR-007 the date phrase recognition is delegated to
/// <see cref="HistDateParser"/>; <see cref="HistDate.ToDateIso"/> owns the
/// <c>YYYYMMDD</c> zero-padding (<c>00</c> substituted for missing month or
/// day). Strict DTD ordering — <c>received</c> first, then <c>revised*</c>,
/// then <c>accepted?</c>, then <c>[histdate datetype="pub"]?</c> — is enforced
/// by this rule, not by the parser.
/// </para>
///
/// <para>
/// Per ADR-002 the rule skips the entire <c>[hist]</c> block when
/// <c>received</c> is missing or unparseable (DTD-required child).
/// Unparseable <c>accepted</c> or <c>published</c> paragraphs are passed
/// through untouched and a per-child warning is recorded; the <c>[hist]</c>
/// block still emits with the children that did parse.
/// </para>
/// </summary>
public sealed class EmitHistTagRule : IFormattingRule
{
    public const string HistReceivedMissingMessage = "hist_received_missing";
    public const string HistReceivedUnparseableMessage = "hist_received_unparseable";
    public const string HistAcceptedUnparseableMessage = "hist_accepted_unparseable";
    public const string HistPublishedUnparseableMessage = "hist_published_unparseable";
    public const string HistAlreadyTaggedMessage = "hist_already_tagged";

    private const string HistTagName = "hist";
    private const string ReceivedTagName = "received";
    private const string AcceptedTagName = "accepted";
    private const string HistdateTagName = "histdate";
    private const string PubDateType = "pub";

    private static readonly Regex ReceivedMarker = new(
        @"^\s*Received\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex AcceptedMarker = new(
        @"^\s*Accepted\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex PublishedMarker = new(
        @"^\s*Published\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public string Name => nameof(EmitHistTagRule);

    public RuleSeverity Severity => RuleSeverity.Optional;

    public void Apply(WordprocessingDocument doc, FormattingContext ctx, IReport report)
    {
        ArgumentNullException.ThrowIfNull(doc);
        ArgumentNullException.ThrowIfNull(ctx);
        ArgumentNullException.ThrowIfNull(report);

        var body = doc.MainDocumentPart?.Document?.Body;
        if (body is null)
        {
            report.Warn(Name, HistReceivedMissingMessage);
            return;
        }

        var candidates = CollectCandidates(body);
        if (candidates.AnyHistLiteral)
        {
            // Idempotency: a prior run already wrapped the block. Emit an info
            // entry so the diagnostic JSON can distinguish "already tagged"
            // from "nothing found" downstream.
            report.Info(Name, HistAlreadyTaggedMessage);
            return;
        }

        if (candidates.Received is null)
        {
            report.Warn(Name, HistReceivedMissingMessage);
            return;
        }

        var receivedDate = HistDateParser.ParseReceived(candidates.Received.Text);
        if (receivedDate is null)
        {
            report.Warn(Name, HistReceivedUnparseableMessage);
            return;
        }

        HistDate? acceptedDate = null;
        if (candidates.Accepted is not null)
        {
            acceptedDate = HistDateParser.ParseAccepted(candidates.Accepted.Text);
            if (acceptedDate is null)
            {
                report.Warn(Name, HistAcceptedUnparseableMessage);
            }
        }

        HistDate? publishedDate = null;
        if (candidates.Published is not null)
        {
            publishedDate = HistDateParser.ParsePublished(candidates.Published.Text);
            if (publishedDate is null)
            {
                report.Warn(Name, HistPublishedUnparseableMessage);
            }
        }

        // Strict DTD order: received → revised* → accepted? → histdate(pub)?.
        // Revised is reserved on HistoryDates for future articles; no corpus
        // pair currently exercises it.
        var entries = new List<EmittedEntry>(3)
        {
            new(candidates.Received!, candidates.Received!.Text, receivedDate, ReceivedTagName, new[] { ("dateiso", receivedDate.ToDateIso()) }),
        };
        if (acceptedDate is not null && candidates.Accepted is not null)
        {
            entries.Add(new EmittedEntry(
                candidates.Accepted,
                candidates.Accepted.Text,
                acceptedDate,
                AcceptedTagName,
                new[] { ("dateiso", acceptedDate.ToDateIso()) }));
        }
        if (publishedDate is not null && candidates.Published is not null)
        {
            entries.Add(new EmittedEntry(
                candidates.Published,
                candidates.Published.Text,
                publishedDate,
                HistdateTagName,
                new[] { ("dateiso", publishedDate.ToDateIso()), ("datetype", PubDateType) }));
        }

        for (var i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            RewriteHistoryParagraph(
                entry.Candidate.Paragraph,
                entry.Text,
                entry.Date,
                entry.TagName,
                entry.Attrs,
                prependHistOpening: i == 0,
                appendHistClosing: i == entries.Count - 1);
        }

        ctx.History = new HistoryDates(
            Received: receivedDate,
            Revised: Array.Empty<HistDate>(),
            Accepted: acceptedDate,
            Published: publishedDate);

        report.Info(Name, $"emitted [hist] block with {entries.Count} dated child element(s)");
    }

    private static Candidates CollectCandidates(Body body)
    {
        ParagraphCandidate? received = null;
        ParagraphCandidate? accepted = null;
        ParagraphCandidate? published = null;
        var anyHistLiteral = false;

        foreach (var paragraph in body.Elements<Paragraph>())
        {
            var text = ParagraphPlainText(paragraph);
            if (text.Contains("[hist", StringComparison.Ordinal)
                || text.Contains("[received", StringComparison.Ordinal)
                || text.Contains("[accepted", StringComparison.Ordinal)
                || text.Contains("[histdate", StringComparison.Ordinal))
            {
                anyHistLiteral = true;
                continue;
            }

            if (received is null && ReceivedMarker.IsMatch(text))
            {
                received = new ParagraphCandidate(paragraph, text);
                continue;
            }
            if (accepted is null && AcceptedMarker.IsMatch(text))
            {
                accepted = new ParagraphCandidate(paragraph, text);
                continue;
            }
            if (published is null && PublishedMarker.IsMatch(text))
            {
                published = new ParagraphCandidate(paragraph, text);
            }
        }

        return new Candidates(received, accepted, published, anyHistLiteral);
    }

    private static void RewriteHistoryParagraph(
        Paragraph paragraph,
        string originalText,
        HistDate date,
        string tagName,
        IReadOnlyList<(string Key, string Value)> tagAttrs,
        bool prependHistOpening,
        bool appendHistClosing)
    {
        var (prefix, suffix) = SplitAroundSource(originalText, date.SourceText);
        var pPr = paragraph.GetFirstChild<ParagraphProperties>()?.CloneNode(deep: true) as ParagraphProperties;

        paragraph.RemoveAllChildren();
        if (pPr is not null)
        {
            paragraph.AppendChild(pPr);
        }

        if (prependHistOpening)
        {
            paragraph.AppendChild(
                TagEmitter.OpeningTag(HistTagName, Array.Empty<(string, string)>()));
        }

        if (prefix.Length > 0)
        {
            paragraph.AppendChild(BuildPlainRun(prefix));
        }

        paragraph.AppendChild(TagEmitter.OpeningTag(tagName, tagAttrs));
        paragraph.AppendChild(BuildPlainRun(date.SourceText));
        paragraph.AppendChild(TagEmitter.ClosingTag(tagName));

        if (suffix.Length > 0)
        {
            paragraph.AppendChild(BuildPlainRun(suffix));
        }

        if (appendHistClosing)
        {
            paragraph.AppendChild(TagEmitter.ClosingTag(HistTagName));
        }
    }

    private static (string Prefix, string Suffix) SplitAroundSource(string text, string source)
    {
        // SourceText is the trimmed, header-stripped phrase that
        // HistDateParser preserved verbatim; it is a substring of the
        // original paragraph text in every corpus pair. If it is not present
        // (defensive: future parser changes), fall back to placing it at the
        // end of the text so we still produce a well-formed [hist] block.
        var index = text.IndexOf(source, StringComparison.Ordinal);
        if (index < 0)
        {
            return (text, string.Empty);
        }
        return (text[..index], text[(index + source.Length)..]);
    }

    private static Run BuildPlainRun(string text)
        => new(
            RewriteHeaderMvpRule.CreateBaseRunProperties(),
            new Text(text) { Space = SpaceProcessingModeValues.Preserve });

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

    private sealed record ParagraphCandidate(Paragraph Paragraph, string Text);

    private sealed record Candidates(
        ParagraphCandidate? Received,
        ParagraphCandidate? Accepted,
        ParagraphCandidate? Published,
        bool AnyHistLiteral);

    private sealed record EmittedEntry(
        ParagraphCandidate Candidate,
        string Text,
        HistDate Date,
        string TagName,
        IReadOnlyList<(string Key, string Value)> Attrs);
}
