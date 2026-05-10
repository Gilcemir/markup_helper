using System.Globalization;

namespace DocFormatter.Core.Rules.Phase2.HistDateParsing;

/// <summary>
/// Calendar date parsed out of a Phase 4 history paragraph
/// (<c>received</c> / <c>accepted</c> / <c>histdate datetype="pub"</c>).
/// <see cref="Month"/> and <see cref="Day"/> may be <see langword="null"/> when
/// the source phrase is incomplete (e.g. <c>"Published 2024"</c>).
/// <see cref="SourceText"/> is the trimmed, header-stripped phrase preserved
/// verbatim so the emitter rule can write it as the wrapped tag content.
/// </summary>
public sealed record HistDate(int Year, int? Month, int? Day, string SourceText)
{
    /// <summary>
    /// Returns the SciELO <c>dateiso</c> attribute value: <c>YYYYMMDD</c>,
    /// zero-padded, with <c>00</c> substituted for missing month or day
    /// (per <c>docs/scielo_context/README.md</c> invariant 5).
    /// </summary>
    public string ToDateIso()
    {
        var year = Year.ToString("D4", CultureInfo.InvariantCulture);
        var month = Month.HasValue
            ? Month.Value.ToString("D2", CultureInfo.InvariantCulture)
            : "00";
        var day = Day.HasValue
            ? Day.Value.ToString("D2", CultureInfo.InvariantCulture)
            : "00";
        return $"{year}{month}{day}";
    }
}
