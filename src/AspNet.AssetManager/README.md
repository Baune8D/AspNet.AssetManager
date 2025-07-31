# AspNet.AssetManager
[![Build status](https://ci.appveyor.com/api/projects/status/u369u4wt45hsw53f?svg=true)](https://ci.appveyor.com/project/Baune8D/aspnet-assetmanager)
[![codecov](https://codecov.io/gh/Baune8D/AspNet.AssetManager/branch/main/graph/badge.svg?token=M4KiXgJBnw)](https://codecov.io/gh/Baune8D/AspNet.AssetManager)
[![NuGet Badge](https://buildstats.info/nuget/AspNet.AssetManager)](https://www.nuget.org/packages/AspNet.AssetManager)

See the template repository, for example, of usage: [AspNet.Frontends](https://github.com/Baune8D/AspNet.Frontends)

Initialize AspNet.AssetManager components in `Program.cs`:
```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAssetManager(builder.Configuration, builder.Environment);
```

Use extensions to get the bundle name:
```csharp
var bundle = ViewData.GetBundleName() // Gets the bundle name from ViewData["Bundle"]
var bundle = Html.GetBundleName() // Gets the bundle name from the view context
```

Recommended use in eg. `_Layout.cshtml`:
```csharp
var bundle = ViewData.GetBundleName() ?? Html.GetBundleName();
// Gets the bundle name from the view context but allows overriding it in ViewData["Bundle"]
```

Use `AssetService` to get assets:
```csharp
@inject IAssetService AssetService

@await AssetService.AssetPath
// Generates: /Path/To/Assets

@await AssetService.GetBundlePathAsync("SomeBundle.js")
// Generates: /Path/To/Assets/SomeBundle.js?v=cache-buster

@await AssetService.GetScriptTagAsync("SomeBundle")
// Generates: <script src="/Path/To/Assets/SomeBundle.js?v=cache-buster"></script>

@await AssetService.GetLinkTagAsync("SomeBundle")
// Generates: <link href="/Path/To/Assets/SomeBundle.css?v=cache-buster" rel=\"stylesheet\" />

@await AssetService.GetStyleTagAsync("SomeBundle")
// Generates: <style>Inlined CSS</style
```
Overloads exist on `GetBundlePathAsync` in case no extension is applied to the bundle name.

Overloads exist on `GetScriptTagAsync` to change the load behavior to e.g. `async` and/or `defer`.

A fallback bundle can be set on: `GetScriptTagAsync`, `GetLinkTagAsync`, `GetStyleTagAsync`
```csharp
@await AssetService.GetScriptTagAsync("SomeBundle", "FallbackBundle")
// Generates: <script src="/Path/To/Assets/SomeBundle.js?v=cache-buster"></script>
// Or if SomeBundle does not exist: <script src="/Path/To/Assets/FallbackBundle.js?v=cache-buster"></script>
```

## Example _Layout.cshtml

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
    @await AssetService.GetLinkTagAsync(bundle, "Layout");
</head>
<body>
    @RenderBody()
    @await AssetService.GetScriptTagAsync(bundle, "Layout", ScriptLoad.Defer);
    @await RenderSectionAsync("Scripts", required: false)
</body>
</html>
```
