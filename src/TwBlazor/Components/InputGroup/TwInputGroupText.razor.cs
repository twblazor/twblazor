// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// Static text in a <see cref="TwInputGroupAddon"/>, such as a protocol, a currency symbol or a unit, that is read with
/// the input of its <see cref="TwInputGroup"/>.
/// </summary>
/// <remarks>
/// The text's id is added to the input's <c>aria-describedby</c>, so a screen reader announces "https://" or "kg" when
/// the input takes focus. Set <see cref="DescribesInput"/> to <see langword="false"/> for text that adds nothing for a
/// screen reader user, or that the input's label already says.
/// </remarks>
public partial class TwInputGroupText : TwBlazorComponentBase, IDisposable
{
    private string? registeredId;
    private TwInputGroup? registeredGroup;

    private TwInputGroupTheme theme => options.Theme.Components.Require<TwInputGroupTheme>();

    [CascadingParameter] private TwInputGroupContext? inputGroup { get; set; }

    /// <summary>
    /// Gets or sets the text to show. <see cref="ChildContent"/> is shown instead when it is set.
    /// </summary>
    [Parameter] public string? Text { get; set; }

    /// <summary>
    /// Gets or sets content to show instead of <see cref="Text"/>.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets whether the text is added to the input's <c>aria-describedby</c>. Defaults to <see langword="true"/>.
    /// </summary>
    [Parameter] public bool DescribesInput { get; set; } = true;

    private string classes => new ClassBuilder(theme.Text).AddClass(Class).Build();

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        var group = DescribesInput ? inputGroup?.Group : null;
        var id = group is null ? null : Id;
        if (ReferenceEquals(group, registeredGroup) && id == registeredId)
        {
            return;
        }

        Unregister();
        if (group is not null && id is not null)
        {
            group.AddDescription(id);
            registeredGroup = group;
            registeredId = id;
        }
    }

    private void Unregister()
    {
        if (registeredGroup is not null && registeredId is not null)
        {
            registeredGroup.RemoveDescription(registeredId);
        }

        registeredGroup = null;
        registeredId = null;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Unregister();
        GC.SuppressFinalize(this);
    }
}
