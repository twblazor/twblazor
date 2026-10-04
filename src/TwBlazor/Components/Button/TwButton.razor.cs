// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using TwBlazor.Builders;
using TwBlazor.Configuration;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Utilities;
using Color = TwBlazor.Enums.Color;

namespace TwBlazor.Components;

/// <summary>
/// Represents a configurable button component that supports custom content, navigation, and event handling in a Blazor
/// application.
/// </summary>
/// <remarks>TwButton provides a flexible button element that can display a text label or custom content, handle
/// click events, and optionally perform navigation using a specified URL. The component supports standard button types,
/// color customization, and can be disabled to prevent user interaction. When the Href property is set, the button
/// renders as a link and uses NavigationManager for navigation. If both Label and ChildContent are provided,
/// ChildContent takes precedence and replaces the label. The component is designed for use within Blazor applications
/// and integrates with the Blazor event and navigation systems.</remarks>
public partial class TwButton : TwBlazorComponentBase
{
    [Inject] public NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private ButtonBuilder buttonBuilder { get; set; } = null!;

    private TwButtonTheme theme => options.Theme.Components.Require<TwButtonTheme>();

    /// <summary>
    /// Gets or sets the callback that is invoked when the button is clicked.
    /// </summary>
    /// <remarks>Assign an event handler to this property to respond to click events. The callback is
    /// triggered when the user interacts with the component in a way that constitutes a click, such as pressing a mouse
    /// button or tapping on a touch device.</remarks>
    [Parameter] public EventCallback OnClick { get; set; }

    /// <summary>
    /// Gets or sets the text label to display for the button.
    /// </summary>
    [Parameter] public string Label { get; set; } = string.Empty;

    /// <summary>
    /// The type of button (e.g., "button", "submit", "reset").
    /// </summary>
    /// <remarks>
    /// The default is "button".
    /// </remarks>
    [Parameter] public string Type { get; set; } = "button";

    /// <summary>
    /// Gets or sets the color theme of the button.
    /// </summary>
    [Parameter] public Color? Color { get; set; }

    /// <summary>
    /// Gets or sets the button variant.
    /// </summary>
    /// <remarks>
    /// If not set, uses the global default from <see cref="TwBlazorRounded.DefaultRounded"/>.
    /// Default is <see cref="ButtonVariant.Filled"/> (high emphasis).
    /// </remarks>
    [Parameter] public ButtonVariant? Variant { get; set; }

    /// <summary>
    /// Gets or sets whether to display an icon at the start of the button.
    /// </summary>
    [Parameter] public Icon? StartIcon { get; set; }

    /// <summary>
    /// Gets or sets whether to display an icon at the end of the button.
    /// </summary>
    [Parameter] public Icon? EndIcon { get; set; }

    /// <summary>
    /// Sets the button to use <see cref="NavigationManager"/>.
    /// </summary>
    [Parameter] public string Href { get; set; } = string.Empty;

    /// <summary>
    /// Disables the button if set to true.
    /// </summary>
    [Parameter] public bool Disabled { get; set; } = false;

