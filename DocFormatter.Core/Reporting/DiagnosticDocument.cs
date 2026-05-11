using System.Text;

namespace DocFormatter.Core.Reporting;

public sealed record DiagnosticDocument(
    string File,
    string Status,
    DateTime ExtractedAt,
    DiagnosticFields Fields,
    DiagnosticFormatting? Formatting,
    IReadOnlyList<DiagnosticIssue> Issues,
    DiagnosticPhase2? Phase2 = null)
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
            && Equals(Phase2, other.Phase2);
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

        return hash.ToHashCode();
    }
}

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
