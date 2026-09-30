// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace TwBlazor.Configuration.Components;

using System.Diagnostics.CodeAnalysis;
using TwBlazor.Enums;

/// <summary>
/// Theme configuration for input components (<see cref="TwBlazor.Components.TwTextfield{T}"/> and
/// <see cref="TwBlazor.Components.TwSelect{T}"/>).
/// Override any property to customize input styles globally.
/// </summary>
[ExcludeFromCodeCoverage]
public class TwInputTheme
{
    /// <summary>
    /// Gets or sets the default input variant for text fields and selects.
    /// </summary>
    /// <remarks>
    /// Default is <see cref="InputVariant.Filled"/>.
    /// Individual components can override this setting.
    /// </remarks>
    public InputVariant DefaultInputVariant { get; set; } = InputVariant.Filled;

    /// <summary>
    /// Gets or sets the base classes applied to text fields and text areas.
    /// </summary>
    public required string TextfieldBase { get; set; }

    /// <summary>
    /// Gets or sets the base classes applied to selects.
    /// </summary>
    public required string SelectBase { get; set; }

    /// <summary>
    /// Gets or sets the height and text size shared by every single-line input (text fields, selects, and
    /// the date, time and color pickers built on a text field), so they all line up.
    /// </summary>
    /// <remarks>
    /// Keep the height in step with any control laid out beside inputs, such as <see cref="TwPaginationTheme.Size"/>.
    /// </remarks>
    public required string Size { get; set; }

    /// <summary>
    /// Gets or sets the height and text size applied instead of <see cref="Size"/> when an input's
    /// <see cref="TwBlazor.TwBlazorTextInputComponentBase.Dense"/> is enabled.
    /// </summary>
    public required string DenseSize { get; set; }

    /// <summary>
    /// Gets or sets the classes applied to each <c>&lt;option&gt;</c> in a select.
    /// </summary>
    public required string SelectOption { get; set; }

    /// <summary>
    /// Gets or sets the background classes forced onto a select using the Default/Outlined variant.
    /// A native <c>&lt;select&gt;</c> popup renders using the element's own background/text colors, so
    /// a transparent one (those variants' usual background) falls back to the OS's native, often
    /// light, popup surface and can pair unreadable white dark-mode text onto it.
    /// </summary>
    public required string SelectNativeBackground { get; set; }

    /// <summary>
    /// Gets or sets the horizontal padding applied to a select using the <see cref="Enums.InputVariant.Default"/>
    /// variant, which otherwise has none of its own (unlike Filled/Outlined).
    /// </summary>
    public required string SelectDefaultPadding { get; set; }

    /// <summary>
    /// Gets or sets the background override applied to a read-only select, suppressing its dropdown
    /// arrow background image so it doesn't imply the value can still be changed.
    /// </summary>
    public required string SelectReadOnlyBackground { get; set; }

    /// <summary>
    /// Gets or sets the layout classes applied to a multi-select's closed trigger
    /// (<see cref="TwBlazor.Components.TwSelect{T}.Multiple"/>), on top of <see cref="SelectBase"/>.
    /// </summary>
    /// <remarks>
    /// The trigger holds a wrapping row of <see cref="TwBlazor.Components.TwChip"/> chips (one per
    /// selected option, or the placeholder text while none are selected) rather than a single line of
    /// text the way a real &lt;select&gt; would, so it needs its own flex-wrap layout.
    /// </remarks>
    public required string SelectMultiTriggerLayout { get; set; }

    /// <summary>
    /// Gets or sets the classes applied to the real, invisible <c>&lt;select multiple&gt;</c> layered on
    /// top of a multi-select's decorative closed trigger when the native picker is in use.
    /// </summary>
    /// <remarks>
    /// A native <c>&lt;select multiple&gt;</c> can't render as a closed, single-row trigger the way a
    /// single <c>&lt;select&gt;</c> can - it's always an expanded listbox. So the decorative trigger
    /// (styled like <see cref="SelectBase"/>) is what's actually seen, and this real control sits over
    /// it, fully transparent, so a tap/click still lands on genuine native form control and opens the
    /// platform's own multi-select UI (e.g. the full-screen sheet iOS/Android show for it).
    /// </remarks>
    public required string SelectNativeMultiOverlay { get; set; }

