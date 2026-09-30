using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using TwBlazor.Components;

namespace TwBlazor.Tests.Components.PickList;

public class TwPickListTests : TwBlazorTestBase
{
    [Fact]
    public void TwPickList_Renders_WithDefaultValues()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwPickList<string>>();

        // Assert
        var listBoxes = cut.FindAll("ul[role='listbox']");
        Assert.Equal(2, listBoxes.Count);
    }

    [Fact]
    public void TwPickList_Renders_SourceAndTargetItems()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwPickList<string>>(parameters => parameters
            .Add(p => p.SourceItems, ["Apple", "Banana"])
            .Add(p => p.TargetItems, ["Cherry"]));

        // Assert
        var items = cut.FindAll("li[role='option']");
        Assert.Equal(3, items.Count);
        Assert.Contains(items, i => i.TextContent.Trim() == "Apple");
        Assert.Contains(items, i => i.TextContent.Trim() == "Banana");
        Assert.Contains(items, i => i.TextContent.Trim() == "Cherry");
    }

    [Fact]
    public void TwPickList_Renders_WithTextField_ForComplexObjects()
    {
        // Arrange
        List<TestPerson> source = [new() { Id = 1, Name = "Alice" }];

        // Act
        var cut = TestContext.Render<TwPickList<TestPerson>>(parameters => parameters
            .Add(p => p.SourceItems, source)
            .Add(p => p.TextField, nameof(TestPerson.Name)));

        // Assert
        var item = cut.Find("li[role='option']");
        Assert.Equal("Alice", item.TextContent.Trim());
    }

    [Fact]
    public void TwPickList_Renders_EmptyState_WhenListEmpty()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwPickList<string>>(parameters => parameters
            .Add(p => p.EmptyText, "Nothing here"));

        // Assert - the empty-state row is rendered as a disabled, non-selectable option (rather than
        // role="presentation") so the surrounding role="listbox" always has a valid "option" child;
        // see the remarks on aria-required-children in TwPickList.razor.
        var placeholders = cut.FindAll("li[role='option'][aria-disabled='true']");
        Assert.Equal(2, placeholders.Count);
        Assert.All(placeholders, p => Assert.Equal("Nothing here", p.TextContent.Trim()));
    }

    [Fact]
    public void TwPickList_TogglesSelection_OnItemClick()
    {
        // Arrange
        var cut = TestContext.Render<TwPickList<string>>(parameters => parameters
            .Add(p => p.SourceItems, ["Apple"]));

        // Act
        cut.Find("li[role='option']").Click();

        // Assert
        var item = cut.Find("li[role='option']");
        Assert.Equal("true", item.GetAttribute("aria-selected"));
    }

    [Fact]
    public void TwPickList_TogglesSelection_OnEnterKey()
    {
        // Arrange
        var cut = TestContext.Render<TwPickList<string>>(parameters => parameters
            .Add(p => p.SourceItems, ["Apple"]));

        // Act
        cut.Find("li[role='option']").KeyDown(new KeyboardEventArgs { Key = "Enter" });

        // Assert
        var item = cut.Find("li[role='option']");
        Assert.Equal("true", item.GetAttribute("aria-selected"));
    }

    [Fact]
    public void TwPickList_DoesNotToggleSelection_OnOtherKey()
    {
        // Arrange
        var cut = TestContext.Render<TwPickList<string>>(parameters => parameters
            .Add(p => p.SourceItems, ["Apple"]));

        // Act
        cut.Find("li[role='option']").KeyDown(new KeyboardEventArgs { Key = "A" });

        // Assert
        var item = cut.Find("li[role='option']");
        Assert.Equal("false", item.GetAttribute("aria-selected"));
    }

    [Fact]
    public void TwPickList_RaisesSelectedSourceItemsChanged_OnToggle()
    {
        // Arrange
        IEnumerable<string>? changedSelection = null;
        var cut = TestContext.Render<TwPickList<string>>(parameters => parameters
            .Add(p => p.SourceItems, ["Apple"])
            .Add(p => p.SelectedSourceItemsChanged, EventCallback.Factory.Create<IEnumerable<string>>(
                this, values => changedSelection = values)));

        // Act
        cut.Find("li[role='option']").Click();

        // Assert
        Assert.NotNull(changedSelection);
        Assert.Contains("Apple", changedSelection);
    }

    [Fact]
    public void TwPickList_TransfersSelectedItem_ToTarget_OnRightChevronClick()
    {
        // Arrange
        List<string>? newSource = null;
        List<string>? newTarget = null;
        var cut = TestContext.Render<TwPickList<string>>(parameters => parameters
            .Add(p => p.SourceItems, ["Apple", "Banana"])
            .Add(p => p.SourceLabel, "Source")
            .Add(p => p.TargetLabel, "Target")
            .Add(p => p.SourceItemsChanged, EventCallback.Factory.Create<IEnumerable<string>>(
                this, values => newSource = values.ToList()))
            .Add(p => p.TargetItemsChanged, EventCallback.Factory.Create<IEnumerable<string>>(
                this, values => newTarget = values.ToList())));

        // Act
        cut.Find("li[role='option']").Click(); // select "Apple"
        cut.Find("button[aria-label='Move selected items to Target']").Click();

        // Assert
        Assert.NotNull(newSource);
        Assert.NotNull(newTarget);
        Assert.DoesNotContain("Apple", newSource);
        Assert.Contains("Apple", newTarget);
        Assert.Contains("Banana", newSource);
    }

    [Fact]
    public void TwPickList_TransfersSelectedItem_ToSource_OnLeftChevronClick()
    {
        // Arrange
        List<string>? newSource = null;
        List<string>? newTarget = null;
        var cut = TestContext.Render<TwPickList<string>>(parameters => parameters
            .Add(p => p.TargetItems, ["Cherry"])
            .Add(p => p.SourceLabel, "Source")
            .Add(p => p.TargetLabel, "Target")
            .Add(p => p.SourceItemsChanged, EventCallback.Factory.Create<IEnumerable<string>>(
                this, values => newSource = values.ToList()))
            .Add(p => p.TargetItemsChanged, EventCallback.Factory.Create<IEnumerable<string>>(
                this, values => newTarget = values.ToList())));

        // Act - the source list is empty, so scope the query to the target list to avoid matching
        // its disabled placeholder option instead of "Cherry".
        cut.Find("ul[aria-label='Target'] li[role='option']").Click();
        cut.Find("button[aria-label='Move selected items to Source']").Click();

        // Assert
        Assert.NotNull(newSource);
        Assert.NotNull(newTarget);
        Assert.Contains("Cherry", newSource);
        Assert.DoesNotContain("Cherry", newTarget);
    }

    [Fact]
    public void TwPickList_TransfersAllItems_ToTarget_OnDoubleChevronClick()
    {
        // Arrange
        List<string>? newSource = null;
        List<string>? newTarget = null;
        var cut = TestContext.Render<TwPickList<string>>(parameters => parameters
            .Add(p => p.SourceItems, ["Apple", "Banana"])
            .Add(p => p.SourceLabel, "Source")
            .Add(p => p.TargetLabel, "Target")
            .Add(p => p.SourceItemsChanged, EventCallback.Factory.Create<IEnumerable<string>>(
                this, values => newSource = values.ToList()))
            .Add(p => p.TargetItemsChanged, EventCallback.Factory.Create<IEnumerable<string>>(
                this, values => newTarget = values.ToList())));

        // Act - no selection required for "move all"
        cut.Find("button[aria-label='Move all items to Target']").Click();

        // Assert
        Assert.NotNull(newSource);
        Assert.NotNull(newTarget);
        Assert.Empty(newSource);
        Assert.Equal(["Apple", "Banana"], newTarget);
    }

    [Fact]
    public void TwPickList_HidesMoveAllButtons_WhenShowMoveAllButtonsFalse()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwPickList<string>>(parameters => parameters
            .Add(p => p.SourceLabel, "Source")
            .Add(p => p.TargetLabel, "Target")
            .Add(p => p.ShowMoveAllButtons, false));

        // Assert
        Assert.Empty(cut.FindAll("button[aria-label='Move all items to Target']"));
        Assert.Empty(cut.FindAll("button[aria-label='Move all items to Source']"));
        Assert.NotEmpty(cut.FindAll("button[aria-label='Move selected items to Target']"));
    }

    [Fact]
    public void TwPickList_HidesReorderButtons_WhenAllowReorderFalse()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwPickList<string>>(parameters => parameters
            .Add(p => p.SourceLabel, "Source")
            .Add(p => p.AllowReorder, false));

        // Assert
        Assert.Empty(cut.FindAll("button[aria-label='Move selected Source item up']"));
        Assert.Empty(cut.FindAll("button[aria-label='Move selected Source item down']"));
    }

    [Fact]
    public void TwPickList_MovesSelectedItemUp_WithinSourceList()
    {
        // Arrange
        List<string>? newSource = null;
        var cut = TestContext.Render<TwPickList<string>>(parameters => parameters
            .Add(p => p.SourceItems, ["Apple", "Banana", "Cherry"])
            .Add(p => p.SourceLabel, "Source")
            .Add(p => p.SourceItemsChanged, EventCallback.Factory.Create<IEnumerable<string>>(
                this, values => newSource = values.ToList())));

        // Act - select "Banana" (second item) and move it up
        cut.FindAll("li[role='option']")[1].Click();
        cut.Find("button[aria-label='Move selected Source item up']").Click();

        // Assert
        Assert.NotNull(newSource);
        Assert.Equal(["Banana", "Apple", "Cherry"], newSource);
    }

    [Fact]
    public void TwPickList_MovesSelectedItemDown_WithinTargetList()
    {
        // Arrange
        List<string>? newTarget = null;
        var cut = TestContext.Render<TwPickList<string>>(parameters => parameters
            .Add(p => p.TargetItems, ["Apple", "Banana", "Cherry"])
            .Add(p => p.TargetLabel, "Target")
            .Add(p => p.TargetItemsChanged, EventCallback.Factory.Create<IEnumerable<string>>(
                this, values => newTarget = values.ToList())));

        // Act - select "Apple" (first item); the source list is empty, so scope the query to the
        // target list to avoid matching its disabled placeholder option.
        cut.FindAll("ul[aria-label='Target'] li[role='option']")[0].Click();
        cut.Find("button[aria-label='Move selected Target item down']").Click();

        // Assert
        Assert.NotNull(newTarget);
        Assert.Equal(["Banana", "Apple", "Cherry"], newTarget);
    }

    [Theory]
    [InlineData("up", new[] { "Banana", "Cherry", "Apple", "Date" }, new[] { 1, 2 })]
    [InlineData("up", new[] { "Apple", "Banana", "Cherry", "Date" }, new[] { 0, 1 })]
    [InlineData("down", new[] { "Apple", "Date", "Banana", "Cherry" }, new[] { 1, 2 })]
    [InlineData("down", new[] { "Apple", "Banana", "Cherry", "Date" }, new[] { 2, 3 })]
    public void TwPickList_MovesMultiSelectionTogether_AndStopsAtTheEdge(string direction, string[] expected, int[] selectedIndexes)
    {
        // Arrange
        List<string>? newSource = null;
        var cut = TestContext.Render<TwPickList<string>>(parameters => parameters
            .Add(p => p.SourceItems, ["Apple", "Banana", "Cherry", "Date"])
            .Add(p => p.SourceLabel, "Source")
            .Add(p => p.SourceItemsChanged, EventCallback.Factory.Create<IEnumerable<string>>(
                this, values => newSource = values.ToList())));

        // Act
        foreach (var index in selectedIndexes)
        {
            cut.FindAll("ul[aria-label='Source'] li[role='option']")[index].Click();
        }

        cut.Find($"button[aria-label='Move selected Source item {direction}']").Click();

        // Assert - a contiguous block keeps its order and moves as a unit; at the edge nothing shifts
        Assert.NotNull(newSource);
        Assert.Equal(expected, newSource);
    }

    [Fact]
    public void TwPickList_DisablesTransferButtons_WhenNoSelection()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwPickList<string>>(parameters => parameters
            .Add(p => p.SourceItems, ["Apple"])
            .Add(p => p.SourceLabel, "Source")
            .Add(p => p.TargetLabel, "Target"));

        // Assert
        var transferButton = cut.Find("button[aria-label='Move selected items to Target']");
        Assert.NotNull(transferButton.GetAttribute("disabled"));
    }

    [Fact]
    public void TwPickList_DoesNotToggleSelection_WhenDisabled()
    {
        // Arrange
        var cut = TestContext.Render<TwPickList<string>>(parameters => parameters
            .Add(p => p.SourceItems, ["Apple"])
            .Add(p => p.Disabled, true));

        // Act
        cut.Find("li[role='option']").Click();

        // Assert
        var item = cut.Find("li[role='option']");
        Assert.Equal("false", item.GetAttribute("aria-selected"));
    }

    [Fact]
    public void TwPickList_SeedsInitialSelection_FromSelectedSourceItems()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwPickList<string>>(parameters => parameters
            .Add(p => p.SourceItems, ["Apple", "Banana"])
            .Add(p => p.SelectedSourceItems, ["Banana"]));

        // Assert
        var items = cut.FindAll("li[role='option']");
        Assert.Equal("false", items.Single(i => i.TextContent.Trim() == "Apple").GetAttribute("aria-selected"));
        Assert.Equal("true", items.Single(i => i.TextContent.Trim() == "Banana").GetAttribute("aria-selected"));
    }

    [Fact]
    public void TwPickList_Renders_WithId()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwPickList<string>>(parameters => parameters
            .Add(p => p.Id, "test-pick-list"));

        // Assert
        var root = cut.Find("#test-pick-list");
        Assert.NotNull(root);
    }

    [Fact]
    public void TwPickList_Renders_WithCustomClass()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwPickList<string>>(parameters => parameters
            .Add(p => p.Class, "custom-pick-list-class"));

        // Assert
        Assert.NotEmpty(cut.FindAll(".custom-pick-list-class"));
    }

    [Fact]
    public void TwPickList_Renders_WithStyle()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwPickList<string>>(parameters => parameters
            .Add(p => p.Style, "margin-top: 10px;"));

        // Assert
        Assert.NotEmpty(cut.FindAll("[style*='margin-top: 10px']"));
    }

    [Fact]
    public void TwPickList_Renders_WithAriaLabel()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwPickList<string>>(parameters => parameters
            .Add(p => p.AriaLabel, "Fruit picker"));

        // Assert
        Assert.NotEmpty(cut.FindAll("[aria-label='Fruit picker']"));
    }

    [Fact]
    public void TwPickList_Renders_WithAttributes()
    {
        // Arrange
        var attributes = new Dictionary<string, object>
        {
            { "data-test", "pick-list" }
        };

        // Act
        var cut = TestContext.Render<TwPickList<string>>(parameters => parameters
            .Add(p => p.Attributes, attributes));

        // Assert
        Assert.NotEmpty(cut.FindAll("[data-test='pick-list']"));
    }

    private sealed class TestPerson
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
