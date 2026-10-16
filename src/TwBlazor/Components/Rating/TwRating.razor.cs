// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using System.Globalization;
using System.Numerics;
using TwBlazor.Builders;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Localization;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// Represents a rating input that shows a value as a row of icons, such as stars, and lets the user pick a new one.
/// </summary>
/// <remarks>
/// <para>
/// The precision follows <typeparamref name="T"/>: an integer type rates in whole icons, while a type that can hold
/// fractions (<see cref="double"/>, <see cref="decimal"/>, <see cref="float"/>) rates in half icons. The displayed
/// value is rounded to that precision using <see cref="Rounding"/>; the bound value itself is never rewritten.
/// </para>
/// <para>
/// Each selectable value is a visually hidden native radio input, so the rating is a single Tab stop, the arrow keys
/// move through the values, and screen readers announce it as a radio group ("3.5 out of 5"). A half icon is drawn by
/// clipping the filled icon to half its width, so any <see cref="Icon"/> can be used. A read-only rating is announced
/// as a single image ("Rated 3.5 out of 5") instead.
/// </para>
/// </remarks>
/// <typeparam name="T">The numeric type of the value. Use an integer type for whole icons, or a fractional type for half icons.</typeparam>
public partial class TwRating<T> : TwBlazorInputComponentBase where T : INumber<T>
{
    // Gap between each icon starting its pop, so a fill that spans several icons ripples outwards.
    private const int popStaggerMilliseconds = 50;

    // Integer types truncate a half to zero, which is how the precision is read from T.
    private static readonly double _step = T.CreateTruncating(0.5) == T.Zero ? 1 : 0.5;

    private TwRatingTheme theme => options.Theme.Components.Require<TwRatingTheme>();

    /// <summary>
    /// Gets or sets the current rating.
    /// </summary>
    /// <remarks>
    /// Values between steps are displayed rounded to the nearest step allowed by <see cref="Rounding"/>, and values
    /// outside <see cref="Min"/> and <see cref="Max"/> are displayed clamped. The value is only changed by the user.
    /// </remarks>
    [Parameter] public T Value { get; set; } = T.Zero;

    /// <summary>
    /// Gets or sets the callback that is invoked when the user picks a new rating.
    /// </summary>
    [Parameter] public EventCallback<T> ValueChanged { get; set; }

    /// <summary>
    /// Gets or sets the lowest rating the user can pick. Defaults to 0.
    /// </summary>
    /// <remarks>
    /// When this is 0 the user can clear the rating, by clicking the selected icon again or by choosing the
    /// "No rating" option from the keyboard. Values below <see cref="Min"/> are still displayed, they just cannot be
    /// picked. Negative values are treated as 0.
    /// </remarks>
    [Parameter] public T Min { get; set; } = T.Zero;

    /// <summary>
    /// Gets or sets the highest rating, which is also the number of icons shown. Defaults to 5.
    /// </summary>
    [Parameter] public T Max { get; set; } = T.CreateTruncating(5);

    /// <summary>
    /// Gets or sets how a value between steps is rounded for display. Defaults to
    /// <see cref="MidpointRounding.AwayFromZero"/>, so 3.75 shows as 4 and 3.7 as 3.5.
    /// </summary>
    /// <remarks>
    /// Use <see cref="MidpointRounding.ToNegativeInfinity"/> to always round down (3.9 shows as 3.5), or
    /// <see cref="MidpointRounding.ToPositiveInfinity"/> to always round up (3.1 shows as 3.5).
    /// </remarks>
    [Parameter] public MidpointRounding Rounding { get; set; } = MidpointRounding.AwayFromZero;

    /// <summary>
    /// Gets or sets the icon drawn for each step. Defaults to <see cref="Icon.Star_Fill"/>.
    /// </summary>
    /// <remarks>
    /// The filled part of the rating is this icon in <see cref="Color"/>; a half step shows its first half.
    /// </remarks>
    [Parameter] public Icon Icon { get; set; } = Icon.Star_Fill;

