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

    // The button is named for what pressing it does, which also tells a screen reader user which theme is on.
    private string themeToggleLabel => themeIcon == Icon.Sun ? "Switch to light theme" : "Switch to dark theme"; // NOSONAR - used in Navigation.razor template

    // Announced after the theme is switched: nothing else about the change reaches a screen reader.
    private string themeStatusMessage = string.Empty; // NOSONAR - used in Navigation.razor template

    /// <summary>
    /// Classes for the top-bar links. Muted by default, with the current page marked by full-strength text and
    /// a brand gradient underline. The sidebar marks its current row with a fill, so the two levels of
    /// navigation do not look like one list.
    /// </summary>
    private const string navLinkClasses =
        "relative inline-flex items-center gap-2 rounded-md px-3 py-1.5 text-sm font-medium text-gray-600 transition-colors duration-200 hover:bg-gray-100 hover:text-gray-950 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-purple-600 after:absolute after:inset-x-3 after:-bottom-1 after:h-0.5 after:rounded-full after:bg-linear-to-r after:from-purple-600 after:to-fuchsia-500 after:opacity-0 after:transition-opacity after:duration-200 [&.active]:text-gray-950 [&.active]:after:opacity-100 dark:text-gray-400 dark:hover:bg-white/5 dark:hover:text-white dark:after:from-purple-400 dark:after:to-fuchsia-300 dark:[&.active]:text-white";

    /// <summary>
    /// Classes for the icon buttons on the right of the top bar.
    /// </summary>
    private const string navIconClasses =
        "text-gray-600 hover:text-fuchsia-600 dark:text-gray-300 dark:hover:text-fuchsia-300";

#pragma warning disable S1075 // Fixed external documentation link, not environment-specific
    private static readonly string _apiDocumentationUri = "https://twblazor.github.io/twblazor/";
#pragma warning restore S1075

    // The icons of the groups built from components.json, by item id: its categories and the groups nested in them.
    private static readonly Dictionary<string, Icon> _groupIcons = new()
    {
        ["layout"] = Icon.Columns_Gap,
        ["forms"] = Icon.Ui_Checks,
        ["dates-time-navitem"] = Icon.Calendar_Week,
        ["feedback"] = Icon.Chat_Square_Dots,
        ["data"] = Icon.Table,
        ["charts"] = Icon.Bar_Chart_Line,
        ["services"] = Icon.Plug,
    };

    // The ids of the items that show a "New" badge, filled while the navigation items are built.
    private readonly HashSet<string> _newItemIds = [];

    // Category order in the sidebar follows components.json's array order.
    private readonly List<NavigationItem> _navigationItems;

    public Navigation() => _navigationItems = BuildNavigationItems();

    private List<NavigationItem> BuildNavigationItems()
    {
        List<NavigationItem> items =
        [
            new() { Id = "home", Label = "Home", Href = "/", Icon = Icon.House },
            new()
            {
                Id = "get-started-group",
                Label = "Get Started",
                Icon = Icon.Rocket_Takeoff,
                Collapsed = false,
                NavigationItems =
                [
                    new() { Id = "get-started", Label = "Get started", Href = "/get-started" },
                    new() { Id = "tailwind-blazor", Label = "Tailwind with Blazor", Href = "/tailwind-blazor" },
                    new() { Id = "why-twblazor", Label = "Why twblazor?", Href = "/why-twblazor" },
                ],
            },
            new()
            {
                Id = "configuration",
                Label = "Configuration",
                Icon = Icon.Gear,
                NavigationItems =
                [
                    new() { Id = "theme", Label = "Theme", Href = "/theme", Icon = Icon.Brush },
                    new() { Id = "class-merge", Label = "Class Merge", Href = "/class-merge", Icon = Icon.Intersect },
                ],
            },
        ];

        foreach (var category in ComponentCatalog.LoadCategories())
        {
            var id = category.Category.ToLowerInvariant();
            if (category.IsNew)
                _newItemIds.Add(id);

            var children = category.Items.Select(entry => BuildNavigationItem(entry, category.IsNew)).ToList();
            items.Add(new NavigationItem { Id = id, Label = category.Category, Icon = GetGroupIcon(id), NavigationItems = children });
        }

        items.Add(new() { Id = "api-doc", Label = "API Documentation", Href = _apiDocumentationUri, Icon = Icon.Journal_Code });

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
            ? new NavigationItem { Id = entry.Id, Label = entry.Display, Icon = GetGroupIcon(entry.Id), NavigationItems = entry.Items.Select(child => BuildNavigationItem(child, entry.IsNew)).ToList() }
            : new NavigationItem { Id = entry.Id, Label = entry.Display, Href = entry.Url };
    }

    /// <summary>
    /// Returns the icon for a group built from <c>components.json</c>, or <see langword="null"/> when the
    /// group has none in <see cref="_groupIcons"/>.
    /// </summary>
    private static Icon? GetGroupIcon(string id) => _groupIcons.TryGetValue(id, out var icon) ? icon : null;

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
        themeStatusMessage = isDark ? "Dark theme on" : "Light theme on";
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
