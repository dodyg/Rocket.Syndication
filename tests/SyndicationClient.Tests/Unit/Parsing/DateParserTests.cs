using FluentAssertions;
using Rocket.Syndication.Parsing;

namespace SyndicationClient.Tests.Unit.Parsing;

public class DateParserTests
{
    [Test]
    public void ParseRssDate_WithStandardFormat_ReturnsCorrectDate()
    {
        var dateString = "Mon, 01 Jan 2024 12:00:00 GMT";

        var result = DateParser.ParseRssDate(dateString);

        result.Should().NotBeNull();
        result!.Value.Year.Should().Be(2024);
        result!.Value.Month.Should().Be(1);
        result!.Value.Day.Should().Be(1);
        result!.Value.Hour.Should().Be(12);
    }

    [Test]
    public void ParseRssDate_WithTimezone_ReturnsCorrectDate()
    {
        var dateString = "Mon, 01 Jan 2024 12:00:00 -0500";

        var result = DateParser.ParseRssDate(dateString);

        result.Should().NotBeNull();
        result!.Value.Offset.Should().Be(TimeSpan.FromHours(-5));
    }

    [Test]
    public void ParseRssDate_WithNullOrEmpty_ReturnsNull()
    {
        DateParser.ParseRssDate(null).Should().BeNull();
        DateParser.ParseRssDate("").Should().BeNull();
        DateParser.ParseRssDate("   ").Should().BeNull();
    }

    [Test]
    public void ParseRssDate_WithSingleDigitDay_ReturnsCorrectDate()
    {
        var dateString = "Mon, 1 Jan 2024 12:00:00 GMT";

        var result = DateParser.ParseRssDate(dateString);

        result.Should().NotBeNull();
        result!.Value.Day.Should().Be(1);
    }

    [Test]
    public void ParseAtomDate_WithIso8601Format_ReturnsCorrectDate()
    {
        var dateString = "2024-01-01T12:00:00Z";

        var result = DateParser.ParseAtomDate(dateString);

        result.Should().NotBeNull();
        result!.Value.Year.Should().Be(2024);
        result!.Value.Month.Should().Be(1);
        result!.Value.Day.Should().Be(1);
        result!.Value.Hour.Should().Be(12);
    }

    [Test]
    public void ParseAtomDate_WithTimezoneOffset_ReturnsCorrectDate()
    {
        var dateString = "2024-01-01T12:00:00+05:00";

        var result = DateParser.ParseAtomDate(dateString);

        result.Should().NotBeNull();
        result!.Value.Offset.Should().Be(TimeSpan.FromHours(5));
    }

    [Test]
    public void ParseAtomDate_WithMilliseconds_ReturnsCorrectDate()
    {
        var dateString = "2024-01-01T12:00:00.123Z";

        var result = DateParser.ParseAtomDate(dateString);

        result.Should().NotBeNull();
        result!.Value.Millisecond.Should().Be(123);
    }

    [Test]
    public void ParseAtomDate_WithDateOnly_ReturnsCorrectDate()
    {
        var dateString = "2024-01-01";

        var result = DateParser.ParseAtomDate(dateString);

        result.Should().NotBeNull();
        result!.Value.Year.Should().Be(2024);
        result!.Value.Month.Should().Be(1);
        result!.Value.Day.Should().Be(1);
    }
}
