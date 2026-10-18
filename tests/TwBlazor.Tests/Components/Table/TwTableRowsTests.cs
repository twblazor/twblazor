using Bunit;
using Microsoft.AspNetCore.Components;
using TwBlazor.Components;
using TwBlazor.Configuration.Components;

namespace TwBlazor.Tests.Components.Table;

public class TwTableRowsTests : TwBlazorTestBase
{
    public TwTableRowsTests()
    {
        TestContext.JSInterop.Setup<bool>("twDevice.prefersNativePicker").SetResult(true);
    }

    private TwTableTheme tableTheme => Theme.Components.Require<TwTableTheme>();

    private static List<string> Fruit(int count = 12) => [.. Enumerable.Range(1, count).Select(i => $"Fruit {i:D2}")];

    private static RenderFragment<string> row => fruit => builder =>
    {
        builder.OpenElement(0, "tr");
        builder.OpenElement(1, "td");
        builder.AddContent(2, fruit);
        builder.CloseElement();
        builder.CloseElement();
    };

    private static RenderFragment<TwTableRow<string>> indexedRow => row => builder =>
    {
        builder.OpenElement(0, "tr");
        builder.AddAttribute(1, "aria-rowindex", row.AriaRowIndex);
        builder.OpenElement(2, "td");
        builder.AddContent(3, $"{row.Index}:{row.Item}");
        builder.CloseElement();
        builder.CloseElement();
    };

    private static RenderFragment emptyRow => builder =>
    {
        builder.OpenElement(0, "tr");
        builder.OpenElement(1, "td");
        builder.AddContent(2, "Nothing here");
        builder.CloseElement();
        builder.CloseElement();
    };

    private static RenderFragment Rows(
        IEnumerable<string>? items = null,
        TwTableRowsProvider<string>? provider = null,
        bool indexed = false,
        RenderFragment<TwTableRowsPage<string>>? pageTemplate = null,
        RenderFragment? loading = null,
        Action<TwTableRowsResult<string>>? loaded = null,
        Action<TwTableRows<string>>? capture = null) => builder =>
    {
        builder.OpenComponent<TwTableRows<string>>(0);
        if (provider is null)
        {
            builder.AddComponentParameter(1, nameof(TwTableRows<string>.Items), items ?? Fruit());
        }
        else
        {
            builder.AddComponentParameter(2, nameof(TwTableRows<string>.ItemsProvider), provider);
        }
        if (indexed)
        {
            builder.AddComponentParameter(3, nameof(TwTableRows<string>.RowTemplate), indexedRow);
        }
        else
        {
            builder.AddComponentParameter(4, nameof(TwTableRows<string>.ChildContent), row);
        }
        builder.AddComponentParameter(5, nameof(TwTableRows<string>.EmptyContent), emptyRow);
        if (pageTemplate is not null)
        {
            builder.AddComponentParameter(6, nameof(TwTableRows<string>.PageTemplate), pageTemplate);
        }
        if (loading is not null)
        {
            builder.AddComponentParameter(7, nameof(TwTableRows<string>.LoadingContent), loading);
        }
        if (loaded is not null)
        {
            builder.AddComponentParameter(8, nameof(TwTableRows<string>.ItemsLoaded), EventCallback.Factory.Create(new object(), loaded));
        }
        if (capture is not null)
        {
            builder.AddComponentReferenceCapture(9, reference => capture((TwTableRows<string>)reference));
        }
        builder.CloseComponent();
    };

    private IRenderedComponent<TwTable> Render(RenderFragment body, Action<ComponentParameterCollectionBuilder<TwTable>>? configure = null) =>
        TestContext.Render<TwTable>(parameters =>
        {
            parameters.Add(p => p.Id, "fruit").Add(p => p.TableBody, body);
            configure?.Invoke(parameters);
        });

    private static List<string> Cells(IRenderedComponent<TwTable> cut) =>
        [.. cut.FindAll("tbody td").Select(td => td.TextContent.Trim())];

