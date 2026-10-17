// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using TwBlazor.Components;
using TwBlazor.Components.DatePicker;
using TwBlazor.Components.TimePicker;
using TwBlazor.Configuration;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Localization;
using ThemeBase = TwBlazor.Theme.Theme;

namespace TwBlazor.Tests.Localization;

[Collection("ServiceCollectionExtensions")]
public partial class RightToLeftTests
{
    // Classes that are deliberately physical: a bubble centered on its control, the arrow under or over it,
    // an indicator pinned to both sides at once, and charts, which are not mirrored.
    private static readonly HashSet<string> _physicalByDesign =
    [
        "TwTooltipTheme.Top",
        "TwTooltipTheme.Bottom",
        "TwTooltipTheme.ArrowTop",
        "TwTooltipTheme.ArrowBottom",
        "TwTabTheme.ActiveIndicator",
        "TwTabTheme.InactiveIndicator",
        "TwSidebarTheme.NavbarFixedInset"
    ];

    private sealed class CultureOnlyLocalizer : ITwLocalizer
    {
        public string this[string key] => key;

        public string this[string key, params object?[] arguments] => key;

        public TwDateTimeFormats Formats => throw new NotSupportedException();
    }

    private static BunitContext CreateContext(TextDirection direction)
    {
        var context = new BunitContext();
        context.Services.AddTwBlazor(options =>
        {
            options.Localization.Culture = TestCultures.British;
            options.Localization.Direction = direction;
        }, ThemeBase.CreateDefaultTheme());
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        return context;
    }

    private static TwLocalizer CreateLocalizer(string culture, TextDirection? direction = null)
    {
        var options = new TwBlazorOptions { Theme = ThemeBase.CreateDefaultTheme() };
        options.Localization.Culture = CultureInfo.GetCultureInfo(culture);
        options.Localization.Direction = direction;
        return new TwLocalizer(options);
    }

    private static RenderFragment ThreeTabs() => builder =>
    {
        for (var i = 1; i <= 3; i++)
        {
            var label = $"Tab {i}";
            builder.OpenComponent<TwTab>(0);
            builder.SetKey(label);
            builder.AddAttribute(1, "Label", label);
            builder.AddAttribute(2, "ChildContent", (RenderFragment)(content => content.AddContent(0, label)));
            builder.CloseComponent();
        }
    };

    private static RenderFragment Folder(bool collapsed) => builder =>
    {
        builder.OpenComponent<TwTreeListItem>(0);
        builder.AddComponentParameter(1, "Label", "src");
        builder.AddComponentParameter(2, "Collapsed", collapsed);
        builder.AddComponentParameter(3, "ChildContent", (RenderFragment)(children =>
        {
            children.OpenComponent<TwTreeListItem>(0);
            children.AddComponentParameter(1, "Label", "Program.cs");
            children.CloseComponent();
        }));
        builder.CloseComponent();
    };

    private static RenderFragment ThreeSlides() => builder =>
    {
        for (var i = 1; i <= 3; i++)
        {
            var text = $"Slide {i}";
            builder.OpenComponent<TwCarouselItem>(0);
            builder.SetKey(text);
            builder.AddAttribute(1, "ChildContent", (RenderFragment)(content => content.AddContent(0, text)));
            builder.CloseComponent();
        }
    };

    private static string IconOf(AngleSharp.Dom.IElement button) => button.QuerySelector("i")!.GetAttribute("class")!;

    [GeneratedRegex(@"^-?(ml|mr|pl|pr|left|right|border-l|border-r|rounded-l|rounded-r|rounded-tl|rounded-tr|rounded-bl|rounded-br|scroll-ml|scroll-mr|scroll-pl|scroll-pr)(-|$)|^(text|float|clear)-(left|right)$")]
    private static partial Regex PhysicalUtilityRegex();

    private static IEnumerable<(string Path, string Value)> ThemeStrings(object node, HashSet<object> visited)
    {
        if (!visited.Add(node))
        {
            yield break;
        }

        if (node is IEnumerable items)
        {
            foreach (var item in items)
            {
                if (item is not null && item is not string && item.GetType().Namespace?.StartsWith("TwBlazor", StringComparison.Ordinal) == true)
                {
                    foreach (var found in ThemeStrings(item, visited))
                    {
                        yield return found;
                    }
                }
            }
        }

        foreach (var property in node.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (property.GetIndexParameters().Length > 0)
            {
                continue;
            }

            var value = property.GetValue(node);
            if (value is string text)
            {
                yield return ($"{node.GetType().Name}.{property.Name}", text);
            }
            else if (value is not null && value.GetType().IsClass && value.GetType().Namespace?.StartsWith("TwBlazor", StringComparison.Ordinal) == true)
            {
                foreach (var found in ThemeStrings(value, visited))
                {
                    yield return found;
                }
            }
        }
    }

