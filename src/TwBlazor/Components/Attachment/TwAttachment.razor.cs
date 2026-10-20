// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using System.Globalization;
using TwBlazor.Builders;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Localization;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// Represents a file attached to a message: its preview or type icon, its name, its size or upload progress, and
/// buttons to retry or remove it.
/// </summary>
/// <remarks>
/// <para>
/// The attachment shows <see cref="PreviewSrc"/> when it is set and loads, otherwise an icon for the file's type.
/// Under the name it shows the size and type when the file is <see cref="AttachmentStatus.Ready"/>, a progress
/// bar while it is <see cref="AttachmentStatus.Uploading"/>, and an error when the upload
/// <see cref="AttachmentStatus.Failed"/>.
/// </para>
/// <para>
/// Accessibility: the attachment is a group named after the file. Its remove and retry buttons name the file
/// they act on, and the retry button is described by the error. Starting, finishing and failing an upload are
/// announced through a polite live region. The preview is decoration, because the file name is written beside it.
/// </para>
/// </remarks>
public partial class TwAttachment : TwBlazorComponentBase
{
    private static readonly string[] _sizeUnits = ["B", "KB", "MB", "GB", "TB"];

    private TwAttachmentTheme theme => options.Theme.Components.Require<TwAttachmentTheme>();

    private string? failedPreviewSrc;
    private AttachmentStatus? previousStatus;
    private string announcement = string.Empty;

    /// <summary>
    /// Gets or sets the name of the file, for example <c>report.pdf</c>.
    /// </summary>
    [Parameter, EditorRequired] public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the size of the file in bytes. It is shown when the file is ready.
    /// </summary>
    [Parameter] public long? FileSize { get; set; }

    /// <summary>
    /// Gets or sets the file's media type, for example <c>image/png</c>. It picks the icon shown when there is no preview.
    /// </summary>
    [Parameter] public string? ContentType { get; set; }

    /// <summary>
    /// Gets or sets the URL of a thumbnail of the file, such as the image itself.
    /// </summary>
    [Parameter] public string? PreviewSrc { get; set; }

    /// <summary>
    /// Gets or sets the icon shown when there is no preview.
    /// </summary>
    /// <remarks>
    /// If not set, an icon is picked from <see cref="ContentType"/> or the extension of <see cref="FileName"/>.
    /// </remarks>
    [Parameter] public Icon? Icon { get; set; }

    /// <summary>
    /// Gets or sets the URL the file name links to, for opening or downloading the file. It is only a link once
    /// the file is ready.
    /// </summary>
    [Parameter] public string? Href { get; set; }

    /// <summary>
    /// Gets or sets where the link opens. A <c>_blank</c> target adds <c>rel="noopener"</c>.
    /// </summary>
    [Parameter] public string? Target { get; set; }

    /// <summary>
    /// Gets or sets where the file is in its upload.
    /// </summary>
    [Parameter] public AttachmentStatus Status { get; set; } = AttachmentStatus.Ready;

    /// <summary>
    /// Gets or sets how much of the upload is done, from 0 to 100.
    /// </summary>
    /// <remarks>
    /// Leave it unset while <see cref="AttachmentStatus.Uploading"/> when the progress is not known, and the bar
    /// shows that the upload is busy.
    /// </remarks>
    [Parameter] public double? Progress { get; set; }

