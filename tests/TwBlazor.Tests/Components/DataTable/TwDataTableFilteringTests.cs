using Bunit;
using TwBlazor.Components;

namespace TwBlazor.Tests.Components.DataTable;

public class TwDataTableFilteringTests : TwBlazorTestBase
{
    public TwDataTableFilteringTests()
    {
        // The selects inside the filter render as native <select> elements, which the tests can drive.
        TestContext.JSInterop.Setup<bool>("twDevice.prefersNativePicker").SetResult(true);
    }

    public enum Room
    {
        Kitchen,
        Lounge,
        Office
    }

    public record Product(string Name, Room Room, decimal Price, DateTime Added, bool InStock, string? Notes);

    private static List<Product> Products() =>
    [
        new("Kettle", Room.Kitchen, 30m, new DateTime(2026, 1, 10), true, "Steel"),
        new("Armchair", Room.Lounge, 250m, new DateTime(2026, 3, 5), false, null),
        new("Blender", Room.Kitchen, 60m, new DateTime(2026, 2, 20), true, ""),
        new("Desk", Room.Office, 180m, new DateTime(2026, 3, 5, 14, 30, 0), false, "Oak"),
        new("Lamp", Room.Office, 45m, new DateTime(2026, 4, 1), true, "Brass")
    ];

    private static List<TwDataTableColumn<Product>> Columns() =>
    [
        new() { Name = "name", Title = "Name", PropertySelector = p => p.Name, Filterable = true },
        new() { Name = "room", Title = "Room", PropertySelector = p => p.Room, Filterable = true },
        new() { Name = "price", Title = "Price", PropertySelector = p => p.Price, Filterable = true },
        new() { Name = "added", Title = "Added", PropertySelector = p => p.Added, Filterable = true },
        new() { Name = "inStock", Title = "In stock", PropertySelector = p => p.InStock, Filterable = true },
        new() { Name = "notes", Title = "Notes", PropertySelector = p => p.Notes },
    ];

    private IRenderedComponent<TwDataTable<Product>> Render(
        Action<ComponentParameterCollectionBuilder<TwDataTable<Product>>>? configure = null,
        List<TwDataTableColumn<Product>>? columns = null) =>
        TestContext.Render<TwDataTable<Product>>(parameters =>
        {
            parameters
                .Add(p => p.Id, "products")
                .Add(p => p.Items, Products())
                .Add(p => p.Columns, columns ?? Columns())
                .Add(p => p.EnablePagination, false);
            configure?.Invoke(parameters);
        });

    private static List<string> Names(IRenderedComponent<TwDataTable<Product>> cut) =>
        [.. cut.FindAll("tbody tr").Select(r => r.Children[0].TextContent.Trim())];

    private static string Status(IRenderedComponent<TwDataTable<Product>> cut) =>
        cut.Find("[role='status']").TextContent.Trim();

    private static void OpenFilter(IRenderedComponent<TwDataTable<Product>> cut, string column) =>
        cut.Find($"#products-col-{column}-filter").Click();

    private static List<string> Options(IRenderedComponent<TwDataTable<Product>> cut, string id) =>
        [.. cut.FindAll($"#{id} option").Select(o => o.TextContent.Trim())];

    // Picks an option of a native select by its text: option values are the option's place, starting at 1.
    private static void Choose(IRenderedComponent<TwDataTable<Product>> cut, string id, string text)
    {
        var option = cut.FindAll($"#{id} option").Single(o => o.TextContent.Trim() == text);
        cut.Find($"#{id}").Change(option.GetAttribute("value"));
    }

    private static void Apply(IRenderedComponent<TwDataTable<Product>> cut) => cut.Find("#products-filter form").Submit();

    [Fact]
    public void OnlyFilterableColumns_HaveAFilterButton()
    {
        // Act
        var cut = Render();

        // Assert
        var button = cut.Find("#products-col-name-filter");
        Assert.Equal("Filter Name", button.GetAttribute("aria-label"));
        Assert.Equal("dialog", button.GetAttribute("aria-haspopup"));
        Assert.Equal("false", button.GetAttribute("aria-expanded"));
        Assert.Empty(cut.FindAll("#products-col-notes-filter"));
    }

