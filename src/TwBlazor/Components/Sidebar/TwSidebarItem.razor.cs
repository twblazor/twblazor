// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Models;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// Represents a navigation item in a sidebar that can be either a parent item with collapsible children or a link item.
/// </summary>
/// <remarks>
/// The TwSidebarItem component renders differently based on the <see cref="IsParent"/> property:
/// <list type="bullet">
/// <item><description>When <see cref="IsParent"/> is true, it renders as a button with a chevron icon that can toggle child items.</description></item>
/// <item><description>When <see cref="IsParent"/> is false, it renders as a navigation link.</description></item>
/// </list>
/// The component can be initialized using either the <see cref="Href"/> and <see cref="Label"/> parameters directly,
/// or by providing a <see cref="NavigationItem"/> object which will populate these values automatically.
/// </remarks>
public partial class TwSidebarItem : TwBlazorComponentBase
{
    [Inject] private NavigationManager navigationManager { get; set; } = default!;

    private TwSidebarTheme theme => options.Theme.Components.Require<TwSidebarTheme>();

    /// <summary>
    /// Gets or sets whether this sidebar item is a parent item that can have collapsible children.
    /// </summary>
    /// <remarks>
    /// When true, the item is rendered as a button with a chevron icon. When false, it is rendered as a link.
    /// </remarks>
    [Parameter] public bool IsParent { get; set; }

    /// <summary>
    /// Gets or sets whether the parent item's children are collapsed.
    /// </summary>
    /// <remarks>
    /// This property only affects the visual state when <see cref="IsParent"/> is true. 
    /// It controls the rotation of the chevron icon to indicate the collapsed/expanded state.
    /// </remarks>
    [Parameter] public bool IsCollapsed { get; set; }

    /// <summary>
    /// Gets or sets the id of the collapsible child-item container that this item's toggle button controls.
    /// </summary>
    /// <remarks>
    /// Only relevant when <see cref="IsParent"/> is true. Rendered as the toggle button's <c>aria-controls</c>
    /// value so assistive technology can associate the button with the child list it expands/collapses.
    /// </remarks>
    [Parameter] public string? ChildrenId { get; set; }

    /// <summary>
    /// Gets or sets the callback that is invoked when the sidebar item is clicked.
    /// </summary>
    /// <remarks>
    /// This callback is typically used with parent items to toggle the visibility of child navigation items.
    /// </remarks>
    [Parameter] public EventCallback OnClick { get; set; }

    /// <summary>
    /// Gets or sets the URL that the sidebar item navigates to.
    /// </summary>
    /// <remarks>
    /// This property is required unless a <see cref="NavigationItem"/> is provided that contains a Href value.
    /// When <see cref="IsParent"/> is false, this value is used in the anchor tag's href attribute.
    /// </remarks>
    [Parameter] public required string Href { get; set; }

    /// <summary>
    /// Gets or sets the display text for the sidebar item.
    /// </summary>
    /// <remarks>
    /// This property is required unless a <see cref="NavigationItem"/> is provided that contains a Label value.
    /// </remarks>
    [Parameter] public required string Label { get; set; }

    /// <summary>
    /// Gets or sets whether this sidebar item is a child within a parent group.
    /// </summary>
    /// <remarks>
    /// When true, no rounding is applied unless this is also the last child.
    /// </remarks>
    [Parameter] public bool IsChild { get; set; }

    /// <summary>
    /// Gets or sets whether this sidebar item is the last child in a parent's child list.
    /// </summary>
    /// <remarks>
    /// When true, bottom rounded corners are applied to visually close the parent group.
    /// </remarks>
    [Parameter] public bool IsLastChild { get; set; }

    /// <summary>
    /// Gets or sets how many levels deep this item is nested (0 for a top-level item, 1 for its
    /// children, 2 for its grandchildren, and so on).
    /// </summary>
    [Parameter] public int Depth { get; set; }

    /// <summary>
    /// Gets or sets the child content to render within the sidebar item.
    /// </summary>
    /// <remarks>
    /// This is typically used to render nested <see cref="TwSidebarItem"/> components when <see cref="IsParent"/> is true.
    /// </remarks>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the navigation item model that provides the sidebar item's properties.
    /// </summary>
    /// <remarks>
    /// When provided, the <see cref="Href"/> and <see cref="Label"/> properties are automatically populated
    /// from the navigation item during initialization. This allows for easier integration with navigation data models.
    /// </remarks>
    [Parameter] public NavigationItem? NavigationItem { get; set; }

    /// <summary>
    /// Initializes the component and populates properties from the <see cref="NavigationItem"/> if provided.
    /// </summary>
    /// <remarks>
    /// If <see cref="NavigationItem"/> is not null, the <see cref="Href"/> and <see cref="Label"/> properties
    /// are set from the navigation item's corresponding properties, defaulting to empty strings if the values are null.
    /// </remarks>
    protected override void OnParametersSet()
    {
        if (NavigationItem != null)
        {
            Href = NavigationItem.Href ?? string.Empty;
            Label = NavigationItem.Label ?? string.Empty;
        }

        base.OnParametersSet();
    }

    /// <summary>
    /// Gets the CSS classes applied to parent sidebar items.
    /// </summary>
    /// <remarks>
    /// Includes styling for flex layout, spacing, typography, focus states, and dark mode support.
    /// </remarks>
    private string toggleIconClasses =>
        new ClassBuilder(options.Theme.Sizing.Icon.Md)
        .AddClass(theme.NavigationItemToggleIcon)
        .AddClass(IsCollapsed ? "rotate-0" : "rotate-180")
        .AddClass(options.Theme.Flexbox.ShrinkNone).Build();

    private string parentClasses =>
        new ClassBuilder(theme.NavigationItemBase)
        .AddClass(theme.NavigationItemActive, !IsCollapsed)
        .AddClass(theme.NavigationItemParent, Depth == 0 || !IsCollapsed)
        .AddClass(theme.NavigationItemActiveLevelDeep, !IsCollapsed && Depth >= 2)
        .AddClass(roundedBuilder.GetRounded(effectiveRounded), IsCollapsed)
        .AddClass(roundedBuilder.GetRoundedTop(effectiveRounded), !IsCollapsed)
        .AddClass(options.Theme.Rounded.RoundedBottom.None, !IsCollapsed)
        .AddClass(Class).Build();

    /// <summary>
    /// Gets the CSS classes applied to regular (non-parent) sidebar items.
    /// </summary>
    /// <remarks>
    /// Includes styling for flex layout, spacing, typography, hover states, focus states, and dark mode support.
    /// </remarks>
    private string classes =>
        new ClassBuilder(theme.NavigationItemBase)
        .AddClass(theme.NavigationItemActive, isActive)
        .AddClass(roundedBuilder.GetRounded(effectiveRounded), !IsChild)
        .AddClass(roundedBuilder.GetRoundedBottom(effectiveRounded), IsChild && IsLastChild)
        .AddClass(Class).Build();

    /// <summary>
    /// Determines if this navigation item is the currently active page.
    /// </summary>
    private bool isActive
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Href))
                return false;

            var currentUri = navigationManager.ToBaseRelativePath(navigationManager.Uri);
            var itemUri = Href.TrimStart('/');

            return string.Equals(currentUri, itemUri, StringComparison.OrdinalIgnoreCase);
        }
    }
}
