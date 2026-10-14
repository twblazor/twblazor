using Microsoft.AspNetCore.Components;
using TwBlazor.Configuration.Color;

namespace TwBlazor.Docs.Pages.Showcase.Shared;

/// <summary>
/// The frame of a showcase page. It applies the example's theme and stylesheet, shows the brand's own
/// storefront markup, then the component gallery and the theme's source file, with the notice that the page
/// is an independent example.
/// </summary>
public sealed partial class ShowcasePage
{
    private bool isThemed;

    /// <summary>
    /// Gets or sets the example to show.
    /// </summary>
    [Parameter, EditorRequired] public required ShowcaseBrand Brand { get; set; }

    /// <summary>
    /// Gets or sets the brand's storefront: its header, navigation, hero and products.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the brand's footer, shown after the gallery and the theme file.
    /// </summary>
    [Parameter] public RenderFragment? Footer { get; set; }

    private string description => $"See twblazor restyled by a {Brand.Name}-inspired Theme.cs: one custom theme file changes every component on the page.";

    private TwNeutralTextPalette neutralText => Options.Theme.Colors.NeutralText;

    // "theme-light" opts the page out of dark mode for a brand whose own site has no dark version.
    private string pageClass => $"flex-1 {(Brand.LightOnly ? "theme-light" : null)} {Brand.FontClass} {Options.Theme.Colors.NeutralSurface.Background} {neutralText.Heading}";

    private string razorSource => ReadPageSource(Brand.PageName);

    private string sectionHeadingClass => $"text-2xl font-bold tracking-tight md:text-3xl {neutralText.Heading}";

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        // The theme belongs to the whole application, so it is only swapped once the page is running in the
        // visitor's browser. While the site is prerendered at build time every page shares one application.
        if (RendererInfo.IsInteractive)
        {
            ShowcaseThemeSwitcher.Apply(Options, Brand);
            isThemed = true;
        }
    }

    /// <summary>
    /// Reads the Razor source of a showcase page, which the build embeds in the assembly so the page can show its own markup.
    /// </summary>
    /// <param name="pageName">The name of the page's component.</param>
    internal static string ReadPageSource(string pageName)
    {
        using var stream = typeof(ShowcasePage).Assembly.GetManifestResourceStream($"Showcase.{pageName}.razor");
        if (stream is null)
        {
            return string.Empty;
        }

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// Puts the site's own theme back when the visitor leaves the page.
    /// </summary>
    public void Dispose()
    {
        if (isThemed)
        {
            ShowcaseThemeSwitcher.Restore(Options, Brand);
        }
    }
}