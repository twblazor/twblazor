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
    /// Classes for the top-bar links. Muted by default, with the current page marked by a brand-tinted pill that
    /// matches the hover shape. The sidebar marks its current row with a neutral fill and a rail, so the two
    /// levels of navigation do not look like one list.
    /// </summary>
    private const string navLinkClasses =
        "inline-flex items-center gap-2 rounded-md px-3 py-1.5 text-sm font-medium text-gray-600 transition-colors duration-200 motion-reduce:transition-none hover:bg-gray-100 hover:text-gray-950 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-purple-600 [&.active]:bg-purple-50 [&.active]:text-purple-700 forced-colors:[&.active]:underline forced-colors:[&.active]:underline-offset-4 dark:text-gray-300 dark:hover:bg-white/5 dark:hover:text-white dark:focus-visible:outline-purple-300 dark:[&.active]:bg-purple-400/15 dark:[&.active]:text-purple-100";

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
        ["containers"] = Icon.Grid_1X2,
        ["typography"] = Icon.Type,
        ["forms"] = Icon.Ui_Checks,
        ["inputs-navitem"] = Icon.Input_Cursor_Text,
        ["feedback"] = Icon.Chat_Square_Dots,
        ["data"] = Icon.Table,
        ["charts"] = Icon.Bar_Chart_Line,
        ["chart-comparison-navitem"] = Icon.Bar_Chart,
        ["chart-time-navitem"] = Icon.Graph_Up,
        ["chart-parts-navitem"] = Icon.Pie_Chart,
        ["chart-hierarchy-navitem"] = Icon.Diagram_3,
        ["chart-distribution-navitem"] = Icon.Distribute_Vertical,
        ["chart-relationship-navitem"] = Icon.Bullseye,
        ["chart-variables-navitem"] = Icon.Sliders,
        ["chart-flow-navitem"] = Icon.Share,
        ["chart-sets-navitem"] = Icon.Intersect,
        ["chat"] = Icon.Chat_Left_Dots,
        ["services"] = Icon.Plug,
    };

    // The ids of the items that show a "New" badge, filled while the navigation items are built.
    private readonly HashSet<string> _newItemIds = [];

    // Category order in the sidebar follows components.json's array order. Labels other than component names are
    // translated: the items are built per component instance, in the language of the request.
    private readonly List<NavigationItem> _navigationItems;

    public Navigation() => _navigationItems = BuildNavigationItems();

    private List<NavigationItem> BuildNavigationItems()
    {
        List<NavigationItem> items =
        [
            new() { Id = "home", Label = Loc.Text("Nav_Home", "Home"), Href = "/", Icon = Icon.House },
            new()
            {
                Id = "get-started-group",
                Label = Loc.Text("Nav_GetStartedGroup", "Get Started"),
                Icon = Icon.Rocket_Takeoff,
                Collapsed = false,
                NavigationItems =
                [
                    new() { Id = "get-started", Label = Loc.Text("Nav_GetStarted", "Get started"), Href = "/get-started" },
                    new() { Id = "tailwind-blazor", Label = Loc.Text("Nav_TailwindBlazor", "Tailwind with Blazor"), Href = "/tailwind-blazor" },
                    new() { Id = "why-twblazor", Label = Loc.Text("Nav_WhyTwBlazor", "Why twblazor?"), Href = "/why-twblazor" },
                ],
            },
            new()
            {
                Id = "configuration",
                Label = Loc.Text("Nav_Configuration", "Configuration"),
                Icon = Icon.Gear,
                NavigationItems =
                [
                    new() { Id = "theme", Label = Loc.Text("Nav_Theme", "Theme"), Href = "/theme", Icon = Icon.Brush },
                    new() { Id = "class-merge", Label = Loc.Text("Nav_ClassMerge", "Class Merge"), Href = "/class-merge", Icon = Icon.Intersect },
                    new()
                    {
                        Id = "localization",
                        Label = Loc.Text("Nav_Localization", "Localization"),
                        Icon = Icon.Translate,
                        NavigationItems =
                        [
                            new() { Id = "configuring-localization", Label = Loc.Text("Nav_ConfiguringLocalization", "Configuring localization"), Href = "/localization" },
                            new() { Id = "rtl-provider", Label = Loc.Text("Nav_RtlProvider", "RTL Provider"), Href = "/direction-provider" },
                        ],
                    },
                ],
            },
        ];

        foreach (var category in ComponentCatalog.LoadCategories())
        {
            var id = category.Category.ToLowerInvariant();
            if (category.IsNew)
                _newItemIds.Add(id);

            var children = category.Items.Select(entry => BuildNavigationItem(entry, category.IsNew)).ToList();
            items.Add(new NavigationItem { Id = id, Label = Loc.Text("Nav_Group_" + id, category.Category), Icon = GetGroupIcon(id), NavigationItems = children });
        }

        items.Add(new() { Id = "api-doc", Label = Loc.Text("Nav_ApiDocs", "API Documentation"), Href = _apiDocumentationUri, Icon = Icon.Journal_Code });
        items.Add(new() { Id = "ai-agents", Label = Loc.Text("Nav_AiAgents", "AI Agents & Skills"), Href = "/ai-agents", Icon = Icon.Robot });

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
            ? new NavigationItem { Id = entry.Id, Label = Loc.Text("Nav_Group_" + entry.Id, entry.Display), Icon = GetGroupIcon(entry.Id), NavigationItems = entry.Items.Select(child => BuildNavigationItem(child, entry.IsNew)).ToList() }
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

    private async Task SearchDialog() => await dialogService.ShowAsync<SearchDisplay>("Search documentation", new TwDialogOptions { NoHeader = true });

    private async Task OpenGitHub() => await JS.InvokeVoidAsync("open", SiteMetadata.GitHubUrl, "_blank", "noopener,noreferrer");

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
