using AngleSharp.Dom;
using Bunit;
using Microsoft.AspNetCore.Components;
using TwBlazor.Components;
using TwBlazor.Configuration.Components;

namespace TwBlazor.Tests.Components.DataTable;

public class TwDataTableEditingTests : TwBlazorTestBase
{
    public TwDataTableEditingTests()
    {
        TestContext.JSInterop.Setup<bool>("twDevice.prefersNativePicker").SetResult(true);
    }

    private TwTableTheme tableTheme => Theme.Components.Require<TwTableTheme>();

    private static List<Product> Products() =>
    [
        new() { Name = "Kettle", Price = 30m, InStock = true, Category = ProductCategory.Kitchen, Added = new DateTime(2026, 1, 5), Stock = 4 },
        new() { Name = "Armchair", Price = 250m, InStock = false, Category = ProductCategory.Furniture, Added = new DateTime(2026, 2, 9), Stock = 0 }
    ];

    private static List<TwDataTableColumn<Product>> Columns() =>
    [
        new()
        {
            Name = "name", Title = "Name", PropertySelector = p => p.Name, IsSortable = true, Editable = true,
            ValueSetter = (p, v) => p.Name = (string?)v ?? string.Empty,
            Validator = (_, v) => string.IsNullOrWhiteSpace(v as string) ? "Enter a name" : null
        },
        new()
        {
            Name = "price", Title = "Price", PropertySelector = p => p.Price, Editable = true,
            ValueSetter = (p, v) => p.Price = (decimal)v!,
            Validator = (_, v) => v is decimal price && price < 0 ? "Enter a price of 0 or more" : null
        },
        new() { Name = "stock", Title = "Stock", PropertySelector = p => p.Stock, Editable = true, ValueSetter = (p, v) => p.Stock = (int)v! },
        new() { Name = "inStock", Title = "In stock", PropertySelector = p => p.InStock, Editable = true, ValueSetter = (p, v) => p.InStock = (bool)v! },
        new() { Name = "category", Title = "Category", PropertySelector = p => p.Category, Editable = true, ValueSetter = (p, v) => p.Category = (ProductCategory)v! },
        new() { Name = "added", Title = "Added", PropertySelector = p => p.Added, Editable = true, ValueSetter = (p, v) => p.Added = (DateTime)v! },
        new() { Name = "code", Title = "Code", PropertySelector = p => p.Name.ToUpperInvariant() }
    ];

    private IRenderedComponent<TwDataTable<Product>> Render(
        Action<ComponentParameterCollectionBuilder<TwDataTable<Product>>>? configure = null,
        List<Product>? items = null,
        List<TwDataTableColumn<Product>>? columns = null,
        bool editable = true) =>
        TestContext.Render<TwDataTable<Product>>(parameters =>
        {
            parameters
                .Add(p => p.Id, "products")
                .Add(p => p.Items, items ?? Products())
                .Add(p => p.Columns, columns ?? Columns())
                .Add(p => p.Editable, editable);
            configure?.Invoke(parameters);
        });

    private static string Status(IRenderedComponent<TwDataTable<Product>> cut) =>
        cut.Find("[role='status']").TextContent.Trim();

    private static string LastFocus(BunitContext context) =>
        (string)context.JSInterop.Invocations.Last(i => i.Identifier == "twDataTable.focus").Arguments[0]!;

    private static IElement EditingRow(IRenderedComponent<TwDataTable<Product>> cut) =>
        cut.Find("#products-save-row").Closest("tr")!;

    [Fact]
    public void Editable_AddsAnActionsColumn_WithAnEditButtonPerRow()
    {
        // Act
        var cut = Render();

        // Assert
        Assert.Equal("Actions", cut.FindAll("thead th")[^1].TextContent.Trim());
        Assert.Equal("Edit Kettle", cut.Find("#products-edit-row-0").GetAttribute("aria-label"));
        Assert.Equal("Edit Armchair", cut.Find("#products-edit-row-1").GetAttribute("aria-label"));
    }

