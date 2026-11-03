// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using System.Globalization;
using TwBlazor.Builders;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// Represents an overlay badge that wraps content - an icon, avatar or button - and overlays a count,
/// text label or dot on one of its corners.
/// </summary>
public partial class TwBadge : TwBlazorComponentBase
{
    private TwBadgeTheme theme => options.Theme.Components.Require<TwBadgeTheme>();

    /// <summary>
    /// Gets or sets the content the badge overlays, such as an icon, avatar or button.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the numeric value shown in the badge.
    /// </summary>
    [Parameter] public int? Count { get; set; }

    /// <summary>
    /// Gets or sets the maximum value shown before the badge caps at "{Max}+". Defaults to 99.
    /// </summary>
    [Parameter] public int Max { get; set; } = 99;

    /// <summary>
    /// Gets or sets the text label shown in the badge instead of a count.
    /// </summary>
    [Parameter] public string? Content { get; set; }

    /// <summary>
    /// Gets or sets whether to render a dot instead of a count or label.
    /// </summary>
    [Parameter] public bool Dot { get; set; }

    /// <summary>
    /// Gets or sets the color of the badge. Defaults to <see cref="Color.Primary"/> (the app's primary/purple).
    /// </summary>
    [Parameter] public Color Color { get; set; } = Color.Primary;

    /// <summary>
    /// Gets or sets the corner the badge overlays. Defaults to <see cref="BadgePlacement.TopEnd"/>.
    /// </summary>
    [Parameter] public BadgePlacement Placement { get; set; } = BadgePlacement.TopEnd;

    private string colorClasses => ColorBuilder.GetPaletteColor(Color, theme.Colors, theme.Colors.Primary);

    private string placementClasses => Placement switch
    {
        BadgePlacement.TopStart => theme.TopStart,
        BadgePlacement.BottomStart => theme.BottomStart,
        BadgePlacement.BottomEnd => theme.BottomEnd,
        _ => theme.TopEnd,
    };

    private string? badgeText
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(Content)) return Content;
            if (Count is not int count) return null;
            return Max > 0 && count > Max ? $"{Max}+" : count.ToString(CultureInfo.InvariantCulture);
        }
    }

    private string wrapperClasses => new ClassBuilder(theme.Wrapper).AddClass(Class).Build();

    private string badgeClasses => new ClassBuilder(theme.Badge)
        .AddClass(colorClasses)
        .AddClass(placementClasses)
        .AddClass(roundedBuilder.GetRounded(Rounded ?? Enums.Rounded.Full))
        .Build();

    private string dotClasses => new ClassBuilder(theme.Dot)
        .AddClass(colorClasses)
        .AddClass(placementClasses)
        .Build();
}
