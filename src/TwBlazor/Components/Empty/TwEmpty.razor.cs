// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Builders;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Localization;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// Represents an empty state: a placeholder shown where there is no data or content yet, with an icon, a title,
/// a description and optional actions that give the user a next step.
/// </summary>
/// <remarks>
/// <para>
/// The media is the first of these that is set: <see cref="MediaContent"/>, an avatar (<see cref="AvatarSrc"/> or
/// <see cref="AvatarName"/>), then the <see cref="Icon"/>. Set <see cref="Icon"/> to <see langword="null"/> for an
/// empty state with no media.
/// </para>
/// <para>
/// Accessibility: the icon is decorative and hidden from assistive technology. The title is a heading whose level is
/// set with <see cref="TitleLevel"/>, so it fits the page's heading outline. Set <see cref="Live"/> when the empty
/// state replaces results in response to the user, such as a search with no matches, so screen readers announce it.
/// </para>
/// </remarks>
public partial class TwEmpty : TwBlazorComponentBase
{
    private TwEmptyTheme theme => options.Theme.Components.Require<TwEmptyTheme>();

    /// <summary>
    /// Gets or sets the icon shown above the title. Defaults to <see cref="Icon.Inbox"/>.
    /// </summary>
    /// <remarks>
    /// Set it to <see langword="null"/> to show no icon. Ignored when <see cref="MediaContent"/> or an avatar is set.
    /// </remarks>
    [Parameter] public Icon? Icon { get; set; } = Enums.Icon.Inbox;

    /// <summary>
    /// Gets or sets how the icon is shown: on a tinted tile or on its own. Defaults to
    /// <see cref="EmptyMediaVariant.Tile"/>.
    /// </summary>
    [Parameter] public EmptyMediaVariant MediaVariant { get; set; } = EmptyMediaVariant.Tile;

    /// <summary>
    /// Gets or sets the color of the icon and its tile.
    /// </summary>
    /// <remarks>
    /// If not set, a neutral gray is used.
    /// </remarks>
    [Parameter] public Color? Color { get; set; }

    /// <summary>
    /// Gets or sets the URL of a picture shown in an avatar instead of the icon, for example when a person has no
    /// activity yet.
    /// </summary>
    [Parameter] public string? AvatarSrc { get; set; }

    /// <summary>
    /// Gets or sets the name of the person or entity shown in an avatar instead of the icon. It supplies the avatar's
    /// initials when there is no <see cref="AvatarSrc"/>.
    /// </summary>
    [Parameter] public string? AvatarName { get; set; }

    /// <summary>
    /// Gets or sets custom media shown above the title instead of the icon or avatar, such as an illustration or a
    /// group of avatars.
    /// </summary>
    [Parameter] public RenderFragment? MediaContent { get; set; }

    /// <summary>
    /// Gets or sets the title.
    /// </summary>
    /// <remarks>
    /// If not set, the localized <see cref="TwStrings.Empty_Title"/> text ("No data") is shown. Set it to an empty
    /// string to show no title.
    /// </remarks>
    [Parameter] public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the heading level (1 to 6) rendered for <see cref="Title"/>, so it fits the surrounding page's
    /// heading outline. Defaults to 3.
    /// </summary>
    [Parameter] public int TitleLevel { get; set; } = 3;

    /// <summary>
    /// Gets or sets the description shown below the title, such as why there is nothing here and what to do next.
    /// </summary>
    [Parameter] public string? Description { get; set; }

    /// <summary>
    /// Gets or sets extra content shown below the description, such as a search field or a help link.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets custom actions, such as buttons, shown in the row of actions after any built-in ones.
    /// </summary>
    [Parameter] public RenderFragment? ActionContent { get; set; }

    /// <summary>
    /// Gets or sets the label of the built-in primary action button. The button is only shown when this is set.
    /// </summary>
    [Parameter] public string? ActionText { get; set; }

    /// <summary>
    /// Gets or sets the icon shown at the start of the primary action button.
    /// </summary>
    [Parameter] public Icon? ActionIcon { get; set; }

