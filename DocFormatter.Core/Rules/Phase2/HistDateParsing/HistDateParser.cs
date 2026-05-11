using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace DocFormatter.Core.Rules.Phase2.HistDateParsing;

/// <summary>
/// Parses <c>received</c> / <c>accepted</c> / <c>published</c> history
/// phrases from natural-language text into <see cref="HistDate"/> records.
/// Per ADR-007 the parser is implemented from scratch — the original
/// <c>Marcador_de_referencia/BibliographyHandlers/AccessedOnHandler.cs</c>
/// served only as a phrase inventory reference (see
/// <c>adrs/adr-007-phrase-inventory.md</c> in the PRD directory).
/// All entry points return <see langword="null"/> on unrecognized input.
/// Callers (the Phase 4 emitter rule) skip-and-warn per ADR-002.
/// </summary>
public static class HistDateParser
{
    private static readonly string[] ReceivedHeaders = ["Received", "Recebido em"];
    private static readonly string[] AcceptedHeaders = ["Accepted", "Aceito em"];
    private static readonly string[] PublishedHeaders = ["Published", "Publicado em"];

    private static readonly string[] EnglishMonths =
    [
        "January", "February", "March", "April", "May", "June",
        "July", "August", "September", "October", "November", "December",
    ];

    private static readonly string[] EnglishMonthAbbrev =
    [
        "Jan", "Feb", "Mar", "Apr", "May", "Jun",
        "Jul", "Aug", "Sep", "Oct", "Nov", "Dec",
    ];

    private static readonly string[] PortugueseMonths =
    [
        "janeiro", "fevereiro", "marco", "abril", "maio", "junho",
        "julho", "agosto", "setembro", "outubro", "novembro", "dezembro",
    ];

    private static readonly Regex IsoDateRegex =
        new(@"^(\d{4})-(\d{2})-(\d{2})$", RegexOptions.Compiled);

    private static readonly Regex DayMonthYearRegex =
        new(@"^(\d{1,2})\s+([A-Za-z]+)\s+(\d{4})$", RegexOptions.Compiled);

    private static readonly Regex MonthDayYearRegex =
        new(@"^([A-Za-z]+)\s+(\d{1,2})[,.]?\s+(\d{4})$", RegexOptions.Compiled);

