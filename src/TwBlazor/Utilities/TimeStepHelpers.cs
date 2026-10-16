// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace TwBlazor.Utilities;

/// <summary>
/// Shared time stepping calculations used by the time picker components (e.g. <see cref="TwBlazor.Components.TwTimePicker"/>).
/// </summary>
public static class TimeStepHelpers
{
    private const int minutesPerHour = 60;

    private const int minutesPerDay = 1440;

    /// <summary>
    /// Determines whether <paramref name="step"/> is a supported step in minutes: a divisor of an hour
    /// (1, 2, 3, 4, 5, 6, 10, 12, 15, 20 or 30) or a whole number of hours that divides a day
    /// (60, 120, 180, 240, 360, 480 or 720).
    /// </summary>
    /// <param name="step">The step in minutes.</param>
    /// <returns><see langword="true"/> when the step lines up with the clock; otherwise <see langword="false"/>.</returns>
    public static bool IsValidStep(int step) => step > 0
        && (minutesPerHour % step == 0 || (step % minutesPerHour == 0 && step < minutesPerDay && minutesPerDay % step == 0));

    /// <summary>
    /// Throws when <paramref name="step"/> is not supported by <see cref="IsValidStep"/>.
    /// </summary>
    /// <param name="step">The step in minutes.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the step does not line up with the clock.</exception>
    public static void ValidateStep(int step)
    {
        if (!IsValidStep(step))
        {
            throw new ArgumentOutOfRangeException(nameof(step), step,
                "Step must be a number of minutes that divides an hour (1, 2, 3, 4, 5, 6, 10, 12, 15, 20 or 30) " +
                "or a whole number of hours that divides a day (60, 120, 180, 240, 360, 480 or 720).");
        }
    }

    /// <summary>
    /// Moves <paramref name="time"/> forward to the next multiple of <paramref name="step"/> minutes after
    /// midnight, wrapping past midnight. A time already on the step moves a whole step; a time between
    /// steps moves to the next one, as a native <c>&lt;input type="time"&gt;</c> does.
    /// </summary>
    /// <param name="time">The current time.</param>
    /// <param name="step">The step in minutes.</param>
    /// <returns>The stepped time, keeping the seconds of <paramref name="time"/>.</returns>
    public static TimeOnly StepUp(TimeOnly time, int step)
    {
        var minuteOfDay = (time.Hour * minutesPerHour) + time.Minute;
        var next = ((minuteOfDay / step) + 1) * step % minutesPerDay;
        return FromMinuteOfDay(next, time.Second);
    }

    /// <summary>
    /// Moves <paramref name="time"/> back to the previous multiple of <paramref name="step"/> minutes after
    /// midnight, wrapping past midnight. A time already on the step moves a whole step; a time between
    /// steps moves to the one before it.
    /// </summary>
    /// <param name="time">The current time.</param>
    /// <param name="step">The step in minutes.</param>
    /// <returns>The stepped time, keeping the seconds of <paramref name="time"/>.</returns>
    public static TimeOnly StepDown(TimeOnly time, int step)
    {
        var minuteOfDay = (time.Hour * minutesPerHour) + time.Minute;
        var remainder = minuteOfDay % step;
        var previous = minuteOfDay - (remainder == 0 ? step : remainder);
        return FromMinuteOfDay((previous + minutesPerDay) % minutesPerDay, time.Second);
    }

    private static TimeOnly FromMinuteOfDay(int minuteOfDay, int second) =>
        new(minuteOfDay / minutesPerHour, minuteOfDay % minutesPerHour, second);
}
