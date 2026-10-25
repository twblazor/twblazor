// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using AngleSharp.Dom;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using TwBlazor.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Localization;

namespace TwBlazor.Tests.Components.RangeSlider;

public class TwRangeSliderTests : TwBlazorTestBase
{
    private const string lockIdentifier = "twSliderLock.set";

    private IRenderedComponent<TwRangeSlider<int>> Render(int lower = 20, int upper = 80, Action<ComponentParameterCollectionBuilder<TwRangeSlider<int>>>? extra = null) =>
        TestContext.Render<TwRangeSlider<int>>(p =>
        {
            p.Add(x => x.LowerValue, lower)
             .Add(x => x.UpperValue, upper)
             .Add(x => x.Min, 0)
             .Add(x => x.Max, 100)
             .Add(x => x.Step, 1);
            extra?.Invoke(p);
        });

    private static IElement Lower(IRenderedComponent<TwRangeSlider<int>> cut) => cut.FindAll("input[type='range']")[0];

    private static IElement Upper(IRenderedComponent<TwRangeSlider<int>> cut) => cut.FindAll("input[type='range']")[1];

    [Fact]
    public void Renders_TwoRangeInputs_WithTheirValues()
    {
        // Arrange & Act
        var cut = Render(25, 75);

        // Assert
        Assert.Equal(2, cut.FindAll("input[type='range']").Count);
        Assert.Equal("25", Lower(cut).GetAttribute("value"));
        Assert.Equal("75", Upper(cut).GetAttribute("value"));
    }

    [Fact]
    public void BothInputs_ShareMinMaxAndStep()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwRangeSlider<int>>(p => p
            .Add(x => x.LowerValue, 20)
            .Add(x => x.UpperValue, 40)
            .Add(x => x.Min, 10)
            .Add(x => x.Max, 50)
            .Add(x => x.Step, 5));

