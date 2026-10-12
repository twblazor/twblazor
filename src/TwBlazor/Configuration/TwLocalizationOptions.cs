// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Globalization;
using System.Resources;
using TwBlazor.Enums;
using TwBlazor.Localization;

namespace TwBlazor.Configuration;

/// <summary>
/// Configures the language and the date and time formats TwBlazor components render with. It is read once, when
/// <c>AddTwBlazor</c> is called, and is shared by the whole application.
/// </summary>
public class TwLocalizationOptions
{
    /// <summary>
    /// Gets or sets the culture every component uses for its text and its date, time and number formatting.
    /// </summary>
    /// <remarks>
    /// Leave unset to follow the application: text follows <see cref="CultureInfo.CurrentUICulture"/> and formatting
    /// follows <see cref="CultureInfo.CurrentCulture"/>, which is what Blazor's own localization sets.
    /// </remarks>
    public CultureInfo? Culture { get; set; }

    /// <summary>
    /// Gets or sets the direction every component lays itself out in.
    /// </summary>
    /// <remarks>
    /// Leave unset to follow the language: right to left for a culture such as Arabic or Hebrew, left to right
    /// otherwise. The page has to agree, so set <c>dir</c> on the <c>html</c> element to the same direction or
    /// wrap the application in a <c>TwDirectionProvider</c>.
    /// </remarks>
    public TextDirection? Direction { get; set; }

    /// <summary>
    /// Gets the resource files that are searched, last added first, before TwBlazor's built in strings.
    /// </summary>
    /// <remarks>
    /// Each one is a <c>.resx</c> family in your own project (for example <c>TwBlazorStrings.resx</c>,
    /// <c>TwBlazorStrings.de.resx</c>, <c>TwBlazorStrings.fr.resx</c>) whose names are the <see cref="TwStrings"/>
    /// keys. Only the keys you want to change need to be present.
    /// </remarks>
    public IList<ResourceManager> Resources { get; } = [];

    /// <summary>
    /// Gets or sets the date and time formats. Anything left unset comes from the resource files, then from the culture.
    /// </summary>
    public TwDateTimeFormatOptions Formats { get; set; } = new();

    /// <summary>
    /// Adds a resource file family to <see cref="Resources"/>.
    /// </summary>
    /// <param name="resourceManager">The resource manager of the <c>.resx</c> family, for example <c>MyStrings.ResourceManager</c>.</param>
    /// <returns>These options, so calls can be chained.</returns>
    public TwLocalizationOptions AddResource(ResourceManager resourceManager)
    {
        ArgumentNullException.ThrowIfNull(resourceManager);
        Resources.Add(resourceManager);
        return this;
    }

    /// <summary>
    /// Adds the resource file family that <typeparamref name="TResource"/> names to <see cref="Resources"/>.
    /// </summary>
    /// <typeparam name="TResource">
    /// A class with the same name and namespace as the neutral <c>.resx</c> file, in the same assembly: the designer
    /// class Visual Studio generates, or an empty marker class.
    /// </typeparam>
    /// <returns>These options, so calls can be chained.</returns>
    public TwLocalizationOptions AddResource<TResource>() => AddResource(new ResourceManager(typeof(TResource)));
}