    private static void NextPage(IRenderedComponent<TwTable> cut) =>
        cut.Find("nav[aria-label='Table pagination'] button[aria-label='Next page']").Click();

    [Fact]
    public void Rows_RenderEveryItem_OutsideATable()
    {
        // Act
        var cut = TestContext.Render<TwTableRows<string>>(parameters => parameters
            .Add(p => p.Items, Fruit(3))
            .Add(p => p.ChildContent, row));

        // Assert
        Assert.Equal(["Fruit 01", "Fruit 02", "Fruit 03"], cut.FindAll("td").Select(td => td.TextContent));
    }

    [Fact]
    public void Rows_RenderEveryItem_WithoutPagingOrVirtualization()
    {
        // Act
        var cut = Render(Rows());

        // Assert
        Assert.Equal(12, Cells(cut).Count);
        Assert.Empty(cut.FindAll("nav[aria-label='Table pagination']"));
    }

    [Fact]
    public void Rows_ShowTheEmptyContent_WhenThereAreNone()
    {
        // Act
        var cut = Render(Rows(items: []));

        // Assert
        Assert.Equal(["Nothing here"], Cells(cut));
    }

    [Fact]
    public void EnablePagination_ShowsTheFirstPage_AndAPager()
    {
        // Act
        var cut = Render(Rows(), p => p.Add(x => x.EnablePagination, true).Add(x => x.PageSize, 5));

        // Assert
        Assert.Equal(["Fruit 01", "Fruit 02", "Fruit 03", "Fruit 04", "Fruit 05"], Cells(cut));
        var pager = cut.Find("#fruit-pagination");
        Assert.Equal("Table pagination", pager.QuerySelector("nav")?.GetAttribute("aria-label") ?? pager.GetAttribute("aria-label"));
    }

    [Fact]
    public void EnablePagination_MovesThroughThePages_AndRaisesCurrentPageChanged()
    {
        // Arrange
        int? raised = null;
        var cut = Render(Rows(), p => p
            .Add(x => x.EnablePagination, true)
            .Add(x => x.PageSize, 5)
            .Add(x => x.CurrentPageChanged, (int page) => raised = page));

        // Act
        NextPage(cut);

        // Assert
        Assert.Equal(2, raised);
        Assert.Equal("Fruit 06", Cells(cut)[0]);
    }

    [Fact]
    public void CurrentPage_StartsOnTheGivenPage()
    {
        // Act
        var cut = Render(Rows(), p => p.Add(x => x.EnablePagination, true).Add(x => x.PageSize, 5).Add(x => x.CurrentPage, 3));

        // Assert
        Assert.Equal(["Fruit 11", "Fruit 12"], Cells(cut));
    }

    [Fact]
    public void PaginationLabel_RenamesThePager()
    {
        // Act
        var cut = Render(Rows(), p => p.Add(x => x.EnablePagination, true).Add(x => x.PaginationLabel, "Fruit pages"));

        // Assert
        Assert.NotEmpty(cut.FindAll("nav[aria-label='Fruit pages']"));
    }

    [Fact]
    public void ChangingThePageSize_ReturnsToTheFirstPage()
    {
        // Arrange
        int? pageSize = null;
        int? page = null;
        var cut = Render(Rows(), p => p
            .Add(x => x.EnablePagination, true)
            .Add(x => x.PageSize, 5)
            .Add(x => x.PageSizeChanged, (int size) => pageSize = size)
            .Add(x => x.CurrentPageChanged, (int p) => page = p));
        NextPage(cut);

        // Act - option 2 of 5, 10, 25, 50 and 100 is 10 rows a page.
        cut.Find("select").Change("2");

        // Assert
        Assert.Equal(10, pageSize);
        Assert.Equal(1, page);
        Assert.Equal(10, Cells(cut).Count);
    }

