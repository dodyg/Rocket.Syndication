using System.Xml.Linq;
using FluentAssertions;
using SyndicationClient.Models;
using SyndicationClient.Parsing;

namespace SyndicationClient.Tests.Unit.Parsing;

public class AtomFeedParserTests
{
    private readonly AtomFeedParser _parser = new();

    [Test]
    public void CanParse_WithAtomRoot_ReturnsTrue()
    {
        var xml = XDocument.Parse("<feed xmlns=\"http://www.w3.org/2005/Atom\"></feed>");

        var result = _parser.CanParse(xml);

        result.Should().BeTrue();
    }

    [Test]
    public void CanParse_WithRssRoot_ReturnsFalse()
    {
        var xml = XDocument.Parse("<rss version=\"2.0\"><channel></channel></rss>");

        var result = _parser.CanParse(xml);

        result.Should().BeFalse();
    }

    [Test]
    public void Parse_WithBasicFeed_ReturnsCorrectTitle()
    {
        var xml = XDocument.Parse(@"
            <feed xmlns=""http://www.w3.org/2005/Atom"">
                <title>Test Atom Feed</title>
                <subtitle>Test Subtitle</subtitle>
                <link href=""https://example.com"" rel=""alternate""/>
                <id>urn:uuid:test-feed</id>
                <updated>2024-01-01T12:00:00Z</updated>
            </feed>");

        var feed = _parser.Parse(xml);

        feed.Title.Should().Be("Test Atom Feed");
        feed.Description.Should().Be("Test Subtitle");
        feed.Link.Should().Be(new Uri("https://example.com"));
        feed.Type.Should().Be(FeedType.Atom);
    }

    [Test]
    public void Parse_WithEntries_ReturnsCorrectEntryCount()
    {
        var xml = XDocument.Parse(@"
            <feed xmlns=""http://www.w3.org/2005/Atom"">
                <title>Test Feed</title>
                <id>urn:uuid:test</id>
                <updated>2024-01-01T12:00:00Z</updated>
                <entry>
                    <title>Entry 1</title>
                    <id>1</id>
                    <updated>2024-01-01T12:00:00Z</updated>
                </entry>
                <entry>
                    <title>Entry 2</title>
                    <id>2</id>
                    <updated>2024-01-01T12:00:00Z</updated>
                </entry>
            </feed>");

        var feed = _parser.Parse(xml);

        feed.Items.Should().HaveCount(2);
        feed.Items[0].Title.Should().Be("Entry 1");
        feed.Items[1].Title.Should().Be("Entry 2");
    }

    [Test]
    public void Parse_WithDates_ParsesDatesCorrectly()
    {
        var xml = XDocument.Parse(@"
            <feed xmlns=""http://www.w3.org/2005/Atom"">
                <title>Test Feed</title>
                <id>urn:uuid:test</id>
                <updated>2024-01-01T12:00:00Z</updated>
                <entry>
                    <title>Test Entry</title>
                    <id>1</id>
                    <updated>2024-01-01T10:00:00Z</updated>
                    <published>2024-01-01T09:00:00Z</published>
                </entry>
            </feed>");

        var feed = _parser.Parse(xml);

        feed.LastUpdated.Should().NotBeNull();
        feed.LastUpdated!.Value.Year.Should().Be(2024);

        feed.Items[0].UpdatedDate.Should().NotBeNull();
        feed.Items[0].PublishedDate.Should().NotBeNull();
    }

    [Test]
    public void Parse_WithAuthor_ReturnsAuthor()
    {
        var xml = XDocument.Parse(@"
            <feed xmlns=""http://www.w3.org/2005/Atom"">
                <title>Test Feed</title>
                <id>urn:uuid:test</id>
                <updated>2024-01-01T12:00:00Z</updated>
                <author>
                    <name>John Doe</name>
                    <email>john@example.com</email>
                    <uri>https://example.com/john</uri>
                </author>
            </feed>");

        var feed = _parser.Parse(xml);

        feed.Authors.Should().HaveCount(1);
        feed.Authors[0].Name.Should().Be("John Doe");
        feed.Authors[0].Email.Should().Be("john@example.com");
        feed.Authors[0].Uri.Should().Be(new Uri("https://example.com/john"));
    }

    [Test]
    public void Parse_WithCategories_ReturnsCategories()
    {
        var xml = XDocument.Parse(@"
            <feed xmlns=""http://www.w3.org/2005/Atom"">
                <title>Test Feed</title>
                <id>urn:uuid:test</id>
                <updated>2024-01-01T12:00:00Z</updated>
                <category term=""technology"" scheme=""https://example.com/cat"" label=""Technology""/>
                <category term=""news""/>
            </feed>");

        var feed = _parser.Parse(xml);

        feed.Categories.Should().HaveCount(2);
        feed.Categories[0].Name.Should().Be("technology");
        feed.Categories[0].Domain.Should().Be("https://example.com/cat");
        feed.Categories[0].Label.Should().Be("Technology");
    }

    [Test]
    public void Parse_WithHtmlContent_ReturnsContent()
    {
        var xml = XDocument.Parse(@"
            <feed xmlns=""http://www.w3.org/2005/Atom"">
                <title>Test Feed</title>
                <id>urn:uuid:test</id>
                <updated>2024-01-01T12:00:00Z</updated>
                <entry>
                    <title>Test Entry</title>
                    <id>1</id>
                    <updated>2024-01-01T12:00:00Z</updated>
                    <content type=""html""><![CDATA[<p>This is <strong>HTML</strong> content</p>]]></content>
                </entry>
            </feed>");

        var feed = _parser.Parse(xml);

        feed.Items[0].Content.Should().NotBeNull();
        feed.Items[0].Content!.Html.Should().Contain("<strong>HTML</strong>");
        feed.Items[0].Content!.PlainText.Should().Contain("This is HTML content");
    }

    [Test]
    public void Parse_WithLinks_ReturnsCorrectLinks()
    {
        var xml = XDocument.Parse(@"
            <feed xmlns=""http://www.w3.org/2005/Atom"">
                <title>Test Feed</title>
                <id>urn:uuid:test</id>
                <updated>2024-01-01T12:00:00Z</updated>
                <entry>
                    <title>Test Entry</title>
                    <id>1</id>
                    <updated>2024-01-01T12:00:00Z</updated>
                    <link href=""https://example.com/entry/1"" rel=""alternate""/>
                    <link href=""https://example.com/entry/1.pdf"" rel=""enclosure"" type=""application/pdf"" length=""12345""/>
                </entry>
            </feed>");

        var feed = _parser.Parse(xml);

        feed.Items[0].Link.Should().Be(new Uri("https://example.com/entry/1"));
        feed.Items[0].Enclosures.Should().HaveCount(1);
        feed.Items[0].Enclosures[0].Url.Should().Be(new Uri("https://example.com/entry/1.pdf"));
        feed.Items[0].Enclosures[0].MimeType.Should().Be("application/pdf");
    }

    [Test]
    public void Parse_WithGenerator_ReturnsAtomData()
    {
        var xml = XDocument.Parse(@"
            <feed xmlns=""http://www.w3.org/2005/Atom"">
                <title>Test Feed</title>
                <id>urn:uuid:test</id>
                <updated>2024-01-01T12:00:00Z</updated>
                <generator uri=""https://example.com"" version=""1.0"">Test Generator</generator>
            </feed>");

        var feed = _parser.Parse(xml);

        feed.AtomData.Should().NotBeNull();
        feed.AtomData!.Generator.Should().NotBeNull();
        feed.AtomData!.Generator!.Name.Should().Be("Test Generator");
        feed.AtomData!.Generator!.Uri.Should().Be(new Uri("https://example.com"));
        feed.AtomData!.Generator!.Version.Should().Be("1.0");
    }
}
