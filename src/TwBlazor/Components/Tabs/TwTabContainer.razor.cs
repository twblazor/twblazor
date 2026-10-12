// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// Represents a tabbed interface component that allows users to organize and switch between multiple content sections
/// within a user interface.
/// </summary>
/// <remarks>Use this component to create a set of tabs, each displaying different content. The active tab is
/// managed internally, and child content for each tab should be provided using the ChildContent parameter. This
/// component is typically used to improve navigation and organization of related content in a single view.</remarks>
public partial class TwTabContainer : TwBlazorComponentBase, IAsyncDisposable
{
    [Inject] private IJSRuntime jSRuntime { get; set; } = null!;

    /// <summary>
    /// Gets or sets a reference to the tablist element (the div with role="tablist"), used to register a
    /// native JS keydown listener that selectively suppresses the browser's default scroll behavior for
    /// arrow-key/Home/End navigation. See <see cref="OnAfterRenderAsync"/> for why this can't be done with
    /// the declarative <c>@onkeydown:preventDefault</c> directive alone.
    /// </summary>
    private ElementReference tablistElement;

    private bool keydownGuardRegistered;

    /// <summary>
    /// Gets or sets a reference to the tabs element in the user interface.
    /// </summary>
    /// <remarks>This property enables direct interaction with the underlying tabs element, such as for
    /// JavaScript interop or DOM manipulation. Ensure that the element is initialized before accessing this property,
    /// especially when using it in lifecycle methods.</remarks>
    public ElementReference TabsElement { get; set; }

    /// <summary>
    /// Gets or sets the color of all the tabs associated with the component.
    /// </summary>
    /// <remarks>If set to <see langword="null"/>, the default tab color is used. This property allows
    /// customization of the tab's appearance to match application themes or user preferences.</remarks>
    [Parameter] public Color? TabColor { get; set; }

    /// <summary>
    /// Gets or sets whether the tabs should use dense (compact) styling with reduced padding.
    /// </summary>
    /// <remarks>When set to <see langword="true"/>, tabs will have reduced padding for a more compact appearance.
    /// This is useful when space is limited or when displaying many tabs.</remarks>
    [Parameter] public bool Dense { get; set; }

