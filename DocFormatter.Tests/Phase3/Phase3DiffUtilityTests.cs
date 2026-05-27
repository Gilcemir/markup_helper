using DocFormatter.Core.Reporting;
using Xunit;

namespace DocFormatter.Tests.Phase3;

/// <summary>
/// Unit tests for <see cref="Phase3DiffUtility.CompareInjectedOnly"/>, the
/// line-level comparator backing the Phase 3 golden gate. It must accept a
/// produced document that is the source with injected-tag blocks inserted, and
/// flag any change outside those tags — a removed/modified source line or an
/// inserted line that is not part of an injected-tag block.
/// </summary>
public sealed class Phase3DiffUtilityTests
{
    private const string Source =
        "<article>\n" +
        "\t<front>\n" +
        "\t\t<article-meta>\n" +
        "\t\t\t<article-id pub-id-type=\"doi\">10.1/x</article-id>\n" +
        "\t\t</article-meta>\n" +
        "\t</front>\n" +
        "</article>";

    [Fact]
    public void Identical_Documents_AreInjectedTagsOnly()
    {
        var result = Phase3DiffUtility.CompareInjectedOnly(Source, Source);

        Assert.True(result.InjectedTagsOnly);
        Assert.Empty(result.RemovedOrModifiedLines);
        Assert.Empty(result.UnexpectedInsertedLines);
    }

    [Fact]
    public void OtherIdInsertion_IsInjectedTagsOnly()
    {
        var produced =
            "<article>\n" +
            "\t<front>\n" +
            "\t\t<article-meta>\n" +
            "\t\t\t<article-id pub-id-type=\"doi\">10.1/x</article-id>\n" +
            "\t\t\t<article-id pub-id-type=\"other\">00201</article-id>\n" +
            "\t\t</article-meta>\n" +
            "\t</front>\n" +
            "</article>";

        var result = Phase3DiffUtility.CompareInjectedOnly(Source, produced);

        Assert.True(result.InjectedTagsOnly);
    }

    [Fact]
    public void MultiLineInjectedBlock_WithChildLines_IsInjectedTagsOnly()
    {
        // The fn block's <label>/<p>/closing lines carry no signature on their own;
        // they are accepted because the block they belong to does.
        var produced =
            "<article>\n" +
            "\t<front>\n" +
            "\t\t<article-meta>\n" +
            "\t\t\t<article-id pub-id-type=\"doi\">10.1/x</article-id>\n" +
            "\t\t\t<fn fn-type=\"edited-by\">\n" +
            "\t\t\t\t<label>SCIENTIFIC EDITOR:</label>\n" +
            "\t\t\t\t<p>Some Name</p>\n" +
            "\t\t\t</fn>\n" +
            "\t\t</article-meta>\n" +
            "\t</front>\n" +
            "</article>";

        var result = Phase3DiffUtility.CompareInjectedOnly(Source, produced);

        Assert.True(result.InjectedTagsOnly);
        Assert.Empty(result.UnexpectedInsertedLines);
    }

    [Fact]
    public void ModifiedSourceLine_IsFlaggedAsRemovedOrModified()
    {
        // Reformatting an existing line surfaces as a deletion of the original.
        var produced = Source.Replace("\t\t<article-meta>", "\t\t<article-meta >");

        var result = Phase3DiffUtility.CompareInjectedOnly(Source, produced);

        Assert.False(result.InjectedTagsOnly);
        Assert.Contains("\t\t<article-meta>", result.RemovedOrModifiedLines);
    }

    [Fact]
    public void RemovedSourceLine_IsFlaggedAsRemovedOrModified()
    {
        var produced = Source.Replace(
            "\t\t\t<article-id pub-id-type=\"doi\">10.1/x</article-id>\n",
            string.Empty);

        var result = Phase3DiffUtility.CompareInjectedOnly(Source, produced);

        Assert.False(result.InjectedTagsOnly);
        Assert.Contains("\t\t\t<article-id pub-id-type=\"doi\">10.1/x</article-id>", result.RemovedOrModifiedLines);
    }

    [Fact]
    public void InsertedNonInjectedLine_IsFlaggedAsUnexpected()
    {
        // A genuine, non-injected insertion (e.g. incidental reformatting that adds
        // a blank-ish element) is rejected.
        var produced =
            "<article>\n" +
            "\t<front>\n" +
            "\t\t<article-meta>\n" +
            "\t\t\t<article-id pub-id-type=\"doi\">10.1/x</article-id>\n" +
            "\t\t\t<unexpected>noise</unexpected>\n" +
            "\t\t</article-meta>\n" +
            "\t</front>\n" +
            "</article>";

        var result = Phase3DiffUtility.CompareInjectedOnly(Source, produced);

        Assert.False(result.InjectedTagsOnly);
        Assert.Contains("\t\t\t<unexpected>noise</unexpected>", result.UnexpectedInsertedLines);
        Assert.Empty(result.RemovedOrModifiedLines);
    }

    [Fact]
    public void CreditRoleInsertion_IsInjectedTagsOnly()
    {
        var produced =
            "<article>\n" +
            "\t<front>\n" +
            "\t\t<article-meta>\n" +
            "\t\t\t<article-id pub-id-type=\"doi\">10.1/x</article-id>\n" +
            "\t\t\t<role content-type=\"http://credit.niso.org/contributor-roles/conceptualization/\">Conceptualization</role>\n" +
            "\t\t</article-meta>\n" +
            "\t</front>\n" +
            "</article>";

        var result = Phase3DiffUtility.CompareInjectedOnly(Source, produced);

        Assert.True(result.InjectedTagsOnly);
    }
}