    /// <summary>
    /// Gets or sets the URL the primary action button links to. When set, the button renders as a link.
    /// </summary>
    [Parameter] public string? ActionHref { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the primary action button is clicked.
    /// </summary>
    [Parameter] public EventCallback OnAction { get; set; }

    /// <summary>
    /// Gets or sets the label of the built-in secondary action button, shown with an outline after the primary
    /// action. The button is only shown when this is set.
    /// </summary>
    [Parameter] public string? SecondaryActionText { get; set; }

    /// <summary>
    /// Gets or sets the URL the secondary action button links to. When set, the button renders as a link.
    /// </summary>
    [Parameter] public string? SecondaryActionHref { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the secondary action button is clicked.
    /// </summary>
    [Parameter] public EventCallback OnSecondaryAction { get; set; }

    /// <summary>
    /// Gets or sets the size: its padding, the size of the media and the size of the text. Defaults to
    /// <see cref="Size.Md"/>.
    /// </summary>
    [Parameter] public Size Size { get; set; } = Size.Md;

    /// <summary>
    /// Gets or sets whether the empty state has a dashed border, marking out the area that has no content.
    /// </summary>
    [Parameter] public bool Outlined { get; set; }

    /// <summary>
    /// Gets or sets whether the empty state has a subtle background.
    /// </summary>
    [Parameter] public bool Filled { get; set; }

    /// <summary>
    /// Gets or sets whether the empty state is a live region (<c>role="status"</c>), so screen readers announce it
    /// when it appears in response to the user, such as a search or filter that returns no results.
    /// </summary>
    [Parameter] public bool Live { get; set; }

    private string? effectiveTitle => Title ?? localizer[TwStrings.Empty_Title];

    private bool hasTitle => !string.IsNullOrWhiteSpace(effectiveTitle);

    private bool hasHeader => hasTitle || !string.IsNullOrWhiteSpace(Description);

    private bool showAvatar => !string.IsNullOrWhiteSpace(AvatarSrc) || !string.IsNullOrWhiteSpace(AvatarName);

    private bool showPrimaryAction => !string.IsNullOrWhiteSpace(ActionText);

    private bool showSecondaryAction => !string.IsNullOrWhiteSpace(SecondaryActionText);

    private bool hasActions => showPrimaryAction || showSecondaryAction || ActionContent is not null;

    private bool hasAccessibleName => !string.IsNullOrWhiteSpace(AriaLabel) || !string.IsNullOrWhiteSpace(AriaLabelledBy);

    // A live empty state is a status; a named one is a region so its name is announced. Otherwise the root is a
    // plain container, as an aria-label is not allowed on an element with no role.
    private string? effectiveRole => Live ? "status" : hasAccessibleName ? "region" : null;

    private string titleId => $"{Id}-title";

    private string descriptionId => $"{Id}-description";

    private AvatarSize avatarSize => Size switch
    {
        Size.Sm => AvatarSize.Medium,
        Size.Lg => AvatarSize.ExtraLarge,
        _ => AvatarSize.Large
    };

    private bool hasSurface => Outlined || Filled;

    private string rootClasses => new ClassBuilder(theme.Base)
        .AddClass(Size switch
        {
            Size.Sm => theme.Small,
            Size.Lg => theme.Large,
            _ => theme.Medium
        })
        .AddClass(theme.Outlined, Outlined)
        .AddClass(theme.Filled, Filled)
        .AddClass(roundedBuilder.GetRounded(effectiveSurfaceRounded), hasSurface)
        .AddClass(shadowBuilder.GetShadow(Shadow))
        .AddClass(Class)
        .Build();

    private string tileClasses => new ClassBuilder(theme.Tile)
        .AddClass(Size switch
        {
            Size.Sm => theme.TileSmall,
            Size.Lg => theme.TileLarge,
            _ => theme.TileMedium
        })
        .AddClass(ColorBuilder.GetPaletteColor(Color, theme.TileColors, theme.TileNeutral))
        .AddClass(roundedBuilder.GetRounded(effectiveSurfaceRounded))
        .Build();

    private string plainIconClasses => new ClassBuilder(Size switch
        {
            Size.Sm => theme.PlainIconSmall,
            Size.Lg => theme.PlainIconLarge,
            _ => theme.PlainIconMedium
        })
        .AddClass(theme.PlainIcon, Color is null)
        .Build();

    private string titleClasses => new ClassBuilder(theme.Title)
        .AddClass(Size switch
        {
            Size.Sm => theme.TitleSmall,
            Size.Lg => theme.TitleLarge,
            _ => theme.TitleMedium
        })
        .Build();

    private string descriptionClasses => new ClassBuilder(theme.Description)
        .AddClass(Size switch
        {
            Size.Sm => theme.DescriptionSmall,
            Size.Lg => theme.DescriptionLarge,
            _ => theme.DescriptionMedium
        })
        .Build();

    /// <summary>
    /// Renders the title as an &lt;h1&gt;-&lt;h6&gt; element per <see cref="TitleLevel"/>. A dynamic tag name isn't
    /// expressible directly in .razor markup, so the render tree is built by hand.
    /// </summary>
    private RenderFragment renderTitle => builder =>
    {
        builder.OpenElement(0, $"h{Math.Clamp(TitleLevel, 1, 6)}");
        builder.AddAttribute(1, "id", titleId);
        builder.AddAttribute(2, "class", titleClasses);
        builder.AddContent(3, effectiveTitle);
        builder.CloseElement();
    };
}
