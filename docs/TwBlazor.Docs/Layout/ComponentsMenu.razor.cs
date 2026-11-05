using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using TwBlazor.Docs.Services;
using TwBlazor.Enums;

namespace TwBlazor.Docs.Layout;

/// <summary>
/// The "Components" button in the top bar on wide screens. It opens a panel with a search box and a tile for
/// each category in <c>components.json</c>. Choosing a tile lists that category's components in the same panel.
/// Phones use the sidebar instead, so the button is hidden below the <c>lg</c> breakpoint.
/// </summary>
public partial class ComponentsMenu : IDisposable
{
    [Inject] private NavigationManager navigationManager { get; set; } = null!;

    private bool isOpen;
    private string query = string.Empty;
    private MenuCategory? selected;

    // Where focus goes after the next render: the back button when a category opens, its tile when it closes.
    // Without it, focus would fall to the page as the element that had it leaves the panel.
    private Func<ElementReference>? pendingFocus;
    private ElementReference backButton;
    private readonly Dictionary<string, ElementReference> _tileRefs = [];

    private static readonly IReadOnlyList<MenuCategory> _categories = BuildCategories();

    private static readonly int _totalCount = _categories.Sum(category => category.Count);

    private string trimmedQuery => query.Trim();

    private bool isSearching => trimmedQuery.Length > 0;

    private List<(MenuCategory Category, MenuLink Link)> matches => [.. _categories
        .SelectMany(category => category.Groups.SelectMany(group => group.Links).Select(link => (category, link)))
        .Where(match => match.link.Label.Contains(trimmedQuery, StringComparison.OrdinalIgnoreCase))];

    private static string CountLabel(int count) => count == 1
        ? Loc.Text("Nav_Menu_CountOne", "1 component")
        : Loc.Format("Nav_Menu_Count", "{0} components", count.ToString(CultureInfo.CurrentCulture));

    private static string TileId(MenuCategory category) => $"components-menu-tile-{category.Id}";

    protected override void OnInitialized() => navigationManager.LocationChanged += OnLocationChanged;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (pendingFocus is not { } target)
            return;

        pendingFocus = null;
        try
        {
            await target().FocusAsync();
        }
        catch (InvalidOperationException)
        {
            // The element left the page before it could be focused.
        }
    }

    private void OnOpenChanged(bool open)
    {
        isOpen = open;
        if (!open)
        {
            query = string.Empty;
            selected = null;
        }
    }

    private void OnQueryInput(ChangeEventArgs args)
    {
        query = args.Value?.ToString() ?? string.Empty;
        selected = null;
    }

    private void OpenCategory(MenuCategory category)
    {
        selected = category;
        pendingFocus = () => backButton;
    }

    private void CloseCategory()
    {
        if (selected is { } category)
            pendingFocus = () => _tileRefs[category.Id];
        selected = null;
    }

    // Following a link inside the panel leaves it open over the new page unless it is closed here.
    private void OnLocationChanged(object? sender, LocationChangedEventArgs args)
    {
        if (!isOpen)
            return;

        _ = InvokeAsync(() =>
        {
            OnOpenChanged(false);
            StateHasChanged();
        });
    }

    private static List<MenuCategory> BuildCategories() =>
        [.. ComponentCatalog.LoadCategories().Select(category =>
        {
            var id = category.Category.ToLowerInvariant();
            var label = Loc.Text("Nav_Group_" + id, category.Category);

            // Leaves sit in a group of their own, headed by the sub-group they belong to (Charts has several).
            List<MenuGroup> groups = [];
            var loose = category.Items.Where(entry => entry.Items.Count == 0).Select(ToLink).ToList();
            if (loose.Count > 0)
                groups.Add(new MenuGroup(null, loose));
            foreach (var group in category.Items.Where(entry => entry.Items.Count > 0))
                groups.Add(new MenuGroup(Loc.Text("Nav_Group_" + group.Id, group.Display), [.. Leaves(group).Select(ToLink)]));

            return new MenuCategory(id, label, Navigation.GetGroupIcon(id) ?? Icon.Grid_3X3_Gap, groups);
        })];

    private static IEnumerable<ComponentEntry> Leaves(ComponentEntry entry) =>
        entry.Items.Count == 0 ? [entry] : entry.Items.SelectMany(Leaves);

    private static MenuLink ToLink(ComponentEntry entry) => new(entry.Display, entry.Url);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
            navigationManager.LocationChanged -= OnLocationChanged;
    }

    private sealed record MenuCategory(string Id, string Label, Icon Icon, IReadOnlyList<MenuGroup> Groups)
    {
        public int Count => Groups.Sum(group => group.Links.Count);
    }

    private sealed record MenuGroup(string? Heading, IReadOnlyList<MenuLink> Links);

    private sealed record MenuLink(string Label, string Href);
}