    private static readonly Regex PortugueseDateRegex =
        new(
            @"^(\d{1,2})\s+de\s+([A-Za-zÀ-ſ]+)\s+de\s+(\d{4})$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex YearOnlyRegex =
        new(@"^(\d{4})$", RegexOptions.Compiled);

    public static HistDate? ParseReceived(string text)
        => ParseWithHeaders(text, ReceivedHeaders);

    public static HistDate? ParseAccepted(string text)
        => ParseWithHeaders(text, AcceptedHeaders);

    public static HistDate? ParsePublished(string text)
        => ParseWithHeaders(text, PublishedHeaders);

    private static HistDate? ParseWithHeaders(string? text, string[] headers)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        var trimmed = text.TrimStart();
        foreach (var header in headers)
        {
            if (!trimmed.StartsWith(header, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var afterHeader = trimmed[header.Length..];
            var dateSpan = StripSeparator(afterHeader);
            if (dateSpan is null)
            {
                continue;
            }

            var hist = ParseDateSpan(dateSpan);
            if (hist is not null)
            {
                return hist;
            }
        }

        return null;
    }

    /// <summary>
    /// Consumes the punctuation/preposition that separates the header word
    /// from the date phrase. Accepts <c>:</c>, whitespace, and the leading
    /// English "on" / Portuguese "em" prepositions when the header itself
    /// did not already absorb them. Returns <see langword="null"/> when the
    /// next character rules out a real header match (e.g. <c>"Receivedfoo"</c>).
    /// </summary>
    private static string? StripSeparator(string after)
    {
        if (after.Length == 0)
        {
            return null;
        }

        var first = after[0];
        if (first != ':' && !char.IsWhiteSpace(first))
        {
            return null;
        }

        var index = 0;
        if (after[index] == ':')
        {
            index++;
        }

        while (index < after.Length && char.IsWhiteSpace(after[index]))
        {
            index++;
        }

        var rest = after[index..];

        foreach (var preposition in new[] { "on", "em" })
        {
            if (rest.Length > preposition.Length
                && rest.StartsWith(preposition, StringComparison.OrdinalIgnoreCase)
                && char.IsWhiteSpace(rest[preposition.Length]))
            {
                rest = rest[preposition.Length..].TrimStart();
                break;
            }
        }

        return rest.Length == 0 ? null : rest;
    }

    private static HistDate? ParseDateSpan(string span)
    {
        var trimmed = span.Trim().TrimEnd('.', ',').Trim();
        if (trimmed.Length == 0)
        {
            return null;
        }

        var iso = IsoDateRegex.Match(trimmed);
        if (iso.Success)
        {
            var y = ParseInt(iso.Groups[1].Value);
            var mo = ParseInt(iso.Groups[2].Value);
            var d = ParseInt(iso.Groups[3].Value);
            return IsValidDate(y, mo, d) ? new HistDate(y, mo, d, trimmed) : null;
        }

        var dmy = DayMonthYearRegex.Match(trimmed);
        if (dmy.Success)
        {
            var d = ParseInt(dmy.Groups[1].Value);
            var mo = MatchEnglishMonth(dmy.Groups[2].Value);
            var y = ParseInt(dmy.Groups[3].Value);
            return mo.HasValue && IsValidDate(y, mo.Value, d)
                ? new HistDate(y, mo.Value, d, trimmed)
                : null;
        }

        var mdy = MonthDayYearRegex.Match(trimmed);
        if (mdy.Success)
        {
            var mo = MatchEnglishMonth(mdy.Groups[1].Value);
            var d = ParseInt(mdy.Groups[2].Value);
            var y = ParseInt(mdy.Groups[3].Value);
            return mo.HasValue && IsValidDate(y, mo.Value, d)
                ? new HistDate(y, mo.Value, d, trimmed)
                : null;
        }

        var pt = PortugueseDateRegex.Match(trimmed);
        if (pt.Success)
        {
            var d = ParseInt(pt.Groups[1].Value);
            var mo = MatchPortugueseMonth(pt.Groups[2].Value);
            var y = ParseInt(pt.Groups[3].Value);
            return mo.HasValue && IsValidDate(y, mo.Value, d)
                ? new HistDate(y, mo.Value, d, trimmed)
                : null;
        }

        var yo = YearOnlyRegex.Match(trimmed);
        if (yo.Success)
        {
            var y = ParseInt(yo.Groups[1].Value);
            return y is >= 1000 and <= 2999 ? new HistDate(y, null, null, trimmed) : null;
        }

        return null;
    }

    private static int ParseInt(string s)
        => int.Parse(s, CultureInfo.InvariantCulture);

    private static int? MatchEnglishMonth(string s)
    {
        for (var i = 0; i < EnglishMonths.Length; i++)
        {
            if (string.Equals(EnglishMonths[i], s, StringComparison.OrdinalIgnoreCase))
            {
                return i + 1;
            }
        }

        for (var i = 0; i < EnglishMonthAbbrev.Length; i++)
        {
            if (string.Equals(EnglishMonthAbbrev[i], s, StringComparison.OrdinalIgnoreCase))
            {
                return i + 1;
            }
        }

        return null;
    }

    private static int? MatchPortugueseMonth(string s)
    {
        var normalized = StripAccents(s).ToLowerInvariant();
        for (var i = 0; i < PortugueseMonths.Length; i++)
        {
            if (PortugueseMonths[i] == normalized)
            {
                return i + 1;
            }
        }

        return null;
    }

    private static string StripAccents(string s)
    {
        var formD = s.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(formD.Length);
        foreach (var c in formD)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }

        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    private static bool IsValidDate(int year, int month, int day)
    {
        if (year is < 1 or > 9999)
        {
            return false;
        }

        if (month is < 1 or > 12)
        {
            return false;
        }

        if (day < 1)
        {
            return false;
        }

        return day <= DateTime.DaysInMonth(year, month);
    }
}