    /// <summary>
    /// Makes the button readonly if set to true.
    /// </summary>
    /// <remarks>When true, the button cannot be clicked but will still be visible.</remarks>
    [Parameter] public bool Readonly { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether the component should be displayed in a dense layout.
    /// </summary>
    /// <remarks>When set to <see langword="true"/>, the component uses a more compact layout, reducing
    /// spacing between elements. This can be useful for displaying more information in a limited space.</remarks>
    [Parameter] public bool Dense { get; set; } = false;

    /// <summary>
    /// Gets or sets the custom content to display in the button.
    /// </summary>
    /// <remarks>
    /// When provided, this content replaces the Label parameter.
    /// </remarks>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the name of the target frame or window for the navigation request.
    /// </summary>
    [Parameter] public string Target { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether padding is removed from the content.
    /// </summary>
    /// <remarks>When set to <see langword="true"/>, the content is displayed without additional padding,
    /// which may affect the layout and appearance of the component.</remarks>
    [Parameter] public bool IconButton { get; set; }

    /// <summary>
    /// When true, skips the button's base structural classes (sizing/shape), typography, variant/color
    /// classes, focus ring, and shadow - leaving only <see cref="TwBlazorComponentBase.Class"/> (and the
    /// disabled/readonly cursor class) applied.
    /// </summary>
    /// <remarks>
    /// Useful for embedding a button inside another already-styled control (e.g. a chip's close icon)
    /// where the caller wants full control over appearance instead of TwButton's default chrome.
    /// </remarks>
    [Parameter] public bool Plain { get; set; }

    /// <summary>
    /// Gets or sets the text of a tooltip shown when the button is hovered or keyboard-focused.
    /// </summary>
    /// <remarks>
    /// The tooltip is rendered as a <c>role="tooltip"</c> element linked to the button via
    /// <c>aria-describedby</c>, and can be dismissed with Escape without moving focus. Because it only
    /// supplements the button's accessible name, icon-only buttons still need an
    /// <see cref="TwBlazorComponentBase.AriaLabel"/>. Leave unset (the default) to render no tooltip.
    /// </remarks>
    [Parameter] public string? Tooltip { get; set; }

    /// <summary>
    /// Gets a reference to the rendered <c>&lt;button&gt;</c> element, when this instance renders as a button
    /// rather than a link (i.e. <see cref="Href"/> is not set). Used internally by components (such as
    /// <see cref="TwTabContainer"/>) that need to move keyboard focus programmatically, e.g. for roving-tabindex
    /// keyboard navigation patterns.
    /// </summary>
    internal ElementReference element { get; set; }

    /// <summary>
    /// Moves keyboard focus to this button's rendered element.
    /// </summary>
    internal async Task FocusAsync()
    {
        await element.FocusAsync();
    }

    /// <summary>
    /// Gets the effective button variant to use.
    /// </summary>
    private ButtonVariant effectiveVariant => Variant ?? theme.DefaultVariant ?? ButtonVariant.Filled;

    /// <summary>
    /// Gets whether the button is interactive (not disabled or readonly).
    /// </summary>
    private bool isInteractive => !Disabled && !Readonly;

    /// <summary>
    /// Gets the CSS classes applied to the button element.
    /// </summary>
    private string classes => Plain
        ? new ClassBuilder(buttonBuilder.GetCursorClasses(Disabled, Readonly))
            .AddClass(Class)
            .Build()
        : new ClassBuilder()
            .AddClass(buttonBuilder.GetBaseClasses(IconButton, Dense, Rounded))
            .AddClass(buttonBuilder.GetTypographyClasses(theme.ButtonUppercase))
            .AddClass(buttonBuilder.GetVariantClasses(effectiveVariant, Color, !isInteractive, Shadow))
            .AddClass(colorBuilder.GetFocusRing(Color))
            .AddClass(buttonBuilder.GetCursorClasses(Disabled, Readonly))
            .AddClass(shadowBuilder.GetButtonShadow(theme, Shadow))
            .AddClass(Class)
            .Build();

    private bool hasTooltip => !string.IsNullOrWhiteSpace(Tooltip);

    private string tooltipId => $"{Id}-tooltip";

    /// <summary>
    /// Set when the user presses Escape while the tooltip is showing, hiding it until the pointer
    /// leaves or focus moves away (WCAG 1.4.13 "dismissible").
    /// </summary>
    private bool tooltipDismissed;

    private string tooltipWrapperClasses => popoverBuilder.GetTooltipWrapperClasses();

    private string tooltipClasses => popoverBuilder.GetTooltipClasses(tooltipDismissed);

    /// <summary>
    /// Attributes forwarded to the inner <see cref="TwLink"/> when <see cref="Href"/> is set, with the
    /// tooltip's <c>aria-describedby</c> link added.
    /// </summary>
    private Dictionary<string, object> linkAttributes
    {
        get
        {
            var attributes = new Dictionary<string, object>(Attributes);
            if (hasTooltip)
            {
                attributes["aria-describedby"] = tooltipId;
            }

            return attributes;
        }
    }

    private void OnTooltipKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Escape")
        {
            tooltipDismissed = true;
        }
    }

    private void ResetTooltipDismissal() => tooltipDismissed = false;

    private string startIconClasses => new ClassBuilder(theme.IconTypography)
        .AddClass(options.Theme.Spacing.MarginEnd.Md).Build();

    private string endIconClasses => new ClassBuilder(theme.IconTypography)
        .AddClass(options.Theme.Spacing.MarginStart.Md).Build();

    /// <summary>
    /// Handles the click event asynchronously, invoking the associated callback if the component is enabled.
    /// </summary>
    private async Task OnClickAsync()
    {
        if (Disabled || Readonly)
        {
            return;
        }

        if (OnClick.HasDelegate)
        {
            await OnClick.InvokeAsync();
        }
    }
}
