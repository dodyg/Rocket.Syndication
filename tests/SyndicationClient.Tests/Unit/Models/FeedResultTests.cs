using System.Net;
using FluentAssertions;
using SyndicationClient.Models;

namespace SyndicationClient.Tests.Unit.Models;

public class FeedResultTests
{
    [Test]
    public void Success_CreatesFeedResult_WithIsSuccessTrue()
    {
        var feed = new Feed { Title = "Test" };

        var result = FeedResult.Success(feed);

        result.IsSuccess.Should().BeTrue();
        result.Feed.Should().Be(feed);
        result.Error.Should().BeNull();
    }

    [Test]
    public void Success_WithResponseInfo_IncludesResponseInfo()
    {
        var feed = new Feed { Title = "Test" };
        var responseInfo = new HttpResponseInfo
        {
            StatusCode = HttpStatusCode.OK,
            ETag = "\"abc123\""
        };

        var result = FeedResult.Success(feed, responseInfo);

        result.ResponseInfo.Should().Be(responseInfo);
    }

    [Test]
    public void Failure_CreatesFeedResult_WithIsSuccessFalse()
    {
        var error = FeedError.NotFound("Feed not found");

        var result = FeedResult.Failure(error);

        result.IsSuccess.Should().BeFalse();
        result.Feed.Should().BeNull();
        result.Error.Should().Be(error);
    }

    [Test]
    public void NotModified_CreatesFeedResult_WithCachedFeed()
    {
        var cachedFeed = new Feed { Title = "Cached" };
        var responseInfo = new HttpResponseInfo
        {
            StatusCode = HttpStatusCode.NotModified,
            WasNotModified = true
        };

        var result = FeedResult.NotModified(cachedFeed, responseInfo);

        result.IsSuccess.Should().BeTrue();
        result.Feed.Should().Be(cachedFeed);
        result.ResponseInfo!.WasNotModified.Should().BeTrue();
    }
}
