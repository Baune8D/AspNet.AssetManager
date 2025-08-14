// <copyright file="ServiceCollectionExtensionsTests.cs" company="Morten Larsen">
// Copyright (c) Morten Larsen. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.Net.Http;
using AwesomeAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace AspNet.AssetManager.Tests;

public class ServiceCollectionExtensionsTests
{
    private readonly IServiceCollection _serviceCollection;
    private readonly IConfiguration _configuration;

    public ServiceCollectionExtensionsTests()
    {
        _serviceCollection = new ServiceCollection();

        var configuration = new Mock<IConfiguration>();
        var configurationSection = new Mock<IConfigurationSection>();
        configuration
            .Setup(x => x.GetSection("AssetManager"))
            .Returns(configurationSection.Object);

        _configuration = configuration.Object;
        _serviceCollection.AddSingleton(_configuration);
    }

    [Fact]
    public void AddAssetManager_ConfigurationNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        var webHostEnvironment = new Mock<IWebHostEnvironment>().Object;

        // Act
        Action act = () => new ServiceCollection().AddAssetManager(null!, webHostEnvironment);

        // Assert
        act.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void AddAssetManager_Development_ShouldResolveServices()
    {
        // Arrange
        var webHostEnvironment = CreateWebHostEnvironment("Development");
        _serviceCollection.AddSingleton(webHostEnvironment);

        // Act
        _serviceCollection.AddAssetManager(_configuration, webHostEnvironment);
        var provider = _serviceCollection.BuildServiceProvider();
        var httpClientFactory = provider.GetService<IHttpClientFactory>();
        var assetManagerOptions = provider.GetService<IOptions<AssetManagerOptions>>();
        var manifestService = provider.GetService<IManifestService>();
        var tagBuilder = provider.GetService<ITagBuilder>();
        var assetService = provider.GetService<IAssetService>();

        // Assert
        httpClientFactory.Should().NotBeNull();
        VerifyDefaultServices(assetManagerOptions, manifestService, tagBuilder, assetService);
    }

    [Fact]
    public void AddAssetManager_Production_ShouldResolveServices()
    {
        // Arrange
        var webHostEnvironment = CreateWebHostEnvironment("Production");
        _serviceCollection.AddSingleton(webHostEnvironment);

        // Act
        _serviceCollection.AddAssetManager(_configuration, webHostEnvironment);
        var provider = _serviceCollection.BuildServiceProvider();
        var httpClientFactory = provider.GetService<IHttpClientFactory>();
        var assetManagerOptions = provider.GetService<IOptions<AssetManagerOptions>>();
        var manifestService = provider.GetService<IManifestService>();
        var tagBuilder = provider.GetService<ITagBuilder>();
        var assetService = provider.GetService<IAssetService>();

        // Assert
        httpClientFactory.Should().BeNull();
        VerifyDefaultServices(assetManagerOptions, manifestService, tagBuilder, assetService);
    }

    private static void VerifyDefaultServices(
        IOptions<AssetManagerOptions>? assetManagerOptions,
        IManifestService? manifestService,
        ITagBuilder? tagBuilder,
        IAssetService? assetService)
    {
        assetManagerOptions.Should().NotBeNull();
        manifestService.Should().NotBeNull();
        tagBuilder.Should().NotBeNull();
        assetService.Should().NotBeNull();
    }

    private static IWebHostEnvironment CreateWebHostEnvironment(string environment)
    {
        var webHostEnvironmentMock = new Mock<IWebHostEnvironment>();
        webHostEnvironmentMock
            .SetupGet(x => x.EnvironmentName)
            .Returns(environment);

        return webHostEnvironmentMock.Object;
    }
}
