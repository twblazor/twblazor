// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using TwBlazor.Configuration;
using TwBlazor.Configuration.Color;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;

namespace TwBlazor.Docs.Pages.Showcase.Themes;

/// <summary>
/// An example theme inspired by the look of Lego's products. Bright and friendly: a yellow bar, orange pill buttons with dark text, blue links, bold type and flat white cards.
/// </summary>
/// <remarks>
/// This is a complete theme file, the same shape as the default <c>Theme.cs</c>, shown in full on the
/// <c>/showcase/lego</c> page. It is an independent example: twblazor is not affiliated with, endorsed by
/// or sponsored by the brand, and uses none of its assets.
/// </remarks>
public static class LegoTheme
{
    #region CodeExample ShowcaseLegoTheme
    public static TwBlazorTheme CreateTheme()
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
                Primary = "text-brickblue-200",
                Accent = "text-brickorange-200",
                Success = "text-green-200",
                Danger = "text-red-200",
                Warning = "text-yellow-200",
                Info = "text-blue-200",
                Light = "text-white",
                Dark = "text-gray-200"
            },
            Medium =
            {
                Primary = "text-brickblue-600",
                Accent = "text-brickorange-600",
                Success = "text-green-800",
                Danger = "text-red-700",
                Warning = "text-yellow-800",
                Info = "text-blue-600",
                Light = "text-gray-100",
                Dark = "text-gray-950",
            },
            Dark =
            {
                Primary = "text-brickblue-900",
                Accent = "text-brickorange-900",
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
                Primary = "dark:text-brickblue-200",
                Accent = "dark:text-brickorange-200",
                Success = "dark:text-green-200",
                Danger = "dark:text-red-200",
                Warning = "dark:text-yellow-200",
                Info = "dark:text-blue-200",
                Dark = "dark:text-gray-200"
            },
            Medium =
            {
                Primary = "dark:text-brickblue-600",
                Accent = "dark:text-brickorange-600",
                Success = "dark:text-green-600",
                Danger = "dark:text-red-600",
                Warning = "dark:text-yellow-600",
                Info = "dark:text-blue-600",
                Light = "dark:text-white",
                Dark = "dark:text-gray-950",
            },
            Dark =
            {
                Primary = "dark:text-brickblue-900",
                Accent = "dark:text-brickorange-900",
                Success = "dark:text-green-900",
                Danger = "dark:text-red-900",
                Warning = "dark:text-yellow-900",
                Info = "dark:text-blue-900",
                Dark = "dark:text-gray-900"
            }
        };

        var borderColors = new TwBlazorPalette
        {
            Primary = "border-brickblue-600 dark:border-brickblue-500",
            Accent = "border-brickorange-600 dark:border-brickorange-400",
            Success = "border-green-600 dark:border-green-400",
            Danger = "border-red-600 dark:border-red-400",
            Warning = "border-yellow-700 dark:border-yellow-400",
            Info = "border-blue-600 dark:border-blue-400",
            Light = "border-gray-100 dark:border-gray-700",
            Dark = "border-gray-900 dark:border-gray-700"
        };

        var hoverColors = new TwBlazorPalette
        {
            Primary = "hover:bg-brickblue-50 dark:hover:bg-brickblue-900/20",
            Accent = "hover:bg-brickorange-50 dark:hover:bg-brickorange-900/20",
            Success = "hover:bg-green-50 dark:hover:bg-green-900/20",
            Danger = "hover:bg-red-50 dark:hover:bg-red-900/20",
            Warning = "hover:bg-yellow-50 dark:hover:bg-yellow-900/20",
            Info = "hover:bg-blue-50 dark:hover:bg-blue-900/20",
            Light = "hover:bg-gray-100/10",
            Dark = "hover:bg-gray-900/10 dark:hover:bg-white/10"
        };

        var background = new TwBackgroundColor
        {
            Lightest = new()
            {
                Primary = $"bg-brickblue-50",
                Accent = $"bg-brickorange-50",
                Success = $"bg-green-50",
                Danger = $"bg-red-50",
                Warning = $"bg-yellow-50",
                Info = $"bg-blue-50",
                Light = $"bg-white",
                Dark = $"bg-gray-50"
            },
            Light = new()
            {
                Primary = $"bg-brickblue-200",
                Accent = $"bg-brickorange-200",
                Success = $"bg-green-200",
                Danger = $"bg-red-200",
                Warning = $"bg-yellow-200",
                Info = $"bg-blue-200",
                Light = $"bg-white",
                Dark = $"bg-gray-200"
            },
            Medium = new()
            {
                Primary = $"bg-brickblue-600",
                Accent = $"bg-brickorange-600",
                Success = $"bg-green-700",
                Danger = $"bg-red-600",
                Warning = $"bg-yellow-600",
                Info = $"bg-blue-600",
                Light = $"bg-gray-100",
                Dark = $"bg-gray-600",
            },
            Dark = new()
            {
                Primary = $"bg-brickblue-900",
                Accent = $"bg-brickorange-900",
                Success = $"bg-green-900",
                Danger = $"bg-red-900",
                Warning = $"bg-yellow-900",
                Info = $"bg-blue-900",
                Light = $"bg-gray-300",
                Dark = $"bg-gray-900"
            },
            Darkest = new()
            {
                Primary = $"bg-brickblue-950",
                Accent = $"bg-brickorange-950",
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
                Primary = $"dark:bg-brickblue-50",
                Accent = $"dark:bg-brickorange-50",
                Success = $"dark:bg-green-50",
                Danger = $"dark:bg-red-50",
                Warning = $"dark:bg-yellow-50",
                Info = $"dark:bg-blue-50",
                Dark = $"dark:bg-gray-50"
            },
            Light = new()
            {
                Primary = $"dark:bg-brickblue-200",
                Accent = $"dark:bg-brickorange-200",
                Success = $"dark:bg-green-200",
                Danger = $"dark:bg-red-200",
                Warning = $"dark:bg-yellow-200",
                Info = $"dark:bg-blue-200",
                Dark = $"dark:bg-gray-200",
            },
            Medium = new()
            {
                Primary = $"dark:bg-brickblue-600",
                Accent = $"dark:bg-brickorange-600",
                Success = $"dark:bg-green-600",
                Danger = $"dark:bg-red-600",
                Warning = $"dark:bg-yellow-600",
                Info = $"dark:bg-blue-600",
                Light = $"dark:bg-white",
                Dark = $"dark:bg-gray-600",
            },
            Dark = new()
            {
                Primary = $"dark:bg-brickblue-900",
                Accent = $"dark:bg-brickorange-900",
                Success = $"dark:bg-green-900",
                Danger = $"dark:bg-red-900",
                Warning = $"dark:bg-yellow-900",
                Info = $"dark:bg-blue-900",
                Dark = $"dark:bg-gray-900",
            },
            Darkest = new()
            {
                Primary = $"dark:bg-brickblue-950",
                Accent = $"dark:bg-brickorange-950",
                Success = $"dark:bg-green-950",
                Danger = $"dark:bg-red-950",
                Warning = $"dark:bg-yellow-950",
                Info = $"dark:bg-blue-950",
                Dark = $"dark:bg-black",
            }
        };

        var checkBoxRadioButtonColors = new TwBlazorPalette()
        {
            Primary = "checked:bg-brickblue-600 checked:border-brickblue-600 dark:checked:bg-brickblue-500 dark:checked:border-brickblue-500",
            Accent = "checked:bg-brickorange-600 checked:border-brickorange-600 dark:checked:bg-brickorange-500 dark:checked:border-brickorange-500",
            Success = "checked:bg-green-600 checked:border-green-600 dark:checked:bg-green-500 dark:checked:border-green-500",
            Danger = "checked:bg-red-600 checked:border-red-600 dark:checked:bg-red-500 dark:checked:border-red-500",
            Warning = "checked:bg-yellow-700 checked:border-yellow-700 dark:checked:bg-yellow-500 dark:checked:border-yellow-500",
            Info = "checked:bg-blue-600 checked:border-blue-600 dark:checked:bg-blue-500 dark:checked:border-blue-500",
            Light = "checked:bg-white checked:border-gray-900 dark:checked:bg-white dark:checked:border-gray-500",
            Dark = "checked:bg-gray-900 checked:border-gray-900 dark:checked:bg-gray-900 dark:checked:border-gray-900"
        };

        var neutralSurface = new TwSurfacePalette
        {
            Background = "bg-[oklch(100%_0_0)] dark:bg-[oklch(25.33%_0_0)]",
            BackgroundSubtle = "bg-[oklch(98%_0_0)] dark:bg-[oklch(23.26%_0_0)]",
            Border = "border-[oklch(92%_0_0)] dark:border-[oklch(21.15%_0_0)]",
            BorderSubtle = "border-[oklch(98%_0_0)] dark:border-[oklch(23.26%_0_0)]",
            Hover = "hover:bg-[oklch(98%_0_0)] dark:hover:bg-[oklch(23.26%_0_0)]",
            Elevated = "bg-[oklch(97%_0_0)] dark:bg-[oklch(34%_0_0)]",
            Overlay = "bg-[oklch(100%_0_0)] dark:bg-[oklch(40%_0_0)]",
            BorderStrong = "border-[oklch(15%_0_0)]/50 dark:border-[oklch(97.807%_0_0)]/45"
        };

        var neutralText = new TwNeutralTextPalette
        {
            Heading = "text-[oklch(15%_0_0)] dark:text-[oklch(97.807%_0_0)]",
            Secondary = "text-[oklch(40%_0_0)] dark:text-[oklch(88%_0_0)]",
            Muted = "text-[oklch(50%_0_0)] dark:text-[oklch(78%_0_0)]",
            Subtle = "text-[oklch(55%_0_0)] dark:text-[oklch(68%_0_0)]"
        };

        // One height for every single-line input (and the controls laid out beside them, e.g. pagination),
        // so they all line up whichever component renders them.
        var inputHeight = "h-12";
        var inputDenseHeight = "h-8";

        // Single focus language for every focusable surface: a 2px primary ring. The inset form is for rows
        // that sit flush against their container's edge and would otherwise have the ring clipped.
        var focusRing = "focus-visible:ring-2 focus-visible:ring-brickblue-500 dark:focus-visible:ring-brickblue-400";
        var focusRingInset = "focus-visible:ring-2 focus-visible:ring-inset focus-visible:ring-brickblue-500 dark:focus-visible:ring-brickblue-400";

        // Neutral fills reused wherever a component needs a recessed track, a selected row or a pressed state.
        var neutralTrack = "bg-[oklch(95%_0_0)] dark:bg-[oklch(21.15%_0_0)]";

        // A recessed track that stays visible on a white card: progress and slider tracks carry the value, so
        // they need more contrast than the page-chrome fill above.
        var neutralTrackStrong = "bg-[oklch(91%_0_0)] dark:bg-[oklch(32%_0_0)]";
        var neutralHoverText = "hover:text-[oklch(15%_0_0)] dark:hover:text-[oklch(97.807%_0_0)]";
        var neutralRowHover = "hover:bg-[oklch(15%_0_0)]/5 dark:hover:bg-[oklch(97.807%_0_0)]/8";
        var neutralHoverFill = "hover:bg-[oklch(15%_0_0)]/10 dark:hover:bg-[oklch(97.807%_0_0)]/10";
        var neutralHairlineRing = "ring-1 ring-inset ring-[oklch(15%_0_0)]/10 dark:ring-[oklch(97.807%_0_0)]/15";
        var disabledInk = "text-[oklch(15%_0_0)]/40 dark:text-[oklch(97.807%_0_0)]/40";

        var inputLabelText = $"text-sm font-bold {neutralText.Secondary}";

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
            PushEnd = "ms-auto",
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
            FocusOutlineNone = "focus:outline-hidden"
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
            Colors = "transition-colors motion-reduce:transition-none",
            Transform = "transition-transform motion-reduce:transition-none",
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
                Semibold = "font-bold"
            },
            AlignCenter = "text-center",
            WrapBreakWord = "wrap-break-word"
        };

        var rounded = new TwBlazorRounded
        {
            None = "rounded-none",
            Sm = "rounded-md",
            Md = "rounded-lg",
            Lg = "rounded-xl",
            Full = "rounded-full",
            DefaultRounded = Rounded.Md,
            RoundedTop = new()
            {
                None = "rounded-t-none",
                Sm = "rounded-t-md",
                Md = "rounded-t-lg",
                Lg = "rounded-t-xl",
                Full = "rounded-t-full"
            },
            RoundedBottom = new()
            {
                None = "rounded-b-none",
                Sm = "rounded-b-md",
                Md = "rounded-b-lg",
                Lg = "rounded-b-xl",
                Full = "rounded-b-full"
            },
            RoundedStart = new()
            {
                None = "rounded-s-none",
                Sm = "rounded-s-md",
                Md = "rounded-s-lg",
                Lg = "rounded-s-xl",
                Full = "rounded-s-full"
            },
            RoundedEnd = new()
            {
                None = "rounded-e-none",
                Sm = "rounded-e-md",
                Md = "rounded-e-lg",
                Lg = "rounded-e-xl",
                Full = "rounded-e-full"
            }
        };

        var shadows = new TwBlazorShadow
        {
            None = "shadow-none",
            Sm = "shadow-none",
            Md = "shadow-sm",
            Lg = "shadow-md",
            Xl = "shadow-lg",
            HoverSm = "hover:shadow-none",
            HoverMd = "hover:shadow-sm",
            HoverLg = "hover:shadow-lg",
            ActiveMd = "active:shadow-sm",
            DefaultShadow = Shadow.None
        };

        var borderWidth = new TwBlazorBorderWidth
        {
            None = "border-0",
            Thin = "border",
            Medium = "border-[1.2px]",
            Thick = "border-2",
            AccentEdge = "border-s-4"
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
            Popover = new TwPopoverTheme
            {
                Background = neutralSurface.Overlay,
                Border = $"{borderWidth.Thin} {neutralSurface.BorderStrong}",
                TimeRangeSize = $"w-56 {spacing.Padding.Md}",
                ColorSize = $"tw-color-picker-dialog w-64 {spacing.Padding.Lg}"
            },
            Dialog = new TwDialogTheme
            {
                Background = neutralSurface.Background,
                Backdrop = $"{positioning.Fixed} inset-0 z-110 {display.Flex} overflow-y-auto overscroll-contain {spacing.Padding.Xl} bg-black/50",
                Surface = $"{positioning.Relative} {display.Flex} {flexbox.Col} {sizing.FullWidth} max-h-[calc(100vh-2rem)] {neutralText.Heading} {neutralSurface.Background}",
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
            Tooltip = new TwTooltipTheme
            {
                Wrapper = $"{positioning.Relative} {display.InlineFlex} group",
                Bubble = $"{positioning.Absolute} z-130 w-max max-w-xs px-2 py-1 {typography.Size.Xs} {typography.Weight.Medium} {neutralText.Heading} hidden group-hover:block group-focus-visible:block group-has-[:focus-visible]:block group-data-[tw-tooltip-open]:block",
                Dismissed = "!hidden",
                Top = "bottom-full left-1/2 mb-2 -translate-x-1/2 after:absolute after:inset-x-0 after:top-full after:h-2 after:content-['']",
                Bottom = "top-full left-1/2 mt-2 -translate-x-1/2 after:absolute after:inset-x-0 after:bottom-full after:h-2 after:content-['']",
                Left = "end-full top-1/2 me-2 -translate-y-1/2 after:absolute after:inset-y-0 after:start-full after:w-2 after:content-['']",
                Right = "start-full top-1/2 ms-2 -translate-y-1/2 after:absolute after:inset-y-0 after:end-full after:w-2 after:content-['']",
                Arrow = $"{positioning.Absolute} size-2 rotate-45 pointer-events-none",
                ArrowTop = "-bottom-[5px] left-1/2 -translate-x-1/2 border-t-0 border-l-0",
                ArrowBottom = "-top-[5px] left-1/2 -translate-x-1/2 border-b-0 border-r-0",
                ArrowLeft = "-end-[5px] top-1/2 -translate-y-1/2 border-b-0 border-s-0 rtl:-rotate-45",
                ArrowRight = "-start-[5px] top-1/2 -translate-y-1/2 border-t-0 border-e-0 rtl:-rotate-45"
            }
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
                FocusRingBase = "focus-visible:ring-2 focus-visible:ring-offset-2 focus-visible:ring-offset-[oklch(100%_0_0)] dark:focus-visible:ring-offset-[oklch(25.33%_0_0)]",
                FocusColors = new()
                {
                    Primary = "focus-visible:ring-brickblue-500 dark:focus-visible:ring-brickblue-400",
                    Accent = "focus-visible:ring-brickorange-500 dark:focus-visible:ring-brickorange-400",
                    Success = "focus-visible:ring-green-600 dark:focus-visible:ring-green-400",
                    Danger = "focus-visible:ring-red-500 dark:focus-visible:ring-red-400",
                    Warning = "focus-visible:ring-yellow-500 dark:focus-visible:ring-yellow-400",
                    Info = "focus-visible:ring-blue-500 dark:focus-visible:ring-blue-400",
                    Light = "focus-visible:ring-gray-400 dark:focus-visible:ring-gray-300",
                    Dark = "focus-visible:ring-gray-700 dark:focus-visible:ring-gray-300"
                },
                PeerFocusRingBase = "peer-focus-visible:ring-2 peer-focus-visible:ring-offset-2 peer-focus-visible:ring-offset-[oklch(100%_0_0)] dark:peer-focus-visible:ring-offset-[oklch(25.33%_0_0)] forced-colors:peer-focus-visible:outline-2 forced-colors:peer-focus-visible:outline-offset-2 forced-colors:peer-focus-visible:outline-[Highlight]",
                PeerFocusColors = new()
                {
                    Primary = "peer-focus-visible:ring-brickblue-500 dark:peer-focus-visible:ring-brickblue-400",
                    Accent = "peer-focus-visible:ring-brickorange-500 dark:peer-focus-visible:ring-brickorange-400",
                    Success = "peer-focus-visible:ring-green-600 dark:peer-focus-visible:ring-green-400",
                    Danger = "peer-focus-visible:ring-red-500 dark:peer-focus-visible:ring-red-400",
                    Warning = "peer-focus-visible:ring-yellow-500 dark:peer-focus-visible:ring-yellow-400",
                    Info = "peer-focus-visible:ring-blue-500 dark:peer-focus-visible:ring-blue-400",
                    Light = "peer-focus-visible:ring-gray-400 dark:peer-focus-visible:ring-gray-300",
                    Dark = "peer-focus-visible:ring-gray-700 dark:peer-focus-visible:ring-gray-300"
                },
                SurfaceColors = new()
                {
                    Filled = new()
                    {
                        Primary = $"{background.Medium.Primary} hover:bg-brickblue-700 active:bg-brickblue-800 {text.Medium.Light}",
                        Accent = "bg-brickorange-400 hover:bg-brickorange-300 active:bg-brickorange-500 text-gray-950",
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
                        Dark = $"{text.Medium.Dark} {darkText.Light.Dark} {transparentBackground} {hoverColors.Dark} {borderWidth.Medium} {neutralSurface.BorderStrong}",
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
                        Dark = $"{text.Dark.Dark} {darkText.Light.Dark} {transparentBackground} {hoverColors.Dark}",
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
                    StartIconPadding = "pe-2",
                    EndIconPadding = "ps-2",
                    Transition = transition.ColorsSlow,
                    DismissButtonSize = $"{sizing.Icon.Xl} {display.Flex} {anchor.Center} {rounded.Full}",
                    DismissButtonSpacingWithEndIcon = "ms-2",
                    DismissButtonColor = $"text-current {neutralHoverFill} opacity-70 hover:opacity-100 focus-visible:ring-2 focus-visible:ring-current transition-[opacity,background-color] {transition.DurationFast} motion-reduce:transition-none {interaction.FocusOutlineNone}"
                },
                new TwAvatarTheme
                {
                    Colors = new()
                    {
                        Primary = $"{background.Medium.Primary} {text.Medium.Light}",
                        Accent = "bg-brickorange-700 text-gray-100",
                        Success = $"{background.Medium.Success} text-white",
                        Danger = "bg-red-700 text-gray-100",
                        Warning = $"{background.Medium.Warning} {text.Medium.Dark}",
                        Info = $"{background.Medium.Info} {text.Medium.Light}",
                        Light = $"{background.Medium.Light} {text.Medium.Dark}",
                        Dark = $"{background.Dark.Dark} {text.Medium.Light}",
                    },
                    Neutral = $"{neutralTrack} {neutralText.Secondary}",
                    Base = $"{display.InlineFlex} {anchor.Center} {flexbox.ShrinkNone} {overflow.Hidden} {typography.Weight.Semibold} leading-none select-none",
                    Small = $"size-8 {typography.Size.Xs}",
                    Medium = $"size-10 {typography.Size.Sm}",
                    Large = $"size-12 {typography.Size.Base}",
                    ExtraLarge = "size-16 text-xl",
                    Image = $"{sizing.Full} object-cover",
                    Icon = "text-[1.25em]"
                },
                new TwBreadcrumbTheme
                {
                    List = $"{display.InlineFlex} {flexbox.Wrap} {flexbox.Align.Center} {spacing.Gap.Sm} {typography.Size.Sm}",
                    Item = $"{display.InlineFlex} {flexbox.Align.Center} {spacing.Gap.Sm} {sizing.MinWidthNone}",
                    Separator = $"{display.InlineFlex} {flexbox.Align.Center} {neutralText.Subtle} select-none",
                    SeparatorIcon = "text-[0.875em] leading-none",
                    Label = $"{display.InlineFlex} {flexbox.Align.Center} gap-1.5 px-1.5 py-1 {rounded.Md} {typography.WrapBreakWord}",
                    Link = $"{typography.Weight.Medium} {neutralText.Muted} hover:text-brickblue-700 dark:hover:text-brickblue-300 hover:bg-brickblue-50 dark:hover:bg-brickblue-500/15 {transition.ColorsFast} {focusRing} {interaction.FocusOutlineNone}",
                    Current = $"{typography.Weight.Semibold} {neutralText.Heading}",
                    Icon = $"{flexbox.ShrinkNone} leading-none",
                    CurrentIcon = "text-brickblue-600 dark:text-brickblue-400"
                },
                new TwButtonTheme
                {
                    Base = $"{display.InlineFlex} {overflow.Hidden} {anchor.Center} min-h-11 {typography.Size.Sm} {typography.AlignCenter} {transition.ColorsFast} {interaction.FocusOutlineNone} focus-visible:outline-hidden touch-manipulation rounded-full!",
                    Padding = "px-6 py-2",
                    DensePadding = "px-3 py-1",
                    IconButton = $"{display.InlineFlex} {anchor.Center} size-11 {typography.Size.Sm} {rounded.Full} {interaction.FocusOutlineNone}",
                    Typography = "font-bold",
                    IconTypography = $"{typography.Size.Lg} leading-none",
                    Uppercase = $"{textTransform.Uppercase} tracking-wide",
                    DisabledCursor = interaction.DisabledCursor,
                    ReadonlyCursor = interaction.ReadonlyCursor,
                    DefaultCursor = interaction.PointerCursor,
                    DisabledFilled = $"bg-[oklch(15%_0_0)]/15 dark:bg-[oklch(97.807%_0_0)]/15 {disabledInk} shadow-none {interaction.DisabledCursor}",
                    DisabledOutlined = $"{disabledInk} {transparentBackground} {borderWidth.Thin} border-[oklch(15%_0_0)]/15 dark:border-[oklch(97.807%_0_0)]/15 {interaction.DisabledCursor}",
                    DisabledText = $"{disabledInk} {transparentBackground} {interaction.DisabledCursor}"
                },
                new TwCardTheme
                {
                    Container = "px-5 py-5",
                    Bordered = $"{borderWidth.Thin} {neutralSurface.Border}",
                    Title = $"{typography.Size.Lg} {typography.Weight.Semibold} {typography.WrapBreakWord} {neutralText.Heading} mb-4"
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
                    IndicatorContainer = $"{positioning.Absolute} bottom-2 inset-x-0 z-10 w-fit mx-auto {display.Flex} {flexbox.Justify.Center} {flexbox.Align.Center} gap-4 px-4 py-2 {rounded.Full} bg-white/45 dark:bg-black/60 {shadows.Md}",
                    Indicator = $"{positioning.Relative} size-2.5 before:absolute before:-inset-2 before:content-[''] {rounded.Full} {interaction.PointerCursor} {interaction.FocusOutlineNone} {focusRing} focus-visible:ring-offset-2 focus-visible:ring-offset-[oklch(100%_0_0)] dark:focus-visible:ring-offset-[oklch(25.33%_0_0)] forced-colors:border forced-colors:border-[ButtonText] {transition.ColorsFast}",
                    IndicatorActive = "bg-brickblue-600 dark:bg-brickblue-500 forced-colors:bg-[Highlight]",
                    IndicatorInactive = "bg-[oklch(15%_0_0)]/45 dark:bg-[oklch(97.807%_0_0)]/50 hover:bg-[oklch(15%_0_0)]/65 dark:hover:bg-[oklch(97.807%_0_0)]/70"
                },
                new TwCheckboxTheme
                {
                    Colors = checkBoxRadioButtonColors,
                    Base = $"peer {borderWidth.Thick} {neutralSurface.BorderStrong} {focusRing} {interaction.FocusOutlineNone} {transition.ColorsFast} {transition.EaseInOut} {interaction.PointerCursor} appearance-none",
                    Disabled = $"{interaction.DisabledOpacity} {interaction.DisabledCursor}",
                    Hover = $"hover:border-[oklch(15%_0_0)]/40 dark:hover:border-[oklch(97.807%_0_0)]/40 {interaction.PointerCursor}",
                    LabelBase = $"{display.Flex} {positioning.Relative} {flexbox.Align.Center} {spacing.Gap.Md} min-h-[24px] {inputLabelText} select-none",
                    LabelInteractiveCursor = interaction.PointerCursor,
                    LabelNonInteractiveCursor = interaction.PointerEventsNone,
                    LabelDisabled = interaction.DisabledOpacity,
                    IconWrapper = $"{positioning.Absolute} opacity-0 peer-checked:opacity-100 translate-x-1/4 rtl:-translate-x-1/4",
                    IndeterminateIconWrapper = $"{positioning.Absolute} translate-x-1/4 rtl:-translate-x-1/4"
                },
                new TwChartTheme
                {
                    Container = $"{display.Flex} {flexbox.Col} {spacing.Gap.Lg} {sizing.FullWidth} {sizing.MinWidthNone} m-0",
                    Header = $"{display.Flex} {flexbox.Col} gap-0.5",
                    Title = $"{typography.Size.Base} {typography.Weight.Semibold} {neutralText.Heading}",
                    Description = $"{typography.Size.Sm} {neutralText.Muted}",
                    Body = $"{sizing.FullWidth} {sizing.MinWidthNone}",
                    Chart = $"{display.Flex} {flexbox.Col} {spacing.Gap.Lg} {sizing.FullWidth} {sizing.MinWidthNone}",
                    Toolbar = $"{display.Flex} {flexbox.Wrap} {flexbox.Align.Center} gap-x-4 gap-y-2",
                    Legend = $"{display.Flex} {flexbox.Wrap} {flexbox.Align.Center} gap-x-4 gap-y-1 list-none m-0 p-0",
                    LegendItem = $"{display.InlineFlex} {flexbox.Align.Center} {spacing.Gap.Md} {typography.Size.Xs} {neutralText.Secondary}",
                    LegendButton = $"{rounded.Sm} {interaction.PointerCursor} {interaction.FocusOutlineNone} {focusRing} {neutralHoverText} {transition.ColorsFast}",
                    LegendItemHidden = "opacity-50 line-through",
                    LegendSwatch = $"size-2.5 {rounded.Sm} {flexbox.ShrinkNone}",
                    TableToggle = spacing.PushEnd,
                    AxisTitle = $"{typography.Size.Xs} {neutralText.Muted}",
                    XAxisTitle = typography.AlignCenter,
                    Frame = sizing.FullWidth,
                    FrameWithYAxis = $"{display.Grid} grid-cols-[auto_minmax(0,1fr)] {sizing.FullWidth}",
                    YAxis = $"{positioning.Relative} max-w-32 pe-2",
                    AxisSizer = $"{display.Flex} {flexbox.Col} h-0 {overflow.Hidden} invisible",
                    Tick = $"{typography.Size.Xs} leading-none tabular-nums whitespace-nowrap {neutralText.Muted}",
                    YTick = $"{positioning.Absolute} end-2 max-w-full truncate",
                    XAxis = $"{positioning.Relative} h-6",
                    XTick = $"{positioning.Absolute} top-2 truncate",
                    Plot = $"{positioning.Relative} {sizing.FullWidth}",
                    Small = "h-40",
                    Medium = "h-64",
                    Large = "h-96",
                    Square = "aspect-square mx-auto",
                    SquareSmall = "max-w-48",
                    SquareMedium = "max-w-72",
                    SquareLarge = "max-w-md",
                    Canvas = $"{positioning.Absolute} inset-0 {sizing.Full} overflow-visible",
                    Rect = $"{positioning.Absolute} forced-color-adjust-none",
                    RoundedTop = "rounded-t-sm",
                    RoundedBottom = "rounded-b-sm",
                    RoundedLeft = "rounded-l-sm",
                    RoundedRight = "rounded-r-sm",
                    RoundedAll = rounded.Sm,
                    SurfaceRing = "ring-1 ring-[oklch(100%_0_0)] dark:ring-[oklch(25.33%_0_0)]",
                    Dot = $"{positioning.Absolute} {rounded.Full} ring-2 ring-[oklch(100%_0_0)] dark:ring-[oklch(25.33%_0_0)] forced-color-adjust-none",
                    Glyph = $"{positioning.Absolute} {display.Flex} leading-none",
                    Text = $"{positioning.Absolute} {typography.Size.Xs} leading-none tabular-nums whitespace-nowrap",
                    TextMuted = neutralText.Muted,
                    TextPrimary = neutralText.Heading,
                    TextInverse = "text-white",
                    TextStrong = typography.Weight.Semibold,
                    TextLarge = "text-3xl",
                    TextTruncate = "truncate",
                    FillNone = "fill-none",
                    StrokeHairline = "stroke-1",
                    StrokeNormal = "stroke-2",
                    StrokeThick = "stroke-[6px]",
                    SurfaceStroke = "stroke-2 stroke-[oklch(100%_0_0)] dark:stroke-[oklch(25.33%_0_0)]",
                    HitWrapper = "min-w-6 min-h-6 hover:z-10 focus-within:z-10",
                    HitWrapperShaped = interaction.PointerEventsNone,
                    HitShape = $"{positioning.Absolute} inset-0 pointer-events-auto",
                    Hit = $"{positioning.Absolute} inset-0 {rounded.Sm} {interaction.FocusOutlineNone} {focusRing} dark:focus-visible:ring-offset-2 dark:focus-visible:ring-offset-[oklch(97.807%_0_0)]",
                    HitHighlight = $"{positioning.Absolute} inset-0 {rounded.Sm} bg-[oklch(15%_0_0)]/8 dark:bg-[oklch(97.807%_0_0)]/10 invisible group-hover:visible group-has-[:focus-visible]:visible group-data-[tw-tooltip-open]:visible {interaction.PointerEventsNone}",
                    Crosshair = $"{positioning.Absolute} inset-y-0 left-1/2 w-px bg-[oklch(15%_0_0)]/40 dark:bg-[oklch(97.807%_0_0)]/40 invisible group-hover:visible group-has-[:focus-visible]:visible group-data-[tw-tooltip-open]:visible {interaction.PointerEventsNone}",
                    TooltipTitle = $"{display.Block} {typography.Weight.Semibold}",
                    TooltipRow = $"{display.Flex} {flexbox.Align.Center} {spacing.Gap.Md} mt-1 font-normal",
                    TooltipKey = $"h-0.5 w-3 {rounded.Full} {flexbox.ShrinkNone}",
                    TooltipLabel = neutralText.Secondary,
                    TooltipValue = $"{spacing.PushEnd} ps-3 {typography.Weight.Semibold} tabular-nums",
                    Empty = $"py-8 {typography.AlignCenter} {typography.Size.Sm} {neutralText.Muted}",
                    TableRowHeader = typography.Weight.Medium,
                    Series =
                    [
                        new() { Background = "bg-brickblue-600 dark:bg-brickblue-500", Fill = "fill-brickblue-600 dark:fill-brickblue-500", Stroke = "stroke-brickblue-600 dark:stroke-brickblue-500", Text = "text-brickblue-600 dark:text-brickblue-500" },
                        new() { Background = "bg-orange-600", Fill = "fill-orange-600", Stroke = "stroke-orange-600", Text = "text-orange-600" },
                        new() { Background = "bg-teal-600", Fill = "fill-teal-600", Stroke = "stroke-teal-600", Text = "text-teal-600" },
                        new() { Background = "bg-yellow-700", Fill = "fill-yellow-700", Stroke = "stroke-yellow-700", Text = "text-yellow-700" },
                        new() { Background = "bg-pink-500", Fill = "fill-pink-500", Stroke = "stroke-pink-500", Text = "text-pink-500" },
                        new() { Background = "bg-green-600", Fill = "fill-green-600", Stroke = "stroke-green-600", Text = "text-green-600" },
                        new() { Background = "bg-blue-600 dark:bg-blue-500", Fill = "fill-blue-600 dark:fill-blue-500", Stroke = "stroke-blue-600 dark:stroke-blue-500", Text = "text-blue-600 dark:text-blue-500" },
                        new() { Background = "bg-red-600 dark:bg-red-500", Fill = "fill-red-600 dark:fill-red-500", Stroke = "stroke-red-600 dark:stroke-red-500", Text = "text-red-600 dark:text-red-500" }
                    ],
                    Positive = new() { Background = "bg-green-600", Fill = "fill-green-600", Stroke = "stroke-green-600", Text = "text-green-600" },
                    Negative = new() { Background = "bg-red-600 dark:bg-red-500", Fill = "fill-red-600 dark:fill-red-500", Stroke = "stroke-red-600 dark:stroke-red-500", Text = "text-red-600 dark:text-red-500" },
                    Total = new() { Background = "bg-slate-500 dark:bg-slate-400", Fill = "fill-slate-500 dark:fill-slate-400", Stroke = "stroke-slate-500 dark:stroke-slate-400", Text = "text-slate-500 dark:text-slate-400" },
                    Sequential = new() { Background = "bg-brickblue-600 dark:bg-brickblue-500", Fill = "fill-brickblue-600 dark:fill-brickblue-500", Stroke = "stroke-brickblue-600 dark:stroke-brickblue-500", Text = "text-brickblue-600 dark:text-brickblue-500" },
                    Track = new() { Background = "bg-[oklch(91%_0_0)] dark:bg-[oklch(34%_0_0)]", Fill = "fill-[oklch(91%_0_0)] dark:fill-[oklch(34%_0_0)]", Stroke = "stroke-[oklch(91%_0_0)] dark:stroke-[oklch(34%_0_0)]", Text = "text-[oklch(91%_0_0)] dark:text-[oklch(34%_0_0)]" },
                    Grid = new() { Background = "bg-[oklch(92%_0_0)] dark:bg-[oklch(32%_0_0)]", Fill = "fill-[oklch(92%_0_0)] dark:fill-[oklch(32%_0_0)]", Stroke = "stroke-[oklch(92%_0_0)] dark:stroke-[oklch(32%_0_0)]", Text = "text-[oklch(92%_0_0)] dark:text-[oklch(32%_0_0)]" },
                    Axis = new() { Background = "bg-[oklch(78%_0_0)] dark:bg-[oklch(48%_0_0)]", Fill = "fill-[oklch(78%_0_0)] dark:fill-[oklch(48%_0_0)]", Stroke = "stroke-[oklch(78%_0_0)] dark:stroke-[oklch(48%_0_0)]", Text = "text-[oklch(78%_0_0)] dark:text-[oklch(48%_0_0)]" },
                    Ink = new() { Background = "bg-[oklch(15%_0_0)] dark:bg-[oklch(97.807%_0_0)]", Fill = "fill-[oklch(15%_0_0)] dark:fill-[oklch(97.807%_0_0)]", Stroke = "stroke-[oklch(15%_0_0)] dark:stroke-[oklch(97.807%_0_0)]", Text = neutralText.Heading }
                },
                new TwChipTheme
                {
                    Base = $"{display.InlineFlex} {anchor.Center} gap-1.5 {typography.Weight.Medium} {shadows.Sm} {transition.ColorsFast} touch-manipulation",
                    CloseButton = $"{display.Flex} {anchor.Center} {sizing.Icon.Sm} {typography.AlignCenter} {neutralHoverFill} {rounded.Full} {transition.ColorsFast}",
                    ContentButton = $"{display.InlineFlex} {anchor.Center} gap-1.5 bg-transparent text-inherit {rounded.Full} {interaction.PointerCursor} {interaction.FocusOutlineNone} focus-visible:ring-2 focus-visible:ring-current",
                    Sm = "h-5 px-1.5 py-1 text-[10px] leading-none",
                    Md = $"h-6 px-2 py-1 {typography.Size.Xs} leading-none",
                    Lg = $"h-8 px-2.5 py-1.5 {typography.Size.Sm} leading-none"
                },
                new TwCodeBlockTheme
                {
                    Container = $"{overflow.Hidden} p-1 {neutralSurface.BackgroundSubtle} {borderWidth.Thin} {neutralSurface.Border}",
                    Header = $"px-2 py-1 {typography.Size.Xs} {typography.Weight.Medium} {textTransform.Uppercase} tracking-wide {neutralText.Muted}",
                    Panel = $"{rounded.Md} {typography.Size.Sm} {neutralSurface.Background} {neutralText.Heading} {borderWidth.Thin} {neutralSurface.Border}",
                    Code = $"{rounded.Md} {focusRingInset} {interaction.FocusOutlineNone}",
                    CopyButtonWrapper = $"{neutralText.Muted} {neutralHoverText} {transition.ColorsFast}"
                },
                new TwCollapseTheme
                {
                    Container = $"tw-collapse {borderWidth.Thin} {neutralSurface.Border}",
                    Trigger = $"{display.Flex} {flexbox.Align.Center} {flexbox.Justify.Between} {spacing.Gap.Md} {sizing.FullWidth} {spacing.InteractiveRowPadding} text-start {typography.Weight.Medium} {neutralText.Heading} {neutralSurface.Hover} {focusRingInset} {transition.ColorsFast} {interaction.FocusOutlineNone} touch-manipulation",
                    Icon = $"{display.Flex} {flexbox.ShrinkNone} {sizing.Icon.Sm} {spacing.PushEnd} {transition.TransformSlow}",
                    IconOpen = "rotate-180",
                    Content = $"{overflow.Hidden} border-t {neutralSurface.Border} {transition.ColorsSlow}"
                },
                new TwColorPickerTheme
                {
                    Swatch = $"block {flexbox.ShrinkNone} h-7 w-7 {neutralHairlineRing} {shadows.Sm} transition-[box-shadow,opacity] {transition.DurationFast} motion-reduce:transition-none",
                    SwatchDisabled = interaction.DisabledOpacity,
                    SwatchHover = "hover:ring-[oklch(15%_0_0)]/20 dark:hover:ring-[oklch(97.807%_0_0)]/25",
                    InputContainer = $"{display.Flex} {flexbox.Align.Center} {spacing.Gap.Md}",
                    DialogPosition = $"{positioning.Fixed} z-120 {spacing.MarginTop.Md}",
                    PreviewSwatch =$"{flexbox.Flex1} {sizing.MinWidthNone} h-6 {rounded.Full} {neutralHairlineRing} {shadows.Sm}",
                    SelectorSquare = $"{positioning.Relative} {overflow.Hidden} {sizing.FullWidth} h-48 {neutralHairlineRing} {focusRing} {interaction.FocusOutlineNone} cursor-crosshair touch-none",
                    SelectorThumb = $"{positioning.Absolute} {sizing.Icon.Sm} {rounded.Full} border-2 border-white ring-1 ring-black/10 {shadows.Lg} {interaction.PointerEventsNone}",
                    SliderTrack = $"{overflow.Hidden} {sizing.FullWidth} h-2.5 {rounded.Full} {neutralHairlineRing} {interaction.PointerEventsNone}",
                    SliderThumb = $"{positioning.Absolute} top-1/2 {sizing.Icon.Sm} {rounded.Full} border-2 border-white ring-1 ring-black/10 {shadows.Lg} {interaction.PointerEventsNone}",
                    AlphaLabel = $"{typography.Size.Xs} {typography.Weight.Medium} {neutralText.Secondary}",
                    ActionBar = $"{display.Flex} {flexbox.Justify.End} {spacing.Gap.Md} pt-1",
                    ControlRow = $"{display.Flex} {flexbox.Align.Center} {spacing.Gap.Md} {spacing.Padding.Md} {neutralSurface.BackgroundSubtle}",
                    Body = $"{display.Flex} {flexbox.Col} {spacing.Gap.Lg}",
                    SliderRow = $"{flexbox.Flex1} h-6 {rounded.Full} {focusRing} {interaction.FocusOutlineNone}",
                    HueSliderTouch = "touch-none",
                    InputColumn = flexbox.Flex1,
                    ModeSwitchButton = "shrink-0 tracking-wide",
                    AlphaReadoutAlign = "text-end"
                },
                new TwDatePickerTheme
                {
                    Header = $"{typography.AlignCenter} {typography.Weight.Medium} {overlayTheme.Popover.Background} {rounded.RoundedTop.Lg} border-b {neutralSurface.BorderStrong}",
                    WeekdaysHeader = $"{display.Flex} {anchor.Center} h-8 {typography.Size.Xs} font-normal {neutralText.Muted}",
                    Base = $"{positioning.Fixed} z-120 {flexbox.Row} md:flex-row {flexbox.Align.Center} mt-1 px-2 pb-2 {typography.AlignCenter} {typography.Size.Sm} {typography.Weight.Medium} {transition.ColorsFast} {interaction.PointerCursor}",
                    ActiveClass = $"{neutralTrack} {neutralText.Heading} {typography.Weight.Semibold}",
                    SelectedClass = $"bg-brickblue-600 hover:bg-brickblue-700 text-white {typography.Weight.Semibold}",
                    HoverClass = neutralHoverFill,
                    ButtonClass = $"{display.Flex} {flexbox.Align.Center} {flexbox.Justify.Center} h-8 {sizing.FullWidth} {spacing.Padding.Md} tabular-nums {focusRingInset} {interaction.FocusOutlineNone} {transition.ColorsFast}",
                    RangeClass = "bg-brickblue-100 text-brickblue-900 dark:bg-brickblue-500/20 dark:text-brickblue-100",
                    RangeMonthCaptionClass = $"mt-2 {typography.AlignCenter} {typography.Size.Sm} {typography.Weight.Semibold} {neutralText.Heading}",
                    PrevMonthClass = neutralText.Subtle,
                    DefaultFormat = "dd/MM/yyyy",
                    DefaultDateTimeFormat = "dd/MM/yyyy HH:mm",
                    DefaultDateTimeFormat12Hour = "dd/MM/yyyy hh:mm tt",
                    DefaultRangeSeparator = " - ",
                    IconTriggerWrapper = $"{positioning.Absolute} {inset.Top} {inset.Start} {display.Flex} {flexbox.Align.Center} {spacing.PaddingStart.Lg} {interaction.PointerCursor}",
                    IconGlyph = $"{sizing.Icon.Md} {neutralText.Subtle}",
                    TextfieldPadding = "ps-10 pe-3",
                    NativeInputAppearance = "appearance-none !flex items-center [&::-webkit-date-and-time-value]:min-h-0 [&::-webkit-date-and-time-value]:text-start [&::-webkit-datetime-edit]:p-0",
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
                    RangeStageTabsContainer = $"{display.Flex} {spacing.Gap.Sm} {spacing.Padding.Sm} {rounded.Lg} {neutralTrack} {spacing.MarginBottom.Md}",
                    RangeStageTabInactive = neutralText.Subtle,
                    PanelMaxHeight = "overflow-y-auto max-h-[calc(100vh-2rem)]",
                    PanelMaxWidth = "overflow-x-auto max-w-[calc(100vw-2rem)]",
                    DualMonthPanelWidth = "md:w-138",
                    StageTabBase = $"{flexbox.Flex1} py-1.5 {typography.Size.Sm} {typography.Weight.Medium} {typography.AlignCenter} {interaction.PointerCursor} {transition.ColorsFast} {focusRing} {interaction.FocusOutlineNone}",
                    StageTabActive = $"bg-white dark:bg-[oklch(34%_0_0)] {neutralText.Heading} {shadows.Sm}",
                    HeaderNavButton = "py-2 px-4 text-xl"
                },
                new TwFileUploadTheme
                {
                    IconSpacing = "me-2",
                    FileList = $"{typography.Size.Sm} {neutralText.Secondary}",
                    ChipTextColor = "dark:text-white",
                    FocusRing = "has-[:focus-visible]:ring-2 has-[:focus-visible]:ring-brickblue-500 dark:has-[:focus-visible]:ring-brickblue-400 has-[:focus-visible]:ring-offset-2 has-[:focus-visible]:ring-offset-[oklch(100%_0_0)] dark:has-[:focus-visible]:ring-offset-[oklch(25.33%_0_0)] forced-colors:has-[:focus-visible]:outline-2 forced-colors:has-[:focus-visible]:outline-offset-2 forced-colors:has-[:focus-visible]:outline-[Highlight]"
                },
                new TwIconTheme
                {
                    Colors = new()
                    {
                        Primary = "text-brickblue-600 dark:text-brickblue-400",
                        Accent = "text-brickorange-600 dark:text-brickorange-400",
                        Success = "text-green-600 dark:text-green-400",
                        Danger = "text-red-600 dark:text-red-400",
                        Warning = "text-[oklch(65%_0_0)] dark:text-yellow-400",
                        Info = "text-blue-600 dark:text-blue-400",
                        Light = "text-gray-100 dark:text-white",
                        Dark = "text-gray-950 dark:text-gray-950",
                    },
                    HoverBackground = "hover:bg-current/10 dark:hover:bg-current/15",
                    Pulse = "relative tw-icon-pulse"
                },
                new TwLinkTheme
                {
                    Default = $"underline-offset-2 hover:underline {transition.ColorsFast} {focusRing} {interaction.FocusOutlineNone}"
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
                    ButtonGroupHorizontal = $"{flexbox.Row} {flexbox.Wrap}",
                    ButtonGroupFullWidth = sizing.FullWidth,
                    ButtonGroupFullWidthRow = "[&>*]:flex-1",
                    ChipGroupBase = $"{display.Flex} {flexbox.Wrap}",
                    ChipGroupAlignStart = flexbox.Justify.Start,
                    ChipGroupAlignCenter = flexbox.Justify.Center,
                    ChipGroupAlignEnd = flexbox.Justify.End
                },
                new TwInputTheme
                {
                    DefaultInputVariant = InputVariant.Outlined,
                    TextfieldBase = $"{display.Block} {sizing.FullWidth} {sizing.MinWidthNone} max-w-full {neutralText.Heading} placeholder:text-[oklch(15%_0_0)]/65 dark:placeholder:text-[oklch(97.807%_0_0)]/65 {transition.ColorsFast} {transition.EaseInOut} {interaction.FocusOutlineNone}",
                    SelectBase = $"{display.Block} {sizing.FullWidth} pe-10 {neutralText.Heading} bg-[url('data:image/svg+xml;charset=utf-8,%3Csvg%20xmlns%3D%22http%3A%2F%2Fwww.w3.org%2F2000%2Fsvg%22%20fill%3D%22none%22%20viewBox%3D%220%200%2020%2020%22%3E%3Cpath%20stroke%3D%22%236b7280%22%20stroke-linecap%3D%22round%22%20stroke-linejoin%3D%22round%22%20stroke-width%3D%221.5%22%20d%3D%22m6%208%204%204%204-4%22%2F%3E%3C%2Fsvg%3E')] bg-[length:1.5em_1.5em] bg-[right_0.5rem_center] rtl:bg-[left_0.5rem_center] bg-no-repeat {transition.ColorsFast} appearance-none {interaction.FocusOutlineNone} supports-[appearance:base-select]:[appearance:base-select] supports-[appearance:base-select]:flex supports-[appearance:base-select]:items-center [&::picker(select)]:[appearance:base-select] [&::picker-icon]:hidden [&::picker(select)]:border-[oklch(15%_0_0)]/25 dark:[&::picker(select)]:border-[oklch(97.807%_0_0)]/20 [&::picker(select)]:bg-[oklch(100%_0_0)] dark:[&::picker(select)]:bg-[oklch(40%_0_0)]",
                    Size = $"{inputHeight} {typography.Size.Base}",
                    DenseSize = $"{inputDenseHeight} {typography.Size.Sm}",
                    SelectOption = $"{spacing.Padding.Lg} dark:bg-gray-800 dark:text-white supports-[appearance:base-select]:[padding-inline:revert] supports-[appearance:base-select]:py-2 supports-[appearance:base-select]:bg-transparent dark:supports-[appearance:base-select]:bg-transparent supports-[appearance:base-select]:hover:bg-[oklch(97%_0_0)] dark:supports-[appearance:base-select]:hover:bg-[oklch(34%_0_0)] supports-[appearance:base-select]:checked:bg-brickblue-50 supports-[appearance:base-select]:checked:text-brickblue-700 dark:supports-[appearance:base-select]:checked:bg-brickblue-500/20 dark:supports-[appearance:base-select]:checked:text-brickblue-300",
                    SelectNativeBackground = "!bg-white dark:!bg-gray-800",
                    SelectDefaultPadding = "px-3",
                    SelectReadOnlyBackground = "!bg-none",
                    SelectMultiTriggerLayout = $"{display.Flex} {flexbox.Wrap} {flexbox.Align.Center} {spacing.Gap.Sm} min-h-10 py-0.5 {typography.Size.Base}",
                    SelectNativeMultiOverlay = $"{positioning.Absolute} inset-0 {sizing.Full} m-0 opacity-0 {interaction.PointerCursor} {interaction.FocusOutlineNone} touch-manipulation",
                    SelectMultiOpenButton = $"{flexbox.Flex1} min-w-[2rem] min-h-8 truncate text-start bg-transparent border-0 {interaction.FocusOutlineNone}",
                    SelectPanelPosition = $"{positioning.Fixed} z-120 {spacing.MarginTop.Md}",
                    SelectPanelSurface = $"overflow-y-auto max-h-64 {spacing.Padding.Sm}",
                    SelectPanelItemText = "[&_label]:!text-[oklch(15%_0_0)] dark:[&_label]:!text-[oklch(97.807%_0_0)] [&_fieldset]:gap-0 [&_label]:w-full [&_label]:gap-2 [&_label]:px-2 [&_label]:py-2 [&_label]:text-base [&_input]:sr-only [&_label>span]:hidden [&_label::before]:content-['✓'] [&_label::before]:invisible [&_label::before]:w-4 [&_label::before]:shrink-0 [&_label::before]:text-center [&_label:has(input:checked)::before]:visible [&_label:hover]:bg-[oklch(97%_0_0)] dark:[&_label:hover]:bg-[oklch(34%_0_0)] [&_label:has(input:checked)]:bg-brickblue-50 dark:[&_label:has(input:checked)]:bg-brickblue-500/20 [&_label:has(input:checked)]:!text-brickblue-700 dark:[&_label:has(input:checked)]:!text-brickblue-300 [&_label:has(input:focus-visible)]:ring-2 [&_label:has(input:focus-visible)]:ring-inset [&_label:has(input:focus-visible)]:ring-brickblue-500 dark:[&_label:has(input:focus-visible)]:ring-brickblue-400",
                    SelectCustomTrigger = $"{display.Flex} {flexbox.Align.Center} {flexbox.Justify.Between} {spacing.Gap.Md} {sizing.FullWidth} {sizing.MinWidthNone} text-start {neutralText.Heading} {transition.ColorsFast} {interaction.FocusOutlineNone} {interaction.PointerCursor}",
                    SelectCustomPlaceholder = $"{neutralText.Muted}",
                    SelectCustomChevron = $"{display.Flex} {anchor.Center} {sizing.Icon.Sm} {flexbox.ShrinkNone} leading-none {neutralText.Muted} {transition.TransformFast}",
                    SelectCustomChevronOpen = "rotate-180",
                    SelectCustomListbox = $"{display.Flex} {flexbox.Col} gap-0.5 {interaction.FocusOutlineNone}",
                    SelectCustomOption = $"{display.Flex} {flexbox.Align.Center} {flexbox.Justify.Between} {spacing.Gap.Md} {rounded.Md} px-2 py-2 {typography.Size.Sm} {neutralText.Heading} {interaction.PointerCursor} select-none data-[active=true]:bg-[oklch(95%_0_0)] dark:data-[active=true]:bg-[oklch(48%_0_0)] data-[active=true]:outline-2 data-[active=true]:-outline-offset-2 data-[active=true]:outline-brickblue-600 dark:data-[active=true]:outline-brickblue-300 aria-selected:font-semibold",
                    SelectMultiTriggerBase = $"{positioning.Relative} {sizing.FullWidth} {sizing.MinWidthNone} text-start pe-10 {neutralText.Heading} {transition.ColorsFast} {interaction.FocusOutlineNone} {interaction.PointerCursor}",
                    SelectMultiChipsPadding = "ps-0.75",
                    SelectMultiChevron = $"{positioning.Absolute} end-3 top-1/2 -translate-y-1/2 {display.Flex} {sizing.Icon.Sm} leading-none {neutralText.Muted} {transition.TransformFast} {interaction.PointerEventsNone}",
                    SelectCustomCheck = $"{sizing.Icon.Sm} {flexbox.ShrinkNone} text-brickblue-600 dark:text-brickblue-300",
                    InputLegendBase = $"mb-3 {typography.Size.Base} {typography.Weight.Medium} {neutralText.Secondary}",
                    LabelBase = $"{display.Block} mb-2 {inputLabelText}",
                    OutlinedBorder = $"{borderWidth.Thin} {neutralSurface.BorderStrong}",
                    FilledBorder = $"border-b {neutralSurface.BorderStrong}",
                    FocusBorder = "focus:border-brickblue-600 dark:focus:border-brickblue-400 focus:ring-2 focus:ring-brickblue-500 dark:focus:ring-brickblue-400",
                    FocusVisibleBorder = "focus-visible:border-brickblue-600 dark:focus-visible:border-brickblue-400 focus-visible:ring-2 focus-visible:ring-brickblue-500 dark:focus-visible:ring-brickblue-400",
                    FocusWithinBorder = "has-[:focus-visible]:border-brickblue-600 dark:has-[:focus-visible]:border-brickblue-400 has-[:focus-visible]:ring-2 has-[:focus-visible]:ring-brickblue-500 dark:has-[:focus-visible]:ring-brickblue-400",
                    FilledBackgroundColor = neutralSurface.Elevated,
                    ErrorMessage = $"{spacing.MarginTop.Sm} {typography.Size.Xs} {text.Medium.Danger} {darkText.Medium.Danger}"
                },
                new TwOnThisPageTheme
                {
                    Title = $"{spacing.MarginBottom.Lg} {typography.Weight.Semibold} {neutralText.Heading}",
                    List = $"border-s {neutralSurface.Border}",
                    Link = $"{display.Block} -ms-px py-1 ps-4 border-s border-transparent {neutralText.Muted} {neutralHoverText} hover:border-[oklch(15%_0_0)]/45 dark:hover:border-[oklch(97.807%_0_0)]/45 {transition.ColorsFast} {focusRingInset} {interaction.FocusOutlineNone}",
                    LinkCurrent = $"aria-[current=location]:font-medium aria-[current=location]:border-brickblue-600 dark:aria-[current=location]:border-brickblue-400 aria-[current=location]:text-[oklch(15%_0_0)] dark:aria-[current=location]:text-[oklch(97.807%_0_0)] forced-colors:aria-[current=location]:border-[Highlight]"
                },
                new TwPaginationTheme
                {
                    List = $"{display.Flex} {flexbox.Align.Center} {spacing.Gap.Sm}",
                    Base = $"{display.Flex} {anchor.Center} tabular-nums select-none {transition.ColorsFast} {focusRingInset} {interaction.FocusOutlineNone}",
                    Size = $"{inputHeight} min-w-10 px-3 {typography.Size.Sm}",
                    DenseSize = $"{inputDenseHeight} min-w-8 px-2 {typography.Size.Sm}",
                    ActiveButton = $"{typography.Weight.Semibold} bg-brickblue-50 text-brickblue-700 ring-1 ring-inset ring-brickblue-600/20 dark:bg-brickblue-500/20 dark:text-brickblue-300 dark:ring-brickblue-400/30 forced-colors:outline forced-colors:outline-2 forced-colors:-outline-offset-2 forced-colors:outline-[Highlight] {interaction.PointerCursor}",
                    Buttons = $"{neutralText.Secondary} {neutralHoverFill} {neutralHoverText} {interaction.PointerCursor}",
                    DisabledButton = "text-[oklch(15%_0_0)]/40 dark:text-[oklch(97.807%_0_0)]/40 cursor-not-allowed"
                },
                new TwPickListTheme
                {
                    Container = $"{display.Flex} {flexbox.Align.Start} {spacing.Gap.Lg}",
                    Column = $"{display.Flex} {flexbox.Col} {spacing.Gap.Sm} {flexbox.Flex1} {sizing.MinWidthNone}",
                    ColumnHeader = $"{display.Flex} {flexbox.Align.Center} {flexbox.Justify.Between} {spacing.Gap.Sm} min-h-8",
                    Label = $"{typography.Size.Sm} {typography.Weight.Medium} {neutralText.Heading}",
                    ReorderButtons = $"{display.Flex} {flexbox.Align.Center} {spacing.Gap.Sm}",
                    ReorderButton = $"{neutralText.Muted} {neutralHoverFill} {neutralHoverText} {transition.ColorsFast} aria-disabled:text-[oklch(15%_0_0)]/40 dark:aria-disabled:text-[oklch(97.807%_0_0)]/40 aria-disabled:hover:bg-transparent dark:aria-disabled:hover:bg-transparent aria-disabled:cursor-not-allowed",
                    TransferButton = $"{borderWidth.Thin} {neutralSurface.BorderStrong} {neutralText.Secondary} {neutralHoverFill} {neutralHoverText} {transition.ColorsFast} aria-disabled:text-[oklch(15%_0_0)]/40 dark:aria-disabled:text-[oklch(97.807%_0_0)]/40 aria-disabled:border-[oklch(15%_0_0)]/20 dark:aria-disabled:border-[oklch(97.807%_0_0)]/20 aria-disabled:hover:bg-transparent dark:aria-disabled:hover:bg-transparent aria-disabled:cursor-not-allowed",
                    TransferButtonColumn = $"{display.Flex} {flexbox.Col} {flexbox.Align.Center} {flexbox.Justify.Center} {flexbox.AlignSelf.Center} {spacing.Gap.Md}",
                    ListBox = $"overflow-y-auto {sizing.FullWidth} h-64 {spacing.Padding.Sm} {neutralSurface.Background} {borderWidth.Thin} {neutralSurface.BorderStrong}",
                    ListBoxDisabled = $"{interaction.DisabledOpacity} {interaction.PointerEventsNone}",
                    Item = $"{display.Flex} {flexbox.Align.Center} {spacing.Gap.Md} {rounded.Md} px-3 py-2 {typography.Size.Sm} {neutralText.Heading} {neutralRowHover} aria-selected:bg-brickblue-50 aria-selected:text-brickblue-700 aria-selected:hover:bg-brickblue-100 dark:aria-selected:bg-brickblue-500/20 dark:aria-selected:text-brickblue-300 dark:aria-selected:hover:bg-brickblue-500/30 {focusRingInset} {transition.ColorsFast} {interaction.PointerCursor} select-none {interaction.FocusOutlineNone}",
                    ItemSelected = typography.Weight.Medium,
                    EmptyState = $"px-3 py-8 {typography.AlignCenter} {typography.Size.Sm} {neutralText.Subtle}"
                },
                new TwProgressTheme
                {
                    Colors = new()
                    {
                        Primary = "[&::-webkit-progress-value]:bg-brickblue-600 dark:[&::-webkit-progress-value]:bg-brickblue-500 [&::-moz-progress-bar]:bg-brickblue-600 dark:[&::-moz-progress-bar]:bg-brickblue-500",
                        Accent = "[&::-webkit-progress-value]:bg-brickorange-600 dark:[&::-webkit-progress-value]:bg-brickorange-500 [&::-moz-progress-bar]:bg-brickorange-600 dark:[&::-moz-progress-bar]:bg-brickorange-500",
                        Success = "[&::-webkit-progress-value]:bg-green-600 dark:[&::-webkit-progress-value]:bg-green-500 [&::-moz-progress-bar]:bg-green-600 dark:[&::-moz-progress-bar]:bg-green-500",
                        Danger = "[&::-webkit-progress-value]:bg-red-600 dark:[&::-webkit-progress-value]:bg-red-500 [&::-moz-progress-bar]:bg-red-600 dark:[&::-moz-progress-bar]:bg-red-500",
                        Warning = "[&::-webkit-progress-value]:bg-yellow-500 dark:[&::-webkit-progress-value]:bg-yellow-400 [&::-moz-progress-bar]:bg-yellow-500 dark:[&::-moz-progress-bar]:bg-yellow-400",
                        Info = "[&::-webkit-progress-value]:bg-blue-600 dark:[&::-webkit-progress-value]:bg-blue-500 [&::-moz-progress-bar]:bg-blue-600 dark:[&::-moz-progress-bar]:bg-blue-500",
                        Light = "[&::-webkit-progress-value]:bg-gray-400 dark:[&::-webkit-progress-value]:bg-gray-300 [&::-moz-progress-bar]:bg-gray-400 dark:[&::-moz-progress-bar]:bg-gray-300",
                        Dark = "[&::-webkit-progress-value]:bg-gray-900 dark:[&::-webkit-progress-value]:bg-gray-100 [&::-moz-progress-bar]:bg-gray-900 dark:[&::-moz-progress-bar]:bg-gray-100"
                    },
                    Base = $"{display.Block} {overflow.Hidden} {sizing.FullWidth} {sizing.MinWidthNone} tabular-nums {neutralTrackStrong} {rounded.Full} {borderWidth.Thin} {neutralSurface.BorderStrong} forced-colors:[&::-webkit-progress-value]:bg-[Highlight] forced-colors:[&::-moz-progress-bar]:bg-[Highlight] indeterminate:animate-pulse indeterminate:motion-reduce:animate-none appearance-none [&::-webkit-progress-bar]:rounded-full [&::-webkit-progress-bar]:bg-transparent [&::-webkit-progress-value]:rounded-full [&::-webkit-progress-value]:transition-[width] [&::-webkit-progress-value]:{transition.DurationSlow} [&::-moz-progress-bar]:rounded-full [&::-moz-progress-bar]:transition-[width] [&::-moz-progress-bar]:{transition.DurationSlow}",
                    Small = "h-1.5",
                    Medium = "h-2.5",
                    Large = "h-4",
                    Wrapper = $"{positioning.Relative} {display.Block}",
                    Animated = $"{positioning.Absolute} inset-y-0 start-0 {overflow.Hidden} {rounded.Full} {interaction.PointerEventsNone} transition-[width] {transition.DurationSlow} tw-wave [--tw-wave-color:rgb(255_255_255/0.7)] dark:[--tw-wave-color:rgb(255_255_255/0.4)]"
                },
                new TwTabTheme
                {
                    TabBase = $"{positioning.Relative} {display.InlineFlex} {anchor.Center} {typography.Weight.Medium} {typography.Size.Sm} {transition.ColorsFast} {focusRingInset} {interaction.FocusOutlineNone} touch-manipulation",
                    TabPadding = "px-6 py-3",
                    TabDensePadding = "px-4 py-2",
                    ActiveIndicator = "after:absolute after:bottom-0 after:left-0 after:right-0 after:h-0.5 after:bg-current forced-colors:after:bg-[Highlight] after:scale-x-100 after:transition-transform after:duration-200 after:motion-reduce:transition-none",
                    InactiveIndicator = $"{neutralText.Muted} {neutralHoverText} {neutralRowHover} after:absolute after:bottom-0 after:left-0 after:right-0 after:h-0.5 after:bg-current after:scale-x-0 after:transition-transform after:duration-200 after:motion-reduce:transition-none",
                    DisabledTab = $"{neutralText.Muted} {interaction.DisabledOpacity} {interaction.DisabledCursor}",
                    TabListContainer = $"{display.Flex} {flexbox.Wrap} {overflow.Hidden} {borderWidth.Thin} {neutralSurface.Border}",
                    PanelContainer = $"px-6 py-5 border-x border-b {neutralSurface.Border} {focusRingInset} {interaction.FocusOutlineNone}",
                    Background = neutralSurface.Background,
                },
                new TwSkeletonTheme
                {
                    Base = $"{display.Block} {positioning.Relative} {overflow.Hidden} {neutralTrack}",
                    Text = $"{sizing.FullWidth} h-4",
                    Circle = "size-12",
                    Rectangle = $"{sizing.FullWidth} h-24",
                    MeasuringWrapper = "invisible",
                    Pulse = "animate-pulse motion-reduce:animate-none",
                    Wave = "tw-wave"
                },
                new TwSidebarTheme
                {
                    SkipLink = "focus:not-sr-only focus:absolute focus:top-2 focus:start-2 focus:z-140 focus:px-3 focus:py-2 focus:text-sm focus:font-medium focus:bg-[oklch(100%_0_0)] dark:focus:bg-[oklch(40%_0_0)] focus:text-[oklch(15%_0_0)] dark:focus:text-[oklch(97.807%_0_0)] focus:rounded focus:shadow-lg",
                    MobileOverlay = "inset-0 z-90 lg:hidden bg-black/50",
                    DrawerCloseButton = "lg:hidden absolute top-2 end-2 text-lg",
                    MobileClosed = "-translate-x-full rtl:translate-x-full",
                    MobileOpen = "lg:relative translate-x-0",
                    NavigationList = "overflow-y-auto pb-6",
                    NavbarFixedInset = "left-0 right-0",
                    NavbarNavigationResponsive = "lg:flex lg:flex-1 lg:flex-row lg:w-auto lg:p-0 lg:bg-transparent lg:shadow-none",
                    NavbarNavigationExpanded = "flex flex-1",
                    NavigationItemToggleLabel = "me-auto text-start",
                    NavigationItemToggleIcon = "transition-transform",
                    Navbar = $"{display.Flex} z-40 {flexbox.Align.Center} {flexbox.ShrinkNone} {sizing.FullWidth} min-h-16 px-4 py-2 bg-brickyellow-400 text-black",
                    NavbarContent = $"{display.Flex} {flexbox.Wrap} {flexbox.Align.Center} {spacing.Gap.Xl} {sizing.FullWidth}",
                    NavbarBrand = flexbox.ShrinkNone,
                    NavbarNavigation = $"{flexbox.Align.Center} {spacing.Gap.Md}",
                    NavbarActions = $"{display.Flex} {flexbox.Align.Center} {spacing.Gap.Md} {spacing.PushEnd}",
                    NavbarActionsCollapsible = "max-lg:ms-0 max-lg:mt-2 max-lg:flex-wrap max-lg:justify-center max-lg:border-t max-lg:border-current/15 max-lg:pt-3",
                    NavbarToggle = $"lg:hidden {spacing.PushEnd} -me-2 size-11 {flexbox.ShrinkNone} text-2xl",
                    NavbarToggleIcon = $"text-xl {neutralText.Secondary} {neutralHoverText} {transition.ColorsFast}",
                    NavbarLink = $"{display.InlineFlex} {flexbox.Align.Center} {spacing.Gap.Md} px-3 py-1.5 {typography.Size.Sm} {typography.Weight.Medium} {neutralText.Secondary} {neutralHoverFill} {neutralHoverText} {rounded.Md} {transition.ColorsFast} {focusRing} {interaction.FocusOutlineNone}",
                    NavbarLinkCollapsible = "max-lg:w-full max-lg:min-h-11 max-lg:px-3 max-lg:text-base",
                    NavbarLinkActive = $"{neutralTrack} {neutralText.Heading}",
                    NavbarMobileMenu = "max-lg:basis-full max-lg:-mx-4 max-lg:-mt-2 max-lg:-mb-2 max-lg:max-h-[calc(100dvh-3.5rem)] max-lg:overflow-y-auto max-lg:overscroll-contain max-lg:items-stretch max-lg:gap-1 max-lg:border-t max-lg:border-current/15 max-lg:px-2 max-lg:py-2 max-lg:[&>a]:flex max-lg:[&>a]:min-h-11 max-lg:[&>a]:w-full max-lg:[&>a]:items-center max-lg:[&>a]:px-3 max-lg:[&>a]:text-base",
                    Sidebar = $"z-100 overflow-auto scrollbar-gutter-stable overscroll-contain {flexbox.ShrinkNone} h-dvh w-64 {spacing.Padding.Xl} {neutralSurface.Background} border-e {neutralSurface.Border} {transition.TransformFast} {transition.EaseInOut}",
                    NavigationItemBase = $"{display.Flex} {flexbox.Align.Center} {spacing.Gap.Md} {sizing.MinWidthNone} px-3 py-2 {typography.Size.Sm} {neutralText.Secondary} {neutralSurface.Hover} {focusRingInset} {transition.ColorsFast} {interaction.FocusOutlineNone} {interaction.PointerCursor}",
                    NavigationItemActive = $"{neutralTrack} {neutralText.Heading}",
                    NavigationItemCurrent = $"{positioning.Relative} {overflow.Hidden} before:absolute before:inset-y-0 before:start-0 before:w-0.5 before:bg-linear-to-b before:from-brickblue-600 before:to-brickorange-500 forced-colors:before:bg-[Highlight]",
                    NavigationItemParent = typography.Weight.Semibold,
                    NavigationItemIcon = flexbox.ShrinkNone,
                    NavigationItemContent = $"{display.InlineFlex} {flexbox.Align.Center} {flexbox.ShrinkNone} {spacing.PushEnd}",
                    NavigationDropdownContainer = neutralSurface.BackgroundSubtle,
                    NavigationItemActiveLevelDeep = $"border-s-2 {border.Neutral.Strong}",
                    NavigationDropdownContainerDeep = string.Empty,
                    NavigationGroupRailDeep = $"border-s-2 {border.Neutral.Strong}",
                    MainContent = $"overflow-y-auto scrollbar-gutter-stable start-0 {flexbox.Flex1} {sizing.FullWidth} {focusRingInset} {interaction.FocusOutlineNone} transition-[margin] {transition.DurationFast} motion-reduce:transition-none {transition.EaseInOut}",
                    MainContentRoot = $"{display.Flex} overflow-x-hidden {flexbox.Col} h-dvh {sizing.FullWidth} {transparentBackground} {neutralSurface.Background} {neutralText.Heading} transition-[margin] {transition.DurationFast} motion-reduce:transition-none {transition.EaseInOut}"
                },
                new TwTreeListTheme
                {
                    Container = $"{display.Flex} {flexbox.Col} {typography.Size.Sm}",
                    Group = $"{display.Flex} {flexbox.Col} gap-0.5 mt-0.5 ms-3 ps-2 border-s {neutralSurface.Border}",
                    Row = $"group/row {display.Flex} {flexbox.Align.Center} {spacing.Gap.Sm} {sizing.MinWidthNone} {rounded.Md} px-2 py-1.5 {typography.Size.Sm} {neutralText.Heading} {neutralRowHover} [li:focus-visible>&]:ring-inset [li:focus-visible>&]:ring-2 [li:focus-visible>&]:ring-brickblue-500 dark:[li:focus-visible>&]:ring-brickblue-400 forced-colors:[li:focus-visible>&]:outline-2 forced-colors:[li:focus-visible>&]:-outline-offset-2 forced-colors:[li:focus-visible>&]:outline-[Highlight] {transition.ColorsFast} {interaction.PointerCursor}",
                    RowDisabled = $"{interaction.DisabledOpacity} {interaction.PointerEventsNone}",
                    ToggleSlot = $"{display.InlineFlex} {flexbox.ShrinkNone} {flexbox.Align.Center} {flexbox.Justify.Center} size-6",
                    ToggleIcon = $"{display.InlineFlex} {flexbox.ShrinkNone} {flexbox.Align.Center} {flexbox.Justify.Center} size-6 {rounded.Full} {neutralText.Muted} {neutralHoverFill} {neutralHoverText} transition-[transform,background-color,color] {transition.DurationFast} motion-reduce:transition-none",
                    ToggleIconOpen = "rotate-90 rtl:-rotate-90",
                    Label = "truncate",
                    ItemIcon = $"{display.InlineFlex} {flexbox.ShrinkNone} {flexbox.Align.Center} {flexbox.Justify.Center} {sizing.Icon.Sm} {neutralText.Muted}",
                    CheckboxWrapper = $"{display.InlineFlex} {flexbox.Align.Center} {flexbox.ShrinkNone} h-4",
                    ItemOutline = "outline-hidden"
                },
                new TwRadioButtonTheme
                {
                    Colors = checkBoxRadioButtonColors,
                    Base = $"peer {sizing.Icon.Md} {rounded.Full} {borderWidth.Thick} {neutralSurface.BorderStrong} {focusRing} {interaction.FocusOutlineNone} {transition.ColorsFast} {transition.EaseInOut} {interaction.PointerCursor} appearance-none",
                    Disabled = $"{interaction.DisabledOpacity} {interaction.DisabledCursor}",
                    Hover = $"hover:border-[oklch(15%_0_0)]/40 dark:hover:border-[oklch(97.807%_0_0)]/40 {interaction.PointerCursor}",
                    LabelBase = $"{display.Flex} {positioning.Relative} {flexbox.Align.Center} {spacing.Gap.Md} min-h-[24px] {inputLabelText} select-none",
                    LabelInteractiveCursor = interaction.PointerCursor,
                    LabelNonInteractiveCursor = interaction.PointerEventsNone,
                    LabelDisabled = interaction.DisabledOpacity,
                    IconWrapper = $"{positioning.Absolute} start-0 {display.Flex} {flexbox.Align.Center} {flexbox.Justify.Center} {sizing.Icon.Md} opacity-0 peer-checked:opacity-100 {interaction.PointerEventsNone}"
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
                    Base = $"peer {positioning.Absolute} inset-0 z-20 {sizing.Full} m-0 {transparentBackground} appearance-none {interaction.PointerCursor} {interaction.FocusOutlineNone} focus-visible:outline-hidden touch-manipulation",
                    Track = $"{positioning.Absolute} inset-x-0 top-1/2 {overflow.Hidden} h-2 {neutralTrackStrong} {borderWidth.Thin} {neutralSurface.BorderStrong} {rounded.Full} -translate-y-1/2 {interaction.PointerEventsNone}",
                    Fill = $"{sizing.FullHeight} forced-colors:bg-[Highlight]",
                    Thumb = $"{positioning.Absolute} top-1/2 z-10 {sizing.Icon.Md} bg-white dark:bg-gray-100 {rounded.Full} {borderWidth.Thick} -translate-x-1/2 rtl:translate-x-1/2 -translate-y-1/2 {shadows.Md} ring-1 ring-black/5 peer-hover:scale-110 peer-active:scale-95 {transition.TransformFast} {transition.EaseInOut} {interaction.PointerEventsNone}",
                    Bubble = $"{positioning.Absolute} bottom-full z-10 mb-2 px-2 py-1 whitespace-nowrap {typography.Size.Xs} {typography.Weight.Medium} tabular-nums {overlayTheme.Popover.Background} {neutralText.Heading} {overlayTheme.Popover.Border} {rounded.Md} -translate-x-1/2 rtl:translate-x-1/2 {shadows.Lg} opacity-0 scale-95 peer-hover:opacity-100 peer-hover:scale-100 peer-focus-visible:opacity-100 peer-focus-visible:scale-100 transition-[opacity,transform] {transition.DurationFast} motion-reduce:transition-none {transition.EaseInOut} {interaction.PointerEventsNone}"
                },
                new TwStepperTheme
                {
                    HorizontalList = $"hidden sm:flex sm:items-start {sizing.FullWidth}",
                    VerticalList = $"{display.Flex} {flexbox.Col} {sizing.FullWidth}",
                    Step = $"{display.Flex} {flexbox.Col} {flexbox.Align.Center} {spacing.Gap.Sm} shrink-0 {typography.AlignCenter}",
                    VerticalStep = $"group {display.Flex} {spacing.Gap.Lg}",
                    VerticalIndicatorColumn = $"{display.Flex} {flexbox.Col} {flexbox.Align.Center}",
                    VerticalContentColumn = $"{display.Flex} {flexbox.Col} {spacing.Gap.Sm} {flexbox.Flex1} {sizing.MinWidthNone} pt-2.5 pb-8 group-last:pb-0",
                    Connector = $"{flexbox.Flex1} h-0.5 mt-[19px] mx-1 border-t-2 {rounded.Full} {transition.ColorsSlow}",
                    VerticalConnector = $"{flexbox.Flex1} w-0 min-h-6 my-1 border-s-2 {rounded.Full} {transition.ColorsSlow}",
                    ConnectorNeutral = neutralSurface.Border,
                    Circle = $"{display.Flex} {anchor.Center} shrink-0 {sizing.Icon.Xl} {rounded.Full} {typography.Weight.Semibold} {typography.Size.Sm} {transition.ColorsFast} {interaction.FocusOutlineNone}",
                    CircleUpcoming = $"{neutralSurface.Background} {neutralText.Muted} border-2 {neutralSurface.BorderStrong}",
                    CircleDisabled = $"{interaction.DisabledOpacity} {interaction.DisabledCursor}",
                    CircleClickable = interaction.PointerCursor,
                    CircleFrame = $"{display.InlineFlex} shrink-0 p-0.5 border-2 {rounded.Full} {transition.ColorsFast}",
                    CircleFrameInactive = "border-transparent",
                    CircleUpcomingClickable = $"{neutralRowHover} hover:border-[oklch(15%_0_0)]/45 dark:hover:border-[oklch(97.807%_0_0)]/45 {neutralHoverText}",
                    LabelActive = typography.Weight.Semibold,
                    CircleIcon = "leading-none",
                    Label = $"{typography.Size.Sm} {typography.Weight.Medium} {neutralText.Heading}",
                    LabelUpcoming = $"{typography.Size.Sm} {typography.Weight.Medium} {neutralText.Muted}",
                    Description = $"{typography.Size.Xs} {neutralText.Subtle}",
                    Content = $"{spacing.MarginTop.Xl} {spacing.Padding.Xl} {borderWidth.Thin} {neutralSurface.Border} {rounded.Md}",
                    VerticalContent = "pt-2",
                    MobileContainer = $"{display.Flex} sm:hidden {flexbox.Col} {spacing.Gap.Sm} {sizing.FullWidth}",
                    MobileLabel = $"{typography.Size.Sm} {typography.Weight.Medium} {neutralText.Heading}",
                    MobileProgressLabel = display.ScreenReaderOnly
                },
                new TwSwitchTheme
                {
                    Colors = new()
                    {
                        Primary = "peer-checked:bg-brickblue-600 dark:peer-checked:bg-brickblue-500",
                        Accent = "peer-checked:bg-brickorange-600 dark:peer-checked:bg-brickorange-500",
                        Success = "peer-checked:bg-green-600 dark:peer-checked:bg-green-500",
                        Danger = "peer-checked:bg-red-600 dark:peer-checked:bg-red-500",
                        Warning = "peer-checked:bg-yellow-600 dark:peer-checked:bg-yellow-500",
                        Info = "peer-checked:bg-blue-600 dark:peer-checked:bg-blue-500",
                        Light = "peer-checked:bg-white dark:peer-checked:bg-gray-300",
                        Dark = "peer-checked:bg-gray-900 dark:peer-checked:bg-gray-800",
                    },
                    Switch = $"{positioning.Absolute} top-1/2 start-0.5 {sizing.Icon.Md} bg-white {rounded.Full} -translate-y-1/2 {shadows.Md} ring-1 ring-black/15 forced-colors:bg-[ButtonText] peer-checked:translate-x-full rtl:peer-checked:-translate-x-full {transition.TransformFast} {transition.EaseInOut}",
                    Track = $"{positioning.Absolute} inset-0 bg-[oklch(88%_0_0)] dark:bg-[oklch(34%_0_0)] {rounded.Full} {neutralHairlineRing} {borderWidth.Thin} {neutralSurface.BorderStrong} forced-colors:peer-checked:bg-[Highlight] peer-disabled:opacity-40 transition-[background-color,opacity] {transition.DurationFast} motion-reduce:transition-none {transition.EaseInOut} peer-disabled:pointer-events-none",
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
                        Primary = "border-t-brickblue-600 dark:border-t-brickblue-500",
                        Accent = "border-t-brickorange-600 dark:border-t-brickorange-500",
                        Success = "border-t-green-600 dark:border-t-green-500",
                        Danger = "border-t-red-600 dark:border-t-red-500",
                        Warning = "border-t-yellow-500 dark:border-t-yellow-500",
                        Info = "border-t-blue-600 dark:border-t-blue-500",
                        Light = "border-t-white",
                        Dark = "border-t-gray-900",
                    },
                    Wrapper = $"{display.InlineFlex} {flexbox.Align.Center} {spacing.Gap.Md}",
                    Base = $"{display.InlineBlock} {rounded.Full} animate-spin motion-reduce:[animation-duration:3s]",
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
                    TextfieldPadding = "ps-10 pe-3",
                    NativeInputAppearance = "appearance-none !flex items-center [&::-webkit-date-and-time-value]:min-h-0 [&::-webkit-date-and-time-value]:text-start [&::-webkit-datetime-edit]:p-0",
                    PanelWrapper = $"{positioning.Absolute} top-full start-0 z-120 {spacing.MarginTop.Md}",
                    RangeStageTabsContainer = $"{display.Flex} {spacing.Gap.Sm} {spacing.Padding.Sm} {rounded.Lg} {neutralTrack} {spacing.MarginBottom.Md}",
                    RangeStageTabInactive = neutralText.Subtle,
                    StageTabBase = $"{flexbox.Flex1} py-1.5 {typography.Size.Sm} {typography.Weight.Medium} {typography.AlignCenter} {interaction.PointerCursor} {transition.ColorsFast} {focusRing} {interaction.FocusOutlineNone}",
                    StageTabActive = $"bg-white dark:bg-[oklch(34%_0_0)] {neutralText.Heading} {shadows.Sm}",
                    StageTabHover = neutralHoverText,
                    BodySurface = $"{spacing.Padding.Xl} {typography.AlignCenter} {typography.Weight.Medium} {neutralText.Heading}",
                    BodyRoot = "",
                    BodyInner = $"{spacing.Padding.Md} {typography.AlignCenter} {typography.Weight.Medium} {neutralText.Heading}",
                    ContentRow = $"{display.Flex} {anchor.Center} {spacing.Gap.Lg}",
                    Column = $"{display.Flex} {flexbox.Col} {flexbox.Align.Center} {spacing.Gap.Sm}",
                    StepButton = $"{neutralText.Subtle} hover:text-brickblue-600 dark:hover:text-brickblue-400 {interaction.PointerCursor}",
                    NumberWrapper = $"{display.Flex} {flexbox.Align.Center} {flexbox.Justify.Center}",
                    NumberInput = $"w-12 py-1 {typography.AlignCenter} {typography.Size.Lg} {typography.Weight.Semibold} {neutralText.Heading} {transition.ColorsFast} {rounded.Md} {focusRing} {interaction.FocusOutlineNone}",
                    Separator = $"self-center px-1 {typography.Size.Lg} {typography.Weight.Semibold} {neutralText.Subtle}",
                    AmPmWrapper = $"{display.Flex} {flexbox.Align.Center} ms-2",
                    AmPmButtonClass = "min-w-12"
                },
                new TwTableTheme
                {
                    Wrapper = $"{positioning.Relative} overflow-auto focus-visible:outline-2 focus-visible:-outline-offset-2 focus-visible:outline-brickblue-600 dark:focus-visible:outline-brickblue-300",
                    ContainerClip = overflow.Hidden,
                    Base = $"{sizing.FullWidth} {typography.Size.Sm} text-start {neutralText.Heading}",
                    Bordered = $"{borderWidth.Thin} {neutralSurface.Border}",
                    Header = $"{typography.Size.Sm} {typography.Weight.Semibold} {neutralText.Secondary} {neutralSurface.BackgroundSubtle} border-b {neutralSurface.Border}",
                    Body = neutralSurface.Background,
                    BodyStriped = "[&>tr:nth-child(even)]:bg-[oklch(98%_0_0)] [&>tr:nth-child(even)]:dark:bg-[oklch(23.26%_0_0)]",
                    BodyHoverable = "[&>tr:hover]:!bg-[oklch(95%_0_0)] [&>tr:hover]:dark:!bg-[oklch(21.15%_0_0)] [&>tr]:transition-colors [&>tr]:duration-200 [&>tr]:motion-reduce:transition-none",
                    RowDivider = "divide-y divide-[oklch(94%_0_0)] dark:divide-[oklch(21.15%_0_0)]",
                    Footer = $"border-t {neutralSurface.Border}",
                    SearchColumn = "md:w-1/2",
                    SortIcon = $"{typography.Size.Sm} {neutralText.Subtle} {neutralHoverText}",
                    HeaderCellPadding = "px-4 py-2.5",
                    BodyCellPadding = $"px-4 py-2.5 {typography.WrapBreakWord}",
                    EmptyState = $"py-8 {typography.AlignCenter} {neutralText.Muted}"
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
                    Container = $"{positioning.Fixed} bottom-0 end-4 z-140 {display.Flex} {flexbox.Col} {spacing.Gap.Md} max-w-md {spacing.Padding.Xl}",
                    Toast = $"{display.Flex} {flexbox.Align.Start} {spacing.Gap.Md} {spacing.Padding.Xl} {shadows.Lg} {transition.ColorsFast} {transition.EaseInOut}",
                    ToastWidth = "w-[300px] max-w-full",
                    Timestamp = $"{typography.Size.Xs} opacity-70",
                    CloseButton = $"{flexbox.ShrinkNone} {spacing.Padding.Sm} {neutralHoverFill} {rounded.Full} focus-visible:ring-2 focus-visible:ring-current {transition.ColorsFast} {interaction.FocusOutlineNone}"
                },
                new TwCalendarTheme
                {
                    Container = $"{display.Flex} {flexbox.Col} {sizing.FullWidth} {spacing.Gap.Lg}",
                    Header = $"{display.Flex} {flexbox.Align.Center} {flexbox.Justify.Between} {flexbox.Wrap} {spacing.Gap.Lg} pb-4 border-b {neutralSurface.Border}",
                    HeaderDateGroup = $"{display.Flex} {flexbox.Col} {spacing.Gap.Sm}",
                    HeaderTitleRow = $"{display.Flex} {flexbox.Align.Center} {spacing.Gap.Md}",
                    HeaderTitle = $"{typography.Size.Lg} {typography.Weight.Semibold} {neutralText.Heading}",
                    HeaderSubtitle = $"{typography.Size.Xs} {neutralText.Muted}",
                    NavButton = $"{display.Flex} {anchor.Center} size-8 {rounded.Full} {neutralSurface.Hover} {focusRing} {interaction.FocusOutlineNone} {transition.ColorsFast}",
                    ViewButtonActive = $"{background.Lightest.Primary} {darkBackground.Dark.Primary}",
                    AddButton = "shrink-0",
                    SearchButton = neutralText.Subtle,
                    ScrollContainer = "overflow-y-auto",
                    TimeGutter = $"{positioning.Relative} {display.Flex} {flexbox.Col} {flexbox.ShrinkNone} w-10 sm:w-16 text-end {spacing.Padding.Sm}",
                    TimeGutterLabel = $"text-[10px] sm:text-xs {neutralText.Subtle} -translate-y-2",
                    NowIndicatorLabel = $"{positioning.Absolute} end-1 sm:end-2 -translate-y-1/2 text-[10px] sm:text-xs {typography.Weight.Semibold} {text.Medium.Primary} {darkText.Light.Primary} {neutralSurface.Background} px-1 {rounded.Sm} {interaction.PointerEventsNone}",
                    NowIndicatorLine = $"{positioning.Absolute} inset-x-0 border-t-2 {borderColors.Primary} {interaction.PointerEventsNone} z-10",
                    NowIndicatorDot = $"{positioning.Absolute} -translate-x-1/2 rtl:translate-x-1/2 -translate-y-1/2 size-2.5 {rounded.Full} {background.Medium.Primary} {interaction.PointerEventsNone} z-10",
                    SlotRow = $"h-12 border-t {neutralSurface.BorderSubtle}",
                    SlotRowHover = $"{neutralSurface.Hover} {interaction.PointerCursor}",
                    DayColumnHeader = $"{sizing.MinWidthNone} {typography.Size.Xs} {typography.Weight.Semibold} {typography.AlignCenter} {neutralText.Secondary} py-2 border-b {neutralSurface.Border} {focusRingInset} {interaction.FocusOutlineNone}",
                    DayColumnHeaderLabel = "sr-only sm:not-sr-only",
                    DayColumnHeaderCompact = "gap-0.5 sm:hidden",
                    DayColumnHeaderWeekday = $"text-[10px] {typography.Weight.Medium} {textTransform.Uppercase} {neutralText.Subtle}",
                    DayColumnHeaderDayNumber = $"{display.Flex} {anchor.Center} size-7 {rounded.Full} {typography.Size.Sm} {neutralText.Heading}",
                    DayColumnHeaderDayNumberToday = $"{background.Medium.Primary} {text.Medium.Light}",
                    DayColumnDivider = $"border-s {neutralSurface.BorderSubtle}",
                    EventChip = $"{positioning.Absolute} {overflow.Hidden} {rounded.Sm} {display.Flex} {flexbox.Col} {flexbox.Align.Start} text-start {borderWidth.AccentEdge} {borderColors.Primary} {background.Lightest.Primary} {darkBackground.Dark.Primary} {text.Dark.Primary} {darkText.Light.Primary} {spacing.Padding.Sm} {interaction.PointerCursor} {focusRing} {interaction.FocusOutlineNone} {transition.ColorsFast}",
                    EventChipTitle = $"{typography.Size.Xs} {typography.Weight.Semibold} {typography.WrapBreakWord}",
                    EventChipTime = $"{typography.Size.Xs} opacity-75",
                    EventChipCompact = "max-sm:border-s-2 max-sm:px-0.5",
                    EventChipTitleCompact = "max-sm:text-[10px] max-sm:leading-tight",
                    EventChipTimeCompact = "max-sm:hidden",
                    EventChipReadOnly = $"{interaction.ReadonlyCursor} opacity-90",
                    EventChipDraggable = "cursor-grab active:cursor-grabbing",
                    DropPlaceholder = $"{overflow.Hidden} {rounded.Sm} border-2 border-dashed {borderColors.Primary} {background.Lightest.Primary} {darkBackground.Dark.Primary} {text.Dark.Primary} {darkText.Light.Primary} {typography.Size.Xs} opacity-70 z-10",
                    TodayHighlight = $"animate-pulse motion-reduce:animate-none [animation-iteration-count:1] {background.Light.Primary} dark:bg-brickblue-500/50",
                    AllDayRow = $"border-b {neutralSurface.Border}",
                    AllDayLabel = $"{flexbox.ShrinkNone} w-10 sm:w-16 {spacing.Padding.Sm} text-end text-[10px] sm:text-xs {neutralText.Subtle}",
                    AllDayGrid = "gap-y-1 py-1",
                    AllDayChipInset = "mx-0.5",
                    SegmentSpan = $"{positioning.Relative} z-10",
                    SegmentContinuesBefore = "rounded-s-none",
                    SegmentContinuesAfter = "rounded-e-none",
                    SegmentSpacer = $"h-5 {flexbox.ShrinkNone}",
                    MonthGrid = $"{sizing.FullWidth} border-collapse table-fixed",
                    MonthCell = $"align-top {spacing.Padding.Sm} max-sm:p-0.5 border {neutralSurface.BorderSubtle} h-24",
                    MonthCellPrevNext = $"{neutralSurface.BackgroundSubtle} {neutralText.Subtle}",
                    MonthDayNumber = $"{display.Block} {sizing.FullWidth} text-start {typography.Size.Xs} {typography.Weight.Medium} {neutralText.Heading}",
                    MonthEventList = $"{display.Flex} {flexbox.Col} {spacing.Gap.Sm} mt-1",
                    MonthEventRow = $"{display.Flex} {flexbox.Align.Center} {spacing.Gap.Sm} {sizing.FullWidth} {overflow.Hidden} {rounded.Sm} text-start max-sm:border-s-2 max-sm:px-0.5 {borderWidth.AccentEdge} {borderColors.Primary} {background.Lightest.Primary} {darkBackground.Dark.Primary} {text.Dark.Primary} {darkText.Light.Primary} px-1.5 py-0.5 {interaction.PointerCursor} {focusRing} {interaction.FocusOutlineNone} {transition.ColorsFast}",
                    MonthEventName = $"{typography.Size.Xs} max-sm:text-[10px] {typography.Weight.Semibold} truncate max-sm:text-clip",
                    MonthEventTime = $"{typography.Size.Xs} opacity-75 {flexbox.ShrinkNone} max-sm:hidden",
                    MonthOverflowLabel = $"{typography.Size.Xs} max-sm:text-[10px] text-start whitespace-nowrap {neutralText.Subtle} {interaction.PointerCursor}",
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
}