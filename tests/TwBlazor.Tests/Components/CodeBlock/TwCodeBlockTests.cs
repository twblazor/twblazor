using Bunit;
using Microsoft.AspNetCore.Components.Web;
using TwBlazor.Components;
using TwBlazor.Enums;

namespace TwBlazor.Tests.Components.CodeBlock;

public class TwCodeBlockTests : TwBlazorTestBase
{
    public TwCodeBlockTests()
    {
        TestContext.JSInterop.SetupVoid("twCodeBlock.highlightElement", _ => true);
    }

    [Fact]
    public void TwCodeBlock_AcceptsInlineParameter()
    {
        // Arrange & Act - Inline is currently an unused/vestigial parameter with no effect
        // on rendering, but should still be settable without error.
        var cut = TestContext.Render<TwCodeBlock>(parameters => parameters
            .Add(p => p.Inline, true));

        // Assert
        Assert.NotNull(cut.Find("div"));
    }

    [Fact]
    public void TwCodeBlock_Renders_WithDefaultValues()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCodeBlock>();

        // Assert
        var div = cut.Find("div");
        Assert.NotNull(div);
        var code = cut.Find("code");
        Assert.NotNull(code);
        Assert.Contains("language-html", code.GetAttribute("class"));
    }

    [Fact]
    public void TwCodeBlock_GeneratesId_WhenNotProvided()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCodeBlock>();

        // Assert
        var div = cut.Find("div");
        var id = div.GetAttribute("id");
        Assert.NotNull(id);
        Assert.StartsWith("codeblock-", id);
    }

    [Fact]
    public void TwCodeBlock_UsesProvidedId()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCodeBlock>(parameters => parameters
            .Add(p => p.Id, "custom-code-id"));

        // Assert
        var div = cut.Find("div");
        Assert.Equal("custom-code-id", div.GetAttribute("id"));
    }

    [Fact]
    public void TwCodeBlock_GeneratesUniqueIds_ForMultipleInstances()
    {
        // Arrange & Act
        var cut1 = TestContext.Render<TwCodeBlock>();
        var cut2 = TestContext.Render<TwCodeBlock>();

        // Assert
        var id1 = cut1.Find("div").GetAttribute("id");
        var id2 = cut2.Find("div").GetAttribute("id");
        Assert.NotEqual(id1, id2);
    }

    [Fact]
    public void TwCodeBlock_RendersContent()
    {
        // Arrange
        var content = "const x = 10;";

        // Act
        var cut = TestContext.Render<TwCodeBlock>(parameters => parameters
            .Add(p => p.Content, content));

        // Assert
        var code = cut.Find("code");
        Assert.Contains(content, code.TextContent);
    }

    [Fact]
    public void TwCodeBlock_RendersWithHtmlContent()
    {
        // Arrange
        var htmlContent = "<div class=\"test\">Hello</div>";

        // Act
        var cut = TestContext.Render<TwCodeBlock>(parameters => parameters
            .Add(p => p.Content, htmlContent));

        // Assert
        var code = cut.Find("code");
        Assert.Contains(htmlContent, code.TextContent);
    }

    [Fact]
    public void TwCodeBlock_AppliesLanguageClass()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCodeBlock>(parameters => parameters
            .Add(p => p.Language, "javascript"));

        // Assert
        var code = cut.Find("code");
        Assert.Contains("language-javascript", code.GetAttribute("class"));
    }

    [Fact]
    public void TwCodeBlock_DefaultLanguage_IsHtml()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCodeBlock>();

        // Assert
        var code = cut.Find("code");
        Assert.Contains("language-html", code.GetAttribute("class"));
    }

    [Theory]
    [InlineData("csharp", "language-csharp")]
    [InlineData("python", "language-python")]
    [InlineData("typescript", "language-typescript")]
    [InlineData("css", "language-css")]
    public void TwCodeBlock_AppliesCorrectLanguageClass_ForEachLanguage(string language, string expectedClass)
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCodeBlock>(parameters => parameters
            .Add(p => p.Language, language));

        // Assert
        var code = cut.Find("code");
        Assert.Contains(expectedClass, code.GetAttribute("class"));
    }

    [Fact]
    public void TwCodeBlock_HasDefaultClasses()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCodeBlock>();

        // Assert
        var div = cut.Find("div");
        var classes = div.GetAttribute("class");
        Assert.Contains("relative", classes);
        Assert.Contains("flex", classes);
        Assert.Contains("flex-col", classes);
        Assert.Contains("rounded", classes);
    }

    [Fact]
    public void TwCodeBlock_AppliesCustomClass()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCodeBlock>(parameters => parameters
            .Add(p => p.Class, "custom-code-class"));

        // Assert
        var div = cut.Find("div");
        var classes = div.GetAttribute("class");
        Assert.Contains("custom-code-class", classes);
        Assert.Contains("relative", classes);
        Assert.Contains("flex", classes);
    }

    [Fact]
    public void TwCodeBlock_RendersCopyButton()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCodeBlock>();

        // Assert
        var button = cut.Find("button");
        Assert.NotNull(button);
        var icon = cut.Find("i.bi-copy");
        Assert.NotNull(icon);
        Assert.False(button.HasAttribute("disabled"));
    }

    [Fact]
    public void TwCodeBlock_CopyButton_HasCorrectClasses()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCodeBlock>();

        // Assert
        var button = cut.Find("button");
        var classes = button.GetAttribute("class");
        Assert.Contains("hover:text-", classes);
    }

    [Fact]
    public void TwCodeBlock_CopyButton_SitsInTheHeaderNotOverTheCode()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCodeBlock>();

        // Assert
        var header = cut.Find("div > div");
        Assert.NotNull(header.QuerySelector("button"));
        Assert.DoesNotContain("absolute", cut.Find("button").GetAttribute("class"));
        Assert.Contains("justify-between", header.GetAttribute("class"));
    }

    [Theory]
    [InlineData("html", "Razor")]
    [InlineData("csharp", "C#")]
    [InlineData("bash", "Terminal")]
    [InlineData("css", "CSS")]
    [InlineData("Bash", "Terminal")]
    [InlineData("razor", "RAZOR")]
    public void TwCodeBlock_Header_ShowsADisplayNameForTheLanguage(string language, string expected)
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCodeBlock>(parameters => parameters
            .Add(p => p.Language, language));

        // Assert
        Assert.Equal(expected, cut.Find("div > div > span").TextContent);
    }

    [Fact]
    public void TwCodeBlock_Header_ShowsTheTitle_WhenOneIsGiven()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCodeBlock>(parameters => parameters
            .Add(p => p.Language, "bash")
            .Add(p => p.Title, "Install"));

        // Assert
        Assert.Equal("Install", cut.Find("div > div > span").TextContent);
    }

    [Fact]
    public void TwCodeBlock_Panel_WrapsTheCodeInAnInsetSurface()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCodeBlock>();

        // Assert
        var pre = cut.Find("pre");
        Assert.Contains("text-sm", pre.GetAttribute("class"));
        Assert.NotNull(pre.QuerySelector("code"));
    }

    [Fact]
    public async Task TwCodeBlock_Copy_CallsJavaScriptInterop()
    {
        // Arrange
        var content = "const test = 'hello';";
        var jsRuntime = TestContext.JSInterop;
        jsRuntime.SetupVoid("navigator.clipboard.writeText", _ => true).SetVoidResult();

        var cut = TestContext.Render<TwCodeBlock>(parameters => parameters
            .Add(p => p.Content, content));

        // Act
        var button = cut.Find("button");
        await button.ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());

        // Wait for the delay
        await Task.Delay(1100, Xunit.TestContext.Current.CancellationToken); // Wait slightly more than the 1000ms delay

        // Assert
        var invocations = jsRuntime.Invocations["navigator.clipboard.writeText"];
        var invocation = Assert.Single(invocations);
        Assert.Equal(content, invocation.Arguments[0]);
    }

    [Fact]
    public void TwCodeBlock_OnAfterRender_CallsHighlightElement()
    {
        // Arrange
        var jsRuntime = TestContext.JSInterop;
        jsRuntime.SetupVoid("twCodeBlock.highlightElement", _ => true);

        // Act
        var cut = TestContext.Render<TwCodeBlock>();

        // Assert - Should be called on first render
        var invocations = jsRuntime.Invocations["twCodeBlock.highlightElement"];
        Assert.Single(invocations);
    }

    [Fact]
    public void TwCodeBlock_OnAfterRender_OnlyCallsOnFirstRender()
    {
        // Arrange
        var jsRuntime = TestContext.JSInterop;
        jsRuntime.SetupVoid("jCodeBlock.highlightElement", _ => true);
        jsRuntime.SetupVoid("navigator.clipboard.writeText", _ => true);

        // Act
        var cut = TestContext.Render<TwCodeBlock>(parameters => parameters
            .Add(p => p.Content, "initial content"));

        var initialInvocationCount = jsRuntime.Invocations["jCodeBlock.highlightElement"].Count;

        // Re-render by clicking copy button (this triggers a state change and re-render)
        var button = cut.Find("button");
        button.Click();

        // Assert - Should still only be called once (on first render)
        var invocations = jsRuntime.Invocations["jCodeBlock.highlightElement"];
        Assert.Equal(initialInvocationCount, invocations.Count);
    }

    [Fact]
    public void TwCodeBlock_RendersWithPreElement()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCodeBlock>();

        // Assert
        var pre = cut.Find("pre");
        Assert.NotNull(pre);
    }

    [Fact]
    public void TwCodeBlock_CodeElement_IsInsidePreElement()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCodeBlock>();

        // Assert
        var pre = cut.Find("pre");
        var code = pre.QuerySelector("code");
        Assert.NotNull(code);
    }

    [Fact]
    public void TwCodeBlock_RendersWithEmptyContent()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCodeBlock>(parameters => parameters
            .Add(p => p.Content, string.Empty));

        // Assert
        var code = cut.Find("code");
        Assert.Empty(code.TextContent.Trim());
    }

    [Fact]
    public void TwCodeBlock_RendersWithNullContent()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCodeBlock>(parameters => parameters
            .Add(p => p.Content, null));

        // Assert
        var code = cut.Find("code");
        Assert.NotNull(code);
    }

    [Fact]
    public void TwCodeBlock_RendersWithMultilineContent()
    {
        // Arrange
        var multilineContent = @"function test() {
    return 'hello';
}";

        // Act
        var cut = TestContext.Render<TwCodeBlock>(parameters => parameters
            .Add(p => p.Content, multilineContent));

        // Assert
        var code = cut.Find("code");
        Assert.Contains("function test()", code.TextContent);
        Assert.Contains("return 'hello'", code.TextContent);
    }

    [Fact]
    public void TwCodeBlock_RendersWithAllProperties()
    {
        // Arrange
        var content = "console.log('test');";
        var language = "javascript";
        var customId = "my-code-block";
        var customClass = "shadow-lg";

        // Act
        var cut = TestContext.Render<TwCodeBlock>(parameters => parameters
            .Add(p => p.Id, customId)
            .Add(p => p.Content, content)
            .Add(p => p.Language, language)
            .Add(p => p.Class, customClass));

        // Assert
        var div = cut.Find("div");
        var code = cut.Find("code");

        Assert.Equal(customId, div.GetAttribute("id"));
        Assert.Contains(customClass, div.GetAttribute("class"));
        Assert.Contains(content, code.TextContent);
        Assert.Contains($"language-{language}", code.GetAttribute("class"));
    }

    [Fact]
    public void TwCodeBlock_InheritsFromTwBlazorComponentBase()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCodeBlock>();

        // Assert
        Assert.IsType<TwBlazorComponentBase>(cut.Instance, exactMatch: false);
    }

    [Fact]
    public void TwCodeBlock_SupportsAriaLabel()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCodeBlock>(parameters => parameters
            .Add(p => p.AriaLabel, "Code example"));

        // Assert - Component accepts the property without error
        var div = cut.Find("div");
        Assert.NotNull(div);
    }

    [Fact]
    public void TwCodeBlock_PreservesWhitespace_InContent()
    {
        // Arrange
        var contentWithSpaces = "    indented code\n        more indented";

        // Act
        var cut = TestContext.Render<TwCodeBlock>(parameters => parameters
            .Add(p => p.Content, contentWithSpaces));

        // Assert
        var code = cut.Find("code");
        Assert.Contains("    indented", code.TextContent);
    }

    [Fact]
    public void TwCodeBlock_HandlesSpecialCharacters_InContent()
    {
        // Arrange
        var specialContent = "const str = \"Hello & <world>\";";

        // Act
        var cut = TestContext.Render<TwCodeBlock>(parameters => parameters
            .Add(p => p.Content, specialContent));

        // Assert
        var code = cut.Find("code");
        Assert.Contains("Hello", code.TextContent);
    }

    [Fact]
    public void TwCodeBlock_AppliesDefaultShadowClass_FromGlobalOptions()
    {
        // Arrange & Act - no Shadow parameter; global default is Shadow.Sm
        var cut = TestContext.Render<TwCodeBlock>();

        // Assert
        var classes = cut.Find("div").GetAttribute("class");
        Assert.Contains("shadow-sm", classes);
    }

    [Fact]
    public void TwCodeBlock_AppliesExplicitShadowClass_WhenShadowParameterSet()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCodeBlock>(parameters => parameters
            .Add(p => p.Shadow, Shadow.Lg));

        // Assert
        var classes = cut.Find("div").GetAttribute("class");
        Assert.Contains("shadow-lg", classes);
        Assert.DoesNotContain("shadow-sm", classes);
    }

    [Fact]
    public void TwCodeBlock_AppliesShadowNoneClass_WhenShadowIsNone()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCodeBlock>(parameters => parameters
            .Add(p => p.Shadow, Shadow.None));

        // Assert
        var classes = cut.Find("div").GetAttribute("class");
        Assert.Contains("shadow-none", classes);
        Assert.DoesNotContain("shadow-sm", classes);
    }

    [Fact]
    public void TwCodeBlock_AppliesDefaultRoundedClass_FromGlobalOptions()
    {
        // Arrange & Act - no Rounded parameter; global default is Rounded.Md
        var cut = TestContext.Render<TwCodeBlock>();

        // Assert
        var classes = cut.Find("div").GetAttribute("class");
        Assert.Contains("rounded", classes);
    }

    [Fact]
    public void TwCodeBlock_AppliesExplicitRoundedClass_WhenRoundedParameterSet()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCodeBlock>(parameters => parameters
            .Add(p => p.Rounded, Rounded.Lg));

        // Assert
        var classes = cut.Find("div").GetAttribute("class");
        Assert.Contains("rounded-lg", classes);
        Assert.DoesNotContain("rounded-2xl", classes);
    }

    [Fact]
    public void TwCodeBlock_AppliesRoundedNoneClass_WhenRoundedIsNone()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCodeBlock>(parameters => parameters
            .Add(p => p.Rounded, Rounded.None));

        // Assert
        var classes = cut.Find("div").GetAttribute("class");
        Assert.Contains("rounded-none", classes);
        Assert.DoesNotContain("rounded-2xl", classes);
    }

    [Fact]
    public async Task TwCodeBlock_DisposeAsync_DoesNotThrow_WhenCtsIsSet()
    {
        // After rendering, OnAfterRenderAsync creates the CancellationTokenSource
        // so the `is not null` branch in DisposeAsync is exercised.
        var cut = TestContext.Render<TwCodeBlock>();

        var exception = await Record.ExceptionAsync(
            () => ((IAsyncDisposable)cut.Instance).DisposeAsync().AsTask());

        Assert.Null(exception);
    }

    [Fact]
    public async Task TwCodeBlock_DisposeAsync_CancelsPendingCopyOperation()
    {
        // Arrange - Setup clipboard interop to allow the Copy method to start.
        // SetVoidResult() is required so the write completes immediately and execution
        // actually reaches the Task.Delay(1000) below, which is what this test cancels.
        var jsRuntime = TestContext.JSInterop;
        jsRuntime.SetupVoid("navigator.clipboard.writeText", _ => true).SetVoidResult();

        var cut = TestContext.Render<TwCodeBlock>(parameters => parameters
            .Add(p => p.Content, "test content"));

        // Start Copy (button click initiates the async operation)
        var button = cut.Find("button");
        var clickTask = button.ClickAsync(new MouseEventArgs());

        // Give the Copy operation a moment to start and reach the Task.Delay
        await Task.Delay(50, Xunit.TestContext.Current.CancellationToken);

        // Dispose should cancel the pending delay
        await ((IAsyncDisposable)cut.Instance).DisposeAsync();

        // The TaskCanceledException catch inside Copy must swallow the cancellation
        var exception = await Record.ExceptionAsync(() => clickTask);
        Assert.Null(exception);
    }

    [Fact]
    public void OnAfterRenderAsync_SuppressesTaskCanceledException_FromHighlightElement()
    {
        // Arrange
        TestContext.JSInterop.SetupVoid("twCodeBlock.highlightElement", _ => true)
            .SetException(new TaskCanceledException());

        // Act & Assert - should not throw/crash the render
        var cut = TestContext.Render<TwCodeBlock>();
        Assert.NotNull(cut.Find("div"));
    }

    [Fact]
    public void OnAfterRenderAsync_SuppressesJSException_FromHighlightElement()
    {
        // Arrange
        TestContext.JSInterop.SetupVoid("twCodeBlock.highlightElement", _ => true)
            .SetException(new Microsoft.JSInterop.JSException("JS interop unavailable"));

        // Act & Assert - should not throw/crash the render
        var cut = TestContext.Render<TwCodeBlock>();
        Assert.NotNull(cut.Find("div"));
    }

    [Fact]
    public void OnAfterRenderAsync_SuppressesInvalidOperationException_FromHighlightElement()
    {
        // Arrange
        TestContext.JSInterop.SetupVoid("twCodeBlock.highlightElement", _ => true)
            .SetException(new InvalidOperationException("JS runtime unavailable"));

        // Act & Assert - should not throw/crash the render
        var cut = TestContext.Render<TwCodeBlock>();
        Assert.NotNull(cut.Find("div"));
    }

    [Fact]
    public async Task Copy_SuppressesJSException_FromClipboardWrite()
    {
        // Arrange
        TestContext.JSInterop.SetupVoid("navigator.clipboard.writeText", _ => true)
            .SetException(new Microsoft.JSInterop.JSException("clipboard unavailable"));

        var cut = TestContext.Render<TwCodeBlock>(parameters => parameters
            .Add(p => p.Content, "test"));

        // Act & Assert - should not throw/crash
        var button = cut.Find("button");
        var exception = await Record.ExceptionAsync(() => button.ClickAsync(new MouseEventArgs()));
        Assert.Null(exception);
    }

    [Fact]
    public async Task Copy_SuppressesInvalidOperationException_FromClipboardWrite()
    {
        // Arrange
        TestContext.JSInterop.SetupVoid("navigator.clipboard.writeText", _ => true)
            .SetException(new InvalidOperationException("clipboard unavailable"));

        var cut = TestContext.Render<TwCodeBlock>(parameters => parameters
            .Add(p => p.Content, "test"));

        // Act & Assert - should not throw/crash
        var button = cut.Find("button");
        var exception = await Record.ExceptionAsync(() => button.ClickAsync(new MouseEventArgs()));
        Assert.Null(exception);
    }
}