    [Fact]
    public void Editable_IsOff_ByDefault()
    {
        // Act
        var cut = Render(editable: false);

        // Assert
        Assert.Empty(cut.FindAll("[id^='products-edit-row']"));
        Assert.Equal(7, cut.FindAll("thead th").Count);
    }

    [Fact]
    public void ActionsColumn_IsCountedInTheColumnGroup_AndTheEmptyRow()
    {
        // Arrange
        var columns = Columns();
        columns[0].Width = "10rem";

        // Act
        var cut = Render(columns: columns, items: []);

        // Assert
        Assert.Equal(8, cut.FindAll("colgroup col").Count);
        Assert.Equal("8", cut.Find("tbody td").GetAttribute("colspan"));
    }

    [Fact]
    public void CanEditRow_HidesTheEditButtonForRowsItRejects()
    {
        // Act
        var cut = Render(p => p.Add(x => x.CanEditRow, (Func<Product, bool>)(p => p.InStock)));

        // Assert
        Assert.NotEmpty(cut.FindAll("#products-edit-row-0"));
        Assert.Empty(cut.FindAll("#products-edit-row-1"));
    }

    [Fact]
    public void RowLabel_NamesTheRowInTheButtons()
    {
        // Act
        var cut = Render(p => p.Add(x => x.RowLabel, (Func<Product, string>)(p => $"the {p.Name.ToLowerInvariant()}")));

        // Assert
        Assert.Equal("Edit the kettle", cut.Find("#products-edit-row-0").GetAttribute("aria-label"));
    }

    [Fact]
    public void Edit_TurnsTheEditableCellsIntoEditors_ThatMatchTheValueTypes()
    {
        // Arrange
        var cut = Render();

        // Act
        cut.Find("#products-edit-row-0").Click();

        // Assert
        var row = EditingRow(cut);
        Assert.Contains(tableTheme.EditingRow, row.GetAttribute("class"));
        Assert.Equal("Kettle", row.QuerySelector("#products-edit-name")!.GetAttribute("value"));
        Assert.Equal("Name", row.QuerySelector("#products-edit-name")!.GetAttribute("aria-label"));
        Assert.Equal("decimal", row.QuerySelector("#products-edit-price")!.GetAttribute("inputmode"));
        Assert.Equal("numeric", row.QuerySelector("#products-edit-stock")!.GetAttribute("inputmode"));
        Assert.Equal("checkbox", row.QuerySelector("#products-edit-inStock")!.GetAttribute("type"));
        Assert.Equal("SELECT", row.QuerySelector("#products-edit-category")!.TagName);
        Assert.NotNull(row.QuerySelector("#products-edit-added"));
        Assert.Contains("KETTLE", row.Children[6].TextContent);
        Assert.Equal("Editing Kettle", Status(cut));
        Assert.Equal("products-edit-name", LastFocus(TestContext));
    }

    [Fact]
    public void WhileEditing_TheOtherEditButtonsAreDisabled()
    {
        // Arrange
        var cut = Render();

        // Act
        cut.Find("#products-edit-row-0").Click();

        // Assert
        Assert.True(cut.Find("#products-edit-row-1").HasAttribute("disabled"));
    }

    [Fact]
    public void Edit_RegistersTheRowForEscape()
    {
        // Arrange
        var cut = Render();

        // Act
        cut.Find("#products-edit-row-0").Click();

        // Assert
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twDialog.registerEscape");
    }

    [Fact]
    public void Save_WritesTheValuesToTheRow_AndRaisesRowSaved()
    {
        // Arrange
        var products = Products();
        TwDataTableRowEdit<Product>? saved = null;
        var cut = Render(p => p.Add(x => x.RowSaved, (TwDataTableRowEdit<Product> e) => saved = e), items: products);
        cut.Find("#products-edit-row-0").Click();

        // Act
        cut.Find("#products-edit-name").Change("Steel kettle");
        cut.Find("#products-edit-inStock").Change(false);
        cut.Find("#products-edit-category").Change("2");
        cut.Find("#products-save-row").Click();

        // Assert
        Assert.Equal("Steel kettle", products[0].Name);
        Assert.False(products[0].InStock);
        Assert.Equal(ProductCategory.Furniture, products[0].Category);
        Assert.NotNull(saved);
        Assert.Same(products[0], saved.Item);
        Assert.Equal("Kettle", saved.OriginalValues["name"]);
        Assert.Equal(["name", "inStock", "category"], saved.ChangedColumns);
        Assert.Empty(cut.FindAll("#products-save-row"));
        Assert.Equal("Steel kettle saved", Status(cut));
    }

