// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace TwBlazor.Configuration.Components;

/// <summary>
/// Theme configuration for the stepper components (<see cref="TwBlazor.Components.TwStepper"/>,
/// <see cref="TwBlazor.Components.TwStep"/>).
/// Override any property to customize stepper styles globally.
/// </summary>
[ExcludeFromCodeCoverage]
public class TwStepperTheme
{
    /// <summary>
    /// Gets or sets the classes for the horizontal orientation's <c>&lt;ol&gt;</c>. Hidden below the
    /// "sm" breakpoint, where the mobile fallback (<see cref="MobileContainer"/>) is shown instead.
    /// </summary>
    public required string HorizontalList { get; set; }

    /// <summary>
    /// Gets or sets the classes for the vertical orientation's <c>&lt;ol&gt;</c>.
    /// </summary>
    public required string VerticalList { get; set; }

    /// <summary>
    /// Gets or sets the classes for a single step's <c>&lt;li&gt;</c> in the horizontal orientation - a
    /// content-sized column holding the circle, label, and description. Sits alongside separate
    /// connector <c>&lt;li&gt;</c>s (see <see cref="Connector"/>) which alone stretch to fill the row.
    /// </summary>
    public required string Step { get; set; }

    /// <summary>
    /// Gets or sets the classes for a single step's <c>&lt;li&gt;</c> in the vertical orientation.
    /// </summary>
    public required string VerticalStep { get; set; }

    /// <summary>
    /// Gets or sets the classes for the circle+connector column within a vertical step.
    /// </summary>
    public required string VerticalIndicatorColumn { get; set; }

    /// <summary>
    /// Gets or sets the classes for the label+description+content column within a vertical step.
    /// </summary>
    public required string VerticalContentColumn { get; set; }

    /// <summary>
    /// Gets or sets the structural (colorless) classes for the horizontal connector between two steps -
    /// a flex-growing <c>&lt;li&gt;</c> sibling to <see cref="Step"/> that alone stretches to fill the
    /// row, so the (content-sized) steps stay evenly connected regardless of container width. Its color
    /// comes from <see cref="ConnectorNeutral"/> or the active <see cref="TwBlazor.Enums.Color"/>.
    /// </summary>
    public required string Connector { get; set; }

    /// <summary>
    /// Gets or sets the structural (colorless) classes for the vertical connector between two steps.
    /// Its color comes from <see cref="ConnectorNeutral"/> or the active <see cref="TwBlazor.Enums.Color"/>.
    /// </summary>
    public required string VerticalConnector { get; set; }

    /// <summary>
    /// Gets or sets the neutral border color applied to a connector that hasn't been reached yet.
    /// </summary>
    public required string ConnectorNeutral { get; set; }

    /// <summary>
    /// Gets or sets the shared base classes for a step's circular indicator button, regardless of
    /// orientation or status.
    /// </summary>
    public required string Circle { get; set; }

    /// <summary>
    /// Gets or sets the classes applied to an upcoming (not yet reached) step's circle.
    /// </summary>
    public required string CircleUpcoming { get; set; }

    /// <summary>
    /// Gets or sets the classes applied to a disabled step's circle.
    /// </summary>
    public required string CircleDisabled { get; set; }

    /// <summary>
    /// Gets or sets the classes applied to a step's circle when it can be clicked to navigate to it.
    /// </summary>
    public required string CircleClickable { get; set; }

    /// <summary>
    /// Gets or sets the classes for the checkmark/custom icon rendered inside a step's circle. The icon
    /// is a font glyph, so it's sized via <see cref="Circle"/>'s own font size rather than an explicit
    /// width/height - this only needs to tighten its line height for centering within the circle.
    /// </summary>
    public required string CircleIcon { get; set; }

    /// <summary>
    /// Gets or sets the text classes for an active or completed step's label.
    /// </summary>
    public required string Label { get; set; }

    /// <summary>
    /// Gets or sets the text classes for an upcoming step's label.
    /// </summary>
    public required string LabelUpcoming { get; set; }

    /// <summary>
    /// Gets or sets the text classes for a step's secondary description.
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    /// Gets or sets the classes for the shared panel that renders the active step's content below the
    /// horizontal step row.
    /// </summary>
    public required string Content { get; set; }

    /// <summary>
    /// Gets or sets the classes for the wrapper around a vertical step's inline content, rendered
    /// beneath its label while that step is active.
    /// </summary>
    public required string VerticalContent { get; set; }

    /// <summary>
    /// Gets or sets the classes for the mobile fallback container (progress bar + "Step X of N" text),
    /// shown below the "sm" breakpoint instead of <see cref="HorizontalList"/>.
    /// </summary>
    public required string MobileContainer { get; set; }

    /// <summary>
    /// Gets or sets the text classes for the "Step X of N: Label" text in the mobile fallback.
    /// </summary>
    public required string MobileLabel { get; set; }

    /// <summary>
    /// Gets or sets the classes applied to the mobile fallback progress bar's visible label, kept for
    /// its accessible name but visually hidden since <see cref="MobileLabel"/> already shows the same
    /// information beneath it.
    /// </summary>
    public required string MobileProgressLabel { get; set; }

    /// <summary>
    /// Gets or sets the classes for the frame around every step indicator. The current step's frame is drawn in
    /// the stepper's color as a ring around the indicator; the others use <see cref="CircleFrameInactive"/>.
    /// </summary>
    public string CircleFrame { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the classes added to <see cref="CircleFrame"/> for every step except the current one,
    /// normally a transparent border so the frame takes up space without being seen.
    /// </summary>
    public string CircleFrameInactive { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the classes added to <see cref="CircleUpcoming"/> for an upcoming step that can be
    /// clicked, which is any enabled one in a non-linear stepper: its hover state.
    /// </summary>
    public string CircleUpcomingClickable { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the classes added to <see cref="Label"/> for the current step's label.
    /// </summary>
    public string LabelActive { get; set; } = string.Empty;
}
