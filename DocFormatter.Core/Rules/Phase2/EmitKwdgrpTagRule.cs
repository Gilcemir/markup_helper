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
/// Wraps the keywords paragraph in a <c>[kwdgrp language="en"]…[/kwdgrp]</c>
/// pair, emits the leading section title in <c>[sectitle]Keywords:[/sectitle]</c>,
/// and wraps each parsed term in <c>[kwd]term[/kwd]</c>. The rule recognizes
/// the paragraph by a leading "Keywords:" / "Palavras-chave:" marker and
/// parses the trailing items on either <c>,</c> or <c>;</c>; the original
/// separator is preserved verbatim as plain text between the <c>[kwd]</c>
/// runs. Each tag literal is emitted as its own <see cref="Run"/> so the
/// Word Markup VBA <c>color(tag)</c> mapping paints per-tag colors.
/// </summary>
public sealed class EmitKwdgrpTagRule : IFormattingRule
{
    public const string KeywordsBlockNotFoundMessage = "keywords_block_not_found";

    private const string TagName = "kwdgrp";
    private const string SectitleTagName = "sectitle";
    private const string KwdTagName = "kwd";
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

        var parsed = ParseKeywordsParagraph(plainText);

        RewriteKeywordsParagraph(paragraph, parsed);

        ctx.Keywords = new KeywordsGroup(Language, paragraph, parsed.Keywords);

        report.Info(
            Name,
            $"wrapped keywords paragraph in [{TagName} language=\"{Language}\"] "
            + $"with [{SectitleTagName}] and {parsed.Keywords.Count} [{KwdTagName}] term(s)");
    }

    private static void RewriteKeywordsParagraph(Paragraph paragraph, ParsedKeywords parsed)
    {
        var pPr = paragraph.GetFirstChild<ParagraphProperties>()?.CloneNode(deep: true)
            as ParagraphProperties;

        paragraph.RemoveAllChildren();
        if (pPr is not null)
        {
            paragraph.AppendChild(pPr);
        }

        paragraph.AppendChild(TagEmitter.OpeningTag(TagName, OpeningAttrs));

        if (parsed.SectitleText.Length > 0)
        {
            paragraph.AppendChild(
                TagEmitter.OpeningTag(SectitleTagName, Array.Empty<(string, string)>()));
            paragraph.AppendChild(BuildPlainRun(parsed.SectitleText));
            paragraph.AppendChild(TagEmitter.ClosingTag(SectitleTagName));
        }

        if (parsed.SeparatorAfterSectitle.Length > 0)
        {
            paragraph.AppendChild(BuildPlainRun(parsed.SeparatorAfterSectitle));
        }

        for (var i = 0; i < parsed.Keywords.Count; i++)
        {
            paragraph.AppendChild(
                TagEmitter.OpeningTag(KwdTagName, Array.Empty<(string, string)>()));
            paragraph.AppendChild(BuildPlainRun(parsed.Keywords[i]));
            paragraph.AppendChild(TagEmitter.ClosingTag(KwdTagName));

            if (i < parsed.Separators.Count && parsed.Separators[i].Length > 0)
            {
                paragraph.AppendChild(BuildPlainRun(parsed.Separators[i]));
            }
        }

        if (parsed.Trailing.Length > 0)
        {
            paragraph.AppendChild(BuildPlainRun(parsed.Trailing));
        }

        paragraph.AppendChild(TagEmitter.ClosingTag(TagName));
    }

    private static Run BuildPlainRun(string text)
        => new(
            RewriteHeaderMvpRule.CreateBaseRunProperties(),
            new Text(text) { Space = SpaceProcessingModeValues.Preserve });

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

    private static ParsedKeywords ParseKeywordsParagraph(string paragraphText)
    {
        var match = KeywordsMarkerPattern.Match(paragraphText);
        if (!match.Success)
        {
            return new ParsedKeywords(
                SectitleText: paragraphText,
                SeparatorAfterSectitle: string.Empty,
                Keywords: Array.Empty<string>(),
                Separators: Array.Empty<string>(),
                Trailing: string.Empty);
        }

        // The matched span (e.g. "Keywords:" with any leading whitespace) is
        // the section title verbatim. Anything beyond it is either separator
        // whitespace or the comma/semicolon-separated kwd list.
        var sectitleText = paragraphText[..match.Length];
        var rest = paragraphText[match.Length..];

        // Capture the run of whitespace that follows the colon as the
        // separator between [sectitle] and the first [kwd]. The corpus shape
        // is exactly one space ("Keywords: K1"), but the parser is tolerant.
        var sepLen = 0;
        while (sepLen < rest.Length && char.IsWhiteSpace(rest[sepLen]))
        {
            sepLen++;
        }
        var separatorAfterSectitle = rest[..sepLen];
        var afterSeparator = rest[sepLen..];

        if (afterSeparator.Length == 0)
        {
            return new ParsedKeywords(
                SectitleText: sectitleText,
                SeparatorAfterSectitle: separatorAfterSectitle,
                Keywords: Array.Empty<string>(),
                Separators: Array.Empty<string>(),
                Trailing: string.Empty);
        }

        // Tokenize the trailing items while preserving the exact separator
        // text (`, `, `; `, `,`, etc.) between them. This keeps the produced
        // output faithful to the input — `[kwd]A[/kwd], [kwd]B[/kwd]` rather
        // than normalizing every separator to a single comma+space.
        var keywords = new List<string>();
        var separators = new List<string>();
        var i = 0;
        while (i < afterSeparator.Length)
        {
            var termStart = i;
            while (i < afterSeparator.Length && !IsKeywordSeparator(afterSeparator[i]))
            {
                i++;
            }
            var rawTerm = afterSeparator[termStart..i];
            var trimmedTerm = rawTerm.Trim();
            if (trimmedTerm.Length > 0)
            {
                keywords.Add(trimmedTerm);
            }

            if (i >= afterSeparator.Length)
            {
                break;
            }

            // Consume the separator and any trailing whitespace.
            var sepStart = i;
            i++; // the ',' or ';'
            while (i < afterSeparator.Length && char.IsWhiteSpace(afterSeparator[i]))
            {
                i++;
            }
            // Only emit a separator if we have another term coming, otherwise
            // it becomes the trailing piece.
            if (trimmedTerm.Length > 0)
            {
                separators.Add(afterSeparator[sepStart..i]);
            }
        }

        // If the last term ended with no separator but had untrimmed
        // whitespace, the trim above dropped it. The keywords list is the
        // trimmed truth; no extra trailing handling needed here. A trailing
        // separator (rare, e.g. "K1; K2;") leaks into `separators` and is
        // intentionally kept so the round-tripped paragraph preserves it.
        var trailingSeparator = string.Empty;
        if (separators.Count > keywords.Count)
        {
            trailingSeparator = separators[^1];
            separators.RemoveAt(separators.Count - 1);
        }

        return new ParsedKeywords(
            SectitleText: sectitleText,
            SeparatorAfterSectitle: separatorAfterSectitle,
            Keywords: keywords,
            Separators: separators,
            Trailing: trailingSeparator);
    }

    private static bool IsKeywordSeparator(char c)
    {
        foreach (var sep in KeywordSeparators)
        {
            if (c == sep)
            {
                return true;
            }
        }
        return false;
    }

    private sealed record ParsedKeywords(
        string SectitleText,
        string SeparatorAfterSectitle,
        IReadOnlyList<string> Keywords,
        IReadOnlyList<string> Separators,
        string Trailing);

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
