// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Text;
using System.Xml;

namespace TwBlazor.Export.Packaging;

/// <summary>
/// Prepares text for an XML part.
/// </summary>
internal static class XmlCharacters
{
    /// <summary>
    /// Removes the characters XML cannot hold, such as most control characters, which <see cref="XmlWriter"/> would
    /// otherwise refuse to write.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>The text without the characters XML cannot hold.</returns>
    public static string Clean(string text)
    {
        if (text.All(XmlConvert.IsXmlChar))
        {
            return text;
        }

        var clean = new StringBuilder(text.Length);
        var i = 0;
        while (i < text.Length)
        {
            if (XmlConvert.IsXmlChar(text[i]))
            {
                clean.Append(text[i]);
                i++;
            }
            else if (i + 1 < text.Length && XmlConvert.IsXmlSurrogatePair(text[i + 1], text[i]))
            {
                // A character outside the Basic Multilingual Plane, such as an emoji, is two UTF-16 units.
                clean.Append(text, i, 2);
                i += 2;
            }
            else
            {
                i++;
            }
        }

        return clean.ToString();
    }
}
