using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Rocket.Syndication.Models;
using Rocket.Syndication.Parsing;

namespace SyndicationClient.Tests.Unit.Parsing;

public class FeedParserPipelineTests
{
    private readonly FeedParserPipeline _pipeline;

    public FeedParserPipelineTests()
    {
        var parsers = new IFeedParser[]
        {
            new RssFeedParser(),
            new AtomFeedParser()
        };
        _pipeline = new FeedParserPipeline(parsers, NullLogger<FeedParserPipeline>.Instance);
    }

    [Test]
    public void Parse_WithRssFeed_UsesRssParser()
    {
        var content = @"<?xml version=""1.0""?>
            <rss version=""2.0"">
                <channel>
                    <title>RSS Feed</title>
                </channel>
            </rss>";

        var result = _pipeline.Parse(content);

        result.IsSuccess.Should().BeTrue();
        result.Feed!.Type.Should().Be(FeedType.Rss);
    }

    [Test]
    public void Parse_WithAtomFeed_UsesAtomParser()
    {
        var content = @"<?xml version=""1.0""?>
            <feed xmlns=""http://www.w3.org/2005/Atom"">
                <title>Atom Feed</title>
                <id>urn:test</id>
                <updated>2024-01-01T00:00:00Z</updated>
            </feed>";

        var result = _pipeline.Parse(content);

        result.IsSuccess.Should().BeTrue();
        result.Feed!.Type.Should().Be(FeedType.Atom);
    }

    [Test]
    public void Parse_WithInvalidXml_ReturnsError()
    {
        var content = "Not valid XML at all!";

        var result = _pipeline.Parse(content);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Type.Should().Be(FeedErrorType.ParseError);
    }

    [Test]
    public void Parse_WithUnknownFormat_ReturnsError()
    {
        var content = @"<?xml version=""1.0""?>
            <unknown>
                <something>This is not a feed</something>
            </unknown>";

        var result = _pipeline.Parse(content);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Type.Should().Be(FeedErrorType.InvalidFeed);
    }

    [Test]
    public async Task ParseAsync_WithStream_ReturnsFeed()
    {
        var content = @"<?xml version=""1.0""?>
            <rss version=""2.0"">
                <channel>
                    <title>Stream Feed</title>
                </channel>
            </rss>";

        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));

        var result = await _pipeline.ParseAsync(stream, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Feed!.Title.Should().Be("Stream Feed");
    }
}
