using System.Globalization;
using System.Text.RegularExpressions;

namespace TwBlazor.Docs.Services;

/// <summary>
/// Site-wide constants and helpers behind every docs page's title, canonical URL and social tags,
/// kept in one place so the pages can't drift apart on any of them.
/// </summary>
internal static partial class SiteMetadata
{
    /// <summary>
    /// The public origin of the docs site. Must match <c>SitemapGenerator.DefaultBaseUrl</c> in the build
    /// tools, which writes the sitemap entries the canonical URLs have to agree with.
    /// </summary>
#pragma warning disable S1075 // The site's fixed public origin, not environment-specific
    public const string BaseUrl = "https://twblazor.com";
#pragma warning restore S1075

    public const string SiteName = "twblazor";

    public const string Locale = "en_GB";

    /// <summary>
    /// The default social sharing image, served from the docs static web assets.
    /// </summary>
    public const string ImagePath = "/_content/TwBlazor.Docs/images/og-image.png";

    public const int ImageWidth = 1200;

    public const int ImageHeight = 630;

    public const string ImageAlt = "twblazor: Tailwind CSS components for Blazor";

    /// <summary>
    /// Builds the absolute URL for a route or asset path.
    /// </summary>
    /// <param name="path">A root-relative path such as <c>/card</c> or <c>/</c>.</param>
    /// <returns>The path prefixed with <see cref="BaseUrl"/>.</returns>
    public static string AbsoluteUrl(string path) => BaseUrl + path;

    /// <summary>
    /// The longest a full <c>&lt;title&gt;</c> can be before Google starts truncating it in results.
    /// </summary>
    public const int MaxTitleLength = 60;

    /// <summary>
    /// Writes a page title the way every page on the site is titled: <c>{name} | twblazor - {summary}</c>.
    /// </summary>
    /// <param name="name">What the page is about, e.g. a component type name.</param>
    /// <param name="summary">A short description of it, e.g. <c>Blazor Card Component</c>.</param>
    /// <returns>The full <c>&lt;title&gt;</c> text.</returns>
    public static string ComposeTitle(string name, string summary) => $"{name} | {SiteName} - {summary}";

    /// <summary>
    /// Builds the default title for a component page, e.g. <c>TwDateRangePicker</c> becomes
    /// <c>TwDateRangePicker | twblazor - Blazor Date Range Picker Component</c>, so the title carries the
    /// words people actually search for as well as the type name. The trailing "Component" is dropped when
    /// keeping it would push the title past <see cref="MaxTitleLength"/>.
    /// </summary>
    /// <param name="componentName">The component type name, including its <c>Tw</c> prefix.</param>
    /// <returns>The full <c>&lt;title&gt;</c> text.</returns>
    public static string ComponentTitle(string componentName)
    {
        var bare = componentName.StartsWith("Tw", StringComparison.Ordinal) ? componentName[2..] : componentName;
        var words = WordBoundaryPattern().Replace(bare, " ");
        var withSuffix = ComposeTitle(componentName, $"Blazor {words} Component");

        return withSuffix.Length > MaxTitleLength ? ComposeTitle(componentName, $"Blazor {words}") : withSuffix;
    }

    /// <summary>
    /// Formats a date the way it is shown in a page's "last updated" line, independent of the
    /// machine's culture so prerendered output is identical wherever it is built.
    /// </summary>
    /// <param name="date">The date to format.</param>
    /// <returns>The date as e.g. <c>24 September 2026</c>.</returns>
    public static string FormatDate(DateOnly date) => date.ToString("d MMMM yyyy", CultureInfo.InvariantCulture);

    /// <summary>
    /// Formats a date as the ISO 8601 value used by <c>datetime</c> attributes and meta tags.
    /// </summary>
    /// <param name="date">The date to format.</param>
    /// <returns>The date as <c>yyyy-MM-dd</c>.</returns>
    public static string FormatIsoDate(DateOnly date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    [GeneratedRegex("(?<=[a-z])(?=[A-Z])")]
    private static partial Regex WordBoundaryPattern();
}
