// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace TwBlazor.Enums;

/// <summary>
/// Defines where a file attachment is in its upload.
/// </summary>
public enum AttachmentStatus
{
    /// <summary>
    /// The file is attached and ready. Its size and type are shown.
    /// </summary>
    Ready,

    /// <summary>
    /// The file is being uploaded. A progress bar is shown in place of its size.
    /// </summary>
    Uploading,

    /// <summary>
    /// The upload failed. An error is shown in place of its size.
    /// </summary>
    Failed
}
