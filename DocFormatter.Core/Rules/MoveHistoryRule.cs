using System.Text.RegularExpressions;
using DocFormatter.Core.Pipeline;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace DocFormatter.Core.Rules;

public sealed class MoveHistoryRule : IFormattingRule
{
    public const string AnchorMissingMessage =
        "INTRODUCTION anchor not found — history move skipped";

    public const string AlreadyAdjacentMessage =
        "history already adjacent to INTRODUCTION — no-op";

    public const string MovedMessagePrefix =
        "history moved (3 paragraphs placed before INTRODUCTION at position ";

    public const string MovedMessageOriginInfix = " from index ";

    public const string PartialBlockMessagePrefix = "history partial: ";

    public const string OutOfOrderMessagePrefix = "history out of order ";

    public const string NotAdjacentMessagePrefix = "history not adjacent ";

    public const string NotFoundMessage = "history block not found — nothing to move";

    private static readonly Regex HistoryMarkerRegex = new(
        @"^(received|accepted|published)\s*[:\-–—]\s*.+",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public string Name => nameof(MoveHistoryRule);

    public RuleSeverity Severity => RuleSeverity.Optional;

    public void Apply(WordprocessingDocument doc, FormattingContext ctx, IReport report)
    {
        ArgumentNullException.ThrowIfNull(doc);
        ArgumentNullException.ThrowIfNull(ctx);
        ArgumentNullException.ThrowIfNull(report);

        var mainPart = doc.MainDocumentPart;
        var body = mainPart?.Document?.Body;
        if (body is null)
        {
            return;
        }

        var anchor = BodySectionDetector.FindIntroductionAnchor(body, mainPart);
        if (anchor is null)
        {
            report.Warn(Name, AnchorMissingMessage);
            return;
        }

        var paragraphs = body.Elements<Paragraph>().ToList();
        var anchorIndex = paragraphs.IndexOf(anchor);

        Paragraph? received = null;
        Paragraph? accepted = null;
        Paragraph? published = null;
        var receivedIndex = -1;
        var acceptedIndex = -1;
        var publishedIndex = -1;

        for (var i = 0; i < anchorIndex; i++)
        {
            var current = paragraphs[i];
            var trimmed = (current.InnerText ?? string.Empty).Trim();
            if (trimmed.Length == 0)
            {
                continue;
            }

            var match = HistoryMarkerRegex.Match(trimmed);
            if (!match.Success)
            {
                continue;
            }

            var kind = match.Groups[1].Value.ToLowerInvariant();
            switch (kind)
            {
                case "received":
                    if (received is null)
                    {
                        received = current;
                        receivedIndex = i;
                    }
                    break;
                case "accepted":
                    if (accepted is null)
                    {
                        accepted = current;
                        acceptedIndex = i;
                    }
                    break;
                case "published":
                    if (published is null)
                    {
                        published = current;
                        publishedIndex = i;
                    }
                    break;
            }
        }

        if (received is null)
        {
            report.Info(Name, NotFoundMessage);
            return;
        }

        if (accepted is null || published is null)
        {
            var aFlag = accepted is not null ? 1 : 0;
            var pFlag = published is not null ? 1 : 0;
            report.Warn(
                Name,
                $"{PartialBlockMessagePrefix}Received=1 Accepted={aFlag} Published={pFlag} — not moved");
            return;
        }

        if (!(receivedIndex < acceptedIndex && acceptedIndex < publishedIndex))
        {
            var orderText = DescribeMarkerOrder(receivedIndex, acceptedIndex, publishedIndex);
            report.Warn(Name, $"{OutOfOrderMessagePrefix}({orderText}) — not moved");
            return;
        }

        var gap = CountNonEmptyBetween(paragraphs, receivedIndex, acceptedIndex)
                  + CountNonEmptyBetween(paragraphs, acceptedIndex, publishedIndex);
        if (gap > 0)
        {
            report.Warn(
                Name,
                $"{NotAdjacentMessagePrefix}(gap of {gap} non-empty paragraphs between markers) — not moved");
            return;
        }

        if (publishedIndex == anchorIndex - 1
            && acceptedIndex == publishedIndex - 1
            && receivedIndex == acceptedIndex - 1)
        {
            report.Info(Name, AlreadyAdjacentMessage);
            return;
        }

        received.Remove();
        accepted.Remove();
        published.Remove();
        body.InsertBefore(received, anchor);
        body.InsertBefore(accepted, anchor);
        body.InsertBefore(published, anchor);

        var finalAnchorIndex = body.Elements<Paragraph>().ToList().IndexOf(anchor);
        report.Info(
            Name,
            $"{MovedMessagePrefix}{finalAnchorIndex}{MovedMessageOriginInfix}{receivedIndex})");
    }

    private static string DescribeMarkerOrder(int receivedIndex, int acceptedIndex, int publishedIndex)
    {
        var ordered = new[]
            {
                (Index: receivedIndex, Label: "Received"),
                (Index: acceptedIndex, Label: "Accepted"),
                (Index: publishedIndex, Label: "Published"),
            }
            .OrderBy(t => t.Index)
            .Select(t => t.Label);
        return string.Join("→", ordered);
    }

    private static int CountNonEmptyBetween(List<Paragraph> paragraphs, int leftIndex, int rightIndex)
    {
        if (rightIndex <= leftIndex + 1)
        {
            return 0;
        }

        var count = 0;
        for (var i = leftIndex + 1; i < rightIndex; i++)
        {
            var trimmed = (paragraphs[i].InnerText ?? string.Empty).Trim();
            if (trimmed.Length > 0)
            {
                count++;
            }
        }

        return count;
    }
}
