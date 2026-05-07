using System.Text;
using DocFormatter.Core.Options;
using DocFormatter.Core.Pipeline;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace DocFormatter.Core.Rules;

public sealed class ExtractCorrespondingAuthorRule : IFormattingRule
{
    public const string NoMarkerMessage =
        "no corresponding author marker found";

    public const string SecondMarkerMessage =
        "second corresponding-author marker found in author paragraphs; ignoring (first wins)";

    public const string EmailExtractionFailedMessage =
        "corresponding-author marker found but email could not be extracted";

    public const string OrcidPromotedMessage =
        "ORCID promoted to corresponding author";

    private readonly FormattingOptions _options;

    public ExtractCorrespondingAuthorRule(FormattingOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options;
    }

    public string Name => nameof(ExtractCorrespondingAuthorRule);

    public RuleSeverity Severity => RuleSeverity.Optional;

    public void Apply(WordprocessingDocument doc, FormattingContext ctx, IReport report)
    {
        ArgumentNullException.ThrowIfNull(doc);
        ArgumentNullException.ThrowIfNull(ctx);
        ArgumentNullException.ThrowIfNull(report);

        _ = doc.MainDocumentPart?.Document?.Body
            ?? throw new InvalidOperationException("document is missing its body");

        if (ctx.AuthorParagraphs.Count == 0)
        {
            report.Info(Name, NoMarkerMessage);
            return;
        }

        if (!TryRunPassA(ctx, report))
        {
            report.Info(Name, NoMarkerMessage);
            return;
        }

        RunPassB(ctx, report);
        PromoteOrcidIfApplicable(ctx, report);
    }

    private bool TryRunPassA(FormattingContext ctx, IReport report)
    {
        var current = ctx.AuthorParagraphs[^1].NextSibling<Paragraph>();
        while (current is not null)
        {
            var text = GetParagraphPlainText(current);
            if (StartsWithAbstractMarker(text))
            {
                return false;
            }

            var match = _options.CorrespondingMarkerRegex.Match(text);
            if (!match.Success)
            {
                current = current.NextSibling<Paragraph>();
                continue;
            }

            ctx.CorrespondingAffiliationParagraph = current;
            var trailer = text.Substring(match.Index);
            StripTrailerStartingAt(current, match.Index);
            TrimTrailingWhitespace(current);

            var emailMatch = _options.EmailRegex.Match(trailer);
            if (emailMatch.Success)
            {
                ctx.CorrespondingEmail = emailMatch.Value;
            }
            else
            {
                report.Warn(Name, EmailExtractionFailedMessage);
            }

            var orcidMatch = _options.OrcidIdRegex.Match(trailer);
            if (orcidMatch.Success)
            {
                ctx.CorrespondingOrcid = orcidMatch.Value;
            }

            if (IsParagraphEffectivelyEmpty(current))
            {
                current.Remove();
                // Honor the FormattingContext invariant: a rule that removes a
                // paragraph it published into context MUST null the field so
                // downstream consumers do not dereference a detached node.
                ctx.CorrespondingAffiliationParagraph = null;
            }

            return true;
        }

        return false;
    }

    private void RunPassB(FormattingContext ctx, IReport report)
    {
        var authorIndex = 0;
        var matchCount = 0;
        var warnedSecondMarker = false;

        foreach (var paragraph in ctx.AuthorParagraphs)
        {
            foreach (var run in paragraph.Elements<Run>())
            {
                var text = GetRunText(run);
                if (text.Length == 0)
                {
                    continue;
                }

                if (IsSuperscript(run))
                {
                    if (text.Contains('*'))
                    {
                        OnStarFound(ctx, report, authorIndex, ref matchCount, ref warnedSecondMarker);
                    }

                    continue;
                }

                var i = 0;
                while (i < text.Length)
                {
                    var sepLen = MatchSeparatorAt(text, i);
                    if (sepLen > 0)
                    {
                        authorIndex++;
                        i += sepLen;
                        continue;
                    }

                    if (text[i] == '*')
                    {
                        OnStarFound(ctx, report, authorIndex, ref matchCount, ref warnedSecondMarker);
                    }

                    i++;
                }
            }
        }
    }

    private void OnStarFound(FormattingContext ctx, IReport report, int authorIndex, ref int matchCount, ref bool warnedSecondMarker)
    {
        matchCount++;
        if (matchCount == 1)
        {
            if (authorIndex >= 0 && authorIndex < ctx.Authors.Count)
            {
                ctx.CorrespondingAuthorIndex = authorIndex;
            }

            return;
        }

        if (warnedSecondMarker)
        {
            return;
        }

        report.Warn(Name, SecondMarkerMessage);
        warnedSecondMarker = true;
    }

