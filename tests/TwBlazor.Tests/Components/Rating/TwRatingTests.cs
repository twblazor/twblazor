using AngleSharp.Dom;
using Bunit;
using Microsoft.AspNetCore.Components.Web;
using System.Globalization;
using TwBlazor.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using ThemeBase = TwBlazor.Theme.Theme;

namespace TwBlazor.Tests.Components.Rating;

public class TwRatingTests : TwBlazorTestBase
{
    private TwRatingTheme ratingTheme => Theme.Components.Require<TwRatingTheme>();

    private static string[] Widths(IRenderedComponent<TwRating<double>> cut) =>
        [.. cut.FindAll("[aria-hidden='true'] > span").Select(clip => clip.GetAttribute("style")!)];

    #region Rendering

    [Fact]
    public void Renders_FiveIcons_ByDefault()
    {
        // Act
        var cut = TestContext.Render<TwRating<double>>();

        // Assert
        Assert.Equal(5, cut.FindAll("[role='radiogroup'] > span").Count);
    }

    [Fact]
    public void Renders_OneIconPerWholeStep_UpToMax()
    {
        // Act
        var cut = TestContext.Render<TwRating<int>>(p => p.Add(x => x.Max, 10));

        // Assert
        Assert.Equal(10, cut.FindAll("[role='radiogroup'] > span").Count);
    }

    [Fact]
    public void IntegerType_RatesInWholeSteps()
    {
        // Act
        var cut = TestContext.Render<TwRating<int>>();

        // Assert - a radio for 1 to 5, plus No rating
        var values = cut.FindAll("input[type='radio']").Select(input => input.GetAttribute("value")).ToArray();
        Assert.Equal(["0", "1", "2", "3", "4", "5"], values);
    }

    [Theory]
    [InlineData(typeof(double))]
    [InlineData(typeof(decimal))]
    [InlineData(typeof(float))]
    public void FractionalTypes_RateInHalfSteps(Type type)
    {
        // Act
        var cut = TestContext.Render(builder =>
        {
            builder.OpenComponent(0, typeof(TwRating<>).MakeGenericType(type));
            builder.CloseComponent();
        });

        // Assert
        var values = cut.FindAll("input[type='radio']").Select(input => input.GetAttribute("value")).ToArray();
        Assert.Equal(["0", "0.5", "1", "1.5", "2", "2.5", "3", "3.5", "4", "4.5", "5"], values);
    }

    [Fact]
    public void HalfValue_FillsHalfAnIcon()
    {
        // Act
        var cut = TestContext.Render<TwRating<double>>(p => p.Add(x => x.Value, 3.5));

        // Assert
        Assert.Equal(["width:100%", "width:100%", "width:100%", "width:50%", "width:0%"], Widths(cut));
    }

    [Fact]
    public void UsesTheChosenIcons()
    {
        // Act
        var cut = TestContext.Render<TwRating<double>>(p => p
            .Add(x => x.Icon, Enums.Icon.Heart_Fill)
            .Add(x => x.EmptyIcon, Enums.Icon.Heart));

        // Assert
        Assert.Equal(5, cut.FindAll("i.bi-heart-fill").Count);
        Assert.Equal(5, cut.FindAll("i.bi-heart").Count);
    }

    [Fact]
    public void EmptyIcon_DefaultsToTheIcon()
    {
        // Act
        var cut = TestContext.Render<TwRating<double>>();

        // Assert - the same glyph twice per step: the muted base and the filled copy
        Assert.Equal(10, cut.FindAll("i.bi-star-fill").Count);
    }

    [Fact]
    public void FilledIcons_UseTheWarningColor_ByDefault_AndTheChosenColorOtherwise()
    {
        // Act
        var plain = TestContext.Render<TwRating<double>>();
        var colored = TestContext.Render<TwRating<double>>(p => p.Add(x => x.Color, Color.Danger));

        // Assert
        Assert.Contains(ratingTheme.Colors.Warning.Split(' ')[0], plain.Find("[aria-hidden='true'] > span > i").ClassName);
        Assert.Contains(ratingTheme.Colors.Danger.Split(' ')[0], colored.Find("[aria-hidden='true'] > span > i").ClassName);
    }