    [Fact]
    public void FilterButton_OpensANamedDialog_OutsideTheTable()
    {
        // Arrange
        var cut = Render();

        // Act
        OpenFilter(cut, "name");

        // Assert
        var dialog = cut.Find("#products-filter");
        Assert.Equal("dialog", dialog.GetAttribute("role"));
        Assert.Equal("true", dialog.GetAttribute("aria-modal"));
        Assert.Equal("Filter Name", cut.Find($"#{dialog.GetAttribute("aria-labelledby")}").TextContent.Trim());
        Assert.Null(dialog.Closest("table"));
        Assert.Equal("true", cut.Find("#products-col-name-filter").GetAttribute("aria-expanded"));
        var open = Assert.Single(TestContext.JSInterop.Invocations, i => i.Identifier == "twDataTable.openFilter");
        Assert.Equal("products-col-name-filter", open.Arguments[1]);
    }

    [Theory]
    [InlineData("name", "Contains", 8)]
    [InlineData("price", "Is equal to", 8)]
    [InlineData("added", "Is equal to", 8)]
    [InlineData("inStock", "Is equal to", 2)]
    [InlineData("room", "Is equal to", 2)]
    public void Conditions_SuitTheColumnsValues(string column, string first, int count)
    {
        // Arrange
        var cut = Render();

        // Act
        OpenFilter(cut, column);

        // Assert
        var options = Options(cut, "products-filter-first-operator");
        Assert.Equal(first, options[0]);
        Assert.Equal(count, options.Count);
    }

    [Fact]
    public void ATextFilter_KeepsTheMatchingRows_AndAnnouncesThem()
    {
        // Arrange
        var cut = Render();
        OpenFilter(cut, "name");

        // Act
        cut.Find("#products-filter-first-value").Input("l");
        Apply(cut);

        // Assert
        Assert.Equal(["Kettle", "Blender", "Lamp"], Names(cut));
        Assert.Equal("Filter applied to Name, showing 3 rows", Status(cut));
        Assert.Equal("Filter Name, filtered", cut.Find("#products-col-name-filter").GetAttribute("aria-label"));
        Assert.Empty(cut.FindAll("#products-filter"));
        var focus = TestContext.JSInterop.Invocations.Last(i => i.Identifier == "twDataTable.focus");
        Assert.Equal("products-col-name-filter", focus.Arguments[0]);
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twDataTable.closeFilter");
    }

    [Fact]
    public void TwoConditions_CombineWithAndOrOr()
    {
        // Arrange
        var cut = Render();
        OpenFilter(cut, "price");
        Choose(cut, "products-filter-first-operator", "Is greater than");
        cut.Find("#products-filter-first-value").Input("40");
        Choose(cut, "products-filter-second-operator", "Is less than");
        cut.Find("#products-filter-second-value").Input("200");

        // Act
        Apply(cut);

        // Assert - over 40 and under 200.
        Assert.Equal(["Blender", "Desk", "Lamp"], Names(cut));

        // Act - switch to Or: over 40, or under 200, is every row.
        OpenFilter(cut, "price");
        cut.Find("#products-filter-or").Click();
        Assert.Equal("true", cut.Find("#products-filter-or").GetAttribute("aria-pressed"));
        Assert.Equal("false", cut.Find("#products-filter-and").GetAttribute("aria-pressed"));
        Apply(cut);

        // Assert
        Assert.Equal(5, Names(cut).Count);
    }

    [Fact]
    public void ADateFilter_ComparesByDay()
    {
        // Arrange
        var cut = Render();
        OpenFilter(cut, "added");

        // Act
        cut.Find("#products-filter-first-value").Input("2026-03-05");
        Apply(cut);

        // Assert - the desk was added at 14:30 on the same day.
        Assert.Equal(["Armchair", "Desk"], Names(cut));
    }

    [Fact]
    public void ATrueOrFalseFilter_OffersYesAndNo()
    {
        // Arrange
        var cut = Render();
        OpenFilter(cut, "inStock");

        // Act
        Choose(cut, "products-filter-first-value", "No");
        Apply(cut);

        // Assert
        Assert.Equal(["Armchair", "Desk"], Names(cut));
    }

    [Fact]
    public void AnEnumFilter_OffersItsValues()
    {
        // Arrange
        var cut = Render();
        OpenFilter(cut, "room");
        Assert.Contains("Office", Options(cut, "products-filter-first-value"));

        // Act
        Choose(cut, "products-filter-first-operator", "Is not equal to");
        Choose(cut, "products-filter-first-value", "Kitchen");
        Apply(cut);

        // Assert
        Assert.Equal(["Armchair", "Desk", "Lamp"], Names(cut));
    }

