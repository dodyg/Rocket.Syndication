using System.Xml.Linq;
using SyndicationClient.Models;
using SyndicationClient.Models.Atom;
using SyndicationClient.Models.Extensions;

namespace SyndicationClient.Parsing;

/// <summary>
/// Parser for Atom feeds.
/// </summary>
public class AtomFeedParser : IFeedParser
{
    /// <inheritdoc />
    public bool CanParse(XDocument document)
    {
        var root = document.Root;
        if (root == null) return false;

        // Check for Atom namespace
        if (root.Name.Namespace == XmlNamespaces.Atom)
            return true;

        // Check for feed element with atom namespace declaration
        if (root.Name.LocalName.Equals("feed", StringComparison.OrdinalIgnoreCase))
        {
            var ns = root.GetDefaultNamespace();
            return ns.NamespaceName.Contains("atom", StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    /// <inheritdoc />
    public Feed Parse(XDocument document)
    {
        var root = document.Root!;
        var ns = root.GetDefaultNamespace();

        return new Feed
        {
            Title = GetTextValue(root, ns + "title") ?? "Untitled Feed",
            Description = GetTextValue(root, ns + "subtitle"),
            Link = GetAlternateLink(root, ns),
            LastUpdated = DateParser.ParseAtomDate(GetElementValue(root, ns + "updated")),
            Language = root.Attribute(XNamespace.Xml + "lang")?.Value,
            Copyright = GetTextValue(root, ns + "rights"),
            Image = ParseLogo(root, ns),
            Items = ParseEntries(root, ns).ToList(),
            Categories = ParseCategories(root, ns).ToList(),
            Authors = ParsePersons(root, ns, "author").ToList(),
            Type = FeedType.Atom,
            AtomData = ParseAtomData(root, ns),
            DublinCore = ParseDublinCore(root),
            ITunes = ParseITunes(root)
        };
    }

    private static string? GetElementValue(XElement parent, XName name)
    {
        return parent.Element(name)?.Value;
    }

    private static string? GetTextValue(XElement parent, XName name)
    {
        var element = parent.Element(name);
        if (element == null) return null;

        var type = element.Attribute("type")?.Value?.ToLowerInvariant() ?? "text";

        return type switch
        {
            "html" or "xhtml" => element.Value,
            _ => element.Value
        };
    }

    private static Uri? ParseUri(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return Uri.TryCreate(value.Trim(), UriKind.Absolute, out var uri) ? uri : null;
    }

    private static Uri? GetAlternateLink(XElement element, XNamespace ns)
    {
        var links = element.Elements(ns + "link");

        // First try to find alternate link
        var alternate = links.FirstOrDefault(l =>
            l.Attribute("rel")?.Value?.Equals("alternate", StringComparison.OrdinalIgnoreCase) == true);

        // Fall back to link without rel or with empty rel
        alternate ??= links.FirstOrDefault(l =>
            l.Attribute("rel") == null || string.IsNullOrEmpty(l.Attribute("rel")?.Value));

        return ParseUri(alternate?.Attribute("href")?.Value);
    }

    private static FeedImage? ParseLogo(XElement root, XNamespace ns)
    {
        var logo = GetElementValue(root, ns + "logo");
        var icon = GetElementValue(root, ns + "icon");

        var url = ParseUri(logo) ?? ParseUri(icon);
        if (url == null) return null;

        return new FeedImage
        {
            Url = url,
            Title = GetTextValue(root, ns + "title")
        };
    }

    private static IEnumerable<FeedItem> ParseEntries(XElement root, XNamespace ns)
    {
        var entries = root.Elements(ns + "entry");

        foreach (var entry in entries)
        {
            yield return ParseEntry(entry, ns);
        }
    }

    private static FeedItem ParseEntry(XElement entry, XNamespace ns)
    {
        var id = GetElementValue(entry, ns + "id") ?? Guid.NewGuid().ToString();

        var content = ParseContent(entry, ns);

        return new FeedItem
        {
            Id = id,
            Title = GetTextValue(entry, ns + "title"),
            Link = GetAlternateLink(entry, ns),
            Content = content,
            PublishedDate = DateParser.ParseAtomDate(GetElementValue(entry, ns + "published")),
            UpdatedDate = DateParser.ParseAtomDate(GetElementValue(entry, ns + "updated")),
            Authors = ParsePersons(entry, ns, "author").ToList(),
            Categories = ParseCategories(entry, ns).ToList(),
            Enclosures = ParseEnclosures(entry, ns).ToList(),
            Media = ParseMedia(entry),
            AtomData = ParseAtomEntryData(entry, ns),
            DublinCore = ParseDublinCore(entry),
            ITunes = ParseItemITunes(entry)
        };
    }

    private static FeedContent? ParseContent(XElement entry, XNamespace ns)
    {
        var contentElement = entry.Element(ns + "content");
        var summaryElement = entry.Element(ns + "summary");

        var element = contentElement ?? summaryElement;
        if (element == null) return null;

        var type = element.Attribute("type")?.Value?.ToLowerInvariant() ?? "text";
        var value = element.Value;

        // Handle xhtml content
        if (type == "xhtml")
        {
            var div = element.Elements().FirstOrDefault();
            value = div?.ToString() ?? element.Value;
        }

        return new FeedContent
        {
            Html = type is "html" or "xhtml" ? value : null,
            PlainText = type == "text" ? value : HtmlUtils.StripHtml(value),
            ContentType = type switch
            {
                "html" or "xhtml" => "text/html",
                "text" => "text/plain",
                _ => type
            }
        };
    }

    private static IEnumerable<FeedAuthor> ParsePersons(XElement element, XNamespace ns, string localName)
    {
        var persons = element.Elements(ns + localName);

        foreach (var person in persons)
        {
            yield return new FeedAuthor
            {
                Name = GetElementValue(person, ns + "name"),
                Email = GetElementValue(person, ns + "email"),
                Uri = ParseUri(GetElementValue(person, ns + "uri"))
            };
        }
    }

    private static IEnumerable<FeedCategory> ParseCategories(XElement element, XNamespace ns)
    {
        var categories = element.Elements(ns + "category");

        foreach (var cat in categories)
        {
            var term = cat.Attribute("term")?.Value;
            if (!string.IsNullOrWhiteSpace(term))
            {
                yield return new FeedCategory
                {
                    Name = term,
                    Domain = cat.Attribute("scheme")?.Value,
                    Label = cat.Attribute("label")?.Value
                };
            }
        }
    }

    private static IEnumerable<FeedEnclosure> ParseEnclosures(XElement entry, XNamespace ns)
    {
        var links = entry.Elements(ns + "link")
            .Where(l => l.Attribute("rel")?.Value?.Equals("enclosure", StringComparison.OrdinalIgnoreCase) == true);

        foreach (var link in links)
        {
            var url = ParseUri(link.Attribute("href")?.Value);
            if (url != null)
            {
                yield return new FeedEnclosure
                {
                    Url = url,
                    MimeType = link.Attribute("type")?.Value,
                    Length = long.TryParse(link.Attribute("length")?.Value, out var len) ? len : null
                };
            }
        }
    }

    private static FeedMediaContent? ParseMedia(XElement entry)
    {
        var mediaContent = entry.Element(XmlNamespaces.Media + "content");
        var mediaThumbnail = entry.Element(XmlNamespaces.Media + "thumbnail");
        var mediaGroup = entry.Element(XmlNamespaces.Media + "group");

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
            Title = entry.Element(XmlNamespaces.Media + "title")?.Value,
            Description = entry.Element(XmlNamespaces.Media + "description")?.Value
        };
    }

    private static AtomFeedData ParseAtomData(XElement root, XNamespace ns)
    {
        return new AtomFeedData
        {
            Id = GetElementValue(root, ns + "id"),
            Generator = ParseGenerator(root.Element(ns + "generator")),
            Icon = ParseUri(GetElementValue(root, ns + "icon")),
            Logo = ParseUri(GetElementValue(root, ns + "logo")),
            Subtitle = GetTextValue(root, ns + "subtitle"),
            Links = ParseLinks(root, ns).ToList(),
            Contributors = ParseAtomPersons(root, ns, "contributor").ToList()
        };
    }

    private static AtomGenerator? ParseGenerator(XElement? generator)
    {
        if (generator == null) return null;

        return new AtomGenerator
        {
            Name = generator.Value,
            Uri = ParseUri(generator.Attribute("uri")?.Value),
            Version = generator.Attribute("version")?.Value
        };
    }

    private static IEnumerable<AtomLink> ParseLinks(XElement element, XNamespace ns)
    {
        var links = element.Elements(ns + "link");

        foreach (var link in links)
        {
            var href = ParseUri(link.Attribute("href")?.Value);
            if (href != null)
            {
                yield return new AtomLink
                {
                    Href = href,
                    Rel = link.Attribute("rel")?.Value,
                    Type = link.Attribute("type")?.Value,
                    HrefLang = link.Attribute("hreflang")?.Value,
                    Title = link.Attribute("title")?.Value,
                    Length = long.TryParse(link.Attribute("length")?.Value, out var len) ? len : null
                };
            }
        }
    }

    private static IEnumerable<AtomPerson> ParseAtomPersons(XElement element, XNamespace ns, string localName)
    {
        var persons = element.Elements(ns + localName);

        foreach (var person in persons)
        {
            yield return new AtomPerson
            {
                Name = GetElementValue(person, ns + "name"),
                Email = GetElementValue(person, ns + "email"),
                Uri = ParseUri(GetElementValue(person, ns + "uri"))
            };
        }
    }

    private static AtomEntryData ParseAtomEntryData(XElement entry, XNamespace ns)
    {
        return new AtomEntryData
        {
            Id = GetElementValue(entry, ns + "id"),
            Summary = ParseAtomText(entry.Element(ns + "summary")),
            Content = ParseAtomContent(entry.Element(ns + "content")),
            Links = ParseLinks(entry, ns).ToList(),
            Contributors = ParseAtomPersons(entry, ns, "contributor").ToList(),
            Source = ParseSource(entry.Element(ns + "source"), ns),
            Rights = GetTextValue(entry, ns + "rights")
        };
    }

    private static AtomText? ParseAtomText(XElement? element)
    {
        if (element == null) return null;

        return new AtomText
        {
            Value = element.Value,
            Type = element.Attribute("type")?.Value
        };
    }

    private static AtomContent? ParseAtomContent(XElement? element)
    {
        if (element == null) return null;

        return new AtomContent
        {
            Value = element.Value,
            Type = element.Attribute("type")?.Value,
            Src = ParseUri(element.Attribute("src")?.Value)
        };
    }

    private static AtomSource? ParseSource(XElement? source, XNamespace ns)
    {
        if (source == null) return null;

        return new AtomSource
        {
            Id = GetElementValue(source, ns + "id"),
            Title = GetTextValue(source, ns + "title"),
            Updated = DateParser.ParseAtomDate(GetElementValue(source, ns + "updated")),
            Links = ParseLinks(source, ns).ToList()
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
            Creator = element.Element(dc + "creator")?.Value,
            Date = DateParser.ParseAtomDate(element.Element(dc + "date")?.Value),
            Subject = element.Element(dc + "subject")?.Value,
            Description = element.Element(dc + "description")?.Value,
            Publisher = element.Element(dc + "publisher")?.Value,
            Contributor = element.Element(dc + "contributor")?.Value,
            Type = element.Element(dc + "type")?.Value,
            Format = element.Element(dc + "format")?.Value,
            Identifier = element.Element(dc + "identifier")?.Value,
            Source = element.Element(dc + "source")?.Value,
            Language = element.Element(dc + "language")?.Value,
            Relation = element.Element(dc + "relation")?.Value,
            Coverage = element.Element(dc + "coverage")?.Value,
            Rights = element.Element(dc + "rights")?.Value
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
            Author = element.Element(itunes + "author")?.Value,
            Subtitle = element.Element(itunes + "subtitle")?.Value,
            Summary = element.Element(itunes + "summary")?.Value,
            ImageUrl = ParseUri(element.Element(itunes + "image")?.Attribute("href")?.Value),
            Explicit = ParseExplicit(element.Element(itunes + "explicit")?.Value),
            Owner = ParseOwner(element.Element(itunes + "owner")),
            Categories = ParseITunesCategories(element).ToList(),
            Keywords = ParseKeywords(element.Element(itunes + "keywords")?.Value),
            Block = element.Element(itunes + "block")?.Value?.Equals("yes", StringComparison.OrdinalIgnoreCase),
            Complete = element.Element(itunes + "complete")?.Value?.Equals("yes", StringComparison.OrdinalIgnoreCase)
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
            Author = item.Element(itunes + "author")?.Value,
            Subtitle = item.Element(itunes + "subtitle")?.Value,
            Summary = item.Element(itunes + "summary")?.Value,
            ImageUrl = ParseUri(item.Element(itunes + "image")?.Attribute("href")?.Value),
            Duration = ParseDuration(item.Element(itunes + "duration")?.Value),
            Explicit = ParseExplicit(item.Element(itunes + "explicit")?.Value),
            Episode = int.TryParse(item.Element(itunes + "episode")?.Value, out var ep) ? ep : null,
            Season = int.TryParse(item.Element(itunes + "season")?.Value, out var s) ? s : null,
            EpisodeType = item.Element(itunes + "episodeType")?.Value,
            Block = item.Element(itunes + "block")?.Value?.Equals("yes", StringComparison.OrdinalIgnoreCase)
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
            Name = owner.Element(XmlNamespaces.ITunes + "name")?.Value,
            Email = owner.Element(XmlNamespaces.ITunes + "email")?.Value
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
