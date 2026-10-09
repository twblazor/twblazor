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
        "inline-flex items-center gap-2 rounded px-3 py-1.5 text-sm font-medium text-gray-600 transition-colors duration-200 hover:bg-purple-50 hover:text-purple-900 [&.active]:bg-purple-100 [&.active]:text-purple-900 dark:text-gray-300 dark:hover:bg-purple-500/15 dark:hover:text-white dark:[&.active]:bg-purple-500/20 dark:[&.active]:text-white";

    /// <summary>
    /// The brand gradient, shared by the logo and the wordmark in the sidebar header so the two always match,
    /// in light and dark.
    /// </summary>
    private const string brandGradientClasses =
        "bg-linear-to-r from-purple-700 to-fuchsia-600 dark:from-purple-400 dark:to-fuchsia-300";

    /// <summary>
    /// Classes for the logo. The gradient shows through the shape of <c>logo-mark.svg</c> (the bolt without its backdrop), used as a mask, because an
    /// image cannot follow the site's theme toggle. The path is relative to the stylesheet.
    /// </summary>
    private const string logoClasses =
        brandGradientClasses + " size-8 me-2 shrink-0 mask-[url(../images/logo-mark.svg)] mask-contain mask-center mask-no-repeat";

    /// <summary>
    /// Classes for the "twblazor" wordmark: the gradient clipped to the letters.
    /// </summary>
    private const string wordmarkClasses = brandGradientClasses + " bg-clip-text text-transparent";

    /// <summary>
    /// Classes for the icon buttons on the right of the top bar.
    /// </summary>
    private const string navIconClasses =
        "text-gray-600 hover:text-fuchsia-600 dark:text-gray-300 dark:hover:text-fuchsia-300";

#pragma warning disable S1075 // Fixed external documentation link, not environment-specific
    private static readonly string _apiDocumentationUri = "https://twblazor.github.io/twblazor/";
#pragma warning restore S1075

    // The ids of the items that show a "New" badge, filled while the navigation items are built.
    private readonly HashSet<string> _newItemIds = [];

    // Category order in the sidebar follows components.json's array order.
    private readonly List<NavigationItem> _navigationItems;

    public Navigation() => _navigationItems = BuildNavigationItems();

    private List<NavigationItem> BuildNavigationItems()
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
                    new() { Id = "why-twblazor", Label = "Why twblazor?", Href = "/why-twblazor" },
                ],
            },
            new() { Id = "theme", Label = "Theme", Href = "/theme" },
        ];

        foreach (var category in ComponentCatalog.LoadCategories())
        {
            var id = category.Category.ToLowerInvariant();
            if (category.IsNew)
                _newItemIds.Add(id);

            var children = category.Items.Select(entry => BuildNavigationItem(entry, category.IsNew)).ToList();
            items.Add(new NavigationItem { Id = id, Label = category.Category, NavigationItems = children });
        }

        items.Add(new() { Id = "api-doc", Label = "API Documentation", Href = _apiDocumentationUri });

        return items;
    }

    // An entry with nested Items (e.g. "Dates & Time" grouping the date/time pickers under "Forms")
    // is itself a group rather than a leaf link, so it recurses - to any depth components.json uses.
    // A new entry inside a section that is already badged gets no badge of its own: one badge on the section
    // says it for all of them.
    private NavigationItem BuildNavigationItem(ComponentEntry entry, bool isInNewSection)
    {
        if (entry.IsNew && !isInNewSection)
            _newItemIds.Add(entry.Id);

        return entry.Items.Count > 0
            ? new NavigationItem { Id = entry.Id, Label = entry.Display, NavigationItems = entry.Items.Select(child => BuildNavigationItem(child, entry.IsNew)).ToList() }
            : new NavigationItem { Id = entry.Id, Label = entry.Display, Href = entry.Url };
    }

    private bool IsNew(NavigationItem item) => item.Id is { } id && _newItemIds.Contains(id);

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
