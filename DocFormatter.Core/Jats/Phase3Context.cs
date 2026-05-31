using System.Xml.Linq;

namespace DocFormatter.Core.Jats;

/// <summary>
/// The per-document working state passed to each <see cref="IJatsInjector"/>
/// (ADR-003). Carries the read-only parsed docx <see cref="Source"/>, the target
/// <see cref="Xml"/> document that injectors mutate, the resolved <c>other</c>
/// number from <c>other.txt</c>, and the <see cref="IConfirmer"/> gate (ADR-006).
/// </summary>
public sealed class Phase3Context
{
    /// <summary>The parsed read-only docx source (ADR-002).</summary>
    public required DocxSource Source { get; init; }

    /// <summary>The JATS XML document — the only artifact injectors mutate.</summary>
    public required XDocument Xml { get; init; }

    /// <summary>
    /// The <c>other</c> number from <c>other.txt</c>, or <see langword="null"/>
    /// when the entry is missing (handled as a fail-loud case in the
    /// <c>OtherIdInjector</c>).
    /// </summary>
    public string? OtherNumber { get; init; }

    /// <summary>The inline confirmation gate for ambiguous proposals (ADR-006).</summary>
    public required IConfirmer Confirm { get; init; }
}