    [Theory]
    [InlineData(Size.Sm)]
    [InlineData(Size.Md)]
    [InlineData(Size.Lg)]
    public void AppliesTheSizeClasses(Size size)
    {
        // Act
        var cut = TestContext.Render<TwRating<double>>(p => p.Add(x => x.Size, size));

        // Assert
        var expected = size switch { Size.Sm => ratingTheme.Small, Size.Lg => ratingTheme.Large, _ => ratingTheme.Medium };
        Assert.Contains(expected, cut.Find("[role='radiogroup']").ClassName);
    }

    [Fact]
    public void FractionalMax_EndsOnAHalfIcon()
    {
        // Act
        var cut = TestContext.Render<TwRating<double>>(p => p
            .Add(x => x.Max, 4.5)
            .Add(x => x.Value, 9));

        // Assert
        Assert.Equal(5, cut.FindAll("[role='radiogroup'] > span").Count);
        var radios = cut.FindAll("input[type='radio']");
        Assert.Equal("4.5", radios[^1].GetAttribute("value"));
        Assert.Equal(["width:100%", "width:100%", "width:100%", "width:100%", "width:50%"], Widths(cut));
    }

    [Fact]
    public void MaxBelowOneStep_StillShowsOneStep()
    {
        // Act
        var cut = TestContext.Render<TwRating<int>>(p => p.Add(x => x.Max, 0));

        // Assert
        Assert.Single(cut.FindAll("[role='radiogroup'] > span"));
        Assert.Equal("1 out of 1", cut.Find("input[value='1']").GetAttribute("aria-label"));
    }

    [Fact]
    public void NegativeMin_IsTreatedAsZero()
    {
        // Act
        var cut = TestContext.Render<TwRating<int>>(p => p.Add(x => x.Min, -3));

        // Assert
        Assert.NotNull(cut.Find("input[data-rating-empty]"));
        Assert.Equal("1", cut.FindAll("input[type='radio']")[1].GetAttribute("value"));
    }

    [Fact]
    public void MinAboveMax_LeavesOnlyTheTopValue()
    {
        // Act
        var cut = TestContext.Render<TwRating<int>>(p => p.Add(x => x.Min, 8));

        // Assert
        Assert.Equal(["5"], cut.FindAll("input[type='radio']").Select(input => input.GetAttribute("value")).ToArray());
    }

    [Fact]
    public void HugeAndNegativeValues_AreDisplayedClamped()
    {
        // Act
        var huge = TestContext.Render<TwRating<double>>(p => p.Add(x => x.Value, double.MaxValue));
        var negative = TestContext.Render<TwRating<double>>(p => p.Add(x => x.Value, -2));

        // Assert
        Assert.Equal("5", huge.Find("input:checked").GetAttribute("value"));
        Assert.NotNull(negative.Find("input[data-rating-empty]:checked"));
    }

    [Fact]
    public void Class_IsAddedToTheGroup()
    {
        // Act
        var cut = TestContext.Render<TwRating<double>>(p => p.Add(x => x.Class, "my-rating"));

        // Assert
        Assert.Contains("my-rating", cut.Find("[role='radiogroup']").ClassName);
    }

    #endregion

    #region Rounding

    [Theory]
    [InlineData(3.7, MidpointRounding.AwayFromZero, "3.5")]
    [InlineData(3.75, MidpointRounding.AwayFromZero, "4")]
    [InlineData(3.2, MidpointRounding.AwayFromZero, "3")]
    [InlineData(3.9, MidpointRounding.ToNegativeInfinity, "3.5")]
    [InlineData(3.1, MidpointRounding.ToPositiveInfinity, "3.5")]
    [InlineData(3.25, MidpointRounding.ToEven, "3")]
    public void DisplayedValue_IsRoundedToAHalf_UsingRounding(double value, MidpointRounding rounding, string expected)
    {
        // Act
        var cut = TestContext.Render<TwRating<double>>(p => p
            .Add(x => x.Value, value)
            .Add(x => x.Rounding, rounding));

        // Assert
        Assert.Equal(expected, cut.Find("input:checked").GetAttribute("value"));
    }

