using System.Text;
using System.Text.RegularExpressions;
using DocFormatter.Core.Models.Phase2;
using DocFormatter.Core.Options;
using DocFormatter.Core.Pipeline;
using DocFormatter.Core.TagEmission;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace DocFormatter.Core.Rules.Phase2;

/// <summary>
/// Wraps the abstract section in a <c>[xmlabstr language="en"]…[/xmlabstr]</c>
/// pair, emits the heading as <c>[sectitle]…[/sectitle]</c>, and wraps each
/// body paragraph in <c>[p]…[/p]</c>. The heading paragraph is the one whose
/// normalized text starts with "Abstract" / "Resumo" per
/// <see cref="FormattingOptions.AbstractMarkers"/>. The body span runs from
/// the first non-empty paragraph after the heading up to (but excluding) the
/// next "Keywords:" / "Palavras-chave:" paragraph or the end of the body.
/// Each emitted tag literal lives in its own <see cref="Run"/> so the Word
/// Markup VBA <c>color(tag)</c> mapping paints per-tag colors.
///
/// <para>
/// The corpus tag name is <c>xmlabstr</c> (NOT <c>abstract</c>) — see
/// <c>docs/scielo_context</c> and the task 05 corpus probe in
/// <c>memory/MEMORY.md</c>. <c>language="en"</c> is hard-coded for the initial
/// rollout per PRD Non-Goals.
/// </para>
///
/// <para>
/// Per ADR-002 the rule skips and warns rather than aborting. Reason codes:
/// <see cref="AbstractHeadingNotFoundMessage"/> when no heading paragraph
/// matches, and <see cref="AbstractBodyNotFoundMessage"/> when the heading is
/// found but no following non-empty paragraph exists to wrap.
/// </para>
/// </summary>
public sealed class EmitAbstractTagRule : IFormattingRule
{
    public const string AbstractHeadingNotFoundMessage = "abstract_heading_not_found";
    public const string AbstractBodyNotFoundMessage = "abstract_body_not_found";
    public const string DocumentBodyMissingMessage = "document_body_missing";

    private const string TagName = "xmlabstr";
    private const string SectitleTagName = "sectitle";
    private const string ParagraphTagName = "p";
    private const string Language = "en";

    private static readonly IReadOnlyList<(string Key, string Value)> OpeningAttrs =
        new[] { ("language", Language) };

    // Same shape as EmitKwdgrpTagRule.KeywordsMarkerPattern. Used here to
    // stop the abstract body span before the keywords paragraph.
    private static readonly Regex KeywordsBoundaryPattern = new(
        @"^\s*(keywords|key\s*words|palavras[-\s]chave)\s*:",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private readonly FormattingOptions _options;

    public EmitAbstractTagRule(FormattingOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options;
    }

    public string Name => nameof(EmitAbstractTagRule);

    public RuleSeverity Severity => RuleSeverity.Optional;

    public void Apply(WordprocessingDocument doc, FormattingContext ctx, IReport report)
    {
        ArgumentNullException.ThrowIfNull(doc);
        ArgumentNullException.ThrowIfNull(ctx);
        ArgumentNullException.ThrowIfNull(report);

        var body = doc.MainDocumentPart?.Document?.Body;
        if (body is null)
        {
            report.Warn(Name, DocumentBodyMissingMessage);
            return;
        }

        var heading = FindHeadingParagraph(body);
        if (heading is null)
        {
            report.Warn(Name, AbstractHeadingNotFoundMessage);
            return;
        }

        var bodyParagraphs = CollectBodyParagraphs(heading);
        if (bodyParagraphs.Count == 0)
        {
            report.Warn(Name, AbstractBodyNotFoundMessage);
            return;
        }

        RewriteHeadingAsSectitle(heading);
        WrapBodyParagraphsInP(bodyParagraphs);

        // Wrap the whole span: opening on the heading (before the [sectitle]
        // we just emitted), closing on the last body paragraph (after the
        // closing [/p] we just emitted).
        TagEmitter.InsertOpeningBefore(heading, TagName, OpeningAttrs);
        TagEmitter.InsertClosingAfter(bodyParagraphs[^1], TagName);

        ctx.Abstract = new AbstractMarker(Language, heading, bodyParagraphs[^1]);

        report.Info(
            Name,
            $"wrapped abstract in [{TagName} language=\"{Language}\"] "
            + $"with [{SectitleTagName}] and {bodyParagraphs.Count} [{ParagraphTagName}] paragraph(s)");
    }

    private static void RewriteHeadingAsSectitle(Paragraph heading)
    {
        var headingText = ParagraphPlainText(heading);
        var pPr = heading.GetFirstChild<ParagraphProperties>()?.CloneNode(deep: true)
            as ParagraphProperties;

        heading.RemoveAllChildren();
        if (pPr is not null)
        {
            heading.AppendChild(pPr);
        }

        heading.AppendChild(
            TagEmitter.OpeningTag(SectitleTagName, Array.Empty<(string, string)>()));
        if (headingText.Length > 0)
        {
            heading.AppendChild(BuildPlainRun(headingText));
        }
        heading.AppendChild(TagEmitter.ClosingTag(SectitleTagName));
    }

    private static void WrapBodyParagraphsInP(IReadOnlyList<Paragraph> bodyParagraphs)
    {
        foreach (var p in bodyParagraphs)
        {
            // Insert [p]…[/p] around the existing inline content of each body
            // paragraph. Preserves the original runs (and their formatting)
            // and only adds two new Runs at the boundaries — keeps the rest
            // of the document's structure intact.
            TagEmitter.InsertOpeningBefore(p, ParagraphTagName, Array.Empty<(string, string)>());
            TagEmitter.InsertClosingAfter(p, ParagraphTagName);
        }
    }

    private static List<Paragraph> CollectBodyParagraphs(Paragraph heading)
    {
        var result = new List<Paragraph>();
        var current = heading.NextSibling<Paragraph>();
        while (current is not null)
        {
            var text = ParagraphPlainText(current);
            if (KeywordsBoundaryPattern.IsMatch(text))
            {
                break;
            }
            if (!string.IsNullOrWhiteSpace(text))
            {
                result.Add(current);
            }
            current = current.NextSibling<Paragraph>();
        }
        return result;
    }

    private static Run BuildPlainRun(string text)
        => new(
            RewriteHeaderMvpRule.CreateBaseRunProperties(),
            new Text(text) { Space = SpaceProcessingModeValues.Preserve });

    private Paragraph? FindHeadingParagraph(Body body)
    {
        foreach (var paragraph in body.Elements<Paragraph>())
        {
            var text = ParagraphPlainText(paragraph).TrimStart();
            foreach (var marker in _options.AbstractMarkers)
            {
                if (text.StartsWith(marker, StringComparison.OrdinalIgnoreCase))
                {
                    var afterMarker = text.AsSpan(marker.Length).TrimStart();
                    if (afterMarker.IsEmpty)
                    {
                        // Heading paragraph carries ONLY the marker text — the
                        // body lives in the next paragraph. This is the shape
                        // the corpus has after Phase 1's RewriteAbstractRule
                        // splits the abstract into a heading + body.
                        return paragraph;
                    }

                    // Reject paragraphs where the marker is just a prefix of a
                    // longer sentence ("Abstract submission deadline ..."), so
                    // we don't accidentally wrap unrelated content — but keep
                    // scanning later paragraphs in case the real heading lives
                    // further down.
                    break;
                }
            }
        }

        return null;
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
