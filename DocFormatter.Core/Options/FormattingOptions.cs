using System.Text.RegularExpressions;

namespace DocFormatter.Core.Options;

public sealed partial class FormattingOptions
{
    public Regex DoiRegex { get; } = BuildDoiRegex();

    public Regex OrcidIdRegex { get; } = BuildOrcidIdRegex();

    public string OrcidUrlMarker { get; } = "orcid.org";

    public IReadOnlyList<string> AuthorSeparators { get; } = new[] { ", ", " and " };

    public IReadOnlyList<string> AbstractMarkers { get; } = new[] { "abstract", "resumo" };

    [GeneratedRegex(@"^10\.\d{4,9}/[-._;()/:A-Z0-9]+$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex BuildDoiRegex();

    [GeneratedRegex(@"\b\d{4}-\d{4}-\d{4}-\d{3}[\dX]\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex BuildOrcidIdRegex();
}
