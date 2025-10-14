// <copyright file="ManifestServiceTests.cs" company="Baune8D">
// Copyright (c) Baune8D. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AspNet.AssetManager.Tests.Data;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace AspNet.AssetManager.Tests;

public sealed class ManifestServiceTests : IDisposable
{
    private const string HttpClientKeyValueResponse = $$"""
                                                        {
                                                          "{{TestValues.JsonBundleJs}}": "{{TestValues.JsonResultBundleJs}}",
                                                          "{{TestValues.JsonBundleCss}}": "{{TestValues.JsonResultBundleCss}}"
                                                        }
                                                        """;

    private const string HttpClientViteResponse = $$"""
                                                    {
                                                      "Assets/{{TestValues.JsonBundleJs}}": {
                                                        "file": "{{TestValues.JsonResultBundleJs}}",
                                                        "name": "{{TestValues.JsonBundleName}}",
                                                        "src": "{{TestValues.JsonSrcBundleJs}}",
                                                        "css": [
                                                          "{{TestValues.JsonResultBundleCss}}"
                                                        ]
                                                      }
                                                    }
                                                    """;

    private const string HttpClientViteDevResponse = $$"""
                                                    {
                                                      "Assets/{{TestValues.JsonBundleJs}}": {
                                                        "name": "{{TestValues.JsonBundleName}}",
                                                        "src": "{{TestValues.JsonSrcBundleJs}}"
                                                      }
                                                    }
                                                    """;

    private ManifestService? _manifestService;

    public void Dispose()
    {
        _manifestService?.Dispose();
    }

