// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace TwBlazor.Enums;

/// <summary>
/// Defines which side of a conversation a chat message sits on.
/// </summary>
public enum MessageAlign
{
    /// <summary>
    /// The message sits at the start edge, with its avatar first. Used for messages from other people or an assistant.
    /// </summary>
    Start,

    /// <summary>
    /// The message sits at the end edge, with its avatar last. Used for the current user's own messages.
    /// </summary>
    End
}