    [Fact]
    public void Rounding_DoesNotRewriteTheBoundValue()
    {
        // Arrange
        var changes = 0;

        // Act
        TestContext.Render<TwRating<double>>(p => p
            .Add(x => x.Value, 3.7)
            .Add(x => x.ValueChanged, (double _) => changes++));

        // Assert
        Assert.Equal(0, changes);
    }

    [Fact]
    public void ValuesOutsideTheRange_AreDisplayedClamped()
    {
        // Act
        var cut = TestContext.Render<TwRating<double>>(p => p.Add(x => x.Value, 9));

        // Assert
        Assert.Equal("5", cut.Find("input:checked").GetAttribute("value"));
    }

    #endregion

    #region Selecting

    [Fact]
    public void ChangingARadio_UpdatesTheValue()
    {
        // Arrange
        double? changed = null;
        var cut = TestContext.Render<TwRating<double>>(p => p
            .Add(x => x.Value, 1)
            .Add(x => x.ValueChanged, (double value) => changed = value));

        // Act
        cut.Find("input[value='3.5']").Change(true);

        // Assert
        Assert.Equal(3.5, changed);
        Assert.Equal("3.5", cut.Find("input:checked").GetAttribute("value"));
    }

    [Fact]
    public void ChangingARadio_ConvertsToTheBoundType()
    {
        // Arrange
        decimal? changed = null;
        var cut = TestContext.Render<TwRating<decimal>>(p => p
            .Add(x => x.ValueChanged, (decimal value) => changed = value));

        // Act
        cut.Find("input[value='2.5']").Change(true);

        // Assert
        Assert.Equal(2.5m, changed);
    }

    [Fact]
    public void ClickingTheSelectedValue_ClearsTheRating()
    {
        // Arrange
        double? changed = null;
        var cut = TestContext.Render<TwRating<double>>(p => p
            .Add(x => x.Value, 3)
            .Add(x => x.ValueChanged, (double value) => changed = value));
        var id = cut.Find("input[value='3']").Id;

        // Act
        cut.Find($"label[for='{id}']").Click();

        // Assert
        Assert.Equal(0, changed);
        Assert.NotNull(cut.Find("input[data-rating-empty]:checked"));
    }

    [Fact]
    public void ClickingAnotherValue_DoesNotClear()
    {
        // Arrange
        var changes = 0;
        var cut = TestContext.Render<TwRating<double>>(p => p
            .Add(x => x.Value, 3)
            .Add(x => x.ValueChanged, (double _) => changes++));
        var id = cut.Find("input[value='4']").Id;

        // Act - the label click itself changes nothing; the radio's change event that follows does
        cut.Find($"label[for='{id}']").Click();

        // Assert
        Assert.Equal(0, changes);
    }

    [Fact]
    public void Min_HidesLowerValues_AndPreventsClearing()
    {
        // Arrange
        var changes = 0;
        var cut = TestContext.Render<TwRating<int>>(p => p
            .Add(x => x.Min, 2)
            .Add(x => x.Value, 2)
            .Add(x => x.ValueChanged, (int _) => changes++));

        // Assert
        Assert.Empty(cut.FindAll("input[data-rating-empty]"));
        Assert.Equal(["2", "3", "4", "5"], cut.FindAll("input[type='radio']").Select(input => input.GetAttribute("value")).ToArray());

        // Act
        cut.Find($"label[for='{cut.Find("input[value='2']").Id}']").Click();

        // Assert
        Assert.Equal(0, changes);
    }

    [Fact]
    public void Disabled_IgnoresChanges()
    {
        // Arrange
        var changes = 0;
        var cut = TestContext.Render<TwRating<double>>(p => p
            .Add(x => x.Disabled, true)
            .Add(x => x.ValueChanged, (double _) => changes++));

        // Act
        cut.Find("input[value='4']").Change(true);

        // Assert
        Assert.Equal(0, changes);
    }

