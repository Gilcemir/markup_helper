using System.Text;
using DocFormatter.Core.Options;
using DocFormatter.Core.Pipeline;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace DocFormatter.Core.Rules;

public sealed class RewriteAbstractRule : IFormattingRule
{
    public const string AbstractNotFoundMessage =
        "Abstract paragraph not found";

    public const string StructuralItalicRemovedMessage =
        "structural italic wrapper removed from abstract body";

    public const string ResumoNormalizedMessage =
        "Resumo normalized to Abstract";

    public const string MissingSeparatorMessage =
        "Abstract marker found but no separator after it; body preserved as-is";

    public const string CanonicalLineInsertedMessage =
        "Corresponding author line inserted";

    public const string RecoveredEmailMessage =
        "recovered email from pre-existing corresponding-author line";

    public const string ReplacedTypedLineMessagePrefix =
        "replaced pre-existing corresponding-author line: ";

    private const string CanonicalHeadingText = "Abstract";

    private readonly FormattingOptions _options;

    public RewriteAbstractRule(FormattingOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options;
    }

    public string Name => nameof(RewriteAbstractRule);

    public RuleSeverity Severity => RuleSeverity.Optional;

    public void Apply(WordprocessingDocument doc, FormattingContext ctx, IReport report)
    {
        ArgumentNullException.ThrowIfNull(doc);
        ArgumentNullException.ThrowIfNull(ctx);
        ArgumentNullException.ThrowIfNull(report);

        var body = doc.MainDocumentPart?.Document?.Body
            ?? throw new InvalidOperationException("document is missing its body");

        var match = FindAbstractParagraph(body);
        if (match is null)
        {
            report.Warn(Name, AbstractNotFoundMessage);
            return;
        }

        var (abstractParagraph, markerStart, markerLength, detectedMarker) = match.Value;
        var plainText = GetParagraphPlainText(abstractParagraph);

        var (bodyStartOffset, separatorFound) = ResolveBodyStart(plainText, markerStart, markerLength);

        if (!string.Equals(detectedMarker, CanonicalHeadingText, StringComparison.OrdinalIgnoreCase))
        {
            report.Info(Name, ResumoNormalizedMessage);
        }

        if (!separatorFound)
        {
            report.Warn(Name, MissingSeparatorMessage);
        }

        StripLeadingContentBefore(abstractParagraph, bodyStartOffset);

        if (BodyItalicIsStructuralWrapper(abstractParagraph))
        {
            StripItalicFromAllRuns(abstractParagraph);
            report.Info(Name, StructuralItalicRemovedMessage);
        }

        var headingParagraph = BuildHeadingParagraph();
        body.InsertBefore(headingParagraph, abstractParagraph);

        HandleCorrespondingAuthor(body, ctx, report, headingParagraph, abstractParagraph);
    }

    private (Paragraph paragraph, int markerStart, int markerLength, string detectedMarker)? FindAbstractParagraph(Body body)
    {
        foreach (var paragraph in body.Elements<Paragraph>())
        {
            foreach (var run in paragraph.Descendants<Run>())
            {
                var raw = run.InnerText;
                if (string.IsNullOrWhiteSpace(raw))
                {
                    continue;
                }

                var trimmed = raw.TrimStart();
                foreach (var marker in _options.AbstractMarkers)
                {
                    if (trimmed.StartsWith(marker, StringComparison.OrdinalIgnoreCase))
                    {
                        var plain = GetParagraphPlainText(paragraph);
                        var leadingWhitespace = plain.Length - plain.TrimStart().Length;
                        return (paragraph, leadingWhitespace, marker.Length, marker);
                    }
                }

                break;
            }
        }

        return null;
    }

    private static (int bodyStart, bool separatorFound) ResolveBodyStart(string plainText, int markerStart, int markerLength)
    {
        var afterMarker = markerStart + markerLength;
        var sepStart = afterMarker;
        while (sepStart < plainText.Length && char.IsWhiteSpace(plainText[sepStart]))
        {
            sepStart++;
        }

        if (sepStart < plainText.Length && IsSeparatorChar(plainText[sepStart]))
        {
            var sepEnd = sepStart + 1;
            while (sepEnd < plainText.Length && char.IsWhiteSpace(plainText[sepEnd]))
            {
                sepEnd++;
            }

            return (sepEnd, true);
        }

        return (afterMarker, false);
    }

    private static bool IsSeparatorChar(char c)
        => c is '-' or ':' or '—' or '–';

