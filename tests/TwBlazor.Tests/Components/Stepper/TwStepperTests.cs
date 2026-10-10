using Bunit;
using Microsoft.AspNetCore.Components;
using TwBlazor.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;

namespace TwBlazor.Tests.Components.Stepper;

public class TwStepperTests : TwBlazorTestBase
{
    private sealed class StepSpec
    {
        public required string Label { get; init; }
        public string? Description { get; init; }
        public bool Disabled { get; init; }
        public TwBlazor.Enums.Icon? Icon { get; init; }
        public RenderFragment? ChildContent { get; init; }
    }

    private static RenderFragment BuildSteps(params StepSpec[] steps) => builder =>
    {
        var seq = 0;
        foreach (var step in steps)
        {
            builder.OpenComponent<TwStep>(seq++);
            builder.AddAttribute(seq++, "Label", step.Label);
            if (step.Description != null)
            {
                builder.AddAttribute(seq++, "Description", step.Description);
            }
            if (step.Disabled)
            {
                builder.AddAttribute(seq++, "Disabled", true);
            }
            if (step.Icon.HasValue)
            {
                builder.AddAttribute(seq++, "Icon", step.Icon.Value);
            }
            if (step.ChildContent != null)
            {
                builder.AddAttribute(seq++, "ChildContent", step.ChildContent);
            }
            builder.CloseComponent();
        }
    };

    private TwStepperTheme StepperTheme => Theme.Components.Require<TwStepperTheme>();

    #region nav aria attributes

    [Fact]
    public void TwStepper_WithNoAriaLabel_DefaultsTo_Progress()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwStepper>(p => p
            .Add(x => x.ChildContent, BuildSteps(new StepSpec { Label = "Step 1" })));

