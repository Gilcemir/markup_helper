namespace DocFormatter.Core.Pipeline;

public sealed record ReportEntry(
    string Rule,
    ReportLevel Level,
    string Message,
    DateTime Timestamp);
