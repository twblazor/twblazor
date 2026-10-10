// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using TwBlazor.Builders;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Utilities;

namespace TwBlazor.Components.TimePicker;

public partial class TwTimePickerBody
{
    [Inject] private InputVariantBuilder inputVariantBuilder { get; set; } = null!;

    private TwTimePickerTheme theme => options.Theme.Components.Require<TwTimePickerTheme>();

    private TwInputTheme inputTheme => options.Theme.Components.Require<TwInputTheme>();

    /// <summary>
    /// Gets or sets the visual variant (Default, Outlined, Filled) of the hour/minute number inputs.
    /// Callers (<see cref="TwBlazor.Components.TwTimePicker"/> and friends) pass their own resolved
    /// <c>effectiveVariant</c> down explicitly. Left <see langword="null"/> (e.g. if this component is
    /// ever used standalone), <see cref="effectiveVariant"/> falls back to the global
    /// <see cref="TwInputTheme.DefaultInputVariant"/> instead of silently rendering as an unstyled
    /// <see cref="InputVariant.Default"/> - a non-nullable <c>Variant</c> parameter caused exactly that
    /// bug once before on <see cref="TwBlazor.Components.TwDateTimePicker"/>.
    /// </summary>
    [Parameter] public InputVariant? Variant { get; set; }

    /// <summary>
    /// Gets the effective input variant to use: <see cref="Variant"/> when explicitly set by the
    /// caller, otherwise the global default configured via <see cref="TwInputTheme.DefaultInputVariant"/>.
    /// </summary>
    private InputVariant effectiveVariant => Variant ?? inputTheme.DefaultInputVariant;

    /// <summary>
    /// Gets or sets a value indicating whether the time should be displayed in 12-hour format.
    /// </summary>
    [Parameter] public bool Is12HourFormat { get; set; }

    /// <summary>
    /// The selected time value to display.
    /// </summary>
    [Parameter] public TimeOnly SelectedTime { get; set; } = TimeOnly.FromDateTime(DateTime.Now);

    /// <summary>
    /// Event callback invoked when the selected time changes.
    /// </summary>
    [Parameter] public EventCallback<TimeOnly> SelectedTimeChanged { get; set; }

    /// <summary>
    /// Event callback invoked when the hour increment button is clicked.
    /// </summary>
    [Parameter] public EventCallback OnHourIncreased { get; set; }

    /// <summary>
    /// Event callback invoked when the hour decrement button is clicked.
    /// </summary>
    [Parameter] public EventCallback OnHourDecreased { get; set; }

    /// <summary>
    /// Event callback invoked when the minute increment button is clicked.
    /// </summary>
    [Parameter] public EventCallback OnMinuteIncreased { get; set; }

    /// <summary>
    /// Event callback invoked when the minute decrement button is clicked.
    /// </summary>
    [Parameter] public EventCallback OnMinuteDecreased { get; set; }

    /// <summary>
    /// Event callback invoked when the hour input value changes.
    /// </summary>
    [Parameter] public EventCallback<string> OnHourValueChanged { get; set; }

    /// <summary>
    /// Event callback invoked when the minute input value changes.
    /// </summary>
    [Parameter] public EventCallback<string> OnMinuteValueChanged { get; set; }

    private string rootClasses => new ClassBuilder(theme.BodyRoot).Build();

    private string classes => new ClassBuilder(theme.BodyInner).AddClass(Class).Build();

    /// <summary>
    /// Gets the classes for the hour/minute number inputs. Structural/typography classes come from
    /// <see cref="TwTimePickerTheme.NumberInput"/>; border, background, and rounding come from
    /// <see cref="InputVariantBuilder"/> via <see cref="Variant"/> so these inputs render the same
    /// Default/Outlined/Filled look as every other text input instead of a look hardcoded independent
    /// of the configured variant. The hover border and focus ring colors are resolved dynamically from
    /// the shared theme color tokens (<see cref="TwBlazor.Builders.ColorBuilder.GetFocusRing"/>) so the
    /// inputs track the app's primary color.
    /// </summary>
    private string numberInputClasses => new ClassBuilder(theme.NumberInput)
        .AddClass(inputVariantBuilder.GetClasses(effectiveVariant, inputTheme))
        .AddClass(ToHoverVariant(options.Theme.Border.Colors.Primary))
        .AddClass(colorBuilder.GetFocusRing(Enums.Color.Primary))
        .Build();

    /// <summary>
    /// Rewrites a plain/"dark:"-prefixed color token string (e.g.
    /// <c>"border-purple-600 dark:border-purple-500"</c>) into its hover-only equivalent
    /// (<c>"hover:border-purple-600 dark:hover:border-purple-500"</c>), matching the
    /// <c>dark:hover:</c> convention already used by the shared hover color tokens.
    /// </summary>
    private static string ToHoverVariant(string classes) =>
        string.Join(' ', classes.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(c => c.StartsWith("dark:", StringComparison.Ordinal)
                ? $"dark:hover:{c["dark:".Length..]}"
                : $"hover:{c}"));

