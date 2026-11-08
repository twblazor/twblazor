// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace TwBlazor.Configuration.Components;

/// <summary>
/// Theme configuration for the rich text editor (<see cref="TwBlazor.Components.TwTextEditor"/>).
/// Override any property to customize text editor styles globally.
/// </summary>
/// <remarks>
/// The editor's border, background, focus ring and rounding come from <see cref="TwInputTheme"/>, the same as
/// <see cref="TwBlazor.Components.TwTextfield{T}"/>. The toolbar is made of other components with their own themes:
/// <see cref="TwToggleTheme"/> for the toggles, toggle groups and toolbar buttons,
/// <see cref="TwInputTheme"/> for the text style list, the link field and the HTML source view, and
/// <see cref="TwSeparatorTheme"/> for the lines between groups. This theme only holds the layout of the editor and
/// the look of the text inside it.
/// </remarks>
[ExcludeFromCodeCoverage]
public class TwTextEditorTheme
{
    /// <summary>
    /// Gets or sets the layout classes for the box around the toolbar and the text, on top of the input variant's
    /// border and background.
    /// </summary>
    public required string Root { get; set; }

    /// <summary>
    /// Gets or sets the classes added to <see cref="Root"/> while the editor is disabled.
    /// </summary>
    public required string Disabled { get; set; }

    /// <summary>
    /// Gets or sets the classes for the toolbar: a wrapping row of controls above the text.
    /// </summary>
    public required string Toolbar { get; set; }

    /// <summary>
    /// Gets or sets the classes added to each <see cref="TwBlazor.Components.TwToggleGroup{T}"/> in the toolbar. The
    /// groups are outlined toggle groups, so a pressed button gets a flat tint; these classes take away the group's own
    /// border and padding, so the buttons sit directly on the toolbar. They need the important modifier (<c>!</c>),
    /// since they replace classes the toggle group sets itself.
    /// </summary>
    public string ToolbarGroup { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the classes for the line drawn between groups of toolbar controls, added to the
    /// <see cref="TwBlazor.Components.TwSeparator"/>.
    /// </summary>
    public string ToolbarSeparator { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the classes for the text style list (paragraph and headings) in the toolbar, added to the
    /// root of its <see cref="TwBlazor.Components.TwSelect{T}"/>, such as its width.
    /// </summary>
    public string BlockSelect { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the classes for the editable text area: its padding, scrolling and the placeholder shown while it
    /// is empty (the area has a <c>data-empty</c> attribute then, and the placeholder text in
    /// <c>data-placeholder</c>).
    /// </summary>
    public required string Content { get; set; }

    /// <summary>
    /// Gets or sets the classes added to <see cref="Content"/> for the <see cref="Enums.InputVariant.Default"/>
    /// variant, which has no side border, so the text lines up with the label.
    /// </summary>
    public string ContentDefaultVariant { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the classes that style the formatted text inside the editor: headings, lists, quotes, code blocks
    /// and links. They target the elements inside the area, such as <c>[&amp;_h1]:text-2xl</c>, since the browser's
    /// own styles for them are reset by Tailwind.
    /// </summary>
    /// <remarks>Links and other text need the 4.5:1 contrast ratio against the editor's background.</remarks>
    public required string Typography { get; set; }

    /// <summary>
    /// Gets or sets the classes for the bar that opens under the toolbar to add or change a link.
    /// </summary>
    public required string LinkPanel { get; set; }

    /// <summary>
    /// Gets or sets the classes added to the root of the link address field, such as how much of the bar it fills.
    /// </summary>
    public string LinkInput { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the classes added to the link address field's input, such as start padding that leaves room for
    /// the link icon drawn over its start, so the text does not run underneath it.
    /// </summary>
    public string LinkInputField { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the classes for the area that holds the HTML source view.
    /// </summary>
    public required string Source { get; set; }

    /// <summary>
    /// Gets or sets the classes added to the HTML source view's text area, such as a monospaced font.
    /// </summary>
    public string SourceInput { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the classes for the helper text shown under the editor
    /// (<see cref="TwBlazor.Components.TwTextEditor.HelperText"/>).
    /// </summary>
    /// <remarks>
    /// The text is read out with the editor, and needs the 4.5:1 text contrast ratio against the page.
    /// </remarks>
    public required string HelperText { get; set; }
}
