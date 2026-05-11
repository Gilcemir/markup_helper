using DocFormatter.Core.Rules.Phase2.HistDateParsing;
using Xunit;

namespace DocFormatter.Tests.Phase2;

/// <summary>
/// TDD-first test suite for <see cref="HistDateParser"/>. Every recognized
/// phrase shape catalogued in
/// <c>.compozy/tasks/phase-2-tagging-author-fixes/adrs/adr-007-phrase-inventory.md</c>
/// has its test here. The tests were authored before the parser
/// implementation, per ADR-007.
/// </summary>
public sealed class HistDateParserTests
{
    // -- Header parsing: Received --

    [Fact]
    public void ParseReceived_ColonSeparator_DayMonthYear_FullMonth_Parses()
    {
        var result = HistDateParser.ParseReceived("Received: 31 January 2025");
        Assert.NotNull(result);
        Assert.Equal(2025, result!.Year);
        Assert.Equal(1, result.Month);
        Assert.Equal(31, result.Day);
        Assert.Equal("31 January 2025", result.SourceText);
    }

    [Fact]
    public void ParseReceived_OnSeparator_DayMonthYear_FullMonth_Parses()
    {
        var result = HistDateParser.ParseReceived("Received on 12 March 2024");
        Assert.NotNull(result);
        Assert.Equal(new HistDate(2024, 3, 12, "12 March 2024"), result);
    }

    [Fact]
    public void ParseReceived_ColonSeparator_IsoDate_Parses()
    {
        var result = HistDateParser.ParseReceived("Received: 2024-04-15");
        Assert.NotNull(result);
        Assert.Equal(new HistDate(2024, 4, 15, "2024-04-15"), result);
    }

    [Fact]
    public void ParseReceived_PortuguesePhrase_RecognizedOrNull()
    {
        // Portuguese is a stretch shape per the ADR notes. The parser MUST
        // either recognize it cleanly or return null without throwing.
        var result = HistDateParser.ParseReceived("Recebido em 12 de março de 2024");
        if (result is not null)
        {
            Assert.Equal(2024, result.Year);
            Assert.Equal(3, result.Month);
            Assert.Equal(12, result.Day);
        }
    }

    // -- Header parsing: Accepted --

    [Fact]
    public void ParseAccepted_ColonSeparator_IsoDate_Parses()
    {
        var result = HistDateParser.ParseAccepted("Accepted: 2024-04-15");
        Assert.NotNull(result);
        Assert.Equal(new HistDate(2024, 4, 15, "2024-04-15"), result);
    }

    [Fact]
    public void ParseAccepted_ColonSeparator_DayMonthYear_FullMonth_Parses()
    {
        var result = HistDateParser.ParseAccepted("Accepted: 7 January 2026");
        Assert.NotNull(result);
        Assert.Equal(new HistDate(2026, 1, 7, "7 January 2026"), result);
    }

    // -- Header parsing: Published --

    [Fact]
    public void ParsePublished_BareSeparator_YearOnly_Parses()
    {
        var result = HistDateParser.ParsePublished("Published 2024");
        Assert.NotNull(result);
        Assert.Equal(2024, result!.Year);
        Assert.Null(result.Month);
        Assert.Null(result.Day);
        Assert.Equal("2024", result.SourceText);
    }

    [Fact]
    public void ParsePublished_ColonSeparator_DayMonthYear_Parses()
    {
        var result = HistDateParser.ParsePublished("Published: 24 February 2026");
        Assert.NotNull(result);
        Assert.Equal(new HistDate(2026, 2, 24, "24 February 2026"), result);
    }

    // -- Each abbreviated English month --

    [Theory]
    [InlineData("Received: 12 Jan 2024", 1)]
    [InlineData("Received: 12 Feb 2024", 2)]
    [InlineData("Received: 12 Mar 2024", 3)]
    [InlineData("Received: 12 Apr 2024", 4)]
    [InlineData("Received: 12 May 2024", 5)]
    [InlineData("Received: 12 Jun 2024", 6)]
    [InlineData("Received: 12 Jul 2024", 7)]
    [InlineData("Received: 12 Aug 2024", 8)]
    [InlineData("Received: 12 Sep 2024", 9)]
    [InlineData("Received: 12 Oct 2024", 10)]
    [InlineData("Received: 12 Nov 2024", 11)]
    [InlineData("Received: 12 Dec 2024", 12)]
    public void ParseReceived_AbbreviatedMonths_Parse(string input, int expectedMonth)
    {
        var result = HistDateParser.ParseReceived(input);
        Assert.NotNull(result);
        Assert.Equal(2024, result!.Year);
        Assert.Equal(expectedMonth, result.Month);
        Assert.Equal(12, result.Day);
    }

