// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace TwBlazor.Configuration.Components;

/// <summary>
/// Theme configuration for the skeleton loading placeholder component (<see cref="TwBlazor.Components.TwSkeleton"/>).
/// Override any property to customize skeleton styles globally.
/// </summary>
[ExcludeFromCodeCoverage]
public class TwSkeletonTheme
{
    /// <summary>
    /// Gets or sets the base classes shared by every placeholder block, including its background color
    /// and the <c>overflow-hidden</c>/<c>relative</c> positioning the <see cref="Wave"/> shimmer needs.
    /// </summary>
    public required string Base { get; set; }

    /// <summary>
    /// Gets or sets the classes for a <see cref="Enums.SkeletonType.Text"/> placeholder (a single line).
    /// </summary>
    public required string Text { get; set; }

    /// <summary>
    /// Gets or sets the classes for a <see cref="Enums.SkeletonType.Circle"/> placeholder.
    /// </summary>
    public required string Circle { get; set; }

    /// <summary>
    /// Gets or sets the classes for a <see cref="Enums.SkeletonType.Rectangle"/> placeholder.
    /// </summary>
    public required string Rectangle { get; set; }

    /// <summary>
    /// Gets or sets the classes applied to the wrapper that holds a hidden copy of <c>ChildContent</c>
    /// while it is being measured.
    /// </summary>
    public required string MeasuringWrapper { get; set; }

    /// <summary>
    /// Gets or sets the classes applied for <see cref="Enums.SkeletonAnimation.Pulse"/>.
    /// </summary>
    public required string Pulse { get; set; }

    /// <summary>
    /// Gets or sets the classes applied for <see cref="Enums.SkeletonAnimation.Wave"/>. Pairs with the
    /// <c>tw-wave</c> shimmer defined in <c>input.css</c>, which needs the <see cref="Base"/>
    /// class's <c>relative</c>/<c>overflow-hidden</c> to clip correctly.
    /// </summary>
    public required string Wave { get; set; }
}
