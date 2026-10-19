// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace TwBlazor.Models;

/// <summary>
/// Represents an item in a radio button group with a label and value.
/// </summary>
/// <typeparam name="TValue">The type of the value associated with the radio button.</typeparam>
[ExcludeFromCodeCoverage]
public class RadioGroupItem<TValue>
{
    /// <summary>
    /// Gets or sets the display label for the radio button.
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value associated with the radio button.
    /// </summary>
    public TValue Value { get; set; } = default!;
}