    [Fact]
    public void Save_ReturnsFocusToTheRowsEditButton_AfterTheRowMoves()
    {
        // Arrange - sorted by name, renaming Armchair to Zebra chair moves it to the end.
        var cut = Render();
        cut.Find("[aria-label='Sort by Name']").Click();
        cut.Find("#products-edit-row-0").Click();

        // Act
        cut.Find("#products-edit-name").Change("Zebra chair");
        cut.Find("#products-save-row").Click();

        // Assert
        Assert.Equal("products-edit-row-1", LastFocus(TestContext));
        Assert.Equal("Edit Zebra chair", cut.Find("#products-edit-row-1").GetAttribute("aria-label"));
    }

    [Fact]
    public void Save_IsBlockedByAValidator_WhichMarksTheFieldAndFocusesIt()
    {
        // Arrange
        var products = Products();
        var cut = Render(items: products);
        cut.Find("#products-edit-row-0").Click();

        // Act
        cut.Find("#products-edit-name").Change(" ");
        cut.Find("#products-save-row").Click();

        // Assert
        Assert.Equal("Kettle", products[0].Name);
        var input = cut.Find("#products-edit-name");
        Assert.Equal("true", input.GetAttribute("aria-invalid"));
        var errorId = input.GetAttribute("aria-describedby");
        Assert.Equal("Enter a name", cut.Find($"#{errorId}").TextContent.Trim());
        Assert.Equal("1 field needs fixing before saving", Status(cut));
        Assert.Equal("products-edit-name", LastFocus(TestContext));
    }

    [Fact]
    public void ChangingAnInvalidValue_ClearsItsError()
    {
        // Arrange
        var cut = Render();
        cut.Find("#products-edit-row-0").Click();
        cut.Find("#products-edit-name").Change(" ");
        cut.Find("#products-save-row").Click();

        // Act
        cut.Find("#products-edit-name").Change("Kettle");

        // Assert
        Assert.Null(cut.Find("#products-edit-name").GetAttribute("aria-invalid"));
    }

    [Fact]
    public void Cancel_LeavesTheRowAsItWas_AndRaisesRowEditCancelled()
    {
        // Arrange
        var products = Products();
        Product? cancelled = null;
        var cut = Render(p => p.Add(x => x.RowEditCancelled, (Product p) => cancelled = p), items: products);
        cut.Find("#products-edit-row-0").Click();
        cut.Find("#products-edit-name").Change("Changed");

        // Act
        cut.Find("#products-cancel-row").Click();

        // Assert
        Assert.Equal("Kettle", products[0].Name);
        Assert.Same(products[0], cancelled);
        Assert.Equal("Editing Kettle cancelled", Status(cut));
        Assert.Equal("products-edit-row-0", LastFocus(TestContext));
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twDialog.unregisterEscape");
    }

    [Fact]
    public async Task Escape_CancelsTheEdit()
    {
        // Arrange
        var cut = Render();
        cut.Find("#products-edit-row-0").Click();

        // Act - the Escape stack calls this when the row is the topmost entry.
        await cut.Instance.CancelEditFromEscapeAsync();

        // Assert
        Assert.Empty(cut.FindAll("#products-save-row"));
        Assert.Equal("Editing Kettle cancelled", Status(cut));
    }

    [Fact]
    public async Task Escape_DoesNothing_WhenNoRowIsEdited()
    {
        // Arrange
        var cut = Render();

        // Act
        await cut.Instance.CancelEditFromEscapeAsync();

        // Assert
        Assert.Equal(string.Empty, Status(cut));
    }

