// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace TwBlazor.Enums;

/// <summary>
/// Defines a format a <see cref="TwBlazor.Components.TwTextEditor"/> can apply to its text.
/// </summary>
/// <remarks>
/// <see cref="Bold"/>, <see cref="Italic"/>, <see cref="Underline"/> and <see cref="Strikethrough"/> apply to the
/// selected text. The others apply to the whole paragraphs the selection touches, and a paragraph has one of
/// <see cref="Paragraph"/>, <see cref="Heading1"/>, <see cref="Heading2"/>, <see cref="Heading3"/>,
/// <see cref="Blockquote"/> and <see cref="CodeBlock"/> at a time.
/// </remarks>
public enum TextEditorFormat
{
    /// <summary>
    /// Bold text (<c>&lt;strong&gt;</c>).
    /// </summary>
    Bold,

    /// <summary>
    /// Italic text (<c>&lt;em&gt;</c>).
    /// </summary>
    Italic,

    /// <summary>
    /// Underlined text (<c>&lt;u&gt;</c>).
    /// </summary>
    Underline,

    /// <summary>
    /// Struck-through text (<c>&lt;s&gt;</c>).
    /// </summary>
    Strikethrough,

    /// <summary>
    /// A plain paragraph (<c>&lt;p&gt;</c>).
    /// </summary>
    Paragraph,

    /// <summary>
    /// A top level heading (<c>&lt;h1&gt;</c>).
    /// </summary>
    Heading1,

    /// <summary>
    /// A second level heading (<c>&lt;h2&gt;</c>).
    /// </summary>
    Heading2,

    /// <summary>
    /// A third level heading (<c>&lt;h3&gt;</c>).
    /// </summary>
    Heading3,

    /// <summary>
    /// A quote (<c>&lt;blockquote&gt;</c>). Applying it to a quote turns it back into a paragraph.
    /// </summary>
    Blockquote,

    /// <summary>
    /// A block of code (<c>&lt;pre&gt;</c>). Applying it to a code block turns it back into a paragraph.
    /// </summary>
    CodeBlock,

    /// <summary>
    /// A bulleted list (<c>&lt;ul&gt;</c>). Applying it to a bulleted list turns the items back into paragraphs.
    /// </summary>
    BulletList,

    /// <summary>
    /// A numbered list (<c>&lt;ol&gt;</c>). Applying it to a numbered list turns the items back into paragraphs.
    /// </summary>
    NumberedList,

    /// <summary>
    /// A link (<c>&lt;a&gt;</c>). Reported while the caret is in a link; use
    /// <see cref="TwBlazor.Components.TwTextEditor.InsertLinkAsync(string)"/> to add one.
    /// </summary>
    Link
}
