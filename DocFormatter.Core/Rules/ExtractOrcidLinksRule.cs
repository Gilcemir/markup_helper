using DocFormatter.Core.Options;
using DocFormatter.Core.Pipeline;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Blip = DocumentFormat.OpenXml.Drawing.Blip;

namespace DocFormatter.Core.Rules;

public sealed class ExtractOrcidLinksRule : IFormattingRule
{
    public const string MissingAuthorsParagraphMessage =
        "authors paragraph not found; skipping ORCID extraction";

    private readonly FormattingOptions _options;

    public ExtractOrcidLinksRule(FormattingOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options;
    }

    public string Name => nameof(ExtractOrcidLinksRule);

    public RuleSeverity Severity => RuleSeverity.Optional;

    public void Apply(WordprocessingDocument doc, FormattingContext ctx, IReport report)
    {
        ArgumentNullException.ThrowIfNull(doc);
        ArgumentNullException.ThrowIfNull(ctx);
        ArgumentNullException.ThrowIfNull(report);

        var mainPart = doc.MainDocumentPart
            ?? throw new InvalidOperationException("document is missing its MainDocumentPart");
        var body = mainPart.Document?.Body
            ?? throw new InvalidOperationException("document is missing its body");

        var authors = HeaderParagraphLocator.FindAuthorsParagraph(body);
        if (authors is null)
        {
            report.Warn(Name, MissingAuthorsParagraphMessage);
            return;
        }

        var hyperlinks = authors.Elements<Hyperlink>().ToList();
        var relationshipsToDelete = new HashSet<string>(StringComparer.Ordinal);

        foreach (var hyperlink in hyperlinks)
        {
            var rId = hyperlink.Id?.Value;
            if (string.IsNullOrEmpty(rId))
            {
                continue;
            }

            var relationship = mainPart.HyperlinkRelationships.FirstOrDefault(r => r.Id == rId);
            if (relationship is null)
            {
                continue;
            }

            var url = relationship.Uri?.ToString() ?? string.Empty;
            if (!url.Contains(_options.OrcidUrlMarker, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var match = _options.OrcidIdRegex.Match(url);
            if (!match.Success)
            {
                report.Warn(
                    Name,
                    $"hyperlink target contains '{_options.OrcidUrlMarker}' but no ORCID ID was found: '{url}'");
                continue;
            }

            var orcidId = match.Value;
            var runIndex = CountRunChildrenBefore(authors, hyperlink);
            var runProperties = FindFirstTextRunProperties(hyperlink);

            var replacement = BuildPlainRun(orcidId, runProperties);
            authors.ReplaceChild(replacement, hyperlink);

            ctx.OrcidStaging[runIndex] = orcidId;
            relationshipsToDelete.Add(rId);
            report.Info(Name, $"extracted ORCID '{orcidId}' at run index {runIndex}");
        }

        WarnOnFreeStandingOrcidBadges(authors, mainPart, report);

        foreach (var rId in relationshipsToDelete)
        {
            if (IsRelationshipStillReferenced(mainPart, rId))
            {
                continue;
            }

            mainPart.DeleteReferenceRelationship(rId);
        }
    }

    private static int CountRunChildrenBefore(Paragraph paragraph, OpenXmlElement target)
    {
        var count = 0;
        foreach (var child in paragraph.ChildElements)
        {
            if (ReferenceEquals(child, target))
            {
                break;
            }

            if (child is Run)
            {
                count++;
            }
        }

        return count;
    }

    private static RunProperties? FindFirstTextRunProperties(Hyperlink hyperlink)
    {
        foreach (var run in hyperlink.Descendants<Run>())
        {
            if (!run.Descendants<Text>().Any())
            {
                continue;
            }

            return run.RunProperties?.CloneNode(true) as RunProperties;
        }

        return null;
    }

    private static Run BuildPlainRun(string text, RunProperties? runProperties)
    {
        var run = new Run();
        if (runProperties is not null)
        {
            run.AppendChild(runProperties);
        }

        run.AppendChild(new Text(text) { Space = SpaceProcessingModeValues.Preserve });
        return run;
    }

    private void WarnOnFreeStandingOrcidBadges(Paragraph paragraph, MainDocumentPart mainPart, IReport report)
    {
        foreach (var drawing in paragraph.Descendants<Drawing>())
        {
            if (drawing.Ancestors<Hyperlink>().Any())
            {
                continue;
            }

            var blip = drawing.Descendants<Blip>().FirstOrDefault();
            var embedId = blip?.Embed?.Value;
            if (string.IsNullOrEmpty(embedId))
            {
                continue;
            }

            var target = ResolveRelationshipTarget(mainPart, embedId);
            if (target is null)
            {
                continue;
            }

            if (target.Contains(_options.OrcidUrlMarker, StringComparison.OrdinalIgnoreCase))
            {
                report.Warn(
                    Name,
                    $"free-standing ORCID badge image left in place (target='{target}'); manual cleanup required");
            }
        }
    }

    private static string? ResolveRelationshipTarget(MainDocumentPart mainPart, string rId)
    {
        var external = mainPart.ExternalRelationships.FirstOrDefault(r => r.Id == rId);
        if (external is not null)
        {
            return external.Uri.ToString();
        }

        var hyper = mainPart.HyperlinkRelationships.FirstOrDefault(r => r.Id == rId);
        if (hyper is not null)
        {
            return hyper.Uri?.ToString();
        }

        var pair = mainPart.Parts.FirstOrDefault(p => p.RelationshipId == rId);
        return pair.OpenXmlPart?.Uri.ToString();
    }

    private static bool IsRelationshipStillReferenced(MainDocumentPart mainPart, string rId)
    {
        var document = mainPart.Document;
        if (document is null)
        {
            return false;
        }

        foreach (var element in document.Descendants())
        {
            foreach (var attribute in element.GetAttributes())
            {
                if (string.Equals(attribute.Value, rId, StringComparison.Ordinal))
                {
                    return true;
                }
            }
        }

        return false;
    }
}