    [Fact]
    public void EditTemplate_ReplacesTheEditor()
    {
        // Arrange
        var products = Products();
        var columns = Columns();
        columns[0].EditTemplate = context => builder =>
        {
            builder.OpenElement(0, "button");
            builder.AddAttribute(1, "id", context.Id);
            builder.AddAttribute(2, "aria-label", context.Label);
            builder.AddAttribute(3, "onclick", EventCallback.Factory.Create(this, () => context.SetValue($"{context.Value} (edited)")));
            builder.AddContent(4, context.Invalid ? context.ErrorMessage : "Rename");
            builder.CloseElement();
        };
        var cut = Render(columns: columns, items: products);
        cut.Find("#products-edit-row-0").Click();

        // Act
        cut.Find("#products-edit-name").Click();
        cut.Find("#products-save-row").Click();

        // Assert
        Assert.Equal("Kettle (edited)", products[0].Name);
    }

    [Fact]
    public void AColumnWithoutAValueSetter_PassesItsValueToRowSaved()
    {
        // Arrange
        var products = Products();
        var columns = Columns();
        columns[0].ValueSetter = null;
        TwDataTableRowEdit<Product>? saved = null;
        var cut = Render(p => p.Add(x => x.RowSaved, (TwDataTableRowEdit<Product> e) => saved = e), columns: columns, items: products);
        cut.Find("#products-edit-row-0").Click();

        // Act
        cut.Find("#products-edit-name").Change("Renamed");
        cut.Find("#products-save-row").Click();

        // Assert
        Assert.Equal("Kettle", products[0].Name);
        Assert.Equal("Renamed", saved?.Values["name"]);
    }

    [Fact]
    public void ARowWithoutEditableColumns_FocusesTheSaveButton()
    {
        // Arrange
        var columns = Columns();
        columns.ForEach(c => c.Editable = false);
        var cut = Render(columns: columns);

        // Act
        cut.Find("#products-edit-row-0").Click();

        // Assert
        Assert.Equal("products-save-row", LastFocus(TestContext));
    }

    [Fact]
    public void AnEmptyValue_TakesItsEditorFromAnotherRow()
    {
        // Arrange - the first row has no price, so the editor type comes from the second.
        List<NullableProduct> items = [new("Mystery", null), new("Lamp", 12.5m)];
        List<TwDataTableColumn<NullableProduct>> columns =
        [
            new() { Name = "name", Title = "Name", PropertySelector = p => p.Name },
            new() { Name = "price", Title = "Price", PropertySelector = p => p.Price, Editable = true }
        ];
        var cut = TestContext.Render<TwDataTable<NullableProduct>>(parameters => parameters
            .Add(p => p.Id, "products")
            .Add(p => p.Items, items)
            .Add(p => p.Columns, columns)
            .Add(p => p.Editable, true));

        // Act
        cut.Find("#products-edit-row-0").Click();

        // Assert - a number field rather than a text field.
        Assert.Equal("decimal", cut.Find("#products-edit-price").GetAttribute("inputmode"));
    }

    [Fact]
    public void EditingAVirtualizedRow_KeepsItsRowIndex()
    {
        // Arrange
        var cut = Render(p => p.Add(x => x.EnableVirtualization, true));

        // Act
        cut.Find("#products-edit-row-1").Click();

        // Assert
        Assert.Equal("3", EditingRow(cut).GetAttribute("aria-rowindex"));
    }

    [Fact]
    public void ChangedColumns_OnlyListsValuesThatChanged()
    {
        // Arrange
        TwDataTableRowEdit<Product>? saved = null;
        var cut = Render(p => p.Add(x => x.RowSaved, (TwDataTableRowEdit<Product> e) => saved = e));
        cut.Find("#products-edit-row-0").Click();

        // Act
        cut.Find("#products-save-row").Click();

        // Assert
        Assert.Empty(saved!.ChangedColumns);
    }

    public enum ProductCategory
    {
        Kitchen,
        Furniture
    }

    public class Product
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public bool InStock { get; set; }
        public ProductCategory Category { get; set; }
        public DateTime Added { get; set; }
    }

    public record NullableProduct(string Name, decimal? Price);
}
