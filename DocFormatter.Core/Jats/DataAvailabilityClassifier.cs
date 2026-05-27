using System.Text;

namespace DocFormatter.Core.Jats;

/// <summary>
/// The outcome of classifying a data-availability statement into one of the five
/// SPS <c>@specific-use</c> values. <see cref="IsConfident"/> is the confidence
/// signal the <see cref="DataAvailabilityInjector"/> uses to gate auto-apply vs
/// the <see cref="IConfirmer"/> prompt (ADR-001/ADR-005): a single unambiguous
/// keyword winner is confident; no match or a tie is not.
/// </summary>
public sealed record DataAvailabilityClassification(string SpecificUse, bool IsConfident, string Reason);

/// <summary>
/// Classifies a free-text data-availability statement into one of the five SPS
/// 1.10 <c>@specific-use</c> values via a conservative keyword heuristic seeded
/// from <c>docs/scielo_context/jats/data_availability.md</c>. The classification
/// is reported as confident only when exactly one value's keywords match, which
/// keeps the auto-apply gate tight and surfaces genuinely ambiguous statements
/// for confirmation (ADR-001: favor prompting over a silently wrong category).
/// </summary>
public static class DataAvailabilityClassifier
{
    /// <summary>Dados disponíveis em repositório (com link/DOI).</summary>
    public const string DataAvailable = "data-available";

    /// <summary>Disponíveis apenas mediante solicitação.</summary>
    public const string DataAvailableUponRequest = "data-available-upon-request";

    /// <summary>Disponíveis no corpo do documento.</summary>
    public const string DataInArticle = "data-in-article";

    /// <summary>Não disponíveis.</summary>
    public const string DataNotAvailable = "data-not-available";

    /// <summary>Uso não informado / nenhum dado gerado ou utilizado.</summary>
    public const string Uninformed = "uninformed";

    /// <summary>
    /// The best-guess value proposed when the statement matches no keyword class.
    /// Mirrors the predecessor MathML project's manual default (see
    /// <c>data_availability.md</c>): "available upon request" is the most common
    /// statement, so it is the safest guess to put in front of the operator.
    /// </summary>
    public const string DefaultSpecificUse = DataAvailableUponRequest;

    // Ordered by the five-value table in data_availability.md. The first entry is
    // the tie-breaker when two classes draw, so the surfaced best-guess is stable.
    private static readonly (string Value, string[] Keywords)[] Corpus =
    {
        (DataAvailable, new[]
        {
            "openly available", "publicly available", "freely available",
            "repository", "repositório", "deposited", "depositad",
            "doi.org", "https://", "http://", "available at",
            "figshare", "zenodo", "dryad", "genbank", "github", "accession",
        }),
        (DataAvailableUponRequest, new[]
        {
            "upon request", "upon reasonable request", "on request",
            "corresponding author", "from the author", "from the authors",
            "mediante solicitação", "sob solicitação",
        }),
        (DataInArticle, new[]
        {
            "in the article", "within the article", "in this article",
            "in the manuscript", "within the manuscript", "in the paper",
            "tables and figures", "no artigo", "no próprio artigo", "neste artigo",
        }),
        (DataNotAvailable, new[]
        {
            "not available", "cannot be shared", "not be shared",
            "due to ethical", "due to legal", "due to privacy",
            "confidential", "restricted", "não estão disponíveis", "não disponíveis",
        }),
        (Uninformed, new[]
        {
            "no new data", "not applicable", "no data were created",
            "no data were generated", "no data were analyzed", "no data were analysed",
            "no datasets were generated", "nenhum dado", "não se aplica",
        }),
    };

    /// <summary>
    /// Classifies <paramref name="statement"/>. A value <em>matches</em> when any
    /// of its keywords occurs in the text. The result is confident only when
    /// exactly one of the five values matches — this is the conservative gate
    /// (ADR-001/ADR-005): a statement that hits two categories at once is a
    /// judgment call and is surfaced for confirmation rather than auto-applied.
    /// When no value matches, or several do, the best guess is proposed
    /// (<see cref="DataAvailabilityClassification.IsConfident"/> = <see langword="false"/>):
    /// the value with the most keyword hits, falling back to
    /// <see cref="DefaultSpecificUse"/> when nothing matches.
    /// </summary>
    public static DataAvailabilityClassification Classify(string? statement)
    {
        if (string.IsNullOrWhiteSpace(statement))
        {
            return new DataAvailabilityClassification(
                DefaultSpecificUse,
                IsConfident: false,
                "Statement is empty; cannot classify.");
        }

        // Normalize to NFC first so accented pt-BR keywords ("repositório",
        // "não estão disponíveis") match regardless of whether the docx text arrived
        // composed (NFC) or decomposed (NFD); the source keyword literals are NFC.
        var text = statement.Normalize(NormalizationForm.FormC).ToLowerInvariant();
        var hits = Corpus
            .Select(entry => (entry.Value, Count: entry.Keywords.Count(k => text.Contains(k, StringComparison.Ordinal))))
            .Where(s => s.Count > 0)
            .ToList();

        if (hits.Count == 0)
        {
            return new DataAvailabilityClassification(
                DefaultSpecificUse,
                IsConfident: false,
                "No data-availability keyword matched; proposing the default.");
        }

        if (hits.Count == 1)
        {
            return new DataAvailabilityClassification(
                hits[0].Value,
                IsConfident: true,
                $"Keyword match classified the statement as '{hits[0].Value}'.");
        }

        // Several categories matched: ambiguous. Propose the strongest as a best
        // guess (Corpus order breaks ties since OrderByDescending is stable).
        var matched = hits.Select(s => s.Value);
        var best = hits.OrderByDescending(s => s.Count).First().Value;
        return new DataAvailabilityClassification(
            best,
            IsConfident: false,
            $"Ambiguous: keywords matched {string.Join(", ", matched)}; proposing '{best}'.");
    }
}
