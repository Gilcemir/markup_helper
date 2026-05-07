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

    // Cross-rule paragraph references published by Phase 2 rules.
    //
    // Invariant: a rule that publishes a paragraph reference into this context
    // must NOT delete that paragraph later in the pipeline. A downstream rule
    // that decides to remove a paragraph stored here MUST null the matching
    // field first, otherwise the next consumer dereferences a detached node.
    public Paragraph? DoiParagraph { get; set; }

    public Paragraph? SectionParagraph { get; set; }

    public Paragraph? TitleParagraph { get; set; }

    public Paragraph? AuthorBlockEndParagraph { get; set; }

    public Paragraph? CorrespondingAffiliationParagraph { get; set; }

    public string? CorrespondingEmail { get; set; }

    public string? CorrespondingOrcid { get; set; }

    public int? CorrespondingAuthorIndex { get; set; }
}
