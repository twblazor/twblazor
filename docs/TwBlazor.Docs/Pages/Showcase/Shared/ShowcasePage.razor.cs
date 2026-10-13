using Microsoft.AspNetCore.Components;
using TwBlazor.Configuration.Color;

namespace TwBlazor.Docs.Pages.Showcase.Shared;

/// <summary>
/// A whole showcase page: a brand-inspired navigation bar, hero, product grid and component gallery, all drawn
/// with that example's own theme, followed by the theme's source file.
/// </summary>
public partial class ShowcasePage
{
    private bool isThemed;
    private int basketCount;

    /// <summary>
    /// Gets or sets the example to show.
    /// </summary>
    [Parameter, EditorRequired] public required ShowcaseBrand Brand { get; set; }

    private string description => $"See twblazor restyled by a {Brand.Name}-inspired Theme.cs: one custom theme file changes every component on the page.";

    private TwNeutralTextPalette neutralText => Options.Theme.Colors.NeutralText;

    private string pageClass => $"flex-1 {Brand.FontClass} {Options.Theme.Colors.NeutralSurface.BackgroundSubtle} {neutralText.Heading}";

    private string sectionHeadingClass => $"text-2xl font-bold tracking-tight md:text-3xl {neutralText.Heading}";

    private string basketText => basketCount switch
    {
        0 => "Your basket is empty.",
        1 => "1 item in your basket.",
        _ => $"{basketCount} items in your basket."
    };

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
    /// Puts the site's own theme back when the visitor leaves the page.
    /// </summary>
    public void Dispose()
    {
        if (isThemed)
        {
            ShowcaseThemeSwitcher.Restore(Options, Brand);
        }

        GC.SuppressFinalize(this);
    }
}