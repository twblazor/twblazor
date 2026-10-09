using TwBlazor.Docs.Compiler;

namespace TwBlazor.Docs.Tests.Build;

public sealed class ComponentThemeDependencyScannerTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "twblazor-deps-" + Guid.NewGuid().ToString("N"));

    public ComponentThemeDependencyScannerTests() => Directory.CreateDirectory(_root);

    public void Dispose() => Directory.Delete(_root, recursive: true);

    private void Write(string relativePath, string content)
    {
        var path = Path.Combine(_root, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content);
    }

    [Fact]
    public void Scan_ReturnsTheThemesAComponentRequiresItself()
    {
        Write("TwCard.razor.cs", "var t = options.Theme.Components.Require<TwCardTheme>();");

        var result = ComponentThemeDependencyScanner.Scan(_root, ["TwCard"]);

        Assert.Equal(["TwCardTheme"], result["TwCard"]);
    }

    [Fact]
    public void Scan_IncludesThemesOfComponentsItRenders()
    {
        Write("TwDatePicker.razor", "<TwButton Label=\"Open\" />");
        Write("TwDatePicker.razor.cs", "Require<TwDatePickerTheme>();");
        Write("TwButton.razor.cs", "Require<TwButtonTheme>();");

        var result = ComponentThemeDependencyScanner.Scan(_root, ["TwDatePicker"]);

        Assert.Equal(["TwButtonTheme", "TwDatePickerTheme"], result["TwDatePicker"]);
    }

    [Fact]
    public void Scan_FollowsDependenciesTransitively()
    {
        Write("TwA.razor", "<TwB />");
        Write("TwB.razor", "<TwC />");
        Write("TwC.razor.cs", "Require<TwCTheme>();");

        var result = ComponentThemeDependencyScanner.Scan(_root, ["TwA"]);

        Assert.Equal(["TwCTheme"], result["TwA"]);
    }

    [Fact]
    public void Scan_FollowsBuilderAndBaseClassReferences()
    {
        Write("TwSwitch.razor.cs", "class TwSwitch : TwInputBase { var b = new ChipBuilder(); }");
        Write("TwInputBase.cs", "Require<TwInputTheme>();");
        Write("ChipBuilder.cs", "Require<TwChipTheme>();");

        var result = ComponentThemeDependencyScanner.Scan(_root, ["TwSwitch"]);

        Assert.Equal(["TwChipTheme", "TwInputTheme"], result["TwSwitch"]);
    }

    [Fact]
    public void Scan_IgnoresTypesOnlyMentionedInComments()
    {
        Write("TwCard.razor.cs", "// see TwButton\n/// <see cref=\"TwButton\"/>\nRequire<TwCardTheme>();");
        Write("TwButton.razor.cs", "Require<TwButtonTheme>();");

        var result = ComponentThemeDependencyScanner.Scan(_root, ["TwCard"]);

        Assert.Equal(["TwCardTheme"], result["TwCard"]);
    }

    [Fact]
    public void Scan_HandlesCyclesBetweenComponents()
    {
        Write("TwA.razor", "<TwB />\nRequire<TwATheme>();");
        Write("TwB.razor", "<TwA />\nRequire<TwBTheme>();");

        var result = ComponentThemeDependencyScanner.Scan(_root, ["TwA"]);

        Assert.Equal(["TwATheme", "TwBTheme"], result["TwA"]);
    }

    [Fact]
    public void Scan_MapsAnUnknownComponentToNoThemes()
    {
        var result = ComponentThemeDependencyScanner.Scan(_root, ["TwMissing"]);

        Assert.Empty(result["TwMissing"]);
    }

    [Fact]
    public void Scan_SkipsBuildOutput()
    {
        Write("TwCard.razor.cs", "Require<TwCardTheme>();");
        Write(Path.Combine("obj", "TwCard.cs"), "Require<TwStaleTheme>();");

        var result = ComponentThemeDependencyScanner.Scan(_root, ["TwCard"]);

        Assert.Equal(["TwCardTheme"], result["TwCard"]);
    }

    [Fact]
    public void Scan_IncludesThemesOfTypesInTheComponentsFolder()
    {
        Write(Path.Combine("Button", "TwButton.razor.cs"), "Require<TwButtonTheme>();");
        Write(Path.Combine("Button", "TwButtonGroup.razor.cs"), "Require<TwGroupsTheme>();");
        Write(Path.Combine("Card", "TwCard.razor.cs"), "Require<TwCardTheme>();");

        var result = ComponentThemeDependencyScanner.Scan(_root, ["TwButton", "TwCard"]);

        Assert.Equal(["TwButtonTheme", "TwGroupsTheme"], result["TwButton"]);
        Assert.Equal(["TwCardTheme"], result["TwCard"]);
    }

    [Fact]
    public void Scan_OfTheRealLibrary_GivesGroupedComponentsTheGroupsTheme()
    {
        var names = ComponentThemeDependencyScanner.ReadComponentNames(Paths.ComponentsJsonPath);

        var result = ComponentThemeDependencyScanner.Scan(Paths.LibrarySourcePath, names);

        Assert.Contains("TwGroupsTheme", result["TwButton"]);
        Assert.Contains("TwGroupsTheme", result["TwCheckbox"]);
        Assert.Contains("TwGroupsTheme", result["TwChip"]);
        Assert.Contains("TwGroupsTheme", result["TwRadioButton"]);
    }

    [Fact]
    public void ReadComponentNames_IncludesNestedGroupEntries_AndSkipsNamelessOnes()
    {
        var path = Path.Combine(_root, "components.json");
        File.WriteAllText(path, """
            [
              { "category": "Forms", "items": [
                { "display": "Button", "name": "TwButton" },
                { "display": "Dates", "items": [ { "display": "Date", "name": "TwDatePicker" } ] }
              ] },
              { "category": "Theme customisation", "items": [ { "display": "Theme Builder", "url": "/theme-builder" } ] }
            ]
            """);

        var names = ComponentThemeDependencyScanner.ReadComponentNames(path);

        Assert.Equal(["TwButton", "TwDatePicker"], names);
    }

    [Fact]
    public void Scan_OfTheRealLibrary_GivesEveryDocumentedComponentItsOwnTheme()
    {
        var names = ComponentThemeDependencyScanner.ReadComponentNames(Paths.ComponentsJsonPath);

        var result = ComponentThemeDependencyScanner.Scan(Paths.LibrarySourcePath, names);

        Assert.Contains("TwCheckboxTheme", result["TwCheckbox"]);
        Assert.Contains("TwDatePickerTheme", result["TwDateRangePicker"]);
        Assert.Contains("TwSidebarTheme", result["TwNavbar"]);
        Assert.Contains("TwOverlayTheme", result["TwDialog"]);
    }
}
