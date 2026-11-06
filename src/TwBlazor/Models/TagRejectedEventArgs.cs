// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using TwBlazor.Enums;

namespace TwBlazor.Models;

/// <summary>
/// Describes a tag that <see cref="Components.TwTagInput"/> did not add, passed to its
/// <see cref="Components.TwTagInput.OnTagRejected"/> callback.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class TagRejectedEventArgs
{
    /// <summary>
    /// Gets the tag that was not added, with surrounding white space removed.
    /// </summary>
    public required string Tag { get; init; }

    /// <summary>
    /// Gets why the tag was not added.
    /// </summary>
    public required TagRejectionReason Reason { get; init; }

    /// <summary>
    /// Gets the message shown under the field and announced to screen readers.
    /// </summary>
    public required string Message { get; init; }
}
