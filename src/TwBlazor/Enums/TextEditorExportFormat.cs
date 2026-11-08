// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace TwBlazor.Enums;

/// <summary>
/// Defines a file format a <see cref="TwBlazor.Components.TwTextEditor"/> can export its content as.
/// </summary>
public enum TextEditorExportFormat
{
    /// <summary>
    /// Markdown (<c>.md</c>), in the CommonMark syntax with GitHub's strikethrough. Underlined text, which Markdown
    /// has no syntax for, is kept as <c>&lt;u&gt;</c> tags.
    /// </summary>
    Markdown,

    /// <summary>
    /// A Word document (<c>.docx</c>), in the Office Open XML format read by Microsoft Word, Google Docs, Pages and
    /// LibreOffice.
    /// </summary>
    Docx,

    /// <summary>
    /// An OpenDocument text document (<c>.odt</c>), the ISO standard format of LibreOffice and other office suites.
    /// </summary>
    Odt,

    /// <summary>
    /// A web page (<c>.html</c>) that opens on its own in any browser, with simple readable styles.
    /// </summary>
    Html,

    /// <summary>
    /// Plain text (<c>.txt</c>), with list markers and quote markers written as text.
    /// </summary>
    PlainText,

    /// <summary>
    /// Rich Text Format (<c>.rtf</c>), read by almost every word processor, including WordPad and TextEdit.
    /// </summary>
    Rtf
}
