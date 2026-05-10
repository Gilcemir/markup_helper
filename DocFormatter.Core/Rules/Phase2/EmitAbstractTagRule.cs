using System.Text;
using DocFormatter.Core.Models.Phase2;
using DocFormatter.Core.Options;
using DocFormatter.Core.Pipeline;
using DocFormatter.Core.TagEmission;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace DocFormatter.Core.Rules.Phase2;

/// <summary>
/// Wraps the abstract section in a <c>[xmlabstr language="en"]…[/xmlabstr]</c>
/// pair. The opening literal is inserted before the first inline of the
/// heading paragraph (the paragraph whose normalized text starts with
/// "Abstract" / "Resumo" per <see cref="FormattingOptions.AbstractMarkers"/>);
/// the closing literal is inserted after the last inline of the immediately
/// following non-empty paragraph (the abstract body).
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

    private const string TagName = "xmlabstr";
    private const string Language = "en";

    private static readonly IReadOnlyList<(string Key, string Value)> OpeningAttrs =
        new[] { ("language", Language) };

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
            report.Warn(Name, AbstractHeadingNotFoundMessage);
            return;
        }

        var heading = FindHeadingParagraph(body);
        if (heading is null)
        {
            report.Warn(Name, AbstractHeadingNotFoundMessage);
            return;
        }

        var bodyParagraph = FindFollowingNonEmptyParagraph(heading);
        if (bodyParagraph is null)
        {
            report.Warn(Name, AbstractBodyNotFoundMessage);
            return;
        }

        TagEmitter.InsertOpeningBefore(heading, TagName, OpeningAttrs);
        TagEmitter.InsertClosingAfter(bodyParagraph, TagName);

        ctx.Abstract = new AbstractMarker(Language, heading, bodyParagraph);

        report.Info(Name, $"wrapped abstract in [{TagName} language=\"{Language}\"]");
    }

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
                    // we don't accidentally wrap unrelated content.
                    return null;
                }
            }
        }

        return null;
    }

    private static Paragraph? FindFollowingNonEmptyParagraph(Paragraph anchor)
    {
        var current = anchor.NextSibling<Paragraph>();
        while (current is not null)
        {
            if (!IsEmpty(current))
            {
                return current;
            }
            current = current.NextSibling<Paragraph>();
        }
        return null;
    }

    private static bool IsEmpty(Paragraph paragraph)
        => string.IsNullOrWhiteSpace(ParagraphPlainText(paragraph));

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
