using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TwBlazor.Docs.Layout;

namespace TwBlazor.Docs.Pages;

/// <summary>
/// The showcase's workspace form. A component of its own so dragging its colour picker re-renders this card only.
/// </summary>
public partial class ShowcaseWorkspaceCard
{
    /// <summary>
    /// Gets or sets the showcase's example colour. The workspace form's colour picker follows it each time it
    /// changes, and can still be set to anything in between.
    /// </summary>
    [Parameter] public string BrandColour { get; set; } = "#9333ea";

    private string? appliedBrandColour;

    // The workspace form.
    private static readonly IReadOnlyList<string> _logoTypes = [".png", ".svg"];
    private string workspaceName = "Atlas Labs";
    private string workspaceColour = "#9333ea";
    private List<IBrowserFile>? workspaceLogo;
    private bool workspaceCreated;

    private void CreateWorkspace() => workspaceCreated = true;

    protected override void OnParametersSet()
    {
        if (BrandColour != appliedBrandColour)
        {
            appliedBrandColour = BrandColour;
            workspaceColour = BrandColour;
        }
    }
}
