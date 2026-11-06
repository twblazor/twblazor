// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace TwBlazor.Configuration.Components;

/// <summary>
/// Theme configuration for the tag input (<see cref="TwBlazor.Components.TwTagInput"/>).
/// Override any property to customize tag input styles globally.
/// </summary>
/// <remarks>
/// The field's border, background, focus ring and rounding come from <see cref="TwInputTheme"/> through the
/// input variant, the same as <see cref="TwBlazor.Components.TwTextfield{T}"/>, and the clear button and the
/// suggestions list reuse <see cref="TwInputTheme"/>'s clear button and listbox classes. Each tag is a
/// <see cref="TwBlazor.Components.TwChip"/>, styled by <see cref="TwChipTheme"/>. This theme only holds the layout
/// that is particular to a field of tags.
/// </remarks>
[ExcludeFromCodeCoverage]
public class TwTagInputTheme
{
    /// <summary>
    /// Gets or sets the layout classes for the field: a wrapping row holding the tags and the text input, on top
    /// of the input variant's border and background.
    /// </summary>
    public required string Field { get; set; }

    /// <summary>
    /// Gets or sets the minimum height and text size of the field, so a field with no tags lines up with a
    /// <see cref="TwBlazor.Components.TwTextfield{T}"/>. It grows taller as its tags wrap onto more rows.
    /// </summary>
    public required string Size { get; set; }

    /// <summary>
    /// Gets or sets the classes applied instead of <see cref="Size"/> when the field's
    /// <see cref="TwBlazor.TwBlazorTextInputComponentBase.Dense"/> is enabled.
    /// </summary>
    public required string DenseSize { get; set; }

    /// <summary>
    /// Gets or sets the start padding applied while the field shows tags, so the first tag sits as far from the
    /// start edge as it does from the top and bottom. Not applied to the <see cref="Enums.InputVariant.Default"/>
    /// variant, which has no side padding.
    /// </summary>
    public string ChipsPadding { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the classes for the list that holds the tags. It should not create a box of its own
    /// (<c>display: contents</c>), so the tags and the text input wrap together as one row.
    /// </summary>
    public required string TagList { get; set; }

    /// <summary>
    /// Gets or sets the classes for the text input the tags are typed into. It fills the rest of the row and
    /// moves onto a new row when too little room is left.
    /// </summary>
    public required string Input { get; set; }

    /// <summary>
    /// Gets or sets the classes for the area over the end of the field that holds the clear button. It spans the
    /// field's height and centers the button vertically.
    /// </summary>
    public required string EndContent { get; set; }

    /// <summary>
    /// Gets or sets the classes added to <see cref="TwInputTheme.ClearButton"/> for the clear button, such as its
    /// height, since the field can be several rows tall.
    /// </summary>
    public string ClearButton { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the classes for the helper text shown under the field
    /// (<see cref="TwBlazor.Components.TwTagInput.HelperText"/>).
    /// </summary>
    /// <remarks>
    /// The text is read out with the field, and needs the 4.5:1 text contrast ratio against the page.
    /// </remarks>
    public required string HelperText { get; set; }
}
