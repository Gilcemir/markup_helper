using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using DocFormatter.Core.Models;
using DocFormatter.Core.Options;
using DocFormatter.Core.Pipeline;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Blip = DocumentFormat.OpenXml.Drawing.Blip;

namespace DocFormatter.Core.Rules;

public sealed partial class ExtractAuthorsRule : IFormattingRule
{
    public const string MissingAuthorsParagraphMessage = "authors paragraph not found";

    private readonly FormattingOptions _options;

    public ExtractAuthorsRule(FormattingOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options;
    }

    public string Name => nameof(ExtractAuthorsRule);

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

        var paragraphs = HeaderParagraphLocator.FindAuthorsParagraphs(body, _options.AbstractMarkers);
        if (paragraphs.Count == 0)
        {
            report.Warn(Name, MissingAuthorsParagraphMessage);
            return;
        }

        // Share the detected paragraphs with downstream rules. The body is
        // about to be mutated (hyperlinks removed) which would change what the
        // locator returns on a second call.
        ctx.AuthorParagraphs.AddRange(paragraphs);

        var builders = new List<AuthorBuilder> { new() };
        var hyperlinksToRemove = new List<(Paragraph Paragraph, Hyperlink Hyperlink)>();
        var relationshipsToDelete = new HashSet<string>(StringComparer.Ordinal);

        for (var i = 0; i < paragraphs.Count; i++)
        {
            var paragraph = paragraphs[i];

            // Cross-paragraph boundary: if the previous paragraph already
            // produced a non-empty author, treat the line break as an implicit
            // separator and start a fresh builder for the next author.
            if (i > 0 && !IsBuilderEmpty(builders[^1]))
            {
                builders.Add(new AuthorBuilder());
            }

            var tokens = TokenizeParagraph(paragraph, mainPart, hyperlinksToRemove, relationshipsToDelete, report);
            ConsumeTokens(tokens, builders);
        }

        FlagSuspicions(builders);
        EmitAuthors(builders, ctx, report);

        foreach (var paragraph in paragraphs)
        {
            WarnOnFreeStandingOrcidBadges(paragraph, mainPart, report);
        }

        foreach (var (paragraph, hyperlink) in hyperlinksToRemove)
        {
            paragraph.RemoveChild(hyperlink);
        }

