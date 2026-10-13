using TwBlazor.Configuration;

namespace TwBlazor.Docs.Pages.Showcase.Shared;

/// <summary>
/// Swaps the site's theme for a showcase page's and puts it back afterwards.
/// </summary>
/// <remarks>
/// TwBlazor has one theme per application, held by the registered <see cref="TwBlazorOptions"/>, so a showcase
/// page replaces it while it is on screen. That suits the published site, which runs in the browser where the
/// application has a single visitor. Under a server host the options are shared by every connected visitor, so
/// they would all see the swap; the showcase pages are meant for the WebAssembly site.
/// </remarks>
internal static class ShowcaseThemeSwitcher
{
    private static TwBlazorTheme? siteTheme;

    /// <summary>
    /// Makes a showcase theme the application's theme, remembering the site's own the first time.
    /// </summary>
    /// <param name="options">The application's options.</param>
    /// <param name="brand">The example whose theme to apply.</param>
    public static void Apply(TwBlazorOptions options, ShowcaseBrand brand)
    {
        // Going straight from one showcase page to another, the current theme is the previous page's, not the site's.
        if (!ShowcaseBrand.All.Any(other => ReferenceEquals(other.Theme, options.Theme)))
        {
            siteTheme = options.Theme;
        }

        options.Theme = brand.Theme;
    }

    /// <summary>
    /// Puts the site's theme back, unless another showcase page has already taken over.
    /// </summary>
    /// <param name="options">The application's options.</param>
    /// <param name="brand">The example whose page is leaving.</param>
    public static void Restore(TwBlazorOptions options, ShowcaseBrand brand)
    {
        // A page that replaces this one applies its theme before this one is disposed, so only undo our own.
        if (siteTheme is not null && ReferenceEquals(options.Theme, brand.Theme))
        {
            options.Theme = siteTheme;
        }
    }
}