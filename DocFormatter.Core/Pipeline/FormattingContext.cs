using DocFormatter.Core.Models;

namespace DocFormatter.Core.Pipeline;

public sealed class FormattingContext
{
    public string? Doi { get; set; }

    public string? ElocationId { get; set; }

    public string? ArticleTitle { get; set; }

    public List<Author> Authors { get; } = new();
}