    /// <summary>
    /// Sets the hour value of the selected time.
    /// </summary>
    /// <param name="hourValue">The hour value as a string to be parsed.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task SetHourValue(string hourValue)
    {
        if (OnHourValueChanged.HasDelegate)
        {
            await OnHourValueChanged.InvokeAsync(hourValue);
        }
        else
        {
            _ = int.TryParse(hourValue, out var hour);
            await UpdateSelectedTime(UpdateTime(hour, SelectedTime.Minute));
        }
    }

    /// <summary>
    /// Sets the minute value of the selected time.
    /// </summary>
    /// <param name="minuteValue">The minute value as a string to be parsed.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task SetMinuteValue(string minuteValue)
    {
        if (OnMinuteValueChanged.HasDelegate)
        {
            await OnMinuteValueChanged.InvokeAsync(minuteValue);
        }
        else
        {
            _ = int.TryParse(minuteValue, out var minute);
            await UpdateSelectedTime(UpdateTime(SelectedTime.Hour, minute));
        }
    }

    /// <summary>
    /// Updates the selected time and invokes the relevant event callbacks.
    /// </summary>
    /// <param name="newTime">The new time value to set.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task UpdateSelectedTime(TimeOnly newTime)
    {
        SelectedTime = newTime;
        if (SelectedTimeChanged.HasDelegate)
            await SelectedTimeChanged.InvokeAsync(SelectedTime);
        StateHasChanged();
    }

    /// <summary>
    /// Increments the hour of the selected time by one hour.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task IncrementHour()
    {
        if (OnHourIncreased.HasDelegate)
        {
            await OnHourIncreased.InvokeAsync();
        }
        else
        {
            var hour = (SelectedTime.Hour + 1) % 24;
            await UpdateSelectedTime(new TimeOnly(hour, SelectedTime.Minute, SelectedTime.Second));
        }
    }

    /// <summary>
    /// Decrements the hour of the selected time by one hour.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task DecrementHour()
    {
        if (OnHourDecreased.HasDelegate)
        {
            await OnHourDecreased.InvokeAsync();
        }
        else
        {
            var hour = (SelectedTime.Hour + 23) % 24;
            await UpdateSelectedTime(new TimeOnly(hour, SelectedTime.Minute, SelectedTime.Second));
        }
    }

    /// <summary>
    /// Increments the minute of the selected time by one minute.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task IncrementMinute()
    {
        if (OnMinuteIncreased.HasDelegate)
        {
            await OnMinuteIncreased.InvokeAsync();
        }
        else
        {
            var minute = SelectedTime.Minute + 1;
            var hour = SelectedTime.Hour;
            if (minute >= 60)
            {
                minute = 0;
                hour = (hour + 1) % 24;
            }
            await UpdateSelectedTime(new TimeOnly(hour, minute, SelectedTime.Second));
        }
    }

    /// <summary>
    /// Decrements the minute of the selected time by one minute.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task DecrementMinute()
    {
        if (OnMinuteDecreased.HasDelegate)
        {
            await OnMinuteDecreased.InvokeAsync();
        }
        else
        {
            var minute = SelectedTime.Minute - 1;
            var hour = SelectedTime.Hour;
            if (minute < 0)
            {
                minute = 59;
                hour = (hour + 23) % 24;
            }
            await UpdateSelectedTime(new TimeOnly(hour, minute, SelectedTime.Second));
        }
    }

    /// <summary>
    /// Toggles between AM and PM for 12-hour format.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    // Arrow Up and Arrow Down step the value, as they do in a native number field.
    private Task OnHourKeyDown(KeyboardEventArgs e) => e.Key switch
    {
        "ArrowUp" => IncrementHour(),
        "ArrowDown" => DecrementHour(),
        _ => Task.CompletedTask
    };

    private Task OnMinuteKeyDown(KeyboardEventArgs e) => e.Key switch
    {
        "ArrowUp" => IncrementMinute(),
        "ArrowDown" => DecrementMinute(),
        _ => Task.CompletedTask
    };

    private async Task ToggleAmPm()
    {
        var hour = SelectedTime.Hour;

        // Toggle between AM and PM by adding or subtracting 12 hours
        if (hour >= 12)
        {
            // PM to AM
            hour -= 12;
        }
        else
        {
            // AM to PM
            hour += 12;
        }

        await UpdateSelectedTime(new TimeOnly(hour, SelectedTime.Minute, SelectedTime.Second));
    }

    /// <summary>
    /// Normalizes and applies hour/minute changes based on the current format (12h vs 24h).
    /// Validation rules:
    ///  - 24h format: hour clamped to 0–23.
    ///  - 12h format: incoming hour clamped to 1–12, then converted to 24h preserving the current AM/PM period.
    ///  - Minutes clamped to 0–59.
    /// </summary>
    private TimeOnly UpdateTime(int newHour, int newMinute)
    {
        newMinute = Math.Clamp(newMinute, 0, 59);
        newHour = Is12HourFormat ? Normalize12HourTo24Hour(newHour) : Math.Clamp(newHour, 0, 23);
        return new TimeOnly(newHour, newMinute, SelectedTime.Second);
    }

    private int Normalize12HourTo24Hour(int hour)
    {
        hour = Math.Clamp(hour, 1, 12);
        var currentIsPm = SelectedTime.Hour >= 12;
        if (hour == 12)
            return currentIsPm ? 12 : 0; // 12 PM = 12, 12 AM = 0
        return currentIsPm ? hour + 12 : hour;
    }
}
