// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using System.Globalization;
using System.Linq.Expressions;
using TwBlazor.Builders;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Localization;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// A slider with two handles for choosing a lower and an upper value within a range.
/// </summary>
/// <remarks>
/// Built the same way as <see cref="TwSlider{T}"/>: each handle is a native range input that is invisible but
/// still handles pointer, touch, keyboard and screen reader interaction, with the track, fill and thumbs drawn
/// separately for full styling control. The handles cannot pass each other, and <see cref="MinDistance"/> can
/// keep a gap between them.
/// </remarks>
/// <typeparam name="T">The numeric type of the values, such as <see cref="int"/>, <see cref="double"/> or <see cref="decimal"/>.</typeparam>
public partial class TwRangeSlider<T> : TwBlazorInputComponentBase
{
    // The range slider is drawn with the slider theme, so both always match; its own theme only adds the second handle.
    private TwSliderTheme sliderTheme => options.Theme.Components.Require<TwSliderTheme>();

    private TwRangeSliderTheme theme => options.Theme.Components.Require<TwRangeSliderTheme>();

    /// <summary>
    /// Gets or sets the lower value, set by the first handle.
    /// </summary>
    [Parameter] public T LowerValue { get; set; } = default!;

    /// <summary>
    /// Gets or sets the callback invoked when the lower value changes. Used by <c>@bind-LowerValue</c>.
    /// </summary>
    [Parameter] public EventCallback<T> LowerValueChanged { get; set; }

    /// <summary>
    /// Gets or sets the expression that identifies the bound lower value. Set automatically by
    /// <c>@bind-LowerValue</c>, it lets an enclosing <see cref="EditForm"/> or <see cref="TwForm"/> validate the property.
    /// </summary>
    [Parameter] public Expression<Func<T>>? LowerValueExpression { get; set; }

    /// <summary>
    /// Gets or sets the upper value, set by the second handle.
    /// </summary>
    [Parameter] public T UpperValue { get; set; } = default!;

    /// <summary>
    /// Gets or sets the callback invoked when the upper value changes. Used by <c>@bind-UpperValue</c>.
    /// </summary>
    [Parameter] public EventCallback<T> UpperValueChanged { get; set; }

    /// <summary>
    /// Gets or sets the expression that identifies the bound upper value. Set automatically by
    /// <c>@bind-UpperValue</c>. It is used for validation when <see cref="LowerValueExpression"/> is not set.
    /// </summary>
    [Parameter] public Expression<Func<T>>? UpperValueExpression { get; set; }

    /// <summary>
    /// Gets or sets the lowest value either handle can take.
    /// </summary>
    [Parameter] public required T Min { get; set; }

    /// <summary>
    /// Gets or sets the highest value either handle can take.
    /// </summary>
    [Parameter] public required T Max { get; set; }

    /// <summary>
    /// Gets or sets the amount a handle moves with each step. Must be positive.
    /// </summary>
    [Parameter] public required T Step { get; set; }

    /// <summary>
    /// Gets or sets the smallest gap allowed between the lower and upper values. Defaults to zero, which lets
    /// both handles sit on the same value.
    /// </summary>
    [Parameter] public T MinDistance { get; set; } = default!;

    /// <summary>
    /// Gets or sets the form field name. The lower input is submitted as <c>{Name}-lower</c> and the upper input
    /// as <c>{Name}-upper</c>.
    /// </summary>
    [Parameter] public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the color of the filled range between the handles. Defaults to the primary color.
    /// </summary>
    [Parameter] public Color? Color { get; set; }

    private ElementReference lowerRef;
    private ElementReference upperRef;

    // The values written to the native inputs. They usually match LowerValue and UpperValue, but are briefly set
    // to the browser's value when a move is rejected (see SyncRejectedMove).
    private string? lowerInputValue;
    private string? upperInputValue;

    // Whether the arrow keys are currently blocked on the native inputs (see TwSlider for why).
    private bool keysLocked;

    [Inject] private IJSRuntime jsRuntime { get; set; } = null!;

    private string upperId => $"{Id}-upper";

    private string labelId => string.IsNullOrEmpty(LabelId) ? $"{Id}-label" : LabelId;

    private string? lowerName => string.IsNullOrEmpty(Name) ? null : $"{Name}-lower";

    private string? upperName => string.IsNullOrEmpty(Name) ? null : $"{Name}-upper";

    private string? groupAriaLabel => string.IsNullOrWhiteSpace(AriaLabel) ? null : AriaLabel;

