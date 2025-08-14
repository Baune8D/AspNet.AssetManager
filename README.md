# AspNet.AssetManager
[![Build status](https://ci.appveyor.com/api/projects/status/u369u4wt45hsw53f?svg=true)](https://ci.appveyor.com/project/Baune8D/aspnet-assetmanager)
[![codecov](https://codecov.io/gh/Baune8D/AspNet.AssetManager/branch/main/graph/badge.svg?token=M4KiXgJBnw)](https://codecov.io/gh/Baune8D/AspNet.AssetManager)
![NuGet Version](https://img.shields.io/nuget/v/AspNet.AssetManager)

See the template repository, for usage examples: [AspNet.Frontends](https://github.com/Baune8D/AspNet.Frontends)

Initialize `AspNet.AssetManager` in `Program.cs`:
```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAssetManager(builder.Configuration, builder.Environment);
```

Use extensions to get the bundle name:
```csharp
var bundle = ViewData.GetBundleName() // Gets the bundle name from ViewData["Bundle"]
var bundle = Html.GetBundleName() // Generates the bundle name from the view context
```

Recommended use in eg. `_Layout.cshtml`:
```csharp
var bundle = ViewData.GetBundleName() ?? Html.GetBundleName();
// Generates the bundle name from the view context if not overridin in ViewData["Bundle"]
```

Use `AssetService` to get assets:
```csharp
@inject IAssetService AssetService

@await AssetService.WebPath
// Returns: /web/path

@await AssetService.GetBundlePathAsync("SomeBundle.js")
// Returns: /web/path/SomeBundle.js

@await AssetService.GetScriptTagAsync("SomeBundle")
// Generates: <script src="/web/path/SomeBundle.js"></script>

@await AssetService.GetLinkTagAsync("SomeBundle")
// Generates: <link href="/web/path/SomeBundle.css" rel="stylesheet" />

@await AssetService.GetStyleTagAsync("SomeBundle")
// Generates: <style>Inlined CSS</style
```

Overloads exist on `GetBundlePathAsync` in case no extension is applied for the bundle name:
```csharp
@await AssetService.GetBundlePathAsync("SomeBundle", FileType.JS)
// Returns: /web/path/SomeBundle.js
```

Overloads exist on `GetScriptTagAsync` to change the load behavior to e.g. `async` and/or `defer`:
```csharp
@await AssetService.GetScriptTagAsync("SomeBundle", ScriptLoad.Async)
// Generates: <script src="/web/path/SomeBundle.js" async></script>
```

A fallback bundle can be specified on: `GetScriptTagAsync`, `GetLinkTagAsync`, `GetStyleTagAsync`:
```csharp
@await AssetService.GetScriptTagAsync("SomeBundle", fallback: "FallbackBundle")
// Generates: <script src="/web/path/SomeBundle.js"></script>
// Or if 'SomeBundle' does not exist: <script src="/web/path/FallbackBundle.js"></script>
```

## Configuration: `appsettings.json`
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

* **`PublicDevServer` is the only required setting.**
* `ManifestType` can be either `KeyValue` or `Vite`.

## Example: `_Layout.cshtml`

```razor
@using AspNet.AssetManager

@inject IAssetService AssetService

@{
    var bundle = ViewData.GetBundleName() ?? Html.GetBundleName();
}

<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>@ViewData["Title"] - AspNet.AssetManager</title>
    @await AssetService.GetLinkTagAsync(bundle, fallback: "Layout");
</head>
<body>
    @RenderBody()
    @await AssetService.GetScriptTagAsync(bundle, fallback: "Layout", ScriptLoad.Defer);
    @await RenderSectionAsync("Scripts", required: false)
</body>
</html>
```
