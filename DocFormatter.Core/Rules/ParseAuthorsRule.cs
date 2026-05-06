using System.Text;
using System.Text.RegularExpressions;
using DocFormatter.Core.Models;
using DocFormatter.Core.Options;
using DocFormatter.Core.Pipeline;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace DocFormatter.Core.Rules;

public sealed partial class ParseAuthorsRule : IFormattingRule
{
    public const string MissingAuthorsParagraphMessage = "authors paragraph not found";

    private readonly FormattingOptions _options;

    public ParseAuthorsRule(FormattingOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options;
    }

    public string Name => nameof(ParseAuthorsRule);

    public RuleSeverity Severity => RuleSeverity.Optional;

    public void Apply(WordprocessingDocument doc, FormattingContext ctx, IReport report)
    {
        ArgumentNullException.ThrowIfNull(doc);
        ArgumentNullException.ThrowIfNull(ctx);
        ArgumentNullException.ThrowIfNull(report);

        var body = doc.MainDocumentPart?.Document?.Body
            ?? throw new InvalidOperationException("document is missing its body");

        var authorsParagraph = HeaderParagraphLocator.FindAuthorsParagraph(body);
        if (authorsParagraph is null)
        {
            report.Warn(Name, MissingAuthorsParagraphMessage);
            return;
        }

        var builders = TokenizeAndSplit(authorsParagraph, ctx.OrcidStaging);
        FlagSuspicions(builders);
        EmitAuthors(builders, ctx, report);
    }

    private List<AuthorBuilder> TokenizeAndSplit(Paragraph paragraph, IReadOnlyDictionary<int, string> orcidStaging)
    {
        var builders = new List<AuthorBuilder> { new() };
        var runs = paragraph.Elements<Run>().ToList();

        for (var i = 0; i < runs.Count; i++)
        {
            var run = runs[i];

            if (orcidStaging.TryGetValue(i, out var orcidId))
            {
                builders[^1].OrcidId ??= orcidId;
                continue;
            }

            if (IsSuperscript(run))
            {
                foreach (var label in SplitLabels(GetRunText(run)))
                {
                    builders[^1].AddLabel(label);
                }

                continue;
            }

            ProcessTextRun(GetRunText(run), builders);
        }

        return builders;
    }

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
                builder.MarkLow($"empty name fragment");
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
        }
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

    [GeneratedRegex(@"^(Jr|Sr|II|III|IV)\.?$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex SuspiciousSuffixRegex();

    [GeneratedRegex(@"\p{L}", RegexOptions.CultureInvariant)]
    private static partial Regex AlphabeticRegex();

    private sealed class AuthorBuilder
    {
        private readonly StringBuilder _name = new();

        public List<string> Labels { get; } = new();

        public string? OrcidId { get; set; }

        public AuthorConfidence Confidence { get; private set; } = AuthorConfidence.High;

        public List<string> Warnings { get; } = new();

        public void AppendName(char c) => _name.Append(c);

        public string GetName() => _name.ToString();

        public void AddLabel(string label) => Labels.Add(label);

        public void MarkLow(string warning)
        {
            Confidence = AuthorConfidence.Low;
            Warnings.Add(warning);
        }
    }
}
