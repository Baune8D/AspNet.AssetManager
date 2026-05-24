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

    // Vite emits standalone CSS chunks as root-level entries without a "name" property.
    // The bundle we look up appears AFTER the nameless entry to ensure the loop iterates past it.
    private const string HttpClientViteResponseWithNamelessCss = $$"""
                                                    {
                                                      "_Layout-abc123.css": {
                                                        "file": "_Layout-abc123.css",
                                                        "src": "_Layout-abc123.css"
                                                      },
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

    private const string Bundle = "Bundle.js";
    private const string HttpClientResponse = "File content";

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
            .ThrowExactlyAsync<DevServerException>()
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

    [Theory]
    [InlineData(TestValues.JsonBundleJs, TestValues.JsonResultBundleJs)]
    [InlineData("InvalidBundle.js", null)]
    public async Task GetFromManifest_ProductionViteManifestWithNamelessEntry_ShouldNotThrow(string bundle, string? resultBundle)
    {
        // Arrange
        var assetConfigurationMock = DependencyMocker.GetAssetConfiguration(TestValues.Production, ManifestType.Vite);
        var httpClientFactoryMock = DependencyMocker.GetHttpClientFactory(HttpStatusCode.OK, HttpClientViteResponseWithNamelessCss, true);
        var fileSystemMock = DependencyMocker.GetFileSystem(HttpClientViteResponseWithNamelessCss);
        _manifestService = new ManifestService(assetConfigurationMock.Object, fileSystemMock.Object, httpClientFactoryMock.Object);

        // Act
        var result = await _manifestService.GetFromManifestAsync(bundle);

        // Assert
        result.Should().Be(resultBundle);
    }

    [Theory]
    [InlineData(ManifestType.KeyValue, HttpClientKeyValueResponse, TestValues.JsonBundleCss, new[] { TestValues.JsonResultBundleCss })]
    [InlineData(ManifestType.KeyValue, HttpClientKeyValueResponse, "InvalidBundle.css", new string[0])]
    [InlineData(ManifestType.Vite, HttpClientViteResponse, TestValues.JsonBundleCss, new[] { TestValues.JsonResultBundleCss })]
    [InlineData(ManifestType.Vite, HttpClientViteResponse, "InvalidBundle.css", new string[0])]
    [InlineData(ManifestType.Vite, HttpClientViteDevResponse, TestValues.JsonBundleCss, new string[0])]
    public async Task GetCssFromManifest_Production_ShouldReturnExpectedCssList(
        ManifestType manifestType,
        string httpClientResponse,
        string bundle,
        string[] expectedCss)
    {
        // Arrange
        var assetConfigurationMock = DependencyMocker.GetAssetConfiguration(TestValues.Production, manifestType);
        var httpClientFactoryMock = DependencyMocker.GetHttpClientFactory(HttpStatusCode.OK, httpClientResponse, true);
        var fileSystemMock = DependencyMocker.GetFileSystem(httpClientResponse);
        _manifestService = new ManifestService(assetConfigurationMock.Object, fileSystemMock.Object, httpClientFactoryMock.Object);

        // Act
        var result = await _manifestService.GetCssFromManifestAsync(bundle);

        // Assert
        result.Should().Equal(expectedCss);
    }

    [Fact]
    public async Task GetCssFromManifest_ViteCssOnImportedChunk_ShouldFollowImports()
    {
        // Arrange - Vite assigns CSS to the chunk that imports it. When an entry
        // depends on a shared chunk (typical for layouts), the entry's `css` field
        // is empty and the CSS lives on the imported chunk's manifest entry.
        const string manifest = """
                                {
                                  "Assets/Layout.bundle.ts": {
                                    "file": "Layout-DEBycRcF.js",
                                    "name": "Layout",
                                    "src": "Assets/Layout.bundle.ts",
                                    "imports": ["__Layout.cshtml-B7H2t3_E.js"]
                                  },
                                  "__Layout.cshtml-B7H2t3_E.js": {
                                    "file": "_Layout.cshtml-B7H2t3_E.js",
                                    "name": "_Layout.cshtml",
                                    "css": ["_Layout-WWSQZMs6.css"]
                                  }
                                }
                                """;

        var assetConfigurationMock = DependencyMocker.GetAssetConfiguration(TestValues.Production, ManifestType.Vite);
        var fileSystemMock = DependencyMocker.GetFileSystem(manifest);
        _manifestService = new ManifestService(assetConfigurationMock.Object, fileSystemMock.Object);

        // Act
        var result = await _manifestService.GetCssFromManifestAsync("Layout.css");

        // Assert
        result.Should().Equal("_Layout-WWSQZMs6.css");
    }

    [Fact]
    public async Task GetCssFromManifest_ViteTransitiveImports_ShouldReturnDependencyFirstOrder()
    {
        // Arrange - cascade order: imports' CSS first (foundation), entry's CSS last (overrides).
        const string manifest = """
                                {
                                  "src/entry.ts": {
                                    "file": "entry.js",
                                    "name": "entry",
                                    "imports": ["chunk-mid.js"],
                                    "css": ["entry.css"]
                                  },
                                  "chunk-mid.js": {
                                    "file": "chunk-mid.js",
                                    "name": "mid",
                                    "imports": ["chunk-leaf.js"],
                                    "css": ["mid.css"]
                                  },
                                  "chunk-leaf.js": {
                                    "file": "chunk-leaf.js",
                                    "name": "leaf",
                                    "css": ["leaf.css"]
                                  }
                                }
                                """;

        var assetConfigurationMock = DependencyMocker.GetAssetConfiguration(TestValues.Production, ManifestType.Vite);
        var fileSystemMock = DependencyMocker.GetFileSystem(manifest);
        _manifestService = new ManifestService(assetConfigurationMock.Object, fileSystemMock.Object);

        // Act
        var result = await _manifestService.GetCssFromManifestAsync("entry.css");

        // Assert
        result.Should().Equal("leaf.css", "mid.css", "entry.css");
    }

    [Fact]
    public async Task GetCssFromManifest_ViteSharedImport_ShouldDeduplicate()
    {
        // Arrange - two import paths reach the same shared chunk; its CSS must appear once.
        const string manifest = """
                                {
                                  "src/entry.ts": {
                                    "file": "entry.js",
                                    "name": "entry",
                                    "imports": ["chunk-b.js", "chunk-c.js"]
                                  },
                                  "chunk-b.js": {
                                    "file": "chunk-b.js",
                                    "name": "b",
                                    "imports": ["chunk-shared.js"]
                                  },
                                  "chunk-c.js": {
                                    "file": "chunk-c.js",
                                    "name": "c",
                                    "imports": ["chunk-shared.js"]
                                  },
                                  "chunk-shared.js": {
                                    "file": "chunk-shared.js",
                                    "name": "shared",
                                    "css": ["shared.css"]
                                  }
                                }
                                """;

        var assetConfigurationMock = DependencyMocker.GetAssetConfiguration(TestValues.Production, ManifestType.Vite);
        var fileSystemMock = DependencyMocker.GetFileSystem(manifest);
        _manifestService = new ManifestService(assetConfigurationMock.Object, fileSystemMock.Object);

        // Act
        var result = await _manifestService.GetCssFromManifestAsync("entry.css");

        // Assert
        result.Should().Equal("shared.css");
    }

    [Fact]
    public async Task GetCssFromManifest_ViteImportCycle_ShouldNotLoop()
    {
        // Arrange - manifest with a cycle between two chunks; walking must terminate.
        const string manifest = """
                                {
                                  "a.js": {
                                    "file": "a.js",
                                    "name": "a",
                                    "imports": ["b.js"],
                                    "css": ["a.css"]
                                  },
                                  "b.js": {
                                    "file": "b.js",
                                    "name": "b",
                                    "imports": ["a.js"],
                                    "css": ["b.css"]
                                  }
                                }
                                """;

        var assetConfigurationMock = DependencyMocker.GetAssetConfiguration(TestValues.Production, ManifestType.Vite);
        var fileSystemMock = DependencyMocker.GetFileSystem(manifest);
        _manifestService = new ManifestService(assetConfigurationMock.Object, fileSystemMock.Object);

        // Act
        var result = await _manifestService.GetCssFromManifestAsync("a.css");

        // Assert
        result.Should().Equal("b.css", "a.css");
    }

    [Fact]
    public async Task GetFileContent_Null_ShouldThrowArgumentNullException()
    {
        // Arrange
        var assetConfigurationMock = DependencyMocker.GetAssetConfiguration(TestValues.Development);
        var httpClientFactoryMock = DependencyMocker.GetHttpClientFactory(HttpStatusCode.OK, HttpClientResponse);
        var fileSystemMock = DependencyMocker.GetFileSystem(HttpClientResponse);
        _manifestService = new ManifestService(assetConfigurationMock.Object, fileSystemMock.Object, httpClientFactoryMock.Object);

        // Act
        Func<Task> act = () => _manifestService.GetFileContentAsync(null!);

        // Assert
        await act.Should().ThrowExactlyAsync<ArgumentNullException>();
        fileSystemMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetFileContent_DevelopmentNoHttpClient_ShouldThrowArgumentNullException()
    {
        // Arrange
        var assetConfigurationMock = DependencyMocker.GetAssetConfiguration(TestValues.Development);
        var fileSystemMock = DependencyMocker.GetFileSystem(HttpClientResponse);
        _manifestService = new ManifestService(assetConfigurationMock.Object, fileSystemMock.Object);

        // Act
        Func<Task> act = () => _manifestService.GetFileContentAsync("InvalidBundle");

        // Assert
        await act.Should().ThrowExactlyAsync<ArgumentNullException>();
        fileSystemMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetFileContent_Development_ShouldReturnStyleTag()
    {
        // Arrange
        var assetConfigurationMock = DependencyMocker.GetAssetConfiguration(TestValues.Development);
        var httpClientFactoryMock = DependencyMocker.GetHttpClientFactory(HttpStatusCode.OK, HttpClientResponse);
        var fileSystemMock = DependencyMocker.GetFileSystem(HttpClientResponse);
        _manifestService = new ManifestService(assetConfigurationMock.Object, fileSystemMock.Object, httpClientFactoryMock.Object);

        // Act
        var result = await _manifestService.GetFileContentAsync(Bundle);
        var result2 = await _manifestService.GetFileContentAsync(Bundle);

        // Assert
        result.Should().Contain(HttpClientResponse);
        result2.Should().Contain(HttpClientResponse);
        fileSystemMock.VerifyNoOtherCalls();
        httpClientFactoryMock.Verify(x => x.CreateClient(It.IsAny<string>()), Times.Once);
        httpClientFactoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetFileContent_DevelopmentQueryString_ShouldReturnStyleTag()
    {
        // Arrange
        var assetConfigurationMock = DependencyMocker.GetAssetConfiguration(TestValues.Development);
        var httpClientFactoryMock = DependencyMocker.GetHttpClientFactory(HttpStatusCode.OK, HttpClientResponse);
        var fileSystemMock = DependencyMocker.GetFileSystem(HttpClientResponse);
        _manifestService = new ManifestService(assetConfigurationMock.Object, fileSystemMock.Object, httpClientFactoryMock.Object);

        // Act
        var result = await _manifestService.GetFileContentAsync($"{Bundle}?v=123");
        var result2 = await _manifestService.GetFileContentAsync($"{Bundle}?v=123");

        // Assert
        result.Should().Contain(HttpClientResponse);
        result2.Should().Contain(HttpClientResponse);
        fileSystemMock.VerifyNoOtherCalls();
        httpClientFactoryMock.Verify(x => x.CreateClient(It.IsAny<string>()), Times.Once);
        httpClientFactoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetFileContent_Production_ShouldReturnStyleTag()
    {
        // Arrange
        var assetConfigurationMock = DependencyMocker.GetAssetConfiguration(TestValues.Production);
        var httpClientFactoryMock = DependencyMocker.GetHttpClientFactory(HttpStatusCode.OK, HttpClientResponse);
        var fileSystemMock = DependencyMocker.GetFileSystem(HttpClientResponse);
        _manifestService = new ManifestService(assetConfigurationMock.Object, fileSystemMock.Object, httpClientFactoryMock.Object);

        // Act
        var result = await _manifestService.GetFileContentAsync(Bundle);
        var result2 = await _manifestService.GetFileContentAsync(Bundle);

        // Assert
        result.Should().Contain(HttpClientResponse);
        result2.Should().Contain(HttpClientResponse);
        fileSystemMock.Verify(x => x.File.ReadAllTextAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        fileSystemMock.VerifyNoOtherCalls();
    }
}
