namespace DocFormatter.Core.Pipeline;

public interface IReport
{
    IReadOnlyList<ReportEntry> Entries { get; }

    ReportLevel HighestLevel { get; }

    void Info(string rule, string message);

    void Warn(string rule, string message);

    void Error(string rule, string message);
}
