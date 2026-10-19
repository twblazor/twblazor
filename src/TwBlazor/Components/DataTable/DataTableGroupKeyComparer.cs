// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace TwBlazor.Components;

/// <summary>
/// Orders <see cref="TwDataTable{TItem}"/> group keys with blank keys first, falling back to their text when two keys cannot be compared directly.
/// </summary>
internal sealed class DataTableGroupKeyComparer : IComparer<object?>
{
    /// <summary>
    /// The shared instance. It is kept outside the generic table, so every <c>TwDataTable&lt;TItem&gt;</c> uses this one.
    /// </summary>
    public static readonly DataTableGroupKeyComparer Instance = new();

    /// <inheritdoc />
    public int Compare(object? x, object? y)
    {
        if (x is null && y is null)
        {
            return 0;
        }

        if (x is null)
        {
            return -1;
        }

        if (y is null)
        {
            return 1;
        }

        try
        {
            return Comparer<object>.Default.Compare(x, y);
        }
        catch (ArgumentException)
        {
            return string.Compare(x.ToString(), y.ToString(), StringComparison.CurrentCulture);
        }
    }
}
