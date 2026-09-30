// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using TwBlazor.Configuration;
using TwBlazor.Configuration.Color;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;

namespace TwBlazor.Tests.Configuration;

public class TwBlazorPaletteTests
{
    [Fact]
    public void DefaultValues_AreAllEmptyStrings()
    {
        var palette = new TwBlazorPalette();

        Assert.Equal(string.Empty, palette.Primary);
        Assert.Equal(string.Empty, palette.Accent);
        Assert.Equal(string.Empty, palette.Success);
        Assert.Equal(string.Empty, palette.Danger);
        Assert.Equal(string.Empty, palette.Warning);
        Assert.Equal(string.Empty, palette.Info);
        Assert.Equal(string.Empty, palette.Light);
        Assert.Equal(string.Empty, palette.Dark);
    }

    [Fact]
    public void Properties_RoundTrip_AssignedValues()
    {
        var palette = new TwBlazorPalette
        {
            Primary = "text-purple-600",
            Accent = "text-fuchsia-600",
            Success = "text-green-600",
            Danger = "text-red-600",
            Warning = "text-yellow-600",
            Info = "text-blue-600",
            Light = "text-white",
            Dark = "text-black",
        };

        Assert.Equal("text-purple-600", palette.Primary);
        Assert.Equal("text-fuchsia-600", palette.Accent);
        Assert.Equal("text-green-600", palette.Success);
        Assert.Equal("text-red-600", palette.Danger);
        Assert.Equal("text-yellow-600", palette.Warning);
        Assert.Equal("text-blue-600", palette.Info);
        Assert.Equal("text-white", palette.Light);
        Assert.Equal("text-black", palette.Dark);
    }
}

public class TwSurfaceColorTests
{
    [Fact]
    public void DefaultValues_AreEmptyPaletteInstances()
    {
        var surface = new TwSurfaceColor();

        Assert.NotNull(surface.Filled);
        Assert.NotNull(surface.Text);
        Assert.NotNull(surface.Outlined);
        Assert.Equal(string.Empty, surface.Filled.Primary);
    }

    [Fact]
    public void Properties_RoundTrip_AssignedPalettes()
    {
        var filled = new TwBlazorPalette { Primary = "bg-purple-600" };
        var text = new TwBlazorPalette { Primary = "text-purple-600" };
        var outlined = new TwBlazorPalette { Primary = "border-purple-600" };

        var surface = new TwSurfaceColor { Filled = filled, Text = text, Outlined = outlined };

        Assert.Same(filled, surface.Filled);
        Assert.Same(text, surface.Text);
        Assert.Same(outlined, surface.Outlined);
    }
}

public class TwBlazorRoundedTests
{
    [Fact]
    public void DefaultRounded_IsLg()
    {
        var rounded = new TwBlazorRounded();

        Assert.Equal(Rounded.Lg, rounded.DefaultRounded);
    }

    [Fact]
    public void DefaultValues_AreEmptyStrings_AndScalesAreNonNull()
    {
        var rounded = new TwBlazorRounded();

        Assert.Equal(string.Empty, rounded.None);
        Assert.Equal(string.Empty, rounded.Sm);
        Assert.Equal(string.Empty, rounded.Md);
        Assert.Equal(string.Empty, rounded.Lg);
        Assert.Equal(string.Empty, rounded.Full);
        Assert.NotNull(rounded.RoundedTop);
        Assert.NotNull(rounded.RoundedBottom);
        Assert.NotNull(rounded.RoundedStart);
        Assert.NotNull(rounded.RoundedEnd);
    }

    [Fact]
    public void Properties_RoundTrip_AssignedValues()
    {
        var scale = new TwBlazorRoundedScale { None = "rounded-none", Sm = "rounded-sm", Md = "rounded-md", Lg = "rounded-lg", Full = "rounded-full" };

        var rounded = new TwBlazorRounded
        {
            DefaultRounded = Rounded.None,
            None = "rounded-none",
            Sm = "rounded-sm",
            Md = "rounded-md",
            Lg = "rounded-lg",
            Full = "rounded-full",
            RoundedTop = scale,
            RoundedBottom = scale,
            RoundedStart = scale,
            RoundedEnd = scale,
        };

        Assert.Equal(Rounded.None, rounded.DefaultRounded);
        Assert.Equal("rounded-none", rounded.None);
        Assert.Equal("rounded-sm", rounded.Sm);
        Assert.Equal("rounded-md", rounded.Md);
        Assert.Equal("rounded-lg", rounded.Lg);
        Assert.Equal("rounded-full", rounded.Full);
        Assert.Same(scale, rounded.RoundedTop);
        Assert.Same(scale, rounded.RoundedBottom);
        Assert.Same(scale, rounded.RoundedStart);
        Assert.Same(scale, rounded.RoundedEnd);
    }
}

