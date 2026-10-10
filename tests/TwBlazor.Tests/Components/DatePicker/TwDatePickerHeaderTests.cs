using Bunit;
using Microsoft.AspNetCore.Components;
using TwBlazor.Components.DatePicker;

namespace TwBlazor.Tests.Components.DatePicker;

public class TwDatePickerHeaderTests : TwBlazorTestBase
{
    [Fact]
    public void RendersButtons_AndInvokesCallbacks()
    {
        // Arrange
        var previousInvoked = false;
        var nextInvoked = false;
        var titleInvoked = false;

        // Act & Assert
        var cut = TestContext.Render<TwDatePickerHeader>(p => p
            .Add(x => x.OnPreviousClick, EventCallback.Factory.Create(this, () => previousInvoked = true))
            .Add(x => x.OnNextClick, EventCallback.Factory.Create(this, () => nextInvoked = true))
            .Add(x => x.OnTitleClick, EventCallback.Factory.Create(this, () => titleInvoked = true))
            .AddChildContent("Title")
        );

        var buttons = cut.FindAll("button");
        Assert.True(buttons.Count >= 3);

        // Previous
        buttons[0].Click();
        // Title switch
        buttons[1].Click();
        // Next
        buttons[2].Click();

        Assert.True(previousInvoked);
        Assert.True(titleInvoked);
        Assert.True(nextInvoked);
    }

    [Fact]
    public void PreviousDisabled_RendersPreviousButtonDisabled_AndSuppressesItsClick()
    {
        // Arrange
        var previousInvoked = false;

        // Act
        var cut = TestContext.Render<TwDatePickerHeader>(p => p
            .Add(x => x.OnPreviousClick, EventCallback.Factory.Create(this, () => previousInvoked = true))
            .Add(x => x.PreviousDisabled, true)
            .AddChildContent("Title")
        );

        var previousButton = cut.Find(".prev-btn");
        previousButton.Click();

        // Assert
        Assert.Equal("true", previousButton.GetAttribute("aria-disabled"));
        Assert.False(previousInvoked);
    }

    [Fact]
    public void NextDisabled_RendersNextButtonDisabled_AndSuppressesItsClick()
    {
        // Arrange
        var nextInvoked = false;

        // Act
        var cut = TestContext.Render<TwDatePickerHeader>(p => p
            .Add(x => x.OnNextClick, EventCallback.Factory.Create(this, () => nextInvoked = true))
            .Add(x => x.NextDisabled, true)
            .AddChildContent("Title")
        );

        var nextButton = cut.Find(".next-btn");
        nextButton.Click();

        // Assert
        Assert.Equal("true", nextButton.GetAttribute("aria-disabled"));
        Assert.False(nextInvoked);
    }

    [Fact]
    public void PreviousAndNextDisabled_DefaultToFalse()
    {
        // Arrange & Act — existing TwDatePicker usage doesn't set these, so both arrows must stay
        // enabled by default. OnPreviousClick/OnNextClick need a delegate for TwIcon to render an
        // actual <button> at all (see TwIcon.razor's OnClick.HasDelegate check).
        var cut = TestContext.Render<TwDatePickerHeader>(p => p
            .Add(x => x.OnPreviousClick, EventCallback.Factory.Create(this, () => { }))
            .Add(x => x.OnNextClick, EventCallback.Factory.Create(this, () => { }))
            .AddChildContent("Title")
        );

        // Assert
        Assert.False(cut.Find(".prev-btn").HasAttribute("disabled"));
        Assert.False(cut.Find(".next-btn").HasAttribute("disabled"));
    }
}