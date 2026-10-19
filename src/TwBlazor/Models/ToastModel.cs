// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using TwBlazor.Enums;

namespace TwBlazor.Models;

/// <summary>
/// Represents a toast notification message.
/// </summary>
[ExcludeFromCodeCoverage]
public class ToastModel
{
    /// <summary>
    /// Gets or sets the unique identifier for the toast.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the title of the toast.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the message content of the toast.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the color theme of the toast.
    /// </summary>
    public Color? Color { get; set; }

    /// <summary>
    /// Gets or sets the icon to display in the toast.
    /// </summary>
    public Icon? Icon { get; set; }

    /// <summary>
    /// Gets or sets the duration in milliseconds before the toast auto-dismisses.
    /// </summary>
    /// <remarks>
    /// Set to 0 to disable auto-dismiss. Default is 5000ms (5 seconds).
    /// </remarks>
    public int Duration { get; set; } = 5000;

    /// <summary>
    /// Gets or sets the timestamp when the toast was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets whether the pointer is currently hovering this toast.
    /// </summary>
    public bool IsHovered { get; private set; }

    /// <summary>
    /// Gets whether this toast (or a descendant, e.g. its close button) currently has keyboard focus.
    /// </summary>
    public bool IsFocused { get; private set; }

    /// <summary>
    /// Gets whether the toast's auto-dismiss timer is currently paused, i.e. because the pointer is
    /// hovering it, it has keyboard focus, or both.
    /// </summary>
    /// <remarks>
    /// Hover and focus are tracked independently so that, e.g., moving the pointer away from a toast
    /// that still has keyboard focus (or vice versa) doesn't prematurely resume the auto-dismiss timer
    /// while the toast is still being interacted with.
    /// </remarks>
    public bool IsPaused => IsHovered || IsFocused;

    /// <summary>
    /// The UTC time this toast most recently transitioned from unpaused to paused, used to accumulate
    /// <see cref="pausedDuration"/> once it transitions back to fully unpaused.
    /// </summary>
    private DateTime? pausedAt;

    /// <summary>
    /// The total time this toast has spent paused so far, excluded from the elapsed time used by
    /// <see cref="IsBurnt"/> so a pause doesn't count against the toast's display duration.
    /// </summary>
    private TimeSpan pausedDuration = TimeSpan.Zero;

    /// <summary>
    /// Marks the toast as hovered by the pointer, pausing its auto-dismiss timer.
    /// </summary>
    public void PauseForHover()
    {
        IsHovered = true;
        BeginPauseIfNeeded();
    }

    /// <summary>
    /// Marks the toast as no longer hovered by the pointer, resuming its auto-dismiss timer only if it
    /// also doesn't currently have keyboard focus.
    /// </summary>
    public void ResumeForHover()
    {
        IsHovered = false;
        EndPauseIfNoLongerPaused();
    }

    /// <summary>
    /// Marks the toast as having keyboard focus, pausing its auto-dismiss timer.
    /// </summary>
    public void PauseForFocus()
    {
        IsFocused = true;
        BeginPauseIfNeeded();
    }

    /// <summary>
    /// Marks the toast as no longer having keyboard focus, resuming its auto-dismiss timer only if the
    /// pointer also isn't currently hovering it.
    /// </summary>
    public void ResumeForFocus()
    {
        IsFocused = false;
        EndPauseIfNoLongerPaused();
    }

    private void BeginPauseIfNeeded() => pausedAt ??= DateTime.UtcNow;

    private void EndPauseIfNoLongerPaused()
    {
        if (IsPaused || !pausedAt.HasValue)
            return;

        pausedDuration += DateTime.UtcNow - pausedAt.Value;
        pausedAt = null;
    }

    /// <summary>
    /// Gets whether the toast has exceeded its display duration.
    /// </summary>
    /// <remarks>
    /// Always <see langword="false"/> while <see cref="IsPaused"/>, and time spent paused is excluded from
    /// the elapsed duration once resumed, so hovering/focusing a toast never counts against its lifetime.
    /// </remarks>
    public bool IsBurnt =>
        Duration > 0
        && !IsPaused
        && DateTime.UtcNow.Subtract(CreatedAt).Subtract(pausedDuration).TotalMilliseconds >= Duration;

    /// <summary>
    /// Gets the elapsed time text for display.
    /// </summary>
    public string ElapsedTimeText
    {
        get
        {
            var elapsed = DateTime.UtcNow.Subtract(CreatedAt);
            if (elapsed.TotalSeconds < 60)
                return "just now";
            if (elapsed.TotalMinutes < 60)
                return $"{(int)elapsed.TotalMinutes}m ago";
            if (elapsed.TotalHours < 24)
                return $"{(int)elapsed.TotalHours}h ago";
            return $"{(int)elapsed.TotalDays}d ago";
        }
    }
}