        foreach (var rId in relationshipsToDelete)
        {
            if (IsRelationshipStillReferenced(mainPart, rId))
            {
                continue;
            }

            mainPart.DeleteReferenceRelationship(rId);
        }
    }

    private List<Token> TokenizeParagraph(
        Paragraph paragraph,
        MainDocumentPart mainPart,
        List<(Paragraph Paragraph, Hyperlink Hyperlink)> hyperlinksToRemove,
        HashSet<string> relationshipsToDelete,
        IReport report)
    {
        var tokens = new List<Token>();
        foreach (var child in paragraph.ChildElements)
        {
            switch (child)
            {
                case Run run:
                    AppendRunTokens(run, tokens, report);
                    break;
                case Hyperlink hyperlink:
                    AppendHyperlinkTokens(hyperlink, paragraph, mainPart, tokens, hyperlinksToRemove, relationshipsToDelete, report);
                    break;
            }
        }

        return tokens;
    }

    private void AppendRunTokens(Run run, List<Token> tokens, IReport report)
    {
        var text = GetRunText(run);
        if (text.Length == 0)
        {
            return;
        }

        // Whitespace-only superscript runs are formatting artifacts (e.g. an
        // accidentally-typed space in superscript mode). They carry no label
        // content, but they DO carry visual whitespace that may belong to a
        // separator (" and ", ", ") that crosses run boundaries — so emit the
        // whitespace as a Text token and let the consume pass merge it.
        if (IsSuperscript(run) && !string.IsNullOrWhiteSpace(text))
        {
            foreach (var label in SplitLabels(text))
            {
                tokens.Add(new Token(TokenKind.Label, label));
            }

            return;
        }

        AppendPlainTextTokens(text, tokens, report);
    }

    // A byline may carry the ORCID as plain text (ORCID's display guidelines allow
    // the bare URI). Left in the name buffer it reaches SciELO Markup as the last
    // name token and mark_authors promotes it to <surname> (CBAB 5316, all seven
    // authors). Emit it as an Orcid token between the surrounding Text tokens so
    // the consume pass attaches it to the current author exactly like a hyperlink
    // ORCID (credit-corpus-v26n3-fixes ADR-006). No leading word boundary: the
    // corpus glues the id to the surname ("Bruzi0000-0001-…").
    private void AppendPlainTextTokens(string text, List<Token> tokens, IReport report)
    {
        var cursor = 0;
        foreach (Match match in PlainOrcidRegex().Matches(text))
        {
            if (match.Index > cursor)
            {
                tokens.Add(new Token(TokenKind.Text, text[cursor..match.Index]));
            }

            var orcidId = match.Groups["id"].Value;
            report.Info(Name, $"extracted ORCID '{orcidId}' from plain text");
            tokens.Add(new Token(TokenKind.Orcid, orcidId));
            cursor = match.Index + match.Length;
        }

        if (cursor < text.Length)
        {
            tokens.Add(new Token(TokenKind.Text, text[cursor..]));
        }
    }

    private void AppendHyperlinkTokens(
        Hyperlink hyperlink,
        Paragraph owningParagraph,
        MainDocumentPart mainPart,
        List<Token> tokens,
        List<(Paragraph Paragraph, Hyperlink Hyperlink)> hyperlinksToRemove,
        HashSet<string> relationshipsToDelete,
        IReport report)
    {
        var rId = hyperlink.Id?.Value;
        var url = ResolveHyperlinkTarget(mainPart, rId);
        var innerText = GetHyperlinkText(hyperlink);
        var hasDrawing = hyperlink.Descendants<Drawing>().Any();

        var hasUrl = !string.IsNullOrEmpty(url);
        var match = hasUrl ? _options.OrcidIdRegex.Match(url!) : Match.Empty;
        var hasOrcidMarker = hasUrl
            && url!.Contains(_options.OrcidUrlMarker, StringComparison.OrdinalIgnoreCase);
        var isOrcidHyperlink = match.Success || hasOrcidMarker;

        if (!isOrcidHyperlink)
        {
            if (innerText.Length > 0)
            {
                tokens.Add(new Token(TokenKind.Text, innerText));
            }

            return;
        }

        if (!match.Success)
        {
            report.Warn(
                Name,
                $"hyperlink target contains '{_options.OrcidUrlMarker}' but no ORCID ID was found: '{url}'");
            if (innerText.Length > 0)
            {
                tokens.Add(new Token(TokenKind.Text, innerText));
            }

            return;
        }

        var orcidId = match.Value;
        report.Info(Name, $"extracted ORCID '{orcidId}' from hyperlink");

        // Order matters: ORCID first, then visible text. The Orcid token
        // flushes any buffered text (which may include a trailing separator
        // like " and " that opens the next author), then attaches the id to
        // the (now fresh) builder. The Text token afterwards fills the name.
        tokens.Add(new Token(TokenKind.Orcid, orcidId));

        if (!IsBadgeContent(innerText, hasDrawing))
        {
            tokens.Add(new Token(TokenKind.Text, innerText));
        }

        hyperlinksToRemove.Add((owningParagraph, hyperlink));
        if (!string.IsNullOrEmpty(rId))
        {
            relationshipsToDelete.Add(rId);
        }
    }

    private void ConsumeTokens(IReadOnlyList<Token> tokens, List<AuthorBuilder> builders)
    {
        var buffer = new StringBuilder();
        foreach (var token in tokens)
        {
            switch (token.Kind)
            {
                case TokenKind.Text:
                    buffer.Append(token.Value);
                    break;
                case TokenKind.Label:
                    FlushBuffer(buffer, builders);
                    builders[^1].AddLabel(token.Value);
                    break;
                case TokenKind.Orcid:
                    FlushBuffer(buffer, builders);
                    AttachOrcid(builders[^1], token.Value);
                    break;
            }
        }

        FlushBuffer(buffer, builders);
    }

    // The first ORCID seen for an author wins (a hyperlink and its plain-text
    // caption normally agree). A second, different id is a byline defect the
    // operator must look at, so it is warned without lowering confidence: the name
    // itself is intact.
    private static void AttachOrcid(AuthorBuilder builder, string orcidId)
    {
        if (builder.OrcidId is null)
        {
            builder.OrcidId = orcidId;
            return;
        }

        if (!string.Equals(builder.OrcidId, orcidId, StringComparison.OrdinalIgnoreCase))
        {
            builder.Warn($"conflicting ORCID '{orcidId}' ignored; keeping '{builder.OrcidId}'");
        }
    }

    private void FlushBuffer(StringBuilder buffer, List<AuthorBuilder> builders)
    {
        if (buffer.Length == 0)
        {
            return;
        }

        ProcessTextRun(buffer.ToString(), builders);
        buffer.Clear();
    }

    private bool IsBadgeContent(string innerText, bool hasDrawing)
    {
        if (hasDrawing)
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(innerText))
        {
            return true;
        }

        if (_options.OrcidIdRegex.IsMatch(innerText))
        {
            return true;
        }

        return string.Equals(innerText.Trim(), "ORCID", StringComparison.OrdinalIgnoreCase);
    }

    private static string ResolveHyperlinkTarget(MainDocumentPart mainPart, string? rId)
    {
        if (string.IsNullOrEmpty(rId))
        {
            return string.Empty;
        }

        var rel = mainPart.HyperlinkRelationships.FirstOrDefault(r => r.Id == rId);
        return rel?.Uri?.ToString() ?? string.Empty;
    }

    private static string GetHyperlinkText(Hyperlink hyperlink)
        => string.Concat(hyperlink.Descendants<Text>().Select(t => t.Text));

    private void ProcessTextRun(string text, List<AuthorBuilder> builders)
    {
        var index = 0;
        while (index < text.Length)
        {
            var sepLength = MatchSeparatorAt(text, index);
            if (sepLength > 0)
            {
                builders.Add(new AuthorBuilder());
                index += sepLength;
                continue;
            }

            builders[^1].AppendName(text[index]);
            index++;
        }
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

    private static void FlagSuspicions(List<AuthorBuilder> builders)
    {
        for (var i = 0; i < builders.Count; i++)
        {
            var builder = builders[i];
            var name = builder.GetName().Trim();

            if (string.IsNullOrEmpty(name))
            {
                builder.MarkLow("empty name fragment");
                continue;
            }

            if (!AlphabeticRegex().IsMatch(name))
            {
                builder.MarkLow($"name fragment '{name}' contains no alphabetic characters");
                continue;
            }

            if (i > 0 && SuspiciousSuffixRegex().IsMatch(name))
            {
                var previous = builders[i - 1];
                var previousName = previous.GetName().Trim();
                previous.MarkLow(
                    $"possible incorrect split: trailing fragment '{name}' looks like a name suffix (Jr/Sr/II/III/IV) for '{previousName}'");
                builder.MarkLow($"name fragment '{name}' looks like a name suffix (Jr/Sr/II/III/IV)");
            }

            WarnOnRepeatedLastToken(builder, name);
        }
    }

    // SciELO Markup's markup_surname_and_fname takes the last token as the surname
    // and then tags its FIRST occurrence in the range (find_text_in_range), so a
    // name whose last token repeats an earlier one ("Nguyen Hoai Nguyen", CBAB
    // 5613) comes out as nested [fname][surname]…[/surname]…[/fname]. The name is
    // correct, so confidence stays; the operator is told before Markup runs
    // (credit-corpus-v26n3-fixes ADR-006).
    private static void WarnOnRepeatedLastToken(AuthorBuilder builder, string name)
    {
        var tokens = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length < 2)
        {
            return;
        }

        var last = FoldName(tokens[^1]);
        if (tokens[..^1].Any(t => FoldName(t) == last))
        {
            builder.Warn(
                $"last name token '{tokens[^1]}' repeats an earlier token; "
                + "SciELO Markup mark_authors will tag the first occurrence as the surname");
        }
    }

    /// <summary>Lowercases and strips diacritics so name tokens compare equal across accents.</summary>
    private static string FoldName(string value)
    {
        var decomposed = value.ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);
        foreach (var c in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(c);
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }

    private void EmitAuthors(List<AuthorBuilder> builders, FormattingContext ctx, IReport report)
    {
        if (builders.Count == 1 && IsBuilderEmpty(builders[0]))
        {
            report.Warn(Name, "authors paragraph yielded zero parseable name fragments");
            return;
        }

        for (var i = 0; i < builders.Count; i++)
        {
            var builder = builders[i];
            var name = builder.GetName().Trim();
            var authorNum = i + 1;

            ctx.Authors.Add(new Author(name, builder.Labels.ToArray(), builder.OrcidId, builder.Confidence));

            foreach (var warning in builder.Warnings)
            {
                report.Warn(Name, $"author #{authorNum} ('{name}'): {warning}");
            }
        }

        var distinctLabels = ctx.Authors
            .SelectMany(a => a.AffiliationLabels)
            .Distinct(StringComparer.Ordinal)
            .Count();

        report.Info(
            Name,
            $"detected {ctx.Authors.Count} author(s) with {distinctLabels} distinct affiliation label(s); "
            + "affiliation paragraph parsing is out of scope for the MVP");
    }

    private static bool IsBuilderEmpty(AuthorBuilder builder)
        => string.IsNullOrWhiteSpace(builder.GetName())
            && builder.Labels.Count == 0
            && builder.OrcidId is null;

    private static bool IsSuperscript(Run run)
    {
        var vert = run.RunProperties?.VerticalTextAlignment;
        if (vert?.Val is null)
        {
            return false;
        }

        return vert.Val.Value == VerticalPositionValues.Superscript;
    }

    private static string GetRunText(Run run)
        => string.Concat(run.Descendants<Text>().Select(t => t.Text));

    private static IEnumerable<string> SplitLabels(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            yield break;
        }

        foreach (var token in text.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        {
            yield return token;
        }
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

    [GeneratedRegex(@"^(Jr|Sr|II|III|IV)\.?$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex SuspiciousSuffixRegex();

    [GeneratedRegex(@"\p{L}", RegexOptions.CultureInvariant)]
    private static partial Regex AlphabeticRegex();

    // Plain-text ORCID, optionally prefixed by the orcid.org URL. Unlike
    // FormattingOptions.OrcidIdRegex there is no leading \b, so an id glued to the
    // surname still matches; the trailing lookahead keeps a longer digit run out.
    [GeneratedRegex(
        @"(?:https?://(?:www\.)?orcid\.org/)?(?<id>\d{4}-\d{4}-\d{4}-\d{3}[\dX])(?![\dX])",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex PlainOrcidRegex();

    private enum TokenKind
    {
        Text,
        Label,
        Orcid,
    }

    private readonly record struct Token(TokenKind Kind, string Value);

    private sealed class AuthorBuilder
    {
        private readonly StringBuilder _name = new();

        public List<string> Labels { get; } = new();

        public string? OrcidId { get; set; }

        public AuthorConfidence Confidence { get; private set; } = AuthorConfidence.High;

        public List<string> Warnings { get; } = new();

        public void AppendName(char c) => _name.Append(c);

        public string GetName() => _name.ToString();

        public void AddLabel(string label)
        {
            // ADR-008: a label that is purely a corresp marker (currently '*')
            // must merge onto the trailing affiliation label rather than form
            // its own entry. RewriteHeaderMvpRule comma-joins the labels into a
            // single superscript run, and Markup's mark_authors macro
            // (markup_macros.txt:4799) treats ',' as an inter-author separator
            // when the range has no ';'. Joining ["1", "*"] as "1,*" makes
            // Markup mis-split at the comma and fail to auto-mark the author.
            // Merging gives "1*" — same shape passing articles already produce.
            if (IsCorrespMarker(label) && Labels.Count > 0)
            {
                Labels[^1] = Labels[^1] + label;
                return;
            }

            Labels.Add(label);
        }

        private static bool IsCorrespMarker(string label)
        {
            if (string.IsNullOrEmpty(label))
            {
                return false;
            }

            foreach (var c in label)
            {
                if (c != '*')
                {
                    return false;
                }
            }

            return true;
        }

        public void MarkLow(string warning)
        {
            Confidence = AuthorConfidence.Low;
            Warnings.Add(warning);
        }

        /// <summary>Records a warning about the byline without lowering the name's confidence.</summary>
        public void Warn(string warning) => Warnings.Add(warning);
    }
}
