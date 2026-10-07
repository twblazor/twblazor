using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TwBlazor.Docs.Services;
using TwBlazor.Enums;
using TwBlazor.Models;
using TwBlazor.Services;

namespace TwBlazor.Docs.Layout;

public partial class Navigation : IDisposable
{
    [Parameter] public RenderFragment? ChildContent { get; set; }

    [Parameter] public bool MainContentPadding { get; set; }

    [Inject] private ITwDialogService dialogService { get; set; } = null!;

    private Icon themeIcon = Icon.Moon; // NOSONAR - used in Navigation.razor template

    private bool isSidebarOpen = true; // NOSONAR - bound in Navigation.razor template

    /// <summary>
    /// Classes for the top-bar links. Neutral by default, with the current page marked by a filled pill.
    /// </summary>
    private const string navLinkClasses =
        "inline-flex items-center gap-2 rounded px-3 py-1.5 text-sm font-medium text-gray-600 transition-colors duration-200 hover:bg-gray-100 hover:text-gray-950 [&.active]:bg-gray-100 [&.active]:text-gray-950 dark:text-gray-300 dark:hover:bg-white/10 dark:hover:text-white dark:[&.active]:bg-white/10 dark:[&.active]:text-white";

    /// <summary>
    /// Classes for the icon buttons on the right of the top bar.
    /// </summary>
    private const string navIconClasses =
        "text-gray-600 hover:text-gray-950 dark:text-gray-300 dark:hover:text-white";

#pragma warning disable S1075 // Fixed external documentation link, not environment-specific
    private static readonly string _apiDocumentationUri = "https://twblazor.github.io/twblazor/";
#pragma warning restore S1075

    // Category order in the sidebar follows components.json's array order.
    private readonly List<NavigationItem> _navigationItems = BuildNavigationItems();

    private static List<NavigationItem> BuildNavigationItems()
    {
        List<NavigationItem> items =
        [
            new() { Id = "home", Label = "Home", Href = "/" },
            new()
            {
                Id = "get-started-group",
                Label = "Get Started",
                Collapsed = false,
                NavigationItems =
                [
                    new() { Id = "get-started", Label = "Get started", Href = "/get-started" },
                    new() { Id = "tailwind-blazor", Label = "Tailwind with Blazor", Href = "/tailwind-blazor" },
                    new() { Id = "library-comparison", Label = "Library comparison", Href = "/blazor-component-library-comparison" },
                ],
            },
            new() { Id = "theme", Label = "Theme", Href = "/theme" },
        ];

        foreach (var category in ComponentCatalog.LoadCategories())
        {
            var children = category.Items.Select(BuildNavigationItem).ToList();
            items.Add(new NavigationItem { Id = category.Category.ToLowerInvariant(), Label = category.Category, NavigationItems = children });
        }

        items.Add(new() { Id = "api-doc", Label = "API Documentation", Href = _apiDocumentationUri });

        return items;
    }

    // An entry with nested Items (e.g. "Dates & Time" grouping the date/time pickers under "Forms")
    // is itself a group rather than a leaf link, so it recurses - to any depth components.json uses.
    private static NavigationItem BuildNavigationItem(ComponentEntry entry) =>
        entry.Items.Count > 0
            ? new NavigationItem { Id = entry.Id, Label = entry.Display, NavigationItems = entry.Items.Select(BuildNavigationItem).ToList() }
            : new NavigationItem { Id = entry.Id, Label = entry.Display, Href = entry.Url, New = entry.IsNew };

    private readonly CancellationTokenSource _cts = new();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            try
            {
                var isDark = await JS.InvokeAsync<bool>(
                    "themeToggle.isDarkMode",
                    _cts.Token);
                themeIcon = isDark ? Icon.Sun : Icon.Moon;
                isSidebarOpen = !await JS.InvokeAsync<bool>("twSidebar.isMobileViewport", _cts.Token);
                StateHasChanged();
            }
            catch (OperationCanceledException)
            {
                // Circuit was interrupted (hot reload, navigation, or disconnect) — expected.
            }
        }
    }

    private async Task ToggleTheme()
    {
        var isDark = await JS.InvokeAsync<bool>("themeToggle.toggle", _cts.Token);
        themeIcon = isDark ? Icon.Sun : Icon.Moon;
    }

    private async Task SearchDialog() => await dialogService.ShowAsync<SearchDisplay>(options: new TwDialogOptions { NoHeader = true });

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _cts.Cancel();
            _cts.Dispose();
        }
    }
}
