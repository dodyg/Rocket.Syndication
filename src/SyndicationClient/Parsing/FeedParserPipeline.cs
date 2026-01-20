using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using SyndicationClient.Models;

namespace SyndicationClient.Parsing;

/// <summary>
/// Manages a pipeline of feed parsers and selects the appropriate one for a document.
/// </summary>
public class FeedParserPipeline
{
    private readonly IReadOnlyList<IFeedParser> _parsers;
    private readonly ILogger<FeedParserPipeline> _logger;

    public FeedParserPipeline(IEnumerable<IFeedParser> parsers, ILogger<FeedParserPipeline> logger)
    {
        _parsers = parsers.ToList();
        _logger = logger;
    }

    /// <summary>
    /// Parses the given XML content into a Feed.
    /// </summary>
    /// <param name="content">The XML content to parse.</param>
    /// <returns>The result of the parsing operation.</returns>
    public FeedResult Parse(string content)
    {
        try
        {
            var document = XDocument.Parse(content);
            return Parse(document);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse XML content");
            return FeedResult.Failure(FeedError.Parse("Failed to parse XML content", ex));
        }
    }

    /// <summary>
    /// Parses the given XML document into a Feed.
    /// </summary>
    /// <param name="document">The XML document to parse.</param>
    /// <returns>The result of the parsing operation.</returns>
    public FeedResult Parse(XDocument document)
    {
        foreach (var parser in _parsers)
        {
            if (parser.CanParse(document))
            {
                _logger.LogDebug("Using parser {ParserType} for document", parser.GetType().Name);
                try
                {
                    var feed = parser.Parse(document);
                    return FeedResult.Success(feed);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Parser {ParserType} failed to parse document", parser.GetType().Name);
                    return FeedResult.Failure(FeedError.Parse($"Failed to parse feed: {ex.Message}", ex));
                }
            }
        }

        _logger.LogWarning("No parser found for document with root element: {RootElement}",
            document.Root?.Name.LocalName);
        return FeedResult.Failure(FeedError.InvalidFeed("Unable to determine feed type. No suitable parser found."));
    }

    /// <summary>
    /// Parses the given stream into a Feed.
    /// </summary>
    /// <param name="stream">The stream containing XML content.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the parsing operation.</returns>
    public async Task<FeedResult> ParseAsync(Stream stream, CancellationToken cancellationToken)
    {
        try
        {
            var document = await XDocument.LoadAsync(stream, LoadOptions.None, cancellationToken);
            return Parse(document);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse stream content");
            return FeedResult.Failure(FeedError.Parse("Failed to parse stream content", ex));
        }
    }
}
