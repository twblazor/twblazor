// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Builders;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// Represents an alert component that displays informational messages to users.
/// </summary>
/// <remarks>
/// The TwAlert component is a flexible notification element that can display text or custom content
/// with optional icons and a dismissible close button. It supports various color schemes through
/// the Color parameter and can be shown or hidden using the Dismissed parameter.
/// </remarks>
public partial class TwAlert : TwBlazorComponentBase
{
    private TwAlertTheme theme => options.Theme.Components.Require<TwAlertTheme>();

    /// <summary>
    /// Gets or sets the text content to display in the alert.
    /// </summary>
    /// <remarks>
    /// This text is displayed when no ChildContent is provided. If both Text and ChildContent are set,
    /// ChildContent takes precedence.
    /// </remarks>
    [Parameter] public string? Text { get; set; }

    /// <summary>
    /// Gets or sets the icon to display at the start of the alert.
    /// </summary>
    [Parameter] public Icon? StartIcon { get; set; }

    /// <summary>
    /// Gets or sets the icon to display at the end of the alert.
    /// </summary>
    [Parameter] public Icon? EndIcon { get; set; }

    /// <summary>
    /// Gets or sets the Tailwind padding classes between the start icon and the text.
    /// Defaults to the theme's small padding when not set.
    /// </summary>
    [Parameter] public string? StartIconPadding { get; set; }

    /// <summary>
    /// Gets or sets the Tailwind padding classes between the text and the end icon.
    /// Defaults to the theme's small padding when not set.
    /// </summary>
    [Parameter] public string? EndIconPadding { get; set; }

    /// <summary>
    /// Gets or sets the callback that is invoked when the alert is dismissed.
    /// </summary>
    [Parameter] public EventCallback OnDismiss { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the alert can be dismissed by the user.
    /// </summary>
    /// <remarks>
    /// When set to true, a close button will be displayed that allows the user to dismiss the alert.
    /// </remarks>
    [Parameter] public bool Dismissible { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the alert is currently dismissed (hidden).
    /// </summary>
    [Parameter] public bool Dismissed { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the alert should use compact padding.
    /// </summary>
    [Parameter] public bool Dense { get; set; }

    /// <summary>
    /// Gets or sets the callback that is invoked when the Dismissed state changes.
    /// </summary>
    [Parameter] public EventCallback<bool> DismissedChanged { get; set; }

    /// <summary>
    /// Gets or sets the custom content to display in the alert.
    /// </summary>
    /// <remarks>
    /// When provided, this content replaces the Text parameter.
    /// </remarks>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the color scheme for the alert.
    /// </summary>
    [Parameter] public Color? Color { get; set; }

    private string classes =>
        new ClassBuilder("tw-alert")
        .AddClass(shadowBuilder.GetShadow(effectiveShadow))
        .AddClass(roundedBuilder.GetRounded(effectiveRounded))
        .AddClass(GetAlertColor(Color))
        .AddClass(options.Theme.Display.Hidden, Dismissed)
        .AddClass(theme.DensePadding, Dense)
        .AddClass(theme.Padding, !Dense)
        .AddClass(options.Theme.Display.Flex, !Dismissed)
        .AddClass(options.Theme.Flexbox.Align.Center, !Dismissed)
        .AddClass(options.Theme.Spacing.Gap.Sm, !Dismissed)
        .AddClass(theme.Transition, !Dismissed)
        .AddClass(Class)
        .Build();

    private string startIconClasses => StartIconPadding ?? theme.StartIconPadding;

    private string endIconClasses =>
        new ClassBuilder(options.Theme.Spacing.PushEnd)
        .AddClass(EndIconPadding ?? theme.EndIconPadding)
        .Build();

    private string dismissButtonClasses =>
        new ClassBuilder(options.Theme.Display.InlineFlex)
        .AddClass(options.Theme.Flexbox.Align.Center)
        .AddClass(options.Theme.Flexbox.Justify.Center)
        .AddClass(theme.DismissButtonSize)
        .AddClass(EndIcon is null ? options.Theme.Spacing.PushEnd : theme.DismissButtonSpacingWithEndIcon)
        .AddClass(theme.DismissButtonColor)
        .Build();

    private string GetAlertColor(Color? color) => ColorBuilder.GetPaletteColor(color, theme.Colors, theme.Colors.Primary);

    private async Task HandleDismiss()
    {
        Dismissed = true;

        if (DismissedChanged.HasDelegate)
        {
            await DismissedChanged.InvokeAsync(Dismissed);
        }

        if (OnDismiss.HasDelegate)
        {
            await OnDismiss.InvokeAsync();
        }

        StateHasChanged();
    }
}
