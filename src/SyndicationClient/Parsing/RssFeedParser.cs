using System.Xml.Linq;
using SyndicationClient.Models;
using SyndicationClient.Models.Extensions;
using SyndicationClient.Models.Rss;

namespace SyndicationClient.Parsing;

/// <summary>
/// Parser for RSS 2.0 feeds.
/// </summary>
public class RssFeedParser : IFeedParser
{
    /// <inheritdoc />
    public bool CanParse(XDocument document)
    {
        var root = document.Root;
        if (root == null) return false;

        // RSS 2.0: root is <rss> with version attribute
        if (root.Name.LocalName.Equals("rss", StringComparison.OrdinalIgnoreCase))
            return true;

        // RSS 1.0: root is <rdf:RDF> with RSS namespace
        if (root.Name.LocalName.Equals("RDF", StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }

    /// <inheritdoc />
    public Feed Parse(XDocument document)
    {
        var root = document.Root!;
        var channel = root.Element("channel") ?? root.Descendants().FirstOrDefault(e =>
            e.Name.LocalName.Equals("channel", StringComparison.OrdinalIgnoreCase));

        if (channel == null)
            throw new InvalidOperationException("RSS feed does not contain a channel element");

        var version = root.Attribute("version")?.Value;

        return new Feed
        {
            Title = GetElementValue(channel, "title") ?? "Untitled Feed",
            Description = GetElementValue(channel, "description"),
            Link = ParseUri(GetElementValue(channel, "link")),
            LastUpdated = DateParser.ParseRssDate(GetElementValue(channel, "lastBuildDate"))
                          ?? DateParser.ParseRssDate(GetElementValue(channel, "pubDate")),
            Language = GetElementValue(channel, "language"),
            Copyright = GetElementValue(channel, "copyright"),
            Image = ParseImage(channel),
            Items = ParseItems(channel).ToList(),
            Categories = ParseCategories(channel).ToList(),
            Authors = ParseAuthors(channel).ToList(),
            Type = FeedType.Rss,
            RssData = ParseRssData(channel, version),
            DublinCore = ParseDublinCore(channel),
            ITunes = ParseITunes(channel)
        };
    }

    private static string? GetElementValue(XElement parent, string localName)
    {
        return parent.Elements()
            .FirstOrDefault(e => e.Name.LocalName.Equals(localName, StringComparison.OrdinalIgnoreCase))
            ?.Value;
    }

    private static string? GetElementValue(XElement parent, XName name)
    {
        return parent.Element(name)?.Value;
    }

    private static Uri? ParseUri(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return Uri.TryCreate(value.Trim(), UriKind.Absolute, out var uri) ? uri : null;
    }

    private static FeedImage? ParseImage(XElement channel)
    {
        var image = channel.Element("image");
        if (image == null) return null;

        var url = ParseUri(GetElementValue(image, "url"));
        if (url == null) return null;

        return new FeedImage
        {
            Url = url,
            Title = GetElementValue(image, "title"),
            Link = ParseUri(GetElementValue(image, "link")),
            Width = int.TryParse(GetElementValue(image, "width"), out var w) ? w : null,
            Height = int.TryParse(GetElementValue(image, "height"), out var h) ? h : null,
            Description = GetElementValue(image, "description")
        };
    }

    private static IEnumerable<FeedItem> ParseItems(XElement channel)
    {
        var items = channel.Elements()
            .Where(e => e.Name.LocalName.Equals("item", StringComparison.OrdinalIgnoreCase));

        foreach (var item in items)
        {
            yield return ParseItem(item);
        }
    }

    private static FeedItem ParseItem(XElement item)
    {
        var guid = GetElementValue(item, "guid");
        var link = GetElementValue(item, "link");
        var id = guid ?? link ?? Guid.NewGuid().ToString();

        var description = GetElementValue(item, "description");
        var contentEncoded = GetElementValue(item, XmlNamespaces.Content + "encoded");
        var htmlContent = contentEncoded ?? description;

        return new FeedItem
        {
            Id = id,
            Title = GetElementValue(item, "title"),
            Link = ParseUri(link),
            Content = htmlContent != null ? new FeedContent
            {
                Html = htmlContent,
                PlainText = HtmlUtils.StripHtml(htmlContent),
                ContentType = "text/html"
            } : null,
            PublishedDate = DateParser.ParseRssDate(GetElementValue(item, "pubDate")),
            Authors = ParseItemAuthors(item).ToList(),
            Categories = ParseCategories(item).ToList(),
            Enclosures = ParseEnclosures(item).ToList(),
            Media = ParseMedia(item),
            RssData = ParseRssItemData(item),
            DublinCore = ParseDublinCore(item),
            ITunes = ParseItemITunes(item)
        };
    }

    private static IEnumerable<FeedAuthor> ParseAuthors(XElement element)
    {
        var author = GetElementValue(element, "author");
        if (!string.IsNullOrWhiteSpace(author))
        {
            yield return new FeedAuthor { Email = author };
        }

        var managingEditor = GetElementValue(element, "managingEditor");
        if (!string.IsNullOrWhiteSpace(managingEditor))
        {
            yield return new FeedAuthor { Email = managingEditor };
        }

        // Dublin Core creator
        var dcCreator = GetElementValue(element, XmlNamespaces.DublinCore + "creator");
        if (!string.IsNullOrWhiteSpace(dcCreator))
        {
            yield return new FeedAuthor { Name = dcCreator };
        }

        // iTunes author
        var itunesAuthor = GetElementValue(element, XmlNamespaces.ITunes + "author");
        if (!string.IsNullOrWhiteSpace(itunesAuthor))
        {
            yield return new FeedAuthor { Name = itunesAuthor };
        }
    }

    private static IEnumerable<FeedAuthor> ParseItemAuthors(XElement item)
    {
        var author = GetElementValue(item, "author");
        if (!string.IsNullOrWhiteSpace(author))
        {
            yield return new FeedAuthor { Email = author };
        }

        var dcCreator = GetElementValue(item, XmlNamespaces.DublinCore + "creator");
        if (!string.IsNullOrWhiteSpace(dcCreator))
        {
            yield return new FeedAuthor { Name = dcCreator };
        }
    }

    private static IEnumerable<FeedCategory> ParseCategories(XElement element)
    {
        var categories = element.Elements()
            .Where(e => e.Name.LocalName.Equals("category", StringComparison.OrdinalIgnoreCase));

        foreach (var cat in categories)
        {
            var name = cat.Value;
            if (!string.IsNullOrWhiteSpace(name))
            {
                yield return new FeedCategory
                {
                    Name = name.Trim(),
                    Domain = cat.Attribute("domain")?.Value
                };
            }
        }
    }

    private static IEnumerable<FeedEnclosure> ParseEnclosures(XElement item)
    {
        var enclosures = item.Elements()
            .Where(e => e.Name.LocalName.Equals("enclosure", StringComparison.OrdinalIgnoreCase));

        foreach (var enc in enclosures)
        {
            var url = ParseUri(enc.Attribute("url")?.Value);
            if (url != null)
            {
                yield return new FeedEnclosure
                {
                    Url = url,
                    MimeType = enc.Attribute("type")?.Value,
                    Length = long.TryParse(enc.Attribute("length")?.Value, out var len) ? len : null
                };
            }
        }
    }

    private static FeedMediaContent? ParseMedia(XElement item)
    {
        var mediaContent = item.Element(XmlNamespaces.Media + "content");
        var mediaThumbnail = item.Element(XmlNamespaces.Media + "thumbnail");
        var mediaGroup = item.Element(XmlNamespaces.Media + "group");

        if (mediaGroup != null)
        {
            mediaContent ??= mediaGroup.Element(XmlNamespaces.Media + "content");
            mediaThumbnail ??= mediaGroup.Element(XmlNamespaces.Media + "thumbnail");
        }

        if (mediaContent == null && mediaThumbnail == null) return null;

        return new FeedMediaContent
        {
            Url = ParseUri(mediaContent?.Attribute("url")?.Value),
            MimeType = mediaContent?.Attribute("type")?.Value,
            Medium = mediaContent?.Attribute("medium")?.Value,
            Width = int.TryParse(mediaContent?.Attribute("width")?.Value, out var w) ? w : null,
            Height = int.TryParse(mediaContent?.Attribute("height")?.Value, out var h) ? h : null,
            Duration = int.TryParse(mediaContent?.Attribute("duration")?.Value, out var d) ? d : null,
            ThumbnailUrl = ParseUri(mediaThumbnail?.Attribute("url")?.Value),
            Title = GetElementValue(item, XmlNamespaces.Media + "title"),
            Description = GetElementValue(item, XmlNamespaces.Media + "description")
        };
    }

    private static RssFeedData ParseRssData(XElement channel, string? version)
    {
        return new RssFeedData
        {
            Version = version,
            ManagingEditor = GetElementValue(channel, "managingEditor"),
            WebMaster = GetElementValue(channel, "webMaster"),
            Docs = ParseUri(GetElementValue(channel, "docs")),
            Cloud = ParseCloud(channel.Element("cloud")),
            Ttl = int.TryParse(GetElementValue(channel, "ttl"), out var ttl) ? ttl : null,
            Rating = GetElementValue(channel, "rating"),
            TextInput = ParseTextInput(channel.Element("textInput")),
            SkipHours = ParseSkipHours(channel.Element("skipHours")).ToList(),
            SkipDays = ParseSkipDays(channel.Element("skipDays")).ToList(),
            Generator = GetElementValue(channel, "generator")
        };
    }

    private static RssCloud? ParseCloud(XElement? cloud)
    {
        if (cloud == null) return null;

        return new RssCloud
        {
            Domain = cloud.Attribute("domain")?.Value,
            Port = int.TryParse(cloud.Attribute("port")?.Value, out var port) ? port : null,
            Path = cloud.Attribute("path")?.Value,
            RegisterProcedure = cloud.Attribute("registerProcedure")?.Value,
            Protocol = cloud.Attribute("protocol")?.Value
        };
    }

    private static RssTextInput? ParseTextInput(XElement? textInput)
    {
        if (textInput == null) return null;

        return new RssTextInput
        {
            Title = GetElementValue(textInput, "title"),
            Description = GetElementValue(textInput, "description"),
            Name = GetElementValue(textInput, "name"),
            Link = ParseUri(GetElementValue(textInput, "link"))
        };
    }

    private static IEnumerable<int> ParseSkipHours(XElement? skipHours)
    {
        if (skipHours == null) yield break;

        foreach (var hour in skipHours.Elements("hour"))
        {
            if (int.TryParse(hour.Value, out var h))
                yield return h;
        }
    }

    private static IEnumerable<string> ParseSkipDays(XElement? skipDays)
    {
        if (skipDays == null) yield break;

        foreach (var day in skipDays.Elements("day"))
        {
            if (!string.IsNullOrWhiteSpace(day.Value))
                yield return day.Value.Trim();
        }
    }

    private static RssItemData ParseRssItemData(XElement item)
    {
        var guidElement = item.Element("guid");
        return new RssItemData
        {
            Guid = guidElement?.Value,
            IsPermaLink = guidElement?.Attribute("isPermaLink")?.Value?.Equals("true", StringComparison.OrdinalIgnoreCase),
            Comments = ParseUri(GetElementValue(item, "comments")),
            Source = ParseSource(item.Element("source"))
        };
    }

    private static RssSource? ParseSource(XElement? source)
    {
        if (source == null) return null;

        return new RssSource
        {
            Name = source.Value,
            Url = ParseUri(source.Attribute("url")?.Value)
        };
    }

    private static DublinCoreData? ParseDublinCore(XElement element)
    {
        var dc = XmlNamespaces.DublinCore;

        var hasAnyValue = element.Elements()
            .Any(e => e.Name.Namespace == dc);

        if (!hasAnyValue) return null;

        return new DublinCoreData
        {
            Creator = GetElementValue(element, dc + "creator"),
            Date = DateParser.ParseAtomDate(GetElementValue(element, dc + "date")),
            Subject = GetElementValue(element, dc + "subject"),
            Description = GetElementValue(element, dc + "description"),
            Publisher = GetElementValue(element, dc + "publisher"),
            Contributor = GetElementValue(element, dc + "contributor"),
            Type = GetElementValue(element, dc + "type"),
            Format = GetElementValue(element, dc + "format"),
            Identifier = GetElementValue(element, dc + "identifier"),
            Source = GetElementValue(element, dc + "source"),
            Language = GetElementValue(element, dc + "language"),
            Relation = GetElementValue(element, dc + "relation"),
            Coverage = GetElementValue(element, dc + "coverage"),
            Rights = GetElementValue(element, dc + "rights")
        };
    }

    private static ITunesData? ParseITunes(XElement element)
    {
        var itunes = XmlNamespaces.ITunes;

        var hasAnyValue = element.Elements()
            .Any(e => e.Name.Namespace == itunes);

        if (!hasAnyValue) return null;

        return new ITunesData
        {
            Author = GetElementValue(element, itunes + "author"),
            Subtitle = GetElementValue(element, itunes + "subtitle"),
            Summary = GetElementValue(element, itunes + "summary"),
            ImageUrl = ParseUri(element.Element(itunes + "image")?.Attribute("href")?.Value),
            Explicit = ParseExplicit(GetElementValue(element, itunes + "explicit")),
            Owner = ParseOwner(element.Element(itunes + "owner")),
            Categories = ParseITunesCategories(element).ToList(),
            Keywords = ParseKeywords(GetElementValue(element, itunes + "keywords")),
            Block = GetElementValue(element, itunes + "block")?.Equals("yes", StringComparison.OrdinalIgnoreCase),
            Complete = GetElementValue(element, itunes + "complete")?.Equals("yes", StringComparison.OrdinalIgnoreCase)
        };
    }

    private static ITunesData? ParseItemITunes(XElement item)
    {
        var itunes = XmlNamespaces.ITunes;

        var hasAnyValue = item.Elements()
            .Any(e => e.Name.Namespace == itunes);

        if (!hasAnyValue) return null;

        return new ITunesData
        {
            Author = GetElementValue(item, itunes + "author"),
            Subtitle = GetElementValue(item, itunes + "subtitle"),
            Summary = GetElementValue(item, itunes + "summary"),
            ImageUrl = ParseUri(item.Element(itunes + "image")?.Attribute("href")?.Value),
            Duration = ParseDuration(GetElementValue(item, itunes + "duration")),
            Explicit = ParseExplicit(GetElementValue(item, itunes + "explicit")),
            Episode = int.TryParse(GetElementValue(item, itunes + "episode"), out var ep) ? ep : null,
            Season = int.TryParse(GetElementValue(item, itunes + "season"), out var s) ? s : null,
            EpisodeType = GetElementValue(item, itunes + "episodeType"),
            Block = GetElementValue(item, itunes + "block")?.Equals("yes", StringComparison.OrdinalIgnoreCase)
        };
    }

    private static bool? ParseExplicit(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        value = value.Trim().ToLowerInvariant();
        return value switch
        {
            "yes" or "true" or "explicit" => true,
            "no" or "false" or "clean" => false,
            _ => null
        };
    }

    private static ITunesOwner? ParseOwner(XElement? owner)
    {
        if (owner == null) return null;

        return new ITunesOwner
        {
            Name = GetElementValue(owner, XmlNamespaces.ITunes + "name"),
            Email = GetElementValue(owner, XmlNamespaces.ITunes + "email")
        };
    }

    private static IEnumerable<ITunesCategory> ParseITunesCategories(XElement element)
    {
        var categories = element.Elements(XmlNamespaces.ITunes + "category");

        foreach (var cat in categories)
        {
            var text = cat.Attribute("text")?.Value;
            if (!string.IsNullOrWhiteSpace(text))
            {
                var subcat = cat.Element(XmlNamespaces.ITunes + "category");
                yield return new ITunesCategory
                {
                    Text = text,
                    Subcategory = subcat != null ? new ITunesCategory
                    {
                        Text = subcat.Attribute("text")?.Value ?? ""
                    } : null
                };
            }
        }
    }

    private static IReadOnlyList<string> ParseKeywords(string? keywords)
    {
        if (string.IsNullOrWhiteSpace(keywords)) return [];

        return keywords.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
    }

    private static int? ParseDuration(string? duration)
    {
        if (string.IsNullOrWhiteSpace(duration)) return null;

        var parts = duration.Split(':');
        return parts.Length switch
        {
            1 => int.TryParse(parts[0], out var sec) ? sec : null,
            2 => int.TryParse(parts[0], out var min) && int.TryParse(parts[1], out var sec2)
                ? min * 60 + sec2 : null,
            3 => int.TryParse(parts[0], out var hr) && int.TryParse(parts[1], out var min2)
                 && int.TryParse(parts[2], out var sec3)
                ? hr * 3600 + min2 * 60 + sec3 : null,
            _ => null
        };
    }
}
