using System.Xml.Linq;
using FluentAssertions;
using SyndicationClient.Models;
using SyndicationClient.Parsing;

namespace SyndicationClient.Tests.Unit.Parsing;

public class RssFeedParserTests
{
    private readonly RssFeedParser _parser = new();

    [Test]
    public void CanParse_WithRssRoot_ReturnsTrue()
    {
        var xml = XDocument.Parse("<rss version=\"2.0\"><channel></channel></rss>");

        var result = _parser.CanParse(xml);

        result.Should().BeTrue();
    }

    [Test]
    public void CanParse_WithAtomRoot_ReturnsFalse()
    {
        var xml = XDocument.Parse("<feed xmlns=\"http://www.w3.org/2005/Atom\"></feed>");

        var result = _parser.CanParse(xml);

        result.Should().BeFalse();
    }

    [Test]
    public void Parse_WithBasicFeed_ReturnsCorrectTitle()
    {
        var xml = XDocument.Parse(@"
            <rss version=""2.0"">
                <channel>
                    <title>Test Feed</title>
                    <link>https://example.com</link>
                    <description>Test Description</description>
                </channel>
            </rss>");

        var feed = _parser.Parse(xml);

        feed.Title.Should().Be("Test Feed");
        feed.Description.Should().Be("Test Description");
        feed.Link.Should().Be(new Uri("https://example.com"));
        feed.Type.Should().Be(FeedType.Rss);
    }

    [Test]
    public void Parse_WithItems_ReturnsCorrectItemCount()
    {
        var xml = XDocument.Parse(@"
            <rss version=""2.0"">
                <channel>
                    <title>Test Feed</title>
                    <item><title>Item 1</title></item>
                    <item><title>Item 2</title></item>
                    <item><title>Item 3</title></item>
                </channel>
            </rss>");

        var feed = _parser.Parse(xml);

        feed.Items.Should().HaveCount(3);
        feed.Items[0].Title.Should().Be("Item 1");
        feed.Items[1].Title.Should().Be("Item 2");
        feed.Items[2].Title.Should().Be("Item 3");
    }

    [Test]
    public void Parse_WithPubDate_ParsesDateCorrectly()
    {
        var xml = XDocument.Parse(@"
            <rss version=""2.0"">
                <channel>
                    <title>Test Feed</title>
                    <item>
                        <title>Test Item</title>
                        <pubDate>Mon, 01 Jan 2024 12:00:00 GMT</pubDate>
                    </item>
                </channel>
            </rss>");

        var feed = _parser.Parse(xml);

        feed.Items[0].PublishedDate.Should().NotBeNull();
        feed.Items[0].PublishedDate!.Value.Year.Should().Be(2024);
        feed.Items[0].PublishedDate!.Value.Month.Should().Be(1);
        feed.Items[0].PublishedDate!.Value.Day.Should().Be(1);
    }

    [Test]
    public void Parse_WithCategories_ReturnsCategories()
    {
        var xml = XDocument.Parse(@"
            <rss version=""2.0"">
                <channel>
                    <title>Test Feed</title>
                    <category>Tech</category>
                    <category domain=""topics"">Programming</category>
                </channel>
            </rss>");

        var feed = _parser.Parse(xml);

        feed.Categories.Should().HaveCount(2);
        feed.Categories[0].Name.Should().Be("Tech");
        feed.Categories[1].Name.Should().Be("Programming");
        feed.Categories[1].Domain.Should().Be("topics");
    }

    [Test]
    public void Parse_WithEnclosure_ReturnsEnclosure()
    {
        var xml = XDocument.Parse(@"
            <rss version=""2.0"">
                <channel>
                    <title>Test Feed</title>
                    <item>
                        <title>Test Item</title>
                        <enclosure url=""https://example.com/audio.mp3"" type=""audio/mpeg"" length=""12345""/>
                    </item>
                </channel>
            </rss>");

        var feed = _parser.Parse(xml);

        feed.Items[0].Enclosures.Should().HaveCount(1);
        feed.Items[0].Enclosures[0].Url.Should().Be(new Uri("https://example.com/audio.mp3"));
        feed.Items[0].Enclosures[0].MimeType.Should().Be("audio/mpeg");
        feed.Items[0].Enclosures[0].Length.Should().Be(12345);
    }

    [Test]
    public void Parse_WithContentEncoded_ReturnsFullContent()
    {
        var xml = XDocument.Parse(@"
            <rss version=""2.0"" xmlns:content=""http://purl.org/rss/1.0/modules/content/"">
                <channel>
                    <title>Test Feed</title>
                    <item>
                        <title>Test Item</title>
                        <description>Short description</description>
                        <content:encoded><![CDATA[<p>Full <strong>content</strong> here</p>]]></content:encoded>
                    </item>
                </channel>
            </rss>");

        var feed = _parser.Parse(xml);

        feed.Items[0].Content.Should().NotBeNull();
        feed.Items[0].Content!.Html.Should().Contain("<strong>content</strong>");
        feed.Items[0].Content!.PlainText.Should().Contain("Full content here");
    }

    [Test]
    public void Parse_WithImage_ReturnsImage()
    {
        var xml = XDocument.Parse(@"
            <rss version=""2.0"">
                <channel>
                    <title>Test Feed</title>
                    <image>
                        <url>https://example.com/logo.png</url>
                        <title>Logo</title>
                        <link>https://example.com</link>
                        <width>144</width>
                        <height>144</height>
                    </image>
                </channel>
            </rss>");

        var feed = _parser.Parse(xml);

        feed.Image.Should().NotBeNull();
        feed.Image!.Url.Should().Be(new Uri("https://example.com/logo.png"));
        feed.Image!.Title.Should().Be("Logo");
        feed.Image!.Width.Should().Be(144);
        feed.Image!.Height.Should().Be(144);
    }

    [Test]
    public void Parse_WithDublinCore_ReturnsDublinCoreData()
    {
        var xml = XDocument.Parse(@"
            <rss version=""2.0"" xmlns:dc=""http://purl.org/dc/elements/1.1/"">
                <channel>
                    <title>Test Feed</title>
                    <item>
                        <title>Test Item</title>
                        <dc:creator>John Doe</dc:creator>
                    </item>
                </channel>
            </rss>");

        var feed = _parser.Parse(xml);

        feed.Items[0].DublinCore.Should().NotBeNull();
        feed.Items[0].DublinCore!.Creator.Should().Be("John Doe");
        feed.Items[0].Authors.Should().Contain(a => a.Name == "John Doe");
    }

    [Test]
    public void Parse_WithGuid_SetsCorrectId()
    {
        var xml = XDocument.Parse(@"
            <rss version=""2.0"">
                <channel>
                    <title>Test Feed</title>
                    <item>
                        <title>Test Item</title>
                        <guid isPermaLink=""true"">https://example.com/article/1</guid>
                    </item>
                </channel>
            </rss>");

        var feed = _parser.Parse(xml);

        feed.Items[0].Id.Should().Be("https://example.com/article/1");
        feed.Items[0].RssData!.Guid.Should().Be("https://example.com/article/1");
        feed.Items[0].RssData!.IsPermaLink.Should().BeTrue();
    }
}
