using Microsoft.AspNetCore.Components;
using IconEnum = TwBlazor.Enums.Icon;

namespace TwBlazor.Docs.Layout;

/// <summary>
/// The title bar of a code window on the landing page, styled like a Windows 11 window: an icon and file name
/// on the left and the minimize, maximize and close buttons on the right.
/// </summary>
public partial class CodeWindowBar
{
    /// <summary>
    /// Gets or sets the file name shown in the bar.
    /// </summary>
    [Parameter, EditorRequired]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the icon shown before the file name.
    /// </summary>
    [Parameter]
    public IconEnum Icon { get; set; } = IconEnum.File_Earmark_Code;
}
