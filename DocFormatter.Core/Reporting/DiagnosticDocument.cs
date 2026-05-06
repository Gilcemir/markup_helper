using System.Text;

namespace DocFormatter.Core.Reporting;

public sealed record DiagnosticDocument(
    string File,
    string Status,
    DateTime ExtractedAt,
    DiagnosticFields Fields,
    IReadOnlyList<DiagnosticIssue> Issues)
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
            && Issues.SequenceEqual(other.Issues);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(File, StringComparer.Ordinal);
        hash.Add(Status, StringComparer.Ordinal);
        hash.Add(ExtractedAt);
        hash.Add(Fields);
        foreach (var issue in Issues)
        {
            hash.Add(issue);
        }

        return hash.ToHashCode();
    }
}

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
