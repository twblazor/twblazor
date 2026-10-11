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
/// Represents a generic progress bar that displays completion of a task towards a maximum value.
/// </summary>
/// <typeparam name="T">The type of the value represented by the progress bar. Must support conversion to <see cref="double"/> for percentage calculation.</typeparam>
public partial class TwProgress<T> : TwBlazorInputComponentBase
{
    private TwProgressTheme theme => options.Theme.Components.Require<TwProgressTheme>();

    /// <summary>
    /// Gets or sets the current value of the progress bar.
    /// </summary>
    [Parameter] public T Value { get; set; } = default!;

    /// <summary>
    /// Gets or sets the value that represents 100% completion.
    /// </summary>
    [Parameter] public required T Max { get; set; }

    /// <summary>
    /// Gets or sets whether the progress bar shows an indeterminate (unknown-duration) animation instead of a specific <see cref="Value"/>.
    /// </summary>
    /// <remarks>
    /// When <c>true</c>, the <c>value</c> attribute is omitted from the rendered &lt;progress&gt; element, which is
    /// how the HTML spec distinguishes an indeterminate progress bar from one that is determinately at 0%.
    /// </remarks>
    [Parameter] public bool Indeterminate { get; set; }

    /// <summary>
    /// Gets or sets whether a shimmer animation sweeps across the filled portion of the progress bar to show
    /// that work is ongoing.
    /// </summary>
    /// <remarks>
    /// Purely decorative: it does not change the value, role or busy state announced to assistive technology,
    /// and it is suppressed for users who prefer reduced motion. It has no effect while
    /// <see cref="Indeterminate"/> is <c>true</c>.
    /// </remarks>
    [Parameter] public bool Animated { get; set; }

    /// <summary>
    /// Gets or sets the color of the progress bar's filled portion.
    /// </summary>
    /// <remarks>
    /// If not set, defaults to blue.
    /// </remarks>
    [Parameter] public Color? Color { get; set; }

    /// <summary>
    /// Gets or sets the size (thickness) of the progress bar.
    /// </summary>
    [Parameter] public ProgressSize Size { get; set; } = ProgressSize.Medium;

    /// <summary>
    /// Gets or sets the id(s) of element(s) that describe the progress bar for assistive technology, in addition to any error message id.
    /// </summary>
    [Parameter] public string AriaDescribedBy { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the progress bar's region is announced as busy to assistive technology.
    /// </summary>
    /// <remarks>
    /// If not set, defaults to <see cref="Indeterminate"/> so an indeterminate progress bar is announced as busy
    /// while a determinate one (even at 0%) is not.
    /// </remarks>
    [Parameter] public bool? AriaBusy { get; set; }

    /// <summary>
    /// Gets the effective aria-busy value, falling back to <see cref="Indeterminate"/> when <see cref="AriaBusy"/> is not set.
    /// </summary>
    private bool effectiveAriaBusy => AriaBusy ?? Indeterminate;

    /// <summary>
    /// Gets the value rendered as the &lt;progress&gt; element's "value" attribute, or <c>null</c> to omit it
    /// (rendering an indeterminate progress bar) when <see cref="Indeterminate"/> is <c>true</c>.
    /// </summary>
    private object? progressValue => Indeterminate ? null : Value;

    /// <summary>
    /// Gets whether the shimmer overlay is rendered. An indeterminate bar has no filled portion to shimmer over.
    /// </summary>
    private bool showShimmer => Animated && !Indeterminate;

    /// <summary>
    /// Gets the classes applied to the element wrapping the &lt;progress&gt; element.
    /// </summary>
    private string wrapperClasses => theme.Wrapper;

    /// <summary>
    /// Gets the classes applied to the shimmer overlay.
    /// </summary>
    private string shimmerClasses => theme.Animated;

    /// <summary>
    /// Gets the inline style that sizes the shimmer overlay to the filled portion of the bar.
    /// </summary>
    private string shimmerStyle => $"width: {percentageText}%";

    /// <summary>
    /// Gets the aria-invalid value, or <c>null</c> to omit the attribute when the progress bar is valid.
    /// </summary>
    private string? ariaInvalid => Invalid ? "true" : null;

    /// <summary>
    /// Gets the aria-busy value, or <c>null</c> to omit the attribute when the progress bar is not busy.
    /// </summary>
    private string? ariaBusy => effectiveAriaBusy ? "true" : null;

    /// <summary>
    /// Gets the fallback text content rendered for browsers that don't support &lt;progress&gt;.
    /// </summary>
    private string fallbackText => Indeterminate ? "Loading..." : $"{percentageText}%";

    /// <summary>
    /// Gets the combined aria-describedby value from <see cref="AriaDescribedBy"/> and the base class's validation error id, or <c>null</c> if neither applies.
    /// </summary>
    private string? describedByIds
    {
        get
        {
            List<string> ids = [];

            if (!string.IsNullOrWhiteSpace(AriaDescribedBy))
            {
                ids.Add(AriaDescribedBy);
            }

            if (errorId != null)
            {
                ids.Add(errorId);
            }

            return ids.Count > 0 ? string.Join(" ", ids) : null;
        }
    }

    /// <summary>
    /// Gets the height classes for the current <see cref="Size"/>.
    /// </summary>
    private string sizeClasses => Size switch
    {
        ProgressSize.Small => theme.Small,
        ProgressSize.Medium => theme.Medium,
        ProgressSize.Large => theme.Large,
        _ => theme.Medium
    };

    /// <summary>
    /// Gets the classes applied to the &lt;progress&gt; element.
    /// </summary>
    private string classes => new ClassBuilder(theme.Base)
        .AddClass(sizeClasses)
        .AddClass(GetProgressColor(Color))
        .AddClass(options.Theme.Interaction.DisabledOpacity, Disabled)
        .AddClass(Class)
        .Build();

    private string GetProgressColor(Color? color) => ColorBuilder.GetPaletteColor(color, theme.Colors, theme.Colors.Primary);

    /// <summary>
    /// Gets the current value as a percentage (0-100) of <see cref="Max"/>, used for the fallback text content
    /// rendered for browsers that don't support &lt;progress&gt;.
    /// </summary>
    private double percentage
    {
        get
        {
            try
            {
                var max = Convert.ToDouble(Max, CultureInfo.InvariantCulture);
                var value = Convert.ToDouble(Value, CultureInfo.InvariantCulture);

                if (max <= 0)
                {
                    return 0;
                }

                return Math.Clamp(value / max * 100, 0, 100);
            }
            catch (Exception ex) when (ex is InvalidCastException or FormatException or OverflowException)
            {
                return 0;
            }
        }
    }

    private string percentageText => percentage.ToString("0.###", CultureInfo.InvariantCulture);
}
