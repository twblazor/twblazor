// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace TwBlazor.Configuration.Components;

/// <summary>
/// Theme configuration for numeric fields (<see cref="TwBlazor.Components.TwNumeric{T}"/>).
/// Override any property to customize numeric field styles globally.
/// </summary>
/// <remarks>
/// The field is a <see cref="TwBlazor.Components.TwTextfield{T}"/>, so its border, background, focus ring, sizes, clear
/// button and the rows drawn over its start and end come from <see cref="TwInputTheme"/>. This theme only covers the
/// prefix and suffix text and the decrease and increase buttons. The field's padding is widened to make room for them,
/// assuming each button is 2rem wide.
/// </remarks>
[ExcludeFromCodeCoverage]
public class TwNumericTheme
{
    /// <summary>
    /// Gets or sets the classes for the prefix and suffix text, such as a currency symbol or a unit.
    /// </summary>
    /// <remarks>
    /// The text is read out as part of the field's value, and needs the 4.5:1 text contrast ratio against the field.
    /// </remarks>
    public required string Adornment { get; set; }

    /// <summary>
    /// Gets or sets the classes for the decrease and increase buttons, including their hover and disabled states. Each
    /// button is 2rem wide and as tall as the field.
    /// </summary>
    public required string Button { get; set; }

    /// <summary>
    /// Gets or sets the classes for the icon inside each button.
    /// </summary>
    public required string ButtonIcon { get; set; }
}