        // Assert
        foreach (var input in cut.FindAll("input[type='range']"))
        {
            Assert.Equal("10", input.GetAttribute("min"));
            Assert.Equal("50", input.GetAttribute("max"));
            Assert.Equal("5", input.GetAttribute("step"));
        }
    }

    [Fact]
    public void GeneratesIds_ForBothInputs_WhenNoIdIsGiven()
    {
        // Arrange & Act
        var cut = Render();

        // Assert
        var lowerId = Lower(cut).GetAttribute("id");
        Assert.StartsWith("rangeslider-", lowerId);
        Assert.Equal($"{lowerId}-upper", Upper(cut).GetAttribute("id"));
    }

    [Fact]
    public void UsesTheGivenId_ForTheLowerInput()
    {
        // Arrange & Act
        var cut = Render(extra: p => p.Add(x => x.Id, "price"));

        // Assert
        Assert.Equal("price", Lower(cut).GetAttribute("id"));
        Assert.Equal("price-upper", Upper(cut).GetAttribute("id"));
    }

    [Fact]
    public void Name_IsSplitIntoLowerAndUpperFields()
    {
        // Arrange & Act
        var cut = Render(extra: p => p.Add(x => x.Name, "price"));

        // Assert
        Assert.Equal("price-lower", Lower(cut).GetAttribute("name"));
        Assert.Equal("price-upper", Upper(cut).GetAttribute("name"));
    }

    [Fact]
    public void Handles_HaveLocalizedAccessibleNames()
    {
        // Arrange & Act
        var cut = Render();

        // Assert
        Assert.Equal(Localizer[TwStrings.RangeSlider_Minimum], Lower(cut).GetAttribute("aria-label"));
        Assert.Equal(Localizer[TwStrings.RangeSlider_Maximum], Upper(cut).GetAttribute("aria-label"));
    }

    [Fact]
    public void Group_IsLabelledByTheLabel()
    {
        // Arrange & Act
        var cut = Render(extra: p => p.Add(x => x.Id, "price").Add(x => x.Label, "Price"));

        // Assert
        var group = cut.Find("[role='group']");
        var label = cut.Find("label");
        Assert.Equal("price-label", group.GetAttribute("aria-labelledby"));
        Assert.Equal("price-label", label.GetAttribute("id"));
        Assert.Equal("price", label.GetAttribute("for"));
    }

    [Fact]
    public void Group_UsesAriaLabel_WhenThereIsNoLabel()
    {
        // Arrange & Act
        var cut = Render(extra: p => p.Add(x => x.AriaLabel, "Price range"));

        // Assert
        var group = cut.Find("[role='group']");
        Assert.Equal("Price range", group.GetAttribute("aria-label"));
        Assert.False(group.HasAttribute("aria-labelledby"));
    }

    [Fact]
    public void Fill_SpansFromTheLowerToTheUpperValue()
    {
        // Arrange & Act
        var cut = Render(25, 75);

        // Assert
        var style = cut.Find($"[style*='width:']").GetAttribute("style");
        Assert.Contains("inset-inline-start:25%", style);
        Assert.Contains("width:50%", style);
    }

    [Fact]
    public void Thumbs_ArePositionedAtEachValue()
    {
        // Arrange & Act
        var cut = Render(10, 90);

        // Assert
        var thumbs = cut.FindAll("div[aria-hidden='true'][style^='inset-inline-start']")
            .Select(e => e.GetAttribute("style"))
            .ToList();
        Assert.Contains("inset-inline-start:10%", thumbs);
        Assert.Contains("inset-inline-start:90%", thumbs);
    }

    [Fact]
    public void Bubbles_ShowEachValue()
    {
        // Arrange & Act
        var cut = Render(10, 90);

        // Assert
        var text = cut.Markup;
        Assert.Contains(">10</div>", text);
        Assert.Contains(">90</div>", text);
    }

    [Fact]
    public void MovingTheLowerHandle_UpdatesTheLowerValue()
    {
        // Arrange
        var changed = 0;
        var cut = Render(extra: p => p.Add(x => x.LowerValueChanged, v => changed = v));

        // Act
        Lower(cut).Input("30");

        // Assert
        Assert.Equal(30, changed);
        Assert.Equal(30, cut.Instance.LowerValue);
    }

    [Fact]
    public void MovingTheUpperHandle_UpdatesTheUpperValue()
    {
        // Arrange
        var changed = 0;
        var cut = Render(extra: p => p.Add(x => x.UpperValueChanged, v => changed = v));

        // Act
        Upper(cut).Input("60");

        // Assert
        Assert.Equal(60, changed);
        Assert.Equal(60, cut.Instance.UpperValue);
    }

    [Fact]
    public void LowerHandle_CannotPassTheUpperHandle()
    {
        // Arrange
        int? changed = null;
        var cut = Render(20, 50, p => p.Add(x => x.LowerValueChanged, v => changed = v));

        // Act
        Lower(cut).Input("70");

        // Assert
        Assert.Equal(50, changed);
        Assert.Equal("50", Lower(cut).GetAttribute("value"));
    }

    [Fact]
    public void UpperHandle_CannotPassTheLowerHandle()
    {
        // Arrange
        int? changed = null;
        var cut = Render(40, 80, p => p.Add(x => x.UpperValueChanged, v => changed = v));

        // Act
        Upper(cut).Input("10");

        // Assert
        Assert.Equal(40, changed);
        Assert.Equal("40", Upper(cut).GetAttribute("value"));
    }

    [Fact]
    public void RejectedMove_WritesTheAcceptedValueBackToTheInput()
    {
        // Arrange - the lower handle is already touching the upper one
        var cut = Render(50, 50);

        // Act - the browser has moved the native input to 51
        Lower(cut).Input("51");

        // Assert
        Assert.Equal("50", Lower(cut).GetAttribute("value"));
        Assert.Equal(50, cut.Instance.LowerValue);
    }

    [Fact]
    public void MinDistance_KeepsAGapBetweenTheHandles()
    {
        // Arrange
        var cut = Render(20, 50, p => p.Add(x => x.MinDistance, 10));

        // Act
        Lower(cut).Input("48");
        Upper(cut).Input("21");

        // Assert
        Assert.Equal(40, cut.Instance.LowerValue);
        Assert.Equal(50, cut.Instance.UpperValue);
    }

    [Fact]
    public void Values_StayWithinMinAndMax()
    {
        // Arrange
        var cut = Render(20, 80);

        // Act
        Lower(cut).Input("-50");
        Upper(cut).Input("500");

        // Assert
        Assert.Equal(0, cut.Instance.LowerValue);
        Assert.Equal(100, cut.Instance.UpperValue);
    }

    [Fact]
    public void Decimal_Values_AreSupported()
    {
        // Arrange
        var cut = TestContext.Render<TwRangeSlider<decimal>>(p => p
            .Add(x => x.LowerValue, 1.5m)
            .Add(x => x.UpperValue, 4m)
            .Add(x => x.Min, 0m)
            .Add(x => x.Max, 5m)
            .Add(x => x.Step, 0.5m));

        // Act
        cut.FindAll("input[type='range']")[0].Input("2.5");

        // Assert
        Assert.Equal(2.5m, cut.Instance.LowerValue);
        Assert.Equal("2.5", cut.FindAll("input[type='range']")[0].GetAttribute("value"));
    }

    [Theory]
    [InlineData("not a number")]
    [InlineData("")]
    public void InvalidInput_IsIgnored(string value)
    {
        // Arrange
        var changed = false;
        var cut = Render(extra: p => p.Add(x => x.LowerValueChanged, _ => changed = true));

        // Act
        Lower(cut).Input(value);

        // Assert
        Assert.False(changed);
        Assert.Equal(20, cut.Instance.LowerValue);
    }

    [Fact]
    public void ReadOnly_IgnoresMoves_AndMarksBothInputs()
    {
        // Arrange
        var changed = false;
        var cut = Render(extra: p => p
            .Add(x => x.ReadOnly, true)
            .Add(x => x.LowerValueChanged, _ => changed = true));

        // Act
        Lower(cut).Input("30");

        // Assert
        Assert.False(changed);
        Assert.All(cut.FindAll("input[type='range']"), i => Assert.Equal("true", i.GetAttribute("aria-readonly")));
    }

    [Fact]
    public void ReadOnly_LocksTheKeysOnBothInputs()
    {
        // Arrange & Act
        var cut = Render(extra: p => p.Add(x => x.ReadOnly, true));
        cut.Render();

        // Assert
        Assert.Equal(2, TestContext.JSInterop.Invocations.Count(i => i.Identifier == lockIdentifier));
    }

    [Fact]
    public void ReadOnly_WhenTheCircuitHasGone_DoesNotThrow()
    {
        // Arrange
        TestContext.JSInterop.SetupVoid(lockIdentifier, _ => true).SetException(new JSDisconnectedException("gone"));

        // Act
        var cut = Render(extra: p => p.Add(x => x.ReadOnly, true));

        // Assert
        Assert.Equal("true", Lower(cut).GetAttribute("aria-readonly"));
    }

    [Fact]
    public void ReadOnly_WhenScriptIsNotAvailableYet_RetriesOnTheNextRender()
    {
        // Arrange
        TestContext.JSInterop.SetupVoid(lockIdentifier, _ => true).SetException(new InvalidOperationException("prerendering"));

        // Act
        var cut = Render(extra: p => p.Add(x => x.ReadOnly, true));
        cut.Render();

        // Assert
        Assert.True(TestContext.JSInterop.Invocations.Count(i => i.Identifier == lockIdentifier) >= 2);
    }

    [Fact]
    public void Disabled_DisablesBothInputs_AndIgnoresMoves()
    {
        // Arrange
        var changed = false;
        var cut = Render(extra: p => p
            .Add(x => x.Disabled, true)
            .Add(x => x.UpperValueChanged, _ => changed = true));

        // Act
        Upper(cut).Input("90");

        // Assert
        Assert.False(changed);
        Assert.All(cut.FindAll("input[type='range']"), i => Assert.True(i.HasAttribute("disabled")));
    }

    [Fact]
    public void Class_IsMergedOntoTheWrapper()
    {
        // Arrange & Act
        var cut = Render(extra: p => p.Add(x => x.Class, "my-range"));

        // Assert
        Assert.Contains("my-range", cut.Find("[role='group']").ClassList);
    }

    [Fact]
    public void UnmatchedAttributes_AreAppliedToTheWrapper()
    {
        // Arrange & Act
        var cut = Render(extra: p => p.AddUnmatched("data-test", "range"));

        // Assert
        Assert.Equal("range", cut.Find("[role='group']").GetAttribute("data-test"));
    }

    [Fact]
    public void Color_TintsTheFill()
    {
        // Arrange & Act
        var cut = Render(extra: p => p.Add(x => x.Color, Color.Danger));
        var expected = Theme.Components.Require<TwSliderTheme>().Colors.Danger.Split(' ')[0];

        // Assert
        Assert.Contains(expected, cut.Find("[style*='width:']").ClassList);
    }

    [Fact]
    public void Inputs_UseTheSliderBase_WithTheRangeSliderOverridesWinning()
    {
        // Arrange & Act
        var cut = Render();
        var sliderBase = Theme.Components.Require<TwSliderTheme>().Base.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var classes = Lower(cut).ClassList;

        // Assert - shared classes come from the slider theme
        Assert.Contains("peer", classes);
        Assert.Contains("appearance-none", sliderBase);
        Assert.Contains("appearance-none", classes);

        // The range slider's position and width replace the slider's, rather than both being applied
        Assert.Contains("inset-[0_-0.625rem]", classes);
        Assert.Contains("w-[calc(100%+1.25rem)]", classes);
        Assert.DoesNotContain("inset-0", classes);
        Assert.DoesNotContain("w-full", classes);
    }

    [Fact]
    public void SharedParts_ComeFromTheSliderTheme()
    {
        // Arrange & Act
        var cut = Render();
        var slider = Theme.Components.Require<TwSliderTheme>();

        // Assert
        Assert.Contains(slider.Track.Split(' ')[0], cut.Find("[role='group'] > div[aria-hidden='true']").ClassList);
        Assert.All(slider.Wrapper.Split(' ', StringSplitOptions.RemoveEmptyEntries), c => Assert.Contains(c, cut.Find("[role='group']").ClassList));
    }

    [Fact]
    public void HandleNearTheEnd_PutsTheLowerHandleOnTop()
    {
        // Arrange & Act - both handles at the end: only the lower one can still move
        var cut = Render(100, 100);

        // Assert
        Assert.Contains("z-index:2", Lower(cut).ParentElement!.GetAttribute("style"));
        Assert.Contains("z-index:1", Upper(cut).ParentElement!.GetAttribute("style"));
    }

    [Fact]
    public void HandleNearTheStart_PutsTheUpperHandleOnTop()
    {
        // Arrange & Act - both handles at the start: only the upper one can still move
        var cut = Render(0, 0);

        // Assert
        Assert.Contains("z-index:1", Lower(cut).ParentElement!.GetAttribute("style"));
        Assert.Contains("z-index:2", Upper(cut).ParentElement!.GetAttribute("style"));
    }

    [Fact]
    public void InsideAnEditForm_ShowsTheValidationMessage()
    {
        // Arrange
        var model = new PriceFilter { Low = 10, High = 20 };
        var context = new EditContext(model);
        var messages = new ValidationMessageStore(context);

        var cut = TestContext.Render<CascadingValue<EditContext>>(p => p
            .Add(x => x.Value, context)
            .Add(x => x.IsFixed, false)
            .AddChildContent<TwRangeSlider<int>>(s => s
                .Add(x => x.LowerValue, model.Low)
                .Add(x => x.LowerValueExpression, () => model.Low)
                .Add(x => x.UpperValue, model.High)
                .Add(x => x.Min, 0)
                .Add(x => x.Max, 100)
                .Add(x => x.Step, 1)));

        // Act
        messages.Add(context.Field(nameof(PriceFilter.Low)), "Pick a higher minimum.");
        context.NotifyValidationStateChanged();

        // Assert
        cut.WaitForAssertion(() =>
        {
            Assert.Contains("Pick a higher minimum.", cut.Markup);
            Assert.All(cut.FindAll("input[type='range']"), i => Assert.Equal("true", i.GetAttribute("aria-invalid")));
        });
    }

    [Fact]
    public void Group_UsesAriaLabelledBy_OverTheLabel()
    {
        // Arrange & Act
        var cut = Render(extra: p => p.Add(x => x.Label, "Price").Add(x => x.AriaLabelledBy, "heading"));

        // Assert
        Assert.Equal("heading", cut.Find("[role='group']").GetAttribute("aria-labelledby"));
    }

    [Fact]
    public void Label_UsesTheGivenLabelId()
    {
        // Arrange & Act
        var cut = Render(extra: p => p.Add(x => x.Label, "Price").Add(x => x.LabelId, "price-heading"));

        // Assert
        Assert.Equal("price-heading", cut.Find("label").GetAttribute("id"));
        Assert.Equal("price-heading", cut.Find("[role='group']").GetAttribute("aria-labelledby"));
    }

    [Fact]
    public void Group_HasNoName_WhenThereIsNoLabelOrAriaLabel()
    {
        // Arrange & Act
        var group = Render().Find("[role='group']");

        // Assert
        Assert.False(group.HasAttribute("aria-label"));
        Assert.False(group.HasAttribute("aria-labelledby"));
    }

    [Fact]
    public void NoName_LeavesTheInputsUnnamed()
    {
        // Arrange & Act
        var cut = Render();

        // Assert
        Assert.All(cut.FindAll("input[type='range']"), i => Assert.False(i.HasAttribute("name")));
    }

    [Fact]
    public void Inputs_AreNotMarkedReadOnlyOrInvalid_ByDefault()
    {
        // Arrange & Act
        var cut = Render();

        // Assert
        Assert.All(cut.FindAll("input[type='range']"), i =>
        {
            Assert.False(i.HasAttribute("aria-readonly"));
            Assert.False(i.HasAttribute("aria-invalid"));
            Assert.False(i.HasAttribute("disabled"));
        });
    }

    [Fact]
    public void Invalid_MarksBothInputs_AndShowsTheErrorMessage()
    {
        // Arrange & Act
        var cut = Render(extra: p => p.Add(x => x.Invalid, true).Add(x => x.ErrorMessage, "Pick a range."));

        // Assert
        Assert.Contains("Pick a range.", cut.Markup);
        Assert.All(cut.FindAll("input[type='range']"), i => Assert.Equal("true", i.GetAttribute("aria-invalid")));
    }

    [Fact]
    public void MinEqualToMax_PutsBothHandlesAtTheStart()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwRangeSlider<int>>(p => p
            .Add(x => x.LowerValue, 5)
            .Add(x => x.UpperValue, 5)
            .Add(x => x.Min, 5)
            .Add(x => x.Max, 5)
            .Add(x => x.Step, 1));

        // Assert
        Assert.Contains("inset-inline-start:0%;width:0%", cut.Find("[style*='width:']").GetAttribute("style"));
    }

    [Fact]
    public void UpperValueBelowTheLowerValue_DrawsAnEmptyFill()
    {
        // Arrange & Act
        var cut = Render(60, 40);

        // Assert
        Assert.Contains("inset-inline-start:60%;width:0%", cut.Find("[style*='width:']").GetAttribute("style"));
    }

    [Fact]
    public void NullableValues_WithNoValue_StartAtTheEnds_AndCanBeMoved()
    {
        // Arrange
        var cut = TestContext.Render<TwRangeSlider<int?>>(p => p
            .Add(x => x.LowerValue, null)
            .Add(x => x.UpperValue, null)
            .Add(x => x.Min, 0)
            .Add(x => x.Max, 100)
            .Add(x => x.Step, 1));
        var inputs = cut.FindAll("input[type='range']");
        Assert.False(inputs[0].HasAttribute("value"));

        // Act
        cut.FindAll("input[type='range']")[0].Input("30");
        cut.FindAll("input[type='range']")[1].Input("10");

        // Assert - with no upper value the lower handle can use the whole track,
        // and the upper handle stops at the lower one
        Assert.Equal(30, cut.Instance.LowerValue);
        Assert.Equal(30, cut.Instance.UpperValue);
    }

    [Fact]
    public void NullableValues_UpperHandle_WithNoLowerValue_StopsAtMin()
    {
        // Arrange
        var cut = TestContext.Render<TwRangeSlider<int?>>(p => p
            .Add(x => x.LowerValue, null)
            .Add(x => x.UpperValue, 50)
            .Add(x => x.Min, 10)
            .Add(x => x.Max, 100)
            .Add(x => x.Step, 1));

        // Act
        cut.FindAll("input[type='range']")[1].Input("0");

        // Assert
        Assert.Equal(10, cut.Instance.UpperValue);
    }

    [Fact]
    public void NonNumericType_IgnoresMoves()
    {
        // Arrange - DateTime can't be turned into or made from a number
        var changed = false;
        var cut = TestContext.Render<TwRangeSlider<DateTime>>(p => p
            .Add(x => x.LowerValue, new DateTime(2026, 1, 1))
            .Add(x => x.UpperValue, new DateTime(2026, 6, 1))
            .Add(x => x.Min, DateTime.MinValue)
            .Add(x => x.Max, DateTime.MaxValue)
            .Add(x => x.Step, DateTime.MinValue)
            .Add(x => x.LowerValueChanged, _ => changed = true)
            .Add(x => x.UpperValueChanged, _ => changed = true));

        // Act
        cut.FindAll("input[type='range']")[0].Input("30");
        cut.FindAll("input[type='range']")[1].Input("30");

        // Assert
        Assert.False(changed);
        Assert.Contains("inset-inline-start:0%;width:0%", cut.Find("[style*='width:']").GetAttribute("style"));
    }

    [Fact]
    public void NonFormattableType_WritesItsTextToTheInput()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwRangeSlider<string>>(p => p
            .Add(x => x.LowerValue, "20")
            .Add(x => x.UpperValue, "80")
            .Add(x => x.Min, "0")
            .Add(x => x.Max, "100")
            .Add(x => x.Step, "1"));

        // Act
        cut.FindAll("input[type='range']")[0].Input("30");

        // Assert
        Assert.Equal("30", cut.FindAll("input[type='range']")[0].GetAttribute("value"));
        Assert.Equal("30", cut.Instance.LowerValue);
    }

    [Fact]
    public void NullEventValue_IsIgnored()
    {
        // Arrange
        var changed = false;
        var cut = Render(extra: p => p.Add(x => x.UpperValueChanged, _ => changed = true));

        // Act
        Upper(cut).Input(new ChangeEventArgs { Value = null });

        // Assert
        Assert.False(changed);
    }

    [Fact]
    public void ReadOnly_IgnoresUpperMoves()
    {
        // Arrange
        var changed = false;
        var cut = Render(extra: p => p
            .Add(x => x.ReadOnly, true)
            .Add(x => x.UpperValueChanged, _ => changed = true));

        // Act
        Upper(cut).Input("90");

        // Assert
        Assert.False(changed);
    }

    [Fact]
    public void Disabled_IgnoresLowerMoves_AndDimsTheTrackAndThumbs()
    {
        // Arrange
        var changed = false;
        var cut = Render(extra: p => p
            .Add(x => x.Disabled, true)
            .Add(x => x.LowerValueChanged, _ => changed = true));

        // Act
        Lower(cut).Input("10");

        // Assert
        Assert.False(changed);
        Assert.Contains("cursor-not-allowed", Lower(cut).ClassList);
        Assert.Contains("opacity-40", cut.Find("[role='group'] > div[aria-hidden='true']").ClassList);
    }

    [Fact]
    public void ReadOnlyAndDisabled_DoesNotLockTheKeys()
    {
        // Arrange & Act
        Render(extra: p => p.Add(x => x.ReadOnly, true).Add(x => x.Disabled, true));

        // Assert
        Assert.DoesNotContain(TestContext.JSInterop.Invocations, i => i.Identifier == lockIdentifier);
    }

    [Fact]
    public void ReadOnly_TurnedOff_UnlocksTheKeys()
    {
        // Arrange
        var cut = Render(extra: p => p.Add(x => x.ReadOnly, true));

        // Act
        cut.Render(p => p.Add(x => x.ReadOnly, false));

        // Assert
        var calls = TestContext.JSInterop.Invocations.Where(i => i.Identifier == lockIdentifier).ToList();
        Assert.Equal(4, calls.Count);
        Assert.All(calls.Skip(2), call => Assert.False(Assert.IsType<bool>(call.Arguments[1])));
    }

    [Fact]
    public void NegativeMinDistance_IsTreatedAsZero()
    {
        // Arrange
        var cut = Render(20, 50, p => p.Add(x => x.MinDistance, -10));

        // Act
        Lower(cut).Input("60");

        // Assert
        Assert.Equal(50, cut.Instance.LowerValue);
    }

    [Fact]
    public void UpperHandle_RejectedMove_WritesTheAcceptedValueBackToTheInput()
    {
        // Arrange - both handles at the start
        var cut = Render(0, 0);

        // Act - the browser has moved the upper input below the lower handle
        Upper(cut).Input("-1");

        // Assert
        Assert.Equal("0", Upper(cut).GetAttribute("value"));
        Assert.Equal(0, cut.Instance.UpperValue);
    }

    [Fact]
    public void InsideAnEditForm_ValidatesTheUpperValue_WhenOnlyItIsBound()
    {
        // Arrange
        var model = new PriceFilter { Low = 10, High = 20 };
        var context = new EditContext(model);
        var messages = new ValidationMessageStore(context);

        var cut = TestContext.Render<CascadingValue<EditContext>>(p => p
            .Add(x => x.Value, context)
            .Add(x => x.IsFixed, false)
            .AddChildContent<TwRangeSlider<int>>(s => s
                .Add(x => x.LowerValue, model.Low)
                .Add(x => x.UpperValue, model.High)
                .Add(x => x.UpperValueExpression, () => model.High)
                .Add(x => x.Min, 0)
                .Add(x => x.Max, 100)
                .Add(x => x.Step, 1)));

        // Act
        messages.Add(context.Field(nameof(PriceFilter.High)), "Pick a lower maximum.");
        context.NotifyValidationStateChanged();

        // Assert
        cut.WaitForAssertion(() => Assert.Contains("Pick a lower maximum.", cut.Markup));
    }

    [Fact]
    public void InsideAnEditForm_WithNoBoundExpression_IgnoresValidationMessages()
    {
        // Arrange
        var model = new PriceFilter { Low = 10, High = 20 };
        var context = new EditContext(model);
        var messages = new ValidationMessageStore(context);

        var cut = TestContext.Render<CascadingValue<EditContext>>(p => p
            .Add(x => x.Value, context)
            .Add(x => x.IsFixed, false)
            .AddChildContent<TwRangeSlider<int>>(s => s
                .Add(x => x.LowerValue, model.Low)
                .Add(x => x.UpperValue, model.High)
                .Add(x => x.Min, 0)
                .Add(x => x.Max, 100)
                .Add(x => x.Step, 1)));

        // Act
        messages.Add(context.Field(nameof(PriceFilter.Low)), "Pick a higher minimum.");
        context.NotifyValidationStateChanged();

        // Assert
        Assert.DoesNotContain("Pick a higher minimum.", cut.Markup);
    }

    private sealed class PriceFilter
    {
        public int Low { get; set; }

        public int High { get; set; }
    }
}