    #endregion

    #region Hover preview

    [Fact]
    public void Hovering_PreviewsTheValue_UntilThePointerLeaves()
    {
        // Arrange
        var cut = TestContext.Render<TwRating<double>>(p => p.Add(x => x.Value, 1));
        var id = cut.Find("input[value='2.5']").Id;

        // Act
        cut.Find($"label[for='{id}']").TriggerEvent("onmouseenter", new MouseEventArgs());

        // Assert
        Assert.Equal(["width:100%", "width:100%", "width:50%", "width:0%", "width:0%"], Widths(cut));

        // Act
        cut.Find("[role='radiogroup']").TriggerEvent("onmouseleave", new MouseEventArgs());

        // Assert
        Assert.Equal(["width:100%", "width:0%", "width:0%", "width:0%", "width:0%"], Widths(cut));
    }

    #endregion

    #region Animation

    [Fact]
    public void Picking_PopsTheNewlyFilledIcons_InOrder()
    {
        // Arrange
        var cut = TestContext.Render<TwRating<double>>(p => p.Add(x => x.Value, 2));

        // Act
        cut.Find("input[value='4']").Change(true);

        // Assert
        var popping = cut.FindAll($".{ratingTheme.Pop}");
        Assert.Equal(2, popping.Count);
        Assert.Equal("animation-delay:0ms", popping[0].GetAttribute("style"));
        Assert.Equal("animation-delay:50ms", popping[1].GetAttribute("style"));
    }

    [Fact]
    public void Lowering_PopsOnlyThePickedIcon()
    {
        // Arrange
        var cut = TestContext.Render<TwRating<double>>(p => p.Add(x => x.Value, 4));

        // Act
        cut.Find("input[value='2']").Change(true);

        // Assert
        Assert.Single(cut.FindAll($".{ratingTheme.Pop}"));
    }

    [Fact]
    public void ConsecutivePicks_AlternateTheKeyframes_SoThePopReplays()
    {
        // Arrange
        var cut = TestContext.Render<TwRating<double>>(p => p.Add(x => x.Value, 2));

        // Act
        cut.Find("input[value='3']").Change(true);
        var first = cut.Find($".{ratingTheme.Pop}").GetAttribute("data-pop");
        cut.Find("input[value='2']").Change(true);
        cut.Find("input[value='3']").Change(true);
        var third = cut.Find($".{ratingTheme.Pop}").GetAttribute("data-pop");
        cut.Find("input[value='2']").Change(true);
        var fourth = cut.Find($".{ratingTheme.Pop}").GetAttribute("data-pop");

        // Assert
        Assert.Equal(first, third);
        Assert.NotEqual(third, fourth);
    }

    [Fact]
    public void DisableAnimation_TurnsOffThePopAndTheHoverGrowth()
    {
        // Arrange
        var cut = TestContext.Render<TwRating<double>>(p => p.Add(x => x.DisableAnimation, true));

        // Act
        cut.Find("input[value='3']").Change(true);

        // Assert
        Assert.Empty(cut.FindAll($".{ratingTheme.Pop}"));
        Assert.DoesNotContain(ratingTheme.ItemHover, cut.Find("[role='radiogroup'] > span").ClassName);
    }

    [Fact]
    public void ItemsGrowOnHover_WhenAnimated()
    {
        // Act
        var cut = TestContext.Render<TwRating<double>>();

        // Assert
        Assert.Contains(ratingTheme.ItemHover, cut.Find("[role='radiogroup'] > span").ClassName);
    }

    #endregion

    #region Accessibility

    [Fact]
    public void IsARadioGroup_NamedByItsLabel()
    {
        // Act
        var cut = TestContext.Render<TwRating<double>>(p => p
            .Add(x => x.Id, "stay")
            .Add(x => x.Label, "How was your stay?"));

        // Assert
        var group = cut.Find("[role='radiogroup']");
        Assert.Equal("stay-label", group.GetAttribute("aria-labelledby"));
        Assert.Equal("How was your stay?", cut.Find("#stay-label").TextContent.Trim());
    }

