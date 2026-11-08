using Bunit;
using TwBlazor.Components;
using TwBlazor.Enums;
using TwBlazor.Localization;
using TwBlazor.Models;

namespace TwBlazor.Tests.Components.Filter;

public class TwColumnFilterEditorTests : TwBlazorTestBase
{
    private IRenderedComponent<TwColumnFilterEditor> Render(ColumnFilter filter, Action<ComponentParameterCollectionBuilder<TwColumnFilterEditor>>? configure = null) =>
        TestContext.Render<TwColumnFilterEditor>(p =>
        {
            p.Add(x => x.Id, "f");
            p.Add(x => x.Filter, filter);
            configure?.Invoke(p);
        });

    private static List<string> Options(IRenderedComponent<TwColumnFilterEditor> cut, string id)
    {
        cut.Find($"#{id}").Click();
        return [.. cut.FindAll("[role='option']").Select(o => o.TextContent.Trim())];
    }

    [Fact]
    public void Renders_TwoConditions_AndTheLogicButtons()
    {
        var filter = new ColumnFilter { First = new() { Operator = FilterOperator.Contains } };
        var cut = Render(filter);

        Assert.NotNull(filter.Second);
        Assert.NotEmpty(cut.FindAll("#f-first-operator"));
        Assert.NotEmpty(cut.FindAll("#f-first-value"));
        Assert.NotEmpty(cut.FindAll("#f-second-operator"));
        var group = cut.Find("[role='group']");
        Assert.Equal(Localizer[TwStrings.DataTable_FilterLogic], group.GetAttribute("aria-label"));
        Assert.Equal("true", cut.Find("#f-and").GetAttribute("aria-pressed"));
        Assert.Equal("false", cut.Find("#f-or").GetAttribute("aria-pressed"));
    }

    [Fact]
    public void Editing_ChangesTheFilter_AndReportsIt()
    {
        var changes = 0;
        var filter = new ColumnFilter();
        var cut = Render(filter, p => p.Add(x => x.OnChange, () => changes++));

        cut.Find("#f-first-value").Input("tea");
        cut.Find("#f-or").Click();
        Options(cut, "f-second-operator");
        cut.FindAll("[role='option']").Single(o => o.TextContent.Trim() == Localizer["DataTable_OperatorIsEmpty"]).Click();

        Assert.Equal("tea", filter.First.Value);
        Assert.Equal(FilterLogic.Or, filter.Logic);
        Assert.Equal(FilterOperator.IsEmpty, filter.Second!.Operator);
        Assert.Empty(cut.FindAll("#f-second-value"));
        Assert.Equal(3, changes);
    }

    [Fact]
    public void Type_PicksTheComparisonsAndTheField()
    {
        var number = Render(new ColumnFilter { First = new() { Operator = FilterOperator.IsGreaterThan } }, p => p.Add(x => x.Type, FilterType.Number));
        Assert.Contains(Localizer["DataTable_OperatorIsGreaterThan"], Options(number, "f-first-operator"));
        Assert.NotEmpty(number.FindAll("#f-first-value"));

        var date = Render(new ColumnFilter { First = new() { Operator = FilterOperator.IsEqualTo } }, p => p.Add(x => x.Type, FilterType.Date));
        Assert.Equal("date", date.Find("#f-first-value").GetAttribute("type"));

        var yesNo = Render(new ColumnFilter { First = new() { Operator = FilterOperator.IsEqualTo } }, p => p.Add(x => x.Type, FilterType.Boolean));
        Assert.Equal([Localizer[TwStrings.DataTable_Yes], Localizer[TwStrings.DataTable_No]], Options(yesNo, "f-first-value").Where(o => o.Length > 0 && o != Localizer[TwStrings.Select_Placeholder]));
    }

    [Fact]
    public void Operators_AndChoices_CanBeChosen()
    {
        var filter = new ColumnFilter { First = new() { Operator = FilterOperator.IsEqualTo } };
        var cut = Render(filter, p => p
            .Add(x => x.Type, FilterType.Enum)
            .Add(x => x.Operators, [FilterOperator.IsEqualTo])
            .Add(x => x.Choices, [new KeyValuePair<string, string>("a", "Apple"), new KeyValuePair<string, string>("b", "Banana")]));

        Assert.Equal([Localizer["DataTable_OperatorIsEqualTo"]], Options(cut, "f-first-operator"));
        var choices = Options(cut, "f-first-value");
        Assert.Contains("Apple", choices);
        cut.FindAll("[role='option']").Single(o => o.TextContent.Trim() == "Banana").Click();
        Assert.Equal("b", filter.First.Value);
    }

    [Fact]
    public void FieldIdPrefix_NamesTheFields()
    {
        var cut = Render(new ColumnFilter(), p => p.Add(x => x.FieldIdPrefix, "panel"));

        Assert.NotEmpty(cut.FindAll("#panel-first-operator"));
        Assert.NotEmpty(cut.FindAll("#panel-and"));
        Assert.Equal("f", cut.Find("div").GetAttribute("id"));
    }

    [Theory]
    [InlineData("Apple pie", FilterOperator.Contains, "pie", FilterType.Text, true)]
    [InlineData("Apple pie", FilterOperator.DoesNotContain, "pie", FilterType.Text, false)]
    [InlineData("Apple pie", FilterOperator.StartsWith, "app", FilterType.Text, true)]
    [InlineData("Apple pie", FilterOperator.EndsWith, "PIE", FilterType.Text, true)]
    [InlineData(12.5, FilterOperator.IsGreaterThan, "10", FilterType.Number, true)]
    [InlineData(12.5, FilterOperator.IsLessThanOrEqualTo, "10", FilterType.Number, false)]
    [InlineData(null, FilterOperator.IsNotEqualTo, "10", FilterType.Number, true)]
    [InlineData(true, FilterOperator.IsEqualTo, "true", FilterType.Boolean, true)]
    [InlineData("x", FilterOperator.IsEqualTo, "not a number", FilterType.Number, true)]
    [InlineData("", FilterOperator.IsEmpty, null, FilterType.Text, true)]
    [InlineData("a", FilterOperator.IsNotEmpty, null, FilterType.Auto, true)]
    public void Matches_FollowsTheConditions(object? value, FilterOperator op, string? target, FilterType type, bool expected)
    {
        var filter = new ColumnFilter { First = new() { Operator = op, Value = target } };

        Assert.Equal(expected, filter.Matches(value, type));
    }

    [Fact]
    public void Matches_CombinesTwoConditions_AndAnEmptyFilterPassesEverything()
    {
        var both = new ColumnFilter
        {
            First = new() { Operator = FilterOperator.IsGreaterThan, Value = "10" },
            Second = new() { Operator = FilterOperator.IsLessThan, Value = "20" },
        };

        Assert.True(both.Matches(15, FilterType.Number));
        Assert.False(both.Matches(25, FilterType.Number));

        both.Logic = FilterLogic.Or;
        Assert.True(both.Matches(25, FilterType.Number));

        Assert.True(new ColumnFilter().Matches("anything"));
        Assert.True(new ColumnFilter { Second = new() { Operator = FilterOperator.Contains, Value = "a" } }.Matches("cat"));
        Assert.True(new ColumnFilter { First = new() { Operator = FilterOperator.IsEqualTo, Value = "2026-01-02" } }.Matches(new DateOnly(2026, 1, 2), FilterType.Date));
    }
}