    /// <summary>
    /// Gets or sets the icon drawn behind the filled part, for the unfilled steps. Defaults to <see cref="Icon"/>,
    /// shown in a muted tint.
    /// </summary>
    /// <remarks>
    /// An outline icon (such as <see cref="Icon.Star"/> with <see cref="Icon.Star_Fill"/>) adds a shape cue on top of
    /// the color, which helps anyone who cannot easily tell the two colors apart.
    /// </remarks>
    [Parameter] public Icon? EmptyIcon { get; set; }

    /// <summary>
    /// Gets or sets the color of the filled icons. Defaults to <see cref="Color.Warning"/>.
    /// </summary>
    [Parameter] public Color? Color { get; set; }

    /// <summary>
    /// Gets or sets the size of the icons. Defaults to <see cref="Size.Md"/>.
    /// </summary>
    [Parameter] public Size Size { get; set; } = Size.Md;

    /// <summary>
    /// Gets or sets whether the hover growth and the "pop" played when a rating is picked are turned off.
    /// </summary>
    /// <remarks>
    /// Both animations are always skipped for users who ask their system for reduced motion.
    /// </remarks>
    [Parameter] public bool DisableAnimation { get; set; }

    /// <summary>
    /// Gets or sets the name shared by the rating's radio inputs. Defaults to the component's id.
    /// </summary>
    [Parameter] public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The value the pointer is resting on, previewed in place of <see cref="Value"/> until the pointer leaves.
    /// </summary>
    private double? hoverValue;

    /// <summary>
    /// The icons that pop after the last pick, mapped to their place in the ripple.
    /// </summary>
    private readonly Dictionary<int, int> _popOrder = [];

    /// <summary>
    /// Counts picks, so consecutive pops can alternate keyframe names: re-applying the same animation to an element
    /// does not restart it, but switching its name does.
    /// </summary>
    private int popGeneration;

    private bool interactive => !ReadOnly && !Disabled;

    private double max => Math.Max(RoundDown(double.CreateSaturating(Max)), _step);

    private double min => Math.Clamp(RoundUp(double.CreateSaturating(Min)), 0, max);

    private int iconCount => (int)Math.Ceiling(max);

    /// <summary>
    /// Gets the bound value as it is displayed: rounded to a step and kept between 0 and <see cref="Max"/>.
    /// </summary>
    private double current => Math.Clamp(Math.Round(double.CreateSaturating(Value) / _step, Rounding) * _step, 0, max);

    private double shown => hoverValue ?? current;

    private string groupName => string.IsNullOrEmpty(Name) ? Id! : Name;

    private string labelId => string.IsNullOrEmpty(LabelId) ? $"{Id}-label" : LabelId;

    private string valueTextId => $"{Id}-value";

    private bool hasLabel => !string.IsNullOrWhiteSpace(Label);

    private bool canClear => interactive && min <= 0;

    /// <summary>
    /// Gets the id(s) that name the rating: the consumer's own, or the visible label's.
    /// </summary>
    private string? nameIds => !string.IsNullOrEmpty(AriaLabelledBy) ? AriaLabelledBy : hasLabel ? labelId : null;

    private string readOnlyText => localizer[TwStrings.Rating_ReadOnly, current, max];

    /// <summary>
    /// Gets the read-only rating's accessible name, used when no element names it. When one does, the name is built
    /// from that element and the value text instead, through <see cref="readOnlyLabelledBy"/>.
    /// </summary>
    private string? readOnlyLabel => nameIds is not null
        ? null
        : string.IsNullOrWhiteSpace(AriaLabel) ? readOnlyText : $"{AriaLabel}, {readOnlyText}";

    private string? readOnlyLabelledBy => nameIds is null ? null : $"{nameIds} {valueTextId}";

    private string filledColor => ColorBuilder.GetPaletteColor(Color, theme.Colors, theme.Colors.Warning);