    [Fact]
    public void LabelId_IsUsedForTheLabel()
    {
        // Act
        var cut = TestContext.Render<TwRating<double>>(p => p
            .Add(x => x.Label, "Score")
            .Add(x => x.LabelId, "score-heading"));

        // Assert
        Assert.Equal("score-heading", cut.Find("[role='radiogroup']").GetAttribute("aria-labelledby"));
        Assert.NotNull(cut.Find("#score-heading"));
    }

    [Fact]
    public void AriaLabelledBy_TakesPrecedenceOverTheLabel()
    {
        // Act
        var cut = TestContext.Render<TwRating<double>>(p => p
            .Add(x => x.Label, "Score")
            .Add(x => x.AriaLabelledBy, "external-heading"));

        // Assert
        Assert.Equal("external-heading", cut.Find("[role='radiogroup']").GetAttribute("aria-labelledby"));
    }

    [Fact]
    public void AriaLabel_NamesAnUnlabelledGroup()
    {
        // Act
        var cut = TestContext.Render<TwRating<double>>(p => p.Add(x => x.AriaLabel, "Product rating"));

        // Assert
        var group = cut.Find("[role='radiogroup']");
        Assert.Equal("Product rating", group.GetAttribute("aria-label"));
        Assert.Null(group.GetAttribute("aria-labelledby"));
        Assert.Empty(cut.FindAll("label:not([for])"));
    }

    [Fact]
    public void ReadOnly_WithAriaLabelledBy_IsNamedByThatElementAndTheValue()
    {
        // Act
        var cut = TestContext.Render<TwRating<double>>(p => p
            .Add(x => x.Id, "average")
            .Add(x => x.AriaLabelledBy, "external-heading")
            .Add(x => x.Value, 3)
            .Add(x => x.ReadOnly, true));

        // Assert
        Assert.Equal("external-heading average-value", cut.Find("[role='img']").GetAttribute("aria-labelledby"));
    }

    [Fact]
    public void BecomingReadOnly_DropsTheHoverPreview()
    {
        // Arrange
        var cut = TestContext.Render<TwRating<double>>(p => p.Add(x => x.Value, 1));
        cut.Find($"label[for='{cut.Find("input[value='4']").Id}']").TriggerEvent("onmouseenter", new MouseEventArgs());

        // Act
        cut.Render(p => p.Add(x => x.ReadOnly, true));

        // Assert
        Assert.Equal(["width:100%", "width:0%", "width:0%", "width:0%", "width:0%"],
            cut.FindAll("[aria-hidden='true'] > span").Select(clip => clip.GetAttribute("style")!).ToArray());
    }

    [Fact]
    public void RadiosShareAName_AndAreEachNamedByTheirValue()
    {
        // Act
        var cut = TestContext.Render<TwRating<double>>(p => p.Add(x => x.Id, "stay"));

        // Assert
        var radios = cut.FindAll("input[type='radio']");
        Assert.All(radios, radio => Assert.Equal("stay", radio.GetAttribute("name")));
        Assert.Equal("No rating", radios[0].GetAttribute("aria-label"));
        Assert.Equal("3.5 out of 5", cut.Find("input[value='3.5']").GetAttribute("aria-label"));
    }

    [Fact]
    public void Name_OverridesTheRadioName()
    {
        // Act
        var cut = TestContext.Render<TwRating<double>>(p => p.Add(x => x.Name, "score"));

        // Assert
        Assert.All(cut.FindAll("input[type='radio']"), radio => Assert.Equal("score", radio.GetAttribute("name")));
    }

    [Fact]
    public void IconsAreHiddenFromAssistiveTech()
    {
        // Act
        var cut = TestContext.Render<TwRating<double>>();

        // Assert
        Assert.All(cut.FindAll("i.bi"), icon => Assert.NotNull(icon.Closest("[aria-hidden='true']")));
    }

