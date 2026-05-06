namespace DocFormatter.Core.Pipeline;

public sealed class Report : IReport
{
    private readonly List<ReportEntry> _entries = new();
    private ReportLevel _highest = ReportLevel.Info;

    public IReadOnlyList<ReportEntry> Entries => _entries;

    public ReportLevel HighestLevel => _highest;

    public void Info(string rule, string message) => Append(rule, ReportLevel.Info, message);

    public void Warn(string rule, string message) => Append(rule, ReportLevel.Warn, message);

    public void Error(string rule, string message) => Append(rule, ReportLevel.Error, message);

    private void Append(string rule, ReportLevel level, string message)
    {
        _entries.Add(new ReportEntry(rule, level, message, DateTime.UtcNow));
        if (level > _highest)
        {
            _highest = level;
        }
    }
}
