using AngleSharp.Dom;
using Bunit;
using Microsoft.AspNetCore.Components;
using TwBlazor.Components;
using TwBlazor.Components.DatePicker;
using TwBlazor.Components.TimePicker;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;

namespace TwBlazor.Tests.Components;

/// <summary>
/// A component's Rounded must reach the parts drawn inside it (tabs, picker buttons, tracks, toolbars), not
/// leave them on the theme's default or a fixed radius.
/// </summary>
public class RoundedFollowThroughTests : TwBlazorTestBase
{
    public sealed record Product(int Id, string Name);

    public RoundedFollowThroughTests()
    {
        // The default differs from the value under test, so a part ignoring the instance value shows up.
        Theme.Rounded.DefaultRounded = Rounded.Lg;
        Theme.Components.Require<TwInputTheme>().DefaultInputVariant = InputVariant.Outlined;
        TestContext.JSInterop.Mode = JSRuntimeMode.Loose;
    }

    private static void AssertRadius(IElement element, string expected)
    {
        var radius = element.ClassList.Where(c => c == "rounded" || c.StartsWith("rounded-")).ToList();
        Assert.Equal([expected], radius);
    }

    [Theory]
    [InlineData(Rounded.None, "rounded-none", "rounded-none")]
    [InlineData(Rounded.Lg, "rounded-lg", "rounded")]
    [InlineData(Rounded.Full, "rounded-full", "rounded-full")]
    public void IsolatedTabs_BarTakesTheRounding_AndEachTabIsOneStepTighter(Rounded rounded, string bar, string tab)
    {
        var cut = TestContext.Render<TwTabContainer>(p => p
            .Add(x => x.Rounded, rounded)
            .AddChildContent<TwTab>(t => t.Add(x => x.Label, "One")));

        AssertRadius(cut.Find("[role='tablist']"), bar);
        AssertRadius(cut.Find("[role='tab']"), tab);
    }

    [Fact]
    public void Carousel_IndicatorBar_FollowsTheRounding()
    {
        var cut = TestContext.Render<TwCarousel>(p => p
            .Add(x => x.Rounded, Rounded.None)
            .Add(x => x.ChildContent, ThreeSlides()));

        var theme = Theme.Components.Require<TwCarouselTheme>();
        var bar = cut.FindAll("div").Single(d => d.ClassName?.StartsWith(theme.IndicatorContainer) == true);
        AssertRadius(bar, "rounded-none");
    }

    [Fact]
    public void Progress_BarAndShimmer_FollowTheRounding()
    {
        var cut = TestContext.Render<TwProgress<int>>(p => p
            .Add(x => x.Rounded, Rounded.None)
            .Add(x => x.Animated, true)
            .Add(x => x.Value, 40)
            .Add(x => x.Max, 100));

        AssertRadius(cut.Find("progress"), "rounded-none");
        AssertRadius(cut.Find("progress + span"), "rounded-none");
    }

    [Fact]
    public void Slider_TrackFollowsTheRounding()
    {
        var cut = TestContext.Render<TwSlider<int>>(p => p
            .Add(x => x.Rounded, Rounded.None)
            .Add(x => x.Value, 5)
            .Add(x => x.Max, 10));

        var track = cut.Find("div[aria-hidden='true']");
        AssertRadius(track, "rounded-none");
        AssertRadius(track.FirstElementChild!, "rounded-none");
    }

    [Fact]
    public void RangeSlider_TrackFollowsTheRounding()
    {
        var cut = TestContext.Render<TwRangeSlider<int>>(p => p
            .Add(x => x.Rounded, Rounded.None)
            .Add(x => x.LowerValue, 2)
            .Add(x => x.UpperValue, 8)
            .Add(x => x.Max, 10));

        var track = cut.Find("div[aria-hidden='true']");
        AssertRadius(track, "rounded-none");
        AssertRadius(track.FirstElementChild!, "rounded-none");
    }

    [Fact]
    public void DataTable_SearchAndPagination_FollowTheRounding()
    {
        var items = Enumerable.Range(1, 12).Select(i => new Product(i, $"Item {i}")).ToList();

        var cut = TestContext.Render<TwDataTable<Product>>(p => p
            .Add(x => x.Id, "products")
            .Add(x => x.Items, items)
            .Add(x => x.Searchable, true)
            .Add(x => x.RowsPerPage, 5)
            .Add(x => x.Rounded, Rounded.None));

        AssertRadius(cut.Find("#products-search"), "rounded-none");
        var pageButtons = cut.FindAll("nav ul li button");
        Assert.NotEmpty(pageButtons);
        Assert.All(pageButtons, b => Assert.Contains("rounded-none", b.ClassList));
    }