    private void PromoteOrcidIfApplicable(FormattingContext ctx, IReport report)
    {
        if (ctx.CorrespondingAuthorIndex is not { } idx)
        {
            return;
        }

        if (ctx.CorrespondingOrcid is not { } orcid)
        {
            return;
        }

        if (idx < 0 || idx >= ctx.Authors.Count)
        {
            return;
        }

        var author = ctx.Authors[idx];
        if (author.OrcidId is not null)
        {
            return;
        }

        ctx.Authors[idx] = author with { OrcidId = orcid };
        report.Info(Name, OrcidPromotedMessage);
    }

    private bool StartsWithAbstractMarker(string text)
    {
        var trimmed = text.TrimStart();
        foreach (var marker in _options.AbstractMarkers)
        {
            if (trimmed.StartsWith(marker, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private int MatchSeparatorAt(string text, int position)
    {
        var bestLength = 0;
        foreach (var separator in _options.AuthorSeparators)
        {
            if (separator.Length == 0)
            {
                continue;
            }

            if (position + separator.Length > text.Length)
            {
                continue;
            }

            if (string.CompareOrdinal(text, position, separator, 0, separator.Length) == 0
                && separator.Length > bestLength)
            {
                bestLength = separator.Length;
            }
        }

        return bestLength;
    }

    private static string GetParagraphPlainText(Paragraph paragraph)
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

    private static string GetRunText(Run run)
        => string.Concat(run.Descendants<Text>().Select(t => t.Text));

    private static bool IsSuperscript(Run run)
    {
        var vert = run.RunProperties?.VerticalTextAlignment;
        return vert?.Val is { } val && val.Value == VerticalPositionValues.Superscript;
    }

    private static void StripTrailerStartingAt(Paragraph paragraph, int starOffset)
    {
        var currentOffset = 0;
        var children = paragraph.ChildElements.ToList();
        var foundBoundary = false;

        foreach (var child in children)
        {
            if (foundBoundary)
            {
                child.Remove();
                continue;
            }

            var len = MeasureChildLength(child);
            if (currentOffset + len <= starOffset)
            {
                currentOffset += len;
                continue;
            }

            foundBoundary = true;
            var innerOffset = starOffset - currentOffset;

            if (child is Run run)
            {
                TruncateRunAt(run, innerOffset);
            }
            else
            {
                child.Remove();
            }

            currentOffset += len;
        }
    }

    private static int MeasureChildLength(OpenXmlElement child)
    {
        switch (child)
        {
            case Run run:
                {
                    var len = 0;
                    foreach (var c in run.ChildElements)
                    {
                        if (c is Text t)
                        {
                            len += t.Text.Length;
                        }
                        else if (c is Break)
                        {
                            len += 1;
                        }
                    }

                    return len;
                }
            case Hyperlink hyperlink:
                {
                    var len = 0;
                    foreach (var t in hyperlink.Descendants<Text>())
                    {
                        len += t.Text.Length;
                    }

                    len += hyperlink.Descendants<Break>().Count();
                    return len;
                }
            default:
                return 0;
        }
    }

    private static void TruncateRunAt(Run run, int innerOffset)
    {
        var seen = 0;
        var boundaryHit = false;

        foreach (var child in run.ChildElements.ToList())
        {
            if (boundaryHit)
            {
                if (child is Text or Break)
                {
                    child.Remove();
                }

                continue;
            }

            switch (child)
            {
                case Text t:
                    {
                        var len = t.Text.Length;
                        if (seen + len <= innerOffset)
                        {
                            seen += len;
                            continue;
                        }

                        var localOffset = innerOffset - seen;
                        if (localOffset == 0)
                        {
                            t.Remove();
                        }
                        else
                        {
                            t.Text = t.Text.Substring(0, localOffset);
                        }

                        boundaryHit = true;
                        break;
                    }
                case Break:
                    {
                        if (seen + 1 <= innerOffset)
                        {
                            seen += 1;
                            continue;
                        }

                        child.Remove();
                        boundaryHit = true;
                        break;
                    }
            }
        }

        if (!run.Descendants<Text>().Any() && !run.Descendants<Break>().Any())
        {
            run.Remove();
        }
    }

    private static void TrimTrailingWhitespace(Paragraph paragraph)
    {
        while (true)
        {
            var lastText = paragraph.Descendants<Text>().LastOrDefault();
            if (lastText is null)
            {
                return;
            }

            var trimmed = lastText.Text.TrimEnd();
            if (trimmed.Length == lastText.Text.Length)
            {
                return;
            }

            if (trimmed.Length == 0)
            {
                var parent = lastText.Parent;
                lastText.Remove();
                if (parent is Run r
                    && !r.Descendants<Text>().Any()
                    && !r.Descendants<Break>().Any())
                {
                    r.Remove();
                }

                continue;
            }

            lastText.Text = trimmed;
            return;
        }
    }

    private static bool IsParagraphEffectivelyEmpty(Paragraph paragraph)
    {
        foreach (var t in paragraph.Descendants<Text>())
        {
            if (!string.IsNullOrWhiteSpace(t.Text))
            {
                return false;
            }
        }

        return true;
    }
}
