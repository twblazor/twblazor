using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using TwBlazor.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Localization;

namespace TwBlazor.Tests.Components.Textfield;

public class TwTextfieldClearableTests : TwBlazorTestBase
{
    private TwInputTheme inputTheme => Theme.Components.Require<TwInputTheme>();

    private string clearLabel => Localizer[TwStrings.Textfield_Clear];

    [Fact]
    public void WithoutClearableOrContent_TheInputIsNotWrapped()
    {
        // Act
        var cut = TestContext.Render<TwTextfield<string>>(p => p.Add(x => x.Value, "Hello"));

        // Assert
        Assert.Empty(cut.FindAll("button"));
        Assert.NotEqual(inputTheme.FieldWrapper, cut.Find("input").ParentElement!.GetAttribute("class"));
    }

    [Fact]
    public void Clearable_ShowsTheButton_WhileThereIsAValue()
    {
        // Act
        var cut = TestContext.Render<TwTextfield<string>>(p => p.Add(x => x.Id, "search").Add(x => x.Clearable, true).Add(x => x.Value, "Order 1042"));

        // Assert
        var button = cut.Find($"button[aria-label='{clearLabel}']");
        Assert.Equal("button", button.GetAttribute("type"));
        Assert.Equal("-1", button.GetAttribute("tabindex"));
        Assert.Equal("search", button.GetAttribute("aria-controls"));
        Assert.Contains(inputTheme.ClearablePadding, cut.Find("input").GetAttribute("class"));
    }

    [Fact]
    public void Clearable_DisablesTheButton_WhileEmpty()
    {
        // Act
        var cut = TestContext.Render<TwTextfield<string>>(p => p.Add(x => x.Clearable, true));

        // Assert
        Assert.True(cut.Find($"button[aria-label='{clearLabel}']").HasAttribute("disabled"));
    }

    [Fact]
    public void Clearable_EnablesTheButton_WhileThereIsAValue()
    {
        // Act
        var cut = TestContext.Render<TwTextfield<string>>(p => p.Add(x => x.Clearable, true).Add(x => x.Value, "x"));

        // Assert
        Assert.False(cut.Find($"button[aria-label='{clearLabel}']").HasAttribute("disabled"));
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void Clearable_HidesTheButton_WhenDisabledOrReadOnly(bool disabled, bool readOnly)
    {
        // Act
        var cut = TestContext.Render<TwTextfield<string>>(p => p
            .Add(x => x.Clearable, true)
            .Add(x => x.Value, "Locked")
            .Add(x => x.Disabled, disabled)
            .Add(x => x.ReadOnly, readOnly));

        // Assert
        Assert.Empty(cut.FindAll("button"));
        Assert.DoesNotContain(inputTheme.ClearablePadding, cut.Find("input").GetAttribute("class"));
    }

    [Fact]
    public void Clearable_IsIgnored_ForMultiline()
    {
        // Act
        var cut = TestContext.Render<TwTextfield<string>>(p => p.Add(x => x.Clearable, true).Add(x => x.Multiline, true).Add(x => x.Value, "Notes"));

        // Assert
        Assert.Empty(cut.FindAll("button"));
    }

    [Fact]
    public async Task Clearing_EmptiesTheValue_ThenInvokesOnClear()
    {
        // Arrange
        var calls = new List<string>();
        string? value = "Order 1042";
        var cut = TestContext.Render<TwTextfield<string>>(p => p
            .Add(x => x.Clearable, true)
            .Add(x => x.Value, value)
            .Add(x => x.ValueChanged, v =>
            {
                value = v;
                calls.Add("changed");
            })
            .Add(x => x.OnClear, () => calls.Add("cleared")));

        // Act
        await cut.Find("button").ClickAsync(new MouseEventArgs());

        // Assert
        Assert.Null(value);
        Assert.Equal(["changed", "cleared"], calls);
        Assert.True(cut.Find("button").HasAttribute("disabled"));
    }

    [Fact]
    public async Task Clearing_RunsTheValidator()
    {
        // Arrange
        var cut = TestContext.Render<TwTextfield<string>>(p => p
            .Add(x => x.Clearable, true)
            .Add(x => x.Value, "jack")
            .Add(x => x.Validator, v => Task.FromResult<string?>(string.IsNullOrEmpty(v) ? "Username is required." : null)));

        // Act
        await cut.Find("button").ClickAsync(new MouseEventArgs());

        // Assert
        Assert.Equal("Username is required.", cut.Find("[role='alert']").TextContent);
    }

    [Fact]
    public async Task Clearing_MovesFocusBackToTheField()
    {
        // Arrange
        var cut = TestContext.Render<TwTextfield<string>>(p => p.Add(x => x.Clearable, true).Add(x => x.Value, "x"));

        // Act
        await cut.Find("button").ClickAsync(new MouseEventArgs());

        // Assert
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "Blazor._internal.domWrapper.focus");
    }

