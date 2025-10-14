// <copyright file="DependencyMocker.cs" company="Baune8D">
// Copyright (c) Baune8D. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.IO.Abstractions;
using System.Net;
using System.Net.Http;
using System.Threading;
using Microsoft.AspNetCore.Hosting;
using Moq;

namespace AspNet.AssetManager.Tests.Data;

/// <summary>
/// Static functions for mocking common dependencies.
/// </summary>
public static class DependencyMocker
{
    /// <summary>
    /// Mock IWebHostEnvironment.
    /// </summary>
    /// <param name="environmentName">The environment name.</param>
    /// <returns>The WebHostEnvironment object.</returns>
    public static Mock<IWebHostEnvironment> GetWebHostEnvironment(string environmentName)
    {
        var webHostEnvironmentMock = new Mock<IWebHostEnvironment>();

        webHostEnvironmentMock
            .SetupGet(x => x.EnvironmentName)
            .Returns(environmentName);

        webHostEnvironmentMock
            .SetupGet(x => x.WebRootPath)
            .Returns(TestValues.WebRootPath);

        return webHostEnvironmentMock;
    }

    /// <summary>
    /// Mock IFileSystem.
    /// </summary>
    /// <param name="fileContent">The response content of ReadAllText.</param>
    /// <returns>The FileSystem object.</returns>
    public static Mock<IFileSystem> GetFileSystem(string fileContent)
    {
        var fileSystemMock = new Mock<IFileSystem>();

        fileSystemMock
            .Setup(x => x.File.ReadAllTextAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(fileContent);

        return fileSystemMock;
    }

    /// <summary>
    /// Mock IHttpClientFactory.
    /// </summary>
    /// <param name="httpStatusCode">The response status code.</param>
    /// <param name="content">The response content.</param>
    /// <param name="json">If the response should be JSON.</param>
    /// <returns>The HttpClientFactory object.</returns>
    public static Mock<IHttpClientFactory> GetHttpClientFactory(HttpStatusCode httpStatusCode, string content = "", bool json = false)
    {
        var httpClientFactory = new Mock<IHttpClientFactory>();

        httpClientFactory
            .Setup(x => x.CreateClient(It.IsAny<string>()))
            .Returns<string>(_ => new HttpClient(new HttpMessageHandlerStub(httpStatusCode, content, json)));

        return httpClientFactory;
    }

    /// <summary>
    /// Mock IAssetConfiguration.
    /// </summary>
    /// <param name="environmentName">The environment name.</param>
    /// <param name="manifestType">The manifest type.</param>
    /// <returns>The SharedSettings object.</returns>
    public static Mock<IAssetConfiguration> GetAssetConfiguration(string environmentName, ManifestType manifestType = ManifestType.KeyValue)
    {
        var isDevelopment = environmentName == TestValues.Development;

        var sharedSettings = new Mock<IAssetConfiguration>();

        sharedSettings
            .SetupGet(x => x.DevelopmentMode)
            .Returns(isDevelopment);

        sharedSettings
            .SetupGet(x => x.AssetsDirectoryPath)
            .Returns(isDevelopment
                ? "https://domain/assets/"
                : "/Some/Path/To/Assets/");

        sharedSettings
            .SetupGet(x => x.AssetsWebPath)
            .Returns(TestValues.AssetsWebPath);

        sharedSettings
            .SetupGet(x => x.ManifestPath)
            .Returns(isDevelopment
                ? "https://domain/manifest.json"
                : "/Some/Path/To/Manifest.json");

        sharedSettings
            .SetupGet(x => x.ManifestType)
            .Returns(manifestType);

        return sharedSettings;
    }
}