    private void HandleCorrespondingAuthor(
        Body body,
        FormattingContext ctx,
        IReport report,
        Paragraph headingParagraph,
        Paragraph abstractBody)
    {
        var typedLine = FindTypedCorrespondingAuthorLine(ctx, headingParagraph, abstractBody);

        var email = ctx.CorrespondingEmail;
        var emailRecovered = false;

        if (email is null && typedLine is not null)
        {
            var typedText = GetParagraphPlainText(typedLine);
            var emailMatch = _options.EmailRegex.Match(typedText);
            if (emailMatch.Success)
            {
                email = emailMatch.Value;
                ctx.CorrespondingEmail = email;
                emailRecovered = true;
            }
        }

        if (email is null)
        {
            return;
        }

        if (typedLine is not null)
        {
            var originalText = GetParagraphPlainText(typedLine).Trim();
            typedLine.Remove();

            var canonical = BuildCorrespondingAuthorParagraph(email);
            body.InsertBefore(canonical, headingParagraph);

            if (emailRecovered)
            {
                report.Info(Name, RecoveredEmailMessage);
            }
            else
            {
                report.Info(Name, ReplacedTypedLineMessagePrefix + "'" + originalText + "'");
            }
        }
        else
        {
            var canonical = BuildCorrespondingAuthorParagraph(email);
            body.InsertBefore(canonical, headingParagraph);
            report.Info(Name, CanonicalLineInsertedMessage);
        }
    }

    private Paragraph? FindTypedCorrespondingAuthorLine(
        FormattingContext ctx,
        Paragraph headingParagraph,
        Paragraph abstractBody)
    {
        Paragraph? current;
        if (ctx.AuthorParagraphs.Count > 0)
        {
            current = ctx.AuthorParagraphs[^1].NextSibling<Paragraph>();
        }
        else
        {
            current = headingParagraph.Parent?.Elements<Paragraph>().FirstOrDefault();
        }

        while (current is not null
               && !ReferenceEquals(current, headingParagraph)
               && !ReferenceEquals(current, abstractBody))
        {
            var text = GetParagraphPlainText(current);
            if (_options.CorrespondingAuthorLabelRegex.IsMatch(text))
            {
                return current;
            }

            current = current.NextSibling<Paragraph>();
        }

        return null;
    }

    private static Paragraph BuildHeadingParagraph()
    {
        var properties = RewriteHeaderMvpRule.CreateBaseRunProperties();
        properties.AppendChild(new Bold());
        return new Paragraph(
            new Run(
                properties,
                new Text(CanonicalHeadingText) { Space = SpaceProcessingModeValues.Preserve }));
    }

    private static Paragraph BuildCorrespondingAuthorParagraph(string email)
    {
        return new Paragraph(
            new Run(
                RewriteHeaderMvpRule.CreateBaseRunProperties(),
                new Text("Corresponding author: " + email) { Space = SpaceProcessingModeValues.Preserve }));
    }

    private static bool BodyItalicIsStructuralWrapper(Paragraph body)
    {
        var anyNonWhitespaceRun = false;
        foreach (var run in body.Descendants<Run>())
        {
            var text = string.Concat(run.Descendants<Text>().Select(t => t.Text));
            if (string.IsNullOrWhiteSpace(text))
            {
                continue;
            }

            anyNonWhitespaceRun = true;
            if (!IsRunItalic(run))
            {
                return false;
            }
        }

        return anyNonWhitespaceRun;
    }

    private static bool IsRunItalic(Run run)
    {
        var italic = run.RunProperties?.Italic;
        if (italic is null)
        {
            return false;
        }

        if (italic.Val is null)
        {
            return true;
        }

        return italic.Val.Value;
    }

    private static void StripItalicFromAllRuns(Paragraph body)
    {
        foreach (var run in body.Descendants<Run>().ToList())
        {
            run.RunProperties?.Italic?.Remove();
        }
    }

    private static void StripLeadingContentBefore(Paragraph paragraph, int bodyStartOffset)
    {
        if (bodyStartOffset <= 0)
        {
            return;
        }

        var currentOffset = 0;
        var children = paragraph.ChildElements.ToList();
        var foundBoundary = false;

        foreach (var child in children)
        {
            if (foundBoundary)
            {
                break;
            }

            if (child is ParagraphProperties)
            {
                continue;
            }

            var len = MeasureChildLength(child);
            if (len == 0)
            {
                continue;
            }

            if (currentOffset + len <= bodyStartOffset)
            {
                child.Remove();
                currentOffset += len;
                continue;
            }

            var innerOffset = bodyStartOffset - currentOffset;
            if (child is Run run)
            {
                TruncateRunFromStart(run, innerOffset);
            }
            else
            {
                child.Remove();
            }

            foundBoundary = true;
        }
    }

    private static void TruncateRunFromStart(Run run, int innerOffset)
    {
        if (innerOffset <= 0)
        {
            return;
        }

        var seen = 0;
        var boundaryHit = false;

        foreach (var child in run.ChildElements.ToList())
        {
            if (boundaryHit)
            {
                break;
            }

            switch (child)
            {
                case Text t:
                    {
                        var len = t.Text.Length;
                        if (seen + len <= innerOffset)
                        {
                            seen += len;
                            t.Remove();
                            continue;
                        }

                        var localOffset = innerOffset - seen;
                        if (localOffset > 0 && localOffset < len)
                        {
                            t.Text = t.Text.Substring(localOffset);
                            t.Space = SpaceProcessingModeValues.Preserve;
                        }

                        boundaryHit = true;
                        break;
                    }
                case Break:
                    {
                        if (seen + 1 <= innerOffset)
                        {
                            seen += 1;
                            child.Remove();
                            continue;
                        }

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
}
