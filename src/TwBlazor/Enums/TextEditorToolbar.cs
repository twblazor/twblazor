// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace TwBlazor.Enums;

/// <summary>
/// Defines which controls the toolbar of a <see cref="TwBlazor.Components.TwTextEditor"/> shows. Each preset holds
/// every control of the one before it.
/// </summary>
public enum TextEditorToolbar
{
    /// <summary>
    /// No toolbar. The keyboard shortcuts, such as <kbd>Ctrl</kbd> + <kbd>B</kbd> for bold, still work.
    /// </summary>
    None,

    /// <summary>
    /// Bold, italic and underline, and bulleted and numbered lists.
    /// </summary>
    Simple,

    /// <summary>
    /// The <see cref="Simple"/> controls plus a text style list (paragraph and headings), strikethrough and links.
    /// The default.
    /// </summary>
    Standard,

    /// <summary>
    /// The <see cref="Standard"/> controls plus quotes, code blocks, clear formatting, undo and redo, and an HTML
    /// source view.
    /// </summary>
    Full,

    /// <summary>
    /// Only the controls in <see cref="TwBlazor.Components.TwTextEditor.ToolbarContent"/>.
    /// </summary>
    Custom
}
