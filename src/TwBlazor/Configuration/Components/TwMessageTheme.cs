// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace TwBlazor.Configuration.Components;

/// <summary>
/// Theme configuration for the chat message row (<see cref="TwBlazor.Components.TwMessage"/>).
/// Override any property to customize message styles globally.
/// </summary>
[ExcludeFromCodeCoverage]
public class TwMessageTheme
{
    /// <summary>
    /// Gets or sets the classes for the row that holds the avatar and the content column.
    /// </summary>
    public required string Base { get; set; }

    /// <summary>
    /// Gets or sets the classes that keep a continued message's content in line with the messages above it,
    /// in place of the avatar it does not repeat. Match its width to the avatar's.
    /// </summary>
    public required string AvatarSpacer { get; set; }

    /// <summary>
    /// Gets or sets the classes for the column that holds the header, the content and the footer.
    /// </summary>
    public required string Content { get; set; }

    /// <summary>
    /// Gets or sets the classes for the header row that holds the author and the time.
    /// </summary>
    public required string Header { get; set; }

    /// <summary>
    /// Gets or sets the classes for the author's name.
    /// </summary>
    public required string Author { get; set; }

    /// <summary>
    /// Gets or sets the classes for the time the message was sent.
    /// </summary>
    public required string Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the classes for the footer under the content, such as message actions or a delivery state.
    /// </summary>
    public required string Footer { get; set; }
}
