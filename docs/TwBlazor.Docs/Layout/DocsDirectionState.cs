using TwBlazor.Enums;

namespace TwBlazor.Docs.Layout;

/// <summary>
/// The direction the reader picked in the navbar, shared from the layout down to the switcher. A
/// <see langword="null"/> <see cref="Direction"/> means "follow the language".
/// </summary>
public sealed class DocsDirectionState(Func<TextDirection?, Task> setDirection)
{
    public TextDirection? Direction { get; internal set; }

    public Task SetAsync(TextDirection? direction) => setDirection(direction);
}
