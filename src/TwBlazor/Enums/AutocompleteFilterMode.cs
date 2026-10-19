// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace TwBlazor.Enums;

/// <summary>
/// Represents how <see cref="TwBlazor.Components.TwAutocomplete{T}"/> matches the typed text against each option.
/// </summary>
public enum AutocompleteFilterMode
{
    /// <summary>
    /// Shows the options whose text contains the typed text anywhere.
    /// </summary>
    Contains,

    /// <summary>
    /// Shows the options whose text starts with the typed text.
    /// </summary>
    StartsWith
}
