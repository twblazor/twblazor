using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using TwBlazor.Components;
using TwBlazor.Localization;

namespace TwBlazor.Tests.Components.Select;

public class TwSelectClearableAndTemplateTests : TwBlazorTestBase
{
    private static readonly string[] _countries = ["Canada", "France", "Germany"];

    private sealed record Person(string Name, string Role);

    private static readonly Person[] _people = [new("Ada Lovelace", "Engineer"), new("Grace Hopper", "Architect")];

    private string clearLabel => Localizer[TwStrings.Textfield_Clear];

    private string clearSelector => $"button[aria-label='{clearLabel}']";

    private IRenderedComponent<TwSelect<string>> Render(bool native, Action<ComponentParameterCollectionBuilder<TwSelect<string>>>? configure = null, bool clearable = true) =>
        TestContext.Render<TwSelect<string>>(p =>
        {
            p.Add(x => x.Id, "country");
            p.Add(x => x.PreferNativePicker, native);
            p.Add(x => x.Values, _countries);
            p.Add(x => x.Clearable, clearable);
            configure?.Invoke(p);
        });

    private static RenderFragment<Person> personTemplate => person => builder =>
        builder.AddMarkupContent(0, $"<b>{person.Name}</b> <em>{person.Role}</em>");

    #region Clearable

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task Clearable_ShowsAClearButton_OutOfTheTabOrder_InEveryMode(bool native, bool multiple)
    {
        var cut = Render(native, p => p.Add(x => x.Multiple, multiple));

        var clear = cut.Find(clearSelector);
        Assert.Equal("-1", clear.GetAttribute("tabindex"));
        Assert.Equal("country", clear.GetAttribute("aria-controls"));
        Assert.True(clear.HasAttribute("disabled"));
        await Task.CompletedTask;
    }

    [Fact]
    public async Task Clearable_IsOff_ByDefault()
    {
        var cut = Render(false, p => p.Add(x => x.SelectedValue, "France"), clearable: false);

        Assert.Empty(cut.FindAll(clearSelector));
        await Task.CompletedTask;
    }

