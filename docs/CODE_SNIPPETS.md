# How Code Snippets Are Generated

The code samples shown on the documentation site aren't hand-copied, they're
extracted directly from real, compiled source (docs pages and a handful of
actual production files), so what you see on the docs site is exactly the
code that renders or runs. This avoids the usual problem of a pasted example
silently drifting out of sync with the component it's demonstrating.

Extraction happens in two ways, depending on where the source snippet lives.

## 1. `<CodeExample>` tags (Razor markup)

Wrap a block of markup in a docs page with a `<CodeExample Name="...">` tag:

```razor
<CodeExample Name="ButtonBasic">
    <TwButton Text="Click me" />
</CodeExample>
```

The markup between the tags is extracted verbatim, it's the exact markup
that renders live on the page, not a separate copy. Optionally add
`CSharpName="..."` to also show an accompanying C# snippet pulled from a
`#region` block (see below) alongside the Razor markup.

## 2. `#region CodeExample <Name>` blocks (C#)

Wrap any block of real C# in a `.cs` file (or inside a Razor `@code` block)
with a named region:

```csharp
#region CodeExample GetStartedTheme
public static TwBlazorTheme CreateDefaultTheme()
{
    ...
}
#endregion
```

This can extract from any real source file, not just docs pages, for
example `src/TwBlazor.Theme/Theme.cs` and `docs/TwBlazor.Server/Program.cs` are
scanned directly, so the "example" shown for configuring a theme is the
actual default theme shipped with the library.

Outside a `@code` block, a bare `#region` isn't valid Razor markup, so wrap
the markers in a Razor comment instead:

```razor
@* #region CodeExample MyExample *@
<div>...</div>
@* #endregion *@
```

Regions can nest (e.g. a smaller named example reused inside a larger one); 
the inner markers are stripped from the outer snippet's text but the inner
region still gets extracted as its own entry too.

## How it's built

`TwBlazor.BuildTools` scans `docs/TwBlazor.Docs/Pages`, `docs/TwBlazor.Docs/Layout`,
`src/TwBlazor.Theme/Theme.cs`, and `docs/TwBlazor.Server/Program.cs` for both kinds of
markers and generates `docs/TwBlazor.Docs/Generated/CodeExamples.cs` - a static
`CodeExamples.Get("Name")` lookup. Docs pages call that (directly, or via the
`<CodeExample Name="...">` layout component) to render the snippet through
`<ExampleCodeBlock>` / `<TwCodeBlock>`.

This runs automatically before `TwBlazor.Docs` builds. If a generated
snippet looks stale, regenerate it manually from the repository root:

```
dotnet run --project build/TwBlazor.BuildTools
```

Every `Name` must be unique across the whole docs site, regardless of which
of the two mechanisms defined it - a duplicate name prints a `WARNING` during
generation rather than failing the build, so keep an eye on build output
when adding a new example.