    [Fact]
    public void IsEmpty_NeedsNoValue()
    {
        // Arrange
        var columns = Columns();
        columns[5].Filterable = true;
        var cut = Render(columns: columns);
        OpenFilter(cut, "notes");

        // Act
        Choose(cut, "products-filter-first-operator", "Is empty");

        // Assert
        Assert.Empty(cut.FindAll("#products-filter-first-value"));

        // Act
        Apply(cut);

        // Assert
        Assert.Equal(["Armchair", "Blender"], Names(cut));
    }

    [Fact]
    public void ClearingAFilter_ShowsEveryRowAgain()
    {
        // Arrange
        var cut = Render();
        OpenFilter(cut, "name");
        cut.Find("#products-filter-first-value").Input("Kettle");
        Apply(cut);
        OpenFilter(cut, "name");
        Assert.Equal("Kettle", cut.Find("#products-filter-first-value").GetAttribute("value"));

        // Act
        cut.Find("#products-filter-clear").Click();

        // Assert
        Assert.Equal(5, Names(cut).Count);
        Assert.Equal("Filter removed from Name", Status(cut));
        Assert.Equal("Filter Name", cut.Find("#products-col-name-filter").GetAttribute("aria-label"));
    }

    [Fact]
    public async Task ClosingTheFilter_LeavesItAsItWas()
    {
        // Arrange
        var cut = Render();
        OpenFilter(cut, "name");
        cut.Find("#products-filter-first-value").Input("Kettle");

        // Act - Escape and a click outside both call this.
        await cut.InvokeAsync(() => cut.Instance.CloseFilterAsync());

        // Assert
        Assert.Equal(5, Names(cut).Count);
        Assert.Empty(cut.FindAll("#products-filter"));
        Assert.Equal("products-col-name-filter", TestContext.JSInterop.Invocations.Last(i => i.Identifier == "twDataTable.focus").Arguments[0]);
    }

    [Fact]
    public void ClickingTheFilterButtonAgain_ClosesIt()
    {
        // Arrange
        var cut = Render();
        OpenFilter(cut, "name");

        // Act
        OpenFilter(cut, "name");

        // Assert
        Assert.Empty(cut.FindAll("#products-filter"));
    }

    [Fact]
    public void OpeningAnotherColumnsFilter_ReplacesTheFirst()
    {
        // Arrange
        var cut = Render();
        OpenFilter(cut, "name");

        // Act
        OpenFilter(cut, "price");

        // Assert
        Assert.Equal("Filter Price", cut.Find("#products-filter-heading").TextContent.Trim());
        Assert.Equal("false", cut.Find("#products-col-name-filter").GetAttribute("aria-expanded"));
    }

    [Fact]
    public void AValueThatIsNotANumber_FiltersNothing()
    {
        // Arrange - the price filter's number field rejects text, so a filter saved with bad text is used instead.
        var cut = Render(p => p.Add(x => x.State, new TwDataTableState
        {
            Columns = [new() { Name = "price", Filter = new() { First = new() { Operator = TwDataTableFilterOperator.IsGreaterThan, Value = "lots" } } }]
        }));

        // Assert
        Assert.Equal(5, Names(cut).Count);
    }

    [Fact]
    public void FilterOperators_ChoosesTheConditionsOnOffer()
    {
        // Arrange
        var columns = Columns();
        columns[0].FilterOperators = [TwDataTableFilterOperator.StartsWith, TwDataTableFilterOperator.EndsWith];
        var cut = Render(columns: columns);

        // Act
        OpenFilter(cut, "name");

        // Assert
        Assert.Equal(["Starts with", "Ends with"], Options(cut, "products-filter-first-operator"));
    }

    [Fact]
    public void FilterType_OverridesTheValuesType()
    {
        // Arrange
        var columns = Columns();
        columns[2].FilterType = TwDataTableFilterType.Text;
        var cut = Render(columns: columns);

        // Act
        OpenFilter(cut, "price");

        // Assert
        Assert.Equal("Contains", Options(cut, "products-filter-first-operator")[0]);
    }

