// <copyright file="TestValues.cs" company="Morten Larsen">
// Copyright (c) Morten Larsen. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace AspNet.AssetManager.Tests.Data;

/// <summary>
/// Fixed values for use in testing.
/// </summary>
public static class TestValues
{
    /// <summary>
    /// Fixed value for development environment.
    /// </summary>
    public const string Development = "Development";

    /// <summary>
    /// Fixed value for production environment.
    /// </summary>
    public const string Production = "Production";

    /// <summary>
    /// Web root path used in mocked WebHostEnvironment.
    /// </summary>
    public const string WebRootPath = "/Path/To/wwwroot";

    /// <summary>
    /// Assets web path used in mocked SharedSettings.
    /// </summary>
    public const string AssetsWebPath = "/Path/To/Assets/";

    /// <summary>
    /// Bundle filename used in json result from HttpMessageHandlerStub.
    /// </summary>
    public const string JsonBundleName = "Bundle";

    /// <summary>
    /// Bundle filename used in json result from HttpMessageHandlerStub.
    /// </summary>
    public const string JsonBundleJs = $"{JsonBundleName}.js";

    /// <summary>
    /// Bundle filename used in json result from HttpMessageHandlerStub.
    /// </summary>
    public const string JsonBundleCss = $"{JsonBundleName}.css";

    /// <summary>
    /// Bundle result filename used in json result from HttpMessageHandlerStub.
    /// </summary>
    public const string JsonResultBundleJs = $"{JsonBundleName}.min.js";

    /// <summary>
    /// Bundle result filename used in json result from HttpMessageHandlerStub.
    /// </summary>
    public const string JsonResultBundleCss = $"{JsonBundleName}.min.js";

    /// <summary>
    /// Bundle result filename used in json result from HttpMessageHandlerStub.
    /// </summary>
    public const string JsonSrcBundleJs = $"Assets/{JsonBundleName}.min.js";
}
