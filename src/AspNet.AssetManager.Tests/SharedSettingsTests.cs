// <copyright file="SharedSettingsTests.cs" company="Morten Larsen">
// Copyright (c) Morten Larsen. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using AspNet.AssetManager.Tests.Data;
using AwesomeAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace AspNet.AssetManager.Tests;

public class SharedSettingsTests
{
    private const string PublicDevServer = "https://public.dev";
    private const string InternalDevServer = "https://internal.dev";
    private const string PublicPath = "/public";
    private const string ManifestFile = "manifest.json";

    private static string DevAssetsWebPathResult => $"{PublicDevServer}{PublicPath}";

    private static string ProdAssetsDirectoryPathResult => $"{TestValues.WebRootPath}{PublicPath}";

    private static string ProdAssetsWebPathResult => PublicPath;

    private static string ProdManifestPathResult => $"{ProdAssetsDirectoryPathResult}{ManifestFile}";

    [Fact]
    public void Constructor_OptionsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        var webHostEnvironment = new Mock<IWebHostEnvironment>();

        // Act
        Action act = () => _ = new SharedSettings(null!, webHostEnvironment.Object);

        // Assert
        act.Should().ThrowExactly<ArgumentNullException>();
        webHostEnvironment.VerifyNoOtherCalls();
    }

    [Fact]
    public void Constructor_WebHostEnvironmentNull_ShouldThrowArgumentNullException()
    {
        // Act
        var optionsMock = MockOptions(InternalDevServer);
        Action act = () => _ = new SharedSettings(optionsMock.Object, null!);

        // Assert
        act.Should().ThrowExactly<ArgumentNullException>();
        optionsMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(InternalDevServer)]
    [InlineData("/Internal/Path")]
    public void Constructor_Development_ShouldSetAllVariables(string internalDevServer)
    {
        // Arrange
        var optionsMock = MockOptions(internalDevServer);
        var webHostEnvironmentMock = DependencyMocker.GetWebHostEnvironment(TestValues.Development);

        // Act
        var sharedSettings = new SharedSettings(optionsMock.Object, webHostEnvironmentMock.Object);

        // Assert
        sharedSettings.DevelopmentMode.Should().BeTrue();
        sharedSettings.AssetsDirectoryPath.Should().Be(DevAssetsDirectoryPathResult(internalDevServer));
        sharedSettings.AssetsWebPath.Should().Be(DevAssetsWebPathResult);
        sharedSettings.ManifestPath.Should().Be(DevManifestPathResult(internalDevServer));
        sharedSettings.ManifestType.Should().Be(ManifestType.KeyValue);
        webHostEnvironmentMock.VerifyGet(x => x.EnvironmentName, Times.Once);
        webHostEnvironmentMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(InternalDevServer)]
    [InlineData("/Internal/Path")]
    public void Constructor_Production_ShouldSetAllVariables(string internalDevServer)
    {
        // Arrange
        var optionsMock = MockOptions(internalDevServer);
        var webHostEnvironmentMock = DependencyMocker.GetWebHostEnvironment(TestValues.Production);

        // Act
        var sharedSettings = new SharedSettings(optionsMock.Object, webHostEnvironmentMock.Object);

        // Assert
        sharedSettings.DevelopmentMode.Should().BeFalse();
        sharedSettings.AssetsDirectoryPath.Should().Be(ProdAssetsDirectoryPathResult);
        sharedSettings.AssetsWebPath.Should().Be(ProdAssetsWebPathResult);
        sharedSettings.ManifestPath.Should().Be(ProdManifestPathResult);
        sharedSettings.ManifestType.Should().Be(ManifestType.KeyValue);
        webHostEnvironmentMock.VerifyGet(x => x.EnvironmentName, Times.Once);
        webHostEnvironmentMock.VerifyGet(x => x.WebRootPath, Times.Once);
        webHostEnvironmentMock.VerifyNoOtherCalls();
    }

    private static string DevAssetsDirectoryPathResult(string internalDevServer) => $"{internalDevServer}{PublicPath}";

    private static string DevManifestPathResult(string internalDevServer) => $"{DevAssetsDirectoryPathResult(internalDevServer)}{ManifestFile}";

    private static Mock<IOptions<AssetManagerOptions>> MockOptions(string internalDevServer)
    {
        var optionsMock = new Mock<IOptions<AssetManagerOptions>>();

        optionsMock
            .SetupGet(x => x.Value)
            .Returns(new AssetManagerOptions
            {
                PublicDevServer = PublicDevServer,
                InternalDevServer = internalDevServer,
                PublicPath = PublicPath,
                ManifestFile = ManifestFile,
            });

        return optionsMock;
    }
}
