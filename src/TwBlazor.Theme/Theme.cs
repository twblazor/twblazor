using System.Reflection.Metadata;
using TwBlazor.Configuration;
using TwBlazor.Configuration.Color;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;

[assembly: MetadataUpdateHandler(typeof(TwBlazor.Theme.ThemeUpdateHandler))]

namespace TwBlazor.Theme;

public static class Theme
{
    #region CodeExample GetStartedTheme
    public static TwBlazorTheme CreateDefaultTheme()
    {
        #region utility classes

        var flexbox = new TwBlazorFlexbox
        {
            Row = "flex-row",
            RowReverse = "flex-row-reverse",
            Col = "flex-col",
            ColReverse = "flex-col-reverse",
            Wrap = "flex-wrap",
            NoWrap = "flex-nowrap",
            WrapReverse = "flex-wrap-reverse",
            Grow = "grow",
            GrowNone = "grow-0",
            Shrink = "shrink",
            ShrinkNone = "shrink-0",
            Flex1 = "flex-1",
            Justify = new()
            {
                Start = "justify-start",
                Center = "justify-center",
                End = "justify-end",
                Between = "justify-between",
                Around = "justify-around",
                Evenly = "justify-evenly"
            },
            Align = new()
            {
                Start = "items-start",
                Center = "items-center",
                End = "items-end",
                Stretch = "items-stretch",
                Baseline = "items-baseline"
            },
            AlignContent = new()
            {
                Start = "content-start",
                Center = "content-center",
                End = "content-end",
                Between = "content-between",
                Around = "content-around",
                Evenly = "content-evenly",
                Stretch = "content-stretch"
            },
            AlignSelf = new()
            {
                Start = "self-start",
                Center = "self-center",
                End = "self-end",
                Stretch = "self-stretch",
                Baseline = "self-baseline"
            }
        };

        var anchor = new TwAnchorPosition
        {
            Center = $"{flexbox.Align.Center} {flexbox.Justify.Center}",
            CenterLeft = $"{flexbox.Align.Center} {flexbox.Justify.Start}",
            CenterRight = $"{flexbox.Align.Center} {flexbox.Justify.End}",
            TopCenter = $"{flexbox.Align.Start} {flexbox.Justify.Center}",
            TopLeft = $"{flexbox.Align.Start} {flexbox.Justify.Start}",
            TopRight = $"{flexbox.Align.Start} {flexbox.Justify.End}",
            BottomCenter = $"{flexbox.Align.End} {flexbox.Justify.Center}",
            BottomLeft = $"{flexbox.Align.End} {flexbox.Justify.Start}",
            BottomRight = $"{flexbox.Align.End} {flexbox.Justify.End}",
        };

        var text = new TwTextColor
        {
            Light =
            {
                Primary = "text-purple-200",
                Accent = "text-fuchsia-200",
                Success = "text-green-200",
                Danger = "text-red-200",
                Warning = "text-yellow-200",
                Info = "text-blue-200",
                Light = "text-white",
                Dark = "text-gray-200"
            },
            Medium =
            {
                Primary = "text-purple-600",
                Accent = "text-fuchsia-600",
                Success = "text-green-800",
                Danger = "text-red-700",
                Warning = "text-yellow-800",
                Info = "text-blue-600",
                Light = "text-gray-100",
                Dark = "text-gray-950",
            },
            Dark =
            {
                Primary = "text-purple-900",
                Accent = "text-fuchsia-900",
                Success = "text-green-900",
                Danger = "text-red-900",
                Warning = "text-yellow-900",
                Info = "text-blue-900",
                Light = "text-gray-300",
                Dark = "text-gray-900"
            }
        };

        var darkText = new TwTextColor
        {
            Light =
            {
                Primary = "dark:text-purple-200",
                Accent = "dark:text-fuchsia-200",
                Success = "dark:text-green-200",
                Danger = "dark:text-red-200",
                Warning = "dark:text-yellow-200",
                Info = "dark:text-blue-200",
                Dark = "dark:text-gray-200"
            },
            Medium =
            {
                Primary = "dark:text-purple-600",
                Accent = "dark:text-fuchsia-600",
                Success = "dark:text-green-600",
                Danger = "dark:text-red-600",
                Warning = "dark:text-yellow-600",
                Info = "dark:text-blue-600",
                Light = "dark:text-white",
                Dark = "dark:text-gray-950",
            },
            Dark =
            {
                Primary = "dark:text-purple-900",
                Accent = "dark:text-fuchsia-900",
                Success = "dark:text-green-900",
                Danger = "dark:text-red-900",
                Warning = "dark:text-yellow-900",
                Info = "dark:text-blue-900",
                Dark = "dark:text-gray-900"
            }
        };

        var borderColors = new TwBlazorPalette
        {
            Primary = "border-purple-600 dark:border-purple-500",
            Accent = "border-fuchsia-600 dark:border-fuchsia-400",
            Success = "border-green-600 dark:border-green-400",
            Danger = "border-red-600 dark:border-red-400",
            Warning = "border-yellow-700 dark:border-yellow-400",
            Info = "border-blue-600 dark:border-blue-400",
            Light = "border-gray-100 dark:border-gray-700",
            Dark = "border-gray-900 dark:border-gray-700"
        };

        var hoverColors = new TwBlazorPalette
        {
            Primary = "hover:bg-purple-50 dark:hover:bg-purple-900/20",
            Accent = "hover:bg-fuchsia-50 dark:hover:bg-fuchsia-900/20",
            Success = "hover:bg-green-50 dark:hover:bg-green-900/20",
            Danger = "hover:bg-red-50 dark:hover:bg-red-900/20",
            Warning = "hover:bg-yellow-50 dark:hover:bg-yellow-900/20",
            Info = "hover:bg-blue-50 dark:hover:bg-blue-900/20",
            Light = "hover:bg-gray-100/10",
            Dark = "hover:bg-gray-900/10"
        };

        var background = new TwBackgroundColor
        {
            Lightest = new()
            {
                Primary = $"bg-purple-50",
                Accent = $"bg-fuchsia-50",
                Success = $"bg-green-50",
                Danger = $"bg-red-50",
                Warning = $"bg-yellow-50",
                Info = $"bg-blue-50",
                Light = $"bg-white",
                Dark = $"bg-gray-50"
            },
            Light = new()
            {
                Primary = $"bg-purple-200",
                Accent = $"bg-fuchsia-200",
                Success = $"bg-green-200",
                Danger = $"bg-red-200",
                Warning = $"bg-yellow-200",
                Info = $"bg-blue-200",
                Light = $"bg-white",
                Dark = $"bg-gray-200"
            },
            Medium = new()
            {
                Primary = $"bg-purple-600",
                Accent = $"bg-fuchsia-600",
                Success = $"bg-green-700",
                Danger = $"bg-red-600",
                Warning = $"bg-yellow-600",
                Info = $"bg-blue-600",
                Light = $"bg-gray-100",
                Dark = $"bg-gray-600",
            },
            Dark = new()
            {
                Primary = $"bg-purple-900",
                Accent = $"bg-fuchsia-900",
                Success = $"bg-green-900",
                Danger = $"bg-red-900",
                Warning = $"bg-yellow-900",
                Info = $"bg-blue-900",
                Light = $"bg-gray-300",
                Dark = $"bg-gray-900"
            },
            Darkest = new()
            {
                Primary = $"bg-purple-950",
                Accent = $"bg-fuchsia-950",
                Success = $"bg-green-950",
                Danger = $"bg-red-950",
                Warning = $"bg-yellow-950",
                Info = $"bg-blue-950",
                Light = $"bg-gray-950",
                Dark = $"bg-black"
            }
        };

        var darkBackground = new TwBackgroundColor
        {
            Lightest = new()
            {
                Primary = $"dark:bg-purple-50",
                Accent = $"dark:bg-fuchsia-50",
                Success = $"dark:bg-green-50",
                Danger = $"dark:bg-red-50",
                Warning = $"dark:bg-yellow-50",
                Info = $"dark:bg-blue-50",
                Dark = $"dark:bg-gray-50"
            },
            Light = new()
            {
                Primary = $"dark:bg-purple-200",
                Accent = $"dark:bg-fuchsia-200",
                Success = $"dark:bg-green-200",
                Danger = $"dark:bg-red-200",
                Warning = $"dark:bg-yellow-200",
                Info = $"dark:bg-blue-200",
                Dark = $"dark:bg-gray-200",
            },
            Medium = new()
            {
                Primary = $"dark:bg-purple-600",
                Accent = $"dark:bg-fuchsia-600",
                Success = $"dark:bg-green-600",
                Danger = $"dark:bg-red-600",
                Warning = $"dark:bg-yellow-600",
                Info = $"dark:bg-blue-600",
                Light = $"dark:bg-white",
                Dark = $"dark:bg-gray-600",
            },
            Dark = new()
            {
                Primary = $"dark:bg-purple-900",
                Accent = $"dark:bg-fuchsia-900",
                Success = $"dark:bg-green-900",
                Danger = $"dark:bg-red-900",
                Warning = $"dark:bg-yellow-900",
                Info = $"dark:bg-blue-900",
                Dark = $"dark:bg-gray-900",
            },
            Darkest = new()
            {
                Primary = $"dark:bg-purple-950",
                Accent = $"dark:bg-fuchsia-950",
                Success = $"dark:bg-green-950",
                Danger = $"dark:bg-red-950",
                Warning = $"dark:bg-yellow-950",
                Info = $"dark:bg-blue-950",
                Dark = $"dark:bg-black",
            }
        };

        var checkBoxRadioButtonColors = new TwBlazorPalette()
        {
            Primary = "checked:bg-purple-600 checked:border-purple-600 dark:checked:bg-purple-500 dark:checked:border-purple-500",
            Accent = "checked:bg-fuchsia-600 checked:border-fuchsia-600 dark:checked:bg-fuchsia-500 dark:checked:border-fuchsia-500",
            Success = "checked:bg-green-600 checked:border-green-600 dark:checked:bg-green-500 dark:checked:border-green-500",
            Danger = "checked:bg-red-600 checked:border-red-600 dark:checked:bg-red-500 dark:checked:border-red-500",
            Warning = "checked:bg-yellow-700 checked:border-yellow-700 dark:checked:bg-yellow-500 dark:checked:border-yellow-500",
            Info = "checked:bg-blue-600 checked:border-blue-600 dark:checked:bg-blue-500 dark:checked:border-blue-500",
            Light = "checked:bg-white checked:border-gray-900 dark:checked:bg-white dark:checked:border-gray-500",
            Dark = "checked:bg-gray-900 checked:border-gray-900 dark:checked:bg-gray-900 dark:checked:border-gray-900"
        };

        var neutralSurface = new TwSurfacePalette
        {
            Background = "bg-[oklch(100%_0_0)] dark:bg-[oklch(25.33%_0.016_252.42)]",
            BackgroundSubtle = "bg-[oklch(98%_0_0)] dark:bg-[oklch(23.26%_0.014_253.1)]",
            Border = "border-[oklch(95%_0_0)] dark:border-[oklch(21.15%_0.012_254.09)]",
            BorderSubtle = "border-[oklch(98%_0_0)] dark:border-[oklch(23.26%_0.014_253.1)]",
            Hover = "hover:bg-[oklch(98%_0_0)] dark:hover:bg-[oklch(23.26%_0.014_253.1)]",
            Elevated = "bg-[oklch(97%_0_0)] dark:bg-[oklch(34%_0.018_253)]",
            Overlay = "bg-[oklch(100%_0_0)] dark:bg-[oklch(40%_0.016_253)]",
            BorderStrong = "border-[oklch(21%_0.006_285.885)]/25 dark:border-[oklch(97.807%_0.029_256.847)]/20"
        };

        var neutralText = new TwNeutralTextPalette
        {
            Heading = "text-[oklch(21%_0.006_285.885)] dark:text-[oklch(97.807%_0.029_256.847)]",
            Secondary = "text-[oklch(40%_0.006_285.885)] dark:text-[oklch(88%_0.02_256.847)]",
            Muted = "text-[oklch(50%_0.006_285.885)] dark:text-[oklch(78%_0.02_256.847)]",
            Subtle = "text-[oklch(55%_0.006_285.885)] dark:text-[oklch(68%_0.02_256.847)]"
        };

        // One height for every single-line input (and the controls laid out beside them, e.g. pagination),
        // so they all line up whichever component renders them.
        var inputHeight = "h-10";
        var inputDenseHeight = "h-8";

        var inputLabelText = $"text-xs font-normal tracking-wide {neutralText.Muted}";

        var display = new TwBlazorDisplay
        {
            Block = "block",
            InlineBlock = "inline-block",
            Flex = "flex",
            InlineFlex = "inline-flex",
            Grid = "grid",
            InlineGrid = "inline-grid",
            Hidden = "hidden",
            Contents = "contents",
            ScreenReaderOnly = "sr-only"
        };

        var spacing = new TwBlazorSpacing
        {
            Gap = new()
            {
                Sm = "gap-1",
                Md = "gap-2",
                Lg = "gap-3",
                Xl = "gap-4"
            },
            InteractiveRowPadding = "px-4 py-3",
            PushEnd = "ml-auto",
            Padding = new()
            {
                Sm = "p-1",
                Md = "p-2",
                Lg = "p-3",
                Xl = "p-4"
            },
            Margin = new()
            {
                Sm = "m-1",
                Md = "m-2",
                Lg = "m-3",
                Xl = "m-4"
            },
            MarginTop = new()
            {
                Sm = "mt-1",
                Md = "mt-2",
                Lg = "mt-3",
                Xl = "mt-4"
            },
            MarginBottom = new()
            {
                Sm = "mb-1",
                Md = "mb-2",
                Lg = "mb-3",
                Xl = "mb-4"
            },
            PaddingTop = new()
            {
                Sm = "pt-1",
                Md = "pt-2",
                Lg = "pt-3",
                Xl = "pt-4"
            },
            PaddingStart = new()
            {
                Sm = "ps-1",
                Md = "ps-2",
                Lg = "ps-3",
                Xl = "ps-4"
            },
            MarginStart = new()
            {
                Sm = "ms-1",
                Md = "ms-2",
                Lg = "ms-3",
                Xl = "ms-4"
            },
            MarginEnd = new()
            {
                Sm = "me-1",
                Md = "me-2",
                Lg = "me-3",
                Xl = "me-4"
            }
        };

        var interaction = new TwBlazorInteraction
        {
            DisabledOpacity = "opacity-40",
            PointerCursor = "cursor-pointer",
            DisabledCursor = "cursor-not-allowed",
            ReadonlyCursor = "cursor-default",
            PointerEventsNone = "pointer-events-none",
            FocusOutlineNone = "focus:outline-none"
        };

        var sizing = new TwBlazorSizing
        {

            FullWidth = "w-full",
            FullHeight = "h-full",
            Full = "w-full h-full",
            MinWidthNone = "min-w-0",
            Icon = new()
            {
                Xs = "size-3",
                Sm = "size-4",
                Md = "size-5",
                Lg = "size-6",
                Xl = "size-8"
            }
        };

        var overflow = new TwBlazorOverflow
        {
            Hidden = "overflow-hidden"
        };

        var transition = new TwBlazorTransition
        {
            Colors = "transition-colors",
            Transform = "transition-transform",
            EaseInOut = "ease-in-out",
            DurationFast = "duration-200",
            DurationSlow = "duration-300"
        };
        transition.ColorsFast = $"{transition.Colors} {transition.DurationFast}";
        transition.ColorsSlow = $"{transition.Colors} {transition.DurationSlow}";
        transition.TransformFast = $"{transition.Transform} {transition.DurationFast}";
        transition.TransformSlow = $"{transition.Transform} {transition.DurationSlow}";

        var typography = new TwBlazorTypography
        {
            Size = new()
            {
                Xs = "text-xs",
                Sm = "text-sm",
                Base = "text-base",
                Lg = "text-lg"
            },
            Weight = new()
            {
                Medium = "font-medium",
                Semibold = "font-semibold"
            },
            AlignCenter = "text-center",
            WrapBreakWord = "wrap-break-word"
        };

        var rounded = new TwBlazorRounded
        {
            None = "rounded-none",
            Sm = "rounded-sm",
            Md = "rounded",
            Lg = "rounded-lg",
            Full = "rounded-full",
            DefaultRounded = Rounded.Md,
            RoundedTop = new()
            {
                None = "rounded-t-none",
                Sm = "rounded-t-sm",
                Md = "rounded-t",
                Lg = "rounded-t-lg",
                Full = "rounded-t-full"
            },
            RoundedBottom = new()
            {
                None = "rounded-b-none",
                Sm = "rounded-b-sm",
                Md = "rounded-b",
                Lg = "rounded-b-lg",
                Full = "rounded-b-full"
            },
            RoundedStart = new()
            {
                None = "rounded-s-none",
                Sm = "rounded-s-sm",
                Md = "rounded-s",
                Lg = "rounded-s-lg",
                Full = "rounded-s-full"
            },
            RoundedEnd = new()
            {
                None = "rounded-e-none",
                Sm = "rounded-e-sm",
                Md = "rounded-e",
                Lg = "rounded-e-lg",
                Full = "rounded-e-full"
            }
        };

        var shadows = new TwBlazorShadow
        {
            None = "shadow-none",
            Sm = "shadow-sm",
            Md = "shadow",
            Lg = "shadow-lg",
            Xl = "shadow-xl",
            HoverSm = "hover:shadow-sm",
            HoverMd = "hover:shadow",
            HoverLg = "hover:shadow-xl",
            ActiveMd = "active:shadow",
            DefaultShadow = Shadow.Sm
        };

        var borderWidth = new TwBlazorBorderWidth
        {
            None = "border-0",
            Thin = "border",
            Medium = "border-[1.2px]",
            Thick = "border-2",
            AccentEdge = "border-l-4"
        };

        // Combined border home: one width scale plus semantic and neutral color, so a component theme
        // never has to reach into three separate places to build a border class.
        var border = new TwBlazorBorder
        {
            Width = borderWidth,
            Colors = borderColors,
            Neutral = new()
            {
                Base = neutralSurface.Border,
                Subtle = neutralSurface.BorderSubtle,
                Strong = neutralSurface.BorderStrong
            }
        };

        var positioning = new TwBlazorPositioning
        {
            Static = "static",
            Relative = "relative",
            Absolute = "absolute",
            Fixed = "fixed",
            Sticky = "sticky"
        };

        var inset = new TwBlazorInset
        {
            Top = "top-0",
            Bottom = "bottom-0",
            Start = "start-0",
            End = "end-0"
        };

        var textTransform = new TwBlazorTextTransform
        {
            Uppercase = "uppercase",
            Lowercase = "lowercase",
            Capitalize = "capitalize",
            NormalCase = "normal-case"
        };

        var transparentBackground = "bg-transparent";

        var overlayTheme = new TwOverlayTheme
        {
            DialogBackground = neutralSurface.Background,
            PopoverBackground = neutralSurface.Overlay,
            PopoverBorder = $"{borderWidth.Thin} {neutralSurface.BorderStrong}",
            TimeRangePopoverSize = $"w-56 {spacing.Padding.Md}",
            ColorPopoverSize = $"tw-color-picker-dialog w-64 {spacing.Padding.Lg}",
            TooltipWrapper = $"{positioning.Relative} {display.InlineFlex} group",
            TooltipBubble = $"{positioning.Absolute} bottom-full left-1/2 z-130 mb-2 -translate-x-1/2 px-2 py-1 whitespace-nowrap {typography.Size.Xs} {typography.Weight.Medium} {neutralText.Heading} invisible group-hover:visible group-has-[:focus-visible]:visible"
        };

        #endregion

        return new TwBlazorTheme
        {
            Anchor = anchor,
            Position = positioning,
            Inset = inset,
            Display = display,
            Flexbox = flexbox,
            Spacing = spacing,
            Sizing = sizing,
            Interaction = interaction,
            Border = border,
            TextTransform = textTransform,
            Transition = transition,
            Typography = typography,
            Overflow = overflow,
            Colors = new()
            {
                TextColors = text,
                DarkTextColors = darkText,
                HoverColors = hoverColors,
                Transparent = transparentBackground,
                LightBackground = background,
                DarkBackground = darkBackground,
                FocusRingBase = "focus-visible:ring-2",
                FocusColors = new()
                {
                    Primary = "focus:ring-purple-500/20",
                    Accent = "focus:ring-fuchsia-500/20",
                    Success = "focus:ring-green-500/20",
                    Danger = "focus:ring-red-500/20",
                    Warning = "focus:ring-yellow-500/20",
                    Info = "focus:ring-blue-500/20",
                    Light = "focus:ring-white/20",
                    Dark = "focus:ring-gray-900/20"
                },
                SurfaceColors = new()
                {
                    Filled = new()
                    {
                        Primary = $"{background.Medium.Primary} hover:bg-purple-700 active:bg-purple-800 {text.Medium.Light}",
                        Accent = "bg-fuchsia-700 hover:bg-fuchsia-800 active:bg-fuchsia-900 text-gray-100",
                        Success = $"{background.Medium.Success} hover:bg-green-700 active:bg-green-800 text-white",
                        Danger = "bg-red-700 hover:bg-red-800 active:bg-red-900 text-gray-100",
                        Warning = $"{background.Medium.Warning} hover:bg-yellow-600 active:bg-yellow-700 {text.Medium.Dark}",
                        Info = $"{background.Medium.Info} hover:bg-blue-700 active:bg-blue-800 {text.Medium.Light}",
                        Light = $"{background.Medium.Light} hover:bg-gray-50 active:bg-gray-100 {text.Medium.Dark}",
                        Dark = $"{background.Dark.Dark} hover:bg-gray-800 active:bg-gray-700 {text.Medium.Light}",
                    },
                    Outlined = new()
                    {
                        Primary = $"{text.Medium.Primary} {darkText.Light.Primary} {transparentBackground} {hoverColors.Primary} {borderWidth.Medium} {borderColors.Primary}",
                        Accent = $"{text.Medium.Accent} {darkText.Light.Accent} {transparentBackground} {hoverColors.Accent} {borderWidth.Medium} {borderColors.Accent}",
                        Success = $"{text.Medium.Success} {darkText.Light.Success} {transparentBackground} {hoverColors.Success} {borderWidth.Medium} {borderColors.Success}",
                        Danger = $"{text.Medium.Danger} {darkText.Light.Danger} {transparentBackground} {hoverColors.Danger} {borderWidth.Medium} {borderColors.Danger}",
                        Warning = $"{text.Medium.Warning} {darkText.Light.Warning} {transparentBackground} {hoverColors.Warning} {borderWidth.Medium} {borderColors.Warning}",
                        Info = $"{text.Medium.Info} {darkText.Light.Info} {transparentBackground} {hoverColors.Info} {borderWidth.Medium} {borderColors.Info}",
                        Light = $"{text.Light.Dark} {transparentBackground} {hoverColors.Light} {borderWidth.Medium} {borderColors.Light}",
                        Dark = $"{text.Medium.Dark} {transparentBackground} {hoverColors.Dark} {borderWidth.Medium} {borderColors.Dark}",
                    },
                    Text = new()
                    {
                        Primary = $"{text.Medium.Primary} {darkText.Light.Primary} {transparentBackground} {hoverColors.Primary}",
                        Accent = $"{text.Medium.Accent} {darkText.Light.Accent} {transparentBackground} {hoverColors.Accent}",
                        Success = $"{text.Medium.Success} {darkText.Light.Success} {transparentBackground} {hoverColors.Success}",
                        Danger = $"{text.Medium.Danger} {darkText.Light.Danger} {transparentBackground} {hoverColors.Danger}",
                        Warning = $"{text.Medium.Warning} {darkText.Light.Warning} {transparentBackground} {hoverColors.Warning}",
                        Info = $"{text.Medium.Info} {darkText.Light.Info} {transparentBackground} {hoverColors.Info}",
                        Light = $"{text.Light.Dark} {transparentBackground} {hoverColors.Light}",
                        Dark = $"{text.Dark.Dark} {transparentBackground} {hoverColors.Dark}",
                    },
                },
                NeutralSurface = neutralSurface,
                NeutralText = neutralText
            },
            Shadows = shadows,
            Rounded = rounded,
            Components =
            [
                overlayTheme,
                new TwAlertTheme
                {
                    Colors = new()
                    {
                        Primary = $"{background.Light.Primary} {darkBackground.Dark.Primary} {text.Dark.Primary} {darkText.Light.Primary} {borderWidth.AccentEdge} {borderColors.Primary}",
                        Accent = $"{background.Light.Accent} {darkBackground.Dark.Accent} {text.Dark.Accent} {darkText.Light.Accent} {borderWidth.AccentEdge} {borderColors.Accent}",
                        Success = $"{background.Light.Success} {darkBackground.Dark.Success} {text.Dark.Success} {darkText.Light.Success} {borderWidth.AccentEdge} {borderColors.Success}",
                        Danger = $"{background.Light.Danger} {darkBackground.Dark.Danger} {text.Dark.Danger} {darkText.Light.Danger} {borderWidth.AccentEdge} {borderColors.Danger}",
                        Warning = $"{background.Light.Warning} {darkBackground.Dark.Warning} {text.Dark.Warning} {darkText.Light.Warning} {borderWidth.AccentEdge} {borderColors.Warning}",
                        Info = $"{background.Light.Info} {darkBackground.Dark.Info} {text.Dark.Info} {darkText.Light.Info} {borderWidth.AccentEdge} {borderColors.Info}",
                        Light = $"{background.Light.Light} {darkBackground.Dark.Light} {text.Medium.Dark} {darkText.Medium.Dark} {borderWidth.AccentEdge} {borderColors.Light}",
                        Dark = $"{background.Light.Dark} {darkBackground.Dark.Dark} {text.Medium.Dark} {darkText.Light.Dark} {borderWidth.AccentEdge} {borderColors.Dark}",
                    },
                    TextWrapper = $"{flexbox.Flex1} {sizing.MinWidthNone} {typography.WrapBreakWord}",
                    Padding = "py-4 px-6",
                    DensePadding = "py-2 px-3",
                    StartIconPadding = "pr-2",
                    EndIconPadding = "pl-2",
                    Transition = transition.ColorsSlow,
                    DismissButtonSize = $"{sizing.Icon.Xl} {rounded.Full}",
                    DismissButtonSpacingWithEndIcon = "ml-2",
                    DismissButtonColor = $"text-current hover:bg-white hover:bg-opacity-20 dark:hover:bg-gray-800 dark:hover:bg-opacity-20 opacity-60 hover:opacity-100 focus:ring-2 focus:ring-current focus:ring-opacity-50 transition-[opacity,background-color] {transition.DurationFast} {interaction.FocusOutlineNone}"
                },
                new TwBreadcrumbTheme
                {
                    List = $"{display.InlineFlex} {flexbox.Wrap} {spacing.Gap.Lg}",
                    Item = $"{display.Flex} {flexbox.Align.Center} {spacing.Gap.Md}",
                    Separator = $"font-bold {neutralText.Subtle}",
                    Label = typography.WrapBreakWord
                },
                new TwButtonTheme
                {
                    Base = $"{display.InlineFlex} {overflow.Hidden} {anchor.Center} h-8 {typography.Size.Sm} {transition.ColorsFast} {interaction.FocusOutlineNone} focus-visible:outline-none touch-manipulation",
                    Padding = "px-6",
                    DensePadding = "px-3 py-1.5",
                    IconButton = $"{display.Flex} {anchor.Center} h-8 w-8 text-sm/6 {rounded.Full} {interaction.FocusOutlineNone}",
                    Typography = typography.Weight.Medium,
                    IconTypography = $"{typography.Size.Lg} leading-none",
                    Uppercase = $"{textTransform.Uppercase} tracking-wide",
                    DisabledCursor = interaction.DisabledCursor,
                    ReadonlyCursor = interaction.ReadonlyCursor,
                    DefaultCursor = interaction.PointerCursor,
                    DisabledFilled = $"bg-[oklch(21%_0.006_285.885)]/15 dark:bg-[oklch(97.807%_0.029_256.847)]/15 text-[oklch(21%_0.006_285.885)]/40 dark:text-[oklch(97.807%_0.029_256.847)]/40 shadow-none {interaction.DisabledCursor}",
                    DisabledOutlined = $"text-[oklch(21%_0.006_285.885)]/40 dark:text-[oklch(97.807%_0.029_256.847)]/40 {transparentBackground} {borderWidth.Thin} border-[oklch(21%_0.006_285.885)]/15 dark:border-[oklch(97.807%_0.029_256.847)]/15 {interaction.DisabledCursor}",
                    DisabledText = $"text-[oklch(21%_0.006_285.885)]/40 dark:text-[oklch(97.807%_0.029_256.847)]/40 {transparentBackground} {interaction.DisabledCursor}"
                },
                new TwCardTheme
                {
                    Container = "px-6 py-5",
                    Bordered = $"{borderWidth.Thin} {neutralSurface.Border}",
                    Title = $"{typography.Size.Lg} {typography.Weight.Semibold} {typography.WrapBreakWord} {neutralText.Heading}"
                },
                new TwCarouselTheme
                {
                    Container = $"{positioning.Relative} {overflow.Hidden} {sizing.FullWidth}",
                    Viewport = $"{positioning.Relative} {sizing.Full}",
                    Slide = sizing.Full,
                    SlideContent = sizing.Full,
                    ArrowButton = $"{positioning.Absolute} top-1/2 -translate-y-1/2 z-10 text-lg {shadows.Md}",
                    ArrowButtonStart = "start-2",
                    ArrowButtonEnd = "end-2",
                    PlayPauseButton = $"{positioning.Absolute} top-2 end-2 z-10 text-sm {shadows.Md}",
                    IndicatorContainer = $"{positioning.Absolute} bottom-2 inset-x-0 z-10 w-fit mx-auto {display.Flex} {flexbox.Justify.Center} {flexbox.Align.Center} {spacing.Gap.Sm} px-3 py-1.5 {rounded.Full} bg-white/45 dark:bg-black/60 {shadows.Md}",
                    Indicator = $"size-2.5 {rounded.Full} {interaction.PointerCursor} focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-blue-500 {transition.ColorsFast}",
                    IndicatorActive = "bg-purple-600 dark:bg-purple-500",
                    IndicatorInactive = "bg-[oklch(21%_0.006_285.885)]/20 dark:bg-[oklch(97.807%_0.029_256.847)]/25 hover:bg-[oklch(21%_0.006_285.885)]/40 dark:hover:bg-[oklch(97.807%_0.029_256.847)]/40"
                },
                new TwCheckboxTheme
                {
                    Colors = checkBoxRadioButtonColors,
                    Base = $"peer {borderWidth.Thick} border-[oklch(95%_0_0)] dark:border-[oklch(21.15%_0.012_254.09)] {transition.ColorsFast} {transition.EaseInOut} {interaction.PointerCursor} appearance-none",
                    Disabled = $"{interaction.DisabledOpacity} {interaction.DisabledCursor}",
                    Hover = $"hover:border-[oklch(21%_0.006_285.885)]/40 dark:hover:border-[oklch(97.807%_0.029_256.847)]/40 {interaction.PointerCursor}",
                    LabelBase = $"{display.Flex} {positioning.Relative} {flexbox.Align.Center} {spacing.Gap.Md} min-h-[24px] {inputLabelText} select-none",
                    LabelInteractiveCursor = interaction.PointerCursor,
                    LabelNonInteractiveCursor = interaction.PointerEventsNone,
                    LabelDisabled = interaction.DisabledOpacity,
                    IconWrapper = $"{positioning.Absolute} opacity-0 peer-checked:opacity-100 translate-x-1/4",
                    IndeterminateIconWrapper = $"{positioning.Absolute} translate-x-1/4"
                },
                new TwChipTheme
                {
                    Base = $"{display.InlineFlex} {anchor.Center} gap-1.5 {typography.Weight.Medium} {shadows.Sm} {transition.ColorsFast} touch-manipulation",
                    CloseButton = $"{display.Flex} {anchor.Center} {sizing.Icon.Sm} {typography.AlignCenter} hover:bg-[oklch(21%_0.006_285.885)]/10 dark:hover:bg-[oklch(97.807%_0.029_256.847)]/10 {rounded.Full}",
                    Sm = "h-5 px-1.5 py-1 text-[10px] leading-none",
                    Md = $"h-6 px-2 py-1 {typography.Size.Xs} leading-none",
                    Lg = $"h-8 px-2.5 py-1.5 {typography.Size.Sm} leading-none"
                },
                new TwCodeBlockTheme
                {
                    Container = $"{overflow.Hidden} p-1 {neutralSurface.BackgroundSubtle} {borderWidth.Thin} {neutralSurface.Border}",
                    Header = $"px-2 py-1 {typography.Size.Xs} {typography.Weight.Medium} tracking-wide {neutralText.Muted}",
                    Panel = $"{rounded.Md} {typography.Size.Sm} {neutralSurface.Background} {neutralText.Heading} {borderWidth.Thin} {neutralSurface.Border}",
                    CopyButtonWrapper = $"{neutralText.Muted} hover:text-[oklch(21%_0.006_285.885)] dark:hover:text-[oklch(97.807%_0.029_256.847)]"
                },
                new TwCollapseTheme
                {
                    Container = $"tw-collapse {borderWidth.Thin} {neutralSurface.Border}",
                    Trigger = $"{display.Flex} {flexbox.Align.Center} {flexbox.Justify.Between} {spacing.Gap.Md} {sizing.FullWidth} {spacing.InteractiveRowPadding} text-left {typography.Weight.Medium} {neutralText.Heading} {neutralSurface.Hover} focus-visible:ring-2 focus-visible:ring-inset focus-visible:ring-blue-500 {transition.ColorsFast} {interaction.FocusOutlineNone} touch-manipulation",
                    Icon = $"{display.Flex} {flexbox.ShrinkNone} {sizing.Icon.Sm} {spacing.PushEnd} {transition.TransformSlow}",
                    IconOpen = "rotate-180",
                    Content = $"{overflow.Hidden} border-t {neutralSurface.Border} {transition.ColorsSlow}"
                },
                new TwColorPickerTheme
                {
                    Swatch = $"block {flexbox.ShrinkNone} h-7 w-7 ring-1 ring-inset ring-[oklch(21%_0.006_285.885)]/10 dark:ring-[oklch(97.807%_0.029_256.847)]/15 {shadows.Sm} transition-[box-shadow,opacity] {transition.DurationFast}",
                    SwatchDisabled = interaction.DisabledOpacity,
                    SwatchHover = "hover:ring-[oklch(21%_0.006_285.885)]/20 dark:hover:ring-[oklch(97.807%_0.029_256.847)]/25",
                    InputContainer = $"{display.Flex} {flexbox.Align.Center} {spacing.Gap.Md}",
                    DialogPosition = $"{positioning.Fixed} z-120 mt-2",
                    PreviewSwatch =$"{flexbox.Flex1} {sizing.MinWidthNone} h-6 {rounded.Full} ring-1 ring-inset ring-[oklch(21%_0.006_285.885)]/10 dark:ring-[oklch(97.807%_0.029_256.847)]/15 {shadows.Sm}",
                    SelectorSquare = $"{positioning.Relative} {overflow.Hidden} {sizing.FullWidth} h-48 ring-1 ring-inset ring-[oklch(21%_0.006_285.885)]/10 dark:ring-[oklch(97.807%_0.029_256.847)]/10 cursor-crosshair touch-none",
                    SelectorThumb = $"{positioning.Absolute} {sizing.Icon.Sm} {rounded.Full} border-2 border-white ring-1 ring-black/10 {shadows.Lg} {interaction.PointerEventsNone}",
                    SliderTrack = $"{overflow.Hidden} {sizing.FullWidth} h-2.5 {rounded.Full} ring-1 ring-inset ring-[oklch(21%_0.006_285.885)]/10 dark:ring-[oklch(97.807%_0.029_256.847)]/10 {interaction.PointerEventsNone}",
                    SliderThumb = $"{positioning.Absolute} top-1/2 {sizing.Icon.Sm} {rounded.Full} border-2 border-white ring-1 ring-black/10 {shadows.Lg} {interaction.PointerEventsNone}",
                    AlphaLabel = $"{typography.Size.Xs} {typography.Weight.Medium} {neutralText.Secondary}",
                    ActionBar = $"{display.Flex} {flexbox.Justify.End} {spacing.Gap.Md} pt-1",
                    ControlRow = $"{display.Flex} {flexbox.Align.Center} {spacing.Gap.Md} {spacing.Padding.Md} {neutralSurface.BackgroundSubtle}",
                    Body = $"{display.Flex} {flexbox.Col} {spacing.Gap.Lg}",
                    SliderRow = $"{flexbox.Flex1} h-6",
                    HueSliderTouch = "touch-none",
                    InputColumn = flexbox.Flex1,
                    ModeSwitchButton = "shrink-0 tracking-wide",
                    AlphaReadoutAlign = "text-right"
                },
                new TwDatePickerTheme
                {
                    Header = $"{typography.AlignCenter} {typography.Weight.Medium} {overlayTheme.PopoverBackground} {rounded.RoundedTop.Lg} border-b {neutralSurface.BorderStrong}",
                    WeekdaysHeader = $"{display.Flex} {anchor.Center} h-8 {typography.Size.Xs} {typography.Weight.Semibold} tracking-wide {text.Medium.Primary} {darkText.Light.Primary}",
                    Base = $"{positioning.Fixed} z-120 {flexbox.Row} md:flex-row {flexbox.Align.Center} mt-1 px-2 pb-2 {typography.AlignCenter} {typography.Size.Sm} {typography.Weight.Medium} {transition.ColorsFast} {interaction.PointerCursor}",
                    ActiveClass = "bg-purple-50 dark:bg-purple-500/30",
                    ButtonClass = $"{display.Flex} {flexbox.Align.Center} {flexbox.Justify.Center} h-8 {sizing.FullWidth} {spacing.Padding.Md}",
                    RangeClass = "bg-purple-100 dark:bg-purple-500/20",
                    RangeMonthCaptionClass = $"mt-2 {typography.AlignCenter} {typography.Size.Sm} {typography.Weight.Semibold} {text.Medium.Primary} {darkText.Light.Primary}",
                    PrevMonthClass = "text-gray-400 dark:text-gray-600",
                    DefaultFormat = "dd/MM/yyyy",
                    DefaultDateTimeFormat = "dd/MM/yyyy HH:mm",
                    DefaultDateTimeFormat12Hour = "dd/MM/yyyy hh:mm tt",
                    DefaultRangeSeparator = " - ",
                    IconTriggerWrapper = $"{positioning.Absolute} {inset.Top} {inset.Start} {display.Flex} {flexbox.Align.Center} {spacing.PaddingStart.Lg} {interaction.PointerCursor}",
                    IconGlyph = $"{sizing.Icon.Md} {neutralText.Subtle}",
                    TextfieldPadding = "pl-10 pr-3",
                    NativeInputAppearance = "appearance-none !flex items-center [&::-webkit-date-and-time-value]:min-h-0 [&::-webkit-date-and-time-value]:text-left [&::-webkit-datetime-edit]:p-0",
                    PanelWidth = "w-67",
                    HeaderControls = $"{display.Flex} {flexbox.Justify.Between} {flexbox.Align.Center} h-9",
                    Body = display.Flex,
                    YearsGrid = $"{display.Grid} grid-cols-5 justify-items-stretch {spacing.Gap.Sm} {sizing.FullWidth} {spacing.MarginTop.Md}",
                    MonthsGrid = $"{display.Grid} grid-cols-6 justify-items-stretch {spacing.Gap.Sm} {sizing.FullWidth} {spacing.MarginTop.Md}",
                    DualMonthLayout = $"{display.Flex} {flexbox.Col} md:flex-row {spacing.Gap.Xl}",
                    DualMonthColumn = "md:flex-1 md:min-w-0",
                    DayGrid = $"{sizing.FullWidth} border-collapse",
                    DayHeaderCellPadding = "p-0",
                    DayAbbreviation = "no-underline",
                    RangeStageTabsContainer = $"{display.Flex} {spacing.Gap.Sm} {spacing.PaddingTop.Md} {spacing.MarginBottom.Md}",
                    RangeStageTabInactive = neutralText.Subtle,
                    PanelMaxHeight = "overflow-y-auto max-h-[calc(100vh-2rem)]",
                    PanelMaxWidth = "overflow-x-auto max-w-[calc(100vw-2rem)]",
                    DualMonthPanelWidth = "md:w-138",
                    StageTabBase = $"{flexbox.Flex1} py-1 {typography.Size.Xs} {typography.Weight.Medium} {typography.AlignCenter} {interaction.PointerCursor}",
                    HeaderNavButton = "py-2 px-4 text-xl"
                },
                new TwFileUploadTheme
                {
                    IconSpacing = "me-2",
                    FileList = $"{typography.Size.Sm} text-gray-600 dark:text-gray-400",
                    ChipTextColor = "dark:text-white"
                },
                new TwIconTheme
                {
                    Colors = new()
                    {
                        Primary = "text-purple-600 dark:text-purple-400",
                        Accent = "text-fuchsia-600 dark:text-fuchsia-400",
                        Success = "text-green-600 dark:text-green-400",
                        Danger = "text-red-600 dark:text-red-400",
                        Warning = "text-[oklch(65%_0.15_80)] dark:text-yellow-400",
                        Info = "text-blue-600 dark:text-blue-400",
                        Light = "text-gray-100 dark:text-white",
                        Dark = "text-gray-950 dark:text-gray-950",
                    },
                    HoverBackground = "hover:bg-current/10 dark:hover:bg-current/15",
                    Pulse = "relative tw-icon-pulse"
                },
                new TwLinkTheme
                {
                    Default = $"underline-offset-2 hover:underline {transition.ColorsFast}"
                },
                new TwGroupsTheme
                {
                    Gap = spacing.Gap.Md,
                    FieldsetBase = "p-0 m-0 border-none",
                    HorizontalLayout = $"{display.Flex} {flexbox.Row} {flexbox.Wrap}",
                    VerticalLayout = $"{display.Flex} {flexbox.Col}",
                    CheckboxGroupDisabled = $"{interaction.DisabledOpacity} {interaction.PointerEventsNone}",
                    RadioGroupDisabled = $"{interaction.DisabledOpacity} {interaction.PointerEventsNone}",
                    ButtonGroupBase = display.InlineFlex,
                    ButtonGroupVertical = flexbox.Col,
                    ButtonGroupHorizontal = flexbox.Row,
                    ButtonGroupFullWidth = sizing.FullWidth,
                    ButtonGroupFullWidthRow = "[&>*]:flex-1",
                    ChipGroupBase = $"{display.Flex} {flexbox.Wrap}",
                    ChipGroupAlignStart = flexbox.Justify.Start,
                    ChipGroupAlignCenter = flexbox.Justify.Center,
                    ChipGroupAlignEnd = flexbox.Justify.End
                },
                new TwDialogTheme
                {
                    Backdrop = $"{positioning.Fixed} inset-0 z-[110] {display.Flex} overflow-y-auto overscroll-contain {spacing.Padding.Xl} bg-black/50",
                    Surface = $"{positioning.Relative} {display.Flex} {flexbox.Col} {sizing.FullWidth} max-h-[calc(100vh-2rem)] {neutralText.Heading} {overlayTheme.DialogBackground}",
                    Header = $"{display.Flex} {flexbox.Align.Center} {flexbox.Justify.Between} {spacing.Gap.Xl} {flexbox.ShrinkNone} {spacing.InteractiveRowPadding} {neutralSurface.Border}",
                    Title = $"{typography.Size.Lg} {typography.Weight.Semibold}",
                    CloseButton = string.Empty,
                    Content = $"overflow-y-auto {spacing.InteractiveRowPadding}",
                    FullScreen = $"{sizing.Full} max-w-none",
                    FullWidth = sizing.FullWidth,
                    SmallWidth = "sm:max-w-lg",
                    MediumWidth = "sm:max-w-xl",
                    LargeWidth = "sm:max-w-3xl"
                },
                new TwInputTheme
                {
                    DefaultInputVariant = InputVariant.Filled,
                    TextfieldBase = $"{display.Block} {sizing.FullWidth} {sizing.MinWidthNone} max-w-full {neutralText.Heading} placeholder:text-[oklch(21%_0.006_285.885)]/50 dark:placeholder:text-[oklch(97.807%_0.029_256.847)]/50 {transition.ColorsFast} {transition.EaseInOut} {interaction.FocusOutlineNone}",
                    SelectBase = $"{display.Block} {sizing.FullWidth} pr-10 {neutralText.Heading} bg-[url('data:image/svg+xml;charset=utf-8,%3Csvg%20xmlns%3D%22http%3A%2F%2Fwww.w3.org%2F2000%2Fsvg%22%20fill%3D%22none%22%20viewBox%3D%220%200%2020%2020%22%3E%3Cpath%20stroke%3D%22%236b7280%22%20stroke-linecap%3D%22round%22%20stroke-linejoin%3D%22round%22%20stroke-width%3D%221.5%22%20d%3D%22m6%208%204%204%204-4%22%2F%3E%3C%2Fsvg%3E')] bg-[length:1.5em_1.5em] bg-[right_0.5rem_center] bg-no-repeat {transition.ColorsFast} appearance-none {interaction.FocusOutlineNone} supports-[appearance:base-select]:[appearance:base-select] supports-[appearance:base-select]:flex supports-[appearance:base-select]:items-center [&::picker(select)]:[appearance:base-select] [&::picker-icon]:hidden [&::picker(select)]:border-[oklch(21%_0.006_285.885)]/25 dark:[&::picker(select)]:border-[oklch(97.807%_0.029_256.847)]/20 [&::picker(select)]:bg-[oklch(100%_0_0)] dark:[&::picker(select)]:bg-[oklch(40%_0.016_253)]",
                    Size = $"{inputHeight} {typography.Size.Base}",
                    DenseSize = $"{inputDenseHeight} {typography.Size.Sm}",
                    SelectOption = $"{spacing.Padding.Lg} dark:bg-gray-800 dark:text-white supports-[appearance:base-select]:[padding-inline:revert] supports-[appearance:base-select]:py-2 supports-[appearance:base-select]:bg-transparent dark:supports-[appearance:base-select]:bg-transparent supports-[appearance:base-select]:hover:bg-[oklch(97%_0_0)] dark:supports-[appearance:base-select]:hover:bg-[oklch(34%_0.018_253)] supports-[appearance:base-select]:checked:bg-purple-50 supports-[appearance:base-select]:checked:text-purple-700 dark:supports-[appearance:base-select]:checked:bg-purple-500/20 dark:supports-[appearance:base-select]:checked:text-purple-300",
                    SelectNativeBackground = "!bg-white dark:!bg-gray-800",
                    SelectDefaultPadding = "px-3",
                    SelectReadOnlyBackground = "!bg-none",
                    SelectMultiTriggerLayout = $"{display.Flex} {flexbox.Wrap} {flexbox.Align.Center} {spacing.Gap.Sm} min-h-10 py-0.5 {typography.Size.Base}",
                    SelectNativeMultiOverlay = $"{positioning.Absolute} inset-0 {sizing.Full} m-0 opacity-0 {interaction.PointerCursor} {interaction.FocusOutlineNone} touch-manipulation",
                    SelectMultiOpenButton = $"{flexbox.Flex1} min-w-[2rem] min-h-8 truncate text-left bg-transparent border-0 {interaction.FocusOutlineNone}",
                    SelectPanelPosition = $"{positioning.Fixed} z-120 {spacing.MarginTop.Sm}",
                    SelectPanelSurface = $"overflow-y-auto max-h-64 {spacing.Padding.Sm}",
                    SelectPanelItemText = "[&_label]:!text-[oklch(21%_0.006_285.885)] dark:[&_label]:!text-[oklch(97.807%_0.029_256.847)] [&_fieldset]:gap-0 [&_label]:w-full [&_label]:gap-2 [&_label]:px-2 [&_label]:py-2 [&_label]:text-base [&_input]:sr-only [&_label>span]:hidden [&_label::before]:content-['✓'] [&_label::before]:invisible [&_label::before]:w-4 [&_label::before]:shrink-0 [&_label::before]:text-center [&_label:has(input:checked)::before]:visible [&_label:hover]:bg-[oklch(97%_0_0)] dark:[&_label:hover]:bg-[oklch(34%_0.018_253)] [&_label:has(input:checked)]:bg-purple-50 dark:[&_label:has(input:checked)]:bg-purple-500/20 [&_label:has(input:checked)]:!text-purple-700 dark:[&_label:has(input:checked)]:!text-purple-300 [&_label:has(input:focus-visible)]:ring-2 [&_label:has(input:focus-visible)]:ring-inset [&_label:has(input:focus-visible)]:ring-blue-500",
                    InputLegendBase = $"mb-3 {typography.Size.Base} {typography.Weight.Medium} {neutralText.Secondary}",
                    LabelBase = $"{display.Block} mb-2 {inputLabelText}",
                    OutlinedBorder = $"border-1 {neutralSurface.BorderStrong}",
                    FilledBorder = $"border-b-1 {neutralSurface.BorderStrong}",
                    FocusBorder = "focus:border-purple-600 dark:focus:border-purple-500",
                    FilledBackgroundColor = neutralSurface.Elevated,
                    ErrorMessage = $"{spacing.MarginTop.Sm} {typography.Size.Xs} {text.Medium.Danger} {darkText.Medium.Danger}"
                },
                new TwPaginationTheme
                {
                    List = $"{display.Flex} {flexbox.Align.Stretch} {overflow.Hidden} {neutralSurface.Background} {borderWidth.Thin} {neutralSurface.Border} divide-x divide-[oklch(95%_0_0)] dark:divide-[oklch(21.15%_0.012_254.09)]",
                    Base = $"{display.Flex} {anchor.Center} tabular-nums select-none {transition.ColorsFast} focus-visible:ring-2 focus-visible:ring-inset focus-visible:ring-blue-500 {interaction.FocusOutlineNone}",
                    Size = $"{inputHeight} min-w-10 px-3 {typography.Size.Sm}",
                    DenseSize = $"{inputDenseHeight} min-w-8 px-2 {typography.Size.Xs}",
                    ActiveButton = $"{typography.Weight.Semibold} {text.Medium.Primary} {darkText.Medium.Primary} {background.Light.Primary} {darkBackground.Light.Primary} {interaction.PointerCursor}",
                    Buttons = $"{neutralText.Muted} hover:bg-purple-50 dark:hover:bg-purple-900/20 hover:text-[oklch(21%_0.006_285.885)] dark:hover:text-[oklch(97.807%_0.029_256.847)] {interaction.PointerCursor}"
                },
                new TwPickListTheme
                {
                    Container = $"{display.Flex} {flexbox.Align.Start} {spacing.Gap.Lg}",
                    Column = $"{display.Flex} {flexbox.Col} {spacing.Gap.Sm} {flexbox.Flex1} {sizing.MinWidthNone}",
                    ColumnHeader = $"{display.Flex} {flexbox.Align.Center} {flexbox.Justify.Between} {spacing.Gap.Sm} {spacing.MarginStart.Sm}",
                    Label = $"{typography.Size.Sm} {typography.Weight.Medium} {neutralText.Heading}",
                    ReorderButtons = $"{display.Flex} {flexbox.Align.Center} {spacing.Gap.Sm}",
                    TransferButtonColumn = $"{display.Flex} {flexbox.Col} {flexbox.Align.Center} {flexbox.Justify.Center} {spacing.Gap.Sm} {spacing.PaddingTop.Xl}",
                    ListBox = $"overflow-y-auto {sizing.FullWidth} h-64 {spacing.Padding.Sm} {neutralSurface.Background} {borderWidth.Thin} {neutralSurface.Border}",
                    ListBoxDisabled = $"{interaction.DisabledOpacity} {interaction.PointerEventsNone}",
                    Item = $"{display.Flex} {flexbox.Align.Center} {spacing.Gap.Md} {spacing.InteractiveRowPadding} {typography.Size.Sm} {neutralText.Heading} {neutralSurface.Hover} focus-visible:ring-2 focus-visible:ring-inset focus-visible:ring-blue-500 transition-colors duration-150 {interaction.PointerCursor} {interaction.FocusOutlineNone}",
                    ItemSelected = $"{typography.Weight.Semibold} bg-[oklch(95%_0_0)] dark:bg-[oklch(21.15%_0.012_254.09)]",
                    EmptyState = $"{spacing.InteractiveRowPadding} {typography.Size.Sm} italic {neutralText.Subtle}"
                },
                new TwProgressTheme
                {
                    Colors = new()
                    {
                        Primary = "[&::-webkit-progress-value]:bg-purple-600 dark:[&::-webkit-progress-value]:bg-purple-500 [&::-moz-progress-bar]:bg-purple-600 dark:[&::-moz-progress-bar]:bg-purple-500",
                        Accent = "[&::-webkit-progress-value]:bg-fuchsia-600 dark:[&::-webkit-progress-value]:bg-fuchsia-500 [&::-moz-progress-bar]:bg-fuchsia-600 dark:[&::-moz-progress-bar]:bg-fuchsia-500",
                        Success = "[&::-webkit-progress-value]:bg-green-600 dark:[&::-webkit-progress-value]:bg-green-500 [&::-moz-progress-bar]:bg-green-600 dark:[&::-moz-progress-bar]:bg-green-500",
                        Danger = "[&::-webkit-progress-value]:bg-red-600 dark:[&::-webkit-progress-value]:bg-red-500 [&::-moz-progress-bar]:bg-red-600 dark:[&::-moz-progress-bar]:bg-red-500",
                        Warning = "[&::-webkit-progress-value]:bg-yellow-500 dark:[&::-webkit-progress-value]:bg-yellow-400 [&::-moz-progress-bar]:bg-yellow-500 dark:[&::-moz-progress-bar]:bg-yellow-400",
                        Info = "[&::-webkit-progress-value]:bg-blue-600 dark:[&::-webkit-progress-value]:bg-blue-500 [&::-moz-progress-bar]:bg-blue-600 dark:[&::-moz-progress-bar]:bg-blue-500",
                        Light = "[&::-webkit-progress-value]:bg-white dark:[&::-webkit-progress-value]:bg-white [&::-moz-progress-bar]:bg-blue-600 dark:[&::-moz-progress-bar]:bg-blue-500",
                        Dark = "[&::-webkit-progress-value]:bg-gray-900 dark:[&::-webkit-progress-value]:bg-gray-900 [&::-moz-progress-bar]:bg-blue-600 dark:[&::-moz-progress-bar]:bg-blue-500"
                    },
                    Base = $"{display.Block} {overflow.Hidden} {sizing.FullWidth} {sizing.MinWidthNone} tabular-nums bg-[oklch(95%_0_0)] dark:bg-[oklch(21.15%_0.012_254.09)] {rounded.Full} border-none indeterminate:animate-pulse appearance-none [&::-webkit-progress-bar]:rounded-full [&::-webkit-progress-bar]:bg-transparent [&::-webkit-progress-value]:rounded-full [&::-webkit-progress-value]:transition-[width] [&::-webkit-progress-value]:{transition.DurationSlow} [&::-moz-progress-bar]:rounded-full [&::-moz-progress-bar]:transition-[width] [&::-moz-progress-bar]:{transition.DurationSlow}",
                    Small = "h-1.5",
                    Medium = "h-2.5",
                    Large = "h-4"
                },
                new TwTabTheme
                {
                    TabBase = $"{positioning.Relative} tracking-wide {typography.Weight.Medium} {typography.Size.Sm} {transition.ColorsSlow}",
                    TabPadding = "py-5 px-6",
                    TabDensePadding = "px-4",
                    ActiveIndicator = "after:absolute after:bottom-0 after:left-0 after:right-0 after:h-0.5 after:bg-current after:scale-x-100 after:transition-transform after:duration-300",
                    InactiveIndicator = "after:absolute after:bottom-0 after:left-0 after:right-0 after:h-0.5 hover:text-[oklch(21%_0.006_285.885)] dark:hover:text-[oklch(97.807%_0.029_256.847)] after:bg-current after:scale-x-0 hover:after:scale-x-100 after:transition-transform after:duration-300",
                    DisabledTab = $"{interaction.DisabledOpacity} {interaction.DisabledCursor}",
                    TabListContainer = $"{display.Flex} {flexbox.Wrap} border-t border-l border-r border-b {neutralSurface.Border} {shadows.Sm}",
                    PanelContainer = $"p-6 border-l border-r border-b {neutralSurface.Border}",
                    Background = neutralSurface.Background,
                },
                new TwSkeletonTheme
                {
                    Base = $"{display.Block} {positioning.Relative} {overflow.Hidden} bg-[oklch(95%_0_0)] dark:bg-[oklch(21.15%_0.012_254.09)]",
                    Text = $"{sizing.FullWidth} h-4",
                    Circle = "size-12",
                    Rectangle = $"{sizing.FullWidth} h-24",
                    MeasuringWrapper = "invisible",
                    Pulse = "animate-pulse",
                    Wave = "tw-skeleton-wave"
                },
                new TwSidebarTheme
                {
                    SkipLink = "focus:not-sr-only focus:absolute focus:top-2 focus:left-2 focus:z-50 focus:px-3 focus:py-2 focus:text-sm focus:font-medium focus:bg-white focus:text-gray-900 dark:focus:bg-gray-800 dark:focus:text-white focus:rounded-md focus:shadow-lg",
                    MobileOverlay = "inset-0 z-[90] lg:hidden bg-black/50",
                    MobileClosed = "-translate-x-full",
                    MobileOpen = "lg:relative translate-x-0",
                    NavigationList = "overflow-y-auto pb-6",
                    NavbarFixedInset = "left-0 right-0",
                    NavbarNavigationResponsive = "lg:flex lg:flex-1 lg:flex-row lg:w-auto lg:p-0 lg:bg-transparent lg:shadow-none",
                    NavbarNavigationExpanded = "flex flex-1",
                    NavigationItemToggleLabel = "mr-auto text-left",
                    NavigationItemToggleIcon = "transition-transform",
                    Navbar = $"{display.Flex} z-40 {flexbox.Align.Center} {flexbox.ShrinkNone} {sizing.FullWidth} min-h-[56px] {spacing.Padding.Lg} {background.Dark.Primary} {darkBackground.Dark.Primary} {shadows.Sm}",
                    NavbarContent = $"{display.Flex} {flexbox.Wrap} {flexbox.Align.Center} {spacing.Gap.Xl} {sizing.FullWidth}",
                    NavbarBrand = flexbox.ShrinkNone,
                    NavbarNavigation = $"{flexbox.Align.Center} {spacing.Gap.Md}",
                    NavbarActions = $"{display.Flex} {flexbox.Align.Center} {spacing.Gap.Md} {spacing.PushEnd}",
                    NavbarToggle = $"lg:hidden order-first {flexbox.ShrinkNone} text-xl text-white",
                    NavbarToggleIcon = "text-xl text-white",
                    NavbarLink = $"{display.InlineFlex} {flexbox.Align.Center} {spacing.Gap.Md} px-3 py-2 {typography.Size.Sm} {text.Light.Light} hover:bg-white/10 rounded-md {transition.Colors}",
                    NavbarLinkActive = $"{typography.Weight.Semibold} bg-white/15",
                    NavbarMobileMenu = $"{spacing.Padding.Lg} {background.Dark.Primary} {darkBackground.Dark.Primary} {shadows.Lg}",
                    Sidebar = $"z-[100] overflow-auto overscroll-contain {flexbox.ShrinkNone} h-dvh w-64 {spacing.Padding.Xl} {neutralSurface.Background} {shadows.Sm} {transition.TransformFast} {transition.EaseInOut}",
                    NavigationItemBase = $"{display.Flex} {flexbox.Align.Center} {spacing.Gap.Md} {sizing.MinWidthNone} {spacing.InteractiveRowPadding} {typography.Size.Sm} {neutralText.Secondary} {neutralSurface.Hover} focus-visible:ring-inset focus-visible:ring-2 focus-visible:ring-blue-500 {transition.ColorsFast} {interaction.FocusOutlineNone} {interaction.PointerCursor}",
                    NavigationItemActive = $"{typography.Weight.Semibold} bg-[oklch(95%_0_0)] dark:bg-[oklch(21.15%_0.012_254.09)] text-[oklch(21%_0.006_285.885)] dark:text-[oklch(97.807%_0.029_256.847)]",
                    NavigationDropdownContainer = neutralSurface.BackgroundSubtle,
                    NavigationItemActiveLevelDeep = $"border-l-2 {border.Neutral.Strong}",
                    NavigationDropdownContainerDeep = string.Empty,
                    NavigationGroupRailDeep = $"border-l-2 {border.Neutral.Strong}",
                    MainContent = $"overflow-y-auto left-0 {flexbox.Flex1} {sizing.FullWidth} transition-[margin] {transition.DurationFast} {transition.EaseInOut}",
                    MainContentRoot = $"{display.Flex} overflow-x-hidden {flexbox.Col} h-dvh {sizing.FullWidth} {transparentBackground} {neutralSurface.Background} {neutralText.Heading} transition-[margin] {transition.DurationSlow} {transition.EaseInOut}"
                },
                new TwTreeListTheme
                {
                    Container = $"{display.Flex} {flexbox.Col} {typography.Size.Sm}",
                    Group = $"{display.Flex} {flexbox.Col} mt-1 ml-4 pl-2 py-1 {neutralSurface.BackgroundSubtle}",
                    Row = $"group/row {display.Flex} {flexbox.Align.Center} {spacing.Gap.Md} {sizing.MinWidthNone} {spacing.InteractiveRowPadding} {typography.Size.Sm} {neutralText.Heading} group-focus-visible:ring-inset group-focus-visible:ring-2 group-focus-visible:ring-blue-500 {transition.ColorsFast} {interaction.PointerCursor}",
                    RowDisabled = $"{interaction.DisabledOpacity} {interaction.PointerEventsNone}",
                    ToggleSlot = $"{display.InlineFlex} {flexbox.ShrinkNone} {flexbox.Align.Center} {flexbox.Justify.Center} {sizing.Icon.Sm}",
                    ToggleIcon = $"{display.InlineFlex} {flexbox.ShrinkNone} {flexbox.Align.Center} {flexbox.Justify.Center} {sizing.Icon.Sm} {transition.TransformFast}",
                    ToggleIconOpen = "rotate-180 translate-y-px",
                    Label = "truncate group-hover/row:underline",
                    ItemIcon = $"{display.InlineFlex} {flexbox.ShrinkNone} {flexbox.Align.Center} {flexbox.Justify.Center} {sizing.Icon.Sm} {neutralText.Muted}",
                    CheckboxWrapper = $"{display.InlineFlex} {flexbox.Align.Center} {flexbox.ShrinkNone} h-4",
                    ItemOutline = "outline-none"
                },
                new TwRadioButtonTheme
                {
                    Colors = checkBoxRadioButtonColors,
                    Base = $"peer {sizing.Icon.Md} {rounded.Full} {borderWidth.Thick} border-[oklch(95%_0_0)] dark:border-[oklch(21.15%_0.012_254.09)] {transition.ColorsFast} {transition.EaseInOut} {interaction.PointerCursor} appearance-none",
                    Disabled = $"{interaction.DisabledOpacity} {interaction.DisabledCursor}",
                    Hover = $"hover:border-[oklch(21%_0.006_285.885)]/40 dark:hover:border-[oklch(97.807%_0.029_256.847)]/40 {interaction.PointerCursor}",
                    LabelBase = $"{display.Flex} {positioning.Relative} {flexbox.Align.Center} {spacing.Gap.Md} min-h-[24px] {inputLabelText} select-none",
                    LabelInteractiveCursor = interaction.PointerCursor,
                    LabelNonInteractiveCursor = interaction.PointerEventsNone,
                    LabelDisabled = interaction.DisabledOpacity,
                    IconWrapper = $"{positioning.Absolute} left-0 {display.Flex} {flexbox.Align.Center} {flexbox.Justify.Center} {sizing.Icon.Md} opacity-0 peer-checked:opacity-100 {interaction.PointerEventsNone}"
                },
                new TwSliderTheme
                {
                    Colors = new()
                    {
                        Primary = $"{background.Medium.Primary} {darkBackground.Medium.Primary}",
                        Accent = $"{background.Medium.Accent} {darkBackground.Medium.Accent}",
                        Success = $"{background.Medium.Success} {darkBackground.Medium.Success}",
                        Danger = $"{background.Medium.Danger} {darkBackground.Medium.Danger}",
                        Warning = $"{background.Medium.Warning} {darkBackground.Medium.Warning}",
                        Info = $"{background.Medium.Info} {darkBackground.Medium.Info}",
                        Light = $"{background.Medium.Light} {darkBackground.Medium.Light}",
                        Dark = $"{background.Dark.Dark} {darkBackground.Dark.Dark}",
                    },
                    Wrapper = $"{positioning.Relative} {display.Flex} {flexbox.Align.Center} {sizing.FullWidth} h-6 select-none",
                    Base = $"peer {positioning.Absolute} inset-0 z-20 {sizing.Full} m-0 {transparentBackground} appearance-none {interaction.PointerCursor} {interaction.FocusOutlineNone} focus-visible:outline-none touch-manipulation",
                    Track = $"{positioning.Absolute} inset-x-0 top-1/2 {overflow.Hidden} h-1.5 bg-[oklch(95%_0_0)] dark:bg-[oklch(21.15%_0.012_254.09)] {rounded.Full} -translate-y-1/2 {interaction.PointerEventsNone}",
                    Fill = sizing.FullHeight,
                    Thumb = $"{positioning.Absolute} top-1/2 z-10 {sizing.Icon.Md} bg-white dark:bg-gray-100 {rounded.Full} {borderWidth.Thick} -translate-x-1/2 -translate-y-1/2 shadow-md ring-1 ring-black/5 peer-hover:scale-110 peer-active:scale-95 transition-transform duration-100 ease-out {interaction.PointerEventsNone}",
                    Bubble = $"{positioning.Absolute} bottom-full z-10 mb-2 px-2 py-1 whitespace-nowrap text-xs font-medium tabular-nums bg-gray-900 dark:bg-gray-700 text-white rounded-md -translate-x-1/2 {shadows.Lg} opacity-0 scale-95 peer-hover:opacity-100 peer-hover:scale-100 peer-focus-visible:opacity-100 peer-focus-visible:scale-100 transition-[opacity,transform] duration-100 ease-out {interaction.PointerEventsNone}"
                },
                new TwStepperTheme
                {
                    HorizontalList = $"hidden sm:flex sm:items-start {sizing.FullWidth}",
                    VerticalList = $"{display.Flex} {flexbox.Col} {sizing.FullWidth}",
                    Step = $"{display.Flex} {flexbox.Col} {flexbox.Align.Center} {spacing.Gap.Sm} shrink-0 {typography.AlignCenter}",
                    VerticalStep = $"{display.Flex} {spacing.Gap.Lg} pb-8 last:pb-0",
                    VerticalIndicatorColumn = $"{display.Flex} {flexbox.Col} {flexbox.Align.Center}",
                    VerticalContentColumn = $"{display.Flex} {flexbox.Col} {spacing.Gap.Sm} {flexbox.Flex1} pt-1",
                    Connector = $"{flexbox.Flex1} h-0.5 mt-4 mx-2 border-t-2 {transition.ColorsSlow}",
                    VerticalConnector = $"{flexbox.Flex1} w-0 min-h-8 my-1 border-l-2 {transition.ColorsSlow}",
                    ConnectorNeutral = neutralSurface.Border,
                    Circle = $"{display.Flex} {anchor.Center} shrink-0 {sizing.Icon.Xl} {rounded.Full} {typography.Weight.Semibold} {typography.Size.Sm} {transition.ColorsFast} {interaction.FocusOutlineNone}",
                    CircleUpcoming = $"{neutralSurface.Background} {neutralText.Muted} border-2 {neutralSurface.BorderStrong}",
                    CircleDisabled = $"{interaction.DisabledOpacity} {interaction.DisabledCursor}",
                    CircleClickable = interaction.PointerCursor,
                    CircleIcon = "leading-none",
                    Label = $"{typography.Size.Sm} {typography.Weight.Medium} {neutralText.Heading}",
                    LabelUpcoming = $"{typography.Size.Sm} {typography.Weight.Medium} {neutralText.Muted}",
                    Description = $"{typography.Size.Xs} {neutralText.Subtle}",
                    Content = $"{spacing.MarginTop.Xl} {spacing.Padding.Xl} {borderWidth.Thin} {neutralSurface.Border} {rounded.Lg}",
                    VerticalContent = "pt-2",
                    MobileContainer = $"{display.Flex} sm:hidden {flexbox.Col} {spacing.Gap.Sm} {sizing.FullWidth}",
                    MobileLabel = $"{typography.Size.Sm} {typography.Weight.Medium} {neutralText.Heading}",
                    MobileProgressLabel = display.ScreenReaderOnly
                },
                new TwSwitchTheme
                {
                    Colors = new()
                    {
                        Primary = "peer-checked:bg-purple-600 dark:peer-checked:bg-purple-500",
                        Accent = "peer-checked:bg-fuchsia-600 dark:peer-checked:bg-fuchsia-500",
                        Success = "peer-checked:bg-green-600 dark:peer-checked:bg-green-500",
                        Danger = "peer-checked:bg-red-600 dark:peer-checked:bg-red-500",
                        Warning = "peer-checked:bg-yellow-600 dark:peer-checked:bg-yellow-500",
                        Info = "peer-checked:bg-blue-600 dark:peer-checked:bg-blue-500",
                        Light = "peer-checked:bg-white dark:peer-checked:bg-gray-300",
                        Dark = "peer-checked:bg-gray-900 dark:peer-checked:bg-gray-800",
                    },
                    Switch = $"{positioning.Absolute} top-1/2 start-0.5 {sizing.Icon.Md} bg-white {rounded.Full} -translate-y-1/2 {shadows.Md} ring-1 ring-black/15 peer-checked:translate-x-full {transition.TransformSlow} {transition.EaseInOut}",
                    Track = $"{positioning.Absolute} inset-0 bg-gray-300 dark:bg-[oklch(21.15%_0.012_254.09)] {rounded.Full} shadow-inner ring-1 ring-inset ring-black/10 dark:ring-white/10 peer-disabled:{interaction.DisabledOpacity} transition-[background-color,opacity] {transition.DurationSlow} {transition.EaseInOut} peer-disabled:pointer-events-none",
                    WrapperSize = "w-10 h-6",
                    Base = "peer sr-only",
                    LabelBase = $"{display.InlineFlex} {flexbox.Align.Center} {spacing.Gap.Md} {inputLabelText} select-none",
                    LabelInteractiveCursor = interaction.PointerCursor,
                    LabelNonInteractiveCursor = interaction.PointerEventsNone,
                    LabelDisabled = interaction.DisabledOpacity
                },
                new TwSpinnerTheme
                {
                    Colors = new()
                    {
                        Primary = "border-t-purple-600 dark:border-t-purple-500",
                        Accent = "border-t-fuchsia-600 dark:border-t-fuchsia-500",
                        Success = "border-t-green-600 dark:border-t-green-500",
                        Danger = "border-t-red-600 dark:border-t-red-500",
                        Warning = "border-t-yellow-500 dark:border-t-yellow-500",
                        Info = "border-t-blue-600 dark:border-t-blue-500",
                        Light = "border-t-white",
                        Dark = "border-t-gray-900",
                    },
                    Wrapper = $"{display.InlineFlex} {flexbox.Align.Center} {spacing.Gap.Md}",
                    Base = $"{display.InlineBlock} {rounded.Full} animate-spin",
                    Track = neutralSurface.Border,
                    LightTrack = "border-white/25",
                    DarkTrack = "border-gray-900/15",
                    Small = $"{sizing.Icon.Md} border-3",
                    Medium = $"{sizing.Icon.Xl} border-5",
                    Large = "size-12 border-5",
                    Label = $"{typography.Size.Sm} {neutralText.Muted}"
                },
                new TwTimePickerTheme
                {
                    PickerRoot = positioning.Relative,
                    IconWrapper = $"{positioning.Absolute} {inset.Top} {inset.Start} {display.Flex} {anchor.Center} w-10 ps-2 {interaction.PointerCursor}",
                    IconGlyph = $"{sizing.Icon.Md} {neutralText.Subtle}",
                    TextfieldPadding = "pl-10 pr-3",
                    NativeInputAppearance = "appearance-none !flex items-center [&::-webkit-date-and-time-value]:min-h-0 [&::-webkit-date-and-time-value]:text-left [&::-webkit-datetime-edit]:p-0",
                    PanelWrapper = $"{positioning.Absolute} top-full left-0 z-120 {spacing.MarginTop.Md}",
                    RangeStageTabsContainer =$"{display.Flex} {spacing.Gap.Sm} {spacing.MarginBottom.Md}",
                    RangeStageTabInactive = neutralText.Subtle,
                    StageTabBase = $"{flexbox.Flex1} py-1 {typography.Size.Xs} {typography.Weight.Medium} {typography.AlignCenter} {interaction.PointerCursor}",
                    BodySurface = $"{spacing.Padding.Xl} {typography.AlignCenter} {typography.Weight.Medium} {neutralText.Heading}",
                    BodyRoot = "",
                    BodyInner = $"{spacing.Padding.Md} {typography.AlignCenter} {typography.Weight.Medium} {neutralText.Heading}",
                    ContentRow = $"{display.Flex} {anchor.Center} {spacing.Gap.Lg}",
                    Column = $"{display.Flex} {flexbox.Col} {flexbox.Align.Center} {spacing.Gap.Sm}",
                    StepButton = $"{neutralText.Subtle} hover:text-purple-600 dark:hover:text-purple-400 {interaction.PointerCursor}",
                    NumberWrapper = $"{display.Flex} {flexbox.Align.Center} {flexbox.Justify.Center}",
                    NumberInput = $"w-12 py-1 {typography.AlignCenter} {typography.Size.Lg} {typography.Weight.Semibold} {neutralText.Heading} {transition.ColorsFast} {interaction.FocusOutlineNone}",
                    Separator = $"self-center px-1 {typography.Size.Lg} {typography.Weight.Semibold} {neutralText.Subtle}",
                    AmPmWrapper = $"{display.Flex} {flexbox.Align.Center} ml-2",
                    AmPmButtonClass = "min-w-12"
                },
                new TwTableTheme
                {
                    Wrapper = "overflow-auto",
                    ContainerClip = overflow.Hidden,
                    Base = $"{sizing.FullWidth} {typography.Size.Sm} text-left rtl:text-right {neutralText.Heading}",
                    Bordered = $"{borderWidth.Thin} {neutralSurface.Border}",
                    Header = $"{textTransform.Uppercase} {typography.Size.Xs} {typography.Weight.Semibold} tracking-wide {neutralSurface.BackgroundSubtle} border-b {neutralSurface.Border}",
                    Body = neutralSurface.Background,
                    BodyStriped = "[&>tr:nth-child(even)]:bg-[oklch(98%_0_0)] [&>tr:nth-child(even)]:dark:bg-[oklch(23.26%_0.014_253.1)]",
                    BodyHoverable = "[&>tr:hover]:!bg-purple-50 [&>tr:hover]:dark:!bg-purple-900/20 [&>tr]:transition-colors [&>tr]:duration-200",
                    RowDivider = "divide-y divide-[oklch(95%_0_0)] dark:divide-[oklch(21.15%_0.012_254.09)]",
                    Footer = $"border-t {neutralSurface.Border}",
                    SearchColumn = "md:w-1/2",
                    SortIcon = $"{typography.Size.Sm} text-gray-500 hover:text-gray-900 dark:text-gray-400 dark:hover:text-gray-200",
                    HeaderCellPadding = spacing.InteractiveRowPadding,
                    BodyCellPadding = $"{spacing.InteractiveRowPadding} {typography.WrapBreakWord}",
                    EmptyState = $"py-8 {typography.AlignCenter} text-gray-500 dark:text-gray-400"
                },
                new TwToastTheme
                {
                    Colors = new()
                    {
                        Primary = $"{background.Light.Primary} {darkBackground.Light.Primary} {text.Dark.Primary} {borderWidth.AccentEdge} {borderColors.Primary}",
                        Accent = $"{background.Light.Accent} {darkBackground.Light.Accent} {text.Dark.Accent} {borderWidth.AccentEdge} {borderColors.Accent}",
                        Success = $"{background.Light.Success} {darkBackground.Light.Success} {text.Dark.Success} {borderWidth.AccentEdge} {borderColors.Success}",
                        Danger = $"{background.Light.Danger} {darkBackground.Light.Danger} {text.Dark.Danger} {borderWidth.AccentEdge} {borderColors.Danger}",
                        Warning = $"{background.Light.Warning} {darkBackground.Light.Warning} {text.Dark.Warning} {borderWidth.AccentEdge} {borderColors.Warning}",
                        Info = $"{background.Light.Info} {darkBackground.Light.Info} {text.Dark.Info} {borderWidth.AccentEdge} {borderColors.Info}",
                        Light = $"{background.Light.Light} {darkBackground.Light.Light} {text.Dark.Dark} {borderWidth.AccentEdge} {borderColors.Light}",
                        Dark = $"{background.Dark.Dark} {text.Light.Light} {borderWidth.AccentEdge} {borderColors.Dark}"
                    },
                    HeaderClasses = $"{display.Flex} {flexbox.Col} {flexbox.Flex1} {spacing.Gap.Md} {sizing.MinWidthNone}",
                    IconContainer = flexbox.ShrinkNone,
                    Title = $"{typography.Weight.Semibold} {typography.Size.Sm} {typography.WrapBreakWord}",
                    Message = $"{typography.Size.Sm} {typography.WrapBreakWord}",
                    Container = $"{positioning.Fixed} bottom-0 right-4 z-50 {display.Flex} {flexbox.Col} {spacing.Gap.Md} max-w-md {spacing.Padding.Xl}",
                    Toast = $"{display.Flex} {flexbox.Align.Start} {spacing.Gap.Md} {spacing.Padding.Xl} {shadows.Sm} {transition.ColorsFast} {transition.EaseInOut}",
                    ToastWidth = "w-[300px] max-w-full",
                    Timestamp = $"{typography.Size.Xs} opacity-70",
                    CloseButton = $"{flexbox.ShrinkNone} {spacing.Padding.Sm} hover:bg-[oklch(21%_0.006_285.885)]/10 dark:hover:bg-[oklch(97.807%_0.029_256.847)]/10 {rounded.Full} focus:ring-2 focus:ring-offset-1 focus:ring-offset-transparent focus:ring-current/40 {transition.Colors} {interaction.FocusOutlineNone}"
                },
                new TwCalendarTheme
                {
                    Container = $"{display.Flex} {flexbox.Col} {sizing.FullWidth} {spacing.Gap.Lg}",
                    Header = $"{display.Flex} {flexbox.Align.Center} {flexbox.Justify.Between} {flexbox.Wrap} {spacing.Gap.Lg} pb-4 border-b {neutralSurface.Border}",
                    HeaderDateGroup = $"{display.Flex} {flexbox.Col} {spacing.Gap.Sm}",
                    HeaderTitleRow = $"{display.Flex} {flexbox.Align.Center} {spacing.Gap.Md}",
                    HeaderTitle = $"{typography.Size.Lg} {typography.Weight.Semibold} {neutralText.Heading}",
                    HeaderSubtitle = $"{typography.Size.Xs} {neutralText.Muted}",
                    NavButton = $"{display.Flex} {anchor.Center} size-8 {rounded.Full} {neutralSurface.Hover}",
                    ViewButtonActive = $"{background.Lightest.Primary} {darkBackground.Dark.Primary}",
                    AddButton = "shrink-0",
                    SearchButton = neutralText.Subtle,
                    ScrollContainer = "overflow-y-auto",
                    TimeGutter = $"{positioning.Relative} {display.Flex} {flexbox.Col} {flexbox.ShrinkNone} w-16 text-right {spacing.Padding.Sm}",
                    TimeGutterLabel = $"{typography.Size.Xs} {neutralText.Subtle} -translate-y-2",
                    NowIndicatorLabel = $"{positioning.Absolute} right-2 -translate-y-1/2 {typography.Size.Xs} {typography.Weight.Semibold} {text.Medium.Primary} {darkText.Light.Primary} {neutralSurface.Background} px-1 {rounded.Sm} {interaction.PointerEventsNone}",
                    NowIndicatorLine = $"{positioning.Absolute} inset-x-0 border-t-2 {borderColors.Primary} {interaction.PointerEventsNone} z-10",
                    NowIndicatorDot = $"{positioning.Absolute} -translate-x-1/2 -translate-y-1/2 size-2.5 {rounded.Full} {background.Medium.Primary} {interaction.PointerEventsNone} z-10",
                    SlotRow = $"h-12 border-t {neutralSurface.BorderSubtle}",
                    SlotRowHover = $"{neutralSurface.Hover} {interaction.PointerCursor}",
                    DayColumnHeader = $"{typography.Size.Xs} {typography.Weight.Semibold} {typography.AlignCenter} {neutralText.Secondary} py-2 border-b {neutralSurface.Border}",
                    DayColumnDivider = $"border-l {neutralSurface.BorderSubtle}",
                    EventChip = $"{positioning.Absolute} {overflow.Hidden} {rounded.Sm} {display.Flex} {flexbox.Col} {flexbox.Align.Start} {borderWidth.AccentEdge} {borderColors.Primary} {background.Lightest.Primary} {darkBackground.Dark.Primary} {text.Dark.Primary} {darkText.Light.Primary} {spacing.Padding.Sm} {interaction.PointerCursor} {transition.ColorsFast}",
                    EventChipTitle = $"{typography.Size.Xs} {typography.Weight.Semibold} {typography.WrapBreakWord}",
                    EventChipTime = $"{typography.Size.Xs} opacity-75",
                    EventChipReadOnly = $"{interaction.ReadonlyCursor} opacity-90",
                    EventChipDraggable = "cursor-grab active:cursor-grabbing",
                    DropPlaceholder = $"{overflow.Hidden} {rounded.Sm} border-2 border-dashed {borderColors.Primary} {background.Lightest.Primary} {darkBackground.Dark.Primary} {text.Dark.Primary} {darkText.Light.Primary} {typography.Size.Xs} opacity-70 z-10",
                    TodayHighlight = $"animate-pulse [animation-iteration-count:1] {background.Light.Primary} dark:bg-purple-500/50",
                    AllDayRow = $"border-b {neutralSurface.Border}",
                    AllDayLabel = $"{flexbox.ShrinkNone} w-16 {spacing.Padding.Sm} text-right {typography.Size.Xs} {neutralText.Subtle}",
                    AllDayGrid = "gap-y-1 py-1",
                    AllDayChipInset = "mx-0.5",
                    SegmentSpan = $"{positioning.Relative} z-10",
                    SegmentContinuesBefore = "rounded-l-none",
                    SegmentContinuesAfter = "rounded-r-none",
                    SegmentSpacer = $"h-5 {flexbox.ShrinkNone}",
                    MonthGrid = $"{sizing.FullWidth} border-collapse table-fixed",
                    MonthCell = $"align-top {spacing.Padding.Sm} border {neutralSurface.BorderSubtle} h-24",
                    MonthCellPrevNext = $"{neutralSurface.BackgroundSubtle} {neutralText.Subtle}",
                    MonthDayNumber = $"{display.Block} {sizing.FullWidth} text-left {typography.Size.Xs} {typography.Weight.Medium} {neutralText.Heading}",
                    MonthEventList = $"{display.Flex} {flexbox.Col} {spacing.Gap.Sm} mt-1",
                    MonthEventRow = $"{display.Flex} {flexbox.Align.Center} {spacing.Gap.Sm} {sizing.FullWidth} {overflow.Hidden} {rounded.Sm} {borderWidth.AccentEdge} {borderColors.Primary} {background.Lightest.Primary} {darkBackground.Dark.Primary} {text.Dark.Primary} {darkText.Light.Primary} px-1.5 py-0.5 {interaction.PointerCursor} {transition.ColorsFast}",
                    MonthEventName = $"{typography.Size.Xs} {typography.Weight.Semibold} truncate",
                    MonthEventTime = $"{typography.Size.Xs} opacity-75 {flexbox.ShrinkNone}",
                    MonthOverflowLabel = $"{typography.Size.Xs} {neutralText.Subtle} {interaction.PointerCursor}",
                    EventDialogFields = $"{display.Flex} {flexbox.Col} {spacing.Gap.Xl} -mt-3",
                    EventDialogActions = $"{display.Flex} {flexbox.Justify.End} {spacing.Gap.Md} {spacing.MarginTop.Xl}",
                    EventDialogAccent = $"h-1 {sizing.FullWidth} {rounded.Full} {background.Medium.Primary}",
                    EventDialogDateRow = $"{display.Grid} grid-cols-1 sm:grid-cols-2 {spacing.Gap.Xl}",
                    EventDialogSummary = $"{display.Flex} {flexbox.Align.Center} {spacing.Gap.Lg} {rounded.Md} {borderWidth.AccentEdge} {borderColors.Primary} {background.Lightest.Primary} {darkBackground.Dark.Primary} {neutralText.Heading} {spacing.Padding.Lg} {typography.Size.Sm}",
                    EventDialogSummaryIcon = $"{flexbox.ShrinkNone} {text.Medium.Primary} {darkText.Light.Primary}",
                    EventDialogReadOnlyField = $"{display.Flex} {flexbox.Col} {spacing.Gap.Sm}",
                    EventDialogReadOnlyLabel = $"{typography.Size.Xs} font-normal tracking-wide {neutralText.Muted}",
                    EventDialogReadOnlyValue = $"{typography.Size.Base} {neutralText.Heading} whitespace-pre-line"
                }
            ]
        };
    }
    #endregion

    public static TwBlazorTheme DefaultTheme { get; internal set; } = CreateDefaultTheme();
}

internal static class ThemeUpdateHandler
{
    internal static void UpdateApplication(Type[]? _) => Theme.DefaultTheme = Theme.CreateDefaultTheme();
}