    [Theory]
    [InlineData("ar", TextDirection.RightToLeft)]
    [InlineData("ar-EG", TextDirection.RightToLeft)]
    [InlineData("he", TextDirection.RightToLeft)]
    [InlineData("fa", TextDirection.RightToLeft)]
    [InlineData("ur", TextDirection.RightToLeft)]
    [InlineData("en-GB", TextDirection.LeftToRight)]
    [InlineData("zh-Hans", TextDirection.LeftToRight)]
    public void Localizer_Direction_FollowsTheLanguage(string culture, TextDirection expected)
    {
        Assert.Equal(expected, CreateLocalizer(culture).Direction);
    }

    [Theory]
    [InlineData("en-GB", TextDirection.RightToLeft)]
    [InlineData("ar", TextDirection.LeftToRight)]
    public void Localizer_Direction_UsesTheConfiguredDirection_OverTheLanguage(string culture, TextDirection configured)
    {
        Assert.Equal(configured, CreateLocalizer(culture, configured).Direction);
    }

    [Theory]
    [InlineData("ar", TextDirection.RightToLeft)]
    [InlineData("en-GB", TextDirection.LeftToRight)]
    public void CustomLocalizer_ThatDoesNotImplementDirection_FollowsTheCurrentUICulture(string culture, TextDirection expected)
    {
        // Arrange
        var previous = CultureInfo.CurrentUICulture;
        ITwLocalizer localizer = new CultureOnlyLocalizer();

        try
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(culture);

            // Act & Assert
            Assert.Equal(expected, localizer.Direction);
        }
        finally
        {
            CultureInfo.CurrentUICulture = previous;
        }
    }

    [Theory]
    [InlineData(TextDirection.LeftToRight, "ArrowRight", "Tab 2")]
    [InlineData(TextDirection.LeftToRight, "ArrowLeft", "Tab 3")]
    [InlineData(TextDirection.RightToLeft, "ArrowLeft", "Tab 2")]
    [InlineData(TextDirection.RightToLeft, "ArrowRight", "Tab 3")]
    [InlineData(TextDirection.RightToLeft, "ArrowDown", "Tab 2")]
    [InlineData(TextDirection.RightToLeft, "ArrowUp", "Tab 3")]
    [InlineData(TextDirection.RightToLeft, "End", "Tab 3")]
    public void Tabs_ArrowKeys_FollowTheDirection(TextDirection direction, string key, string expectedTab)
    {
        // Arrange
        using var context = CreateContext(direction);
        var cut = context.Render<TwTabContainer>(p => p.Add(x => x.ChildContent, ThreeTabs()));

        // Act
        cut.Find("div[role='tablist']").KeyDown(new KeyboardEventArgs { Key = key });

        // Assert
        var selected = cut.FindAll("button[role='tab']").Single(tab => tab.GetAttribute("aria-selected") == "true");
        Assert.Equal(expectedTab, selected.TextContent.Trim());
    }

    [Theory]
    [InlineData(TextDirection.LeftToRight, "ArrowRight", "16")]
    [InlineData(TextDirection.LeftToRight, "ArrowLeft", "14")]
    [InlineData(TextDirection.RightToLeft, "ArrowLeft", "16")]
    [InlineData(TextDirection.RightToLeft, "ArrowRight", "14")]
    [InlineData(TextDirection.RightToLeft, "ArrowDown", "22")]
    public void DateGrid_ArrowKeys_FollowTheDirection(TextDirection direction, string key, string expectedDay)
    {
        // Arrange
        using var context = CreateContext(direction);
        var cut = context.Render<TwDatePickerDayView>(p => p
            .Add(x => x.Value, new DateTime(2025, 11, 15))
            .Add(x => x.ValueChanged, EventCallback.Factory.Create<DateTime>(this, _ => { })));

        // Act
        cut.Find("table[role='grid']").KeyDown(new KeyboardEventArgs { Key = key });

        // Assert
        var focused = cut.FindAll("button.day").Single(day => day.GetAttribute("tabindex") == "0");
        Assert.Equal(expectedDay, focused.TextContent.Trim());
    }

    [Theory]
    [InlineData(TextDirection.LeftToRight, "ArrowRight")]
    [InlineData(TextDirection.RightToLeft, "ArrowLeft")]
    public void Tree_TheKeyPointingAtTheChildren_ExpandsACollapsedNode(TextDirection direction, string key)
    {
        // Arrange
        using var context = CreateContext(direction);
        var cut = context.Render<TwTreeList>(p => p.Add(x => x.ChildContent, Folder(collapsed: true)));

        // Act
        cut.Find("[role='treeitem']").KeyDown(new KeyboardEventArgs { Key = key });

        // Assert
        Assert.Equal("true", cut.Find("[role='treeitem']").GetAttribute("aria-expanded"));
    }

    [Theory]
    [InlineData(TextDirection.LeftToRight, "ArrowLeft")]
    [InlineData(TextDirection.RightToLeft, "ArrowRight")]
    public void Tree_TheKeyPointingAwayFromTheChildren_CollapsesAnExpandedNode(TextDirection direction, string key)
    {
        // Arrange
        using var context = CreateContext(direction);
        var cut = context.Render<TwTreeList>(p => p.Add(x => x.ChildContent, Folder(collapsed: false)));

        // Act
        cut.Find("[role='treeitem']").KeyDown(new KeyboardEventArgs { Key = key });

        // Assert
        Assert.Equal("false", cut.Find("[role='treeitem']").GetAttribute("aria-expanded"));
    }

    [Theory]
    [InlineData(TextDirection.LeftToRight, "bi-chevron-right")]
    [InlineData(TextDirection.RightToLeft, "bi-chevron-left")]
    public void Tree_ToggleChevron_PointsAtTheChildren(TextDirection direction, string expectedIcon)
    {
        // Arrange
        using var context = CreateContext(direction);

        // Act
        var cut = context.Render<TwTreeList>(p => p.Add(x => x.ChildContent, Folder(collapsed: true)));

        // Assert
        Assert.Contains(cut.FindAll("i"), icon => icon.ClassList.Contains(expectedIcon));
    }

    [Theory]
    [InlineData(TextDirection.LeftToRight, "bi-chevron-left", "bi-chevron-right")]
    [InlineData(TextDirection.RightToLeft, "bi-chevron-right", "bi-chevron-left")]
    public void Pagination_PreviousAndNextChevrons_PointTheWayTheyMove(TextDirection direction, string previousIcon, string nextIcon)
    {
        // Arrange
        using var context = CreateContext(direction);

        // Act
        var cut = context.Render<TwPagination>(p => p
            .Add(x => x.TotalPages, 5)
            .Add(x => x.ActivePage, 2));

        // Assert
        Assert.Contains(previousIcon, IconOf(cut.Find("[aria-label='Previous page']")));
        Assert.Contains(nextIcon, IconOf(cut.Find("[aria-label='Next page']")));
    }

    [Theory]
    [InlineData(TextDirection.LeftToRight, "bi-chevron-left", "bi-chevron-right")]
    [InlineData(TextDirection.RightToLeft, "bi-chevron-right", "bi-chevron-left")]
    public void Carousel_PreviousAndNextChevrons_PointTheWayTheyMove(TextDirection direction, string previousIcon, string nextIcon)
    {
        // Arrange
        using var context = CreateContext(direction);

        // Act
        var cut = context.Render<TwCarousel>(p => p.Add(x => x.ChildContent, ThreeSlides()));

        // Assert
        Assert.Contains(previousIcon, IconOf(cut.Find("[aria-label='Previous slide']")));
        Assert.Contains(nextIcon, IconOf(cut.Find("[aria-label='Next slide']")));
    }

    [Theory]
    [InlineData(TextDirection.LeftToRight, "bi-arrow-left", "bi-arrow-right")]
    [InlineData(TextDirection.RightToLeft, "bi-arrow-right", "bi-arrow-left")]
    public void DatePickerHeader_PreviousAndNextArrows_PointTheWayTheyMove(TextDirection direction, string previousIcon, string nextIcon)
    {
        // Arrange
        using var context = CreateContext(direction);

        // Act
        var cut = context.Render<TwDatePickerHeader>(p => p
            .Add(x => x.OnPreviousClick, EventCallback.Factory.Create(this, () => { }))
            .Add(x => x.OnNextClick, EventCallback.Factory.Create(this, () => { }))
            .AddChildContent("November 2025"));

        // Assert
        Assert.Contains(previousIcon, IconOf(cut.Find(".prev-btn")));
        Assert.Contains(nextIcon, IconOf(cut.Find(".next-btn")));
    }

    [Theory]
    [InlineData(TextDirection.LeftToRight, "bi-chevron-right", "bi-chevron-left")]
    [InlineData(TextDirection.RightToLeft, "bi-chevron-left", "bi-chevron-right")]
    public void PickList_TransferChevrons_PointAtTheListTheyMoveTo(TextDirection direction, string toTargetIcon, string toSourceIcon)
    {
        // Arrange
        using var context = CreateContext(direction);

        // Act
        var cut = context.Render<TwPickList<string>>(p => p
            .Add(x => x.SourceItems, ["Apple", "Banana"])
            .Add(x => x.TargetItems, ["Cherry"])
            .Add(x => x.SourceLabel, "Available")
            .Add(x => x.TargetLabel, "Chosen"));

        // Assert
        Assert.Contains(toTargetIcon, IconOf(cut.Find("[aria-label='Move selected items to Chosen']")));
        Assert.Contains(toSourceIcon, IconOf(cut.Find("[aria-label='Move selected items to Available']")));
    }

    [Fact]
    public void Slider_PositionsItsThumbFromTheStartOfTheTrack()
    {
        // Arrange
        using var context = CreateContext(TextDirection.RightToLeft);

        // Act
        var cut = context.Render<TwSlider<int>>(p => p
            .Add(x => x.Value, 25)
            .Add(x => x.Min, 0)
            .Add(x => x.Max, 100));

        // Assert - a physical "left" offset would leave the thumb on the wrong side of a mirrored track
        var positioned = cut.FindAll("[style^='inset-inline-start:25%']");
        Assert.Equal(2, positioned.Count);
        Assert.Empty(cut.FindAll("[style^='left:']"));
    }

    [Fact]
    public void TimePicker_KeepsHoursBeforeMinutes()
    {
        // Arrange
        using var context = CreateContext(TextDirection.RightToLeft);

        // Act
        var cut = context.Render<TwTimePickerBody>();

        // Assert - a time is written hours first in every language, so the row is not mirrored
        var row = cut.Find("[dir='ltr']");
        Assert.Equal(2, row.QuerySelectorAll("input[type='text']").Length);
    }

    [Fact]
    public void DefaultTheme_UsesLogicalClasses_SoItMirrorsWithTheDirection()
    {
        // Arrange
        var theme = ThemeBase.CreateDefaultTheme();

        // Act
        var offenders = ThemeStrings(theme, new HashSet<object>(ReferenceEqualityComparer.Instance))
            .Where(entry => !entry.Path.StartsWith(nameof(TwChartTheme), StringComparison.Ordinal) && !_physicalByDesign.Contains(entry.Path))
            .SelectMany(entry => entry.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Where(token => !token.Contains("rtl:", StringComparison.Ordinal) && !token.Contains("ltr:", StringComparison.Ordinal))
                .Where(token => PhysicalUtilityRegex().IsMatch(token[(token.LastIndexOf(':') + 1)..]))
                .Select(token => $"{entry.Path}: {token}"))
            .Distinct()
            .ToList();

        // Assert
        Assert.True(offenders.Count == 0, "Use the logical class (ms-, pe-, start-, text-start, border-s, rounded-e) or pair it with rtl: " + string.Join(", ", offenders));
    }

    [Theory]
    [InlineData("ml-2", true)]
    [InlineData("-mr-px", true)]
    [InlineData("focus:left-2", true)]
    [InlineData("max-sm:border-l-2", true)]
    [InlineData("text-left", true)]
    [InlineData("rounded-r-none", true)]
    [InlineData("ms-2", false)]
    [InlineData("text-start", false)]
    [InlineData("border-s-4", false)]
    [InlineData("placeholder:text-gray-500", false)]
    [InlineData("leading-none", false)]
    public void PhysicalUtilityCheck_RecognisesPhysicalClasses(string token, bool expected)
    {
        Assert.Equal(expected, PhysicalUtilityRegex().IsMatch(token[(token.LastIndexOf(':') + 1)..]));
    }
}
