# AspNet.AssetManager
[![Build status](https://ci.appveyor.com/api/projects/status/u369u4wt45hsw53f?svg=true)](https://ci.appveyor.com/project/Baune8D/aspnet-assetmanager)
[![codecov](https://codecov.io/gh/Baune8D/AspNet.AssetManager/branch/main/graph/badge.svg?token=M4KiXgJBnw)](https://codecov.io/gh/Baune8D/AspNet.AssetManager)
![NuGet Version](https://img.shields.io/nuget/v/AspNet.AssetManager)

Asset manager for ASP.NET Core that enables use of modern frontend tooling.

- Simple TagHelpers: `<link-bundle />`, `<style-bundle />` and `<script-bundle />`
- Automatic bundle name inference from the active view.
- Optional per-view overrides via `ViewData["Bundle"]`
- Works with a dev server (e.g., `Vite`, `Webpack`) in development, and static files in production.

See the template repository, for usage examples: [AspNet.Frontends](https://github.com/Baune8D/AspNet.Frontends)

## Install

```bash
dotnet add package AspNet.AssetManager
```

## Quick start

Register services in `Program.cs`:

```csharp
using AspNet.AssetManager;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAssetManager(builder.Configuration, builder.Environment);
```

Register tag helpers in `Views/_ViewImports.cshtml`:

```csharp
@addTagHelper *, AspNet.AssetManager
```

Use in `Views/Shared/_Layout.cshtml`:
```html
<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="utf-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1.0" />
  <title>@ViewData["Title"] - AspNet.AssetManager</title>
  <link-bundle fallback="Layout" />
</head>
<body>
  @RenderBody()
  <script-bundle defer fallback="Layout" />
  @await RenderSectionAsync("Scripts", required: false)
</body>
</html>
```

### How bundle names are chosen

- By default, the bundle name is inferred from the active view.
  - **Example**: `/Views/Home/Index.cshtml` -> `Views_Home_Index`
- You can override the bundle chosen from view inference using `ViewData["Bundle"]`.
- A bundle name can be directly provided to a tag helper using the `name` attribute.

### Development vs. production behavior

- **Development:**
  - Manifest is fetched from the dev server. If the server is not running, you will see: `Development server not started!`.
  - Tag helpers add `crossorigin="anonymous"` automatically; with `"ManifestType": "Vite"`, script tags also use `type="module"`.
  - Paths point to the dev server: `PublicDevServer` + `PublicPath`.
- **Production:**
  - Manifest is read from disk: `wwwroot` + `PublicPath` + `ManifestFile`.
  - Paths are relative to `PublicPath`, served by `UseStaticFiles()`.

## Configuration (appsettings.json)

```json
{
  "AssetManager": {
    "PublicDevServer": "http://localhost:5173",
    "InternalDevServer": "http://localhost:5173",
    "PublicPath": "/dist/",
    "ManifestFile": "assets-manifest.json",
    "ManifestType": "KeyValue"
  }
}
```

- **PublicDevServer**: required. The browser-facing URL of your dev server (e.g., Vite).
- **InternalDevServer**: optional. If your app runs behind Docker/WSL/etc., point this to where the server is reachable from the app process. Falls back to `PublicDevServer` if omitted.
- **PublicPath**: the public base path for built assets. In production this is under `wwwroot`; in development it is appended to the dev server URL. Default is `/dist/`.
- **ManifestFile**: the filename of your manifest. Default is `assets-manifest.json`.
- **ManifestType**: `KeyValue` or `Vite`. Default is `KeyValue` which matches the format used by `webpack-assets-manifest`.

## Manual rendering

If you prefer code-based rendering or need more control, inject `IAssetService`.

```csharp
@using AspNet.AssetManager
  
@inject IAssetService AssetService

@{
    var bundle = ViewData.GetBundleName() ?? Html.GetBundleName();
}

/* Base paths */
@AssetService.WebPath

/* Resolve paths */
@await AssetService.GetBundlePathAsync("SomeBundle.js")
@await AssetService.GetBundlePathAsync("SomeBundle", FileType.JS)

/* Script tags */
@await AssetService.GetScriptTagAsync("SomeBundle")
@await AssetService.GetScriptTagAsync("SomeBundle", ScriptLoad.Async)
@await AssetService.GetScriptTagAsync("SomeBundle", "FallbackBundle", ScriptLoad.Async)

/* Link tags */
@await AssetService.GetLinkTagAsync("SomeBundle")

/* Style tags */
@await AssetService.GetStyleTagAsync("SomeBundle")
```
