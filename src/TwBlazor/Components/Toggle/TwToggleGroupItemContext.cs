// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace TwBlazor.Components;

/// <summary>
/// Cascaded by a <see cref="TwToggleGroupItem{T}"/> to the <see cref="TwToggle"/> it renders, so the toggle
/// knows it is drawn inside a group and which ARIA pattern to follow.
/// </summary>
/// <param name="Radio">
/// <see langword="true"/> when the group allows a single selection, so the toggle is exposed as a radio with
/// <c>aria-checked</c>; <see langword="false"/> for a toggle button with <c>aria-pressed</c>.
/// </param>
internal sealed record TwToggleGroupItemContext(bool Radio)
{
    /// <summary>
    /// The context of an item in a single-select group.
    /// </summary>
    public static readonly TwToggleGroupItemContext Single = new(true);

    /// <summary>
    /// The context of an item in a multiple-select group.
    /// </summary>
    public static readonly TwToggleGroupItemContext Multiple = new(false);
}
