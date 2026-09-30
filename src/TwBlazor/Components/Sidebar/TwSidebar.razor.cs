// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using TwBlazor.Configuration.Components;
using TwBlazor.Models;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// Represents a responsive sidebar layout component with customizable header, navbar, and sidebar content areas.
/// </summary>
/// <remarks>Use <c>TwSidebar</c> to create a page layout with a collapsible sidebar, optional header, and
/// navigation bar. The component supports dynamic content injection via <see cref="RenderFragment"/> parameters and
/// provides properties to control appearance, layout, and interactivity. Sidebar open state and navigation items can be
/// data-bound for integration with application state. This component is intended for use in Blazor applications and is
/// designed to be flexible for a variety of layout scenarios.</remarks>
public partial class TwSidebar : TwBlazorComponentBase, IDisposable
{
    [Inject] private NavigationManager navigationManager { get; set; } = null!;
    [Inject] private IJSRuntime jsRuntime { get; set; } = null!;

    private TwSidebarTheme theme => options.Theme.Components.Require<TwSidebarTheme>();

    private ElementReference mainContentRef;

    /// <summary>
    /// Gets or sets a value indicating whether the sidebar is currently open.
    /// </summary>
    [Parameter] public bool IsSidebarOpen { get; set; }

    /// <summary>
    /// Gets or sets the callback that is invoked when the sidebar open state changes.
    /// </summary>
    /// <remarks>Use this callback to respond to changes in the sidebar's visibility, such as updating
    /// application state or triggering additional actions when the sidebar is opened or closed.</remarks>
    [Parameter] public EventCallback<bool> IsSidebarOpenChanged { get; set; }

    /// <summary>
    /// Binds the searchable state of the sidebar, allows for sidebar items to be searched.
    /// </summary>
    [Parameter] public bool IsSearchable { get; set; }

    /// <summary>
    /// The page body content.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the content to render in the header area of the sidebar.
    /// </summary>
    /// <remarks>Assign a <see cref="RenderFragment"/> to customize the sidebar header's appearance and layout. If
    /// <see langword="null"/>, the sidebars header will be hidden.</remarks>
    [Parameter] public RenderFragment? HeaderContent { get; set; }

    /// <summary>
    /// Gets or sets the content to be rendered in the navbar area of the component.
    /// </summary>
    /// <remarks>Rendered alongside the sidebar's own drawer toggle, in the embedded
    /// <see cref="TwNavbar"/>'s <see cref="TwNavbar.ChildContent"/> slot. For the navbar's other slots, see
    /// <see cref="NavbarBrandContent"/>, <see cref="NavbarNavigationContent"/>, <see cref="NavbarActionsContent"/>,
    /// and <see cref="NavbarNavigationItems"/>.</remarks>
    [Parameter] public RenderFragment? NavbarContent { get; set; }

    /// <summary>
    /// Gets or sets the content rendered in the embedded navbar's brand slot.
    /// </summary>
    /// <remarks>Passed straight through to <see cref="TwNavbar.BrandContent"/>.</remarks>
    [Parameter] public RenderFragment? NavbarBrandContent { get; set; }

    /// <summary>
    /// Gets or sets custom content rendered in the embedded navbar in place of
    /// <see cref="NavbarNavigationItems"/>.
    /// </summary>
    /// <remarks>Passed straight through to <see cref="TwNavbar.NavigationContent"/>.</remarks>
    [Parameter] public RenderFragment? NavbarNavigationContent { get; set; }

    /// <summary>
    /// Gets or sets the content rendered in the embedded navbar's actions slot.
    /// </summary>
    /// <remarks>Passed straight through to <see cref="TwNavbar.ActionsContent"/>.</remarks>
    [Parameter] public RenderFragment? NavbarActionsContent { get; set; }

    /// <summary>
    /// Gets or sets the navigation items rendered in the embedded navbar when
    /// <see cref="NavbarNavigationContent"/> is not supplied.
    /// </summary>
    /// <remarks>Passed straight through to <see cref="TwNavbar.NavigationItems"/>. This is independent of
    /// <see cref="NavigationItems"/>, which populates the sidebar itself.</remarks>
    [Parameter] public List<NavigationItem> NavbarNavigationItems { get; set; } = [];

    /// <summary>
    /// Gets or sets the content to be rendered in the sidebar area of the component.
    /// </summary>
    /// <remarks>Assign a <see cref="RenderFragment"/> to customize the sidebar's appearance and layout. If
    /// <see langword="null"/>, the sidebar will not display any content.</remarks>
    [Parameter] public RenderFragment? SidebarContent { get; set; }

    /// <summary>
    /// Gets or sets the collection of sidebar items to be displayed in the component.
    /// </summary>
    /// <remarks>If <see langword="null"/> or empty, no sidebar items will be rendered. Changes to this
    /// collection will update the displayed sidebar items accordingly.</remarks>
    [Parameter] public List<NavigationItem> NavigationItems { get; set; } = [];

    /// <summary>
    /// Gets the items currently displayed in the sidebar, either filtered or the full list.
    /// </summary>
    private List<NavigationItem> displayedNavigationItems { get; set; } = [];

    /// <summary>
    /// Gets the unfiltered collection of sidebar items, used for search functionality to maintain the original list of items.
    /// </summary>
    private List<NavigationItem> unfilteredNavigationItems { get; set; } = [];

    /// <summary>
    /// The classes that are applied to the parent div for the sidebar.
    /// </summary>
    [Parameter] public string SidebarClass { get; set; } = string.Empty;

    /// <summary>
    /// The classes that are applied to the parent div for the navbar.
    /// </summary>
    [Parameter] public string NavbarClass { get; set; } = string.Empty;

