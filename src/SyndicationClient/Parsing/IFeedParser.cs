using System.Xml.Linq;
using SyndicationClient.Models;

namespace SyndicationClient.Parsing;

/// <summary>
/// Defines the contract for a feed parser.
/// </summary>
public interface IFeedParser
{
    /// <summary>
    /// Determines whether this parser can parse the given document.
    /// </summary>
    /// <param name="document">The XML document to check.</param>
    /// <returns>True if this parser can handle the document; otherwise, false.</returns>
    bool CanParse(XDocument document);

    /// <summary>
    /// Parses the XML document into a Feed.
    /// </summary>
    /// <param name="document">The XML document to parse.</param>
    /// <returns>The parsed feed.</returns>
    Feed Parse(XDocument document);
}
