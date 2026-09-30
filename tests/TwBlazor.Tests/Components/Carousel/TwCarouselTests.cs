using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using TwBlazor.Components;
using TwBlazor.Enums;

namespace TwBlazor.Tests.Components.Carousel;

public class TwCarouselTests : TwBlazorTestBase
{
    private static RenderFragment ThreeSlides() => builder =>
    {
        builder.OpenComponent<TwCarouselItem>(0);
        builder.AddAttribute(1, "ChildContent", (RenderFragment)(b => b.AddContent(2, "Slide 1")));
        builder.CloseComponent();

        builder.OpenComponent<TwCarouselItem>(3);
        builder.AddAttribute(4, "ChildContent", (RenderFragment)(b => b.AddContent(5, "Slide 2")));
        builder.CloseComponent();

        builder.OpenComponent<TwCarouselItem>(6);
        builder.AddAttribute(7, "ChildContent", (RenderFragment)(b => b.AddContent(8, "Slide 3")));
        builder.CloseComponent();
    };

    private static RenderFragment SingleSlide() => builder =>
    {
        builder.OpenComponent<TwCarouselItem>(0);
        builder.AddAttribute(1, "ChildContent", (RenderFragment)(b => b.AddContent(2, "Only slide")));
        builder.CloseComponent();
    };

    #region rendering

    [Fact]
    public void TwCarousel_Renders_WithAriaAttributes()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCarousel>(p => p.Add(x => x.ChildContent, ThreeSlides()));

        // Assert
        var region = cut.Find("div[role='region']");
        Assert.Equal("carousel", region.GetAttribute("aria-roledescription"));
        Assert.Equal("Carousel", region.GetAttribute("aria-label"));

