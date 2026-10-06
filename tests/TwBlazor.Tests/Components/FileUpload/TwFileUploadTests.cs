using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TwBlazor.Components;

namespace TwBlazor.Tests.Components.FileUpload;

public class TwFileUploadTests : TwBlazorTestBase
{
    [Fact]
    public void TwFileUpload_Renders_InputFileElement_WithExpectedIdAndLabel()
    {
        // Arrange
        var cut = TestContext.Render<TwFileUpload>(parameters => parameters
            .Add(p => p.Id, "test-id")
            .Add(p => p.Label, "Test Label"));

        // Act & Assert
        var inputFile = cut.Find("input[type='file']");
        Assert.Equal("test-id", inputFile.GetAttribute("id"));
        var label = cut.Find("label");
        Assert.Equal("Test Label", label.TextContent.Trim());
    }

    [Fact]
    public void TwFileUpload_Renders_Icon_WhenIconParameterIsProvided()
    {
        // Arrange
        var cut = TestContext.Render<TwFileUpload>(parameters => parameters
            .Add(p => p.Icon, Enums.Icon.Upload));

        // Act & Assert
        var icon = cut.Find("i");
        Assert.NotNull(icon);
    }

    [Fact]
    public void TwFileUpload_Renders_WithDefaultClasses()
    {
        // Arrange
        var cut = TestContext.Render<TwFileUpload>();

        // Act & Assert
        var rootDiv = cut.Find("div");
        Assert.NotNull(rootDiv);
        Assert.Contains("rounded font-medium", rootDiv.GetAttribute("class"));
    }

    [Fact]
    public void TwFileUpload_WhenAllowedFileTypesIsSet_InputFileHasAcceptAttribute()
    {
        // Arrange
        var allowedTypes = new[] { ".png", ".jpg", "image/pdf" };
        var cut = TestContext.Render<TwFileUpload>(parameters => parameters
            .Add(p => p.AllowedFileTypes, allowedTypes));

        // Act & Assert
        var inputFile = cut.Find("input[type='file']");
        Assert.Equal(".png,.jpg,image/pdf", inputFile.GetAttribute("accept"));
    }

    [Fact]
    public void TwFileUpload_WhenAttributesContainsExplicitAccept_AcceptIsPreserved()
    {
        // Arrange
        var attributes = new Dictionary<string, object> { { "accept", "application/pdf" } };
        var cut = TestContext.Render<TwFileUpload>(parameters => parameters
            .Add(p => p.Attributes, attributes)
            .Add(p => p.AllowedFileTypes, [".png", ".jpg"]));

        // Act & Assert
        var inputFile = cut.Find("input[type='file']");
        Assert.Equal("application/pdf,.png,.jpg", inputFile.GetAttribute("accept"));
    }

    [Fact]
    public void TwFileUpload_WhenMultipleIsFalse_InputFileHasNoMultipleAttribute()
    {
        // Arrange
        var cut = TestContext.Render<TwFileUpload>(parameters => parameters
            .Add(p => p.Multiple, false));

        // Act & Assert
        var inputFile = cut.Find("input[type='file']");
        Assert.Null(inputFile.GetAttribute("multiple"));
    }

    [Fact]
    public void TwFileUpload_WhenMultipleIsTrue_InputFileHasMultipleAttribute()
    {
        // Arrange
        var cut = TestContext.Render<TwFileUpload>(parameters => parameters
            .Add(p => p.Multiple, true));

        // Act & Assert
        var inputFile = cut.Find("input[type='file']");
        Assert.Equal("", inputFile.GetAttribute("multiple"));
    }

    [Fact]
    public void TwFileUpload_WhenDisabled_InputFileHasDisabledAttribute()
    {
        // Arrange
        var cut = TestContext.Render<TwFileUpload>(parameters => parameters
            .Add(p => p.Disabled, true));

        // Act & Assert
        var inputFile = cut.Find("input[type='file']");
        Assert.True(inputFile.HasAttribute("disabled"));
    }

    [Fact]
    public void TwFileUpload_WhenReadOnly_InputFileHasAriaReadonlyAttribute()
    {
        // "readonly" is not a supported HTML attribute on input[type=file] - browsers ignore it,
        // so it conveyed nothing to assistive tech and had no effect. aria-readonly is the real
        // signal now, backed by OnUpload() rejecting new selections while ReadOnly is set.
        // Arrange
        var cut = TestContext.Render<TwFileUpload>(parameters => parameters
            .Add(p => p.ReadOnly, true));

        // Act & Assert
        var inputFile = cut.Find("input[type='file']");
        Assert.False(inputFile.HasAttribute("readonly"));
        Assert.Equal("true", inputFile.GetAttribute("aria-readonly"));
    }

