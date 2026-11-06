// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using TwBlazor.Configuration.Color;

namespace TwBlazor.Configuration.Components;

/// <summary>
/// Theme configuration for the separator component (<see cref="TwBlazor.Components.TwSeparator"/>).
/// Override any property to customize separator styles globally.
/// </summary>
/// <remarks>
/// A separator without a label is a single element that is itself the line. A separator with a label is a row
/// (or, when vertical, a column) holding a line, the label and a second line; the lines take their color from it.
/// The line is drawn as a border so it stays visible in forced colors (Windows high contrast) mode.
/// </remarks>
[ExcludeFromCodeCoverage]
public class TwSeparatorTheme
{
    /// <summary>
    /// Gets or sets the base classes applied to every separator's root element.
    /// </summary>
    public required string Base { get; set; }

    /// <summary>
    /// Gets or sets the size classes for a horizontal separator without a label, such as its width.
    /// </summary>
    public required string Horizontal { get; set; }

    /// <summary>
    /// Gets or sets the size classes for a vertical separator without a label, such as stretching to the height
    /// of its row.
    /// </summary>
    public required string Vertical { get; set; }

    /// <summary>
    /// Gets or sets the layout classes for a horizontal separator with a label: the row holding the lines and
    /// the label.
    /// </summary>
    public required string LabeledHorizontal { get; set; }

    /// <summary>
    /// Gets or sets the layout classes for a vertical separator with a label: the column holding the lines and
    /// the label.
    /// </summary>
    public required string LabeledVertical { get; set; }

    /// <summary>
    /// Gets or sets the border classes that draw a horizontal line, on the separator itself or on each line
    /// beside a label.
    /// </summary>
    public required string HorizontalLine { get; set; }

    /// <summary>
    /// Gets or sets the border classes that draw a vertical line, on the separator itself or on each line
    /// beside a label.
    /// </summary>
    public required string VerticalLine { get; set; }

    /// <summary>
    /// Gets or sets the classes applied to each line beside a label, such as taking the separator's border color.
    /// </summary>
    public required string LabelLine { get; set; }

    /// <summary>
    /// Gets or sets the classes for a line beside a label that fills the space left over.
    /// </summary>
    public required string LineStretch { get; set; }

    /// <summary>
    /// Gets or sets the classes for the short line before a label aligned to the start, or after one aligned to
    /// the end.
    /// </summary>
    public required string LineInset { get; set; }

    /// <summary>
    /// Gets or sets the classes for the label.
    /// </summary>
    public required string Label { get; set; }

    /// <summary>
    /// Gets or sets the classes for a <see cref="Enums.SeparatorThickness.Thin"/> line.
    /// </summary>
    public required string Thin { get; set; }

    /// <summary>
    /// Gets or sets the classes for a <see cref="Enums.SeparatorThickness.Medium"/> line.
    /// </summary>
    public required string Medium { get; set; }

    /// <summary>
    /// Gets or sets the classes for a <see cref="Enums.SeparatorThickness.Thick"/> line.
    /// </summary>
    public required string Thick { get; set; }

    /// <summary>
    /// Gets or sets the classes for a <see cref="Enums.SeparatorVariant.Solid"/> line.
    /// </summary>
    public required string Solid { get; set; }

    /// <summary>
    /// Gets or sets the classes for a <see cref="Enums.SeparatorVariant.Dashed"/> line.
    /// </summary>
    public required string Dashed { get; set; }

    /// <summary>
    /// Gets or sets the classes for a <see cref="Enums.SeparatorVariant.Dotted"/> line.
    /// </summary>
    public required string Dotted { get; set; }

    /// <summary>
    /// Gets or sets the border and text classes of a separator with no color.
    /// </summary>
    public required string Neutral { get; set; }

    /// <summary>
    /// Gets or sets the border and text classes of a separator for each <see cref="Enums.Color"/>.
    /// </summary>
    public required TwBlazorPalette Colors { get; set; }
}