    /// <summary>
    /// Gets or sets the error shown when the upload failed.
    /// </summary>
    /// <remarks>
    /// If not set, a localized "{file} failed to upload" message is shown.
    /// </remarks>
    [Parameter] public string? ErrorText { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked by the remove button. The button is shown only when this is set.
    /// </summary>
    [Parameter] public EventCallback OnRemove { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked by the retry button. The button is shown only when this is set and the
    /// upload failed.
    /// </summary>
    [Parameter] public EventCallback OnRetry { get; set; }

    /// <summary>
    /// Gets or sets extra actions shown before the retry and remove buttons.
    /// </summary>
    [Parameter] public RenderFragment? Actions { get; set; }

    /// <summary>
    /// Gets or sets whether the retry and remove buttons are disabled.
    /// </summary>
    [Parameter] public bool Disabled { get; set; }

    private bool showPreview => !string.IsNullOrWhiteSpace(PreviewSrc) && PreviewSrc != failedPreviewSrc;

    private bool isLink => !string.IsNullOrWhiteSpace(Href) && Status == AttachmentStatus.Ready;

    private string? effectiveRel => Target == "_blank" ? "noopener" : null;

    private bool hasActions => Actions != null || OnRemove.HasDelegate || (OnRetry.HasDelegate && Status == AttachmentStatus.Failed);

    private string errorId => $"{Id}-error";

    private Icon effectiveIcon => Icon ?? GetFileIcon(FileName, ContentType);

    private List<string> metaParts
    {
        get
        {
            List<string> parts = [];
            if (FileSize.HasValue)
            {
                parts.Add(FormatFileSize(FileSize.Value, localizer.Formats.Culture));
            }

            var extension = Path.GetExtension(FileName);
            if (extension.Length > 1)
            {
                parts.Add(extension[1..].ToUpperInvariant());
            }

            return parts;
        }
    }

    private string classes => new ClassBuilder(theme.Base)
        .AddClass(theme.Failed, Status == AttachmentStatus.Failed)
        .AddClass(roundedBuilder.GetRounded(effectiveRounded))
        .AddClass(Class)
        .Build();

    private string nameLinkClasses => new ClassBuilder(theme.Name)
        .AddClass(theme.NameLink)
        .Build();

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (previousStatus == Status)
        {
            return;
        }

        announcement = Status switch
        {
            AttachmentStatus.Uploading => localizer[TwStrings.Attachment_Uploading, FileName],
            AttachmentStatus.Failed => localizer[TwStrings.Attachment_Failed, FileName],
            _ when previousStatus == AttachmentStatus.Uploading => localizer[TwStrings.Attachment_Uploaded, FileName],
            _ => string.Empty
        };
        previousStatus = Status;
    }

    // Remembers which preview failed, so a new PreviewSrc gets its own chance to load.
    private void OnPreviewError() => failedPreviewSrc = PreviewSrc;

    /// <summary>
    /// Formats a file size in bytes with the largest unit that keeps the number at least 1, using steps of 1024.
    /// </summary>
    /// <param name="bytes">The size in bytes.</param>
    /// <param name="culture">The culture whose number format is used.</param>
    /// <returns>The size with its unit, for example <c>1.5 MB</c>.</returns>
    internal static string FormatFileSize(long bytes, CultureInfo culture)
    {
        double size = Math.Max(bytes, 0);
        var unit = 0;
        while (size >= 1024 && unit < _sizeUnits.Length - 1)
        {
            size /= 1024;
            unit++;
        }

        return $"{size.ToString(unit == 0 ? "0" : "0.#", culture)} {_sizeUnits[unit]}";
    }

    /// <summary>
    /// Picks the icon for a file from its media type, or from its extension when the media type does not say.
    /// </summary>
    /// <param name="fileName">The file name, whose extension is used when <paramref name="contentType"/> is not enough.</param>
    /// <param name="contentType">The file's media type, for example <c>image/png</c>.</param>
    /// <returns>The icon for the kind of file, or a plain file icon when the kind is not known.</returns>
    internal static Icon GetFileIcon(string? fileName, string? contentType)
    {
        var type = contentType?.ToLowerInvariant() ?? string.Empty;
        if (type.StartsWith("image/", StringComparison.Ordinal))
        {
            return Enums.Icon.File_Earmark_Image;
        }

        if (type.StartsWith("audio/", StringComparison.Ordinal))
        {
            return Enums.Icon.File_Earmark_Music;
        }

        if (type.StartsWith("video/", StringComparison.Ordinal))
        {
            return Enums.Icon.File_Earmark_Play;
        }

        if (type == "application/pdf")
        {
            return Enums.Icon.File_Earmark_Pdf;
        }

        return Path.GetExtension(fileName ?? string.Empty).ToLowerInvariant() switch
        {
            ".png" or ".jpg" or ".jpeg" or ".gif" or ".webp" or ".svg" or ".bmp" or ".avif" => Enums.Icon.File_Earmark_Image,
            ".mp3" or ".wav" or ".ogg" or ".flac" or ".m4a" => Enums.Icon.File_Earmark_Music,
            ".mp4" or ".mov" or ".webm" or ".avi" or ".mkv" => Enums.Icon.File_Earmark_Play,
            ".pdf" => Enums.Icon.File_Earmark_Pdf,
            ".doc" or ".docx" or ".odt" or ".rtf" => Enums.Icon.File_Earmark_Word,
            ".xls" or ".xlsx" or ".ods" or ".csv" => Enums.Icon.File_Earmark_Spreadsheet,
            ".ppt" or ".pptx" or ".odp" => Enums.Icon.File_Earmark_Slides,
            ".zip" or ".rar" or ".7z" or ".tar" or ".gz" => Enums.Icon.File_Earmark_Zip,
            ".txt" or ".md" or ".log" => Enums.Icon.File_Earmark_Text,
            ".cs" or ".razor" or ".js" or ".ts" or ".json" or ".html" or ".css" or ".xml" or ".py" or ".sql" => Enums.Icon.File_Earmark_Code,
            _ => Enums.Icon.File_Earmark
        };
    }
}
