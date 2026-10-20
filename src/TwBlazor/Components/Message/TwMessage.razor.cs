// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using System.Globalization;
using TwBlazor.Builders;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// Represents one message in a conversation: an avatar beside a column holding the author, the time, the content
/// and an optional footer.
/// </summary>
/// <remarks>
/// <para>
/// Put the message's text in a <see cref="TwBubble"/> as its content. The bubble picks up the message's
/// <see cref="Align"/>, so the current user's messages can sit at the end edge and everyone else's at the start.
/// </para>
/// <para>
/// Accessibility: each message is an <c>article</c> named by its author and time, so screen reader users can move
/// from message to message. The avatar is decoration, because the author's name is already written beside it. A
/// <see cref="Continued"/> message hides its header from view but keeps it for screen readers, so every message
/// still says who sent it. Put the messages in a container with <c>role="log"</c> so new ones are announced.
/// </para>
/// </remarks>
public partial class TwMessage : TwBlazorComponentBase
{
    /// <summary>
    /// The name of the cascading value that passes <see cref="Align"/> down to the <see cref="TwBubble"/>s inside.
    /// </summary>
    internal const string alignCascadeName = "TwMessageAlign";

    private TwMessageTheme theme => options.Theme.Components.Require<TwMessageTheme>();

    /// <summary>
    /// Gets or sets the name of the person or assistant who sent the message. It is shown above the content and
    /// supplies the avatar's initials.
    /// </summary>
    [Parameter] public string? Author { get; set; }

    /// <summary>
    /// Gets or sets when the message was sent. It is shown beside the author.
    /// </summary>
    [Parameter] public DateTime? Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the .NET format string the <see cref="Timestamp"/> is shown with.
    /// </summary>
    /// <remarks>
    /// If not set, the current culture's short time is used, for example <c>14:05</c> in en-GB.
    /// </remarks>
    [Parameter] public string? TimestampFormat { get; set; }

    /// <summary>
    /// Gets or sets which side of the conversation the message sits on.
    /// </summary>
    [Parameter] public MessageAlign Align { get; set; } = MessageAlign.Start;

    /// <summary>
    /// Gets or sets whether the avatar column is shown.
    /// </summary>
    [Parameter] public bool ShowAvatar { get; set; } = true;

    /// <summary>
    /// Gets or sets the URL of the avatar's picture.
    /// </summary>
    [Parameter] public string? AvatarSrc { get; set; }

    /// <summary>
    /// Gets or sets the avatar's initials.
    /// </summary>
    /// <remarks>
    /// If not set, they are worked out from <see cref="Author"/>, unless <see cref="AvatarIcon"/> is set.
    /// </remarks>
    [Parameter] public string? AvatarInitials { get; set; }

    /// <summary>
    /// Gets or sets an icon the avatar shows in place of initials worked out from the <see cref="Author"/>, for
    /// example <see cref="Icon.Robot"/> for an assistant.
    /// </summary>
    /// <remarks>
    /// A picture from <see cref="AvatarSrc"/> or explicit <see cref="AvatarInitials"/> still take precedence.
    /// </remarks>
    [Parameter] public Icon? AvatarIcon { get; set; }

    /// <summary>
    /// Gets or sets the color behind the avatar's initials or icon.
    /// </summary>
    [Parameter] public Color? AvatarColor { get; set; }

    /// <summary>
    /// Gets or sets custom content shown in place of the built-in <see cref="TwAvatar"/>.
    /// </summary>
    [Parameter] public RenderFragment? AvatarContent { get; set; }

    /// <summary>
    /// Gets or sets whether the message continues one from the same author just above it. Its avatar and header
    /// are hidden from view, and the content stays in line with the message above.
    /// </summary>
    [Parameter] public bool Continued { get; set; }

    /// <summary>
    /// Gets or sets the content of the message, usually a <see cref="TwBubble"/>.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets content shown under the message, such as actions or a delivery state.
    /// </summary>
    [Parameter] public RenderFragment? Footer { get; set; }

    private bool hasAuthor => !string.IsNullOrWhiteSpace(Author);

    private bool hasHeader => hasAuthor || Timestamp.HasValue;

    private string authorId => $"{Id}-author";

    private string timeId => $"{Id}-time";

    private string? avatarInitials => (string.IsNullOrWhiteSpace(AvatarInitials), AvatarIcon.HasValue) switch
    {
        (false, _) => AvatarInitials,
        (true, true) => null,
        _ => TwAvatar.GetInitials(Author)
    };

    private Icon avatarIcon => AvatarIcon ?? Icon.Person_Fill;

    private string? effectiveLabelledBy
    {
        get
        {
            if (AriaLabelledBy != null || AriaLabel != null || !hasHeader)
            {
                return AriaLabelledBy;
            }

            return string.Join(' ', new[] { hasAuthor ? authorId : null, Timestamp.HasValue ? timeId : null }.OfType<string>());
        }
    }

    private string displayTimestamp => Timestamp.HasValue
        ? localizer.Formats.Format(Timestamp.Value, TimestampFormat ?? localizer.Formats.ShortTime)
        : string.Empty;

    private string machineTimestamp => Timestamp.HasValue ? FormatMachineTimestamp(Timestamp.Value) : string.Empty;

    private string classes => new ClassBuilder(theme.Base)
        .AddClass(options.Theme.Flexbox.RowReverse, Align == MessageAlign.End)
        .AddClass(Class)
        .Build();

    private string contentClasses => new ClassBuilder(theme.Content)
        .AddClass(Align == MessageAlign.End ? options.Theme.Flexbox.Align.End : options.Theme.Flexbox.Align.Start)
        .Build();

    private string headerClasses => Continued ? options.Theme.Display.ScreenReaderOnly : theme.Header;

    /// <summary>
    /// Formats a time for the <c>datetime</c> attribute of a <c>time</c> element, which machines read.
    /// </summary>
    /// <param name="value">The time the message was sent.</param>
    /// <returns>The time in ISO 8601 form, ending in <c>Z</c> when <paramref name="value"/> is in UTC.</returns>
    internal static string FormatMachineTimestamp(DateTime value) =>
        value.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture) + (value.Kind == DateTimeKind.Utc ? "Z" : string.Empty);
}
