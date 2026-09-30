using Bunit;
using Microsoft.AspNetCore.Components;
using TwBlazor.Components;
using TwBlazor.Configuration.Components;

namespace TwBlazor.Tests.Components;

/// <summary>
/// Every single-line input shares the same height, taken from <see cref="TwInputTheme.Size"/> and
/// <see cref="TwInputTheme.DenseSize"/>, so they line up whichever component renders them.
/// </summary>
public class DenseInputTests : TwBlazorTestBase
{
    private static readonly string[] _values = ["A", "B"];

    private TwInputTheme inputTheme => Theme.Components.Require<TwInputTheme>();

    private static string InputClass(IRenderedComponent<IComponent> cut) => cut.Find("input").GetAttribute("class")!;

    [Fact]
    public void TwTextfield_UsesInputSize_ByDefault()
    {
        var cut = TestContext.Render<TwTextfield<string>>();

        Assert.Contains(inputTheme.Size, InputClass(cut));
        Assert.DoesNotContain(inputTheme.DenseSize, InputClass(cut));
    }

    [Fact]
    public void TwTextfield_UsesDenseSize_WhenDense()
    {
        var cut = TestContext.Render<TwTextfield<string>>(parameters => parameters
            .Add(p => p.Dense, true));

        Assert.Contains(inputTheme.DenseSize, InputClass(cut));
        Assert.DoesNotContain(inputTheme.Size, InputClass(cut));
    }

    [Fact]
    public void TwSelect_Multiple_IgnoresDense_BecauseTheHeightFollowsItsChips()
    {
        var cut = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.Multiple, true)
            .Add(p => p.PreferNativePicker, false)
            .Add(p => p.Dense, true)
            .Add(p => p.Values, _values));

        var trigger = cut.Find("button[aria-haspopup='listbox']").ParentElement!.GetAttribute("class");
        Assert.DoesNotContain(inputTheme.DenseSize, trigger);
        Assert.DoesNotContain(inputTheme.Size, trigger);
    }

    [Fact]
    public void TwDatePicker_PassesDenseToItsTextfieldAndIcon()
    {
        var cut = TestContext.Render<TwDatePicker>(parameters => parameters
            .Add(p => p.Dense, true));

        Assert.Contains(inputTheme.DenseSize, InputClass(cut));
        Assert.Contains(inputTheme.DenseSize, cut.Find("div[aria-label='Open date picker']").GetAttribute("class"));
    }

    [Fact]
    public void TwDateRangePicker_PassesDenseToItsTextfieldAndIcon()
    {
        var cut = TestContext.Render<TwDateRangePicker>(parameters => parameters
            .Add(p => p.Dense, true));

        Assert.Contains(inputTheme.DenseSize, InputClass(cut));
        Assert.Contains(inputTheme.DenseSize, cut.Find("div[aria-label='Open date range picker']").GetAttribute("class"));
    }

    [Fact]
    public void TwDateTimePicker_PassesDenseToItsTextfield()
    {
        var cut = TestContext.Render<TwDateTimePicker>(parameters => parameters
            .Add(p => p.Dense, true));

        Assert.Contains(inputTheme.DenseSize, InputClass(cut));
    }

    [Fact]
    public void TwDateTimeRangePicker_PassesDenseToItsTextfieldAndIcon()
    {
        var cut = TestContext.Render<TwDateTimeRangePicker>(parameters => parameters
            .Add(p => p.Dense, true));

        Assert.Contains(inputTheme.DenseSize, InputClass(cut));
        Assert.Contains(inputTheme.DenseSize, cut.Find("div[aria-label='Open datetime range picker']").GetAttribute("class"));
    }

    [Fact]
    public void TwTimePicker_PassesDenseToItsTextfieldAndIcon()
    {
        var cut = TestContext.Render<TwTimePicker>(parameters => parameters
            .Add(p => p.Dense, true));

        Assert.Contains(inputTheme.DenseSize, InputClass(cut));
        Assert.Contains(inputTheme.DenseSize, cut.Find("div[aria-label='Open time picker']").GetAttribute("class"));
    }

    [Fact]
    public void TwTimeRangePicker_PassesDenseToItsTextfieldAndIcon()
    {
        var cut = TestContext.Render<TwTimeRangePicker>(parameters => parameters
            .Add(p => p.Dense, true));

        Assert.Contains(inputTheme.DenseSize, InputClass(cut));
        Assert.Contains(inputTheme.DenseSize, cut.Find("div[aria-label='Open time range picker']").GetAttribute("class"));
    }

    [Fact]
    public void TwColorPicker_PassesDenseToItsTextfield()
    {
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.Dense, true));

        Assert.Contains(inputTheme.DenseSize, InputClass(cut));
    }
}
