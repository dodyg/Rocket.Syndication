using System.Text.RegularExpressions;
using System.Web;

namespace SyndicationClient.Parsing;

/// <summary>
/// Provides utility methods for HTML processing.
/// </summary>
public static partial class HtmlUtils
{
    /// <summary>
    /// Strips HTML tags from a string and returns plain text.
    /// </summary>
    public static string? StripHtml(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return null;

        // Remove script and style elements completely
        var text = ScriptStyleRegex().Replace(html, string.Empty);

        // Replace common block elements with newlines
        text = BlockElementRegex().Replace(text, "\n");

        // Replace BR tags with newlines
        text = BrTagRegex().Replace(text, "\n");

        // Remove all remaining HTML tags
        text = HtmlTagRegex().Replace(text, string.Empty);

        // Decode HTML entities
        text = HttpUtility.HtmlDecode(text);

        // Normalize whitespace
        text = MultipleSpacesRegex().Replace(text, " ");
        text = MultipleNewlinesRegex().Replace(text, "\n\n");

        return text.Trim();
    }

    [GeneratedRegex(@"<script[^>]*>[\s\S]*?</script>|<style[^>]*>[\s\S]*?</style>", RegexOptions.IgnoreCase)]
    private static partial Regex ScriptStyleRegex();

    [GeneratedRegex(@"</(p|div|h[1-6]|li|tr|table|blockquote|pre)>", RegexOptions.IgnoreCase)]
    private static partial Regex BlockElementRegex();

    [GeneratedRegex(@"<br\s*/?>", RegexOptions.IgnoreCase)]
    private static partial Regex BrTagRegex();

    [GeneratedRegex(@"<[^>]+>")]
    private static partial Regex HtmlTagRegex();

    [GeneratedRegex(@"[ \t]+")]
    private static partial Regex MultipleSpacesRegex();

    [GeneratedRegex(@"\n{3,}")]
    private static partial Regex MultipleNewlinesRegex();
}