public class TwBlazorRoundedScaleTests
{
    [Fact]
    public void DefaultValues_AreAllEmptyStrings()
    {
        var scale = new TwBlazorRoundedScale();

        Assert.Equal(string.Empty, scale.None);
        Assert.Equal(string.Empty, scale.Sm);
        Assert.Equal(string.Empty, scale.Md);
        Assert.Equal(string.Empty, scale.Lg);
        Assert.Equal(string.Empty, scale.Full);
    }

    [Fact]
    public void Properties_RoundTrip_AssignedValues()
    {
        var scale = new TwBlazorRoundedScale
        {
            None = "rounded-t-none",
            Sm = "rounded-t-sm",
            Md = "rounded-t-md",
            Lg = "rounded-t-lg",
            Full = "rounded-t-full",
        };

        Assert.Equal("rounded-t-none", scale.None);
        Assert.Equal("rounded-t-sm", scale.Sm);
        Assert.Equal("rounded-t-md", scale.Md);
        Assert.Equal("rounded-t-lg", scale.Lg);
        Assert.Equal("rounded-t-full", scale.Full);
    }
}

public class TwBlazorShadowTests
{
    [Fact]
    public void DefaultShadow_IsSm()
    {
        var shadow = new TwBlazorShadow();

        Assert.Equal(Shadow.Sm, shadow.DefaultShadow);
    }

    [Fact]
    public void DefaultValues_AreAllEmptyStrings()
    {
        var shadow = new TwBlazorShadow();

        Assert.Equal(string.Empty, shadow.None);
        Assert.Equal(string.Empty, shadow.Sm);
        Assert.Equal(string.Empty, shadow.Md);
        Assert.Equal(string.Empty, shadow.Lg);
        Assert.Equal(string.Empty, shadow.HoverSm);
        Assert.Equal(string.Empty, shadow.HoverMd);
        Assert.Equal(string.Empty, shadow.HoverLg);
        Assert.Equal(string.Empty, shadow.ActiveMd);
    }

    [Fact]
    public void Properties_RoundTrip_AssignedValues()
    {
        var shadow = new TwBlazorShadow
        {
            DefaultShadow = Shadow.Lg,
            None = "shadow-none",
            Sm = "shadow-sm",
            Md = "shadow-md",
            Lg = "shadow-lg",
            HoverSm = "hover:shadow-sm",
            HoverMd = "hover:shadow-md",
            HoverLg = "hover:shadow-lg",
            ActiveMd = "active:shadow-md",
        };

        Assert.Equal(Shadow.Lg, shadow.DefaultShadow);
        Assert.Equal("shadow-none", shadow.None);
        Assert.Equal("shadow-sm", shadow.Sm);
        Assert.Equal("shadow-md", shadow.Md);
        Assert.Equal("shadow-lg", shadow.Lg);
        Assert.Equal("hover:shadow-sm", shadow.HoverSm);
        Assert.Equal("hover:shadow-md", shadow.HoverMd);
        Assert.Equal("hover:shadow-lg", shadow.HoverLg);
        Assert.Equal("active:shadow-md", shadow.ActiveMd);
    }
}

public class TwButtonThemeTests
{
    private static TwButtonTheme CreateTheme() => new()
    {
        Uppercase = "uppercase",
        Base = "base",
        Padding = "padding",
        DensePadding = "dense-padding",
        IconButton = "icon-button",
        Typography = "typography",
        IconTypography = "icon-typography",
        DisabledCursor = "disabled-cursor",
        ReadonlyCursor = "readonly-cursor",
        DefaultCursor = "default-cursor",
        DisabledFilled = "disabled-filled",
        DisabledOutlined = "disabled-outlined",
        DisabledText = "disabled-text",
    };

    [Fact]
    public void DefaultValues_MatchDocumentedDefaults()
    {
        var theme = CreateTheme();

        Assert.Equal(ButtonVariant.Filled, theme.DefaultVariant);
        Assert.False(theme.ButtonUppercase);
        Assert.Null(theme.ButtonRounded);
        Assert.Null(theme.ButtonShadow);
    }

