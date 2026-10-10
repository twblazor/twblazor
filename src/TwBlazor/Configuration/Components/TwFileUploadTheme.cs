// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace TwBlazor.Configuration.Components;

/// <summary>
/// Theme configuration for the file upload component (<see cref="TwBlazor.Components.TwFileUpload"/>).
/// Override any property to customize file upload styles globally. The upload button itself is
/// themed via <see cref="TwButtonTheme"/> - this covers only the pieces unique to file upload.
/// </summary>
[ExcludeFromCodeCoverage]
public class TwFileUploadTheme
{
    /// <summary>
    /// Gets or sets the spacing classes for the leading icon and the selected-file chips.
    /// </summary>
    public required string IconSpacing { get; set; }

    /// <summary>
    /// Gets or sets the classes for the list of selected-file chips shown beneath the upload control.
    /// </summary>
    public required string FileList { get; set; }

    /// <summary>
    /// Gets or sets the text color for a selected-file chip's label.
    /// </summary>
    public required string ChipTextColor { get; set; }

    /// <summary>
    /// Gets or sets the focus ring for the upload control. The file input sits inside the visible
    /// label, so the ring is drawn on the label while the input has keyboard focus.
    /// </summary>
    public string FocusRing { get; set; } = string.Empty;
}
