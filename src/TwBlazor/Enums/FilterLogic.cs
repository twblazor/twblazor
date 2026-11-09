// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace TwBlazor.Enums;

/// <summary>
/// How the two conditions of a <see cref="TwBlazor.Models.ColumnFilter"/> combine.
/// </summary>
public enum FilterLogic
{
    /// <summary>A value must match both conditions.</summary>
    And,

    /// <summary>A value must match at least one condition.</summary>
    Or,
}
