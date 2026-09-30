<p align="center">
  <img src="./images/banner.svg" height="300" style="height: 300px;">
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

With years of working with Blazor, I've come accross and used a lot of component frameworks, but never one that has ever let me customise it my way without fighting against the library and its built in CSS. That's why I decided to build twblazor; every component is styled with Tailwind CSS classes instead of it's own theming layer, so customising it feels the same as customising your own markup.

Combined with Tailwind's using hot reload, you can fine-tune a button, switch, checkbox, or dropdown down to the exact color, spacing, and radius, and see the change immediately. It all lives in one typed theme file, so there's no hunting through component internals to find what to change.

twblazor is, and always will be, open source under the MIT license, free for personal and commercial projects alike. I welcome contributions, issues, and feedback from anyone using it. The project is actively maintained. New components, accessibility fixes, and documentation land regularly. I have lots of components I plan to add.

Are you using twblazor in your own project? Share it [here](https://github.com/TwBlazor/twblazor/discussions/81) - we would love to see what you're building!

## Setup

1. Install the [twblazor NuGet package](https://www.nuget.org/packages/twblazor) in your Blazor project.
```pwsh
$ dotnet add package twblazor --version 1.11.5
```
2. Head to the [Get Started guide](https://twblazor.com/get-started) for the rest of the setup - stylesheets, imports, providers, theming and dependency injection - covering both Interactive Server and WebAssembly Blazor Web Apps step by step.

## Supported Versions

Security fixes are provided for the three most recent minor release lines. twblazor targets .NET 10 only. See the [security policy](https://github.com/TwBlazor/twblazor/blob/develop/SECURITY.md) for how to report a vulnerability.

| Version  | Supported          | .NET    |
| -------- | ------------------ | ------- |
| 1.10.x+   | :white_check_mark: | .NET 10 |
| < 1.9    | :x:                | .NET 10 |

## Dependencies 

- [Tailwind CSS](https://tailwindcss.com/) - A utility-first CSS framework for styling components.
- [Bootstrap Icons](https://icons.getbootstrap.com/) - An open-source icon library used for our TwIcon component.
