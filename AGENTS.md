# Agent instructions for AspNet.AssetManager

These notes capture conventions that aren't obvious from the source and that have tripped past iterations. Read them before making changes.

## Repository layout

- `src/AspNet.AssetManager/` — the library. Multi-targets net8.0/net9.0/net10.0 (see `Directory.Build.props` / csproj).
- `src/AspNet.AssetManager.Tests/` — xUnit + Moq + AwesomeAssertions.
- `src/AspNet.AssetManager.Demo/` — a Webpack-based demo project. Keep it building; it doesn't exercise the Vite manifest path.
- `build/` — Nuke build (`Build.cs`). The Nuke targets `Compile`, `Test`, `Package`, `PushNuGet`, `PushMyGet`, `UploadCodecov`.
- Branch model: trunk-based on `main`. Tags `v*` trigger NuGet publish via `.github/workflows/pipeline.yml`.

## CI build command (must pass before pushing)

The pipeline runs **Release** configuration with `TreatWarningsAsErrors=true`. A clean `dotnet build` in Debug is *not* sufficient — analyzer rules like SA1507 (no consecutive blank lines), CA1716 (parameter names matching VB keywords), and the rest of StyleCop fail the build only when promoted to errors.

Always reproduce CI locally before pushing:

```bash
dotnet build --configuration Release /p:TreatWarningsAsErrors=true
dotnet test src/AspNet.AssetManager.Tests --configuration Release --no-build --nologo
```

If a rule is genuinely undesirable (e.g. CA1716 firing for the `module` parameter that intentionally matches an HTML attribute), suppress it in `.globalconfig` rather than working around it at the call site.

Use `dotnet test --nologo` to get terse output; multi-target test runs print three result lines (one per TFM).

## Code conventions enforced by analyzers

- StyleCop (`StyleCop.Analyzers`) with custom settings in `stylecop.json` and `.globalconfig`. Already-suppressed: SA1101, SA1204, SA1309, SA1600, CA1716.
- `AnalysisMode = AllEnabledByDefault` (see `Directory.Build.props`). New code must comply with the full CA ruleset.
- Copyright header on every `.cs` file, format:
  ```csharp
  // <copyright file="X.cs" company="Baune8D">
  // Copyright (c) Baune8D. All rights reserved.
  // Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
  // </copyright>
  ```
- Nullable reference types enabled. Use `?` for nullable refs, not `[CanBeNull]`.
- xUnit theory data prefers `string[]`, primitives, and consts — `JsonDocument` and other reference types can't be inlined.

## Manifest model (load-bearing context)

The library supports two `ManifestType`s:

- `KeyValue` — a flat `{ "Bundle.js": "Bundle.min.js" }` map, used with Webpack.
- `Vite` — Vite's standard manifest with `name`/`src`/`file`/`imports`/`css` per entry. For CSS the closure must be walked (an entry's stylesheets can live on chunks it imports), per [Vite's backend integration spec](https://vite.dev/guide/backend-integration.html). `ManifestService.GetCssFromManifestAsync` implements that walk: DFS through `imports`, dependency-first, dedup by file path, cycle-safe.

For Vite dev, the manifest comes from a custom plugin in [`aspnet-buildtools`](https://github.com/Baune8D/aspnet-buildtools) (`src/vite-dev-manifest-plugin.ts`) that emits `{ name, src }` per entry — no `imports`/`css`. The CSS walk gracefully returns empty there because Vite's dev server injects CSS through JS imports at runtime; emitting `<link>` tags in dev would double-load and 404.

`GetFromManifestAsync` is now JS-only for Vite. CSS callers must use `GetCssFromManifestAsync`.

## Tag helpers

Three are exposed: `<script-bundle />`, `<link-bundle />`, `<style-bundle />`. Naming: `Name` / `Fallback` (and `Module` for script). They resolve through `IAssetService`:

- `LinkBundleTagHelper` may emit **multiple** `<link>` elements when the CSS closure has more than one file. The helper sets `output.TagName = null` and writes joined HTML.
- `ScriptBundleTagHelper` builds its tag inline by setting `output.Attributes` (not via `TagBuilder`). When changing script tag attributes, update *both* `ScriptBundleTagHelper` and `TagBuilder.BuildScriptTag` (the latter is used by `IAssetService.GetScriptTagAsync` for programmatic rendering).
- The dev/prod `crossorigin` and `type="module"` attributes have asymmetric logic — see the in-code comments. `type="module"` defaults to `true` for Vite, can be overridden via `Module`/`module` parameter on both the helper and `TagBuilder.BuildScriptTag`.

## Tests

Use the fixtures in `src/AspNet.AssetManager.Tests/Data/`. They mock `IManifestService` (JS via `SetupGetFromManifest`, CSS via `SetupGetCssFromManifest`) and `ITagBuilder`. New CSS-side fixtures should call `SetupGetCssFromManifest`; JS-side fixtures call `SetupGetFromManifest`. `GetBundlePathFixture` calls both since it tests CSS and JS paths from one entry point.

For raw `ManifestService` behavior (closure walking, dedup, cycles), write the manifest JSON inline in the test rather than reusing the shared fixtures — those are built around mocked manifest services, not real JSON.

## Versioning

GitVersion is configured in `GitVersion.yml`. Commit messages drive the bump via [Conventional Commits](https://www.conventionalcommits.org/):

- `feat:` → minor
- `fix:` → patch
- `feat!:` / `fix!:` / `BREAKING CHANGE:` footer → major

To cut a release: merge the PR, tag the merge commit `v<version>` on `main`, push the tag — the pipeline publishes to NuGet. Then create a GitHub release with `gh release create <tag> --title <tag> --notes "..."`.

## Things that have bitten past iterations

- `TestValues.JsonResultBundleCss` is literally `"Bundle.min.js"` (same as `JsonResultBundleJs`) — that's a long-standing test-fixture quirk, not a typo to "fix". Tests that look like they're verifying CSS lookups but expect a `.js` value are doing this on purpose.
- `BuildLinkTag` returns the tag with a trailing space before `/>` (e.g. `<link href="..." rel="stylesheet" crossorigin="anonymous" />`). Don't reformat that template — tests assert on the exact string in places.
- Pipeline only publishes NuGet on tag pushes (`GitHubActions.RefType == "tag"`); main-branch pushes go to MyGet only. Don't expect a NuGet package after a normal merge.
- The pipeline checks out with `fetch-depth: 0` so GitVersion can see history; locally GitVersion needs the same to compute pre-release labels correctly.
