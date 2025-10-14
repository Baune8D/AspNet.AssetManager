// <copyright file="TestValues.cs" company="Baune8D">
// Copyright (c) Baune8D. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace AspNet.AssetManager.Tests.Data;

internal static class TestValues
{
    public const string Development = "Development";

    public const string Production = "Production";

    public const string WebRootPath = "/Path/To/wwwroot";

    public const string AssetsWebPath = "/Path/To/Assets/";

    public const string JsonBundleName = "Bundle";

    public const string JsonBundleJs = $"{JsonBundleName}.js";

    public const string JsonBundleCss = $"{JsonBundleName}.css";

    public const string JsonResultBundleJs = $"{JsonBundleName}.min.js";

    public const string JsonResultBundleCss = $"{JsonBundleName}.min.js";

    public const string JsonSrcBundleJs = $"Assets/{JsonBundleName}.min.js";
}
