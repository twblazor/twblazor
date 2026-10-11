// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Builders;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Utilities;
using TwBlazor.Localization;

namespace TwBlazor.Components;

/// <summary>
/// Represents an animated loading indicator.
/// </summary>
public partial class TwSpinner : TwBlazorComponentBase
{
    private TwSpinnerTheme theme => options.Theme.Components.Require<TwSpinnerTheme>();

    /// <summary>
    /// Gets or sets the color of the spinning arc.
    /// </summary>
    /// <remarks>
    /// If not set, defaults to blue.
    /// </remarks>
    [Parameter] public Color? Color { get; set; }

    /// <summary>
    /// Gets or sets the size of the spinner.
    /// </summary>
    [Parameter] public SpinnerSize Size { get; set; } = SpinnerSize.Medium;

    /// <summary>
    /// Gets or sets the text announced to assistive technology while the spinner is visible.
    /// Defaults to the localized <see cref="TwStrings.Common_LoadingEllipsis"/> text ("Loading...").
    /// </summary>
    /// <remarks>
    /// Always rendered in the DOM so the spinner has an accessible name; visually hidden unless <see cref="ShowLabel"/> is <c>true</c>.
    /// </remarks>
    [Parameter] public string? Label { get; set; }

    private string effectiveLabel => Label ?? localizer[TwStrings.Common_LoadingEllipsis];

    /// <summary>
    /// Gets or sets whether <see cref="Label"/> is displayed visibly next to the spinner, instead of only being announced to assistive technology.
    /// </summary>
    [Parameter] public bool ShowLabel { get; set; }

    /// <summary>
    /// Gets the classes applied to the root wrapper element.
    /// </summary>
    private string wrapperClasses => new ClassBuilder(theme.Wrapper)
        .AddClass(Class)
        .Build();

    /// <summary>
    /// Gets the size and border-width classes for the current <see cref="Size"/>.
    /// </summary>
    private string sizeClasses => Size switch
    {
        SpinnerSize.Small => theme.Small,
        SpinnerSize.Medium => theme.Medium,
        SpinnerSize.Large => theme.Large,
        _ => theme.Medium
    };

    /// <summary>
    /// Gets the classes applied to the spinning indicator itself.
    /// </summary>
    private string classes => new ClassBuilder(theme.Base)
        .AddClass(sizeClasses)
        .AddClass(trackClasses)
        .AddClass(GetSpinnerColor(Color))
        .Build();

    /// <summary>
    /// Gets the dimmed-ring classes for the current <see cref="Color"/>. <see cref="Enums.Color.Light"/>
    /// and <see cref="Enums.Color.Dark"/> get their own track (see <see cref="TwSpinnerTheme.LightTrack"/>/
    /// <see cref="TwSpinnerTheme.DarkTrack"/>) since their arc colors don't read against the shared
    /// neutral <see cref="TwSpinnerTheme.Track"/> used by every other color.
    /// </summary>
    private string trackClasses => Color switch
    {
        Enums.Color.Light => theme.LightTrack,
        Enums.Color.Dark => theme.DarkTrack,
        _ => theme.Track
    };

    private string GetSpinnerColor(Color? color) => ColorBuilder.GetPaletteColor(color, theme.Colors, theme.Colors.Primary);

    /// <summary>
    /// Gets the classes applied to the label text, visible when <see cref="ShowLabel"/> is <c>true</c> and visually hidden (but still accessible) otherwise.
    /// </summary>
    private string labelClasses => ShowLabel ? theme.Label : "sr-only";
}