    [Theory]
    [InlineData(ManifestType.KeyValue, HttpClientKeyValueResponse)]
    [InlineData(ManifestType.Vite, HttpClientViteResponse)]
    public async Task GetFromManifest_DevelopmentNoHttpClient_ShouldThrowArgumentNullException(ManifestType manifestType, string httpClientResponse)
    {
        // Arrange
        var assetConfigurationMock = DependencyMocker.GetAssetConfiguration(TestValues.Development, manifestType);
        var fileSystemMock = DependencyMocker.GetFileSystem(httpClientResponse);
        _manifestService = new ManifestService(assetConfigurationMock.Object, fileSystemMock.Object);

        // Act
        Func<Task> act = () => _manifestService.GetFromManifestAsync("InvalidBundle");

        // Assert
        await act.Should().ThrowExactlyAsync<ArgumentNullException>();
        fileSystemMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(ManifestType.KeyValue, HttpClientKeyValueResponse)]
    [InlineData(ManifestType.Vite, HttpClientViteResponse)]
    public async Task GetFromManifest_DevelopmentRequestFail_ShouldThrowHttpRequestException(ManifestType manifestType, string httpClientResponse)
    {
        // Arrange
        var assetConfigurationMock = DependencyMocker.GetAssetConfiguration(TestValues.Development, manifestType);
        var httpClientFactoryMock = DependencyMocker.GetHttpClientFactory(HttpStatusCode.BadGateway);
        var fileSystemMock = DependencyMocker.GetFileSystem(httpClientResponse);
        _manifestService = new ManifestService(assetConfigurationMock.Object, fileSystemMock.Object, httpClientFactoryMock.Object);

        // Act
        Func<Task> act = () => _manifestService.GetFromManifestAsync("InvalidBundle");

        // Assert
        await act.Should()
            .ThrowExactlyAsync<InvalidOperationException>()
            .WithMessage("Development server not started!");
        fileSystemMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(ManifestType.KeyValue, HttpClientKeyValueResponse)]
    [InlineData(ManifestType.Vite, HttpClientViteResponse)]
    public async Task GetFromManifest_DevelopmentInvalidBundle_ShouldReturnNull(ManifestType manifestType, string httpClientResponse)
    {
        // Arrange
        var assetConfigurationMock = DependencyMocker.GetAssetConfiguration(TestValues.Development, manifestType);
        var httpClientFactoryMock = DependencyMocker.GetHttpClientFactory(HttpStatusCode.OK, httpClientResponse, true);
        var fileSystemMock = DependencyMocker.GetFileSystem(httpClientResponse);
        _manifestService = new ManifestService(assetConfigurationMock.Object, fileSystemMock.Object, httpClientFactoryMock.Object);

        // Act
        var result = await _manifestService.GetFromManifestAsync("InvalidBundle");
        var result2 = await _manifestService.GetFromManifestAsync("InvalidBundle");

        // Assert
        result.Should().BeNull();
        result2.Should().BeNull();
        fileSystemMock.VerifyNoOtherCalls();
        httpClientFactoryMock.Verify(x => x.CreateClient(It.IsAny<string>()), Times.Once);
        httpClientFactoryMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(ManifestType.KeyValue, HttpClientKeyValueResponse, TestValues.JsonBundleJs, TestValues.JsonResultBundleJs)]
    [InlineData(ManifestType.KeyValue, HttpClientKeyValueResponse, TestValues.JsonBundleCss, TestValues.JsonResultBundleCss)]
    [InlineData(ManifestType.Vite, HttpClientViteDevResponse, TestValues.JsonBundleJs, TestValues.JsonSrcBundleJs)]
    [InlineData(ManifestType.Vite, HttpClientViteDevResponse, TestValues.JsonBundleCss, null)]
    public async Task GetFromManifest_DevelopmentValidBundle_ShouldReturnResultBundle(ManifestType manifestType, string httpClientResponse, string bundle, string? resultBundle)
    {
        // Arrange
        var assetConfigurationMock = DependencyMocker.GetAssetConfiguration(TestValues.Development, manifestType);
        var httpClientFactoryMock = DependencyMocker.GetHttpClientFactory(HttpStatusCode.OK, httpClientResponse, true);
        var fileSystemMock = DependencyMocker.GetFileSystem(httpClientResponse);
        _manifestService = new ManifestService(assetConfigurationMock.Object, fileSystemMock.Object, httpClientFactoryMock.Object);

        // Act
        var result = await _manifestService.GetFromManifestAsync(bundle);
        var result2 = await _manifestService.GetFromManifestAsync(bundle);

        // Assert
        result.Should().Be(resultBundle);
        result2.Should().Be(resultBundle);
        fileSystemMock.VerifyNoOtherCalls();
        httpClientFactoryMock.Verify(x => x.CreateClient(It.IsAny<string>()), Times.Once);
        httpClientFactoryMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(ManifestType.KeyValue, HttpClientKeyValueResponse)]
    [InlineData(ManifestType.Vite, HttpClientViteResponse)]
    public async Task GetFromManifest_ProductionInvalidBundle_ShouldReturnNull(ManifestType manifestType, string httpClientResponse)
    {
        // Arrange
        var assetConfigurationMock = DependencyMocker.GetAssetConfiguration(TestValues.Production, manifestType);
        var httpClientFactoryMock = DependencyMocker.GetHttpClientFactory(HttpStatusCode.OK, httpClientResponse, true);
        var fileSystemMock = DependencyMocker.GetFileSystem(httpClientResponse);
        _manifestService = new ManifestService(assetConfigurationMock.Object, fileSystemMock.Object, httpClientFactoryMock.Object);

        // Act
        var result = await _manifestService.GetFromManifestAsync("InvalidBundle");
        var result2 = await _manifestService.GetFromManifestAsync("InvalidBundle");

        // Assert
        result.Should().BeNull();
        result2.Should().BeNull();
        fileSystemMock.Verify(x => x.File.ReadAllTextAsync(It.IsAny<string>(), CancellationToken.None), Times.Once);
        fileSystemMock.VerifyNoOtherCalls();
        httpClientFactoryMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(ManifestType.KeyValue, HttpClientKeyValueResponse, TestValues.JsonBundleJs, TestValues.JsonResultBundleJs)]
    [InlineData(ManifestType.KeyValue, HttpClientKeyValueResponse, TestValues.JsonBundleCss, TestValues.JsonResultBundleCss)]
    [InlineData(ManifestType.Vite, HttpClientViteResponse, TestValues.JsonBundleJs, TestValues.JsonResultBundleJs)]
    [InlineData(ManifestType.Vite, HttpClientViteResponse, TestValues.JsonBundleCss, TestValues.JsonResultBundleCss)]
    public async Task GetFromManifest_ProductionValidBundle_ShouldReturnResultBundle(ManifestType manifestType, string httpClientResponse, string bundle, string resultBundle)
    {
        // Arrange
        var assetConfigurationMock = DependencyMocker.GetAssetConfiguration(TestValues.Production, manifestType);
        var httpClientFactoryMock = DependencyMocker.GetHttpClientFactory(HttpStatusCode.OK, httpClientResponse, true);
        var fileSystemMock = DependencyMocker.GetFileSystem(httpClientResponse);
        _manifestService = new ManifestService(assetConfigurationMock.Object, fileSystemMock.Object, httpClientFactoryMock.Object);

        // Act
        var result = await _manifestService.GetFromManifestAsync(bundle);
        var result2 = await _manifestService.GetFromManifestAsync(bundle);

        // Assert
        result.Should().Be(resultBundle);
        result2.Should().Be(resultBundle);
        fileSystemMock.Verify(x => x.File.ReadAllTextAsync(It.IsAny<string>(), CancellationToken.None), Times.Once);
        fileSystemMock.VerifyNoOtherCalls();
        httpClientFactoryMock.VerifyNoOtherCalls();
    }
}
