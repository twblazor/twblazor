// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace TwBlazor.Enums;

/// <summary>
/// Represents why <see cref="TwBlazor.Components.TwTagInput"/> did not add a tag.
/// </summary>
public enum TagRejectionReason
{
    /// <summary>
    /// The tag has already been added, and <see cref="TwBlazor.Components.TwTagInput.AllowDuplicates"/> is not set.
    /// </summary>
    Duplicate,

    /// <summary>
    /// The field already holds <see cref="TwBlazor.Components.TwTagInput.MaxTags"/> tags.
    /// </summary>
    MaxTags,

    /// <summary>
    /// The tag is longer than <see cref="TwBlazor.Components.TwTagInput.MaxTagLength"/> characters.
    /// </summary>
    MaxLength,

    /// <summary>
    /// <see cref="TwBlazor.Components.TwTagInput.TagValidator"/> returned an error for the tag.
    /// </summary>
    Invalid
}