    private string? groupAriaLabelledBy
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(AriaLabelledBy))
                return AriaLabelledBy;

            if (!string.IsNullOrWhiteSpace(Label))
                return labelId;

            return null;
        }
    }

    private string? ariaReadOnly => ReadOnly ? "true" : null;

    private string? ariaInvalid => fieldInvalid ? "true" : null;

    private string minimumLabel => localizer[TwStrings.RangeSlider_Minimum];

    private string maximumLabel => localizer[TwStrings.RangeSlider_Maximum];

    private string fillStyle => $"inset-inline-start:{Format(lowerPercentage)}%;width:{Format(upperPercentage - lowerPercentage)}%";

    private string lowerPositionStyle => $"inset-inline-start:{Format(lowerPercentage)}%";

    private string upperPositionStyle => $"inset-inline-start:{Format(upperPercentage)}%";

    private string lowerHandleStyle => $"z-index:{(lowerOnTop ? 2 : 1)}";

    private string upperHandleStyle => $"z-index:{(lowerOnTop ? 1 : 2)}";

    private string wrapperClasses => new ClassBuilder(sliderTheme.Wrapper)
        .AddClass(Class)
        .Build();

    private string classes => new ClassBuilder(sliderTheme.Base)
        .AddClass(theme.Base)
        .AddClass(Disabled ? "cursor-not-allowed" : string.Empty)
        .Build();

    private string trackClasses => new ClassBuilder(sliderTheme.Track)
        .AddClass(Disabled ? "opacity-40" : string.Empty)
        .Build();

    private string fillClasses => new ClassBuilder(sliderTheme.Fill)
        .AddClass(theme.Fill)
        .AddClass(roundedBuilder.GetRounded())
        .AddClass(ColorBuilder.GetPaletteColor(Color, sliderTheme.Colors, sliderTheme.Colors.Primary))
        .Build();

    private string thumbClasses => new ClassBuilder(sliderTheme.Thumb)
        .AddClass(colorBuilder.GetBorderColor(Color))
        .AddClass(colorBuilder.GetPeerFocusRing(Color))
        .AddClass(Disabled ? "opacity-40" : string.Empty)
        .Build();

    private double min => ToDouble(Min) ?? 0;

    private double max => ToDouble(Max) ?? 0;

    private double lowerPercentage => Percentage(LowerValue);

    private double upperPercentage => Math.Max(Percentage(UpperValue), lowerPercentage);

    // When both handles sit together, the one that can still move must be on top so it can be grabbed:
    // near the end of the track only the lower handle can move, so it goes on top.
    private bool lowerOnTop => lowerPercentage > 50;

    private double Percentage(T value)
    {
        var number = ToDouble(value);
        if (number is null || max <= min)
            return 0;

        return Math.Clamp((number.Value - min) / (max - min) * 100, 0, 100);
    }

    private static string Format(double percentage) => percentage.ToString("0.###", CultureInfo.InvariantCulture);

    private static double? ToDouble(T value)
    {
        try
        {
            return value is null ? null : Convert.ToDouble(value, CultureInfo.InvariantCulture);
        }
        catch (Exception ex) when (ex is InvalidCastException or FormatException or OverflowException)
        {
            return null;
        }
    }

    private static string? ToInputValue(T value) => value is IFormattable formattable
        ? formattable.ToString(null, CultureInfo.InvariantCulture)
        : value?.ToString();

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        lowerInputValue = ToInputValue(LowerValue);
        upperInputValue = ToInputValue(UpperValue);
    }

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        var shouldLock = ReadOnly && !Disabled;
        if (shouldLock == keysLocked)
            return;

        keysLocked = shouldLock;

        try
        {
            await jsRuntime.InvokeVoidAsync("twSliderLock.set", lowerRef, shouldLock);
            await jsRuntime.InvokeVoidAsync("twSliderLock.set", upperRef, shouldLock);
        }
        catch (JSDisconnectedException)
        {
            // The circuit disconnected before the script could run; nothing to update.
        }
        catch (InvalidOperationException)
        {
            // JS interop is not available yet (prerendering); the next interactive render applies it.
            keysLocked = !shouldLock;
        }
    }

    private async Task HandleLowerInput(ChangeEventArgs e)
    {
        if (!TryReadMove(e, out var requested))
            return;

        // The lower handle can go no higher than the upper value, less the minimum gap.
        var limit = (ToDouble(UpperValue) ?? max) - Gap();
        var accepted = Math.Max(Math.Min(requested, limit), min);
        var rejected = requested > limit || requested < min;

        if (!TryConvert(accepted, out var value))
            return;

        if (rejected)
            await SyncRejectedMove(requested, isLower: true);

        LowerValue = value;
        lowerInputValue = ToInputValue(value);
        await LowerValueChanged.InvokeAsync(value);
        NotifyFieldChanged();
    }

    private async Task HandleUpperInput(ChangeEventArgs e)
    {
        if (!TryReadMove(e, out var requested))
            return;

        // The upper handle can go no lower than the lower value, plus the minimum gap.
        var limit = (ToDouble(LowerValue) ?? min) + Gap();
        var accepted = Math.Min(Math.Max(requested, limit), max);
        var rejected = requested < limit || requested > max;

        if (!TryConvert(accepted, out var value))
            return;

        if (rejected)
            await SyncRejectedMove(requested, isLower: false);

        UpperValue = value;
        upperInputValue = ToInputValue(value);
        await UpperValueChanged.InvokeAsync(value);
        NotifyFieldChanged();
    }

    private double Gap() => Math.Max(ToDouble(MinDistance) ?? 0, 0);

    private bool TryReadMove(ChangeEventArgs e, out double requested)
    {
        requested = 0;
        if (ReadOnly || Disabled)
            return false;

        return double.TryParse(e.Value?.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out requested);
    }

    private static bool TryConvert(double number, out T value)
    {
        try
        {
            var type = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
            value = (T)Convert.ChangeType(number, type, CultureInfo.InvariantCulture);
            return true;
        }
        catch (Exception ex) when (ex is InvalidCastException or FormatException or OverflowException)
        {
            value = default!;
            return false;
        }
    }

    // The browser has already moved the native input past the other handle. Blazor only writes the value back
    // when it differs from the last render, so when the accepted value matches the last render the input would
    // keep showing (and announcing) the rejected value. Rendering the browser's value first makes the following
    // render write the accepted value back. The native input is invisible, so this causes no visible flicker.
    private async Task SyncRejectedMove(double requested, bool isLower)
    {
        var browserValue = requested.ToString(CultureInfo.InvariantCulture);
        if (isLower)
            lowerInputValue = browserValue;
        else
            upperInputValue = browserValue;

        StateHasChanged();
        await Task.Yield();
    }

    /// <inheritdoc />
    protected override FieldIdentifier? CreateFieldIdentifier()
    {
        var expression = LowerValueExpression ?? UpperValueExpression;
        return expression is null ? null : FieldIdentifier.Create(expression);
    }
}
