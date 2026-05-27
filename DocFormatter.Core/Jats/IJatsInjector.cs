using DocFormatter.Core.Pipeline;

namespace DocFormatter.Core.Jats;

/// <summary>
/// A single Phase 3 tag-injection unit. Mirrors <see cref="IFormattingRule"/> but
/// is typed for the docx→XML paradigm (ADR-003): the docx is a read-only source
/// and the target <see cref="System.Xml.Linq.XDocument"/> on the
/// <see cref="Phase3Context"/> is the only mutated artifact.
/// </summary>
public interface IJatsInjector
{
    /// <summary>Report label for this injector, e.g. <c>"other-id"</c>.</summary>
    string Name { get; }

    /// <summary>
    /// Severity governing the pipeline error model (reuses the phase 1/2 enum):
    /// an <see cref="RuleSeverity.Optional"/> injector that throws is logged and
    /// skipped; a <see cref="RuleSeverity.Critical"/> one aborts the document.
    /// </summary>
    RuleSeverity Severity { get; }

    /// <summary>
    /// Applies this injector to the context, mutating <see cref="Phase3Context.Xml"/>
    /// and emitting entries through <paramref name="report"/>.
    /// </summary>
    void Apply(Phase3Context ctx, IReport report);
}