    /// <summary>
    /// Gets or sets the content to be rendered inside the component.
    /// </summary>
    /// <remarks>This property is required and must be set to specify the child elements or markup that will
    /// be displayed within the component.</remarks>
    [Parameter] public required RenderFragment ChildContent { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes for the tabs navigation container.
    /// </summary>
    /// <remarks>
    /// This is the container wrapping the tab navigation buttons.
    /// </remarks>
    [Parameter] public string? TabContainerClass { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes for the tab content area.
    /// </summary>
    /// <remarks>
    /// This is the container wrapping the tab content area.
    /// </remarks>
    [Parameter] public string? ContainerClass { get; set; }

    /// <summary>
    /// Gets or sets whether the background class should be removed from the tab navigation and content
    /// containers, leaving only the border outline.
    /// </summary>
    /// <remarks>Useful when the tabs are placed inside another container that already provides a
    /// background, and the default white/dark background would otherwise clash with it.</remarks>
    [Parameter] public bool TransparentContainer { get; set; }

    private readonly List<TwTab> _tabs = [];

    public TwTab? ActiveTab { get; private set; }

    private TwTabTheme theme => options.Theme.Components.Require<TwTabTheme>();

    private string tabContainerClasses => new ClassBuilder(theme.TabListContainer)
        .AddClass(theme.Background, !TransparentContainer)
        .AddClass($"{roundedBuilder.GetRoundedTop(options.Theme.Rounded.DefaultRounded)}")
        .AddClass(TabContainerClass ?? string.Empty).Build();

    private string containerClasses => new ClassBuilder(theme.PanelContainer)
        .AddClass(theme.Background, !TransparentContainer)
        .AddClass($"{roundedBuilder.GetRoundedBottom(options.Theme.Rounded.DefaultRounded)}")
        .AddClass(ContainerClass ?? string.Empty).Build();

    /// <summary>
    /// Gets the id of the (single, shared) tab panel element, derived from the tab container's own stable
    /// <see cref="TwBlazorComponentBase.Id"/> - deliberately NOT from whichever tab happens to be active.
    /// Every tab's <c>aria-controls</c> attribute references this same id, and it is also used as the
    /// panel's own <c>id</c>. Because there is only one physical panel element whose content swaps when
    /// the active tab changes, that id must stay stable across tab switches; if it were derived from
    /// <see cref="ActiveTab"/> instead, every inactive tab's <c>aria-controls</c> would end up pointing at
    /// whichever *other* tab is currently active rather than at a stable reference to the one real panel.
    /// </summary>
    private string panelId => $"{Id}-panel";

    protected override void OnInitialized()
    {
        _tabs.Clear();
    }

    /// <summary>
    /// Activates the specified tab and adds it to the collection of tabs. If no tab is currently active, the specified
    /// tab becomes the active tab.
    /// </summary>
    /// <remarks>This method ensures that the specified tab is part of the active tab collection. If there is
    /// no active tab when this method is called, the provided tab will be set as the active tab.</remarks>
    /// <param name="tab">The tab to be activated and added to the collection of tabs. Cannot be null.</param>
    public void ActivateTab(TwTab tab)
    {
        _tabs.Add(tab);

        // The initially active tab must be an *enabled* one. The active tab gets tabindex="0" (roving
        // tabindex), but a disabled tab also gets the native `disabled` attribute (via TwButton), and
        // native `disabled` always wins over tabindex - so if the first tab declared in markup were
        // disabled and still became ActiveTab, tabindex="0" would sit on an unfocusable element while
        // every other tab sits at tabindex="-1", leaving no tab in the whole list reachable via the Tab
        // key. Skip disabled tabs when picking the initial active tab: promote the first *enabled* tab
        // that registers, even if an earlier disabled tab was provisionally made active first (tabs
        // register one at a time, in markup order, as each TwTab's OnInitialized runs).
        //
        // Edge case: if every tab turns out to be disabled, fall back to leaving the very first
        // registered tab active rather than leaving ActiveTab null - a tablist with no active tab at all
        // would never render tabindex="0" or aria-selected="true" anywhere, which is equally broken. If a
        // later tab happens to be enabled, it still gets promoted over that fallback.
        if (ActiveTab == null || (ActiveTab.Disabled && !tab.Disabled))
        {
            ActiveTab = tab;
        }

        StateHasChanged();
    }

    /// <summary>
    /// Sets the specified tab as the currently active tab.
    /// </summary>
    /// <param name="tab">The tab to set as active. This parameter cannot be null.</param>
    public void ViewTab(TwTab tab)
    {
        if (tab.Disabled)
        {
            return;
        }

        ActiveTab = tab;
        StateHasChanged();
    }

    /// <summary>
    /// Registers a native JS keydown listener on the tablist that selectively calls
    /// <c>event.preventDefault()</c> only for the arrow-key/Home/End tablist navigation keys, so pressing
    /// them doesn't also trigger the browser's native scroll (Home/End can scroll to the top/bottom of the
    /// page, ArrowUp/Down can scroll it too).
    /// </summary>
    /// <remarks>
    /// This can't be done with the declarative <c>@onkeydown:preventDefault</c> directive: that directive
    /// is a static, per-render-cycle binding that isn't evaluated per-keystroke - setting it to
    /// <c>true</c> would call <c>preventDefault()</c> for <em>every</em> keydown on the tablist, including
    /// Tab. Tab's default browser action is "move focus to the next focusable element", and
    /// <see cref="HandleTabKeyDown"/> has no case for it (it falls through to <c>_ => null</c> and
    /// returns early), so blanket-preventing default would suppress that focus-move with nothing to
    /// replace it - trapping keyboard focus inside the tablist, which is strictly worse than the scroll
    /// nuisance this is meant to fix. Registering a native listener that inspects <c>e.key</c> before
    /// deciding whether to prevent default (mirroring the JS-side event-handling pattern already used by
    /// <c>twDialog</c> for its Tab focus trap) avoids that: it runs alongside Blazor's own
    /// <c>@onkeydown="HandleTabKeyDown"</c> binding (which still does the actual tab-switching logic in
    /// C#) and only ever calls preventDefault for ArrowRight/ArrowLeft/ArrowUp/ArrowDown/Home/End, leaving
    /// Tab (and every other key) completely untouched.
    /// </remarks>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            try
            {
                await jSRuntime.InvokeVoidAsync("twTabs.registerKeydownGuard", tablistElement);
                keydownGuardRegistered = true;
            }
            catch (JSDisconnectedException)
            {
                // The circuit disconnected before the script could run; nothing to register.
            }
        }
    }