    [Fact]
    public async Task Clearing_ANonStringValue_SetsItsDefault()
    {
        // Arrange
        var value = 42;
        var cut = TestContext.Render<TwTextfield<int>>(p => p
            .Add(x => x.Clearable, true)
            .Add(x => x.Value, value)
            .Add(x => x.ValueChanged, v => value = v));

        // Act
        await cut.Find("button").ClickAsync(new MouseEventArgs());

        // Assert
        Assert.Equal(0, value);
    }

    [Fact]
    public void ErrorMessage_IsOnlyShown_WhileInvalid()
    {
        // Arrange
        var cut = TestContext.Render<TwTextfield<string>>(p => p.Add(x => x.Invalid, true).Add(x => x.ErrorMessage, "Required."));
        Assert.Equal("Required.", cut.Find("[role='alert']").TextContent);

        // Act
        cut.Render(p => p.Add(x => x.Invalid, false));

        // Assert
        Assert.Empty(cut.FindAll("[role='alert']"));
    }

    [Fact]
    public void StartAndEndContent_AreDrawnOverTheField()
    {
        // Act
        var cut = TestContext.Render<TwTextfield<string>>(p => p
            .Add(x => x.StartContent, (RenderFragment)(b => b.AddMarkupContent(0, "<span id=\"start\">£</span>")))
            .Add(x => x.EndContent, (RenderFragment)(b => b.AddMarkupContent(0, "<span id=\"end\">kg</span>"))));

        // Assert
        Assert.Equal(inputTheme.FieldWrapper, cut.Find("input").ParentElement!.GetAttribute("class"));
        Assert.Contains(inputTheme.FieldStartContent, cut.Find("#start").ParentElement!.GetAttribute("class"));
        Assert.Contains(inputTheme.FieldEndContent, cut.Find("#end").ParentElement!.GetAttribute("class"));
    }

    [Fact]
    public void EndContent_ComesBeforeTheClearButton()
    {
        // Act
        var cut = TestContext.Render<TwTextfield<string>>(p => p
            .Add(x => x.Clearable, true)
            .Add(x => x.Value, "12")
            .Add(x => x.EndContent, (RenderFragment)(b => b.AddMarkupContent(0, "<span id=\"end\">kg</span>"))));

        // Assert
        var row = cut.Find("#end").ParentElement!;
        Assert.Equal(["SPAN", "BUTTON"], row.Children.Select(child => child.TagName).ToArray());
    }

    [Fact]
    public void FieldContent_IsSizedLikeTheInput()
    {
        // Act
        var cut = TestContext.Render<TwTextfield<string>>(p => p.Add(x => x.Clearable, true).Add(x => x.Value, "x").Add(x => x.Dense, true));

        // Assert
        Assert.Contains(inputTheme.DenseSize, cut.Find("button").ParentElement!.GetAttribute("class"));
    }
}
