// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using TwBlazor.Builders;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// Represents a chip component for displaying compact elements representing an input, attribute, or action.
/// </summary>
public partial class TwChip : TwBlazorComponentBase
{
    [Inject] private ChipBuilder chipBuilder { get; set; } = null!;

    private TwChipTheme theme => options.Theme.Components.Require<TwChipTheme>();

    /// <summary>
    /// Gets or sets the text label to display in the chip.
    /// </summary>
    [Parameter] public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the custom content to display in the chip.
    /// </summary>
    /// <remarks>
    /// When provided, this content replaces the Label parameter.
    /// </remarks>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the color theme of the chip.
    /// </summary>
    [Parameter] public Color Color { get; set; } = Color.Primary;

    /// <summary>
    /// Gets or sets the chip variant.
    /// </summary>
    /// <remarks>
    /// Default is <see cref="ButtonVariant.Filled"/>.
    /// </remarks>
    [Parameter] public ButtonVariant Variant { get; set; } = ButtonVariant.Filled;

    /// <summary>
    /// Gets or sets the size of the chip.
    /// </summary>
    [Parameter] public ChipSize Size { get; set; } = ChipSize.Medium;

    /// <summary>
    /// Gets or sets whether the chip is closable (shows a close button).
    /// </summary>
    /// <remarks>
    /// Closable chips cannot be used with link chips (Href).
    /// </remarks>
    [Parameter] public bool Closable { get; set; } = false;

    /// <summary>
    /// Gets or sets the icon to use for the close button.
    /// </summary>
    [Parameter] public Icon CloseIcon { get; set; } = Icon.X;

    /// <summary>
    /// Gets or sets the callback that is invoked when the close button is clicked.
    /// </summary>
    [Parameter] public EventCallback OnClose { get; set; }

    /// <summary>
    /// Gets or sets the callback that is invoked when the chip is clicked.
    /// </summary>
    /// <remarks>
    /// This callback is not invoked for link chips (when Href is set).
    /// </remarks>
    [Parameter] public EventCallback OnClick { get; set; }

    /// <summary>
    /// Gets or sets whether to display an icon at the start of the chip.
    /// </summary>
    [Parameter] public Icon? StartIcon { get; set; }

    /// <summary>
    /// Gets or sets whether to display an icon at the end of the chip.
    /// </summary>
    [Parameter] public Icon? EndIcon { get; set; }

    /// <summary>
    /// Gets or sets the URL that the chip links to.
    /// </summary>
    /// <remarks>
    /// When set, the chip renders as a link. Link chips cannot use Closable or OnClick.
    /// If Target is set to "_blank", rel="noopener" will be added automatically unless a custom Rel is provided.
    /// </remarks>
    [Parameter] public string Href { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the target frame or window for the navigation request.
    /// </summary>
    [Parameter] public string Target { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the relationship between the linked resource and the current document.
    /// </summary>
    /// <remarks>
    /// When not set and Target is "_blank", defaults to "noopener".
    /// This property overrides the automatic addition of rel="noopener".
    /// </remarks>
    [Parameter] public string? Rel { get; set; }

    /// <summary>
    /// Disables the chip if set to true.
    /// </summary>
    [Parameter] public bool Disabled { get; set; } = false;

    /// <summary>
    /// Gets or sets the avatar text/initials to display at the start of the chip.
    /// </summary>
    /// <remarks>
    /// When set, this takes precedence over StartIcon.
    /// </remarks>
    [Parameter] public string? Avatar { get; set; }

    /// <summary>
    /// Gets the effective rel attribute value.
    /// </summary>
    private string? effectiveRel => Rel ?? (Target == "_blank" ? "noopener" : null);

    /// <summary>
    /// Gets whether the chip is clickable.
    /// </summary>
    private bool isClickable => !string.IsNullOrWhiteSpace(Href) || OnClick.HasDelegate;

    /// <summary>
    /// Gets the CSS classes applied to the chip element.
    /// </summary>
    private string classes => new ClassBuilder()
        .AddClass(chipBuilder.GetBaseClasses(Size, isClickable, Disabled))
        .AddClass(chipBuilder.GetVariantClasses(Variant, Color, Disabled, isClickable))
        .AddClass(colorBuilder.GetFocusRing(Color), !Disabled)
        .AddClass(roundedBuilder.GetRounded(effectiveRounded))
        .AddClass(Class)
        .Build();

    /// <summary>
    /// Handles the click event asynchronously.
    /// </summary>
    private async Task OnClickAsync()
    {
        if (Disabled || !string.IsNullOrWhiteSpace(Href))
        {
            return;
        }

        if (OnClick.HasDelegate)
        {
            await OnClick.InvokeAsync();
        }
    }


    /// <summary>
    /// Handles the close button click event asynchronously.
    /// </summary>
    private async Task OnCloseAsync()
    {
        if (Disabled)
        {
            return;
        }

        if (OnClose.HasDelegate)
        {
            await OnClose.InvokeAsync();
        }
    }

    /// <summary>
    /// Gets the avatar classes based on chip size.
    /// </summary>
    private string GetAvatarClasses()
    {
        var sizeClass = Size switch
        {
            ChipSize.Small => "w-3.5 h-3.5 text-[9px]",
            ChipSize.Medium => "w-4 h-4 text-[10px]",
            ChipSize.Large => "w-5 h-5 text-xs",
            _ => "w-4 h-4 text-[10px]"
        };

        // Outlined/Text chips render currentColor as saturated text on a light/transparent chip
        // background, so a currentColor-tinted circle stays light and reads fine against it.
        // Filled/Elevated chips render currentColor as light text on a saturated background - the
        // same currentColor tint there produces near-white initials on a near-white circle, so a
        // dark overlay is used instead to keep the initials legible.
        var avatarBackground = Variant is ButtonVariant.Outlined or ButtonVariant.Text ? "bg-current/10" : "bg-black/20";

        return $"inline-flex items-center justify-center rounded-full {avatarBackground} {sizeClass} font-semibold -ml-1";
    }

    /// <summary>
    /// Gets the icon size based on chip size.
    /// </summary>
    private string GetIconSize()
    {
        // Bootstrap Icons are font glyphs sized via font-size, not width/height, so the
        // box is sized to match via text-* + leading-none rather than w-*/h-* utilities.
        // Fixed w-*/h-* boxes left extra space the glyph didn't fill, throwing off items-center.
        return Size switch
        {
            ChipSize.Small => "text-[10px] leading-none",
            ChipSize.Medium => "text-xs leading-none",
            ChipSize.Large => "text-base leading-none",
            _ => "text-xs leading-none"
        };
    }
}
