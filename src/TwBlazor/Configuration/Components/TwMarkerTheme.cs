// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace TwBlazor.Configuration.Components;

/// <summary>
/// Theme configuration for the status marker and separator (<see cref="TwBlazor.Components.TwMarker"/>).
/// Override any property to customize marker styles globally.
/// </summary>
[ExcludeFromCodeCoverage]
public class TwMarkerTheme
{
    /// <summary>
    /// Gets or sets the classes for a <see cref="Enums.MarkerVariant.Status"/> marker's row.
    /// </summary>
    public required string Status { get; set; }

    /// <summary>
    /// Gets or sets the classes for the box that holds a status marker's icon or spinner.
    /// </summary>
    public required string Icon { get; set; }

    /// <summary>
    /// Gets or sets the icon color of a <see cref="Enums.MarkerStatus.Pending"/> marker.
    /// </summary>
    public required string Pending { get; set; }

    /// <summary>
    /// Gets or sets the icon color of a <see cref="Enums.MarkerStatus.Succeeded"/> marker.
    /// </summary>
    public required string Succeeded { get; set; }

    /// <summary>
    /// Gets or sets the icon color of a <see cref="Enums.MarkerStatus.Failed"/> marker.
    /// </summary>
    public required string Failed { get; set; }

    /// <summary>
    /// Gets or sets the classes for a status marker's label.
    /// </summary>
    public required string Label { get; set; }

    /// <summary>
    /// Gets or sets the classes for the detail after a status marker's label, such as how long the step took.
    /// </summary>
    public required string Detail { get; set; }

    /// <summary>
    /// Gets or sets the classes for a <see cref="Enums.MarkerVariant.Separator"/> marker's row.
    /// </summary>
    public required string Separator { get; set; }

    /// <summary>
    /// Gets or sets the classes for the rule on either side of a separator's label.
    /// </summary>
    public required string Line { get; set; }

    /// <summary>
    /// Gets or sets the classes for a separator's label.
    /// </summary>
    public required string SeparatorLabel { get; set; }
}