    // -- Each full English month --

    [Theory]
    [InlineData("Received: 12 January 2024", 1)]
    [InlineData("Received: 12 February 2024", 2)]
    [InlineData("Received: 12 March 2024", 3)]
    [InlineData("Received: 12 April 2024", 4)]
    [InlineData("Received: 12 May 2024", 5)]
    [InlineData("Received: 12 June 2024", 6)]
    [InlineData("Received: 12 July 2024", 7)]
    [InlineData("Received: 12 August 2024", 8)]
    [InlineData("Received: 12 September 2024", 9)]
    [InlineData("Received: 12 October 2024", 10)]
    [InlineData("Received: 12 November 2024", 11)]
    [InlineData("Received: 12 December 2024", 12)]
    public void ParseReceived_FullMonths_Parse(string input, int expectedMonth)
    {
        var result = HistDateParser.ParseReceived(input);
        Assert.NotNull(result);
        Assert.Equal(2024, result!.Year);
        Assert.Equal(expectedMonth, result.Month);
        Assert.Equal(12, result.Day);
    }

    // -- Day formats --

    [Fact]
    public void ParseReceived_TwoDigitDayWithLeadingZero_PreservesNumericValue()
    {
        var result = HistDateParser.ParseReceived("Received on 05 March 2024");
        Assert.NotNull(result);
        Assert.Equal(5, result!.Day);
        Assert.Equal(3, result.Month);
        Assert.Equal(2024, result.Year);
    }

    [Fact]
    public void ParseReceived_SingleDigitDay_Parses()
    {
        var result = HistDateParser.ParseReceived("Received: 4 November 2025");
        Assert.NotNull(result);
        Assert.Equal(4, result!.Day);
        Assert.Equal(11, result.Month);
        Assert.Equal(2025, result.Year);
    }

    [Fact]
    public void ParseReceived_AccessedOnShape_MonthDayYear_Parses()
    {
        // The original AccessedOnHandler shape: "Month Day, Year". This
        // shape does not appear in the [hist] paragraphs of the SciELO
        // before/ corpus, but the parser MUST recognize it because the
        // task requirements call for English month names (full and
        // abbreviated) and "mixed forms".
        var result = HistDateParser.ParseReceived("Received: March 12, 2024");
        Assert.NotNull(result);
        Assert.Equal(2024, result!.Year);
        Assert.Equal(3, result.Month);
        Assert.Equal(12, result.Day);
    }

    // -- Year-only --

    [Fact]
    public void ParsePublished_YearOnlyInsideHeader_ReturnsYearWithNullMonthAndDay()
    {
        var result = HistDateParser.ParsePublished("Published: 2024");
        Assert.NotNull(result);
        Assert.Equal(2024, result!.Year);
        Assert.Null(result.Month);
        Assert.Null(result.Day);
    }

    // -- Unrecognized / empty --

    [Theory]
    [InlineData("Hello world")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("12/03/2024")] // slashes not supported
    [InlineData("Received: 30 February 2024")] // invalid calendar date
    public void ParseReceived_UnrecognizedOrInvalid_ReturnsNull(string input)
    {
        Assert.Null(HistDateParser.ParseReceived(input));
    }

    [Theory]
    [InlineData("Hello world")]
    [InlineData("")]
    [InlineData("   ")]
    public void ParseAccepted_UnrecognizedOrEmpty_ReturnsNull(string input)
    {
        Assert.Null(HistDateParser.ParseAccepted(input));
    }

    [Theory]
    [InlineData("Hello world")]
    [InlineData("")]
    [InlineData("   ")]
    public void ParsePublished_UnrecognizedOrEmpty_ReturnsNull(string input)
    {
        Assert.Null(HistDateParser.ParsePublished(input));
    }

