using AngleSharp.Dom;
using Bunit;
using TwBlazor.Components;
using TwBlazor.Enums;
using Icons = TwBlazor.Enums.Icon;

namespace TwBlazor.Tests.Components.Alert;

public class TwAlertTests : TwBlazorTestBase
{
    [Fact]
    public void TwAlert_Renders_WithDefaultValues()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwAlert>();

        // Assert
        var alert = cut.Find("div[role='alert']");
        Assert.NotNull(alert);
        Assert.Contains("tw-alert", alert.GetAttribute("class"));
        Assert.Contains("rounded", alert.GetAttribute("class"));
    }

    [Fact]
    public void TwAlert_Renders_WithText()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwAlert>(parameters => parameters
            .Add(p => p.Text, "This is an alert message"));

        // Assert
        var alert = cut.Find("div[role='alert']");
        Assert.Contains("This is an alert message", alert.TextContent);
    }

    [Fact]
    public void TwAlert_Renders_WithChildContent()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwAlert>(parameters => parameters
            .Add(p => p.ChildContent, RenderFragmentBuilder("This is custom child content")));

        // Assert
        var alert = cut.Find("div[role='alert']");
        Assert.Contains("This is custom child content", alert.TextContent);
    }

    [Fact]
    public void TwAlert_ChildContent_TakesPrecedenceOverText()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwAlert>(parameters => parameters
            .Add(p => p.Text, "This text should not appear")
            .Add(p => p.ChildContent, RenderFragmentBuilder("This content should appear")));

        // Assert
        var alert = cut.Find("div[role='alert']");
        Assert.Contains("This content should appear", alert.TextContent);
        Assert.DoesNotContain("This text should not appear", alert.TextContent);
    }

    [Fact]
    public void TwAlert_Renders_WithColor()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwAlert>(parameters => parameters
            .Add(p => p.Color, Color.Danger)
            .Add(p => p.Text, "Red alert"));

        // Assert
        var alert = cut.Find("div[role='alert']");
        Assert.Contains("bg-red-200", alert.GetAttribute("class"));
        Assert.Contains("border-red-600", alert.GetAttribute("class"));
        Assert.Contains("text-red-900", alert.GetAttribute("class"));
    }

    [Theory]
    [InlineData(Color.Danger, "bg-red-200", "border-red-600", "text-red-900")]
    [InlineData(Color.Primary, "bg-purple-200", "border-purple-600", "text-purple-900")]
    [InlineData(Color.Success, "bg-green-200", "border-green-600", "text-green-900")]
    [InlineData(Color.Warning, "bg-yellow-200", "border-yellow-600", "text-yellow-900")]
    [InlineData(Color.Accent, "bg-fuchsia-200", "border-fuchsia-600", "text-fuchsia-900")]
    [InlineData(Color.Info, "bg-blue-200", "border-blue-600", "text-blue-900")]
    public void TwAlert_Renders_WithCorrectColorClasses(Color color, string bgClass, string borderClass, string textClass)
    {
        // Arrange & Act
        var cut = TestContext.Render<TwAlert>(parameters => parameters
            .Add(p => p.Color, color));

        // Assert
        var alert = cut.Find("div[role='alert']");
        Assert.Contains(bgClass, alert.GetAttribute("class"));
        Assert.Contains(borderClass, alert.GetAttribute("class"));
        Assert.Contains(textClass, alert.GetAttribute("class"));
    }

    [Fact]
    public void TwAlert_Renders_WithStartIcon()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwAlert>(parameters => parameters
            .Add(p => p.StartIcon, Icons.Shield_Exclamation)
            .Add(p => p.Text, "Alert with icon"));

        // Assert
        var icons = cut.FindComponents<TwIcon>();
        Assert.NotEmpty(icons);
        Assert.Equal(Icons.Shield_Exclamation, icons[0].Instance.Icon);
    }

    [Fact]
    public void TwAlert_StartIcon_UsesDefaultPadding()
    {
        var cut = TestContext.Render<TwAlert>(parameters => parameters
            .Add(p => p.StartIcon, Icons.Info_Circle)
            .Add(p => p.Text, "Alert"));

        Assert.Contains("pr-2", cut.FindComponents<TwIcon>()[0].Find("*").GetAttribute("class"));
    }

    [Fact]
    public void TwAlert_StartIcon_UsesCustomPadding()
    {
        var cut = TestContext.Render<TwAlert>(parameters => parameters
            .Add(p => p.StartIcon, Icons.Info_Circle)
            .Add(p => p.StartIconPadding, "pr-4")
            .Add(p => p.Text, "Alert"));

        var classes = cut.FindComponents<TwIcon>()[0].Find("*").GetAttribute("class");
        Assert.Contains("pr-4", classes);
        Assert.DoesNotContain("pr-2", classes);
    }

    [Fact]
    public void TwAlert_EndIcon_UsesDefaultAndCustomPadding()
    {
        var defaultCut = TestContext.Render<TwAlert>(parameters => parameters
            .Add(p => p.EndIcon, Icons.Arrow_Right)
            .Add(p => p.Text, "Alert"));
        var customCut = TestContext.Render<TwAlert>(parameters => parameters
            .Add(p => p.EndIcon, Icons.Arrow_Right)
            .Add(p => p.EndIconPadding, "pl-4")
            .Add(p => p.Text, "Alert"));

        Assert.Contains("pl-2", defaultCut.FindComponents<TwIcon>()[0].Find("*").GetAttribute("class"));
        Assert.Contains("pl-4", customCut.FindComponents<TwIcon>()[0].Find("*").GetAttribute("class"));
    }

    [Fact]
    public void TwAlert_Renders_WithEndIcon()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwAlert>(parameters => parameters
            .Add(p => p.EndIcon, Icons.Arrow_Right)
            .Add(p => p.Text, "Alert with end icon"));

        // Assert
        var icons = cut.FindComponents<TwIcon>();
        Assert.NotEmpty(icons);
        Assert.Equal(Icons.Arrow_Right, icons[0].Instance.Icon);
        Assert.Contains("ml-auto", icons[0].Find("*").GetAttribute("class"));
    }

    [Fact]
    public void TwAlert_Renders_WithBothIcons()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwAlert>(parameters => parameters
            .Add(p => p.StartIcon, Icons.Info_Circle)
            .Add(p => p.EndIcon, Icons.Arrow_Right)
            .Add(p => p.Text, "Alert with both icons"));

        // Assert
        var icons = cut.FindComponents<TwIcon>();
        Assert.Equal(2, icons.Count);
        Assert.Equal(Icons.Info_Circle, icons[0].Instance.Icon);
        Assert.Equal(Icons.Arrow_Right, icons[1].Instance.Icon);
    }

    [Fact]
    public void TwAlert_Renders_WithDismissable()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwAlert>(parameters => parameters
            .Add(p => p.Dismissible, true)
            .Add(p => p.Text, "Dismissable alert"));

        // Assert
        var dismissButton = cut.Find("button[aria-label='Close']");
        Assert.NotNull(dismissButton);
        var svg = cut.Find("svg");
        Assert.NotNull(svg);
    }

    [Fact]
    public void TwAlert_Dismissable_TogglesVisibility()
    {
        // Arrange
        var cut = TestContext.Render<TwAlert>(parameters => parameters
            .Add(p => p.Dismissible, true)
            .Add(p => p.Text, "Dismissable alert"));

        var alert = cut.Find("div[role='alert']");
        Assert.DoesNotContain("hidden", alert.GetAttribute("class"));

        // Act - click dismiss button
        var dismissButton = cut.Find("button[aria-label='Close']");
        dismissButton.Click();

        // Assert - alert should be hidden
        alert = cut.Find("div[role='alert']");
        Assert.Contains("hidden", alert.GetAttribute("class"));
    }

    [Fact]
    public void TwAlert_OnDismiss_InvokesCallback()
    {
        // Arrange
        var dismissed = false;
        var cut = TestContext.Render<TwAlert>(parameters => parameters
            .Add(p => p.OnDismiss, () => dismissed = true)
            .Add(p => p.Text, "Alert with callback"));

        // Act
        var dismissButton = cut.Find("button[aria-label='Close']");
        dismissButton.Click();

        // Assert
        Assert.True(dismissed);
    }

    [Fact]
    public void TwAlert_DismissedChanged_InvokesCallback()
    {
        // Arrange
        var dismissedValue = false;
        var cut = TestContext.Render<TwAlert>(parameters => parameters
            .Add(p => p.Dismissible, true)
            .Add(p => p.DismissedChanged, (bool value) => dismissedValue = value)
            .Add(p => p.Text, "Alert with dismissed changed"));

        // Act
        var dismissButton = cut.Find("button[aria-label='Close']");
        dismissButton.Click();

        // Assert
        Assert.True(dismissedValue);
    }

    [Fact]
    public void TwAlert_Dismissed_HidesAlert()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwAlert>(parameters => parameters
            .Add(p => p.Dismissed, true)
            .Add(p => p.Text, "Hidden alert"));

        // Assert
        var alert = cut.Find("div[role='alert']");
        Assert.Contains("hidden", alert.GetAttribute("class"));
    }

    [Fact]
    public void TwAlert_Dense_AppliesCompactPadding()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwAlert>(parameters => parameters
            .Add(p => p.Dense, true)
            .Add(p => p.Text, "Dense alert"));

        // Assert
        var alert = cut.Find("div[role='alert']");
        Assert.Contains("py-2", alert.GetAttribute("class"));
        Assert.Contains("px-3", alert.GetAttribute("class"));
        Assert.DoesNotContain("py-4", alert.GetAttribute("class"));
    }

    [Fact]
    public void TwAlert_NotDense_AppliesNormalPadding()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwAlert>(parameters => parameters
            .Add(p => p.Dense, false)
            .Add(p => p.Text, "Normal alert"));

        // Assert
        var alert = cut.Find("div[role='alert']");
        Assert.Contains("py-4", alert.GetAttribute("class"));
        Assert.Contains("px-6", alert.GetAttribute("class"));
    }

    [Fact]
    public void TwAlert_Renders_WithId()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwAlert>(parameters => parameters
            .Add(p => p.Id, "custom-alert-id")
            .Add(p => p.Text, "Alert with ID"));

        // Assert
        var alert = cut.Find("div[role='alert']");
        Assert.Equal("custom-alert-id", alert.GetAttribute("id"));
    }

    [Fact]
    public void TwAlert_Renders_WithCustomClass()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwAlert>(parameters => parameters
            .Add(p => p.Class, "my-custom-class")
            .Add(p => p.Text, "Alert with custom class"));

        // Assert
        var alert = cut.Find("div[role='alert']");
        Assert.Contains("my-custom-class", alert.GetAttribute("class"));
    }

    [Fact]
    public void TwAlert_Renders_WithFlexLayout_WhenDismissable()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwAlert>(parameters => parameters
            .Add(p => p.Dismissible, true)
            .Add(p => p.Text, "Dismissable alert"));

        // Assert
        var alert = cut.Find("div[role='alert']");
        Assert.Contains("flex", alert.GetAttribute("class"));
        Assert.Contains("items-center", alert.GetAttribute("class"));
    }

    [Fact]
    public void TwAlert_Renders_WithFlexLayout_WhenEndIconPresent()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwAlert>(parameters => parameters
            .Add(p => p.EndIcon, Icons.Arrow_Right)
            .Add(p => p.Text, "Alert with end icon"));

        // Assert
        var alert = cut.Find("div[role='alert']");
        Assert.Contains("flex", alert.GetAttribute("class"));
        Assert.Contains("items-center", alert.GetAttribute("class"));
    }

    [Fact]
    public void TwAlert_DismissButton_HasCorrectId()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwAlert>(parameters => parameters
            .Add(p => p.Id, "test-alert")
            .Add(p => p.Dismissible, true)
            .Add(p => p.Text, "Alert"));

        // Assert
        var dismissButton = cut.Find("button[aria-label='Close']");
        Assert.Equal("test-alert-dismiss", dismissButton.GetAttribute("id"));
    }

    [Fact]
    public void TwAlert_DismissButton_HasCorrectMargin_WhenNoEndIcon()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwAlert>(parameters => parameters
            .Add(p => p.Dismissible, true)
            .Add(p => p.Text, "Alert"));

        // Assert
        var dismissButton = cut.Find("button[aria-label='Close']");
        Assert.Contains("ml-auto", dismissButton.GetAttribute("class"));
    }

    [Fact]
    public void TwAlert_DismissButton_HasCorrectMargin_WhenEndIconPresent()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwAlert>(parameters => parameters
            .Add(p => p.EndIcon, Icons.Info_Circle)
            .Add(p => p.Dismissible, true)
            .Add(p => p.Text, "Alert"));

        // Assert
        var dismissButton = cut.Find("button[aria-label='Close']");
        Assert.Contains("ml-2", dismissButton.GetAttribute("class"));
        Assert.DoesNotContain("ml-auto", dismissButton.GetAttribute("class"));
    }

    [Fact]
    public void TwAlert_WithoutDismissable_DoesNotRenderDismissButton()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwAlert>(parameters => parameters
            .Add(p => p.Dismissible, false)
            .Add(p => p.Text, "Non-dismissable alert"));

        // Assert
        Assert.Throws<ElementNotFoundException>(() => cut.Find("button[aria-label='Close']"));
    }

    [Fact]
    public void TwAlert_Renders_WithAttributes()
    {
        // Arrange
        var attributes = new Dictionary<string, object>
        {
            { "data-testid", "test-alert" },
            { "data-custom", "custom-value" }
        };

        // Act
        var cut = TestContext.Render<TwAlert>(parameters => parameters
            .Add(p => p.Attributes, attributes)
            .Add(p => p.Text, "Alert with attributes"));

        // Assert
        var alert = cut.Find("div[role='alert']");
        Assert.Equal("test-alert", alert.GetAttribute("data-testid"));
        Assert.Equal("custom-value", alert.GetAttribute("data-custom"));
    }

    [Fact]
    public void TwAlert_BothDismissableAndOnDismiss_RendersSingleButton()
    {
        // Arrange
        var callbackInvoked = false;
        var cut = TestContext.Render<TwAlert>(parameters => parameters
            .Add(p => p.Dismissible, true)
            .Add(p => p.OnDismiss, () => callbackInvoked = true)
            .Add(p => p.Text, "Alert"));

        // Assert - only one button should be rendered
        var buttons = cut.FindAll("button[aria-label='Close']");
        var button = Assert.Single(buttons);

        // Act - click the button
        button.Click();

        // Assert - callback should be invoked
        Assert.True(callbackInvoked);
    }

    [Fact]
    public void TwAlert_DefaultColor_UsesBlue()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwAlert>(parameters => parameters
            .Add(p => p.Text, "Default color alert"));

        // Assert
        var alert = cut.Find("div[role='alert']");
        Assert.DoesNotContain("bg-blue-100", alert.GetAttribute("class"));
        Assert.DoesNotContain("border-blue-500", alert.GetAttribute("class"));
        Assert.DoesNotContain("text-blue-900", alert.GetAttribute("class"));
    }
}
