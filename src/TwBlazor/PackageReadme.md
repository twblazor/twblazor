[![twblazor: Tailwind CSS components for Blazor](https://raw.githubusercontent.com/twblazor/twblazor/main/images/readme-banner.png)](https://twblazor.com/)

[![Quality gate status](https://sonarcloud.io/api/project_badges/measure?project=twblazor_twblazor&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=twblazor_twblazor)
[![Bugs](https://sonarcloud.io/api/project_badges/measure?project=twblazor_twblazor&metric=bugs)](https://sonarcloud.io/summary/new_code?id=twblazor_twblazor)
[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=twblazor_twblazor&metric=reliability_rating)](https://sonarcloud.io/summary/new_code?id=twblazor_twblazor)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=twblazor_twblazor&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=twblazor_twblazor)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=twblazor_twblazor&metric=coverage)](https://sonarcloud.io/summary/new_code?id=twblazor_twblazor)
[![Duplicated Lines (%)](https://sonarcloud.io/api/project_badges/measure?project=twblazor_twblazor&metric=duplicated_lines_density)](https://sonarcloud.io/summary/new_code?id=twblazor_twblazor)
[![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=twblazor_twblazor&metric=ncloc)](https://sonarcloud.io/summary/new_code?id=twblazor_twblazor)
[![Technical Debt](https://sonarcloud.io/api/project_badges/measure?project=twblazor_twblazor&metric=sqale_index)](https://sonarcloud.io/summary/new_code?id=twblazor_twblazor)

[API Documentation (docfx)](https://twblazor.github.io/twblazor/) • [Component Documentation (twblazor.com)](https://twblazor.com/) • [Get Started](https://twblazor.com/get-started) • [AI Agents & Skills](https://twblazor.com/ai-agents) • [Support development 💜](https://buymeacoffee.com/twblazor)

## Setup

The fastest way to start is the project template. Install the [twblazor.templates](https://www.nuget.org/packages/twblazor.templates) .NET tool once:

```pwsh
dotnet tool install -g twblazor.templates
```

Then create a complete app with the sidebar layout. The command creates the solution, pages, layout, theme, Tailwind build and restores the packages:

**WebAssembly**

```pwsh
twblazor new MyApp -i wasm -t sidebar
```

**Auto**

```pwsh
twblazor new MyApp -i auto -t sidebar
```

**Server**

```pwsh
twblazor new MyApp -i server -t sidebar
```

`twblazor new` is the only command you need after the tool is installed. If you are adding twblazor to an existing app instead, install the [twblazor NuGet package](https://www.nuget.org/packages/twblazor) and follow the [Get Started guide](https://twblazor.com/get-started) for stylesheets, imports, providers, theming and dependency injection:

```pwsh
dotnet add package twblazor --version 3.1.2
```

Using an AI coding assistant? Point it at [twblazor.com/llms.txt](https://twblazor.com/llms.txt) so it uses the real components, setup steps and Theme builder instead of guessing.

## Dependencies

- [Tailwind CSS](https://tailwindcss.com/) - A utility-first CSS framework for styling components.
- [Bootstrap Icons](https://icons.getbootstrap.com/) - An open-source icon library used for our TwIcon component.

---

[![Support twblazor on Buy Me a Coffee](https://raw.githubusercontent.com/twblazor/twblazor/main/images/twblazor-bmc-banner.png)](https://buymeacoffee.com/twblazor)

This library is powered by Tailwind, Blazor and a dangerous amount of coffee. You can help with that last one at **[buymeacoffee.com/twblazor](https://buymeacoffee.com/twblazor)**. Thank you! ☕