    [Fact]
    public void ParseReceived_HeaderMissing_ReturnsNull()
    {
        // The bare date phrase, without a "Received" header, must be
        // rejected by ParseReceived (each entry point requires its own
        // header word so the rule can dispatch unambiguously).
        Assert.Null(HistDateParser.ParseReceived("12 March 2024"));
    }

    // -- HistDate.ToDateIso --

    [Fact]
    public void ToDateIso_FullDate_ReturnsZeroPaddedYYYYMMDD()
    {
        var d = new HistDate(2024, 3, 12, "12 March 2024");
        Assert.Equal("20240312", d.ToDateIso());
    }

    [Fact]
    public void ToDateIso_MissingDay_PadsDayWith00()
    {
        var d = new HistDate(2024, 3, null, "March 2024");
        Assert.Equal("20240300", d.ToDateIso());
    }

    [Fact]
    public void ToDateIso_MissingMonthAndDay_PadsBothWith00()
    {
        var d = new HistDate(2024, null, null, "2024");
        Assert.Equal("20240000", d.ToDateIso());
    }

    [Fact]
    public void ToDateIso_SingleDigitDay_PadsToTwoDigits()
    {
        var d = new HistDate(2024, 12, 5, "5 December 2024");
        Assert.Equal("20241205", d.ToDateIso());
    }

    [Fact]
    public void ToDateIso_SingleDigitMonth_PadsToTwoDigits()
    {
        var d = new HistDate(2024, 1, 31, "31 January 2024");
        Assert.Equal("20240131", d.ToDateIso());
    }

    // -- Round-trip across the [hist] inventory of the after/ corpus --
    // Each datum here is the (header-stripped) date string that appears
    // wrapped inside [received|accepted|histdate] in
    // examples/phase-2/after/<id>.docx, paired with the dateiso the
    // corpus emits. The round-trip ensures that every corpus phrase
    // parses to a non-null HistDate AND that ToDateIso() reproduces the
    // exact dateiso the after/ corpus carries.
    [Theory]
    [InlineData("Received", "31 January 2025", "20250131")]
    [InlineData("Accepted", "01 July 2025", "20250701")]
    [InlineData("Published", "18 March 2025", "20250318")]
    [InlineData("Received", "20 May 2025", "20250520")]
    [InlineData("Accepted", "7 January 2026", "20260107")]
    [InlineData("Published", "24 February 2026", "20260224")]
    [InlineData("Received", "29 October 2025", "20251029")]
    [InlineData("Accepted", "07 January 2026", "20260107")]
    [InlineData("Published", "09 April 2026", "20260409")]
    [InlineData("Received", "10 October 2025", "20251010")]
    [InlineData("Accepted", "18 February 2026", "20260218")]
    [InlineData("Received", "02 October 2025", "20251002")]
    [InlineData("Accepted", "28 December 2025", "20251228")]
    [InlineData("Published", "20 February 2026", "20260220")]
    [InlineData("Received", "22 October 2025", "20251022")]
    [InlineData("Accepted", "9 February 2026", "20260209")]
    [InlineData("Received", "4 November 2025", "20251104")]
    [InlineData("Accepted", "28 January 2026", "20260128")]
    [InlineData("Received", "10 November 2025", "20251110")]
    [InlineData("Accepted", "18 March 2026", "20260318")]
    [InlineData("Received", "20 January 2026", "20260120")]
    [InlineData("Accepted", "13 March 2026", "20260313")]
    [InlineData("Published", "18 March 2026", "20260318")]
    [InlineData("Received", "18 February 2026", "20260218")]
    [InlineData("Accepted", "3 April 2026", "20260403")]
    [InlineData("Published", "11 April 2026", "20260411")]
    public void RoundTrip_AfterCorpusPhrases_ParseAndProduceMatchingDateIso(
        string header,
        string datePhrase,
        string expectedDateIso)
    {
        var input = $"{header}: {datePhrase}";
        var result = header switch
        {
            "Received" => HistDateParser.ParseReceived(input),
            "Accepted" => HistDateParser.ParseAccepted(input),
            "Published" => HistDateParser.ParsePublished(input),
            _ => null,
        };

        Assert.NotNull(result);
        Assert.Equal(expectedDateIso, result!.ToDateIso());
        Assert.Equal(datePhrase, result.SourceText);
    }
}