    [Fact]
    public void NoRatingIsChecked_WhenTheValueIsZero()
    {
        // Act
        var cut = TestContext.Render<TwRating<double>>();

        // Assert
        Assert.Single(cut.FindAll("input:checked"));
        Assert.NotNull(cut.Find("input[data-rating-empty]:checked"));
    }

    [Fact]
    public void Disabled_DisablesTheRadios_AndRemovesThePointerTargets()
    {
        // Act
        var cut = TestContext.Render<TwRating<double>>(p => p.Add(x => x.Disabled, true));

        // Assert
        Assert.All(cut.FindAll("input[type='radio']"), radio => Assert.True(radio.HasAttribute("disabled")));
        Assert.Empty(cut.FindAll("label[for]"));
        Assert.Equal("true", cut.Find("[role='radiogroup']").GetAttribute("aria-disabled"));
    }

    [Fact]
    public void Invalid_DescribesTheGroupWithTheError()
    {
        // Act
        var cut = TestContext.Render<TwRating<double>>(p => p
            .Add(x => x.Id, "order")
            .Add(x => x.Invalid, true)
            .Add(x => x.ErrorMessage, "Please pick a rating."));

        // Assert
        var group = cut.Find("[role='radiogroup']");
        Assert.Equal("true", group.GetAttribute("aria-invalid"));
        Assert.Equal("order-error", group.GetAttribute("aria-describedby"));
        Assert.Equal("Please pick a rating.", cut.Find("#order-error").TextContent);
    }

    [Fact]
    public void ReadOnly_IsASingleImage_NamedAfterItsValue()
    {
        // Act
        var cut = TestContext.Render<TwRating<double>>(p => p.Add(x => x.Value, 4.5).Add(x => x.ReadOnly, true));

        // Assert
        var image = cut.Find("[role='img']");
        Assert.Equal("Rated 4.5 out of 5", image.GetAttribute("aria-label"));
        Assert.Empty(cut.FindAll("input"));
        Assert.Empty(cut.FindAll("label[for]"));
        Assert.DoesNotContain(ratingTheme.ItemHover, cut.Find("[role='img'] > span").ClassName);
    }

    [Fact]
    public void ReadOnly_WithALabel_IsNamedByTheLabelAndTheValue()
    {
        // Act
        var cut = TestContext.Render<TwRating<double>>(p => p
            .Add(x => x.Id, "average")
            .Add(x => x.Label, "Average review")
            .Add(x => x.Value, 4.5)
            .Add(x => x.ReadOnly, true));

        // Assert
        var image = cut.Find("[role='img']");
        Assert.Null(image.GetAttribute("aria-label"));
        Assert.Equal("average-label average-value", image.GetAttribute("aria-labelledby"));
        Assert.Equal("Rated 4.5 out of 5", cut.Find("#average-value").TextContent);
    }

    [Fact]
    public void ReadOnly_WithAnAriaLabel_PrefixesTheValue()
    {
        // Act
        var cut = TestContext.Render<TwRating<double>>(p => p
            .Add(x => x.AriaLabel, "Average review")
            .Add(x => x.Value, 4)
            .Add(x => x.ReadOnly, true));

        // Assert
        Assert.Equal("Average review, Rated 4 out of 5", cut.Find("[role='img']").GetAttribute("aria-label"));
    }

    [Fact]
    public void AnnouncesValues_InTheConfiguredLanguage()
    {
        // Arrange
        using var context = new BunitContext();
        context.Services.AddTwBlazor(options => options.Localization.Culture = CultureInfo.GetCultureInfo("fr"), ThemeBase.CreateDefaultTheme());
        context.JSInterop.Mode = JSRuntimeMode.Loose;

        // Act
        var cut = context.Render<TwRating<double>>();

        // Assert - French writes the decimal with a comma
        Assert.Equal("3,5 sur 5", cut.Find("input[value='3.5']").GetAttribute("aria-label"));
        Assert.Equal("Aucune note", cut.Find("input[data-rating-empty]").GetAttribute("aria-label"));
    }

    #endregion
}
