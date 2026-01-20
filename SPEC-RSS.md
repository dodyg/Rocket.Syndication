# SyndicationClient - Technical Specification

## Overview

A .NET class library for consuming RSS and Atom feeds with full specification compliance, modern async patterns, and enterprise-ready features.

## Project Structure

- **Type**: Class Library Only
- **Naming Convention**: `SyndicationClient.*`
  - Main library: `SyndicationClient`
  - Test project: `SyndicationClient.Tests`
- **NuGet Package**: Single package (`SyndicationClient`)
- **Target Framework**: .NET 10 only (to leverage C# 14 features)

## Specifications Compliance

- **RSS**: Full RSS 2.0 specification - https://www.rssboard.org/rss-specification
- **Atom**: Full Atom specification - https://www.ietf.org/rfc/rfc5023.txt

## API Design

### Core Interface Pattern

- **Pattern**: Interface + Dependency Injection
- **Style**: Async-only (all methods return `Task<T>`)
- **Cancellation**: `CancellationToken` required on all async methods

### Main Interfaces

```csharp
public interface ISyndicationClient
{
    Task<Feed> GetFeedAsync(string url, CancellationToken cancellationToken);
    Task<Feed> GetFeedAsync(Uri uri, CancellationToken cancellationToken);
    Task<Feed> ParseFeedAsync(string content, CancellationToken cancellationToken);
    Task<Feed> ParseFeedAsync(Stream stream, CancellationToken cancellationToken);
    Task<IReadOnlyList<FeedLink>> DiscoverFeedsAsync(string htmlUrl, CancellationToken cancellationToken);
}
```

### Input Sources

- URL fetching (primary use case)
- String parsing (for pre-fetched content)
- Stream parsing (for file or custom sources)

## HTTP Client Management

- **Implementation**: `IHttpClientFactory` for proper connection pooling and DI integration
- **User-Agent**: Default value (`SyndicationClient/1.0`) with configurable override
- **Timeouts**: 30-second default timeout, configurable
- **Redirects**: Follow automatically up to configurable limit (default: 5)
- **Encoding**: Auto-detect from XML declaration and HTTP headers

## Configuration

### Options Pattern

```csharp
public class SyndicationClientOptions
{
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
    public int MaxRedirects { get; set; } = 5;
    public string UserAgent { get; set; } = "SyndicationClient/1.0";
    public bool EnableCaching { get; set; } = true;
    public ValidationMode ValidationMode { get; set; } = ValidationMode.Lenient;
    public RetryPolicyOptions RetryPolicy { get; set; } = new();
}
```

- Use `IOptions<SyndicationClientOptions>` pattern
- Support configuration via `appsettings.json`
- DI registration extensions: `services.AddSyndicationClient(options => ...)`

## Authentication Support

Full authentication support including:
- HTTP Basic Authentication
- Bearer token authentication
- Custom headers (for API keys)
- Cookie-based authentication

```csharp
public class FeedCredentials
{
    public AuthenticationType Type { get; init; }
    public string? Username { get; init; }
    public string? Password { get; init; }
    public string? BearerToken { get; init; }
    public IDictionary<string, string>? CustomHeaders { get; init; }
}
```

## Caching

Full HTTP caching support:
- **ETags**: Store and send `If-None-Match` headers
- **Last-Modified**: Store and send `If-Modified-Since` headers
- **304 Responses**: Handle `Not Modified` responses efficiently
- **Cache Storage**: Pluggable cache interface with in-memory default

## Resilience (Polly Integration)

Built-in resilience policies using Polly:
- **Retry Policy**: Configurable retry count and backoff strategy
- **Circuit Breaker**: Prevent cascade failures
- **Timeout Policy**: Per-request timeout handling

```csharp
public class RetryPolicyOptions
{
    public int MaxRetries { get; set; } = 3;
    public TimeSpan InitialDelay { get; set; } = TimeSpan.FromSeconds(1);
    public BackoffType BackoffType { get; set; } = BackoffType.Exponential;
    public bool EnableCircuitBreaker { get; set; } = true;
    public int CircuitBreakerThreshold { get; set; } = 5;
}
```

## Data Models

### Immutability

- All models implemented as C# `record` types with `init`-only properties
- Thread-safe by design

### Unified Feed Model

Single `Feed` and `FeedItem` model that normalizes RSS and Atom:

```csharp
public record Feed
{
    public required string Title { get; init; }
    public string? Description { get; init; }
    public Uri? Link { get; init; }
    public DateTimeOffset? LastUpdated { get; init; }
    public string? Language { get; init; }
    public string? Copyright { get; init; }
    public FeedImage? Image { get; init; }
    public IReadOnlyList<FeedItem> Items { get; init; }
    public IReadOnlyList<FeedCategory> Categories { get; init; }
    public IReadOnlyList<FeedAuthor> Authors { get; init; }

    // Format-specific access
    public FeedType Type { get; init; }
    public RssFeedData? RssData { get; init; }
    public AtomFeedData? AtomData { get; init; }
}

public record FeedItem
{
    public required string Id { get; init; }
    public string? Title { get; init; }
    public Uri? Link { get; init; }
    public FeedContent? Content { get; init; }
    public DateTimeOffset? PublishedDate { get; init; }
    public DateTimeOffset? UpdatedDate { get; init; }
    public IReadOnlyList<FeedAuthor> Authors { get; init; }
    public IReadOnlyList<FeedCategory> Categories { get; init; }
    public IReadOnlyList<FeedEnclosure> Enclosures { get; init; }
    public FeedMediaContent? Media { get; init; }

    // Format-specific access
    public RssItemData? RssData { get; init; }
    public AtomEntryData? AtomData { get; init; }
}
```

### Content Handling

Provide both HTML and plain text versions:

```csharp
public record FeedContent
{
    public string? Html { get; init; }
    public string? PlainText { get; init; }  // Stripped version
    public string? ContentType { get; init; }
}
```

### Date Handling

- Use `DateTimeOffset` to preserve timezone information
- Handle various date formats commonly found in feeds
- Nullable for optional/missing dates

## Feed Type Detection

- **Auto-detection**: Automatically detect RSS vs Atom from XML content
- Examine root element and namespace to determine feed type
- No explicit specification required from caller

## Feed Discovery

Parse HTML pages to discover feed URLs:

```csharp
public record FeedLink
{
    public required Uri Url { get; init; }
    public string? Title { get; init; }
    public FeedType Type { get; init; }
}
```

Looks for:
- `<link rel="alternate" type="application/rss+xml">`
- `<link rel="alternate" type="application/atom+xml">`

## Error Handling

### Hybrid Approach

- **Result Pattern**: For expected failures (feed not found, parse errors, network issues)
- **Exceptions**: For unexpected/programmer errors (invalid arguments, configuration errors)

```csharp
public record FeedResult
{
    public bool IsSuccess { get; init; }
    public Feed? Feed { get; init; }
    public FeedError? Error { get; init; }
    public HttpResponseInfo? ResponseInfo { get; init; }
}

public record FeedError
{
    public FeedErrorType Type { get; init; }
    public string Message { get; init; }
    public Exception? InnerException { get; init; }
}

public enum FeedErrorType
{
    NetworkError,
    Timeout,
    NotFound,
    Unauthorized,
    ParseError,
    InvalidFeed,
    EncodingError
}
```

### Validation Mode

- **Lenient (Default)**: Parse what's available, use nulls/defaults for missing fields
- Support for strict mode via configuration if needed

## Built-in Extensions

Support for common RSS/Atom namespace extensions:

- **Dublin Core** (`dc:`): Creator, date, subject, etc.
- **Content** (`content:encoded`): Full article content
- **Media RSS** (`media:`): Thumbnails, content, player
- **iTunes Podcast** (`itunes:`): Author, duration, image, categories

## Extensibility

### Parser Pipeline

Allow registering custom parsers:

```csharp
public interface IFeedParser
{
    bool CanParse(XDocument document);
    Feed Parse(XDocument document);
}

// Registration
services.AddSyndicationClient(options =>
{
    options.Parsers.Add<CustomFeedParser>();
});
```

## Logging

- **Implementation**: `Microsoft.Extensions.Logging` integration
- **Configurable**: Log levels for different operations
- **Events Logged**:
  - Feed fetch start/complete
  - Cache hits/misses
  - Retry attempts
  - Parse errors
  - Authentication events

## Testing Requirements

### Framework

- **TUnit** (https://tunit.dev/)

### Test Categories

1. **Unit Tests**
   - Parser tests with mocked XML content
   - Model validation tests
   - Configuration tests
   - Error handling tests

2. **Integration Tests**
   - Real HTTP feed fetching
   - Test URLs:
     - https://bookriot.com/feed/
     - https://feeds.feedburner.com/Maryse
     - https://modernmrsdarcy.com/feed/

3. **Edge Case Tests**
   - Malformed feeds (missing required elements)
   - Large feeds (performance testing)
   - Timeout handling
   - Redirect chains
   - Various character encodings
   - Invalid XML handling
   - Empty feeds
   - Feeds with CDATA content
   - Date format variations

### Test Infrastructure

- Mock HTTP responses using `MockHttpMessageHandler`
- Sample feed files for deterministic testing
- Separate test settings for integration tests

## Dependencies

### Required NuGet Packages

- `Microsoft.Extensions.Http` - IHttpClientFactory
- `Microsoft.Extensions.Options` - Options pattern
- `Microsoft.Extensions.Logging.Abstractions` - Logging
- `Microsoft.Extensions.DependencyInjection.Abstractions` - DI
- `Polly.Extensions.Http` - Resilience policies
- `System.ServiceModel.Syndication` - XML parsing helpers (optional)

### Test Dependencies

- `TUnit` - Test framework
- `NSubstitute` or `Moq` - Mocking
- `FluentAssertions` - Assertion library

## Project Files Structure

```
SyndicationClient/
├── SyndicationClient.sln
├── src/
│   └── SyndicationClient/
│       ├── SyndicationClient.csproj
│       ├── ISyndicationClient.cs
│       ├── SyndicationClient.cs
│       ├── SyndicationClientOptions.cs
│       ├── Models/
│       │   ├── Feed.cs
│       │   ├── FeedItem.cs
│       │   ├── FeedContent.cs
│       │   ├── FeedAuthor.cs
│       │   ├── FeedCategory.cs
│       │   ├── FeedEnclosure.cs
│       │   ├── FeedImage.cs
│       │   ├── FeedLink.cs
│       │   ├── FeedResult.cs
│       │   └── Rss/
│       │   └── Atom/
│       ├── Parsing/
│       │   ├── IFeedParser.cs
│       │   ├── FeedParserPipeline.cs
│       │   ├── RssFeedParser.cs
│       │   ├── AtomFeedParser.cs
│       │   └── Extensions/
│       │       ├── DublinCoreParser.cs
│       │       ├── MediaRssParser.cs
│       │       └── ITunesParser.cs
│       ├── Http/
│       │   ├── FeedHttpClient.cs
│       │   └── CachingHandler.cs
│       ├── Caching/
│       │   ├── IFeedCache.cs
│       │   └── InMemoryFeedCache.cs
│       ├── Authentication/
│       │   └── FeedCredentials.cs
│       ├── Discovery/
│       │   └── FeedDiscoveryService.cs
│       └── DependencyInjection/
│           └── ServiceCollectionExtensions.cs
└── tests/
    └── SyndicationClient.Tests/
        ├── SyndicationClient.Tests.csproj
        ├── Unit/
        │   ├── Parsing/
        │   └── Models/
        ├── Integration/
        │   └── LiveFeedTests.cs
        └── TestData/
            ├── sample-rss.xml
            └── sample-atom.xml
```

## Build & Quality

- Run `dotnet format` after code changes
- Run `dotnet build` to verify compilation
- Run `dotnet test` to execute all tests
- Follow C# coding conventions: https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions
