// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using TwBlazor.Enums;

namespace TwBlazor.Configuration.Components;

/// <summary>
/// Theme configuration for button components (<see cref="TwBlazor.Components.TwButton"/>).
/// Override any property to customize button styles globally.
/// </summary>
[ExcludeFromCodeCoverage]
public class TwButtonTheme
{
    /// <summary>
    /// Gets or sets the default border radius for buttons.
    /// </summary>
    /// <remarks>
    /// Configure button rounded corners via theme.
    /// Set to <see cref="Rounded.None"/> to disable rounded corners.
    /// If not set, falls back to global <see cref="TwBlazorRounded.DefaultRounded"/>.
    /// Default is <c>null</c> (uses global default).
    /// </remarks>
    public Rounded? ButtonRounded { get; set; }

    /// <summary>
    /// Gets or sets the default shadow level for buttons.
    /// </summary>
    /// <remarks>
    /// Configure button shadows via theme.
    /// Set to <see cref="Shadow.None"/> to disable shadows.
    /// If not set, falls back to global <see cref="TwBlazorShadow.DefaultShadow"/>.
    /// Default is <c>null</c> (uses global default).
    /// </remarks>
    public Shadow? ButtonShadow { get; set; }

    /// <summary>
    /// Gets or sets the default button variant for all buttons.
    /// </summary>
    /// <remarks>
    /// Default is <see cref="ButtonVariant.Filled"/>.
    /// Individual buttons can override this setting.
    /// </remarks>
    public ButtonVariant? DefaultVariant { get; set; } = ButtonVariant.Filled;

    /// <summary>
    /// Gets or sets whether to use uppercase text for buttons by default.
    /// </summary>
    /// <remarks>
    /// Default is <c>false</c>.
    /// </remarks>
    public bool ButtonUppercase { get; set; } = false;

    /// <summary>
    /// Gets or sets the uppercase class for button text if <see cref="ButtonUppercase" /> is <c>true</c>.
    /// </summary>
    public required string Uppercase { get; set; }

    /// <summary>
    /// Gets or sets the base classes applied to all buttons.
    /// </summary>
    public required string Base { get; set; }

    /// <summary>
    /// Gets or sets the padding classes for non-icon buttons.
    /// </summary>
    public required string Padding { get; set; }

    /// <summary>
    /// Gets or sets the padding classes for dense buttons.
    /// </summary>
    public required string DensePadding { get; set; }

    /// <summary>
    /// Gets or sets the classes for icon buttons.
    /// </summary>
    public required string IconButton { get; set; }

    /// <summary>
    /// Gets or sets the typography classes for button text.
    /// </summary>
    public required string Typography { get; set; }

    /// <summary>
    /// Gets or sets the size/line-height classes for a button's start/end icon.
    /// </summary>
    public required string IconTypography { get; set; }

    /// <summary>
    /// Gets or sets the disabled cursor class.
    /// </summary>
    public required string DisabledCursor { get; set; }

    /// <summary>
    /// Gets or sets the readonly cursor class.
    /// </summary>
    public required string ReadonlyCursor { get; set; }

    /// <summary>
    /// Gets or sets the default cursor class.
    /// </summary>
    public required string DefaultCursor { get; set; }

    /// <summary>
    /// Gets or sets the disabled background and text classes.
    /// </summary>
    public required string DisabledFilled { get; set; }

    /// <summary>
    /// Gets or sets the disabled outlined button classes.
    /// </summary>
    public required string DisabledOutlined { get; set; }

    /// <summary>
    /// Gets or sets the disabled text button classes.
    /// </summary>
    public required string DisabledText { get; set; }
}
