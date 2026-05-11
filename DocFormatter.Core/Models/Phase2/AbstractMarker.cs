using DocumentFormat.OpenXml.Wordprocessing;

namespace DocFormatter.Core.Models.Phase2;

/// <summary>
/// Identifies the abstract section in the document body so emitter rules can
/// agree on which paragraphs the <c>[xmlabstr language="…"]…[/xmlabstr]</c>
/// pair wraps. The heading paragraph is the one matching one of the configured
/// <c>AbstractMarkers</c> (e.g. "Abstract"); the body paragraph is the next
/// non-heading paragraph that carries the abstract prose.
/// </summary>
public sealed record AbstractMarker(
    string Language,
    Paragraph HeadingParagraph,
    Paragraph BodyParagraph);
