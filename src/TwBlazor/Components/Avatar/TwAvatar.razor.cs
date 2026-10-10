// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TwBlazor.Builders;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// Represents a person or an entity with a picture, their initials or an icon.
/// </summary>
/// <remarks>
/// <para>
/// The avatar shows the first of these that is available: the image at <see cref="Src"/>, the
/// <see cref="Initials"/> (worked out from <see cref="Name"/> when not given), then the <see cref="Icon"/>. If the
/// image fails to load, the avatar falls back to the initials or the icon.
/// </para>
/// <para>
/// Accessibility: with a <see cref="Name"/> (or an <c>AriaLabel</c>) the avatar is exposed as an image with that
/// name. Without one it is treated as decoration and hidden from assistive technology, which is right when the
/// name is already written beside it.
/// </para>
/// </remarks>
public partial class TwAvatar : TwBlazorComponentBase
{
    private TwAvatarTheme theme => options.Theme.Components.Require<TwAvatarTheme>();

    [Inject] private IJSRuntime jsRuntime { get; set; } = null!;

    private ElementReference imageRef;
    private string? failedSrc;
    private string? checkedSrc;

    /// <summary>
    /// Gets or sets the URL of the picture.
    /// </summary>
    [Parameter] public string? Src { get; set; }

    /// <summary>
    /// Gets or sets the name of the person or entity. It names the avatar for assistive technology and supplies
    /// the initials when <see cref="Initials"/> is not set.
    /// </summary>
    [Parameter] public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the initials shown when there is no picture.
    /// </summary>
    /// <remarks>
    /// If not set, they are the first letters of the first and last words of <see cref="Name"/>.
    /// </remarks>
    [Parameter] public string? Initials { get; set; }

    /// <summary>
    /// Gets or sets the icon shown when there is no picture and no initials.
    /// </summary>
    [Parameter] public Icon Icon { get; set; } = Icon.Person_Fill;

    /// <summary>
    /// Gets or sets the color behind the initials or icon.
    /// </summary>
    /// <remarks>
    /// If not set, a neutral gray is used.
    /// </remarks>
    [Parameter] public Color? Color { get; set; }

    /// <summary>
    /// Gets or sets the size of the avatar.
    /// </summary>
    [Parameter] public AvatarSize Size { get; set; } = AvatarSize.Medium;

    private bool showImage => !string.IsNullOrWhiteSpace(Src) && Src != failedSrc;

    private string effectiveInitials => string.IsNullOrWhiteSpace(Initials) ? GetInitials(Name) : Initials.Trim();

    private string? accessibleName => AriaLabel ?? (AriaLabelledBy == null && !string.IsNullOrWhiteSpace(Name) ? Name : null);

    private bool isDecorative => accessibleName == null && AriaLabelledBy == null;

    private string sizeClasses => Size switch
    {
        AvatarSize.Small => theme.Small,
        AvatarSize.Large => theme.Large,
        AvatarSize.ExtraLarge => theme.ExtraLarge,
        _ => theme.Medium
    };

    private string classes => new ClassBuilder(sizeClasses)
        .AddClass(theme.Base)
        .AddClass(ColorBuilder.GetPaletteColor(Color, theme.Colors, theme.Neutral))
        .AddClass(roundedBuilder.GetRounded(Rounded ?? Enums.Rounded.Full))
        .AddClass(Class)
        .Build();

    /// <summary>
    /// Works out up to two initials from a name: the first letters of its first and last words.
    /// </summary>
    /// <param name="name">The name, for example <c>"Jane Doe"</c>.</param>
    /// <returns>The initials in upper case, or an empty string when the name has no words.</returns>
    internal static string GetInitials(string? name)
    {
        var words = (name ?? string.Empty).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0)
        {
            return string.Empty;
        }

        string[] chosen = words.Length == 1 ? [words[0]] : [words[0], words[^1]];
        return string.Concat(chosen.Select(word => StringInfo.GetNextTextElement(word).ToUpper(CultureInfo.CurrentCulture)));
    }

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!showImage || checkedSrc == Src)
        {
            return;
        }

        checkedSrc = Src;

        try
        {
            // A picture can fail before the component becomes interactive (a prerendered page), and that error
            // event is never replayed, so ask once whether it has already failed.
            if (await jsRuntime.InvokeAsync<bool>("twAvatar.isBroken", imageRef))
            {
                OnImageError();
                StateHasChanged();
            }
        }
        catch (JSDisconnectedException)
        {
            // The circuit disconnected before the script could run; nothing to check.
        }
        catch (InvalidOperationException)
        {
            // JS interop is unavailable while prerendering; the check runs again once interactive.
            checkedSrc = null;
        }
    }

    // Remembers which picture failed, so a new Src gets its own chance to load.
    private void OnImageError() => failedSrc = Src;
}