    [Fact]
    public void FewerRows_MoveBackToTheLastPage()
    {
        // Arrange
        int? raised = null;
        var cut = Render(Rows(), p => p
            .Add(x => x.EnablePagination, true)
            .Add(x => x.PageSize, 5)
            .Add(x => x.CurrentPage, 3)
            .Add(x => x.CurrentPageChanged, (int page) => raised = page));

        // Act
        cut.Render(p => p.Add(x => x.TableBody, Rows(items: Fruit(4))));

        // Assert
        Assert.Equal(1, raised);
        Assert.Equal(4, Cells(cut).Count);
    }

    [Fact]
    public void PageTemplate_GetsTheRowsOnThePage()
    {
        // Arrange
        RenderFragment<TwTableRowsPage<string>> template = page => builder =>
        {
            builder.OpenElement(0, "tr");
            builder.OpenElement(1, "td");
            builder.AddContent(2, string.Join(",", page.Rows.Select(r => r.Index)));
            builder.CloseElement();
            builder.CloseElement();
        };

        // Act
        var cut = Render(Rows(pageTemplate: template), p => p.Add(x => x.EnablePagination, true).Add(x => x.PageSize, 3).Add(x => x.CurrentPage, 2));

        // Assert
        Assert.Equal(["3,4,5"], Cells(cut));
    }

    [Fact]
    public void EnableVirtualization_ScrollsTheRows_InPlaceOfThePager()
    {
        // Act
        var cut = Render(Rows(items: Fruit(4)), p => p.Add(x => x.EnableVirtualization, true).Add(x => x.EnablePagination, true));

        // Assert
        Assert.Equal(4, Cells(cut).Count);
        Assert.Empty(cut.FindAll("nav[aria-label='Table pagination']"));
        Assert.Equal("max-height:32rem", cut.Find("table").ParentElement!.GetAttribute("style"));
    }

    [Fact]
    public void EnableVirtualization_WithARowTemplate_NumbersTheRowsForAssistiveTechnology()
    {
        // Act
        var cut = Render(Rows(items: Fruit(3), indexed: true), p => p.Add(x => x.EnableVirtualization, true));

        // Assert
        Assert.Equal("4", cut.Find("table").GetAttribute("aria-rowcount"));
        Assert.Equal(["2", "3", "4"], cut.FindAll("tbody tr[aria-rowindex]").Select(r => r.GetAttribute("aria-rowindex")));
        Assert.Equal(["0:Fruit 01", "1:Fruit 02", "2:Fruit 03"], Cells(cut));
    }

    [Fact]
    public void HeaderRowCount_ShiftsTheRowNumbers()
    {
        // Act
        var cut = Render(Rows(items: Fruit(2), indexed: true), p => p.Add(x => x.EnableVirtualization, true).Add(x => x.HeaderRowCount, 2));

        // Assert
        Assert.Equal("4", cut.Find("table").GetAttribute("aria-rowcount"));
        Assert.Equal("3", cut.Find("tbody tr[aria-rowindex]").GetAttribute("aria-rowindex"));
    }

    [Fact]
    public void EnableVirtualization_WithoutARowTemplate_LeavesTheRowCountOff()
    {
        // Act
        var cut = Render(Rows(items: Fruit(3)), p => p.Add(x => x.EnableVirtualization, true));

        // Assert
        Assert.Null(cut.Find("table").GetAttribute("aria-rowcount"));
    }

    [Fact]
    public void RowTemplate_HasNoRowIndex_WhenNotVirtualized()
    {
        // Act
        var cut = Render(Rows(items: Fruit(2), indexed: true));

        // Assert
        Assert.Null(cut.Find("tbody tr").GetAttribute("aria-rowindex"));
        Assert.Null(cut.Find("table").GetAttribute("aria-rowcount"));
    }

    [Fact]
    public void EnableVirtualization_ShowsTheEmptyContent_WhenThereAreNoRows()
    {
        // Act
        var cut = Render(Rows(items: []), p => p.Add(x => x.EnableVirtualization, true));

        // Assert
        Assert.Equal(["Nothing here"], Cells(cut));
    }