    [Fact]
    public void DatePickerCalendar_DaysAndTitleButton_FollowTheRounding()
    {
        var cut = TestContext.Render<TwDatePickerCalendar>(p => p
            .Add(x => x.Rounded, Rounded.Full)
            .Add(x => x.AnchorDate, new DateTime(2026, 3, 15)));

        Assert.Contains("rounded-full", cut.Find("button.view-switch").ClassList);
        Assert.All(cut.FindAll("button.day"), d => AssertRadius(d, "rounded-full"));
    }

    [Fact]
    public void TimePickerBody_ButtonsFollowTheRounding()
    {
        var cut = TestContext.Render<TwTimePickerBody>(p => p
            .Add(x => x.Rounded, Rounded.None)
            .Add(x => x.Is12HourFormat, true)
            .Add(x => x.ShowActions, true));

        var amPm = cut.FindAll("button[aria-pressed]");
        Assert.Equal(2, amPm.Count);
        Assert.All(amPm, b => AssertRadius(b, "rounded-none"));
        AssertRadius(cut.Find("div[role='group']"), "rounded-none");

        var actions = cut.FindAll("button").Where(b => b.TextContent.Trim() is "Now" or "Done").ToList();
        Assert.Equal(2, actions.Count);
        Assert.All(actions, b => Assert.Contains("rounded-none", b.ClassList));
    }

    [Fact]
    public void IconButton_IsRoundByDefault_AndFollowsAnExplicitRounded()
    {
        var round = TestContext.Render<TwIcon>(p => p
            .Add(x => x.Icon, TwBlazor.Enums.Icon.Plus)
            .Add(x => x.AriaLabel, "Add")
            .Add(x => x.OnClick, () => { }));
        var square = TestContext.Render<TwIcon>(p => p
            .Add(x => x.Icon, TwBlazor.Enums.Icon.Plus)
            .Add(x => x.AriaLabel, "Add")
            .Add(x => x.Rounded, Rounded.None)
            .Add(x => x.OnClick, () => { }));

        AssertRadius(round.Find("button"), "rounded-full");
        AssertRadius(square.Find("button"), "rounded-none");
    }

    [Fact]
    public void Carousel_ArrowsAndDots_FollowTheRounding()
    {
        var cut = TestContext.Render<TwCarousel>(p => p
            .Add(x => x.Id, "slides")
            .Add(x => x.Rounded, Rounded.None)
            .Add(x => x.ChildContent, ThreeSlides()));

        AssertRadius(cut.Find("#slides-previous"), "rounded-none");
        AssertRadius(cut.Find("#slides-next"), "rounded-none");
        var dots = cut.FindAll("button[aria-label^='Slide']");
        Assert.Equal(3, dots.Count);
        Assert.All(dots, d => AssertRadius(d, "rounded-none"));
    }

    [Fact]
    public void DatePickerHeader_ArrowsAndTopCorners_FollowTheRounding()
    {
        var cut = TestContext.Render<TwDatePickerCalendar>(p => p
            .Add(x => x.Rounded, Rounded.None)
            .Add(x => x.AnchorDate, new DateTime(2026, 3, 15)));

        Assert.Contains("rounded-t-none", cut.Find(".datepicker-header").ClassList);
        AssertRadius(cut.Find(".prev-btn button, button.prev-btn"), "rounded-none");
        AssertRadius(cut.Find(".next-btn button, button.next-btn"), "rounded-none");
    }

    [Fact]
    public void Numeric_InnerButtons_AreOneStepTighterThanTheField()
    {
        var cut = TestContext.Render<TwNumeric<int>>(p => p
            .Add(x => x.Rounded, Rounded.Lg)
            .Add(x => x.Clearable, true)
            .Add(x => x.Value, 3));

        var buttons = cut.FindAll("button");
        Assert.Equal(3, buttons.Count);
        Assert.All(buttons, b => AssertRadius(b, "rounded"));
    }

    private static RenderFragment ThreeSlides() => builder =>
    {
        for (var i = 0; i < 3; i++)
        {
            var text = $"Slide {i + 1}";
            builder.OpenComponent<TwCarouselItem>(0);
            builder.AddAttribute(1, "ChildContent", (RenderFragment)(b => b.AddContent(2, text)));
            builder.CloseComponent();
        }
    };
}
