namespace DocFormatter.Core.Jats;

/// <summary>
/// One contributor's row in a <see cref="CreditOutcome"/>: the author key as
/// written in the statement, the written role <paramref name="Terms"/>, how the
/// key resolved against the XML contributors (<see cref="CreditResolution"/>),
/// the terms that did not map to a CRediT role, and whether this run actually
/// wrote <c>&lt;role&gt;</c> elements for the contributor.
/// </summary>
public sealed record CreditEntryOutcome(
    string AuthorKey,
    IReadOnlyList<string> Terms,
    string Resolution,
    IReadOnlyList<string> UnknownTerms,
    bool Applied);

/// <summary>
/// The single structured result of <c>credit-roles</c> for one document
/// (ADR-005): the raw statement body, the shape the parser recognized, one
/// <see cref="CreditEntryOutcome"/> per parsed author entry (empty for prose,
/// header-empty and absent statements), and the document-level
/// <see cref="Disposition"/> (see <see cref="CreditDisposition"/>). Written
/// only by <see cref="CreditRolesInjector"/> onto
/// <see cref="Phase3Context.Credit"/>; read by the diagnostic and the batch
/// summary so the three consumers agree by construction.
/// </summary>
/// <remarks>
/// <see cref="Shape"/> is <see cref="CreditShape.Prose"/> when there is no body
/// to parse (header-empty and absent), matching
/// <see cref="CreditStatementParser.Parse(string?)"/> on blank input.
/// </remarks>
public sealed record CreditOutcome(
    string? Raw,
    CreditShape Shape,
    IReadOnlyList<CreditEntryOutcome> Entries,
    string Disposition);

/// <summary>The <see cref="CreditOutcome.Disposition"/> vocabulary.</summary>
public static class CreditDisposition
{
    /// <summary>Structured, every term mapped and every author resolved: written without a prompt.</summary>
    public const string AutoApplied = "autoApplied";

    /// <summary>Gated (unknown term or unresolved author); the operator accepted the clean subset.</summary>
    public const string Confirmed = "confirmed";

    /// <summary>Gated; the operator declined, nothing written.</summary>
    public const string Skipped = "skipped";

    /// <summary>Gated; the operator chose document-wide free-text roles (ADR-007).</summary>
    public const string FreeText = "freeText";

    /// <summary>The body is free prose; surfaced, never auto-applied.</summary>
    public const string Prose = "prose";

    /// <summary>The <c>CREDIT STATEMENT</c> header is present but its body is empty (INV-02).</summary>
    public const string HeaderEmpty = "headerEmpty";

    /// <summary>No <c>CREDIT STATEMENT</c> header on the docx.</summary>
    public const string Absent = "absent";
}

/// <summary>The <see cref="CreditEntryOutcome.Resolution"/> vocabulary, mirroring <see cref="ResolveStatus"/>.</summary>
public static class CreditResolution
{
    /// <summary>Exactly one contributor matched.</summary>
    public const string Resolved = "resolved";

    /// <summary>No contributor matched.</summary>
    public const string NotFound = "notFound";

    /// <summary>More than one contributor matched.</summary>
    public const string Ambiguous = "ambiguous";

    /// <summary>Maps a resolver <see cref="ResolveStatus"/> onto this vocabulary.</summary>
    public static string From(ResolveStatus status) => status switch
    {
        ResolveStatus.Resolved => Resolved,
        ResolveStatus.NotFound => NotFound,
        ResolveStatus.Ambiguous => Ambiguous,
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unknown resolve status."),
    };
}