    [Fact]
    public void EnableVirtualization_KeepsTheHeaderInView()
    {
        // Act
        var cut = Render(Rows(), p => p.Add(x => x.EnableVirtualization, true).Add(x => x.TableHeader, RenderFragmentBuilder(string.Empty)));

        // Assert
        Assert.Contains(tableTheme.StickyHeader, cut.Find("thead").GetAttribute("class"));
    }

    [Fact]
    public void MaxHeight_OverridesTheVirtualizedHeight()
    {
        // Act
        var cut = Render(Rows(), p => p.Add(x => x.EnableVirtualization, true).Add(x => x.MaxHeight, "20rem"));

        // Assert
        Assert.Equal("max-height:20rem", cut.Find("table").ParentElement!.GetAttribute("style"));
    }

    [Fact]
    public void CurrentItems_AndTotalCount_DescribeTheView()
    {
        // Arrange
        TwTableRows<string>? rows = null;

        // Act
        Render(Rows(capture: r => rows = r), p => p.Add(x => x.EnablePagination, true).Add(x => x.PageSize, 5));

        // Assert
        Assert.Equal(12, rows!.TotalCount);
        Assert.Equal(["Fruit 01", "Fruit 02", "Fruit 03", "Fruit 04", "Fruit 05"], rows.CurrentItems);
    }

    private sealed class Server
    {
        public List<TwTableRowsRequest> Requests { get; } = [];

        public ValueTask<TwTableRowsResult<string>> Provide(TwTableRowsRequest request)
        {
            Requests.Add(request);
            var all = Fruit();
            var page = all.Skip(request.StartIndex).Take(request.Count ?? all.Count).ToList();
            return ValueTask.FromResult(new TwTableRowsResult<string>(page, all.Count));
        }
    }

    [Fact]
    public void ItemsProvider_LoadsOnePageAtATime()
    {
        // Arrange
        var server = new Server();

        // Act
        var cut = Render(Rows(provider: server.Provide), p => p.Add(x => x.EnablePagination, true).Add(x => x.PageSize, 5));

        // Assert
        var request = Assert.Single(server.Requests);
        Assert.Equal(0, request.StartIndex);
        Assert.Equal(5, request.Count);
        Assert.Equal(5, Cells(cut).Count);
        Assert.NotEmpty(cut.FindAll("#fruit-pagination button[aria-label='Next page']"));
    }

    [Fact]
    public async Task ItemsProvider_AsksForTheNextPage()
    {
        // Arrange
        var server = new Server();
        var cut = Render(Rows(provider: server.Provide), p => p.Add(x => x.EnablePagination, true).Add(x => x.PageSize, 5));

        // Act
        NextPage(cut);
        await cut.WaitForStateAsync(() => server.Requests.Count == 2);

        // Assert
        Assert.Equal(5, server.Requests[^1].StartIndex);
        Assert.Equal("Fruit 06", Cells(cut)[0]);
    }

    [Fact]
    public void ItemsProvider_IsAskedForEveryRow_WithoutPaging()
    {
        // Arrange
        var server = new Server();

        // Act
        var cut = Render(Rows(provider: server.Provide));

        // Assert
        Assert.Null(Assert.Single(server.Requests).Count);
        Assert.Equal(12, Cells(cut).Count);
    }

    [Fact]
    public async Task ItemsProvider_ShowsTheLoadingContent_AndMarksTheTableBusy()
    {
        // Arrange
        var pending = new TaskCompletionSource<TwTableRowsResult<string>>();
        RenderFragment loading = builder => builder.AddMarkupContent(0, "<tr><td>Loading fruit</td></tr>");

        // Act
        var cut = Render(Rows(provider: _ => new ValueTask<TwTableRowsResult<string>>(pending.Task), loading: loading));

        // Assert
        Assert.Equal("true", cut.Find("table").GetAttribute("aria-busy"));
        Assert.Equal(["Loading fruit"], Cells(cut));

        // Act
        await cut.InvokeAsync(() => pending.SetResult(new(["Apple"], 1)));

        // Assert
        await cut.WaitForAssertionAsync(() => Assert.Equal(["Apple"], Cells(cut)));
        Assert.Null(cut.Find("table").GetAttribute("aria-busy"));
    }