    [Fact]
    public void Properties_RoundTrip_AssignedValues()
    {
        var theme = CreateTheme();
        theme.ButtonRounded = Rounded.Full;
        theme.ButtonShadow = Shadow.Lg;
        theme.DefaultVariant = ButtonVariant.Outlined;
        theme.ButtonUppercase = true;

        Assert.Equal(Rounded.Full, theme.ButtonRounded);
        Assert.Equal(Shadow.Lg, theme.ButtonShadow);
        Assert.Equal(ButtonVariant.Outlined, theme.DefaultVariant);
        Assert.True(theme.ButtonUppercase);
        Assert.Equal("uppercase", theme.Uppercase);
        Assert.Equal("base", theme.Base);
        Assert.Equal("padding", theme.Padding);
        Assert.Equal("dense-padding", theme.DensePadding);
        Assert.Equal("icon-button", theme.IconButton);
        Assert.Equal("typography", theme.Typography);
        Assert.Equal("disabled-cursor", theme.DisabledCursor);
        Assert.Equal("readonly-cursor", theme.ReadonlyCursor);
        Assert.Equal("default-cursor", theme.DefaultCursor);
        Assert.Equal("disabled-filled", theme.DisabledFilled);
        Assert.Equal("disabled-outlined", theme.DisabledOutlined);
        Assert.Equal("disabled-text", theme.DisabledText);
    }
}

public class TwCardThemeTests
{
    [Fact]
    public void DefaultValues_AreEmptyStrings()
    {
        var theme = new TwCardTheme();

        Assert.Equal(string.Empty, theme.Container);
        Assert.Equal(string.Empty, theme.Bordered);
    }

    [Fact]
    public void Properties_RoundTrip_AssignedValues()
    {
        var theme = new TwCardTheme { Container = "p-4", Bordered = "border" };

        Assert.Equal("p-4", theme.Container);
        Assert.Equal("border", theme.Bordered);
    }
}

public class TwInputThemeTests
{
    private static TwInputTheme CreateTheme() => new()
    {
        TextfieldBase = "textfield-base",
        SelectBase = "select-base",
        Size = "input-size",
        DenseSize = "input-dense-size",
        LabelBase = "label-base",
        InputLegendBase = "legend-base",
        OutlinedBorder = "outlined-border",
        FilledBorder = "filled-border",
        FocusBorder = "focus-border",
        FilledBackgroundColor = "filled-bg",
        ErrorMessage = "error-message",
        SelectOption = "select-option",
        SelectNativeBackground = "select-native-bg",
        SelectDefaultPadding = "select-default-padding",
        SelectReadOnlyBackground = "select-readonly-bg",
        SelectMultiTriggerLayout = "select-multi-trigger-layout",
        SelectNativeMultiOverlay = "select-native-multi-overlay",
        SelectMultiOpenButton = "select-multi-open-button",
        SelectPanelPosition = "select-panel-position",
        SelectPanelSurface = "select-panel-surface",
        SelectPanelItemText = "select-panel-item-text",
    };

    [Fact]
    public void DefaultInputVariant_IsFilled()
    {
        var theme = CreateTheme();

        Assert.Equal(InputVariant.Filled, theme.DefaultInputVariant);
    }

    [Fact]
    public void Properties_RoundTrip_AssignedValues()
    {
        var theme = CreateTheme();
        theme.DefaultInputVariant = InputVariant.Outlined;

        Assert.Equal(InputVariant.Outlined, theme.DefaultInputVariant);
        Assert.Equal("textfield-base", theme.TextfieldBase);
        Assert.Equal("select-base", theme.SelectBase);
        Assert.Equal("input-size", theme.Size);
        Assert.Equal("input-dense-size", theme.DenseSize);
        Assert.Equal("label-base", theme.LabelBase);
        Assert.Equal("legend-base", theme.InputLegendBase);
        Assert.Equal("outlined-border", theme.OutlinedBorder);
        Assert.Equal("filled-border", theme.FilledBorder);
        Assert.Equal("focus-border", theme.FocusBorder);
        Assert.Equal("filled-bg", theme.FilledBackgroundColor);
    }
}

public class TwAnchorPositionTests
{
    [Fact]
    public void Properties_RoundTrip_AssignedValues()
    {
        var anchor = new TwAnchorPosition
        {
            Center = "center",
            CenterLeft = "center-left",
            CenterRight = "center-right",
            TopCenter = "top-center",
            TopLeft = "top-left",
            TopRight = "top-right",
            BottomCenter = "bottom-center",
            BottomLeft = "bottom-left",
            BottomRight = "bottom-right",
        };

        Assert.Equal("center", anchor.Center);
        Assert.Equal("center-left", anchor.CenterLeft);
        Assert.Equal("center-right", anchor.CenterRight);
        Assert.Equal("top-center", anchor.TopCenter);
        Assert.Equal("top-left", anchor.TopLeft);
        Assert.Equal("top-right", anchor.TopRight);
        Assert.Equal("bottom-center", anchor.BottomCenter);
        Assert.Equal("bottom-left", anchor.BottomLeft);
        Assert.Equal("bottom-right", anchor.BottomRight);
    }
}

