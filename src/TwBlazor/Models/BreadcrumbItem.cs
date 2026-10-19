// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using TwBlazor.Enums;

namespace TwBlazor.Models;

[ExcludeFromCodeCoverage]
public class BreadcrumbItem
{
    /// <summary>
    /// Gets or sets the icon associated with the object. This icon can be used to visually represent the object in a
    /// user interface.
    /// </summary>
    /// <remarks>If the icon is not set, the property will return null. Ensure that the icon is properly
    /// initialized before use to avoid null reference exceptions.</remarks>
    public Icon? Icon { get; set; }
    /// <summary>
    /// Gets or sets the label text display associated with the entity.
    /// </summary>
    /// <remarks>The name is a required field and cannot be null or empty. It is used to identify the entity
    /// in various contexts.</remarks>
    public required string Label { get; set; }
    /// <summary>
    /// Gets or sets the URI that identifies the resource.
    /// </summary>
    /// <remarks>The Href property must be a valid URI format. It is required for the proper identification of
    /// the resource it represents.</remarks>
    public string? Href { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the element is the current item within a set of items.
    /// </summary>
    /// <remarks>This property is typically used in accessibility scenarios to indicate the current state of
    /// an element in relation to other elements. It is important for assistive technologies to convey the current
    /// status to users.</remarks>
    public bool AriaCurrent { get; set; }
}