    [Fact]
    public void ItemsLoaded_IsRaised_WithTheRowsAndTotal()
    {
        // Arrange
        var server = new Server();
        TwTableRowsResult<string>? loaded = null;

        // Act
        Render(Rows(provider: server.Provide, loaded: r => loaded = r), p => p.Add(x => x.EnablePagination, true).Add(x => x.PageSize, 5));

        // Assert
        Assert.Equal(12, loaded?.TotalCount);
        Assert.Equal(5, loaded?.Items.Count);
    }

    [Fact]
    public async Task ItemsProvider_CancelsALoadThatANewerOneReplaced()
    {
        // Arrange
        var tokens = new List<CancellationToken>();
        var first = new TaskCompletionSource<TwTableRowsResult<string>>();
        var server = new Server();
        TwTableRows<string>? rows = null;
        var cut = Render(Rows(provider: request =>
        {
            tokens.Add(request.CancellationToken);
            return tokens.Count == 1 ? new ValueTask<TwTableRowsResult<string>>(first.Task) : server.Provide(request);
        }, capture: r => rows = r));

        // Act
        await rows!.RefreshDataAsync();
        await cut.InvokeAsync(() => first.SetResult(new(["Stale"], 1)));

        // Assert
        Assert.True(tokens[0].IsCancellationRequested);
        Assert.DoesNotContain("Stale", Cells(cut));
        Assert.Equal(12, Cells(cut).Count);
    }

    [Fact]
    public async Task RefreshDataAsync_ShowsChangedItems_WithoutAProvider()
    {
        // Arrange
        var fruit = Fruit(2);
        TwTableRows<string>? rows = null;
        var cut = Render(Rows(items: fruit, capture: r => rows = r));

        // Act
        fruit.Add("Fruit 03");
        await rows!.RefreshDataAsync();

        // Assert
        Assert.Equal(3, Cells(cut).Count);
    }

    [Fact]
    public void ItemsProvider_WithVirtualization_LoadsTheRowsInView()
    {
        // Arrange
        var server = new Server();

        // Act
        var cut = Render(Rows(provider: server.Provide, indexed: true), p => p.Add(x => x.EnableVirtualization, true));

        // Assert
        Assert.NotEmpty(server.Requests);
        Assert.Equal("13", cut.Find("table").GetAttribute("aria-rowcount"));
        Assert.Equal("2", cut.Find("tbody tr[aria-rowindex]").GetAttribute("aria-rowindex"));
    }

    [Fact]
    public async Task RefreshDataAsync_WithVirtualization_AsksAgain()
    {
        // Arrange
        var server = new Server();
        TwTableRows<string>? rows = null;
        var cut = Render(Rows(provider: server.Provide, indexed: true, capture: r => rows = r), p => p.Add(x => x.EnableVirtualization, true));
        var before = server.Requests.Count;

        // Act
        await rows!.RefreshDataAsync();

        // Assert
        await cut.WaitForStateAsync(() => server.Requests.Count > before);
        Assert.True(server.Requests.Count > before);
    }

    [Fact]
    public async Task DisposeAsync_CancelsALoadStillRunning()
    {
        // Arrange
        CancellationToken token = default;
        var pending = new TaskCompletionSource<TwTableRowsResult<string>>();
        TwTableRows<string>? rows = null;
        Render(Rows(provider: request =>
        {
            token = request.CancellationToken;
            return new ValueTask<TwTableRowsResult<string>>(pending.Task);
        }, capture: r => rows = r));

        // Act
        await rows!.DisposeAsync();

        // Assert
        Assert.True(token.IsCancellationRequested);
    }
}
