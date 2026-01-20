using System.Xml.Linq;

namespace SyndicationClient.Parsing;

/// <summary>
/// Contains XML namespace definitions used in feed parsing.
/// </summary>
public static class XmlNamespaces
{
    public static readonly XNamespace Atom = "http://www.w3.org/2005/Atom";
    public static readonly XNamespace DublinCore = "http://purl.org/dc/elements/1.1/";
    public static readonly XNamespace Content = "http://purl.org/rss/1.0/modules/content/";
    public static readonly XNamespace Media = "http://search.yahoo.com/mrss/";
    public static readonly XNamespace ITunes = "http://www.itunes.com/dtds/podcast-1.0.dtd";
    public static readonly XNamespace Slash = "http://purl.org/rss/1.0/modules/slash/";
    public static readonly XNamespace Wfw = "http://wellformedweb.org/CommentAPI/";
    public static readonly XNamespace Sy = "http://purl.org/rss/1.0/modules/syndication/";
}
