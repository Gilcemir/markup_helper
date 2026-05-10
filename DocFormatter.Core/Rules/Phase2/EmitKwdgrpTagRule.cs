using System.Text;
using System.Text.RegularExpressions;
using DocFormatter.Core.Models.Phase2;
using DocFormatter.Core.Pipeline;
using DocFormatter.Core.TagEmission;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace DocFormatter.Core.Rules.Phase2;

/// <summary>
/// Wraps the keywords paragraph in a <c>[kwdgrp language="en"]…[/kwdgrp]</c>
/// pair. The rule recognizes the paragraph by a leading "Keywords:" /
/// "Palavras-chave:" marker and parses the trailing items on either
/// <c>,</c> or <c>;</c>. <em>Individual <c>[kwd]</c> wrappers are NOT
/// emitted</em>: SciELO Markup auto-marks them per
/// <c>docs/scielo_context/REENTRANCE.md</c>, so pre-marking would produce
/// duplicate tags downstream (anti-duplication invariant from ADR-001).
/// </summary>
public sealed class EmitKwdgrpTagRule : IFormattingRule
{
    public const string KeywordsBlockNotFoundMessage = "keywords_block_not_found";

    private const string TagName = "kwdgrp";
    private const string Language = "en";

    private static readonly IReadOnlyList<(string Key, string Value)> OpeningAttrs =
        new[] { ("language", Language) };

    // Match the leading "Keywords:" / "Palavras-chave:" marker (any locale of
    // either label, optional whitespace before the colon). The colon is
    // mandatory — a paragraph that starts with the word "Keywords" but lacks
    // a colon is more likely a section heading than the kwdgrp paragraph.
    private static readonly Regex KeywordsMarkerPattern = new(
        @"^\s*(keywords|key\s*words|palavras[-\s]chave)\s*:",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly char[] KeywordSeparators = new[] { ',', ';' };

    public string Name => nameof(EmitKwdgrpTagRule);

    public RuleSeverity Severity => RuleSeverity.Optional;

    public void Apply(WordprocessingDocument doc, FormattingContext ctx, IReport report)
    {
        ArgumentNullException.ThrowIfNull(doc);
        ArgumentNullException.ThrowIfNull(ctx);
        ArgumentNullException.ThrowIfNull(report);

        var body = doc.MainDocumentPart?.Document?.Body;
        if (body is null)
        {
            report.Warn(Name, KeywordsBlockNotFoundMessage);
            return;
        }

        var (paragraph, plainText) = FindKeywordsParagraph(body);
        if (paragraph is null || plainText is null)
        {
            report.Warn(Name, KeywordsBlockNotFoundMessage);
            return;
        }

        var keywords = ParseKeywords(plainText);

        TagEmitter.InsertOpeningBefore(paragraph, TagName, OpeningAttrs);
        TagEmitter.InsertClosingAfter(paragraph, TagName);

        ctx.Keywords = new KeywordsGroup(Language, paragraph, keywords);

        report.Info(
            Name,
            $"wrapped keywords paragraph in [{TagName} language=\"{Language}\"] ({keywords.Count} term(s))");
    }

    private static (Paragraph? Paragraph, string? PlainText) FindKeywordsParagraph(Body body)
    {
        foreach (var paragraph in body.Elements<Paragraph>())
        {
            var text = ParagraphPlainText(paragraph);
            if (KeywordsMarkerPattern.IsMatch(text))
            {
                return (paragraph, text);
            }
        }
        return (null, null);
    }

    private static IReadOnlyList<string> ParseKeywords(string paragraphText)
    {
        var match = KeywordsMarkerPattern.Match(paragraphText);
        if (!match.Success)
        {
            return Array.Empty<string>();
        }

        var afterMarker = paragraphText[match.Length..].Trim();
        if (afterMarker.Length == 0)
        {
            return Array.Empty<string>();
        }

        return afterMarker
            .Split(KeywordSeparators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(static k => k.Length > 0)
            .ToArray();
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
