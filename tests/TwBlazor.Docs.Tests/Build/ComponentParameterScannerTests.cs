using TwBlazor.Docs.Compiler;

namespace TwBlazor.Docs.Tests.Build;

public sealed class ComponentParameterScannerTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "twblazor-params-" + Guid.NewGuid().ToString("N"));

    public ComponentParameterScannerTests() => Directory.CreateDirectory(_root);

    public void Dispose() => Directory.Delete(_root, recursive: true);

    private void Write(string relativePath, string content)
    {
        var path = Path.Combine(_root, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content);
    }

    [Fact]
    public void Scan_ListsOwnParametersBeforeInheritedOnes_WithTheirSummaries()
    {
        Write("TwBase.cs", """
            public class TwBase : ComponentBase
            {
                /// <summary>Gets or sets the id.</summary>
                [Parameter] public string? Id { get; set; }
            }
            """);
        Write("Button/TwButton.razor.cs", """
            /// <summary>A button.</summary>
            public partial class TwButton : TwBase
            {
                /// <summary>Gets or sets the <see cref="Color"/> of the button, or <see langword="null"/>.</summary>
                [Parameter] public Color? Color { get; set; }
            }
            """);

        var button = ComponentParameterScanner.Scan(_root)["TwButton"];

        Assert.Equal("A button.", button.Summary);
        Assert.Equal(["Color", "Id"], button.Parameters.Select(p => p.Name));
        Assert.Equal("Color?", button.Parameters[0].Type);
        Assert.Equal("Gets or sets the `Color` of the button, or `null`.", button.Parameters[0].Description);
        Assert.Equal("TwBase", button.Parameters[1].DeclaredIn);
        Assert.Equal("Gets or sets the id.", button.Parameters[1].Description);
    }

    [Fact]
    public void Scan_ResolvesInheritDocFromTheNamedMember()
    {
        Write("ITwComponent.cs", """
            public interface ITwComponent
            {
                /// <summary>Gets or sets the CSS classes.</summary>
                string Class { get; set; }
            }
            """);
        Write("TwCard.cs", """
            public class TwCard : ComponentBase, ITwComponent
            {
                /// <inheritdoc cref="ITwComponent.Class" />
                [Parameter] public string Class { get; set; } = "";
            }
            """);

        var card = ComponentParameterScanner.Scan(_root)["TwCard"];

        Assert.Equal("Gets or sets the CSS classes.", Assert.Single(card.Parameters).Description);
    }

    [Fact]
    public void Scan_MergesTheParametersOfEveryPartOfAPartialClass()
    {
        Write("Select/TwSelect.razor.cs", """
            public partial class TwSelect<T> : ComponentBase
            {
                [Parameter] public T Value { get; set; }
            }
            """);
        Write("Select/TwSelect.Options.cs", """
            public partial class TwSelect<T>
            {
                [Parameter] public bool Multiple { get; set; }
            }
            """);

        var select = ComponentParameterScanner.Scan(_root)["TwSelect"];

        Assert.Equal(["Multiple", "Value"], select.Parameters.Select(p => p.Name).Order());
        Assert.EndsWith("Select", select.Folder);
    }

    [Fact]
    public void Scan_SkipsAbstractNonPublicAndNonTwTypes_AndNonParameterProperties()
    {
        Write("Types.cs", """
            public abstract class TwChartBase : ComponentBase { [Parameter] public string Title { get; set; } }
            internal class TwInternal : ComponentBase { }
            public class Helper : ComponentBase { }
            public class TwChip : ComponentBase
            {
                [Parameter] public string Label { get; set; }
                [CascadingParameter] public string Cascaded { get; set; }
                public string NotAParameter { get; set; }
                [Parameter] protected string Hidden { get; set; }
            }
            """);

        var components = ComponentParameterScanner.Scan(_root);

        Assert.Equal(["TwChip"], components.Keys);
        Assert.Equal(["Label"], components["TwChip"].Parameters.Select(p => p.Name));
    }

    [Fact]
    public void Scan_IgnoresBuildOutput()
    {
        Write(Path.Combine("obj", "TwGenerated.cs"), "public class TwGenerated : ComponentBase { }");

        Assert.Empty(ComponentParameterScanner.Scan(_root));
    }
}
