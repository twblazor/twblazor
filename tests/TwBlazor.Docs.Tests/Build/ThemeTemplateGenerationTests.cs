using TwBlazor.Docs.Compiler;

namespace TwBlazor.Docs.Tests.Build;

public class ThemeTemplateGenerationTests
{
    [Fact]
    public void GenerateThemeTemplateClass_EmitsTheSplitTemplateAndDependencyMap()
    {
        // Arrange
        var template = new ThemeTemplate(
            ["TwBlazor.Configuration"],
            [new ThemeStatement("display", "        var display = \"block\";")],
            "        return new Theme\n        [",
            [new ThemeBlock("TwCardTheme", "                new TwCardTheme { Container = \"px-6\" }")],
            "        ];");
        var map = new Dictionary<string, List<string>> { ["TwCard"] = ["TwCardTheme"] };

        // Act
        var code = CodeGenerator.GenerateThemeTemplateClass(template, map);

        // Assert
        Assert.Contains("internal static class ThemeTemplateData", code);
        Assert.Contains("@\"TwBlazor.Configuration\"", code);
        Assert.Contains("(@\"display\", @\"        var display = \"\"block\"\";\")", code);
        Assert.Contains("(@\"TwCardTheme\", @\"                new TwCardTheme { Container = \"\"px-6\"\" }\")", code);
        Assert.Contains("(@\"TwCard\", [@\"TwCardTheme\"])", code);
    }
}
