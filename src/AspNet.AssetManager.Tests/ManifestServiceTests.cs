// <copyright file="ManifestServiceTests.cs" company="Morten Larsen">
// Copyright (c) Morten Larsen. All rights reserved.
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
    private const string HttpClientKeyValueResponse = $"{{\"{TestValues.JsonBundle}\":\"{TestValues.JsonResultBundle}\"}}";

    private const string HttpClientViteResponse = $"{{\"/Assets/{TestValues.JsonBundle}.js\":{{\"name\":\"{TestValues.JsonBundle}\",\"file\":\"{TestValues.JsonResultBundle}\"}}}}";

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
        var sharedSettingsMock = DependencyMocker.GetSharedSettings(TestValues.Development, manifestType);
        var fileSystemMock = DependencyMocker.GetFileSystem(httpClientResponse);
        _manifestService = new ManifestService(sharedSettingsMock.Object, fileSystemMock.Object);

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
        var sharedSettingsMock = DependencyMocker.GetSharedSettings(TestValues.Development, manifestType);
        var httpClientFactoryMock = DependencyMocker.GetHttpClientFactory(HttpStatusCode.BadGateway);
        var fileSystemMock = DependencyMocker.GetFileSystem(httpClientResponse);
        _manifestService = new ManifestService(sharedSettingsMock.Object, fileSystemMock.Object, httpClientFactoryMock.Object);

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
        var sharedSettingsMock = DependencyMocker.GetSharedSettings(TestValues.Development, manifestType);
        var httpClientFactoryMock = DependencyMocker.GetHttpClientFactory(HttpStatusCode.OK, httpClientResponse, true);
        var fileSystemMock = DependencyMocker.GetFileSystem(httpClientResponse);
        _manifestService = new ManifestService(sharedSettingsMock.Object, fileSystemMock.Object, httpClientFactoryMock.Object);

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
    [InlineData(ManifestType.KeyValue, HttpClientKeyValueResponse)]
    [InlineData(ManifestType.Vite, HttpClientViteResponse)]
    public async Task GetFromManifest_DevelopmentValidBundle_ShouldReturnResultBundle(ManifestType manifestType, string httpClientResponse)
    {
        // Arrange
        var sharedSettingsMock = DependencyMocker.GetSharedSettings(TestValues.Development, manifestType);
        var httpClientFactoryMock = DependencyMocker.GetHttpClientFactory(HttpStatusCode.OK, httpClientResponse, true);
        var fileSystemMock = DependencyMocker.GetFileSystem(httpClientResponse);
        _manifestService = new ManifestService(sharedSettingsMock.Object, fileSystemMock.Object, httpClientFactoryMock.Object);

        // Act
        var result = await _manifestService.GetFromManifestAsync(TestValues.JsonBundle);
        var result2 = await _manifestService.GetFromManifestAsync(TestValues.JsonBundle);

        // Assert
        result.Should().Be(TestValues.JsonResultBundle);
        result2.Should().Be(TestValues.JsonResultBundle);
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
        var sharedSettingsMock = DependencyMocker.GetSharedSettings(TestValues.Production, manifestType);
        var httpClientFactoryMock = DependencyMocker.GetHttpClientFactory(HttpStatusCode.OK, httpClientResponse, true);
        var fileSystemMock = DependencyMocker.GetFileSystem(httpClientResponse);
        _manifestService = new ManifestService(sharedSettingsMock.Object, fileSystemMock.Object, httpClientFactoryMock.Object);

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
    [InlineData(ManifestType.KeyValue, HttpClientKeyValueResponse)]
    [InlineData(ManifestType.Vite, HttpClientViteResponse)]
    public async Task GetFromManifest_ProductionValidBundle_ShouldReturnResultBundle(ManifestType manifestType, string httpClientResponse)
    {
        // Arrange
        var sharedSettingsMock = DependencyMocker.GetSharedSettings(TestValues.Production, manifestType);
        var httpClientFactoryMock = DependencyMocker.GetHttpClientFactory(HttpStatusCode.OK, httpClientResponse, true);
        var fileSystemMock = DependencyMocker.GetFileSystem(httpClientResponse);
        _manifestService = new ManifestService(sharedSettingsMock.Object, fileSystemMock.Object, httpClientFactoryMock.Object);

        // Act
        var result = await _manifestService.GetFromManifestAsync(TestValues.JsonBundle);
        var result2 = await _manifestService.GetFromManifestAsync(TestValues.JsonBundle);

        // Assert
        result.Should().Be(TestValues.JsonResultBundle);
        result2.Should().Be(TestValues.JsonResultBundle);
        fileSystemMock.Verify(x => x.File.ReadAllTextAsync(It.IsAny<string>(), CancellationToken.None), Times.Once);
        fileSystemMock.VerifyNoOtherCalls();
        httpClientFactoryMock.VerifyNoOtherCalls();
    }
}
