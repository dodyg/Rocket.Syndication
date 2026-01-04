using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using SyndicationClient.DependencyInjection;
using SyndicationClient.Models;

namespace SyndicationClient.Tests.Integration;

[Category("Integration")]
public class LiveFeedTests
{
    private ISyndicationClient _client = null!;

    [Before(Test)]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSyndicationClient(options =>
        {
            options.Timeout = TimeSpan.FromSeconds(30);
        });

        var provider = services.BuildServiceProvider();
        _client = provider.GetRequiredService<ISyndicationClient>();
    }

    [Test]
    public async Task GetFeedAsync_BookRiot_ReturnsFeed(CancellationToken cancellationToken)
    {
        // https://bookriot.com/feed/
        var result = await _client.GetFeedAsync("https://bookriot.com/feed/", cancellationToken);

        result.IsSuccess.Should().BeTrue($"Expected success but got error: {result.Error?.Message}");
        result.Feed.Should().NotBeNull();
        result.Feed!.Title.Should().NotBeNullOrEmpty();
        result.Feed!.Items.Should().NotBeEmpty();
        result.Feed!.Type.Should().Be(FeedType.Rss);
    }

    [Test]
    public async Task GetFeedAsync_FeedburnerMaryse_ReturnsFeed(CancellationToken cancellationToken)
    {
        // https://feeds.feedburner.com/Maryse
        var result = await _client.GetFeedAsync("https://feeds.feedburner.com/Maryse", cancellationToken);

        result.IsSuccess.Should().BeTrue($"Expected success but got error: {result.Error?.Message}");
        result.Feed.Should().NotBeNull();
        result.Feed!.Title.Should().NotBeNullOrEmpty();
        result.Feed!.Items.Should().NotBeEmpty();
    }

    [Test]
    public async Task GetFeedAsync_ModernMrsDarcy_ReturnsFeed(CancellationToken cancellationToken)
    {
        // https://modernmrsdarcy.com/feed/
        var result = await _client.GetFeedAsync("https://modernmrsdarcy.com/feed/", cancellationToken);

        result.IsSuccess.Should().BeTrue($"Expected success but got error: {result.Error?.Message}");
        result.Feed.Should().NotBeNull();
        result.Feed!.Title.Should().NotBeNullOrEmpty();
        result.Feed!.Items.Should().NotBeEmpty();
    }

    [Test]
    public async Task GetFeedAsync_InvalidUrl_ReturnsError(CancellationToken cancellationToken)
    {
        var result = await _client.GetFeedAsync("https://this-domain-does-not-exist-12345.com/feed", cancellationToken);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Type.Should().Be(FeedErrorType.NetworkError);
    }

    [Test]
    public async Task GetFeedAsync_NotFoundUrl_ReturnsError(CancellationToken cancellationToken)
    {
        var result = await _client.GetFeedAsync("https://httpstat.us/404", cancellationToken);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Type.Should().Be(FeedErrorType.NotFound);
    }

    [Test]
    public async Task GetFeedAsync_FeedItems_HaveRequiredFields(CancellationToken cancellationToken)
    {
        var result = await _client.GetFeedAsync("https://bookriot.com/feed/", cancellationToken);

        result.IsSuccess.Should().BeTrue();

        foreach (var item in result.Feed!.Items)
        {
            item.Id.Should().NotBeNullOrEmpty("Every item should have an ID");
            // Title is optional in lenient mode but most feeds have it
            // Link and content are also typically present
        }
    }

    [Test]
    public async Task ParseFeedAsync_WithValidContent_ReturnsFeed(CancellationToken cancellationToken)
    {
        var content = @"<?xml version=""1.0"" encoding=""UTF-8""?>
            <rss version=""2.0"">
                <channel>
                    <title>Test Feed</title>
                    <link>https://example.com</link>
                    <description>Test</description>
                    <item>
                        <title>Test Item</title>
                        <link>https://example.com/1</link>
                    </item>
                </channel>
            </rss>";

        var result = await _client.ParseFeedAsync(content, cancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Feed.Should().NotBeNull();
        result.Feed!.Title.Should().Be("Test Feed");
        result.Feed!.Items.Should().HaveCount(1);
    }

    [Test]
    public async Task ParseFeedAsync_WithInvalidXml_ReturnsError(CancellationToken cancellationToken)
    {
        var content = "This is not valid XML";

        var result = await _client.ParseFeedAsync(content, cancellationToken);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Type.Should().Be(FeedErrorType.ParseError);
    }
}
