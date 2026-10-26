// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace TwBlazor.Enums;

/// <summary>
/// The emphasis of text, resolved against the theme's neutral text palette.
/// </summary>
public enum TextTone
{
    /// <summary>
    /// The default body and heading text colour, the highest-contrast neutral text.
    /// </summary>
    Default,

    /// <summary>
    /// Supporting copy, one step quieter than <see cref="Default"/>.
    /// </summary>
    Secondary,

    /// <summary>
    /// Muted text for labels, captions and other lower-emphasis content.
    /// </summary>
    Muted,

    /// <summary>
    /// The quietest text colour, for decorative or low-emphasis content.
    /// </summary>
    Subtle,

    /// <summary>
    /// No colour class: the text inherits the colour of its surrounding element.
    /// </summary>
    Inherit
}

/// <summary>
/// A size on the theme's shared font-size scale.
/// </summary>
public enum TextSize
{
    /// <summary>
    /// The smallest size, for dense captions and small chips.
    /// </summary>
    Xxs,

    /// <summary>
    /// Extra-small text.
    /// </summary>
    Xs,

    /// <summary>
    /// Small text.
    /// </summary>
    Sm,

    /// <summary>
    /// The base (default) text size.
    /// </summary>
    Base,

    /// <summary>
    /// Large text.
    /// </summary>
    Lg,

    /// <summary>
    /// Extra-large text.
    /// </summary>
    Xl,

    /// <summary>
    /// The 2xl text size.
    /// </summary>
    Xl2,

    /// <summary>
    /// The 3xl text size.
    /// </summary>
    Xl3,

    /// <summary>
    /// The 4xl text size.
    /// </summary>
    Xl4,

    /// <summary>
    /// The 5xl text size.
    /// </summary>
    Xl5
}

/// <summary>
/// A weight on the theme's shared font-weight scale.
/// </summary>
public enum TextWeight
{
    /// <summary>
    /// Normal weight.
    /// </summary>
    Normal,

    /// <summary>
    /// Medium weight.
    /// </summary>
    Medium,

    /// <summary>
    /// Semibold weight.
    /// </summary>
    Semibold,

    /// <summary>
    /// Bold weight.
    /// </summary>
    Bold
}
