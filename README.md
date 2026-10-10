<p align="center">
  <img src="./images/banner.svg" alt="twblazor" height="200" style="height: 200px;">
</p>
<p align="center">

[![Quality gate status](https://sonarcloud.io/api/project_badges/measure?project=twblazor_twblazor&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=twblazor_twblazor)
[![Bugs](https://sonarcloud.io/api/project_badges/measure?project=twblazor_twblazor&metric=bugs)](https://sonarcloud.io/summary/new_code?id=twblazor_twblazor)
[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=twblazor_twblazor&metric=reliability_rating)](https://sonarcloud.io/summary/new_code?id=twblazor_twblazor)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=twblazor_twblazor&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=twblazor_twblazor)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=twblazor_twblazor&metric=coverage)](https://sonarcloud.io/summary/new_code?id=twblazor_twblazor)
[![Duplicated Lines (%)](https://sonarcloud.io/api/project_badges/measure?project=twblazor_twblazor&metric=duplicated_lines_density)](https://sonarcloud.io/summary/new_code?id=twblazor_twblazor)
[![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=twblazor_twblazor&metric=ncloc)](https://sonarcloud.io/summary/new_code?id=twblazor_twblazor)
[![Technical Debt](https://sonarcloud.io/api/project_badges/measure?project=twblazor_twblazor&metric=sqale_index)](https://sonarcloud.io/summary/new_code?id=twblazor_twblazor)
[![License: MIT](https://img.shields.io/badge/license-MIT-9810fa)](https://github.com/TwBlazor/twblazor/blob/develop/LICENSE.txt)
[![Stars](https://img.shields.io/github/stars/TwBlazor/twblazor?style=flat&color=9810fa)](https://github.com/TwBlazor/twblazor/stargazers)
[![Contributors](https://img.shields.io/github/contributors/TwBlazor/twblazor?color=9810fa)](https://github.com/TwBlazor/twblazor/graphs/contributors)
[![Discussions](https://img.shields.io/github/discussions/TwBlazor/twblazor?color=9810fa)](https://github.com/TwBlazor/twblazor/discussions)
[![NuGet Version](https://img.shields.io/nuget/v/twblazor?color=ec4899)](https://www.nuget.org/packages/twblazor)
[![NuGet Downloads](https://img.shields.io/nuget/dt/twblazor?color=ec4899)](https://www.nuget.org/packages/twblazor)

</p>

![Alt](https://repobeats.axiom.co/api/embed/e0dc678b816b4fc2767b56d42291540a4d63beb0.svg "Repobeats analytics image")

<p align="center">
    <a href="https://twblazor.github.io/twblazor/" target="_blank">API Documentation (docfx)</a> &bullet; <a href="https://twblazor.com/" target="_blank">Component Documentation (twblazor.com)</a> &bullet; <a href="https://twblazor.com/get-started" target="_blank">Get Started</a>
</p>

## Why twblazor?

Most Blazor component libraries make you fight their built-in CSS the moment you want something to look different. We built twblazor so customising a component feels like customising your own markup: every component is styled with plain Tailwind CSS classes, all gathered in one typed theme file.

Change a color, spacing or radius, and with Tailwind + hot reload you see it straight away. There is no digging through component internals to find what to change.

twblazor is open source under the MIT license, free for personal and commercial projects. It is actively maintained, with new components, accessibility fixes and documentation landing regularly, and we welcome contributions, issues and feedback from everyone using it.

Using twblazor in your own project? Share it [here](https://github.com/TwBlazor/twblazor/discussions/81). We would love to see what you're building!

## Setup

1. Install the [twblazor NuGet package](https://www.nuget.org/packages/twblazor) in your Blazor project.
```pwsh
$ dotnet add package twblazor --version 1.14.2
```
2. Head to the [Get Started guide](https://twblazor.com/get-started) for the rest of the setup - stylesheets, imports, providers, theming and dependency injection - covering both Interactive Server and WebAssembly Blazor Web Apps step by step.

## Class merging

When a component builds its `class` attribute, conflicting Tailwind utilities are merged so the last one wins. A `Class="px-2"` you pass to a component replaces the theme's `px-4` instead of both reaching the element. It is built into twblazor, so there is no extra package, and it needs no setup. Classes that are not Tailwind utilities are always kept. It follows the rules of tailwind-merge for Tailwind CSS v4, including variants, arbitrary values, the important modifier and prefixed classes such as `tw:px-4`.

To tune it, set `ClassMerge` on the options you pass to `AddTwBlazor`:

```csharp
builder.Services.AddTwBlazor(options =>
{
    // Turn merging off entirely.
    // options.ClassMerge.Enabled = false;

    // Teach it your own utilities: of btn, btn-sm, btn-lg only the last one is kept.
    options.ClassMerge.Groups.Add(new TwClassGroup("btn-size", ["btn"]));

    // If your Tailwind build uses prefix(tw), only merge classes written as tw:px-4.
    // options.ClassMerge.Prefix = "tw";
}, Theme.CreateTheme);
```

See the [Class Merge docs](https://twblazor.com/class-merge) for every rule and option.

## Supported Versions

| Version      | Supported          | .NET    |
| ------------ | ------------------ | ------- |
| 1.9 - 1.10+  | :white_check_mark: | .NET 10 |
| < 1.9        | :x:                | .NET 10 |

## Dependencies 

- [Tailwind CSS](https://tailwindcss.com/) - A utility-first CSS framework for styling components.
- [Bootstrap Icons](https://icons.getbootstrap.com/) - An open-source icon library used for our TwIcon component.
- [tailwind-merge](https://github.com/dcastil/tailwind-merge) - The rules our built-in class merging follows, ported to C# so there is no npm package to install.
