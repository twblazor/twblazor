// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace TwBlazor.Models;

/// <summary>
/// One entry in a <see cref="TwBlazor.Components.TwOnThisPage"/>: a link to a section of the page.
/// </summary>
/// <param name="Id">The DOM id of the section the entry links to.</param>
/// <param name="Title">The text of the link, usually the section's heading.</param>
public sealed record OnThisPageItem(string Id, string Title);