public class TwTextColorTests
{
    [Fact]
    public void DefaultValues_AreEmptyPaletteInstances()
    {
        var textColor = new TwTextColor();

        Assert.NotNull(textColor.Light);
        Assert.NotNull(textColor.Medium);
        Assert.NotNull(textColor.Dark);
        Assert.Equal(string.Empty, textColor.Light.Primary);
    }

    [Fact]
    public void Properties_RoundTrip_AssignedPalettes()
    {
        var light = new TwBlazorPalette { Primary = "text-purple-200" };
        var medium = new TwBlazorPalette { Primary = "text-purple-600" };
        var dark = new TwBlazorPalette { Primary = "text-purple-900" };

        var textColor = new TwTextColor { Light = light, Medium = medium, Dark = dark };

        Assert.Same(light, textColor.Light);
        Assert.Same(medium, textColor.Medium);
        Assert.Same(dark, textColor.Dark);
    }
}

public class TwBackgroundColorTests
{
    [Fact]
    public void DefaultValues_AreEmptyPaletteInstances()
    {
        var backgroundColor = new TwBackgroundColor();

        Assert.NotNull(backgroundColor.Light);
        Assert.NotNull(backgroundColor.Medium);
        Assert.NotNull(backgroundColor.Dark);
        Assert.Equal(string.Empty, backgroundColor.Light.Primary);
    }

    [Fact]
    public void Properties_RoundTrip_AssignedPalettes()
    {
        var light = new TwBlazorPalette { Primary = "bg-purple-200" };
        var medium = new TwBlazorPalette { Primary = "bg-purple-600" };
        var dark = new TwBlazorPalette { Primary = "bg-purple-900" };

        var backgroundColor = new TwBackgroundColor { Light = light, Medium = medium, Dark = dark };

        Assert.Same(light, backgroundColor.Light);
        Assert.Same(medium, backgroundColor.Medium);
        Assert.Same(dark, backgroundColor.Dark);
    }
}

public class TwBlazorColorTests
{
    [Fact]
    public void DefaultValues_AreNonNull_AndFocusRingBaseIsEmpty()
    {
        var colors = new TwBlazorColor();

        Assert.NotNull(colors.TextColors);
        Assert.NotNull(colors.DarkTextColors);
        Assert.NotNull(colors.HoverColors);
        Assert.Equal(string.Empty, colors.FocusRingBase);
        Assert.NotNull(colors.FocusColors);
        Assert.Equal(string.Empty, colors.Transparent);
        Assert.NotNull(colors.DarkBackground);
        Assert.NotNull(colors.LightBackground);
        Assert.NotNull(colors.SurfaceColors);
    }

    [Fact]
    public void Properties_RoundTrip_AssignedValues()
    {
        var textColors = new TwTextColor();
        var darkTextColors = new TwTextColor();
        var hoverColors = new TwBlazorPalette { Primary = "hover:bg-purple-700" };
        var focusColors = new TwBlazorPalette { Primary = "focus:ring-purple-500" };
        var darkBackground = new TwBackgroundColor();
        var lightBackground = new TwBackgroundColor();
        var surfaceColors = new TwSurfaceColor();

        var colors = new TwBlazorColor
        {
            TextColors = textColors,
            DarkTextColors = darkTextColors,
            HoverColors = hoverColors,
            FocusRingBase = "focus:ring-2",
            FocusColors = focusColors,
            Transparent = "bg-transparent",
            DarkBackground = darkBackground,
            LightBackground = lightBackground,
            SurfaceColors = surfaceColors,
        };

        Assert.Same(textColors, colors.TextColors);
        Assert.Same(darkTextColors, colors.DarkTextColors);
        Assert.Same(hoverColors, colors.HoverColors);
        Assert.Equal("focus:ring-2", colors.FocusRingBase);
        Assert.Same(focusColors, colors.FocusColors);
        Assert.Equal("bg-transparent", colors.Transparent);
        Assert.Same(darkBackground, colors.DarkBackground);
        Assert.Same(lightBackground, colors.LightBackground);
        Assert.Same(surfaceColors, colors.SurfaceColors);
    }
}

public class TwBlazorThemeTests
{
    [Fact]
    public void DefaultValues_SubObjectsAreNonNull()
    {
        var theme = new TwBlazorTheme { Anchor = new TwAnchorPosition { Center = "", CenterLeft = "", CenterRight = "", TopCenter = "", TopLeft = "", TopRight = "", BottomCenter = "", BottomLeft = "", BottomRight = "" } };

        Assert.NotNull(theme.Shadows);
        Assert.NotNull(theme.Rounded);
        Assert.NotNull(theme.Colors);
        Assert.NotNull(theme.Components);
    }