        // Assert
        var nav = cut.Find("nav");
        Assert.Equal("Progress", nav.GetAttribute("aria-label"));
        Assert.Null(nav.GetAttribute("aria-labelledby"));
    }

    [Fact]
    public void TwStepper_WithAriaLabel_UsesIt_OverDefault()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwStepper>(p => p
            .Add(x => x.AriaLabel, "Checkout progress")
            .Add(x => x.ChildContent, BuildSteps(new StepSpec { Label = "Step 1" })));

        // Assert
        var nav = cut.Find("nav");
        Assert.Equal("Checkout progress", nav.GetAttribute("aria-label"));
    }

    [Fact]
    public void TwStepper_WithAriaLabelledBy_OmitsDefaultAriaLabel()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwStepper>(p => p
            .Add(x => x.AriaLabelledBy, "external-heading")
            .Add(x => x.ChildContent, BuildSteps(new StepSpec { Label = "Step 1" })));

        // Assert
        var nav = cut.Find("nav");
        Assert.Null(nav.GetAttribute("aria-label"));
        Assert.Equal("external-heading", nav.GetAttribute("aria-labelledby"));
    }

    #endregion

    #region appearance

    private static string FirstClass(string classes) => classes.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0];

    [Theory]
    [InlineData(StepperOrientation.Horizontal)]
    [InlineData(StepperOrientation.Vertical)]
    public void TwStepper_WithNoColor_FillsTheCompletedAndCurrentSteps_WithThePrimaryColor(StepperOrientation orientation)
    {
        // Arrange & Act
        var cut = TestContext.Render<TwStepper>(p => p
            .Add(x => x.ActiveStepIndex, 1)
            .Add(x => x.Orientation, orientation)
            .Add(x => x.ChildContent, BuildSteps(
                new StepSpec { Label = "One" }, new StepSpec { Label = "Two" }, new StepSpec { Label = "Three" })));

        // Assert
        var fill = FirstClass(Theme.Colors.SurfaceColors.Filled.Primary);
        var buttons = cut.FindAll("button");
        Assert.Contains(fill, buttons[0].ClassList);
        Assert.Contains(fill, buttons[1].ClassList);
        Assert.DoesNotContain(fill, buttons[2].ClassList);
    }

    [Fact]
    public void TwStepper_WithAColor_UsesIt_ForTheIndicatorsAndTheCurrentStepsRing()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwStepper>(p => p
            .Add(x => x.ActiveStepIndex, 0)
            .Add(x => x.Color, Color.Success)
            .Add(x => x.ChildContent, BuildSteps(new StepSpec { Label = "One" }, new StepSpec { Label = "Two" })));

        // Assert
        var current = cut.Find("button[aria-current='step']");
        Assert.Contains(FirstClass(Theme.Colors.SurfaceColors.Filled.Success), current.ClassList);
        Assert.DoesNotContain(FirstClass(Theme.Colors.SurfaceColors.Filled.Primary), current.ClassList);
        Assert.Contains(FirstClass(Theme.Border.Colors.Success), current.ParentElement!.ClassList);
    }

    [Theory]
    [InlineData(StepperOrientation.Horizontal)]
    [InlineData(StepperOrientation.Vertical)]
    public void TwStepper_FramesEveryIndicator_AndOnlyShowsTheFrame_AroundTheCurrentStep(StepperOrientation orientation)
    {
        // Arrange & Act
        var cut = TestContext.Render<TwStepper>(p => p
            .Add(x => x.ActiveStepIndex, 1)
            .Add(x => x.Orientation, orientation)
            .Add(x => x.ChildContent, BuildSteps(
                new StepSpec { Label = "One" }, new StepSpec { Label = "Two" }, new StepSpec { Label = "Three" })));

        // Assert
        var frames = cut.FindAll("button").Select(button => button.ParentElement!).ToList();
        var ring = FirstClass(Theme.Border.Colors.Primary);

        Assert.All(frames, frame =>
        {
            Assert.Equal("SPAN", frame.TagName);
            Assert.Contains("border-2", frame.ClassList);
        });
        Assert.Contains(ring, frames[1].ClassList);
        Assert.DoesNotContain("border-transparent", frames[1].ClassList);
        Assert.All(new[] { frames[0], frames[2] }, frame =>
        {
            Assert.Contains("border-transparent", frame.ClassList);
            Assert.DoesNotContain(ring, frame.ClassList);
        });
    }

    [Fact]
    public void TwStepper_EmphasisesTheCurrentStepsLabel()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwStepper>(p => p
            .Add(x => x.ActiveStepIndex, 1)
            .Add(x => x.ChildContent, BuildSteps(
                new StepSpec { Label = "One" }, new StepSpec { Label = "Two" }, new StepSpec { Label = "Three" })));

        // Assert
        string LabelClasses(string label) => cut.FindAll("li > span").Single(span => span.TextContent.Trim() == label).GetAttribute("class")!;

        Assert.NotEmpty(StepperTheme.LabelActive);
        Assert.Contains(StepperTheme.LabelActive, LabelClasses("Two"));
        Assert.DoesNotContain(StepperTheme.LabelActive, LabelClasses("One"));
        Assert.Equal(StepperTheme.LabelUpcoming, LabelClasses("Three"));
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void TwStepper_GivesAnUpcomingStepAHoverState_OnlyWhenItCanBeClicked(bool linear, bool expected)
    {
        // Arrange & Act
        var cut = TestContext.Render<TwStepper>(p => p
            .Add(x => x.ActiveStepIndex, 0)
            .Add(x => x.Linear, linear)
            .Add(x => x.ChildContent, BuildSteps(
                new StepSpec { Label = "One" }, new StepSpec { Label = "Two" }, new StepSpec { Label = "Three", Disabled = true })));

        // Assert
        var hover = FirstClass(StepperTheme.CircleUpcomingClickable);
        var buttons = cut.FindAll("button");

        Assert.Equal(expected, buttons[1].ClassList.Contains(hover));
        Assert.DoesNotContain(hover, buttons[2].ClassList);
        Assert.DoesNotContain(hover, buttons[0].ClassList);
    }

    #endregion

    #region horizontal rendering

    [Fact]
    public void TwStepper_Horizontal_RendersStepIndicators_ForAllStatuses()
    {
        // Arrange - Step 1 completed, Step 2 active (with a custom icon), Step 3 upcoming
        var cut = TestContext.Render<TwStepper>(p => p
            .Add(x => x.ActiveStepIndex, 1)
            .Add(x => x.ChildContent, BuildSteps(
                new StepSpec { Label = "Account", Description = "Create your login" },
                new StepSpec { Label = "Address", Icon = TwBlazor.Enums.Icon.Star },
                new StepSpec { Label = "Review" })));

        // Assert - three circles, two connectors (never a trailing one)
        var buttons = cut.FindAll("button");
        Assert.Equal(3, buttons.Count);
        var connectors = cut.FindAll("li[aria-hidden='true']");
        Assert.Equal(2, connectors.Count);

        // Completed step 1: checkmark icon, not the digit
        Assert.NotNull(buttons[0].QuerySelector("i.bi-check-lg"));
        Assert.Equal("step", buttons[1].GetAttribute("aria-current"));
        Assert.Null(buttons[0].GetAttribute("aria-current"));
        Assert.Null(buttons[2].GetAttribute("aria-current"));
        Assert.Equal("Step 1: Account, completed", buttons[0].GetAttribute("aria-label"));

        // Active step 2 has a custom icon, which takes precedence over the digit
        Assert.NotNull(buttons[1].QuerySelector("i.bi-star"));
        Assert.Equal("Step 2: Address, current", buttons[1].GetAttribute("aria-label"));

        // Upcoming step 3 has no icon, so it shows its number
        Assert.Null(buttons[2].QuerySelector("i"));
        Assert.Equal("3", buttons[2].TextContent.Trim());
        Assert.Equal("Step 3: Review, not started", buttons[2].GetAttribute("aria-label"));

        // Labels and the one supplied description
        Assert.Contains("Account", cut.Markup);
        var descriptionSpans = cut.FindAll("span").Where(s => s.TextContent.Trim() == "Create your login").ToList();
        Assert.Single(descriptionSpans);
    }

    [Fact]
    public void TwStepper_Horizontal_Linear_DisablesStepsAheadOfActive()
    {
        // Arrange
        var cut = TestContext.Render<TwStepper>(p => p
            .Add(x => x.ActiveStepIndex, 1)
            .Add(x => x.ChildContent, BuildSteps(
                new StepSpec { Label = "Step 1" },
                new StepSpec { Label = "Step 2" },
                new StepSpec { Label = "Step 3" })));

        // Assert - only the not-yet-reached step is disabled
        var buttons = cut.FindAll("button");
        Assert.False(buttons[0].HasAttribute("disabled"));
        Assert.False(buttons[1].HasAttribute("disabled"));
        Assert.True(buttons[2].HasAttribute("disabled"));
    }

    [Fact]
    public void TwStepper_Horizontal_ExplicitlyDisabledStep_IsAlwaysDisabled()
    {
        // Arrange - Linear=false would otherwise allow jumping ahead, but Disabled overrides that
        var cut = TestContext.Render<TwStepper>(p => p
            .Add(x => x.Linear, false)
            .Add(x => x.ChildContent, BuildSteps(
                new StepSpec { Label = "Step 1" },
                new StepSpec { Label = "Step 2", Disabled = true },
                new StepSpec { Label = "Step 3" })));

        // Assert
        var buttons = cut.FindAll("button");
        Assert.True(buttons[1].HasAttribute("disabled"));
    }

    [Fact]
    public void TwStepper_Horizontal_RendersContentPanel_ForActiveStepWithChildContent()
    {
        // Arrange & Act - the (only, active) step has ChildContent
        var cut = TestContext.Render<TwStepper>(p => p
            .Add(x => x.ChildContent, BuildSteps(
                new StepSpec { Label = "Step 1", ChildContent = b => b.AddContent(0, "Step 1 body") })));

        // Assert
        Assert.Contains("Step 1 body", cut.Markup);
    }

    [Fact]
    public void TwStepper_Horizontal_RendersNoContentPanel_ForActiveStepWithoutChildContent()
    {
        // Arrange & Act - Step 1 has ChildContent but isn't active; the active Step 2 has none
        var cut = TestContext.Render<TwStepper>(p => p
            .Add(x => x.ActiveStepIndex, 1)
            .Add(x => x.ChildContent, BuildSteps(
                new StepSpec { Label = "Step 1", ChildContent = b => b.AddContent(0, "Step 1 body") },
                new StepSpec { Label = "Step 2" })));

        // Assert - the panel is entirely absent, not just emptied
        Assert.DoesNotContain("Step 1 body", cut.Markup);
    }

    [Fact]
    public void TwStepper_Horizontal_WithoutDescription_RendersNoDescriptionSpan()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwStepper>(p => p
            .Add(x => x.ChildContent, BuildSteps(new StepSpec { Label = "Step 1" })));

        // Assert
        var descriptionSpans = cut.FindAll("span").Where(s => s.TextContent.Trim() == "Optional");
        Assert.Empty(descriptionSpans);
    }

    [Theory]
    [InlineData(0, 3, 33, "Step 1 of 3: Step 1")]
    [InlineData(1, 3, 67, "Step 2 of 3: Step 2")]
    [InlineData(2, 3, 100, "Step 3 of 3: Step 3")]
    public void TwStepper_Horizontal_RendersMobileProgress_WithCorrectValueAndLabel(int activeIndex, int stepCount, int expectedPercentage, string expectedLabel)
    {
        // Arrange
        var steps = Enumerable.Range(1, stepCount).Select(i => new StepSpec { Label = $"Step {i}" }).ToArray();
        var cut = TestContext.Render<TwStepper>(p => p
            .Add(x => x.ActiveStepIndex, activeIndex)
            .Add(x => x.ChildContent, BuildSteps(steps)));

        // Assert
        var progress = cut.Find("progress");
        Assert.Equal(expectedPercentage.ToString(), progress.GetAttribute("value"));
        var label = cut.FindAll("p").Single(p => p.TextContent.Trim() == expectedLabel);
        Assert.NotNull(label);
    }

    [Fact]
    public void TwStepper_Horizontal_WithNoSteps_ShowsZeroProgressAndNoMobileLabel()
    {
        // Arrange & Act - ChildContent with no TwStep children at all
        var cut = TestContext.Render<TwStepper>(p => p
            .Add(x => x.ChildContent, _ => { }));

        // Assert
        Assert.Null(cut.Instance.ActiveStep);
        var progress = cut.Find("progress");
        Assert.Equal("0", progress.GetAttribute("value"));
        Assert.Empty(cut.FindAll("p"));
        Assert.Empty(cut.FindAll("button"));
    }

    #endregion

    #region vertical rendering

    [Fact]
    public void TwStepper_Vertical_RendersStepIndicators_ForAllStatuses()
    {
        // Arrange - Step 1 completed, Step 2 active (with inline content), Step 3 upcoming
        var cut = TestContext.Render<TwStepper>(p => p
            .Add(x => x.Orientation, StepperOrientation.Vertical)
            .Add(x => x.ActiveStepIndex, 1)
            .Add(x => x.ChildContent, BuildSteps(
                new StepSpec { Label = "Account", Description = "Create your login" },
                new StepSpec { Label = "Address", ChildContent = b => b.AddContent(0, "Enter your address") },
                new StepSpec { Label = "Review" })));

        // Assert - three circles, two connectors between them (never a trailing one)
        var buttons = cut.FindAll("button");
        Assert.Equal(3, buttons.Count);
        var connectors = cut.FindAll("span[aria-hidden='true']");
        Assert.Equal(2, connectors.Count);

        Assert.NotNull(buttons[0].QuerySelector("i.bi-check-lg"));
        Assert.Equal("step", buttons[1].GetAttribute("aria-current"));
        Assert.Equal("3", buttons[2].TextContent.Trim());

        // Only the active step's inline content is rendered
        Assert.Contains("Enter your address", cut.Markup);

        var descriptionSpans = cut.FindAll("span").Where(s => s.TextContent.Trim() == "Create your login").ToList();
        Assert.Single(descriptionSpans);
    }

    [Fact]
    public void TwStepper_Vertical_InlineContent_OnlyShowsForActiveStep()
    {
        // Arrange & Act - both steps declare ChildContent; only the active one (index 1) should render it
        var cut = TestContext.Render<TwStepper>(p => p
            .Add(x => x.Orientation, StepperOrientation.Vertical)
            .Add(x => x.ActiveStepIndex, 1)
            .Add(x => x.ChildContent, BuildSteps(
                new StepSpec { Label = "Step 1", ChildContent = b => b.AddContent(0, "Body one") },
                new StepSpec { Label = "Step 2", ChildContent = b => b.AddContent(0, "Body two") })));

        // Assert
        Assert.DoesNotContain("Body one", cut.Markup);
        Assert.Contains("Body two", cut.Markup);
    }

    [Fact]
    public void TwStepper_Vertical_NeverRenders_HorizontalContentPanel()
    {
        // Arrange & Act - active step has ChildContent, but orientation is vertical
        var cut = TestContext.Render<TwStepper>(p => p
            .Add(x => x.Orientation, StepperOrientation.Vertical)
            .Add(x => x.ChildContent, BuildSteps(
                new StepSpec { Label = "Step 1", ChildContent = b => b.AddContent(0, "Inline body") })));

        // Assert - rendered once inline beneath the step, never a second time in the shared bottom
        // panel that the horizontal layout uses for the same condition.
        var occurrences = cut.Markup.Split("Inline body").Length - 1;
        Assert.Equal(1, occurrences);
    }

    #endregion

    #region navigation

    [Fact]
    public async Task NextStep_And_PreviousStep_SkipDisabledSteps_AndStopAtBoundaries()
    {
        // Arrange
        var cut = TestContext.Render<TwStepper>(p => p
            .Add(x => x.ChildContent, BuildSteps(
                new StepSpec { Label = "Step 1" },
                new StepSpec { Label = "Step 2", Disabled = true },
                new StepSpec { Label = "Step 3" })));

        // Act & Assert - NextStep from 0 skips the disabled Step 2, landing on Step 3
        await cut.InvokeAsync(() => cut.Instance.NextStep());
        Assert.Equal(2, cut.Instance.ActiveStepIndex);

        // Act & Assert - already at the last enabled step, NextStep is a no-op
        await cut.InvokeAsync(() => cut.Instance.NextStep());
        Assert.Equal(2, cut.Instance.ActiveStepIndex);

        // Act & Assert - PreviousStep skips the disabled Step 2, landing back on Step 1
        await cut.InvokeAsync(() => cut.Instance.PreviousStep());
        Assert.Equal(0, cut.Instance.ActiveStepIndex);

        // Act & Assert - already at the first step, PreviousStep is a no-op
        await cut.InvokeAsync(() => cut.Instance.PreviousStep());
        Assert.Equal(0, cut.Instance.ActiveStepIndex);
    }

    [Fact]
    public async Task GoToStep_RaisesActiveStepIndexChanged_OnlyWhenItActuallyMoves()
    {
        // Arrange
        int? changedIndex = null;
        var cut = TestContext.Render<TwStepper>(p => p
            .Add(x => x.ActiveStepIndex, 1)
            .Add(x => x.ActiveStepIndexChanged, EventCallback.Factory.Create<int>(this, i => changedIndex = i))
            .Add(x => x.ChildContent, BuildSteps(
                new StepSpec { Label = "Step 1" },
                new StepSpec { Label = "Step 2" },
                new StepSpec { Label = "Step 3", Disabled = true })));

        // Act & Assert - moving to a different, valid, non-disabled step raises the callback
        await cut.InvokeAsync(() => cut.Instance.GoToStep(0));
        Assert.Equal(0, changedIndex);
        Assert.Equal(0, cut.Instance.ActiveStepIndex);

        // Act & Assert - already-active index is a no-op
        changedIndex = null;
        await cut.InvokeAsync(() => cut.Instance.GoToStep(0));
        Assert.Null(changedIndex);

        // Act & Assert - disabled step is a no-op
        await cut.InvokeAsync(() => cut.Instance.GoToStep(2));
        Assert.Null(changedIndex);
        Assert.Equal(0, cut.Instance.ActiveStepIndex);

        // Act & Assert - out-of-range indices are no-ops
        await cut.InvokeAsync(() => cut.Instance.GoToStep(-1));
        await cut.InvokeAsync(() => cut.Instance.GoToStep(99));
        Assert.Null(changedIndex);
        Assert.Equal(0, cut.Instance.ActiveStepIndex);
    }

    [Theory]
    [InlineData(0, true, false)]
    [InlineData(1, false, false)]
    [InlineData(2, false, true)]
    public void IsFirstStep_And_IsLastStep_ReflectActiveStepIndex(int activeIndex, bool expectedFirst, bool expectedLast)
    {
        // Arrange & Act
        var cut = TestContext.Render<TwStepper>(p => p
            .Add(x => x.ActiveStepIndex, activeIndex)
            .Add(x => x.ChildContent, BuildSteps(
                new StepSpec { Label = "Step 1" },
                new StepSpec { Label = "Step 2" },
                new StepSpec { Label = "Step 3" })));

        // Assert
        Assert.Equal(expectedFirst, cut.Instance.IsFirstStep);
        Assert.Equal(expectedLast, cut.Instance.IsLastStep);
    }

    [Fact]
    public void ActiveStep_IsNull_WhenActiveStepIndexIsOutOfRange()
    {
        // Arrange & Act - negative index
        var cutNegative = TestContext.Render<TwStepper>(p => p
            .Add(x => x.ActiveStepIndex, -1)
            .Add(x => x.ChildContent, BuildSteps(new StepSpec { Label = "Step 1" })));

        // Assert
        Assert.Null(cutNegative.Instance.ActiveStep);

        // Arrange & Act - index beyond the last step
        var cutTooHigh = TestContext.Render<TwStepper>(p => p
            .Add(x => x.ActiveStepIndex, 5)
            .Add(x => x.ChildContent, BuildSteps(new StepSpec { Label = "Step 1" })));

        // Assert
        Assert.Null(cutTooHigh.Instance.ActiveStep);
    }

    #endregion

    #region click behavior

    [Fact]
    public void Linear_ClickingStepAheadOfActive_DoesNothing()
    {
        // Arrange
        var cut = TestContext.Render<TwStepper>(p => p
            .Add(x => x.ChildContent, BuildSteps(
                new StepSpec { Label = "Step 1" },
                new StepSpec { Label = "Step 2" },
                new StepSpec { Label = "Step 3" })));

        // Act
        cut.FindAll("button")[2].Click();

        // Assert
        Assert.Equal(0, cut.Instance.ActiveStepIndex);
    }

    [Fact]
    public void Linear_ClickingACompletedStep_NavigatesBackToIt()
    {
        // Arrange
        var cut = TestContext.Render<TwStepper>(p => p
            .Add(x => x.ActiveStepIndex, 2)
            .Add(x => x.ChildContent, BuildSteps(
                new StepSpec { Label = "Step 1" },
                new StepSpec { Label = "Step 2" },
                new StepSpec { Label = "Step 3" })));

        // Act
        cut.FindAll("button")[0].Click();

        // Assert
        Assert.Equal(0, cut.Instance.ActiveStepIndex);
    }

    [Fact]
    public void NonLinear_ClickingAnyEnabledStep_Navigates()
    {
        // Arrange
        var cut = TestContext.Render<TwStepper>(p => p
            .Add(x => x.Linear, false)
            .Add(x => x.ChildContent, BuildSteps(
                new StepSpec { Label = "Step 1" },
                new StepSpec { Label = "Step 2" },
                new StepSpec { Label = "Step 3" })));

        // Act - jump straight to the last step
        cut.FindAll("button")[2].Click();

        // Assert
        Assert.Equal(2, cut.Instance.ActiveStepIndex);
    }

    [Fact]
    public void ClickingADisabledStep_NeverActivatesIt_RegardlessOfLinear()
    {
        // Arrange
        var cut = TestContext.Render<TwStepper>(p => p
            .Add(x => x.Linear, false)
            .Add(x => x.ChildContent, BuildSteps(
                new StepSpec { Label = "Step 1" },
                new StepSpec { Label = "Step 2", Disabled = true },
                new StepSpec { Label = "Step 3" })));

        // Act
        cut.FindAll("button")[1].Click();

        // Assert
        Assert.Equal(0, cut.Instance.ActiveStepIndex);
    }

    #endregion

    #region connector and circle colors

    [Theory]
    [InlineData(null)]
    [InlineData(Color.Success)]
    public void Horizontal_ConnectorColor_ReflectsCompletedVsNeutral(Color? color)
    {
        // Arrange - 3 steps, active index 1: the connector before it (index 0) is "completed",
        // the connector at/after it (index 1) is not.
        var cut = TestContext.Render<TwStepper>(p => p
            .Add(x => x.ActiveStepIndex, 1)
            .Add(x => x.Color, color)
            .Add(x => x.ChildContent, BuildSteps(
                new StepSpec { Label = "Step 1" },
                new StepSpec { Label = "Step 2" },
                new StepSpec { Label = "Step 3" })));

        // Assert
        var connectors = cut.FindAll("li[aria-hidden='true']");
        var expectedAccent = ColorBuilder.GetBorderColor(color);
        Assert.Contains(expectedAccent.Split(' ')[0], connectors[0].GetAttribute("class"));
        Assert.Contains(StepperTheme.ConnectorNeutral.Split(' ')[0], connectors[1].GetAttribute("class"));
    }

    [Fact]
    public void Vertical_ConnectorColor_ReflectsCompletedVsNeutral()
    {
        // Arrange
        var cut = TestContext.Render<TwStepper>(p => p
            .Add(x => x.Orientation, StepperOrientation.Vertical)
            .Add(x => x.ActiveStepIndex, 1)
            .Add(x => x.ChildContent, BuildSteps(
                new StepSpec { Label = "Step 1" },
                new StepSpec { Label = "Step 2" },
                new StepSpec { Label = "Step 3" })));

        // Assert
        var connectors = cut.FindAll("span[aria-hidden='true']");
        var expectedAccent = ColorBuilder.GetBorderColor(null);
        Assert.Contains(expectedAccent.Split(' ')[0], connectors[0].GetAttribute("class"));
        Assert.Contains(StepperTheme.ConnectorNeutral.Split(' ')[0], connectors[1].GetAttribute("class"));
    }

    #endregion

    [Fact]
    public async Task RegisterStep_DoesNotDuplicate_WhenSameStepInstanceRegisteredTwice()
    {
        // Arrange - two real steps registered via normal rendering
        var cut = TestContext.Render<TwStepper>(p => p
            .Add(x => x.ChildContent, BuildSteps(
                new StepSpec { Label = "Step 1" },
                new StepSpec { Label = "Step 2" })));
        Assert.Equal(2, cut.FindAll("button").Count);

        // A bare instance constructed outside the render pipeline, purely to get a second TwStep
        // reference for the duplicate-registration check below.
#pragma warning disable BL0005 // Component parameter should not be set outside of its component
        var extraStep = new TwStep { Parent = cut.Instance, Label = "Step 3" };
#pragma warning restore BL0005

        // Act - register the same brand-new instance twice
        await cut.InvokeAsync(() => cut.Instance.RegisterStep(extraStep));
        await cut.InvokeAsync(() => cut.Instance.RegisterStep(extraStep));

        // Assert - only added once
        Assert.Equal(3, cut.FindAll("button").Count);
    }
}
