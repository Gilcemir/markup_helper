using System.Text;
using DocFormatter.Core.Options;
using DocFormatter.Core.Pipeline;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace DocFormatter.Core.Rules;

public sealed class ExtractTopTableRule : IFormattingRule
{
    public const string CriticalAbortMessage =
        "este arquivo não está no formato de entrada esperado — pode já estar formatado, ou ser de outra fonte";

    private static readonly string[] HeaderKeys = { "id", "elocation", "doi" };

    private readonly FormattingOptions _options;

    public ExtractTopTableRule(FormattingOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options;
    }

    public string Name => nameof(ExtractTopTableRule);

    public RuleSeverity Severity => RuleSeverity.Critical;

    public void Apply(WordprocessingDocument doc, FormattingContext ctx, IReport report)
    {
        ArgumentNullException.ThrowIfNull(doc);
        ArgumentNullException.ThrowIfNull(ctx);
        ArgumentNullException.ThrowIfNull(report);

        var body = doc.MainDocumentPart?.Document?.Body
            ?? throw new InvalidOperationException(CriticalAbortMessage);

        var firstContentElement = body.ChildElements.FirstOrDefault(e => e is not SectionProperties);
        if (firstContentElement is not Table table || !IsThreeByOne(table))
        {
            throw new InvalidOperationException(CriticalAbortMessage);
        }

        var cells = table.Elements<TableRow>().First().Elements<TableCell>().ToList();
        var cellTexts = cells.Select(GetCellPlainText).ToList();

        string idValue;
        string elocationValue;
        string doiValue;

        var headerMapping = TryHeaderMapping(cellTexts);
        if (headerMapping is { } mapped)
        {
            (idValue, elocationValue, doiValue) = mapped;
        }
        else
        {
            report.Warn(Name, "headers absent, fell back to positional mapping");
            idValue = cellTexts[0];
            elocationValue = cellTexts[1];
            doiValue = cellTexts[2];
        }

        var normalizedCells = cellTexts.Select(StripDoiUrlPrefix).ToList();
        var normalizedPositionalDoi = StripDoiUrlPrefix(doiValue);
        if (!string.IsNullOrEmpty(normalizedPositionalDoi) && _options.DoiRegex.IsMatch(normalizedPositionalDoi))
        {
            ctx.Doi = normalizedPositionalDoi;
        }
        else
        {
            var fallbackDoi = normalizedCells
                .FirstOrDefault(v => !string.IsNullOrEmpty(v) && _options.DoiRegex.IsMatch(v));
            if (fallbackDoi is not null)
            {
                ctx.Doi = fallbackDoi;
                report.Warn(
                    Name,
                    $"DOI cell did not match DOI regex; using DOI-shaped value found in another cell: '{fallbackDoi}'");
            }
            else
            {
                ctx.Doi = null;
                report.Warn(Name, "no cell contains a DOI-shaped value; setting Doi=null");
            }
        }

        if (headerMapping is null)
        {
            elocationValue = ResolveElocation(elocationValue, cellTexts, report);
        }

        ctx.ElocationId = elocationValue;
        if (string.IsNullOrWhiteSpace(elocationValue))
        {
            report.Warn(Name, "elocation cell is empty");
        }

        table.Remove();

        report.Info(
            Name,
            $"top table extracted; ElocationId='{ctx.ElocationId ?? string.Empty}', Doi='{ctx.Doi ?? "<none>"}', Id='{idValue}'");
    }

    private static bool IsThreeByOne(Table table)
    {
        var rows = table.Elements<TableRow>().ToList();
        if (rows.Count != 1)
        {
            return false;
        }

        var cells = rows[0].Elements<TableCell>().ToList();
        return cells.Count == 3;
    }

    private static (string Id, string Elocation, string Doi)? TryHeaderMapping(IReadOnlyList<string> cellTexts)
    {
        var detected = cellTexts.Select(DetectHeader).ToList();
        if (detected.Any(d => d.Header is null))
        {
            return null;
        }

        var headerSet = detected.Select(d => d.Header!).ToHashSet(StringComparer.Ordinal);
        if (!headerSet.SetEquals(HeaderKeys))
        {
            return null;
        }

        var id = string.Empty;
        var elocation = string.Empty;
        var doi = string.Empty;
        foreach (var (header, value) in detected)
        {
            switch (header)
            {
                case "id":
                    id = value;
                    break;
                case "elocation":
                    elocation = value;
                    break;
                case "doi":
                    doi = value;
                    break;
            }
        }

        return (id, elocation, doi);
    }

    private static (string? Header, string Value) DetectHeader(string cellText)
    {
        if (string.IsNullOrWhiteSpace(cellText))
        {
            return (null, string.Empty);
        }

        var trimmed = cellText.Trim();
        var lines = trimmed
            .Split('\n', StringSplitOptions.None)
            .Select(l => l.Trim())
            .ToList();

        if (lines.Count >= 2)
        {
            var firstLineLower = lines[0].ToLowerInvariant();
            foreach (var key in HeaderKeys)
            {
                if (firstLineLower == key)
                {
                    var value = string.Join('\n', lines.Skip(1)).Trim();
                    return (key, value);
                }
            }
        }

        var separatorIndex = trimmed.IndexOfAny(new[] { ':', '=' });
        if (separatorIndex > 0)
        {
            var possibleHeader = trimmed[..separatorIndex].Trim().ToLowerInvariant();
            if (HeaderKeys.Contains(possibleHeader))
            {
                var value = trimmed[(separatorIndex + 1)..].Trim();
                return (possibleHeader, value);
            }
        }

        return (null, cellText);
    }

    private static string GetCellPlainText(TableCell cell)
    {
        var sb = new StringBuilder();
        var firstParagraph = true;
        foreach (var paragraph in cell.Elements<Paragraph>())
        {
            if (!firstParagraph)
            {
                sb.Append('\n');
            }
            firstParagraph = false;

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
                    case TabChar:
                        sb.Append('\t');
                        break;
                }
            }
        }
        return sb.ToString();
    }

    private string StripDoiUrlPrefix(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        var trimmed = value.Trim();
        foreach (var prefix in _options.DoiUrlPrefixes)
        {
            if (trimmed.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return trimmed[prefix.Length..];
            }
        }

        return trimmed;
    }

    private string ResolveElocation(string elocationValue, IReadOnlyList<string> allCells, IReport report)
    {
        if (string.IsNullOrWhiteSpace(elocationValue) || _options.ElocationRegex.IsMatch(elocationValue))
        {
            return elocationValue;
        }

        var fallback = allCells
            .FirstOrDefault(v => !string.IsNullOrEmpty(v) && _options.ElocationRegex.IsMatch(v));
        if (fallback is not null)
        {
            report.Warn(
                Name,
                $"positional ELOCATION cell did not match shape; using ELOCATION-shaped value from another cell: '{fallback}'");
            return fallback;
        }

        report.Warn(Name, "no cell contains an ELOCATION-shaped value; setting ElocationId=empty");
        return string.Empty;
    }
}