    [Fact]
    public void Properties_RoundTrip_AssignedValues()
    {
        var shadows = new TwBlazorShadow();
        var rounded = new TwBlazorRounded();
        var colors = new TwBlazorColor();
        var anchor = new TwAnchorPosition { Center = "center", CenterLeft = "", CenterRight = "", TopCenter = "", TopLeft = "", TopRight = "", BottomCenter = "", BottomLeft = "", BottomRight = "" };
        TwBlazorComponents components = [];

        var theme = new TwBlazorTheme
        {
            Shadows = shadows,
            Rounded = rounded,
            Colors = colors,
            Anchor = anchor,
            Components = components,
        };

        Assert.Same(shadows, theme.Shadows);
        Assert.Same(rounded, theme.Rounded);
        Assert.Same(colors, theme.Colors);
        Assert.Same(anchor, theme.Anchor);
        Assert.Same(components, theme.Components);
    }
}

public class TwBlazorComponentsTests
{
    [Fact]
    public void Get_ReturnsNull_WhenThemeNotRegistered()
    {
        TwBlazorComponents components = [];

        Assert.Null(components.Get<TwCardTheme>());
    }

    [Fact]
    public void Add_ThenGet_ReturnsTheRegisteredTheme()
    {
        TwBlazorComponents components = [];
        var cardTheme = new TwCardTheme { Container = "p-4" };

        components.Add(cardTheme);

        Assert.Same(cardTheme, components.Get<TwCardTheme>());
    }

    [Fact]
    public void Add_ReturnsSameInstance_ToSupportChaining()
    {
        TwBlazorComponents components = [];

        var result = components.Add(new TwCardTheme { Container = "p-4" });

        Assert.Same(components, result);
    }

    [Fact]
    public void Add_OverwritesPreviouslyRegisteredThemeOfSameType()
    {
        TwBlazorComponents components = [];
        var first = new TwCardTheme { Container = "p-2" };
        var second = new TwCardTheme { Container = "p-4" };

        components.Add(first);
        components.Add(second);

        Assert.Same(second, components.Get<TwCardTheme>());
    }

    [Fact]
    public void Require_ReturnsTheRegisteredTheme_WhenPresent()
    {
        TwBlazorComponents components = [];
        var cardTheme = new TwCardTheme { Container = "p-4" };
        components.Add(cardTheme);

        Assert.Same(cardTheme, components.Require<TwCardTheme>());
    }

    [Fact]
    public void Require_Throws_WhenThemeNotRegistered()
    {
        TwBlazorComponents components = [];

        var ex = Assert.Throws<InvalidOperationException>(() => components.Require<TwCardTheme>());
        Assert.Contains(nameof(TwCardTheme), ex.Message);
    }

    [Fact]
    public void Contains_ReflectsWhetherThemeHasBeenAdded()
    {
        TwBlazorComponents components = [];

        Assert.False(components.Contains<TwCardTheme>());

        components.Add(new TwCardTheme { Container = "p-4" });

        Assert.True(components.Contains<TwCardTheme>());
    }

    [Fact]
    public void Enumeration_YieldsAllRegisteredThemes()
    {
        TwBlazorComponents components = [];
        var cardTheme = new TwCardTheme { Container = "p-4" };
        var buttonTheme = new TwButtonTheme
        {
            Uppercase = "uppercase",
            Base = "base",
            Padding = "padding",
            DensePadding = "dense-padding",
            IconButton = "icon-button",
            Typography = "typography",
            IconTypography = "icon-typography",
            DisabledCursor = "disabled-cursor",
            ReadonlyCursor = "readonly-cursor",
            DefaultCursor = "default-cursor",
            DisabledFilled = "disabled-filled",
            DisabledOutlined = "disabled-outlined",
            DisabledText = "disabled-text",
        };

        components.Add(cardTheme).Add(buttonTheme);

        Assert.Contains(cardTheme, components);
        Assert.Contains(buttonTheme, components);
        Assert.Equal(2, components.Count());
    }

    [Fact]
    public void Enumeration_ViaNonGenericEnumerable_YieldsAllRegisteredThemes()
    {
        System.Collections.IEnumerable components = new TwBlazorComponents().Add(new TwCardTheme { Container = "p-4" });

        List<object> items = [.. components];

        var theme = Assert.Single(items);
        Assert.IsType<TwCardTheme>(theme);
    }
}
