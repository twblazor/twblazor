// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace TwBlazor.Components;

/// <summary>
/// Timing values shared by the calendar components. Kept out of the generic calendar classes because a
/// static field in a generic type is duplicated per closed type instead of shared.
/// </summary>
internal static class TwCalendarTimings
{
    /// <summary>
    /// How often the Day/Week views refresh their current-time indicator.
    /// </summary>
    internal static TimeSpan nowTickInterval { get; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// How long the Week/Month view pulses the day after "Today" is pressed.
    /// </summary>
    internal static TimeSpan highlightDuration { get; } = TimeSpan.FromSeconds(2);

    /// <summary>
    /// How long a drag has to be live before the views switch into drag mode.
    /// </summary>
    internal static TimeSpan dragActivationDelay { get; } = TimeSpan.FromMilliseconds(25);

    /// <summary>
    /// Every view, in switcher order.
    /// </summary>
    internal static TwCalendarView[] allViews { get; } = [TwCalendarView.Day, TwCalendarView.Week, TwCalendarView.Month];
}
