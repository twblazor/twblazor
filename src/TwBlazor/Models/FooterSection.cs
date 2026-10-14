// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using TwBlazor.Enums;

namespace TwBlazor.Models;

/// <summary>
/// A link in a <see cref="TwBlazor.Components.TwFooter"/>, either in one of its sections or along its bottom row.
/// </summary>
/// <param name="Label">The text of the link.</param>
/// <param name="Href">The address the link goes to.</param>
/// <param name="Icon">An optional icon shown before the label. It is decorative: the label names the link.</param>
/// <param name="OpensInNewTab">
/// Whether the link opens in a new tab. When it does, the link says so to assistive technology and is given
/// <c>rel="noopener noreferrer"</c>.
/// </param>
public sealed record FooterLink(string Label, string Href, Icon? Icon = null, bool OpensInNewTab = false)
{
    /// <summary>
    /// Creates a link from a label and an address, so a list of links can be written as pairs.
    /// </summary>
    /// <param name="link">The label and the address.</param>
    public static implicit operator FooterLink((string Label, string Href) link) => new(link.Label, link.Href);
}

/// <summary>
/// A titled group of links in a <see cref="TwBlazor.Components.TwFooter"/>, drawn as one column.
/// </summary>
/// <param name="Title">The heading of the group.</param>
/// <param name="Links">The links in the group, in order.</param>
public sealed record FooterSection(string Title, IReadOnlyList<FooterLink> Links);