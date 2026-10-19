// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace TwBlazor.Models;

/// <summary>
/// Represents an item in a checkbox group with a label, value, and selection state.
/// </summary>
/// <typeparam name="TValue">The type of the value associated with the checkbox.</typeparam>
[ExcludeFromCodeCoverage]
public class CheckboxGroupItem<TValue>
{
    /// <summary>
    /// Gets or sets the display label for the checkbox.
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value associated with the checkbox.
    /// </summary>
    public TValue Value { get; set; } = default!;

    /// <summary>
    /// Gets or sets whether this checkbox is selected.
    /// </summary>
    public bool IsSelected { get; set; }
}