    /// <summary>
    /// Gets or sets the classes for the real, focusable &lt;button&gt; at the end of a multi-select's
    /// custom-popover trigger that actually opens the popover.
    /// </summary>
    /// <remarks>
    /// Deliberately a plain &lt;button&gt; sibling after the selected-option chips (not a wrapping
    /// element around them) - an ARIA "button" role wrapping other focusable controls (each chip's own
    /// close button) is a nested-interactive-controls violation, since assistive tech can't reliably
    /// represent one interactive control inside another. It fills any remaining row width so most of the
    /// closed trigger's empty space is still clickable, and shows <see cref="TwBlazor.Components.TwSelect{T}.Placeholder"/>
    /// when nothing is selected.
    /// </remarks>
    public required string SelectMultiOpenButton { get; set; }

    /// <summary>
    /// Gets or sets the positioning classes for a multi-select's custom checkbox-list popover panel.
    /// </summary>
    public required string SelectPanelPosition { get; set; }

    /// <summary>
    /// Gets or sets the padding/scroll classes appended to a multi-select popover panel's surface
    /// (background/border/rounded/shadow come from <c>PopoverBuilder.GetSurfaceClasses</c> instead).
    /// </summary>
    public required string SelectPanelSurface { get; set; }

    /// <summary>
    /// Gets or sets the classes that style each option row inside a multi-select popover panel so it
    /// matches the options of a single <see cref="TwBlazor.Components.TwSelect{T}"/>'s customizable
    /// dropdown: no visible checkbox, a leading tick and a tinted row when selected, and a hover tint.
    /// </summary>
    /// <remarks>
    /// <see cref="TwBlazor.Components.TwCheckboxGroup{TValue}"/> is still what renders the options, so
    /// every checkbox stays a real, keyboard-focusable input; it is only visually hidden (<c>sr-only</c>) and
    /// the row (its label) shows the selected and focus state instead. These use arbitrary descendant
    /// selectors targeting the elements the group renders, scoped to just this popover rather than changing
    /// every standalone checkbox.
    /// <para>
    /// The text color uses <c>!important</c> to win over <see cref="TwCheckboxTheme.LabelBase"/>'s muted
    /// caption-style color, which is too low-contrast against <see cref="TwOverlayTheme.PopoverBackground"/>
    /// in dark mode once it is the primary, repeated content of a whole option list.
    /// </para>
    /// </remarks>
    public required string SelectPanelItemText { get; set; }

    /// <summary>
    /// Gets or sets the base classes for input labels.
    /// </summary>
    public required string LabelBase { get; set; }

    /// <summary>
    /// Gets or sets the CSS classes applied to the input legend (group) element to define its visual styling.
    /// </summary>
    public required string InputLegendBase { get; set; }

    /// <summary>
    /// Gets or sets the default border for outlined inputs.
    /// </summary>
    public required string OutlinedBorder { get; set; }

    /// <summary>
    /// Gets or sets the default border for filled inputs.
    /// </summary>
    public required string FilledBorder { get; set; }

    /// <summary>
    /// Gets or sets the focus border for inputs.
    /// </summary>
    public required string FocusBorder { get; set; }

    /// <summary>
    /// Gets or sets the filled variant background color.
    /// </summary>
    public required string FilledBackgroundColor { get; set; }

    /// <summary>
    /// Gets or sets the classes for an input's validation error message, rendered by <see cref="TwBlazor.Components.TwInputRoot"/>.
    /// </summary>
    public required string ErrorMessage { get; set; }
}