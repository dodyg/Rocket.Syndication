using System.Xml.Linq;
using FluentAssertions;
using Rocket.Syndication.Parsing;

namespace SyndicationClient.Tests.Unit.Parsing;

public class ITunesParserTests
{
    private readonly RssFeedParser _parser = new();

    [Test]
    public void Parse_WithITunesFeed_ReturnsITunesData()
    {
        var xml = XDocument.Parse(@"
            <rss version=""2.0"" xmlns:itunes=""http://www.itunes.com/dtds/podcast-1.0.dtd"">
                <channel>
                    <title>Test Podcast</title>
                    <itunes:author>Podcast Host</itunes:author>
                    <itunes:subtitle>The best podcast</itunes:subtitle>
                    <itunes:summary>A longer summary</itunes:summary>
                    <itunes:explicit>no</itunes:explicit>
                    <itunes:image href=""https://example.com/art.jpg""/>
                </channel>
            </rss>");

        var feed = _parser.Parse(xml);

        feed.ITunes.Should().NotBeNull();
        feed.ITunes!.Author.Should().Be("Podcast Host");
        feed.ITunes!.Subtitle.Should().Be("The best podcast");
        feed.ITunes!.Summary.Should().Be("A longer summary");
        feed.ITunes!.Explicit.Should().BeFalse();
        feed.ITunes!.ImageUrl.Should().Be(new Uri("https://example.com/art.jpg"));
    }

    [Test]
    public void Parse_WithITunesOwner_ReturnsOwner()
    {
        var xml = XDocument.Parse(@"
            <rss version=""2.0"" xmlns:itunes=""http://www.itunes.com/dtds/podcast-1.0.dtd"">
                <channel>
                    <title>Test Podcast</title>
                    <itunes:owner>
                        <itunes:name>Owner Name</itunes:name>
                        <itunes:email>owner@example.com</itunes:email>
                    </itunes:owner>
                </channel>
            </rss>");

        var feed = _parser.Parse(xml);

        feed.ITunes.Should().NotBeNull();
        feed.ITunes!.Owner.Should().NotBeNull();
        feed.ITunes!.Owner!.Name.Should().Be("Owner Name");
        feed.ITunes!.Owner!.Email.Should().Be("owner@example.com");
    }

    [Test]
    public void Parse_WithITunesCategories_ReturnsCategories()
    {
        var xml = XDocument.Parse(@"
            <rss version=""2.0"" xmlns:itunes=""http://www.itunes.com/dtds/podcast-1.0.dtd"">
                <channel>
                    <title>Test Podcast</title>
                    <itunes:category text=""Technology"">
                        <itunes:category text=""Tech News""/>
                    </itunes:category>
                    <itunes:category text=""Education""/>
                </channel>
            </rss>");

        var feed = _parser.Parse(xml);

        feed.ITunes.Should().NotBeNull();
        feed.ITunes!.Categories.Should().HaveCount(2);
        feed.ITunes!.Categories[0].Text.Should().Be("Technology");
        feed.ITunes!.Categories[0].Subcategory.Should().NotBeNull();
        feed.ITunes!.Categories[0].Subcategory!.Text.Should().Be("Tech News");
        feed.ITunes!.Categories[1].Text.Should().Be("Education");
    }

    [Test]
    public void Parse_WithITunesEpisode_ReturnsEpisodeData()
    {
        var xml = XDocument.Parse(@"
            <rss version=""2.0"" xmlns:itunes=""http://www.itunes.com/dtds/podcast-1.0.dtd"">
                <channel>
                    <title>Test Podcast</title>
                    <item>
                        <title>Episode 1</title>
                        <itunes:duration>1:30:45</itunes:duration>
                        <itunes:episode>1</itunes:episode>
                        <itunes:season>1</itunes:season>
                        <itunes:episodeType>full</itunes:episodeType>
                    </item>
                </channel>
            </rss>");

        var feed = _parser.Parse(xml);
        var item = feed.Items[0];

        item.ITunes.Should().NotBeNull();
        item.ITunes!.Duration.Should().Be(5445); // 1*3600 + 30*60 + 45
        item.ITunes!.Episode.Should().Be(1);
        item.ITunes!.Season.Should().Be(1);
        item.ITunes!.EpisodeType.Should().Be("full");
    }

    [Test]
    [Arguments("yes", true)]
    [Arguments("true", true)]
    [Arguments("explicit", true)]
    [Arguments("no", false)]
    [Arguments("false", false)]
    [Arguments("clean", false)]
    public void Parse_WithExplicitValues_ParsesCorrectly(string value, bool expected)
    {
        var xml = XDocument.Parse($@"
            <rss version=""2.0"" xmlns:itunes=""http://www.itunes.com/dtds/podcast-1.0.dtd"">
                <channel>
                    <title>Test Podcast</title>
                    <itunes:explicit>{value}</itunes:explicit>
                </channel>
            </rss>");

        var feed = _parser.Parse(xml);

        feed.ITunes!.Explicit.Should().Be(expected);
    }

    [Test]
    [Arguments("30", 30)]
    [Arguments("5:30", 330)]
    [Arguments("1:30:45", 5445)]
    public void Parse_WithDurationFormats_ParsesCorrectly(string duration, int expectedSeconds)
    {
        var xml = XDocument.Parse($@"
            <rss version=""2.0"" xmlns:itunes=""http://www.itunes.com/dtds/podcast-1.0.dtd"">
                <channel>
                    <title>Test Podcast</title>
                    <item>
                        <title>Episode</title>
                        <itunes:duration>{duration}</itunes:duration>
                    </item>
                </channel>
            </rss>");

        var feed = _parser.Parse(xml);

        feed.Items[0].ITunes!.Duration.Should().Be(expectedSeconds);
    }

    [Test]
    public void Parse_WithITunesKeywords_ReturnsKeywords()
    {
        var xml = XDocument.Parse(@"
            <rss version=""2.0"" xmlns:itunes=""http://www.itunes.com/dtds/podcast-1.0.dtd"">
                <channel>
                    <title>Test Podcast</title>
                    <itunes:keywords>tech, news, programming, software</itunes:keywords>
                </channel>
            </rss>");

        var feed = _parser.Parse(xml);

        feed.ITunes!.Keywords.Should().HaveCount(4);
        feed.ITunes!.Keywords.Should().Contain("tech");
        feed.ITunes!.Keywords.Should().Contain("programming");
    }
}
