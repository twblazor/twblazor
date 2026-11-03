using Bunit;
using Microsoft.AspNetCore.Components;
using TwBlazor.Components;
using TwBlazor.Configuration.Components;

namespace TwBlazor.Tests.Components.Table;

public class TwTableResizingTests : TwBlazorTestBase
{
    private TwTableTheme tableTheme => Theme.Components.Require<TwTableTheme>();

    private static RenderFragment Columns(Action<string>? nameWidthChanged = null, bool priceResizable = true, int span = 1) => builder =>
    {
        builder.OpenComponent<TwTableColumn>(0);
        builder.AddComponentParameter(1, nameof(TwTableColumn.Id), "col-name");
        builder.AddComponentParameter(2, nameof(TwTableColumn.Label), "Name");
        builder.AddComponentParameter(3, nameof(TwTableColumn.MinWidth), 80);
        if (nameWidthChanged is not null)
        {
            builder.AddComponentParameter(4, nameof(TwTableColumn.WidthChanged), EventCallback.Factory.Create(new object(), nameWidthChanged));
        }
        builder.CloseComponent();

        builder.OpenComponent<TwTableColumn>(5);
        builder.AddComponentParameter(6, nameof(TwTableColumn.Id), "col-price");
        builder.AddComponentParameter(7, nameof(TwTableColumn.Label), "Price");
        builder.AddComponentParameter(8, nameof(TwTableColumn.Width), "120px");
        builder.AddComponentParameter(9, nameof(TwTableColumn.Resizable), priceResizable);
        builder.AddComponentParameter(10, nameof(TwTableColumn.Span), span);
        builder.CloseComponent();
    };

    private IRenderedComponent<TwTable> Render(bool resizable = true, RenderFragment? columns = null, Action<IReadOnlyDictionary<string, string>>? resized = null) =>
        TestContext.Render<TwTable>(parameters =>
        {
            parameters
                .Add(p => p.Id, "products")
                .Add(p => p.ResizableColumns, resizable)
                .Add(p => p.ColumnGroup, columns ?? Columns());
            if (resized is not null)
            {
                parameters.Add(p => p.ColumnsResized, resized);
            }
        });

    [Fact]
    public void ResizableColumns_IsOff_ByDefault()
    {
        // Act
        var cut = Render(resizable: false);

        // Assert
        Assert.Empty(cut.FindAll("[data-tw-resize-layer]"));
        Assert.Empty(cut.FindAll("col[data-tw-resizable]"));
        Assert.DoesNotContain(TestContext.JSInterop.Invocations, i => i.Identifier == "twTable.attachResize");
    }

    [Fact]
    public void ResizableColumns_MarksTheColumnsForTheResizeScript()
    {
        // Act
        var cut = Render();

        // Assert
        var name = cut.Find("#col-name");
        Assert.Equal("true", name.GetAttribute("data-tw-resizable"));
        Assert.Equal("Name", name.GetAttribute("data-tw-label"));
        Assert.Equal("80", name.GetAttribute("data-tw-min"));
        Assert.Equal("2000", name.GetAttribute("data-tw-max"));
        Assert.Contains(tableTheme.FixedLayout, cut.Find("table").GetAttribute("class"));
    }

    [Fact]
    public void ResizableColumns_AddsALayerForTheSeparators_WithLocalizedNames()
    {
        // Act
        var cut = Render();

        // Assert - outside the table, so the separators' names are not part of the column headings.
        var layer = cut.Find("[data-tw-resize-layer]");
        Assert.Equal("TABLE", layer.PreviousElementSibling!.TagName);
        Assert.Equal("Resize {0}", layer.GetAttribute("data-label-format"));
        Assert.Equal("{0} pixels", layer.GetAttribute("data-value-format"));
        Assert.Equal(tableTheme.ResizeHandle, layer.GetAttribute("data-handle-class"));
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twTable.attachResize");
    }

    [Fact]
    public void AColumnThatIsNotResizable_OrSpansColumns_IsLeftUnmarked()
    {
        // Act
        var notResizable = Render(columns: Columns(priceResizable: false));
        var spanning = Render(columns: Columns(span: 2));

        // Assert
        Assert.Null(notResizable.Find("#col-price").GetAttribute("data-tw-resizable"));
        Assert.Null(spanning.Find("#col-price").GetAttribute("data-tw-resizable"));
    }

    [Fact]
    public async Task CommitColumnWidths_SizesTheColumns_AndRaisesTheEvents()
    {
        // Arrange
        string? nameWidth = null;
        IReadOnlyDictionary<string, string>? resized = null;
        var cut = Render(columns: Columns(w => nameWidth = w), resized: r => resized = r);

        // Act
        await cut.InvokeAsync(() => cut.Instance.CommitColumnWidths(new() { ["col-name"] = 180, ["col-price"] = 96, ["missing"] = 50 }));

        // Assert
        Assert.Equal("width:180px;", cut.Find("#col-name").GetAttribute("style"));
        Assert.Equal("width:96px;", cut.Find("#col-price").GetAttribute("style"));
        Assert.Equal("180px", nameWidth);
        Assert.Equal(new Dictionary<string, string> { ["col-name"] = "180px", ["col-price"] = "96px" }, resized);
    }

    [Fact]
    public async Task CommitColumnWidths_RejectsNull()
    {
        // Arrange
        var cut = Render();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => cut.Instance.CommitColumnWidths(null!));
    }

    [Fact]
    public async Task CommitColumnWidths_DoesNotRaiseTheEvent_WhenNoColumnMatches()
    {
        // Arrange
        var raised = false;
        var cut = Render(resized: _ => raised = true);

        // Act
        await cut.InvokeAsync(() => cut.Instance.CommitColumnWidths(new() { ["missing"] = 50 }));

        // Assert
        Assert.False(raised);
    }

    [Fact]
    public async Task ANewWidthFromTheParent_ReplacesAResizedWidth()
    {
        // Arrange
        var cut = Render();
        await cut.InvokeAsync(() => cut.Instance.CommitColumnWidths(new() { ["col-price"] = 200 }));

        // Act
        cut.Render(p => p.Add(x => x.ColumnGroup, builder =>
        {
            builder.OpenComponent<TwTableColumn>(0);
            builder.AddComponentParameter(1, nameof(TwTableColumn.Id), "col-price");
            builder.AddComponentParameter(2, nameof(TwTableColumn.Width), "150px");
            builder.CloseComponent();
        }));

        // Assert
        Assert.Equal("width:150px;", cut.Find("#col-price").GetAttribute("style"));
    }

    [Fact]
    public void LaterRenders_RelayTheColumnsOut()
    {
        // Arrange
        var cut = Render();

        // Act
        cut.Render(p => p.Add(x => x.Striped, true));

        // Assert
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twTable.layoutResize");
    }

    [Fact]
    public void TurningResizingOff_DetachesTheScript()
    {
        // Arrange
        var cut = Render();

        // Act
        cut.Render(p => p.Add(x => x.ResizableColumns, false));

        // Assert
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twTable.detachResize");
    }

    [Fact]
    public async Task DisposeAsync_DetachesTheScript()
    {
        // Arrange
        var cut = Render();

        // Act
        await cut.Instance.DisposeAsync();

        // Assert
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twTable.detachResize");
    }
}
