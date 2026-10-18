// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using TwBlazor.Configuration.Components;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// A line between groups of items in a <see cref="TwMenu"/>. Assistive technology reads it as a separator, and the
/// arrow keys pass over it.
/// </summary>
public partial class TwMenuDivider : TwBlazorComponentBase
{
    private TwMenuTheme theme => options.Theme.Components.Require<TwMenuTheme>();

    private string dividerClasses => new ClassBuilder(theme.Divider).AddClass(Class).Build();
}
