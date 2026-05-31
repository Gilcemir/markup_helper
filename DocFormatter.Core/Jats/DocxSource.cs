namespace DocFormatter.Core.Jats;

/// <summary>
/// The parsed, read-only view of a SciELO Markup <c>.docx</c> source as needed
/// by Phase 3 (ADR-002). Carries the <c>[doc]</c> header keys used to pair the
/// document with its XML and <c>other.txt</c> entry (ADR-004), plus the three
/// trailing untagged sections the Markup tool drops: the responsible editor
/// line(s), the data-availability statement, and the CRediT statement.
/// </summary>
/// <remarks>
/// The header keys are mandatory (the document cannot be paired without them).
/// The three section fields are nullable: an absent section is a valid state to
/// be reported downstream by the injectors, not an error here.
/// </remarks>
public sealed class DocxSource
{
    /// <summary>The <c>[doc]@elocatid</c> value, e.g. <c>e54492621</c>.</summary>
    public required string ElocationId { get; init; }

    /// <summary>The <c>[doi]…[/doi]</c> value from the header.</summary>
    public required string Doi { get; init; }

    /// <summary>The "Scientific Editor:" name, or <see langword="null"/> when absent.</summary>
    public string? ScientificEditor { get; init; }

    /// <summary>The optional "Associate Editor:" name, or <see langword="null"/> when absent.</summary>
    public string? AssociateEditor { get; init; }

    /// <summary>The DATA AVAILABILITY body text, or <see langword="null"/> when absent.</summary>
    public string? DataAvailabilityText { get; init; }

    /// <summary>
    /// The CREDIT STATEMENT body text returned verbatim and unparsed; shape
    /// detection (role-keyed / author-keyed / prose) belongs to a later task.
    /// <see langword="null"/> when the section is absent.
    /// </summary>
    public string? CreditStatementRaw { get; init; }
}