    [Fact]
    public void Filters_AreSavedInTheState_AndRestoredFromIt()
    {
        // Arrange
        TwDataTableState? raised = null;
        var cut = Render(p => p.Add(x => x.StateChanged, (TwDataTableState s) => raised = s));
        OpenFilter(cut, "name");
        cut.Find("#products-filter-first-value").Input("e");
        Apply(cut);

        // Assert
        var filter = raised?.Columns.Single(c => c.Name == "name").Filter;
        Assert.Equal(TwDataTableFilterOperator.Contains, filter?.First.Operator);
        Assert.Equal("e", filter?.First.Value);
        Assert.Null(filter?.Second);

        // Act
        var restored = Render(p => p.Add(x => x.State, raised));

        // Assert
        Assert.Equal(Names(cut), Names(restored));
    }

    [Fact]
    public void AFilter_KeepsWorking_WhileItsColumnIsHidden()
    {
        // Act
        var cut = Render(p => p.Add(x => x.State, new TwDataTableState
        {
            Columns = [new() { Name = "room", Visible = false, Filter = new() { First = new() { Operator = TwDataTableFilterOperator.IsEqualTo, Value = "Office" } } }]
        }));

        // Assert
        Assert.Equal(["Desk", "Lamp"], Names(cut));
    }

    [Fact]
    public void ASavedFilter_IsDropped_ForAColumnThatCannotBeFiltered()
    {
        // Act
        var cut = Render(p => p.Add(x => x.State, new TwDataTableState
        {
            Columns = [new() { Name = "notes", Filter = new() { First = new() { Operator = TwDataTableFilterOperator.IsEmpty } } }]
        }));

        // Assert
        Assert.Equal(5, Names(cut).Count);
        Assert.Null(cut.Instance.GetState().Columns.Single(c => c.Name == "notes").Filter);
    }

    [Fact]
    public async Task ResetState_RemovesTheFilters()
    {
        // Arrange
        var cut = Render(p => p.Add(x => x.State, new TwDataTableState
        {
            Columns = [new() { Name = "room", Filter = new() { First = new() { Operator = TwDataTableFilterOperator.IsEqualTo, Value = "Office" } } }]
        }));

        // Act
        await cut.InvokeAsync(() => cut.Instance.ResetStateAsync());

        // Assert
        Assert.Equal(5, Names(cut).Count);
    }

    [Fact]
    public void NoMatchingRows_ShowsTheNoResultsMessage()
    {
        // Arrange
        var cut = Render();
        OpenFilter(cut, "name");

        // Act
        cut.Find("#products-filter-first-value").Input("zzz");
        Apply(cut);

        // Assert
        Assert.Equal("No results match your search", cut.Find("tbody td").TextContent.Trim());
        Assert.Equal("Filter applied to Name, showing 0 rows", Status(cut));
    }

    [Fact]
    public void AnItemsProvider_IsSentTheFilters()
    {
        // Arrange
        var requests = new List<TwDataTableItemsRequest>();
        var cut = TestContext.Render<TwDataTable<Product>>(parameters => parameters
            .Add(p => p.Id, "products")
            .Add(p => p.Columns, Columns())
            .Add(p => p.ItemsProvider, request =>
            {
                requests.Add(request);
                return ValueTask.FromResult(new TwDataTableItemsResult<Product>(Products(), 5));
            }));
        OpenFilter(cut, "name");

        // Act
        cut.Find("#products-filter-first-value").Input("Lamp");
        Apply(cut);

        // Assert
        var filter = requests[^1].Filters["name"];
        Assert.Equal("Lamp", filter.First.Value);
    }

    [Fact]
    public void Filters_AreCopies_InTheProviderRequest()
    {
        // Arrange
        var filter = new TwDataTableColumnFilter
        {
            First = new() { Operator = TwDataTableFilterOperator.IsGreaterThan, Value = "10" },
            Logic = TwDataTableFilterLogic.Or,
            Second = new() { Operator = TwDataTableFilterOperator.IsEmpty }
        };

        // Act
        var copy = filter.Clone();
        copy.First.Value = "20";

        // Assert
        Assert.Equal("10", filter.First.Value);
        Assert.Equal(TwDataTableFilterLogic.Or, copy.Logic);
        Assert.True(copy.Second!.IsActive);
        Assert.False(new TwDataTableFilterCondition { Operator = TwDataTableFilterOperator.Contains }.IsActive);
    }
}
