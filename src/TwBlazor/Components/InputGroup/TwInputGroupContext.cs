// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using TwBlazor.Enums;

namespace TwBlazor.Components;

/// <summary>
/// What a <see cref="TwInputGroup"/> cascades to the inputs, addons and buttons inside it. A new instance is cascaded
/// whenever any of it changes, so they re-render with the group.
/// </summary>
/// <param name="Group">The group itself, which inputs report their validation messages to.</param>
/// <param name="Variant">The group's effective input variant.</param>
/// <param name="Dense">Whether the group, and the inputs and buttons inside it, use the compact size.</param>
/// <param name="Rounded">The group's effective rounding.</param>
/// <param name="ControlClasses">The classes added to an input inside the group, after its own variant classes.</param>
/// <param name="ControlRootClasses">The classes added to the root element of the outermost input inside the group.</param>
/// <param name="DescribedBy">The ids of the group's texts that describe its input, separated by spaces, or <see langword="null"/>.</param>
internal sealed record TwInputGroupContext(
    TwInputGroup Group,
    InputVariant Variant,
    bool Dense,
    Rounded Rounded,
    string ControlClasses,
    string ControlRootClasses,
    string? DescribedBy)
{
    /// <summary>
    /// Joins an input's own <c>aria-describedby</c> ids with the group's.
    /// </summary>
    /// <param name="context">The cascaded group, or <see langword="null"/> outside a group.</param>
    /// <param name="ownIds">The input's own ids, such as its error message's, or <see langword="null"/>.</param>
    /// <returns>The ids separated by spaces, or <see langword="null"/> when there are none.</returns>
    public static string? JoinDescribedBy(TwInputGroupContext? context, string? ownIds)
    {
        var groupIds = context?.DescribedBy;
        if (string.IsNullOrEmpty(groupIds))
        {
            return string.IsNullOrEmpty(ownIds) ? null : ownIds;
        }

        return string.IsNullOrEmpty(ownIds) ? groupIds : $"{ownIds} {groupIds}";
    }
}