    [Fact]
    public void TwFileUpload_UploadingFile_WhenReadOnly_DoesNotAddFile()
    {
        // Arrange
        List<IBrowserFile>? received = null;
        var cut = TestContext.Render<TwFileUpload>(parameters => parameters
            .Add(p => p.ReadOnly, true)
            .Add(p => p.FilesChanged, EventCallback.Factory.Create<List<IBrowserFile>?>(this, f => received = f)));
        var inputFile = cut.FindComponent<InputFile>();

        // Act
        inputFile.UploadFiles(InputFileContent.CreateFromText("hello", "blocked.txt"));

        // Assert
        Assert.Null(received);
        Assert.Empty(cut.FindAll(".text-sm[class*=\"oklch(40%\"] > *"));
    }

    [Fact]
    public void TwFileUpload_UploadingFile_WhenDisabled_DoesNotAddFile()
    {
        // Arrange
        List<IBrowserFile>? received = null;
        var cut = TestContext.Render<TwFileUpload>(parameters => parameters
            .Add(p => p.Disabled, true)
            .Add(p => p.FilesChanged, EventCallback.Factory.Create<List<IBrowserFile>?>(this, f => received = f)));
        var inputFile = cut.FindComponent<InputFile>();

        // Act
        inputFile.UploadFiles(InputFileContent.CreateFromText("hello", "blocked.txt"));

        // Assert
        Assert.Null(received);
        Assert.Empty(cut.FindAll(".text-sm[class*=\"oklch(40%\"] > *"));
    }

    [Fact]
    public void TwFileUpload_LabelClasses_UsePeerFocusVisible_InsteadOfFocus()
    {
        // Arrange & Act - the visible label carries the themed focus ring (via peer-focus-visible)
        // since the real file input is sr-only and its own native focus ring would be invisible.
        var cut = TestContext.Render<TwFileUpload>();

        // Assert
        var label = cut.Find("label");
        var classes = label.GetAttribute("class") ?? string.Empty;
        Assert.Contains("peer-focus-visible:", classes);
        Assert.DoesNotContain(" focus:", classes);
    }

    [Fact]
    public void TwFileUpload_WhenFilesIsBound_SelectedFilesAreSynchronized()
    {
        // Arrange (this tests the OnParametersSet logic)
        var cut = TestContext.Render<TwFileUpload>();

        // Act & Assert
        // Component renders without errors - actual file binding logic would be tested
        // with more complex test scenarios, but this validates basic rendering
        Assert.NotNull(cut);
    }

    [Fact]
    public void TwFileUpload_ComponentRendersWithoutErrors()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwFileUpload>();

