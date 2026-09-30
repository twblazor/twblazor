# Contributing Guidelines

Thanks for contributing! 🎉  
Whether it's fixing bugs, improving docs, or adding features your help is appreciated.

By submitting a pull request, you agree that your contribution is licensed
under the project's MIT License (see [LICENSE.txt](LICENSE.txt)) and may be
distributed as part of the project.

---

## 📖 Index

- [Code of Conduct](#-code-of-conduct)
- [Prerequisites](#️-prerequisites)
- [Project Structure](#project-structure)
- [Top-Level Folders](#-top-level-folders)
- [Getting Started](#-getting-started)
- [CSS](#css)
- [Coding Guidelines](#-coding-guidelines)
- [Component Guidelines](#-component-guidelines-if-applicable)
- [Adding a New Component](#-adding-a-new-component)
- [Testing](#-testing)
- [Branching Strategy](#-branching-strategy)
- [Pull Requests](#-pull-requests)
- [Releasing & Versioning](#-releasing--versioning)
- [Common Mistakes](#-common-mistakes)
- [Before Large Changes](#-before-large-changes)
- [Keeping Your Branch Updated](#-keeping-your-branch-updated)
- [CI / Checks](#-ci--checks)
- [Quick Checklist](#-quick-checklist)

---

## 📋 Code of Conduct
Be respectful and constructive. See `CODE_OF_CONDUCT.md`.

---

## ⚙️ Prerequisites
- Latest stable .NET SDK (.NET 10)

---

## Project Structure

```
TwBlazor/
├── build/                        # Build-time tooling
│   ├── TwBlazor.BuildTools/      # Generates Generated/CodeExamples.cs before TwBlazor.Docs builds
│   └── TwBlazor.Docs.Compiler/   # CodeExample/#region extraction library used by TwBlazor.BuildTools
│       ├── CodeExampleExtractor.cs
│       ├── RegionSnippetExtractor.cs
│       ├── CodeGenerator.cs
│       ├── Paths.cs
│       └── SnippetTextUtils.cs
├── docs/                         # Documentation site content, hosts, and docfx (API reference) config
│   ├── templates/                # Custom docfx template
│   ├── toc.yml
│   ├── TwBlazor.Docs/            # Documentation content (shared by the Server and WASM hosts)
│   │   ├── Generated/
│   │   ├── Layout/
│   │   ├── Pages/
│   │   └── wwwroot/
│   ├── TwBlazor.Server/          # Server-side host
│   │   ├── Components/
│   │   │   ├── Layout/
│   │   │   └── Pages/
│   │   └── Program.cs
│   ├── TwBlazor.WASM/            # WASM project
│   │   └── Program.cs
│   └── TwBlazor.WasmHost/        # WASM host
│       ├── Pages/
│       │   └── _Host.cshtml
│       └── Program.cs
├── src/                          # Published library code
│   ├── TwBlazor/                 # Component library
│   │   ├── Builders/
│   │   ├── Components/
│   │   ├── Configuration/
│   │   ├── Enums/
│   │   ├── Extensions/
│   │   ├── Models/
│   │   ├── Services/
│   │   ├── Utilities/
│   │   └── wwwroot/
│   └── TwBlazor.Theme/           # Theme definitions
│       └── Theme.cs
└── tests/                        # Automated tests
    ├── vitest.config.js          # JS unit test config (see TwBlazor.Tests/js)
    ├── TwBlazor.Tests/           # Component library unit tests (bUnit + xunit + JS/vitest)
    ├── TwBlazor.Docs.Tests/      # Docs site and build tooling tests (pages, search, sitemap, XML docs)
    └── TwBlazor.A11yTests/       # Accessibility (axe-core/Playwright) tests
```

`docfx.json` itself stays at the repository root (it's picked up by `docfx docfx.json` in
[`deploy-docs.yml`](.github/workflows/deploy-docs.yml)), but its `src`/`resource` paths point into
`src/TwBlazor` and `docs/TwBlazor.Docs` above.

---

## 🧱 Top-Level Folders

- `/src` → the published component library (`TwBlazor`, `TwBlazor.Theme`)
- `/build` → build-time tooling (`TwBlazor.BuildTools`, `TwBlazor.Docs.Compiler`)
- `/docs` → documentation content, site hosts, and docfx config (`TwBlazor.Docs`, `TwBlazor.Server`, `TwBlazor.WASM`, `TwBlazor.WasmHost`, `templates/`, `toc.yml`)
- `/tests` → automated tests (`TwBlazor.Tests`, `TwBlazor.Docs.Tests`, `TwBlazor.A11yTests`, `vitest.config.js`)

See [Project Structure](#project-structure) above for the full tree.

---

## 🚀 Getting Started

Run these commands from the root of the repository.

### Run the docs site (Blazor Server)

```
dotnet run --project docs/TwBlazor.Server
```

### Run the docs site (Blazor WebAssembly)

```
dotnet run --project docs/TwBlazor.WasmHost
```

This serves the compiled `TwBlazor.Wasm` app via the `TwBlazor.WasmHost` host project.

> Tip: swap `dotnet run` for `dotnet watch run` on either command to get automatic rebuilds on file changes. Console output will show the local URL to browse to (e.g. `https://localhost:xxxx`).

### Run the tests

```
dotnet test tests/TwBlazor.Tests
dotnet test tests/TwBlazor.Docs.Tests
```

### Rebuild the doc examples

Code snippets shown on the documentation site are extracted from real source (docs pages and a few production files) by `TwBlazor.BuildTools`. See [CODE_SNIPPETS.md](docs/CODE_SNIPPETS.md) for how this works. This runs automatically before `TwBlazor.Docs` builds, but you can trigger it manually if generated snippets look stale:

```
dotnet run --project build/TwBlazor.BuildTools
```

---

## CSS

TwBlazor itself ships no pre-built stylesheet. Since Tailwind can't compile classes from a .DLL, every
class TwBlazor's components use is expressed as a literal in [`src/TwBlazor.Theme/Theme.cs`](src/TwBlazor.Theme/Theme.cs)
instead, and a consuming app builds its own Tailwind output from its own copy of that file - see the
["Get Started" guide](https://twblazor.com/get-started) for the consumer-facing side of this.

The only CSS actually built inside this repo is the documentation site's own. The easiest way is the
custom `watch-tailwind.ps1` PowerShell script in the root of the repository, which builds it and
watches for changes to the input CSS file, rebuilding as necessary:

`.\watch-tailwind.ps1`

You can also build it manually:

- `cd ./docs/TwBlazor.Docs`
- `npx @tailwindcss/cli -i ./wwwroot/css/input.css -o ./wwwroot/css/output.css --watch`

**NOTE: You may also have to run a clean and rebuild of the solution to ensure the new CSS file is picked up by the project if building manually, we advise you use `watch-tailwind.ps1` detailed above.**

---

## 🧑‍💻 Coding Guidelines

### Do:
- Write clean, readable code
- Keep components simple and focused
- Add comments for public APIs
- Follow existing patterns

### Don't:
- Put complex logic in property getters/setters
- Modify component state unpredictably
- Break existing functionality

---

## 🧩 Component Guidelines (if applicable)

- Keep parameters simple (no hidden side effects)
- Use clear naming
- Avoid directly mutating inputs-use events/callbacks instead
- Keep UI and logic separated where possible

---

## 🆕 Adding a New Component

A new component touches four places: the component itself, its docs page, its
tests, and an entry in [`components.json`](components.json). Using `TwTreeList`
as a reference:

1. **Component** - add `src/TwBlazor/Components/<Name>/Tw<Name>.razor` (and
   `.razor.cs` code-behind). Follow the [Component Guidelines](#-component-guidelines-if-applicable)
   above.
2. **Docs page** - add `docs/TwBlazor.Docs/Pages/<Name>/<Name>.razor` showing
   the component's variants/states. This is the page real users see, and is
   also what the accessibility tests scan (see below).
3. **Tests** - add `tests/TwBlazor.Tests/Components/<Name>/Tw<Name>Tests.cs`
   following the [Testing](#-testing) naming conventions below.
4. **Register it in `components.json`** - add an entry to the appropriate
   category array at the repo root:

   ```json
   { "id": "my-component-navitem", "display": "My Component", "name": "TwMyComponent", "url": "/my-component" }
   ```

   - `id` - unique DOM/nav id, conventionally `<kebab-name>-navitem`
   - `display` - label shown in the docs sidebar
   - `name` - the component's C# type name
   - `url` - route of the docs page from step 2
   - `isNew` (optional) - set `true` to show a "New" badge in the sidebar

`components.json` is the single source of truth that indexes every documented
component, and is consumed in two places:

- **Docs navigation** - [`Navigation.razor.cs`](docs/TwBlazor.Docs/Layout/Navigation.razor.cs)
  deserializes it (via an embedded resource) to build the sidebar. Category
  order and item order in the file are exactly the order rendered.
- **Accessibility tests** - [`AccessibilityRoutes.cs`](tests/TwBlazor.A11yTests/AccessibilityRoutes.cs)
  reads the same embedded resource to enumerate every component's `url` and
  runs an axe-core scan against it in both light and dark mode. Adding your
  component's route to `components.json` is what gets it covered by these
  tests automatically - no separate test wiring is needed.

Since `components.json` is embedded as a resource in `TwBlazor.Docs.csproj`,
no project file changes are required beyond editing the JSON itself.

---

## 🧪 Testing

- Add tests for any non-trivial logic
- Ensure all tests pass before submitting
- Keep tests:
  - Small
  - Independent
  - Descriptive

### Naming
- `Subject_Action_ExpectedResult` - use the full component or method name as the subject  
  **Examples:**
  - `TwDataTable_Renders_WithEmptyItems`
  - `TwDataTable_Pageable_ShowsPaginationControls`
  - `UpdateApplication_DoesNothing_WhenOptionsIsNull`

---

## 🌿 Branching Strategy

We follow a **develop-main** workflow:

- **`main`** → Production-ready code, tagged releases only
- **`develop`** → Integration branch for all features and fixes
- **`feature/*`** → New features (e.g., `feature/toast-component`)
- **`bug/*`** → Bug fixes (e.g., `bug/fix-modal-close`)

### Merge Flow

```
feature/my-feature  →  develop  →  main
     bug/my-fix     ↗          ↗
```

**Important:** 
- ✅ PRs from `feature/*` or `bug/*` → `develop`
- ✅ PRs from `develop` → `main` (releases only)
- ❌ Direct PRs to `main` from feature/bug branches are **blocked**

---

## 🔀 Pull Requests

- Keep PRs **focused on a single change**
- Fork the repo and create a **feature branch** from `develop`:
  - `feature/my-feature`
  - `bug/my-bug`
- **Always target `develop`** for feature/bug PRs
- Make sure:
  - Project builds
  - Tests pass
- Add tests when changing logic
- Link related issues (e.g. Fixes #123)
- Include screenshots/gifs for UI changes
- Avoid unrelated refactoring

### PR title

Every PR title is checked by an automated **Lint PR** check and must follow
[Conventional Commits](https://www.conventionalcommits.org/) format:

`<type>(<optional scope>): <subject>.`

- **Valid types:** `feat`, `fix`, `bug`, `docs`, `style`, `refactor`, `perf`, `test`, `build`, `ci`, `chore`, `revert`
- The subject (the part after `type:`) must be **10 to 100 characters**
- The subject must **end with a full stop**

**Examples:**
- `feat(button): add outlined variant.`
- `fix(modal): resolve close event not firing.`
- `chore: update dependencies.`

> ⚠️ PRs with titles that don't follow this format will **fail** the automated `Lint PR` check.

### PR description

Opening a PR prefills the description from
[`.github/pull_request_template.md`](.github/pull_request_template.md). The same
`Lint PR` check requires every section to actually be filled in:

- **Changes**, what does this PR change?
- **Testing**, how was it verified?
- **Checklist**, the "I have read the Contributing Guidelines and Code of
  Conduct" item must be ticked

An unedited template, a section left as just the placeholder comment, or an
unticked contributing-guidelines item will **fail** the check.

---

## 📦 Releasing & Versioning

Versions are not stored in a file: they come entirely from Git tags, read at
build time by [MinVer](https://github.com/adamralph/minver). You never need to
bump a version number by hand.

### `develop` → GitHub Packages (preview)

Every push to `develop` builds and publishes a **prerelease** NuGet package to
the [GitHub Packages feed](https://github.com/TwBlazor/twblazor/pkgs/nuget/TwBlazor)
automatically, versioned like `1.2.1-preview.4`. This happens on every merge, so
you don't need to do anything to get a preview package out.

### `develop` → `main` → NuGet.org (release)

A release is just a pull request from `develop` into `main`:

1. Open the PR as usual (see [Pull Requests](#-pull-requests) above).
2. Check the bump label. **`release:patch` is added automatically** when the PR
   is opened, so a patch release needs no action. For anything else, remove it
   and add the one you want:
   - `release:major`, for breaking changes, e.g. `1.2.3` → `2.0.0`
   - `release:minor`, for backwards compatible changes, e.g. `1.2.3` → `1.3.0`
   - `release:patch`, for fixes only, e.g. `1.2.3` → `1.2.4` (the default)

   Exactly one is required. Applying two, say `release:major` alongside
   `release:minor`, makes the version ambiguous and fails the **Verify Release
   Bump Label** check, as does removing all of them. Other labels such as `bug`
   are ignored. A bot comments on the PR with the bump currently selected.
3. Merge the PR.

Merging automatically:
- Tags the merge commit with the new version (e.g. `v1.2.0`)
- Publishes the **stable** package to both GitHub Packages and
  [NuGet.org](https://www.nuget.org/packages/twblazor)
- Creates a GitHub Release with the packed `.nupkg` attached

There is nothing further to do on `develop` afterwards. Its next build picks
up the new release tag on its own and continues previewing from there (e.g.
`1.2.1-preview.0`, then `.1`, and so on).

---

## 🚫 Common Mistakes

- Mixing multiple changes in one PR
- Skipping tests for logic changes
- Breaking existing behaviour
- Large PRs without prior discussion

---

## 💬 Before Large Changes

Open an issue first to discuss your idea.

---

## 🔄 Keeping Your Branch Updated

- Pull latest changes from the main branch regularly
- Resolve conflicts early

---

## 🤖 CI / Checks

All PRs must pass:
- Build
- Tests
- Linting, formatting and SonarCloud checks

---

## ✅ Quick Checklist

Before submitting:
- [ ] Code builds  
- [ ] Tests pass  
- [ ] Tests added 
- [ ] PR is focused  
- [ ] Issue linked
