// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace TwBlazor.Configuration.Components;

/// <summary>
/// Theme configuration for the form component (<see cref="TwBlazor.Components.TwForm"/>) and its error summary.
/// Override any property to customize form styles globally.
/// </summary>
[ExcludeFromCodeCoverage]
public class TwFormTheme
{
    /// <summary>
    /// Gets or sets the classes applied to the form element, such as the gap between its fields.
    /// </summary>
    public required string Base { get; set; }

    /// <summary>
    /// Gets or sets the classes for the error summary box shown above the fields after a failed submit.
    /// </summary>
    public required string Summary { get; set; }

    /// <summary>
    /// Gets or sets the classes for the error summary's heading.
    /// </summary>
    public required string SummaryTitle { get; set; }

    /// <summary>
    /// Gets or sets the classes for the list of errors in the summary.
    /// </summary>
    public required string SummaryList { get; set; }

    /// <summary>
    /// Gets or sets the classes for an error in the summary that links to its field.
    /// </summary>
    public required string SummaryLink { get; set; }
}
