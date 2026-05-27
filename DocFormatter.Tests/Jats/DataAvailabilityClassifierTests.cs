using DocFormatter.Core.Jats;
using Xunit;

namespace DocFormatter.Tests.Jats;

public sealed class DataAvailabilityClassifierTests
{
    [Fact]
    public void Classify_UponReasonableRequest_IsDataAvailableUponRequest_Confident()
    {
        var result = DataAvailabilityClassifier.Classify(
            "The datasets generated and/or analyzed during the current study are available " +
            "from the corresponding author upon reasonable request.");

        Assert.Equal(DataAvailabilityClassifier.DataAvailableUponRequest, result.SpecificUse);
        Assert.True(result.IsConfident);
    }

    [Fact]
    public void Classify_RepositoryLinkAndDoi_IsDataAvailable_Confident()
    {
        var result = DataAvailabilityClassifier.Classify(
            "The data that support the findings of this study are openly available in the " +
            "Dryad repository at https://doi.org/10.5061/dryad.abc123.");

        Assert.Equal(DataAvailabilityClassifier.DataAvailable, result.SpecificUse);
        Assert.True(result.IsConfident);
    }

    [Fact]
    public void Classify_NoNewData_IsUninformed_Confident()
    {
        var result = DataAvailabilityClassifier.Classify(
            "No new data were created or analyzed in this study.");

        Assert.Equal(DataAvailabilityClassifier.Uninformed, result.SpecificUse);
        Assert.True(result.IsConfident);
    }

    [Fact]
    public void Classify_DataInArticle_IsDataInArticle_Confident()
    {
        var result = DataAvailabilityClassifier.Classify(
            "All data generated during this study are included in this article and its tables and figures.");

        Assert.Equal(DataAvailabilityClassifier.DataInArticle, result.SpecificUse);
        Assert.True(result.IsConfident);
    }

    [Fact]
    public void Classify_NotAvailable_IsDataNotAvailable_Confident()
    {
        var result = DataAvailabilityClassifier.Classify(
            "The data are not available because they cannot be shared due to ethical restrictions.");

        Assert.Equal(DataAvailabilityClassifier.DataNotAvailable, result.SpecificUse);
        Assert.True(result.IsConfident);
    }

    [Fact]
    public void Classify_NoKeywordMatch_ProposesDefault_NotConfident()
    {
        var result = DataAvailabilityClassifier.Classify(
            "Os procedimentos seguiram as diretrizes institucionais vigentes.");

        Assert.Equal(DataAvailabilityClassifier.DefaultSpecificUse, result.SpecificUse);
        Assert.False(result.IsConfident);
    }

    [Fact]
    public void Classify_ConflictingKeywords_IsAmbiguous_NotConfident()
    {
        // One keyword each from "upon request" (request) and "no new data"
        // (uninformed) → a genuine 1-1 tie that must not auto-apply.
        var result = DataAvailabilityClassifier.Classify(
            "Data are available upon request; no new data were generated for this work.");

        Assert.False(result.IsConfident);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Classify_BlankStatement_ProposesDefault_NotConfident(string? statement)
    {
        var result = DataAvailabilityClassifier.Classify(statement);

        Assert.Equal(DataAvailabilityClassifier.DefaultSpecificUse, result.SpecificUse);
        Assert.False(result.IsConfident);
    }
}
