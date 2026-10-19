// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace TwBlazor.Configuration.Components;

/// <summary>
/// Theme configuration for the file attachment chip (<see cref="TwBlazor.Components.TwAttachment"/>).
/// Override any property to customize attachment styles globally.
/// </summary>
[ExcludeFromCodeCoverage]
public class TwAttachmentTheme
{
    /// <summary>
    /// Gets or sets the classes for the attachment's outer box.
    /// </summary>
    public required string Base { get; set; }

    /// <summary>
    /// Gets or sets the classes added to the outer box when the upload failed.
    /// </summary>
    public required string Failed { get; set; }

    /// <summary>
    /// Gets or sets the classes for the preview image.
    /// </summary>
    public required string Preview { get; set; }

    /// <summary>
    /// Gets or sets the classes for the box that holds the file type icon when there is no preview.
    /// </summary>
    public required string IconBox { get; set; }

    /// <summary>
    /// Gets or sets the classes for the column that holds the file name and its size, progress or error.
    /// </summary>
    public required string Details { get; set; }

    /// <summary>
    /// Gets or sets the classes for the file name.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the classes added to the file name when it is a link.
    /// </summary>
    public required string NameLink { get; set; }

    /// <summary>
    /// Gets or sets the classes for the file's size and type.
    /// </summary>
    public required string Meta { get; set; }

    /// <summary>
    /// Gets or sets the classes for the error shown when the upload failed.
    /// </summary>
    public required string Error { get; set; }

    /// <summary>
    /// Gets or sets the classes for the row of action buttons.
    /// </summary>
    public required string Actions { get; set; }
}
