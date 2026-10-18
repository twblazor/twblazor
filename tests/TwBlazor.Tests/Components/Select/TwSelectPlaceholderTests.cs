using Bunit;
using TwBlazor.Components;

namespace TwBlazor.Tests.Components.Select;

public class TwSelectPlaceholderTests : TwBlazorTestBase
{
    private static readonly string[] _fruit = ["Apple", "Banana", "Cherry"];

    private IRenderedComponent<TwSelect<string?>> Render(bool native, Action<string?> changed, string? selected = "Banana", bool required = false) =>
        TestContext.Render<TwSelect<string?>>(parameters => parameters
            .Add(p => p.PreferNativePicker, native)
            .Add(p => p.Values, _fruit)
            .Add(p => p.Placeholder, "Choose a fruit")
            .Add(p => p.Required, required)
            .Add(p => p.SelectedValue, selected)
            .Add(p => p.SelectedValueChanged, changed));

    [Fact]
    public void NativeSelect_ChoosingThePlaceholder_ClearsTheSelection()
    {
        // Arrange
        var raised = new List<string?>();
        var cut = Render(native: true, raised.Add);

        // Act
        cut.Find("select").Change("0");

        // Assert
        Assert.Equal([null], raised);
        Assert.Null(cut.Instance.SelectedValue);
        Assert.Empty(cut.FindAll("option[value='1'][selected], option[value='2'][selected], option[value='3'][selected]"));
    }

    [Fact]
    public void NativeSelect_ChoosingAValue_StillSelectsIt()
    {
        // Arrange
        var raised = new List<string?>();
        var cut = Render(native: true, raised.Add, selected: null);

        // Act
        cut.Find("select").Change("3");

        // Assert
        Assert.Equal(["Cherry"], raised);
    }

    [Fact]
    public void NativeSelect_IgnoresThePlaceholder_WhenRequired()
    {
        // Arrange - a required select renders no placeholder, so a stray "0" must not clear it.
        var raised = new List<string?>();
        var cut = Render(native: true, raised.Add, required: true);

        // Act
        cut.Find("select").Change("0");

        // Assert
        Assert.Empty(raised);
        Assert.Equal("Banana", cut.Instance.SelectedValue);
    }

    [Fact]
    public void NativeSelect_IgnoresAnUnknownOption()
    {
        // Arrange
        var raised = new List<string?>();
        var cut = Render(native: true, raised.Add);

        // Act
        cut.Find("select").Change("99");

        // Assert
        Assert.Empty(raised);
    }

    [Fact]
    public async Task Listbox_ChoosingThePlaceholder_ClearsTheSelection()
    {
        // Arrange
        var raised = new List<string?>();
        var cut = Render(native: false, raised.Add);

        // Act
        await cut.InvokeAsync(() => cut.Instance.SelectOptionAsync(0));

        // Assert
        Assert.Equal([null], raised);
        Assert.Null(cut.Instance.SelectedValue);
        Assert.Contains("Choose a fruit", cut.Find("button").TextContent);
    }

    [Fact]
    public async Task Listbox_ChoosingThePlaceholderAgain_DoesNotRaiseAChange()
    {
        // Arrange
        var raised = new List<string?>();
        var cut = Render(native: false, raised.Add, selected: null);

        // Act
        await cut.InvokeAsync(() => cut.Instance.SelectOptionAsync(0));

        // Assert
        Assert.Empty(raised);
    }

    [Fact]
    public async Task Listbox_IgnoresThePlaceholder_WhenRequired()
    {
        // Arrange
        var raised = new List<string?>();
        var cut = Render(native: false, raised.Add, required: true);

        // Act
        await cut.InvokeAsync(() => cut.Instance.SelectOptionAsync(0));

        // Assert
        Assert.Empty(raised);
        Assert.Equal("Banana", cut.Instance.SelectedValue);
    }

    [Fact]
    public void ClearingSelectedValueFromTheParent_ShowsThePlaceholderAgain()
    {
        // Arrange
        var cut = Render(native: false, _ => { });
        Assert.Contains("Banana", cut.Find("button").TextContent);

        // Act
        cut.Render(p => p.Add(x => x.SelectedValue, null));

        // Assert
        Assert.Contains("Choose a fruit", cut.Find("button").TextContent);
        Assert.DoesNotContain("Banana", cut.Find("button").TextContent);
    }

    [Fact]
    public void ASelectedValueThatLeavesTheList_ShowsThePlaceholder()
    {
        // Arrange
        var cut = Render(native: true, _ => { });

        // Act
        cut.Render(p => p.Add(x => x.Values, ["Apple", "Cherry"]));

        // Assert
        Assert.Empty(cut.FindAll("option[value='1'][selected], option[value='2'][selected]"));
    }

    [Fact]
    public void SelectedValue_KeepsThePersonsChoice_WhenTheParentDoesNotPassItBack()
    {
        // Arrange - the parent only listens for changes and never sets SelectedValue.
        var cut = TestContext.Render<TwSelect<string?>>(parameters => parameters
            .Add(p => p.PreferNativePicker, true)
            .Add(p => p.Values, _fruit)
            .Add(p => p.SelectedValueChanged, (string? _) => { }));

        // Act
        cut.Find("select").Change("2");
        cut.Render(p => p.Add(x => x.Placeholder, "Pick one"));

        // Assert
        Assert.Equal("Banana", cut.Instance.SelectedValue);
        Assert.True(cut.Find("option[value='2']").HasAttribute("selected"));
    }
}
