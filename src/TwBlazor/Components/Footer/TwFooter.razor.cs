// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Localization;
using TwBlazor.Models;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// A page footer: columns of titled links, an optional brand area above them, free-form content, and a bottom
/// row for a copyright line and a few legal links. Every part is optional, so it works as a one-line footer
/// or a full site map.
/// </summary>
/// <remarks>
/// <para>
/// Pass <see cref="Sections"/> for the common case and the component writes the markup. For anything else,
/// use <see cref="ChildContent"/>, which is rendered between the sections and the bottom row.
/// </para>
/// <para>
/// Accessibility: the root is a <c>footer</c> element, which is the page's content information landmark when it
/// is not inside an <c>article</c>, <c>section</c> or <c>main</c>. The sections sit in a <c>nav</c> landmark named
/// by <see cref="NavigationLabel"/>. Each section is a heading followed by a list that the heading names, so a
/// screen reader announces the group and how many links it holds. Links are ordinary tab stops in reading
/// order with a visible focus ring, and a link that opens in a new tab says so.
/// </para>
/// </remarks>
public partial class TwFooter : TwBlazorComponentBase
{
    private TwFooterTheme theme => options.Theme.Components.Require<TwFooterTheme>();

    /// <summary>
    /// Gets or sets the titled groups of links, each drawn as a column.
    /// </summary>
    /// <remarks>
    /// Sections without a title or without any links are left out.
    /// </remarks>
    [Parameter] public IReadOnlyList<FooterSection> Sections { get; set; } = [];

    /// <summary>
    /// Gets or sets the links along the bottom row, typically legal pages such as privacy and terms.
    /// </summary>
    [Parameter] public IReadOnlyList<FooterLink> BottomLinks { get; set; } = [];

    /// <summary>
    /// Gets or sets the copyright line shown in the bottom row.
    /// </summary>
    [Parameter] public string? Copyright { get; set; }

    /// <summary>
    /// Gets or sets content shown above the sections, such as a logo and a short description.
    /// </summary>
    [Parameter] public RenderFragment? BrandContent { get; set; }

    /// <summary>
    /// Gets or sets free-form content, rendered after the sections and before the bottom row.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets extra content for the bottom row, after the copyright line and the bottom links, such as
    /// social media links or a language picker.
    /// </summary>
    [Parameter] public RenderFragment? BottomContent { get; set; }

    /// <summary>
    /// Gets or sets the heading level of the section titles, from 1 to 6.
    /// </summary>
    /// <remarks>
    /// Default is 2. Choose the level that follows on from the headings above the footer on your page.
    /// </remarks>
    [Parameter] public int HeadingLevel { get; set; } = 2;

    /// <summary>
    /// Gets or sets the accessible name of the navigation landmark that holds the sections.
    /// Defaults to the localized <see cref="TwStrings.Footer_Label"/> text ("Footer links").
    /// </summary>
    /// <remarks>
    /// A page usually has other navigation landmarks, so this name is what tells them apart.
    /// </remarks>
    [Parameter] public string? NavigationLabel { get; set; }

    private string effectiveNavigationLabel => NavigationLabel ?? localizer[TwStrings.Footer_Label];

    private string footerClasses => new ClassBuilder(theme.Footer).AddClass(Class).Build();

    private List<FooterSection> visibleSections =>
        [.. Sections.Where(section => !string.IsNullOrWhiteSpace(section.Title) && GetVisibleLinks(section.Links).Count > 0)];

    private List<FooterLink> visibleBottomLinks => GetVisibleLinks(BottomLinks);

    private bool hasBottom => !string.IsNullOrWhiteSpace(Copyright) || visibleBottomLinks.Count > 0 || BottomContent is not null;

    private static List<FooterLink> GetVisibleLinks(IReadOnlyList<FooterLink> links) =>
        [.. links.Where(link => !string.IsNullOrWhiteSpace(link.Label) && !string.IsNullOrWhiteSpace(link.Href))];

    private RenderFragment RenderHeading(string title, string id) => builder =>
    {
        builder.OpenElement(0, $"h{Math.Clamp(HeadingLevel, 1, 6)}");
        builder.AddAttribute(1, "id", id);
        builder.AddAttribute(2, "class", theme.SectionTitle);
        builder.AddContent(3, title);
        builder.CloseElement();
    };

    private RenderFragment RenderLink(FooterLink link) => builder =>
    {
        builder.OpenElement(0, "a");
        builder.AddAttribute(1, "href", link.Href);
        builder.AddAttribute(2, "class", theme.Link);

        if (link.OpensInNewTab)
        {
            builder.AddAttribute(3, "target", "_blank");
            builder.AddAttribute(4, "rel", "noopener noreferrer");
        }

        if (link.Icon is { } icon)
        {
            builder.OpenComponent<TwIcon>(5);
            builder.AddComponentParameter(6, nameof(TwIcon.Icon), icon);
            builder.AddComponentParameter(7, nameof(TwIcon.Plain), true);
            builder.CloseComponent();
        }

        builder.AddContent(8, link.Label);

        if (link.OpensInNewTab)
        {
            builder.OpenElement(9, "span");
            builder.AddAttribute(10, "class", options.Theme.Display.ScreenReaderOnly);
            builder.AddContent(11, $" ({localizer[TwStrings.Footer_OpensInNewTab]})");
            builder.CloseElement();
        }

        builder.CloseElement();
    };
}