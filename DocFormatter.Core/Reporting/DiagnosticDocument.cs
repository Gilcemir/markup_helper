using System.Text;

namespace DocFormatter.Core.Reporting;

public sealed record DiagnosticDocument(
    string File,
    string Status,
    DateTime ExtractedAt,
    DiagnosticFields Fields,
    DiagnosticFormatting? Formatting,
    IReadOnlyList<DiagnosticIssue> Issues,
    DiagnosticPhase2? Phase2 = null,
    DiagnosticPhase3? Phase3 = null)
{
    public bool Equals(DiagnosticDocument? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return string.Equals(File, other.File, StringComparison.Ordinal)
            && string.Equals(Status, other.Status, StringComparison.Ordinal)
            && ExtractedAt.Equals(other.ExtractedAt)
            && Fields.Equals(other.Fields)
            && Equals(Formatting, other.Formatting)
            && Issues.SequenceEqual(other.Issues)
            && Equals(Phase2, other.Phase2)
            && Equals(Phase3, other.Phase3);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(File, StringComparer.Ordinal);
        hash.Add(Status, StringComparer.Ordinal);
        hash.Add(ExtractedAt);
        hash.Add(Fields);
        hash.Add(Formatting);
        foreach (var issue in Issues)
        {
            hash.Add(issue);
        }
        hash.Add(Phase2);
        hash.Add(Phase3);

        return hash.ToHashCode();
    }
}

/// <summary>
/// The Phase 3 (JATS tag-injection) diagnostic section: one entry per injected
/// tag carrying the value that reached the XML and the disposition that produced
/// it (TechSpec "Monitoring and Observability"). The four tags appear in the
/// injector run order (ADR-003); <see cref="CreditStatement"/> carries the
/// CREDIT statement text and per-author resolution when <c>credit-roles</c>
/// recorded an outcome.
/// </summary>
public sealed record DiagnosticPhase3(
    DiagnosticPhase3Tag OtherId,
    DiagnosticPhase3Tag EditedBy,
    DiagnosticPhase3Tag DataAvailability,
    DiagnosticPhase3Tag CreditRoles,
    DiagnosticCreditStatement? CreditStatement = null);

/// <summary>
/// The CREDIT statement as <c>credit-roles</c> saw it (credit-corpus-v26n3-fixes
/// ADR-005): the <paramref name="Raw"/> body read from the docx, the camelCase
/// <see cref="DocFormatter.Core.Jats.CreditShape"/> name the parser recognized,
/// and one <see cref="DiagnosticCreditEntry"/> per parsed author entry (empty
/// for prose, header-empty and absent statements). <see langword="null"/> on
/// <see cref="DiagnosticPhase3.CreditStatement"/> when the injector recorded no
/// outcome. Lets the operator diagnose a pending article without reopening the
/// docx.
/// </summary>
public sealed record DiagnosticCreditStatement(
    string? Raw,
    string Shape,
    IReadOnlyList<DiagnosticCreditEntry> Entries)
{
    public bool Equals(DiagnosticCreditStatement? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return string.Equals(Raw, other.Raw, StringComparison.Ordinal)
            && string.Equals(Shape, other.Shape, StringComparison.Ordinal)
            && Entries.SequenceEqual(other.Entries);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Raw, StringComparer.Ordinal);
        hash.Add(Shape, StringComparer.Ordinal);
        foreach (var entry in Entries)
        {
            hash.Add(entry);
        }

        return hash.ToHashCode();
    }
}

/// <summary>
/// One author entry of a <see cref="DiagnosticCreditStatement"/>: the key as
/// written, the written role <paramref name="Terms"/>, how the key resolved
/// against the XML contributors (<c>resolved</c>/<c>notFound</c>/<c>ambiguous</c>),
/// the terms that did not map to a CRediT role, and whether this run wrote
/// <c>&lt;role&gt;</c> elements for the contributor.
/// </summary>
public sealed record DiagnosticCreditEntry(
    string AuthorKey,
    IReadOnlyList<string> Terms,
    string Resolution,
    IReadOnlyList<string> UnknownTerms,
    bool Applied)
{
    public bool Equals(DiagnosticCreditEntry? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return string.Equals(AuthorKey, other.AuthorKey, StringComparison.Ordinal)
            && string.Equals(Resolution, other.Resolution, StringComparison.Ordinal)
            && Applied == other.Applied
            && Terms.SequenceEqual(other.Terms, StringComparer.Ordinal)
            && UnknownTerms.SequenceEqual(other.UnknownTerms, StringComparer.Ordinal);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(AuthorKey, StringComparer.Ordinal);
        hash.Add(Resolution, StringComparer.Ordinal);
        hash.Add(Applied);
        foreach (var term in Terms)
        {
            hash.Add(term, StringComparer.Ordinal);
        }

        foreach (var term in UnknownTerms)
        {
            hash.Add(term, StringComparer.Ordinal);
        }

        return hash.ToHashCode();
    }
}

