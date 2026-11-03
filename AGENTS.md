# AGENTS.md

Working notes for AI coding agents (OpenCode, Claude Code, others) in this repository. Short on purpose —
add only guidance an agent would plausibly miss.

## Project

- TwBlazor: open-source (MIT) Tailwind CSS component library for Blazor (Server + WebAssembly), all projects
  targeting `net10.0`. Solution file is `TwBlazor.slnx` (XML solution, not `.sln`).
- Versions come from Git tags via MinVer (tag prefix `v`, e.g. `v1.1.0` → `1.1.0`; commits after a tag →
  `1.1.1-preview.N`). Never bump versions by hand; packages are always built in Release.

## Commands (run from repo root)

- `dotnet restore`
- `dotnet build` — CI builds Release: `dotnet build --configuration Release --no-restore`
- Tests: `dotnet test tests/TwBlazor.Tests`, `dotnet test tests/TwBlazor.ArchitectureTests` and
  `dotnet test tests/TwBlazor.Docs.Tests` (CI adds
  `--no-build --configuration Release --results-directory ./test-results` plus trx reporting).
- JS unit tests (library JS in `src/TwBlazor/wwwroot/js`): `npm test` — runs
  `vitest run --coverage --config tests/vitest.config.js`; include pattern is `tests/TwBlazor.Tests/js/**/*.test.js`.
- Docs site: `dotnet run --project docs/TwBlazor.Server` or `dotnet run --project docs/TwBlazor.WasmHost`.
- Regenerate doc code snippets: `dotnet run --project build/TwBlazor.BuildTools`. It runs automatically before
  `TwBlazor.Docs` builds, but only when one of its inputs changed; force it with `-p:ForceDocsGeneration=true`.

## Layout

- `src/TwBlazor` — published component library (Razor SDK, package id `twblazor`).
- `src/TwBlazor.Theme` — `Theme.cs` holding every Tailwind class literal used by the components (`TwBlazorTheme`).
- `docs/` — docs content (`TwBlazor.Docs`), hosts (`TwBlazor.Server`, `TwBlazor.WASM`, `TwBlazor.WasmHost`), docfx config.
- `build/` — `TwBlazor.BuildTools` + `TwBlazor.Docs.Compiler` (code snippet extraction).
- `tests/` — `TwBlazor.Tests` (bUnit/xUnit), `TwBlazor.ArchitectureTests` (reflection rules over the public API),
  `TwBlazor.Docs.Tests`, `TwBlazor.A11yTests` (axe-core + Playwright).
- `ARCHITECTURE.md` has the project dependency, runtime and docs-build diagrams. Projects in `build/` must never be
  referenced from `src/` or the docs hosts (it would put Roslyn in the WebAssembly download).

## Gotchas

- The library ships no prebuilt CSS: Tailwind cannot compile from a DLL, so component classes are expressed as
  literals in `src/TwBlazor.Theme/Theme.cs`. Consuming apps compile Tailwind from that file; the docs site
  builds its CSS via the Tailwind CLI (`.\watch-tailwind.ps1` or
  `npx @tailwindcss/cli -i ./docs/TwBlazor.Docs/wwwroot/css/input.css -o ./docs/TwBlazor.Docs/wwwroot/css/output.css`).
  A manual build may require a clean rebuild before the CSS is picked up.
- Keep the Tailwind watch step (`.\watch-tailwind.ps1`, which runs the Tailwind CLI in watch mode against
  `docs/TwBlazor.Docs/wwwroot/css/input.css`) **active during local UI component adjustments**. If the watcher is
  not running, component style changes will not be picked up and missing styles will not surface as errors.
- `components.json` at the repo root is the single source of truth for docs navigation and accessibility-test
  routes (embedded resource; no `.csproj` edit needed when adding a component). Adding a component means touching:
  the component, its docs page, its tests, and a `components.json` entry — the docs page is also what the a11y
  tests scan.
- Doc snippets are extracted from `<CodeExample Name="...">` razor tags or `#region CodeExample <Name>` C#
  regions into `docs/TwBlazor.Docs/Generated/CodeExamples.cs`. The generator runs automatically before docs
  builds; duplicate example names only warn.
- C# style is enforced via `.editorconfig` + `Directory.Build.props` (`EnforceCodeStyleInBuild`); builds fail
  on violations (e.g. IDE1006).

## Tests / quality

- Before submitting: build, run unit + docs tests, add tests for logic changes; the PR checks require the
  header check, build, tests, SonarCloud scan and vulnerability scan to pass.
- Copyright header check (library C# files under `src/TwBlazor/`): `pwsh ./scripts/validate-headers.ps1`
  (add `-Fix`); runs in the PR check to `develop`.
- Accessibility tests (opt-in via `[x] Run the accessibility tests` in the PR description; run after the unit
  and docs tests; also run on pushes to `main`/`develop`): build first, install chromium once, then
  `dotnet test tests/TwBlazor.A11yTests`. Run them when changing how a component renders, its ARIA attributes,
  keyboard handling, or colors.

## Workflow

- Branch naming: `feature/*`, `bug/*`, `chore/*` from `develop`. PRs target `develop`; `develop` → `main` releases only.
  No direct PRs from feature/bug/chore branches into `main`.
- PR title: Conventional Commits `<type>(<scope>): <subject>.`; subject must be 10–100 characters and end with
  a full stop. Types include feat/fix/docs/style/refactor/perf/test/build/ci/chore/revert; enforced by
  `scripts/validate-pr.ps1`.
- PR body must include Changes, Testing and Checklist sections (with the contributing-guidelines item ticked),
  or the Lint PR check fails.
- Releases: pick exactly one bump label (`release:patch`, `release:minor`, `release:major`) on the
  develop→main PR; merging publishes the stable packages.

## References

- `ARCHITECTURE.md`, `CONTRIBUTING.md`, `README.md`, `docs/CODE_SNIPPETS.md`, `.github/workflows/`.