    private string sizeClasses => Size switch
    {
        Size.Sm => theme.Small,
        Size.Lg => theme.Large,
        _ => theme.Medium
    };

    private string groupClasses => new ClassBuilder(theme.Base)
        .AddClass(sizeClasses)
        .AddClass(theme.EmptyFocusRing, canClear)
        .AddClass(theme.Disabled, Disabled)
        .AddClass(Class)
        .Build();

    private string itemClasses => new ClassBuilder(theme.Item)
        .AddClass(theme.ItemFocusRing, !ReadOnly)
        .AddClass(theme.ItemHover, interactive && !DisableAnimation)
        .Build();

    private string emptyClasses => theme.Empty;

    private string filledClasses => new ClassBuilder(theme.Filled)
        .AddClass(filledColor)
        .Build();

    private string VisualClasses(bool pops) => new ClassBuilder(theme.Visual)
        .AddClass(theme.Pop, pops)
        .Build();

    /// <summary>
    /// Gets how much of the icon at <paramref name="position"/> (1-based) a rating of <paramref name="rating"/> fills:
    /// 0, 0.5 or 1.
    /// </summary>
    private static double Fill(double rating, int position) => Math.Clamp(rating - (position - 1), 0, 1);

    private static string Percent(double fill) => (fill * 100).ToString("0.###", CultureInfo.InvariantCulture);

    /// <summary>
    /// Gets the values the icon at <paramref name="position"/> (1-based) can be set to, its half first.
    /// </summary>
    private IEnumerable<double> ValuesFor(int position)
    {
        if (_step < 1)
        {
            var half = position - 0.5;
            if (half >= min && half > 0 && half <= max)
                yield return half;
        }

        if (position >= min && position <= max)
            yield return position;
    }

    private bool IsHalf(double value) => value % 1 != 0;

    private string RadioId(double value) => $"{Id}-{(int)Math.Round(value / _step)}";

    private static string Invariant(double value) => value.ToString(CultureInfo.InvariantCulture);

    private string OptionLabel(double value) => localizer[TwStrings.Rating_Value, value, max];

    private static double RoundDown(double value) => Math.Floor(value / _step) * _step;

    private static double RoundUp(double value) => Math.Ceiling(value / _step) * _step;

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (!interactive)
            hoverValue = null;
    }

    private void Preview(double value)
    {
        if (interactive)
            hoverValue = value;
    }

    private void EndPreview() => hoverValue = null;

    /// <summary>
    /// Clears the rating when the user clicks the icon half that is already selected. A radio input fires no change
    /// for that click, so it is picked up from the pointer target instead, which only a pointer can reach.
    /// </summary>
    private async Task ClearIfSelectedAsync(double value)
    {
        if (canClear && value == current)
        {
            hoverValue = null;
            await SelectAsync(0);
        }
    }

    private async Task SelectAsync(double value)
    {
        if (!interactive || value == current)
            return;

        QueuePop(current, value);

        Value = T.CreateSaturating(value);
        await ValueChanged.InvokeAsync(Value);
    }

    /// <summary>
    /// Marks the icons that pop for a change from <paramref name="from"/> to <paramref name="to"/>: those that gained
    /// fill, plus the icon that was picked, in order so the pop ripples from the start.
    /// </summary>
    private void QueuePop(double from, double to)
    {
        _popOrder.Clear();

        if (DisableAnimation)
            return;

        popGeneration++;
        var picked = (int)Math.Ceiling(to);
        var order = 0;

        for (var position = 1; position <= iconCount; position++)
        {
            var after = Fill(to, position);
            if (after > 0 && (after > Fill(from, position) || position == picked))
                _popOrder[position] = order++;
        }
    }

    private string? PopStyle(int order) =>
        $"animation-delay:{(order * popStaggerMilliseconds).ToString(CultureInfo.InvariantCulture)}ms";
}
