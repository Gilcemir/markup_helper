using DocFormatter.Core.Pipeline;

namespace DocFormatter.Core.Reporting;

public static class ReportWriter
{
    public static void Write(string filePath, IReport report)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath);
        ArgumentNullException.ThrowIfNull(report);

        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllLines(filePath, FormatLines(report));
    }

    public static IEnumerable<string> FormatLines(IReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        foreach (var entry in report.Entries)
        {
            yield return Format(entry);
        }
    }

    public static string Format(ReportEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        return $"[{LevelLabel(entry.Level)}] {entry.Rule} — {entry.Message}";
    }

    private static string LevelLabel(ReportLevel level) => level switch
    {
        ReportLevel.Info => "INFO",
        ReportLevel.Warn => "WARN",
        ReportLevel.Error => "ERROR",
        _ => level.ToString().ToUpperInvariant(),
    };
}
