// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace TwBlazor.Configuration.Components;

/// <summary>
/// Theme configuration for card components (<see cref="TwBlazor.Components.TwCard"/>).
/// Override any property to customize card styles globally.
/// </summary>
[ExcludeFromCodeCoverage]
public class TwCardTheme
{
    /// <summary>
    /// Gets or sets the classes for the card's outer element, which stacks the media, content and footer.
    /// </summary>
    public string Container { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the classes for the card border when its <c>Bordered</c> parameter is <c>true</c>.
    /// </summary>
    public string Bordered { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the classes for the card's <c>Title</c> heading.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the classes for the padded area that holds the header and the card's main content.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the classes for the header, which holds the <c>Title</c> and <c>HeaderContent</c>.
    /// </summary>
    public string Header { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the classes for the media area at the top of the card, which holds the image and
    /// <c>MediaContent</c>. Its top corners follow the card's rounding.
    /// </summary>
    public string Media { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the classes for the image set through <c>ImageSrc</c>.
    /// </summary>
    public string Image { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the classes for the footer that holds <c>FooterContent</c>, such as actions.
    /// </summary>
    public string Footer { get; set; } = string.Empty;
}