/// <summary>
/// A single Phase 3 tag outcome: the injector <paramref name="Tag"/> label, the
/// <paramref name="Value"/> written to the XML (the <c>other</c> number, the
/// <c>specific-use</c> category, the joined editor names, or the role count;
/// <see langword="null"/> when nothing was written), and the
/// <paramref name="Disposition"/> that produced it (a
/// <see cref="DocFormatter.Core.Jats.ConfirmDisposition"/> name for prompted tags,
/// else one of <c>autoApplied</c>/<c>skipped</c>/
/// <c>absent</c>/<c>failed</c>).
/// </summary>
public sealed record DiagnosticPhase3Tag(
    string Tag,
    string? Value,
    string Disposition);

public sealed record DiagnosticPhase2(
    DiagnosticField Elocation,
    DiagnosticField Abstract,
    DiagnosticField Keywords,
    DiagnosticField Corresp,
    IReadOnlyList<DiagnosticAuthorXref> Xref,
    DiagnosticField Hist)
{
    public bool Equals(DiagnosticPhase2? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Elocation.Equals(other.Elocation)
            && Abstract.Equals(other.Abstract)
            && Keywords.Equals(other.Keywords)
            && Corresp.Equals(other.Corresp)
            && Xref.SequenceEqual(other.Xref)
            && Hist.Equals(other.Hist);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Elocation);
        hash.Add(Abstract);
        hash.Add(Keywords);
        hash.Add(Corresp);
        foreach (var xref in Xref)
        {
            hash.Add(xref);
        }
        hash.Add(Hist);
        return hash.ToHashCode();
    }
}

public sealed record DiagnosticAuthorXref(
    int AuthorIndex,
    IReadOnlyList<string> Affiliations,
    bool Corresp,
    bool HasAuthorid)
{
    public bool Equals(DiagnosticAuthorXref? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return AuthorIndex == other.AuthorIndex
            && Corresp == other.Corresp
            && HasAuthorid == other.HasAuthorid
            && Affiliations.SequenceEqual(other.Affiliations, StringComparer.Ordinal);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(AuthorIndex);
        hash.Add(Corresp);
        hash.Add(HasAuthorid);
        foreach (var aff in Affiliations)
        {
            hash.Add(aff, StringComparer.Ordinal);
        }
        return hash.ToHashCode();
    }
}

public sealed record DiagnosticFormatting(
    DiagnosticAlignment? AlignmentApplied,
    DiagnosticAbstract? AbstractFormatted,
    bool? AuthorBlockSpacingApplied,
    DiagnosticCorrespondingEmail? CorrespondingEmail,
    DiagnosticHistoryMove? HistoryMove,
    DiagnosticSectionPromotion? SectionPromotion);

public sealed record DiagnosticAlignment(bool Doi, bool Section, bool Title);

public sealed record DiagnosticAbstract(
    bool HeadingRewritten,
    bool BodyDeitalicized,
    bool InternalItalicPreserved);

public sealed record DiagnosticCorrespondingEmail(string? Value, string? Reason);

public sealed record DiagnosticHistoryMove(
    bool Applied,
    string? SkippedReason,
    bool AnchorFound,
    int? FromIndex,
    int? ToIndexBeforeIntro,
    int ParagraphsMoved);

public sealed record DiagnosticSectionPromotion(
    bool Applied,
    string? SkippedReason,
    bool AnchorFound,
    int? AnchorParagraphIndex,
    int SectionsPromoted,
    int SubsectionsPromoted,
    int SkippedParagraphsInsideTables,
    int SkippedParagraphsBeforeAnchor);

public sealed record DiagnosticFields(
    DiagnosticField Doi,
    DiagnosticField Elocation,
    DiagnosticField Title,
    IReadOnlyList<DiagnosticAuthor> Authors)
{
    public bool Equals(DiagnosticFields? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Doi.Equals(other.Doi)
            && Elocation.Equals(other.Elocation)
            && Title.Equals(other.Title)
            && Authors.SequenceEqual(other.Authors);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Doi);
        hash.Add(Elocation);
        hash.Add(Title);
        foreach (var author in Authors)
        {
            hash.Add(author);
        }

        return hash.ToHashCode();
    }
}

public sealed record DiagnosticField(string? Value, FieldConfidence Confidence);

public sealed record DiagnosticAuthor(
    string Name,
    IReadOnlyList<string> AffiliationLabels,
    string? Orcid,
    FieldConfidence Confidence)
{
    public bool Equals(DiagnosticAuthor? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return string.Equals(Name, other.Name, StringComparison.Ordinal)
            && string.Equals(Orcid, other.Orcid, StringComparison.Ordinal)
            && Confidence == other.Confidence
            && AffiliationLabels.SequenceEqual(other.AffiliationLabels, StringComparer.Ordinal);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Name, StringComparer.Ordinal);
        hash.Add(Orcid, StringComparer.Ordinal);
        hash.Add(Confidence);
        foreach (var label in AffiliationLabels)
        {
            hash.Add(label, StringComparer.Ordinal);
        }

        return hash.ToHashCode();
    }

    private bool PrintMembers(StringBuilder builder)
    {
        builder.Append("Name = ").Append(Name);
        builder.Append(", AffiliationLabels = [").Append(string.Join(", ", AffiliationLabels)).Append(']');
        builder.Append(", Orcid = ").Append(Orcid ?? "null");
        builder.Append(", Confidence = ").Append(Confidence);
        return true;
    }
}

public sealed record DiagnosticIssue(string Rule, string Level, string Message);
