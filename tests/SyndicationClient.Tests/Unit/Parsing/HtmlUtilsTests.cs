using FluentAssertions;
using SyndicationClient.Parsing;

namespace SyndicationClient.Tests.Unit.Parsing;

public class HtmlUtilsTests
{
    [Test]
    public void StripHtml_WithPlainText_ReturnsUnchanged()
    {
        var input = "This is plain text";

        var result = HtmlUtils.StripHtml(input);

        result.Should().Be("This is plain text");
    }

    [Test]
    public void StripHtml_WithSimpleTags_RemovesTags()
    {
        var input = "<p>This is <strong>bold</strong> text</p>";

        var result = HtmlUtils.StripHtml(input);

        result.Should().Contain("This is bold text");
    }

    [Test]
    public void StripHtml_WithScriptTags_RemovesScriptContent()
    {
        var input = "Hello<script>alert('xss');</script>World";

        var result = HtmlUtils.StripHtml(input);

        result.Should().NotContain("script");
        result.Should().NotContain("alert");
        result.Should().Contain("Hello");
        result.Should().Contain("World");
    }

    [Test]
    public void StripHtml_WithStyleTags_RemovesStyleContent()
    {
        var input = "Hello<style>.class { color: red; }</style>World";

        var result = HtmlUtils.StripHtml(input);

        result.Should().NotContain("style");
        result.Should().NotContain("color");
    }

    [Test]
    public void StripHtml_WithHtmlEntities_DecodesEntities()
    {
        var input = "&lt;p&gt;Test &amp; Demo&lt;/p&gt;";

        var result = HtmlUtils.StripHtml(input);

        result.Should().Contain("<p>");
        result.Should().Contain("Test & Demo");
    }

    [Test]
    public void StripHtml_WithNullOrEmpty_ReturnsNull()
    {
        HtmlUtils.StripHtml(null).Should().BeNull();
        HtmlUtils.StripHtml("").Should().BeNull();
        HtmlUtils.StripHtml("   ").Should().BeNull();
    }

    [Test]
    public void StripHtml_WithBrTags_ConvertsToNewlines()
    {
        var input = "Line 1<br/>Line 2<br>Line 3";

        var result = HtmlUtils.StripHtml(input);

        result.Should().Contain("\n");
    }

    [Test]
    public void StripHtml_WithNestedTags_RemovesAllTags()
    {
        var input = "<div><p><span><strong>Nested</strong></span></p></div>";

        var result = HtmlUtils.StripHtml(input);

        result.Should().Contain("Nested");
        result.Should().NotContain("<");
        result.Should().NotContain(">");
    }
}
