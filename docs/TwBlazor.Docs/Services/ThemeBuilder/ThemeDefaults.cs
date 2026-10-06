using TwBlazor.Enums;

namespace TwBlazor.Docs.Services.ThemeBuilder;

/// <summary>
/// The enum-valued defaults a theme sets for every component that doesn't override them per instance.
/// </summary>
/// <param name="Rounded">The corner radius, <c>TwBlazorRounded.DefaultRounded</c>.</param>
/// <param name="Shadow">The shadow, <c>TwBlazorShadow.DefaultShadow</c>.</param>
/// <param name="InputVariant">The input style, <c>TwInputTheme.DefaultInputVariant</c>.</param>
/// <param name="ButtonVariant">The button style, <c>TwButtonTheme.DefaultVariant</c>.</param>
public sealed record ThemeDefaults(Rounded Rounded, Shadow Shadow, InputVariant InputVariant, ButtonVariant ButtonVariant)
{
    /// <summary>
    /// What the default theme sets, and so what the generated file contains when nothing is changed.
    /// </summary>
    public static ThemeDefaults Default { get; } = new(Rounded.Md, Shadow.Sm, InputVariant.Outlined, ButtonVariant.Filled);
}