        var slide = cut.Find("div[role='group']");
        Assert.Equal("slide", slide.GetAttribute("aria-roledescription"));
        Assert.Equal("1 of 3", slide.GetAttribute("aria-label"));
    }

    [Fact]
    public void TwCarousel_SlideGroup_HasFullHeightAndWidthClasses_SoContentCanFillTheSlide()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCarousel>(p => p.Add(x => x.ChildContent, ThreeSlides()));
        var theme = Theme.Components.Require<TwBlazor.Configuration.Components.TwCarouselTheme>();

        // Assert
        var slide = cut.Find("div[role='group']");
        foreach (var themeClass in theme.SlideContent.Split(' '))
        {
            Assert.Contains(themeClass, slide.GetAttribute("class"));
        }
    }

    [Fact]
    public void TwCarousel_WithAriaLabel_UsesIt_OverDefault()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCarousel>(p => p
            .Add(x => x.AriaLabel, "Featured products")
            .Add(x => x.ChildContent, ThreeSlides()));

        // Assert
        Assert.Equal("Featured products", cut.Find("div[role='region']").GetAttribute("aria-label"));
    }

    [Fact]
    public void TwCarousel_OnlyRendersActiveSlide()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCarousel>(p => p.Add(x => x.ChildContent, ThreeSlides()));

        // Assert - check the rendered slide panel's own text, since the indicator dots' aria-labels
        // ("Slide 2 of 3") would otherwise produce a false-positive substring match against the markup.
        var panel = cut.Find("div[role='group']").TextContent;
        Assert.Contains("Slide 1", panel);
        Assert.DoesNotContain("Slide 2", panel);
        Assert.DoesNotContain("Slide 3", panel);
    }

    [Fact]
    public void TwCarousel_WithSingleSlide_HidesArrowsAndIndicators()
    {
        // Arrange & Act - arrows/indicators are meaningless with only one slide
        var cut = TestContext.Render<TwCarousel>(p => p.Add(x => x.ChildContent, SingleSlide()));

        // Assert
        Assert.Empty(cut.FindAll("button"));
    }

    [Fact]
    public void TwCarousel_IndicatorContainer_IsOverlaidInsideTheViewport()
    {
        // Arrange & Act - indicators are overlaid on top of the slide (like the arrow/pause buttons), not
        // rendered as a separate row below it.
        var cut = TestContext.Render<TwCarousel>(p => p.Add(x => x.ChildContent, ThreeSlides()));

        // Assert
        var region = cut.Find("div[role='region']");
        var viewport = region.Children[0];
        var indicatorContainer = cut.Find("div[role='group'][aria-label='Choose slide to display']");

        Assert.Contains("overflow-hidden", region.GetAttribute("class"));
        Assert.Equal(viewport, indicatorContainer.ParentElement);
        Assert.Contains("absolute", indicatorContainer.GetAttribute("class"));
    }

    [Fact]
    public void TwCarousel_ShowArrowsFalse_HidesArrowButtons()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCarousel>(p => p
            .Add(x => x.ShowArrows, false)
            .Add(x => x.ChildContent, ThreeSlides()));

        // Assert
        Assert.Empty(cut.FindAll("button[aria-label='Previous slide']"));
        Assert.Empty(cut.FindAll("button[aria-label='Next slide']"));
    }

    [Fact]
    public void TwCarousel_ShowIndicatorsFalse_HidesIndicatorDots()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCarousel>(p => p
            .Add(x => x.ShowIndicators, false)
            .Add(x => x.ChildContent, ThreeSlides()));

        // Assert
        Assert.Empty(cut.FindAll("button[aria-label^='Slide']"));
    }

    [Fact]
    public void TwCarousel_RendersOneIndicatorPerSlide_AndMarksTheActiveOne()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCarousel>(p => p.Add(x => x.ChildContent, ThreeSlides()));

        // Assert
        var indicators = cut.FindAll("button[aria-label^='Slide']");
        Assert.Equal(3, indicators.Count);
        Assert.Equal("true", indicators[0].GetAttribute("aria-current"));
        Assert.Equal("false", indicators[1].GetAttribute("aria-current"));
        Assert.Equal("false", indicators[2].GetAttribute("aria-current"));
    }

    #endregion

    #region navigation

    [Fact]
    public async Task NextSlide_And_PreviousSlide_MoveBetweenSlides()
    {
        // Arrange
        var cut = TestContext.Render<TwCarousel>(p => p.Add(x => x.ChildContent, ThreeSlides()));

        // Act & Assert
        await cut.InvokeAsync(() => cut.Instance.NextSlide());
        Assert.Equal(1, cut.Instance.SelectedIndex);

        await cut.InvokeAsync(() => cut.Instance.PreviousSlide());
        Assert.Equal(0, cut.Instance.SelectedIndex);
    }

    [Fact]
    public async Task NextSlide_WrapsToFirst_WhenLoopEnabled_AndOnLastSlide()
    {
        // Arrange
        var cut = TestContext.Render<TwCarousel>(p => p
            .Add(x => x.SelectedIndex, 2)
            .Add(x => x.ChildContent, ThreeSlides()));

        // Act
        await cut.InvokeAsync(() => cut.Instance.NextSlide());

        // Assert
        Assert.Equal(0, cut.Instance.SelectedIndex);
    }

    [Fact]
    public async Task NextSlide_StaysOnLastSlide_WhenLoopDisabled()
    {
        // Arrange
        var cut = TestContext.Render<TwCarousel>(p => p
            .Add(x => x.Loop, false)
            .Add(x => x.SelectedIndex, 2)
            .Add(x => x.ChildContent, ThreeSlides()));

        // Act
        await cut.InvokeAsync(() => cut.Instance.NextSlide());

        // Assert
        Assert.Equal(2, cut.Instance.SelectedIndex);
    }

    [Fact]
    public async Task PreviousSlide_WrapsToLast_WhenLoopEnabled_AndOnFirstSlide()
    {
        // Arrange
        var cut = TestContext.Render<TwCarousel>(p => p.Add(x => x.ChildContent, ThreeSlides()));

        // Act
        await cut.InvokeAsync(() => cut.Instance.PreviousSlide());

        // Assert
        Assert.Equal(2, cut.Instance.SelectedIndex);
    }

    [Fact]
    public async Task PreviousSlide_StaysOnFirstSlide_WhenLoopDisabled()
    {
        // Arrange
        var cut = TestContext.Render<TwCarousel>(p => p
            .Add(x => x.Loop, false)
            .Add(x => x.ChildContent, ThreeSlides()));

        // Act
        await cut.InvokeAsync(() => cut.Instance.PreviousSlide());

        // Assert
        Assert.Equal(0, cut.Instance.SelectedIndex);
    }

    [Fact]
    public void Loop_False_DisablesArrowButtons_AtBoundaries()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCarousel>(p => p
            .Add(x => x.Loop, false)
            .Add(x => x.ChildContent, ThreeSlides()));

        // Assert - on the first slide, only the previous arrow is disabled
        Assert.True(cut.Find("button[aria-label='Previous slide']").HasAttribute("disabled"));
        Assert.False(cut.Find("button[aria-label='Next slide']").HasAttribute("disabled"));
    }

    [Fact]
    public async Task GoToSlide_NavigatesDirectly_AndRaisesSelectedIndexChanged()
    {
        // Arrange
        int? changedIndex = null;
        var cut = TestContext.Render<TwCarousel>(p => p
            .Add(x => x.SelectedIndexChanged, EventCallback.Factory.Create<int>(this, i => changedIndex = i))
            .Add(x => x.ChildContent, ThreeSlides()));

        // Act
        await cut.InvokeAsync(() => cut.Instance.GoToSlide(2));

        // Assert
        Assert.Equal(2, changedIndex);
        Assert.Equal(2, cut.Instance.SelectedIndex);
        Assert.Contains("Slide 3", cut.Markup);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(99)]
    public async Task GoToSlide_OutOfRange_DoesNothing(int index)
    {
        // Arrange
        var cut = TestContext.Render<TwCarousel>(p => p.Add(x => x.ChildContent, ThreeSlides()));

        // Act
        await cut.InvokeAsync(() => cut.Instance.GoToSlide(index));

        // Assert
        Assert.Equal(0, cut.Instance.SelectedIndex);
    }

    [Fact]
    public async Task GoToSlide_AlreadySelected_DoesNotRaiseSelectedIndexChanged()
    {
        // Arrange
        var raised = false;
        var cut = TestContext.Render<TwCarousel>(p => p
            .Add(x => x.SelectedIndexChanged, EventCallback.Factory.Create<int>(this, _ => raised = true))
            .Add(x => x.ChildContent, ThreeSlides()));

        // Act
        await cut.InvokeAsync(() => cut.Instance.GoToSlide(0));

        // Assert
        Assert.False(raised);
    }

    [Fact]
    public void ClickingAnIndicator_NavigatesToThatSlide()
    {
        // Arrange
        var cut = TestContext.Render<TwCarousel>(p => p.Add(x => x.ChildContent, ThreeSlides()));

        // Act
        cut.FindAll("button[aria-label^='Slide']")[1].Click();

        // Assert
        Assert.Equal(1, cut.Instance.SelectedIndex);
        Assert.Contains("Slide 2", cut.Markup);
    }

    [Fact]
    public void ClickingTheNextArrow_AdvancesToTheNextSlide()
    {
        // Arrange
        var cut = TestContext.Render<TwCarousel>(p => p.Add(x => x.ChildContent, ThreeSlides()));

        // Act
        cut.Find("button[aria-label='Next slide']").Click();

        // Assert
        Assert.Equal(1, cut.Instance.SelectedIndex);
    }

    [Fact]
    public void ClickingThePreviousArrow_MovesToThePreviousSlide()
    {
        // Arrange
        var cut = TestContext.Render<TwCarousel>(p => p
            .Add(x => x.SelectedIndex, 1)
            .Add(x => x.ChildContent, ThreeSlides()));

        // Act
        cut.Find("button[aria-label='Previous slide']").Click();

        // Assert
        Assert.Equal(0, cut.Instance.SelectedIndex);
    }

    #endregion

    #region keyboard

    [Fact]
    public void ArrowRightKey_AdvancesToTheNextSlide()
    {
        // Arrange
        var cut = TestContext.Render<TwCarousel>(p => p.Add(x => x.ChildContent, ThreeSlides()));

        // Act
        cut.Find("div[role='region']").KeyDown(new KeyboardEventArgs { Key = "ArrowRight" });

        // Assert
        Assert.Equal(1, cut.Instance.SelectedIndex);
    }

    [Fact]
    public void ArrowLeftKey_MovesToThePreviousSlide()
    {
        // Arrange
        var cut = TestContext.Render<TwCarousel>(p => p
            .Add(x => x.SelectedIndex, 1)
            .Add(x => x.ChildContent, ThreeSlides()));

        // Act
        cut.Find("div[role='region']").KeyDown(new KeyboardEventArgs { Key = "ArrowLeft" });

        // Assert
        Assert.Equal(0, cut.Instance.SelectedIndex);
    }

    [Fact]
    public void UnhandledKey_DoesNotChangeSelectedIndex()
    {
        // Arrange
        var cut = TestContext.Render<TwCarousel>(p => p.Add(x => x.ChildContent, ThreeSlides()));

        // Act
        cut.Find("div[role='region']").KeyDown(new KeyboardEventArgs { Key = "A" });

        // Assert
        Assert.Equal(0, cut.Instance.SelectedIndex);
    }

    #endregion

    #region swipe gestures

    [Fact]
    public void SwipeLeft_PastThreshold_AdvancesToNextSlide()
    {
        // Arrange
        var cut = TestContext.Render<TwCarousel>(p => p.Add(x => x.ChildContent, ThreeSlides()));
        var viewport = cut.Find("div[role='region'] > div");

        // Act - finger moves from x=200 to x=100 (a 100px leftward swipe)
        viewport.TouchStart(new TouchEventArgs { ChangedTouches = [new TouchPoint { ClientX = 200 }] });
        viewport.TouchEnd(new TouchEventArgs { ChangedTouches = [new TouchPoint { ClientX = 100 }] });

        // Assert
        Assert.Equal(1, cut.Instance.SelectedIndex);
    }

    [Fact]
    public void SwipeRight_PastThreshold_MovesToPreviousSlide()
    {
        // Arrange
        var cut = TestContext.Render<TwCarousel>(p => p
            .Add(x => x.SelectedIndex, 1)
            .Add(x => x.ChildContent, ThreeSlides()));
        var viewport = cut.Find("div[role='region'] > div");

        // Act
        viewport.TouchStart(new TouchEventArgs { ChangedTouches = [new TouchPoint { ClientX = 100 }] });
        viewport.TouchEnd(new TouchEventArgs { ChangedTouches = [new TouchPoint { ClientX = 200 }] });

        // Assert
        Assert.Equal(0, cut.Instance.SelectedIndex);
    }

    [Fact]
    public void Swipe_BelowThreshold_DoesNotChangeSlide()
    {
        // Arrange
        var cut = TestContext.Render<TwCarousel>(p => p.Add(x => x.ChildContent, ThreeSlides()));
        var viewport = cut.Find("div[role='region'] > div");

        // Act - only a 10px drag, well under the swipe threshold
        viewport.TouchStart(new TouchEventArgs { ChangedTouches = [new TouchPoint { ClientX = 100 }] });
        viewport.TouchEnd(new TouchEventArgs { ChangedTouches = [new TouchPoint { ClientX = 90 }] });

        // Assert
        Assert.Equal(0, cut.Instance.SelectedIndex);
    }

    [Fact]
    public void Swipe_DoesNothing_WhenSwipeGestureDisabled()
    {
        // Arrange
        var cut = TestContext.Render<TwCarousel>(p => p
            .Add(x => x.EnableSwipeGesture, false)
            .Add(x => x.ChildContent, ThreeSlides()));
        var viewport = cut.Find("div[role='region'] > div");

        // Act
        viewport.TouchStart(new TouchEventArgs { ChangedTouches = [new TouchPoint { ClientX = 200 }] });
        viewport.TouchEnd(new TouchEventArgs { ChangedTouches = [new TouchPoint { ClientX = 100 }] });

        // Assert
        Assert.Equal(0, cut.Instance.SelectedIndex);
    }

    #endregion

    #region indicator color

    [Fact]
    public void ActiveIndicator_UsesThemeDefault_WhenNoColorSpecified()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCarousel>(p => p.Add(x => x.ChildContent, ThreeSlides()));
        var theme = Theme.Components.Require<TwBlazor.Configuration.Components.TwCarouselTheme>();

        // Assert
        var indicators = cut.FindAll("button[aria-label^='Slide']");
        Assert.Contains(theme.IndicatorActive.Split(' ')[0], indicators[0].GetAttribute("class"));
    }

    [Fact]
    public void ActiveIndicator_UsesSpecifiedColor()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCarousel>(p => p
            .Add(x => x.Color, Color.Success)
            .Add(x => x.ChildContent, ThreeSlides()));

        // Assert
        var indicators = cut.FindAll("button[aria-label^='Slide']");
        var expected = ColorBuilder.GetFilledVariantColor(Color.Success).Split(' ')[0];
        Assert.Contains(expected, indicators[0].GetAttribute("class"));
    }

    #endregion

    #region button color

    [Fact]
    public void ArrowButtons_DefaultToLightColor()
    {
        // Arrange & Act - the arrow/play-pause buttons get their appearance from TwButton's filled-variant
        // color classes (via ButtonColor), not from bespoke background classes in the theme.
        var cut = TestContext.Render<TwCarousel>(p => p.Add(x => x.ChildContent, ThreeSlides()));

        // Assert
        var expected = ColorBuilder.GetFilledVariantColor(Color.Light).Split(' ')[0];
        Assert.Contains(expected, cut.Find("button[aria-label='Previous slide']").GetAttribute("class"));
        Assert.Contains(expected, cut.Find("button[aria-label='Next slide']").GetAttribute("class"));
    }

    [Fact]
    public void ArrowButtons_UseSpecifiedButtonColor()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCarousel>(p => p
            .Add(x => x.ButtonColor, Color.Primary)
            .Add(x => x.ChildContent, ThreeSlides()));

        // Assert
        var expected = ColorBuilder.GetFilledVariantColor(Color.Primary).Split(' ')[0];
        Assert.Contains(expected, cut.Find("button[aria-label='Previous slide']").GetAttribute("class"));
        Assert.Contains(expected, cut.Find("button[aria-label='Next slide']").GetAttribute("class"));
    }

    [Fact]
    public void PlayPauseButton_UsesSpecifiedButtonColor()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCarousel>(p => p
            .Add(x => x.AutoPlay, true)
            .Add(x => x.ButtonColor, Color.Danger)
            .Add(x => x.ChildContent, ThreeSlides()));

        // Assert
        var expected = ColorBuilder.GetFilledVariantColor(Color.Danger).Split(' ')[0];
        Assert.Contains(expected, cut.Find("button[aria-label='Pause automatic slideshow']").GetAttribute("class"));
    }

    #endregion

    #region autoplay

    [Fact]
    public void AutoPlay_False_RendersNoPlayPauseButton()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCarousel>(p => p.Add(x => x.ChildContent, ThreeSlides()));

        // Assert
        Assert.Empty(cut.FindAll("button[aria-label$='automatic slideshow']"));
    }

    [Fact]
    public void AutoPlay_True_RendersPauseButton_ThatTogglesToPlay()
    {
        // Arrange
        var cut = TestContext.Render<TwCarousel>(p => p
            .Add(x => x.AutoPlay, true)
            .Add(x => x.ChildContent, ThreeSlides()));

        // Assert - playing initially, so the control offers to pause
        Assert.NotNull(cut.Find("button[aria-label='Pause automatic slideshow']"));

        // Act
        cut.Find("button[aria-label='Pause automatic slideshow']").Click();

        // Assert - now paused, so the control offers to resume
        Assert.NotNull(cut.Find("button[aria-label='Play automatic slideshow']"));
    }

    [Fact]
    public void PlayPauseButton_TogglesWhilePointerIsOverCarousel()
    {
        // Arrange - the pointer is always over the carousel when its own button is clicked
        var cut = TestContext.Render<TwCarousel>(p => p
            .Add(x => x.AutoPlay, true)
            .Add(x => x.ChildContent, ThreeSlides()));
        cut.Find("div[role='region'] > div").MouseEnter();

        // Assert - hovering pauses playback but must not flip the control to "play"
        Assert.NotNull(cut.Find("button[aria-label='Pause automatic slideshow']"));

        // Act & Assert - pause, then resume
        cut.Find("button[aria-label='Pause automatic slideshow']").Click();
        Assert.NotNull(cut.Find("button[aria-label='Play automatic slideshow']"));

        cut.Find("button[aria-label='Play automatic slideshow']").Click();
        Assert.NotNull(cut.Find("button[aria-label='Pause automatic slideshow']"));
    }

    [Fact]
    public void PlayPauseButton_TogglesWhileFocusIsWithinCarousel()
    {
        // Arrange - clicking the button leaves keyboard focus on it, inside the carousel
        var cut = TestContext.Render<TwCarousel>(p => p
            .Add(x => x.AutoPlay, true)
            .Add(x => x.ChildContent, ThreeSlides()));
        cut.Find("div[role='region']").FocusIn();

        // Act & Assert
        Assert.NotNull(cut.Find("button[aria-label='Pause automatic slideshow']"));
        cut.Find("button[aria-label='Pause automatic slideshow']").Click();
        Assert.NotNull(cut.Find("button[aria-label='Play automatic slideshow']"));
    }

    [Fact]
    public void IsAutoPlayPaused_IncludesPointerAndFocus_ButManualStateDoesNot()
    {
        // Arrange
        var cut = TestContext.Render<TwCarousel>(p => p
            .Add(x => x.AutoPlay, true)
            .Add(x => x.ChildContent, ThreeSlides()));

        // Act & Assert - hover and focus are tracked independently, so leaving one does not clear the other
        cut.Find("div[role='region'] > div").MouseEnter();
        cut.Find("div[role='region']").FocusIn();
        Assert.True(cut.Instance.IsAutoPlayPaused);
        Assert.False(cut.Instance.IsAutoPlayManuallyPaused);

        cut.Find("div[role='region'] > div").MouseLeave();
        Assert.True(cut.Instance.IsAutoPlayPaused);

        cut.Find("div[role='region']").FocusOut();
        Assert.False(cut.Instance.IsAutoPlayPaused);
    }

    [Fact]
    public async Task AutoPlay_ResumesAfterPlayIsPressed_WhilePointerIsStillOverCarousel()
    {
        // Arrange
        var cut = TestContext.Render<TwCarousel>(p => p
            .Add(x => x.AutoPlay, true)
            .Add(x => x.AutoPlayInterval, TimeSpan.FromMilliseconds(30))
            .Add(x => x.ChildContent, ThreeSlides()));
        cut.Find("div[role='region'] > div").MouseEnter();

        // Act - pause, then press play again with the pointer still over the carousel
        await cut.InvokeAsync(() => cut.Find("button[aria-label='Pause automatic slideshow']").Click());
        await cut.InvokeAsync(() => cut.Find("button[aria-label='Play automatic slideshow']").Click());
        var resumedAt = cut.Instance.SelectedIndex;

        // Assert
        cut.WaitForState(() => cut.Instance.SelectedIndex != resumedAt, TimeSpan.FromSeconds(2));
        Assert.NotEqual(resumedAt, cut.Instance.SelectedIndex);
    }

    [Fact]
    public async Task AutoPlay_AdvancesSlidesOnATimer_UnlessPaused()
    {
        // Arrange
        var cut = TestContext.Render<TwCarousel>(p => p
            .Add(x => x.AutoPlay, true)
            .Add(x => x.AutoPlayInterval, TimeSpan.FromMilliseconds(30))
            .Add(x => x.ChildContent, ThreeSlides()));

        // Act & Assert - left running, it advances on its own
        cut.WaitForState(() => cut.Instance.SelectedIndex != 0, TimeSpan.FromSeconds(2));

        // Act - pausing stops further advancement
        await cut.InvokeAsync(() => cut.Find("button[aria-label='Pause automatic slideshow']").Click());
        var pausedAt = cut.Instance.SelectedIndex;
        await Task.Delay(150, Xunit.TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(pausedAt, cut.Instance.SelectedIndex);
    }

    [Fact]
    public async Task DisposeAsync_StopsTheAutoPlayTimer_WithoutThrowing()
    {
        // Arrange
        var cut = TestContext.Render<TwCarousel>(p => p
            .Add(x => x.AutoPlay, true)
            .Add(x => x.AutoPlayInterval, TimeSpan.FromMilliseconds(20))
            .Add(x => x.ChildContent, ThreeSlides()));

        // Act
        await cut.Instance.DisposeAsync();
        await Task.Delay(100, Xunit.TestContext.Current.CancellationToken);

        // Assert - no exception from the timer continuing to fire against a disposed component
        Assert.NotNull(cut.Instance);
    }

    #endregion

    #region custom navigation and indicators

    [Fact]
    public async Task TwCarousel_LeftNavigation_ReplacesDefaultArrow_AndCanDriveTheCarouselViaItsContext()
    {
        // Arrange
        RenderFragment<TwCarousel> customLeft = context => builder =>
        {
            builder.OpenElement(0, "button");
            builder.AddAttribute(1, "class", "custom-prev");
            builder.AddAttribute(2, "onclick", EventCallback.Factory.Create(this, () => context.PreviousSlide()));
            builder.AddContent(3, "Custom Prev");
            builder.CloseElement();
        };

        var cut = TestContext.Render<TwCarousel>(p => p
            .Add(x => x.SelectedIndex, 1)
            .Add(x => x.LeftNavigation, customLeft)
            .Add(x => x.ChildContent, ThreeSlides()));

        // Assert - the default previous arrow is gone, replaced by the custom content
        Assert.Empty(cut.FindAll("button[aria-label='Previous slide']"));
        var customButton = cut.Find("button.custom-prev");

        // Act - the context passed to the render fragment exposes the carousel's public API
        await cut.InvokeAsync(() => customButton.Click());

        // Assert
        Assert.Equal(0, cut.Instance.SelectedIndex);
    }

    [Fact]
    public async Task TwCarousel_RightNavigation_ReplacesDefaultArrow_AndCanDriveTheCarouselViaItsContext()
    {
        // Arrange
        RenderFragment<TwCarousel> customRight = context => builder =>
        {
            builder.OpenElement(0, "button");
            builder.AddAttribute(1, "class", "custom-next");
            builder.AddAttribute(2, "onclick", EventCallback.Factory.Create(this, () => context.NextSlide()));
            builder.AddContent(3, "Custom Next");
            builder.CloseElement();
        };

        var cut = TestContext.Render<TwCarousel>(p => p
            .Add(x => x.RightNavigation, customRight)
            .Add(x => x.ChildContent, ThreeSlides()));

        // Assert - the default next arrow is gone, replaced by the custom content
        Assert.Empty(cut.FindAll("button[aria-label='Next slide']"));
        var customButton = cut.Find("button.custom-next");

        // Act
        await cut.InvokeAsync(() => customButton.Click());

        // Assert
        Assert.Equal(1, cut.Instance.SelectedIndex);
    }

    [Fact]
    public void TwCarousel_LeftNavigation_Renders_EvenWhenShowArrowsIsFalse()
    {
        // Arrange & Act - explicit custom navigation is shown regardless of ShowArrows, which only
        // governs the built-in default arrows
        RenderFragment<TwCarousel> customLeft = context => builder =>
        {
            builder.OpenElement(0, "button");
            builder.AddAttribute(1, "class", "custom-prev");
            builder.CloseElement();
        };

        var cut = TestContext.Render<TwCarousel>(p => p
            .Add(x => x.ShowArrows, false)
            .Add(x => x.LeftNavigation, customLeft)
            .Add(x => x.ChildContent, ThreeSlides()));

        // Assert
        Assert.NotEmpty(cut.FindAll("button.custom-prev"));
    }

    [Fact]
    public async Task TwCarousel_Indicators_ReplacesDefaultDots_AndCanDriveTheCarouselViaItsContext()
    {
        // Arrange
        RenderFragment<TwCarousel> customIndicators = context => builder =>
        {
            foreach (var i in Enumerable.Range(0, 3))
            {
                var sequence = i * 3;
                builder.OpenElement(sequence, "button");
                builder.AddAttribute(sequence + 1, "class", $"custom-indicator-{i}");
                builder.AddAttribute(sequence + 2, "onclick", EventCallback.Factory.Create(this, () => context.GoToSlide(i)));
                builder.CloseElement();
            }
        };

        var cut = TestContext.Render<TwCarousel>(p => p
            .Add(x => x.Indicators, customIndicators)
            .Add(x => x.ChildContent, ThreeSlides()));

        // Assert - the default indicator dots are gone, replaced by the custom content
        Assert.Empty(cut.FindAll("button[aria-label^='Slide']"));
        var customIndicator = cut.Find("button.custom-indicator-2");

        // Act
        await cut.InvokeAsync(() => customIndicator.Click());

        // Assert
        Assert.Equal(2, cut.Instance.SelectedIndex);
    }

    #endregion

    [Fact]
    public async Task RegisterItem_DoesNotDuplicate_WhenSameSlideInstanceRegisteredTwice()
    {
        // Arrange
        var cut = TestContext.Render<TwCarousel>(p => p.Add(x => x.ChildContent, ThreeSlides()));
        Assert.Equal(3, cut.FindAll("button[aria-label^='Slide']").Count);

#pragma warning disable BL0005 // Component parameter should not be set outside of its component
        var extraSlide = new TwCarouselItem { Parent = cut.Instance, ChildContent = b => b.AddContent(0, "Extra") };
#pragma warning restore BL0005

        // Act
        await cut.InvokeAsync(() => cut.Instance.RegisterItem(extraSlide));
        await cut.InvokeAsync(() => cut.Instance.RegisterItem(extraSlide));

        // Assert - only added once
        Assert.Equal(4, cut.FindAll("button[aria-label^='Slide']").Count);
    }
}
