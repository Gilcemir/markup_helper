using DocFormatter.Core.Models;
using DocumentFormat.OpenXml.Wordprocessing;

namespace DocFormatter.Core.Pipeline;

public sealed class FormattingContext
{
    public string? Doi { get; set; }

    public string? ElocationId { get; set; }

    public string? ArticleTitle { get; set; }

    public List<Author> Authors { get; } = new();

    // Paragraphs that contained the original author block. ExtractAuthorsRule
    // populates this; RewriteHeaderMvpRule reads from it. Sharing the list
    // means both rules agree on the boundary even after ExtractAuthorsRule has
    // mutated the body (e.g., by removing ORCID hyperlinks).
    public List<Paragraph> AuthorParagraphs { get; } = new();
}
