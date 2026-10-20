// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Builders;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// Represents a tab within a tab container, allowing users to navigate between different content sections.
/// </summary>
/// <remarks>A TwTab is typically used as a child of a TwTabContainer component. The tab can be enabled or
/// disabled using the Disabled property, and its appearance can be customized with the Color property. The tab's label
/// is displayed to the user, and it is activated by the parent tab container. Only one tab is active at a time within a
/// container.</remarks>
public partial class TwTab : TwBlazorComponentBase
{
    /// <summary>
    /// The parent tab container that this tab belongs to. This property is required and is set via cascading parameters.
    /// </summary>
    [CascadingParameter] public required TwTabContainer Parent { get; set; }

    /// <summary>
    /// The label to display for this tab. This is a required parameter and is used to identify the tab in the user interface.
    /// </summary>
    [Parameter] public required string Label { get; set; }

    /// <summary>
    /// The content to be displayed when this tab is active. This is a required parameter and should contain the markup or components that make up the tab's content.
    /// </summary>
    [Parameter] public required RenderFragment ChildContent { get; set; }

    /// <summary>
    /// The color of the tab's text and underline indicator while it is the active tab. If not specified, it will inherit the color from the parent tab container.
    /// </summary>
    [Parameter] public Color? Color { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the component is disabled and cannot be interacted with by the user.
    /// </summary>
    /// <remarks>When set to <see langword="true"/>, the component will not respond to user input. This is
    /// typically used to prevent interaction during loading states or when certain conditions are not met.</remarks>
    [Parameter] public bool Disabled { get; set; }

    /// <summary>
    /// A reference to the rendered <see cref="TwButton"/> instance for this tab within the parent
    /// <see cref="TwTabContainer"/>'s tablist. Used to move keyboard focus to this tab (roving tabindex
    /// pattern) when it becomes active via arrow-key navigation.
    /// </summary>
    internal TwButton? buttonRef { get; set; }

    private TwTabTheme theme => options.Theme.Components.Require<TwTabTheme>();

    /// <summary>
    /// Gets the CSS classes that are applied to the tab element based on its current state and configuration.
    /// </summary>
    /// <remarks>The returned class string reflects the tab's active, disabled, dense and container states. Only the active
    /// tab is drawn in its <see cref="Color"/> (text, plus the underline indicator in container mode); inactive and disabled tabs use the
    /// theme's neutral text so the selected tab is the single colored item in the list.</remarks>
    public string TabClasses => new ClassBuilder(tabPadding)
        .AddClass(theme.TabBase)
        .AddClass(theme.IsolatedTab, !Parent.Container)
        .AddClass(tabTextColor, isActive && !Disabled)
        .AddClass(Parent.Container ? theme.ActiveIndicator : theme.IsolatedActiveTab, isActive && !Disabled)
        .AddClass(Parent.Container ? theme.InactiveIndicator : theme.IsolatedInactiveTab, !isActive && !Disabled)
        .AddClass(theme.DisabledTab, Disabled)
        .AddClass(Class).Build();

    private bool isActive => Parent.ActiveTab == this;

    private string tabPadding => Parent.Container
        ? (Parent.Dense ? theme.TabDensePadding : theme.TabPadding)
        : (Parent.Dense ? theme.IsolatedTabDensePadding : theme.IsolatedTabPadding);

    /// <summary>
    /// The active tab's text color classes. Unlike <see cref="ColorBuilder.GetTextColor"/>'s other consumer
    /// (<see cref="TwIcon"/>, which should inherit its surrounding element's color via
    /// <c>currentColor</c> when no explicit <see cref="Color"/> is set), a tab button has no useful
    /// color to inherit, so it falls back to the same brand-purple pairing an explicit
    /// <see cref="Enums.Color.Primary"/> would produce (<c>text-purple-600 dark:text-purple-200</c>)
    /// instead of an empty string.
    /// </summary>
    private string tabTextColor
    {
        get
        {
            var color = colorBuilder.GetTextColor(Color);
            return string.IsNullOrEmpty(color) ? colorBuilder.GetTextColor(Enums.Color.Primary) : color;
        }
    }

    protected override void OnInitialized()
    {
        Color ??= Parent.TabColor;
        Parent.ActivateTab(this);
        base.OnInitialized();
    }
}