    /// <summary>
    /// The classes that are applied to the toggle snackbar.
    /// </summary>
    [Parameter] public string ToggleButtonClass { get; set; } = string.Empty;

    /// <summary>
    /// The classes that are applied to the main content div.
    /// </summary>
    [Parameter] public string MainContentClass { get; set; } = string.Empty;

    /// <summary>
    /// The classes that are applied to the main content root div.
    /// </summary>
    [Parameter] public string MainContentRootClass { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the navbar should be fixed at the top.
    /// </summary>
    [Parameter] public bool IsNavbarFixed { get; set; }

    protected override void OnInitialized()
    {
        navigationManager.LocationChanged += OnLocationChanged;

        base.OnInitialized();
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        unfilteredNavigationItems = NavigationItems ?? [];
        ApplyFilter();
    }

    private async Task ToggleSidebar()
    {
        IsSidebarOpen = !IsSidebarOpen;

        if (IsSidebarOpenChanged.HasDelegate)
        {
            await IsSidebarOpenChanged.InvokeAsync(IsSidebarOpen);
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            navigationManager.LocationChanged -= OnLocationChanged;
        }
    }

    // A layout that hosts TwSidebar isn't re-rendered by client-side navigation (only the routed
    // content is), so without this the active-link highlight in TwSidebarItem would never refresh
    // to match the new URL. The drawer itself is only auto-closed on a mobile viewport - on desktop
    // IsSidebarOpen represents a persistent panel (see sidebarClasses, which has no lg: reset), so
    // closing it there on every navigation would hide the sidebar entirely instead of just the
    // transient mobile overlay.
    private void OnLocationChanged(object? sender, LocationChangedEventArgs e) => _ = HandleLocationChangedAsync(e.Location.Contains('#'));

    // The scrollable region is the main content div rather than the window, so the router's own
    // scroll-to-top on navigation never reaches it. Fragment navigations are left alone so in-page
    // anchor links keep scrolling to their target.
    private async Task HandleLocationChangedAsync(bool hasFragment)
    {
        if (!hasFragment)
        {
            await jsRuntime.InvokeVoidAsync("twSidebar.scrollToTop", mainContentRef);
        }

        if (IsSidebarOpen && await jsRuntime.InvokeAsync<bool>("twSidebar.isMobileViewport"))
        {
            IsSidebarOpen = false;

            if (IsSidebarOpenChanged.HasDelegate)
            {
                await IsSidebarOpenChanged.InvokeAsync(false);
            }
        }

        await InvokeAsync(StateHasChanged);
    }

    private string searchTerm { get; set; } = string.Empty;

    private void OnSearchInput(string value)
    {
        searchTerm = value;
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            displayedNavigationItems = unfilteredNavigationItems;
        }
        else
        {
            displayedNavigationItems = unfilteredNavigationItems
                .Select(item => FilterNavigationItem(item, searchTerm))
                .Where(item => item is not null)
                .Cast<NavigationItem>()
                .ToList();
        }
    }

    private static NavigationItem? FilterNavigationItem(NavigationItem item, string term)
    {
        var labelMatches = item.Label?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false;

        var matchingChildren = item.NavigationItems
            .Select(child => FilterNavigationItem(child, term))
            .Where(child => child is not null)
            .Cast<NavigationItem>()
            .ToList();

        if (labelMatches)
        {
            return new NavigationItem
            {
                Id = item.Id,
                Href = item.Href,
                Icon = item.Icon,
                Label = item.Label,
                Collapsed = item.Collapsed,
                TopNavigation = item.TopNavigation,
                Hidden = item.Hidden,
                NavigationItems = item.NavigationItems
            };
        }

        if (matchingChildren.Count > 0)
        {
            return new NavigationItem
            {
                Id = item.Id,
                Href = item.Href,
                Icon = item.Icon,
                Label = item.Label,
                Collapsed = false,
                TopNavigation = item.TopNavigation,
                Hidden = item.Hidden,
                NavigationItems = matchingChildren
            };
        }

        return null;
    }

    private string rootClasses =>
        new ClassBuilder(options.Theme.Position.Relative)
            .AddClass(options.Theme.Display.Flex)
            .AddClass(options.Theme.Sizing.FullWidth)
            .AddClass(options.Theme.Flexbox.Row)
            .AddClass(Class)
            .Build();

    private string skipLinkClasses =>
        new ClassBuilder(options.Theme.Display.ScreenReaderOnly)
            .AddClass(theme.SkipLink).Build();

    private string mobileOverlayClasses =>
        new ClassBuilder(options.Theme.Position.Fixed)
            .AddClass(theme.MobileOverlay).Build();

    private string navigationListClasses =>
        new ClassBuilder(options.Theme.Spacing.MarginTop.Lg)
            .AddClass(options.Theme.Display.Flex)
            .AddClass(options.Theme.Flexbox.Col)
            .AddClass(options.Theme.Spacing.Gap.Sm)
            .AddClass(theme.NavigationList).Build();

    private string sidebarClasses =>
        new ClassBuilder(theme.Sidebar)
            .AddClass(options.Theme.Position.Fixed)
            .AddClass(theme.MobileClosed, !IsSidebarOpen)
            .AddClass(theme.MobileOpen, IsSidebarOpen)
            .AddClass(SidebarClass).Build();

    private string mainContentClasses =>
        new ClassBuilder(theme.MainContent)
            .AddClass(options.Theme.Position.Absolute, IsNavbarFixed)
            .AddClass(options.Theme.Sizing.FullHeight, IsNavbarFixed)
            .AddClass(options.Theme.Position.Relative, !IsNavbarFixed)
            .AddClass(MainContentClass).Build();

    private string mainContentRootClasses =>
        new ClassBuilder(theme.MainContentRoot)
            .AddClass(MainContentRootClass).Build();

}
