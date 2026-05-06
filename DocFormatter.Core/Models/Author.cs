using System.Text;

namespace DocFormatter.Core.Models;

public sealed record Author(
    string Name,
    IReadOnlyList<string> AffiliationLabels,
    string? OrcidId,
    AuthorConfidence Confidence = AuthorConfidence.High)
{
    public bool Equals(Author? other)
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
            && string.Equals(OrcidId, other.OrcidId, StringComparison.Ordinal)
            && Confidence == other.Confidence
            && AffiliationLabels.SequenceEqual(other.AffiliationLabels, StringComparer.Ordinal);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Name, StringComparer.Ordinal);
        hash.Add(OrcidId, StringComparer.Ordinal);
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
        if (OrcidId is not null)
        {
            builder.Append(", OrcidId = ").Append(OrcidId);
        }

        builder.Append(", Confidence = ").Append(Confidence);
        return true;
    }
}