    /// <summary>
    /// Releases the JS-side keydown guard registered for this tablist.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (!keydownGuardRegistered)
        {
            return;
        }

        try
        {
            await jSRuntime.InvokeVoidAsync("twTabs.unregisterKeydownGuard", tablistElement);
        }
        catch (JSDisconnectedException)
        {
            // The circuit is already gone; nothing left to clean up.
        }
        catch (InvalidOperationException)
        {
            // JS interop unavailable during teardown (e.g. prerendering); safe to ignore.
        }
        finally
        {
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// Implements the WAI-ARIA APG "Tabs" keyboard interaction pattern with automatic activation: ArrowRight/
    /// ArrowDown move to the next enabled tab (wrapping), ArrowLeft/ArrowUp move to the previous enabled tab
    /// (wrapping), Home/End move to the first/last enabled tab. Moving focus also selects the tab and shows
    /// its panel immediately, and keyboard focus is moved to the newly-active tab's rendered element so that
    /// focus visibly follows the selection. In a right to left layout ArrowLeft and ArrowRight swap.
    /// </summary>
    /// <param name="e">The keyboard event args from the tablist's <c>onkeydown</c> handler.</param>
    private async Task HandleTabKeyDown(KeyboardEventArgs e)
    {
        var target = ToLogicalKey(e.Key) switch
        {
            "ArrowRight" or "ArrowDown" => GetAdjacentEnabledTab(1),
            "ArrowLeft" or "ArrowUp" => GetAdjacentEnabledTab(-1),
            "Home" => _tabs.FirstOrDefault(t => !t.Disabled),
            "End" => _tabs.LastOrDefault(t => !t.Disabled),
            _ => null,
        };

        if (target == null || target == ActiveTab)
        {
            return;
        }

        ActiveTab = target;
        StateHasChanged();

        if (target.buttonRef != null)
        {
            await target.buttonRef.FocusAsync();
        }
    }

    /// <summary>
    /// Finds the next enabled tab relative to <see cref="ActiveTab"/> in the given direction, wrapping around
    /// the ends of the tab list. Disabled tabs are skipped over.
    /// </summary>
    /// <param name="direction">1 to move forward (next), -1 to move backward (previous).</param>
    private TwTab? GetAdjacentEnabledTab(int direction)
    {
        if (ActiveTab == null || _tabs.Count == 0)
        {
            return null;
        }

        var currentIndex = _tabs.IndexOf(ActiveTab);
        if (currentIndex < 0)
        {
            return null;
        }

        for (var offset = 1; offset <= _tabs.Count; offset++)
        {
            var nextIndex = ((currentIndex + (direction * offset)) % _tabs.Count + _tabs.Count) % _tabs.Count;
            if (!_tabs[nextIndex].Disabled)
            {
                return _tabs[nextIndex];
            }
        }

        return null;
    }
}
