using System.Globalization;

namespace SyndicationClient.Parsing;

/// <summary>
/// Provides date parsing utilities for various feed date formats.
/// </summary>
public static class DateParser
{
    private static readonly string[] RssDateFormats =
    [
        "ddd, dd MMM yyyy HH:mm:ss zzz",
        "ddd, dd MMM yyyy HH:mm:ss Z",
        "ddd, dd MMM yyyy HH:mm:ss",
        "dd MMM yyyy HH:mm:ss zzz",
        "dd MMM yyyy HH:mm:ss Z",
        "dd MMM yyyy HH:mm:ss",
        "yyyy-MM-ddTHH:mm:sszzz",
        "yyyy-MM-ddTHH:mm:ssZ",
        "yyyy-MM-ddTHH:mm:ss",
        "yyyy-MM-dd HH:mm:ss",
        "yyyy-MM-dd",
        "ddd, d MMM yyyy HH:mm:ss zzz",
        "ddd, d MMM yyyy HH:mm:ss Z",
        "ddd, d MMM yyyy HH:mm:ss",
    ];

    private static readonly string[] Iso8601Formats =
    [
        "yyyy-MM-ddTHH:mm:ss.fffffffzzz",
        "yyyy-MM-ddTHH:mm:ss.ffffffzzz",
        "yyyy-MM-ddTHH:mm:ss.fffffzzz",
        "yyyy-MM-ddTHH:mm:ss.ffffzzz",
        "yyyy-MM-ddTHH:mm:ss.fffzzz",
        "yyyy-MM-ddTHH:mm:ss.ffzzz",
        "yyyy-MM-ddTHH:mm:ss.fzzz",
        "yyyy-MM-ddTHH:mm:sszzz",
        "yyyy-MM-ddTHH:mm:ss.fffffffZ",
        "yyyy-MM-ddTHH:mm:ss.ffffffZ",
        "yyyy-MM-ddTHH:mm:ss.fffffZ",
        "yyyy-MM-ddTHH:mm:ss.ffffZ",
        "yyyy-MM-ddTHH:mm:ss.fffZ",
        "yyyy-MM-ddTHH:mm:ss.ffZ",
        "yyyy-MM-ddTHH:mm:ss.fZ",
        "yyyy-MM-ddTHH:mm:ssZ",
        "yyyy-MM-ddTHH:mm:ss",
        "yyyy-MM-dd",
    ];

    /// <summary>
    /// Parses an RSS date string into a DateTimeOffset.
    /// </summary>
    public static DateTimeOffset? ParseRssDate(string? dateString)
    {
        if (string.IsNullOrWhiteSpace(dateString))
            return null;

        dateString = NormalizeDateString(dateString);

        foreach (var format in RssDateFormats)
        {
            if (DateTimeOffset.TryParseExact(dateString, format, CultureInfo.InvariantCulture,
                    DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeUniversal, out var result))
            {
                return result;
            }
        }

        // Try general parsing as fallback
        if (DateTimeOffset.TryParse(dateString, CultureInfo.InvariantCulture,
                DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeUniversal, out var fallbackResult))
        {
            return fallbackResult;
        }

        return null;
    }

    /// <summary>
    /// Parses an ISO 8601/Atom date string into a DateTimeOffset.
    /// </summary>
    public static DateTimeOffset? ParseAtomDate(string? dateString)
    {
        if (string.IsNullOrWhiteSpace(dateString))
            return null;

        dateString = dateString.Trim();

        foreach (var format in Iso8601Formats)
        {
            if (DateTimeOffset.TryParseExact(dateString, format, CultureInfo.InvariantCulture,
                    DateTimeStyles.AllowWhiteSpaces, out var result))
            {
                return result;
            }
        }

        // Try general parsing as fallback
        if (DateTimeOffset.TryParse(dateString, CultureInfo.InvariantCulture,
                DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeUniversal, out var fallbackResult))
        {
            return fallbackResult;
        }

        return null;
    }

    private static string NormalizeDateString(string dateString)
    {
        dateString = dateString.Trim();

        // Replace common timezone abbreviations
        dateString = dateString
            .Replace("GMT", "+0000")
            .Replace("EST", "-0500")
            .Replace("EDT", "-0400")
            .Replace("CST", "-0600")
            .Replace("CDT", "-0500")
            .Replace("MST", "-0700")
            .Replace("MDT", "-0600")
            .Replace("PST", "-0800")
            .Replace("PDT", "-0700")
            .Replace("UT", "+0000");

        return dateString;
    }
}
