# AspNet.AssetManager

[![Build](https://github.com/Baune8D/AspNet.AssetManager/actions/workflows/pipeline.yml/badge.svg?branch=main)](https://github.com/Baune8D/AspNet.AssetManager/actions/workflows/pipeline.yml)
[![codecov](https://codecov.io/gh/Baune8D/AspNet.AssetManager/branch/main/graph/badge.svg?token=M4KiXgJBnw)](https://codecov.io/gh/Baune8D/AspNet.AssetManager)
[![NuGet Version](https://img.shields.io/nuget/v/AspNet.AssetManager)](https://www.nuget.org/packages/AspNet.AssetManager)
[![NuGet Downloads](https://img.shields.io/nuget/dt/AspNet.AssetManager)](https://www.nuget.org/packages/AspNet.AssetManager)
[![License: MIT](https://img.shields.io/github/license/Baune8D/AspNet.AssetManager)](https://github.com/Baune8D/AspNet.AssetManager/blob/main/LICENSE)

**Use Vite or Webpack with ASP.NET Core MVC and Razor Pages.** AspNet.AssetManager reads your bundler's manifest and renders the correct `<script>` and `<link>` tags: from the dev server (with hot reload) in development, and from hashed static files in production.

```cshtml
<link-bundle fallback="Layout" />
<script-bundle defer fallback="Layout" />
```

## Features

- **Tag helpers:** `<link-bundle />`, `<script-bundle />` and `<style-bundle />`.
- **Per-view bundles by convention:** the bundle name is inferred from the view being rendered, e.g. `Views/Home/Index.cshtml` → `Views_Home_Index`.
- **Fallbacks:** if a view has no bundle of its own, a shared one (e.g. `Layout`) is used.
- **Vite and Webpack:** supports Vite manifests and key/value manifests such as `webpack-assets-manifest`.
- **Same markup in every environment:** assets come from the dev server in development and from `wwwroot` in production.
- **Full Vite CSS:** `<link-bundle />` collects stylesheets from imported shared chunks, in the same order Vite applies them.
- **Targets .NET 8, 9 and 10.**

## Getting started

The quickest way to start is a project template from [AspNet.Frontend.Templates](https://github.com/Baune8D/AspNet.Frontend.Templates). It sets up the .NET side and the bundler config for you:

```bash
dotnet new install AspNet.Frontend.Templates
dotnet new mvcvite   # also: mvcvitets, mvcwebpack(ts), razorvite(ts), razorwebpack(ts)
```

To add AspNet.AssetManager to an existing project, follow the steps below.

### 1. Install

```bash
dotnet add package AspNet.AssetManager
```

### 2. Register services

`Program.cs`:

```csharp
using AspNet.AssetManager;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddAssetManager(builder.Configuration, builder.Environment);

var app = builder.Build();

app.UseStaticFiles();
// ...
```

### 3. Import the tag helpers

`Views/_ViewImports.cshtml`:

```cshtml
@addTagHelper *, AspNet.AssetManager
```

### 4. Render bundles in your layout

`Views/Shared/_Layout.cshtml`:

```cshtml
<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="utf-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1.0" />
  <title>@ViewData["Title"]</title>
  <link-bundle fallback="Layout" />
</head>
<body>
  @RenderBody()
  <script-bundle defer fallback="Layout" />
  @await RenderSectionAsync("Scripts", required: false)
</body>
</html>
```

Each page now loads its own bundle (`Views_Home_Index`, `Views_Account_Login`, ...) if one exists, and the `Layout` bundle if not.

### 5. Configure your bundler

#### Vite

`appsettings.json`:

```json
{
  "AssetManager": {
    "PublicDevServer": "http://localhost:5173",
    "ManifestType": "Vite"
  }
}
```

`vite.config.js`. [`aspnet-buildtools`](https://www.npmjs.com/package/aspnet-buildtools) finds the entry points for your views and serves a manifest from the Vite dev server:

```js
import { viteConfig } from 'aspnet-buildtools';
import { defineConfig } from 'vite';

export default defineConfig({
  ...viteConfig,
  build: {
    ...viteConfig.build,
    assetsDir: '',
    manifest: 'assets-manifest.json', // must match AssetManager:ManifestFile
    outDir: 'wwwroot/dist',           // must match wwwroot + AssetManager:PublicPath
  },
  server: {
    cors: true,
  },
});
```

For hot module replacement, add the Vite client to your layout in development:

```cshtml
<environment include="Development">
  <script src="http://localhost:5173/@@vite/client" type="module"></script>
</environment>
```

#### Webpack

`appsettings.json`:

```json
{
  "AssetManager": {
    "PublicDevServer": "http://localhost:9000"
  }
}
```

`webpack.config.js`, using [`webpack-assets-manifest`](https://www.npmjs.com/package/webpack-assets-manifest) and [`aspnet-buildtools`](https://www.npmjs.com/package/aspnet-buildtools):

```js
import path from 'node:path';
import { webpackConfig } from 'aspnet-buildtools';
import { WebpackAssetsManifest } from 'webpack-assets-manifest';

export default {
  ...webpackConfig,
  output: {
    path: path.resolve(import.meta.dirname, 'wwwroot/dist'),
    filename: '[name]-[contenthash].js',
    publicPath: '/dist/',
  },
  plugins: [new WebpackAssetsManifest()],
  devServer: {
    headers: { 'Access-Control-Allow-Origin': '*' },
    port: 9000,
  },
};
```

## Tag helpers

| Tag helper | Renders | Attributes |
| --- | --- | --- |
| `<link-bundle />` | One or more `<link rel="stylesheet">` tags | `name`, `fallback` |
| `<script-bundle />` | A `<script src="...">` tag | `name`, `fallback`, `async`, `defer`, `module` |
| `<style-bundle />` | An inline `<style>` tag with the bundle's CSS (useful for critical CSS) | `name`, `fallback` |

- `name`: bundle to render. If omitted, it is resolved as described in [Bundle names](#bundle-names).
- `fallback`: bundle to render if `name` cannot be found in the manifest.
- `module`: adds `type="module"`. Defaults to `true` for Vite manifests and `false` otherwise.

Nothing is rendered if neither bundle exists, so it is safe to put these tags in a shared layout.

## Bundle names

The bundle for a tag helper is chosen in this order:

1. The `name` attribute, e.g. `<script-bundle name="Checkout" />`.
2. `ViewData["Bundle"]`, set in a view or controller. A value starting with `/` is converted the same way as view paths: `"/Some/Bundle"` → `Some_Bundle`.
3. The path of the view being rendered:

| View | Bundle name |
| --- | --- |
| `Views/Home/Index.cshtml` | `Views_Home_Index` |
| `Pages/Account/Login.cshtml` | `Pages_Account_Login` |
| `Areas/Admin/Views/Users/Edit.cshtml` | `Areas_Admin_Views_Users_Edit` |

If that bundle is not in the manifest, the `fallback` bundle is used.

[`aspnet-buildtools`](https://www.npmjs.com/package/aspnet-buildtools) produces these names: a `Views/Home/Index.cshtml.js` (or `.ts`) next to a view becomes the `Views_Home_Index` entry, and any `*.bundle.js` file becomes a named bundle (`Layout.bundle.js` → `Layout`).

## Configuration

All settings live in the `AssetManager` section of `appsettings.json`:

| Setting | Default | Description |
| --- | --- | --- |
| `PublicDevServer` | — | **Required in development.** The dev server URL the browser uses, e.g. `http://localhost:5173`. |
| `InternalDevServer` | `PublicDevServer` | The dev server URL the ASP.NET Core app uses to fetch the manifest. Set this when the app runs in Docker, WSL, etc. and reaches the dev server at a different address. |
| `PublicPath` | `/dist/` | Base path for built assets, under `wwwroot` in production. |
| `ManifestFile` | `assets-manifest.json` | File name of the manifest, relative to `PublicPath`. |
| `ManifestType` | `KeyValue` | `KeyValue` (flat `{ "name.js": "name-hash.js" }`, e.g. `webpack-assets-manifest`) or `Vite`. |

### Development vs. production

The app counts as being in development when `IWebHostEnvironment.IsDevelopment()` returns true.

| | Development | Production |
| --- | --- | --- |
| Manifest | Fetched from the dev server on each request | Read from `wwwroot` + `PublicPath` + `ManifestFile` once, then cached |
| Asset URLs | `PublicDevServer` + `PublicPath` (Vite: `PublicDevServer` + `/`) | `PublicPath`, served by `UseStaticFiles()` |
| Extra attributes | `crossorigin="anonymous"` | — |

If the dev server is not running, a `DevServerException` ("Development server not started!") is thrown.

## Rendering from code

For full control, inject `IAssetService`:

```cshtml
@using AspNet.AssetManager
@inject IAssetService AssetService

@{
    var bundle = ViewData.GetBundleName() ?? Html.GetBundleName();
}

@* Tags *@
@await AssetService.GetScriptTagAsync(bundle, "Layout", ScriptLoad.Defer)
@await AssetService.GetLinkTagAsync(bundle, "Layout")
@await AssetService.GetStyleTagAsync("Critical")

@* Paths *@
@AssetService.WebPath
@await AssetService.GetBundlePathAsync("SomeBundle.js")
@await AssetService.GetBundlePathAsync("SomeBundle", FileType.CSS)
```

`ScriptLoad` can be `Normal`, `Async`, `Defer` or `AsyncDefer`.

## Related projects

- [AspNet.Frontend.Templates](https://github.com/Baune8D/AspNet.Frontend.Templates): `dotnet new` templates and example projects for Vite and Webpack.
- [aspnet-buildtools](https://github.com/Baune8D/aspnet-buildtools): npm package that generates bundler entry points and aliases from your Razor views.

## Building from source

The build uses [NUKE](https://nuke.build/):

```bash
./build.sh Test        # Windows: .\build.cmd Test
```

`src/AspNet.AssetManager.Demo` is a small Webpack-based MVC app for trying changes locally (`npm install && npm start`, then `dotnet run`).

## License

[MIT](https://github.com/Baune8D/AspNet.AssetManager/blob/main/LICENSE)