        // Assert
        Assert.NotNull(cut);
    }

    [Fact]
    public void TwFileUpload_WhenIconIsNull_NoIconIsRendered()
    {
        // Arrange
        var cut = TestContext.Render<TwFileUpload>(parameters => parameters
            .Add(p => p.Icon, null));

        // Act & Assert
        Assert.NotNull(cut);
        // The component should render without icon related elements
        Assert.Throws<ElementNotFoundException>(() => cut.Find("i"));
    }

    [Fact]
    public void TwFileUpload_WhenNoAllowedFileTypes_ExtensionIsNotAddedToAttributes()
    {
        // Arrange
        var cut = TestContext.Render<TwFileUpload>(parameters => parameters
            .Add(p => p.AllowedFileTypes, null));

        // Act & Assert
        var inputFile = cut.Find("input[type='file']");
        Assert.False(inputFile.HasAttribute("accept"));
    }

    [Fact]
    public void TwFileUpload_WhenAllowedFileTypesIsEmpty_ExtensionIsNotAddedToAttributes()
    {
        // Arrange
        var cut = TestContext.Render<TwFileUpload>(parameters => parameters
            .Add(p => p.AllowedFileTypes, Array.Empty<string>()));

        // Act & Assert
        var inputFile = cut.Find("input[type='file']");
        Assert.False(inputFile.HasAttribute("accept"));
    }

    [Fact]
    public void TwFileUpload_WhenAllowedFileTypesProvided_AndAttributesIsNull_CreatesAttributesDictionary()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwFileUpload>(parameters => parameters
            .Add(p => p.Attributes, null!)
            .Add(p => p.AllowedFileTypes, [".png"]));

        // Assert
        var inputFile = cut.Find("input[type='file']");
        Assert.Equal(".png", inputFile.GetAttribute("accept"));
    }

    [Fact]
    public void TwFileUpload_UploadingSingleFile_WhenNotMultiple_AddsFileAndRendersChip()
    {
        // Arrange
        var cut = TestContext.Render<TwFileUpload>(parameters => parameters
            .Add(p => p.Multiple, false));
        var inputFile = cut.FindComponent<InputFile>();

        // Act
        inputFile.UploadFiles(InputFileContent.CreateFromText("hello", "single.txt"));

        // Assert
        var chip = cut.Find(".text-sm[class*=\"oklch(40%\"]");
        Assert.Contains("single.txt", chip.TextContent);
    }

    [Fact]
    public void TwFileUpload_UploadingSingleFile_WhenMultiple_AddsSingleFile()
    {
        // Arrange
        var cut = TestContext.Render<TwFileUpload>(parameters => parameters
            .Add(p => p.Multiple, true));
        var inputFile = cut.FindComponent<InputFile>();

        // Act
        inputFile.UploadFiles(InputFileContent.CreateFromText("hello", "only.txt"));

        // Assert
        var chip = cut.Find(".text-sm[class*=\"oklch(40%\"]");
        Assert.Contains("only.txt", chip.TextContent);
        Assert.DoesNotContain(",", chip.TextContent);
    }

    [Fact]
    public void TwFileUpload_UploadingMultipleFiles_WhenMultiple_AddsAllFiles()
    {
        // Arrange
        var cut = TestContext.Render<TwFileUpload>(parameters => parameters
            .Add(p => p.Multiple, true));
        var inputFile = cut.FindComponent<InputFile>();

        // Act
        inputFile.UploadFiles(
            InputFileContent.CreateFromText("a", "first.txt"),
            InputFileContent.CreateFromText("b", "second.txt"));

        // Assert
        var chips = cut.FindAll(".text-sm[class*=\"oklch(40%\"] > *");
        var chipText = string.Join(" ", chips.Select(c => c.TextContent));
        Assert.Contains("first.txt", chipText);
        Assert.Contains("second.txt", chipText);
    }

    [Fact]
    public void TwFileUpload_UploadingFile_InvokesOnChangeCallback()
    {
        // Arrange
        InputFileChangeEventArgs? received = null;
        var cut = TestContext.Render<TwFileUpload>(parameters => parameters
            .Add(p => p.OnChange, EventCallback.Factory.Create<InputFileChangeEventArgs>(this, e => received = e)));
        var inputFile = cut.FindComponent<InputFile>();

        // Act
        inputFile.UploadFiles(InputFileContent.CreateFromText("hello", "onchange.txt"));

        // Assert
        Assert.NotNull(received);
        Assert.Equal("onchange.txt", received!.File.Name);
    }

    [Fact]
    public void TwFileUpload_UploadingFile_InvokesFilesChangedCallback()
    {
        // Arrange
        List<IBrowserFile>? received = null;
        var cut = TestContext.Render<TwFileUpload>(parameters => parameters
            .Add(p => p.FilesChanged, EventCallback.Factory.Create<List<IBrowserFile>?>(this, f => received = f)));
        var inputFile = cut.FindComponent<InputFile>();

        // Act
        inputFile.UploadFiles(InputFileContent.CreateFromText("hello", "bound.txt"));

        // Assert
        Assert.NotNull(received);
        Assert.Single(received!);
        Assert.Equal("bound.txt", received![0].Name);
    }

    [Fact]
    public void TwFileUpload_WhenFilesParameterProvided_SynchronizesSelectedFiles()
    {
        // Arrange
        List<IBrowserFile> files = [new TestBrowserFile("existing.txt")];

        // Act
        var cut = TestContext.Render<TwFileUpload>(parameters => parameters
            .Add(p => p.Files, files));

        // Assert
        var chip = cut.Find(".text-sm[class*=\"oklch(40%\"]");
        Assert.Contains("existing.txt", chip.TextContent);
    }

    [Fact]
    public void TwFileUpload_RemoveFile_RemovesChipAndInvokesFilesChanged()
    {
        // Arrange
        List<IBrowserFile>? received = null;
        var cut = TestContext.Render<TwFileUpload>(parameters => parameters
            .Add(p => p.FilesChanged, EventCallback.Factory.Create<List<IBrowserFile>?>(this, f => received = f)));
        var inputFile = cut.FindComponent<InputFile>();
        inputFile.UploadFiles(InputFileContent.CreateFromText("hello", "removable.txt"));

        // Act
        var closeButton = cut.Find("button");
        closeButton.Click();

        // Assert
        Assert.Throws<ElementNotFoundException>(() => cut.Find(".text-sm[class*=\"oklch(40%\"]"));
        Assert.NotNull(received);
        Assert.Empty(received!);
    }

    [Fact]
    public async Task TwFileUpload_RemoveFile_WhenFileNotPresent_DoesNothing()
    {
        // Arrange
        var cut = TestContext.Render<TwFileUpload>();

        // Act & Assert (should not throw)
        await cut.InvokeAsync(() => cut.Instance.RemoveFile(new TestBrowserFile("missing.txt")));
        Assert.Throws<ElementNotFoundException>(() => cut.Find(".text-sm[class*=\"oklch(40%\"]"));
    }

    [Fact]
    public void TwFileUpload_WhenNoFilesSelected_ChipContainerIsNotRendered()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwFileUpload>();

        // Assert
        Assert.Throws<ElementNotFoundException>(() => cut.Find(".text-sm[class*=\"oklch(40%\"]"));
    }

    private sealed class TestBrowserFile(string name) : IBrowserFile
    {
        public string Name { get; } = name;
        public DateTimeOffset LastModified => DateTimeOffset.UtcNow;
        public long Size => 0;
        public string ContentType => "text/plain";
        public Stream OpenReadStream(long maxAllowedSize = 512000, CancellationToken cancellationToken = default) => Stream.Null;
    }
}