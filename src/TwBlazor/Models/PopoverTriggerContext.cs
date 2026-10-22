// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace TwBlazor.Models;

/// <summary>
/// What a custom trigger for a <see cref="Components.TwPopover"/> needs: the attributes that tie it to the panel for
/// assistive technology, and a way to open and close the panel.
/// </summary>
/// <remarks>
/// Splat <see cref="Attributes"/> onto the element that opens the popover, which should be a button, and call
/// <see cref="ToggleAsync"/> from its click handler.
/// </remarks>
[ExcludeFromCodeCoverage]
public sealed class PopoverTriggerContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PopoverTriggerContext"/> class.
    /// </summary>
    /// <param name="attributes">The trigger's id and ARIA attributes.</param>
    /// <param name="isOpen">Whether the panel is open.</param>
    /// <param name="toggleAsync">Opens the panel when it is closed, and closes it when it is open.</param>
    public PopoverTriggerContext(IReadOnlyDictionary<string, object> attributes, bool isOpen, Func<Task> toggleAsync)
    {
        Attributes = attributes;
        IsOpen = isOpen;
        ToggleAsync = toggleAsync;
    }

    /// <summary>
    /// Gets the trigger's id and the <c>aria-haspopup</c>, <c>aria-expanded</c> and <c>aria-controls</c> attributes
    /// that tie it to the panel.
    /// </summary>
    public IReadOnlyDictionary<string, object> Attributes { get; }

    /// <summary>
    /// Gets whether the panel is open.
    /// </summary>
    public bool IsOpen { get; }

    /// <summary>
    /// Gets the function that opens the panel when it is closed, and closes it when it is open.
    /// </summary>
    public Func<Task> ToggleAsync { get; }
}
