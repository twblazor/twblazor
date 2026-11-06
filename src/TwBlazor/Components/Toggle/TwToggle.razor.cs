// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Builders;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Utilities;
using Color = TwBlazor.Enums.Color;

namespace TwBlazor.Components;

/// <summary>
/// A two-state button that is either pressed (on) or not pressed (off), such as a bold button in a text
/// formatting toolbar.
/// </summary>
/// <remarks>
/// Renders a native button with <c>aria-pressed</c>, so screen readers announce it as a toggle button and
/// Enter or Space switches it. Bind the state with <c>@bind-Pressed</c>. The pressed state is also exposed as
/// <c>data-state="on"</c> or <c>data-state="off"</c>, so a <see cref="TwBlazorComponentBase.Class"/> can style
/// it with Tailwind's <c>data-[state=on]:</c> variant. A toggle that shows only an icon needs an
/// <see cref="TwBlazorComponentBase.AriaLabel"/>; keep the label the same in both states, since the pressed
/// state is announced separately. Use <see cref="TwToggleGroup{T}"/> for a set of related toggles.
/// </remarks>
public partial class TwToggle : TwBlazorComponentBase
{
    private TwToggleTheme theme => options.Theme.Components.Require<TwToggleTheme>();

    /// <summary>
    /// Gets or sets whether the toggle is pressed (on).
    /// </summary>
    [Parameter] public bool Pressed { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked with the new state when the toggle is pressed or released.
    /// </summary>
    [Parameter] public EventCallback<bool> PressedChanged { get; set; }

    /// <summary>
    /// Gets or sets the text shown in the toggle. Ignored when <see cref="ChildContent"/> is set.
    /// </summary>
    [Parameter] public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the icon shown before the label, or on its own for an icon-only toggle.
    /// </summary>
    [Parameter] public Icon? Icon { get; set; }

    /// <summary>
    /// Gets or sets custom content shown in the toggle in place of <see cref="Label"/>.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets how the toggle is drawn while it is not pressed.
    /// </summary>
    /// <remarks>If not set, uses <see cref="TwToggleTheme.DefaultVariant"/>, which defaults to
    /// <see cref="ToggleVariant.Text"/>.</remarks>
    [Parameter] public ToggleVariant? Variant { get; set; }

    /// <summary>
    /// Gets or sets the size of the toggle. Defaults to <see cref="Enums.Size.Md"/>.
    /// </summary>
    [Parameter] public Size? Size { get; set; }

    /// <summary>
    /// Gets or sets the color of the toggle while it is pressed, and of its focus ring.
    /// </summary>
    /// <remarks>If not set, a pressed toggle is drawn in a neutral tint.</remarks>
    [Parameter] public Color? Color { get; set; }

    /// <summary>
    /// Gets or sets whether the toggle is disabled. A disabled toggle cannot be focused or switched.
    /// </summary>
    [Parameter] public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets the text of a tooltip shown when the toggle is hovered or keyboard-focused.
    /// </summary>
    /// <remarks>The tooltip only describes the toggle, so an icon-only toggle still needs an
    /// <see cref="TwBlazorComponentBase.AriaLabel"/>.</remarks>
    [Parameter] public string? Tooltip { get; set; }

    /// <summary>
    /// Gets or sets the context supplied by an enclosing <see cref="TwToggleGroupItem{T}"/> when this toggle is
    /// one of a toggle group's items.
    /// </summary>
    [CascadingParameter] private TwToggleGroupItemContext? groupItem { get; set; }

    private TwButton? button;

    private bool inGroup => groupItem is not null;

    private ToggleVariant effectiveVariant => Variant ?? theme.DefaultVariant ?? ToggleVariant.Text;

    private Size effectiveSize => Size ?? Enums.Size.Md;

    private Rounded effectiveToggleRounded => Rounded ?? theme.ToggleRounded ?? options.Theme.Rounded.DefaultRounded;

    private string sizeClasses => effectiveSize switch
    {
        Enums.Size.Sm => theme.Sm,
        Enums.Size.Lg => theme.Lg,
        _ => theme.Md
    };

    private string iconClasses => effectiveSize switch
    {
        Enums.Size.Sm => theme.IconSm,
        Enums.Size.Lg => theme.IconLg,
        _ => theme.IconMd
    };

    /// <summary>
    /// The pressed colors: the <see cref="Color"/>'s palette entry when one is set, otherwise the neutral tint,
    /// which on the filled bar of a <see cref="ToggleVariant.Text"/> group is a raised surface instead.
    /// </summary>
    private string pressedClasses => GetPressedClasses();

    private string GetPressedClasses()
    {
        if (Color.HasValue)
        {
            return ColorBuilder.GetPaletteColor(Color, theme.Colors, theme.Pressed);
        }

        return inGroup && effectiveVariant == ToggleVariant.Text ? theme.GroupPressed : theme.Pressed;
    }

    private string classes =>
        new ClassBuilder(theme.Base)
        .AddClass(sizeClasses)
        .AddClass(roundedBuilder.GetRounded(effectiveToggleRounded))
        .AddClass(Pressed ? pressedClasses : theme.Unpressed)
        .AddClass(theme.PressedForcedColors, Pressed)
        .AddClass(theme.Hover, !Pressed && !Disabled)
        // In a group the border goes around the whole group rather than each item.
        .AddClass(theme.Outlined, effectiveVariant == ToggleVariant.Outlined && !inGroup)
        .AddClass(theme.Disabled, Disabled)
        .AddClass(colorBuilder.GetFocusRing(Color))
        .AddClass(shadowBuilder.GetShadow(Shadow))
        .AddClass(Class)
        .Build();

    /// <summary>
    /// The attributes handed to the inner button: the consumer's own, plus the pressed state. A single-select
    /// toggle group's items are radios, which announce their state with <c>aria-checked</c> instead of
    /// <c>aria-pressed</c>.
    /// </summary>
    private Dictionary<string, object> buttonAttributes
    {
        get
        {
            var attributes = new Dictionary<string, object>(Attributes);
            var state = Pressed ? "true" : "false";

            if (groupItem?.Radio == true)
            {
                attributes["role"] = "radio";
                attributes["aria-checked"] = state;
            }
            else
            {
                attributes["aria-pressed"] = state;
            }

            attributes["data-state"] = Pressed ? "on" : "off";
            return attributes;
        }
    }

    /// <summary>
    /// Moves keyboard focus to the toggle. Used by <see cref="TwToggleGroup{T}"/> for its arrow key navigation.
    /// </summary>
    internal async Task FocusAsync()
    {
        if (button is not null)
        {
            await button.FocusAsync();
        }
    }

    private async Task OnClickAsync()
    {
        if (Disabled)
        {
            return;
        }

        Pressed = !Pressed;
        await PressedChanged.InvokeAsync(Pressed);
    }
}