    [Theory]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(false, false, true)]
    public async Task Clearable_HidesTheButton_WhenDisabledReadOnlyOrARequiredSingleSelect(bool disabled, bool readOnly, bool required)
    {
        var cut = Render(false, p => p
            .Add(x => x.SelectedValue, "France")
            .Add(x => x.Disabled, disabled)
            .Add(x => x.ReadOnly, readOnly)
            .Add(x => x.Required, required));

        Assert.Empty(cut.FindAll(clearSelector));
        await Task.CompletedTask;
    }

    [Fact]
    public async Task Clearable_StaysAvailable_OnARequiredMultipleSelect()
    {
        var cut = Render(false, p => p
            .Add(x => x.Multiple, true)
            .Add(x => x.Required, true)
            .Add(x => x.SelectedValues, ["France"]));

        Assert.False(cut.Find(clearSelector).HasAttribute("disabled"));
        await Task.CompletedTask;
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Clear_ResetsASingleSelection_AndMovesFocusToTheField(bool native)
    {
        var received = new List<string>();
        var cleared = 0;
        var cut = Render(native, p => p
            .Add(x => x.SelectedValue, "France")
            .Add(x => x.SelectedValueChanged, EventCallback.Factory.Create<string>(this, received.Add))
            .Add(x => x.OnClear, EventCallback.Factory.Create(this, () => cleared++)));

        await cut.Find(clearSelector).ClickAsync(new());

        Assert.Equal([null!], received);
        Assert.Equal(1, cleared);
        Assert.True(cut.Find(clearSelector).HasAttribute("disabled"));
        var focus = TestContext.JSInterop.Invocations.Last(i => i.Identifier == "twFocus.focusById");
        Assert.Equal("country", focus.Arguments[0]);
    }

    [Fact]
    public async Task Clear_SelectsThePlaceholder_OfTheNativeSelect()
    {
        var cut = Render(true, p => p.Add(x => x.SelectedValue, "France"));
        Assert.Equal("2", cut.Find("select").GetAttribute("value"));

        await cut.Find(clearSelector).ClickAsync(new());

        Assert.Equal("0", cut.Find("select").GetAttribute("value"));
        Assert.True(cut.Find("option[value='0']").HasAttribute("selected"));
    }

    [Fact]
    public async Task Clear_ShowsThePlaceholder_OnTheCustomTrigger()
    {
        var cut = Render(false, p => p
            .Add(x => x.Placeholder, "Pick one")
            .Add(x => x.SelectedValue, "France"));

        await cut.Find(clearSelector).ClickAsync(new());

        Assert.Equal("Pick one", cut.Find("[role='combobox']").TextContent.Trim());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Clear_EmptiesAMultipleSelection(bool native)
    {
        IEnumerable<string>? received = null;
        var cut = Render(native, p => p
            .Add(x => x.Multiple, true)
            .Add(x => x.SelectedValues, ["Canada", "Germany"])
            .Add(x => x.SelectedValuesChanged, EventCallback.Factory.Create<IEnumerable<string>>(this, v => received = v)));

        await cut.Find(clearSelector).ClickAsync(new());

        Assert.NotNull(received);
        Assert.Empty(received);
        Assert.Empty(cut.FindComponents<TwChip>());
    }

    [Fact]
    public async Task Clear_DeselectsTheNativeMultipleSelect()
    {
        var cut = Render(true, p => p
            .Add(x => x.Multiple, true)
            .Add(x => x.SelectedValues, ["Canada"]));

        await cut.Find(clearSelector).ClickAsync(new());

        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twSelect.deselectAll");
    }

    [Fact]
    public async Task Clear_DoesNotOpenTheList_OfAMultipleSelect()
    {
        var cut = Render(false, p => p
            .Add(x => x.Multiple, true)
            .Add(x => x.SelectedValues, ["Canada"]));

        await cut.Find(clearSelector).ClickAsync(new());

        Assert.Empty(cut.FindAll("[data-tw-popover]"));
    }

    [Theory]
    [InlineData("Delete")]
    [InlineData("Backspace")]
    public async Task DeleteOrBackspace_OnTheTrigger_ClearsTheSelection(string key)
    {
        var received = new List<string>();
        var cut = Render(false, p => p
            .Add(x => x.SelectedValue, "France")
            .Add(x => x.SelectedValueChanged, EventCallback.Factory.Create<string>(this, received.Add)));

        await cut.Find("[role='combobox']").KeyDownAsync(new KeyboardEventArgs { Key = key });

        Assert.Equal([null!], received);
    }

    [Fact]
    public async Task DeleteOnTheTrigger_DoesNothing_WithoutClearable()
    {
        var calls = 0;
        var cut = Render(false, p => p
            .Add(x => x.SelectedValue, "France")
            .Add(x => x.SelectedValueChanged, EventCallback.Factory.Create<string>(this, _ => calls++)), clearable: false);

        await cut.Find("[role='combobox']").KeyDownAsync(new KeyboardEventArgs { Key = "Delete" });

        Assert.Equal(0, calls);
    }

    [Fact]
    public async Task ClearAsync_DoesNothing_WhenThereIsNoSelection()
    {
        var calls = 0;
        var cut = Render(false, p => p.Add(x => x.OnClear, EventCallback.Factory.Create(this, () => calls++)));

        await cut.InvokeAsync(() => cut.Instance.ClearAsync());

        Assert.Equal(0, calls);
    }

    [Fact]
    public async Task Clear_DoesNotThrow_WhenTheCircuitHasDisconnected()
    {
        TestContext.JSInterop.SetupVoid("twFocus.focusById", _ => true).SetException(new JSDisconnectedException("circuit gone"));
        var cut = Render(false, p => p.Add(x => x.SelectedValue, "France"));

        var exception = await Record.ExceptionAsync(() => cut.InvokeAsync(() => cut.Instance.ClearAsync()));

        Assert.Null(exception);
    }

    [Theory]
    [InlineData(false, false, "[role='combobox'] span")]
    [InlineData(true, false, "select")]
    [InlineData(false, true, "[role='combobox']")]
    public async Task Clearable_KeepsTheTextClearOfTheClearButton(bool native, bool multiple, string selector)
    {
        var cut = Render(native, p => p.Add(x => x.Multiple, multiple));

        var element = multiple ? cut.Find(selector).ParentElement! : cut.Find(selector);
        Assert.StartsWith("padding-inline-end:", element.GetAttribute("style"));
        await Task.CompletedTask;
    }

    #endregion

    #region ItemTemplate

    [Fact]
    public async Task ItemTemplate_RendersEachOption_InTheCustomListbox()
    {
        var cut = TestContext.Render<TwSelect<Person>>(p => p
            .Add(x => x.PreferNativePicker, false)
            .Add(x => x.Values, _people)
            .Add(x => x.PropertyName, nameof(Person.Name))
            .Add(x => x.ItemTemplate, personTemplate));

        await cut.Find("[role='combobox']").ClickAsync(new());

        var options = cut.FindAll("[role='option'] b");
        Assert.Equal(["Ada Lovelace", "Grace Hopper"], options.Select(o => o.TextContent));
        Assert.Equal("Engineer", cut.Find("[role='option'] em").TextContent);
    }

    [Fact]
    public async Task ItemTemplate_RendersEachOption_InTheMultipleListbox()
    {
        var cut = TestContext.Render<TwSelect<Person>>(p => p
            .Add(x => x.PreferNativePicker, false)
            .Add(x => x.Multiple, true)
            .Add(x => x.Values, _people)
            .Add(x => x.PropertyName, nameof(Person.Name))
            .Add(x => x.ItemTemplate, personTemplate));

        await cut.Find("[role='combobox']").ClickAsync(new());

        Assert.Equal(2, cut.FindAll("[role='option'] em").Count);
    }

    [Fact]
    public async Task ItemTemplate_KeepsTheDisplayText_OnTheClosedTrigger()
    {
        var cut = TestContext.Render<TwSelect<Person>>(p => p
            .Add(x => x.PreferNativePicker, false)
            .Add(x => x.Values, _people)
            .Add(x => x.PropertyName, nameof(Person.Name))
            .Add(x => x.SelectedValue, _people[1])
            .Add(x => x.ItemTemplate, personTemplate));

        var trigger = cut.Find("[role='combobox']");
        Assert.Equal("Grace Hopper", trigger.TextContent.Trim());
        Assert.Empty(trigger.QuerySelectorAll("em"));
        await Task.CompletedTask;
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ItemTemplate_FallsBackToTheDisplayText_InTheNativePicker(bool multiple)
    {
        var cut = TestContext.Render<TwSelect<Person>>(p => p
            .Add(x => x.PreferNativePicker, true)
            .Add(x => x.Multiple, multiple)
            .Add(x => x.Values, _people)
            .Add(x => x.PropertyName, nameof(Person.Name))
            .Add(x => x.ItemTemplate, personTemplate));

        var texts = cut.FindAll("option").Select(o => o.TextContent.Trim()).Where(t => t.Length > 0 && !t.StartsWith("Select", StringComparison.Ordinal)).ToList();
        Assert.Equal(["Ada Lovelace", "Grace Hopper"], texts);
        Assert.Empty(cut.FindAll("option em"));
        await Task.CompletedTask;
    }

    #endregion
}
